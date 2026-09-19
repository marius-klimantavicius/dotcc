# Validation ledger

All observed executions below are Linux x64. Windows execution has not run.
Every skipped/unrun form remains open; a decoder pass is not CPU execution.

| Surface | Observed result | Reproduce |
| --- | --- | --- |
| Immutable source inputs | Archive/file/license hashes and offline verification pass | `scripts/fetch.sh --offline` |
| Native Blink interpreter | JIT/linear-memory-disabled build; 25 upstream assembly cases agree with direct Linux | `scripts/native-oracle.sh` |
| Native static musl HTTP guest | Six exact-wire request cases pass on direct Linux and native Blink; 18 guest syscall names observed | `scripts/build-guest.sh`, `scripts/test-native-service.py`, repeat with `--blink build/native/blink` |
| Native archive/link audit | Relink byte-identical; 89 of 147 archive members, 185 dynamic imports | `scripts/audit-native-dependencies.py` |
| Actual upstream decoder | Native/staged/raw JIT/raw AOT/optimized JIT/optimized AOT outputs agree for 11 encodings and ABI fields | `scripts/probe-decoder.sh` |
| Actual interpreter embedding | Native arithmetic, memory/faults, seven-step budget, exit and exit_group traps pass twice | `scripts/probe-core.sh --native-only` |
| Actual core ABI/register storage | 236 outputs match native profile in raw/optimized JIT/AOT, separate consumer and whole-library AOT roots | `tests/CoreAbi/run.py` |
| Namespace / assignment-guard repairs | Warning-free build; 2225 unit pass; 505 functional pass, 1027 skipped | `scripts/test-repository.sh` |
| Negated jump / nested string repairs | Warning-free build; 2228 unit pass; 507 functional pass, 1031 skipped | `scripts/test-repository.sh` |
| Inferred outer-array dimensions | Warning-free build; 2231 unit pass; 508 functional pass, 1033 skipped | `scripts/test-repository.sh` |
| Borrowed va_list formatting | Native/four-mode formatter matches; build clean, 2234 unit/509 functional pass, 1035 skipped | `scripts/test-repository.sh` and `artifacts/vsnprintf/attempt-cxeunj4b/receipt.json` |
| Declaration order / external pointer ownership | Build clean; 2235 unit/514 functional pass, 1037 skipped | `scripts/test-repository.sh` |
| Managed interpreter embedding | Translation in progress; no managed instruction execution claimed | `scripts/probe-core.sh` |
| Private memory filesystem | Four independent assertion groups pass Linux JIT/AOT; guest callback integration pending | `scripts/test-host-files.sh` |
| Private TCP namespace | Four assertion groups pass Linux JIT/AOT, including real backpressure/cancellation and same guest port in two instances | `scripts/test-host-sockets.sh` |
| Unified instance I/O | Four groups pass Linux JIT/AOT: common fd limits, dup lifetimes, bounded streams and disposal | `scripts/test-instance-io.sh` |
| Host storage ABI | 202 measurements match native in raw/optimized JIT/AOT; timer14, resource28 and times6 additional rows pass | `tests/HostAbi/run-managed.py` |
| Signal-aware virtual mask jumps | Native POSIX/virtual adapter agree with raw/optimized JIT/AOT; host OS mask unchanged | `tests/HostSignals/run.py` |
| Unmanaged ordinary jumps | Native output matches raw/optimized JIT/AOT with forced compacting GC | `tests/JumpStorage/run.py` |
| Jump / function parameter / array typedef repairs | Build pass; 2222 unit pass; 500 functional pass, 1025 skipped | `scripts/test-repository.sh` |
| Bounded anonymous host memory | Native staged InitMap/allocator and raw/optimized JIT/AOT agree; staged actual native core also passes | `tests/HostMemory/run.py` |
| Owned diagnostic reads / explicit byte order | Native untouched/staged and four managed modes agree; independent actual upstream load/store byte checks pass | `tests/HostMemory/run-diagnostic.py` |
| Authored clock / entropy callbacks | Native C invariants and four managed modes pass, including injected providers and failures | `tests/HostEnvironment/run.py` |
| Private file/vector C callbacks | Native C and four managed modes pass; 128 KiB bytes, sparse files, captured streams and independent workers | `tests/HostIo/run.py` |
| Translated C TCP callbacks | Native C and four managed modes pass; real clients, two private ports, exact 128 KiB response and canceled accept | `tests/HostNetwork/run.py` |
| CPUID exclusions | 16 queries match four managed modes; eight native configurations and seven native instruction probes pass | `tests/HostCpu/run.py` |
| Terminal storage declarations | 11 native rows match raw/optimized JIT/AOT; ioctl remains isolated/unimplemented | `tests/HostAbi/run-managed.py --ioctl` |
| Private stat metadata | Native layout/invariants and four managed modes pass; old file/descriptor regressions pass | `tests/HostFileMetadata/run.py` |
| Private poll readiness | Native and four managed modes pass; real TCP, timeout, FIN/HUP, close and infinite-empty disposal | `tests/HostReadiness/run.py` |
| Private process identity | Native C invariants and four managed modes pass; private IDs, errno and concurrent owners | `tests/HostIdentity/run.py` |
| Private openat / fcntl | Native/four-mode control and metadata pass, native exec oracle and existing fd regressions pass | `tests/HostFileControl/run.py` |
| Unexpected host termination | Native direct/indirect kind/status and four managed modes pass; controllers survive | `tests/HostTermination/run.py` |
| Managed service runner | Not implemented or qualified | P4/P5 open |
| Repository baseline | Build pass; 2218 unit pass; 490 functional pass, 1009 skipped | `scripts/test-repository.sh` |
| Bit-field repair regressions | Build pass; 2218 unit pass; 491 functional pass, 1011 skipped | `scripts/test-repository.sh` |
| Array parameter / tagged return repairs | Build pass; 2218 unit pass; 494 functional pass, 1017 skipped | `scripts/test-repository.sh` |

