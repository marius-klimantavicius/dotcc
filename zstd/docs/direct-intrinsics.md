# Direct intrinsic emission

Status: direct emission and the selected Linux x64 product matrix qualified,
2026-10-20. The accepted adapter-based optimization remains the historical
reference in [optimizations.md](optimizations.md). This follow-up has separate
compiler, downstream, product and timing evidence. Direct emission is not a
general performance improvement over that already optimized reference.

## Outcome and boundary

Known direct calls selected by a typed intrinsic rule emit their BCL
operation at the caller. Function-pointer and exported calls keep the original
signature and wrapper identity. The compiler chooses BCL operations; the .NET
JIT or NativeAOT compiler still decides their machine instruction lowering.
Compression, decompression, streaming, dictionary/training and hashing algorithms
remain upstream C translated through DotCC. Generated C# is never textually patched.

Selection is bound to the resolved C symbol, including a rule matched by a late
declaration. It does not rewrite a same-named local callback or member. Direct
intrinsic arguments receive the original C formal conversions before rendering:
narrowing/truncation, signedness, enum conversion and widening must preserve the
C result and select the correct BCL overload. Every argument executes once;
ordinary memory reads/writes and other call effects stay visible to IR passes.

The change is intrinsic-only. Authored managedMethod targets, passInstance,
noreturn contracts, canonical function addresses and object-link conflict checks
retain their prior behavior. Existing rendered object fragments materialize
intrinsic calls while retaining the selected contracts; object-only linking does
not retroactively apply a new profile.

The closed intrinsic registry covers ordinary native/LE/BE fixed-width
unaligned memory using Unsafe/BinaryPrimitives, swaps, bit counts and selected
endian/derived operations. It accepts exact ordinary byte or void pointer
contracts and rejects volatile/atomic accesses and const writes. LP64 size_t
remains unsigned 64-bit. Nonzero-only upstream bit helper domains remain unchanged.
LE24 read/write retain two authored adapters to guarantee exactly three-byte
accesses and one-time argument evaluation without unsafe expression duplication.
Curated result expressions need explicit return conversions, safe parentheses and
valid discard/for/comma lowering; profiles cannot supply arbitrary C# templates.

## Preserved optimized reference

Before any mutation, clean HEAD `1f9677154` and its current optimized products
were copied into the read-only ordinary-copy snapshot
`artifacts/optimizations/baseline-optimized-1f9677154-20261020T031725Z`.
All 8665 files / 1,865,238,823 bytes were rehashed; no symlink or writable path is
present. Manifest SHA256:
`e8e4ff12a5ae5cfe89cad16b98229cc9eb23379ba69e9a9d0921fff2810bf9e2`.
The snapshot includes both exact canonical products, authored adapter/API/root
metadata, configuration, benchmark harness and reports/binaries, current complete
tool closures, qualified receipt/work and the committed source archive. The
initial untouched-C snapshot is separate and unchanged.

The same 29-workload harness and settings compare the current adapter baseline
with direct emission across raw/processed JIT/NativeAOT. Source/frame identity,
warmup, repeated samples, alternating process order, allocation observations and
exact final source binding remain required. Faster code is not a prerequisite
for correctly emitting direct calls; meaningful regressions must be investigated
and results reported honestly.

## Gates

| Gate | State |
| --- | --- |
| Current optimized products/tools/source preserved immutably | Passed |
| Unchanged optimized-reference baseline measurement | Passed: all 29 workloads in all four cells |
| Native-backed failing direct-call/conversion/effect regression | Passed: native reference, eight before-fix structural failures and preserved failing enum/underscore cases |
| Symbol-bound intrinsic-only implementation and closed registry | Passed: focused compiler/functional suites |
| Actual caller-body assertions in raw and processed direct/object/split output | Passed: focused tests, four-cell fixture and actual product IL audit |
| Pointer identity, late declarations, shadowing, narrowing/overload conversions | Passed |
| Discarded, void-cast, comma and for initializer/increment expressions | Passed, including source locals/parameters named `_` |
| Focused compiler/functional tests and affected Blink/production smoke | Passed: 170 unit / 34 functional, Blink 336 rows/four cells, SQLite and QuickJS current-tool smoke |
| Complete 31-unit zstd emission and required selection/caller audit | Passed first integration: 747 intrinsic + 48 LE24 selections |
| Native/primitive/ABI/consumer/behavior/lifecycle/interop/upstream/audit matrix | Passed: final receipt 20261020-065322-dcdf2024, all four cells |
| Ordinary source-preserving copied delivery | Passed: ordinary solution and 47-file copied closure, both forms |
| Matched final-product benchmark against preserved optimized reference | Passed: 116 exact comparisons and exact final-product binding; performance regressions recorded below |

