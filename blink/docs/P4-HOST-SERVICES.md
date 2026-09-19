# P4 host-service integration ledger

P4 started after selected P3 completion at `c07228d`. This ledger distinguishes
current implementation, standalone translated C callbacks, actual guest syscall
execution, and actual service startup. Those are different evidence levels.
Only normal functional/lifecycle cases are added; custom fault injection and
invalid/malformed ELF remain excluded. P5/P6 do not start in this phase.

## Qualified contracts and resumed P4 integration

| Work | Owner | State |
| --- | --- | --- |
| Actual guest file/descriptor/vector/readiness path | Inputs worker, `tests/GuestIo/` | Native/all-four pass `guest-io/attempt-tnq5itfa`; exact bytes/state and cleanup |
| Cancellation through existing asynchronous I/O bridges | Consumer worker, `HostIo`, `HostNetwork`, `HostMessages`, `HostReadiness`, `tests/HostIoCancellation/` | All-four callback pass; final project/core integration passes `translation/attempt-16eer8t5` and `core-execution/attempt-g8r4tp0k` |
| Clock/entropy/thread-ID/private signal-state dispatch | Inputs worker, `tests/GuestEnvironment/` | Native/all-four pass `guest-environment/attempt-emy2577e`; exact finite state invariants |
| Bounded guest TCP syscall exchange | Consumer worker, `tests/GuestTcp/` | Native/all-four pass `guest-tcp/attempt-8r2oj_2k`; no ELF/HTTP/service loop |
| Standard streams and terminal query | Inputs worker, `tests/GuestStreams/` | Native/all-four pass `guest-streams/attempt-21z_o_3s`; exact captures and ordinary ENOTTY |
| Actual startup inventory and integration | Coordinator | Exact source/receipt map below; service startup remains unqualified |
| Actual translated service startup | Unqualified | Earlier automated service-worker task rejection remains binding; no renamed/recovered worker or surrogate startup pass |

Heavy builds are serialized. Shared compiler inputs and P0–P3 qualification stay
frozen. A changed host snapshot requires explicit integration provenance; tests
must not silently combine old canonical objects with current mutable host files.

## Pinned service's observed syscall surface

The service binary SHA256 is
`916eeadfde4092c55bd2d05d5cacbe20482f4cabda8c0436fcfb68d690f2a9b8`.
The native Blink oracle observed **18 guest syscall names** across startup and
six requests. Linux's 19-name set additionally includes the harness's `execve`.
The following map is source review, not a managed service execution trace.

| Observed names | Selected implementation route | Current evidence / gap |
| --- | --- | --- |
| `arch_prctl` | `syscall.c:SysArchPrctl` sets guest FS base for `ARCH_SET_FS` | Actual fixed TLS fixture passes; general musl service startup unrun |
| `set_tid_address` | `syscall.c:SysSetTidAddress` stores guest `ctid`, returns virtual `tid` | Actual guest ctid/tid state passes GuestEnvironment native/all-four |
| `open` | `SysOpen`/`open.c:SysOpenat` → `OverlaysOpen` → authored `HostFileControl.c` → `HostIoBridge` → `InstanceIo` private filesystem | Current standalone HostIo and loader access pass; guest dispatch passes GuestIo |
| `read`, `writev` | Actual guest buffer/iovec marshalling → `kFdCbHost` callbacks → `HostIoBridge` → private descriptor table | Guest marshalling, cursor and cleanup pass GuestIo; inherited standard-stream capture passes GuestStreams |
| `close` | `close.c:SysClose` plus upstream fd table → private `InstanceIo.Close` | GuestIo checks upstream empty fd table and only three remaining private standard descriptors |
| `ioctl` | Guest request translation → authored `HostTerminal.c`/`HostTerminalBridge` | Actual AddStdFd/captured stdout TIOCGWINSZ returns ENOTTY with unchanged valid buffer in GuestStreams native/all-four |
| `socket`, `bind`, `listen`, `accept`, `getsockname`, `setsockopt`, `shutdown` | Guest syscall marshalling → `HostNetworkBridge` → `InstanceIo`/private `VirtualTcpNetwork` | Actual blocking IPv4/TCP and option query/set pass GuestTcp; actual service startup remains separate |
| `sendto`, `recvfrom` | Guest marshalled message → `HostMessagesBridge` → private stream send/receive | Guest NOSIGNAL is handled upstream before the flags-zero bridge; exact finite exchange/EOF passes GuestTcp |
| `poll` | Guest pollfd marshalling → per-fd callback `poll(...,0)` → `HostReadinessBridge`; repeated waits use upstream `nanosleep` | Ready-file path passes GuestIo; TCP nonready/ready poll(0) passes GuestTcp; guest poll/sleep interruption remains separate |
| `exit_group` | Upstream `trapexit` state and `HaltMachine(kMachineExitTrap)`; host exit fallthrough binds a contained termination exception | Current normal core passes status 42; an owning service lifetime is not inferred |

