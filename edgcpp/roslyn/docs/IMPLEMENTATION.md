# RoslynObj implementation ledger

**Resumed at the user's explicit request on 2026-10-13.** Historical restart state
is saved in local historical pause notes, with both worker notes. Both implementation
lanes are active; full approved M0–M5 work remains required.

**Current verified boundary: 263 tables / 39,144 stable kinds; final137 PASS1795,
self/WITHOUT ROWID PASS73, build zero warnings/errors, all four strict compilers,
316 direct Git-pinned inputs and 25 canonical backend files per stage.** The
coordinator is recording this significant milestone commit, then both workers
continue the reviewed common metadata contract. Full M0–M5 remains in progress;
current field counts are 567 verified /550 mapped pending validation /136 mapped
pending implementation /493 raw pending, with exact exclusions separate.

The user approved phases 3–4 on 2026-10-12, including complete M0–M5 delivery.
The accepted profile prioritizes stock EDG lowering and documents lost original
source queries. This ledger records evidence and outstanding work; a schema or
vertical slice alone does not complete the approved scope.

On 2026-10-12 the user explicitly excluded C++/CLI support. C++/CLI and C++/CX
implementation and metadata/profile gates are removed from the task. Native
Microsoft C++ extensions and source-semantic Microsoft ABI coverage remain
required. Existing shared adapters/catalog IDs are retained without claiming
managed-dialect support.

## Ownership

The coordinator (GPT-6.1-Sol, high) owns the shared schema, stable catalog, coverage
matrix, integration, compiler scheduling and commits. The native implementer
(GPT-6.1-Sol, xhigh) owns backend/build/native fixtures; the managed implementer
(GPT-6.1-Sol, xhigh) owns managed projects/tests. The primary agent owns user
communication and independent review. Workers never commit independently. After the initial reader checkpoint, the managed implementer also owns `backend/roslyn_templates.h` and `backend/roslyn_expressions_extra.h`; the native implementer integrates and serializes compiler builds. After `8d9c4bd88`, the managed implementer also owns extracted `backend/roslyn_execution.h`; shared initializer/lifetime/EH policies stay native-owned.

## Source and environment

- Pristine upstream: `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`, EDG 7.0,
  `/tmp/roslynobj-edg-upstream-20261012`.
- Independent staging/build created by the new `build/prepare.py`; core files
  checked by SHA256 and Git blobs. Existing modified embedded EDG is preserved.
- .NET SDK 10.0.112, target net10.0 / C# 14.
- Managed provider Microsoft.Data.Sqlite 10.0.12 restored successfully.
- Native SQLite 3.45.1; format requires SQLite 3.37+ for STRICT tables.
- Stock pinned `edg-cmakedef` tool blob
  `975ebb61fc22af5565c78395ff390c10662d7b5e` verified and copied into staging.

## Milestone state

| Gate | State | Evidence / remaining acceptance |
|---|---|---|
| M0 | In progress | Pristine pinned core verified unchanged; independent compiler, global backend wrapper, lowering1 / tables1 / fullEH0. Basic, C++20 EH/virtual/template, exact scalars and macro fixtures publish atomically with zero unresolved FKs. Valid-compilation missing-parent and destination-directory failures return nonzero, preserve existing contents and leave no temporary output. Remaining: all compatibility profiles and complete EH adjunct/profile probes. |
| M1 | In progress | Generated STRICT DDL, authoritative explicit stable IDs, exact scalar/unit contract and valid managed fixture exist. Full field coverage worksheet inventories 1,993 declarations, including plain structs and multi-declarators. Complete field/property mapping and all configured scalar/EH forms remain required. |
| M2 | In progress | Verified native→reader checkpoint: selected add call, initialization execution containment, ordinary symbols/types/control flow, source positions and current signatures. Broader complete-family field audit remains required. |
| M3 | In progress | Real C++ fixture exports 8 physical regions /6 actions /32 EH operations /8 lifetimes; reader navigates all actions. Observable destructor array fixture now produces count3/element-size4. Template/class/ABI and remaining cleanup adjunct completeness remain open. |
| M4 | In progress | Exhaustive active adapters and all planned compatibility profiles with valid upstream inputs. |
| M5 | In progress | Bounded managed page/row caches, sequential keyset paging and batched children tested. Full large-TU measurements, independent-object packaging and determinism remain required. |

## Verified initial checkpoint

Commit: `ef479e0f8` (2026-10-12). Only owned RoslynObj/design files were staged;
unrelated dirty EDG and workspace changes were preserved. Both implementers
continue after this checkpoint; it does not finish phases 3–4.

The managed harness passes 74 assertions with zero build warnings/errors, including
all four real EDG objects (`basic`, `m0`, `exact`, `macros`) and a valid normalized
fixture. Exact evidence includes unsigned 128-bit magnitude, extended finite
significand/exponent, negative zero, infinity, quiet NaN payload 801, target-byte
strings including NULs, distinct constant identities sharing one string value,
EH region zero versus the configured sentinel and cleanup actions.

Macro assertions resolve SUM and INC spellings, definition/undef/redefinition,
canonical arguments, the INC→OUTER→INNER parent chain and `#line` mapping to
`logical.cpp` line 200. Physical versus presumed-file/include refinement remains
part of provenance acceptance. No full token/trivia/source-tree claim is made.

Storage inspection passes all four objects with no BLOB/REAL semantic columns
and zero foreign-key failures. Initial 4 KiB-page objects measure 1,150,976 bytes
(basic/macros), 1,224,704 bytes (m0), and 1,159,168 bytes (exact); these are
measurements of the current slice, not final size commitments.

Bounded reader tests measured 1,001 variables /111 commands /12 ms and 2,008
ordinary operations /496 commands /49 ms. A later maximum-page test verifies
bounded prefetch and companion batches of at most 900 parameters. Initial page
size packaging comparison on a 339-entity basic object measured 456,704 bytes at
1 KiB, 677,888 at 2 KiB, 1,138,688 at 4 KiB, and 2,080,768 at 8 KiB. Large-TU
throughput must be measured before changing the default.

## Current next slice

Nine genuine outputs now export with zero foreign-key failures: `basic`, `m0`,
`exact`, `macros`, `objects`, `templates`, `attributes`, `types`, and `scale`. The managed
self-fixture plus all nine real outputs pass 113 assertions. Template evidence includes
six definitions, five headers, six parameters and twelve instances, exact default
argument 3, alias/variable-template navigation and retained specialization flags.
The specialization relationship projection is verified; broader partial/template-template/pack and prototype-role fixtures remain required.

The managed implementer extends retained templates and residual expression arms;
the native implementer integrates both headers while completing remaining native
unions, exact discriminants and profiles. The schema catalog now contains 38,747
explicit stable kinds, including 36,979 pinned builtin-function tags; bulk extension
rows are published only when used. The field and kind worksheets retain every
unresolved row as pending rather than claiming support from table presence.

The first template-family field batch closes 21 direct template mappings with valid native
and managed evidence; 28 template mappings still require their specific guarded
or nonzero fixtures. Eight canonical parameter-descriptor mappings are verified with actual nonzero
retained instances; additional flags/default/extension branches still need fixtures. Explicit exclusions distinguish typed linked-list plumbing,
frontend template symbols, cache checksum and C++-generator-only fields. All other
unclassified fields remain required work. Observed kind tags are tracked separately
from complete payload/navigation coverage.

Four common type/expression mappings are also verified: exact type size/alignment,
incompleteness and required expression type navigation. Inclusive token-character
range endpoints follow stock `end_pos_curr_token` (`lexical.h:2949–2957`);
captured columns remain logical columns rather than byte offsets. Scoped
availability metadata still requires implementation and real profile evidence.

The generated large fixture retains 76,465 entities and all 1,000 ordinary function
definitions. Native wrapper wall time is 0.527 seconds; managed traversal observes
7,023 operations and 1,001 calls in 648 ms. These measurements are initial warm
single-run observations, not final cold/warm/RSS acceptance.

The reproducible packaging harness compares valid snapshot copies without changing
the source object. Current 76,465-entity output measures 4,524,032 / 4,720,640 /
5,238,784 / 6,389,760 bytes with 1 / 2 / 4 / 8 KiB pages, respectively. Full entity,
constant, atom and structural-edge scans take 26.96 / 23.20 / 23.81 / 23.36 ms.
The 341-entity output measures 497,664 / 733,184 / 1,232,896 / 2,252,800 bytes.
Managed traversal and native write throughput at alternate page sizes still need
measurement before selecting the writer default.

## Shared contract

