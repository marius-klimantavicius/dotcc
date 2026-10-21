# Additive control-flow integration handoff

Actual integration against accepted string/Unicode main f6d0e8dc8, retaining
99643b48e frontend bytes. Implementation agent ran no builds/tests/native/Git.
Full PLAN remains active; this is an early integrated source checkpoint.

The accepted isolated contribution is retained unchanged in
`evidence/controlflow-implementation-handoff.md`, SHA-256
fe063c1121c9c6988ea8eeb79df645bee00b5ca5561d0da2d9cc8cacd5defffe.
Its source/routine hashes, exact branch attribution and bounded118-group review
are the algorithm reference for this merge, not proof of main136 behavior.

## Imported and manually merged paths

Imported authored ScalarControlFlow.cs, ControlFlowTests.cs and all18
m4-controlflow fixtures. Main central files were never replaced from oldbase446:
Expressions.cs adds assignment precedence/semantic construction around current
literal/pointer/subscript/sizeof logic; Parser.cs adds control dispatch and for
scope restrictions around canonical Unicode lookup and journaled array inference.
SemanticGraph.cs adds effect/dependency queries, assignment and structured
statement/target nodes, preserving current array/parameter/category metadata.
ConstantEvaluation/ConstantStorage add explicit control work, transfer state,
readonly/lifetime-origin/store observations and per-evaluation cleanup while
retaining every array/pointer/nonobject/ref_indirect result hook.
Program adds16 registrations and replaces only actual obsolete assignment/if
Unsupported expectations. All current120 registrations stay; expected count136.

## Storage and cleanup invariants

ConstantCell remains the actual scalar/element identity. ReadOnly defaults to
actual allocated cv, not reference access cv; public readonly/origin fields are
nullable for unknown objects. BeganInEvaluation is explicit: local/value-param
cells true, local array elements true, materialized temporaries !isStatic,
preexisting/imported/global/literal objects false. Every scalar store uses the
same known/alive/readonly/origin/access-order checks for ordinary objects and
array elements. Assignment observations point to the same immutable snapshots as
Array.Elements and reference binding observations.

LeaveScope preserves LeaveAggregateScope and owner-cell expiration on normal,
break/continue/return paths. Frame exit removes aggregate activation slots.
Nonobject one-past markers never become ConstantCells. Direct transport versus
ref_indirect/load/store/persistent-result gates remain from accepted strings.
The Location helper rejects nonobject stores; pointer and scalar payloads remain
distinct, with no pointer or string integer surrogate. Result snapshots include
both modules' observations and every mutable collection clears on every exit.

## Queries, source order and templates

Both iterative effect/dependency traversals explicitly handle StringLiteral,
ArrayInitialization, SizeOf, DependentSizeOf, Subscript and PointerArithmetic.
sizeof ignores its unevaluated operand; subscript traversal and execution retain
original SourceLeft/SourceRight and PointerOperandIsSecond. Composite pointer /
subscript dependency getters use the iterative helper rather than recursive
child getters. Unknown effects remain conservative.

Only known nondependent scalar arrays in analyzed template bodies are newly
allowed. Initial signatures and dependent elements/bounds remain Unsupported,
with explicit dependent-bound detection before constant evaluation; there are no
placeholder array types/values standing for dependence. Concrete replay uses
normal array inference, owner journal and expression semantics. All existing
Unicode identity, scoped type-cache ownership, ErrorCount, replay/rollback and
source/provenance rules remain.

## New exact integrated gate and authored controls

The existing ControlCombined group additionally calls ControlArrayCombined in
ControlFlowArrayTests.cs; no extra registration changes136. Shared exact files:

- m4-controlflow-array-gate.cpp/.h and native-only array-witness.cpp.
- array-readonly.cpp, array-expired.cpp and array-nonobject.cpp negatives.

The positive source combines included Unicode macros, mixed string prefixes,
known local char16 array inference, int/long template instances and reuse,
while/if/continue/break, element compound stores and references returned by the
frozen earlier const-reference helper despite later void overload/caller hiding.
Queries inspect actual final/stored values, identical element/reference/store
snapshots, readonly/lifetime-origin/expiration, source-visible candidates and
retained macro/include origins after disposal. Native witness includes unchanged
source and checks literal array type plus later mutable helper void result.

The reversed target `(counter=1)[cells+(counter=2)-1]='Z'` must produce290 and
counter stores1,2; this discriminates source E1-before-E2 against190. Negative
source proves literal const assignment diagnosis, expired array store and onepast
nonobject store with no successful writes. sizeof(a[index=1]) has no effects or
writes. A flat2048-term subscript index checks iterative public dependency/effect
queries and budget/failure/success/repeated-failure interpreter cleanup.

All tests are authored, awaiting serialized main Release/JIT/AOT and independent
review. Isolated118 and old main120 receipts do not qualify these merged bytes.
Remaining selection/condition declarations, switch/goto, range-for, increments,
shift/pointer/class stores, dependent arrays and all other inventory gaps remain
required; bounded integration is not full language/campaign completion.
