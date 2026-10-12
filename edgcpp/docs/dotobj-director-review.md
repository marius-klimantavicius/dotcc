# Director review and continuation

The director stays active during the authorized DuckDB campaign, reviews each
milestone and checks ongoing work every few minutes. A completed fixture or
commit is not the campaign stopping condition. User pause overrides continuation.

The supervisor is GPT-6.1-Sol/high; implementers are GPT-6.1-Sol/xhigh. The
director is GPT-6-Astra/high. The supervisor owns integration, builds and commits.

## Translation acceptance

Use the original portable manifest recorded in
`artifacts/review/dotobj-relational-duckdb-configured-scope-v1/inventory.txt`:
2,635 units, comprising 2,207 library units, 148 tests, 224 musl units and 56
libc++ provider units. Preserve the manifest's per-unit language modes and seven
static extensions; native extension loading remains disabled.

Proceed library first, tests next, with provider coverage separately visible.
One complete unit is a checkpoint, not DuckDB translation completion. The
immediate requested deliverable is actual C# source generated through the new
normalized SQLite semantic objects; compiling/running the entire library is a
later checkpoint. Required unresolved provider declarations are permitted in
that source deliverable, with an explicit imports manifest. Missing executable
trees, fabricated implementations and opaque semantic placeholders are not.

The frozen old raw-graph campaign remains stopped. Scale the new path through
measured bounded batches after complete-unit evidence; preserve artifacts and
report exact source coverage, failures, memory and object size.

## Decisions reviewed on 2026-10-11

- Shared construction destination: add nullable
  `initializations.destination_initialization_id`, referencing the exact
  EDG-selected master initialization, and a shared-result action kind. Existing
  initialization identity reservation handles recursive relationships. Stock
  `lower_init.c` explicitly permits a master without a variable for return and
  other complex destinations. Do not invent expression owners or reconstruct
  shared storage from source syntax. Preserve EDG's cleanup ownership.
- Placement construction: retain ordered argument references, selected
  initialization and failure cleanup, plus the stock lowering's null-guard
  decision through a narrow callback and explicit allocation field. Evaluate
  placement arguments once, use selected deallocation after construction
  failure, and preserve rethrow. Do not guess behavior from allocator names.
- In an explicit source-emission mode, unresolved providers may become required
  public partial methods and typed external-storage address accessors. Preserve
  exact selected signatures and identities; provide no fake bodies, default
  storage, guessed imports or erased calls. Retain strict provider resolution
  as the default mode.
- Ordinary minimal schema/callback extensions do not require a blocking director
  approval. The supervisor records the selected EDG fact and concrete consumer
  need, notifies the director, and continues. Fundamental changes to the compact
  relational semantic contract still require review.

## Review checks

Review actual changes and retained evidence, rather than agent status alone:

- Is EDG the semantic authority, with sufficient expression/type/lifetime facts?
- Is every added relation needed by generation/linking or requested navigation?
- Are source structure and names preserved where C# allows them?
- Is the current work removing an observed project blocker?
- Have complete real objects and generated real source counts advanced?
- Are workers active, decisions answered, and tasks reassigned after completion?

The first review found the director had ended its turn while construction
decisions were queued. Workers continued independent features, but the primary
DuckDB path remained blocked. This is corrected by active periodic reviews and
delegating ordinary in-contract decisions to the supervisor.

## Active review record (UTC, 2026-10-11)

