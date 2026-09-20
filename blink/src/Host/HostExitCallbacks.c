#include "HostExitCallbacks.h"
#include <errno.h>
#ifndef ELOOP
#define ELOOP 40 /* Measured campaign host ABI; host-errors.h defines it in core. */
#endif

typedef void (*ExitCallback)(void);
#ifdef BLINK_MANAGED_GUEST_THREADS
#include <pthread.h>
static pthread_mutex_t exit_lock = PTHREAD_MUTEX_INITIALIZER;
static ExitCallback exit_callbacks[BLINK_HOST_EXIT_CALLBACK_LIMIT];
static int exit_phase;
static int exit_count;
static int exit_invoked;
static unsigned long exit_generation;
static void ExitLock(void) { if(pthread_mutex_lock(&exit_lock))abort(); }
static void ExitUnlock(void) { if(pthread_mutex_unlock(&exit_lock))abort(); }
#else
static _Thread_local ExitCallback exit_callbacks[BLINK_HOST_EXIT_CALLBACK_LIMIT];
static _Thread_local int exit_phase;
static _Thread_local int exit_count;
static _Thread_local int exit_invoked;
static _Thread_local unsigned long exit_generation;
static void ExitLock(void) { }
static void ExitUnlock(void) { }
#endif
enum ExitPhase { ExitOff, ExitAccepting, ExitRunning, ExitDrained, ExitFailed };
static int ExitFinish(int result) { ExitUnlock();return result; }
static int ExitError(int error) { errno=error;return ExitFinish(-1); }
int BlinkHostExitCallbacksBegin(void) {
  ExitLock();
  if(exit_phase!=ExitOff)return ExitError(EBUSY);
  ++exit_generation;exit_count=0;exit_invoked=0;exit_phase=ExitAccepting;
  return ExitFinish(0);
}
int atexit(ExitCallback callback) {
  ExitLock();
  if(exit_phase==ExitOff)return ExitError(ENODEV);
  if(exit_phase!=ExitAccepting && exit_phase!=ExitRunning)return ExitError(ECANCELED);
  if(!callback)return ExitError(EINVAL);
  if(exit_count==BLINK_HOST_EXIT_CALLBACK_LIMIT)return ExitError(ENOMEM);
  exit_callbacks[exit_count++]=callback;
  return ExitFinish(0);
}
int BlinkHostExitCallbacksRun(void) {
  ExitLock();
  unsigned long generation=exit_generation;
  if(exit_phase==ExitOff)return ExitError(ENODEV);
  if(exit_phase==ExitRunning)return ExitError(EBUSY);
  if(exit_phase==ExitFailed)return ExitError(ECANCELED);
  if(exit_phase==ExitDrained)return ExitFinish(0);
  exit_phase=ExitRunning;
  while(exit_count) {
    if(exit_invoked==BLINK_HOST_EXIT_INVOCATION_LIMIT) {
      exit_phase=ExitFailed;return ExitError(ELOOP);
    }
    ExitCallback callback=exit_callbacks[--exit_count];
    exit_callbacks[exit_count]=0;
    ++exit_invoked;
    /* No lock may cross an arbitrary callback: it may register another
     * callback, end this context, or unwind through a managed exception. */
    ExitUnlock();
    callback();
    ExitLock();
    /* Explicit owner teardown, including one called from a callback, cancels
     * this run. Never overwrite the state of a newly begun context. */
    if(exit_generation!=generation)return ExitError(ECANCELED);
  }
  exit_phase=ExitDrained;
  return ExitFinish(0);
}
void BlinkHostExitCallbacksEnd(void) {
  ExitLock();
  for(int i=0;i<BLINK_HOST_EXIT_CALLBACK_LIMIT;++i)exit_callbacks[i]=0;
  ++exit_generation;exit_count=0;exit_invoked=0;exit_phase=ExitOff;
  ExitUnlock();
}
int BlinkHostExitCallbacksPending(void) { ExitLock();return ExitFinish(exit_count); }
int BlinkHostExitCallbacksInvoked(void) { ExitLock();return ExitFinish(exit_invoked); }
