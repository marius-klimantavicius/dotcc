# libghostty-vt implementation ledger

Updated: 2026-10-30. Retained C-route gates are accepted with the explicit upstream-test deferral; see [C-ROUTE-ACCEPTANCE.md](C-ROUTE-ACCEPTANCE.md). Direct Zig remains unqualified.

## Current user scope

On 2026-10-30 the user explicitly deferred upstream tests until the direct Zig
phase. This supersedes earlier ledger entries requiring the 2,622-case matrix
before the C-route completion commit. Those entries are chronological evidence
under their then-current scope, not the current acceptance policy. All case
inventories, specialist suites, partial passes and failed/cancelled attempts
remain retained; deferred rows do not count as passes.

Current C-route verification selects `native`, `consumer`, `qualification`,
`factory-allocation` and `audit` through `config/verification.json`. Native
whole-engine/boundary controls, all four consumer and 24-case qualification
rows (including whole-library AOT), factory matrix, ordinary root solution,
warn/off reproduction/preservation and relevant shared/downstream regressions
remain required. The upstream-only host-I/O probe and render/core suites resume
in the direct Zig phase after the accepted C-route commit.

## Repository baseline

`sqlite` at `d3b1ccd975fd883f4f6b75d4b70fa4c9f112fc81`, preserving all preexisting
tracked/untracked changes. Initial coordinated command:
`dotnet build DotCC/DotCC.csproj -c Release --no-restore -m:1` passes with zero
warnings/errors (`/tmp/libghostty-initial-compiler-build.log`).

Framework baseline: `python3 -B -m unittest discover -s Scripts/campaigns/tests -v`
runs 90 tests, with 89 passing and one preexisting failure:
`test_campaign_helpers_keep_hash_literals_in_metadata` finds a digest literal
in preexisting untracked `blink/scripts/full_jit_copy_bindings.py`. Original log:
`/tmp/libghostty-framework-baseline.log`. No Blink file was changed to hide it.

## Blocker ledger

| ID | Classification | Reproducer/expected vs observed | Regression/fix | Full-input retry |
| --- | --- | --- | --- | --- |
| Z1 | Compiler grammar | Pinned `src/lib_vt.zig`, line 187: valid trailing argument comma in multiline `@export`; parser rejects it. | Frontend agent implementing shared grammar support and native-valid reduction. | Pending; original log `artifacts/zig-frontend-probes/initial-lib-vt.log`. |
| C1 | Support-header compatibility | Original `zig.h:6177` expands symbol assembler labels; DotCC parser rejects `__asm`. | Exact typed forwarding macro policy, plus removal of labels only on unused Windows intrinsic declarations; see ADAPTATIONS.md. | Advances to C2. |
| C2 | Shared compiler grammar/layout | Genuine freestanding generated `engine.c:7853`, `typedef __attribute__((aligned(1))) uint64_t aligned__1_u64_9;` rejected. | Scalar typedef alignment metadata and prefix/suffix grammar; actual explicit storage/layout, compatibility and aligned locals/globals. Native/emitted direct and object-link fixtures match, including actual record size 10. No annotation erased. | Actual full C retry advances to C3. |
| C3 | Shared C lexer | Generated `engine.c:15909` contains valid `"\?"`; string lexer rejected it although decoding already supports it. | Accept escaped question marks; native/emitted literal bytes agree. | Actual full C retry advances to C4. |
| C4 | Shared declarators | Generated `engine.c:19664` needs pointer-to-const-function-pointer local storage; retry at 19716 also needs the corresponding abstract cast. | Compose pointer levels/qualifiers structurally for named callbacks and abstract callback types. Native and direct/object-link execution all produce `callbacks 41 42 7`; read-only diagnostics preserved. | Actual full C retry reaches C5. |
| C5 | Shared expressions | Generated `engine.c:25395` uses `(struct T){{...}}.array[index]`; direct compound-literal postfix access rejected. | Place C99 compound literals in the postfix expression tier. Native and direct/object-link reductions produce `postfix 42 7`; 3510 unit cases pass. | Current accessor engine advances to line 447923, C6. |
| C6 | Shared callback parameters | Accessor engine line 447923 declares a named const callback parameter `void (*const a1)(...)`. | Share structural named declarator construction across callbacks and array-pointer parameters. Native/direct/object-link reductions produce `parameters 42 41`; all 3516 unit tests and six affected functional cases pass. | The complete 530948-line accessor engine now parses. Semantic C7 is first failure. |
| C7 | Source ABI | Full source semantic check fails its line-28 assertion requiring `sizeof(long double)==16` and alignment 16; the existing DotCC default models long double as 8 bytes. | Investigating explicit source-ABI storage support with loud rejection of unsupported numerical operations. Assertions remain enabled; no engine values use this type at this pin. | Not passed. |

