#include <stdio.h>
int AsyncSocketsCommon(void);
int main(void) {
  int result = AsyncSocketsCommon();
  if (result) { fprintf(stderr, "async socket contract stage %d\n", result); return 1; }
  puts("nonblocking listener and accepted sockets passed");
  puts("drain-to-EAGAIN edges and MSG_PEEK passed");
  puts("opaque data maxEvents alias lifetime and descriptor reuse passed");
  return 0;
}
