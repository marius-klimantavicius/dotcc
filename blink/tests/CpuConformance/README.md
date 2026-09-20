# Qualified normal CPU conformance corpus

The Kestrel extension now passes **514 normal native cases** in
`artifacts/cpu-conformance/attempt-havagw22/receipt.json`, SHA256
`a400af6ae56ad7e0ba4e6a7b5a157caea94b3ab2d418c7857281ba3cb300c267`.
Ten appended SS/SD/PS/PD comparison-mask and actual Hashtable conversion cases
retain every preceding descriptor. Six expose original upstream mismatches;
all ten match hardware after the reviewed eight-store source correction.
Raw/optimized JIT/NativeAOT qualification of this extension is pending a fresh
corrected canonical core. No custom fault cases execute.

The prior finite P3 CPU matrix passes **504 native cases and 2,016
managed comparisons**: 504 each in raw JIT, raw NativeAOT, postprocessed JIT and
postprocessed NativeAOT. This completes the bounded CPU case set selected for
P3; it does not certify an exhaustive ISA or general x86-64 application support.
Guest memory, ELF/TLS and host/service gates have separate evidence.

Managed receipt: `artifacts/cpu-conformance-managed/attempt-disfjyq2/receipt.json`,
SHA256 `e3b4a964d69e0bced3d2896ea093f66c535008709fbd196318bc0fe7b99aa72e`.
Fresh native receipt: `artifacts/cpu-conformance/attempt-27pjwx09/receipt.json`,
SHA256 `0e9557adcbebe0bca31ea6109ce9fa3889279abb4c18f88876d4ae8440ccdffa`.
All modes agree with the reviewed native interpreter under the exact comparison
contracts below. Hardware/original/staged native and all four managed RDTSC
invariant checks pass; timestamp values remain recorded without equality claims.

The run uses core-execution/attempt-273a6hks and canonical object assembly
`14c483263fd7e92b9522e1e414ceea7ce68a770ca561c8a1b2e33928d7e6b86b`.
It retains 108 exact canonical objects and replaces only the authored CPU
frontend. The corrected ALU producer object hash is
`0c12ac042d30048bca4559dcc387e7d9f114cee2f5a9d3114d22fa0662e081fe`;
the matching integer-boundary receipt hash is
`30e2bdeb2579c9b720458aad25ced148c8cdb594d1f1164f8421acef1aa3a8dd`.
The final receipt verifies compiler, postprocessor, implementation and frozen
inputs, raw source immutability, object identities and output binary hashes.
No compiler or generated C# was edited for these CPU corrections.

## Selection and exclusions

The runners select 514 normal cases from 560 descriptors. All first 550
remain unchanged; ten normal rows are appended at IDs 550–559. The 46 historical
custom fault rows (39 SIGFPE, seven SIGSEGV) retain their bytes and stable IDs but
are excluded from execution and never counted as passes. Direct case entry
also rejects a nonzero fault expectation. Native and managed receipts record
the identical selected/excluded IDs, reasons and reviewed source hashes.

`selection.py` pins the first 495, 512, 514, 546 and 550 descriptor digests, preserving
both qualified inputs and inputs from the recorded failures. The normal rows
cover integer/flags operations, signed division, valid instruction/data page
crossings, SSE/SSE2 lanes, masked FP state, NaN/infinity/conversion/rounding cases
and eleven CPUID queries. `faultState` is a state-comparison contract, not by
itself an expectation of a fault; exact operations that complete normally remain
selected even when exceptions are unmasked.

## Normal-return independent hardware witness

`hardware.c` executes the unchanged selected bytes on real Linux x86-64 hardware.
It appends LEA R15,[RIP-7] and RET, without INT3, deliberate signal delivery,
protected data holes or a signal handler. LEA records the actual address after
the tested bytes and changes no flags; RET returns to `hardware-capture.S`.
Unexpected faults fail the subprocess naturally instead of becoming expected
results. No expected ALU results are computed by the witness.

The System V assembly function preserves RBX/RBP/R12–R15, keeps the call stack
aligned and captures AX/BX/CX/DX, RFLAGS, both XMM lanes and MXCSR before any
flag-changing cleanup. It restores the calling C environment's MXCSR and clears
DF before returning. C compile-time offset assertions check the capture record.
The fixed reviewed corpus never modifies R12–R15. Only the exact balanced
stack recipe at ID545 temporarily switches RSP and restores it before returning
to the witness; all other rows leave RSP untouched. Source hashes in `selection.py` deliberately require renewed review
before any new corpus instructions can execute with this contract.
The appended CMPXCHG8B and FXSR cases also use caller-saved RDI; the trampoline
does not keep its private state there. The code capacity is32 bytes; original
instruction strings and lengths are unchanged.