`schema/roslynobj.sql` is generated from `schema/generate.py` and the authoritative
append-only `schema/kinds.tsv`. Existing catalog IDs never depend on design prose
or EDG enum ordinals. Source catalogs include every pinned builtin-function discriminant; large extension catalogs persist only the actually referenced rows. DDL uses scalar integer/text columns, typed foreign keys,
explicit semantic flags and ordered child rows. Canonical decimal TEXT preserves
target quantities beyond signed 64-bit range. No semantic JSON/BLOB/struct dumps
are allowed. Exact value subtype rows reference `constant_value` identities.

Execution containment (`structural_child`) is distinct from semantic references
and backing metadata. `representation_role_id` labels execution, metadata and
source-semantic forms. Record layout size/alignment project the associated type
row; nonvirtual size/alignment are separate EDG facts. Generated EH table variable
identities project normalized records through `eh_table_projection`, avoiding a
second stored aggregate copy. Source-selected virtual-call declarations and
private original-type/signature pointers are not reconstructed.

Structured content edges refer to child constant values and their retained types;
`constant_component` separately preserves every nested occurrence and its source
identity. This prevents exact aggregate/repeat/dependent content sharing from
substituting the first occurrence's source graph. Fixed-point values denote
`scaled_integer * 2^-scale`; string units identify target bytes explicitly.

## Current required work

All pending coverage rows, native/managed active union payloads, exact float
representations, profile probes and M0–M5 acceptance remain required. Inactive
configuration and parser/cache exclusions need concrete field-level reasons.
No successful input may omit required data under an optional-capability label.

## Expanded primary checkpoint

Commit `627a85e4b` contains 192 normalized tables, 38,747
append-only stable kind tags and 1,221 generated managed properties. Tests pass
113 assertions across a valid normalized fixture and nine genuine EDG objects.
Native storage inspection confirms complete headers, no semantic BLOB/REAL columns
and zero foreign-key failures. Ordinary output failure cleanup passes again.

New evidence includes named class/enum/typedef descriptor correspondence; distinct
enumerator symbols and exact constant occurrences; indexed enum references;
declaration-node GetSymbol/GetType; regional initializer projection; source and ABI
parameter roles; exact structured content sharing with distinct nested occurrences;
and actual retained template specialization navigation. Plain, qualified and aligned
typedef size/alignment queries match EDG-evaluated sizeof/alignof constants. Ordinary
quantities use stock size_of_type/alignment_of_type; optional differing descriptor
quantities preserve raw IL fields without duplicating equal facts.

Current scale capture retains 77,507 entities, 5,006 constant occurrences and 1,004
explicit region roots. The 4 KiB-page artifact is 5,746,688 bytes. Export wall time
is 3.796 seconds; reader traversal observed 7,023 operations /1,001 calls in about
722 ms using 38,997 commands. These are measured open M5 optimization targets.
The component child-FK index reduced an earlier 4.621-second export to 4.260;
nullable unnamed constant names reduced it further, but targeted indexes alone
have not eliminated the regression. Broad index proliferation is not assumed.

Coverage at that checkpoint recorded 44 primary mappings verified, 118 implemented mappings
awaiting their specific fixture/profile evidence, 18 mappings awaiting implementation,
and 1,696 raw pending fields. Twenty-three managed-dialect rows are excluded by
the user's explicit scope change; other field exclusions have specific reasons.

This checkpoint did not close M0–M5. A separate retained fixture reached an
unimplemented statement kind and fails before publication, as required. Prototype
auxiliary role closure, evaluated-edge promotion, full native/source profiles,
remaining type/execution/common fields, scoped availability, typed visitors and
large-input performance remain required. Unvalidated retained/template-edge input
fixtures are outside this commit's verified fixture batch.

The checkpoint independently verifies 176 materialized stock source inputs against
the pinned Git blobs without changing staging; the expanded preparation hash gate
is included for subsequent full builds. Declaration projections without a Source
are semantic/generated records and do not claim retained original-source
occurrences. Exact source-origin/range interning was still pending at this checkpoint.

## Verified native/profile checkpoint

Commit `2939f715e` records the following tested 198-table snapshot.

The current contract contains 198 tables and 38,888 stable kind tags. The reader
generates 196 detail classes and 1,280 properties. The final checkpoint passes 240 public
assertions across the valid self fixture and 26 genuine native/profile objects.
All inspected objects publish complete headers with zero FK failures and no
semantic BLOB/REAL columns. The primary fixture batch includes
retained coroutine copies, extended template metadata and active
float16/float128/bfloat constants. Source-profile statements, VLA, native Microsoft,
template-edge and expanded retained fixtures also publish with zero FK failures.
Strict source prototype metadata closure, throw/condition/catch initialization
execution exactly once, actual coroutine parameter copies, module imports,
EntryPoint and bit-precise 9/129 versus 16/136 storage bits all pass. Remaining
field coverage and profile gates are still open.

Native exact-content names and source-origin/range tuples are now interned while
symbol, declaration and operation occurrences remain distinct. The current scale
fixture retains 71,446 entities, 1,021 names, 13,038 origins and 11,022 ranges;
export takes 3.127 seconds. Scoped availability is populated, including 105 rows
in basic and 1,646 in retained. Exact state/reason API validation remains required.

The inventory includes source positions from `basics.h` as well as guarded IL
structures: 2,000 fields, 116 verified mappings, 162 implemented mappings awaiting
specific evidence, 17 mappings awaiting implementation and 1,524 raw pending
fields. Exact source/header audits preserve gaps in physical-versus-presumed
files, include flags and source declaration history. They exclude only typed
list storage, documented frontend/bookkeeping state and the user's managed-dialect
scope. Current family closure continues alongside implementation.

The packaging harness now also compares all 61 composite-primary-key tables
using WITHOUT ROWID, retaining rowid identity tables. Valid copies preserve every
table's row count and have zero FK failures. At 4 KiB, current scale drops from
5,910,528 to 5,185,536 bytes and basic from 1,384,448 to 1,130,496 bytes; ordinary
scans remain approximately 24 ms and 0.23 ms. Native insertion and WITHOUT ROWID managed
navigation comparisons remain required before choosing that storage default.
These measurements use valid snapshot repackaging, not malformed inputs.

Final snapshot verification independently compares every one of the 183
materialized stock files in each of four staging profiles with pinned Git blobs.
Ordinary valid-compilation publication failure tests preserve existing
destinations and clean temporary output. Exact dependency/incompleteness, source
physical mapping/declaration history, ordered bases/classes, maximum native guard
profiles and final storage/default measurements remain required, as recorded in
REVIEW.md. No frontend module-entity object is dereferenced.

On valid rowid copies at 1/2/4/8 KiB, full managed scale sweeps issue about
22,160 commands and 163,078 rows per pass; post-JIT timing is 0.87–0.89 seconds,
RSS 104–113 MB and caches remain bounded at 4,096 rows/128 pages. Those copies
measure 5.11/5.33/5.91/7.16 MB. Cold-start JIT and native insertion remain separate
measurement gates; no universal storage default is inferred from one sample.


## Source/type/identity integration in progress

The frozen shared contract now contains 232 normalized tables and 39,060 stable
append-only kinds. Both workers are integrating this batch; its genuine native
refresh is pending, so it does not replace the verified checkpoint above. The
managed 231-table precursor built without warnings/errors and passed 70 self
assertions on each of rowid and composite WITHOUT ROWID layouts. New field
mappings cover canonical physical files versus presumed mapping descriptors,
ordered source/declaration occurrences, native Microsoft attributes, exact
contextual local expressions, effective enum/type queries and separate template
descriptors versus canonical declared parameter/template symbols. Actual
placeholder pointer relations may have plural declaration bindings; no name or
coordinate deduplication establishes symbol identity.

Controlled native 198-table writer comparisons (three runs per layout) measured
scale rowid median 3.22 seconds/40,932 KiB RSS/6,012,928 bytes versus composite
WITHOUT ROWID median 3.21 seconds/41,424 KiB/5,300,224 bytes. Tiny basic measured
0.11 seconds for either layout and decreased from 1,388,544 to 1,130,496 bytes. Complete
table counts matched and foreign-key/integrity checks passed. These comparisons
justify selecting the composite relation policy after current genuine reader
integration; page-size writer measurement remains separate.

The preparation hash gate now verifies 316 materialized stock source/build
inputs, including C++ metadata sources and build tooling. A private changed
stock wrapper is rejected before building. Expanded genuine profile verification
is pending; no core source changes are authorized.

Actual parameter-flag assertions close retained default arguments, by-value copy
passing, copy-constructor parameters, explicit-object parameters and expanded
packs with repeated original parameter numbers and distinct signature ordinals.
The catalog worksheet now includes every 39,060 explicit kind. REVIEW.md preserves
remaining concrete lifetime/EH addressing, constructor/local-static roots,
variable obligations/unions, routine body/eligibility, reflection dispatch,
attribute plural ownership and complete guarded field/profile closure work.


