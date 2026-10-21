# Independent control-flow main-integration baseline

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Starting from
accepted string checkpoint99643b48e and the reported raw-fixture provenance-only
follow-upf6d0e8dc8. No reviewer Git operation independently inspects those supplied
commit IDs. Main's accepted execution baseline is120/120 JIT/AOT and118 mapped
source/test/fixture/data/tool files. The isolated control-flow acceptance is
supporting evidence, not acceptance of this main merge.

Read `review-controlflow-isolated-round1.md` and the actual held handoff/source
under `/home/marius/.t3/worktrees/dotcc/t3-c8f7a1e2/edgcpp/manual-csharp`.
That module's102-group base,118 isolated groups,116-file maps and CF-R1-01/02/03
closure cannot be substituted for a combined main136-group proof. Source merge
is currently active; this baseline approves no unexecuted implementation.
The reviewer writes only review-controlflow-integration reports, performs no
source/test/tool edits, builds/tests, frontend/native execution or Git actions.

## Inspected contracts and finite main review gates

- Actual interpret.c30206ff separates nondereferenceable/runtime/expired/const
  store failures from C++20 uninitialized-local assignment. New local array
  elements must explicitly begin in the evaluation; imported global elements or
  static extended reference temporaries must not gain that origin from an active
  caller frame. Actual allocated type controls readonly; reference access cv
  does not mutate an object's flag. Unknown facts stay nullable.
- interpret.c27299ff schedules assignment RHS before destination evaluation;
  compound assignment must evaluate that destination once and load it afterward.
  The store result remains a location until explicit glvalue conversion. Array
  stores must use the same stable element cell seen by aliases and observations.
- Preserve all accepted Array/Pointer/PointerLocation/NonObjectLocation payloads,
  global constant-usability rules, original initializer replay, reference-return
  ref_indirect gate and actual pointer truth/persistent-result lifetime split.
  One-past reference transport must not create a fake cell; assignment/load or
  named/reference-call indirection must preserve their separate rejection gates.
- Structured transfers must run aggregate and scalar scope cleanup before the
  owning loop consumes break/continue. For-continue executes increment;
  do-continue executes condition. Return preserves its computed result while
  local/element/temp owners expire. Unselected branches remain semantically
  checked and empty infinite loops remain budgeted work.
- The isolated iterative dependency/effect helpers must explicitly traverse every
  main string/subscript/pointer node. Subscript uses original SourceLeft/Right;
  sizeof is unevaluated and dependent sizeof remains value-dependent. No fallback
  into recursive composite virtual getters may reintroduce a deep-graph stack
  failure ahead of the evaluator budget. Preserve the conservative extension
  contract for unknown graph kinds.
- Inspect sequencing beyond scalar BinaryExpression. Builtin subscript sequences
  E1 before E2 even in integer[pointer] spelling; a discriminating source can
  record index-call then base-call as12 and return12*16+'b'=290. Pointer+offset
  is a different operator: sibling caller-argument writes/read conflicts must not
  escape unsequenced accounting merely because the new node bypasses scalar
  ScheduleBinary. Exact native classification is required before alleging a
  specific rejected/accepted input. The first proposed `pass("abc",i=1)+i`
  negative expectation was disproved by native accept0 and is retained/excluded;
  it establishes no required rejection or finding. Actual call-boundary rules
  and matching direct-scalar controls must settle any subsequent allegation.
- Fixed nondependent arrays/pointers in template bodies must be real analyzed
  prototype/replay paths if claimed by the coupled gate. Dependent/compound
  declarator omissions remain explicit Unsupported. Preserve frozen definition
  lookup, canonical Unicode names, instance identity/reuse and owner rollback.
  Newly mutable graph links need journaling; parser-local loop state must unwind
  on failure and not leak between independent definition/instance parsers.

The upcoming coupled fixture must combine element stores/aliases, actual loops
and transfers, template replay/frozen overloads, Unicode/macros/includes,
retained provenance/disposal and forbidden readonly/global/expired/nonobject
writes. Produced assertions and matching main held Release/JIT/NativeAOT/native
maps/artifacts require independent review. A passing isolated module or merely
preserving the aggregate group count is insufficient.

Full M0–M6 remains active. Scalar-control limitations, broader resource/parser
qualification, general pointers/classes/templates/lifetimes and the separately
authored floating foundation remain required and unaccepted here.

## Sequencing evidence qualification discovered during integration

Correct-value native controls accept both the proposed pointer-call expression
and direct `i+(i=1)`. Earlier reject controls asserting a zero result therefore
do not independently prove native detection of unsequenced undefined behavior.
Those expectation mismatches remain retained/excluded rather than hidden.

The inspected C++20 draft makes an unsequenced same-scalar modification/read
undefined, and excludes such operations from core constant expressions. A
managed direct-scalar detector can be an explicitly documented stricter
adaptation; it is not observed native parity. Function-body execution has a
separate indeterminate-sequencing rule, while arguments are evaluated before
that body. Keep caller argument accesses distinct from callee invocation facts;
do not force blanket acceptance/rejection from one native result. Sources:
[N4861 intro.execution paragraphs10–11](https://timsong-cpp.github.io/cppwp/n4861/intro.execution#10),
[N4861 expr.const paragraph5.7](https://timsong-cpp.github.io/cppwp/n4861/expr.const#5.7).
No additional implementation finding is allocated from the provisional probe.
