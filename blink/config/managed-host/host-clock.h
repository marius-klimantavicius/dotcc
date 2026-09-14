#ifndef BLINK_MANAGED_HOST_CLOCK_H
#define BLINK_MANAGED_HOST_CLOCK_H
#include <time.h>
/* Explicit C ABI clock identifiers, independent of the managed host OS. */
#define CLOCK_REALTIME 0
#define CLOCK_MONOTONIC 1
#define clock_gettime blink_host_clock_gettime
int clock_gettime(int, struct timespec *);
#define clock_getres blink_host_clock_getres
int clock_getres(int, struct timespec *);
#endif
