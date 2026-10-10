# Thread-local initialization and destruction

The semantic object preserves a local-static guard's storage domain. Process
statics use the observed Itanium acquire/release protocol. Thread-local statics
use the stock first-byte probe and publication after successful initialization;
no cross-thread monitor is acquired. Both original object and guard storage
must be thread-local, and the source owner, type and lifetime remain exact.

`set_local_static_guard_var` supplies the publication observation. Its discarded
byte assignment becomes a typed `thread-publish` action. The object verifier
rejects a process acquire/release operation on a thread-local guard, a changed
storage domain, a missing publication, and a changed operation result type.

EDG also retains a guard cleanup region for thread-local initialization.
`lower_init.c::add_local_static_guard_var_cleanup`, `lib_src/throw.c` and
`lib_src/guard.c` establish that this resets the first byte after failure. The
backend keeps that owned region and emits `AbortThreadInitialization`; it does
not invoke the process guard's monitor-release operation. Initialization may
then be retried on the same thread.

Nontrivial objects retain their original persistent destruction obligation and
its `thread` domain. Registration must match the same original variable,
initializer owner and selected complete destructor. The shared process runtime
adds `ObjectProgram.RegisterThreadDestructor`, forwarding to the same registry
as `__cxa_thread_atexit`. Explicit thread release drains callbacks before
releasing the translated pinned TLS storage. The implementation uses BCL thread
storage and is compatible with NativeAOT. No CLR finalizer is used as a C++
thread-exit substitute; managed hosts explicitly release translated thread state.

The scalar guard fixture verifies failure/retry, once-only publication and
independence across eight BCL worker threads plus the initial thread. The
nontrivial fixture also checks constructor failure, exactly-once destruction,
thread callbacks before process callbacks and idempotent explicit release.
Native C++ and stock C O0/O2 are the semantic oracles. Resealed negative objects
verify source-domain and publication ownership; generated programs run under
JIT and NativeAOT. Acceptance receipts and exact counts are recorded in the
supervisor milestone evidence.
