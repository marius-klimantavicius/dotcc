#ifndef BLINK_CAMPAIGN_HOST_EXIT_CALLBACKS_H
#define BLINK_CAMPAIGN_HOST_EXIT_CALLBACKS_H
#include <stdlib.h>
#define BLINK_HOST_EXIT_CALLBACK_LIMIT 128
#define BLINK_HOST_EXIT_INVOCATION_LIMIT 256
#define atexit blink_host_atexit
int atexit(void (*)(void));
int BlinkHostExitCallbacksBegin(void);
int BlinkHostExitCallbacksRun(void);
void BlinkHostExitCallbacksEnd(void);
int BlinkHostExitCallbacksPending(void);
int BlinkHostExitCallbacksInvoked(void);
#endif
