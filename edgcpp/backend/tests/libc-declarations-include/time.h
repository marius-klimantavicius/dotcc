/* Fixed LP64 time declarations. No host struct tm/timespec crosses this ABI.
   Declaring functions here does not provide their runtime implementations. */
#ifndef __EDG_DOT_LIBC_TIME_H
#define __EDG_DOT_LIBC_TIME_H
#include <stddef.h>
typedef long time_t;
typedef long clock_t;
#define CLOCKS_PER_SEC 1000000L
#define TIME_UTC 1
struct timespec { time_t tv_sec; long tv_nsec; };
struct tm {
    int tm_sec, tm_min, tm_hour, tm_mday, tm_mon, tm_year;
    int tm_wday, tm_yday, tm_isdst;
    long tm_gmtoff;
    const char *tm_zone;
};
#ifdef __cplusplus
extern "C" {
#endif
clock_t clock(void);
double difftime(time_t, time_t);
time_t mktime(struct tm *);
time_t time(time_t *);
char *asctime(const struct tm *);
char *ctime(const time_t *);
struct tm *gmtime(const time_t *);
struct tm *localtime(const time_t *);
size_t strftime(char *, size_t, const char *, const struct tm *);
int timespec_get(struct timespec *, int);
/* Common thread-safe extensions, still gated by provider bindings. */
struct tm *gmtime_r(const time_t *, struct tm *);
struct tm *localtime_r(const time_t *, struct tm *);
char *asctime_r(const struct tm *, char *);
char *ctime_r(const time_t *, char *);
#ifdef __cplusplus
}
#endif
#endif