Build and timing windows are serialized under shared campaign locks and budgets.
Keep historical failed attempts and separate new acceptance from both the original
port and the adapter optimization. No push is authorized by this campaign.


## Optimized-reference measurements

`artifacts/optimizations/direct-intrinsic-original-benchmark-20261020/report.json`
passes all 29 cases in each of four cells. This measures the existing adapter
implementation before compiler mutation; final comparisons use fresh
alternating processes against these verified baseline binaries. The complete
sample/quartile/MAD, allocation, input/frame, source/binary and host/tool records
remain in the report. Host and settings match the preceding benchmark: Linux x64,
AMD Ryzen 9 7950X CPU 0, .NET 10, tiering disabled, at least 200 ms warmup and five
calibrated batches of at least approximately 100 ms per workload.

| 128 KiB text, level 3 (MiB/s) | Raw JIT | Raw NativeAOT | Processed JIT | Processed NativeAOT |
| --- | ---: | ---: | ---: | ---: |
| Compression | 571.16 | 556.56 | 705.48 | 619.85 |
| Decompression | 596.85 | 614.04 | 608.04 | 593.57 |

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py --baseline zstd/artifacts/optimizations/baseline-optimized-1f9677154-20261020T031725Z --output zstd/artifacts/optimizations/direct-intrinsic-original-repeat --variant original
```

Every benchmark run uses a fresh output directory. The original snapshot and
existing untouched-C snapshot remain unchanged; no generated product is replaced
by this reference measurement.


## Shared implementation and first complete retry

Selected intrinsic metadata is attached to the bound function symbol. Direct
emission consults that identity after declaration binding, including late
selection; explicit wrapper operations remain in place for exported and indirect
calls. Arguments run through their original C formal conversions with explicit
BCL overload binding. Curated result expressions preserve the C result type and
are fully parenthesized; object contracts and managedMethod/passInstance/noreturn
paths are unchanged.

Discarded scalar results use the underlying BCL invocation without unused result
casts/arithmetic. An unused no-argument endian fact disappears. This avoids
introducing `_ = ...` assignments that would overwrite a legitimate C local or
parameter named `_`. Mixed CommaOp/CommaSeq loop clauses retain argument/call/
assignment/volatile effects while omitting proven-pure unused operands. No
identifier policy, API name, temporary sink, delegate, boxing or allocation was
introduced. A newly exposed enum-constant narrowing defect is repaired using the
same unchecked out-of-range constant guard as ordinary integer coercions.

Shared evidence is indexed by
`../../artifacts/function-override-direct-20261020/shared-summary.json`, retaining
before-fix commands/TRX, native fixtures, source/tool digests and exact tests.
Eight direct/object and single/split caller assertions fail before the feature;
the managedMethod isolation control passes. Native enum truncation passes while
the first generated C# fails CS0221. Native underscore preservation passes while
both pre-repair managed controls return the wrong result. Those failed records
remain history, not relabeled passes.

Final focused validation passes 170 unit and 34 functional cases, including
existing instance callback/passInstance and two new intrinsic instance-owner
controls. The actual Release tools also pass the native-backed fixture across
raw/processed JIT/NativeAOT, with actual caller source bodies inspected in both
forms:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 DotCC.FunctionalTests/Fixtures/function-intrinsic-calls/qualify.py --output artifacts/function-override-direct-20261020/qualified
```