Shared C2/C3 validation: **3504/3504 DotCC unit tests**, 89 focused alignment/GNU
unit tests, and **6/6 focused functional tests** (three Zig execution cases,
direct/object-link actual scalar alignment/storage, escaped-question execution)
pass. Logs: `artifacts/zig-frontend-probes/all-unit-tests-after-alignment.log`,
`scalar-alignment-functional.log`; full-input retry logs:
`engine-after-alignment.log`, `engine-after-question-escape.log`.

C4 validation: **3510/3510 DotCC unit tests**, 19 focused callback cases and
two direct/object-link callback execution cases pass. Logs:
`all-unit-tests-after-callback.log`, `qualified-callback-functional.log`,
`qualified-callback-unit-tests.log`. Actual full source retries:
`engine-after-callback-storage.log`, `engine-after-callback-cast.log`.

Milestone commits so far: `89e9bb534` (P0 campaign/native controls),
`a19c47f2d` (generic Zig expression/import repairs and regressions).

## P0 validation

Shared recipe discovery/layout passes. Existing product, toolchain and supplemental
license archives/trees are reusable with `--fetch never`, both `--hashes warn`
and `off`; fetch invocation also passed from `/tmp`. Layout includes the canonical
root solution, authored owning API/sample and shared Bash dispatch wrappers.

`bash libghostty/scripts/test.sh --suite native --fetch never` passes with 16
assertion cases, original native engine and genuine whole-engine C exact parity:
`artifacts/campaign/20261029-154051-f5367cca/receipt.json`, specialist report
`native-oracle.json`. Native checks cover creation, ASCII/render, cursor/erase,
SGR, split Unicode, visibility, screens, resize/reflow, reset, callbacks/title,
formatter buffers, ordinary errors, allocation failure, instance independence
and 32 balanced allocator lifecycle cycles. Additional authored allocator/ABI
adapter controls pass natively against both engines; their integrated rerun
is required after adapter edits. ABI observations: pointer 8, formatter options
56, allocator 16 and vtable 32 bytes.

`translate.sh --fetch never --tools reuse --form raw --jobs 1` with the preserved
initial compiler closure reaches C2, with no product published:
`artifacts/campaign/20261029-154246-a24e2a2c/receipt.json`, `000-engine.log`.
This attempt uses the full actual upstream build/module command, generated
Unicode inputs, freestanding C target, and authored ABI adapter. A preceding
attempt failed because a manually copied diagnostic tool closure omitted
satellite resources; it was corrected by preserving the complete closure and
was not mistaken for an engine failure.

Direct Zig findings/repairs and native/emitted regression receipts are detailed
in [ZIG-FRONTEND.md](ZIG-FRONTEND.md). At this point 556 Zig unit tests and three
native/emitted execution reductions pass; direct root semantic/export support
remains incomplete. The complete product proceeds through the genuine C route.

## Acceptance evidence

Current typed-address ABI integration preserves all **93 upstream exports**:
`artifacts/abi-export-inventory.json` records identical native-C export inventories
before/after the staged accessor extension. The current full native suite passes
16 oracle plus two boundary cases:
`artifacts/campaign/20261029-160333-22351e10/receipt.json`. New independent physical
ABI probes measure six public aggregates using actual storage sizes and field
address differences; native/emitted JIT transcripts match exactly at
`artifacts/abi-layout-managed.stdout` versus `tests/native/abi-expected.txt`.
These public header probes do not substitute for actual emitted-engine execution.

