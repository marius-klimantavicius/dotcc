/* Native-checked reduction: all three arrays must be local to each thread. */
struct Entry { int value; };
typedef struct Entry Entries[2];
static _Thread_local int scalars[2];
static _Thread_local struct Entry records[2];
static _Thread_local Entries aliases;
void SetEntries(int value) { scalars[0]=value;records[0].value=value;aliases[0].value=value; }
int CheckEntries(int value) { return scalars[0]==value && records[0].value==value && aliases[0].value==value; }
#ifndef BLINK_MANAGED_TLS
#define _GNU_SOURCE 1
#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
static pthread_barrier_t barrier;
static void *Worker(void *argument) {
  int value=(int)(intptr_t)argument;
  SetEntries(value);pthread_barrier_wait(&barrier);
  return (void *)(intptr_t)!CheckEntries(value);
}
int main(void) {
  pthread_t a,b;void *ra,*rb;
  if(pthread_barrier_init(&barrier,0,2) || pthread_create(&a,0,Worker,(void *)71) || pthread_create(&b,0,Worker,(void *)92))return 1;
  if(pthread_join(a,&ra) || pthread_join(b,&rb) || ra || rb)return 2;
  pthread_barrier_destroy(&barrier);puts("native explicit and alias TLS arrays: PASS");return 0;
}
#endif
