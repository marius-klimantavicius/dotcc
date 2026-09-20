# P5 actual NativeAOT syscall audit

This is a source/trace audit, not translated execution qualification. The genuine
.NET service completed `/health` and `/stop` on native Linux. Its startup needs
more than the selected P4 single-threaded runtime. No profile, compiler, runtime,
container or guest changes were made for this audit.

## Current interpretation

This initial glibc audit is historical evidence, not the current threaded profile
status. Static-musl startup later reached clone after the qualified membarrier
boundary. A separate threaded prototype now builds with an authored C# owner;
actual layout and normal clone/TLS/futex execution now pass all four forms. The real .NET
service has not passed translated startup. Follow [PROGRESS.md](PROGRESS.md) for
the latest exact attempts; do not infer current support from this older table.

## Evidence and pins

The current native witness is `artifacts/dotnet-guest/attempt-5130ydrk/`:

| Input | SHA-256 |
| --- | --- |
| `receipt.json` | `0c5841150df27f83e0aa49d4225b84dde1cc041caff1f0de16ea14d1ce1bd2ac` |
| `native.strace` | `9a34ed7d637d37d5d2482a12909708cbdce410a5d38ff808225834055b369094` |
| `publish/DotNetService` | `87985a99c01e395468de19e0fae5f8c907dbcf50ad08cf4fb07613aeda124be4` |

The trace has 382 physical lines and 48 distinct syscall names. Resumed lines
must be joined to their thread's unfinished call. `futex`, `epoll_wait` and the
signal thread's pipe `read` end with `= ?` during process exit: these are observed
blocking operations, not successful returned calls. The first native witness
`attempt-ly2kzxeo`, trace SHA
`630cfdbb36d2105ef0ec1a8fb9e4d44ac7fe313f09f23e3fbe94fcb9be71f5dd`,
is retained; the later publish records the inherited MSBuild input explicitly.

The comparison profile is delivery `translation/attempt-4yjaed1_`, frozen
profile `generated/core-profile/attempt-avmik3zz`, inputs SHA
`8672d963474bbf2aad87367b3ff9c119df3d9eee2f26b257ce5f2effc2f20c4e`,
108-object assembly receipt SHA
`247b7eace45a36b095fea4df03bfaf282631477de09fbaffa239398d8462a25e`.
Upstream Blink revision is `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`, archive
SHA `e0bad68ba2927a1ca1d53537afee1644e11a1e683d26676a36e5b9b17a63d6af`.

| Audited source, relative to repository unless stated | SHA-256 |
| --- | --- |
| pinned upstream `blink/syscall.c` | `4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3` |
| pinned upstream `blink/loader.c` | `8175b61a1de47f9decf91e71f682e73f5260b66ca1eba6686c2d4d302bcd751c` |
| `blink/config/core-config.h` | `e4ec3ae07e706a1d4d688e1666958f2cc543ff04c9009ab3da06b4045ff893df` |
| `blink/config/managed-host/pthread.h` | `513f36814486f7358cef61562186b4afbc9faeb936e559914f1a5acee53f281b` |
| P4 baseline `blink/src/Managed.Emulation.Execution/GuestExecution.cs` at `7653b85` | `aed38e7412dcb049bd5de6ed67fa902857385133762fc2e747726ce528252cb2` |
| `blink/src/Host/HostMemory.c` | `f2ed022212a0facbcad8aedb2920358e27dc0ab18a9613b43943634aa744c0b4` |
| `DotCC.Libc/PthreadLib.cs` | `6420ae8b206df4ba0d75c9a5aea3b05fc94b2e80d38acf3deac5eea76880e940` |

## Observed surface and selected support

Source paths below use `ref/blink-<revision>/blink/` for upstream files and
`src/Host/` for authored bridges. Availability of a helper does not qualify its
use by this guest. Existing P3/P4 evidence covers the selected contracts in
`VALIDATION.md`, not this dynamic, threaded application.

