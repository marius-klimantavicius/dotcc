#include <sys/statvfs.h>
#include <sys/ioctl.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <errno.h>

#define CHECK(x) do { if (!(x)) { printf("failure %d errno %d\n", __LINE__, errno); return 1; } ++checks; } while (0)
int main(int argc, char **argv) {
    int checks = 0;
    struct statvfs info;
    CHECK(argc == 2);
    CHECK(statvfs(".", &info) == 0);
    CHECK(info.f_bsize > 0 && info.f_frsize > 0 && info.f_blocks > 0);
    CHECK(info.f_bfree <= info.f_blocks && info.f_bavail <= info.f_bfree);
    CHECK(info.f_ffree <= info.f_files && info.f_favail <= info.f_ffree && info.f_namemax > 0);
    errno = 0;
    CHECK(statvfs("missing-filesystem", &info) == -1 && errno == ENOENT);
    int fd = open(argv[1], O_RDWR);
    CHECK(fd >= 0);
    struct winsize window = {0};
    CHECK(ioctl(fd, TIOCGWINSZ, &window) == 0);
    CHECK(window.ws_row == 37 && window.ws_col == 101 && window.ws_xpixel == 7 && window.ws_ypixel == 11);
    CHECK(close(fd) == 0);
    errno = 0;
    CHECK(ioctl(fd, TIOCGWINSZ, &window) == -1 && errno == EBADF);
    FILE *file = fopen("ordinary-file", "w");
    CHECK(file != NULL && fclose(file) == 0);
    fd = open("ordinary-file", O_RDONLY);
    CHECK(fd >= 0);
    errno = 0;
    CHECK(ioctl(fd, TIOCGWINSZ, &window) == -1 && errno == ENOTTY);
    CHECK(close(fd) == 0 && unlink("ordinary-file") == 0);
    printf("filesystem/terminal %d checks passed\n", checks);
    return checks != 15;
}
