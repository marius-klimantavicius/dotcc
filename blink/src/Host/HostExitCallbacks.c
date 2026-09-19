#include "HostExitCallbacks.h"
#include <errno.h>
#ifndef ELOOP
#define ELOOP 40 /* Measured campaign host ABI; host-errors.h defines it in core. */
#endif

typedef void (*ExitCallback)(void);
static _Thread_local ExitCallback exit_callbacks[BLINK_HOST_EXIT_CALLBACK_LIMIT];
static _Thread_local int exit_phase;
static _Thread_local int exit_count;
static _Thread_local int exit_invoked;
static _Thread_local unsigned long exit_generation;
enum ExitPhase { ExitOff, ExitAccepting, ExitRunning, ExitDrained, ExitFailed };
static int ExitError(int error) { errno=error;return -1; }
int BlinkHostExitCallbacksBegin(void) {
  if(exit_phase!=ExitOff)return ExitError(EBUSY);
  ++exit_generation;exit_count=0;exit_invoked=0;exit_phase=ExitAccepting;
  return 0;
}
int atexit(ExitCallback callback) {
  if(exit_phase==ExitOff)return ExitError(ENODEV);
  if(exit_phase!=ExitAccepting && exit_phase!=ExitRunning)return ExitError(ECANCELED);
  if(!callback)return ExitError(EINVAL);
  if(exit_count==BLINK_HOST_EXIT_CALLBACK_LIMIT)return ExitError(ENOMEM);
  exit_callbacks[exit_count++]=callback;
  return 0;
}
int BlinkHostExitCallbacksRun(void) {
  unsigned long generation=exit_generation;
  if(exit_phase==ExitOff)return ExitError(ENODEV);
  if(exit_phase==ExitRunning)return ExitError(EBUSY);
  if(exit_phase==ExitFailed)return ExitError(ECANCELED);
  if(exit_phase==ExitDrained)return 0;
  exit_phase=ExitRunning;
  while(exit_count) {
    if(exit_invoked==BLINK_HOST_EXIT_INVOCATION_LIMIT) {
      exit_phase=ExitFailed;return ExitError(ELOOP);
    }
    ExitCallback callback=exit_callbacks[--exit_count];
    exit_callbacks[exit_count]=0;
    ++exit_invoked;
    callback();
    /* Explicit owner teardown, including one called from a callback, cancels
     * this run. Never overwrite the state of a newly begun context. */
    if(exit_generation!=generation)return ExitError(ECANCELED);
  }
  exit_phase=ExitDrained;
  return 0;
}
void BlinkHostExitCallbacksEnd(void) {
  for(int i=0;i<BLINK_HOST_EXIT_CALLBACK_LIMIT;++i)exit_callbacks[i]=0;
  ++exit_generation;exit_count=0;exit_invoked=0;exit_phase=ExitOff;
}
int BlinkHostExitCallbacksPending(void) { return exit_count; }
int BlinkHostExitCallbacksInvoked(void) { return exit_invoked; }
