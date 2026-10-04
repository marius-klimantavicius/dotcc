#define _DEFAULT_SOURCE 1
#define _POSIX_C_SOURCE 200809L
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <sys/mman.h>

#ifndef POSIX_NATIVE_ORACLE
static void print_layout(void) {
#define FIELD(t,n) printf(" %lu", (unsigned long)offsetof(struct t, n))
    printf("stat %lu", (unsigned long)sizeof(struct stat));
    FIELD(stat, st_dev); FIELD(stat, st_ino); FIELD(stat, st_mode);
    FIELD(stat, st_nlink); FIELD(stat, st_uid); FIELD(stat, st_gid);
    FIELD(stat, st_size); FIELD(stat, st_atime); FIELD(stat, st_mtime);
    FIELD(stat, st_ctime); FIELD(stat, st_rdev); FIELD(stat, st_blksize); FIELD(stat, st_blocks);
    printf("\ntimeval %lu", (unsigned long)sizeof(struct timeval));
    FIELD(timeval, tv_sec); FIELD(timeval, tv_usec);
    printf("\nflock %lu", (unsigned long)sizeof(struct flock));
    FIELD(flock, l_type); FIELD(flock, l_whence); FIELD(flock, l_start); FIELD(flock, l_len); FIELD(flock, l_pid);
    printf("\n");
#undef FIELD
}
#endif

#ifdef POSIX_LAYOUT_ONLY
int main(void) { print_layout(); return 0; }
#else
#define CHECK(e) do { if (!(e)) { printf("failure line=%d errno=%d\n", __LINE__, errno); return 1; } } while (0)

/* Deliberately leave resources open: the managed consumer verifies owner
   Shutdown/Dispose closes descriptors and unmaps the surviving view. */
int posix_leave_resources(const char *path) {
    long page = sysconf(_SC_PAGESIZE);
    int fd = open(path, O_RDWR | O_CREAT | O_TRUNC, 0600);
    if (fd < 0 || ftruncate(fd, page) != 0) return -1;
    unsigned char *memory = mmap(0, (size_t)page, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    if (memory == MAP_FAILED) { close(fd); return -1; }
    memory[0] = 77;
    return fd;
}

int main(void) {
#ifndef POSIX_NATIVE_ORACLE
    print_layout();
#endif
    const char *name = "posix-translated.bin";
    const unsigned char data[4] = {11, 22, 33, 44};
    unsigned char buffer[4] = {0};
    char cwd[4096];
    struct stat a, b;
    struct timeval times[2] = {{1700000000, 123456}, {1700001000, 654321}}, now;
    struct flock lock = {0};
    long page = sysconf(_SC_PAGESIZE);
    CHECK(page >= 4096 && getpid() > 0 && getcwd(cwd, sizeof(cwd)) == cwd && cwd[0] == '/');
    CHECK(mkdir("posix-subdir", 0700) == 0 && rmdir("posix-subdir") == 0);
    int fd = open(name, O_RDWR | O_CREAT | O_EXCL, 0600);
    CHECK(fd >= 0);
    CHECK(open(name, O_RDWR | O_CREAT | O_EXCL, 0600) == -1 && errno == EEXIST);
    CHECK(fcntl(fd, F_SETFD, FD_CLOEXEC) == 0 && fcntl(fd, F_GETFD) == FD_CLOEXEC);
    CHECK((fcntl(fd, F_GETFL) & O_ACCMODE) == O_RDWR);
    CHECK(write(fd, data, sizeof(data)) == 4 && lseek(fd, 0, SEEK_CUR) == 4);
    CHECK(pwrite(fd, data, sizeof(data), 8) == 4 && lseek(fd, 0, SEEK_CUR) == 4);
    CHECK(lseek(fd, 0, SEEK_SET) == 0 && read(fd, buffer, sizeof(buffer)) == 4 && buffer[2] == 33);
    CHECK(pread(fd, buffer, sizeof(buffer), 8) == 4 && buffer[3] == 44);
    CHECK(ftruncate(fd, page) == 0 && fstat(fd, &a) == 0 && a.st_size == page && S_ISREG(a.st_mode));
    CHECK(stat(name, &b) == 0 && a.st_dev == b.st_dev && a.st_ino == b.st_ino && b.st_size == page);
    CHECK(lstat(name, &b) == 0 && S_ISREG(b.st_mode) && access(name, R_OK | W_OK) == 0);
    CHECK(fchmod(fd, 0600) == 0 && fchown(fd, (uid_t)-1, (gid_t)-1) == 0);
    lock.l_type = F_WRLCK; lock.l_whence = SEEK_SET; lock.l_start = 7; lock.l_len = 9;
    CHECK(fcntl(fd, F_SETLK, &lock) == 0);
    lock.l_type = F_WRLCK;
    CHECK(fcntl(fd, F_GETLK, &lock) == 0 && lock.l_type == F_UNLCK);
    lock.l_type = F_UNLCK;
    CHECK(fcntl(fd, F_SETLK, &lock) == 0);
    CHECK(utimes(name, times) == 0 && fstat(fd, &a) == 0 && a.st_atime == 1700000000 && a.st_mtime == 1700001000);
    CHECK(gettimeofday(&now, 0) == 0 && now.tv_sec > 0 && now.tv_usec >= 0 && now.tv_usec < 1000000);
    CHECK(sleep(0) == 0 && usleep(1) == 0);
    unsigned char *shared = mmap(0, (size_t)page, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    CHECK(shared != MAP_FAILED && shared[1] == 22);
    shared[1] = 81;
    CHECK(pread(fd, buffer, sizeof(buffer), 0) == 4 && buffer[1] == 81);
    unsigned char *copy = mmap(0, (size_t)page, PROT_READ | PROT_WRITE, MAP_PRIVATE, fd, 0);
    CHECK(copy != MAP_FAILED && copy[1] == 81);
    copy[1] = 93;
    CHECK(shared[1] == 81 && pread(fd, buffer, sizeof(buffer), 0) == 4 && buffer[1] == 81);
    CHECK(fsync(fd) == 0 && fdatasync(fd) == 0 && close(fd) == 0);
    CHECK(shared[1] == 81 && copy[1] == 93);
    printf("io 11 %u 33 44; private %u; size-page yes\n", shared[1], copy[1]);
    CHECK(munmap(shared, (size_t)page) == 0 && munmap(copy, (size_t)page) == 0);
    CHECK(read(fd, buffer, sizeof(buffer)) == -1 && errno == EBADF);
    CHECK(unlink(name) == 0 && stat(name, &a) == -1 && errno == ENOENT);
    printf("metadata locks time errno paths: passed\n");
    return 0;
}
#endif
