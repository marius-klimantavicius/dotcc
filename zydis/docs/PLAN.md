# Translate Zydis with DotCC

Status: implementation in progress; P0–P6 acceptance remains open.
Campaign: `zydis/`. Updated: 2026-10-27.

This plan adopts [translation baseline v4](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md), using the project template's
decisions, contracts, milestones and acceptance structure. The authorized outcome
is a complete managed full-library Zydis product. No pushes or PRs are authorized.

## Agent orchestration

The supervisor runs `gpt-6.1-sol` high; implementation children run the same model
with explicit `target.options.reasoningEffort=xhigh` through T3 delegation. Live
capabilities and returned configurations were checked. Parallel ownership:

| Package | Owned paths | Acceptance/dependency |
| --- | --- | --- |
| Source/recipe | config, campaign recipe, wrappers, ignored ref | Pinned complete library/Zycore manifest, canonical framework integration |
| Native/qualification | tests, scripts/qualification.py | Native control, translated ABI/callback probes and upstream case inventory |
| Owning API/consumer | src, samples/ManagedConsumer | Real translated decoding/encoding/formatting; ordinary project reference |
| Supervisor | docs, README, integration, shared fixes and builds | Review, full matrix, reproduction, commits |

All agents share the intentionally dirty checkout on branch `sqlite`. Initial
status, HEAD and binary diff are preserved at
`/tmp/zydis-campaign-baseline-20261027/`. Preexisting compiler/CLI/docs/Blink edits,
untracked runtime-selection tests and sandbox-xed remain outside this campaign's
ownership. Only narrowly staged campaign work may enter commits. The supervisor
serializes managed tool/product builds; native agent outputs are private.
Child completion is an integration event and must lead to subsequent work until
P6 passes. Repair compiler/runtime defects structurally and retry the full input.

## Outcome and scope

Translate every source unit of the full default Zydis library, retaining its
decoder tables/state machine, encoder, formatter, disassembler, register and
instruction metadata, segment/address utilities and required Zycore primitives.
Deliver `TranslatedZydis` and an owning API. A separate managed application reads
x86 bytes, decodes instructions/operands, formats Intel and AT&T text, constructs
or converts encoder requests, emits machine code and decodes the result.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required | Full library profile: decoder, encoder, formatter, instruction/register metadata, disassembly, address and NOP utilities; 16/32/64-bit modes; ordinary invalid-input/buffer errors; formatter callbacks and tokens |
| Excluded | Upstream command-line tools/examples as delivered applications; these remain controls/fixtures. Zycore OS/thread/process APIs unused by Zydis are outside dependency closure. No native Zydis backend |
| Later extensions | Additional host OS/architectures, performance campaigns and externally supplied fuzz corpora |
| Baseline departures | None planned. Whole-library rooted qualification and normally trimmed ordinary consumer are distinct gates |

## Decisions and feasibility work

| ID | Decision | Choice and reason | Status/evidence |
| --- | --- | --- | --- |
| D1 | Inputs | Zydis v4.1.1 a2278f1d254e492f6a6b39f6cb5d1f5d515659dc and Zycore gitlink 0b2432ced0884fd152b471d97ecf0258ff4d859f; metadata config/inputs.json, 636 regular-file entries in source-inventory.json | Decided; P0 inventory reviewed |
| D2 | Closure/configuration | All 18 upstream C11 library units, 10 required Zycore headers and zero out-of-line product Zycore units; explicit x64/Linux static-build flags and standard libc profile in translation.json; full AVX512/KNC/segment features | Decided; P0 manifest/native controls |
| D3 | ABI/encoding | Initial executed target Linux x64, LP64 little endian; fixed-width instruction bytes and ASCII formatter text; native C ABI controls compared to actual emitted storage | Decided; P1 verification |
| D4 | Dependencies/host | Shared DotCC C runtime and managed BCL only for product; translated required Zycore source. Native compiler/CMake only for controls. No native disassembly/encoding imports | Decided; P2 import audit |
| D5 | State/lifecycle | Caller-owned decoder/formatter/request structs; immutable library tables; owning facade copies outputs and pins buffers only for synchronous calls. Independent calls may run concurrently; no blocking service/cancellation protocol | Decided; P3/P4 probes |
| D6 | Delivery | Class Zydis, namespace Managed.Zydis, product TranslatedZydis. Authored API linked from src; shared link_flags 256 KB splitting/literal pool/inline deduplication | Decided; P2 |
| D7 | Controls | Exact-source native library and asserted C probes; applicable upstream cases inventoried and exercised through translated implementation | Decided; P0/P4 |
| D8 | Execution | Linux x64 default profile, raw/processed × JIT/NativeAOT execution. Root full low-level library in qualification; ordinary consumer trims normally | Decided; P4–P6 |
| D9 | Testing | Native/emitted ABI, callbacks/context, full feature families, upstream fixtures, errors, repeated/concurrent owning calls. No custom prolonged stress/fuzz/performance requirement | Decided; P4 |

