#include <sys/ioctl.h>
#include <errno.h>

/* Run with stdout redirected to a regular file. There are deliberately no
   stdio imports: ioctl itself must arrange standard descriptor registration. */
int main(void) {
    struct winsize size;
    errno = 0;
    int result = ioctl(1, TIOCGWINSZ, &size);
    return result == -1 && errno == ENOTTY ? 0 : (errno == EBADF ? 9 : 1);
}