| Observed calls | Actual trace role | Selected implementation and remaining issue |
| --- | --- | --- |
| `execve` | Initial Linux launch of this executable | Launcher action, not evidence that this service needs an in-guest exec. `GuestExecution.Run` calls `LoadProgram`. |
| `openat`, `read`, `pread64`, `fstat`, `close`, `access` | Loader reads libc/libm; runtime reads system configuration and entropy | `syscall.c` Vfs/descriptor paths reach `HostIoBridge` and private `InstanceIo`. Ordinary files are qualified, but exact dependency files and virtual runtime files are not supplied by the current service image. Missing preload is tolerated in this trace. |
| `mmap`, `mprotect`, `munmap`, `brk`, `madvise` | Shared libraries, stacks and GC reservation/commit | Upstream `SysMmap`/`ReserveVirtual`/`ProtectVirtual` plus `HostMemory.c` exist. Current owner seeds **64 MiB RLIMIT_AS** through `GuestResources`; `SysMmap` checks guest virtual pages against that limit. The trace reserves **268,316,028,928 bytes PROT_NONE**, plus other mappings. This is a concrete quota mismatch, even though reservation is not committed physical RAM. Pinned `SysMadvise` returns success without action; traced DONTDUMP/DODUMP hints do not prove host dump policy. |
| `arch_prctl`, `set_tid_address` | Initial FS base and clear-TID pointer | Selected upstream paths and valid static TLS fixture exist. No qualification of the glibc dynamic TLS block or cloned-thread TLS. |
| `clone3` (three successes) | Finalizer, sockets and signal-handler threads | No pinned `clone3` dispatcher. `clone`/`SysSpawn` are excluded by `DISABLE_THREADS` and absent `HAVE_FORK`. A possible libc clone fallback is not exercised by this successful native trace. |
| `set_robust_list` (four successes), `rseq` (four successes) | Main/child thread registration | Robust-list dispatcher is in the excluded thread/fork block; upstream cleanup algorithm exists. No pinned rseq dispatcher. Whether rseq failure is tolerated must be established separately, not assumed from success here. |
| `futex` | Finalizer waits with WAIT_BITSET_PRIVATE\|CLOCK_REALTIME and MATCH_ANY, no timeout | Futex dispatch is excluded. Even enabling it is insufficient: pinned `SysFutex` handles WAIT/WAKE but explicitly rejects WAIT_BITSET. |
| `epoll_create1`, `epoll_wait` | Sockets runtime creates epoll descriptor and background thread waits | Pinned algorithms are guarded by `HAVE_EPOLL_PWAIT1`, absent in selected config. No private epoll ownership bridge. No `epoll_ctl` call is observed; do not invent one as an observed requirement. |
| `pipe2`, `read` | Signal-handler pipe with CLOEXEC; thread blocks on read | HostPipes bridge/private pipe implementation exists, but guest pipe2 dispatcher is excluded with thread/fork support. Existing inherited-pipe cancellation tests do not qualify guest pipe creation. |
| `rt_sigaction`, `rt_sigprocmask` | Runtime installs SIGSEGV/FPE/PIPE and other handlers, controls masks | Selected guest signal state and host registration exist. Registration/masks were qualified separately; cross-thread delivery and group teardown for NativeAOT are not. No signal fault injection is needed for this audit. |
| `membarrier` | Query and REGISTER_PRIVATE_EXPEDITED return success | No pinned dispatcher. Exact runtime fallback is unproved; do not return fake successful registration without a corresponding memory-ordering contract. |
| `prctl` | Three PR_SET_NAME calls label runtime threads | Pinned `SysPrctl` does not implement PR_SET_NAME. Likely descriptive, but this trace does not demonstrate tolerated failure. |
| `getpid`, `gettid`, `sched_yield`, `sched_getaffinity` | Identity/yield/capacity queries | Upstream selected paths exist; `SysSchedGetaffinity` has a GetCpuCount fallback. Thread identity and scheduler behavior must follow the eventual private thread owner, not real unrelated host thread IDs. |
| `prlimit64`, `sysinfo`, `statfs`, `get_mempolicy`, `getdents64` | Limits, memory/topology/cgroup discovery | Upstream limit and directory paths exist. `sysinfo_linux` falls back to a fixed 1 GiB memory value without HAVE_SYSINFO/SYSCTL; this is not an accurate private quota query. `statfs` has a private capacity path, not Linux cgroup discovery semantics. No get_mempolicy dispatcher. Successful discovery probes are not automatically essential nor proven dispensable. |
| `getrandom` | Eight bytes with GRND_NONBLOCK | Selected HostEnvironment/BCL entropy path exists. Runtime also reads `/dev/urandom`; syscall getrandom support does not supply that descriptor automatically. |
| `socket`, `bind`, `listen`, `getsockname`, `accept4`, `recvmsg`, `sendto`, `shutdown` | Actual IPv4 HTTP listener and two requests | Selected HostNetwork/HostMessages/SocketQueries support bounded IPv4 TCP. Two recvmsg calls have one iovec and no control data; two sendto calls have null destination. These shapes fit existing selected message contracts, but complete NativeAOT execution is unqualified. |
| `socket` probes | IPv6 datagram, IPv4 datagram, Unix datagram each opened and immediately closed | Outside selected IPv4/TCP host policy. They appear to be capability probes; absence of traffic alone does not prove failure is accepted. Ordinary unsupported-family/type results remain honest; do not add success placeholders. |
| `setsockopt` | REUSEADDR; accepted sockets get SO_SNDTIMEO_OLD and SO_RCVTIMEO_OLD, each five seconds | REUSEADDR supported. Current `HostNetworkBridge.blink_host_setsockopt` selects reuse/buffer sizes/TCP_NODELAY, not timeval send/receive timeouts. This is an observed concrete option gap; cancellation tokens alone do not implement socket timeouts. |
| `fcntl`, `ioctl`, `write` | F_DUPFD_CLOEXEC stdout; console TCGETS; readiness/stop text | Descriptor duplication/streams exist. All three TCGETS calls return ENOTTY and service continues. P4 TIOCGWINSZ evidence is a different request; preserve normal ENOTTY, do not claim a terminal. |
| `exit_group` | Native main exits zero while three helpers are blocked | Current single-machine C# cleanup is qualified. Threaded exit must wake/join all guest execution threads before freeing their shared system, mappings, callbacks or IO. |

