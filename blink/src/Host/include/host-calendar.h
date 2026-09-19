#ifndef BLINK_HOST_CALENDAR_H
#define BLINK_HOST_CALENDAR_H
#include <time.h>
#define time blink_host_time
#define gmtime_r blink_host_gmtime_r
#define localtime_r blink_host_localtime_r
time_t time(time_t *);
struct tm *gmtime_r(time_t *, struct tm *);
struct tm *localtime_r(time_t *, struct tm *);
#endif
