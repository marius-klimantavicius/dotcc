#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <sched.h>
#include <sys/time.h>
#ifdef BLINK_MANAGED_YIELD
#include "host-yield.h"
#endif
#define CHECK(x) do{if(!(x)){printf("yield failure %d\n",__LINE__);return __LINE__;}}while(0)
int YieldProbe(void) {
  for(int i=0;i<1000;++i){errno=123;CHECK(!sched_yield() && errno==123);}
  int (*call)(void)=sched_yield;CHECK(!call() && errno==123);
  puts("scheduler yield: successful hint, errno and function pointer: PASS");return 0;
}
#ifdef BLINK_MANAGED_YIELD
int YieldWorker(int tag) {for(int i=0;i<1000;++i){errno=tag;CHECK(!sched_yield() && errno==tag);}return 0;}
int YieldUnbound(void){CHECK(sched_yield()==-1 && errno==ENODEV);return 0;}
#else
int main(void){return YieldProbe();}
#endif
