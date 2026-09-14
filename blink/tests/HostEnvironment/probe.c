#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <time.h>
#include <sys/random.h>
#ifdef BLINK_MANAGED_ENVIRONMENT
#include "host-clock.h"
#endif

long ClockSeconds(int clock) { struct timespec t; return clock_gettime(clock,&t) ? -1 : t.tv_sec; }
long ClockNanos(int clock) { struct timespec t; return clock_gettime(clock,&t) ? -1 : t.tv_nsec; }
int EntropySum(void) {
  unsigned char bytes[16]; int sum=0;
  if(getrandom(bytes,sizeof(bytes),0)!=sizeof(bytes)) return -1;
  for(int i=0;i<16;i++)sum+=bytes[i];
  return sum;
}
int HostErrors(void) {
  unsigned char bytes[1024];
  struct timespec t;
  if(clock_gettime(987,&t)!=-1) return 1;
  if(getrandom(bytes,32,8)!=-1 || errno!=EINVAL) return 2;
  if(getentropy(bytes,257)!=-1 || errno!=EIO) return 3;
  return 0;
}
int main(void) {
  struct timespec a,b,real;
  unsigned char bytes[512];
  if(clock_gettime(CLOCK_MONOTONIC,&a) || clock_gettime(CLOCK_MONOTONIC,&b) || clock_gettime(CLOCK_REALTIME,&real)) return 1;
  if(a.tv_nsec<0 || a.tv_nsec>=1000000000 || b.tv_nsec<0 || b.tv_nsec>=1000000000 || real.tv_nsec<0 || real.tv_nsec>=1000000000) return 2;
  if(b.tv_sec<a.tv_sec || (b.tv_sec==a.tv_sec && b.tv_nsec<a.tv_nsec) || real.tv_sec<=0) return 3;
  puts("real clocks: valid nanoseconds and nondecreasing monotonic time: PASS");
  if(getrandom(bytes,64,0)!=64 || getentropy(bytes,64)) return 4;
  if(getrandom(bytes,64,8)!=-1 || errno!=EINVAL) return 5;
  if(getentropy(bytes,257)!=-1 || errno!=EIO) return 6;
  puts("real entropy: complete small requests and explicit invalid requests: PASS");
  printf("clock storage %zu %zu\n",sizeof(struct timespec),_Alignof(struct timespec));
  return 0;
}
