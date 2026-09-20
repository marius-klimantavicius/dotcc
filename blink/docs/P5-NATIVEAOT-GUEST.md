# P5 NativeAOT guest qualification

The required guest is a genuine C# HTTP service published to native machine
code and executed by translated Blink. Publishing the emulator host with AOT
does not satisfy this requirement. The existing C service remains a separate
qualified P4 fixture.

## Active threaded qualification

The pinned static-musl guest now builds and passes native HTTP with the bounded
GC configuration in `tests/DotNetService/GC-PROFILE.md`. Actual translated startup
passes membarrier and stops at clone ENOSYS in the stable single-thread profile.
The separate `generated/ThreadedBlink` prototype now builds from all108 producers
with real shared host memory/synchronization and an authored C# lifecycle owner.
Its layout and normal clone/TLS/futex execution gates pass all four managed
forms. Actual threaded .NET startup now passes clone, GC reservation and musl
stack discovery. The qualified real empty-epoll boundary now permits actual
READY and HTTP accept; the next observed failure is SO_SNDTIMEO ENOPROTOOPT on
the accepted socket. No response case passes yet. Registrations remain explicitly
unsupported; stable delivery remains separate. P5 completion is not claimed.
Current receipts and ownership are recorded at the end of
[PROGRESS.md](PROGRESS.md).

The following build/trace sections preserve the earlier glibc and static-musl
findings with their exact producer identities.

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

The single ordinary attempt now passes: image pull 64.65 seconds, publish
17.39 seconds, with no retries, additional installations or build workarounds.
Receipt `dotnet-guest-musl/attempt-8za50rji/receipt.json` SHA-256
`8876eaf0cda1c0cc9ec81e163a9293745caa436215dec90790ff6f57acce8505`
records the actual static ET_EXEC (no interpreter or dynamic dependencies),
exact native HTTP replies and normal shutdown. The 1,618,312-byte guest ELF
at that attempt's `publish/DotNetService` has SHA-256
`b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3`.
The named build container was removed. Source, package and artifact hashes were
independently checked. See [build commands and evidence](../tests/DotNetService/MUSL.md).

The musl native trace uses clone and WAIT/WAKE_PRIVATE, simplifying two syscall
variants compared with glibc. It still creates genuine runtime threads, uses
epoll/pipe2 and reserves a large GC virtual range. Translated execution of this
static candidate is the next gate; the native pass alone does not satisfy it.
The user's stop condition did not trigger: compilation was straightforward.

## First static execution and required barrier

The static candidate executes 139,230 instructions before guest exit -1, with
no readiness or HTTP case. The same result is reproduced with 21 bounded
syscall observations in `dotnet-guest-execution/attempt-w6n64zmg/receipt.json`
(SHA-256 `ac264c5e2ee12364fba52a8cb099eed4578744a945d8363d1e457b754783d838`).
QUERY membarrier returns ENOSYS, fallback mlock returns ENOSYS, and the guest
cleans up before exit_group(-1). The owner joins and releases its memory.

The actual runtime package pins source commit
`95017c711e6afc1085133d440e42b4bd78155701`. Its
[GC initialization](https://github.com/dotnet/dotnet/blob/95017c711e6afc1085133d440e42b4bd78155701/src/runtime/src/coreclr/gc/unix/gcenv.unix.cpp)
first checks PRIVATE_EXPEDITED bit 8 and registration command 16; success avoids
the memory-locking fallback. The pending boundary therefore implements those
commands with actual
[BCL process-wide memory barriers](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.memorybarrierprocesswide?view=net-10.0),
owner-scoped registration, and no host memory-pinning claim. Initial qualification
is guarded to the existing one-guest-thread/no-fork profile. This API supplies a
real process-wide fence; future guest threading still needs shared owner and
lifecycle qualification. No boundary or translated guest pass is claimed until
the corresponding executions finish.


## Pending worker delivery integration

The existing `Managed.Emulation` controller is a process/protocol implementation;
its earlier protocol fixtures do not establish real guest execution. Its current
1 MiB total-image admission limit rejects the selected 1,618,312-byte ELF. A
bounded 2 MiB image profile would admit this guest; its 2,157,752-byte base64 body
plus the selected configuration fits the existing 3 MiB capped frame. Preserve
serialization and frame validation before launching a worker.

After actual guest compatibility passes, the worker must consume the threaded
C# owner, reserve raw standard streams for protocol frames, pass the four guest
GC/environment entries separately from host settings, and emit readiness only
after actual guest output and loopback publication. One execution per fresh
worker process is required. The current owner has a fixed 64 MiB backing limit
and a 16-total-worker bound; reject unsupported options until genuinely
parameterized. Do not dispose borrowed IO/stop/storage until all execution
workers join and the owner is quiescent. The sample still uses the earlier C
fixture and single-thread owner; its readiness and exact response checks are
reusable, but its current success is not a .NET guest pass.
