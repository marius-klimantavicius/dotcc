# Host and translation boundary inventory

The complete selected 109-source core now binds, builds and executes in all four
Linux x64 forms. [The binding manifest](../config/host-bindings.json) records the
selected C-to-host operations; [validation](VALIDATION.md) records their native
and managed contract gates. The direct IL/import inventory is complete within
the limits in [DEPENDENCIES.md](DEPENDENCIES.md). Actual service startup and the
owning translated worker remain unqualified, so P4/P5 are still open.

The implemented host surface includes:

| Surface | Current contract and limits |
| --- | --- |
| Guest backing memory | Bounded private mappings, page protection metadata and file mappings; ordinary host data, with guest PTE enforcement. See [memory](HOST-MEMORY.md) and [file mapping](HOST-FILE-MAPPING.md). |
| Files and descriptors | Private namespace, file contents/metadata, shared-offset duplication, directories, readiness and bounded pipes. Advanced namespace/process operations return explicit errors. See [I/O](HOST-IO-BRIDGE.md), [descriptors](HOST-DESCRIPTORS.md) and [paths](HOST-PATHS.md). |
| TCP | BCL sockets behind private endpoint metadata and explicit publication, with leases, cancellation and close/drain tests. See [network bridge](HOST-NETWORK-BRIDGE.md) and [readiness](HOST-READINESS.md). |
| Environment and identity | Explicit private argv/environment-related storage, virtual process identity, clocks and entropy providers. See [environment](HOST-ENVIRONMENT.md), [identity](HOST-IDENTITY.md) and [clocks](HOST-CLOCKS.md). |
| Limits and termination | Private resource limits and synchronous guest status/fault handling; guest exits are trapped. Positive asynchronous signal/timer operations remain restricted. See [process policy](HOST-PROCESS-POLICY.md), [termination](HOST-TERMINATION.md) and [signal operations](HOST-SIGNAL-OPS.md). |

Qualification uses one translated owner per discarded worker process. Passing
individual host contracts does not establish actual guest-service startup,
complete indirect-call containment, or a hardened sandbox. Generic diagnostic
Console output still needs capture by the eventual worker, separately from
guest descriptor output. Current CPU advertisements and remaining execution
coverage are maintained in [HOST-CPU.md](HOST-CPU.md).

## Original P0 source-family inventory

The following design inventory is retained as the source-review baseline.
`scripts/inventory.py` records upstream C/header hashes, includes, explicit
extern declarations and TLS declarations in `config/source-inventory.json`.
The later frozen managed closure and runtime receipts refine this initial list.

| Upstream area | Selected role | Required embedding action |
| --- | --- | --- |
| `x86.c`, `x86error.c`, `bitscan.c` | Decoder probe closure | Keep upstream algorithms; compare layouts and decoded fields. |
| `machine.c`, `alu*.c`, `flags.c`, `divmul.c`, `bit.c`, `sse*.c`, `mmx.c`, `clmul.c`, `cmpxchg.c`, `xadd.c`, `xchg.c`, `op101.c` | Interpreter integer/SIMD dispatch | Retain interpreter and qualify advertised instructions; no native JIT. |
| `instruction.c`, `memory.c`, `memorymalloc.c`, `map.c`, `pml4t.c`, `address.c`, `stack.c`, `modrm.c` | Explicit guest address translation | Route allocation/mapping to host callbacks; forbid guest-address pointer shortcut. |
| `loader.c`, `elf.c`, `argv.c`, `reset.c` | ELF/argv/env setup | Keep Linux guest layout; bounded memory-backed image reads and explicit identity. |
| `syscall.c`, `xlat.c`, `iovs.c`, `fds.c`, `open.c`, `close.c`, `preadv.c`, `ioctl.c` | Linux ABI and descriptor marshalling | Checked guest copies and Linux errno; context-owned BCL files/sockets/readiness. |
| `signal.c`, `throw.c`, `machine.c` | Halt/fault/attention | Synchronous instance-result unwind and page-lock cleanup; no CLR signal recovery or host exit. |
| `time.c`, `random.c`, `rdrand.c` | Time and entropy | Explicit deterministic-test/BCL callbacks; qualify CPUID promises. |
| `vfs.c`, `hostfs.c`, `devfs.c`, `procfs.c`, `overlays.c`, `path.c` | Native filesystem implementation | Replace host services with private namespace; no host-root fallback. |
| `blink.c`, `blinkenlights.c`, `oneoff.c`, `panel.c`, `cga.c`, `mda.c`, `bios*.c`, `metal.c`, `pty.c` | CLI/TUI/devices | Excluded product surfaces; references must disappear through profile/adapter, never success stubs. |
| `jit.c`, `jitflush.c`, `uop.c`, `fusion.c`, `smc.c` | Native JIT machinery | Disable native generation; retain only genuinely required non-JIT data management. |

The table names source families, not a claim that every named file must be linked.
Support utilities and diagnostic dependencies still need actual link auditing.

`JitlessDispatch` loads/decodes the current guest instruction, advances `ip`,
dispatches `GetOp`, commits any pending stash and resets `oplen`. Budget accounting
must occur after this cleanup. `ExecuteInstruction` surrounds this dispatch;
`Actor` is unbounded and `Blink` uses `sigsetjmp(m->onhalt, 1)`. A replacement
embedding loop must preserve faults, attention, lock/stash cleanup, and distinguish
budget exhaustion from guest exit. REP/syscalls still need separate interruption.

TLS `g_machine` and `g_siginfo`, process flags, JIT maps, logging, and initialization
prevent concurrent contexts in one worker until audited. The initial host table
will carry an opaque instance handle; callbacks must not store movable managed
objects directly in C memory. Callback exceptions become guest errors or a pending
instance fault. Handles and pinned buffers need explicit acquire/release lifetimes.

## CPUID qualification gap

Upstream `cpuid.c` advertises SSE3, PCLMULQDQ, SSSE3, POPCNT, RDRAND, CMPXCHG16B,
TSC, PAE, CMPXCHG8B, CMOV, CLFLUSH, MMX, FXSAVE, SSE/SSE2, FSGSBASE, ERMS,
RDSEED, RDPID, LAHF, SYSCALL, NX, RDTSCP and long mode. BMI2/ADX are conditional.
FPU in basic leaf 1 follows `DISABLE_X87`, but **extended leaf 0x80000001 sets FPU
unconditionally**. The native baseline therefore does not certify the proposed
restricted managed CPUID profile. A reviewed staged capability-mask adaptation
and per-bit instruction tests are required; suppressing only x87 configuration
is insufficient. AES and SSE4.1/4.2 are explicitly clear in the inspected source.

This original source inspection alone did not qualify managed host services or
isolation. The service fixture's native observed syscall ledger is maintained
separately in `GUEST.md`; an actual managed startup trace remains missing.

The demonstrated exclusion mismatch is now corrected by the qualified staged
adaptation in [HOST-CPU.md](HOST-CPU.md). Its per-bit inventory and observed
matrices supersede the open mismatch above; broad instruction qualification
remains required.
