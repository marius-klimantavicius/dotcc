#ifndef BLINK_HOST_GUEST_THREADS_H
#define BLINK_HOST_GUEST_THREADS_H
#include "abi.h"

struct Machine;
struct System;

/* Zero transfers this already-created Machine to a managed owning worker.
 * Nonzero promises no worker started: SysSpawn retains rollback ownership.
 * The callback must not throw after starting a worker. */
int blink_host_guest_thread_start(struct Machine *child);

/* Record the exit and unwind on the calling worker. These never free Machine
 * or join while the caller still holds translated syscall/page locks. */
_Noreturn void blink_host_guest_thread_exit(struct Machine *, int status);
_Noreturn void blink_host_guest_group_exit(struct Machine *, int status);

/* Only an owner-coordinated, lock-free stop/join point may call this export.
 * Unsupported competing callers fail explicitly; never native-kill or free
 * the calling Machine behind its C# owner. */
void blink_host_guest_stop_other_threads(struct System *);

/* Existing upstream implementation, newly exported for worker cleanup. */
void ClearChildTid(struct Machine *);

/* Internal managed pthread ABI: signed 64-bit identity, campaign signal set.
 * These return POSIX error numbers directly, preserving errno. */
int blink_host_guest_pthread_sigmask(int, const blink_host_sigset *, blink_host_sigset *);
int blink_host_guest_pthread_kill(long thread, int signal);
/* No-fork profile: always ENOTSUP, errno unchanged; no callback registration. */
int blink_host_guest_pthread_atfork(void (*prepare)(void), void (*parent)(void),
                                  void (*child)(void));
#endif
