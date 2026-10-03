/* Defined C control-flow cases. Modified surviving locals are volatile. */
#include <setjmp.h>
static jmp_buf shared[4];
static int calls;
static jmp_buf* choose(void){++calls;return &shared[0];}
static int after_block(void) {
  volatile int value=0;
  {if(setjmp(shared[0]))return value;}
  value=7;longjmp(shared[0],0);
}
static int many_buffers_one_site(void) {
  volatile int index;
  for(index=0;index<2;++index) {
    switch(setjmp(shared[index])){case 0:break;case 9:return 10+index;default:return -1;}
  }
  longjmp(shared[0],9);
}
static int repeat_and_value(void) {
  volatile int count=0;
  switch(setjmp(shared[0])){case 0:break;case 17:break;default:return -2;}
  if(++count<3)longjmp(shared[0],17);
  return count;
}
static int discarded(void) {
  volatile int value=0;
  setjmp(shared[0]);
  if(++value==1)longjmp(shared[0],0);
  return value;
}
static int side_effect(void) {
  calls=0;
  if(setjmp(*choose())==0)longjmp(shared[0],5);
  return calls;
}
static int recurse(int depth) {
  if(setjmp(shared[depth]))return depth+20;
  if(depth)return recurse(depth-1);
  longjmp(shared[2],5);
}
static int while_guard(void) {
  volatile int value=0;
  while(!setjmp(shared[0])){++value;longjmp(shared[0],1);}
  return value;
}
static int do_guard(void) {
  volatile int value=0;
  do {if(++value==2)longjmp(shared[0],9);}while(setjmp(shared[0])==0);
  return value;
}
static int for_guard(void) {
  volatile int value=0;
  for(;setjmp(shared[0])==0;){if(++value==2)longjmp(shared[0],3);}
  return value;
}
static int reversed_comparison(void) {
  volatile int value=0;
  if(7<setjmp(shared[0]))return value;
  value=5;longjmp(shared[0],8);
}
static int rearm_and_goto(void) {
  if(setjmp(shared[0]))return -3;
  goto next;
next:
  if(setjmp(shared[0]))return 7;
  longjmp(shared[0],0);
}
int main(void) {
  if(after_block()!=7)return 1;
  if(many_buffers_one_site()!=12)return 2;
  if(repeat_and_value()!=3)return 3;
  if(discarded()!=2)return 4;
  if(side_effect()!=1)return 5;
  if(recurse(3)!=22)return 6;
  if(while_guard()!=1)return 7;
  if(do_guard()!=2)return 8;
  if(for_guard()!=2)return 9;
  if(reversed_comparison()!=5)return 10;
  if(rearm_and_goto()!=7)return 11;
  return 0;
}
