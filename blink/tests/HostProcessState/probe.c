#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include <errno.h>
#include <pthread.h>
#include <string.h>

#ifdef BLINK_MANAGED_PROCESS_STATE
#if !defined(BLINK_HOST_GUEST_THREADS_H) || !defined(BLINK_MANAGED_SIGNAL_H)
#error The reviewed threaded pthread and campaign signal overlays must be selected
#endif
_Static_assert(sizeof(pthread_t)==8, "managed thread identity");
_Static_assert(sizeof(pthread_mutex_t)==4, "managed mutex handle");
_Static_assert(sizeof(pthread_cond_t)==4, "managed condition handle");
_Static_assert(sizeof(pthread_mutexattr_t)==4, "managed mutex attribute");
#endif

static pthread_mutex_t gate = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t changed = PTHREAD_COND_INITIALIZER;
static int phase;
static char order[4];
static int callback_count;
static int callback_error;
static void HandlerA(int signal) { (void)signal; }
static void HandlerB(int signal) { (void)signal; }
static void CallbackA(void) { order[callback_count++]='A'; }
static void CallbackC(void) { order[callback_count++]='C'; }
static void CallbackB(void) {
  order[callback_count++]='B';
  if(atexit(CallbackC))callback_error=1;
}
int ProcessStateBegin(void) {
  errno=71;
  if(BlinkHostSignalActionsBegin() || BlinkHostExitCallbacksBegin())return 1;
  return errno==71 ? 0 : 2;
}
static int PublishResult(int next, int result) {
  phase=next;
  if(pthread_cond_broadcast(&changed))result=90;
  if(pthread_mutex_unlock(&gate))result=91;
  return result;
}
int ProcessStateWorker(int second) {
  struct sigaction action={0},old={0};
  errno=72+second;
  if(pthread_mutex_lock(&gate))return 3;
  if(second) {
    while(!phase)if(pthread_cond_wait(&changed,&gate))return PublishResult(-1,4);
    if(phase!=1)return PublishResult(-1,5);
    if(sigaction(SIGUSR1,0,&old) || old.sa_handler!=HandlerA || old.sa_flags!=SA_RESTART)
      return PublishResult(-1,6);
    action.sa_handler=HandlerB;action.sa_flags=SA_RESTART;
    if(sigaction(SIGUSR1,&action,&old) || old.sa_handler!=HandlerA || atexit(CallbackB))
      return PublishResult(-1,7);
    if(errno!=73)return PublishResult(-1,8);
    return PublishResult(2,0);
  }
  action.sa_handler=HandlerA;action.sa_flags=SA_RESTART;
  if(sigaction(SIGUSR1,&action,&old) || old.sa_handler!=SIG_DFL || atexit(CallbackA))
    return PublishResult(-1,9);
  phase=1;
  if(pthread_cond_broadcast(&changed))return PublishResult(-1,10);
  while(phase==1)if(pthread_cond_wait(&changed,&gate))return PublishResult(-1,11);
  if(phase!=2)return PublishResult(-1,12);
  if(sigaction(SIGUSR1,0,&old) || old.sa_handler!=HandlerB || errno!=72)
    return PublishResult(-1,13);
  return PublishResult(2,0);
}
int ProcessStateFinish(void) {
  struct sigaction old={0};
  errno=74;
  if(sigaction(SIGUSR1,0,&old) || old.sa_handler!=HandlerB)return 14;
  if(BlinkHostExitCallbacksPending()!=2 || BlinkHostExitCallbacksInvoked()!=0)return 15;
  if(BlinkHostExitCallbacksRun() || callback_error || callback_count!=3 || strcmp(order,"BCA"))return 16;
  if(BlinkHostExitCallbacksRun() || callback_count!=3 || BlinkHostExitCallbacksPending()!=0 ||
     BlinkHostExitCallbacksInvoked()!=3 || errno!=74)return 17;
  BlinkHostExitCallbacksEnd();BlinkHostSignalActionsEnd();
  if(BlinkHostExitCallbacksPending()!=0 || BlinkHostExitCallbacksInvoked()!=0)return 18;
  if(pthread_cond_destroy(&changed) || pthread_mutex_destroy(&gate))return 19;
  return 0;
}