The applicable upstream render control passes **58/58**, zero failures/skips and
zero live allocator blocks, with exact original-Zig/native-C transcripts:
`build/native-tools/upstream-context-r3/artifacts/receipt.json` and
`upstream-render.json`. The loaded render inventory contains 61 named tests;
three Kitty graphics tests are explicitly excluded because graphics was excluded
from the agreed product before testing. Exact accepted output is retained in
`tests/upstream/expected.txt`. The new diagnostic managed profile is unrun.

An auxiliary broad native Debug test attempt reports C-API superset
3403 pass / 155 skip / 2 fail of 3560, and Zig module 2928 pass / 105 skip /
2 fail of 3035. The failures are an out-of-product TinyIo OS path case and a
full-reset test expecting glyph registration despite the disabled glyph feature.
This auxiliary run is not a passing campaign gate. Remaining applicable core
test groups are being classified; no full upstream qualification is claimed.

The canonical read-only corpus host-service control passes native and actual
emitted JIT execution, including nested directory traversal, short-name retry,
positional binary reads, EOF, ordinary errors and complete handle cleanup.
Secure entropy uses the native OS provider and shared BCL cryptographic provider;
the control checks statuses and freshness invariants, never exact random bytes.
Current integrated receipt:
`artifacts/campaign/20261029-163238-847ff3ba/receipt.json` (the report's entropy
description was added afterward; the actual entropy checks already passed).
This test service does not replace any terminal algorithm. Its full core
raw/processed JIT/AOT exercise remains required.

The first combined core original-native run executed all 2573 discovered
candidates: 2564 passed, nine skipped, zero failed and no live allocations.
Nine exact scope/placeholder exclusions are being recorded; the 2564 active
required cases remain required. The auxiliary genuine C run compiled 455 MB
of unchanged test/engine C and reported 2559 pass, 10 skip, four failures of
2573. Four failures require the real secure entropy service just implemented;
the additional skip requires hosted C-allocator capability in the test product.
Neither auxiliary result is a passing final core gate. No required assertion is
removed in resolving these host services.

No translated terminal engine product or raw/processed JIT/NativeAOT runtime
gate is accepted yet. Pins and downloaded references are available locally;
native control and direct Zig translation work are running independently.

### Default product priority

The user requests the campaign deliverable according to the plan and baseline.
The immediate critical path is the full default scalar engine and ABI boundary
through ordinary `translate.sh`: emit/link, raw build, postprocess, processed
build and transactional canonical publication. Broad diagnostic qualification
is deferred until this product exists; all original P4/P6 gates remain required.
The complete engine currently parses and passes all source ABI assertions under
explicit `sourceAbi.longDouble=x87-storage16`; the next observed backend error
is `array member of a temporary aggregate requires a hoistable storage context`.
No canonical product has yet been published.

The temporary-array failure was reduced to conditional returned aggregates and
compound literal array members. Native and emitted execution now agree exactly
(`temporary 42 43 8 41 2`), including unselected side effects and retained bytes.
The narrow shared fix classifies temporary array projections as branch-local
storage effects. Stronger lifetime regression work remains in progress. The
normal default scalar translation retry is
`artifacts/campaign/20261029-164601-fc693464/receipt.json`; its result remains
running at this update. The independent actual boundary object emits successfully
(307,629 bytes; `build/native-tools/boundary-object-probe/result.json`). A
separate real declaration-only link reduction reproduces conflicting unused
external header contracts; that generic repair is in progress.

The default scalar retry `20261029-164601-fc693464` completed both object
emissions: the full genuine engine C# is
`build/campaign/20261029-164601-fc693464/objects/000-engine.cs` (42 MB),
plus the actual ABI boundary object. Linking fails on conflicting external type
contracts for `ghostty_alloc` between engine-private types and an unused public
header declaration. The generation-side contract repair preserves all definitions,
actual calls/addresses and conservative global contracts; focused work is in
progress. No `.csproj`/raw build or canonical publication is claimed.

