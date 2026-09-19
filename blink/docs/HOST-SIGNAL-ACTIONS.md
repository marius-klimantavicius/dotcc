# Private signal disposition registration

`src/Host/HostSignalActions.c` stores signal dispositions in
a bounded per-worker table. It implements registration, old-action output,
and queries. It installs no operating-system handlers and provides no
asynchronous dispatcher. `kill`, `raise`, and `sigsuspend` remain outside
this module; registering a handler does not qualify incoming signal delivery.

Call `BlinkHostSignalActionsBegin` before registration. A second Begin
without End returns `EBUSY`. `BlinkHostSignalActionsEnd` clears every record
and ends the registration context; calls outside a context return `ENODEV`.
A new context starts with default dispositions and empty action masks.
One context belongs to one dedicated C worker; reset it only after upstream
state that relies on its registrations is discarded. The table has 65
native-measured 152-byte action slots, including unused/reserved positions.
It contains function-pointer values and inline mask bytes, never CLR object
references. The emitted TLS backing remains rooted with the worker.

## State and policy

Ordinary `sigaction` calls preserve `SIG_DFL`, `SIG_IGN`, and actual simple
or siginfo function pointers. Records are copied by value, so later caller
changes do not mutate the registry. Queries and old-action output observe
that stored value. Input is captured before old output is written, including
the private supported case of aliased input/output pointers. Rejected calls
leave registry state and old output unchanged.

Valid public signal numbers are 1–64 excluding the native libc-reserved
32 and 33. `SIGKILL` and `SIGSTOP` allow queries but reject updates with
`EINVAL`. Invalid numbers and `SIG_ERR` registration are rejected. Only
`SA_SIGINFO` and `SA_RESTART` are accepted as registration metadata.
`SA_SIGINFO` selects the siginfo callback shape; `SA_RESTART` is retained
and queried, without claiming any syscall restart behavior. Other flags,
including alternate-stack, nodefer, reset-hand, and child-process flags,
return `ENOTSUP` until their delivery/process semantics are implemented.

Action masks retain their low 64 bits except `SIGKILL` and `SIGSTOP`.
Native action masks retain reserved bits 32 and 33 when explicitly supplied;
this differs from the separately qualified `sigprocmask` operation.
Unused higher mask storage is zeroed. Native libc inserts its own restorer
trampoline and private `0x04000000` flag; this module has no native signal
trampoline, does not read the caller's restorer field, and reports a null
restorer with no private flag. Native comparisons cover public fields and
these explicitly documented differences, not arbitrary padding bytes.

`signal` uses the measured native BSD registration form: `SA_RESTART` plus
the signal's own mask bit, returning the previous handler or `SIG_ERR` on
failure. Successful registration/query preserves `errno`. Neither default
nor ignore sentinels are callable function pointers.

## Upstream use and integration

`SysSigaction` in `blink/syscall.c` owns the guest Linux disposition records.
It separately asks the host to register default, ignore, or `OnSignal`,
which only enqueues a guest signal. Host registration uses `SA_SIGINFO`
and may request `SA_NOCLDSTOP`/`SA_NOCLDWAIT`; these child-process flags are
explicitly rejected here. Upstream logs host registration failures and
has already stored its guest record, so this boundary does not upgrade that
behavior into complete signal semantics. `ResetSignalDispositions` also
uses `signal(..., SIG_DFL)`. The diagnostic `debug.c` fault-probing handlers
were removed by the separate qualified diagnostic adaptation; CLI/TUI
operating-system handler setup is not selected for the managed core.

Include `HostSignalActions.h` in the binding preamble and compile its C
implementation. C struct tags and function names occupy separate namespaces,
while nested C# types and methods cannot share a name. The new header leaves
the measured `blink_host_sigaction` struct tag intact and maps ordinary
`sigaction(...)` calls to the distinct `blink_host_register_sigaction`
implementation. Its explicit declaration supports taking and calling the
actual registration function pointer. A bare upstream `sigaction` designator
is not qualified by this profile; none is used by the selected source.
No shared signal ABI layout or existing HostSignals/HostSignalOps source
was changed.

## Evidence and compiler findings

Run `python3 blink/tests/HostSignalActions/run.py` for native/staged and
raw/optimized JIT/AOT checks. Add `--object-link` to emit the probe and
adapter as separate objects before linking the same four managed modes.
Fixtures compare registration/query behavior, public flags, action masks,
default/ignore transitions, copy semantics, invalid and unmaskable signals,
and explicitly invoked retrieved simple/siginfo pointers. Direct invocation
tests pointer preservation only; they are not asynchronous delivery tests.
Private tests also check lifecycle reset, unsupported flags, aliased records,
the actual authored registration function pointer, and two workers holding
different dispositions across forced compacting GC.

Test-only native queries compare real process dispositions and blocked
signal masks before and after the private operations. Managed tests query
both SIGUSR1 and SIGUSR2; the product contains no native signal calls.
Each runner freezes source/header/compiler identities, preserves raw C#,
and treats `CS8500` as a build error.
Direct-emission receipt `artifacts/host-signal-actions/attempt-c37fwec0/receipt.json`
passes native/staged and all four managed modes after B019. The final
separate-object receipt `artifacts/host-signal-actions/attempt-1qbw0fxz/receipt.json`
passes the same matrix with both B019 and B020 corrected. Focused repository
checks pass 12 units and five functional cases, including native and
direct/object-linked two-worker GC regressions; two external oracle rows
are skipped because those tools are unavailable. The centralized full suite passes a warning-free build,2238 unit and520
functional tests, with1041 explicit skips (artifacts/repository-tls-global-member.log).

The first concurrency test exposed B019: explicit `_Thread_local` arrays
lost their storage flag during IR construction and became shared arrays.
Native reduction `tls-record-array.c` passed two workers, while emitted
explicit scalar/record arrays were shared and typedef arrays were correctly
thread-local (`artifacts/host-signal-actions/tls-array-q49gip58`). The fix
preserves storage flags through the normal array paths to the existing
pinned TLS backend. Unsupported initialized TLS arrays now fail explicitly
instead of silently using shared storage.

The reduced direct/object-link GC regression then exposed B020: object
linking deduplicated global lines and removed repeated getter braces and
`[ThreadStatic]` attributes. The fix merges complete generated members,
retaining their attributes and bodies. The original explicit registry table
was retained throughout; no storage reshaping hides either compiler defect.
