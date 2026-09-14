#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef BLINK_PRIVATE_EXIT_CALLBACKS
#include "host-errors.h"
#include "HostExitCallbacks.h"
#include <setjmp.h>
#endif
#define CHECK(x) do { if(!(x)){printf("exit callback failure %d\n",__LINE__);return __LINE__;} } while(0)
static _Thread_local char trace[1024];
static _Thread_local int position;
static _Thread_local int callback_error;
static void Put(char value){if(position<1023){trace[position++]=value;trace[position]=0;}else callback_error=1;}
static void A(void){Put('A');}
static void C(void){Put('C');}
static void B(void){Put('B');if(atexit(C))callback_error=2;}
static void Finish(void){
  if(strcmp(trace,"ABCA") || callback_error){puts("native LIFO callback result mismatch");
#ifndef BLINK_PRIVATE_EXIT_CALLBACKS
    _Exit(1);
#endif
    callback_error=3;return;
  }
  puts("atexit LIFO, duplicates, reentrant registration and real callbacks: PASS");
}
int SetupCallbacks(void){
  position=0;trace[0]=0;callback_error=0;
  CHECK(!atexit(Finish) && !atexit(A) && !atexit(B) && !atexit(A));return 0;
}
#ifdef BLINK_PRIVATE_EXIT_CALLBACKS
int CallbackResult(void){return callback_error;}
static void Count(void){++callback_error;}
static void Loop(void){++callback_error;if(atexit(Loop))callback_error=-1000;}
static void Reenter(void){
  if(BlinkHostExitCallbacksRun()!=-1 || errno!=EBUSY)callback_error=-1;
  if(BlinkHostExitCallbacksBegin()!=-1 || errno!=EBUSY)callback_error=-2;
}
static void ReplaceContext(void){
  BlinkHostExitCallbacksEnd();
  if(BlinkHostExitCallbacksBegin() || atexit(Count))callback_error=-3;
}
int PrivateCallbackChecks(void){
  CHECK(BlinkHostExitCallbacksRun()==0 && BlinkHostExitCallbacksPending()==0);
  CHECK(atexit(A)==-1 && errno==ECANCELED);
  BlinkHostExitCallbacksEnd();CHECK(atexit(A)==-1 && errno==ENODEV);
  CHECK(!BlinkHostExitCallbacksBegin());CHECK(atexit(0)==-1 && errno==EINVAL);
  callback_error=0;
  for(int i=0;i<BLINK_HOST_EXIT_CALLBACK_LIMIT;++i)CHECK(!atexit(Count));
  CHECK(atexit(A)==-1 && errno==ENOMEM && BlinkHostExitCallbacksPending()==128);
  CHECK(!BlinkHostExitCallbacksRun() && callback_error==128 && BlinkHostExitCallbacksInvoked()==128);
  CHECK(!BlinkHostExitCallbacksRun() && callback_error==128);
  BlinkHostExitCallbacksEnd();CHECK(!BlinkHostExitCallbacksBegin());callback_error=0;
  CHECK(!atexit(Reenter));CHECK(!BlinkHostExitCallbacksRun() && !callback_error);
  BlinkHostExitCallbacksEnd();CHECK(!BlinkHostExitCallbacksBegin());callback_error=0;
  CHECK(!atexit(A) && !atexit(ReplaceContext));
  CHECK(BlinkHostExitCallbacksRun()==-1 && errno==ECANCELED && !callback_error);
  CHECK(BlinkHostExitCallbacksPending()==1 && BlinkHostExitCallbacksInvoked()==0);
  CHECK(!BlinkHostExitCallbacksRun() && callback_error==1);
  BlinkHostExitCallbacksEnd();CHECK(!BlinkHostExitCallbacksBegin());callback_error=0;
  CHECK(!atexit(A) && !atexit(Loop));
  CHECK(BlinkHostExitCallbacksRun()==-1 && errno==ELOOP && callback_error==256);
  CHECK(BlinkHostExitCallbacksPending()==2 && BlinkHostExitCallbacksInvoked()==256);
  CHECK(BlinkHostExitCallbacksRun()==-1 && errno==ECANCELED);
  CHECK(atexit(A)==-1 && errno==ECANCELED);
  CHECK(BlinkHostExitCallbacksBegin()==-1 && errno==EBUSY);
  BlinkHostExitCallbacksEnd();CHECK(BlinkHostExitCallbacksPending()==0 && BlinkHostExitCallbacksInvoked()==0);
  return 0;
}
static _Thread_local jmp_buf jump;
static void Jump(void){longjmp(jump,7);}
int JumpCallbackChecks(void){
  CHECK(!BlinkHostExitCallbacksBegin());callback_error=0;
  CHECK(!atexit(Count) && !atexit(Jump));
  int code=setjmp(jump);
  if(!code){BlinkHostExitCallbacksRun();return 1;}
  CHECK(code==7 && !callback_error && BlinkHostExitCallbacksPending()==1 && BlinkHostExitCallbacksInvoked()==1);
  CHECK(BlinkHostExitCallbacksRun()==-1 && errno==EBUSY);
  BlinkHostExitCallbacksEnd();return 0;
}
int WorkerSetup(int value){
  callback_error=value;CHECK(!BlinkHostExitCallbacksBegin());CHECK(!atexit(Count));return 0;
}
int WorkerFinish(int value){
  CHECK(!BlinkHostExitCallbacksRun() && callback_error==value+1);BlinkHostExitCallbacksEnd();return 0;
}
#ifdef BLINK_MANAGED_EXIT_CALLBACKS
void ThrowingExitCallback(void);
int SetupThrowingCallback(void){
  CHECK(!BlinkHostExitCallbacksBegin());callback_error=0;
  CHECK(!atexit(Count) && !atexit(ThrowingExitCallback));return 0;
}
int CheckThrownCallback(void){
  CHECK(!callback_error && BlinkHostExitCallbacksPending()==1 && BlinkHostExitCallbacksInvoked()==1);
  CHECK(BlinkHostExitCallbacksRun()==-1 && errno==EBUSY);BlinkHostExitCallbacksEnd();return 0;
}
#endif
#endif
#ifndef BLINK_MANAGED_EXIT_CALLBACKS
int main(void){
#ifdef BLINK_PRIVATE_EXIT_CALLBACKS
  if(BlinkHostExitCallbacksBegin())return 1;
#endif
  int result=SetupCallbacks();
#ifdef BLINK_PRIVATE_EXIT_CALLBACKS
  if(!result)result=BlinkHostExitCallbacksRun();
  if(!result)result=CallbackResult();
  if(!result)result=PrivateCallbackChecks();
  if(!result)result=JumpCallbackChecks();
  BlinkHostExitCallbacksEnd();
#endif
  return result;
}
#endif