During the 232 integration audit, the field worksheet now records 122 verified
mappings, 177 implemented mappings awaiting exact activation, 17 required adapter
gaps, and 1,498 raw pending rows. Current source-file/sequence fields have explicit
physical/presumed relation mappings; they remain pending genuine refresh evidence.
Typed list-pointer exclusions preserve actual ordered relations, while numeric
sequence endpoints remain semantic fields. These counts are work tracking, not
full-family acceptance.


First 232 integration evidence: strict primary compiler build passes. Thirteen
fresh primary objects pass complete-header/typed-expression/no-BLOB-REAL/FK
inspection and 161 ordinary reader assertions, including canonical placeholder
bindings with distinct descriptors and no phantom symbols. Source 232 compilation
and remaining profile refresh are underway. The genuine fixed-point probe exposed
stock in-place canonical type lowering; the native adapter now uses audited public
existing-cache pointer identity and is awaiting fresh probe validation. Complex/
imaginary original-format identity needs a genuine C99 numeric fixture as well.
Field coverage advances to 129 verified mappings and 234 implemented mappings
awaiting exact activation; 14 adapter gaps and 1,434 raw pending rows remain. This
is progress evidence, not acceptance of full phases 3–4.


## Resumed validation on 2026-10-13

Both saved implementation lanes resumed with their previous disjoint ownership.
The previously uncompiled member-pointer guard now passes primary/source builds
and genuine native pointer-member probes; source retains six actual forms and
class/casting associations, while primary stock lowering retains two null
pointer-member constants and lowered value forms. Original converted class is
not inferred from the target member's declaring class.

The revised retained concurrency signature-string projection passes 101 assertions.
Source 232 source_order/provenance passes 82 with actual secondary/member/sublist/
linkage/STDC order and physical/presumed separation. Stronger exact coordinate and
physical occurrence assertions remain underway. Fresh stock module interface 261
and import 278 entities publish with zero FKs. Managed acceptance is pending.
Cfront 232 refresh runs in the single native lane; exported 232 remains next. The
existing macro replacement_range adapter is being completed and will be refreshed
at a stable compiler boundary; no new shared schema change is in progress.


User priority update on 2026-10-13: source locations are nice to have. Preserve
direct available metadata without guessing, but optional coordinate completeness
and intricate provenance probes are not phase acceptance gates. Executable
semantics/types/lifetimes/API and actual resulting IL tables/variables take
priority. Compiler construction bookkeeping stays excluded. Both workers received
the update; already-near-complete direct source assertions passed 90 and further
provenance expansion stopped.


## Verified 232-table semantic/type checkpoint

The 2026-10-13 stable snapshot passes 421 assertions with a valid composite
WITHOUT ROWID self fixture plus 44 genuine primary/source/cfront-like/exported
objects. New exact fixed-point, complex/imaginary and GNU label-difference values
survive stock lowering; pointer-member queries guard current type kinds and source
six-form class/casting associations pass. Native Microsoft attributes activate
all six exact argument variants under the actual recognition/suppression flags.
Canonical template placeholder/parameter identity, execution versus metadata,
source-semantic initializer containment, typed visitors and semantic declaration
order pass ordinary public navigation. Optional direct source metadata remains
available without reconstruction; it is not a phase completion gate.

All 45 published 232 artifacts (including the separately benchmarked scale object)
pass complete-header, typed-expression, zero-FK and no semantic BLOB/REAL checks.
Each of four profiles independently matches 316 stock source/build inputs to the
pinned Git blobs; all canonical backend files are byte-identical in staged trees.
The obsolete inactive native source-sequence header was synchronized and strictly
rebuilt before the snapshot. Ordinary publication failures preserve destinations
and remove temporary output. The reader builds with zero warnings/errors;
generated 230 detail classes /1,581 properties and 66 literal field writes match.

Actual concurrent disposal, bounded eviction and valid generous-cache prefetch
reach the 900-key cap. Eight valid rowid/composite-WITHOUT-ROWID scale copies each
retain 85,520 entities /7,023 executable operations over cold/warm sweeps, with
4,096 cached rows /128 pages. Commands 26,550–26,552, fetched rows 191,199–191,201,
post-initial-JIT time 1.02–1.07 seconds and RSS 109–119MB remain bounded. At 4KiB,
rowid 7,192,576 bytes versus composite 6,402,048 bytes; warm traversal 1041 versus
1040ms. Native 198 writer A/B also found no throughput regression. Composite
policy/page-size default selection and native page-size writer comparisons
remain open M5 work.

This checkpoint does not complete phases 3–4. NEXT-CONTRACT.md records the next
canonical attribute/lifetime/initializer and contextual EH schema batch. GNU
variable cleanup/binding/TLS, routine and reflection closure, maximum native
profiles and all remaining semantic field/kind rows remain required. Source
location priorities follow the user's instruction; compiler construction records
are excluded, actual resulting operations/types/tables/variables are captured.


Checkpoint `b7f52f778` committed 66 entirely owned files after final 421 assertions,
all 45 artifact inspections and independent four-profile 316-input pin checks.
Unrelated index records were preserved byte-for-byte. The next frozen semantic
contract contains 243 tables /39,067 stable kinds, with exact canonical attribute/
argument ownership, block/try/local-static lifetime bindings, constructor order,
contextual EH storage and the audited variable flag/union surface. Managed now
owns roslyn_attributes.h exclusively; native owns lifetime/EH/variable adapters.
Both lanes continue implementation, not a new approval or pause.

Measured composite ordered relations now use WITHOUT ROWID; integer identity
and single-key tables retain rowid, and writer page size remains 4KiB. At the checkpoint, the field
ledger recorded 144 verified mappings, 327 mapped awaiting exact positive activation,
14 explicit adapter gaps and 1,326 raw pending fields. Optional source-coordinate
rows are not phase gates under the user's priority; remaining semantic rows and
all other approved native extension/profile acceptance remain required.


## Active 243-table genuine integration

Primary strict compilation passes with the stock reflection feature enabled by
the independent profile configuration. First genuine `basic`, `m0`,
`cleanup_semantics` and `variables` objects have all 243 tables and zero foreign-key
failures. The cleanup fixture activates 18 lifetime rows: 15 actual scope bindings,
one block descriptor, one try descriptor and one legitimate null/unbound binding.
It retains 21 minimal cleanup obligations, seven recorded child links, four array
records (including runtime count -1), a VLA-count ABI supplement, a no-object
cleanup location and 12 local-static initialization wrappers. The variable fixture
retains two structured-binding memberships, a GNU cleanup obligation and distinct
standard/extension TLS facts. These are inspected native facts; full public reader
activation and source/cfront/exported refresh remain in progress.

The reader compiles cleanly against the frozen contract and both valid rowid and
composite-WITHOUT-ROWID self fixtures pass 73 assertions. Generated 241 detail
classes /1,701 properties and 72 literal writes are synchronized. This does not
establish genuine native semantic acceptance by itself.

Exact field mapping now records 144 verified fields, 445 mapped awaiting
positive activation, 13 explicit partial adapter gaps and 1182 raw pending
fields. Canonical variable/lifetime/initializer adapters have compiled mappings;
compiler worklist/hash/slice-bit and definitively front-end-allocated construction
records have explicit exclusions. CLI-only fields follow the user's scope change;
native Microsoft COM/IDL/interface/property semantics remain required. Remaining
semantic modifiers/initializer-defined entities/instantiation and lvalue-use flags
are still open. Optional coordinates are not completion gates.


Final 243 validation has accepted 69 snapshot objects and exact canonical backend
bytes /316 pinned stock inputs in all four stages. Before committing, a genuine
partial thrown-aggregate probe exposed an incorrect integer-handle base at member
offsets 0 and 8. The exact pinned producer uses the runtime thrown object, so the
final contract appends thrown_object_relative and distinguishes no-object release
through its actual retained void() prototype. The checkpoint remains open until
this correction is refreshed and publicly asserted. The region format also has
no unordered bit: the action fact is nullable/unavailable, while retained
initialization flags and executable region links remain authoritative.

Cfront cleanup now activates indirect VTT/delegation supplement locations and VLA
count location; source constructor initializer order is real. Source-only unknown
attribute retention produces two distinct deprecated descriptors sharing one
canonical argument; an unretained vendor template-parameter marker is not claimed
as owner-sharing evidence. Both native and source reflection publish genuine
nine reflected values and one data-member specification. Managed resumed its
required Sol xhigh model after a transient capacity error and continues actual
family scalar/navigation assertions, including recipe lifetime descendants.


## Final 243 checkpoint evidence

