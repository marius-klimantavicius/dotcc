# CPU conformance seed corpus

Run `python3 blink/tests/CpuConformance/run.py --staged-fp` on Linux x86-64. It builds an
independent hardware witness and a standalone interpreter consumer linked to the
pinned native archive, then runs each corpus case in a separate subprocess.
It does not build or qualify the managed core.

The corpus contains 495 cases: the original 24 instruction cases and seven
CPUID queries, plus 460 scalar FP correction witnesses and four policy CPUID queries. The original 31 input
rows remain unchanged.
Both consumers execute the same literal instruction bytes and start with the
same RAX/RCX/RDX, arithmetic flags, XMM0/XMM1 and memory contents. `describe.c`
exports every input byte, code/data placement, step bound, fault expectation and
flag mask into the receipt's `corpus.stdout`. All 4KiB/8KiB mapped data contents
are compared, not just the location an instruction should modify.

| Case | Witness |
| --- | --- |
| ADD overflow | Signed boundary `INT64_MAX+1`, six arithmetic flags |
| ADD carry | Unsigned all-ones plus one, six arithmetic flags |
| SUB borrow | Zero minus one, six arithmetic flags |
| SHL count64 | Masked-zero shift preserves value and arithmetic flags |
| SHR count1 | Defined CF/PF/ZF/SF/OF; AF excluded |
| SAR count63 | Defined CF/PF/ZF/SF; AF and OF excluded |
| Signed IDIV | Negative dividend -17 divided by 5; quotient/remainder from hardware; flags excluded |
| IDIV overflow | INT64_MIN divided by -1; real divide fault and faulting IP |
| SSE2 PADDD | Four differing 32-bit XMM lanes, wrapping values, flags preserved |
| Decode boundary | Ten-byte MOVABS begins three bytes before a page boundary |
| Data boundary | Eight-byte load/add/store straddles two guest pages |
| Data fault | Eight-byte load straddles an accessible page and unavailable second page |

SSE2 remains advertised by the campaign CPUID profile and its XMM execution
handlers remain selected (`docs/HOST-CPU.md`). No x87/MMX/BMI2/ADX/AVX behavior
is assumed here. This small corpus is not exhaustive coverage of any family.

## Independent hardware witness

`hardware.c` maps private test-only code/data, copies the corpus bytes, changes
code pages from writable to executable, initializes registers with a short
inline assembly trampoline, then jumps to the bytes. No arithmetic, division,
shift or SIMD result is computed in C. An appended INT3 stops successful cases;
Linux `ucontext` captures the actual registers, flags and XMM bytes. Division and
memory cases instead capture actual synchronous SIGFPE/SIGSEGV. Signal handlers
and executable mappings exist only in these short-lived native test processes,
not in the product host boundary.

Hardware INT3 advances RIP by one beyond the corpus bytes; the witness subtracts
only that sentinel byte. Interpreter output uses IP relative to its corpus
start. Success is compared at the next instruction; faults at the faulting
instruction. The hardware inaccessible second data page uses a PROT_NONE guard;
the interpreter's second guest page is absent. Only the common fault category
and restart IP are compared. Linux si_code and Blink's halt/signal-code outputs
are preserved independently and are not claimed identical.

Only architecturally defined flags are compared. IDIV flags are excluded;
fault cases compare fault/IP/memory and retain, but do not compare, general
register/XMM/flag captures. Other cases compare all initialized general
registers and both initialized XMM registers. Reserved/privileged flag bits and
Blink's internal lazy-flag bookkeeping are retained in raw captures but masked
out. The hardware does not count retired steps (reported -1); the interpreter
must meet its explicit successful-step/fault bound.

## Interpreter and provenance

`interpreter.c` uses unchanged pinned `NewSystem`, `NewMachine`,
`ReserveVirtual`, `CopyToUser`, `ExecuteInstruction`, `CopyFromUser`, and
synchronous `sigsetjmp` halt handling. Its only frontend signal hook records
upstream outcomes. Guest instructions, decoding, memory translation, fault
construction and SIMD/ALU algorithms are not replaced.

