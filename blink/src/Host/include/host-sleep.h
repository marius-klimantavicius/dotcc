#ifndef BLINK_PRIVATE_HOST_SLEEP_H
#define BLINK_PRIVATE_HOST_SLEEP_H
#include <time.h>
#define nanosleep blink_host_nanosleep
int nanosleep(const struct timespec *, struct timespec *);
#endif
