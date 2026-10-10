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
