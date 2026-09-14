#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdio.h>
#include <sys/time.h>

#define SIZE(t) printf(#t ".size %zu\n" #t ".alignment %zu\n", sizeof(t), _Alignof(t))
#define OFFSET(t, f) printf(#t "." #f " %zu\n", offsetof(t, f))
int main(void) {
  SIZE(suseconds_t);
  SIZE(struct timeval);
  OFFSET(struct timeval, tv_sec);
  OFFSET(struct timeval, tv_usec);
  SIZE(struct timezone);
  OFFSET(struct timezone, tz_minuteswest);
  OFFSET(struct timezone, tz_dsttime);
  SIZE(struct itimerval);
  OFFSET(struct itimerval, it_interval);
  OFFSET(struct itimerval, it_value);
  return 0;
}
