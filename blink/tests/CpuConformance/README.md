# Normal CPU conformance corpus

The default runners select **500 normal cases** from 546 inputs: the original
495 descriptors remain unchanged, and 51 normal cases are appended at IDs495–545.
The remaining 46 custom fault cases (39 SIGFPE, seven SIGSEGV) retain
their original bytes and stable indices but are excluded from execution under
the current user scope. They are not counted as passes. Native and managed
receipts contain the identical selected IDs/names, all excluded IDs/names with
reasons, full corpus digest and reviewed source hashes. Direct native or
translated case entry also refuses a row with a nonzero fault expectation.
The selector also verifies the exact canonical digest of the first495 exported
descriptors, the first512 descriptors from the preserved failing attempt, and the ordered appended names. The468-case matrix is qualified below; the newest32 rows await execution.
The earlier449-case matrix remains historical evidence.

The selected cases retain normal integer/flags operations, signed division,
valid instruction/data page crossings, SSE/SSE2 operations, masked FP status,
NaN/infinity/conversion/rounding cases and all 11 CPUID rows. A zero fault
expectation is decisive: `faultState` identifies a state-comparison contract,
not necessarily a trapping case. Explicitly unmasked but exact operations that
complete normally remain selected.

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

## Reproduction

With the pinned native archive and current matching core already qualified:

```sh
python3 blink/tests/CpuConformance/run.py --staged-fp --staged-integer
python3 blink/tests/CpuConformance/run-managed.py --staged-fp --staged-integer \
  --core-receipt <current-qualified-core-execution-receipt.json>
```

The native runner snapshots hardware/C/assembly/input sources and the pinned
archive, reviewed scalar/integer corrections and CPUID policy. It exports all original
case descriptions but executes only the recorded normal selection. The managed
runner recomputes that selection and rejects a different native selection or
comparison list. Both runners hash their live implementation inputs at start and
check those files again before success; the final comparison sequence must equal
the selected rows in every requested mode. It derives a CPU frontend over the exact qualified core objects;
unchanged object hashes, canonical include paths and compiler identities must
match. Reviewed scalar objects are reused only with matching source identities. Integer
staging requires a newly qualified canonical profile containing exact corrected
alu/machine producers and the identical integer boundary receipt; it fails
closed on an older or mismatched profile.
No generated C# is patched.

Each selected row runs in a fresh process in raw JIT, raw NativeAOT,
postprocessed JIT and postprocessed NativeAOT, for **2,000 expected comparisons**.
The managed consumer binds private owners, forces compacting GC, executes the
actual translated interpreter and discards the process after owner teardown.
Its instruction outputs are compared with real hardware; virtual CPUID outputs
instead match the exact staged native policy, while physical CPUID remains an
environment witness. Defined flag masks and full memory/state comparisons are
retained. No service worker or malformed-image tests are part of this harness.

The normal-return implementation passed native 449 and all **1,796 managed
comparisons** against core-execution/attempt-8vbjtywv. Native receipt:
`artifacts/cpu-conformance/attempt-7j03qmvz/receipt.json`, SHA256
`b33297b3c0ba375772763b274f8236dd042bdc8fa6e292ebe0206776dbb85c73`.
Managed receipt: `artifacts/cpu-conformance-managed/attempt-3r1msqyr/receipt.json`,
SHA256 `4692e7760c37899215cec4025fcd116fd0eeeb22ddd3c47a320067d6a0e75ff0`.
The derived link retains 108 exact qualified objects and replaces only the CPU
frontend; reviewed scalar corrections already match those retained producers.
All four modes agree with the staged native interpreter. The unchanged-original
native comparison retains 359 differing normal rows separately; this does not
reclassify them as passes or conceal the reviewed scalar corrections.
The dedicated launch temporary directory was `artifacts/cpu-normal-launch/tmp`.
No shared compiler was rebuilt. Each receipt verifies the exact ordered selection
and live implementation hashes at completion. [Historical evidence](HISTORICAL.md) preserves prior
matrices and defect receipts, including now-excluded cases. The historical
495-case pass is not a fresh pass for this changed witness. Remaining instruction
coverage is described in [COVERAGE.md](COVERAGE.md).

## Qualified appended normal coverage

