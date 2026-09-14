#ifndef BLINK_MANAGED_HOST_SYS_TIMES_H
#define BLINK_MANAGED_HOST_SYS_TIMES_H
#include <time.h>
/* Source-required LP64 process-time record; no clock implementation. */
struct tms { clock_t tms_utime, tms_stime, tms_cutime, tms_cstime; };
#define times blink_host_times
clock_t times(struct tms *);
#endif
