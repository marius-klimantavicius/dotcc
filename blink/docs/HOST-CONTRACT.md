# Host and translation boundary inventory

Status: reviewed P0 design and lexical inventory, not a completed host adapter or
proved reachable closure. `scripts/inventory.py` records each upstream C/header
hash, includes, explicit extern declarations and TLS declarations in
`config/source-inventory.json`. Native archive symbol extraction and staged
managed linking must refine this inventory before P2 can pass.

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

No managed host services, complete syscall inventory, or isolated execution gate
is marked passed by this document. The service fixture's observed syscall ledger
is maintained separately in `GUEST.md`.
