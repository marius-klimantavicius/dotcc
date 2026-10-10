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

- `lifetime_export`: exact generated cleanup ownership in ART; v53 generated
  array zero-model/constructor callbacks accepted. Scratch diagnostic queued.
- `base_provenance`: full typed source concept/requirement graph and declaration
  references, preserving dependent substitution environments. Scratch only.
- Supervisor: verified integration/commits, weak routine linkage and remaining
  measured project families, bounded project probes and storage planning.

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

Committed v42 as e9adc068c (30 explicit owned paths). v43 is active. Workers
may apply their persistent scratch patches: base source-only compiler type
traits/sizeof/alignof, lifetime original allocation-element pairs and exact
vector-delete ownership. Supervisor bumps profile, integrates common readers,
schedules107/117 when both freeze, expands real per-TU failure frontier.

107/117 builds: native SHA
d30313e5df97eba289cc23ba7ce1c35494e54c31b2d8c4e9defc0d57ce29261c;
managed build clean. Typed trait/size/alignment fixture passes 35 checks.
Original array allocation-element pairing with two same-type new expressions
passes 83 checks (dotobj-vector-allocation-pair-v43-107-002), including swapped
and missing original-pair rejection. Initial wrong-owned-call mutation touched
two primary keys; corrected to mutate one protocol row. Real107 core/parquet
units advance to dependent nested member-template identity for pointer_traits
rebind; base worker traces exact original prototype. Lifetime worker implements
vector-delete protocol in canonical sources after107 snapshot. No new commit
yet; all current107 outputs retain frozen tooling107 and managed117.

109/119 focused acceptance complete: source cases including NativeAOT 66,
prior source metadata regressions 73, full vector metadata/generation 123,
native/stock vector oracle 28. Native109 SHA
3b96c468acca7528b3bcb53619b3e1c5960ca22277bfc4e7f23527b509610bed.
Four real ICU units verify/link again to 269 definitions; other 23 representative
units now stop at template-template parameter _MapOrSetSpecifics. New evidence
dotobj-duckdb-traits-vector-delete-20261010.json. Commit v43 next; base prepares
source-template reference/formal graph in scratch, lifetime investigates observed
allocator noncopyable temporary initialization on frozen109 (no runtime detour).

Committed v43 as bd371da99 (27 owned files). Profile44 active. Base worker
applies reusable original template-reference/formal-parameter graph; lifetime
worker confirmed current109 noncopyable generated temporary default-value
rejection and now owns exact original initializer-to-generated-storage support.
Next native110/managed120 when both coherent; preserve all previous evidence.


111/121 v44 acceptance complete: 1,042 broad checks plus 120 focused checks
including NativeAOT. Native111 SHA
bc59d5de9fff15721c5de8b63933baa34db7bc48858aa363ec72903805827714.
Expanded33 actual units: nine packaged, seven verified; seven primary plus two
RTTI providers link to 9,710 definitions in18.2 seconds under2GiB managed heap
limit. No DuckDB C# compilation/run claim. Review v44/seven-units, detailed
receipt dotobj-duckdb-template-formals-temporaries-20261010.json. Commit next.
Workers prepare v45 in persistent scratch: exact dependent nested declaration
relation (base); function-designator reference adjustment (lifetime). Supervisor
investigates delayed defaulted-special-member exception signature capture.

Committed v44 as8036b0304 (27 owned paths). Profile45 active; workers may apply
narrow scratch changes, preserving unrelated edits. Next native112/managed122.


112/123 v45 acceptance: 47 focused checks including NativeAOT. Native112 SHA
0406415bfd2dc418728d3588f7a3f7aa0d07279e6fc1f311a06c83b08ce82832.
Expanded160 completed: 128 packaged, 82 verified. Twelve verified primaries
plus two RTTI providers link to10,779 definitions in19.6 seconds with2GiB managed
heap limit. Current ranks: static lifetime sibling capture33, final noexcept9,
field destructor finalization8, by-value transport9. Queue documented in
supervisor-priorities; full receipt nested-types-function-references-20261010.
Commit v45 next. v46 scratch ready: supervisor original child-list ordinal
attachment + project packaging diagnostics; base finalized field destructor +
separate diagnostic crash guard; lifetime exact generated exception-spec
finalization relation. Actual ConcurrentQueue placement ownership still being
diagnosed, not claimed fixed. Preserve all160 evidence and unrelated edits.

