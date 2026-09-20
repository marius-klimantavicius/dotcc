#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#ifdef BLINK_MANAGED_MEMBARRIER
#include "host-membarrier.h"
#define Barrier blink_host_membarrier
#else
#include <sys/syscall.h>
#include <unistd.h>
#include <pthread.h>
static int Barrier(int command, unsigned flags, int cpu) {
  return (int)syscall(SYS_membarrier, command, flags, cpu);
}
#endif
static volatile int shared_value;
int ProcessBarrierInitialize(void) {
  errno=37;
  if ((Barrier(0,0,0)&24)!=24 || errno!=37) return 1;
  if (Barrier(16,0,0)!=0 || errno!=37) return 2;
  shared_value=0;
  return 0;
}
int ProcessBarrierStep(int step) {
  errno=37;
  if (step==1) {
    if (shared_value!=0) return 3;
    shared_value=42;
  } else {
    if (shared_value!=42) return 4;
    shared_value+=10;
  }
  /* Registration happened on a different execution thread. */
  if (Barrier(8,0,0)!=0 || errno!=37) return 5;
  return 0;
}
int ProcessBarrierFinish(void) {
  if (shared_value!=52) return 6;
  puts("process registration=24 workers=2 fences=2 value=52");
  return 0;
}
#ifndef BLINK_MANAGED_MEMBARRIER
static pthread_mutex_t gate=PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t changed=PTHREAD_COND_INITIALIZER;
static int ready, first_done, errors[2];
static void *Worker(void *arg) {
  long index=(long)arg;
  pthread_mutex_lock(&gate);
  ++ready;pthread_cond_broadcast(&changed);
  while (ready!=2 || (index==2 && !first_done)) pthread_cond_wait(&changed,&gate);
  pthread_mutex_unlock(&gate);
  errors[index-1]=ProcessBarrierStep((int)index);
  if (index==1) {
    pthread_mutex_lock(&gate);first_done=1;
    pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);
  }
  return 0;
}
int main(void) {
  pthread_t first,second;
  if (ProcessBarrierInitialize()) return 10;
  if (pthread_create(&first,0,Worker,(void*)1L)) return 11;
  if (pthread_create(&second,0,Worker,(void*)2L)) return 12;
  if (pthread_join(first,0) || pthread_join(second,0)) return 13;
  if (errors[0] || errors[1]) return 14;
  if (pthread_cond_destroy(&changed) || pthread_mutex_destroy(&gate)) return 15;
  return ProcessBarrierFinish();
}
#endif
