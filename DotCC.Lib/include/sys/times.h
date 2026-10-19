#ifndef _SYS_TIMES_H
#define _SYS_TIMES_H

#include <time.h>

/* DotCC's LP64 process-accounting ABI: four signed 64-bit clock_t fields.
   times() and sysconf(_SC_CLK_TCK) use the SAME host tick units; these are
   unrelated to clock()'s existing CLOCKS_PER_SEC millisecond contract.
   Linux LP64 is implemented by the shared runtime's actual OS service.
   Other host ABIs fail with ENOTSUP rather than manufacture CPU counters. */
struct tms {
    clock_t tms_utime;
    clock_t tms_stime;
    clock_t tms_cutime;
    clock_t tms_cstime;
};

/* CPU time of this process and terminated children already waited for;
   return elapsed real-time ticks since an arbitrary fixed point, or -1 with
   errno on failure. Linux permits a null buffer to query elapsed ticks only.
   A non-null buffer must point to valid writable struct tms storage. */
clock_t times(struct tms *buffer);

#endif
