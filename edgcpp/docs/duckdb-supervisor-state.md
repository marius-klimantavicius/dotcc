# DuckDB translation supervisor — resumed 2026-10-10

The user authorized continuous coordinated implementation of complete DuckDB
translation. First finish EDG/owned `.dotobj` semantic export, packaging,
linking and readable C# generation for the complete project. Only then complete
missing libc++/shared runtime bindings and compiled JIT/NativeAOT execution.
Earlier phase-boundary stop instructions and the old merged-IL restart recipe
are superseded. There is no authorization to weaken semantic checks or omit
required bodies, initialization, destruction or exception handling.

## Ownership and current work

Supervisor `/root/duckdb_supervisor` owns build/prepare scheduling, integration,
project probes, durable evidence and Git staging/commits. Root is read-only while
that ownership is active. Native edits are canonical `backend/` files; generated
upstream overlays and unrelated existing work remain unstaged.

- `lifetime_export`: original DuckDB `exception.cpp`/`date.cpp` unsupported
  exception construct kind 10, then `allocator.cpp` initialization with destructor.
  Owns lifetime portions of `dot_gen_object.h` and focused lifecycle fixtures.
- `base_provenance`: original DuckDB `string_util.cpp` physical base lacking direct
  original declaration. Owns record/base portions of `dot_gen_object.h`, relevant
  source-base capture and focused base fixtures.
- Supervisor: schema/managed integration as required, full-manifest provenance,
  broader bounded-memory frontier probes and milestone acceptance.

Workers consult stock EDG C lowering/backend before choosing representations.
Overlapping native files have function-level ownership, with edits coordinated.
Workers do not run preparation, compiler builds or Git mutations.

## Baseline and project identity

- Baseline commit `35030a956`, object profile `scalar-control-v37`.
- Persistent root `/mnt/storage/dotcc-edg-campaign-20261003`.
- Frozen native `dotobj-native075-verified-snapshot/cpfe-dot`.
- Frozen managed `dotobj-project-managed-build-085/bin/DotObj.Linker/debug/dotobj-link.dll`.
- Full manifest `dotobj-duckdb-runtime-manifest-003/project.json`: **2,635 units**,
  bundled dependencies, seven static extensions, native extension loading disabled.
- Five-unit `dotobj-duckdb-frontier-075-types/results.json` is a sample only.
  `hugeint.cpp` packages and generates 280 definitions; four other units reject
  the gates assigned above. No complete DuckDB generation/build/run is claimed.
- Existing source stage `duckdb-portable-correspondence-001/source`; preserve its
  pinned acquisition/adapter provenance. Do not adopt unrelated uncommitted
  `duckdb/edg/portable.py` changes silently.

## Acceptance and continuation

1. Capture exact original/lowered semantics for current gates, add native and
   stock-C reference fixtures, validate owned metadata and malformed inputs.
2. Freeze tools, run focused and appropriate broad regressions, rerun the original
   failing units, retain failure evidence and commit the verified milestone.
3. Expand beyond the sample across the full manifest; advance exporter, verifier,
   linker and emitter failure frontiers until every required unit is accounted
   for and complete C# is emitted without source/EDG access during linking.
4. Then implement remaining libc++/runtime requirements; verify native DuckDB
   behavior/tests under managed JIT and NativeAOT, recording tested platforms.

Use independent original-source per-unit EDG invocations, initially at most two
workers with 8-GiB per-worker caps. Never restore whole-program IL union or an
external preprocessing pipeline. Preserve intermediates and monitor storage.
Generation, compilation and runtime achievements are separate. Update this file
and the campaign ledger at significant transitions; do not stop at a small
milestone unless the user pauses or a genuine external blocker prevents progress.

## Live frontier after first implementation build

