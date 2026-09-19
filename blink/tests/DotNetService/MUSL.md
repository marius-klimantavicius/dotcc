# Bounded static-musl NativeAOT attempt

The single standard publish and native HTTP oracle passed in
`artifacts/dotnet-guest-musl/attempt-8za50rji/`. This does not qualify execution
inside translated Blink or provide missing guest threads. The C# service and
project bytes were unchanged.

| Observed artifact | SHA-256 |
| --- | --- |
| `receipt.json` | `8876eaf0cda1c0cc9ec81e163a9293745caa436215dec90790ff6f57acce8505` |
| `publish/DotNetService` | `b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3` |
| `native.strace` | `a7f552522363dff3c2909d6ed8b5173e90e30e4ac8e07d4904400a78091585ef` |
| Frozen runner | `0f89f12c6eb2f990e4841cc60d329f6db85df78f33412bc2c665605fa0d74833` |

The image pull took 64.65 seconds and container publish took 17.39 seconds.
There were no retries, additional installations or build workarounds. The
1,618,312-byte binary is x86-64 ET_EXEC with no PT_INTERP or NEEDED entries;
PT_TLS has 24 initialized bytes and 288 total bytes. SDK 10.0.401 and resolved
NativeAOT compiler/runtime packages 10.0.12 were recorded. Both `/health` and
`/stop` responses matched exactly, followed by `STOPPED`, exit zero and empty
stderr. Named-container removal succeeded. A separate final inspection
rechecked 83 recorded source/input/log/package/tool/binary hashes.

## Actual musl thread and syscall inventory

The hashed native trace above contains 348 physical lines and 44 syscall names.
The image records `musl-1.2.5-r23`. Three real helper threads are named
`.NET Finalizer`, `.NET Sockets` and `.NET SigHandler`; source availability alone
does not qualify those threads in translated Blink.

All three `clone` calls use flags **0x007d0f00**: VM, FS, FILES, SIGHAND, THREAD,
SYSVSEM, SETTLS, PARENT_SETTID, CHILD_CLEARTID and DETACHED (strace prints the last
as `0x400000`). The low signal byte is zero; CHILD_SETTID is absent. The pinned
`SysSpawn` mandatory/supported masks accept these flags after ignoring DETACHED.
There is no clone3 call in this musl witness.

| Child / name | Supplied child stack | Supplied TLS base | Child clear-TID address |
| --- | --- | --- | --- |
| 1874207 / Finalizer | `0x7e53ec7659d8` | `0x7e53ec765b38` | `0x599090` |
| 1874208 / Sockets | `0x7e14e248f9d8` | `0x7e14e248fb38` | `0x599090` |
| 1874209 / SigHandler | `0x7e14e23049d8` | `0x7e14e2304b38` | `0x599090` |

Each stack mapping reserves 1,585,152 bytes PROT_NONE then makes 1,576,960 bytes
RW starting 8,192 bytes above its base. TLS is 352 bytes above the supplied
clone stack pointer. These are observations of this run, not a new layout ABI.
The parent-TID output is decoded by strace as `[child tid]`; its pointer address
is not exposed by this capture and must not be inferred from TLS. The common
clear-TID pointer is also the main thread's initial `set_tid_address` argument;
do not assume one distinct clear-TID word per Machine. Main FS is set separately
to `0x7e53ec767138`. No `rseq` or `set_robust_list` call is observed.

| Observed operation | Outcome | Pinned upstream / selected-profile consequence |
| --- | --- | --- |
| Main FUTEX_WAIT_PRIVATE at `0x5981e0`, expected `2147483650` (`0x80000002`), timeout NULL | Returns 0 after finalizer wake | `SysFutex` strips PRIVATE and accepts WAIT; unsigned expected-value semantics matter. Dispatch remains excluded by DISABLE_THREADS. |
| Finalizer FUTEX_WAKE_PRIVATE at the same address, count1 | Returns1 | Existing wake queue algorithm is relevant, but requires actual mutex/condition ownership and shared guest memory. |
| Finalizer FUTEX_WAIT_PRIVATE at `0x7e53ec765904`, expected2, timeout NULL | Still blocked at exit_group (`= ?`) | No timed wait, WAIT_BITSET, CLOCK_REALTIME flag, or normal wake of this second wait is observed. Do not import the glibc WAIT_BITSET requirement into this finite musl subset. |
| epoll_create1(CLOEXEC), sockets-thread epoll_pwait | Creation succeeds; wait remains blocked at exit | No epoll_ctl observed. The unfinished trace does not expose the wait's remaining arguments. Selected HAVE_EPOLL_PWAIT1 is absent. |
| pipe2(CLOEXEC), signal-handler read(fd7) | Creation succeeds; read remains blocked at exit | Host private pipes exist; guest pipe2 dispatcher is currently excluded with thread/fork support. |
| PR_SET_NAME, membarrier query/register, get_mempolicy | Succeed natively | Unsupported pinned dispatch/operation variants remain; this trace does not establish tolerated failure for them. |
| Main mremap(old page,4096,8192,flags0) | 30 ENOMEM results then one EFAULT | This is libc stack discovery, not a request that should always succeed. Pinned `SysMremap` always returns ENOMEM, which does not supply the observed terminating condition. |
| Large GC PROT_NONE reservation | 268,316,028,928 bytes succeeds | Current64MiB guest RLIMIT_AS cannot accept it. Sparse guest mappings still require page-table storage; merely increasing AS without a memory/GC policy is insufficient. |
| IPv4 TCP / HTTP | Actual normal request/response work succeeds | Existing selected network shapes are relevant; five-second SO_SNDTIMEO_OLD/RCVTIMEO_OLD options still require actual semantics. |