All four strict builds and final 51 matrix /24 durable smoke objects pass. The
reader passes 502 matrix assertions and 317 smoke assertions; valid rowid and
composite self fixtures pass 73. Independent coordinator inspection of all 75
paths confirms 243 tables, zero foreign-key failures, complete expression type
links and no semantic BLOB/REAL columns. Stage verification records exact
canonical backend bytes and 316 pinned stock inputs for each profile. The reader
build has zero warnings/errors; generated 241 details /1,697 properties and
72 literal writes match the amended schema.

The final decoder distinguishes thrown-object offsets 0 and 8 from the actual
no-argument no-object release, including cfront object-taking wrappers. Unknown
action unordered state stays null. Ordinary array storage/count reverse contexts,
VTT/VLA locations, constructor/destructor order and execution roots, local-static
and handler lifetime projections pass. Canonical distinct attributes share their
actual argument and group; type/expression argument arms and actual reflected
entities/data-member specifications navigate through public properties.

The last narrow tuple review found no writer omission: stock tuple-like get calls
use actual dynamic initialization and stmk_init statements, and both calls are
reachable exactly once. Ordinary initk_binding expressions are alias recipes.
A durable small fixture has refreshed six variable paths; its direct public
assertions pass. The final combined 75 reopened objects pass 750 assertions,
including valid composite self fixtures. Six affected variable paths separately
pass157. Exact scalar record counts are now742 variable /280 initialization,
with172 lifetime /66 attribute /54 argument records.

The field ledger now records 226 verified, 395 mapped awaiting representative
conditional/identity activation, 13 explicit partial adapter gaps and 1144 raw
pending fields. Direct scalar proof includes source/guard/column/property mapping
and a real family getter sweep: 742 variable, 66 attribute, 54 argument, 172
lifetime and 280 initialization records. A false value is valid evidence for an
unconditional copy; active unions, unavailable states and ordering/identity/
execution obligations require positive representative cases. See the durable
managed semantic243 evidence and scalar-sweep TSV files. No scalar sweep closes
legacy unwritten columns or unsupported guarded payloads.

Full phases remain active. Next implementation families are native class/base/
layout/fields and managed routine/signature/GNU supplement, followed by remaining
expression/constant/extension/profile closure and M5 writer/storage performance.
Native scale export currently measures 86,523 entities /5.308s versus the prior
232 ~3.8s; reverse-FK scans and native page-size comparisons remain measured
follow-up work, not a completed performance gate. Source coordinates remain
optional under the user priority.


## Committed 243 milestone and active 258 contract

Checkpoint `0cae3dd0b` commits the verified 243-table lifetime/cleanup/attribute/
variable/reflection slice described above. Both implementers remain active on
the requested GPT-6.1-Sol xhigh model. The shared next contract is frozen at
258 tables /39,133 stable kinds: 66 append-only additions, zero changed prior
ID mappings. Actual DDL execution and generated native schema/catalog byte
synchronization passed; this is contract validation, not semantic acceptance.

Native owns class/base/field/layout adapters and the common declaration-modifier
helper. Managed owns the disjoint routine/current-signature/GNU-supplement and
canonical requires-clause adapter plus reader/navigation tests. The native lane
serializes all compiler/export builds. Field-sized contracts are preserved in
`schema/layout-field-contract.tsv` and `schema/routine-field-contract.tsv`.
Both families must produce adapter, public-property and genuine-fixture evidence
before their ledger rows close. C/GNU/native-MS layout and routine pilot sources
already parse using accepted243 compilers; the new semantic fields await258
exports and public assertions.

The current 24-run native page-size benchmark completed on valid basic/scale
objects with equal table counts and zero FK failures. Scale median writer time
is5.432/5.397/5.140/5.160s for1/2/4/8KiB;1KiB saves56.6% on tiny basic objects
and11.3% on scale versus4KiB. Final default selection weighs reader evidence;
reverse-FK indexing remains a separately measured writer optimization.


Fresh source258 now verifies the actual Microsoft ABI runtime decisions after
stock Win64 suffix overrides: pointer-member sizing and bitfield allocation1,
container retention1, seven actual container-offset rows, event interface1 and
UUID1. Overwide declared96/actual32 and dependent NULL widths distinguish raw
requests from established layout; all11source smokes reopen FK0/1KiB. Other
three stages are synchronizing final capability writes before the planned final
89-path native snapshot. Full public assertions/pin gates remain in progress.


## Verified258 class/layout and current-signature milestone

Final all4 strict compilers and93 genuine snapshot paths pass at258 tables /
39,133 stable kinds. Prior39,067 IDs remain unchanged;66 were appended. Native
objects reopen with zero FK failures, actual1KiB pages, complete expression type
links and no semantic BLOB/REAL columns. Coordinator independently compares
all316 materialized stock inputs per stage directly to Gitpin and all24 staged
backend files to canonical bytes. Native/compiler and reader commands are idle
for this short checkpoint.

Reader build has zero warnings/errors, valid default self PASS73 and final93
with composite fixtures PASS1028. Generator/native-write checks validate256
details /2001 properties and82 literal writes. Positive tests cover exact
class/base postorder/preorder, base-owned override order, covariant contextual
subobjects, real vcall/table links, C/generated/dependent layout availability,
declared96 versus actual32 width and Microsoft offset37/bit1/container1byte.
Canonical GNU aliases/ifunc/inline partners/multiversion dispatch and exact
architecture members, callable-versus-written routine type, body availability,
this/object qualifiers, current ABI parameters and original pack coordinates
navigate through ordinary properties. Genuine cached and subordinate-prototype
exception union branches, dynamic lists/empty and noexcept true/false/dependent/
indeterminate pass in all4. Lambda closure flags and scoped-enum members have
actual direct write/projection evidence, rather than relying on column defaults.

The field ledger now records398 verified,499 mapped awaiting conditional/public
closure,80 exact remaining adapter gaps and794 unaudited fields. Full phases
remain active. Next native family is exact constant occurrences/value arms and
existing native extension activation; managed owns executable expressions and
statements in a disjoint header, including genuine source VLA evaluation sites.
Common namespace/using/label/scope and remaining template/type/profile fields,
final public format compatibility and M5 measured throughput/storage closure
remain required. Optional source locations do not block these semantic gates.

## Active post-258 constant and execution release

Verified milestone `8d9c4bd8886d375dc13eed409aa62de4c95c0aa2` committed 51 owned
files after PASS1028 over 93 genuine snapshots, self PASS73, all four strict
builds, unchanged 316 pinned stock inputs per stage and byte-exact canonical
backend copies. Unrelated index/workspace changes were preserved.

The next contract is frozen at 263 tables/39,144 kinds; all 39,133 prior mappings
are preserved and eleven IDs appended. Both implementers are released on disjoint
constant/type and expression/statement/VLA modules. See NEXT-CONTRACT.md and the
two canonical schema field-contract TSVs. Generation/DDL admission is verified;
new adapters/API/positive semantic coverage remain pending. Full M0–M5 stays active.

The 263 development contract now generates an exact schema SHA256 for a required
writer/reader capability. Admission must reject a genuine older checkpoint shape
before ordinary lazy navigation; malformed/corruption campaigns remain excluded.
The unwritten baseline constant.is_literal is nullable unknown, not default false.

The following final 263 acceptance replaces the earlier in-progress native and
reader status reports. Exact checkpoint evidence is recorded below.

### Accepted 263 constant/execution milestone (`a409df1be`)

Final exact bytes pass **1795 public reader assertions / 137 genuine objects**,
self/WITHOUT ROWID fixtures PASS73, build zero warnings/errors, 261 generated
detail facades / 2048 properties and 144 native literal-write admission checks.
A genuine archived 258 object is rejected at Open by the required development
contract, before semantic navigation. The new-family subset passes245 over40
paths; source temporary source-type identity positive passed before the final
137 rerun. Conditional payloads without activation remain pending.

Field proof join closes170 source-to-writer-to-SQL-to-getter rows from specific
public/scalar evidence. Current worksheet: **567 verified,550 mapped pending
validation,136 mapped pending implementation,493 raw pending**, with exact
exclusions separate. Scalar mapping closure permits false direct recorded flags
only with compiled write/guard evidence plus real family sweeps; active union,
identity, ordering and availability require the listed positive cases.

Fresh large object:86,609 entities /7023 operations /6,532,096 bytes. Reader cold
28,273 commands /197,597 rows /1819.92ms /113.7MB RSS; warm28,274 /197,598 /1292.93ms
/115.8MB. Row/page caches stay at4096/128 bounds. This demonstrates current bounds;
remaining M5 writer indices/host profiles/complete field and kind closure remain
required. Native stock77 recompilation on reconfigure is tracked for a later
build-only fix rather than changing this verified boundary.


## Active 266 common metadata integration

