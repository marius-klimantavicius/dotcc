# P5: observed .NET NativeAOT runtime requirements

This is the preserved **raw-socket baseline** inventory. The user subsequently
selected ASP.NET Core/Kestrel as the required P5 guest. Its native producer and
configuration are documented in `tests/KestrelService/README.md`; translated
startup evidence and the SIMD mask correction are in
`tests/KestrelGuestExecution/README.md`. The original results below are not
Kestrel passes. Later registered asynchronous socket qualification is recorded
in `tests/HostAsyncSockets/README.md`; the empty-epoll limits below describe the
older baseline profile only.

This inventory covers one genuine C# synchronous HTTP service, compiled to a
static Linux x64 musl NativeAOT ELF, loaded and executed by the translated Blink
interpreter. It does not substitute a C service or native emulator. The guest
is NativeAOT in every case; the **emulator host** separately runs as raw JIT,
raw NativeAOT, optimized JIT and optimized NativeAOT.

All four modes passed the same normal workload: startup, publication, exact
`/health` response, exact `/stop` response, guest group exit 0 and complete worker
cleanup. This inventories the exercised runtime surface, not general .NET/Linux
compatibility. No new executions were performed to write this document.

## Exact evidence

Paths below are relative to `blink/`. Receipts retain complete producer and
source identities; old failures remain unchanged.

| Evidence | Receipt or artifact | SHA256 |
| --- | --- | --- |
| Static .NET guest producer | `artifacts/dotnet-guest-musl/attempt-8za50rji/receipt.json` | `8876eaf0cda1c0cc9ec81e163a9293745caa436215dec90790ff6f57acce8505` |
| Actual guest ELF | Producer's `publish/DotNetService`, 1,618,312 bytes | `b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3` |
| Native controlled GC workload | `artifacts/dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json` | `f145da71d5032421fdd40bd368d248cad4918065b0b07ee545e32772fab7bb55` |
| Actual native syscall trace | Same native attempt, `native.strace` | `1e687dddd89a8565e178d4ad7415ec825d001dd7258120be7aac518f87c050ac` |
| Threaded delivery | `artifacts/threaded-delivery/attempt-6zot2nmd/receipt.json` | `5367a49b019f896f36f023e8503411ab92cac2e89fe1cb151d5c973926f67985` |
| Complete 108-object assembly | `artifacts/core/objects/65ab05cfe4e470c02ca03cb81a979647d58e66f1717e60f1686c87bfad00f53e/receipt.json` | `fac989b4ef13eb51cfe663d4656ffc7d3e42d4aea4ddfed1a5bbde27e1248694` |
| Actual four-mode guest execution | `artifacts/dotnet-threaded-guest-execution/attempt-febf3tyc/receipt.json` | `087d1f9af88019af1762135d35d11f4c4f9d0dca4ad86f25a952267df742144d` |

The guest producer records SDK **10.0.401**, runtime **10.0.12**, the exact musl
publish command and packages. Its ELF has no INTERP or NEEDED entries and has a
PT_TLS segment with file size `0x18`, memory size `0x120`, alignment 8. These are
properties of the pinned guest, not the host runtime's version or image.

Only that ELF is mounted, at `/bin/dotnet-service`, with argv
`["dotnet-service", "8080"]`. The exact guest environment is:

```
LANG=C
DOTNET_GCHeapHardLimit=1000000
DOTNET_GCRegionRange=2000000
DOTNET_GCRegionSize=100000
```

These are the literal strings from the successful native witness. They are
passed to the guest, not installed as emulator-host tuning. The host owner is
bounded to 64 MiB, an aggregate 20,000,000 guest instructions, a 30-second stop
owner, and at most 16 workers. They are the selected workload's limits, not
universal NativeAOT requirements.

## Observed successful execution surface

“Required” here means used by this selected path or demonstrated as a blocking
prerequisite by its preserved prior failure. It is not a claim that every .NET
program necessarily needs each operation. Counts describe the four-mode traces;
ordinary scheduling changes are retained.

