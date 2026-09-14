#ifndef BLINK_HOST_SIGNALS_H
#define BLINK_HOST_SIGNALS_H
#include "abi.h"
#include <setjmp.h>

#ifdef BLINK_HOST_NATIVE_ORACLE
/* Actual native jump storage, followed by separately owned virtual state. */
typedef struct {
  jmp_buf ordinary;
  int32_t mask_saved;
  blink_host_sigset mask;
} BlinkHostSignalJump;
typedef jmp_buf *BlinkHostJumpSlot;
#define BlinkHostSignalSetjmp(env, save) setjmp(*PrepareVirtualSignalJump((env), (save)))
#define BlinkHostOrdinaryJump(env, value) longjmp((env)->ordinary, (value))
#define BlinkHostSlotAddress(env) (&(env)->ordinary)
#else
typedef blink_host_signal_jump_storage BlinkHostSignalJump;
typedef uint64_t *BlinkHostJumpSlot;
#define BlinkHostSignalSetjmp(env, save) sigsetjmp((env), (save))
#define BlinkHostOrdinaryJump(env, value) longjmp((env)->ordinary.words, (value))
#define BlinkHostSlotAddress(env) ((env)->ordinary.words)
#endif

/* One active emulation context per worker. This mask models host-side virtual
 * delivery, not Blink's distinct guest Linux blocked-signal state. */
void BlinkHostDeliveryMaskReset(void);
void BlinkHostDeliveryMaskRead(blink_host_sigset *);
void BlinkHostDeliveryMaskWrite(const blink_host_sigset *);
BlinkHostJumpSlot PrepareVirtualSignalJump(BlinkHostSignalJump *, int);
void blink_host_siglongjmp(BlinkHostSignalJump *, int);
#endif
