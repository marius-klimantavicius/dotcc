#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <time.h>
#include <stdint.h>
#ifdef BLINK_MANAGED_SLEEP
#include "host-sleep.h"
#include "host-clock.h"
#else
#include <signal.h>
#include <unistd.h>
#endif
#define CHECK(x) do { if(!(x)){printf("sleep failure %d\n",__LINE__);return __LINE__;} } while(0)
int SleepProbe(void) {
  struct timespec req={0,10000000},rem={71,92},before,after;
  CHECK(!clock_gettime(CLOCK_MONOTONIC,&before));
  errno=123;CHECK(!nanosleep(&req,&rem) && errno==123);
  CHECK(!clock_gettime(CLOCK_MONOTONIC,&after));
  CHECK((after.tv_sec-before.tv_sec)*1000000000L+after.tv_nsec-before.tv_nsec>=10000000);
  req.tv_nsec=-1;CHECK(nanosleep(&req,&rem)==-1 && errno==EINVAL && rem.tv_sec==71 && rem.tv_nsec==92);
  req.tv_nsec=1000000000;CHECK(nanosleep(&req,&rem)==-1 && errno==EINVAL);
  req.tv_sec=-1;req.tv_nsec=0;CHECK(nanosleep(&req,&rem)==-1 && errno==EINVAL);
  req.tv_sec=0;errno=123;CHECK(!nanosleep(&req,0) && errno==123);
  puts("relative sleep: monotonic deadline, invalid input, optional output, errno: PASS");return 0;
}
int SleepInterrupted(long seconds) {
  struct timespec req={seconds,0},rem={-1,-1};
  int (*call)(const struct timespec *,struct timespec *)=nanosleep;
  CHECK(call(&req,&rem)==-1 && errno==EINTR);
  CHECK(rem.tv_sec>=0 && rem.tv_sec<=seconds && rem.tv_nsec>=0 && rem.tv_nsec<1000000000);
  CHECK(rem.tv_sec || rem.tv_nsec);return 0;
}
#ifdef BLINK_MANAGED_SLEEP
int SleepErrorProbe(int expected) {
  struct timespec req={0,0},rem={71,92};
  CHECK(nanosleep(&req,&rem)==-1 && errno==expected && rem.tv_sec==71 && rem.tv_nsec==92);return 0;
}
int SleepNull(void) { CHECK(nanosleep(0,0)==-1 && errno==EFAULT);return 0; }
#else
static void Handler(int sig) {(void)sig;}
int main(void) {
  if(SleepProbe())return 1;
  struct sigaction action={0};action.sa_handler=Handler;sigemptyset(&action.sa_mask);
  if(sigaction(SIGALRM,&action,0))return 2;
  ualarm(20000,0);return SleepInterrupted(1);
}
#endif