Committed v45 as c360a0d99 (24 owned paths). Profile46 active. Supervisor
applied exact cached static-child ordinal attachment and clearer project failure
stage/worker diagnostics. Workers may apply narrow finalized field/noexcept
patches; next native113 / managed124. No cleanup and no runtime detour.


113/124 v46 integration: expanded 160 completed with 135 packaged and 127
individually verified (previous 128/82). Exact static child-list attachment,
finalized field destructor selection and generated exception-specification
provenance clear the largest selected shared gates. Focused field acceptance
42 including NativeAOT; exception-policy actual-object acceptance 15 with nine
resealed negatives, and bind_recursive_cte_node + RTTI emits 1,043 definitions.
Combined 48-unit generation remains rejected: the same vector constructor has
an unresolved absent defaulted exception spec in one TU and resolved noexcept
in another. No relaxed matching or whole-project generation claim. Lifetime
worker stages a stock semantic query at a valid original frontend boundary.
Broad acceptance001 passed 939 checks then stopped on a stale test SQL INSERT
arity; corrected runner002 is in progress with unchanged113/124 implementation.
Next scratch: base uninitialized-new ownership; supervisor recursive discarded
record result validation (diagnostic managed125), with real four-unit verification
and focused native/stock reducer. No next production changes applied yet.

113/124 v46 acceptance completed: 1,042 unique broad checks across retained
939-check prefix plus 116-check continuation (13 setup checks overlap), all
passing. Explicit SQL columns fix the test schema migration. Focused57 pass;
field lifecycle has NativeAOT. Evidence finalized-semantics-20261010.json.
No cross-TU generation success claimed beyond the validated one-unit1043
definitions. Commit v46 next, then apply staged v47 batch.

Committed v46 as e55465436 (20 owned files). Profile47 active. Base applies
uninitialized-new source ownership; lifetime applies valid frontend semantic
completion query for referenced defaulted exception specs. Supervisor applies
recursive discard-result validation and for-step address preservation. Next
native114/managed126 (125 is prior-profile scratch discard probe).


v47 current tools: native114 failed C++ compilation (new writer scope); corrected
native115 SHA346ef68ae0afc61e0fc7b17572546744a7517d7b7fc5df255dbef2bd2fc115a4
and managed126 passed uninitialized-new50 and discarded-record33, both including
NativeAOT. Its 500-unit inventory runs at expanded-frontier-115 (two jobs,8GiB
per native process) with managed126 incremental verifier. This is broader
inventory, not final exception-policy acceptance. Original query placement was
proved too late by GDB: forcing a defaulted body already prelowered its class.
Native116 moves the same guarded stock semantic query before body generation;
SHA1acd4a5cb9b475e9989cd75a41c99ba54472988612ef792075ffb3aa91f9fce8.
Managed128 adds observed RHS-first overloaded assignment-call sequencing;
its prior diagnostic127 source and exact native/stock/JIT reducer are retained.
116/128 defaulted policy paired-TU29 includes NativeAOT; provenance15 includes
nine resealed negatives. Actual pair retry uses6GiB after honest2GiB bad_alloc
resource failure. Operator-call-order116 passes16 incl NativeAOT. Broad116/128
runs dotobj-full-v47-001. All receipts persist; no v47 commit until actual pair
and integration evidence is assessed. Base stages next deferred address-only
imported routine entities: both return/parameter reducers pass18 native/stock
oracles, original incomplete types must be resolved from complete linked ABI.


v47 accepted: native117 SHA828753d236d0139739f7751c49cd045b4a08e05f4d5a303dc97210d964592e74
and managed128 pass all 1,042 broad checks. Focused 143 checks cover scalar
uninitialized placement-new ownership, nested discarded record expressions,
RHS-first overloaded assignment calls, and lazy referenced defaulted exception
specification completion, including NativeAOT and malformed-metadata rejection.
Stock EDG explicitly supports prelowered class base fields during exception-spec
completion; query is guarded by original routine ABI, not class prelower state.
Unused ill-formed dependent defaulted initializers remain lazy. Actual paired
DuckDB vector constructor policies now agree; combined link advances to a
ReplacementScan callback-layout mismatch. No combined generation claimed.
Broad001 failed only because copied harness lacked stock cpfe; corrected002
passes, both retained. Expanded500 inventory still uses earlier115/126.
Evidence: dotobj-duckdb-allocation-policy-20261010.json. Next coherent batch:
deferred external incomplete by-value declarations plus opaque callback storage,
owned primary virtual-base vptr access, and floating classification intrinsics.


