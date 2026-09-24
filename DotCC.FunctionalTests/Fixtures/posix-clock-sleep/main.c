#define _POSIX_C_SOURCE 200809L
#include <time.h>
#include <errno.h>
#include <stdio.h>

int main(void) {
    struct timespec before, after, realtime;
    struct timespec delay = {0, 1234567};
    if (clock_gettime(CLOCK_MONOTONIC, &before) || nanosleep(&delay, NULL) ||
        clock_gettime(CLOCK_MONOTONIC, &after) || clock_gettime(CLOCK_REALTIME, &realtime)) return 1;
    long elapsed = (after.tv_sec - before.tv_sec) * 1000000000L + after.tv_nsec - before.tv_nsec;
    if (elapsed < delay.tv_nsec || after.tv_nsec < 0 || after.tv_nsec >= 1000000000L) return 2;
    time_t now = time(NULL);
    if (realtime.tv_sec > now || now - realtime.tv_sec > 2) return 3;
    delay.tv_nsec = 1000000000L;
    if (nanosleep(&delay, NULL) != -1 || errno != EINVAL) return 4;
    if (clock_gettime(-1, &after) != -1 || errno != EINVAL) return 5;
    puts("realtime monotonic sleep errors ok");
    return 0;
}