| Time | Evidence inspected | Decision or correction |
| --- | --- | --- |
| 16:27 | Current initialization schema and stock `lower_init.c` master destinations | Approved shared-destination FK and placement facts; removed ordinary-extension approval bottleneck; fixed full inventory and original language modes. |
| 16:30 | Managed `Unit`, `LinkedProgram` and function-body loading | Catalogs remain fully resident. Measure real-unit and bounded-batch memory before increasing the link batch; do not divert the current exporter work. |
| 16:31 | Shared-result native patch and managed destination emitter | One FK and bound destination are sufficient. Structured branches construct into selected storage; binding restoration and cleanup ownership reviewed. |
| 16:33 | Compiler entry point, invocation environment and legacy capture gate | New relational runs clear legacy mode variables and bypass the old raw-graph emitter. |
| 16:36 | Shared-results-v1 generated source and retained test/build evidence | Direct caller-result construction and local cleanup match the intended projection; self-address and failure tests pass native/C#/NativeAOT. Real hugeint probe advances to placement construction. |
| 16:42 | Placement emitter, stock catch-region construction and actual allocator source | Require exact cleanup ownership and protected-action references. Check actual source wrappers rather than testing only a convenient synthetic expression. |
| 16:48 | Actual hugeint placement-copy failure and exception hash-table jump | Fold the existing selected record-copy action into the placement milestone; retain source jumps instead of rewriting all control flow. |
| 16:56 | Placement-v1 generated allocator/initialization/catch sequence | Saved arguments, guard and selected protected action match the intended representation; native/C#/NativeAOT and related regressions pass in `4e27fb690`. |
| 16:58 | EDG label lifetime definitions and exporter `attach_lifetime` | A jump's common lifetime is a reference, not a new cleanup-region owner. Store it separately; do not overwrite lexical lifetime ownership. |
| 17:07 | Vacuous-destruction lowering and managed discarded locations | Preserve operand evaluation without loading/copying an unused record; optional destructor-name operand is navigation. Corrected the dot-field receiver lookup to ordered operand zero. |
| 17:15 | Stock runtime array zeroing and nonvirtual record delete | Project the selected byte-zero action and saved extent rather than deriving scalar zero representations. Record exact complete-destructor entry, pointer reuse and null guard; do not invent a destruction `finally` absent from this selected path. |
| 17:22 | Allocation/deallocation emitters and new fixture source | Existing schema suffices. Byte count and delete pointer are saved once; selected guard and member destruction precede the exact sized deallocator. Fixture tests pointer mutation during destruction, null paths and allocation-length failure. |
| 17:23 | Real exception-v12 and hugeint-v46 probe reports | Exception advances to the lambda blocker shared with checksum. Correct the observed array-count correspondence, then prioritize closure facts selected by EDG. Complete real-object count remains zero. |
| 17:30 | Allocation-actions-v3 generated source and verification | Saved byte extent and source pointer, selected null guards, complete destruction and sized deallocator reviewed. Native/C#/NativeAOT and related regressions pass; milestone `8e08ed5a0`. |
| 17:33 | Lambda patch and stock `lower_lambda` | Reuse the front end's closure type and aggregate initialization. No capture-analysis machinery, schema extension or new lowering callback needed. Generic call-operator prototypes must remain distinct from selected executable instantiations. |
| 17:36 | Local session database and latest agent turn records | Verified both implementers use `gpt-6.1-sol` with `xhigh`, and supervisor uses `gpt-6.1-sol` with `high`. Director runtime identifies `gpt-6-astra` with `high`; old agent path names do not change configuration. |
| 17:44 | Real exception-v13/checksum-v7 and file-scope lowering | Both pass lambda expressions and reach final startup-list capture. Prioritize this shared boundary over virtual delete. Dynamic startup order/guards are distinct from static data initialization; use EDG's selection. |
| 17:51 | Lambda-v3 evidence, checksum-v8 diagnostic and stock lambda entry generation | Captured values/references reuse selected aggregate actions. Startup diagnostics must exclude stock-ignored prototypes before claiming executable work. `_FUN` receives its body in `define_default_version_of_routine`; capture the selected generated call instead of synthesizing forwarding in C#. |
| 18:00 | Routine-address projection and checksum-v9 declaration failure | Reduced one selected routine-address constant from three synthetic nodes to one typed node. Preserve dynamic exception specifications as ordered type metadata, independently of executable policy support. |
| 18:10 | Lambda-v6 generated wrapper, dynamic-spec proof and member-pointer patch | Export exact scope-owned wrapper storage without inferring declarations from reads. Dynamic-spec declarations pass native/C#/NativeAOT with defined unsupported bodies rejected explicitly. Member-pointer owner/member references reuse existing type columns; retain source owner alias and reject unsupported nonzero modifiers. Complete real objects remain zero. |
| 18:25 | Array-bound patch, stock local-expression lookup and checksum-v12/exception-v15 reports | Two nullable type columns retain selected source bounds without re-evaluation. Stock lookup uses the recorded enclosing routine; missing required local correspondence remains an explicit exporter boundary. Both real probes advance to the same source `sizeof...` expression. Keep that projection small and move the managed worker onto required external declarations for source emission. Complete real objects remain zero. |
| 18:33 | Staged array/query metadata, array-bound-metadata-v2 SQL summary and import emitter patch | Fixed/zero/incomplete counts and selected dependent bounds remain distinct in schema 1023 (44 tables). Native/C#/NativeAOT proof passes; checksum-v13 advances to a source type-trait builtin. Batch direct source builtin projections without evaluating traits. Required-provider mode preserves signatures and link identities; verify function pointers and stable external-storage addresses, and keep unused source declarations out of imports. |
| 18:37 | Required-providers-v1 generated declarations and native/C#/NativeAOT reports | Function/global/RTTI address tests pass, but a generated global property consumes `get_imported_value`, renaming the C function to `get_imported_value__2`. Require original callable names to take priority over generated storage members, and add an exact-name fixture assertion before committing this milestone. |
| 18:47 | Required-providers-v2 source and real bundled ICU utf_impl-v1 object, timing and node 139 | Exact callable name is now preserved; provider/builtin milestone committed as `c68043962`. First complete configured-scope bundled object: 503,808 bytes, 14 functions, 1,652 nodes; full export 0.09 s / 17,920 KiB peak RSS. Core objects remain zero. C# stops at a plain-local nested assignment inside a short-circuit condition; prioritize direct named assignment preserving that conditional evaluation over independent bitfield breadth. |
| 18:52 | ICU generated-v2 complete source and required-import manifest | All 14 recorded definitions emit, with local assignments still inside short-circuit conditions and seven required provider declarations. Generation alone passes; compilation/execution is not claimed. Real source also exposes avoidable discarded-local compound-assignment helpers (`c &= 7`); direct the managed worker to retain ordinary named compound statements where exact selected types/order allow them. Native query milestone is `34d97152a`; dependent enum values next use an expression FK rather than forced integer text. |
| 19:03 | ICU generated-v3, source-variable patch and enum reference paths | `2b6db2560` emits ordinary local bitwise statements and removes unused helpers; real ICU source retains all 14 definitions. Nonreal variables use source-only declaration/initializer nodes and are excluded from executable symbol resolution. Caught the constant-path early return bypassing dependent enum identity attachment; native confirmed and added exact-ID verification before final acceptance build. No heuristic name/value matching. |
| 19:21 | Bitfield emitter, stock signedness/offset facts, bitfields-v1 reports and staged snapshot | Named properties use only recorded occupied bytes, signedness and target ordering; cross-byte canaries and string-shaped layouts pass native/C#/NativeAOT. Wider fields still use per-byte assembly, so this is correctness support, not final efficiency. Commit verified bitfield facts separately from ongoing source-query changes. Keep native on the shared core blocker and managed on actual ICU compilation/provider reachability using existing runtime support. |
| 19:26 | Batched recipe operator/query patch and source-query-recipes-v3 reports | Typed source operators retain ordered operands without forcing executable dispatch or RTTI storage. Fixture passes native/C#/NativeAOT; checksum-v19 advances to an aggregate initializer with a dependent target type. Preserve its selected type and ordered source elements without requiring runtime field layout or resolving the alias again. ICU differential harness remains an isolated verification task using existing runtime adapters. |
| 19:36 | ICU differential files, chimp constants source and progressing core export processes | Independently confirmed identical native/C#/NativeAOT result hashes and unchanged generated ICU bodies. Chimp is a complete data-only core object/source with zero imports, not an algorithm pass. Its per-byte static writes violate requested readability; assigned typed span constants plus `CopyTo`. Core checksum-v20 now reaches actual source-recipe initialization ownership; pursue that observed boundary while exception-v23 continues around 292 MiB RSS, without scaling the corpus. |

