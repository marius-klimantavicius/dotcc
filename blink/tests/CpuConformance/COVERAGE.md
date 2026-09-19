# Remaining CPU coverage

The expanded 31-case corpus is a seed, not P3 completion; five FP rows are
measured failures (see `FP-FINDINGS.md`). Its passing modes qualify
only those exact inputs and defined-state comparisons. The retained feature
inventory remains `blink/docs/HOST-CPU.md`; no feature is considered covered just
because its CPUID bit is advertised or its handler is linked.

| Area | Current evidence | Important gaps |
| --- | --- | --- |
| Integer ALU/flags | ADD/SUB64 boundaries, ADC8, SBB32 zero-extension, IMUL64 overflow | Broader ADC/SBB widths, INC/DEC carry preservation, logical/multiply families and broader operands |
| Shifts | SHL masked64, SHR1, SAR63 | Count0/width/width+1 across widths, rotates/through-carry, double shifts, remaining defined flag boundaries |
| Division | Signed64 -17/5 and INT64_MIN/-1 fault | Divide-by-zero, unsigned division, other widths, maximum/minimum valid quotients/remainders |
| Decoder/addressing | One MOVABS crossing an instruction page | Prefix combinations, operand/address sizes, 15-byte length limit, ModRM/SIB/RIP-relative, FS/GS addresses, invalid encodings |
| Memory/fault restart | Unaligned8-byte cross-page read/write; crossing read fault | Crossing stores/fault atomicity, write protection, NX, canonical-address faults, alignment-sensitive operations, stack/REP restart |
| SSE/SSE2 | PADDD, PXOR, signed-zero ADDSD and exact ADDPS; measured FP failures | Other packed widths, saturation/comparison/shuffle/masks, scalar/packed moves and broader floating operations |
| Floating point | Signed zero/exact lanes pass; quiet-NaN AF, conversion precision and scalar-single rounding failures measured | Fix measured defects; signaling NaNs/payload rules, infinities, subnormals, FTZ/DAZ, remaining rounding modes, sticky preservation/unmasked exceptions, conversion limits and approximations |
| FXSR | Earlier separate native-only FX save/restore witness | Actual translated FXSAVE/FXRSTOR/MXCSR state round trips and masks |
| SSE3/SSSE3/PCLMULQDQ/POPCNT | Advertised/retained handler audit only | Actual translated hardware differential cases for each family |
| CMPXCHG8B/16B | Advertised/retained handler audit only | Success/failure state, alignment and atomicity contracts |
| CMOV/LAHF/SAHF/FSGSBASE | One CMOV condition now passes; other paths remain unqualified | Other conditions/widths, flag/register and address effects, save/restore |
| ERMS | REP paths retained | Direction, overlap, zero length, page-boundary fault restart; no speed claim |
| TSC/RDTSCP/RDPID/RDRAND/RDSEED | Separate capability/host contracts | Instruction register/flag semantics and bounded/injected-source properties; real clock/random values must not be compared for equality across runs |
| CLFLUSH | Retained interpreter fence | Instruction/fault behavior; physical cache effects are not modeled |
| PAE/NX/long mode | Current long-mode mappings only | Broader page-table and execution-permission cases |
| SYSCALL | Separate core exit/exit_group checks | Instruction-level RCX/R11/IP semantics and broader service contracts remain separate |

Current fault comparisons intentionally exclude unspecified register/flag
outcomes. Full raw captures are retained for diagnosis; broadening an assertion
requires identifying defined architecture behavior, not accepting whatever the
interpreter happens to produce. Additional FP cases likewise need real hardware
or pinned independent references and explicit exception/rounding assumptions.

Excluded x87/MMX/BMI2/ADX instruction rejection has earlier native witnesses;
those exact rejection cases still need the actual translated-core witness.
Features whose CPUID advertisements are clear (including AES/SSE4/AVX/XSAVE)
are not promoted into this profile by these tests. No compiler or instruction
algorithm was changed to obtain the current corpus results.

## Actual advertised feature inventory and proposed narrowing

The current complete-core CPUID instruction rows, not an inferred historical
profile, decide the reported advertisements. `feature_inventory` in the expanded
receipt records 41 locations for each managed mode, checks excluded bits remain
clear, and compares all four registers with the staged native profile. Physical
hardware feature differences are retained separately and are expected.

For a reviewed narrower profile, the currently advertised but instruction-
unqualified SSE3, SSSE3, PCLMULQDQ, POPCNT, CMPXCHG16B, FSGSBASE, ERMS,
RDRAND, RDSEED, RDPID, LAHF/SAHF and RDTSCP are explicit reduction candidates.
This is a proposal only: first reconcile dependent claims (for example clock
features), existing host contracts and the guest's required ISA. No bit is
cleared here. A single CMOV case likewise does not certify its whole family.
Baseline SSE/SSE2 floating defects cannot be resolved merely by removing
optional-extension advertisements from a usable x86-64 profile.

Older `HostCpu` evidence is retained: 16 focused direct-OpCpuid queries in all four modes,
eight native feature-toggle combinations and seven native instruction witnesses.
In particular the native FXSR/PXOR and excluded-family rejection results still
matter, but they do not count as complete-managed-core execution of those cases.
The current expansion adds seven full-core CPUID instruction queries; cache,
thermal, OS/architecture and unknown-leaf behavior from the older 16-query
focused fixture has not all been rerun through this corpus.
