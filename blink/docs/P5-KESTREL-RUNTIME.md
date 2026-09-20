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
in the native receipt. Managed regression and actual corrected Kestrel execution
remain pending; native success does not close P5.

The old sysinfo fallback's zero available-memory result is a separate known
modeling discrepancy. It is not the cause established by the SIMD observation.
No guest quota increase or native-emulator fallback is substituted for the fix.
