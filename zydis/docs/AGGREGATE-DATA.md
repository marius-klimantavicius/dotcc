# Aggregate byte-data emission and comparison profiles

Current policy: the Zydis campaign's `default` profile uses `--aggregate-data`.
`aggregate-data` remains an explicit alias for the same optimized emission policy;
`typed` selects the unoptimized typed-initializer comparison. **The optimized
canonical default passed fresh full qualification on Linux x64, 2026-10-27.**
See [current acceptance and receipt](ACCEPTANCE.md).

Earlier typed-default and optional aggregate-data runs **passed on Linux x64,
2026-10-27**, including all 18 product units, raw/processed × JIT/NativeAOT,
rooted AOT, managed boundaries, consumers and audit. Paired measurements and exact
native table-byte comparisons passed. Those receipts and benchmark labels retain
their original meanings: historical `default`/`Default` means typed initialization;
historical `aggregate-data`/`Aggregate` means byte-data emission. They do not
substitute for the new canonical-default receipt.

## Current profiles and output paths

- `default` is still the first/default recipe profile. It enables
  `--aggregate-data` and publishes the canonical products at
  `generated/TranslatedZydis/TranslatedZydis.csproj` and
  `generated/TranslatedZydis.Raw/TranslatedZydis.csproj`, with qualification
  companions alongside them.
- `aggregate-data` enables the same option and retains its existing alternate
  paths below. It aliases the emission policy, while remaining an independently
  selected, stamped and published profile.
- `typed` omits `--aggregate-data`. Its products and companions have the same
  names under `generated/profiles/typed/`, including
  `generated/profiles/typed/TranslatedZydis/TranslatedZydis.csproj` and
  `generated/profiles/typed/TranslatedZydis.Raw/TranslatedZydis.csproj`.

The retained `aggregate-data` outputs are:

```text
generated/profiles/aggregate-data/TranslatedZydis.Raw/TranslatedZydis.csproj
generated/profiles/aggregate-data/TranslatedZydis/TranslatedZydis.csproj
generated/profiles/aggregate-data/TranslatedZydisQualification.Raw/TranslatedZydisQualification.csproj
generated/profiles/aggregate-data/TranslatedZydisQualification/TranslatedZydisQualification.csproj
```

Both optimized profiles request the option while emitting **every one of the
same 18 product objects** and new test-only objects. The typed profile selects
ordinary initialization for those same objects. The companion reuses the exact
product objects; product link commands do not receive the option because
initializers are already baked into object fragments. Native C flags, upstream
features, public APIs and acceptance gates are identical. Raw/processed form
selection is independent of initializer policy; all profiles require both forms.

The framework stages, postprocesses, builds and transactionally publishes each
profile independently. Product trees have `zydis-emission-profile.json`, and
all companion input identities include the profile/option/1024-byte threshold,
including the default and even under `--hashes off`. Missing or mismatched
identities require regeneration. An old typed-default tree is not a current
optimized default, and copying a companion between profiles is rejected.
`build.sh` only rebuilds existing output; use translate or verify to change
emission policy. The root solution and ordinary sample keep their canonical
default reference. Select an alternate product explicitly with `TranslatedProject`.

## Compiler contract and fallback