The263 checkpoint is committed as `a409df1be08efeee4f8ebf952a53bb5298306c91`.
The current frozen266-table /39,145-kind common contract preserves all prior IDs.
All four strict compilers pass; the serialized146-output refresh is running.
The migrated reader builds clean, self PASS73, first genuine basic/m0 PASS104;
archived263 admission rejects before navigation. Exact retained constant-target
references independently pass195 and page-size1 UNION86.

Read-only secondary declaration, pragma and module reviews reconcile66 rows
without promoting verification. Current ledger:567 verified,608 mapped awaiting
validation,168 planned adapter mappings,403 raw pending; exact exclusions remain
separate. Raw pending does not mean absent implementation. The separated remaining
semantic/profile/performance work is in CLOSURE.md. Both implementers remain
active on disjoint ownership; full phases3–4 and M0–M5 stay open.


The266 native refresh now passes146 genuine objects (native45/source46/cfront29/
exported26), all266 tables, FK0,1KiB and current required fingerprint. All four
stages match26 canonical backend files and316 pinned stock inputs. The compiler
lane is idle; large artifact refresh uses the same verified snapshot. Reader
source/Microsoft pilot PASS137 verifies using qualifier/owner/set links, alias
chain, proxy absence of declarations/layout, common identity and native flags.
Full public acceptance remains in progress. Compiled native-common266-mappings.tsv
joins83 exact field mappings without verification promotion; worksheet now567
verified,662 mapped awaiting validation,114 planned implementation,400 raw pending.


### Narrow266 scale and preindex correction

First common-metadata scale export passed FK0/266 tables/1KiB but took87.933s
with45,024KiB maximum RSS,98,697 entities and7,571,456bytes. EXPLAIN parent
INSERT and reverse-query evidence identifies seven populated scan paths: origin
references from constant/semantic_node/correspondence (5008/7028/8188 rows), and
position references from origin/spelling/range endpoints (18047/35091/16049 each).
Seven narrow reverse indexes are now generated, with unchanged semantic payload
and catalog. After performance and exact snapshot acceptance remain pending.

The retained-scope prepass must follow associated namespace/class/enum/prototype
scopes as well as resident file/function region roots and actual child lists.
Pinned stock region chains omit those associated scopes. Idempotent collection
before newly reached scope hydration and final plural using-owner reconciliation
avoid visit-order-dependent owner claims; final membership is never inferred from
target parents. Native implements this narrow correction in the same refresh.
The earlier source pilots already retained final owners through late updates;
this closes the preindex guarantee rather than inventing additional memberships.


The final owner-corrected146 reader gate passes1915 assertions (fresh log
/tmp/roslynobj-managed-common-final146-owner-corrected.log). All retained using
scalar owners now match exactly one actual scope-list membership; zero/plural
memberships yield null. The inherited-constructor metadata descriptor surviving
lowering no longer borrows the generated routine's file parent. Stock
lower_il22241–22258 clears the original using list while retaining that routine
association. Source positive owner/set/qualifier cases remain required.

Final scale after seven indexes and owner correction:30.381s /45,056KiB RSS /
98,697 entities /8,865,792bytes, same rowcounts as before. The seven indexes reduce
87.933s by65.45% at17.09% byte growth. Writer throughput remains an open measured
M5 item; it is not declared closed by this checkpoint. Reader cold32,002 commands /
231,910 rows /2086.59ms /111.16MB; warm32,001 /231,909 /1551.19ms /110.72MB, bounded
4096-row /128-page caches. Final self/compatibility/evidence checks are pending
before commit. Next actual gaps/profiles remain separated in CLOSURE.md.


### Accepted266 common metadata milestone (`dec4f0a86`)

Final exact snapshot:146 genuine objects PASS1915; self/WITHOUT ROWID PASS73;
archived genuine263 rejects at Open; clean build0 warnings/errors;264 generated
detail facades /2089 properties;144 native literal-write admission checks.
All four strict compilers,26 canonical backend files and316 pinned Git inputs per
stage independently verified. Required framed SQL-plus-full-catalog digest is
9547581ad6fd52c32342290c282807cbffd3b9b5d694bac1f870177895cde2e2.
All39,144 prior catalog IDs preserved; one new canonical descriptor kind appended.

Field-sized proof joins close46 additional rows. Worksheet now613 verified,
667 mapped awaiting validation,114 planned adapter rows,314 raw pending, with
explicit exclusions separate. Kind worksheet now includes every39,145 catalog ID;
new positive kind cases do not imply complete remaining payload/field closure.
92 public evidence rows and83 native-field joins preserve scalar versus positive
versus pending/exclusion status; no default-only or table-only promotion.

Final reader scale cold2115ms /32,002 commands /231,910 rows, warm1561ms /32,001 /
231,909, bounded4096 rows/128 pages. Writer87.933→30.381s at17.09% size growth is
recorded as measured progress, with residual throughput/storage optimization still
open. Next bounded variable/UPC/designated statement-source-entry gaps, removal of
phantom fields, build-wrapper timestamp correction and configured native host/
Sun/USL profiles continue immediately after commit. CLOSURE.md remains the full
completion checklist; phases3–4 are not yet complete.


## Active267 bounded variable and execution release

Milestone266 committed as `dec4f0a86c3889b96ed11378eacc5d505a1fdddf` (67 owned
paths); unrelated index byte-identical. Frozen267 tables /39,145 unchanged kinds
adds7 guarded nullable variable facts, ordered initializer-defined entities and
exact Statement.SourceSequenceEntry. Removes phantom Statement constexpr and
NameReference builtin columns. UPC affinity uses the existing payload/role.
Both implementers continue disjoint variable/build/profile and execution/API
work; final field proof and residual M5 work remain active. See NEXT-CONTRACT.md.


267 native strict and real build-wrapper verification pass: changed owned header
rebuilds backend/link only in13.519s, byte-identical generated-config mtime stays
unchanged, subsequent no-op build0.841s/Ninja no work. Five genuine native pilots
pass FK0: variable initializer entity list4closure types, UPC affinity1descriptor,
name-reference calls5arguments/4exact formal descriptors/one null varargs tail.
Source compiler/public positives are next; configured profile and M5 work remain
serialized native tasks.


267 reader builds clean, self73 and native five pilots PASS113. Source basic/m0/
initializer/UPC public checks pass; ordinary name-reference positive activation
requires source-only DEFAULT_RECORD_FORM_OF_NAME_REFERENCE1, explicitly lexical
wrapper creation0. Runtime capability records C mode disabling this stock option.
No primary original-source or coordinate recovery is added. The amendment has
exact current call ParameterType mapping and audited phantom-column removals;
ordinary typed init/result statement visitors and label lifetime projections use
existing recorded payloads. Final profiles/genuine matrix/M5 remain in progress.

Read-only expression34/statement26/routine-type6 mappings reconcile existing
adapters without verification promotion. All substantive new gaps remain assigned
and no ledger reconciliation holds the serialized compiler lane.


## Accepted267 bounded execution milestone

Final exact snapshot:158 genuine objects PASS1963; self/WITHOUT ROWID PASS73;
archived genuine266 rejects at Open; clean build0 warnings/errors;265 generated
detail facades /2089 properties;144 native literal-write admission checks.
All four strict compilers and316 pinned inputs plus26 canonical backend files
per stage independently verified. All objects have267 tables,1KiB pages, zero FK
failures and no semantic BLOB/REAL storage. Required framed contract digest:
`c07b7fef31a8063ed6cdb013b3d57ab67039e32d865e27431fe8050c7f41ae84`.

Accepted behavior includes exact call-formal descriptor mapping, initializer-defined
entity reachability, UPC affinity execution, designated statement source entries,
typed name/init/result/label navigation and removal of audited unwritten fields.
Source-only name-form recording preserves legitimate empty original-argument lists.
Seven inactive guarded variable flags remain nullable and await positive profile
activation. The generated-header rebuild fix passes backend-only and no-op builds.

Coverage remains613 verified /732 mapped pending validation /114 planned /248 raw
pending. Mapping reconciliations are conservative; full phases3–4 are still open.
The private FK bulk-load candidate is measured, not yet canonical: scale median
30.28s to0.59s with identical logical rows/IDs across six pairs, excluding artifact
identity and stock __TIME__, same8,879,104 bytes. Candidate publication performs
complete FK checks before commit and restores enforcement before publication.
Next work adopts and validates that policy, measures removal of writer-only indexes,
finishes configured Sun/USL/host numeric profiles and reconciles remaining exact
field/kind evidence. Six profile drafts are outside this checkpoint.


After semantic267 commit d2cdd575e, conservative routine/type/override and storage
reconciliation maps existing producers without new verified claims. Current field
counts616 verified /818 mapped pending validation /43 planned /219 raw pending.
Storage bookkeeping dispositions preserve actual macro invocation records and
contextual local expression/scope associations; no raw pointer/tree/block data.