The runner snapshots the exact native archive, headers and config, corpus and
harness sources. It verifies the pinned source-inventory hashes and retains
compiler version, CPU/kernel identity, disassembly, link map, binary hashes,
full unmasked captures, complete mapped-memory dumps and comparison masks.
Each child has a bounded timeout; mismatch or abnormal exit fails the run and
preserves artifacts. It does not turn an unsupported hardware platform into a
passing reference.

`CpuInterpreterCase(index)` is the shared authored fixture entry. The native
runner above does not qualify managed execution. The separate translated-core
runner below reuses this same fixture and compares it with fresh hardware and
native interpreter outputs; P3 remains open (see `COVERAGE.md`).

## Actual translated-core consumer

Run `python3 blink/tests/CpuConformance/run-managed.py --staged-fp --core-receipt
blink/artifacts/core-execution/attempt-n9ligxbc/receipt.json`. The baseline must be a passing,
non-diagnostic actual core matrix produced by the same frozen compiler.

The runner retains all108 upstream/host objects from that109-object baseline
and replaces only its authored frontend/driver object. The new driver suppresses
the native CLI main, owns the single frontend signal hook, and supplies
`CpuConformanceRun(index)`. It seeds guest resource limits before NewMachine and
uses the same memory, file-reader, virtual-signal-action and exit-callback owner
lifecycle as the qualified core driver. No instruction implementation, upstream
source, existing profile, or generated C# is edited.

The exact canonical include paths are required, not only equal header bytes:
anonymous aggregate identities in the existing object format incorporate those
paths. Copies of these headers and their verified hashes are retained as
artifacts, while emission uses the canonical paths of the reused objects.
The derived object set and replaced object are recorded explicitly; it is not
presented as an unchanged canonical whole-profile cache entry.

Raw JIT, raw NativeAOT, postprocessed JIT and postprocessed NativeAOT each execute
all corpus cases in separate processes, using copied frozen host sources/bindings.
Each process binds real private owners, forces compacting GC, executes the actual
translated core, runs exit callbacks before tearing down owners, and exits.
No translated call follows mapping disposal: upstream slab globals still point
into those mappings. The managed consumer never invokes the native witnesses.

Every managed result compares defined state with actual hardware and the halt,
completed-step and captured signal results with native Blink. Full mapped-memory
contents are compared; retained mapping counts and charged bytes are recorded
and bounded. The semantic postprocessor touches a copy, and raw generated source
hashes are checked at completion. A failed comparison retains its exact inputs,
outputs, compiler/object identities and diagnostic logs.

## Expanded diagnostic matrix and CPUID inventory

The original 12-case complete-core matrix remains a passing baseline at
`artifacts/cpu-conformance-managed/attempt-yvylj8k6/receipt.json`. The expansion
adds ADC8, SBB32, IMUL64, CMOV, PXOR, signed-zero ADDSD, exact ADDPS lanes,
quiet-NaN UCOMISD, CVT/CVTT conversions, explicit rounding controls and seven
CPUID instruction leaves. MXCSR input is explicitly loaded in hardware and set
in the guest, then captured without masking defined exception/status bits.

The original 31-case expansion deliberately exposes upstream floating defects.
The later scalar corpus retains those failures in its original-native rows.
Default runs fail conformance. To finish collecting all rows without treating
those failures as passes, use `--observe-differences` on either runner. Such a
run can return successfully as an observation job while its receipt records
`completed: true` and `passed: false`; this flag must not serve as a CI
conformance waiver. Every discrepancy remains in the receipt. Managed results
are also compared independently against native Blink (`nativeDifferences`), so
an inherited upstream mismatch is distinguished from a translation regression.
See `FP-FINDINGS.md` for the reduced failing inputs and exact invariant failures.

CPUID register values describe the selected virtual CPU and should not equal
the physical CPU. For these rows, the native reference uses the existing
hash-checked HostCpu feature-guard adaptation; the managed result must match
that native profile in all four output registers. Hardware CPUID is retained
as an environment witness, not substituted as the expected virtual identity.
The upper 32 bits of native outputs must be zero; managed equality enforces the
same. Shared feature/profile files are never modified by these runners.

`features.py` decodes 41 observed feature locations, validates the selected
exclusions and records actual advertisements with bounded evidence labels.
The narrowed policy checks twelve optional feature bits and invariant TSC are
clear, plus zero outputs for thermal/power and unknown-leaf queries. This
advertisement policy does not imply retained handlers reject those instructions.
The runner consumes the explicit HostCpu staging policy; it does not infer
policy from observed failures. Clearing optional bits cannot repair baseline
SSE/SSE2 defects; the scalar correction is qualified independently.

