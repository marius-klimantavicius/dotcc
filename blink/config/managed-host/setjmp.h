#ifndef BLINK_MANAGED_SETJMP_H
#define BLINK_MANAGED_SETJMP_H
#include "abi.h"
typedef blink_host_jump_storage jmp_buf[1];
typedef blink_host_signal_jump_storage sigjmp_buf[1];
int setjmp(uint64_t *);
void longjmp(uint64_t *, int);
uint64_t *PrepareVirtualSignalJump(sigjmp_buf, int);
void blink_host_siglongjmp(sigjmp_buf, int);
/* The ordinary opaque prefix is larger than generic dotcc's numeric slot. */
#define setjmp(env) setjmp((uint64_t *)(env))
#define _setjmp(env) setjmp((uint64_t *)(env))
#define longjmp(env, value) longjmp((uint64_t *)(env), (value))
/* Explicit virtual-mask capture occurs once before the generic intrinsic arms
 * its numeric slot. Signal-aware longjmp restores that mask before unwinding. */
#define sigsetjmp(env, save) setjmp(PrepareVirtualSignalJump((env), (save)))
#define siglongjmp blink_host_siglongjmp
#endif
