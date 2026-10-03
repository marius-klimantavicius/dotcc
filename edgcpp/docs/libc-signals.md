# Synchronous signal profile

The authored `libc/signals` component implements source `signal`, `raise`,
`abort` and `__assert_fail`. Its two C translation units contain the handler
state and dispatch; no native libc or ambient managed signal registry is used.
This is the synchronous part of the signal provider. External OS delivery,
POSIX masks/actions and the default C++ terminate-to-abort connection remain
required integration work.

## Representation and delivery

`sig_atomic_t` is signed 32-bit. Six public values use the portable profile's
fixed numbering: SIGINT2, SIGILL4, SIGABRT6, SIGFPE8, SIGSEGV11 and SIGTERM15.
The function-pointer sentinels are SIG_DFL0, SIG_IGN1 and SIG_ERR-1. Generated
C/C++ tests check these encodings, the exact callback signature and ordinary
routine-address calls. They are translated ABI values, not assertions that all
native operating systems use this numbering.

A six-entry source function-pointer array becomes ordinary pinned owner global
storage. Generic GNU atomic operations map through the accepted EDG lowering to
exact-width AtomicOps operations. Registration atomically exchanges the handler;
delivery loads it and uses a compare/exchange loop to replace a callable handler
with SIG_DFL **before calling it**. SIG_IGN stays installed. All initial entries
are SIG_DFL. Invalid signal numbers and SIG_ERR registrations return SIG_ERR
(or -1 from raise) with EINVAL; successful dispatch preserves errno unless the
handler changes it. Arbitrary invalid function pointers remain source undefined
behavior, rather than values the provider pretends it can validate.

The callback runs on the source caller's thread before raise returns. Existing
canonical managed function-pointer adapters supply the current owner and entry
lease. There is no closure, callback dictionary or transient pinned allocation.
The atomic table provides publication and concurrent registration as a profile
extension; tests verify actual cross-thread exchange and zero steady-state
registration allocation. Owners have separate dispositions while sharing the
same generated code address. Disposing one owner leaves another owner's table
unaffected.

Reset-before-call is an explicit implementation choice allowed by the C signal
contract. The native host oracle does not assume its own libc makes the same
choice. C also limits recursive raise from handlers and returning handlers for
computational-exception signals; the tests only return from SIGINT/SIGABRT
handlers and ignore the other signal numbers.
[C11 draft N1570, 7.14](https://www.open-std.org/jtc1/sc22/wg14/www/docs/n1570.pdf).

## Fatal and nonlocal paths

Default delivery calls the fixed private `__edg_signal_terminate(int)` import.
`LibcSignals.Terminate` invokes Environment.FailFast using a fixed literal,
without formatting or allocation before termination. Source catches, managed
catches/finally, automatic C++ destructors and atexit callbacks do not intercept
or run for that action. This is process-wide termination, including when a
managed library owner calls it; an owner-only termination mode is still absent.

The source abort implementation dispatches SIGABRT and, if dispatch returns,
invokes the same fatal import regardless of the handler's new disposition.
Ignoring SIGABRT cannot make abort return. Recursive abort after a callable
handler has been reset reaches the default action. A C handler can instead leave
through longjmp; no runtime finally forces abort after that nonlocal transfer.
The generated C test verifies this separately. C++17 disallows throwing and
other non-signal-safe evaluations in a signal handler; no new exception recovery
semantics are claimed here.
[C++17 draft signal rules](https://timsong-cpp.github.io/cppwp/n4659/support.runtime).

Assertions first call diagnostic-only `__edg_assert_report`, which uses owner
stderr and the existing orientation-independent output path. Diagnostic failure
is best effort; the source wrapper then calls source abort so SIGABRT handlers
are honored. Existing LibcProcess.Abort and LibcFiles.AssertFail remain available
for the bridge without this source component. A selected source definition wins
over its external fallback import.

## Remaining boundaries

* Asynchronous external SIGINT/SIGTERM and other OS delivery are not subscribed.
  Executable and embedded-library ownership, lifecycle and thread entry must be
  designed before BCL registrations can safely dispatch source callbacks.
* Invalid memory access and CLR arithmetic/hardware faults are not converted to
  resumable C signal callbacks. Explicit source raise is a separate operation.
* POSIX sigaction, signal sets/masks, blocked/pending delivery and process status
  encoding are not supplied by this profile.
* The backend's current default C++ terminate path uses an internal FailFast.
  Routing the standard default terminate handler through this owner-aware abort
  path remains an EH integration requirement.
* Linux x64 JIT/NativeAOT evidence does not establish Windows/macOS execution.

## Provenance and verification

`source.json` has empty upstream sources and hashes for the two authored adapters;
registration uses `sourceBase: libc`, not a musl provenance claim. The EDG C backend
preserves these ordinary C operations and GNU atomic lowering; no EDG code was
changed. dotcc's ambient Console.CancelKeyPress registration and partial POSIX
stubs were audited as references and are not reused.

The manifest marks the implementation boundary explicitly:

```json
{"signals":{"version":1,"delivery":"source-raise-abort","state":"owner","reset":"before-call","defaultAction":"process-failfast","osDelivery":false}}
```

`libc-signals-candidate-20261003-055000` retains the initial isolated source/native
prototype. `libc-signals-translated-20261003-060000` contains the first 11-check
matrix. Review then replaced fatal-message concatenation with a fixed literal
so a formatting allocation cannot throw before FailFast. The final runtime
matrix passes all 11 checks at `libc-signals-translated-20261003-060500/results.json`. `libc-signals-abi-20261003-060000/results.json`
contains eight rejected private import signatures. All paths are beneath
`artifacts/campaign/`; none of the earlier evidence was removed.

There are 19 focused checks in the final matrix and ABI suite. The generated
matrix includes C/C++ static/instance modes under both JIT and NativeAOT; each
mode executes seven fatal subprocess cases in addition to normal owner/atomic
checks. Fatal cases include the separate assertion handler path; subprocess scenarios
are recorded within each matrix check rather than inflating the check total.
