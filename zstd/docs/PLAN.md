# Translate Zstandard with DotCC

Status: implementation in progress; qualification is incomplete.
Campaign: `zstd/`. Updated: 2026-10-19.

This campaign adopts [translation baseline v2](../../docs/plans/translation-baseline.md)
and the [shared campaign framework](../../docs/campaigns.md). The user authorized
implementation, with GPT-6.1 Sol high supervision and GPT-6.1 Sol xhigh implementation
agents. Existing SQLite, picotls, MsQuic and QuickJS campaigns supply production
conventions; their results do not qualify this product.

## Outcome and scope

Translate the actual upstream Zstandard library to `TranslatedZstd`. A separate
managed application compresses files to interoperable `.zst` frames and restores
them, checking exact original bytes. The owning API exposes one-shot and streaming
compression/decompression, reusable contexts, dictionary use and dictionary
training. Entropy coders, all compression strategies, long-distance matching,
frame parsing, checksums and dictionary algorithms execute as translated C#.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required | Complete portable, single-threaded current-format libzstd core: common, compress, decompress and dictBuilder sources; ordinary levels including optimal parsing; one-shot and streaming APIs; checksums; concatenated and skippable frames; raw and trained dictionaries; training; errors, reuse and deterministic disposal; file interchange with native same-pin libzstd. |
| Excluded | Worker-thread compression (`ZSTD_MULTITHREAD` undefined), legacy pre-v0.8 frame decoders (`ZSTD_LEGACY_SUPPORT=0`), assembly/intrinsic acceleration, CLI implementation, seekable/contrib libraries, external sequence-producer plugins and benchmark tools. These are optional upstream configurations or separate products. Standard portable algorithms remain required. |
| Later extensions | Independently qualified native worker scheduling, optional contrib/seekable features, other operating systems/architectures and legacy formats. |
| Baseline departures | None for Linux x64 raw/processed × JIT/NativeAOT acceptance. Discovery may use raw/JIT before the final matrix. |

## Decisions and feasibility