| Surface | Actual observation and implementation boundary |
| --- | --- |
| Static image and guest TLS | Valid static ELF load, `arch_prctl(ARCH_SET_FS, ...)` succeeds once, `set_tid_address` succeeds once. ELF PT_TLS storage and guest FS addresses remain guest state, not CLR thread-local pointers. |
| Real guest threads | Three successful `clone` calls with flags `0x7d0f00`; main plus three distinct guest TIDs. Upstream clone register/TLS/TID algorithms remain, while a separate authored C# owner starts one bound worker per Machine. There is no C execution loop or native pthread launch fallback. |
| Shared memory | Shared owner storage spans the workers, with actual mutex/condition synchronization and per-worker bindings. Twenty observed mmap calls, eighteen mprotect calls and three munmap calls succeed; two brk calls and sixteen madvise calls return nonnegative values. This is exercised behavior, not all mapping/advice semantics. |
| Reserved ranges and protection | Both native and managed traces include a successful `PROT_NONE` request of `0x2001000` (33,558,528) bytes. This is an address-range request, not a measurement of committed bytes or a proof of allocation attribution. Guest PTEs and tracked software protections govern interpreted access; raw host backing remains ordinary RW storage. No hardware-executable guest mapping is used. |
| Runtime stack discovery | Managed main makes 2048 normal `mremap(old,4096,8192,0)` probes: 2047 ENOMEM results then EFAULT on an absent page. Native makes 32 probes: 31 ENOMEM then EFAULT. The staged validation distinguishes the absent range without adding successful resize/relocation. Different stack ranges explain why counts are not compared for equality. |
| Memory barrier capability | Managed `membarrier(QUERY,0,0)` returns 24 and registration command 16 returns 0. Native query returns its broader host mask `0x3ff`. The implemented private expedited fence uses `Interlocked.MemoryBarrierProcessWide`, but **command 8 is not invoked in these four service traces**; its separate focused qualification must not be credited to this workload. |
| Futex wait and shutdown | One child performs FUTEX_WAIT_PRIVATE (operation 128, expected value 2, null timeout) and returns EINTR during cooperative group shutdown. The native wait is unfinished until process exit. This service run does not establish arbitrary futex operations or contention schedules. |
| Empty epoll | `epoll_create1(EPOLL_CLOEXEC)` succeeds. The managed sockets worker waits on fd 3 with maxevents 1024, timeout -1, null mask, sigset size 8, then returns EINTR on group stop. There are **zero epoll_ctl calls and zero delivered epoll events**. The owned empty descriptor/wait is real; registrations remain explicitly unsupported. |
| IPv4 synchronous HTTP | Successful TCP socket, bind, listen, getsockname, two accept4 calls, two recvmsg calls, two sendto guest calls, two shutdown calls and normal closes. The guest sendto route reaches upstream VfsSendmsg and the existing message bridge. No alternate host HTTP implementation supplies the responses. |
| Socket options | Five successful setsockopt calls: reuse-address plus send/receive timeout on each accepted connection. Native shows the two old-timeval options as `{5,0}`, length 16; managed scalar rows show options 21/20, length 16 and success. The scalar trace does not decode pointer bytes; the pinned source/native witness and focused timeout fixture supply that value evidence. |
| Signal and descriptor support | Thirteen rt_sigaction and fourteen rt_sigprocmask calls succeed; a real pipe2 and descriptor duplication/flags support the runtime's signal-helper path. Main writes readiness and stopped output. No asynchronous guest signal delivery is exercised by this ordinary run. |
| Resource and clock queries | sysinfo, prlimit64, getpid/gettid and sched_getaffinity return nonnegative values. CLOCK_REALTIME and CLOCK_MONOTONIC queries succeed. sched_yield counts vary between runs. These are private modeled resources and clocks, not host resource equivalence. |

The private host epoll record is size 16/data offset 8; the guest Linux x64
record is packed size 12/data offset 4. Upstream performs the conversion. The
private pthread mutex/condition/thread storage likewise uses the qualified
managed ABI, not native glibc pthread layout. See `tests/ThreadedLayout` for the
separate native managed-storage-policy and all-four-mode layout gate.

## Tolerated refusals are not implemented capabilities

The workload succeeds despite these actual negative results. They must remain
visible in the inventory rather than being relabeled as successful support.

| Observed refusal per managed mode | Scope |
| --- | --- |
| 12 `open` calls return ENOENT; two statfs calls return ENOENT | The private image contains no procfs/sysfs/cgroup tree. The native trace accesses these host paths and reads host configuration; scalar managed pointer arguments are not a pathname dump, so individual probes are not matched by guessing pointer contents. No whole host filesystem is mounted. |
| get_mempolicy returns ENOSYS once | The selected runtime tolerates unavailable NUMA policy. Native returns 0. No NUMA implementation or native equality is claimed. |
| Three `prctl(PR_SET_NAME, ...)` calls return EINVAL | Native names its Finalizer, Sockets and SigHandler workers. Managed naming support is absent; worker roles are suggested by native observations, not asserted from decoded managed string pointers. |
| Three clock_gettime calls for clock ID 6 return EINVAL | The actual runtime uses a fallback path. The native trace has no corresponding recorded clock_gettime syscall rows; no vDSO or timing equivalence is inferred. |
| IPv6 datagram and Unix datagram socket probes return EAFNOSUPPORT; IPv4 datagram returns EOPNOTSUPP | Only the actual IPv4/TCP path succeeds. UDP, IPv6 and Unix sockets are not newly implemented by this pass. |
| Three terminal ioctl probes return ENOTTY | Captured guest descriptors are not a terminal. |
| Child pipe read returns ECANCELED (125) during shutdown | This is the existing private owner-stop result, distinct from the EINTR results of the futex and epoll waits. All workers still join and release state normally. |