Native076/managed086 implements internal try expressions plus recursive original
base/field ID reservation. The base regression passes14 checks; all four failing
sample units advance. `string_util.cpp` now needs adjusting/covariant wrapper
provenance. `exception.cpp` and `date.cpp` now need source allocation-release
obligations (these are not destructor obligations); the placement-new focused
fixture reproduces the same missing capture. `allocator.cpp` advances to an
unsupported cleanup action flag. The native/stock-C lifetime oracle passes12
scenarios but owned export is not yet accepted. Broad regression076/086 is running.

Worker assignments continue: base worker owns exact stock wrapper provenance;
lifetime worker owns allocation-release source obligations and their internal-try
correspondence. Supervisor integrates additive schema/managed contracts. These
features must retain original obligation types and boundaries rather than hiding
allocation rollback among fake destructors. All076 attempts are retained.

## Accepted v38 milestone and next work

Final native078/managed088:1,042 broad JIT checks,37 lifetime checks/24 native+
stock-C/JIT/NativeAOT scenarios,14 base checks,18 thunk metadata/link checks.
Date now packages but verifier rejects its narrow switch selector. Hugeint still
generates. The 38-subsystem sample packages17 units and all17 independently emit;
combined linking rejects fabricated anonymous-record counter identities.
Evidence: `evidence/dotobj-duckdb-boundaries-20261010.json`.

Next ownership: base worker fixes stable original anonymous nested record identity
and later record storage initialization; lifetime worker implements ordinary-new
rollback and local-static guard cleanup actions; supervisor integrates schema,
managed verification/emission and constexpr/switch selection. Native078 sources
and all v38 validation are frozen for the milestone commit. No libc++ runtime
closure work starts before full generation. New rollback/storage-init fixtures
are next-phase preparation and are excluded from this commit.

## v39 integration in progress

Native079/managed089 built. Stable anonymous-member identities pass35 focused
checks including source-hidden multiunit linking and execution. Constexpr branch
selection and narrow integer switch promotion pass34 checks against native and
stock C, JIT and NativeAOT. Shared BCL guard runtime passes16 direct ABI checks,
including contention/publication, abort/retry and NativeAOT. These are focused
results, not full DuckDB acceptance.

Allocation/guard semantic actions are now represented separately from object
destructor and handler cleanup states. Native080 corrects the stock sized-delete
supplement's zero element-count convention. Original guarded-local-static capture
is still under investigation; full translated rollback acceptance is pending.
Workers: lifetime worker owns guard/allocation capture and acceptance; base worker
investigates original file-scope destruction provenance for the next project gate.
Supervisor owns schema/action verification, emission, selection closure, tools,
project batches and milestone commits. v39 is not yet committed.

## v39 focused acceptance and final verification

Native084/managed090 are frozen. Native080 broad suite passed 1,042 checks; final
native084 broad rerun passed 1,042 checks at `dotobj-full-v39-002`. Focused acceptance:
35anonymous identity,34constexpr/switch,16shared guard ABI,65allocation/localstatic
rollback (native/stock sequential,JIT/AOT,32thread publication),19foldedvoid.
Review link `artifacts/review/dotobj-duckdb-v39` contains retained source outputs.
The38unit sample now packages22 and independently emits21. Combined linking
passes anonymous identity and reaches record transport. Fiveunit final084 sample
packages3:date/hugeint emit,string_util verifier rejects discarded recordplace.
Exception still has physical byvalue transport gate. Allocator now reaches exact
missing ABI for stock make_cxa_dtor_type(void(void*)) used by __cxa_atexit.

Nextworker findings are preserved but excluded from v39implementation acceptance:
`docs/dotobj-global-lifetime-design.md` and27native/stock oraclechecks; global/static
registration must occur at observed successful registration call, including local
static registration before guard publication. Source original global lifetime
ownership and generated array destructor callback provenance must be explicit.
Baseworker also identified string_util's Split nonfinal sequence operand as a
discarded noncopyable basic_istream place, currently incorrectly treated as value
transport by managed verification. No EDG semantic defect is alleged.

