#ifndef BLINK_MANAGED_HOST_SYS_TIME_H
#define BLINK_MANAGED_HOST_SYS_TIME_H
#include "timer-constants.h"
/* Keep the foundational timeval tag/guard shared with dotcc's unistd.h so
 * select/time conversion declarations refer to one C type. LP64 is probed. */
#ifndef _SUSECONDS_T_DEFINED
#define _SUSECONDS_T_DEFINED
typedef long suseconds_t;
#endif
#if !defined(_DOTCC_STRUCT_TIMEVAL) && !defined(__timeval_defined)
#define _DOTCC_STRUCT_TIMEVAL
#define __timeval_defined 1
struct timeval { long tv_sec; long tv_usec; };
#endif
struct timezone { int tz_minuteswest; int tz_dsttime; };
struct itimerval { struct timeval it_interval; struct timeval it_value; };

#define gettimeofday blink_host_gettimeofday
#define settimeofday blink_host_settimeofday
#define getitimer blink_host_getitimer
#define setitimer blink_host_setitimer
int gettimeofday(struct timeval *, void *);
int settimeofday(const struct timeval *, const struct timezone *);
int getitimer(int, struct itimerval *);
int setitimer(int, const struct itimerval *, struct itimerval *);
#endif
