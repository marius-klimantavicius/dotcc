# P5 NativeAOT guest qualification

The required guest is a genuine C# HTTP service published to native machine
code and executed by translated Blink. Publishing the emulator host with AOT
does not satisfy this requirement. The existing C service remains a separate
qualified P4 fixture.

## First build and actual evidence

`tests/DotNetService/` and `scripts/build-dotnet-guest.py` now pass ordinary
`linux-x64` publishing with SDK 10.0.111/runtime 10.0.11, exact native health and
shutdown responses, normal exit and empty stderr. Receipt
`dotnet-guest/attempt-5130ydrk/receipt.json` has SHA-256
`0c5841150df27f83e0aa49d4225b84dde1cc041caff1f0de16ea14d1ce1bd2ac`.
Its real 1,553,192-byte NativeAOT guest ELF has SHA-256
`87985a99c01e395468de19e0fae5f8c907dbcf50ad08cf4fb07613aeda124be4`.

The ELF requires `/lib64/ld-linux-x86-64.so.2`, libc and libm, with PT_TLS
initialized/memory sizes 24/288 and alignment 8. The actual native trace
(`9a34ed7d637d37d5d2482a12909708cbdce410a5d38ff808225834055b369094`)
observes finalizer, sockets and signal-handler threads, futex/epoll/pipe2,
signals/TLS and proc/sysfs/cgroup probes. Optional probes and unfinished waits
must be distinguished from required successful operations.

These are native guest build/reference results only. A bounded raw-JIT
compatibility probe using explicit private dependency files and the same C#
owner is being prepared. No translated NativeAOT guest execution pass is claimed.

## Current profile boundaries

These are source findings pending the actual guest trace:

| Boundary | Current implementation | Consequence |
| --- | --- | --- |
| ELF interpreter | Pinned upstream `blink/loader.c` implements PT_INTERP loading through VfsOpen; `Managed.Emulation.Execution.GuestExecution` currently requires a static image | Dynamic glibc is not intrinsically excluded by Blink. It requires explicit private interpreter/library inputs and a reviewed owner extension. |
| Guest threads | `config/core-config.h` selects DISABLE_THREADS; syscall.c excludes clone/futex dispatch and `config/managed-host/pthread.h` rejects an unqualified pthread ABI | A threaded guest needs actual upstream guest-thread algorithms and managed host synchronization/lifetime support. Existing host pipe or synchronization helpers do not qualify guest thread startup. |
| TLS | P3 qualified a valid fixed static TLS fixture and explicit FS setup | This is not libc thread startup or general dynamic TLS evidence. |
| Execution ownership | C# owns the loop, stop and cleanup; upstream CPU/syscall algorithms remain translated | Profile extensions must preserve that architecture and cannot substitute a native emulator or success stubs. |

The .NET v10 source initializes the finalizer through
[FinalizerHelpers.cpp](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/Runtime/FinalizerHelpers.cpp)
and [PalUnix.cpp](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/Runtime/unix/PalUnix.cpp),
which starts a native thread. This source evidence predicts a requirement even
for a synchronous service; the actual published guest trace will determine its
observed startup/runtime surface. Changing libc does not itself supply missing
thread support.

## Optional bounded musl route

Musl is considered only if it meaningfully simplifies the actual ELF closure.
The read-only environment audit found working Docker/Podman and a cached Ubuntu
SDK image, but no cached Alpine AOT image, local musl-gcc or musl ILC pack.
One official candidate, identified from registry metadata, is:

```text
mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64
sha256:240a20b94625153877c8ec4ed0394d3396f243a5663fe74b8f601c31f1dac3fc
```

That image identifies SDK 10.0.401/runtime 10.0.12, so using it would require a
separately recorded guest build profile rather than silently reusing the local
SDK pin. The documented AOT prerequisites and runtime build targets support a
candidate publish with `linux-musl-x64`, `PublishAot=true`,
`StaticExecutable=true`, `PositionIndependentExecutable=false` and invariant
globalization. Inspect the resulting PT_INTERP/NEEDED entries instead of assuming
static output from those options. See the
[official prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)
and [Unix linker targets](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/BuildIntegration/Microsoft.NETCore.Native.Unix.targets).

No container pull/build has been performed by this audit. If the ordinary musl
publish is very difficult or impossible, stop and report for user direction.
Do not pursue a custom runtime/toolchain or prolonged build workarounds.
