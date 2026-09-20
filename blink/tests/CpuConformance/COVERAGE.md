# Selected CPU qualification and broader coverage limits

The Kestrel extension adds ten normal SSE comparison-mask cases at IDs550–559.
All 514 normal native cases pass in `cpu-conformance/attempt-havagw22`; all
2,056 managed comparisons pass in `cpu-conformance-managed/attempt-jt41ulk6`.
The managed run uses the compiled public delivery baseline and does not assert
a CoreExecution pass. First 550 descriptors and all 46 excluded
fault descriptors remain unchanged. This is a targeted extension, not a new
claim about complete packed floating-point semantics.

The prior finite P3 CPU case set is complete: **504 native cases and 2,016 managed
comparisons**, 504 in each raw/optimized JIT/NativeAOT mode, pass against the
corrected canonical core. The exact contracts and provenance are in [README.md](README.md).
Managed receipt: `artifacts/cpu-conformance-managed/attempt-disfjyq2/receipt.json`,
SHA256 `e3b4a964d69e0bced3d2896ea093f66c535008709fbd196318bc0fe7b99aa72e`.
Fresh native receipt: `artifacts/cpu-conformance/attempt-27pjwx09/receipt.json`,
SHA256 `0e9557adcbebe0bca31ea6109ce9fa3889279abb4c18f88876d4ae8440ccdffa`.

In that prior P3 receipt, all 550 descriptors retain stable IDs; 504 normal rows execute and 46 historical
custom fault rows remain excluded. The original 495, 512, 514 and 546 descriptor
subsets are hash-pinned. The fresh reference retains 368 unchanged-original
native differences separately from the reviewed staged-native pass. Source
corrections are explicit, pinned and checked against canonical producers;
comparison masks were not changed to accept implementation differences.

This completes the agreed finite CPU portion of the selected profile. It does
not certify every operand, encoding, instruction family or x86-64 application.
The broader limits below are not additional pending work for this finite gate.
Guest memory, valid ELF/TLS, host services and lifecycle have separate receipts.

## Qualified normal cases and limits

| Area | Current bounded evidence | Broader limits |
| --- | --- | --- |
| Integer/flags | ADD/SUB, ADC/SBB, INC/DEC carry preservation, NEG at all widths, AND/XOR, IMUL and defined flag boundaries | Other operand combinations, widths and multiply/logical families are not exhaustive. |
| Shifts/rotates | Count-zero/masked counts, SHL/SHR/SAR widths, ROL8/ROR64 count1, SHLD32 count1 | Through-carry rotations, other double shifts and all boundary combinations remain outside this finite set. |
| Division | Valid unsigned8/32 and signed16/32/64 results, upper-register preservation/zeroextension | Division flags are undefined and excluded; no custom arithmetic faults execute. |
| Addressing | Instruction-page crossing, SIB scaling/displacement, negative disp32, RIP-relative literal load, MOVZX/MOVSX and valid cross-page loads/stores | Not every prefix, address size, instruction length or addressing combination. |
| Stack/REP | Exact balanced data-stack PUSH/POP with restored SP; forward/backward valid16-byte MOVSB with full bytes, count and normalized final SI/DI | Not general all-register, overlap or restart qualification. Normalizing SUB overwrites REP arithmetic flags; DF0 is checked. |
| SSE/SSE2 | Packed arithmetic, signed/unsigned saturation, equality/interleave/shuffle, word shifts, aligned/unaligned moves and both captured XMM lanes | Other packed widths, operations, memory encodings and broader arithmetic remain outside the selected recipes. |
| Scalar FP | Reviewed COMIS/UCOMIS and conversion cases with defined limits, ties, rounding modes, NaN/infinity and normal masked-state behavior | Not every result payload, arithmetic underflow/overflow, approximation or FP environment. Custom fault rows remain excluded. |
| FXSR | Actual translated FXSAVE/FXRSTOR roundtrip restoring XMM0/1 and nondefault MXCSR | Saved x87/reserved/vendor bytes are cleared before comparison, not asserted equal; no exhaustive save-image compatibility. |
| CMPXCHG8B | Normal match/nonmatch register, memory and ZF results, including nonmatch zeroextension | Single-thread cases do not establish atomicity. CMPXCHG16B is not advertised. |
| CMOV | Existing CMOVZ plus taken/not-taken CMOVNE32 and CMOVL64, taken CMOVB16 | Other conditions and widths are not exhaustive. |
| RDTSC | Independent AX/DX zeroextension and preserved-state invariants in hardware, original/staged native and all managed modes; raw timestamps retained | No timestamp equality, nonzero, monotonicity, frequency or timing claim. |
| CLFLUSH | Valid mapped address with unchanged compared architectural state | Physical cache effects are not modeled or qualified. |

## Measured feature policy

Eleven full-core CPUID instruction queries measure the actual profile.
`feature_inventory` checks 41 feature locations in every managed mode, validates
excluded advertisements remain clear and compares all four CPUID output
registers with the exact staged native policy. Hardware feature differences
remain separate environment evidence. `features.py` maps bounded executed row
names to retained SSE/SSE2, CMOV, RDTSC, CLFLUSH, CMPXCHG8B and FXSR bits.
A bit or a linked handler alone is not execution evidence.

Optional SSE3, SSSE3, PCLMULQDQ, POPCNT, CMPXCHG16B, FSGSBASE, ERMS, RDRAND,
RDSEED, RDPID, LAHF/SAHF, RDTSCP and invariant-TSC advertisements remain clear;
thermal/power leaf6 is zero. The x87/MMX/BMI2/ADX/SSE4/AES/AVX/XSAVE exclusions
also remain enforced. These cleared bits do not imply undefined-instruction
rejection tests. Normal REP execution does not re-enable ERMS advertisement.

PAE/long-mode/NX are exercised by the separately qualified valid guest-memory,
ELF and TLS paths; the exit/exit_group SYSCALL paths also have separate core
receipts. Their scope must be read from those receipts rather than inferred
from this CPU matrix. No general system-mode or service compatibility is claimed.

## Historical evidence

[HISTORICAL.md](HISTORICAL.md), [FP-FINDINGS.md](FP-FINDINGS.md) and the
[README failure table](README.md#preserved-failures-and-earlier-qualification)
preserve earlier passes and failures, including now-excluded custom faults.
The initial466-row run exposed INC AF and CMPXCHG8B register-width defects; the
500-row run exposed NEG AF. All original inputs and receipts remain preserved.
Their reviewed corrections and the current finite normal matrix are distinct
from unchanged-upstream conformance. Historical rejected/fault/invalid-input
rows are not proposals or requirements to resume those tests.
