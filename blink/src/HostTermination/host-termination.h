#ifndef BLINK_CAMPAIGN_HOST_TERMINATION_H
#define BLINK_CAMPAIGN_HOST_TERMINATION_H
#include <stdlib.h>
#include <unistd.h>
/* Normal guest exit uses upstream System.trapexit. Reaching these host paths
 * is an owning-worker failure, never permission to terminate the controller. */
_Noreturn void blink_host_exit(int);
_Noreturn void blink_host_immediate_exit(int);
_Noreturn void blink_host_abort(void);
#define exit blink_host_exit
#define _Exit blink_host_immediate_exit
#define _exit blink_host_immediate_exit
#define abort blink_host_abort
#endif
