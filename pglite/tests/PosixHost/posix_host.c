/* Actual system/shared-header ABI and host operations; no database algorithms. */
#include <sys/types.h>
#include <sys/mman.h>
#include <fcntl.h>
#include <unistd.h>
#include <pwd.h>
#include <termios.h>
#include <stddef.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>

size_t dph_layout(unsigned int index)
{
    switch (index)
    {
        case 0: return sizeof(struct passwd);
        case 1: return _Alignof(struct passwd);
        case 2: return offsetof(struct passwd, pw_name);
        case 3: return offsetof(struct passwd, pw_passwd);
        case 4: return offsetof(struct passwd, pw_uid);
        case 5: return offsetof(struct passwd, pw_gid);
        case 6: return offsetof(struct passwd, pw_gecos);
        case 7: return offsetof(struct passwd, pw_dir);
        case 8: return offsetof(struct passwd, pw_shell);
        case 9: return sizeof(struct termios);
        case 10: return _Alignof(struct termios);
        case 11: return offsetof(struct termios, c_iflag);
        case 12: return offsetof(struct termios, c_oflag);
        case 13: return offsetof(struct termios, c_cflag);
        case 14: return offsetof(struct termios, c_lflag);
        case 15: return offsetof(struct termios, c_line);
        case 16: return offsetof(struct termios, c_cc);
        case 17: return offsetof(struct termios, c_ispeed);
        case 18: return offsetof(struct termios, c_ospeed);
        case 19: return sizeof(uid_t);
        case 20: return sizeof(gid_t);
        case 21: return sizeof(size_t);
        case 22: return sizeof(cc_t);
        case 23: return sizeof(speed_t);
        default: return 0;
    }
}

static uint64_t digest_string(uint64_t digest, const char *text)
{
    do { digest = (digest ^ (unsigned char)*text) * UINT64_C(1099511628211); } while (*text++);
    return digest;
}

/* Typed struct** is intentional: it must call the runtime's void* result
   address without an implicit generated-struct** to void** conversion. */
int dph_passwd(uint64_t *digest)
{
    struct { uint64_t before; struct passwd value; uint64_t after; } record;
    unsigned char bytes[16386];
    struct passwd *result = (struct passwd *)1;
    int rc;
    record.before = UINT64_C(0x123456789abcdef0);
    record.after = UINT64_C(0x9876543210abcdef);
    memset(bytes, 0xa5, sizeof(bytes));
    rc = getpwuid_r(getuid(), &record.value, (char *)bytes + 1, 1, &result);
    if (rc != ERANGE || result != NULL) return __LINE__;
    rc = getpwuid_r(getuid(), &record.value, (char *)bytes + 1, sizeof(bytes) - 2, &result);
    if (rc != 0 || result != &record.value || result->pw_uid != getuid()) return __LINE__;
    if (!result->pw_name || !result->pw_dir || !result->pw_shell) return __LINE__;
    if (bytes[0] != 0xa5 || bytes[sizeof(bytes) - 1] != 0xa5) return __LINE__;
    if (record.before != UINT64_C(0x123456789abcdef0) || record.after != UINT64_C(0x9876543210abcdef)) return __LINE__;
    *digest = ((uint64_t)result->pw_uid << 32) | result->pw_gid;
    *digest = digest_string(*digest, result->pw_name);
    *digest = digest_string(*digest, result->pw_dir);
    *digest = digest_string(*digest, result->pw_shell);
    return 0;
}

int dph_missing(int *error)
{
    struct passwd record, *result = (struct passwd *)1;
    char buffer[16384];
    *error = getpwuid_r((uid_t)UINT32_MAX, &record, buffer, sizeof(buffer), &result);
    if ((*error != 0 && *error != ENOENT) || result != NULL) return __LINE__;
    return 0;
}

#define DPH_CHECK(expression) do { if (!(expression)) { status = __LINE__; goto cleanup; } } while (0)