V39 acceptance is complete and committed as the allocation/guard/selection
milestone. Native084 and managed090 are immutable acceptance tools. Continue
with the next source/static lifetime and record-value gates; full DuckDB
generation and runtime verification are still incomplete.

## v40 active implementation after commit1983e4c3e

- Lifetime worker owns original global/function-static graphs, source initializer
  ownership, observed destructor registration and generated array destructor
  callbacks; exact stock `make_cxa_dtor_type` capture is included. Dedicated new
  managed static metadata reader and focused tests belong to that worker.
- Base/type worker owns discarded-place verifier/emitter semantics, parameter
  versus return ABI distinction, and deferred routine ABI storage/link resolution.
  Exact reductions show three different causes; no global permission bypass.
- Supervisor owns schema/packager/object-table/source-map integration, nullable
  initializer ownership in common lifetime readers, shared static-finalizer
  runtime integration with existing atexit/process bindings, builds and commits.

The original static graph remains separate. Automatic initializer cleanup rows
have an explicit source-initializer owner alternative, plus exact adoption into
observed generated routines and correspondence to original static lifetimes and
obligations. Static registration follows the actual lowered successful call;
array callbacks retain generated origin instead of invented source declarations.
Runtime registration must share order with plain atexit and process termination,
not drain an independent destructor-only list. Schema contract is retained at
`dotobj-v40-static-wire-contract.json` under persistent storage.

A by-value std::tuple<int&&> parameter is direct under stock argument policy but
requires indirect return storage under the distinct stock return policy. Preserve
both facts independently. Unused routine-pointer fields returning incomplete
classes retain their complete semantic signature and an explicitly deferred ABI;
link resolution may complete it from an exact matching source signature. Only
storage/pointer transport is valid before completion, never an invented call ABI.

## v40 first integration evidence

Frozen native085/managed094 retain the first implementation. Separate record
argument transport passes 24 focused checks and deferred callback ABI passes 29,
including native/stock C, JIT and NativeAOT comparisons. Scalar static lifetime
translation passes production binding and JIT/NativeAOT with the exact native
`120657` trace. Library finalization of the interleaved C++ destructor/atexit
fixture produces `CMrnldb` under JIT, including registration during draining and
two explicit finalization calls. Existing process regressions pass 87 checks
with the shared registry. These are focused results, not full project success.

The first array initializer rejects missing original cleanup correspondence;
the temporary initializer rejects unexplained generated startup statements.
Both gates remain strict while the lifetime worker traces stock lowering.
Five original DuckDB units exposed a separate source/runtime distinction:
stock-suppressed constexpr template members have source initializer information
but no emitted runtime storage identity. Preserve their original source name,
type, location and identity with an explicit `stock-not-emitted` disposition;
executed initialization and registration still require exact emitted storage.

The packager now names SQL columns explicitly and checks complete schema coverage,
preventing new optional ownership fields from shifting positional values. Failed
085 probes and frozen pre-disposition tools remain retained. No v40 milestone is
committed yet; final focused and broad verification must follow integration.

Native090/managed101 now preserve source-only dependent parameter/member types
without inventing execution layouts. A focused original template API fixture
passes 24 checks, including JIT/NativeAOT and resealed malformed metadata. The
real `string_util.cpp` packages again with its unused dependent libc++ types
retained, then reaches an alignment-provenance gate in `basic_istream`. Exact
ordinary virtual-base adjustment observations are the type worker's next task;
the lowered vtable lookup and single evaluation must remain intact.

Static array rollback uses a separate original partial-aggregate obligation;
stock stores these in the global-static lifetime despite their exceptional-only
purpose. Original lifetime kind alone does not establish persistent destruction.
Keep original nested DIP/type/ordinal and initializer containment, with either an
observed cleanup action or the exact stock terminal no-throw elision branch.
The lifetime worker is fixing final adoption/completeness correspondence checks.
Broad native090 regression is running against immutable copied tools/tests.


