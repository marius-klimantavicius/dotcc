# Process and owner termination implementation design

Status: process-mode `exit`, `_Exit`, `quick_exit`, `atexit` and
`at_quick_exit` are implemented and accepted in synthetic native C/C++,
translated static/instance JIT and Linux NativeAOT tests. Executable main return
uses normal process termination; managed `Run` returns to its host. Main-thread
`thrd_exit` and isolated owner termination remain open and are not implied by
per-instance resource isolation. The existing assertion/abort path is unchanged.

## Source distinctions to preserve

| Trigger | Automatic source objects | Callbacks and stream processing |
| --- | --- | --- |
| Return from initial main | Normal scope destruction | Normal registrations, then stream flush/close |
| `exit(status)` | No caller scope unwinding | Normal registrations, then stream flush/close |
| `quick_exit(status)` | No caller scope unwinding | Only quick registrations, then immediate exit |
| `_Exit(status)` | No caller scope unwinding | No registrations; choose no implicit flushing |
| Worker `thrd_exit(result)` | Accepted translated thread cleanup policy | Automatic/C++ TLS/TSS cleanup; join result |
| Main `thrd_exit(result)` | Thread cleanup, not program exit yet | Wait for remaining source threads; last thread causes normal status0 exit |

Normal and quick registrations are separate, support at least32 entries, and
run in reverse registration order. Registrations made by a running handler are
popped next. Repeated/mixed `exit` and `quick_exit` calls are not a supported
reentrancy contract. [C11 draft N1570](https://www.sigbus.info/n1570#7.22.4).

For C++ normal program termination, calling-thread TLS destruction and static
object/atexit ordering also apply. Do not equate C TSS callbacks with C++ TLS
finalizers: immediate process termination must not run other threads' user
cleanup. Dedicated native C/C++ synthetic oracles will pin these sequences.

## Explicit mode and host boundary

Propose `--process-exit process|owner`, independent of namespace, class shape,
splitting and `--instance-methods`. Default `process` preserves existing abort
behavior. Owner mode is an explicit embedding contract; it must be recorded in
metadata and supported by generated host APIs. The coordinator accepted the explicit policy and process default; owner mode
must remain unavailable until worker cancellation and quiescence are complete.

In process mode, drain the requested normal/quick callbacks on the requesting
thread and use `Environment.Exit(status)`; immediate exit skips callbacks.
This naturally stops all worker threads without returning through caller scope
cleanup. Actual process exit is preferable to throwing an ordinary CLR exception
and accidentally running C++ automatic destructors. .NET documents that process
exit terminates background/foreground threads without running their finally
blocks. [Official .NET shutdown documentation](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Hosting/docs/HostShutdown.md).

In owner mode, an immutable owner-tagged termination request carries kind,
status and requesting thread. The first request wins; a TaskCompletionSource
with asynchronous continuations notifies the host without reentering it inside
a callback. A dedicated `ProgramExitTransfer` crosses only matching owning
entry/worker boundaries. Other instances remain active; there is no ambient
fallback singleton. Ordinary exported functions cannot fabricate a return value
after source exit: they throw the owner-tagged transfer to their host boundary.
`Run` can expose a process-result API once it owns complete termination handling.

Publishing a request does **not** make native storage safe to free. Owner mode
must stop new dispatch, quiesce workers, finish the chosen termination callbacks,
and release resources only after all active bindings/leases have left. Shutdown
from a terminating worker cannot synchronously wait on its own lease.

The runtime cannot safely force-abort arbitrary CLR threads: `Thread.Abort` is
unsupported in modern .NET. [Official Thread.Abort API](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.abort?view=net-10.0).
Consequently complete owner termination requires compiler polling at loop
backedges and call/entry boundaries, plus cancellation-aware runtime waits.
Polls are allocation-free until a request exists. Blocking foreign operations
that cannot be interrupted retain the owner and report incomplete quiescence;
they never permit forced disposal or false successful termination. Until this
infrastructure is accepted, owner request notification alone is a partial
hosting primitive, not full source-process exit equivalence.

## Compiler and runtime integration

Use a shared owner termination service with typed static/generic function-pointer
callbacks. Normal `atexit` and existing `__cxa_atexit` registrations must share
one ordered stack. A second normal stack would break their interleaving. Each
record distinguishes no-argument callbacks from argument-bearing C++ destructor
callbacks; never cast incompatible managed calli signatures. Keep quick handlers
in a separate stack. No locks span user callbacks; pop an entry before invocation.
Catch registration OOM and return nonzero without losing existing entries.

The future owner-mode process transfer differs from `CThreadExitTransfer`: generated language
catch/noexcept/dynamic-spec filters ignore both, but source automatic cleanup
must be suppressed for process exit and retained for thread exit. Runtime-only
finally blocks still release invocation leases and temporary allocations.
Thrown-payload construction must retire pending uncaught markers and free its
native carrier deterministically even when a process transfer crosses it.

Current `Shutdown` waits all owner leases and invokes user global/TLS cleanup;
it is unsuitable for immediate/quick exit. Add a distinct resource-only teardown
path, preserving strong storage ownership until quiescence. It must reclaim
completed unjoined thread records without requiring public joins, and discard
mutex/condition bookkeeping only when no thread can still reference it.
`LibcFiles` needs an explicit close/flush policy so `_Exit` does not silently
flush merely because IDisposable was called. Temporary-file removal remains a
provider gate until `tmpfile` exists.

For main `thrd_exit`, establish a source-thread boundary before invoking main,
release initial-thread TLS, and wait without holding its active-entry/worker
lease. New workers may still be created by live source workers. Atomically
recognize the transition to no live source threads, select status0, and execute
normal process termination. Host threads that call exports are not silently
counted as source threads; their thread-exit behavior needs an explicit host
entry contract.

## Concrete implementation sequence

1. Implement the final BCL process-exit primitive and prove it stops live workers
   without caller/worker finally execution under JIT/NativeAOT. Keep accepted
   source abort behavior unchanged.
2. Integrate `atexit`/`at_quick_exit` into generated lifecycle ordering and test
   normal main return, including C++ global destructors and callback addresses.
3. Implement process-mode exit/quick/immediate via fatal subprocess fixtures,
   preserving automatic-vs-static destruction, stream buffering and live workers.
4. Add main thread-exit boundary and last-thread completion with native oracles.
5. Prototype owner request/callback notification and then add owner-mode
   compiler polls/runtime wakeups, owner-tagged host result and
   resource-only teardown; test competing instances, worker-requested exit,
   never-returning source loops, blocked waits and A→B→A host reentry.

Dotcc reference: `DotCC.Libc/RuntimeTerminationLib.cs` provides owner-tagged
requests/notification and explicit unbound process exit; `StdlibLib.cs` explicitly
says it lacks atexit handling. Reuse those ideas, not their incomplete libc
semantics or ambient binding mechanism. Preserve every candidate and failed
probe. All acceptance uses synthetic sources and JIT/NativeAOT, not real projects.

Accepted process-only integration contract:

- `LibcProcessExit.Exit(int)` is the final BCL primitive.
- `atexit`/`at_quick_exit` accept only exact `void()` callback ABI and delegate
  to generated `__edg_register_exit(nint,bool quick)`.
- `exit`/`quick_exit` call `__edg_process_exit(int,bool quick)`; `_Exit` directly
  invokes the BCL primitive. Normal dispatch drains calling-thread C++ TLS and
  unified normal callbacks, flushes existing FILE state, then exits. Quick
  dispatch drains only quick callbacks. No path joins workers or frees the
  translated native heap before actual process termination.
- Executable main-return dispatch uses the normal process helper. Managed `Run`
  remains host-controlled; ordinary owner shutdown still preserves active leases.

The synthetic fixture `libc_process_exit.cpp` tests native C/C++ ordering,
registration during callbacks, forty registrations, source-return versus exit
automatic lifetimes, skipped TSS destructors, quick/immediate buffered output,
and a live worker that never exits voluntarily. Initial native12 evidence is
`libc-process-native-20261003-023000/results.json`; additional normal-main-return
with a live worker is included in the upcoming generated matrix.

## Acceptance and retained evidence

`artifacts/campaign/libc-process-translated-20261003-030100/results.json`
records eighteen checks: native C/C++ seven-mode oracles, all eight generated
executable configurations, and eight managed-host configurations. Each
executable checks normal return, exit, quick exit, immediate exit, forty callback
registrations, exit with a live worker, and main return with a live worker.
Managed tests retain host-controlled return and explicit ReleaseThread/Shutdown;
instance mode checks two independent owners. Callback routine addresses are
exercised, as are exact automatic/TLS/TSS/global destruction traces and buffered
file bytes. No active worker is joined or has its native storage reclaimed
before process termination.

`libc-process-abi-20261003-030100/results.json` records nine rejected malformed
callback/result/status declarations. `libc-process-candidate-20261003-023000/proof/results.json`
records standalone JIT/AOT termination without caller/foreground-worker finally
execution. These are Linux execution results; BCL-based implementation alone
does not establish Windows/macOS acceptance. Registration allocation failure,
concurrent exit behavior outside defined source contracts, signals and temporary
file removal are not covered by this matrix.