Retry `20261029-164959-22fd99b5` passed full default engine/boundary emission
and linking, producing the ordinary raw staged project. Its first raw build
reported 57 CS0161 paths and six unresolved integer compiler-runtime helpers.
The strict support-header `zig_unreachable` rule now preserves the supported
terminating builtin; six generic arithmetic services pass 14 focused tests and
real libgcc edge controls. Retry `20261029-165535-e1751380` resolves all 63 prior
errors and reaches 20 CS7013 metadata identifier length failures from upstream
Zig-mangled names. Generic target-name legalization is the next P2 repair.
No canonical publication or runtime acceptance is claimed yet.

### P2 canonical scalar publication

Normal default scalar retry `artifacts/campaign/20261029-170255-7d3e0289/receipt.json`
passes the complete engine/boundary emission and linking, raw build, semantic
postprocessing, processed build and both final canonical builds. The published
projects are `generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj` and
`generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj`; the framework
current-product pointer records this receipt. All 93 required upstream exports
are present, no non-inline engine bodies were dropped, and no native Ghostty
imports or test entrypoints occur in the scalar source closure. This completes
the initial P2 gate, not behavioral acceptance. Actual owning consumer and
raw/processed JIT/AOT qualification plus final-tool reproduction remain required.
Naming repair focused checks pass 6 units and 4 direct/object functional cases,
including static callback addresses and authored external CLR names.

### First owning execution and atomic boundary

The real owning sample builds against canonical raw output but fails during
terminal allocation in `ArenaAllocator.tryPushNode`: shared `GnuAtomic` rejects
success release / failure acquire. Receipt
`artifacts/campaign/20261029-170654-399ad998/receipt.json` retains the stack.
The original Zig allocator intentionally uses independent release/acquire orders;
its C11 support-header fallback forwards them unchanged. Shared C11 order
validation is retained. Two exact support-header rules strengthen success to
sequential consistency while preserving failure order, the real CAS, expected
value updates and single evaluation of the original success expression. Native
and emitted focused controls are in progress. Normal regeneration/republication
with this policy is running; no owning workflow success is claimed yet.

The CAS policy regeneration `170919-a9d8bd45` passes the complete normal
translation/publication pipeline. The actual canonical owning consumer then
passes both raw/processed JIT (`171252-f8cfacd0`) and raw/processed NativeAOT
(`171340-0c043e6e`). Each executes the meaningful transcript, styles/modes,
byte-split Unicode, alternate screen, resize/reset, two concurrent independent
owners and deterministic disposal through the genuine translated engine.
Focused native original/policy and emitted CAS controls also agree; the old
policy reproduces the original failure. Shared atomic validation remains intact.
The separate 24-case differential profile, rooted AOT, all 2,622 applicable
upstream cases and final-tool regeneration/reproduction remain required.

The actual 24-case differential qualification passes raw/processed JIT and
whole-library-rooted NativeAOT with zero failures/skips:
`artifacts/campaign/20261029-172036-a1c40c5a/receipt.json` and
`qualification.json`. These include native-comparable physical layout, terminal
behavior, allocator/error and lifecycle assertions. The root owning solution
build also passes (`artifacts/root-solution-initial-build.log`). P1/P3/P5 initial
gates now have actual-engine evidence; P4 applicable upstream and final P6
regeneration/reproduction remain open.

First bounded core control `172322-3a5eba26` passes original native and genuine
C for 143 C terminal cases, exact transcript, zero skipped/failed/live allocations
and no fixture handles after each case. Its C input is 32,398,500 bytes and the
native control peaks below 1 GiB, validating bounded partition feasibility.
Managed translation `172527-38e2fec2` then finds a standalone GNU post-definition
packed-attribute grammar gap. Preprocessing correctly advertises/preserves
`struct Header { ... } __attribute__((packed));`; native size 10/alignment 1
assertions pass. This is not an unavailable packed capability or malformed
upstream source. The shared parser repair is in progress; no managed core
assertions have yet passed. Compiled substring-filter extras are inventoried
separately and are never counted by the exact runtime selector.