## Dependency files and normal failures

ELF metadata names `/lib64/ld-linux-x86-64.so.2`, `libm.so.6`, `libc.so.6` and
the loader SONAME. Native trace opens `/etc/ld.so.cache`,
`/lib/x86_64-linux-gnu/libm.so.6` and `/lib/x86_64-linux-gnu/libc.so.6`.
The initial interpreter is loaded by Linux, so absence of its openat in the
process trace does not remove that dependency. Pin file bytes and resolved
symlink targets before mounting a private closure. `loader.c` handles PT_INTERP;
the P4 C# owner's `system->elf.interpreter == null` guard was the immediate owner
restriction. P5 adds an explicit interpreter opt-in; the default remains static.
Musl is optional, not a prerequisite inferred from that guard.

Successful runtime reads include `/dev/urandom`, `/proc/self/maps`,
`/proc/meminfo`, `/proc/self/mountinfo`, `/proc/self/cgroup`,
`/sys/devices/system/cpu/online`, cache index0..3 `size`/`level` files,
`/sys/devices/system/node` directory enumeration, and several ancestor cgroup
`memory.max` files. These are configuration/entropy dependencies in this run;
copying host `/proc/self/maps` into a private guest would describe the wrong
address space. A virtual namespace must describe its actual private resources,
or use a documented runtime fallback. Do not silently mount arbitrary host
proc/sys trees.