Native092/managed104 complete the focused static-lifetime acceptance: 146 checks
pass across six native/stock-C/JIT/NativeAOT scenarios, explicit repeated library
finalization and fifteen resealed malformed-object cases. Evidence is retained at
`dotobj-global-v40-003/results.json`; original native092 tool SHA is
`bcb0826da0e81bc800aeedf549c306336aba837c1d91f9383c3a01f2706f30e0`.
Generated review examples now include array destruction, initializer temporaries,
partial-array rollback and interleaved finalizers. This does not establish full
DuckDB compilation or runtime behavior. Fresh broad regression is running at
`dotobj-full-v40-002`; its earlier attempt stopped on a copied harness asset-root
mistake and remains retained, not counted as acceptance.

Next exact gates: the virtual-base pointer observation must survive stock node
copy/overwrite and null-preservation transformations without losing source/vtable
correspondence; constant initialization of noncopyable records must retain its
original initializer and associated constant-pool identity, including NRVO result
storage. Generic runtime record copying remains forbidden. Real DuckDB temporary
and partial-static lifetime completeness remains the lifetime worker's priority.


## Full project inventory started

The full 2,635-unit manifest is now running with immutable native094 and tooling094,
at `dotobj-duckdb-full-frontier-094`, two workers, 8-GiB per-worker address-space
limits and 1,800-second unit timeout. This is an inventory with strict failure
receipts, not a completed translation. At the first94 completions,34 packaged and
60 failed;41 failures shared `_Storage` allocation rollback elision and13 shared
original default-member initializer ownership. No files are cleaned up.

Focused next results: terminal partial-array elision passes32 native/stock-C/JIT
and invalid-metadata checks (`dotobj-terminal-partial-v40-001`). Real DuckDB
`replace_type.cpp` now packages619 functions with94 original destructions and one
exact stock terminal elision. Constant-pool initialization of noncopyable prvalues
passes26 checks (`dotobj-record-init-v40-007`), including zero and relocated/nonzero
representations and observable destruction. The original named-return fixture is
still blocked, now positively identified as stock empty-destructor removal.

Virtual-base observations now export on native095; remaining managed validation
exposed stock null-preservation mutating the outer expression after copying it.
The next exact observer invalidates only that stale outer proof, retaining the
inner adjustment. The preparer also explicitly removes an obsolete earlier
observation hook; cumulative generated overlays must not retain replaced hooks.
The earlier full regression002 passed580 checks before a second copied-harness
asset-root error. Final harness must preserve the EDG/object-linker sibling layout;
do not repeat the entire suite for each incremental implementation edit.

Before adding the larger original field-default initializer ownership contract,
finish exact no-effect dispositions and virtual-base acceptance and commit the
coherent v40 milestone. Original constructor source graphs must remain intact;
field templates and their observed constructor copies need explicit provenance,
not replacement by synthesized constructor graphs or fabricated file storage.


## Accepted v40 checkpoint

Final native098 SHA `00a9d692dee792c4a278311362d649b032c6da0b0b8b4c9b389eff3155067b78`,
managed109, tooling098. Broad096/107 passed 1,042 checks; narrow final deltas have
focused acceptance: static lifetime146, record initialization138, virtual-base31,
argument ABI24, deferred ABI29, discarded place26, dependent source types24,
terminal partial32, construct/elision coexistence8, plus two no-effect malformed
cases and the prior87 shared-process finalizer checks. Receipts identify tools;
not every receipt ran the final tool pair. Evidence is
`evidence/dotobj-duckdb-static-types-20261010.json`.

The full094 project inventory remains active (session79866). Its immutable v40
checkpoint captured810 completions,456 packaged. Intermediate aggregate results
are not complete DuckDB success. Real string_util passes all owned virtual-base
proofs under109, then still rejects the alignment of an incomplete external VTT
array declaration; the type worker has located that next gate.

