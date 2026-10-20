# Zstandard primitive replacements

Status: optimized `portable` profile qualified on Linux x64, 2026-10-20.
The historical original port acceptance remains separate in
[validation.md](validation.md); the replacement acceptance below uses fresh
translation, differential tests, full qualification and final-product timing.

This follow-up uses DotCC's [typed function overrides](../../docs/function-overrides.md)
to replace small memory and integer primitives with authored aggressively inlined
.NET adapters. Compression, decompression, dictionaries, training and hashing
algorithms remain upstream C translated through DotCC. Generated C# is not edited.
No volatile, atomic, allocator or ownership operation is replaced.

## Preserved reference

Before any source/configuration mutation or regeneration, the clean committed
`00373046c` tree and verified products were copied to the read-only ordinary-copy
snapshot `artifacts/optimizations/baseline-00373046c-20261020T021419Z`.
Its `identity.json` and `manifest.json` record original HEAD, source/configuration,
full selected tool closures, upstream source/archive, both original twelve-source
products, original owning API and root build metadata, accepted receipt
`20261019-204241-00db2fbc` and compiled verification artifacts.
All 5440 files (1,139,351,937 bytes) were rehashed before making them read-only;
there are no symlinks. The original remains available for repeated comparisons.

The default `portable` profile uses the optimized primitives with the same API
and canonical product paths. The explicit `original` profile regenerates unchanged
C primitive bodies as an opt-out/reference. Its narrower executed qualification
scope is recorded separately below. There is no additional optimized alias.

## Replacement boundary and contracts

The baseline object-definition inventory finds 42 selected function identities:
27 MEM helpers in 24 of 31 units, six bit helpers in 23 units, and nine xxhash
helpers only in `xxhash.c`. Typed per-unit profiles must preserve original
signatures, internal linkage and physical declaration/TU identity. Their actual
selection reports must match the expected inventory exactly, including signatures
and managed targets; missing or unexpected selections fail the translation.

| Family | Managed operation | Required contract |
| --- | --- | --- |
| Native reads/writes 16/32/64 | `Unsafe.ReadUnaligned` / `WriteUnaligned` | Exactly the named ordinary memory width, all alignments; original `const void*` / `void*` signatures. |
| Native size_t read; LE/BE read/write | 64-bit load/store or endian operation | Selected translated Linux LP64 `size_t` is `ulong`; never infer it from managed `IntPtr.Size`. |
| LE/BE reads/writes | `BinaryPrimitives` | Exact byte order and width, including reads after intervening writes. |
| LE24 | Three-byte implementation | Exactly three readable/writable bytes; never a masked four-byte access. |
| 32/64/size_t byte swaps | `BinaryPrimitives.ReverseEndianness` | Preserve unsigned bit patterns and C widths, including zero and high-bit values. |
| Endian detection | `BitConverter.IsLittleEndian` | Runtime byte-order fact without constructing a union probe. |
| Leading/trailing zero counts | `BitOperations` | Preserve the upstream nonzero input domain; zero builtin behavior is not a native oracle. |
| High bit / common bytes | Leading/trailing count and byte scaling | Preserve unsigned return and the nonzero domain, selected LP64 and endian convention. |
| xxhash read/swap/endian helpers | Same fixed-width primitives | Preserve exact original signatures and memory effects; no hashing algorithm substitution. |

`MEM_32bits`/`MEM_64bits` already fold and are excluded. Fallback subroutines need
no separate override when their public helper is replaced. Rotations, including
xxhash rotation macros, remain excluded because existing
shift idioms may already lower efficiently; they require measured evidence before
adding another replacement. Algorithms and optional ISA/profile decisions remain
unchanged, including `ZSTD_TRACE=0`.

## Measurement and acceptance gates

Exact names, C signatures and managed targets are in
[primitive-overrides.json](../config/primitive-overrides.json); the required
per-unit closure is in [primitive-inventory.json](../config/primitive-inventory.json).
The only adapter source is [Primitives.cs](../src/Optimization/Primitives.cs),
linked from its authored path into both generated projects and available during
semantic postprocessing, ordinary builds and NativeAOT.

