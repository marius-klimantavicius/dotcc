#ifndef BLINK_MANAGED_SIGNAL_H
#define BLINK_MANAGED_SIGNAL_H
#include "abi.h"
#include "signal-constants.h"
typedef int32_t sig_atomic_t;
/* glibc signal.h exposes this storage type transitively; Blink uses it even
 * without guest threads. It is a scalar identity, not a pthread implementation. */
typedef blink_host_thread_id pthread_t;
typedef blink_host_sigset sigset_t;
typedef blink_host_siginfo siginfo_t;
typedef blink_host_signal_stack stack_t;
#define sigaction blink_host_sigaction
#define sa_handler handler.simple
#define sa_sigaction handler.info
#define sa_mask mask
#define sa_flags flags
#define sa_restorer restorer
#define si_addr payload.fault.address
#define SIG_DFL ((void (*)(int))0)
#define SIG_IGN ((void (*)(int))1)
#define SIG_ERR ((void (*)(int))-1)

/* Deliberately unresolved campaign names: never bind to generic libc's
 * compilation-only signal support or process-wide signal delivery. */
#define signal blink_host_signal
#define sigprocmask blink_host_sigprocmask
#define sigemptyset blink_host_sigemptyset
#define sigfillset blink_host_sigfillset
#define sigaddset blink_host_sigaddset
#define sigdelset blink_host_sigdelset
#define sigismember blink_host_sigismember
#define sigsuspend blink_host_sigsuspend
#define sigaltstack blink_host_sigaltstack
#define kill blink_host_kill
#define raise blink_host_raise
void (*signal(int, void (*)(int)))(int);
int sigaction(int, const struct sigaction *, struct sigaction *);
int sigprocmask(int, const sigset_t *, sigset_t *);
int sigemptyset(sigset_t *);
int sigfillset(sigset_t *);
int sigaddset(sigset_t *, int);
int sigdelset(sigset_t *, int);
int sigismember(const sigset_t *, int);
int sigsuspend(const sigset_t *);
int sigaltstack(const stack_t *, stack_t *);
int kill(int32_t, int);
int raise(int);
#endif
