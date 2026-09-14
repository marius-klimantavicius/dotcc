#include "HostSignalOps.h"
#include "signal-constants.h"
#include <errno.h>
void BlinkHostDeliveryMaskRead(blink_host_sigset *);
void BlinkHostDeliveryMaskWrite(const blink_host_sigset *);

/* Linux/glibc's public set domain excludes its two reserved thread signals. */
static int ValidSignal(int signal) {
  return signal > 0 && signal < NSIG && signal != 32 && signal != 33;
}
static int BadSet(void) { errno = EFAULT; return -1; }
static int BadSignal(void) { errno = EINVAL; return -1; }
int blink_host_sigemptyset(blink_host_sigset *set) {
  if (!set) return BadSet();
  for (int i = 0; i < 16; ++i) set->words[i] = 0;
  return 0;
}
int blink_host_sigfillset(blink_host_sigset *set) {
  if (blink_host_sigemptyset(set)) return -1;
  set->words[0] = ~(UINT64_C(1) << 31 | UINT64_C(1) << 32);
  return 0;
}
int blink_host_sigaddset(blink_host_sigset *set, int signal) {
  if (!set) return BadSet();
  if (!ValidSignal(signal)) return BadSignal();
  set->words[0] |= UINT64_C(1) << (signal - 1);
  return 0;
}
int blink_host_sigdelset(blink_host_sigset *set, int signal) {
  if (!set) return BadSet();
  if (!ValidSignal(signal)) return BadSignal();
  set->words[0] &= ~(UINT64_C(1) << (signal - 1));
  return 0;
}
int blink_host_sigismember(const blink_host_sigset *set, int signal) {
  if (!set) return BadSet();
  /* Native membership permits reserved bit positions, unlike add/delete. */
  if (signal <= 0 || signal >= NSIG) return BadSignal();
  return (set->words[0] >> (signal - 1)) & 1;
}
int blink_host_sigprocmask(int how, const blink_host_sigset *set, blink_host_sigset *old) {
  blink_host_sigset previous, next, requested;
  BlinkHostDeliveryMaskRead(&previous);
  /* Copy before writing caller output. */
  if (set) requested = *set;
  if (set && how != SIG_BLOCK && how != SIG_UNBLOCK && how != SIG_SETMASK)
    return BadSignal();
  if (old) *old = previous;
  if (!set) return 0;
  next = previous;
  if (how == SIG_BLOCK) next.words[0] |= requested.words[0];
  else if (how == SIG_UNBLOCK) next.words[0] &= ~requested.words[0];
  else next.words[0] = requested.words[0];
  next.words[0] &= ~(UINT64_C(1) << (SIGKILL - 1) | UINT64_C(1) << (SIGSTOP - 1) |
                     UINT64_C(1) << 31 | UINT64_C(1) << 32);
  for (int i = 1; i < 16; ++i) next.words[i] = 0;
  BlinkHostDeliveryMaskWrite(&next);
  return 0;
}