Canonical index-free bulk candidate measured0.56s/7,585,792 bytes versus
old-index bulk0.60s/8,879,104 (-14.57% bytes). All267 scalar rows/IDs equal across
six cases excluding the intentionally changed contract digest, artifact identity
and stock __TIME__. Full public reader/overlay acceptance is still pending.
Evidence: performance267/{summary,results,logical-equality}.json.


## Accepted performance267 and configured native profiles

Final exact snapshot:176 genuine objects PASS2092, self/WITHOUT ROWID PASS73,
clean reader build and generator265 facades/2089 properties. Genuine prior267
rejects at Open under the new index-only framed fingerprint
`034cf58664b43af97cf491ab271620b693d96fb64d6ca09e7504927b01101671`.
Ten strict compiler profiles each independently match316 pinned Git inputs and26
canonical backend files. All176 objects independently pass267 tables,1KiB pages,
FK0, complete typed expression links and no semantic BLOB/REAL.

Private bulk construction now uses the documented policy: complete FK checks
before completeness/commit and enforcement restored/verified before publication.
The seven writer-only origin/position reverse indexes are removed. Controlled
scale median30.28s→0.56s,8,879,104→7,585,792 bytes; six logical pairs preserve
all scalar rows/IDs except intentional contract digest, artifact identity and
stock __TIME__. Final exact snapshot scale0.76s/43,980KiB/7,586,816bytes,98,815
entities/7,023 operations. Final reader cold2.627s/warm2.119s,111/112MB RSS with
bounded4096 rows/128 pages. Publication-error checks preserve destination and
remove private temporary files; no corruption fixtures were introduced.

Sun, actual USL2.1/3.0, host-long-double, host-double and official SoftFloat3e
profiles pass18 genuine pilots and196 ordinary API assertions. Stock limits stay
explicit: USL exported-template gates disabled to preserve actual USL signatures;
host-double fixed-point disabled because exact long_Accum exceeds double
precision. Other profiles retain full fixed-point coverage. SoftFloat dependency
pinned a0c6494cdc11865811dec815d5c0049fba9d82a8; no core EDG source edits.
Typed ComplexValue exposes canonical floating components.

Field ledger now655 verified /881 mapped pending validation /39 planned /64 raw.
18 directly captured coordinate mappings and12 optional source omissions remain
outside mandatory semantic gates. Builtin36,977 real discriminants and629 token/
opname/register/constraint entries have exhaustive shared mapping/family evidence
with sentinels/aliases separated and runtime registration limits explicit. No
operator/expression payload is promoted by scalar catalog proof. Full phases3–4
still require remaining exact field/union/identity evidence and actual gap closure.
Next bounded batch removes20 audited unproduced scaffolds plus unreachable
temporary table; native activates seven currently-inactive retained variable facts
through stock-valid instantiation/inlining overlays. Continue without approval hold.


## Accepted260 scaffold and tableless partial-EH milestone

Final182 genuine validation entries (171 distinct paths) PASS2462/self73; clean reader258 detail facades/2028
properties, generator/literal checks and genuine prior267 rejection. Independent
12-stage proof compares316 stock Git inputs and26 canonical backend files per
stage; all182 validation entries have260 tables,1KiB pages,FK0, complete expression type
links and no semantic BLOB/REAL. Contract:
`83418408ba791622839f1c44fadcdaf71de682726d7d08949c85bbd150191e0f`.

Removes20 unwritten fields and7 unreachable scaffolds while preserving exact
virtual/table-variable, current ABI, initializer and lifetime alternatives.
DeclaredVirtualMethods projects recorded owned methods, with inherited/override
navigation through actual bases. Tableless partial EH keeps exact metadata-only
cleanup-start and completed-initialization pointers, nullable numeric states and
actual prologue routine. Genuine positive completed targets2, cleanup references35
and3 unreachable states; actual constructor calls remain2object/4temporary once.
No private cleanup descriptors, generated tables or storage parity are inferred.

Automatic/inline-variable instantiation activates four exact retained variable
flags (positive counts1/1/1/4). Three minimal-inlining flags cannot activate because
stock inline.c35–44 unconditionally requires forbidden full portable EH; guard0
and NULL are explicit, no exceptions-off/core workaround. Lowered dynamic local
static wrappers are removed by stock; source positive ownership remains and
lowered executable constructors are checked directly.

Final scale0.58s/45,548KiB/7,573,504bytes/98,815entities/7,023operations; bounded
reader cold2.160s/warm1.596s,115.5/111.4MB RSS,4096rows/128pages. Field coverage
696 verified/913 mapped awaiting evidence/11 raw/1 planned optional source-list
association.24 captured optional coordinates and12 deliberate source omissions
are outside mandatory semantic gates. Full phases3–4 remain open for exact
remaining scalar/union/identity field evidence and non-scalar catalog closure;
next tiny orphan association uses existing relation, no source reconstruction.


## Post-accepted260 bounded evidence closure

The direct orphan source-sublist association passes incremental source compilation
and three existing exports; source_order/common retain two actual links each,
with one/three ordered entries. Bounded reader family acceptance passes779 on28
current paths, including those canonical descriptors and actual scoped-expression
projections. No schema/catalog identity or coordinate reconstruction is added.

All2,000 field rows now have explicit mappings/dispositions:696verified,
921mapped pending-validation, zero raw/planned. Four remaining CLI-only fields
have exact stock exclusions; requires/for/access-base list mappings use existing
writers/getters and retain positive validation separately. Native type/layout/init
and managed template/parameter/attribute scalar proofs are being reconciled.
Source-only locations and intentionally omitted coordinates remain optional.

Routine43/scalar24/pragma99 catalog evidence preserves aliases/sentinels and
active-context/runtime availability separately from scalar identity. The next
bounded read-only root review covers retained tagged IL-entry dispatch, while
coordinator reconciles custom reserved catalog values; no payload closure is
claimed from a catalog entry alone. Full phases3–4 remain active.


Bounded exact-pack/scalar checkpoint: PASS919 on47 existing genuine paths,
WORself73, clean build/generator/literal gates. Raw template is_pack bit is now
stored separately from the documented ordinary pack-marker convenience. Two
current strict native/source binaries and six affected template exports pass;
other ten stage source bytes are synchronized but binaries/snapshots remain at
accepted260 and will refresh with the next grouped contract. Orphan source lists
pass three existing exports and strengthened canonical/count/order navigation.

Coverage is now849verified/768mapped pending-validation,
with zero raw/planned rows. Exact guard/nonNULL/direct-write scalar proof closes
only the reviewed fields; active union/quantity/identity evidence remains open.
Nine zero-producer tables plus18 no-writer columns are queued for grouped cleanup
(final-unproduced-relations260-review.json), preserving populated alternatives
and stable kind IDs. Full phases3–4 continue immediately.


## Accepted251 producer cleanup and exact operand roles

Current contract251tables/39,145stable kinds:
`c76079ee21b05d278466e87f49f8e8851453b7fe561eb51652e089705e93de71`.
All12 strict stages match316 pristine Git inputs and26 canonical backend files
with exact stock tool pins. Publication validates186 instances over175 distinct
paths; all251tables/FK0/1KiB/complete/type links/no semantic BLOB-or-REAL pass.

Managed PASS2621 on175 paths, WORself73, clean249facades/1975properties,
generator/literal checks and actual prior260 admission rejection. Four genuine
Microsoft/Clang native/source role probes pass105: documented assume/noexcept
operands and builtin assume/constant_p arguments are metadata, retain typed
operand/callee/argument navigation, and ordinary control evaluates once.
classify_type is fully frontend-folded here; its retained arm remains unactivated.

Nine empty relations and18 no-writer columns are removed, preserving all populated
original/declared types, constraints, partners, initializer/handler and cleanup
links. Bounded public navigation has1934 property observations over existing
small families, with explicit depth/fixture limits. This is not automatic union,
quantity or ordering proof. Field coverage866verified/724pending
plus51 captured optional-source mappings and12 omitted optional positions;
zero raw/planned fields. Existing Boolean/getter and conditional positive
reconciliation continues immediately after this milestone.

Scale0.60s/45,564KiB/7,512,064bytes,98,815entities/7,023operations. Reader
cold2698.73ms/warm2018.55ms,111.4/112.4MB RSS, bounded4096rows/128pages.
No core patches, semantic reconstruction, portable full-EH lowering, linking
or translator were introduced. Full phases3–4 remain active.


## Accepted extern-inline and native dependent foreach follow-up

The251 contract and39,145 stable kind IDs are unchanged. All13 strict profiles
pass316 pristine-input and26 canonical-backend checks. Four new distinct objects
(extern-inline objects/routines and dependent foreach native/source) pass251tables,
1KiB pages and FK0. This supplements the accepted175 distinct baseline paths;
it is not a fresh full175-object run. Evidence is closure251/followup and the
extern-inline/foreach subdirectories.

