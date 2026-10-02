# libc++ external thread provider

The initial provider uses the pinned libc++18 external-threading interface.
`backend/runtime/libcxx/config/__external_threading` supplies the `std` helper
types and inline wrappers; `__edg_thread_abi.h` declares 19 fixed C imports.
`dot_gen_threads.h` validates lowered signatures and emits ordinary imported
routine wrappers, preserving the backend's canonical function addresses.
`LibcxxThreads<TOwner>` implements the services with BCL objects.

This is a portable managed ABI, not pthread or a native Windows thread ABI.
Mutexes, recursive mutexes, condition variables, once flags, thread handles and
TLS keys occupy one naturally aligned unsigned 64-bit token. Mutex/condition/
once storage starts at zero. IDs are managed thread IDs; handles are separate
opaque tokens. Timespec is two signed 64-bit fields at offsets 0 and 8, with a
Unix-epoch absolute realtime deadline. Import validation checks its field types,
names, offsets, size and alignment before the pointer is cast to the runtime
layout. Pointer callbacks are managed function pointers with explicit owner
parameters in instance mode. Static mode uses a private reference-type owner
marker for the generic runtime; a generated static class cannot be a generic
type argument.

## Synchronization and storage

- Mutex operations use `Monitor` with explicit ownership and recursion depth.
  Ordinary and recursive mutexes are distinct resource kinds. Try-lock performs
  no per-call delegate or closure allocation.
- Condition wait registers its waiter before releasing the mutex. Signal and
  broadcast select waiters while holding the condition's private lock, so the
  enqueue/unlock interval cannot lose a notification. Every exit reacquires the
  mutex. Timed waits recheck the absolute realtime deadline in bounded BCL waits,
  including wall-clock changes and out-of-range times without integer overflow.
- Once execution publishes completion after a successful callback, resets the
  state when it throws, and wakes waiting callers. The callback runs without a
  context-wide lock held.
- TLS values are per owner and managed thread. Values are cleared individually
  before their destructors run; another key remains observable until its own
  turn. A destructor can reinstall a value, with up to four destructor passes.
  Foreign host threads must use the generated `ReleaseThread` contract.

The context owns BCL objects and token dictionaries. Caller-owned token fields
remain in ordinary translated frames, native heap objects, or pinned global/TLS
arenas. Thread creation allocates its BCL thread/state once. It does not pin a
temporary managed array or allocate a closure for each synchronization operation.
Threads are BCL background threads so process-entry return does not acquire a
new .NET foreground-thread lifetime rule; explicit owner shutdown still waits
for their retained leases.

Each runtime call requires its generated owner's ordinary active-call lease,
or a retained worker/cleanup scope. The context is not a substitute for that
outer lifetime protocol. Its disposal rejects outstanding joinable threads,
active workers, unreleased TLS values and synchronization operations in use.
Tokens from another owner are rejected. Independently generated runtimes do not
share tokens or a native synchronization domain.

## Retained worker lifetime

1. Retain the owner before starting the BCL thread. Release that lease if thread
   creation fails; successful creation transfers it to the worker.
2. Enter the retained worker context before invoking translated code. This
   entry is allowed during shutdown while the storage remains leased.
3. On normal return, leave the active callback scope, then invoke retained
   `ReleaseThreadCore(true)`. It drains C++ thread-local destructors, then these
   TLS-key destructors, then libc thread state, and only then clears storage.
   The original TLS arena remains alive throughout destruction and reentry.
4. Release the retained owner lease last. Shutdown waits for active calls and
   retained workers, including detached workers and workers created by global
   destructors while shutdown is already running.

An exception escaping a worker invokes the termination adapter from a CLR
first-pass filter, before unwinding translated frames. The current generated
adapter follows the backend's `FailFast` termination boundary. Wiring the full
translated `std::terminate` handler provider remains an EH/library task.

## Verification status

`libcxx_threads_runtime_verify.py` passes JIT and Linux x64 NativeAOT in
`artifacts/campaign/libcxx-threads-runtime-003/`. It checks contention, recursive
locking, condition signaling/broadcast/timeouts, once retry after exceptions,
static and owner callbacks, TLS destructor passes, forced GC, owner separation,
and detached-worker shutdown. Fatal child processes emit `body` then `terminate`
without the intervening `finally` marker, verifying first-pass termination.

`libcxx_threads_oracle.c` is a synthetic Linux-only native reference adapter.
It is never linked into the generated C# runtime. The nonthrowing fixture uses
the original EDG C backend with `--no_exceptions` because the generic `libC.a`
reference build does not configure a concurrent EH stack. The harness selects
glibc's supported thread-exit hook instead of that runtime's unconfigured
fallback. The original generated C is unchanged. This oracle and an independent
Clang C++ compilation pass in `libcxx-threads-oracle-002/`.

`libcxx_threads_verify.py` adds generated C# acceptance for the same calls,
independent owners, C++ TLS/key/errno destruction order, shutdown racing a
detached worker, and a global destructor creating another worker. All six
native-reference and generated static/instance JIT/NativeAOT checks pass in
`artifacts/campaign/libcxx-threads-translated-001/`.
`libcxx_threads_fatal_verify.py` also passes four generated static/instance
JIT/NativeAOT child-process checks in `libcxx-threads-fatal-002/`: an exception
escaping a worker terminates during the first handler search, before RAII
cleanup. The fixture explicitly flushes its body and destructor markers so
process termination cannot hide a buffered marker.
These component tests do not yet establish complete `std::thread`, futures,
mutex/condition-variable classes or the platform execution matrix; their
out-of-line libc++ code and exception providers remain required.
