#include <stdio.h>
#include <time.h>
long long TimeoutMilliseconds(void) {
  struct timespec value;
  if (clock_gettime(CLOCK_MONOTONIC, &value)) return -1;
  return (long long)value.tv_sec * 1000 + value.tv_nsec / 1000000;
}
int TimeoutCommon(void);
int main(void) {
  int result = TimeoutCommon();
  if (result) { fprintf(stderr, "native socket timeout assertion=%d\n", result); return 1; }
  puts("timeval16; five-second options; dup inheritance; accept recv read readv recvmsg expiry; normal transfers; zero reset");
  return 0;
}
