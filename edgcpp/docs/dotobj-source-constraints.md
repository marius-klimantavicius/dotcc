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
