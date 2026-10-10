# Owned source constraints and generated execution origins

Profile55 retains original concept declarations, formal parameters, requires
expressions, all four requirement forms, lexical binders, implicit constrained
first-type owners, original expression/lvalue types, operations, ordered operands,
substitutions and declaration references. It stores these as SQLite relations
with stable IDs and source positions, separately from executable function bodies.
Source-only declarations do not fabricate lowered routine ABI slots.

Concept/class template argument packs retain explicit boundaries, empty packs,
pack elements, expansion descriptors and references to original parameters.
Dependent function-template lookups retain available candidates and explicit
argument lists, including an explicit empty list; they do not invent a selected
overload. Class identity and managed verification use the retained pack facts.

Semantic constants can reference the same owned expression/declaration graph.
Class-valued constexpr initializers retain their aggregate type, ordered original
constants and initialization flags. Original sizeof/alignof expressions can be
represented by EDG as constant expressions; the expression-to-constant edge
preserves both the query and its operand type. Dependent boolean operands keep
an unresolved original type until substitution; concrete nonboolean constraints
are rejected. Concept-id and requires-expression result types remain boolean.

These contracts are not a claim of complete dependent-type coverage. The actual
libc++ concept frontier now requires alias-template declarations/instances and
original type operators behind an unresolved alias result. Actual DuckDB also
requires static members whose template parameters belong to their class. Both
remain explicit exporter gates and are the next work; accepting an undifferentiated
unknown type as nominal identity would lose required information.

The same checkpoint adds factory-owned TLS wrappers and initializer aliases.
The factory input retains its original TLS variable/type, exact generated routine
IDs and weak-address guard. Imported declarations may not occur in scope variable
lists, so they are observed before wrapper construction as well as variable
lowering. Generated wrappers are reserved before source traversal and cannot be
mistaken for original functions. Their bodies must match the exact guarded
initializer call and original storage address return. Constant TLS passes
cross-unit, separate-thread and NativeAOT execution. Dynamic TU TLS initialization
remains a separate owned initialization/lifetime contract; it is not relabeled as
process startup.

For initializer-list backing arrays, original temporary type is the entire
const array while its destruction obligation names the element class. The reader
requires matching original owners, exact original leaf type and an observed
full-array cleanup with the exact flattened extent. This also checks malformed
shortened/lengthened cleanup counts; it does not equate arrays with arbitrary
same-layout records. Current mixed array/element acceptance requires that owned
cleanup proof. More general elided or non-EH-only forms stay gated.

Validation:107 focused source-graph checks,53 TLS checks,40 initializer-list
checks and451 existing source-type/template checks. Focused suites include
source-hidden linking, JIT, NativeAOT and resealed metadata rejections. Copied
default-argument execution ownership adds70 checks documented in
[the C++ runtime notes](dotobj-cpp-runtime.md). Retained evidence and limitations
are recorded in `evidence/dotobj-duckdb-constraints-tls-temporaries-20261010.json`.

The accepted profile58 source-graph milestone additionally retains original
unevaluated temporary initialization recipes and selected ordinary members of
concrete class-template instances. Concrete nominal record identities remain
unchanged; their original selected member and enclosing class substitution are
retained independently. This currently covers the selected-member path, not all
otherwise captured concrete template records. A partial specialization requires
both the primary-template and selected-specialization argument lists and remains
an explicit gate pending that contract.

Original new/delete expressions preserve allocated/deleted source type, selected
operator declaration where present, source flags and ordered arguments. Source
operator declarations carry their exact original operator kind. GNU prototype
instantiation deliberately defers even `::new int` lookup; the frontend's
`unknown_dependent_new` decision is observed and copied with the original
supplement, independently of type dependency. Dependent delete retains EDG's
unknown deleted-type placeholder and the actual dependent operand or pointer to
dependent type. Concrete delete operands must match the original pointed-to
type. No callable runtime ABI is fabricated from any of these source-only rows.
Linked initialization and rollback graphs outside the accepted absence case
remain explicit gates.