Reference is pinned Blink revision
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`, selected by
`config/core-config.h`, with explicit `config/host-bindings.json` and authored
headers in `config/managed-host/`. `DISABLE_VFS` maps upstream Vfs operations to
those selected private callbacks. The manifest's 70-name isolation list is a
source-binding mechanism, not 70 missing implementations.

Native receipt hashes:

- Linux `artifacts/guest/native-linux/result.json`:
  `fd1f82c8e28f7b373e724b43a2eee877ea1c2dd1c06fe86986827c250ee77f02`.
- Blink `artifacts/guest/native-blink/result.json`:
  `31e47c9f4ec27a7f6c908b75c131359a80b6ce049370249f2df25133b12a3b8c`.
- Linux/Blink trace hashes respectively:
  `2a1f496c7028140f524eac8426b1f8d532182242c434b6a3759419097d9f4304`,
  `d21a05775c99e9cb230bd640a28fe1b6f3c496f4da2b26d369c970fab9a67e78`.

No clock, entropy, signal or timer syscall appears in this observed service run.
Loader AT_RANDOM consumes entropy separately; P4 still requires its explicit
clock/randomness contracts. Native syscall coverage is bounded by these requests.

## Environment, cancellation and scope limits

`HostEnvironment` implements BCL clock and entropy providers through
`HostEnvironmentBridge` and `HostClockBridge`. Historical HostEnvironment/Clocks
suites include custom invalid/provider-failure cases, so they are not fresh
normal-only P4 passes. Current valid ELF/TLS includes loader randomness; current
core execution qualifies bounded instruction accounting and trapped guest exit
37/exit_group 42. GuestEnvironment now separately qualifies the finite actual syscall set below.

Private signal masks/dispositions/policy and sleep interruption are implemented
in `HostSignals`, `HostSignalActions`, `HostSignalPolicy`, and `HostSleep`.
Positive asynchronous delivery remains restricted. The observed service does
not request it. Historical mixed-scope signal/sleep suites are not rerun as-is.
GuestEnvironment adds normal actual guest disposition/mask registration, queries
and restoration without delivery, with a separate actual host-registry query.

I/O bridges now propagate an owner-supplied cancellation token to existing
`InstanceIo` asynchronous operations. The original one-argument binding API is
retained. Ordinary cancellation/deadline/drain passes at that boundary. Host cancellation
returns `ECANCELED` (125), not a fabricated delivered guest signal. Actual guest
`Poll` and sleep retry also consult `CheckInterrupt` and guest state; bridge-only
cancellation cannot prove instruction-loop or zero-fd guest-poll cancellation.

Socket nonblocking is outside the selected blocking service fixture. Upstream
`FixupSock` asserts successful `F_SETFL(O_NDELAY)` while the private socket model
rejects it. That path must not be described as graceful unsupported behavior.
No optional IPv6, nonblocking or unrelated ISA expansion is added to this phase.

The full P4 gate remains contracts **and actual guest service startup**. P4 will
not be marked complete from standalone host tests, native service results,
controller fixtures or the normal-core sample. The earlier service-worker task
was rejected by automated review for a possible cybersecurity risk, without a
more specific reason. That rejected action is not retried or repackaged here.

## Actual guest-I/O subgate passed

`artifacts/guest-io/attempt-tnq5itfa/receipt.json`, SHA256
`15795abe9f6ead804b60eac9c3f996807f3d4f244c10da8515b583146f3c825c`,
passes native and raw/optimized JIT/NativeAOT with exact ten-row state output.
It retains 108 canonical objects from assembly 14c483 and the exact pre-token
Host snapshot, replacing only the authored frontend. Current source/producer,
closed logs and executed binary identities were independently rehashed.
This closes P4's ordinary filesystem/dup/short-I/O/cleanup checklist item.
It does not qualify TCP waiting, cancellation or actual service startup.

## Callback cancellation subgate passed

`artifacts/host-io-cancellation/attempt-blvv0ho4/receipt.json`, SHA256
`24085e30571d06b3fb41bdc3020cb960bcf708a5926c8323c0000b18a5065f6a`,
passes 22 finite translated-C scenarios and one direct BCL pipe-read reference
in each raw/optimized JIT/NativeAOT form. Native ABI/vector/poll smoke is separate
from the private cancellation contract. Exact transcripts, empty execution
stderr, source/snapshot/log/binary/tool identities pass. See the
[case contracts](../tests/HostIoCancellation/README.md) for pending operation
observations, pre-canceled send/connect limits, partial writes and owner drain.

This implements token propagation in four authored bridges. No C/interpreter or
shared compiler change was needed. The final generated project now includes those changed bridges through a fresh
Host snapshot; affected normal-core execution passes all four forms. Guest poll maps callback ECANCELED to
POLLERR, so this still does not establish actual guest stop/deadline behavior.

## Updated canonical Host integration passed

Final delivery `translation/attempt-16eer8t5` (SHA256
`259b8c3f3e5c8e9b6687b231defbed68d50ac4aa8926dd6b9844e07cc6f808c7`)
verifies and reuses all 109 unchanged C objects, publishes the four changed
authored bridges, and verifies immutable raw/final manifests and builds. New
canonical assembly 734288 has receipt SHA256
`779514d915d56d58a531e390a1d0a143d67bbde14d496e473429d321b689bdf4`.
Affected core execution `core-execution/attempt-g8r4tp0k` passes all four forms
and its ABI/direct boundary checks; SHA256
`45e49ad508405e15142389d94244f61e9098b69e86a1f405eb98ff859582e8eb`.
This updates P4's Host integration without rerunning unrelated P3/P6 suites.
The environment and TCP fixtures will use this exact new canonical snapshot.

## Actual guest environment and signal state passed

`guest-environment/attempt-emy2577e/receipt.json`, SHA256
`45d7a0080dfac1664a95c9bb269f80a3318192483396213ecb4c25ee619f7706`,
passes native and raw/optimized JIT/NativeAOT against canonical 734288. Actual
SYSCALL dispatch covers clock_gettime/getres, getrandom, set_tid_address,
rt_sigaction and rt_sigprocmask across two normal lifecycles. Each execution
retains ten raw observations and ten invariant rows; timestamps, random bytes
and native/virtual identities are not compared numerically across processes.

Native clock resolution is 1 ns; the managed provider reports and observes a
100 ns output quantum on this host. Random return length and valid cross-page
canaries pass without entropy-quality assertions. Guest signal IGN/query/DFL
restoration also checks the actual host disposition registry; private mask and
ctid storage pass without delivery, threads, futex or clear-on-exit claims.
Guest/host cleanup and stable bounded retained pool pass. All 108 retained
producer identities and execution inputs were reviewed; no source fix was needed.
See [the exact fixture contracts](../tests/GuestEnvironment/README.md).

## Actual guest TCP subgate passed

`guest-tcp/attempt-8r2oj_2k/receipt.json`, SHA256
`ad31914a360345f527ae55fcff7dcb669b8e8b86b706971953585e447549a03d`,
passes native and all four managed forms against canonical 734288 with 108
retained producers. One listener and one accepted connection carry an exact
257-byte request and 263-byte response, checked by independent native libc/BCL
peers. SO_REUSEADDR and TCP_NODELAY values and lengths are queried, readiness is
observed before/after peer handoff, and valid cross-page addresses/payloads retain
canaries. Orderly EOF/shutdown/close, guest mapping/fd cleanup and host pending
operation drain pass. Raw private/physical endpoint observations are preserved
without numeric equality between environments. Source/producer/log/binary hashes
and exact five-line state outputs were independently checked.

No implementation repair was needed. The fixture has no service ELF, HTTP or
execution worker, and poll uses timeout zero. It does not qualify service startup
or guest stop/deadline integration. [Exact contracts](../tests/GuestTcp/README.md).

## Actual guest standard streams and terminal query passed

`guest-streams/attempt-21z_o_3s/receipt.json`, SHA256
`0f62de4b0ffed1af3c25a4a9b09863cd8ba098ad790912fd173d455be3c81104`,
passes native and all four managed forms against canonical 734288. Two upstream
lifecycles initialize actual standard fd records via AddStdFd and use SYSCALL
read/writev/write/ioctl. Frozen binary input (34 bytes), stdout (46) and stderr
(14) compare exactly, separately from a bounded diagnostic sidecar. Native
fd0/1/2 are redirected real streams; managed captures belong to InstanceIo.
Managed process stdout/stderr remain empty. Ordinary TIOCGWINSZ returns ENOTTY
with unchanged valid cross-page winsize/canaries. Guest metadata cleanup preserves
the three underlying standard descriptors; guest close(0..2) is not tested.
Sources, 108 retained producers, logs, binaries and every capture were rehashed.
[Exact contracts](../tests/GuestStreams/README.md).

## P4 stop condition

Three of four P4 checklist items are qualified for the finite selected profile:
implemented host contracts, ordinary descriptor/filesystem lifecycle tests, and
the individually mapped 18 syscall names from the pinned native service trace.
There is still no actual translated managed service-startup trace. The remaining
checklist item requires owning execution and outstanding-I/O stop/deadlines,
including controller containment; callback ECANCELED alone does not satisfy it.

The previous automated service-worker rejection remains the external blocker
for that owning integration and actual service execution. Independent contract
work is complete; no worker substitute or additional phase is attempted. P4 is
incomplete and stopped at this blocker. This ledger preserves the successful
contract results without claiming a service run, guest wait cancellation,
optional nonblocking behavior or hardened sandbox isolation.

## Explicit P4 continuation — 2026-09-20

The user requested a renewed P4 attempt. The preceding stop/rejection entries
record that attempt's history. They do not create a permanent prohibition on
ordinary local implementation. Remaining work is now assigned: persistent host
cancellation/deadlines, cancellable sleep, reviewed actual syscall safe points,
an owning interpreter loop, and execution of the pinned controlled service ELF.
Any current rejection will be preserved with its exact reason and operation;
no action is concealed, relabeled or routed around. No new execution pass is
claimed by this resumption, and P5/P6 remain held.
