/* Same unchanged C is required natively and through immutable V21. */
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

static int checks, failures;
static void check(const char *name, int value)
{
    ++checks;
    if (!value) { ++failures; printf("FAIL %s errno=%d\n", name, errno); }
    else printf("PASS %s\n", name);
}

int symlink_control_r37(void)
{
    char buffer[32];
    struct stat st;
    int fd = open("target", O_WRONLY | O_CREAT | O_EXCL, 0600);
    check("create", fd >= 0);
    if (fd >= 0) {
        check("write", write(fd, "hello", 5) == 5);
        check("close", close(fd) == 0);
    }
    check("symlink", symlink("target", "link") == 0);
    memset(buffer, 0x5a, sizeof(buffer));
    check("readlink", readlink("link", buffer, sizeof(buffer)) == 6 && !memcmp(buffer, "target", 6) && buffer[6] == 0x5a);
    memset(buffer, 0x5a, sizeof(buffer));
    check("truncated-readlink", readlink("link", buffer, 3) == 3 && !memcmp(buffer, "tar", 3) && buffer[3] == 0x5a);
    check("lstat-link", lstat("link", &st) == 0 && S_ISLNK(st.st_mode) && st.st_size == 6);
    check("stat-follows-link", stat("link", &st) == 0 && S_ISREG(st.st_mode) && st.st_size == 5);
    check("broken-symlink", symlink("missing", "broken") == 0);
    check("lstat-broken", lstat("broken", &st) == 0 && S_ISLNK(st.st_mode) && st.st_size == 7);
    check("readlink-broken", readlink("broken", buffer, sizeof(buffer)) == 7 && !memcmp(buffer, "missing", 7));
    errno = 0;
    check("stat-broken-ENOENT", stat("broken", &st) == -1 && errno == ENOENT);
    errno = 0;
    check("duplicate-link-EEXIST", symlink("target", "link") == -1 && errno == EEXIST);
    errno = 0;
    check("duplicate-broken-EEXIST", symlink("missing", "broken") == -1 && errno == EEXIST);
    errno = 0;
    check("readlink-regular-EINVAL", readlink("target", buffer, sizeof(buffer)) == -1 && errno == EINVAL);
    errno = 0;
    check("readlink-missing-ENOENT", readlink("absent", buffer, sizeof(buffer)) == -1 && errno == ENOENT);
    errno = 0;
    check("symlink-missing-parent-ENOENT", symlink("target", "absent/link") == -1 && errno == ENOENT);
    printf("checks=%d failures=%d\n", checks, failures);
    return failures ? 1 : 0;
}

int main(void) { return symlink_control_r37(); }