The benchmark uses one identical authored harness and fixed corpus/settings for
preserved-original and candidate products, with fresh isolated build paths.
Real codec cases cover text at 4 KiB/128 KiB/1 MiB and levels 1/3/9, mixed and
incompressible data, level 19, streaming and trained dictionaries. Context-reuse
throughput is reported separately from an allocating convenience roundtrip.
Training/setup and full byte validation occur outside timing. Each process
records output sizes/digests, allocation and GC observations, machine/runtime/tool
identity and harness hash. Matched JIT policy disables tiered compilation;
warmup is at least 200 ms and three calls, followed by five calibrated samples
of at least 100 ms. Report medians and dispersion, and alternate original/candidate
process order where practical. These controlled workload results are not a
universal throughput claim.

| Gate | State |
| --- | --- |
| Untouched source/tool/product reference preserved | Passed |
| Baseline numerical measurements with identical reusable harness | Passed: 29 cases in each raw/processed JIT/AOT cell |
| Every selected primitive compared with original/native C | Passed: 227486 checks, all eight original/candidate form/mode cells |
| All unaligned offsets, endian patterns, exact-width guards and LE24 inaccessible boundary | Passed, including intentional inaccessible-page fault controls |
| Direct/function-pointer calls, single evaluation and memory read-after-write | Passed |
| Full 31-unit regeneration and exact typed-selection closure | Passed: 795 required selections, 42 unique targets |
| Measured candidate improvement/regression review, JIT and NativeAOT | Passed: all 116 final-product medians improve, minimum 1.317× |
| Raw/processed JIT/NativeAOT native/ABI/consumer/behavior/lifecycle/interop/upstream/audit | Passed in final receipt 20261020-053617-98836bfa |
| Newly linked authored adapter source preserved and clean ordinary delivery rebuilt | Passed: final source hash preserved; 47-file copied closure builds/runs both forms |

Each observed failure is classified before repair. Shared compiler changes, if
needed, require native-backed regressions and affected downstream checks. Pure
campaign replacements do not trigger unrelated blanket regression runs. Final
results, commands, selections, limitations and focused commit IDs follow below.

## Untouched original measurements

The first complete original run is
`artifacts/optimizations/original-benchmark-20261020/report.json`; all 29 cases
pass in all four cells. Its source/binary hashes and complete sample/dispersion
records remain available. Host: AMD Ryzen 9 7950X, Linux 6.8 x64, pinned CPU 0;
.NET information is retained in the report/logs. Values below are MiB/s and
measure the stated local workloads; candidate comparisons use fresh alternating
original/candidate runs rather than comparing only against these earlier numbers.

| Workload | Raw JIT | Raw AOT | Processed JIT | Processed AOT |
| --- | ---: | ---: | ---: | ---: |
| 128 KiB text compression, level 3 | 141.20 | 150.54 | 157.96 | 165.76 |
| 128 KiB text decompression, level 3 | 408.68 | 407.93 | 422.92 | 429.74 |
| 1 MiB streaming compression | 143.06 | 150.18 | 160.15 | 166.21 |
| 1 MiB streaming decompression | 410.46 | 417.88 | 422.51 | 431.72 |
| Allocating 128 KiB roundtrip | 95.04 | 99.12 | 103.36 | 107.59 |

Context-reuse rows allocate zero managed bytes per operation on the benchmark
thread. The original allocating raw JIT roundtrip uses approximately 544561
managed bytes per operation. This metric excludes native heap traffic and peak
resident memory. The separate convenience case must not be treated as a single
compression or decompression throughput result.

