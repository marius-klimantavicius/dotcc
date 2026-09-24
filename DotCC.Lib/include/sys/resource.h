#ifndef _SYS_RESOURCE_H
#define _SYS_RESOURCE_H

/* dotcc's <sys/resource.h> — just the getrusage() surface chibi's (chibi time)
   touches. dotcc has no portable per-process CPU accounting, so getrusage()
   zeroes the struct and returns success (best-effort, like chmod): any CPU-time
   delta a caller computes is 0, which is harmless for the R7RS time tests that
   use wall-clock (gettimeofday) rather than CPU time. */

#include <sys/time.h>   /* struct timeval */

#define RUSAGE_SELF     0
#define RUSAGE_CHILDREN (-1)

/* Linux LP64 resource-limit declarations. Runtime policy is provided by the
   hosting environment; these declarations do not change process limits. */
typedef unsigned long rlim_t;
struct rlimit { rlim_t rlim_cur; rlim_t rlim_max; };
#define RLIM_INFINITY ((rlim_t)-1)
#define RLIM_SAVED_MAX RLIM_INFINITY
#define RLIM_SAVED_CUR RLIM_INFINITY
#define RLIMIT_CPU 0
#define RLIMIT_FSIZE 1
#define RLIMIT_DATA 2
#define RLIMIT_STACK 3
#define RLIMIT_CORE 4
#define RLIMIT_RSS 5
#define RLIMIT_NPROC 6
#define RLIMIT_NOFILE 7
#define RLIMIT_OFILE RLIMIT_NOFILE
#define RLIMIT_MEMLOCK 8
#define RLIMIT_AS 9
#define RLIMIT_LOCKS 10
#define RLIMIT_SIGPENDING 11
#define RLIMIT_MSGQUEUE 12
#define RLIMIT_NICE 13
#define RLIMIT_RTPRIO 14
#define RLIMIT_RTTIME 15
#define RLIMIT_NLIMITS 16
#define RLIM_NLIMITS RLIMIT_NLIMITS

int getrlimit(int resource, struct rlimit *limit);
int setrlimit(int resource, const struct rlimit *limit);

struct rusage {
    struct timeval ru_utime;   /* user CPU time */
    struct timeval ru_stime;   /* system CPU time */
    long ru_maxrss;
    long ru_ixrss;
    long ru_idrss;
    long ru_isrss;
    long ru_minflt;
    long ru_majflt;
    long ru_nswap;
    long ru_inblock;
    long ru_oublock;
    long ru_msgsnd;
    long ru_msgrcv;
    long ru_nsignals;
    long ru_nvcsw;
    long ru_nivcsw;
};

int getrusage(int who, struct rusage *usage);

#endif /* _SYS_RESOURCE_H */