Provenance defaults to `--hashes warn`; `off` is supported. Strict hashing is an
optional audit. Existing usable edited trees are preserved and identified as
observed inputs. Exact adaptations, if required, need unique local guards in all
modes and their own ledger entries. No generated C# patching is permitted.

## Source, host and ownership contracts

The config inventory will reconcile selected sources to the pinned upstream
CMake definition and separately identify test tools/fixtures, generated tables,
licenses and unused Zycore services. Preparation stages copies and preserves ref.

| Boundary | Translated responsibility | Host/reused dependency | Ownership/errors/validation |
| --- | --- | --- | --- |
| Instruction buffers | Decode/encode semantics and validation | Pinned spans/caller storage | Borrowed only during synchronous calls; results copied; short/invalid buffers tested |
| Formatting | Syntax, tokenization, properties and dispatch | Managed callback bridge where needed | Rooted context/delegates for call lifetime; exception containment; actual callback probes |
| Struct layout | Upstream nested structs/unions/bitfields | Shared emitted C representation | Native sizes/alignment/offsets compared with actual emitted types and C accessors |
| Allocation/memory | Required Zycore string/vector behavior | Shared runtime memory primitives | Correct ownership/pairing if selected closure allocates; Zydis core itself is allocation-free |
| Lifetime/concurrency | Local decoder/formatter/request state and immutable tables | Owning API | No hidden subprocess/global serialization; repeat and concurrent calls validated |

No source adaptations are preauthorized as a defect workaround. Prefer upstream
feature macros, existing extension points and typed host bindings. Any needed
adaptation is recorded with reason, guards and native/managed checks. Upstream
algorithm replacement is prohibited.

| Risk | Probe | Observable gate |
| --- | --- | --- |
| Large generated lookup tables and nested union initializers | Full source translation plus native decode/encode corpus | Correct emitted initialization; P1/P2/P4 |
| Packed/bitfield/union layouts | Actual translated ABI/accessor probe | Exact relevant native layout/storage values; P1 |
| Formatter function pointers and context | Install/invoke/restore callbacks and token traversal | Exact text/status and callback/context identity; P1/P4 |
| Encoder paths/relative addresses | Decode-to-request, encode/absolute encode and NOP | Native bytes/status plus roundtrip; P3/P4 |
| NativeAOT closure | Rooted library harness and ordinary consumer | Both raw/processed execute; P4/P5 |

## Framework integration and deliverables

Recipe: `scripts/campaign.py`, exporting `Recipe("zydis", ...)`. Default profile
is `default`. Canonical products are
`generated/TranslatedZydis/TranslatedZydis.csproj` and
`generated/TranslatedZydis.Raw/TranslatedZydis.csproj`. The root solution is
`ManagedConsumer.slnx`; separate sample is `samples/ManagedConsumer`, selecting
the library with `TranslatedProject`. Original API sources remain under `src`.
Fixed probes and test entrypoints stay under `tests`, outside the product.

Shared wrappers/framework own acquisition, process/log execution, private tool
snapshots, raw/processed builds, semantic postprocessing and publication. No
project-name branch in the framework is planned. Scripts use portable ordinary
copies and shared Python resolution. Windows orchestration requires no symlinks,
Developer Mode or elevation; Linux-native controls are qualified on Linux only.
Prerequisites: Python 3.11+, .NET 10 SDK/NativeAOT toolchain and C compiler/CMake
for native controls. NuGet restore is separate from `--fetch never`.

