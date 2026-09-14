#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#ifdef BLINK_REAL_POSIX_ORACLE
#include <signal.h>
#else
#include "HostSignalOps.h"
#include "signal-constants.h"
typedef blink_host_sigset sigset_t;
#define sigemptyset blink_host_sigemptyset
#define sigfillset blink_host_sigfillset
#define sigaddset blink_host_sigaddset
#define sigdelset blink_host_sigdelset
#define sigismember blink_host_sigismember
#define sigprocmask blink_host_sigprocmask
#endif
#ifdef BLINK_TEST_MANAGED
void DotccSignalCheckHostMask(void);
#define CheckHostMask() DotccSignalCheckHostMask()
#else
#define CheckHostMask() ((void)0)
#endif
static void Require(int yes) { if (!yes) abort(); }
int main(void) {
  sigset_t initial, set, old, got;
  Require(!sigprocmask(SIG_SETMASK, NULL, &initial));
  CheckHostMask();
  Require(!sigemptyset(&set));
  for (int i = 1; i < 65; ++i) Require(sigismember(&set, i) == 0);
  Require(!sigfillset(&set));
  for (int i = 1; i < 65; ++i) Require(sigismember(&set, i) == (i != 32 && i != 33));
  puts("sets empty=0 full=62 reserved=0");
  int invalid[4] = {0,32,33,65};
  for (int i = 0; i < 4; ++i) {
    errno = 0; Require(sigaddset(&set, invalid[i]) == -1 && errno == EINVAL);
    errno = 0; Require(sigdelset(&set, invalid[i]) == -1 && errno == EINVAL);
  }
  errno = 0; Require(sigismember(&set, 65) == -1 && errno == EINVAL);
  Require(!sigemptyset(&set));
  Require(!sigaddset(&set, SIGUSR1)); Require(!sigaddset(&set, SIGKILL));
  Require(!sigaddset(&set, SIGSTOP)); Require(!sigprocmask(SIG_SETMASK, &set, &old));
  Require(!sigprocmask(99, NULL, &got));
  Require(sigismember(&got, SIGUSR1) == 1 && !sigismember(&got, SIGKILL) && !sigismember(&got, SIGSTOP));
  CheckHostMask();
  Require(!sigemptyset(&set)); Require(!sigaddset(&set, SIGUSR2));
  Require(!sigprocmask(SIG_BLOCK, &set, &old)); Require(sigismember(&old, SIGUSR1));
  Require(!sigprocmask(SIG_UNBLOCK, &set, &old));
  Require(sigismember(&old, SIGUSR1) && sigismember(&old, SIGUSR2));
  Require(!sigprocmask(SIG_SETMASK, NULL, &got));
  Require(sigismember(&got, SIGUSR1) && !sigismember(&got, SIGUSR2));
  errno = 0; Require(sigprocmask(99, &set, NULL) == -1 && errno == EINVAL);
  Require(!sigprocmask(SIG_SETMASK, NULL, &got)); Require(sigismember(&got, SIGUSR1));
  Require(!sigprocmask(SIG_SETMASK, &set, &old));
  Require(sigismember(&old, SIGUSR1) && !sigismember(&old, SIGUSR2));
  Require(!sigprocmask(SIG_SETMASK, NULL, &got));
  Require(!sigismember(&got, SIGUSR1) && sigismember(&got, SIGUSR2));
  CheckHostMask();
  Require(!sigprocmask(SIG_SETMASK, &initial, NULL));
  puts("mask set/block/unblock/query errors=atomic unmaskable=clear");
  return 0;
}
