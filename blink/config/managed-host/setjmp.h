#ifndef BLINK_MANAGED_SETJMP_H
#define BLINK_MANAGED_SETJMP_H
#include "abi.h"
typedef blink_host_jump_storage jmp_buf[1];
typedef blink_host_jump_storage sigjmp_buf[1];
/* Not aliases to dotcc setjmp. Both synchronous unwind and signal-mask state
 * must be implemented and tested before these declarations can be linked. */
#define setjmp blink_host_setjmp
#define _setjmp blink_host_plain_setjmp
#define longjmp blink_host_longjmp
#define sigsetjmp blink_host_sigsetjmp
#define siglongjmp blink_host_siglongjmp
int setjmp(jmp_buf);
int _setjmp(jmp_buf);
void longjmp(jmp_buf, int);
int sigsetjmp(sigjmp_buf, int);
void siglongjmp(sigjmp_buf, int);
#endif
