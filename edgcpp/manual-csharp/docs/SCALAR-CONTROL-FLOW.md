# Scalar assignment and structured control flow

The accepted bounded main integration executes136/136 groups under JIT and
Linux-x64 NativeAOT, with four identical145-file source/data/tool maps, three
actual output fingerprints and independent review in
`evidence/review-controlflow-integration-final.md`. All23 exact native controls
match (9accept0/14reject2) under the retained strict C++20/LP64/used-instantiation
profile with explicit UTF8 source kind. This is a checkpoint, not full coverage.

## Correspondence and ownership

`ScalarControlFlow.cs` follows EDG expr.c assignment scanning and statements.c
structured condition/body/loop-target construction. `ConstantEvaluation.cs` and
`ConstantStorage.cs` follow interpret.c explicit work, location/load/store, frame
and activation cleanup. exprutil.c effect/dependency and il.c graph ownership
correspondence remains explicit in `evidence/controlflow-integrated-handoff.md`;
`source-controlflow.json` fingerprints the five consulted files. No whole-file
port is claimed. Original retained-token template replay and lookup identities
remain authoritative.

Integer simple and arithmetic/bitwise compound assignment yields an lvalue,
evaluates RHS before one destination evaluation, and mutates the actual shared
cell. Simple assignment can initialize a permitted uninitialized local; compound
assignment loads first. Readonly derives from actual object cv, and lifetime
beginning is an explicit origin fact, not inferred from an active caller frame.
Imported/static objects never become mutable constexpr locals by replay.

Expression-condition if/else, while/do/ordinary-for, break/continue preserve actual
loop-target identities, lexical activation owners and cleanup on normal exit,
return and transfer. Scalar and aggregate cells share cleanup; continue reaches
for increment or do condition as appropriate. Both branches receive semantic
checks. Dependency/effect walks use iterative worklists, including string/array/
subscript/pointer nodes; sizeof remains unevaluated.

## Executed coupled proof and boundaries

The array gate combines UTF16 copying, mutable elements, templates/reuse, frozen
overloads and reference helpers, loop/if/continue/break, retained macro/include
origins and post-disposal observations. Tests inspect element/alias/assignment
identity and final97/98/101/0. Reversed-subscript side effects prove290 versus
190 and preserve original source-left/source-right order. Readonly, expired and
one-past nonobject destinations fail without successful mutation. The flat2048-
term expression/12-step budget/reuse control verifies the repaired eager-walk
resource path. CF-R1-01..03 are closed by executed direct controls.

CFI-01 is evidence qualification: the finite direct-scalar unsequenced-conflict
check is a stricter managed adaptation justified for C++20, not native parity.
`evidence/sequencing-qualification.md` preserves correct-value native accepts and
historical expectation mismatches. Caller arguments and callee bodies retain
distinct sequencing. No provisional pointer rejection policy is inferred.

Condition declarations/selection initializers, switch/range-for/if-constexpr/
labels/goto, increments/shifts, general pointer/class stores and complete
sequencing/lifetime/template behavior remain required. Empty selection init is
recognized as valid Unsupported syntax. Floating numerical groundwork alone
does not implement floating parser/type/overload/evaluator support.
