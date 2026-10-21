# Reference constexpr produced-code review, round 1

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Source is still
being implemented; these observations do not accept an unexecuted snapshot.
Only this review report was written. No builds/tests/native/Git were run.

The new ConstantStorage/ConstantEvaluation source was read directly. It now has
separate integer/address runtime values, scalar object cells, explicit loads,
frame-owned automatic cells, full-expression cleanup and immutable public
snapshots. Argument evaluation precedes StartCall parameter storage, preserving
caller identity. Return expressions are evaluated before return cleanup;
callee ownership does not include cells merely passed by reference. The new
discard path executes reference-returning calls while avoiding a bare object
designator's load. These are source observations pending complete integration,
actual assertions and serialized execution.

## RCE-01 — declaration-time global-reference eligibility (provisional high)

`ConstantStorage.ResolveReference` currently schedules every TU reference's
initializer again, regardless of its declaration-time ConstantEvaluation.
Replaying after the graph gains a later function definition may convert a
nonconstant initializer into a usable constant reference retroactively.

Exact native discriminator requested from the supervisor:

```cpp
const int g=3;
constexpr const int& later();
const int& r=later();
constexpr const int& later(){return g;}
static_assert(r==3);
```

The source path can now interpret `later` successfully at the assertion; the
scalar-object path already requires successful declaration-time classification.
This finding remains provisional until the pinned native result establishes
the reference eligibility rule. If rejected natively, acceptance requires a
declaration-time eligibility gate, a negative exact source regression, and a
positive control where the referenced constexpr body is defined before the
reference initializer. Eligible replay must still create fresh evaluation-owned
storage and preserve identity without importing mutable cells from old results.

## Native contracts inspected

Eight retained `native-reference-constexpr-*-contract-round1.json` receipts were
read, including stdout/stderr: address/discard, argument temporary, local
optional-evaluation and extended global temporary accept0; mutable read,
persistent call-result temporary, escaped local temporary and direct return
temporary reject with language status2. The expired controls explicitly
diagnose expired storage. These establish oracle contracts only; they do not
prove the managed implementation or its graph facts.

Further produced-code review and held JIT/NativeAOT/native evidence are required.
Unknown-object/P2280, complete lifetime cleanup, void/class/pointer evaluation,
assignment/control flow and dependent-reference templates remain unqualified.

## RCE-01 disposition — disproved by the pinned native oracle

The exact ordinary-const-reference late-body source **accepts with status0** in
`native-reference-constexpr-declaration-time-late-body-contract-round1.json`.
The assumed rejection is retained as a mismatched, excluded receipt. The
available-body positive control also accepts. Consequently RCE-01 is closed as
a disproved review hypothesis, not a required production repair. A blanket
declaration-time eligibility gate would incorrectly reject the observed native
behavior. Current ordinary-reference replay is consistent with this control.

A `constexpr` reference declaration's required initializer remains a separate
gate; no conclusion about that distinct case follows from this ordinary
reference control. Final reports must preserve this distinction and must not
count the disproved expectation as a native negative pass.
