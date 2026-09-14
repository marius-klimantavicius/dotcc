#include <stddef.h>
#include <stdio.h>
#include <sys/times.h>
#define SIZE(t) printf(#t ".size %zu\n" #t ".alignment %zu\n", sizeof(t), _Alignof(t))
#define OFFSET(t, f) printf(#t "." #f " %zu\n", offsetof(t, f))
int main(void) {
  SIZE(struct tms);
  OFFSET(struct tms, tms_utime);
  OFFSET(struct tms, tms_stime);
  OFFSET(struct tms, tms_cutime);
  OFFSET(struct tms, tms_cstime);
  return 0;
}
