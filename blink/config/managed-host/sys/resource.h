#ifndef BLINK_MANAGED_HOST_SYS_RESOURCE_H
#define BLINK_MANAGED_HOST_SYS_RESOURCE_H
#include <sys/time.h>
#include <time.h>
#include "resource-constants.h"
/* LP64 records qualified against native headers. Calls remain unresolved. */
typedef unsigned long rlim_t;
#define RLIM_INFINITY ((rlim_t)-1)
struct rlimit { rlim_t rlim_cur; rlim_t rlim_max; };
struct rusage {
  struct timeval ru_utime, ru_stime;
  long ru_maxrss, ru_ixrss, ru_idrss, ru_isrss;
  long ru_minflt, ru_majflt, ru_nswap, ru_inblock, ru_oublock;
  long ru_msgsnd, ru_msgrcv, ru_nsignals, ru_nvcsw, ru_nivcsw;
};
#define getrlimit blink_host_getrlimit
#define setrlimit blink_host_setrlimit
#define getrusage blink_host_getrusage
#define getpriority blink_host_getpriority
#define setpriority blink_host_setpriority
int getrlimit(int, struct rlimit *);
int setrlimit(int, const struct rlimit *);
int getrusage(int, struct rusage *);
int getpriority(int, unsigned int);
int setpriority(int, unsigned int, int);
#endif