### Bounded expression factories and user output choices

The namespace/aggregate-data pipeline `173744-b79eaedb` publishes
`Managed.GhosttyVt`; owning four-row execution `174224-0297ff72` and the
24-case four-row/rooted qualification `174734-04d23ea5` pass. These are current
user choices, retained for all later regeneration.

The user then rejected aggregate factories with up to 8,192 parameters.
`StructArrayInitText` now delegates to a generic bounded implementation:
proven primitive/record payloads use get-only span properties and copy into
zeroed inline value storage; runtime values use typed value argument packs.
Create has at most one parameter and fill helpers at most two. Initial focused
checks pass 10 structural and two Release execution cases, plus 16 affected
payload/literal-pool cases. Native GCC O0/O2 agrees on the assertion fixture.
Initial whole-engine retry `180442-750a41c7` passes normal canonical publication.
Actual raw/processed PE audits find max arity two, 3,277 RVA getters and 92
zero-argument factories allocating zero bytes in warmed loops that consume all
result bytes. The initial owning four-row run `180829-fb00986c` also passes.
Reports: `artifacts/aggregate-factories-{raw,processed}-after.json`.

Independent source review found two missing edge controls: comma operands could
interleave after hoisting, and the primitive floating folder could carry a wide
unsigned intermediate through a floating cast. These remain pending repairs and
new regressions; initial output/audit/consumer passes do not close this change.
Final regeneration and both matrices must follow those corrections. No generated
C# is edited. The full upstream/P6 gates remain open, followed by the explicitly
authorized direct Zig phase only after the completed C route is committed.

The revised standalone post-definition aggregate parser repair passes its six
unit cases and both actual physical-layout execution cases. Atomic explicit-order
expression repair passes focused order checks and the emitted fixture, with the
native expectation of 26 order-expression evaluations. The affected focused unit
selection totals 103 passed; unavailable GCC-WSL/MSVC platform oracles are two
explicit skips, separate from three passed local emitted cases. The full upstream
core retry remains required after the factory priority.

The factory edge fixes pass both Release source/object regressions (four total
factory execution cases) and the native `1234`/typed-floating reduction.
Corrected canonical retry `181517-e418fbb7` passes every publication build;
current owning four-row execution `181936-f36c22c9` and root build pass.
Accepted PE/allocation reports use a full getter-set warmup before the strict
zero-byte measurement: `artifacts/aggregate-factories-{raw,processed}-accepted.json`.
They retain default tiering and record setup separately; failed early one-pass
reports are kept. Current output has max helper arity two, 3,277 direct-RVA
getters and 92 zero-argument value factories with no measured allocations.
The 24-case profile has been regenerated (`182302-8ac4498e`); its new four-row
execution is running, so that gate is not yet claimed.

The first full unit run exposed six enum-binding regressions in the shared
`Type ;` parser path introduced for packed definitions. The helper now registers
existing anonymous/tagged enum definitions normally, preserving scope; the
unchanged existing enum tests prove the repair. The rerun passes all 3,688 unit
cases (`all-unit-tests-after-packed-enum-regression.log`). Affected generic
functional checks pass 54 cases, with two explicitly unavailable platform
oracles, separately from the two passed physical source-ABI alignment cases.
Eight alignment unit cases and 67 foreign-ABI proof cases also pass. A test-only
single-document assumption was corrected to validate every actual offsetof
layout document; no layout expectation was relaxed.

The corrected factory differential/rooted-AOT suite passes all four rows,
24 cases each, without failures/skips (`182713-753e56db`). The focused generic
factory milestone is committed as `8efc09dfb`; its alternate index operation
preserved 123,918 unrelated staged entries and left the mixed backend's unrelated
changes intact. This is not the C-route completion commit.