The mremap ENOMEM/EFAULT sequence above is an intentional validation-only path,
not a successful resize capability. `exit_group` has a null return observation
because it unwinds to its owning C# worker; it is not a zero-valued returned
syscall. Earlier clone/epoll ENOSYS and timeout ENOPROTOOPT failures were actual
startup blockers, preserved in the diagnostic README; they are not tolerated
failures in the final four-mode run.

## Native trace limits

The pinned native trace has 340 lines and four observed TIDs. Its epoll line is
literally an unfinished wait after the fd argument; the exit resume does not
recover maxevents, timeout or mask. Those values above come from complete
**managed** scalar observations, not invented native arguments. The native
futex and some mmap/socket/name calls also have unfinished/resumed records.
The receipt distinguishes completed mmap rows from unfinished requests; do not
count an unfinished request as a completed allocation without its return.
Native execve, procfs reads, getdents64 and lseek additionally reflect ordinary
OS process loading and discovery; absence from the private path is not evidence
that every such operation has been implemented.

## CPU and unsupported scope

The frozen threaded profile retains the narrowed CPUID policy described in
`docs/HOST-CPU.md` and the staged scalar FP corrections. It disables the upstream
JIT, linear host mapping, x87, MMX, BMI2, BCD, metal/ROM and fork paths while
selecting real managed guest threads. SSE/SSE2 execution remains selected;
optional advertisements including SSE3/SSSE3, PCLMULQDQ, POPCNT, CMPXCHG16B,
RDRAND/RDSEED, FSGSBASE, ERMS, RDPID, LAHF/SAHF, RDTSCP and invariant TSC are
narrowed. AES, SSE4.1/4.2, AVX/AVX2 and XSAVE advertisements remain clear.
This successful service executes under that policy; syscall traces do not
identify every guest instruction or prove whole-family ISA conformance.
The final finite P3 gate passed 504 selected normal native cases and 2,016 managed
comparisons (504 per raw/optimized JIT/NativeAOT mode), recorded in
`tests/CpuConformance/COVERAGE.md`. Its managed receipt is
`artifacts/cpu-conformance-managed/attempt-disfjyq2/receipt.json` (SHA256
`e3b4a964d69e0bced3d2896ea093f66c535008709fbd196318bc0fe7b99aa72e`).
The original 495-case lineage and later descriptor subsets remain preserved
separately; 46 historical custom fault descriptors are excluded from that final
normal set. These CPU producer/compiler/profile identities are not silently
requalified by the present service pass.

Unqualified features include epoll registrations/events/edge/one-shot semantics,
arbitrary signal delivery, fork/exec, dynamic dependency loading for this static
image, broad thread patterns, real mremap resizing, general framework workloads
and guest nonblocking socket modes. Socket send-expiry under backpressure is
not exercised: ordinary positive transfers with configured deadlines and normal
receive/accept expiry have focused native/all-four-mode evidence, but no peer
saturation or forced delay/disconnect was manufactured. The implemented timeval
rounding/range policy is not expanded into coverage of all inputs.

## Four-mode outcome and reproduction boundary

Each mode uses the exact raw/final source bytes of the delivery above, separate
private projects, an explicitly rooted TranslatedBlink NativeAOT consumer, and
a fresh process. The same guest ELF, native HTTP oracle and four environment
entries are used in all modes. No source postprocessing is repeated for the
optimized cases.

| Host mode | Instructions | Main syscall rows | Child rows | Result JSON SHA256 |
| --- | ---: | ---: | --- | --- |
| Raw JIT | 1,213,688 | 2207 | 7 / 6 / 3 | `e414ca6ed58f98912f1bc4d42a08efcac9467e71571d8347a71a3a7935f70b72` |
| Raw NativeAOT | 1,213,688 | 2207 | 7 / 6 / 3 | `70ef95fcae166078b06caf0639f211aca94c2981d2c8b716ad997a6921956241` |
| Optimized JIT | 1,214,130 | 2208 | 7 / 6 / 3 | `2dd521e21798b7b83ed517ab4f5478c38357a457f066c0710947adf4257b64f1` |
| Optimized NativeAOT | 1,213,246 | 2206 | 7 / 6 / 3 | `83a4a1d3ba9aa1b8f62d4a88c03239667b9dcd96a4a6191433f70e31b2bd5083` |

All traces are complete without truncation. All four results have exit 0,
StopReason None, four released Machines, all workers joined, released shared
memory and quiescence. The 86-byte health response hash is
`76e4d4da2ea0f33465293cb37bfbb4debc063bd83219be2bee6020c2c868eb22`;
the 91-byte stop response hash is
`eb9b2daed2fe19f73b73c0e01d841bfdb65e2bb8b77dc0c7932ea6b008ec0ecf`.
Stdout is exactly `READY 8080\nSTOPPED\n`, stderr is empty.

The runner is `tests/DotNetThreadedGuestExecution/run.py --all-modes`, with the
three receipt arguments documented beside it. This document records the frozen
run, not a promise that historical receipts still match every live source after
later production integration. Private input copies and their hashes preserve
what actually ran. No controller/two-instance/restart or cancellation-workload
claim is added by this normal service pass.