Historical evidence is retained separately: `tests/HostCpu/run.py` previously
qualified 16 direct OpCpuid queries in all four focused modes, eight native
feature-toggle configurations, and seven native instruction/exclusion probes
(including FXSAVE/PXOR/FXRSTOR). That is useful existing evidence, but it does
not imply those 16 queries or seven instruction sequences all executed through
the current complete managed core. The current expansion establishes eleven
CPUID instruction queries and the specific instruction cases recorded
in its own full-core receipt.

## Reviewed staged scalar correction

Use `--staged-fp` on either runner to select the explicit reviewed correction in
[`UpstreamScalarFp`](../../src/UpstreamScalarFp/README.md). The native runner
keeps original native outputs and comparisons alongside staged native and
hardware witnesses. `--observe-differences` is unnecessary for a conformant
staged run; it never turns original failures into expected results.

`make-fp-cases.py` serializes explicit inputs into `fp-cases.h`; it contains no
expected instruction result calculation. Added inputs cover scalar CVT/CVTT
SS/SD-to-32/64, all rounding controls, exact/half-adjacent/limit values, signed
NaNs/infinities, masked and unmasked faults, sticky priority, denormals/DAZ/FTZ,
and COMIS/UCOMIS ordering and memory-fault precedence. Explicit MXCSR presence
allows zero; per-case halt classes distinguish SIMD from divide faults. New
fault comparisons retain defined GPR/XMM/MXCSR/arithmetic-flag state and SIMD
signal codes. Existing integer fault undefined state stays excluded.

The historical scalar managed derivation replaced four objects: the CPU frontend, `cvt.c`,
`ssefloat.c` and `throw.c`. The other 105 objects retain their producer identity.
The original preparation prefix `#include "host-bindings.h"` is verified against
each original source and repeated in separately hashed prepared C files, in
addition to the exact canonical header paths. The first attempt that omitted
this prefix failed aggregate identity validation at link time; its receipt
`artifacts/cpu-conformance-managed/attempt-f_sicaop` is retained. It involved
no generated-source edits.

The strict staged managed matrix passed all **1,964 comparisons**, 491 in each
of raw JIT, raw NativeAOT, optimized JIT and optimized NativeAOT, at
`artifacts/cpu-conformance-managed/attempt-kenqm2yg/receipt.json`. It reports
`completed: true`, `passed: true`, and `native_agreement: true`. The original
31-case failing diagnostic remains preserved; the staged result qualifies only
this reviewed source derivation and its recorded inputs.

For a canonical profile already containing the reviewed scalar correction, use
`--staged-fp` with its exact passing CoreExecution receipt. The runner verifies
the retained CPUID producer source against the fresh native policy and the three
scalar producer sources against the reviewed staged bytes and boundary receipt.
It then reuses those canonical objects and replaces only the authored frontend.
Older original-scalar baselines still require three explicitly recorded scalar
object replacements. A policy mismatch fails before managed emission rather
than silently qualifying a different profile.

## Narrowed canonical policy result

The 495-case matrix against canonical CoreExecution
`artifacts/core-execution/attempt-n9ligxbc/receipt.json` passed all **1,980**
comparisons at `artifacts/cpu-conformance-managed/attempt-jwuzr1go/receipt.json`,
with `completed`, `passed` and `native_agreement` all true. Its fresh native
reference is `artifacts/cpu-conformance/attempt-22ltifm3/receipt.json`.
It retains 108 exact canonical objects and replaces only the CPU frontend;
CPUID and all three reviewed scalar objects are reused without regeneration.
Eleven CPUID instruction queries observe the narrowed optional advertisements,
zero leaf 6 and unknown leaves. The whole-profile runtime, direct IL and
publication receipts are joined in
`artifacts/core/canonical-policy-integration.json`.

An explicit negative run against the preceding canonical policy fails before
managed emission (`artifacts/cpu-conformance-managed/policy-mismatch-rejection.json`).
This prevents a current native policy from silently qualifying an older virtual
CPU. Both historical profiles and their successful original receipts remain
valid evidence for their recorded inputs.
