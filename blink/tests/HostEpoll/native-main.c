#include <stdio.h>
#include <time.h>
int EpollCommon(void);
int EpollEventSize(void);
int EpollEventAlign(void);
int EpollDataOffset(void);
int main(void) {
  struct timespec start, end;
  if (clock_gettime(CLOCK_MONOTONIC, &start)) return 1;
  int result = EpollCommon();
  if (clock_gettime(CLOCK_MONOTONIC, &end)) return 2;
  long long ns = (end.tv_sec - start.tv_sec) * 1000000000LL + end.tv_nsec - start.tv_nsec;
  if (result || ns < 10000000 || ns > 30000000000LL) {
    fprintf(stderr, "common=%d elapsed_ns=%lld\n", result, ns); return 3;
  }
  printf("linux layout %d %d %d\n", EpollEventSize(), EpollEventAlign(), EpollDataOffset());
  puts("empty epoll common passed");
  return 0;
}
