#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
int ProcessStateBegin(void);
int ProcessStateWorker(int);
int ProcessStateFinish(void);
static void *Worker(void *argument) {
  return (void *)(intptr_t)ProcessStateWorker((int)(intptr_t)argument);
}
int main(void) {
  pthread_t first,second;
  void *first_result,*second_result;
  int result=ProcessStateBegin();
  if(result)return result;
  if(pthread_create(&first,0,Worker,0) || pthread_create(&second,0,Worker,(void *)1))return 20;
  if(pthread_join(first,&first_result) || pthread_join(second,&second_result))return 21;
  if(first_result || second_result)return 22;
  if((result=ProcessStateFinish()))return result;
  puts("shared disposition; callbacks BCA once; clean end");
  return 0;
}
