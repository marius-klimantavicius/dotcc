#include <unistd.h>

/* Declare this before any other include can supply ssize_t. */
struct tracker { void (*track)(void *owner, ssize_t delta); };
static ssize_t total;
static void adjust(void *owner, ssize_t delta) { (void)owner; total += delta; }

#include <sys/ioctl.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    struct tracker tracker = {adjust};
    struct winsize window = {24, 80, 640, 480};
    tracker.track(&tracker, -9);
    tracker.track(&tracker, 3);
    printf("%ld %lu %lu\n", (long)total, (unsigned long)sizeof(ssize_t),
           (unsigned long)sizeof(size_t));
    printf("%lu %lu %lu %lu\n", (unsigned long)sizeof(window),
           (unsigned long)_Alignof(struct winsize),
           (unsigned long)offsetof(struct winsize, ws_col),
           (unsigned long)offsetof(struct winsize, ws_ypixel));
    printf("%u %u %u %u %x %x\n", (unsigned)window.ws_row,
           (unsigned)window.ws_col, (unsigned)window.ws_xpixel,
           (unsigned)window.ws_ypixel, TIOCGWINSZ, TIOCSWINSZ);
    return 0;
}