Committed v47: 83f0e6dc0. Wider500115/126 complete: 456 packaged, 447
verified, 38 exporter gates, one package failure, five test-configuration
failures, no exporter crashes. Updated ranked queue from exact first failures.
v48 native118 SHA861a493032d72ebb8979a4d7771a2e4ded270ac1286acc7b593a220234ccbd63
and managed131 built. Includes deferred imported declarations and partial callback
storage equivalence, typed floating classifiers, owned primary virtual-base vptr
path, and exact constant byte-offset alignment. Alignment scratch132 verifies
actual task_scheduler and passes native/stock/JIT/AOT16 plus strict dynamic and
misaligned field-address negative18 suite. Focused v48 deferred parameter38,
partial storage36, partial complete36, floating26 pass with NativeAOT.
Return/virtual-base initial harness selected wrong RTTI provider pair; retained
failed bind logs; corrected118b uses libcxx typeinfo.cpp + abi_typeinfo.cpp,
retry in progress. Current31 real-unit probe targets20 by-value first gates,
nine managed rejects and two cross-TU policy/layout units. Next managed fmt
array-view support staged by base worker; lifetime worker reduces lowered raw
record transfers after explicit conditional-return construction. No full
DuckDB generation/compile/run claim.


v48 accepted native119 SHA604fcfd24fe3db698345e98955503b2fd15e64a99df5aedfd435ec32fd93e223
and managed134: 257 new focused checks (all eight suites include NativeAOT)
plus 60 existing packed-layout and 50 record-transport checks. Original return
fixture now verifies independently emitted Base vtable/RTTI roots and passes40;
root criterion follows stock lowered external vtable definition/needed flags.
Array-view negatives reject before either exact storage or immediate array-decay
contract; corrected diagnostic expectation, preserved failed attempt. Exact
major-bound view support verifies fmt; known external extent obligation retained.
All31 targeted real objects package with118 and verify (30 with131, fmt with133).
Actual cross-unit pair plus RTTI providers emits6,909definitions with134, review
link dotobj-duckdb-v48/cross-unit-pair. Combined31 rejects FileMetaDataC1 callable
exception policy vs lowered imported-wrapper policy; base worker stages distinct
source contract relation, not ignoring mismatch. Full2635 inventory119/134 runs
two8GiB native jobs with incremental verification. No full generation/compile/run.
Supervisor corrected historical package diagnosis: test_parallel_csv has static
array destructor element size0 from typedef size observation, not an asm
operand. Tiny typedef-array reducer reproduces exactly; next scratch fix will
retain typedef source identity and use actual underlying element layout.
Evidence dotobj-duckdb-deferred-callbacks-20261010.json.


Committed v48 aa4d098e0. Profile49 candidate native120
SHAcb330c748f61d440f43e9780f0f6ccfb484789060c2fb8088e666a2c9647d78a
and managed135 built from frozen snapshots. New per-node record storage-copy
proof retains stock evaluation-save/class-assignment semantics without granting
source copy operations; full extent excludes stock tail-padding memcpy path.
Complete constructor callable noexcept derives only from verified original
primary origin; lowered body policy remains separately retained. Static array
destructor observes skip_typerefs(element) for layout while preserving source
alias identity. Focused PASS30 record copy, PASS45 constructor contract, PASS17
static alias array, all including NativeAOT. Initial test-only failures retained:
EDG raw assignment flag differs from test assumption; missing genuine sizeddelete
in ctor fixture now supplied through free. Actual12010-unit probe packages7;
TPCH DBGenContext assignment, async anonymous-record assignment and unique_ptr
construction remain. Seven successful units+RTTI all individually verify135,
combined generation pending. Broad1042 suite running dotobj-full-v49-001.
Full119 inventory resumed with4 bounded8GiB jobs after recording a stop and
preserving in-flight attempts; receipt dotobj-full-119-resume-concurrency.json.
Host had124GiB total/~81GiB available. Frozen compiler/manifest/options unchanged,
--resume validates original fingerprints; max native budget32GiB. No cleanup.


