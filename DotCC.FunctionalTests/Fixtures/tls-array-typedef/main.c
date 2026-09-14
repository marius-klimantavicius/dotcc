#include <stdio.h>
#include <threads.h>

typedef unsigned long long slots[2];
_Thread_local slots tls;
unsigned long long *get_tls(void) { return tls; }
void set_tls(unsigned long long value) { tls[0] = value; tls[1] = value + 1; }
unsigned long long sum_tls(void) { return tls[0] + tls[1]; }

static int worker(void *arg) {
  if (sum_tls() != 0) return -1;
  set_tls(*(unsigned long long *)arg);
  return (int)sum_tls();
}
int main(void) {
  thrd_t a, b;
  unsigned long long first = 10, second = 20;
  if (sum_tls() != 0) return 1;
  set_tls(3);
  if (thrd_create(&a, worker, &first) || thrd_create(&b, worker, &second)) return 2;
  int x, y;
  thrd_join(a, &x);
  thrd_join(b, &y);
  printf("tls=%d,%d,%llu size=%zu\n", x, y, sum_tls(), sizeof(tls));
  return x == 21 && y == 41 && sum_tls() == 7 ? 0 : 3;
}
