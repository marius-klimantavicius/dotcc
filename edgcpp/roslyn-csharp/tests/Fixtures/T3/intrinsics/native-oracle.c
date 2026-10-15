#include <pthread.h>
#include <stdio.h>

extern int fixture(void);
extern int intrinsic_count(void);
extern int intrinsic_trace(void);
extern long long intrinsic_observation(int);
extern void intrinsic_publish(int);
extern int intrinsic_consume(void);
static int mismatch;
static long long sum;
static void *consume_all(void *unused)
{
  (void)unused;
  for (int expected = 1; expected <= 1000; ++expected) {
    int actual = intrinsic_consume();
    if (actual != expected) mismatch = 1;
    sum += actual;
  }
  return NULL;
}
int main(void)
{
  int result = fixture();
  printf("result=%d\nobservations=%d\n", result, intrinsic_count());
  for (int i = 0; i < intrinsic_count(); ++i) printf("%d:%lld\n", i, intrinsic_observation(i));
  printf("trace=%d\n", intrinsic_trace());
  pthread_t worker;
  if (pthread_create(&worker, NULL, consume_all, NULL) != 0) return 1;
  for (int value = 1; value <= 1000; ++value) intrinsic_publish(value);
  if (pthread_join(worker, NULL) != 0 || mismatch || sum != 500500) return 1;
  printf("handoffs=1000\nhandoff-sum=%lld\n", sum);
  return 0;
}
