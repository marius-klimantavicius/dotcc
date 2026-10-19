/* Independent native control; keep this same C unchanged for future emission. */
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#include <limits.h>
#include <stdio.h>
#include <string.h>

static int checks, failures;
static ssize_t (*volatile host_readlink)(const char *, char *, size_t) = readlink;
static int (*volatile host_symlink)(const char *, const char *) = symlink;
static void check(const char *name, int value)
{
    ++checks;
    if (!value) { ++failures; printf("FAIL %s errno=%d\n", name, errno); }
}

static void error_case(const char *name, long result, int expected)
{
    int error = errno;
    printf("ERROR %s %ld %d\n", name, result, error);
    check(name, result == -1 && error == expected);
}

static void read_case(const char *name, const char *path, size_t capacity, long expected, int expected_error)
{
    unsigned char buffer[32];
    memset(buffer, 0x5a, sizeof(buffer));
    errno = 123;
    /* The kernel may accept a large advertised capacity for this small target.
       A real 32-byte object still contains every byte it can actually write. */
    long result = host_readlink(path, (char *)buffer + 4, capacity);
    int error = errno;
    printf("READ %s %ld %d ", name, result, error);
    for (unsigned i = 0; i < sizeof(buffer); ++i) printf("%02x", buffer[i]);
    printf("\n");
    fflush(stdout);
    check(name, result == expected && error == expected_error);
    for (int i = 0; i < 32; ++i)
        check("buffer-extent", buffer[i] == ((expected > 0 && i >= 4 && i < 4 + expected) ? (unsigned char)"../target"[i - 4] : 0x5a));
}

int symlink_boundary_r39(void)
{
    char buffer[32];
    memset(buffer, 0x5a, sizeof(buffer));
    int fd = open("regular", O_WRONLY | O_CREAT | O_EXCL, 0600);
    check("setup-regular", fd >= 0);
    if (fd >= 0) check("close-regular", close(fd) == 0);
    check("setup-directory", mkdir("directory", 0700) == 0);
    check("setup-denied", mkdir("denied", 0700) == 0);
    check("setup-link", symlink("../target", "link") == 0);
    check("setup-broken", symlink("absent", "broken") == 0);
    check("setup-loop", symlink("loop", "loop") == 0);
    errno = 0; error_case("read-missing", readlink("absent", buffer, sizeof(buffer)), ENOENT);
    errno = 0; error_case("read-missing-parent", readlink("absent/link", buffer, sizeof(buffer)), ENOENT);
    errno = 0; error_case("read-not-directory", readlink("regular/link", buffer, sizeof(buffer)), ENOTDIR);
    errno = 0; error_case("read-loop-parent", readlink("loop/link", buffer, sizeof(buffer)), ELOOP);
    errno = 0; error_case("read-regular", readlink("regular", buffer, sizeof(buffer)), EINVAL);
    errno = 0; error_case("read-directory", readlink("directory", buffer, sizeof(buffer)), EINVAL);
    errno = 0; error_case("read-empty", readlink("", buffer, sizeof(buffer)), ENOENT);
    errno = 0; error_case("create-missing-parent", symlink("value", "absent/link"), ENOENT);
    errno = 0; error_case("create-not-directory", symlink("value", "regular/link"), ENOTDIR);
    errno = 0; error_case("create-loop-parent", symlink("value", "loop/link"), ELOOP);
    errno = 0; error_case("create-existing-regular", symlink("value", "regular"), EEXIST);
    errno = 0; error_case("create-existing-directory", symlink("value", "directory"), EEXIST);
    errno = 0; error_case("create-existing-broken", symlink("value", "broken"), EEXIST);
    errno = 0; error_case("create-existing-loop", symlink("value", "loop"), EEXIST);
    errno = 0; error_case("create-empty-target", symlink("", "empty-target"), ENOENT);
    errno = 0; error_case("create-empty-path", symlink("value", ""), ENOENT);
    errno = 0; error_case("read-null-path", host_readlink(NULL, buffer, sizeof(buffer)), EFAULT);
    errno = 0; error_case("read-null-buffer", host_readlink("link", NULL, sizeof(buffer)), EFAULT);
    errno = 0; error_case("read-zero-null", host_readlink(NULL, NULL, 0), EINVAL);
    errno = 0; error_case("create-null-target", host_symlink(NULL, "null-target"), EFAULT);
    errno = 0; error_case("create-null-path", host_symlink("value", NULL), EFAULT);
    check("deny-search", chmod("denied", 0) == 0);
    errno = 0; error_case("read-denied", readlink("denied/link", buffer, sizeof(buffer)), EACCES);
    errno = 0; error_case("create-denied", symlink("value", "denied/link"), EACCES);
    check("restore-search", chmod("denied", 0700) == 0);
    for (unsigned i = 0; i < sizeof(buffer); ++i) check("error-buffer-untouched", buffer[i] == 0x5a);
    read_case("full", "link", 32, 9, 123);
    read_case("truncated", "link", 3, 3, 123);
    read_case("zero", "link", 0, -1, EINVAL);
    read_case("int-max", "link", INT_MAX, 9, 123);
    read_case("int-max-plus-one", "link", (size_t)INT_MAX + 1, -1, EINVAL);
    read_case("uint-max", "link", UINT_MAX, -1, EINVAL);
    read_case("uint-max-plus-one", "link", (size_t)UINT_MAX + 1, -1, EINVAL);
    read_case("uint-max-plus-four", "link", (size_t)UINT_MAX + 4, 3, 123);
    read_case("size-max", "link", (size_t)-1, -1, EINVAL);
    printf("SUMMARY checks=%d failures=%d\n", checks, failures);
    return failures ? 1 : 0;
}

int main(void) { return symlink_boundary_r39(); }
