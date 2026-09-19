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

These are native guest build/reference results only. The bounded raw-JIT
compatibility probe using explicit private dependency files and the same C#
owner now executes the guest but fails before readiness. Receipt
`dotnet-guest-execution/attempt-twmyfw31/receipt.json`, SHA-256
`7ac6b0e12d77ba9ca2ca3f3a8421f6bed0871c99a4eeabb59eaf87c7a1987a5d`,
records 18,791 completed instructions followed by `PanicDueToMmap` and a
contained host exit request 250. The execution thread joined, no HTTP case ran,
and guest stdout/stderr were empty. No translated NativeAOT guest pass is claimed.

The raw baseline is the exact P4 delivery, with its producer compiler identity;
this diagnostic does not qualify the newer compiler. A separate current C#
owner snapshot records the explicit interpreter opt-in. Follow-up receipt
`dotnet-guest-execution/attempt-046ir0z5/receipt.json`, SHA-256
`eee2ab47b268fa9fd83582660eb32dc28c2c71a2a3e46e6d3124dcfe50b9faa7`,
captures RAX=9 and mmap arguments `(0, 2170256, 1, 2050, 3, 0)` at
IP `0x110000025d2c`. These match the initial native libc mapping. Its file is
2,125,328 bytes: the legitimate image span covers 11 whole pages past EOF,
which the private memory adapter currently rejects rather than expose readable
zero pages. Captured host errno is EBADF after the panic's diagnostic output;
it is not proof of the original allocation errno or exact first rejected page.
Removing the EOF rejection alone would be incorrect. This is now a
concrete reason to evaluate an ordinary static musl build; runtime threads and
other contracts would still need implementation.

## Current profile boundaries

These are source findings pending the actual guest trace:

| Boundary | Current implementation | Consequence |
| --- | --- | --- |
| ELF interpreter | Pinned upstream `blink/loader.c` implements PT_INTERP loading through VfsOpen; `GuestExecution` now permits an explicit interpreter opt-in with static-only default | Actual dynamic loader execution reached the guest mmap syscall; the next failure is mapping semantics, not the old owner guard. |
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