| ID | Decision | Choice and reason | Status; evidence/gate |
| --- | --- | --- | --- |
| D1 | Source | Stable v1.5.7 release, verified against the official [release page](https://github.com/facebook/zstd/releases/tag/v1.5.7). URL, revision and checksum belong only in `config/source.json`. Native controls and upstream cases use the same acquired tree. Preserve BSD/GPL notices. | Decided; acquisition/native P0. |
| D2 | Configuration | Explicit JSON/source manifest of every portable standard library C unit in common/compress/decompress/dictBuilder. Upstream supported portable single-thread settings, no algorithm substitutions; C99/C11 as accepted by upstream and DotCC. Source preparation stages copies and preserves usable `ref/`. | Decided profile; manifest/config inspection P0. |
| D3 | ABI | Linux x64 little-endian, LP64, 64-bit `size_t`/pointers, 32-bit `unsigned` and enum. Actual emitted probes compare upstream stream structures, offsets, widths and boundary constants to native observations. Binary data remains bytes; error names are upstream ASCII. | Decided target; actual ABI P1. |
| D4 | Host/dependencies | Existing DotCC C runtime supplies allocation, memory and standard C services. BCL provides managed file/Stream I/O and API ownership. Delivered code may not import native libzstd, spawn a zstd subprocess or use another compression engine. Native executables are controls only. | Decided; import/dependency audit P2/P6. |
| D5 | State/lifetime | Each compressor/decompressor owns its upstream context. Concurrent independent instances are required; an individual context is not shared concurrently. Dictionaries are copied or have explicit retained ownership. Pinned span addresses do not escape calls. Disposal is idempotent and later calls fail clearly. | Decided; lifecycle tests P1/P3/P4. |
| D6 | Delivery | Object emission/link, class `Zstd`, namespace `Managed.Zstd.Native`; raw and processed products use shared transactional publication. Shared `link_flags` supplies 256 KB splitting, literal pooling and inline deduplication. Authored API lives in `src/Managed.Zstd`, referenced normally by consumer through `ZstdProject`. | Decided; P2/P5. |
| D7 | Controls | Build the exact selected upstream sources natively with assertions enabled, bounded compilation and a deliberate failing assertion control. Compare bytes/status/ABI exactly where deterministic; cross-decode native and translated frames and dictionaries. Native oracle has no delivered runtime role. | Decided; P0/P4. |
| D8 | Matrix | Linux x64, default portable single-thread profile, raw and processed libraries, JIT and executable NativeAOT. Normal trimmed owning consumer plus whole-library-rooted AOT harness serve distinct gates. | Decided; P4/P5/P6. |
| D9 | Validation | Meaningful fixed corpus, all selected strategies, streaming tiny chunks/flush/end, dictionary training/use and mismatch, malformed/truncated/checksum frames, lifecycle/reuse and independent contexts. Retain upstream case inventory and actual case outcomes. Long fuzzing, performance targets, allocation-failure injection and extended soak are excluded from this initial contract. | Decided; tests P4. |

Default provenance hashes are `warn`; `off` must work; strict archive comparison
is optional. Exact adaptations require unique local context in all modes. A
compiler failure is repaired structurally in shared code with native-backed
regression and a full selected-source retry. No generated C# edits, hidden stubs,
handwritten replacement compression engine or pruning of required algorithms.

## Source, host and ownership

The source manifest is checked against upstream library build inputs. Units only
excluded by the chosen upstream profile are documented in the source inventory.
Any authored C ABI shim contains boundary normalization/probes only, is itself
translated, and calls the real upstream library. It must not implement a codec.

| Boundary | Upstream responsibility | Managed/runtime responsibility | Validation |
| --- | --- | --- | --- |
| Allocation | Context/dictionary allocation and freeing through standard libc | Existing DotCC allocator; API owns returned handles | Create/use/reset/free, idempotent Dispose, post-disposal rejection and independent owners. |
| Binary buffers | Read/write bounds, frame and dictionary parsing | Pin buffers for each direct invocation; enforce managed argument bounds | Empty/incompressible input, tiny output, malformed/truncated data, actual native/emitted ABI. |
| Streaming | Incremental state, consumption/production and upstream result/error | Stream loops preserve consumed positions and detect truncation/no progress | Tiny input/output chunks, flush/end, concatenated/skippable frames and native exchange. |
| Dictionary | Training, format, IDs and compression/decompression behavior | Retain/copy bytes for exactly the required lifetime | Trained/raw dictionaries, wrong/missing dictionary errors and native exchange. |
| Files | No file services in selected codec core | Separate consumer opens/closes files and writes ordinary `.zst` bytes | Restore exact source bytes through ordinary project references, JIT and AOT. |

Highest risks are actual compiler handling of optimized portable entropy/match
code and static tables, cross-unit symbol/type identity, buffer struct ABI, and
NativeAOT reachability of the full linked library. First full-source emission and
native controls resolve the initial risks; hypotheses are not blockers until
observed. Keep compiler changes disjoint from unrelated dirty EDG/C++/duckdb/libuv
work and use shared production command locks. Emission jobs are bounded, native
builds at most two jobs and managed builds one node.

## Deliverables and framework

- Recipe and shell wrappers: `scripts/campaign.py` and shared dispatch wrappers.
- Metadata and explicit source/profile manifest: `config/`.
- Products: `generated/TranslatedZstd/TranslatedZstd.csproj` and
  `generated/TranslatedZstd.Raw/TranslatedZstd.csproj`.
- Owning API: `src/Managed.Zstd/Managed.Zstd.csproj`.
- Separate application: `samples/ManagedConsumer/ManagedConsumer.csproj`.
- Root solution: `ManagedConsumer.slnx` includes original API and sample projects.
- Native control: `tests/NativeOracle/`; managed qualification: `tests/ManagedTests/`.
- Current evidence: [validation.md](validation.md), [blockers.md](blockers.md).

Scripts use shared Python/Bash resolution and ordinary portable copies. .NET 10,
Python 3.11+, a C toolchain and Linux x64 NativeAOT prerequisites are required for
qualification. Native build/test preparation belongs to suites, not the normal
translation source-to-product pipeline. Other platforms remain unrun.

## Milestones

Each significant validated milestone is committed with focused paths; no push.
Never stage unrelated modifications. Baseline workspace was already dirty in
`docs/plans/summary.md`, EDG/C++, duckdb and libuv before this campaign.

| Phase | Exit gate | Status | Evidence |
| --- | --- | --- | --- |
| P0 | Frozen scope/manifest; shared recipe/layout; working native controls; first real translation attempt. | Passed | Native 11-case control and current-tool full-source failure receipt in validation ledger. |
| P1 | Actual emitted/native ABI and context/dictionary ownership probes. | Planned | Required before boundary acceptance. |
| P2 | Every selected unit emits/links; raw and processed builds; canonical transactional products; required imports audited. | Planned | No generated repairs permitted. |
| P3 | Actual translated one-shot, streaming and dictionary workflows with clean ownership. | Planned | Harness and consumer outputs. |
| P4 | Required feature/error/lifetime/upstream/interoperability rows pass all four matrix cells. | Planned | Case-level reports; excluded upstream scope explicit. |
| P5 | Root solution builds; owning API and separate consumer run meaningful file workflows, JIT/AOT. | Planned | Ordinary project references, no native backend. |
| P6 | Regeneration with final tools, current full matrix, rooted AOT, dependency audit, affected shared regressions and reproduction. | Planned | Required unrun gates keep acceptance open. |

P0 → P1 → P2 → P3 → P4 → P5 → P6; native/API/test authoring proceeds in
parallel, but runtime acceptance always uses the actual translated implementation.

## Validation selection

| Gate | Suite/assertion | Required matrix |
| --- | --- | --- |
| Native reference | `native`: deliberate failing control; exact-pin ABI, codec, streaming, dictionary, training and error cases | Native portable Linux x64 |
| ABI/full-library | `abi`: actual emitted/native widths/offsets/constants plus rooted library compilation | Raw/processed JIT/AOT |
| Useful application | `consumer`: write compressed file, restore and compare original bytes with ordinary owning API | Raw/processed JIT/AOT |
| Required behavior | `behavior`: corpus/levels, stream boundaries/flush, dictionary training/use and file interchange | Raw/processed JIT/AOT |
| Lifetime/errors | `lifecycle`: repeat/reuse/dispose, independent contexts and ordinary invalid-input outcomes | Raw/processed JIT/AOT |
| Upstream coverage | `upstream`: selected actual upstream cases and transparent case/exclusion inventory | Raw/processed JIT/AOT |
| Delivery imports | `audit`: required source/symbol closure, forbidden imports, provenance and licenses | Both forms/current artifacts |
| Shared repairs | Focused compiler tests and native-backed functional execution; affected downstream campaigns chosen by actual repaired behavior | Triggered when shared code changes |

Default suite: `consumer`. Intended verification suites: `native`, `abi`,
`consumer`, `behavior`, `lifecycle`, `upstream`, `audit`; final recipe is
authoritative. Preserve original transcripts and exact case counts. Byte arrays,
statuses and layouts compare exactly; nondeterministic training optimizations
must be compared by documented format/roundtrip invariants if bytes legitimately
differ. No performance acceptance threshold is selected.

## Reproduction and acceptance

```bash
bash Scripts/campaign.sh layout zstd --check
bash zstd/scripts/fetch.sh --fetch missing
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/translate.sh --fetch never --jobs 2
dotnet build zstd/ManagedConsumer.slnx -c Release -m:1
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --fetch never --rid linux-x64 --jobs 2
```

Also validate `--hashes off`, usable local input with `--fetch never`, invocation
from an unrelated working directory, clean private output regeneration and
authored-file preservation. NuGet restore is controlled separately by `--restore`.
Framework receipts distinguish published build products from later qualification
failures. The final report must identify every blocked/unrun required gate and
excluded target honestly. Completion requires the complete selected scope, useful
workflow, current matrix and reproducible delivery evidence.