## Resumption and storage review on 2026-10-12

At 02:54 UTC the user resumed work. The saved eight process identities from the
pause were no longer present; none were signalled or confused with another
process. The existing supervisor and original implementation agents were
reactivated with their retained requested model configurations. Preserve the
uncommitted source-initialization and constant-array changes and restart bounded
probes through the supervisor.

At 03:00 UTC the director reproduced the suspected SQLite construction bottleneck
with the actual 44-table schema. Recursive forward-reference insertion of
1,000 / 4,000 / 12,000 pointer types took 0.083 / 1.223 / 10.548 seconds with
deferred foreign-key enforcement. Indexing every child reference reduced this to
0.010 / 0.043 / 0.125 seconds but adds many persistent indexes. Checking foreign
keys once before commit took 0.0025 / 0.0099 / 0.0287 seconds for insertion and
0.0001 / 0.0004 / 0.0011 seconds for the final check. These are an isolated storage
reproduction, not measured full-TU speedups.

The adapter now retains all declared foreign keys but enforces them with one
`foreign_key_check` before commit, rolling back a broken object. Immediate SQL
CHECK, UNIQUE and NOT NULL constraints are unchanged. This is object-reference
integrity, not C++ semantic verification. No schema or semantic payload changed.
`dotobj-relational-sqlite-writer-v1` compiles the actual adapter and passes valid
12,000-row forward/cyclic references, missing parent references, WITHOUT ROWID
references, invalid updates, immediate constraints and unfinished rollback.
The native valid case completes in 0.042 seconds. Supervisor owns the integration
build and real-unit measurements before attributing campaign throughput gains.

The integrated writer/source-initializer build completes actual checksum-v22 in
1.06 seconds (0.88 seconds user CPU, 199,168 KiB peak RSS), producing an
11,276,288-byte object with 42,592 types, 11,510 symbols, 31,105 nodes,
1,399 functions, 659 initializations and 107 lifetimes. All three definitions
originating in `checksum.cpp` are present. Earlier checksum-v20 rejected after
322.89 seconds; the source-initializer change also advances coverage, so this is
not an otherwise identical whole-TU benchmark. Exception-v24 also completes in
1.85 seconds / 298,752 KiB. These are complete exporter results, not full C#
generation or runtime acceptance.

Checksum generation now reaches a required external virtual table:
`_ZTVSt9exception`, selected address point 16. Extend required-provider metadata
and declarations for this exact ABI storage instead of inventing a table or
requiring libc++ implementation before source emission. Keep strict mode and
the external provider's stable-address obligation explicit. Whole-project
catalog loading remains an open memory-scaling issue; measure bounded batches.

The bounded core batch totals eight distinct attempted units: three complete
exports (Chimp constants, checksum, exceptions) and five explicit rejections.
The rejected probes complete in 0.67–1.12 seconds with 240–339 MiB peak RSS.
The four distinct boundaries are virtual deletion (`hugeint`), dynamic local
static initialization (`allocator`), multi-declarator initialization (`date`
and `string_util`), and string default member initialization (`buffer_manager`).
Prioritize the shared declaration case. Located second probes show two source
declarators followed immediately by their exact two selected initialization
actions. Extend existing fusion only over that matching contiguous sequence;
do not move initialization across intervening executable statements.