int dph_shared(const char *name)
{
    int status = 0, fd = -1, other = -1, truncated = -1;
    unsigned char value = 0;
    unsigned char *mapping = MAP_FAILED;
    fd = shm_open(name, O_RDWR | O_CREAT | O_EXCL, 0600);
    DPH_CHECK(fd >= 0);
    DPH_CHECK((fcntl(fd, F_GETFD) & FD_CLOEXEC) != 0);
    DPH_CHECK(shm_open(name, O_RDWR | O_CREAT | O_EXCL, 0600) == -1 && errno == EEXIST);
    DPH_CHECK(ftruncate(fd, 4096) == 0);
    mapping = mmap(NULL, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    DPH_CHECK(mapping != MAP_FAILED);
    mapping[83] = 0xa7;
    other = shm_open(name, O_RDONLY, 0);
    DPH_CHECK(other >= 0);
    DPH_CHECK(pread(other, &value, 1, 83) == 1 && value == 0xa7);
    DPH_CHECK(mmap(NULL, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, other, 0) == MAP_FAILED && errno == EACCES);
    DPH_CHECK(shm_unlink(name) == 0);
    DPH_CHECK(shm_open(name, O_RDONLY, 0) == -1 && errno == ENOENT);
    DPH_CHECK(close(fd) == 0); fd = -1;
    mapping[84] = 0xa8;
    DPH_CHECK(pread(other, &value, 1, 84) == 1 && value == 0xa8);
    DPH_CHECK(close(other) == 0); other = -1;
    DPH_CHECK(munmap(mapping, 4096) == 0); mapping = MAP_FAILED;
    /* Recreating a name is a fresh object; truncation is real. */
    fd = shm_open(name, O_RDWR | O_CREAT | O_EXCL, 0600);
    DPH_CHECK(fd >= 0 && ftruncate(fd, 4096) == 0);
    truncated = shm_open(name, O_RDWR | O_TRUNC, 0);
    DPH_CHECK(truncated >= 0 && lseek(truncated, 0, SEEK_END) == 0);
cleanup:
    if (mapping != MAP_FAILED) munmap(mapping, 4096);
    if (fd >= 0) close(fd);
    if (other >= 0) close(other);
    if (truncated >= 0) close(truncated);
    shm_unlink(name);
    return status;
}

int dph_terminal(int fd)
{
    struct termios original, changed, observed;
    int status = 0, restore = 0, action;
    DPH_CHECK(tcgetattr(fd, &original) == 0); restore = 1;
    changed = original;
    changed.c_lflag ^= ECHO;
    changed.c_cc[VEOF] = original.c_cc[VEOF] == 4 ? 5 : 4;
    for (action = TCSANOW; action <= TCSAFLUSH; action++)
    {
        DPH_CHECK(tcsetattr(fd, action, &changed) == 0);
        DPH_CHECK(tcgetattr(fd, &observed) == 0);
        DPH_CHECK(observed.c_lflag == changed.c_lflag && observed.c_cc[VEOF] == changed.c_cc[VEOF]);
        DPH_CHECK(observed.c_iflag == original.c_iflag && observed.c_oflag == original.c_oflag);
        DPH_CHECK(observed.c_cflag == original.c_cflag && observed.c_line == original.c_line);
        DPH_CHECK(observed.c_ispeed == original.c_ispeed && observed.c_ospeed == original.c_ospeed);
    }
    DPH_CHECK(tcsetattr(fd, 17, &changed) == -1 && errno == EINVAL);
cleanup:
    if (restore && tcsetattr(fd, TCSANOW, &original) != 0) return __LINE__;
    return status;
}

int dph_terminal_errors(void)
{
    struct termios attributes = {0};
    int fd = open("/dev/null", O_RDWR);
    if (fd < 0) return __LINE__;
    if (tcgetattr(fd, &attributes) != -1 || errno != ENOTTY) { close(fd); return __LINE__; }
    if (tcsetattr(fd, TCSANOW, &attributes) != -1 || errno != ENOTTY) { close(fd); return __LINE__; }
    close(fd);
    if (tcgetattr(-1, &attributes) != -1 || errno != EBADF) return __LINE__;
    return 0;
}

#ifdef DPH_NATIVE_MAIN
#include <stdio.h>
#include <stdlib.h>
/* Test-process-only selection of a real NSS provider. No product import,
   configuration file, parent process or persistent host state is modified. */
extern int __nss_configure_lookup(const char *, const char *);
int main(int argc, char **argv)
{
    uint64_t digest = 0;
    int master = -1, slave = -1, pty_status = -1, status, missing_error = -1;
    char path[256];
    unsigned int index;
    if (argc != 2 && argc != 3) return 24;
    if (argc == 3 && (strcmp(argv[2], "files") != 0 || __nss_configure_lookup("passwd", "files") != 0)) return 25;
    if ((status = dph_passwd(&digest)) || (status = dph_missing(&missing_error)) ||
        (status = dph_shared(argv[1])) || (status = dph_terminal_errors()))
    { fprintf(stderr, "host contract failure at line %d\n", status); return 1; }
    master = posix_openpt(O_RDWR | O_NOCTTY | O_CLOEXEC);
    if (master >= 0 && grantpt(master) == 0 && unlockpt(master) == 0 && ptsname_r(master, path, sizeof(path)) == 0)
    {
        slave = open(path, O_RDWR | O_NOCTTY | O_CLOEXEC);
        if (slave >= 0) pty_status = dph_terminal(slave);
    }
    if (slave >= 0) close(slave);
    if (master >= 0) close(master);
    if (pty_status > 0) { fprintf(stderr, "PTY contract failure at line %d\n", pty_status); return 1; }
    printf("{\"layout\":[");
    for (index = 0; index < 24; index++) printf("%s%zu", index ? "," : "", dph_layout(index));
    printf("],\"nss_digest\":%llu,\"missing_error\":%d,\"pty_available\":%s,\"shared\":true,\"errors\":true}\n",
           (unsigned long long)digest, missing_error, pty_status == 0 ? "true" : "false");
    return 0;
}
#endif