Its `qualified/report.json` retains all commands and tool/source hashes. Unit,
functional and emitted fixture evidence uses the final compiler implementation.
An unrelated existing parser limitation for a `(void)` declarator returning a
function pointer is not repaired or claimed; the identity test uses an unused
int parameter while retaining the same address/deduplication proof.

First complete zstd retry `artifacts/campaign/20261020-064606-43b48ba9/receipt.json`
emits/links all 31 units and builds both canonical products, with exactly 747
intrinsic and 48 managed LE24 selections. The two authored LE24 bodies are
unchanged. Every original C signature, linkage and physical selector remains
unchanged from the optimized reference.

`artifacts/direct-intrinsic-audit-discovery/report.json` passes four private
positive/negative audit-tool controls and both actual product IL checks. Each
assembly scans all 1960 non-selected-wrapper methods for forbidden direct
intrinsic-wrapper or obsolete-adapter edges: none remain. Positive evidence is
separately classified across 1781 algorithm methods, including BIT_ helpers.
Generic MethodSpec calls and endian field reads are decoded; retained ldftn
addresses are permitted. Concrete callers include `BIT_flushBits` to
`WriteUInt64LittleEndian`, `FSE_buildCTable_wksp` to `Unsafe.WriteUnaligned<ulong>`,
`ZSTD_adjustCParams_internal` to `LeadingZeroCount` and `HUF_buildDEltX2U32` to
the endian fact. Wrapper-only evidence and stale calls both fail the private controls,
including a helper outside the algorithm-name prefixes. These discovery checks
do not stand in for the complete final codec/runtime matrix.


## Affected downstream checks

`../../artifacts/function-override-direct-20261020/downstream/report.json` passes
and retains exact commands, receipts, logs, caller checks, tool closures and
owned shared source hashes. There were no shared source changes after the final
tools/fixture qualification, and every selected tool closure still matches it.

| Downstream | Executed scope | Evidence |
| --- | --- | --- |
| Blink | Existing configured-header native 336-row witness; raw/processed JIT/NativeAOT; actual Read/Write caller source bodies. | `blink/artifacts/endian-intrinsics/attempt-nra5f0uv/receipt.json` |
| SQLite | Full current-tool amalgamation regeneration and both builds/JIT consumers; source/object function-identity JIT/NativeAOT. | `sqlite/artifacts/campaign/20261020-064855-adfbf9a9/receipt.json` |
| QuickJS | All five current-tool source units, both builds/JIT consumers and both JIT behavior cells, each 5 harness / 16 engine cases; deliberate assertion controls return 1. | `quickjs/artifacts/campaign/20261020-065102-a9e62254/receipt.json` |

Blink uses an ignored private driver that uniquely adapts only its subprocess
import to the shared production adapter, retaining original `__file__`, original/
adapted hashes and effective bounded commands. Authored Blink files are unchanged.
This applies the campaign's shared locks/budgets without altering its witness or
claiming broader Blink guest qualification. Existing passInstance callback and
managed/noreturn controls execute in the focused compiler suites. Unaffected
production campaign matrices and unrelated extended suites were not repeated.


## Final correctness and delivery before timing

Final receipt `artifacts/campaign/20261020-065322-dcdf2024/receipt.json` passes all
12 tasks and 200 commands, including intentional failure/guard controls. It
freshly regenerates every selected unit, publishes both products and executes
all nine declared suites. This current receipt supersedes discovery products for
qualification and the exact frozen-product benchmark below.

| Suite | Raw JIT | Raw NativeAOT | Processed JIT | Processed NativeAOT |
| --- | --- | --- | --- | --- |
| ABI | 3 | 3 | 3 | 3 |
| Behavior | 30 | 30 | 30 | 30 |
| Lifecycle | 7 | 7 | 7 | 7 |
| Interoperability | 6 + 3 exact native comparisons | 6 + 3 | 6 + 3 | 6 + 3 |
| Actual upstream | 33 | 33 | 33 | 33 |
| Ordinary consumer | Pass | Pass | Pass | Pass |
| Caller/dependency/license/rooted-AOT audit | Pass | Pass | Pass | Pass |