Chimp's compact span output independently passes the original 141-byte data
oracle and eight stable addresses in native/C#/NativeAOT, with identical result
hashes. The required virtual-table provider candidate preserves link identity,
selected address point and entry size; its contract requires stable storage and
generated managed-call signatures. A selected virtual callee is metadata for
dispatch, so it must not itself require a direct-call partial implementation.
Preserve imports for genuine direct calls or function addresses. The focused
proof must obtain its table from a genuine separately translated provider TU,
without fabricating slots or treating a native table as a managed table.

The selected external virtual-table milestone is now committed as `3c733387b`.
Its focused proof obtains the table from a real separately translated provider;
strict mode still rejects unresolved storage. Checksum generated-v3 passes that
boundary and zero-extent fields, then reaches a blanket virtual-base record
emission guard for `basic_ostream`. The two relevant record types have recorded
size 160/alignment 8 and no ordinary fields, with recorded base subobjects.
Separate physical record declaration from unsupported dynamic virtual-base
operations. Preserve hidden base/vptr bytes through managed value copies, and
retain the recorded alignment instead of introducing only byte-aligned storage.

Independently inspected `record-constructor-components-v1` generated source:
constructors receive `&(holder).view` directly and the constructor stores that
same address in `self`. The worker proof passes native/source-hidden C#/NativeAOT,
including ordered argument effects and a throwing constructor. The exact selected
initializer and constructor IDs are retained; no copied temporary is substituted.
The actual date probe now joins hugeint/string_util at virtual deleting destruction.

Independently inspected the corrected `zero-extent-fields-v2` generated source
and retained proof logs. A named pointer property denotes the source zero-sized
field at byte offset 1 without creating storage; size 24, adjacent fields/canaries
and pinned-global addresses survive the tested GC passes. The v1 fixture remains
an explicitly failed folded global-subobject relocation probe, not part of this
coverage claim. No wall-time/RSS measurement was recorded for the v2 proof.

Prepared the next allocator boundary by inspecting its actual local static
`shared_ptr<Allocator>` and stock lower_init.c guard/lifetime sequence. Reuse the
existing shared BCL CppGuards runtime and exact stock guard observations through
compact relational references. Initialization failure must allow retry; persistent
destruction must not be attached to the enclosing function's scope cleanup.
This is next-work guidance, not implemented relational local-static support.

The schema-1026 ABI extension adds four scalar/reference columns, retaining
44 normalized tables. Independent SQL inspection of checksum-v23 finds four
complete and four deleting table slots plus four ordinary slots. Every deleting
slot has its exact ABI symbol/type references; five genuine generated D0 bodies
have one recorded receiver. Exception-v25 has 28 complete / 28 deleting / 27
ordinary slots and 31 receiver-marked ABI types. Distinct complete core units
remain three; refreshed versions do not increase the count. The focused D0
metadata proof checks native once-only receiver/null handling and records the
real complete-destructor call followed by the selected sized delete. C# execution
of those entries is still pending its consumer implementation.

Fresh checksum-v23 exports in 1.02 seconds / 200,448 KiB and is 11,309,056 bytes;
exception-v25 exports in 1.81 seconds / 300,288 KiB and is 22,065,152 bytes.
Date-v6 and Hugeint-v50 advance past virtual deletion and scalar placement
initialization to file-scope dynamic record-array startup schedules. String-util-v5
instead crashes in our State::constant -> static_local -> capture inside the
new ABI factory callback. Launched GDB identifies an exporter read defect; no
EDG semantic change is authorized or indicated. Keep the native signal visible
rather than treating its driver's exit 1 as an unsupported-feature diagnostic.

Independently accepted `complete-record-storage-v1`: native/managed/NativeAOT
result TSVs have SHA256
`f18f6cc137190695e4f8e4f5389fa0c938485c9cfaee2c5bfb91abf0a743adfb`.
Named fields coexist with full-size byte backing and an alignment anchor. The
proof checks alignment 8 and packed alignment 1, embedded offsets/size, generic
C# full-byte value copies and generated-name collisions. This is physical
representation support, not implementation of source virtual-base copy
constructors or dynamic base operations. Checksum generation-v4 passes the
stream-class declaration and reaches the next explicit virtual destructor table
entry guard (0.88 seconds / 106,852 KiB), still producing partial C# only.

The scalar virtual-delete consumer now passes native/source-hidden C#/NativeAOT
execution. Independently inspected its generated D0 body: it invokes the genuine
complete destructor and then the selected sized delete; the caller evaluates its
operand once and null-checks before dispatch. External virtual-only destructors
do not create direct imports, while a genuine direct destructor call retains its
required provider. Native ABI/schema changes are committed as `cb16ed25b`.
Checksum-v23 generation advances to exception-base descriptors in 0.95 seconds /
106,472 KiB. It still produces partial C#, not a complete translation.

String-util-v7 now diagnoses the earlier crash precisely: a generated D1 wrapper
contains a lowering-created EH aggregate whose constant type is not yet assigned.
Stock `pop_generated_routine_context` completes that table after our callback.
Moving capture later and serializing C EH tables would violate the relational
program contract. The rich virtual-base D1 wrapper needs source-like selected
calls and cleanup/lifetime relationships captured before that machinery; it
remains an explicit unsupported case. No EDG semantic change is indicated.

