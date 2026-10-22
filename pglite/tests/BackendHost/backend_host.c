#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/resource.h>
#include <ifaddrs.h>
#include <execinfo.h>
#include <locale.h>
#include <ctype.h>
#include <wctype.h>
#include <math.h>

#define REQUIRE(x) do { if (!(x)) return __LINE__; } while (0)
#define PATH(name, suffix) char name[4096]; snprintf(name, sizeof(name), "%s/%s", directory, suffix)

int dph_backend_probe(const char *directory, unsigned long *observations)
{
    struct ifaddrs *interfaces = NULL;
    struct rlimit limits;
    struct stat status;
    char *end;
    unsigned int previous_mask = umask(0);
    int fd, peer;
    unsigned char marker = 0xa5, value = 0;
    unsigned long count = 0;
    PATH(file, "created");
    PATH(link, "dangling");
    PATH(target, "link-target");
    PATH(output, "copy");
    PATH(folder, "directory");
    PATH(missing, "missing-parent/child");
    PATH(stdio_path, "stdio");

    observations[0] = sizeof(struct ifaddrs);
    observations[1] = _Alignof(struct ifaddrs);
    observations[2] = offsetof(struct ifaddrs, ifa_next);
    observations[3] = offsetof(struct ifaddrs, ifa_name);
    observations[4] = offsetof(struct ifaddrs, ifa_flags);
    observations[5] = offsetof(struct ifaddrs, ifa_addr);
    observations[6] = offsetof(struct ifaddrs, ifa_netmask);
    observations[7] = offsetof(struct ifaddrs, ifa_broadaddr);
    observations[8] = offsetof(struct ifaddrs, ifa_data);
    observations[9] = sizeof(struct rlimit);
    REQUIRE(getrlimit(RLIMIT_STACK, &limits) == 0);
    observations[10] = limits.rlim_cur;
    REQUIRE(getrlimit(RLIMIT_NOFILE, &limits) == 0);
    observations[11] = limits.rlim_cur;

    fd = open(file, O_CREAT | O_EXCL | O_RDWR, 0640);
    REQUIRE(fd >= 0);
    REQUIRE(fstat(fd, &status) == 0);
    observations[12] = status.st_mode & 0777;
    REQUIRE(observations[12] == 0640);
    REQUIRE(write(fd, &marker, 1) == 1);
    errno = 71;
    REQUIRE(posix_fallocate(fd, 4096, 4096) == 0);
    REQUIRE(errno == 71);
    REQUIRE(lseek(fd, 0, SEEK_CUR) == 1);
    REQUIRE(pread(fd, &value, 1, 0) == 1 && value == marker);
    REQUIRE(fdatasync(fd) == 0);
    REQUIRE(close(fd) == 0);
    umask(077);
    fd = open(file, O_CREAT | O_RDWR, 0777);
    REQUIRE(fd >= 0);
    REQUIRE(fstat(fd, &status) == 0 && (status.st_mode & 0777) == 0640);
    REQUIRE(close(fd) == 0);

    REQUIRE(symlink("link-target", link) == 0);
    REQUIRE(open(link, O_CREAT | O_EXCL | O_RDWR, 0666) == -1 && errno == EEXIST);
    REQUIRE(open(link, O_CREAT | O_NOFOLLOW | O_RDWR, 0666) == -1 && errno == ELOOP);
    fd = open(link, O_CREAT | O_RDWR, 0666);
    REQUIRE(fd >= 0);
    REQUIRE(fstat(fd, &status) == 0);
    observations[13] = status.st_mode & 0777;
    REQUIRE(observations[13] == 0600);
    REQUIRE(close(fd) == 0);
    REQUIRE(lstat(link, &status) == 0 && S_ISLNK(status.st_mode));
    REQUIRE(stat(target, &status) == 0 && (status.st_mode & 0777) == 0600);

    REQUIRE(mkdir(folder, 0777) == 0);
    REQUIRE(stat(folder, &status) == 0);
    observations[14] = status.st_mode & 0777;
    REQUIRE(observations[14] == 0700);
    REQUIRE(mkdir(folder, 0777) == -1 && errno == EEXIST);
    REQUIRE(mkdir(missing, 0777) == -1 && errno == ENOENT);
    fd = open(folder, O_RDONLY);
    REQUIRE(fd >= 0);
    REQUIRE(fstat(fd, &status) == 0 && S_ISDIR(status.st_mode));
    REQUIRE(fsync(fd) == 0);
    peer = dup(fd);
    REQUIRE(peer >= 0 && peer != fd);
    REQUIRE(close(fd) == 0);
    REQUIRE(fsync(peer) == 0);
    REQUIRE(read(peer, &value, 1) == -1 && errno == EISDIR);
    REQUIRE(write(peer, &value, 1) == -1 && errno == EBADF);
    REQUIRE(close(peer) == 0);
    REQUIRE(open(folder, O_RDWR) == -1 && errno == EISDIR);
    umask(0);
    FILE *stream = fopen(stdio_path, "w+b");
    REQUIRE(stream != NULL);
    REQUIRE(fseeko(stream, ((off_t)1 << 32) + 7, SEEK_SET) == 0);
    observations[15] = ftello(stream);
    REQUIRE(fclose(stream) == 0);
    REQUIRE(stat(stdio_path, &status) == 0);
    observations[16] = status.st_mode & 0777;
    REQUIRE(observations[16] == 0666);

    fd = open(file, O_RDONLY);
    peer = open(output, O_CREAT | O_EXCL | O_RDWR, 0600);
    REQUIRE(fd >= 0 && peer >= 0);
    off_t from = 0, to = 1;
    REQUIRE(copy_file_range(fd, &from, peer, &to, 1, 0) == 1);
    REQUIRE(from == 1 && to == 2);
    REQUIRE(lseek(fd, 0, SEEK_CUR) == 0 && lseek(peer, 0, SEEK_CUR) == 0);
    value = 0;
    REQUIRE(pread(peer, &value, 1, 1) == 1 && value == marker);
    REQUIRE(close(fd) == 0 && close(peer) == 0);

    REQUIRE(getifaddrs(&interfaces) == 0 && interfaces != NULL);
    for (struct ifaddrs *p = interfaces; p != NULL; p = p->ifa_next)
    {
        REQUIRE(p->ifa_name != NULL);
        if (p->ifa_addr != NULL) count++;
        /* Read both aliases of the actual native union through emitted fields. */
        REQUIRE(p->ifa_broadaddr == p->ifa_dstaddr);
    }
    freeifaddrs(interfaces);
    observations[17] = count;
    REQUIRE(count > 0);
    REQUIRE(setlocale(LC_CTYPE, "C.UTF-8") != NULL);
    REQUIRE(isascii('A') && !isascii(128));
    REQUIRE(iswalpha(0xe9) && iswupper(0x3a3) && iswspace(0x2003));
    REQUIRE(towlower(0x3a3) == 0x3c3 && towupper(0x10428) == 0x10400);
    errno = 72;
    float parsed = strtof("1.00000005960464477539062500001x", &end);
    REQUIRE(parsed > 1.0f && *end == 'x' && errno == 72);
    REQUIRE(tgamma(5.0) == 24.0);
    REQUIRE(lgamma(1.0) == 0.0);
    errno = 0;
    REQUIRE(isinf(tgamma(0.0)) && errno == ERANGE);

    void *frames[32];
    int nframes = backtrace(frames, 32);
    REQUIRE(nframes > 0 && nframes <= 32);
    char **symbols = backtrace_symbols(frames, nframes);
    REQUIRE(symbols != NULL && symbols[0] != NULL && symbols[0][0] != 0);
    free(symbols);
    fd = open(output, O_WRONLY | O_APPEND);
    REQUIRE(fd >= 0);
    backtrace_symbols_fd(frames, nframes, fd);
    REQUIRE(close(fd) == 0);
    umask(previous_mask);
    return 0;
}

#ifdef DPH_NATIVE_MAIN
int main(int argc, char **argv)
{
    unsigned long observations[18] = {0};
    if (argc != 2) return 2;
    int result = dph_backend_probe(argv[1], observations);
    if (result != 0) { fprintf(stderr, "backend host C assertion line %d errno %d\n", result, errno); return 1; }
    for (int i = 0; i < 18; i++) printf("%lu%s", observations[i], i == 17 ? "\n" : " ");
    return 0;
}
#endif