Next v41 ownership: lifetime worker implements exact stock empty-allocation-
initialization disposition (largest early full-project family), then original
field-default template/copy ownership; base worker applies the retained exact
conditional-record initialization patch and addresses incomplete VTT alignment;
supervisor integrates common schema/lifetime coverage and owns ICU register-or-
memory empty-assembly support. All source constructor graphs remain preserved.
No libc++ runtime detour, cleanup, external preprocessing or merged IL is allowed.

## v41 frontier (099/110)

Native099 SHA a1e9e954baf42d22ba88c342c7d0249997cc359485b2b139c47a687ddd7f1eed
and managed110 built successfully. Empty placement cleanup passes32 focused
checks, ICU rm/mr alias barriers29, external declaration alignment32; each
includes native/stock-C/JIT and strict metadata cases. Conditional record
construction still rejects the reduced case; exact observer correction pending.

Real allocator.cpp now packages then hits the managed default-record layout
gate. Real string_util.cpp packages, verifies and generates with two RTTI
providers:1,853 function definitions in dotobj-duckdb-string-util-generated-099.
Review link artifacts/review/dotobj-duckdb-v41/string-util. This is not compiled
or runnable DuckDB. Full2635-unit inventory continues on frozen094; latest
reported checkpoint1060 complete640 packaged,420 gated.

Field-default source template/copy ownership is next; original source constructor
graphs remain intact. Worker owns native/schema/static+field readers, supervisor
common cleanup/array/normal owner integration, base worker conditional record
correspondence and missing finalized new/delete array callback signature hooks.

## Active field-template integration (101/111, not accepted)

Native101 built SHA cb0d3de8dee58adfdf51d45af1476723a8b0a140f74bb13af02fafd07dfb3fd4;
managed111 builds without warnings. New source field-initializer templates and
constructor-copy relations retain the original constructor graph. Common root
cleanup/normal/array owner fields and exact this/member target checks are in
place. First fixture rejects a partial obligation from the original global
field-template graph: retain that original sourceStaticObligationId and record
its execution disposition per constructor. Do not fabricate an automatic source
lifetime or license all constructor copies using one elision observation.

Real101 probes additionally reject unused libcxx __exception_guard_noexceptions
template layout because newly captured field templates currently force an
execution record. Preserve source-only dependent types without requiring an
execution layout until instantiated. Worker is correcting that boundary.

Conditional pooled initialization diagnostic proves prior assumed master-entry
observer path was absent. Base worker traces the tiny reducer under frozen101
to locate the actual stock assignment boundary. Array allocation callback types
now capture, but exact vector-helper allocation rollback ownership remains a
separate explicit gap; no generated-execution success claimed for it.

102/112 focused progress: exact shallow-overwrite propagation fixes conditional
record initialization,25 checks pass (`dotobj-record-conditional-v41-004`).
Field-default array initializer fixture passes37 native/stock/JIT/source-hidden
and malformed-metadata checks (`dotobj-field-default-v41-003`). Native102 SHA
34bc756f6c84c875c919fcb25b69760ea4b7836bef13733435891743dd26023e.
103 pending strict root elisions, automatic adoption of nested field-template
partials, separate ctor-init stock no-effect observer, source-only record split.

A bounded50-object managed baseline inventory completed45 verified,5 gated;
`dotobj-managed-frontier-094-105-001` explicitly matches native094 schema and
managed105. Two signature gates are delayed constructor exception specifications:
vector entity118 in039f07cb247c43a888267cc7 maps execution routine905 terminate
but original routine1495 maps906 unwind, with identical void/this parameters.
Keep exact signature validation; capture actual delayed resolution if confirmed.
Other three verifier gates are record transfer/default initialization policies.

## Accepted v41 checkpoint

Frozen native103 SHA6ec6dd6b4cdad9e57f8f568330c5929039377c87f8e22195517aedcc2f466ace
and managed114 pass1,042 broad checks and288 focused checks across eight
NativeAOT suites. Frozen harness dotobj-v41-acceptance-tools-103 preserves the
EDG sibling assets. Evidence dotobj-duckdb-field-types-20261010.json records
exact receipts, hashes, tool versions and immutable partial project inventory.