Actual first upstream-core retry `182910-7a8cf18a` advances through packed
records and fails at the valid C declaration `_Alignas(128) static struct ...`
on generated line 16118. This is a shared declaration-specifier ordering gap;
no input rewriting or scope reduction is selected. Native/frozen reduction,
generic grammar repair and actual aligned-storage regressions are assigned.
The native/C qualification partitions proceed independently through normal
CLI receipts while shared compiler builds remain supervisor-owned.

### First managed upstream core acceptance and typed host boundary

The ordered storage/alignment grammar repair advances the entire first core
product through emission and linking. Its initial raw build (`190145-1b9d1fe6`)
finds a diagnostic host-contract mismatch: Zig declared allocator results as
byte pointers while the C provider returns `void *`. The authored runners now
use nullable opaque pointers at this ABI and explicit pointer casts inside their
allocator adapters. `tests/upstream/host.h` is authoritative; a native C
syntax/type check of every genuine emitted test product against that header is
now a preparation gate. The old product fails this check with conflicting
prototypes; the corrected product passes. No upstream algorithms or assertions
change.

Normal translation `191027-be5a8ef4` publishes the first 143-case core product.
Its matrix `191842-a755c4a3` passes all 143 exact named cases in raw/processed JIT
and whole-library-rooted NativeAOT: zero skips, failures and live allocations in
every row. This is the first managed upstream partition acceptance, not P4/P6
completion. Native/C controls are repeated for the corrected current harness.

The private-global repair preserves separate objects across C translation units,
including different record types, physical alignment and thread-local duration.
Its focused 33 unit and eight execution cases pass. Full unit verification
exposes incomplete-array declaration dispatch and an older test expecting static
header objects to share storage; those regressions are under repair before the
next combined build. Their failing full-suite log is retained.

Actual factory allocation execution also passes raw/processed JIT and NativeAOT
using private executables compiling the exact canonical sources: 3,277 getters
and 92 zero-argument value factories per form, zero warmed managed allocations,
with nonzero allocation positive controls. See `tests/FactoryAllocation/RESULTS.md`.
The required normal `factory-allocation` suite is integrated; its first raw/JIT
normal receipt is `184722-f2b92a93`. Final-tool full-suite acceptance remains open.

The ordered declaration regression repair adds the missing incomplete-array arm
to the existing declaration path, retaining source array types and completion
identity. Small-stack checks now assert both separate 12,000-element private
header tables and their complete initializer sequences; same-unit tentative,
extern and initialized declarations still share one object and reject duplicate
initialized definitions. This corrects an older cross-unit storage-merging
expectation, without relaxing the storage/stack checks.

The combined tool build passes with zero warnings/errors. All **3,721 unit
tests** pass (`all-unit-tests-after-ordered-storage-r9.log`), and all eight
ordered-storage direct/object execution cases pass
(`ordered-storage-r9-functional.log`). The corrected-host native batch repeats
143 terminal, 138 formatter, 16 Unicode and 99 supporting-C cases: **396** exact
passes per native/C engine, zero skips/failures/live allocations. Every case
checks fixture cleanup. Receipts and phase/RSS details are in
`artifacts/current-host-native-batch-r9/summary.json`.

The next normal managed batch starts with the Unicode product using the newly
validated tools; it retains every selected case and the same four-row matrix.

Unicode diagnostic translation `192838-143d2712` and matrix
`193053-eb32999b` pass all 16 selected cases in every raw/processed JIT/rooted
AOT row. Supporting-C translation `193128-59d197c1` completes emission/linking
but its raw build finds one missing service, `__mulodi4`. The shared runtime
now implements signed 64-bit overflow multiplication from an exact `Int128`
product, returning wrapped low bits and writing the exact 0/1 flag. All 35
focused arithmetic tests pass. A separate native control compares 25,017 pairs
through real pinned Zig/LLVM compiler-rt, GCC builtin overflow and real libgcc
128-bit multiplication: 100,068 guarded calls agree with an arbitrary-precision
oracle. The installed GCC library lacks `__mulodi4`; no substitute native
implementation is used. Full normal supporting-product retry remains required.

