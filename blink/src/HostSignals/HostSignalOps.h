#ifndef BLINK_HOST_SIGNAL_OPS_H
#define BLINK_HOST_SIGNAL_OPS_H
#include "abi.h"
int blink_host_sigemptyset(blink_host_sigset *);
int blink_host_sigfillset(blink_host_sigset *);
int blink_host_sigaddset(blink_host_sigset *, int);
int blink_host_sigdelset(blink_host_sigset *, int);
int blink_host_sigismember(const blink_host_sigset *, int);
int blink_host_sigprocmask(int, const blink_host_sigset *, blink_host_sigset *);
#endif