Observed ordinary failures are exactly missing `/etc/ld.so.preload`, the traced
application cgroup `cpu.max`, `/sys/fs/cgroup/memory.max`, cache index4 `size`
(ENOENT), and three console TCGETS requests (ENOTTY). HTTP still succeeds.
The exact machine-specific cgroup paths remain in the hashed trace; they are
not a portable fixture contract. Successful feature queries such as rseq,
membarrier or datagram socket creation are a separate category from these
demonstrated tolerated failures.

## Minimum upstream thread path and C# ownership constraints

Pinned `syscall.c` contains `SysClone` → `SysSpawn`: validate shared VM/files/
sighand/thread flags, `NewMachine(system, parent)`, set child FS from CLONE_SETTLS,
set child return register/stack, and publish parent/child TIDs. The native
`OnSpawn` callback arms `sigsetjmp`, restores the mask, then calls **C `Blink(m)`**.
That final loop cannot become the production owner under the agreed C# API.
Any adaptation must retain upstream state transitions while handing each
Machine to a C# execution loop with its own jump-buffer/exception boundary.

Native thread.h aliases real pthread ABI types and enables LOCK operations.
The selected no-thread branch replaces them with small placeholders/no-ops;
changing a macro therefore changes structure layouts as well as dispatch.
`config/managed-host/pthread.h` deliberately errors instead of claiming this ABI
is qualified. Generic `DotCC.Libc/PthreadLib.cs` has actual managed Thread and
mutex/condition implementations, but is not proof of Blink's type layouts,
condition-clock semantics, signal masks, or process-shared synchronization.
It invokes its C callback directly and does not establish Blink owner bindings.

`HostIoBridge`, `HostSleepBridge`, and `HostExecutionStopBridge` use ThreadStatic
bindings; other owner bindings and `g_machine` also need an explicit child-thread
setup/teardown plan. `HostMemory.c` stores its mapping registry/budget/file readers
in a C `_Thread_local MemoryOwner`. Independently beginning a child registry
would not make the shared System's pages belong to it. A reviewed shared memory
owner/lease design is required before concurrent guest machines can allocate or
free shared mappings. Do not copy raw ThreadStatic fields or dispose the parent
pool while a child is live.

Futex WAIT/WAKE uses `g_bus` queues, mutexes and timed condition waits, guest
address loads and CheckInterrupt; WAIT_BITSET requires additional semantics.
`ClearChildTid`, `UnlockRobustFutex`/robust-list cleanup, child `SysExit`,
`FreeMachine`, and group termination must be coordinated with C# thread joins.
Thread-local cancellation must bind the same stop owner, wake every pending
operation, and keep all mappings and callback roots alive until all loops exit.
Existing single-run/process ownership is not a multithread lifecycle proof.

## Bounded implementation order for review

1. Pin the actual dynamic ELF dependency closure and review the owner interpreter
   allowance. Decide explicitly how runtime memory discovery/GC settings relate
   to private virtual and committed quotas. Do not attempt the whole application
   with fabricated successful probes.
2. Design one minimal shared-System guest thread path preserving C# loops,
   coherent memory/descriptor ownership, TLS, TID publication and joined cleanup.
   Qualify the actual pthread ABI and synchronization before enabling HAVE_THREADS.
   Choose reviewed clone3 decoding or an independently observed libc clone
   fallback; the current native witness establishes only clone3.
3. Implement/qualify the observed blocking subset: WAIT_BITSET futex, robust/ctid
   lifecycle, guest pipe creation, and bounded epoll creation/wait with real stop
   wakeup. Add exact timeval socket timeout semantics for the observed service.
4. Probe the actual .NET guest again, retaining the first unsupported operation.
   Resolve only concrete remaining discovery/entropy/signal requirements; keep
   optional probes distinct from required successful work. A static musl attempt
   is justified only if it simplifies the loader closure; it does not supply
   missing threads. If that ordinary build becomes difficult, stop and report.

No source availability, historical C service result, native .NET run, or this
audit marks P5 translated .NET execution complete.