| Stable IDs | Added behavior and comparison contract |
| --- | --- |
| 495–498 | INC64/32 and DEC8/16, with CF initially set or clear; defined arithmetic flags and upper-register behavior. |
| 499–504 | SHL8 count0/1, SHR16 count1, SAR32 count31, SHL16 masked count32 and SHL32 masked count33. Count0 preserves all flags; active shifts exclude AF, and SAR31 excludes OF. |
| 505–508 | Valid DIV8/DIV32 and signed IDIV16/IDIV32. Byte/word results preserve upper-register portions; dword results zero-extend. Division flags are undefined and masked out. |
| 509–510 | Normal CMPXCHG8B match/nonmatch at an aligned address. RDI saves the address before EBX becomes a fixed replacement value; ZF and complete memory/register results are compared. Single-thread cases do not establish atomicity. |
| 512–513 | INC8 wrapping and INC16 signed overflow, preserving incoming CF and checking all defined arithmetic flags plus upper-register preservation. |
| 511 | FXSAVE/FXRSTOR roundtrip: clear XMM0/1 and temporarily load the saved MXCSR_MASK with LDMXCSR, then restore the saved XMM and nondefault MXCSR. The512-byte aligned save region is zeroed by ordinary REP STOSQ before memory comparison, excluding unspecified/vendor-specific saved x87/reserved bytes. |

The REP sequence starts with DF clear, uses caller-saved RDI, and writes only
the valid512-byte save region. Its complete memory comparison retains the
surrounding data bytes. None of IDs495–513 modifies RSP or R12–R15, and
none expects a guest fault, invalid access or unsupported-instruction rejection.
The temporary MXCSR value uses the saved mask of writable bits (zero is valid
too); no floating-point arithmetic occurs before FXRSTOR. The31-byte sequence
has nine instructions: Blink's ordinary STOS implementation completes its64
iterations within one `ExecuteInstruction` call.

The first expanded native run completed466 cases but failed IDs495/496 (INC AF)
and510 (CMPXCHG8B upper32 bits). Its receipt is
`artifacts/cpu-conformance/attempt-cjel3vz8/receipt.json`, SHA256
`75f0c3a7bccc23e6b014116d8430a0a3a849637f82ed0fb5228fcf7048150812`.
The managed parent stopped before emission or comparisons:
`artifacts/cpu-conformance-managed/attempt-f1feworz/receipt.json`, SHA256
`e7a1c57b96b812b510c0631dcb9fdb13d46ffb4b22acc136af84047dd69cdc00`.
[Reviewed integer staging](../../src/UpstreamInteger/README.md) corrects these
upstream expressions without changing hardware comparisons. The native runner
retains original-native captures and differences; staged and canonical managed
source identities remain distinct. Corrected native execution passed all468 selected rows, with46 exclusions:
`artifacts/cpu-conformance/attempt-lc9j96ag/receipt.json`, SHA256
`0a58ffb7f3a083709795ac1b86a93bff1fb44420f35690614b9227aefdbc2e41`.
The receipt preserves364 original-native differences separately. INC8/16 add
independent hardware evidence for the other two repaired AF helpers; unchanged
FXSR and the remaining appended normal cases pass. New canonical managed execution passed all **1,872 comparisons** against
core-execution/attempt-yzck8kpr, with exact native agreement in all four modes.
Managed receipt: `artifacts/cpu-conformance-managed/attempt-sgren8zu/receipt.json`,
SHA256 `e470675d116c505f377eff74671f709bac6c0b28a366646e5f6e1c94ffdf23d7`.
Fresh native receipt: `artifacts/cpu-conformance/attempt-x6p4oc67/receipt.json`,
SHA256 `48d2da3321e3b2cf24b619a0d644e5e3383406c93efb4eba79722ad14e097d09`.
The derived CPU link replaces only authored/managed-driver.c and retains108
qualified producer objects, including the exact corrected alu.c and machine.c.
Their identical integer-boundary receipt hash is
`d7d7816ae9ae74f9fda094cd178c831cc009cbb3563f6c5fcc241ef69fbc0729`.
The managed receipt records both corrected producer source/object hashes, all
binary hashes, immutable raw output and unchanged compiler/implementation
identities. The fresh native receipt again preserves364 original differences;
46 custom fault rows remain excluded. This qualifies only the bounded normal
inputs and compared architectural state, not full instruction-family coverage.

## Additional32 normal cases awaiting qualification

The first514 descriptors are pinned to the qualified native corpus. These
additional rows preserve its46 exclusions and require a new500-row native and
2,000-comparison managed matrix. No new execution pass is implied here.

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
