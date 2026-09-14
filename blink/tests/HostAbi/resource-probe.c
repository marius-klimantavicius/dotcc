#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdio.h>
#include <time.h>
#include <sys/resource.h>
#define SIZE(t) printf(#t ".size %zu\n" #t ".alignment %zu\n", sizeof(t), _Alignof(t))
#define OFFSET(t, f) printf(#t "." #f " %zu\n", offsetof(t, f))
int main(void) {
  SIZE(rlim_t);
  SIZE(clock_t);
  SIZE(struct rlimit);
  OFFSET(struct rlimit, rlim_cur);
  OFFSET(struct rlimit, rlim_max);
  SIZE(struct rusage);
  OFFSET(struct rusage, ru_utime);
  OFFSET(struct rusage, ru_stime);
  OFFSET(struct rusage, ru_maxrss);
  OFFSET(struct rusage, ru_ixrss);
  OFFSET(struct rusage, ru_idrss);
  OFFSET(struct rusage, ru_isrss);
  OFFSET(struct rusage, ru_minflt);
  OFFSET(struct rusage, ru_majflt);
  OFFSET(struct rusage, ru_nswap);
  OFFSET(struct rusage, ru_inblock);
  OFFSET(struct rusage, ru_oublock);
  OFFSET(struct rusage, ru_msgsnd);
  OFFSET(struct rusage, ru_msgrcv);
  OFFSET(struct rusage, ru_nsignals);
  OFFSET(struct rusage, ru_nvcsw);
  OFFSET(struct rusage, ru_nivcsw);
  printf("RLIM_INFINITY %llu\n", (unsigned long long)RLIM_INFINITY);
  printf("rlim_t.unsigned %d\n", (rlim_t)-1 > 0);
  return 0;
}