Directed next work toward two shared boundaries: ordered file-scope startup
groups for Date/Hugeint, and class exception-base descriptors for checksum.
Base accessibility and ambiguity must be copied from EDG's selected results
(`preferred_derivation_of` / `access_to_end_of_path`, and `base->ambiguous`),
not recomputed by the managed consumer. Exception payload adjustment uses the
recorded base offset in the concrete complete thrown object. Existing shared
`CppExceptionBase` descriptors already support those facts.

Date-v7 and Hugeint-v51 now complete their first relational exports, raising
distinct complete core units to five of eight attempted. Independent read-only
SQL inspection confirms schema 1027 / 45 tables. Date has 3,066 function bodies
and one priority-zero startup group with four ordered aggregate actions:
`MONTH_NAMES_ABBREVIATED[12]`, `MONTH_NAMES[12]`, `DAY_NAMES[7]`, and
`DAY_NAMES_ABBREVIATED[7]`, retaining source lines 20/22/24/25. Hugeint has 3,889
function bodies and one priority-zero group initializing `POWERS_OF_TEN[39]`
at source line 20. Group rows reference ordinary function/body/action nodes;
they contain no encoded semantic payload or C EH table. Concrete base facts
are recorded, while dependent source types retain nullable selected flags.

Date export: 2.00 seconds / 318,972 KiB, 24,207,360 bytes, SHA256
`12fb8a26dc1d845cde948a565da4bbbed852d0655f31220d71e9974f51a40105`.
Hugeint export: 2.62 seconds / 391,680 KiB, 32,567,296 bytes, SHA256
`10065310cee4ff61262a0c2f3a195f875490447f2eef4fed781587aff326f71b`.
These are complete export results, not full C# generation/runtime claims.

Startup consumer review identified three necessary implementation details:
allocate and zero persistent storage before dynamic actions, write global
initialization nodes into that storage rather than declaring locals, and retain
complete record-array bytes/stride/alignment. Invocation catches must terminate
only after the group's selected cleanup has run. Nondefault priority ordering
across translation units still requires implementation or an explicit boundary;
silently iterating input units would not implement that policy.

Accepted the class exception-base consumer proof in
`dotobj-relational-class-exception-bases-v2`: native, source-hidden C#, and
NativeAOT result files share SHA256
`bc7c1ba6f6c0908b9bdeace0dce5eec636a368979583f11ef9248c4aa44224aa`.
The proof covers public indirect matching, private and ambiguous rejection,
and a virtual-diamond catch/rethrow adjustment of 32 bytes. A genuinely supplied
byte-copy constructor implements that fixture's external constructor body; this
does not prove translated virtual-base construction/copy. Missing recorded flags
produce a located exporter-data boundary rather than C# inheritance analysis.

Fresh checksum-v24 generation passes those descriptors, then stops on
initialization 2 in `__throw_bad_array_new_length`: the constructor action has
value-initialization set and no cleanup/partial flags. Directed the native side
to EDG's `need_zeroing_for_value_initialization` and selected complete/subobject
zeroing behavior rather than reproducing the rule in C#. Date-v7 next rejects
an incomplete array layout; Hugeint-v51 reaches the `ScalarFunction.statistics`
callback field at offset 280. Its source callable returns an incomplete
`unique_ptr<BaseStatistics>` declaration (type 5018), not a value stored in the
containing record. The next investigation must distinguish callable declarations
from actual value-storage or invocation ABI requirements without guessing layout.

Accepted the basic startup-groups-v2 runtime proof: supervisor confirms native,
source-hidden C#, and NativeAOT all exit zero with the same constructor trace and
data checks. Independently inspected generated C#: pinned stores exist before
the static-write phase; the group constructs `first[0]`, `first[1]`, initializes
`middle`, then constructs `second[0]`, `second[1]` directly in persistent storage.
The record-array wrapper retains eight bytes and alignment four. Broader canary,
whole-array-copy, priority and termination probes remain separate coverage.

The next Date boundary is now located: external incomplete arrays in fmt,
including `zero_or_powers_of_10_64`, require an element-address view rather than
an invented storage extent. The generator candidate skips incomplete array
wrapper layout, retains explicit failures for value/stride storage, and preserves
element types for pointer/address views. Verification is pending. The small
schema-1028 candidate records EDG's `need_zeroing_for_value_initialization`
decision; false permits the existing constructor path, while true remains an
explicit selected-zeroing boundary until exact byte/helper behavior is supported.

## Startup extent and incomplete external arrays: independent acceptance

Director reviewed committed startup execution and external incomplete-array
fixtures plus retained generated sources and logs. Startup native, managed and
NativeAOT output agrees exactly: `4\t8\t16\t4\t12\t1234578`, SHA-256
`0433eaf75c6660ba01019276092eebdc859ef9dd409094f3bada1e05ed73d995`.
The canary fixture checks complete array bytes, containing-record offsets,
whole-value copying, constructor order and pinned address stability. It does not
claim persistent destruction, TLS or nondefault-priority initialization.

The external-array proof uses a genuinely separately translated provider, required
address imports, and direct linking in both input orders. Managed and NativeAOT
builds pass in all three configurations. The consumer database retains unknown
outer extents as NULL; nested fixed extents remain recorded and no external storage
is fabricated. Actual DuckDB Date-v8 and Hugeint-v52 pass these former declaration
barriers and converge on the address-of-volatile-field consumer boundary.

