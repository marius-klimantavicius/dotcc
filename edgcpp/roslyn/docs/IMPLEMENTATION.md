# RoslynObj implementation ledger

**Resumed at the user's explicit request on 2026-10-13.** Historical restart state
is saved in local historical pause notes, with both worker notes. Both implementation
lanes are active; full approved M0–M5 work remains required.

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
communication and independent review. Workers never commit independently. After the initial reader checkpoint, the managed implementer also owns `backend/roslyn_templates.h` and `backend/roslyn_expressions_extra.h`; the native implementer integrates and serializes compiler builds.

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
| M4 | Pending | Exhaustive active adapters and all planned compatibility profiles with valid upstream inputs. |
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