v49 accepted: native120 and managed136 retain exact complete constructor AND
destructor callable contracts, while preserving raw lowered body policy; deleting
entries remain excluded. Focused140 checks (30 storage copy,45 constructor,48
destructor,17 alias-array commands) include NativeAOT and strict resealed negatives.
Broad1,042 existing checks all pass, plus one continuation linkage check; exact
SHA-linked prefix/continuation receipt dotobj-full-v49-acceptance.json preserves
all failed diagnostic/private-harness attempts without semantic relaxation.
Seven actual units plus two RTTI providers now generate17,316definitions; review
link dotobj-duckdb-v49/seven-units. No DuckDB compile or run claim. Remaining
three of ten probes need further selected record transfer/construction evidence.
Next50 stages virtual-array deletion callback protocol (17ICU first gates),
anonymous implicit constructor spelling, incomplete opaque handle alignment,
and exact parent-owned label lifetime overlap. Full119 inventory continues
four bounded jobs; latest measured1,199processed1,172packaged1,166verified.
Evidence dotobj-duckdb-storage-copy-contracts-20261010.json.


Committed v49:727321039. Profile50 integrated virtual-cookie-guarded array
destructor metadata and exact graph validation; source-captured anonymous
routine names; opaque-handle unknown alignment; parent-owned block-after-label
lifetime overlap. Managed138 built and frozen; native121 build pending after
two retained observer-scope compile failures (cookie type now explicitly passed
from the stock integer_type(targ_size_t_int_kind) query). No frontend semantics
changed. Affected array/record manifest23units and separate managed5-unit probe
will measure real generation; dependent source-parameter diagnostic now records
name/depth/position/constraint for the existing precise rejection.


v50 accepted native121 SHA005cd33d8e6495aabf4862a59a66b33468c298d80cd4a7d9a2910af41c16005a
and managed140. Focused588 checks across seven suites; anonymous24,opaque18,
label23 include NativeAOT. Virtual default169 and late-member-view195 preserve
source-hidden graph/generation, native/stock semantics and strict negatives;
production vector runtime remains unresolved. Direct-vector117 and array/VTT42
regressions pass. Complete original destructor needs no generated ABI origin;
late raw member callback view matches exact original receiver/policy and finalized
destructor ABI, never a generic pointer relaxation. All17ICU array gates clear.
All25objects (23actual+2RTTI) verify140 and combined generation emits5,943
definitions; review dotobj-duckdb-v50/combined23. Five of six record followups
still require exact stock transfer provenance. Full119 latest1,655processed,
1,615packaged,40fails; managed1,602verified,7rejects, three now-known label
overlap rejects included. No fullDuckDB compile/run. Next ABI batch: observed
member-pointer representation value transport, result-buffer forwarding thunks;
worker also proves stock add_bitwise_copy/add_init_assignment storage copies.
Source constraints diagnostic shows three _Iter concept constraints and regex
dynamic typed exception specification in four libc++ source probes; retain full
required metadata, lower priority than current measured project transfer gates.
Evidence dotobj-duckdb-virtual-array-provenance-20261010.json.


Committed v50: a3e4ffbdc. Candidate v51 native122
SHA1a135f5a24c7782fe6f5f30e76fed4a6979c9521d5476dc67644f5cad4a216a1
and managed142 are frozen. Exact stock complete-object initializer-storage and
bitwise-initialization observations added; scalar member-function-pointer ABI
value transport preserves original member type/owner; result-buffer thunks
forward the unchanged caller destination and adjust only this. Supervisor
reduced six missing source destruction types to deliberately excluded arrays
in the existing frontend new-without-initialization recipe. The same exact
no-initialization source relation now requires matching array shape; existing
false scalar-to-array mutation remains rejected. Fixed-bound array focused50
passes NativeAOT; dynamic-bound variant packages/verifies/generates but runtime
__cxa_throw_bad_array_new_length remains unresolved (retained failed binding).
Member-pointer initializer validation now recognizes original scalar member
pointer backed by the observed ABI record, not a fabricated source class.
Focused member-pointer31 and result-wrapper27 pass with NativeAOT. Original
uninitialized-scalar regression passes. Extended record fixture native/stock
passes, but exact new proof-kind reachability acceptance still in progress.
Current38-unit original-source probe selected all first failures from the
1,992-unit frozen119 snapshot except accepted virtual-delete gates, plus managed
rejects; manifest dotobj-v51-frontier-manifest.json retains original2635 provenance.
Incremental122/142 verification currently27/27 passes, including uniset pair,
number_patternmodifier, DBGen, TPCDS, parser and uninitialized-array users.
Remaining explicit gates include source routine ABI, generated exception member
origin, lambda parameter identity and initialized-array rollback disposition.
No full DuckDB generation/compile/run claim.

