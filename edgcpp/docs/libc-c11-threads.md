# C11 threads implementation checkpoint

The worker-thread API passes generated C/C++ static/instance JIT/NativeAOT
acceptance. Main/host-thread exit and complete process termination remain open;
full C11 thread support is not yet claimed.

The public ABI uses 64-bit opaque owner tokens: `thrd_t`, `tss_t` and `once_flag`
are unsigned long; `mtx_t` and `cnd_t` each contain one unsigned-long token.
It never exposes a platform pthread or Windows structure. The authored
`libc/threads/threads.c` facade delegates to 22 exact-signature private imports.
It is supplied explicitly to tests and is not automatically packaged yet.
Source signatures and result conventions were audited against C11 draft N1570
7.26 and pinned musl 1.2.6 `include/threads.h` and thread wrappers. The BCL
implementation reuses the existing owner-specific libc++ thread service.

`LibcC11Threads.cs` adds C integer-return callbacks, join results, token identity,
C return-status mapping, timed and recursive mutex operations, conditions,
once, sleep/yield and TSS. Shared resources, retained owner lifetime and thread
cleanup remain in `LibcxxThreads.cs`. Its accepted C++ callbacks keep their
existing pointer-return ABI and first-pass termination behavior. Ordinary lock
operations allocate no temporary managed objects after initialization.

The creator publishes its output token before starting the BCL thread, so
callbacks observe initialized caller storage.

A created C thread's `thrd_exit` throws the private-runtime
`CThreadExitTransfer`. Every generated language catch/noexcept/dynamic exception
specification must exclude that control transfer. Only the matching runtime
service/token catches it and stores its integer result. Managed finally blocks
release automatic storage; then generated C++ thread-local destructors run,
followed by up to four TSS destructor passes, then libc TLS reset and retained
owner release. Each TSS value is cleared before its callback, which may install
another value for a later pass. Joining waits through this complete cleanup.
A repeated `thrd_exit` from a C++ TLS or TSS destructor updates the result and
continues remaining cleanup. C++ throw-payload construction retires its pending
uncaught marker before the control transfer unwinds partially constructed
objects, so destructors still observe `uncaught_exceptions()==0`. Mutex
ownership is not automatically relinquished by thread exit.

The following are still explicit integration/completion gates:

- `thrd_exit` from translated main or an externally supplied host thread, and
  normal process termination after the last translated thread. Current runtime
  rejects a call without a translated worker boundary; it does not pretend an
  ordinary managed exception implements that process contract.
- C signal interruption of sleep. CLR Thread.Interrupt is reported as an error,
  never as a fabricated C signal; the separate signal provider must supply it.
- Arbitrary cross-provider/native pthread handles and cancellation are outside
  this owner-token ABI; no host OS identity is guessed from an integer token.

Evidence under `artifacts/campaign/libc-c11-threads-candidate-20261003-011000`:
`runtime-proof-003/results.json` passes standalone JIT/NativeAOT worker tests;
`cpp-regression/results.json` and `fatal-results.json` retain accepted C++
normal and first-pass fatal behavior. `libc-c11-threads-native-20261003-013000`
contains native C/C++ public API oracles. Tests cover four-thread contention,
integer return/exit, four destructor passes, owner rejection, current identity,
once, timed/recursive locks, condition release/reacquisition, sleep and detach.
These are synthetic fixtures. NativeAOT execution evidence is Linux x64.

Reference: [C11 draft N1570 thread library](https://www.sigbus.info/n1570#7.26).

Accepted focused evidence added after integration:

- `libc-c11-threads-runtime-20261003-014500/results.json`: six standalone
  C11/shared-C++ JIT/NativeAOT normal and fatal-boundary checks.
- `libc-c11-threads-abi-20261003-015000/results.json`: thirteen malformed
  private ABI declarations rejected before emission.
- `libc-c11-threads-translated-20261003-020300/results.json`: native C/C++ plus
  all eight generated modes, including nested/array destructor trace5432167,
  throw-copy partial cleanup trace98671, and reentrant TLS exit trace1234.
- `libc-c11-copy-exit-native-20261003-015500/results.json` and
  `libc-c11-tls-exit-native-20261003-020000/results.json`: independent GCC
  references for copy-construction and TLS-destructor exit/uncaught counts.

Failed initial generated probes are retained. They exposed the pending throw
carrier marker, fixed in backend control-transfer handling without changing EDG.

## Initial process thread

The accepted runtime establishes an explicit logical source-thread boundary only
for executable entry. It retains the owner while main is active, supplies a
joinable thrd_t identity, and publishes the main thread's result only after its
thread cleanup. A joining worker waits for logical completion rather than the
physical CLR thread; the latter remains a process waiter after thrd_exit. The
last source thread performs normal process termination with status zero and
retains its own thrd_current identity during atexit callbacks. Descendants created
by surviving source workers remain counted. Normal main return still terminates
the process immediately; managed Run and foreign host calls establish no such
boundary. Owner-mode termination remains unavailable.

Pinned musl 1.2.6 `src/thread/thrd_exit.c` delegates to `__pthread_exit`; the latter
runs TSS destructors and calls exit(0) when the last thread exits. Native C probes
cover no other worker, joining the initial thread, a later descendant, reentrant
exit from an initial TSS destructor, a detached worker and a detached initial
thread. Candidate runtime proofs retain four analogous JIT/AOT modes, including
last-thread callback identity and zero remaining lifetime leases.

C++ interop has an explicit policy: preserve the already accepted automatic →
C++ TLS → C TSS cleanup order for CThreadExitTransfer. Native glibc/GCC probes on
this host instead omit initial-thread C++ TLS destruction when a worker survives;
with no worker, they postpone it until process exit (automatic → C TSS → C++ TLS).
Those observations are retained at
`libc-initial-thread-native-20261003-031500/observations.json`; they are not used
as a false oracle for translated C++ TLS policy. Exact C traces and independently
specified translated C++ traces remain separate checks. Generated-code acceptance passed ten checks at
`libc-initial-thread-translated-20261003-033000/results.json`: native C exact
six-mode oracle, native C++ host observations, and all eight translated
configurations. Every translated mode asserts process-wide/normal-exit and
logical-completion-v1 metadata. Standalone runtime JIT/AOT eight checks and
shared C11/C++ six-check regressions are retained under
`libc-initial-thread-candidate-20261003-031000/proof-002/results.json` and
`regressions-001/results.json`. The reusable standalone and source hooks are
`verify_libc_initial_thread_runtime` and `verify_libc_initial_thread`.
Foreign-host/managed Run thrd_exit remains explicitly unsupported.
