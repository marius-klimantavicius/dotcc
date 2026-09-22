#define _GNU_SOURCE 1
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <sys/time.h>
#include <time.h>
#ifdef BLINK_MANAGED_COARSE_CLOCK
#include "host-clock.h"
#endif
#define CHECK(x) do { if (!(x)) { printf("coarse clock failure %d\n", __LINE__); return __LINE__; } } while (0)
struct Guarded { uint64_t before; struct timespec value; uint64_t after; };
static const uint64_t guard = UINT64_C(0x123456789abcdef0);
static int pointers;
static int clocks;
static struct timespec *Output(struct Guarded *out) { ++pointers; return &out->value; }
static int Clock(void) { ++clocks; return CLOCK_MONOTONIC_COARSE; }
static int Guards(struct Guarded *out) { return out->before == guard && out->after == guard; }
static int Normal(struct timespec *value) { return value->tv_sec >= 0 && value->tv_nsec >= 0 && value->tv_nsec < 1000000000; }

int CoarseClockProbe(void) {
  struct Guarded out = {guard, {71, 92}, guard};
  struct timespec previous = {0, 0};
  pointers = clocks = 0;
  errno = 123;
  CHECK(!clock_getres(Clock(), Output(&out)) && pointers == 1 && clocks == 1);
  CHECK(Guards(&out) && Normal(&out.value) && (out.value.tv_sec || out.value.tv_nsec) && errno == 123);
  CHECK(!clock_getres(CLOCK_MONOTONIC_COARSE, 0));
  for (int i = 0; i < 64; ++i) {
    CHECK(!clock_gettime(Clock(), Output(&out)) && pointers == i + 2 && clocks == i + 2);
    CHECK(Guards(&out) && Normal(&out.value) && errno == 123);
    CHECK(out.value.tv_sec > previous.tv_sec || (out.value.tv_sec == previous.tv_sec && out.value.tv_nsec >= previous.tv_nsec));
#ifdef BLINK_MANAGED_COARSE_CLOCK
    CHECK(out.value.tv_nsec % 1000000 == 0);
#endif
    previous = out.value;
  }
  puts("coarse monotonic clock: valid output, guards, argument evaluation, nondecreasing time and resolution: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_COARSE_CLOCK
int CoarseClockInjected(long seconds, long nanoseconds, long fine_nanoseconds,
                        long resolution_seconds, long resolution_nanoseconds) {
  struct Guarded out = {guard, {71, 92}, guard};
  pointers = clocks = 0;
  errno = 123;
  CHECK(!clock_gettime(Clock(), Output(&out)) && clocks == 1 && pointers == 1);
  CHECK(Guards(&out) && out.value.tv_sec == seconds && out.value.tv_nsec == nanoseconds && errno == 123);
  CHECK(!clock_gettime(CLOCK_MONOTONIC, &out.value));
  CHECK(Guards(&out) && out.value.tv_sec == seconds && out.value.tv_nsec == fine_nanoseconds);
  CHECK(!clock_gettime(CLOCK_REALTIME, &out.value));
  CHECK(Guards(&out) && out.value.tv_sec == 123 && out.value.tv_nsec == 456700);
  CHECK(!clock_getres(Clock(), Output(&out)) && clocks == 2 && pointers == 2);
  CHECK(Guards(&out) && out.value.tv_sec == resolution_seconds && out.value.tv_nsec == resolution_nanoseconds && errno == 123);
  return 0;
}
#else
int main(void) { return CoarseClockProbe(); }
#endif