Focused evidence for these additions: temporary/member metadata has42 combined
checks at `dotobj-v58-temporary-145-168-tail-002/results.json` (referencing its
original execution receipt); allocations have50 checks at
`dotobj-v58-source-allocations-147-173-002/results.json`, both beneath
`/mnt/storage/dotcc-edg-campaign-20261003`. These include source-hidden loading,
JIT/NativeAOT and resealed malformed metadata. Profile58 passes 1,042 broad
integration checks and 421 focused checks across its retained feature cuts.
It is an accepted feature milestone, not a whole-project checkpoint: remaining
original source graph families and the coherent full corpus are still pending.
The combined receipt is `evidence/dotobj-duckdb-source-graph-v58-20261010.json`.

Brace-enclosed source initializer lists retain ordered elements, including an
empty list. Original fold expressions retain the configured operator token and
spelling, left/right association, all ordered operands, and each expression's
original pack-expansion marker; EDG can retain more than two operands after
partial substitution. Original field operands point to the existing source
record/field declaration, with identity derived from original owner and member
ordinal. These are semantic-only nodes and do not request executable layout or
invent a folded result. The focused148/174 suite passed47 checks including JIT,
NativeAOT and seven resealed negative cases, retained at
`dotobj-v58-source-lists-148-174-001/results.json` beneath the campaign root above.

The profile59 partial-class extension preserves both original substitutions:
arguments against the primary template header, and arguments against the
selected partial specialization's header. Each header retains original formals,
defaults, pack markers, constraints, and the partial's pattern arguments. Header
contracts compare the full original pattern details; actual substitutions use
semantic argument identity so equivalent explicit and defaulted arguments do
not acquire different nominal identities. Concrete class ABI identity remains
unchanged. Source-position prototype identity distinguishes separate partial
specializations of one primary.

The focused149/175 suite passed 43 checks, including two partial specializations
of one primary, distinct primary/selected formal arities, selected non-type
arguments, hidden-source verification, JIT, NativeAOT, and six resealed negatives.
Evidence: `dotobj-v59-partial-class-149-175-002/results.json`. The retained first
attempt, `dotobj-v59-partial-class-149-175-001`, separately reproduces the original
pointer-typed `ck_integer` constant verifier gap; the class test now uses an
original requires-bound pointer to isolate the substitution graph. This receipt
does not establish acceptance of the pending profile59 real-project frontier.

Original `ck_integer` values can have pointer source types after a cast, as
specified by stock `il_to_str.c::form_integer_constant`. The source verifier now
accepts canonical zero while retaining the exact pointer and pointee signature;
it assigns no integral width to the pointer. Nonzero addresses remain outside
this bounded slice. The focused150/176 receipt
`dotobj-v59-null-pointers-150-176-001/results.json` passed 29 checks: data-pointer,
const-pointee and function-pointer nulls, hidden-source verification, JIT, and
resealed nonzero/negative/noncanonical pointer and ordinary integer-overflow
rejections. The subsequent `nullptr_t` literal and original non-type formal
extension passed 32 checks in `dotobj-v59-nullptr-151-178-002/results.json`:
canonical null payloads retain their exact source type; template parameters retain
original coordinates and never acquire a fabricated integer payload.

Profile59 retains dependent enum declarations with original class owner, ordered
resolved or symbolic values, scoped/anonymous flags and declared/inferred versus
prototype-placeholder underlying-type provenance. Nonreal enums never acquire
an invented physical representation. `source_enum_increments` records EDG's exact
implicit-successor factory, including its deliberately unknown-typed previous
value clone; this does not relax arbitrary wrapper type equality.

`source_pack_sizes` preserves the original type, expression or template-template
operand of sizeof...(pack), both original category flags and its unsigned result
type. It stores no computed count before substitution. Type and value packs are
validated against their exact parameter declarations. Original auto/decltype(auto)
placeholders preserve EDG's type-specifier coordinates and optional constraint
graph independently from ordinary template-formal identities.


Original namespace ownership now has typed declaration rows and owner edges for
source types, expression declarations, concepts and alias templates. Empty
unnamed namespace spelling stays empty; reopenings reuse the original namespace
entity. Private namespace identity uses its exact parent plus producer UnitId,
including named descendants. Public namespace identities remain shared across
translation units. Record, enum, declaration, concept, alias and template-formal
identities retain this ownership. Formal namespaces are observed from the original
frontend template symbol while available, including later cached recapture;
conflicting original ownership is rejected.

