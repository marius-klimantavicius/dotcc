# Independent final control-flow main-integration review

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. **Accept the bounded
main scalar assignment/control-flow plus array/reference integration represented
by the held round1 bytes.** No unresolved concrete high blocker remains within
the inspected and executed scope. This is main integration acceptance, distinct
from the isolated118-group acceptance, and does not complete the full PLAN.

The reviewer independently read actual EDG branches, isolated handoff/review,
produced main code, tests, fixtures and executed receipts. Only reviewer-owned
reports were written. No implementation/test/tool edits, builds/tests,
frontend/native execution or Git operations were performed by this reviewer.

## Final evidence audit

- Four `controlflow-integrated-{build,tests,nativeaot-publish,nativeaot-tests}-round1.json`
  receipts exit0 in the actual main checkout. Their **identical145-file maps
  match current bytes**, including Unicode data3 and tools3. Release has zero
  warnings/errors; direct-DLL JIT and Linux-x64 NativeAOT each execute
  **136/136 groups, zero failures/skips**. This preserves the accepted120-group
  baseline and adds the16 isolated scalar-control groups with the actual new
  array helper inside ControlCombined.
- `controlflow-integrated-held-snapshot.json` matches those maps/hold and
  `controlflow-integrated-built-artifacts.json`. All three actual output sizes
  and SHA256 values were independently checked against the managed library,
  test DLL and NativeAOT executable.
- Five hashes in `source-controlflow.json` and the retained nine hashes in
  `source-strings.json` match actual upstream files. Their recorded branches do
  not imply whole-file port coverage.
- All23 `native-controlflow-integrated-*-round1.json` receipts match exact current
  source/additional-input/compiler hashes and required profile: **9 accept0,
  14 language reject2**. No unexpected native status or skipped case is counted.
  Commands use strict C++20, used instantiation, disabled predefined macro file,
  no code generation and explicit two-argument UTF8 source kind. Compiler SHA256
  is `e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
  The native array witness includes the unchanged new gate/header; the original
  scalar witness retains its unchanged positive/combined inputs.

## Main merge and actual difficult-path proof

Main central files were merged additively. Current Unicode identifiers,
source/encoding/provenance, ErrorCount/replay/rollback, string decoder/type cache,
array/pointer/pointer-slot payloads, nonobject addresses, reference indirection
and global constant-usability rules survive in produced code and the136-group
run. The scalar module adds real assignment/control AST/work and observations;
it does not skip bodies or replace execution with optional folds.

Actual array elements remain the same ConstantCells used by references, loads,
stores and immutable observations. New local array cells explicitly begin in
the evaluation; imported globals/literals/static reference temporaries do not
gain that fact from an active caller frame. Allocated cv controls readonly,
access-view const does not change an object's flag, and unknown readonly/origin
facts stay nullable. Assignment checks known/liveness/readonly/origin and
operation-width rules before mutation. Simple assignment can initialize an
uninitialized C++20 local; compound assignment loads the single evaluated target.

Scope cleanup preserves both scalar and aggregate owners across normal exits,
break/continue/return. For continuation runs increment; do continuation runs its
condition. Frame exit removes aggregate activations, and finally clears every
new observation/access/aggregate collection. Reference aliases do not extend
array lifetime. Nonobject locations never become fake elements: direct transport
remains distinct from assignment/load/named-reference/reference-call indirection.

The new shared gate/header executes UTF16 string copying and mutable element
stores inside real int/long template instances, while/if/continue/break,
reference-returning early helpers, frozen two-candidate lookup despite a later
better void helper and caller hiding, macro/source origins and retained queries
after disposal. Tests inspect final97/98/101/0, stored97/101, identical element/
reference/assignment snapshots, actual local origin/readonly/expiration,
one replay per reused instance and selected const-character-reference conversion.
The native witness discriminates the literal const-array type and the later
ordinary mutable-helper void result. Negative complements separately reject
readonly literal assignment, expired array storage and a one-past nonobject
store, with no successful mutation recorded for the latter failures.

The reversed subscript destination has real side effects in both original
operands. Its executed290, counter writes1 then2 and retained second-pointer
flag distinguish correct E1-before-E2 from reversed190. Assignment RHS precedes
its single destination evaluation. Public dependency/effect walks explicitly
handle all string/array/subscript/pointer nodes with iterative worklists;
sizeof remains unevaluated. The flat2048-term index/12-step resource/reuse test
executes through failure, successful sizeof and repeated failure without graph
recursion ahead of the budget. General arbitrary nesting/resource qualification
is not inferred from this bounded proof.

## Finding and evidence qualification closure

The isolated CF-R1-01/02/03 responses are preserved and actually executed in
main: allocation origin is explicit, empty selection initialization is recognized
as valid Unsupported syntax, and flat scalar dependency/effect/assignment work
is stack-safe within the tested bounds. No new unjournaled shared graph mutation
was identified; inferred array-type links retain their existing owner journal,
loop targets are immutable graph identities and parser-local loop state unwinds
in finally/separate replay parsers.

**CFI-01 is closed as a claim/evidence qualification.**
`evidence/sequencing-qualification.md` accurately records correct-value native
accepts for direct scalar, scalar-call and provisional pointer-call expressions.
Old false-assertion rejects do not prove native undefined-behavior detection;
historical mismatched expectations stay retained/excluded. The implemented
finite direct-scalar conflict check is a documented stricter adaptation, not
native accept/reject parity. No forced pointer-negative policy was introduced.
The C++20 basis is [N4861 intro.execution10–11](https://timsong-cpp.github.io/cppwp/n4861/intro.execution#10)
and [expr.const5.7](https://timsong-cpp.github.io/cppwp/n4861/expr.const#5.7).
Caller argument effects remain distinct from callee-body invocation sequencing.

Acceptance is limited to integer simple/implemented compound assignments,
expression-condition if/while/do/ordinary-for/break/continue and the actual
array/reference integration above. Selection/condition declarations, if-constexpr,
switch/goto/labels/range-for, increments, shifts, general pointer/class stores,
full sequencing/lifetime/template behavior and all other open M0–M6 requirements
remain required. Separately authored floating groundwork is not production
support. Continue immediately through reviewed additive integration and the
remaining full plan; this checkpoint is not a stopping boundary.
