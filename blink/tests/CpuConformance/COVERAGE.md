# Remaining CPU coverage

The twelve-case corpus is a seed, not P3 completion. Its passing modes qualify
only those exact inputs and defined-state comparisons. The retained feature
inventory remains `blink/docs/HOST-CPU.md`; no feature is considered covered just
because its CPUID bit is advertised or its handler is linked.

| Area | Current evidence | Important gaps |
| --- | --- | --- |
| Integer ALU/flags | Three 64-bit ADD/SUB boundaries | ADC/SBB, INC/DEC carry preservation, logical/multiply families, 8/16/32-bit widths and partial-register/zero-extension behavior, broader operands |
| Shifts | SHL masked64, SHR1, SAR63 | Count0/width/width+1 across widths, rotates/through-carry, double shifts, remaining defined flag boundaries |
| Division | Signed64 -17/5 and INT64_MIN/-1 fault | Divide-by-zero, unsigned division, other widths, maximum/minimum valid quotients/remainders |
| Decoder/addressing | One MOVABS crossing an instruction page | Prefix combinations, operand/address sizes, 15-byte length limit, ModRM/SIB/RIP-relative, FS/GS addresses, invalid encodings |
| Memory/fault restart | Unaligned8-byte cross-page read/write; crossing read fault | Crossing stores/fault atomicity, write protection, NX, canonical-address faults, alignment-sensitive operations, stack/REP restart |
| SSE/SSE2 | One PADDD lane vector | Other packed widths, saturation/comparison/shuffle/masks, scalar/packed moves, all floating operations and conversions |
| Floating point | None in this corpus | NaNs including signaling/payload handling, infinities, signed zero, subnormals, FTZ/DAZ, all MXCSR rounding modes, sticky status/unmasked exceptions, conversion limits and approximate-operation bounds |
| FXSR | Earlier separate native-only FX save/restore witness | Actual translated FXSAVE/FXRSTOR/MXCSR state round trips and masks |
| SSE3/SSSE3/PCLMULQDQ/POPCNT | Advertised/retained handler audit only | Actual translated hardware differential cases for each family |
| CMPXCHG8B/16B | Advertised/retained handler audit only | Success/failure state, alignment and atomicity contracts |
| CMOV/LAHF/SAHF/FSGSBASE | Advertised/retained handler audit only | Flag/register conditions, address effects and save/restore |
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