Inspected opaque callback implementation retains the source signature in SQLite,
emits address-sized storage when the result ABI is unfinalized, and rejects actual
invocation absent a recorded callable ABI. Linking a completed record layout by
exact external identity does not by itself authorize inventing a function ABI.
Runtime proof for this callback slice is tracked separately by the supervisor.

## Callback storage and address/aggregate execution acceptance

Director independently reviewed `opaque-callable-storage-v1`: incomplete source
return declarations retain pointer-sized callback storage; actual provider ABI is
required for function address binding. Both link orders pass native/source-hidden
C#/NativeAOT address identity, copy/null and GC checks. The absent-provider case
fails at a source-located missing finalized ABI. Commit `81c8f34ec`.

Director reviewed `address-record-elements-v2`: volatile field and indexed
location address evaluation calls receiver/index once, while a volatile pointer
load is still rejected. Direct enum/char[48] record elements retain exact array
stride and neighbor bytes in native/source-hidden C#/NativeAOT. Commit `3854a5080`.
The initial generator bug used the unary child accessor for multi-operand fields;
root identified it from the real four-TU failure and the corrected snapshot passed.

Checksum-v25, Exception-v27, Date-v8 and Hugeint-v52 generation-v3 all pass prior
barriers and stop at the same builtin comma in a `for` step (`__locale:552:36`).
SQL confirms checksum has eight for-step commas plus three nested commas, allowing
ordinary C# comma-separated header statements without a new helper or control-flow
rewrite. Generic comma values/lvalues remain a separate execution requirement.

Keep source/readability and allocation improvements on the remaining-work list:
current array-record fixture redundantly clears parent/row/char storage and emits
one pinned allocation per literal with hex bytes. Earlier requested compact
struct initializers, UTF8 spans/comments and pooled literals still apply to this
new consumer. These do not justify expanding the semantic database.

## Initializer expression cleanup and the next shared call boundary

Director accepted `initializer-expression-temporaries-v1` after inspecting the
source, SQL summary, generated wrapper and native/managed/NativeAOT evidence.
The wrapper uses a typed local, marks it complete after its producer returns,
constructs the outer result directly in caller storage and destroys the temporary
once. The fixture checks self-address preservation, success trace 12435, failed
temporary construction trace 1 and outer-call failure trace 123. A genuine
potentially throwing temporary destructor is rejected for caller-provided results
without recorded return-result unwinding support. The existing `CheckReturnCleanup`
checks the selected destructor declaration policy; no result cleanup is invented.
Committed as `6d64f0d6a`.

All four core generation-v5 attempts advance to node 5079, the right-to-left
member call in `sql_identifier.hpp:50`: `lhs += SQLIdentifier::ToString(rhs.raw_string)`.
EDG already records the order, function target, receiver lvalue and argument
reference. Consumer work must materialize values/receiver addresses according to
that order, and reject contexts that bypass the expansion until implemented.

Director requested a measured follow-up on EDG final needed/definition-needed
facts. Existing native and old C# backends use these, while the relational linker
emits every captured definition. Retain all source facts in SQLite. Native
emission facts can guide roots, but the earlier executable graph's actual
references still require ordinary linker dependency closure: a function used
there must not disappear merely because later stock lowering simplified its call.
This is not authorization to reanalyze C++ semantics or discard unsupported
needed code.

## Right-to-left checkpoint and destructor condition timing

Right-to-left call/address/lifetime support is committed as `f32d9a4e0`.
The four current functional core probes pass that boundary and stop at the
unordered expression-temporary group in `Exception::FormatStackTrace`.
The candidate consumer chooses a valid linear evaluation order and destroys
completed temporaries in reverse order without per-call heap allocation.
Its proof must include failed constructors, outer-call failure, and the ordered
receiver neighbor; dynamic conditional producers cannot silently use lexical
completion order.

Fresh Leaf-v3 and StringUtil-v10 exporter attempts pass the earlier ABI signature
timing boundary and expose retained virtual-base cleanup ownership. The active
stock configuration keeps both the D2 original actions and D1 copied actions.
D2 gates its originals on complete-object entry status; D1 supplies a non-null
VTT and then performs its copied actions. Keep both actions and their exact
source-copy relationship, with a typed condition expression/reference.

Director inspected `lower_init.c`: the complete-object comparison is evaluated
once at destructor entry, before default construction-table initialization can
overwrite the same VTT parameter. Re-evaluating `(vtt == NULL)` at cleanup is
incorrect. Preserve the entry-phase typed local/capture and use that stable value
for normal and exceptional cleanup. This requires no C exception tables.

A bounded stock-C checksum comparison in `dotobj-director-native-selection-v1`
stopped immediately because stock targets lack the portable system macro hook.
No code was generated and no equivalent-emission claim is made. Prefer recording
final needed/definition-needed facts from the real portable-target compiler for
the next selection measurement.

## Native emission measurement: selection is now the highest-impact lane