Space preservation: completed full094 and expanded115 corpora moved to
/home/marius/p/dotcc/edgcpp/artifacts/campaign/retained-frontiers-20261010.
Existing /mnt/storage campaign paths remain symlinks; every file/size inventory
and summary/compiler hash verified. No intermediate removed. Receipt
frontier-relocation-20261010.json. Free space after relocation:60GiB persistent,
81GiB workspace. Active full119 untouched.


v51 accepted: all 197 focused checks pass, including NativeAOT in the four
new suites. Final copy fixture requires both initializer-storage and bitwise-
initialization observations. All 30 packaged real units in the 38-unit selection
verify under managed142. Combined generation with two matching RTTI providers
and real PhysicalOperator/PhysicalJoin providers emits 24,797 definitions.
Review: artifacts/review/dotobj-duckdb-v51/combined30. Earlier provider-incomplete
link attempts retained; no declaration policy weakened. Evidence:
dotobj-duckdb-initialization-transports-20261010.json. Next profile52 workers:
base owns exact captureless lambda static-entry origins/formals, lifetime owns
dynamic-array zero-initialization rollback disposition. Supervisor takes TLS
local-static guards. Synthesized zero-model-plus-ctor vector callbacks retain a
separate required generated origin; ordinary source routine ABI stays strict.
No full project generation, compilation or execution claim.


v51 committed 8abe30721. Candidate v52 final native125 SHA
4acb0510dbd9e44bbe6b4a571dcf1ef94dae7b73484ff21a278b48ca8186c42b
and managed146 are frozen. Lambda origin/forwarding, exact dynamic-array zeroing
rollback disposition, per-thread byte guard publication and preserved thread
destruction registration implemented. Stock retains a TLS guard abort region;
its byte reset is preserved without a process monitor. Shared runtime uses the
same thread callback registry as __cxa_thread_atexit and releases TLS afterward.
Focused 274 checks pass (36 lambda, 40 zero-array, 35 scalar TLS, 38 nontrivial
TLS, 125 global-lifetime regression); lambda and TLS include NativeAOT.
All 136 prior failures/rejects re-probed, with exact newer lambda/TLS correction
follow-ups: 67 package, 65 verify; 69 exporter/frontend gates and 2 managed
rejects remain in that selection. Complete frozen baseline: 2,509 package and
2,499 verify out of 2,635. Current 53-library-unit combined generation underway.
Next ranked work: 38 constrained source metadata (base), 7 variadic indirect ABI
(supervisor), owned generated array model/ctor callbacks (lifetime). No real
DuckDB compiled/executed yet. Full latest queue and receipts in priorities doc.


v52 accepted: combined 53 verified library units plus four actual providers
emit 33,105 definitions. Review artifacts/review/dotobj-duckdb-v52/combined53.
Evidence dotobj-duckdb-lambda-thread-lifetimes-20261010.json; all 274 checks pass.
Next native cut126 / managed147 reserved for generated array callback provenance,
variadic ABI transport and source-constraint diagnostics. Concept graph remains
worker scratch until complete typed schema/capture/verification are ready.


v53 accepted with native126 and managed147: 88 focused checks (59 generated
array model/constructor, 29 variadic indirect transport). Variadic direct and
function-pointer calls, fixed indirect parameters, result buffers and va-lists
pass native/stock O0/O2, JIT and NativeAOT. Array callback checks preserve exact
model initialization/copy/constructor order, null data-member pointers and
rollback provenance; runtime provider closure remains deferred. All nine real
selected array/RE2 units package and verify; combined nine plus four providers
emit 9,930 definitions. Review artifacts/review/dotobj-duckdb-v53/combined9;
evidence dotobj-duckdb-generated-array-variadic-20261010.json. Across the 136
prior failures: 76 package, 74 verify, 60 export/frontend gates and two managed
rejects remain. This is not a latest-profile full 2,635-unit rebuild. Next:
source concepts (38), dependent variable references (4), weak linkage (4),
ART cleanup diagnosis, then remaining measured families. Full generation is
still the checkpoint before runtime closure; no full compile/run claim.