Public checks pass constructor128/2 existing paths, extern-inline114/2 new paths,
foreach116/3 including resolved-native contrast, and WORself73; build/generator/
literal checks pass. The extern-inline requirement is an exact recorded EDG flag,
including17 true noninline routines, not an IsInline or body-availability inference.
Native Microsoft dependent sfepk_none retains common fields and nullable deferred
operations without reading inactive STL/array union members or inventing a loop.

Direct scalar proof closes42 additional fields:908 verified and681 required
mapped-pending-validation, with51 captured optional-source mappings and12 omitted
optional-source fields. Routine.inlinable joins the explicit stock configuration
exclusion because minimal inlining requires forbidden full portable EH lowering.
Union identities, quantities, order and executable obligations still require
representative proof. Full phases3–4 continue.

Next: fix the generated aom_modify switch omission (numeric enumerator trailing
whitespace), verify genuine GNU read-write asm, then remove the grouped owner-table
phantoms after exact stock/producer/API review. The ten constant/value candidates
have source-backed alternatives; no reconstruction or optional-position campaign.


## Accepted GNU read-write asm and bounded proof reconciliation

Trim numeric enum assignments before alias classification. The regenerated
header gains exactly aom_modify -> existing319004; schema/catalog contract remains
251/c76079ee21b05d278466e87f49f8e8851453b7fe561eb51652e089705e93de71.
All13 strict stages match316 pristine inputs and26 canonical backend files.
Two new distinct native/source +r objects pass FK0/1KiB/current contract.

Public PASS479 on nine genuine paths verifies actual read-write asm modifier,
register constraint, canonical owner, exact evaluated operand once, input/output
contrast and25 raw header capability Name/Value/Enabled projections. WORself73,
clean build/generator/literal gates pass. This is a bounded supplement to the
accepted175-path baseline.

Coverage948verified/607required pending-validation;85 directly captured optional
source metadata rows and12 intentionally omitted optional-source rows. Thirteen
existing lifetime/constructor/local-static identity/union/list proofs and the
explicit main/module-import tests are reconciled with exact stock writes. Thirty-
three remaining coordinate/range rows plus PrimaryInput are correctly optional;
template parameter ordinal and STDC cx_limited_range remain semantic. No new
coordinate recovery or inferred facts are introduced.

Next required positives: GNU constructor array-copy source_expr, delegation,
local-static initialization-kind getter, and guarded separate file-scope init
routine roots. Primary IA64 requires lazy TLS; non-IA64 nonlazy TLS has an exact
stock do_single_init assertion to investigate without bypass. Grouped owner-table
phantom removal preserves populated alternatives and will regenerate admission
fingerprint before refreshed acceptance. Full phases3–4 remain active.


## Accepted29 owner-table cleanup and semantic activation

Current251tables/39,145stable IDs contract:
`e49f0f888402b4fedd4e40cbd18ec9580fb859f67ac3d539c60975437ce98eaa`.
Remove29 unsupported owner-table placeholders; every populated EDG alternative
remains. This is removal of our unused schema/API slots, not loss of EDG facts.
Source-backed reviews and phantom29-contract251.json specify each alternative.

All14 strict stages pass316 pristine Git inputs/26 canonical backend files and
stock tool pins. Full publication gate198 instances/187 distinct paths passes
251tables/FK0/1KiB/complete/no semantic BLOB-or-REAL/expression types. Reader
PASS3871/187, WORself73, clean249facades/1946properties, generator/literal and
genuine old251 admission rejection pass. Source ctor GNU C++17 array input and
delegation, exact assembly/register arms, priority200/201, separate initialization
routine order, TLS init/wrapper and tableless class-pointer RTTI all pass actual
public identities. The source array example does not assert ConstructorArrayCopy
true; its recorded recipe is nonconstant aggregate with source expression.

Separate-init14th profile keeps lazyTLS1, partialEH and KEEP1. Named register2 is
a configured stock EDG test target descriptor, not a physical-hardware assertion.
Variable.DefinitionType exposes the retained declaration descriptor or null;
existing common DeclaredType denotes current type. Foreach.Initializations follows
actual ordered initialization children. Exact bytes/constants/constraints/typed
ctor targets and actual initialization/lifetime links remain.

Coverage1066verified/488required pending-validation;85 captured optional source
metadata rows and12 explicit optional omissions, no raw/planned fields. Existing
field proof joins are distinct from unactivated unions/kinds and unsupported
original-source queries. Source locations remain nice to have.

Scale0.59s/45,528KiB/7,500,800bytes,98,815entities/7,023operations. Reader
cold2098.59ms/warm1538.39ms,RSS111.5/110.6MB,bounded4096rows/128pages. Prior
packaging/concurrency/eviction proofs stay accepted; no broad repeat campaign.

Next confirmed fix: file-scope statement header roots are reachability metadata;
export them initially7002 so unevaluated sizeof backing blocks are not promoted.
All187 accepted paths lack this root arm; add a tiny stock-approved sizeof
statement-expression side-effect/control probe and preserve real execution
promotion. Scoped-enum empty-scope reachability remains a candidate for native
review, not proven defect. Four additional unwritten optional/count/unknown slots
are queued separately. Full phases3–4 remain active until required proof closes.

### Accepted metadata roles and empty scoped-enum reachability

The e49f251-table contract is unchanged. Metadata header roots, non-function
scope/block and condition associations, and source-sequence tagged statement,
expression and initialization references now hydrate at metadata role7002.
Actual function bodies and EDG-scheduled dynamic initializations retain execution
precedence. The GNU direct-call sizeof subtree is wholly metadata in both native
and source objects; the separate local-initializer case retains EDG's actual
source global initialization and lowered generated startup call. No independent
C++ evaluation rule overrides those retained execution facts.

Actual empty scoped-enum assoc_scope pointers are now exported directly. Public
EnumSymbol.Scopes preserves plural reverse identity, with Scope only when unique.
Root/nested empty and populated enum scopes, parent identities and canonical
members pass; builtin align_val_t legitimately has no scope from its stock
producer. All seven previously retained real-enum cohort shapes are unchanged.

Native root251/metadata-root gates pass14 strict profiles, each316 pinned Git
inputs/26 canonical backend files and stock tool pin. Twenty-five unique fresh
objects pass251tables/e49f/FK0/1KiB/complete/no semantic BLOB-or-REAL checks.
Managed PASS1039 covers those25 paths; PASS278 covers10 existing type/vector/
member-pointer/layout/routine paths; self/WITHOUTROWID73, clean build, generator,
literal fields and diff checks pass. This is bounded acceptance, not a rerun of
all187 archived paths or the unchanged scale/packaging/concurrency campaign.

Field proof1195verified/358required pending,85optional captured/12omitted;
all2000rows have explicit mappings/dispositions. Existing scalar/identity joins
and the new retained-links251 evidence preserve distinct type/definition,
member-pointer class/member, vector element/size, DMI metadata and width96/32,
constructor PrimaryEntry, label lifetime and named initialization statement
identities. The class promoted-local list is consumed staging; actual promoted
types remain retained. M5 exact development admission policy is documented;
workspace positive identity/disposal tests and further semantic proof remain
next required work. No completion claim or user approval hold applies.


### Accepted pragma/scope, retained relations and workspace identity

Five fresh native/source objects in root251/pragma-scope activate UPC access and
coherence, GNU ident/common text and visibility, Microsoft comment/conformance,
actual NRVO variable and file-scope assembly membership. Include-alias kind is
retained but the stock filename pointers are never populated; actual preprocessing
uses private maps and its resolved code remains authoritative. Original alias
names are unavailable, with no map access or source-text reconstruction. Microsoft
if_exists source wrappers are compile-inactive under the exact stock guard; no
new source-generation profile is introduced.

Managed gates pass Class217/8,Constant349/7,Statement349/7,Pragma109/5, comprising
27 ordinary validation instances/25 distinct paths. Workspace82 includes self73
and nine direct identity/disposal checks on two genuine independent artifacts
plus reopens/copies. Build0warnings/errors, generator/literal/diff and separate
self73 pass. Native5 fresh e49f251 objects pass FK0/1KiB/complete/no semantic
BLOB-or-REAL; all14 unchanged compiled stages have reverified316 Git-pinned
inputs/26 canonical backend files/tool pins. No new strict build, full187 run or
scale/packaging campaign is claimed.

Field proof1324verified/213required pending,89captured optional-source/15limits;
all2000rows remain explicitly mapped/dispositioned. The declaration-list type and
routine alternatives remain pending even though variables pass. Source SSI
synthetic declaration flags, consumed promoted-type staging and unavailable
original text queries are distinguished from retained execution/type semantics.
Phase3–4/M0–M5 remains active. Next work is bounded missing branch activation and
exact existing declaration/template/routine/EH public identities.