The follow-up used the existing portable-target `cpfe-dot` and its legacy
`EDG_DOT_TRACE` hook, which runs `routine_body_needed` before C# generation.
Exact original checksum command and environment are retained in
`dotobj-director-native-selection-v2`; no provider-pruning manifest was used.
The complete trace selects four bodies: `Checksum(ulong)`, `ChecksumRemainder`,
`Load<ulong>` and `Checksum(byte*,ulong)`. The later legacy generator stops at
its executable-only no-main check, after the trace. Runtime 0.46s, peak RSS
183296 KiB. This is a needed-body measurement, not a successful C# build.

Checksum-v25 contains 1,404 captured function rows (the earlier 1,399 count was
the link-selected subset). SQL inspection of the four bodies finds references
to each other and external `memcpy`; their initialization counts are 0/6/0/2.
This makes exact final native emission facts plus ordinary linker dependency
closure the next priority over unrelated header-body execution gaps. Preserve
all source facts. Closure must follow required earlier-graph executable facts,
including initializer/cleanup/ABI references, rather than blindly deleting any
body absent from a later stock-lowered native list.

## Unordered temporary acceptance and broader selection evidence

Director inspected `unordered-expression-temporaries-v2` SQL and generated
`evaluate`: a comma-sequenced receiver precedes two unordered Value temporaries;
chosen construction order 1/2 yields cleanup 2/1/receiver. Completion flags follow
successful constructors. Native/source-hidden C#/NativeAOT cover both constructor
failure positions and outer-call failure, with caller-result self identity.
A conditional shared-result producer is explicitly rejected by structural branch
context, independently of EDG's action flags. Generation-v7 for checksum and
exception passes the former unordered boundary and reaches a record-initializer
value in libc++ string:3917. Do not make that unselected header function a new
checksum gate.

The same original-command portable-target trace gives captured/native-selected
counts: checksum 1404/4, date 3066/388, exception 2748/639, hugeint 3889/297.
All traces complete before the legacy executable-only no-main check. Selection
should use the finalized EDG symbol root fact while retaining all captured bodies
as available definitions and traversing actual executable dependencies.

A native oracle from the actual DuckDB checksum.cpp is retained in
`dotobj-director-checksum-oracle-v1`: 727 TSV rows, four input patterns, aligned
buffer storage, all short tails, larger block boundaries, and uint64 overflow
inputs. Native TSV SHA256 is
`8c80e57526c965b12d6491a62c64bcba2d082e077f9900e707efe578ec142512`.
This is a numeric/byte-result oracle, not a C++ standard-library ABI claim.

Unordered temporary milestone committed as `4ae5c0e23`. The native root flag
and managed dependency selection are the active next implementation lane.

## Authoritative emission-root export accepted

Director independently queried `dotobj-relational-duckdb-checksum-v26/checksum.dotobj`:
schema 1030, 1,404 function rows retained, exactly four function roots matching
the independent native trace, no other roots and no unknown flags. Size
11,354,112 bytes; SHA256
`3096ad70b0036b0339dd050d527fc46314bed8ca7227e27fec3ae5a427f58cd4`.
Worker proof reports integrity/FKs clean and export 1.06s/200192 KiB. The focused
root fixture preserves dormant source bodies/storage and selected startup facts.
This accepts root export, not completion of the rich destructor ABI lane.

Director also required a generator capability boundary for new destructor
entry-condition/ABI-prologue and selected cleanup invocation facts until those
are actually emitted. Reading a correct new fact and silently omitting it is
not acceptable; rejecting an unimplemented generator operation is distinct
from rechecking EDG semantics. Plain checksum contains none of these operations.

## Final root selection and first checksum C# generation

The rich destructor fixture v5 independently records the entry destruction
condition before its VTT-writing ABI prologue; D2 originals retain that condition
and D1 copied cleanup actions do not. Explicit generator boundaries remain for
these not-yet-emitted operations. This is export acceptance, not D1 execution.
Native ownership/root milestone is committed as `815d2a79a`.

New schema-1030 objects have function root counts checksum 4, exception 496,
date 338 and hugeint 273. The higher legacy trace counts above include absent
physical C2/D2/CI2 variants; audit found no ordinary-function discrepancy. Retained
source C1/D1 entries are roots. Do not invent extra semantic entities to reproduce
a physical ABI trace census.

Checksum-v26 with `native-root-selection-v2` now emits complete C# selecting
four of 1,404 functions and three of 196 globals, retaining the complete database.
The 727-case native/JIT/NativeAOT comparison is pending. Director source review
found unnecessary discarded-local compound assignment helpers and switch labels;
readability is not complete. Prefer direct compounds for pure RHS expressions,
and an RHS temporary followed by a direct compound when needed to preserve EDG's
RHS-before-old-LHS-read order. Do not infer that calls cannot alias local storage.

Checksum selected-source runtime acceptance: director independently compared all
727 rows in `dotobj-relational-checksum-selected-oracle-v1` against the original
DuckDB native oracle. Managed and Linux x64 NativeAOT TSVs are byte-identical,
SHA256 `8c80e57526c965b12d6491a62c64bcba2d082e077f9900e707efe578ec142512`.
Builds/runs exit zero. Exact generated source is retained unchanged; its sole
external memcpy adapter uses shared `Edg.Runtime.LibcContext.Memcpy`. This proves
one functional TU, not all DuckDB or runtime portability on other systems.