v53 committed fbaf7b0a5. v54 accepted: native128, managed149 weak linkage and
managed150 C pooled-record reader. 212 checks pass (163 weak, 27 variadic
regression, 22 C compound literal); weak and C suites include NativeAOT. Actual
libc++ new/verbose_abort now package, verify and generate 26 definitions with
RTTI providers; actual musl vfscanf now verifies and generates seven definitions.
Review artifacts/review/dotobj-duckdb-v54; evidence
 dotobj-duckdb-weak-and-c-initialization-20261010.json.
Across all 136 baseline failures, 78 package and 77 verify; 58 export/frontend
gates and one managed reject remain. RE2 set/re2 advance from weak linkage to
exact generated TLS routine ABI provenance. ART is reduced to default-argument
Argument temporary copied into generated Element constructor adapter for
new Element[n]; existing source-lifetime ancestry is under investigation.
Base worker finishes coherent full source-concept graph in scratch. No full
project generation/build/run claim; continue toward the generation checkpoint.


v54 committed 6fc0c24f2. Candidate v55 source-concept graph integrated; native129
and managed151 compile. Native129 SHA
98d16e392c7a8d8656fde05138547db5fe5ca89b5344237518b8dbe51c69e3f5.
First fixture fails the still-unrepresented dependent function-template lookup
constant (configured tpck_template_ref); three real concept representatives reach
ordered pack/substitution argument metadata. Base worker batches both exact
families before next native cut. No source metadata omission accepted.
ART ancestry diagnostic129 confirms an intermediate copied DIP already has an
original source lifetime and destruction owned by the caller. Lifetime worker
implements explicit generated-default-adapter execution adoption, retaining the
original owner/type/ordinal and exact copy chain; no new fake source routine.
Supervisor reserves TLS variable-wrapper/initializer ownership next; stock
bodyless aliases and final storage resolution are recorded in retained scratch
 dotobj-tls-wrapper-next/design.md. Native130 / managed152 are next available
cuts. No new committed project-level generation result beyond v54.

Candidate v55 progress (not acceptance): native130 / managed152 preserve exact
concept packs and dependent function-template lookup, including empty argument
boundaries. The focused source-only fixture now exports but finds an implicit
first constrained-type binder verifier gap; three actual representatives advance
to std::common_reference class-template pack capture. Both are integrated for
native132 / managed155. Source graph declarations remain fully owned, with no
fabricated selected overload or placeholder constant.

Native131 / managed154 add observed copied-default-argument execution ownership
and the initial constant-TLS wrapper/alias graph. Actual ART now packages for the
first time; reader/emission and focused throwing cleanup tests are in progress.
RE2 wrapper capture exposes an overly restrictive compiler_generated flag check:
stock make_rout_entry factories do not set that flag. The exact factory identity,
variable, signature and scope checks remain; only the unsupported flag assumption
is corrected in native132. TLS reader allowlist omission in managed153 is fixed
in managed154; the failed attempt is retained. First typed dynamic-spec audit
confirms remaining ios/regex gates are legacy throw(), whose unexpected semantics
must not be silently mapped to noexcept. No new verified actual-unit count yet.

Frozen native131 SHA97c7e045d381c3ad0b3690e63898c764e36bd499c85f6a3dde9870e19f389ba8.
Worker base owns class-pack follow-ups and four dependent-variable declaration
reference bridge; lifetime owns ART focused JIT/AOT acceptance; supervisor owns
TLS exact-body/runtime tests and integration. Persistent disk42GiB free/workspace
80GiB: do not start a second full 70GiB corpus on persistent storage. No cleanup.


ART131/154 accepted focused70 checks, including stock/native O0/O2, JIT,
NativeAOT and13 resealed ownership negatives. Actual ART verifies and emits
3,714 definitions with the two RTTI providers (dotobj-art-managed-131-154).
This is a new actual unit result but latest-profile re-probe still pending.

TLS132 still hit the same ABI gate after removing the unproven generic flag
assumption. Exact133 diagnostics identify the real issue: source-function=1
for _ZTW5value; the source walker incorrectly captured the generated wrapper as
an original declaration. Native134 reserves the stock wrapper at creation and
excludes only factory-owned TLS routines from original-function capture. No
source graph is erased and no frontend semantics are modified. Managed156 also
uses the actual owned `ne` opcode and accepts the exact address-of-storage node.