The focused SourceABI milestone candidate is also independently verified on a
clean HEAD archive with only its 27 selected blobs: 109 unit and four physical
source/object execution cases pass. This excludes the unrelated dirty branch
work. Its commit procedure is being preflighted before touching the real index;
two older staged additions belong to this feature and must not trigger an
unsafe post-commit merge. No completion commit is claimed.

Supporting-C retry `194226-0cc79ea5` closes the arithmetic build failure; matrix
`194748-b647912c` passes all 99 cases in raw/processed JIT and rooted AOT.
The arithmetic repair is committed as `991ff582a`. SourceABI storage and foreign
proof support is committed as `c1ee98aef` after isolated 109-unit/four-execution
validation; its exact 27-file commit preserves 123,913 unrelated staged records.
C11 explicit-order evaluation is committed as `df6fcb722`, after isolated 63
focused tests and the native/emitted 26-evaluation fixture. Its eight-file
commit preserves 123,937 unrelated staged records. These are focused milestones,
not the C-route completion commit.

The strengthened delivery audit passes 25 pure controls and a read-only check
of the historical scalar's actual 11-module/two-object source closure. It now
requires exact complete notice bodies, including this repository's MIT license
for the embedded runtime, and metadata-attributed host-library imports. Its normal
scalar gate awaits final regeneration; historical five-notice artifacts do not
establish the new six-notice delivery check.

The isolated storage candidate passes 162 unit and all ten owned physical
execution cases. Its broader selected execution suite retains two enum object
contract failures also seen on current clean HEAD. That HEAD already contains
our earlier inactive-function linkage filter, so this is not yet classified as
an unrelated pre-campaign failure. A pre-filter comparison and structural repair
are assigned; original expectations remain unchanged.

The affected Zydis control passes all seven selected suites in
`zydis/artifacts/campaign/20261029-200841-18e7773b/receipt.json`: native 393,
ABI four cells, differential 28, rooted companion 786, boundary 20, and four
consumer workflows; zero execution failures/skips. The first attempt is retained:
its audit correctly rejected our omission of the existing `rooted-aot` suite.
The completed retry adds that suite without changing Zydis policy/source files.
The formatter Ghostty diagnostic remains in emission, and P4/P6 remain open.

### Final-tool integration and reproduction checkpoint

The inactive-prototype filter introduced by `6c6523e92` caused the enum object
contract regression: its parent passes the original cases. Graph4 now retains
matching-tag declaration projections separately from active function contracts.
Original enum, call/address/global and opaque-adapter assertions remain intact.
The repair is committed as `d691c6410`; a fresh actual whole-engine/bridge probe
emits both Graph4 objects, links and builds raw output successfully
(`build/graph4-fullengine-probe-r15/receipt.json`). Empty, rootless Graph3 scalar
objects remain compatible through the bounded reader fix `8b8724962`; aggregate
objects missing declaration proof require regeneration. SourceABI foreign guards
remain active.

Ordered/aligned declarations and separate translation-unit private storage are
committed as `347fedabd`. GNU attributes between aggregate tags and names are
handled by the typedef rewriter repair `43fc33d48`. The combined current source
passes all 3,769 unit tests and 203 affected functional executions. The fresh
Graph4 Zydis verification (`203355-4300ba66`) passes all seven selected suites.
The full Valkey retry emits all 164 units (`203338-dace6771`) and passes its 76
raw/processed JIT/NativeAOT consumer cases. Those are selected downstream
regressions, not Ghostty acceptance evidence.

Exploratory managed upstream matrices now cover 427 core cases plus 58 render
cases in all four rows: terminal 143 (`191842-a755c4a3`), Unicode 16
(`193053-eb32999b`), supporting C 99 (`194748-b647912c`), formatters 138
(`202222-30921070`), logical render 31 (`203651-4886d22a`) and render 58
(`204410-28a880c5`). These 485 distinct cases do not close the 2,622-case
matrix. Current-host native/C controls cover 811 of the 2,564 core cases. At this
checkpoint the remaining controls and every partition's final regeneration were
C-route requirements; the later user deferral moves them to direct Zig.

