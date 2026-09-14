#include <stdio.h>
#include <threads.h>
struct Entry { unsigned long long value; };
static _Thread_local struct Entry records[2];
static _Thread_local unsigned long long scalars[2];
static _Thread_local unsigned long long *pointers[2];
static _Thread_local int (*callbacks[2])(int);
static _Thread_local int marker;
static _Thread_local int echo;
static int increment(int value) { return value + 1; }
unsigned long long *get_tls(void) { return &records[0].value; }
void set_tls(unsigned long long value) {
  marker=(int)value;echo=(int)value+1;
  records[0].value=value;records[1].value=value+1;
  scalars[0]=value;scalars[1]=value+1;
  pointers[0]=scalars;callbacks[0]=increment;
}
int check_tls(unsigned long long value) {
  return marker==(int)value && echo==(int)value+1 && records[0].value==value && records[1].value==value+1 &&
    scalars[0]==value && scalars[1]==value+1 &&
    pointers[0]==scalars && pointers[0][0]==value && callbacks[0](17)==18;
}
unsigned long long sum_tls(void) { return records[0].value + records[1].value; }
static int worker(void *argument) {
  if(sum_tls()!=0 || scalars[0]!=0 || pointers[0]!=0 || callbacks[0]!=0)return -1;
  unsigned long long value=*(unsigned long long *)argument;
  set_tls(value);return check_tls(value) ? (int)sum_tls() : -2;
}
int main(void) {
  thrd_t a,b;unsigned long long first=10,second=20;
  set_tls(3);
  if(thrd_create(&a,worker,&first)||thrd_create(&b,worker,&second))return 1;
  int x,y;thrd_join(a,&x);thrd_join(b,&y);
  printf("tls=%d,%d,%llu size=%zu\n",x,y,sum_tls(),sizeof(records));
  return x==21 && y==41 && check_tls(3) ? 0 : 2;
}