## Bounded catalog scaling and first real cross-unit link defect

`dotobj-relational-catalog-scaling-v1` measures one/two/four distinct actual
checksum, exception, date and hugeint objects, without duplicated-object scaling.
Retained managed catalog heap is 30.3/90.6/243.2 MB; peak RSS
79,368/148,688/318,532 KiB. Four inputs contain 331,890 type rows and 98,091
symbol rows. Selecting four checksum functions leaves its roughly 30 MB catalogs
retained. Body leasing alone cannot make the full-project link memory bounded.

The two-input real link separately fails on duplicate strong __digits_base_10.
Director read the original libc++ tables.h:143 inline constexpr definition,
queried both objects (external/strong/raw unmangled link_name), and inspected
variable() export: its generic symbol INSERT omits definition_kind and generic
variables lack a final link-name refresh. Adapt direct EDG inline/COMDAT/linkage
facts; do not bypass duplicate-definition checks or independently reinterpret
C++ source. Native investigation is active.

Bounded-catalog implementation must avoid whole-table materialization disguised
as lazy Values enumeration: query locals by parent and fields by owner, iterate
selected type IDs, and keep record provider indexes as identities rather than
retained full records. Measure the same real inputs before expanding the run.

## Compound source readability and selected next frontiers

Director independently accepted `dotobj-relational-source-compounds-v4` worker
proof and re-compared `dotobj-relational-checksum-selected-oracle-v2`: all 727
managed and NativeAOT rows still equal native. Checksum generated-native-roots-v4
is 6,001 characters and contains no multiply/xor assignment helpers. Pure RHS
local operations emit ordinary compounds; effectful RHS is evaluated once before
the old local is read, with a focused escaped-address mutation case. New integer
multiplication compounds preserve wrap under checked host compilation. This
checked-host claim is limited to these changed operations: other existing raw
arithmetic expressions still rely on the generated project's default context.

The assignment cache now restores an existing RHS temporary binding after its
scoped replacement. Actual Date/Hugeint objects pass their former duplicate-key
failures. Director inspected Date's failing emitted signature and SQL: function
2338, string_t(char const*), action209 is nonvirtual delegation to the same type,
not virtual-base construction. Function2343/action210 is also delegation.
Prioritize this actual narrow case over a generic virtual-base ABI campaign.

StringUtil-v13 passes the new void-constant export and stops at ordinary
new ComplexJSON with EDG-selected constructor/failure operator-delete facts;
source helper.hpp:66:72. Native inline-global metadata remains the first next
export lane because it blocks real multi-unit linking.

## Primary catalog reduction measured

Director reviewed the new SQLite-backed row catalog: bounded process-wide
4,096-entry LRU per primary row family; Values/Keys enumerate SQL without cache
materialization. Parent/owner queries replace repeated whole-table scans. Query
plans confirm field/type-parameter indexes; a symbols-parent index is needed
for fresh objects. Failed Unit construction now clears initialized cache owners.

Independent artifact read of `dotobj-relational-catalog-scaling-v2` confirms
same-input one/two/four catalog managed heap 1,996,648 / 5,119,408 / 12,772,608
bytes, versus 30.3/90.6/243.2 MB before. Row counts are unchanged. Supervisor
reports peak RSS 46,080/57,600/88,064 KiB and identical checksum source. This
accepts primary catalog reduction only: connections, auxiliary catalogs, winner
indexes and emitter name/type plans are not yet globally bounded.

Independent inline-variable audit: director compiled the focused provider/main
with immutable native-inline-globals-v1 into director-inline-global-audit-v1.
Used and dormant left/right namespace variables retain distinct EDG-mangled
identities and weak definitions; shared digits are weak, strong_value remains
strong. Fresh checksum-v27 and exception-v29 SQL both record __digits_base_10
as `_ZNSt11__dotgen_v16__itoa16__digits_base_10E`, weak/root0, and __pow10_64
as `_ZNSt11__dotgen_v16__itoa10__pow10_64E`, weak/root0. This fixes the prior
raw strong header-variable collision without weakening linker diagnostics.

Primary catalog stress independently read: all 331,890 type, 98,091 symbol and
5,137 field point/stream rows match across four real objects; each row-family
cache remains at 4,096 entries and disposing all units releases every cached row.
Emitter selected-ID iteration preserves ordering; owner/parent lookups avoid
full-catalog scans. Connection pooling remains the separate next memory step.

## Precise lifetime frontier classification

After delegation generation, Date-v9 node15139 is a void/prvalue lifetime scope
containing throw node15140, lifetime191, function3795. It is not a record-valued
expression. Hugeint-v53 node16895 is a record-typed **lvalue**, with ref-indirect
operand16896, lifetime238, function4384. Its result needs preserved address
identity across cleanup, not an invented record copy. The old generic diagnostic
grouped void, record, array and function results together; use actual exported
type/category/operand facts to choose the next implementation.

Director also required removing redundant delegation validity checks (same
receiver/destination type identity and impossible member-field combinations).
Trust EDG's recorded delegation relation; retain only required missing-data and
unimplemented representation/cleanup capability boundaries.