Source constraints132 progress to class-valued constexpr aggregate at libc++
ranges::swap; the fixture has a dependent variable-template boolean-result
validation gap. Worker batches typed aggregate initializer/decl-reference graph
and context-specific substitution validation. These remain explicit failures,
not accepted coverage. Next coherent graph cut follows TLS134.


Profile55 acceptance: native136 (SHA39f72d7272683c6bc5442831e724e71fe73a2145b4d2b973b7afed941e100c49),
managed159 final source reader.721 checks pass: graph107, copied default-argument
cleanup70, TLS53, initializer-list40, existing source/template regression451.
TLS and initializer-list suites use frozen managed157/158 respectively before
the disjoint new semantic-constant whitelist correction; final source and ART
suites use159. Focused suites include JIT/NativeAOT and resealed negative objects.
Two old test assumptions were corrected against retained evidence: sizeof/alignof
are owned constant-expression nodes, and variadic member ABI is supported since53.

Actual ART:3,714 definitions; RE2 re2.cc+set.cc:931; caching-file-system-wrapper
DuckDB test:1,773. All four verify; last baseline managed reject is cleared.
Cumulative prior136problem follow-up now81package/81verify,55export/config gates,
zero managed rejects. Not a full latest-profile rebuild. Review links under
artifacts/review/dotobj-duckdb-v55. Largest combined project still53units33,105defs.
Current queue updated in duckdb-supervisor-priorities.md:38alias/concept source
metadata,4static-member template references,2dynamic throw specs,3singleton
provenance/layout families,8test-profile frontend issues. Dynamic-spec patch is
READY in worker scratch, alias/static-member closure continues. Commit55 then
native137/managed160 for next coherent profile56 cut. Continue autonomously.


Profile56 accepted: native137/138 and managed160/161.210 checks pass: static
class-template members37 (native/JIT/NativeAOT), dynamic specifications68
(native/stock semantic oracle, source-hidden generation,10 resealed negatives),
and existing source constraint graph regression105. Original static member
references retain exact class/template origins without invented member formals.
Dynamic throw lists and observed body boundaries remain distinct from noexcept;
the required shared unexpected/replacement provider stays explicitly unresolved.
Empty bodies omit the catch wrapper while retaining boundary metadata. No
translated dynamic-spec execution is claimed.

Actual patas/alprd/chimp/alp all export and verify; combined C# generation emits
3,314 definitions with two RTTI providers. Review: artifacts/review/
dotobj-duckdb-v56/static-members. Cumulative retained-version export coverage is
2,584/2,635;85/136 original problem units now verify,51 export/config gates remain.
This is NOT one coherent latest-profile corpus. Current queue:40 shared alias/
type-operator source metadata,3 singleton exporter families,8 test frontend
profiles. Base worker owns the40-unit source graph; lifetime worker diagnoses
types.cpp exception origin, test_config rollback and abi_atomic layout; supervisor
owns test-profile provenance and integration. No cleanup, no runtime detour.


Portable test-profile milestone (schema56 unchanged): canonical native139
SHAe36e798cceefa77e4ce26faf8682d0ac6c9634e22ceb65df6538a723b4772cd6 and managed161
export/verify all seven selected former frontend failures and jointly generate
5,662 definitions. Full manifest still2635units,148tests, unchanged language
standards, no omitted tests or host OS macros. Catch disables only OS crash
interception and a GCC-only diagnostic probe; existing reviewed terminal
adaptation is reused. Shared headers add fork/kill/bzero declarations, with
unresolved partial imports retained. No process-test execution claimed.

Canonical full next-run manifest: dotobj-test-profile-002/project.json. Previous
manifest003 and source files are intact; unrelated pending PCG adaptation was
not adopted. unittest now passes parsing and reaches source-parent static
destruction ownership. Cumulative exports2591/2635 across retained versions;
remaining44exporter gates =40alias/type-operator graph + types.cpp destructor
origin + test_config member rollback + abi_atomic alignment + unittest static
parent ownership. Workers are implementing exact observed source/cleanup facts.
Review artifacts/review/dotobj-duckdb-v56/tests. Latest full-profile corpus and
full combined link/compile/run remain outstanding. No cleanup.