This accepts the current source-only record/field contract and copied initializer
lifetimes; it does not complete DuckDB. New real source-only gates are dependent
allocator_traits specialization arguments and symbolic array bounds in libcxx
variant. Next base worker owns those typed semantic source representations;
lifetime worker owns exact vector-helper allocation rollback protocol. Supervisor
integrates common coverage/source maps, schedules bounded builds and verification,
and commits. No runtime/libc++ closure detour, cleanup or merged IL.

Committed v41 as bd4a08f6b (44 explicit owned paths). Profile42 now active
in canonical code. Base worker owns typed source template arguments/semantic
constants/dependent array bounds; lifetime worker owns exact array-allocation
helper protocol. Supervisor added ReadArrayAllocationOperations and helper-owned
source obligation coverage union, pending dedicated reader. Next native104 and
managed115 (113 was superseded by114 before acceptance because a final source
record guard landed after113 compilation). Build freezes require worker readiness
acknowledgements before preparation. Full094 inventory remains running.

## Active v42 acceptance (105/115)

Native105 builds successfully, SHA
bbd3b8b4fac87f83594b74f6f8563079cfd18071e957d3e60c4bb19d3047340a;
managed115 builds without warnings. Native104 retained the failed integration
build (NodeShape lacked direct-call classification);105 records an exact boolean
instead of adding a string to every execution node. Base worker runs owned
source-template argument/bound acceptance; lifetime worker runs vector helper
protocol acceptance. Supervisor runs five actual DuckDB frontier units (core
extension, lambda functions, ICU character properties/day periods, CSV options).
No production vector allocation provider or translated execution claim yet.
Full094 baseline continues; checkpoint is separately saved as
dotobj-v42-baseline-inventory-checkpoint.json. No cleanup.

The full frozen094 baseline inventory completed all 2,635 units: 1,402
packaged, 1,223 explicit exporter gates, two exporter range-check exceptions
(task_scheduler/buffer_pool using concurrentqueue), and eight test configuration
or platform API declaration failures. No timeout. This historical inventory
uses v40, not current v42, and is not full project generation evidence. Receipt:
`evidence/dotobj-duckdb-full-baseline-20261010.json`. Current105 verifies and
generates bundled ICU characterproperties (49 definitions); review link under
`artifacts/review/dotobj-duckdb-v42/icu-characterproperties`.

106/116 accepts dependent trait-member constants and exact out-of-line vector
callback ABI provenance. Native106 SHA
988861d85cf043cb07da37089e076dde82c12eb3526c4cf7d3b62ca3c2675efd.
Focused source-template NativeAOT suites pass 79 checks; vector allocation
metadata/generation passes 69 plus 28 native/stock oracle checks. No translated
vector-runtime execution claim. Original full delete[] fixture remains gated.

Diverse actual106 inventory completed 27 representatives: four packaged ICU
units, 23 source-dependent-expression gates. The four ICU units verify and link
with two RTTI providers to 269 generated definitions, review link
`artifacts/review/dotobj-duckdb-v42/icu-four-units`. Broad acceptance first found
a test's positional source_types INSERT assumed 13 columns; source array bound
kind makes 14. Test mutations now name columns explicitly. Frozen harness106b
runs broad002 against unchanged native106/managed116. Workers retain read-only
next-family diagnosis until milestone commit.

Broad002 passed all 1,042 checks with unchanged106/116. v42 accepted evidence
now saved in dotobj-duckdb-source-templates-arrays-20261010.json. Next v43 work
is staged by workers in persistent scratch while the canonical milestone is
committed: base owns typed compiler traits/sizeof/alignof source nodes; lifetime
owns original allocation-to-element pairing and vector-delete protocol.
