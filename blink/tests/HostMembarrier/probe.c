#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#ifdef BLINK_MANAGED_MEMBARRIER
#include "host-membarrier.h"
#define CallBarrier blink_host_membarrier
#else
#include <sys/syscall.h>
#include <unistd.h>
static int CallBarrier(int command, unsigned flags, int cpu_id) {
  return (int)syscall(SYS_membarrier, command, flags, cpu_id);
}
#endif
#define CHECK(x) do { if (!(x)) { printf("membarrier failure %d\n", __LINE__); return __LINE__; } } while (0)

int MembarrierProbe(void) {
  int mask;
  volatile int value = 0;
  errno = 37;
  mask = CallBarrier(0, 0, 0);
  CHECK(mask >= 0 && (mask & 24) == 24 && errno == 37);
  printf("query subset=%d\n", mask & 24);
  CHECK(CallBarrier(16, 0, 0) == 0 && errno == 37);
  CHECK(CallBarrier(16, 0, 0) == 0 && errno == 37);
  puts("registration repeated=2");
  value = 42;
  CHECK(CallBarrier(8, 0, 0) == 0 && errno == 37);
  CHECK(value == 42);
  value += 10;
  CHECK(CallBarrier(8, 0, 0) == 0 && errno == 37);
  CHECK(value == 52);
  printf("fences=2 value=%d\n", value);
  return 0;
}
#ifndef BLINK_MANAGED_MEMBARRIER
int main(void) { return MembarrierProbe(); }
#endif
