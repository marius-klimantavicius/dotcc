# P4 host-service integration ledger

P4 started after selected P3 completion at `c07228d`. This ledger distinguishes
current implementation, standalone translated C callbacks, actual guest syscall
execution, and actual service startup. Those are different evidence levels.
Only normal functional/lifecycle cases are added; custom fault injection and
invalid/malformed ELF remain excluded. P5/P6 do not start in this phase.

## Current work

| Work | Owner | State |
| --- | --- | --- |
| Actual guest file/descriptor/vector/readiness path | Inputs worker, `tests/GuestIo/` | Native/all-four pass `guest-io/attempt-tnq5itfa`; exact bytes/state and cleanup |
| Cancellation through existing asynchronous I/O bridges | Consumer worker, `HostIo`, `HostNetwork`, `HostMessages`, `HostReadiness`, `tests/HostIoCancellation/` | All-four callback pass; final project/core integration passes `translation/attempt-16eer8t5` and `core-execution/attempt-g8r4tp0k` |
| Clock/entropy/thread-ID/private signal-state dispatch | Inputs worker, `tests/GuestEnvironment/` | Native passes `guest-environment/attempt-aeqsjnzi`; all-four qualification active |
| Bounded guest TCP syscall exchange | Consumer worker, `tests/GuestTcp/` | Source preparation; no ELF/HTTP/service loop |
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
| `set_tid_address` | `syscall.c:SysSetTidAddress` stores guest `ctid`, returns virtual `tid` | GuestEnvironment native passes; managed matrix active |
| `open` | `SysOpen`/`open.c:SysOpenat` → `OverlaysOpen` → authored `HostFileControl.c` → `HostIoBridge` → `InstanceIo` private filesystem | Current standalone HostIo and loader access pass; guest dispatch passes GuestIo |
| `read`, `writev` | Actual guest buffer/iovec marshalling → `kFdCbHost` callbacks → `HostIoBridge` → private descriptor table | Guest marshalling, cursor and cleanup pass GuestIo |
| `close` | `close.c:SysClose` plus upstream fd table → private `InstanceIo.Close` | GuestIo checks upstream empty fd table and only three remaining private standard descriptors |
| `ioctl` | Guest request translation → authored `HostTerminal.c`/`HostTerminalBridge` | Captured stdout's ordinary `TIOCGWINSZ` query returns `ENOTTY` in native trace; translated guest path unqualified |
| `socket`, `bind`, `listen`, `accept`, `getsockname`, `setsockopt`, `shutdown` | Guest syscall marshalling → `HostNetworkBridge` → `InstanceIo`/private `VirtualTcpNetwork` | Blocking IPv4/TCP implemented; historical standalone C bridge tests are not actual guest startup |
| `sendto`, `recvfrom` | Guest marshalled message → `HostMessagesBridge` → private stream send/receive | Guest NOSIGNAL is handled upstream before the flags-zero bridge; current guest integration unqualified |
| `poll` | Guest pollfd marshalling → per-fd callback `poll(...,0)` → `HostReadinessBridge`; repeated waits use upstream `nanosleep` | Ready-file path passes GuestIo; interruption of guest poll/sleep remains separate from direct host poll cancellation |
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
37/exit_group 42. Neither establishes arbitrary environment syscalls.

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
