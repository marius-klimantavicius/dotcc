#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <sys/time.h>
#include <time.h>
#ifdef BLINK_MANAGED_CLOCKS
#include "host-clock.h"
#endif
#define CHECK(x) do { if(!(x)){printf("clock failure %d\n",__LINE__);return __LINE__;} } while(0)
int ClockProbe(void) {
  struct timeval tv;
  struct timespec resolution={-1,-1};
  CHECK(!gettimeofday(&tv,0) && tv.tv_sec>0 && tv.tv_usec>=0 && tv.tv_usec<1000000);
  CHECK(!clock_getres(CLOCK_REALTIME,&resolution));
  CHECK(resolution.tv_sec>=0 && resolution.tv_nsec>=0 && resolution.tv_nsec<1000000000 && (resolution.tv_sec || resolution.tv_nsec));
  CHECK(!clock_getres(CLOCK_MONOTONIC,&resolution));
  CHECK(resolution.tv_sec>=0 && resolution.tv_nsec>=0 && resolution.tv_nsec<1000000000 && (resolution.tv_sec || resolution.tv_nsec));
  CHECK(!clock_getres(CLOCK_MONOTONIC,0));
  resolution.tv_sec=71;resolution.tv_nsec=92;
  CHECK(clock_getres(9876,&resolution)==-1 && errno==EINVAL && resolution.tv_sec==71 && resolution.tv_nsec==92);
  CHECK(clock_getres(9876,0)==-1 && errno==EINVAL);
  errno=123;CHECK(!gettimeofday(&tv,0) && errno==123);
  CHECK(!clock_getres(CLOCK_REALTIME,&resolution) && errno==123);
  puts("timeofday and clock resolution: valid output, optional result, errors, errno: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_CLOCKS
int ClockInjected(long seconds, long microseconds, long monotonic_sec, long monotonic_nsec) {
  struct timeval tv;
  struct timespec resolution;
  CHECK(!gettimeofday(&tv,0) && tv.tv_sec==seconds && tv.tv_usec==microseconds);
  CHECK(!clock_getres(CLOCK_REALTIME,&resolution) && !resolution.tv_sec && resolution.tv_nsec==100);
  CHECK(!clock_getres(CLOCK_MONOTONIC,&resolution) && resolution.tv_sec==monotonic_sec && resolution.tv_nsec==monotonic_nsec);
  return 0;
}
int ClockPrivateErrors(void) {
  struct timeval tv={71,92};struct timezone zone={0,0};
  CHECK(gettimeofday(0,0)==-1 && errno==EFAULT);
  CHECK(gettimeofday(&tv,&zone)==-1 && errno==ENOTSUP && tv.tv_sec==71 && tv.tv_usec==92);
  return 0;
}
int ClockProviderError(int resolution_error) {
  struct timeval tv={71,92};struct timespec resolution={71,92};
  if(resolution_error) {
    CHECK(clock_getres(CLOCK_MONOTONIC,&resolution)==-1 && errno==EIO && resolution.tv_sec==71 && resolution.tv_nsec==92);
  } else {
    CHECK(gettimeofday(&tv,0)==-1 && errno==EIO && tv.tv_sec==71 && tv.tv_usec==92);
  }
  return 0;
}
int ClockUnbound(void) {
  struct timeval tv;
  CHECK(gettimeofday(&tv,0)==-1 && errno==19);
  CHECK(clock_getres(CLOCK_MONOTONIC,0)==-1 && errno==19);
  return 0;
}
int UsesClockNanosleep(void) {
#if defined(TIMER_ABSTIME) && !defined(__OpenBSD__)
  return 1;
#else
  return 0;
#endif
}
#else
int main(void){return ClockProbe();}
#endif