The normal hashes-off scalar reproduction `205950-b1f45aae` runs from `/` with
local inputs and rebuilt tools. It performs fresh engine/bridge emission and
canonical publication, then the consumer, factory-allocation and audit suites.
Its selection is deliberately smaller than full acceptance. An 86-file snapshot
protects authored sources and critical original inputs during regeneration.
The then-planned bare hashes-warn verification selected every declared suite,
including all 20 bounded core products and all four upstream execution rows;
that expanded selection was later interrupted by the user scope change below.
The hashes-off terminal receipt passes every selected gate, including both
forms under JIT/NativeAOT; all 86 protected files are unchanged. Independent
read-only review (`artifacts/p6-final-audit-review-r17/review.txt`) finds no
concrete defect in the selected audit/coverage scope and repeats all 26 pure
controls successfully. That expanded hashes-warn reproduction never reached
acceptance and was later cancelled normally. Retained C-route P4/P6 gates and
the completion commit remain open; upstream Zig integration has not begun.

The independent ordinary solution rebuild from `/` also passes after that OFF
publication: `artifacts/p6-off-root-rebuild-r21/report.txt`, exit zero in 29.269
seconds, zero errors and four CS0649 warnings representing two distinct
diagnostics. It builds the original sample, original owning API and canonical
default library without project overrides or copied authored sources. All 86
snapshot facts, 215 protected files, 265 tool-closure hashes, the default product
pointer and HEAD/index bytes remain unchanged. This closes the OFF original-
reference rebuild check; a final rebuild after the WARN scalar publication is
still required.

### Explicit upstream-test deferral and clean interruption

The expanded WARN attempt was stopped at the user's instruction, without
changing its test selection or pretending completion. Verified owned CLI PID
2772208 received SIGINT and exited 130. Parser/stream-two receipt
`234518-39b51fe2` and its active object command are `cancelled`; all recorded
child PIDs drained and the normal campaign lock was released. The eight prior
profile publications remain, including parser/stream-one `221150-4966cb92`,
whose full raw/postprocessed publication passed after 93.7 minutes. The scalar
master verification had not started. All 86 protected facts remained unchanged.
Evidence: `artifacts/p6-full-warn/user-scope-stop.json` and the original receipts.

No further upstream generation/execution is selected for current C acceptance.
The inventory remains 2,564 core plus 58 render cases, with all names and reasons
preserved for the direct Zig phase. The streamlined normal WARN verification
and final ordinary root rebuild now close the remaining C-route gates before
its focused accepted completion commit. Only then does upstream Zig integration
and direct-route implementation begin automatically.

### Retained C-route acceptance

Normal verification `001153-ab4e8deb` passes exactly the five retained suites
after fresh qualification preparation `001153-122ad39a`: native 16+2 controls
and six ABI layouts, consumer four rows, qualification 24×4 with rooted AOT,
factory four rows plus two PE audits, and both delivery audit forms. All 89
current authored/critical facts are preserved. Final ordinary root rebuild
passes once from `/` in 22.907 s, zero errors/four CS0649 warning occurrences
(two distinct); the model-capacity interruption occurred after that build, so
the supervisor completed its report without rerunning it.

The complete retained evidence report is
`build/c-route-evidence-r23/final-evidence-corrected-readers.json`; every gate
passes. The prior reader rejection and exact original source are preserved.
Its fixes distinguish MSBuild project properties from positional project paths
and preliminary warmup from the strict measured pass. Seventeen pure controls
still reject nonzero measured allocations. Product/test code and thresholds
were unchanged. Current helpers have at most two parameters; 3,277 RVA getters
and 92 zero-argument factories pass all JIT/AOT allocation rows with zero warmed
measured allocations. All 93 exports and six full notices are retained.

This accepts the user-selected C scope, not the deferred upstream matrix. The
focused completion commit preserves unrelated staging; only after that commit
does relevant true-upstream integration and direct actual Zig implementation
begin automatically. Deferred inventories and all historical evidence remain.
