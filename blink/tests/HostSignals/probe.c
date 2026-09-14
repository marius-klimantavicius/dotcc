#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <stdlib.h>

#ifdef BLINK_REAL_POSIX_ORACLE
#include <setjmp.h>
#include <signal.h>
typedef sigjmp_buf Jump;
#define Capture(env, save) sigsetjmp((env), (save))
#define Restore(env, value) siglongjmp((env), (value))
static void SetMask(int bits) {
  sigset_t mask;
  sigemptyset(&mask);
  if (bits & 1) sigaddset(&mask, SIGUSR1);
  if (bits & 2) sigaddset(&mask, SIGUSR2);
  if (sigprocmask(SIG_SETMASK, &mask, NULL)) abort();
}
static int ReadMask(void) {
  sigset_t mask;
  if (sigprocmask(SIG_SETMASK, NULL, &mask)) abort();
  return sigismember(&mask, SIGUSR1) | (sigismember(&mask, SIGUSR2) << 1);
}
#else
#include "HostSignals.h"
typedef BlinkHostSignalJump Jump[1];
#define Capture(env, save) BlinkHostSignalSetjmp((env), (save))
#define Restore(env, value) blink_host_siglongjmp((env), (value))
static void SetMask(int bits) {
  blink_host_sigset mask = {0};
  mask.words[0] = (uint64_t)bits;
  BlinkHostDeliveryMaskWrite(&mask);
}
static int ReadMask(void) {
  blink_host_sigset mask;
  BlinkHostDeliveryMaskRead(&mask);
  return (int)mask.words[0];
}
#endif

#ifdef BLINK_TEST_MANAGED
void DotccSignalCheckHostMask(void);
#define CheckHostMask() DotccSignalCheckHostMask()
#else
#include <signal.h>
static sigset_t original_host_mask;
static void CheckHostMask(void) {
#ifndef BLINK_REAL_POSIX_ORACLE
  sigset_t current;
  if (sigprocmask(SIG_SETMASK, NULL, &current)) abort();
  for (int i = 1; i < 65; ++i)
    if (sigismember(&current, i) != sigismember(&original_host_mask, i)) abort();
#endif
}
#endif

static Jump first, second;
static int selector_calls, repeated_hits;
static void Require(int condition) { if (!condition) abort(); }
static void Deep(Jump env, int value) {
  CheckHostMask();
  Restore(env, value);
  abort();
}
static void Deeper(Jump env, int value) { Deep(env, value); }
static void *Select(void) { ++selector_calls; return first; }

int main(void) {
  int rc;
#ifndef BLINK_TEST_MANAGED
  if (sigprocmask(SIG_SETMASK, NULL, &original_host_mask)) abort();
#endif
  CheckHostMask();
  SetMask(1);
  switch (Capture(first, 1)) {
    case 0: SetMask(2); Deeper(first, 0); break;
    case 1: Require(ReadMask() == 1); puts("save=1 zero=1 mask=1"); break;
    default: abort();
  }
  SetMask(1);
  switch (Capture(first, 0)) {
    case 0: SetMask(2); Deeper(first, 5); break;
    case 5: Require(ReadMask() == 2); puts("save=0 value=5 mask=2"); break;
    default: abort();
  }
  SetMask(1);
  switch (Capture(first, 1)) {
    case 0:
      SetMask(2);
      switch (Capture(second, 1)) {
        case 0: SetMask(3); Deeper(second, 2); break;
        case 2: Require(ReadMask() == 2); Deeper(first, 3); break;
        default: abort();
      }
      abort();
    case 3: Require(ReadMask() == 1); puts("nested inner=2 outer=1"); break;
    default: abort();
  }
  /* Arm the same record again without saving: the earlier saved flag must clear. */
  SetMask(3);
  switch (Capture(first, 0)) {
    case 0: SetMask(2); Deeper(first, 4); break;
    case 4: Require(ReadMask() == 2); puts("rearm save=0 mask=2"); break;
    default: abort();
  }
  SetMask(1);
  switch (Capture(Select(), 1)) {
    case 0: SetMask(2); Deeper(first, 7); break;
    case 7:
      Require(ReadMask() == 1);
      ++repeated_hits;
      if (repeated_hits != 3) { SetMask(3); Deeper(first, 7); }
      Require(selector_calls == 1);
      puts("repeat hits=3 evaluated=1 mask=1");
      break;
    default: abort();
  }
  SetMask(1);
  if (!(rc = Capture(first, 1))) {
    SetMask(2);
    Deeper(first, 6);
  } else if (rc == 6) {
    Require(ReadMask() == 1);
    puts("assignment value=6 mask=1");
  } else abort();
#ifdef BLINK_REAL_POSIX_ORACLE
  if (sigprocmask(SIG_SETMASK, &original_host_mask, NULL)) abort();
#else
  BlinkHostDeliveryMaskReset();
  Require(ReadMask() == 0);
#endif
  CheckHostMask();
  puts("complete");
  return 0;
}
