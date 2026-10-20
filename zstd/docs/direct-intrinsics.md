# Direct intrinsic emission

Status: implementation in progress, 2026-10-20. The accepted adapter-based
optimization remains the historical reference in [optimizations.md](optimizations.md).
This follow-up must qualify its own compiler changes and newly generated products.

## Outcome and boundary

Known direct calls selected by a typed intrinsic rule should emit their BCL
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

The closed intrinsic registry will cover ordinary native/LE/BE fixed-width
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
| Native-backed failing direct-call/conversion/effect regression | Pending |
| Symbol-bound intrinsic-only implementation and closed registry | Pending |
| Actual caller-body assertions in raw and processed direct/object/split output | Pending |
| Pointer identity, late declarations, shadowing, narrowing/overload conversions | Pending |
| Discarded, void-cast, comma and for initializer/increment expressions | Pending |
| Focused compiler/functional tests and affected Blink/production smoke | Pending |
| Complete 31-unit zstd emission and required selection/caller audit | Pending |
| Native/primitive/ABI/consumer/behavior/lifecycle/interop/upstream/audit matrix | Pending |
| Ordinary source-preserving copied delivery | Pending |
| Matched final-product benchmark against preserved optimized reference | Pending |

Build and timing windows are serialized under shared campaign locks and budgets.
Keep historical failed attempts and separate new acceptance from both the original
port and the adapter optimization. No push is authorized by this campaign.


## Optimized-reference measurements

`artifacts/optimizations/direct-intrinsic-original-benchmark-20261020/report.json`
passes all 29 cases in each of four cells. This measures the existing adapter
implementation before compiler mutation; candidate comparisons will use fresh
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
