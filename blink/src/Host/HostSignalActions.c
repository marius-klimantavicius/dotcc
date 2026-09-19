#include "HostSignalActions.h"
#include <errno.h>
#include <stdint.h>

static _Thread_local int actions_active;
static _Thread_local struct sigaction actions[65];
static int ActionError(int error) { errno=error;return -1; }
static int PublicSignal(int signal) {
  return signal>0 && signal<65 && signal!=32 && signal!=33;
}
static void ClearActions(void) {
  struct sigaction empty={0};
  for(int i=0;i<65;++i) actions[i]=empty;
}
int BlinkHostSignalActionsBegin(void) {
  if(actions_active)return ActionError(EBUSY);
  ClearActions();actions_active=1;return 0;
}
void BlinkHostSignalActionsEnd(void) {
  ClearActions();actions_active=0;
}
int sigaction(int signal, const struct sigaction *action, struct sigaction *old) {
  struct sigaction requested={0};
  if(!actions_active)return ActionError(ENODEV);
  if(!PublicSignal(signal))return ActionError(EINVAL);
  if(action) {
    if(signal==SIGKILL || signal==SIGSTOP)return ActionError(EINVAL);
    if(action->sa_handler==SIG_ERR)return ActionError(EINVAL);
    if((unsigned)action->sa_flags & ~(unsigned)(SA_SIGINFO|SA_RESTART))return ActionError(ENOTSUP);
    requested.sa_flags=action->sa_flags;
    if(action->sa_flags & SA_SIGINFO) requested.sa_sigaction=action->sa_sigaction;
    else requested.sa_handler=action->sa_handler;
    /* Measured LP64 sigset storage; kernel action masks clear only KILL/STOP.
     * Unlike sigprocmask, reserved thread-signal mask bits are retained. */
    ((uint64_t *)&requested.sa_mask)[0]=((const uint64_t *)&action->sa_mask)[0] &
        ~(UINT64_C(1)<<(SIGKILL-1) | UINT64_C(1)<<(SIGSTOP-1));
  }
  if(old)*old=actions[signal];
  if(action)actions[signal]=requested;
  return 0;
}
void (*signal(int signum, void (*handler)(int)))(int) {
  struct sigaction action={0},old;
  action.sa_handler=handler;
  action.sa_flags=SA_RESTART;
  if(PublicSignal(signum)) ((uint64_t *)&action.sa_mask)[0]=UINT64_C(1)<<(signum-1);
  if(sigaction(signum,&action,&old))return SIG_ERR;
  return old.sa_handler;
}
