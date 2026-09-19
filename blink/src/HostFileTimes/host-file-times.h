#ifndef BLINK_PRIVATE_FILE_TIMES_H
#define BLINK_PRIVATE_FILE_TIMES_H
#include <time.h>
#ifndef UTIME_NOW
#define UTIME_NOW 1073741823L
#endif
#ifndef UTIME_OMIT
#define UTIME_OMIT 1073741822L
#endif
#if UTIME_NOW != 1073741823L || UTIME_OMIT != 1073741822L
#error timestamp sentinel ABI mismatch
#endif
#define futimens blink_host_futimens
#define utimensat blink_host_utimensat
int futimens(int, const struct timespec *);
int utimensat(int, const char *, const struct timespec *, int);
#endif