The frozen155/182 namespace suite passed 53 checks in
`dotobj-v60-namespaces-155-182-001/results.json`: two units share a header but own
private records with distinct identities/layouts (8 and 12 bytes), source files
are hidden before verification/linking, and native O0/O2 and translated JIT agree.
Six resealed negatives cover original namespace spelling, repeated declaration
identity, internal linkage, missing ownership, marker contradiction and parent
cycles. The earlier154 failure is retained; it exposed the missing private
formal ownership path now covered by the paired test. This slice does not yet
establish full concrete-instantiation identity for every public template whose
actual type argument is TU-private; complete concrete argument provenance remains
a separate requirement.

Concrete class substitution capture now retains the exact selected original member
that caused the record snapshot to be enriched. If an unused template's expression
is not otherwise serialized, emitting the enriched record still emits its original
member/prototype declaration closure. These remain semantic declarations and gain
no executable body. The strict existing class substitution verifier is unchanged.
The five-line unused-template reducer reproduces the prior real `exception.cpp`
gate; `dotobj-v60-concrete-witness-155-182-001/results.json` passes 26 native,
source-hidden verification, JIT and malformed-witness checks. These focused receipts
do not claim full DuckDB translation or acceptance of all candidate-profile60 work.

### v60 proxy and lexical type closure

Original nonreal class proxies retain their exact `proxy_of_type` target. An
unknown target remains unknown and unit/occurrence-local, with no invented
physical class layout. Explicit cast destination types retain their lexical
parameter context through alias arguments; borrowed semantic result annotations
do not invent a new prototype owner. Top-level formal qualifiers are restored
from the original parameter descriptor when checking parameter references.
Proxy/cast acceptance155 passed39 checks, and qualifier acceptance154 passed28.

The source ownership frontier now exports43/45 units and verifies41 after the
managed184 local-static temporary correction. This is a bounded checkpoint, not
a full corpus or a generation/compilation claim. Source-only dependent bitfield
width recipes and borrowed instantiated prototype types remain explicit gates.

### v61 original routine type-argument substitution

A function-template instantiation may reuse the exact original type node from
another function's trailing `decltype` return. The consuming prototype does not
own that type's parameter-reference recipe. Capture now records an exact boundary
at an original `tak_type` argument root, suspends only the consuming routine's
binder at that boundary, and preserves the original source type and lexical
owner. Transparent pointer/reference/qualification wrappers remain in the graph.
Stock `il_def.h` documents the routine's ordered actual template argument list;
`il_to_str.c::get_param_for_param_ref` resolves parameter references through the
original prototype stack and inside/outside parameter-list phases.

`source_routine_types.has_borrowed_arguments` requires a corresponding
`source_routine_type_substitutions` row selecting the existing original template
routine declaration. `source_routine_borrowed_arguments` retains exact actual
argument ordinals and source type IDs. The declaration's prototype and argument
closure remain semantic metadata, without a fabricated ABI or executable body.
The verifier checks that the selected argument occurs in the consuming signature,
retains an original parameter recipe, and has not been reparented to the consumer.
Missing original lexical ownership still fails; the adapter does not invent a
binder or waive an unrelated scope conflict.

Frozen158/187 acceptance passed39 checks in
`dotobj-v61-substitutions-158-187-002/results.json`: native O0/O2, hidden-source
verification/linking, exact original argument and prototype identity, nested
pointer recipe ownership, translated JIT, and five resealed negative cases.
The retained001 run exposed only a test setup assumption that an unused consuming
binder would be serialized; its correction mutates the actually retained recipe
binder's owner. The production capture/reader did not change after the frozen
build. This focused receipt does not establish full DuckDB translation.

### v61 original source bitfield widths

`source_bitfield_widths` retains the exact original constant recipe and either
an evaluated declared width or a dependent width. `source_record_fields.bit_width`
is EDG's original stored value-bit count; for a nonreal class that count can be
a placeholder and does not establish a physical layout or declared width. The
reader checks original recipes independently, including C++ declared padding
widths beyond the value type's width. Cross-unit source record comparison also
compares the declared width identity. No dependent integral width is invented.

Native157/managed186 passed36 focused checks (source-hidden JIT and five
malformed cases), including symbolic `sizeof(T::size_type)*8-1`, a width template
parameter, a40-bit declaration with32 value bits, and unnamed zero-width fields.
The real exception.cpp/string_util.cpp objects now verify. Final158/187 record
and layout regression passed153 checks, and the45-unit probe exports/verifies44.
