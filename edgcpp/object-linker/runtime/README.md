# Shared runtime bindings

`bind.py --project OUTPUT` supplies implementations for the runtime contracts
emitted by `dotobj-link link --allow-externs`. It reads the owned link metadata
and source-layout map; it does not need EDG or the original C/C++ input files.
The generated project then builds normally with `dotnet build` or NativeAOT.

The implementation source remains in `edgcpp/backend/runtime`. Bindings reuse
the direct backend's maintained import tables and helper dependency graph.
Allocation helpers are selected from their existing canonical definitions in
`dot_gen_imports.h` and `dot_gen_lifecycle.h`; allocation algorithms are not
forked here. Heap allocations use the same `NativeMemory` implementation.
GNU atomic imports follow the concrete EDG ABI documented in
`dot_gen_atomics.h` and call the existing shared `AtomicOps` implementation.
This includes size-leading generic operations, memory orders, weak compare
exchange, and the older `__sync` operations; no separate synchronization
algorithm is introduced.

Platform imports live in `Edg.Runtime.LibC`. Imports whose signatures mention
translated records stay in their translated owner. Adapters bridge opaque
`FILE*`/`DIR*` and selected portable POSIX structures only after validating the
owned size, alignment, member names, member types, and offsets. A foreign
structure with the same tag name does not qualify. These boundaries connect to
the existing BCL runtime, including its runtime-selected platform operations.

`stdin`, `stdout`, and `stderr` use the actual pinned pointer cells owned by
`LibcFiles`; taking their address or changing their value does not access a
second copy. `errno` resolves to the runtime's per-thread pinned cell. Accessors
are evaluated on the accessing thread. A static pointer relocation to one of
the standard stream cells therefore aliases the same cell as later loads and
stores. The `LibcFiles` and POSIX services share descriptor state.

Locale handles and `lconv` use validated portable layouts. Narrow and wide
FILE formatting/scanning callbacks use the canonical `LibcFiles` session
methods, so callback execution shares the stream lock and `VaList` ABI.
Timezone primitives use `LibcTimezone`; musl retains calendar and timezone rule
algorithms. The binder reuses `dotnet.py`'s hash-checked timezone bundle and
embeds it through a generated `Directory.Build.targets`. The receipt includes
asset hashes and version. No system timezone database is required for named
zones in the pinned bundle.

The current provider owns static per-assembly state. Independent engine
instances and explicit owner disposal are not implemented by this binder yet.
`process.py` supplies `atexit`, `at_quick_exit`, `exit`, `quick_exit`, `_Exit`,
and the exact `__cxa_atexit`/`__cxa_thread_atexit` callback ABIs. It extracts the
canonical registration, reverse-order drain, and termination bodies from
`dot_gen_lifecycle.h` and `dot_gen_process.h`; callback algorithms are not forked.
Registrations during a drain are included, callbacks run outside the registry
lock, and thread finalizers remain local to the registering thread. The static
assembly profile does not unload DSOs; `__cxa_finalize` remains unsupported.

The source executable boundary (`ObjectProgram.Run`) performs normal process
termination after main returns. If FILE services are selected, this hook also
flushes buffered output when no explicit exit function was imported. Quick exit
drains only quick-exit callbacks; `_Exit` skips callbacks and stream flushing.
`InitializeLibrary` and ordinary library exports do not terminate the host or
implicitly drain global callbacks. `ObjectProgram.ReleaseThread`, when process
support is present, explicitly drains this assembly's registered C++ thread
destructors on the calling thread. No CLR finalizer or host `ProcessExit` event
is used. Source C++ lifetime scheduling and instance-owned disposal still
require their corresponding backend support.

`tests/runtime_process_verify.py` checks native/JIT/NativeAOT termination order,
status codes, callback re-registration, buffered flush, explicit TLS release,
concurrent registration, library hosting and malformed callback-ABI rejection.
The native destructor-registration oracle currently uses the Linux C++ ABI.

`threads.py` checks the maintained C11 and libc++ external thread tables and
connects them to `LibcxxThreads`, `LibcC11Threads` and `LibcInitialThread`.
Synchronization, thread creation, TSS, and retained-worker algorithms remain in
the canonical BCL runtime. The object adapter reuses the direct backend's thread
lease bodies and supplies the concrete static assembly owner. Portable tokens
and validated `timespec` layouts cross this boundary; native pthread layouts do
not. Stateless libc++ atomic waits use the existing `LibcxxAtomicWait` service.