## Reproduction and provenance

With the pinned native archive and a fresh matching qualified corrected core
available, set `BLINK_CORE_RECEIPT` to that core receipt path. The historical
P3 receipt above does not contain the new comparison-mask correction:

```sh
python3 blink/tests/CpuConformance/run.py --staged-fp --staged-integer
python3 blink/tests/CpuConformance/run-managed.py --staged-fp --staged-integer \
  --core-receipt "$BLINK_CORE_RECEIPT"
```

The native runner snapshots the independent hardware witness, C/assembly inputs,
native archive and headers, reviewed scalar/integer corrections and CPUID policy.
It retains the unchanged-original captures and their 368 differing rows
separately from the passing reviewed-native comparisons. These original
mismatches are neither hidden nor relabeled as passes.

The managed runner derives its CPU frontend over the exact qualified canonical
objects. Corrected integer objects must already exist in that profile, with
matching source bytes and the identical boundary receipt; older or mismatched
profiles fail closed. Each row runs in a fresh process with owning Host bindings,
compacting GC before execution, and normal owner teardown afterward. It compares
hardware instruction outputs, defined flags and memory; virtual CPUID compares
the exact native profile policy while physical CPUID remains an environment
witness. Only RDTSC AX/DX timestamp equality is omitted, with independent width
and preservation checks instead. Both runners verify exact ordered coverage and
unchanged live implementation inputs before success.

## Qualified appended normal coverage

| Stable IDs | Added behavior and comparison contract |
| --- | --- |
| 495–498 | INC64/32 and DEC8/16, with CF initially set or clear; defined arithmetic flags and upper-register behavior. |
| 499–504 | SHL8 count0/1, SHR16 count1, SAR32 count31, SHL16 masked count32 and SHL32 masked count33. Count0 preserves all flags; active shifts exclude AF, and SAR31 excludes OF. |
| 505–508 | Valid DIV8/DIV32 and signed IDIV16/IDIV32. Byte/word results preserve upper-register portions; dword results zero-extend. Division flags are undefined and masked out. |
| 509–510 | Normal CMPXCHG8B match/nonmatch at an aligned address. RDI saves the address before EBX becomes a fixed replacement value; ZF and complete memory/register results are compared. Single-thread cases do not establish atomicity. |
| 511 | FXSAVE/FXRSTOR roundtrip: clear XMM0/1 and temporarily load the saved MXCSR_MASK with LDMXCSR, then restore the saved XMM and nondefault MXCSR. The512-byte aligned save region is zeroed by ordinary REP STOSQ before memory comparison, excluding unspecified/vendor-specific saved x87/reserved bytes. |
| 512–513 | INC8 wrapping and INC16 signed overflow, preserving incoming CF and checking all defined arithmetic flags plus upper-register preservation. |

The REP sequence starts with DF clear, uses caller-saved RDI, and writes only
the valid512-byte save region. Its complete memory comparison retains the
surrounding data bytes. None of IDs495–513 modifies RSP or R12–R15, and
none expects a guest fault, invalid access or unsupported-instruction rejection.
The temporary MXCSR value uses the saved mask of writable bits (zero is valid
too); no floating-point arithmetic occurs before FXRSTOR. The31-byte sequence
has nine instructions: Blink's ordinary STOS implementation completes its64
iterations within one `ExecuteInstruction` call.

## Qualified baseline gap cases