## Milestones and evidence

Commit significant milestones after their required checks pass; stage only
campaign-owned hunks/files. Never include preexisting changes. No push/PR.

| Phase | Exit gate | Status | Evidence |
| --- | --- | --- | --- |
| P0 | Pinned closure, recipe/layout, asserted native control and first full translation attempt | Passed | config/recipe-checks.json; native controls-05 393/393; canonical 18-unit emit/link; ledger receipts |
| P1 | Actual emitted/native ABI, table storage and callback ownership probes | Passed | Four ABI/feature cells plus rooted AOT companion; direct CLR storage, 46 ABI observations; 20261027-071458-a957ffa0 |
| P2 | Every selected unit emits/links; raw and processed canonical builds; import audit | Passed | All 18 units, four product/companion forms; 43-function/66-type final qualified audit |
| P3 | Useful owning decode/format/encode workflow with clean lifetimes | Passed | Four ordinary consumer cells; exact errors/ownership and concurrent workload |
| P4 | Full feature/error cases and applicable upstream inventory across required matrix | Passed | 4 ABI + 28 feature groups + 1540 upstream cases; rooted AOT adds 786 declared cases; 20 delivered boundary cases; zero failures/skips |
| P5 | Root solution and ordinary project-reference consumer JIT/AOT execution | Passed | Root solution Release build and four ordinary trimmed consumer cells; 20261027-071458-a957ffa0 |
| P6 | Final-tool full regeneration, matrix rerun, affected regressions, reproduction and dependency audit | Planned | Pending |

Dependencies: P0 → P1 → P2 → P3 → P4/P5 → P6. API design/native harness proceed
in parallel with source acquisition. The [implementation ledger](IMPLEMENTATION.md)
records defects, commands, receipts and milestone commits.

## Validation selection

| Behavior | Planned suite/assertion | Matrix | Requirement |
| --- | --- | --- | --- |
| Native control | native; assertion failure control plus full feature cases | Linux x64 native | Required |
| ABI/callback | abi/boundary; native vs actual emitted observations | All four cells | Required |
| Useful workflow | consumer; owning decode/format/encode assertions | All four cells | Required |
| Feature/error/lifetime | qualification; statuses, bytes, text, ownership/concurrency | All four cells | Required |
| Upstream cases | upstream; pinned fixture inventory and case-level counts | All four cells | Required |
| Rooted closure | qualification harness roots full translated assembly | JIT and AOT, both forms | Required |
| Dependency audit | audit; imports/project refs/resources | Both forms | Required |
| Shared regression | Focused compiler/functional regressions for observed fixes, relevant downstream campaigns | According to affected behavior | Required when shared code changes |

Default suite: consumer. Verification suites: inventory, native, abi,
qualification, upstream, rooted-aot, managed-boundary, consumer and audit.
Compare deterministic statuses/bytes/text/layouts
exactly. Mask only documented undefined/pointer-address observations; retain raw
transcripts. Keep exclusions/unrun cases distinct from passes. No performance
thresholds or independent protocol peers apply to this instruction library.

## Reproduction commands and acceptance

```bash
bash Scripts/campaign.sh layout zydis --write
bash Scripts/campaign.sh layout zydis --check
bash zydis/scripts/fetch.sh --fetch missing
bash zydis/scripts/translate.sh --fetch never
dotnet build zydis/ManagedConsumer.slnx -c Release -m:1
bash zydis/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash zydis/scripts/verify.sh --fetch never --rid linux-x64 --form all --mode all
```

Acceptance includes a second full generation into private campaign outputs,
unrelated-working-directory invocation, preservation of authored inputs,
offline source reuse and warn/off provenance behavior. Ordinary scripts must
not delete unrelated files. Current receipts are under `artifacts/campaign`,
with specialist reports attached by the recipe. P0 passes; remaining unexecuted
rows remain open. Additional OS/architecture and prolonged fuzz/
performance work are optional future qualification.
