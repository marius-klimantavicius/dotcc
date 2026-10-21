# Control-flow main produced-source review, round 1

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Read actual held
main code, integrated handoff, isolated source/reference review and authored
combined assertions. No implementation/test/tool edits, builds/tests,
frontend/native execution or Git operations were performed. NativeAOT/native/
artifact matching is still pending here; this is not final acceptance.

The supervisor's first main Release and JIT receipts report zero warnings/errors
and136/136 groups with zero failures/skips. This is the correct additive120+16
registration count; the new array helper runs inside ControlCombined. Isolated
118-group results have not been relabeled as main results.

## Actual integrated code and assertions inspected

- ConstantRuntimeValue preserves Array, Pointer, PointerLocation and
  NonObjectLocation. Result snapshots retain Arrays, both real/nonobject
  reference observation variants and new assignment/control observations.
  Location still rejects a nonobject store/load; reference-return call exposure
  retains the accepted ref_indirect check. New dispatch/fallback adds control
  work without replacing aggregate work handlers.
- Local/value-parameter allocation explicitly sets BeganInEvaluation=true.
  MakeArray passes !isStatic for each existing stable element cell; global and
  literal imports remain false. Materialization passes !isStatic independently
  of active frame. Unknown public origin/readonly facts stay null. Assignment
  checks known/alive/actual readonly/origin before mutation. A const reference
  does not reclassify a mutable destination object.
- LeaveScope calls LeaveAggregateScope, removes scalar locals and expires all
  lifetime-owner cells. For-scope cleanup is scheduled outside its loop work;
  break/continue/return skip executable statements while cleanup work remains.
  Frame exit removes aggregate activations. Finally clears scalar-control and
  aggregate/literal/nonobject state. No new shared mutable parse-state graph
  link was identified that bypasses the existing owner journal.
- Iterative helpers explicitly handle every added string/subscript/pointer
  expression kind; new subscript/pointer dependency getters use that helper.
  SizeOf ignores its operand and DependentSizeOf remains dependent. Current
  known-node walks do not invoke recursive composite fallback. Unknown effects
  remain conservative. Existing reference/materialization children are handled
  explicitly when encountered in those walks.
- Template-body arrays are admitted only when their element/bound is known.
  Dependent bounds are detected before evaluation; dependent element forms
  stay Unsupported. The prototype is analyzed and concrete bodies rescan normal
  array/string/assignment syntax; there is no placeholder or fixture fold.

The actual m4-controlflow-array-gate.cpp/.h fixture contains local UTF16 arrays
inside int/long template instances, mixed string parts, compound element stores,
reference-returning early const helpers, while/if/continue/break and later better
void helper/caller hiding. Tests inspect two instances with one body replay
each, frozen two-candidate sets, final cells97/98/101/0, stored values97/101,
same cell/reference/store snapshots, true local lifetime origin, false readonly,
expired local cells and retained STORE/DATA/RET macro origins after disposal.
The negative complements use real readonly-literal diagnosis, expired local
array store and mutable one-past nonobject store with zero successful writes.

The reversed destination `(counter=1)[cells+(counter=2)-1]='Z'` retains both
source operands and their effects. Its expected290 and counter stores1,2
discriminate E1-before-E2 against reversed190. The unevaluated sizeof control
records zero writes. A flat2048-term subscript index exercises iterative public
queries and a12-step resource stop, then reuses the interpreter across failure,
successful sizeof and repeated failure, with expired local storage snapshots.

## CFI-01 — medium evidence/claim qualification, no forced semantic repair

Correct-value native probes accept direct `i+(i=1)`, `id(i=1)+i` and the
provisional pointer-return-call expression. Earlier isolated controls asserting
zero could reject because of their false assertion and therefore do not prove
native detection of unsequenced undefined behavior. That evidence limitation
must remain visible; no native-parity claim for the managed runtime checker is
justified. Historical expectation mismatches are retained/excluded.

The inspected C++20 draft defines direct same-scalar unsequenced modification/
read as undefined, and excludes such an operation from a core constant
expression. The bounded managed scalar detector is therefore defensible as a
documented stricter adaptation. Caller argument effects and callee-body
indeterminate sequencing remain distinct; no blanket native accept/reject rule
or provisional pointer-negative requirement is inferred. Sources:
[N4861 intro.execution10–11](https://timsong-cpp.github.io/cppwp/n4861/intro.execution#10),
[N4861 expr.const5.7](https://timsong-cpp.github.io/cppwp/n4861/expr.const#5.7).

The supervisor accepted this explicit qualification and is recording the exact
controls/policy. CFI-01 requires honest final support/correspondence wording and
source-hashed receipts; it identifies no currently confirmed code blocker.
The isolated CF-R1-01/02/03 repairs remain independently accepted there and are
preserved by the inspected main merge; their main execution must still be
audited rather than assumed from the old checkout.

No additional concrete high blocker is currently known in the held merge.
Final acceptance still requires identical current main build/JIT/AOT maps,
actual artifacts/source hashes and exact native coupled fixture/type/negative
witnesses. All other unsupported/full-plan requirements remain active.