All required failed/skipped counts are zero. Native and eight original/replaced
primitive cells match 227486 checks, digest `9b3f85183c30d739`, and the real
three-byte guard plus intentional inaccessible-page controls. The unchanged
upstream case groups remain `[1,1,1,11,13,6]`; actual native/emitted context-size
observation remains 582672 in every cell. Tracing remains explicitly off.

`root-solution/report.json` and `copied-delivery/report.json` in that receipt
directory also pass. The stronger delivery helper checks 47 original-byte
source/project/notice/build files, both copied-form builds and consumers, and
actual evaluated references contained in the copied tree (ten commands).
Receipt binding covers 34 generated outputs and seven production inputs. Six
root/sample metadata files absent from historical production_inputs are labeled
explicitly unbound; their copied bytes/closure are still checked, and pre-timing
provenance freezes them separately. No new historical digest is invented.

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/tests/delivery.py --receipt zstd/artifacts/campaign/20261020-065322-dcdf2024/receipt.json --output zstd/artifacts/campaign/20261020-065322-dcdf2024/copied-delivery
```

The recorded delivery output already exists; reproductions need a fresh output
path and the new successful receipt produced by their verification. Measure those
final products afterward, without regenerating between timing and source binding.
Compiler/campaign implementation commits precede the final measurement.

## Final measurements and performance limits

The shared implementation is commit `72039d41f`; campaign adoption is
`e063f8f58`, measured without subsequent regeneration. The earlier preservation
milestone is `128a4f821`. Final report
`artifacts/optimizations/direct-intrinsic-final-benchmark-20261020/report.json`
passes all 116 exact input/frame comparisons. This is direct emission versus
the preceding optimized adapters, not versus untouched C. Both variants use the
same preserved owning API, harness and settings, fresh private build paths, and
original/candidate/candidate/original process order. Each workload pools ten
samples per variant. Full samples, medians, IQRs, allocation observations and
input/frame hashes are retained; `comparison-cases.csv` and `.md` list every row.

Speed ratio is adapter latency divided by direct-emission latency: below 1 is
slower. The median column below summarizes ratios across workloads; it does not
erase the individual regressions.

| Cell | Minimum ratio | Median ratio | Maximum ratio | Slower medians / 29 |
| --- | ---: | ---: | ---: | ---: |
| Raw JIT | 0.816× | 0.983× | 1.040× | 21 |
| Raw NativeAOT | 0.675× | 0.994× | 1.071× | 18 |
| Processed JIT | 0.873× | 1.019× | 1.499× | 13 |
| Processed NativeAOT | 0.708× | 1.000× | 1.116× | 15 |

| Representative speed ratio | Raw JIT | Raw NativeAOT | Processed JIT | Processed NativeAOT |
| --- | ---: | ---: | ---: | ---: |
| 128 KiB text level 3 compression | 0.914× | 0.754× | 0.873× | 0.729× |
| 128 KiB text level 3 decompression | 0.974× | 1.004× | 1.122× | 1.019× |
| 128 KiB mixed-binary level 3 compression | 0.816× | 0.675× | 0.893× | 0.717× |
| 1 MiB text streaming compression | 0.921× | 0.788× | 0.892× | 0.790× |
| 1 MiB text streaming decompression | 1.001× | 1.004× | 1.098× | 1.000× |
| 4 KiB dictionary compression | 0.938× | 0.850× | 0.977× | 0.708× |
| 4 KiB dictionary decompression | 0.983× | 1.009× | 1.165× | 1.007× |

67 of 116 medians are slower. The largest slowdown is raw NativeAOT mixed-binary
compression: adapter/direct median 110.206 / 163.292 microseconds, a 48.2% latency
increase. Its IQRs are 109.522–110.708 / 162.792–164.049 microseconds. The worst
processed NativeAOT case is dictionary compression: 8.458 / 11.948 microseconds,
IQR 8.399–8.518 / 11.927–11.955. Raw JIT mixed-binary compression is
97.626 / 119.651 microseconds, IQR 96.744–98.866 / 118.099–121.277. These repeated
compression regressions are larger than the recorded dispersion. Conversely,
the largest ratio, processed JIT incompressible compression at 1.499×, has a
wide baseline IQR of 21.344–45.162 microseconds versus 21.999–22.312 for direct
emission; that single median is not a stable universal gain.

All 112 context-reuse/streaming observations retain zero managed allocation in
both variants; allocating roundtrip observations remain separate. No codec API,
algorithm, source feature selection, workload or output frame changed. The
default now satisfies the requested direct-call emission contract, with these
measured performance costs; it is not accepted as an across-the-board speedup.
Machine lowering remains the JIT/NativeAOT compiler's decision.

The bounded PE/IL review is retained in
`../../artifacts/function-override-direct-20261020/performance-review/report.json`,
with a reproducible metadata reader and exact assembly/source hashes. The main
double-fast compressor has identical IL after masking only metadata token
operands: 1777 raw / 1414 processed bytes, unchanged conversions and branches.
The ten memory-call operands change to six unsigned32 and four unsigned64
generic loads. Hash4–8 pointer helpers grow from 15 to 21 IL bytes because span
construction moves into their bodies; `ZSTD_count` grows by 37 bytes from direct
endian/bit-count expressions. No extra conversion, wrong overload, allocation
or runtime-service defect was found in these callers.

The measured raw NativeAOT main double-fast method grows from 5452 to 5530
native bytes, with 14 versus 39 static call instructions. The 25 additional
edges are upstream `ZSTD_hash8Ptr` / `7Ptr` / `6Ptr` / `5Ptr` / `4Ptr` helpers
(9 / 4 / 4 / 4 / 4); those edges are absent from the baseline method. This
confirms lost helper inlining. `ZSTD_count_2segments` remains 747 native bytes.
Raw JIT main double-fast grows from 5228 to 5708 native bytes, with 31 versus 38
static calls; the baseline retains arithmetic-hash calls while the candidate
retains pointer-hash calls. `ZSTD_count` remains 305 JIT bytes with zero calls,
and `ZSTD_count_2segments` remains 699 JIT bytes. The bounded disassembly and
exact method/binary/sidecar identities are retained in
`artifacts/optimizations/direct-intrinsic-final-benchmark-20261020/codegen/investigation.json`
and its readable `.md`, with original/candidate JIT and AOT disassembly.
Diagnostic executions use short settings and are separate from acceptance
timing. Static call-site counts are not dynamic call counts; the diagnostic does
not establish what fraction of workload latency those calls cause or isolate
the runtime's inline heuristic. The affected upstream helpers are unadorned static functions;
there is no existing typed override setting for their inline policy. Forcing
arbitrary callers, replacing hash algorithms, reintroducing adapter dependence
or assuming little-endian machine behavior would broaden this change. Such
tuning remains a separate follow-up, and the frozen results are retained.

`final-product-binding.json` passes exact hashes without normalization: all 51
frozen inputs remain unchanged, each cell binds its 13 measured source files,
and candidate tools match the qualified compiler closure. Old and new compiler
identities intentionally differ. The harness and owning API match exactly.
`baseline-posttiming-verification.json` rehashes all 8665 immutable reference
files successfully after timing. The initial untouched-C snapshot is unchanged
and is not the denominator for this follow-up's ratios.

Reproduce in this order, using fresh output directories and the new verification
receipt. Verification regenerates products; timing must follow their final
publication and source binding, without another regeneration afterward:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py --baseline zstd/artifacts/optimizations/baseline-optimized-1f9677154-20261020T031725Z --output zstd/artifacts/optimizations/direct-intrinsic-repeat --variant candidate --compare-to zstd/artifacts/optimizations/direct-intrinsic-original-benchmark-20261020/report.json
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/tests/delivery.py --receipt zstd/artifacts/campaign/NEW_SUCCESSFUL_RECEIPT/receipt.json --output zstd/artifacts/copied-delivery-direct-repeat
```

No required gate for the selected direct-emission scope remains incomplete.
Unselected features/platforms and broader upstream drivers retain the existing
exclusions. The current native/translated original-body primitive fixture covers
all four cells; the separate full `original` opt-out recipe's narrower smoke
qualification remains historical as recorded in [validation.md](validation.md).
This does not claim a fresh full original-profile codec matrix.