The official [musl v1.2.5 pthread_getattr_np source](https://git.musl-libc.org/cgit/musl/tree/src/thread/pthread_getattr_np.c?h=v1.2.5)
walks downward while its mremap probe fails with ENOMEM, ending on another result.
Thus the native EFAULT terminator cannot be collapsed into a generic harmless
failure. This explains the observed sequence; the Alpine revision is recorded
separately and no Alpine source patch equivalence is asserted here.

Three missing configuration files return ENOENT: the application cgroup
`cpu.max`, root cgroup `memory.max`, and cache index4 `size`. One TCGETS and two
TIOCGWINSZ requests return ENOTTY. These failures are demonstrably tolerated by
this native run. Successful IPv6/IPv4/Unix datagram open-and-close probes are a
different category: their failure fallback is not demonstrated. The executable
no longer opens dynamic libc/loader files, but still reads runtime discovery
files and `/dev/urandom`; static linking does not eliminate those contracts.

## Proposed smallest separate threaded profile

This is implementation ordering for review, not implemented or qualified work.
It uses pinned Blink `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`; source identities
are in `docs/P5-SYSCALLS.md`. Existing single-thread profiles remain unchanged.

1. Qualify a private pthread scalar-handle ABI and real mutex/condition behavior
   using existing DotCC pthread facilities where applicable. Removing
   DISABLE_THREADS changes Machine/System/Bus layout and stops thread.h erasing
   `_Thread_local`; create a fresh full profile, not a syscall-only overlay.
   Do not advertise process-shared pthread support.
2. Retain `SysClone`/`SysSpawn` state preparation for the observed flags, FS/stack
   and TID semantics. Replace only the native `OnSpawn`→C `Blink(m)` execution
   handoff with an owning C# callback/loop. Bind Machine/jump-buffer/mask/wait
   state per thread and common IO/stop owners explicitly.
3. Introduce reviewed shared MemoryOwner registry/locking/lifetime leases before
   children access the same System. Current TLS mapping owners cannot independently
   own shared pages. Replace native KillOtherThreads SIGSYS/SIGKILL escalation
   with cooperative wakeup and joins; coordinate SysExit/ClearChildTid/FreeMachine
   so cleanup has exactly one owner and the last System is not freed early.
4. Qualify the observed WAIT/WAKE_PRIVATE handshake, blocked helper wakeup on
   group exit, pipe creation/read, epoll creation/wait, and socket timeouts.
   Robust cleanup remains part of honest Machine destruction, but no robust-list
   registration or WAIT_BITSET implementation is required solely by this trace.
5. Review ordinary NativeAOT GC configuration and private virtual/committed
   quotas, then run the actual static guest to identify the next exact unsupported
   operation. Preserve the mremap stack-discovery distinction and honest capability
   fallbacks. Do not add fabricated success returns or rewrite service behavior.

After the coordinator releases the serial build slot:

```sh
python3 blink/scripts/build-dotnet-guest-musl.py
```

The default engine is rootless Podman; `--engine docker` selects Docker explicitly.
The fixed image is
`mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64` at digest
`sha256:240a20b94625153877c8ec4ed0394d3396f243a5663fe74b8f601c31f1dac3fc`.
Registry metadata identified SDK 10.0.401 and runtime 10.0.12. The runner checks
the actual image platform/digest/environment and SDK version again. The image
already contains clang, build-base and zlib-dev; the runner installs nothing.

`Program.cs` and `DotNetService.csproj` are copied byte-for-byte into a private
attempt. Repository build props, central package props and NuGet configuration
are copied and hashed. The original `global.json` is preserved; a separately
recorded SDK pin selects 10.0.401. Command-line overrides select runtime 10.0.12,
`linux-musl-x64`, `PublishAot=true`, `StaticExecutable=true`,
`PositionIndependentExecutable=false` and invariant globalization. These are
build-profile changes, not C# source changes. NuGet packages and CLI home live
inside the attempt; no host SDK/compiler outputs or emitted Blink objects are
reused.

The receipt records the container image/instance, actual tool hashes/versions,
Alpine package versions, resolved NuGet packages and package manifests, source
and binary hashes, and closed command logs. `readelf` must show an x86-64 ET_EXEC
with no PT_INTERP and no NEEDED dependencies; flags alone do not establish a
static ELF. The published binary then runs on native Linux under strace with
the same exact `/health` and `/stop` HTTP oracle as the ordinary glibc fixture.
Native execution is outside the container so a dependency on its musl loader
cannot be hidden by the container filesystem. No translated guest execution is
performed by this runner.

Artifacts go under `blink/artifacts/dotnet-guest-musl/attempt-*/`; the runner
prints the attempt and final receipt path. The image pull has a ten-minute
limit and the single publish a fifteen-minute limit. A failed build, missing
ordinary prerequisite, nonstatic ELF, or failed native oracle is retained and
reported. **Stop and ask the coordinator/root if this ordinary musl build is
difficult or impossible.** There are no alternate toolchains, custom runtime
builds, package installation steps, automatic retries or service rewrites.

On timeout/interruption the runner terminates its command process group and
removes its named container with force; it records cleanup failures rather
than claiming success. This is ordinary job cleanup, not a guarantee for
unkillable kernel tasks. No injected timeout/failure tests are part of this work.

Primary references: [NativeAOT prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot),
[official Alpine AOT image](https://github.com/dotnet/dotnet-docker/tree/main/src/sdk/10.0/alpine3.23-aot/amd64),
and [NativeAOT static linker settings](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/BuildIntegration/Microsoft.NETCore.Native.Unix.targets).