The decoder verifies actual `XedMachineMode` byte storage, `XedOperands` and
`XedDecodedInst` sizes, `op` offset, decoded instruction length, packed dispatch
encoding, immediate and displacement. Encodings cover NOP, immediate MOV,
register ADD, REP MOVSB, SSE XOR, SYSCALL, UD2, SIB/displacement addressing,
short backward branch, sign-extended immediate and overlength prefixes. The
case set is intentionally small; no execution/flags/CPUID compatibility claim
follows. Current common output SHA-256:
`de5ecc0473fd388b9455569d8b80b23d4ab57e452ba34352903690548da54c92`.

Decoder receipts record compiler/config/adapter/source/generated hashes under
`artifacts/decoder/receipt.json`. The raw generated snapshot remains separate
from the normal semantic postprocessor output. The postprocessor changed 129
boolean-conversion calls, 18 boolean comparisons and four standalone empty
blocks in the observed run; final output comparison remained exact.

Native core layout under the measured native profile is `Machine=22432`,
`System=3016`, `ax=24`, `ip=0`, `flags=12`, `onhalt=1264` bytes. This is not an
emitted-layout result. Core translation attempts retain diagnostic/history and
source/compiler/profile hashes; host profile storage checks do not implement
callbacks or make native process services safe for managed execution.

No P2–P6 completion is claimed. Raw/optimized × JIT/NativeAOT Windows execution,
whole-library-rooted AOT, complete CPU/ELF/memory corpus, per-instance host I/O,
worker lifecycle, performance and final dependent campaign regeneration remain
required by PLAN.md.

## Actual complete-core Linux x64 gate

P1/P2 now pass using core-execution/attempt-ny3j_02m: all109 objects link,
raw/optimized JIT/NativeAOT match the native bounded instruction/fault/exit
witness and configured ABI probe; AOT roots the complete library. Independent
raw/optimized direct IL audits report zero traversed native imports with their
limitations preserved in DEPENDENCIES.md. P3–P6 and Windows execution remain open.
