#ifndef BLINK_HOST_GUEST_THREADS_H
#define BLINK_HOST_GUEST_THREADS_H
#include "abi.h"

struct Machine;
struct System;
struct siginfo_linux;

/* Before any non-fetch guest pointer escapes, or a mapping identity changes,
 * synchronously drain native execution and permanently revoke this range.
 * No lock is retained across the subsequent operation. The sole exemption is
 * LookupAddress2(PAGE_XD, 0), used only by read-only instruction fetch/copy. */
void blink_host_guest_code_revoke(long address, long size);
/* Mapping revocation also retires initial data descriptors, before any
 * permission change, publication, replacement, free or allocator reuse. */
void blink_host_guest_mapping_revoke(long address, long size);

/* Zero transfers this already-created Machine to a managed owning worker.
 * Nonzero promises no worker started: SysSpawn retains rollback ownership.
 * The callback must not throw after starting a worker. */
int blink_host_guest_thread_start(struct Machine *child);

/* Existing upstream implementation, newly exported for worker cleanup. */
void ClearChildTid(struct Machine *);

/* SysExit, SysExitGroup, KillOtherThreads and SignalActor bind directly to
 * authored typed managed methods through the threaded semantic profile.
 * Their pinned upstream declarations provide the C ABI; no C shim is needed. */

/* At ConsumeSignal entry on the owning worker, acknowledge a transient private
 * wake generation before inspecting pending/masked/ignored signals. This must
 * not clear guest signal bits or latch a permanent execution-stop reason. */
void blink_host_guest_signal_checkpoint(struct Machine *);

/* Wake the exact live target after enqueue, including before its worker binds.
 * The owner latches a transient private wake by Machine identity; never resolve
 * an uninitialized pthread identity or emit a native host signal. Returns a
 * POSIX error number directly, preserving errno, like the replaced notification.
 * Signal-zero pthread existence probes remain a separate operation. */
int blink_host_guest_signal_wake(struct Machine *);

/* Called under target System.sig_lock before EnqueueSignal. Record the actual
 * sender only if this signal's pending bit is clear; a coalesced pending signal
 * retains its first sender. Storage is bounded to 64 entries per live Machine,
 * owned and cleared by the managed execution owner. */
void blink_host_guest_signal_enqueue_info(struct Machine *, int signal,
                                          int pid, unsigned uid);

/* Immediate self delivery, under System.sig_lock: scope this sender separately
 * from queued metadata, call unchanged DeliverSignal with SI_TKILL, and restore
 * the scope even on unwind. Never consume pending metadata of the same signal. */
void blink_host_guest_signal_deliver_tkill(struct Machine *, int signal,
                                           int pid, unsigned uid);

/* At DeliverSignal, apply scoped immediate metadata or consume queued metadata
 * exactly once after ConsumeSignal clears the pending bit, setting only
 * SI_TKILL code/pid/uid. An unrelated synchronous delivery must not consume
 * metadata while its pending bit is still set. Absent metadata leaves the
 * upstream siginfo untouched. NULL discards queued metadata for default/ignored
 * pending signals. The callback borrows the frame pointer only for this call. */
void blink_host_guest_signal_apply_info(struct Machine *, int signal,
                                        struct siginfo_linux *);

/* Internal managed pthread ABI: signed 64-bit identity, campaign signal set.
 * These return POSIX error numbers directly, preserving errno. */
int blink_host_guest_pthread_sigmask(int, const blink_host_sigset *, blink_host_sigset *);
int blink_host_guest_pthread_kill(long thread, int signal);
/* No-fork profile: always ENOTSUP, errno unchanged; no callback registration. */
int blink_host_guest_pthread_atfork(void (*prepare)(void), void (*parent)(void),
                                  void (*child)(void));
#endif