Worker completion and explicit hosted `ReleaseThread` run C++ TLS finalizers,
then TSS destructors, release libc TLS, and finally clear owned source TLS.
The binder requires the linker's `ownedThreadStorageRelease` bridge contract
and refuses older outputs that cannot reset source storage correctly. An
executable establishes the initial source thread before initialization; a
`thrd_exit` transfer completes that logical thread, allows workers to join it,
and leaves the last remaining source thread responsible for normal status-zero
process termination. Runtime thread-exit transfers bypass source C++ handlers.
Library initialization does not establish process-entry semantics.

Hosted `ReleaseThread` also resets source TLS and any selected libc TLS when
only process/destructor callbacks are needed. It does not require linking the
C11 or libc++ thread services. Destructors run before storage is cleared, so
they can still access the current source TLS and errno; the next access starts
with their declared initial values. Recursive release during a destructor is
rejected while the outer release owns cleanup; registrations added by callbacks
are still drained in reverse order.

`tests/runtime_threads_verify.py` packages the existing EDG C11 fixtures and
the portable thread provider as independent original-source objects. It checks
contention, recursive/timed mutexes, conditions, once flags, TLS/TSS cleanup,
integer thread results, initial-thread join/detach/descendants, and hosted source
TLS reset in JIT and NativeAOT. These runtime boundaries do not establish full
source C++ RAII or exception lowering.

`guards.py` binds the portable target's 64-bit Itanium `__cxa_guard_acquire`,
`__cxa_guard_release` and `__cxa_guard_abort` signatures. The registry, per-guard
monitor and volatile publication operations come from the canonical managed
lifecycle implementation. `tests/runtime_guards_verify.py` compares contention,
single initialization, abort/retry and independent-guard progress against native
Itanium guards, JIT and NativeAOT. It validates the runtime ABI only: the owned
source graph must also preserve EDG's initialization-guard markers so the outer
fast-path read has acquire semantics, and source exception lowering must invoke
abort when construction fails. True dynamic-static initialization remains gated
until those source semantics are represented.

The binder is intentionally incomplete: unsupported imports remain unresolved
partial methods. No native fallback or fabricated success body is generated.
`--allow-unresolved` can stage the known bindings for review, but unresolved
methods still fail compilation. In strict mode a failed binding attempt writes
only its diagnostic receipt. Use a fresh linked output for each attempt; existing
outputs are retained rather than overwritten.

Project-specific providers can be supplied with repeated
`--provider-receipt RECEIPT.json`. The binder checks the exact link-report hash,
external identity, semantic signature, generated owner/name, and every supplied
implementation file hash. Missing requirements, duplicate providers, changed
implementation files, and stale contracts fail before runtime emission.

For the prepared SQLite Unix/APPDEF profile, first run
`sqlite.py --project OUTPUT`, then pass its `runtime-sqlite.json` receipt to
`bind.py`. This reuses `sqlite/src/HostVfs.Mutex.cs`, validates the nine typed
SQLite mutex callbacks, and reads the needed integer constants from the owned
macro map. It supplies the mutex table, memory barrier, and runtime-selected
database mmap support hook. No original SQLite headers are needed at binding
time. The current adapter uses assembly-lifetime static mutex identities.
`tests/sqlite_runtime_verify.py` verifies this boundary against the actual public
SQLite header, contention, GC, source-free binding, and NativeAOT; it does not
establish full engine execution.

`runtime-bindings.json` records the exact canonical source hashes, import-table
hashes, reused runtime files, generated files, and unresolved requirements.
This is binding evidence, not execution evidence. Run
`tests/runtime_bindings_verify.py --output-dir FRESH --compiler CPFE_DOT
--linker DOTOBJ_LINK_DLL --nativeaot` for native/JIT/NativeAOT comparison,
standard stream address aliases, descriptor interoperability, typed layout
rejection, allocation semantics, and errno thread/GC isolation. The native
oracle currently requires a POSIX host; verified platform coverage is recorded
separately from the runtime's cross-platform design.