Reproduce the original run with the preserved snapshot and a fresh output path:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py --baseline zstd/artifacts/optimizations/baseline-00373046c-20261020T021419Z --output zstd/artifacts/optimizations/original-repeat --variant original
```

The exact typed candidate first full-source receipt is
`artifacts/campaign/20261020-052319-79188bd9/receipt.json`: all 31 units emit/link,
raw and processed semantic/final builds pass, with exactly 795 selections
(24×27 MEM + 23×6 bit + nine xxhash). Primitive receipt
`artifacts/campaign/20261020-052446-a1206c88/receipt.json` passes original and
candidate raw/processed JIT/NativeAOT: 227466 native-matching checks each,
digest `11d3aca8d139d10a`, real three-byte guard boundary and required fault
controls. Nine malformed selection reports fail closed. An unrelated header
comment edit also permits actual typed emission in warn/off modes while strict
explicitly rejects historical hash drift; hashes are provenance, not an ordinary
compatibility allowlist.

The native primitive fixture deliberately selects the exact portable upstream
helper definitions after including system headers. Native GCC otherwise omits
an addressable endian helper or macro-aliases byte swaps to builtins; the first
harness build exposed this in `20261020-052220`. Only the test fixture adjusts
those compiler-detection macros. Delivered upstream sources and feature flags
remain unchanged. Zero/all-one memory and swap cases are separate from the
nonzero-only bit-scan contract. The fresh full matrix and exact final-product
measurements below provide optimized-default acceptance.

## Qualified implementation and delivery

The final portable verification receipt
`artifacts/campaign/20261020-053617-98836bfa/receipt.json` passes all nine declared
suites. Its complete 31-unit translation selects exactly 795 required typed
replacements. Raw/processed JIT/NativeAOT each pass ABI3, behavior30, lifecycle7,
interop6 plus exact native comparisons, the ordinary file consumer, 33 selected
original upstream cases and the import/dependency/license/rooted-AOT audit.
The upstream context-size observation remains native/emitted 582672.

The final primitive fixture includes explicit zero and all-one scalar/buffer
patterns where valid, including the full 64-bit all-one swaps. Native output is
227486 checks, digest `9b3f85183c30d739`, guard3 true, exactly matched in all eight
original/candidate raw/processed JIT/AOT cells. The earlier 227466-check receipt
predates the additional all-one swap cases; its history is not relabeled.

An ordinary solution build and exact copied delivery also pass. Reports are in
the same final receipt directory under `root-solution/report.json` and
`copied-delivery/report.json`. The latter copies 47 source/project/notice/build
metadata files, verifies relative links resolve entirely inside the copied tree,
builds both forms and runs both four-file archive workflows. Every copied file
remains byte-identical; no original workspace source/reference escapes the tree.
Authored adapter source also matches its pre-translation hash after semantic
postprocessing. These current regeneration and ordinary copied-delivery checks
close the relevant source-preservation/relocation gate without repeating another
full portable regeneration.

The first paired comparison passes all 116 cases with faster medians and exact
frame hashes, but its generated product precedes final regeneration. A strict
attempt to bind those measurements by changing only internal `__unit_<64hex>`
suffixes fails: staging identity also changes member order and encoded owner
aliases. That failed proof is retained, not broadened into an equivalence claim.
A second bounded paired comparison therefore measures the final published
products exactly. No codec, adapter or benchmark workload changed. The final
measurements below close the generated-source provenance boundary without
claiming that reordered generated code is byte-equivalent.


## Final paired measurements

`artifacts/optimizations/final-candidate-benchmark-20261020/report.json` passes
all 29 workloads in each of four cells: 116 exact comparisons, with ten measured
batches per variant and case. Every input and compressed frame matches the
preserved original exactly. Final source/project/adapter/configuration hashes,
verification receipt hash and unchanged compiler/postprocessor closures are
retained in `product-provenance.json` beside the report.
`final-product-binding.json` passes exact before/after hashes without normalization;
all 5440 original snapshot files were rehashed unchanged and remain read-only. Implementation commit
`b322ebdd5` was frozen throughout timing; the published portable products remain
unchanged afterward. This report supersedes the earlier candidate timing for
acceptance.

All measured medians improve. The slowest case in every cell is shown so the
result does not depend on selecting only the largest gains:

| Cell | Minimum speedup | Workload | Maximum speedup |
| --- | ---: | --- | ---: |
| Raw JIT | 1.417× | 1 MiB streaming decode | 7.296× |
| Raw NativeAOT | 1.431× | 4 KiB dictionary decode | 4.973× |
| Processed JIT | 1.373× | 4 KiB dictionary decode | 7.873× |
| Processed NativeAOT | 1.317× | 4 KiB dictionary decode | 5.038× |

Representative 128 KiB text, level 3 throughput is in MiB/s. Original and
optimized values come from the same alternating final run:

| Cell | Encode original → optimized | Encode speedup | Decode original → optimized | Decode speedup |
| --- | ---: | ---: | ---: | ---: |
| Raw JIT | 142.51 → 549.17 | 3.854× | 405.61 → 605.76 | 1.493× |
| Raw NativeAOT | 149.59 → 564.43 | 3.773× | 414.03 → 617.01 | 1.490× |
| Processed JIT | 161.80 → 694.86 | 4.295× | 419.43 → 605.17 | 1.443× |
| Processed NativeAOT | 166.20 → 647.21 | 3.894× | 428.46 → 599.31 | 1.399× |

| Additional workload speedup | Raw JIT | Raw NativeAOT | Processed JIT | Processed NativeAOT |
| --- | ---: | ---: | ---: | ---: |
| 1 MiB streaming encode | 3.141× | 3.037× | 3.237× | 2.964× |
| 1 MiB streaming decode | 1.417× | 1.474× | 1.401× | 1.343× |
| 4 KiB dictionary encode | 2.850× | 2.595× | 3.239× | 3.166× |
| 4 KiB dictionary decode | 1.451× | 1.431× | 1.373× | 1.317× |
| Allocating 128 KiB roundtrip | 2.287× | 2.184× | 2.227× | 2.116× |

All 112 context-reuse and streaming comparisons allocate zero managed bytes on
the measured thread for both variants. The allocating roundtrip remains approximately
544–545 kB per operation; small fractional per-operation differences are retained
in the report. Allocation does not include native heap traffic or peak RSS.
Medians, quartiles, MAD, min/max and every batch remain in the JSON; the complete
`comparison-cases.md`/`.csv` table includes all 116 pairs and both variants' IQRs;
`comparison-summary.md`/`.json` contains selected medians and IQRs. These results measure the
combined replacement profile on this host, not an individual primitive's isolated
contribution or an unmeasured target.

Reproduce with fresh output paths and the preserved original snapshot. Replace
`NEW_RUN` with the successful verification receipt directory printed by the first
command; measure and copy those newly published products before regenerating again:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/benchmarks/run.py --baseline zstd/artifacts/optimizations/baseline-00373046c-20261020T021419Z --output zstd/artifacts/optimizations/final-repeat --variant candidate --compare-to zstd/artifacts/optimizations/original-benchmark-20261020/report.json
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/tests/delivery.py --receipt zstd/artifacts/campaign/NEW_RUN/receipt.json --output zstd/artifacts/optimizations/delivery-repeat
```