### Accepted retained declaration and template branches

Five fresh retained-six251 objects pass97 public assertions and nine existing
objects pass518, with14 distinct paths total. Self73, build0 warnings/errors,
generator249 bindings/1946 properties, literal and whitespace checks pass. The
14 unchanged compiled stages retain316 Git-pinned stock inputs and26 canonical
backend files/tool pins. No new compiler build, full187 rerun or scale test is
claimed. Native/managed ready inventories are /tmp/roslyn-native-ready-retained-six251.json
and /tmp/roslyn-managed-ready-declaration-template251.json; root independent proof
is /tmp/roslyn-root-retained-six-independent.json.

Named StaticAssertion and direct decltype/proxy/constraint properties expose
actual EDG facts. Genuine C affiliation, Microsoft based pointer, closure field,
template header requires-clause, nonterse assertion and ordinary array local
reference identities pass. Actual correspondence-to-local_scope_ref origin is
confirmed by read-only pointer traces and matching public scope/routine identity.
The dependent array probe preserves Constant-owned lerk_tpl_param_expr links;
array-specific lerk_dep_array_bound remains pending, with no inferred recovery
or global unavailable claim. Thirty-one positive worksheet rows include21 new
closures and10 previously verified facts.

Current proof1365verified/169required pending,89captured optional-source/15limits;
all2000 rows explicit. Remaining work is actual branch activation, exact existing
public identity proof and documented stock/user limitations, plus five queued
unsupported minimal-contract slots. EDG stays the sole semantic authority.


### Accepted routine/EH identities and dependent constant storage

Current e49f251 contract is unchanged. Managed8existing paths pass350 and4fresh
paths pass92,12 unique total; self73, build0warnings/errors, generator249/1946,
literal/diff checks pass. Native4 objects pass FK0/1KiB/complete/exact contract/no
structured BLOB-or-REAL. Fourteen unchanged compiled stages retain316 Git-pinned
inputs/26 canonical backend files/tool pins. No new strictbuild/full187/scale
campaign is claimed. Ready inventories: /tmp/roslyn-native-ready-constant-dependent251.json
and /tmp/roslyn-managed-ready-routine-eh251.json. Root independent4 envelope is
/tmp/roslyn-root-constant-dependent4-independent.json.

Exact ifunc cache storage and EH region/array table variables remain distinct
from original resolvers and decoded actions. Constructor operands and overlap
lifetime links pass evaluated-edge/metadata tests. Nested throw-type flags have
real direct scalar reads, including false. Dependent sizeof expression, actual
template-template pack and written type operands pass both native/source shapes.
C99 complex lowering creates a resident static variable/copy constant; read-only
pointer traces prove the same occurrence and variable reach the backend. Public
reciprocal initializer/storage and executed return identity pass; source has no
storage and retains ComplexValue. No helper allocation or C-generator emulation.

Current field proof1388verified/144pending. Exact local-scope utility origins,
template parameter metadata and normalized source-list boundaries close through
existing genuine evidence. Original typeref qualifiers are profile-disabled
(CREATE_LEXICAL_TYPEREFS0 all14), and class allocation cache is explicitly consumed
and cleared before backend while resulting calls remain authoritative. The actual
local-expression discriminator uses the EDG family; nine old custom scalar IDs
stay reserved/nonemitted, and eight actual scalar getters have family proof.
Owner-specific unseen payload branches remain separate obligations.

Scoped availability is implemented; final public state/reason acceptance remains.
The source-backed final five-slot contract cleanup is reviewed but unimplemented;
regeneration/admission/new-contract evidence will be coordinated in its own batch.
Full phases3–4/M0–M5 remains active, with no inferred semantic replacements.


### Accepted availability, orphan/derivation and prototype/UUID proof

Current e49f251 remains unchanged. Existing8 paths pass499 and fresh4 pass95,
12 distinct objects total; self73, build0warnings/errors, generator249/1946,
literal/diff checks pass. Native4 currentobjects pass FK0/1KiB/complete/no semantic
BLOB-or-REAL. All14 unchanged stages retain316 Git-pinned inputs/26 canonical
backend files/tool pins; no new strictbuild/full187/scale run is claimed. Ready
inventories are /tmp/roslyn-managed-ready-availability-orphan251.json and
/tmp/roslyn-native-ready-prototype-uuid251.json; root independent4 check is
/tmp/roslyn-root-prototype-uuid4-independent.json.

GetAvailability now has exact actual-owner/state/reason tests for TU, types,
routines and calls, including absent entry omission. Void is always incomplete
per stock allocator and retains absent size/alignment; no independent language
category expectation overrides EDG. Orphan lists preserve per-kind ordinals and
literal scope numbers, without inferring execution/lexical ownership. Derivation
paths expose direct/preferred/access and forward/reverse inclusive-tail segments;
shared steps do not merge wrapper identity or create missing path prefixes.
C prototype scopes contain actual tag types and exclude parameters; source two
routine descriptors share one scope with its separately retained associated type.
Class/enum UUID declarations and __uuidof produce actual lowered storage, exact
GUID initializer fields/bytes and distinct source type-address targets.

Field proof1409verified/122required pending. VLA dimension-variable guard is0
in all14 selected profiles; !ABI_CHANGES_FOR_RTTI is active only USL21/30, whose
existing inputs have no throws, so accessible-base links remain a real positive
queue. Custom entity tag factories all exist:65 actual entity kinds materialize
in the bounded accepted navigation corpus, other cases have explicit reviewed/
unobserved status and separate field obligations. Pragma subgroup enum getters,
scoped fact states and reserved unused fact/local-expression scalar IDs now have
accurate family dispositions, without runtime-every-kind or payload promotion.

Next is reviewed five-column cleanup at this held boundary, followed by genuine
current-contract refresh/admission. Unknown non-type deduction payloads cannot
be silently discarded. USL throw/scoped attribute/module probes follow the new
contract; no user approval or completion pause applies.


### Accepted final five-column development format

Current251 tables/39,145 unchanged stable kinds use framed fingerprint
`4bb94e5feda94e85fe4b6fe4e8eb5f3494f3b112d14a56dc71e6b069861a2559`.
Exactly vector.element_count, parameter.source_origin, include.position,
cleanup_action.is_unordered and transient template deduced_array_bound are removed.
This retires our unsupported/optional/intermediate slots, not EDG semantic facts.
CleanupAction.IsUnordered remains bool? null; Initialization.IsUnordered stays exact.
Unexpected retained unknown-bound non-type arguments fail publication clearly before
union access. No constant creation, frontend recovery or surrogate source facts.

Fourteen strict builds pass316 stock inputs/tool pins and26 canonical backend bytes.
The unified current-format cohort has211 validation instances/211 distinct paths,
including all recent bounded probes, with FK0/1KiB/complete/no structured BLOB-or-REAL
and complete expression types. Reader PASS4437/self73/build0 warnings/errors,
generator249 facades/1941 properties, literal/diff and genuine archived-e49f Open
rejection pass. Earlier124/PASS2610 and failed profile-assumption runs are intermediate;
final assertions use actual IA64_ABI/RECORD_MACRO_ARGS values and exact stored links.

Scale new98,697entities/7,023operations/7,487,488bytes versus matching archived
98,697/7,023/7,488,512 (1KiB smaller). One current export0.57s/44,220KiB RSS;
reader cold2.208s/warm1.572s preserves4096row/128page bounds. No contemporaneous
repeated old/new writer benchmark is claimed. Existing packaging/concurrency/disposal
and private FK-load checks remain accepted. Root independently verifies exact five
schema removals, unchanged survivors and eight matching old/new object row multisets;
only EDG __TIME__ metadata differs after artifact/contract normalization.

Evidence: native-final-five251-evidence.json, final-five-frozen-contract.json,
final-five-contract-independent.json, final-five8-common-columns.json and
final-five-acceptance-review.json. Managed final211-v3 and ready inventory are
recorded with this checkpoint. All source/core inputs remain pristine and unrelated
worktree/index changes are preserved; restart artifacts are excluded.

Field proof remains1409verified/122required pending. Custom scalar/float/reference/
role labels now distinguish active observed, active unobserved and reserved vocabulary;
no tag observation promotes owner payload fields. All2000 fields mapped/dispositioned.
No reviewed current adapter/factory implementation gap is open, but genuine branch
activation and exact public identities remain mandatory. Next native USL throw and
scoped-attribute probes precede module IFC metadata; managed checks existing
expression/EH/asm/capture links. Semantic authority remains EDG. Full phases3–4/M0–M5
continue without an approval pause or public version freeze.
