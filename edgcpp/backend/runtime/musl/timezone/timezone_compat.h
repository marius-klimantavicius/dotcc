/* Platform boundary for unchanged musl timezone rule/conversion algorithms. */
#ifndef EDG_TIMEZONE_COMPAT_H
#define EDG_TIMEZONE_COMPAT_H
#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>
#include <time.h>
#include <limits.h>
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
void __edg_timezone_enter(void);
void __edg_timezone_exit(void);
unsigned char *__edg_timezone_map(const char *, size_t *);
int __edg_timezone_unmap(void *, size_t);
char *__edg_calendar_utc(void);
char *__edg_calendar_zone(const char *);
struct tm *__localtime_r(const time_t *, struct tm *);
#ifdef __cplusplus
}
#endif
#define __tm_gmtoff tm_gmtoff
#define __tm_zone tm_zone
#define __utc (__edg_calendar_utc())
#define __timezone timezone
#define __daylight daylight
#define __tzname tzname
#define LOCK(unused) __edg_timezone_enter()
#define UNLOCK(unused) __edg_timezone_exit()
#define __map_file __edg_timezone_map
#define __munmap __edg_timezone_unmap
/* Generated owners have no setuid execution mode. Host policy controls files. */
#define __EDG_TIMEZONE_SECURE 0
#ifndef NAME_MAX
#define NAME_MAX 255
#endif
#ifndef PATH_MAX
#define PATH_MAX 4096
#endif
#ifndef TZNAME_MAX
#define TZNAME_MAX 6
#endif
#endif