The benchmark command uses the currently published portable products. If verifying
again regenerates them, measure that newly generated product afterward rather
than attributing an earlier product's timing to it. The runner and harness are
unchanged from baseline milestone `32085415a`; shared compiler/runtime sources
were not modified by this optimization campaign.


The stronger copied-delivery rerun is
`artifacts/copied-delivery-receipt-bound-20261020/report.json`, using the same final
qualification receipt. It again builds and runs both forms. Of 47 copied files,
seven authored files match recorded production-input hashes and 34 generated
files match recorded output hashes. Six files absent from historical
`production_inputs` are explicitly labeled unbound: root Build/Packages props,
NuGet config, solution, and consumer project/source. Their copied bytes and
all evaluated references are still checked. The helper rejects stale recorded
adapter or root metadata digests before copying; four private matching/stale/
absent-input controls pass in
`artifacts/delivery-receipt-control-final-20261020/report.json`. This does not
invent historical hashes for files the receipt did not record.


## Original opt-out profile scope

`artifacts/campaign/20261020-055712-5bb14ef6/receipt.json` separately passes full
31-unit emission, raw/processed builds, three ABI cases in each JIT cell, and both
ordinary four-file streaming/dictionary archive consumers. There are zero typed
replacements, no adapter link and no managed optimization calls in these
products. They reside below `generated/profiles/original/`, preserving canonical
portable products.

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --profile original --form all --mode jit --suite abi --suite consumer --tools reuse --jobs 2 --timeout 600 --fetch never --hashes warn
```

`artifacts/original-profile-validation-20261020/report.json` retains both output
hash maps, commands and results; all 40 protected portable product/adapter/config/
receipt-pointer hashes remain unchanged afterward. The new `original` profile's
full NativeAOT, behavior/lifecycle/interop/upstream/audit suites were not rerun.
Its original/replaced primitive fixture does cover all four form/mode cells.
Historical original snapshot qualification remains separate, and the default
optimized portable profile has the complete fresh four-cell matrix.

All required gates for the selected optimized scope pass. Other architectures,
operating systems and upstream feature exclusions retain the plan's qualification
limits. Baseline/harness milestone: `32085415a`; typed replacements and full
correctness milestone: `b322ebdd5`. Final evidence and the isolated delivery
receipt-binding repair are committed together after these checks; no push.
