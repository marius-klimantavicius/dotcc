# P5: selected Kestrel guest runtime

The required workload is the real ASP.NET Core/Kestrel service in
`tests/KestrelService`. Its static NativeAOT ELF is interpreted by translated
Blink. The previous synchronous raw-socket service remains a separate baseline
in [P5-NATIVEAOT-RUNTIME.md](P5-NATIVEAOT-RUNTIME.md).

## Producer and native control

The pinned SDK container is
`mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64@sha256:240a20b94625153877c8ec4ed0394d3396f243a5663fe74b8f601c31f1dac3fc`.
The standard publish uses SDK 10.0.401 and runtime/ASP.NET/ILCompiler 10.0.12.
The 9,371,272-byte ELF has SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`,
no INTERP or NEEDED entries, and PT_TLS file/memory sizes 24/296, alignment 8.
The native producer receipt is `kestrel-guest-musl/attempt-o5jvvf7t/receipt.json`
under `artifacts/`; the complete pin and commands are documented beside the fixture.

The selected native configuration uses the unchanged ELF and six guest variables:

```
LANG=C
DOTNET_GCHeapHardLimit=1000000
DOTNET_GCRegionRange=2000000
DOTNET_GCRegionSize=100000
DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
DOTNET_EnableDiagnostics=0
```

The GC values are hexadecimal runtime settings: 16 MiB heap limit, 32 MiB
region range, and 1 MiB region size. Configuration reload and diagnostic IPC
are disabled for this fixed fixture; genuine Kestrel sockets transport remains.
These values belong to the guest environment, not the emulator process.

`kestrel-native-profile/attempt-zphi57zq/receipt.json` has SHA-256
`e1e3c2ecf4c929f6f13d0f4937757cdc0dc82ee2b55d2c76d1fd88c4ec7db01a`.
Its five ordinary cases are health, 3,592-byte padded health, the same request
in seven writes, missing route, and normal HTTP stop. All pass natively. The
comparison requires status, headers and body, allowing only the actual Date
value to differ after RFC1123 validation. Raw protocol bytes are retained.

The selected trace records 12 TIDs, actual clone/futex/TLS setup, memory mapping
and protection, resource queries, signal dispositions/masks, descriptor
operations and IPv4 TCP. Its async transport uses registered epoll, nonblocking
accept/receive/send, MSG_PEEK, TCP_NODELAY and zero-second SO_LINGER. The trace
also contains optional probes; observing a call is not proof that every return
must succeed or that every .NET application requires that operation. Host launch
execve is not guest exec support. The exact syscall inventory is retained in the
native profile receipt and trace SHA-256
`5114818b518b04fb4e1902f24f3faa2dfda9edcac6c7b06d21c91ffe32122132`.

## Concrete extensions and current gate

The registered socket boundary passes native and raw/optimized JIT/NativeAOT at
`host-async-sockets/attempt-n7spb0qb`. Its finite contract reports real readiness
and renews an edge after an actual EAGAIN drain. It preserves descriptor aliases,
opaque event data, bounded fairness, peeking and ordinary stop/disposal. It does
not claim arbitrary Linux edge-trigger transitions, one-shot/MOD, positive
linger duration or nonblocking connect. Upstream converts guest packed epoll
records into the private host ABI; the bridge returns actual event records.
See `tests/HostAsyncSockets/README.md` for exact coverage and limitations.

Initial translated Kestrel startup fails before readiness with runaway
Hashtable growth. Immutable observations at
`kestrel-guest-execution/attempt-float-registers-smhynls8` identify a pinned
upstream instruction defect: CMPORDSS stores its integer mask through the
floating member of a union. True becomes `bf800000`, not `ffffffff`; the
following ANDPS/CVTTSS2SI turns valid growth products 2.16 and 5.04 into zero
thresholds. The generated code faithfully reflects the upstream defect.

Commit 9d2dc31 changes only eight comparison-result stores to the integer union
member in reviewed staging. The immutable upstream tree and generated output
are not hand-edited. Ten normal scalar/packed mask and actual threshold-sequence
regressions are added; all 514 selected native cases pass, with the original
550 descriptors and 46 fault-case exclusions preserved. Original failures stay
in the native receipt. Managed regression now passes all 2,056 comparisons;
actual corrected Kestrel execution remains partial as described below.

The old sysinfo fallback's zero available-memory result is a separate known
modeling discrepancy. It is not the cause established by the SIMD observation.
No guest quota increase or native-emulator fallback is substituted for the fix.


## First actual Kestrel HTTP and activation recovery

The corrected instruction matrix passes all 2,056 managed comparisons at
`cpu-conformance-managed/attempt-jt41ulk6`. The next ordinary startup failure
exposes the selected 64 MiB address-space ceiling: recorded reservations leave
nine pages, while the Gate thread requests 67. The reproducible reconstruction
and its trace-order/peak-accounting limits are in
`tests/KestrelGuestExecution/reservation-evidence.py`.

The C# owner now supports a selected 128 MiB coupled AS/DATA/backing ceiling,
while preserving the 64 MiB default and unchanged native guest GC settings.
With that profile, `kestrel-guest-execution/attempt-uq52p1wf` reaches READY and
passes health, large and fragmented HTTP comparisons. This is partial execution
evidence, not a service qualification: signal 35 to another guest thread returns
EOPNOTSUPP, followed by a guest worker SIGABRT. The missing-route response and
normal stop remain uncompleted. All nine workers/Machines and resources release.

The actual missing boundary is the internal pthread wake following upstream
signal queuing. The native runtime installs signal 35's ActivationHandler, but
the native five-case trace does not issue the activation observed in managed
execution. Managed wake, sender metadata and C# handler accounting changes were preserved
in [RESTART-P5-KESTREL.md](RESTART-P5-KESTREL.md), then recovered under the user's
2026-09-23 resume instruction. No real host signal or successful no-op
substitutes for that contract.

## Resumed ownership and clock qualification

The recovered per-owner transient wake helper passes 32 translated normal
scenarios and one direct BCL reference in each host form at
`host-io-cancellation/attempt-7bwtbsow`. `SignalActor`, `KillOtherThreads`,
`SysExitGroup` and `SysExit` now select typed managed function overrides; their
upstream C bodies remain unchanged. Narrow launch, signal-frame and sender
metadata adaptations retain the upstream algorithms. The selected boundary
profile, exact contracts and remaining hunks are recorded in
`config/managed-boundaries.json` and `src/UpstreamGuestThreads/README.md`.

Normal GuestSignals qualification passes native Linux and all four managed
forms against public delivery `translation/attempt-8roztrys`. Its managed
receipt `guest-signals-managed/attempt-wzgc590u/receipt.json` has SHA-256
`ec24a1d0d30133108696e1492fbd5760400550425a4d7712a3fc1efcd2cd9c09`.
It checks actual sender PID/UID, pending masked delivery, unmask and two
signal returns, per-thread nested-handler accounting, normal exit and cleanup.
It does not qualify general POSIX signals or blocked guest IO interruption.

The earlier standalone Kestrel receipt `kestrel-guest-execution/attempt-nfmi2iou`
passes all five native HTTP semantics and normal exit, but the later actual
two-instance matrix `kestrel-worker-instances/attempt-5czkvcnt` reaches 100M
instructions in both owners: A health passes, B produces EOF. Both release
resources and have no recorded guest signal/halt or managed exception. This
is a retained failure, not a four-mode service pass.

A separate read-only inventory of the complete `nfmi2iou` trace is preserved at
`kestrel-runtime-inventory/attempt-yl_biksy/receipt.json` (SHA-256
`505f62c115a76122d12c894dbd071dcdbb95013ce7d30c8838f6a3535634da64`).
It records 19,956 syscall observations across 50 syscall kinds, ten guest
threads, the successful asynchronous TCP/futex/memory paths, and every observed
refusal. It explicitly identifies the pre-override, pre-clock product and is
not a new execution or final-product compatibility claim.

Read-only disassembly of the unchanged ELF identifies the heartbeat timer's
low-resolution clock path: `minipal_lowres_ticks` requests clock 6 and reads
its output without testing the return code. The complete prior standalone
trace records 3,910 EINVAL clock-6 results on the inferred heartbeat thread.
`kestrel-clock-diagnosis/attempt-cdb6r_pz/receipt.json` pins the disassembly,
trace and source identities (SHA-256
`9ea3c3954b0010ad4afefc95be1862af68897479b7f052b328f2ea0aee04b22b`).
Both the private `CLOCK_MONOTONIC_COARSE` capability and managed clock support
were missing.

Commit `a4dd163` adds that capability and a same-origin monotonic reading
floored to milliseconds, with provider-aware resolution. Native and all four
managed boundary forms pass at `host-coarse-clock/attempt-ny75u0ux` (SHA-256
`814a144c582b76521d9b429def663464a66afc1a11c66ac436eeddd3400e3841`).
Fresh public generation `translation/attempt-kaddtzlg` passes with 108 newly
emitted objects (receipt SHA-256
`711c66f92de0d2bf3a4a3005c529f316ca29cabfc0a53a3353ea255969c0de11`).
The actual Kestrel retry is underway. Its
standalone gate requires complete traces and successful actual clock-6 calls;
the guest ELF, six variables, 128 MiB memory profile, 100M instruction budget
and 60-second deadline remain unchanged. The correction's effect on actual
Kestrel execution is not claimed before that run.
