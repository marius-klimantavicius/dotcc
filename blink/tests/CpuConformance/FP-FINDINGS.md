# Measured upstream floating-point defects

## Kestrel-discovered SSE comparison masks

Pinned `OpCmppsd` writes integer predicate results (-1/0) into the floating union
member in all eight SS/SD/PS/PD stores. A true predicate therefore becomes numeric
-1.0, not an all-one bit mask. The translated product faithfully repeats this
upstream defect. The staged correction changes only those eight destinations
from `.f` to `.i`, under a second exact original-block pin in `ssefloat.c`.

The actual Hashtable constructor sequence is `MOVAPS xmm1,xmm0; CMPORDSS
xmm1,xmm0; ANDPS xmm1,xmm0; CVTTSS2SI eax,xmm1`. With inputs 2.16 (`400a3d71`)
and 5.04 (`40a147ae`), original native Blink produces integer zero in both cases;
hardware and the corrected native interpreter produce 2 and 5. Existing scalar
conversion corrections also preserve the proper MXCSR precision status.

Native receipt `cpu-conformance/attempt-havagw22` passes all514 selected normal
cases, including ten appended mask/upper-preservation/lane/conversion witnesses.
Six new rows retain their original-upstream mismatch; four false-mask controls
already match. First550 descriptor identity is pinned, and46 historical custom
fault cases remain excluded. The four-mode managed refresh is pending.

The sections below retain the earlier scalar diagnostic history.

The expanded actual-core diagnostic receipt is
`blink/artifacts/cpu-conformance-managed/attempt-s62j5n5q/receipt.json`.
It completed 124 comparisons:104 match hardware or the native CPUID profile,
and 20 reproduce five failing rows across raw/optimized JIT/NativeAOT. All 124
managed rows agree with native Blink on their compared state. The receipt is
`completed: true`, `passed: false`, `native_agreement: true`; this is not a
conformance pass or a compiler workaround.

The linked native reference and the translated core use the same immutable
upstream algorithms. Source provenance:

| Pinned source | SHA256 |
| --- | --- |
| `blink/cvt.c` | `0d036a0c30c19a6063859ebcd22fbc7623209d528e017003abe018d257e75d86` |
| `blink/ssefloat.c` | `cec86d3109804688d07e519d92d96b056899d1d36f0c0ccf644b933299dcaa09` |

Both are from commit `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`. The native
receipt linked by the matrix is `artifacts/cpu-conformance/attempt-s06br_hy/receipt.json`.
That receipt preserves the archive, copied headers/config, complete input bytes,
hardware disassembly and captures. Its only existing source adaptation is the
previously reviewed CPUID feature guard; no floating implementation was edited.

| Case/index and bytes | Defined input | Hardware | Native Blink and all four managed modes |
| --- | --- | --- | --- |
| `ucomisd-quiet-nan` /19: `66 0f 2e c1` | XMM0 low64=`7ff8000000000123` (quiet NaN), XMM1 low64=`3ff0000000000000`, input flags=`8d7` | Defined arithmetic flags=`45`, AF cleared | Defined flags=`55`, incoming AF remains set |
| `cvtsd-nearest-even` /20: `f2 48 0f 2d c0` | Double 2.5 (`4004000000000000`), MXCSR=`1f80` | RAX=2, MXCSR=`1fa0` | RAX=2, MXCSR=`1f80` |
| `cvtsd-round-up` /21: same bytes | Double 2.5, MXCSR=`5f80` | RAX=3, MXCSR=`5fa0` | RAX=3, MXCSR=`5f80` |
| `cvttsd-negative` /22: `f2 48 0f 2c c0` | Double -2.5 (`c004000000000000`), MXCSR=`5f80` | RAX=-2, MXCSR=`5fa0` | RAX=-2, MXCSR=`5f80` |
| `cvtss-round-up` /23: `f3 48 0f 2d c0` | Float 2.5 (`40200000`), MXCSR=`5f80` | RAX=3, MXCSR=`5fa0` | RAX=2, MXCSR=`5f80` |

The arithmetic flag comparison mask is `8d5` (CF/PF/AF/ZF/SF/OF). MXCSR is
compared without removing defined exception/status bits. Bit5 (`20`) is the
missing precision status in the conversion cases. All exceptions in these
inputs are masked; unmasked delivery is not qualified by these measurements.

`OpComissVsWs` in `ssefloat.c` assigns CF/PF/ZF/SF/OF but omits AF.
`OpGdqpWssCvtss2si` in `cvt.c` calls `rintf` without reading guest rounding
control; its double counterpart uses `SseRoundDouble` and selects floor/ceil/
trunc for the other modes. The measured conversion paths do not accrue guest
precision status. These observations identify upstream corrections to review;
no generated C# or generic runtime/compiler change is warranted to conceal them.

## Reproduction and interpretation

Run `python3 blink/tests/CpuConformance/run.py --observe-differences`, or run
`run-managed.py --core-receipt <qualified-core-receipt> --observe-differences`.
Every case runs in a distinct process. The exact case inputs are exported in
`corpus.stdout`; `hardware-19.stdout` through `hardware-23.stdout` and matching
native/managed outputs preserve the complete register, MXCSR and mapped-memory
witnesses. Default strict mode returns failure. Observation mode collects all
rows but retains `passed: false`; it is not an expected-failure pass.

The seven new nonfailing instruction cases cover ADC8, SBB32 zero-extension,
IMUL overflow flags, one CMOV condition, PXOR, negative-zero ADDSD and exact
packed ADDPS. Together with the 12 earlier cases, these give 19 passing instruction
inputs per mode. Seven CPUID instruction leaves match the staged native profile
in all four output registers per mode. These successes do not erase the five
measured FP failures or imply complete feature-family coverage.

## Reviewed staged correction qualified

The original failures above remain the immutable-source baseline. The later
reviewed correction in [`UpstreamScalarFp`](../../src/UpstreamScalarFp/README.md)
stages `cvt.c`, `ssefloat.c` and the SIMD delivery arm of `throw.c`, preserving
original sources and licenses. The scalar handlers now use raw IEEE bits,
guest rounding/masks and bounded integer operations; completed comparisons
clear AF and unmasked exceptions preserve destinations and defined flags.

The expanded corpus retains every original input field and adds 460 scalar
cases. Native strict run `artifacts/cpu-conformance/attempt-7c2xjo6w/receipt.json`
passes all 491 staged rows and preserves 397 mismatching original-native rows.
Intermediate receipts preserve a measured quiet-NaN/denormal priority error in
the first staged comparison helper, then its correction. Unmasked precision
and denormal tests separately exposed upstream `throw.c` reporting invalid
operation for every SIMD exception; that third-source correction was reviewed
after the failure and checked against Linux hardware signal codes and sticky
status priority. None of these disagreements was normalized away.

Actual full-core derivation
`artifacts/cpu-conformance-managed/attempt-kenqm2yg/receipt.json` passes all
1,964 strict comparisons: raw/optimized JIT/NativeAOT each execute 491 cases
and agree with hardware and staged native Blink. The derivation retains 105
original objects and replaces the frontend and three reviewed TUs, using the
same frozen canonical headers and compiler. Original packed/arithmetic FP
algorithms, x87 and JIT paths remain outside this bounded correction.