CLI usage and the library equivalent are documented in
[the flag reference](../../docs/cli.md#aggregate-byte-data).
The standalone compiler default remains **off**; Zydis explicitly enables the
option in its recipe for `default` and `aggregate-data`. Fully constant aggregate
storage of at least 1024 bytes can use immutable byte spans and bounded copies
into each C object's independently allocated writable storage. Serialization
uses the LP64 little-endian target
layout, including packing, bitfield positions, array strides and zero fill.
This changes initializer representation, not C object identity, allocation,
addresses, public types or callable APIs.

The compiler must prove the whole candidate before emitting payloads. Pointer
members/relocations, runtime values or side effects, volatile/atomic storage,
flexible tails, unproved `constexpr` references, nonfinite floating values,
unsupported scalar representations/layouts and small candidates
keep ordinary initialization. Unsupported cases remain supported to the extent
of the existing typed path; opting in does not make an unsupported C program
valid. The emitted copy path guards little endian and the expected element size.
Payloads are immutable data, never shared mutable C destinations. Final linking
deduplicates identical payload records and preserves their bounded spans.

## Fresh qualification commands

The supervisor builds final tools once, then runs these commands **serially**
from the repository root. Reusing those tools gives all three profiles the same
compiler/postprocessor binaries. Each verification regenerates all objects and
executes the same complete nine suites; no reduced feature profile is used.

```bash
dotnet build dotcc.sln -c Release -m:1 -p:UseLocalLalrCc=false
bash zydis/scripts/verify.sh --profile default --fetch never --tools reuse \
  --restore normal --hashes off --form all --mode all --rid linux-x64 --jobs 4
bash zydis/scripts/verify.sh --profile aggregate-data --fetch never --tools reuse \
  --restore normal --hashes off --form all --mode all --rid linux-x64 --jobs 4
bash zydis/scripts/verify.sh --profile typed --fetch never --tools reuse \
  --restore normal --hashes off --form all --mode all --rid linux-x64 --jobs 4
```

Omitting `--profile` selects the same optimized `default` profile. Retain each
printed `zydis/artifacts/campaign/<run-id>/receipt.json`. The new canonical-default
run is [20261027-095741-6051ba79](../artifacts/campaign/20261027-095741-6051ba79/receipt.json).
Its [acceptance evidence](../artifacts/campaign/20261027-095741-6051ba79/optimized-default-acceptance.json)
records source counts, identities, suites and canonical consumer checks.
Alternate-profile commands above are available for independent qualification;
this default-selection follow-up reran the default matrix.
Required gates remain inventory, native, ABI, qualification, all 385 retained
upstream cases, whole-library rooted companion AOT, delivered-product managed boundaries,
ordinary normally trimmed consumer and dependency audit. Both raw and processed
forms must execute under JIT and NativeAOT. Existing transcript comparisons,
case counts, deliberate failure controls and zero-required-skip rules remain.

The ordinary consumer uses the optimized canonical default without an override.
Use `TranslatedProject` explicitly only when selecting an alternate product, for
example the typed comparison:

```bash
dotnet build zydis/ManagedConsumer.slnx -c Release -m:1
dotnet run --project zydis/samples/ManagedConsumer -c Release
dotnet run --project zydis/samples/ManagedConsumer -c Release \
  -p:TranslatedProject="$PWD/zydis/generated/profiles/typed/TranslatedZydis/TranslatedZydis.csproj"
```

## Source accounting and exact table bytes

The original typed canonical baseline is retained at
`artifacts/aggregate-byte-data/baseline.json` at repository root. Its shared raw
`Zydis.cs` is **61,719,465 bytes**. It contains **109,326 encoder field-store
lines totaling 15,148,770 bytes**. These refer to the shared source file, rather
than the sum of all split function files. A read-only accounting check reproduced
those values, including the file hash; all raw generated C# totaled 62,692,420 bytes.
These are historical measurements, not the expected source size of the promoted
canonical default. The original [source-default-raw.json](../../artifacts/aggregate-byte-data/source-default-raw.json)
describes typed emission, and [source-aggregate-raw.json](../../artifacts/aggregate-byte-data/source-aggregate-raw.json)
describes the then-optional optimized profile. Current accounting uses the typed
alternate and optimized canonical output and writes new reports:

```bash
python3 -B zydis/scripts/measure_aggregate_data.py sources \
  --project zydis/generated/profiles/typed/TranslatedZydis.Raw/TranslatedZydis.csproj \
  --output artifacts/aggregate-byte-data/source-typed-raw-current.json
python3 -B zydis/scripts/measure_aggregate_data.py sources \
  --project zydis/generated/TranslatedZydis.Raw/TranslatedZydis.csproj \
  --output artifacts/aggregate-byte-data/source-default-raw-current.json
```

Reports count source bytes, exact `encoder_instructions` field-store lines/bytes,
constant span payload declarations/logical bytes, aggregate destination byte
counts, and encoder copy calls/bytes. Per-file hashes and original authored-source
links are retained. Logical payload byte counts exclude strings and PE section
padding; they do not imply an equal change in executable size.

The separate `tests/AggregateDataProbe` executable exposes no product API and
adds no cases to ordinary qualification transcripts. Native C includes the real
`EncoderData.h`, links original `EncoderData.c` with the same native profile
flags and dumps the external `encoder_instructions` array. The managed probe
reads **the actual emitted `Zydis.Globals.encoder_instructions` storage** after
initialization. Both assert 7,809 rows × 9 bytes = **70,281 bytes**. Every managed
sample retains its binary dump and must match every native byte, not merely the
hash or observable encoder outputs.

The isolated native helper check agrees with the independently captured
`artifacts/aggregate-byte-data/native-encoder-baseline.bin`: SHA-256
`1545f5fbca1609a73d3a10f140ae7a052a055c7344274f7e7515ab8e205ce974`.
Each paired measurement freshly rebuilds and runs its own native control, so
this historical digest does not block deliberately edited usable source trees.

## Paired compile and initialization measurements

For a new comparison, first pass fresh `typed` and optimized `default` (or
`aggregate-data`) verification with the same tool snapshots. Set `TYPED_RECEIPT`
and `OPTIMIZED_RECEIPT` to those actual receipt paths and choose a **new** output
directory. `--baseline-receipt` takes the unoptimized receipt (now normally
`typed`), and `--optimized-receipt` takes the optimized receipt (now normally
`default`). The historical flags `--default-receipt` and `--aggregate-receipt`
remain aliases for those respective comparison roles. Optimized `default` and
optimized `aggregate-data` cannot serve as an off/on comparison.

```bash
python3 -B zydis/scripts/measure_aggregate_data.py compare \
  --baseline-receipt "$TYPED_RECEIPT" \
  --optimized-receipt "$OPTIMIZED_RECEIPT" \
  --output artifacts/aggregate-byte-data/paired-current \
  --form all --mode all --rid linux-x64 --build-samples 3 --init-samples 5
```

This command runs managed builds and belongs to the supervisor's serialized
build schedule. It does not rebuild tools, translate, publish canonical products
or reuse older measurements. It rejects receipts lacking explicit emission
selection, unequal tool snapshots/SDK/native selection/link flags, missing core
objects, or object flags differing beyond `--aggregate-data`. It checks current
companion input identities even when campaign provenance hashing is off. The
recorded emission selection determines which side is optimized; profile names
alone are insufficient. Original historical receipts remain typed-default and
aggregate-data evidence and must not be relabeled as new profile runs.

Each build sample copies the current generated project to private staging,
relocates authored links, and uses fresh output/intermediate paths. Both profiles
use identical probe source, project settings and build argv, including Release,
optimization, portable symbols, single-node builds, disabled shared compilation,
and full translated-library rooting for NativeAOT. Restore and staging are outside
the timed build/publish interval. Profile order alternates between build samples.
The JSON retains SDK/host/settings, original receipts, commands, logs, all output
binary sizes/hashes, source statistics, samples, median/min/max/mean/deviation,
and optimized-minus-unoptimized differences using the actual receipt profile
names. Historical reports retain their original optimized-minus-default labels.
No performance threshold is an acceptance gate.

Initialization uses a fresh process for every sample and explicitly times
`RuntimeHelpers.RunClassConstructor(typeof(Zydis).TypeHandle)`. It includes
initialization allocations and JIT work inside that call, and excludes table
hashing, dump I/O and process startup. Separate process wall time includes those
costs. OS caches, scheduling, clock resolution and tiering still create noise;
retain individual samples and avoid treating a small median change as a proven
speedup. Whole-library rooted probe binary size is distinct from ordinary trimmed
consumer size; both qualification contracts remain required.

## Historical qualification and evidence before default promotion

The evidence and measurements in this and the following sections were captured
before the campaign default changed. `default` is the original typed profile;
`aggregate-data` is the then-optional byte-data profile. Names, timings, sizes,
receipts and measured/final source identities below are preserved with that
provenance. New default-promotion evidence belongs in [ACCEPTANCE.md](ACCEPTANCE.md).

- [Final default receipt](../artifacts/campaign/20261027-083204-8d2d9de2/receipt.json).
- [Final aggregate-data receipt](../artifacts/campaign/20261027-084057-63b89e8a/receipt.json).
- [Combined validated evidence](../../artifacts/aggregate-byte-data/qualification-final.json).
- [Manual regression commands and logs](../../artifacts/aggregate-byte-data/manual-validation-commands.json).
- [Paired measurements and all commands](../../artifacts/aggregate-byte-data/paired-final/report.json).
- [Measured/final source identity](../../artifacts/aggregate-byte-data/paired-final/final-qualified-source-identity.json).
- [Changed files](AGGREGATE-DATA-CHANGED-FILES.txt).

Each profile passes all nine unchanged suites. Each freshly emits all **18**
product units. Native controls pass **393 rows**, including all **385** retained
upstream cases (210 info, 174 encoder/reencoder and one absolute-address case).
Raw/processed × JIT/NativeAOT pass **1,572 core rows**; whole-library rooted AOT
passes **786** additional rows. Delivered managed boundaries pass **20** rows,
and the ordinary normally trimmed consumer passes **four** workflows. There are
zero failed or skipped required rows. Audits retain the complete public API,
authored links/notices, original input identities and native dependency policy.

The historical final default shared `Zydis.cs` was byte-for-byte identical to the
pre-edit 61,719,465-byte baseline. All 18 then-optional object-emission commands carry
`--aggregate-data`; neither product link applies the flag retroactively.
The optional raw and processed products actually use **18** encoder-table copies
(maximum 4096 bytes each), totaling **70,281 bytes**, with **zero** per-field
encoder-table stores. This is checked from emitted copy sites and payload
records, not inferred from merely passing `--aggregate-data`.

The paired run completed **24 clean builds** and **120 fresh-process executions**.
Every execution compared all **70,281 actual managed C-storage bytes** exactly
with a freshly built native dump; SHA-256 was
`1545f5fbca1609a73d3a10f140ae7a052a055c7344274f7e7515ab8e205ce974`.
The paired receipts are the earlier complete passing runs
`20261027-080206-07734e49` and `20261027-081212-0a4d4249`. Subsequent edge repairs
required final tool requalification. The source-identity receipt proves that
every measured generated C# file and authored API input is byte-identical to
the final qualified products. Benchmark binaries belong to that controlled
paired run; canonical build sizes are not mixed into its comparisons.

## Historical measured output and timings

Generated C# totals include the shared source, all split methods and embedded
runtime. Immutable payload sizes count data bytes, excluding PE padding and
string-pool storage.

| Measurement | Default | Aggregate data |
| --- | ---: | ---: |
| Raw generated C# bytes | 62,692,420 | 6,607,052 |
| Processed generated C# bytes | 62,667,722 | 6,582,354 |
| Raw shared `Zydis.cs` bytes | 61,719,465 | 5,635,597 |
| Encoder field stores | 109,326 | 0 |
| Encoder field-store source bytes | 15,148,770 | 0 |
| Encoder byte-copy calls | 0 | 18 |
| Encoder payload bytes | 0 | 70,281 |
| All immutable primitive/aggregate payload bytes | 3,068 | 399,079 |

Nineteen aggregate initializers serialize 396,173 destination bytes. Separate
C allocations remain; payload deduplication does not pool their addresses.
Relative to that historical typed default, the shared source shrinks by 90.87%
for this input and selection.

The following sizes come from identical paired probe builds. JIT rows describe
`TranslatedZydis.dll`; AOT rows describe the whole-library rooted probe executable.
All three builds in each cell produced the same recorded size.

| Form / mode | Default binary bytes | Aggregate binary bytes |
| --- | ---: | ---: |
| raw / JIT | 12,013,568 | 1,573,888 |
| raw / AOT | 11,581,224 | 5,515,048 |
| processed / JIT | 11,981,824 | 1,542,144 |
| processed / AOT | 11,577,128 | 5,506,856 |

Build/publish and initialization medians are seconds. Each cell has three fresh
builds and 15 fresh-process initialization samples (five per build). Restore,
staging, translation and postprocessing are outside the timed build interval.
Initialization includes allocations and any JIT inside the explicit library
class-constructor call; process startup, table hashing and dump I/O are excluded.

| Form / mode | Default build | Aggregate build | Default init | Aggregate init |
| --- | ---: | ---: | ---: | ---: |
| raw / JIT | 18.744985 | 6.725149 | 0.997974252 | 0.042345721 |
| raw / AOT | 25.596669 | 10.183788 | 0.002037684 | 0.000309781 |
| processed / JIT | 18.342780 | 6.473477 | 0.991921171 | 0.042815551 |
| processed / AOT | 25.568796 | 9.884068 | 0.002133641 | 0.000298403 |

These are measurements on this Linux x64 host, not a general speed guarantee.
The JSON retains every sample, min/max/deviation, binary hash, host/SDK/environment,
common build properties, equivalent object/link flags and tool closure. Cache,
scheduling and clock noise remain, especially for sub-millisecond AOT init.
No performance threshold is an acceptance gate.

## Historical shared regressions, review and repairs

- **3,490 unit tests** pass, including 14 new option/threshold/fallback cases.
- **40 focused execution cases** pass: 36 source/object/state/operator/storage/
  constexpr/RVA rows and four native-backed default fixtures. Each intended
  eligible fixture table positively asserts byte-copy selection.
- **206 affected execution cases** pass across aggregate/array/designator/
  bitfield, initializer, owned storage, object linkage and state/TLS behavior.
  This selection overlaps the focused fixtures; counts are not additive.
- **Nine actual CLI checks** pass, including source/object emission, linking and
  rejection of link-only, mixed-object, preprocessing and WAT enablement.
- **Six Zydis Python tests** pass for profile isolation, identity and measurements.
- **Eight GCC O0/O2 fixture controls** pass with exact transcripts. The special
  C23 wrapped-constant control uses `-Dconstexpr=const` with GCC 13 (which lacks
  that keyword); both optimization levels confirm the arithmetic/storage result.
- The separate context/TLS probe executes successfully under **JIT and NativeAOT**,
  checking byte values, stable addresses, mutability, alignment, repeated locals,
  independent contexts, worker TLS and GC. Its source and commands remain under
  `artifacts/aggregate-byte-data/state-aot/`.
- **Four further NativeAOT programs** pass exact native-backed transcripts:
  standalone aligned/TLS/loop storage off/on, the operator table with byte data,
  and the C23 constexpr fallback. Their [receipt and commands](../../artifacts/aggregate-byte-data/supplemental-aot/report.json)
  retain all emissions, publishes, runs and logs.
- The root solution builds with **zero errors**. Existing warnings remain.

Review findings were resolved with the following repairs and execution retries:

1. Hexadecimal integer constants use their typed IR value before parsing a wide
   decimal carrier. Their original spelling must not cause false fallback.
2. A preexisting shell defect dropped initialization when every global used
   special aligned/TLS storage. Program and library shells now retain constructors
   and helpers without requiring an ordinary globals field. The normal nonempty-
   fields output branch is unchanged. The storage fixture covers option-off too.
3. The legacy C23 `constexpr` cache can retain wide integer intermediates that
   differ from actual emitted object initialization. New byte serialization
   rejects such unproved references and uses the typed initializer. Four direct/
   object off/on executions verify the reproduced wrapped-value case.

The new path does not expand parser syntax. Two fixture spellings were adjusted
to equivalent supported brace/specifier ordering, preserving native transcripts.
The [finding ledger](../../artifacts/aggregate-byte-data/findings.json) retains
reproducers, first failures, causes, fixes and retries. Original native inputs and
generated upstream code were not repaired by hand.

The full framework Python suite reproduces its **unchanged baseline**: 89 pass,
one unrelated Blink metadata-literal failure in `run-getcpu.py`. See
[its final log](../../artifacts/aggregate-byte-data/final-framework-tests.log).
This is not counted as a passing check or a Zydis qualification skip.

Milestones: `f9d2090b7` (aligned-only initialization repair), `5a832b532`
(optional byte-data backend/CLI/API and execution regressions), followed by the
profile/docs/evidence commit. No push or PR was performed.

Qualified scope is Linux x64 LP64 little endian. Unsupported representations,
ambiguous union writes, relocation-bearing objects, effectful values, volatile/
atomic data, nonfinite floats and unproved constexpr caches keep existing
lowering. Legacy Zig packed-bitfield layouts are excluded. The copy path throws
on an incompatible runtime endian/element extent. Big-endian execution, other
architectures/OSes and generalized performance claims are outside this evidence.