| IDs | Bounded contract |
| --- | --- |
| 514–521 | ADC16/SBB64 overflow, AND32 zeroextension, XOR16 upper preservation, NEG8 minimum, ROL8/ROR64 count1 and SHLD32 count1. Logical/SHLD AF is excluded; all defined compared flags remain checked. |
| 522–529 | SSE2 signed/unsigned saturation, equality lanes, low-byte interleave, reverse dword shuffle, word shift, valid unaligned MOVDQU and aligned MOVDQA load/store. Both XMM lanes and complete data memory are compared. |
| 530–535 | SIB scaling/displacement, negative disp32, RIP-relative literal load plus forward jump, MOVZX8-to32, MOVSX16-to64 and valid cross-page64-bit store. |
| 536–537 | REP MOVSB copies16 bytes forward/backward between disjoint valid regions. Final RSI/RDI are normalized against BX into captured AX/DX; expected offsets16/48 and-1/31, CX0, full memory and DF0 are checked. MOV/SUB normalization overwrites arithmetic flags: those flags qualify the final SUB, not REP flag preservation. |
| 538–542 | Taken/not-taken CMOVNE32 and signed CMOVL64; taken CMOVB16, with register width effects and unchanged arithmetic flags. |
| 543 | CLFLUSH on valid mapped data preserves compared register/memory/flags/XMM state. It does not model or qualify physical cache effects. |
| 544 | RDTSC retains raw AX/DX samples but checks zeroextended32-bit halves and unchanged CX/BX, data memory, XMM, MXCSR, IP completion and defined flags independently in each hardware/native/managed capture. Timestamp values have no cross-process equality, nonzero, frequency, or timing claim. |
| 545 | Save RSP in caller-saved RDI, switch to valid data+512, PUSH AX/POP CX, restore exact RSP, clear EDI with flag-preserving MOV. This leaves the capture ABI and R12–R15 untouched; full data memory observes the stack write, and CX observes the popped value. |
| 546–549 | NEG16/32/64 minimum values check AF0 and all other defined arithmetic flags; NEG8 input1 checks AF1. Upper-register width effects remain compared. |

The capture compares AX/CX/DX, XMM0/1, MXCSR, completion IP, selected flags
and full data memory. BX is address-dependent and normally excluded from
cross-process equality (CPUID compares it; RDTSC checks preservation in each
process). REP normalizes SI/DI explicitly; the harness does not claim general
all-register comparison or stack restoration beyond the exact balanced recipe.
The independent hardware return additionally depends on the exact restored
stack pointer; native/managed interpreter entry also compares final SP with its
saved initial value before owner cleanup. No injected fault, invalid address, unsupported instruction or
signal sentinel is introduced.

`features.py` links these executed row names to measured CPUID features:
SSE/SSE2 leaf1.DX25/26; CMOV leaf1.DX15 and extended.DX15; RDTSC leaf1.DX4;
CLFLUSH leaf1.DX19; prior CMPXCHG8B and FXSR rows cover their retained advertised
bits. Base integer/addressing rows exercise the selected long-mode interpreter.
Feature mapping describes only bounded execution evidence; it does not claim
full ISA or general x86-64 application compatibility.

## Preserved failures and earlier qualification

The [reviewed integer staging](../../src/UpstreamInteger/README.md) fixes upstream
INC auxiliary carry, CMPXCHG8B nonmatch zeroextension and NEG auxiliary carry.
The immutable upstream reference and hardware comparison masks remain unchanged.

| Evidence | Receipt and SHA256 |
| --- | --- |
| Initial 466-row native failure: INC AF at IDs 495/496 and CMPXCHG8B upper bits at ID 510 | `artifacts/cpu-conformance/attempt-cjel3vz8/receipt.json` — `75f0c3a7bccc23e6b014116d8430a0a3a849637f82ed0fb5228fcf7048150812` |
| Its managed parent stopped before emission/comparisons | `artifacts/cpu-conformance-managed/attempt-f1feworz/receipt.json` — `e7a1c57b96b812b510c0631dcb9fdb13d46ffb4b22acc136af84047dd69cdc00` |
| Corrected 468-row matrix, 1,872 managed comparisons | `artifacts/cpu-conformance-managed/attempt-sgren8zu/receipt.json` — `e470675d116c505f377eff74671f709bac6c0b28a366646e5f6e1c94ffdf23d7` |
| 500-row native failure: NEG8 AF at ID 518; other 31 added rows and RDTSC invariants passed | `artifacts/cpu-conformance/attempt-hmylb6uo/receipt.json` — `ed8badd8f2400f05e13f3fbee247a8d11c3b49b5b711c3e3e10c0e0c7fe5ad70` |
| Corrected standalone native 504-row pass | `artifacts/cpu-conformance/attempt-ovjovt6b/receipt.json` — `798f5c3eb0a01dbcfe5135331ab746931895ee1bc1e56e511538af5ff0fdec33` |

No managed run began for the failing 500-row native selection. The NEG extension
preserves all 546 descriptors from that failure and adds four normal cases for
all repaired widths and both AF states. Its corrected ALU source-body hash is
`095e490901c5cdba26cd02d4c8381be008ce78da3802f7737618db77f7854301`.
Earlier 449-row and historical fault-containing matrices remain receipt-specific
evidence; [HISTORICAL.md](HISTORICAL.md) and [FP-FINDINGS.md](FP-FINDINGS.md)
retain those findings. They are not fresh passes for the current witness.
[Coverage limits](COVERAGE.md) distinguish this completed finite set from broader
ISA qualification that is outside the selected P3 gate.
