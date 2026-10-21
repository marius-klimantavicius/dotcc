# Floating frontend integration handoff

Status: actual central integration in progress against accepted main
`764d67a16`. No implementation-agent build, test, native execution or Git
operation. The isolated floating foundation remains independently owned and
must be imported only after supervisor qualification and a held snapshot.

The integration follows EDG `literals.c:conv_float_literal` (915–1140),
`exprutil.c:promoted_float_kind/determine_arithmetic_conversions_full`
(11738–11898), `overload.c:determine_arg_match_level` (3013 onward),
`interpret.c` typed conversions (27360 onward) and arithmetic (28825 onward).
Existing Apache-2.0 WITH LLVM-exception attribution applies. This is authored
semantic code, not source translation or a native execution fallback.

## Interface and ownership

- Canonical `FloatingType` identities for target binary32 `float` and binary64
  `double`, including const variants. Long double remains a required separate
  format/type task; it is never silently mapped to double.
- Distinct `FloatingLiteralExpression` and immutable public payload snapshots.
  Bits/classification/sign are inspectable without an invented integer value.
- Standard conversions retain explicit glvalue loads and target conversion
  nodes. Float-to-double has Promotion rank; other mixed arithmetic numeric
  conversions have Conversion rank. Candidate comparison remains pairwise.
- One `ConstantCell` identity model for integer and floating objects/elements.
  Its semantic type and separate floating payload preserve the accepted integer
  Value API and all readonly/lifetime/origin/sequencing behavior. Runtime and
  result values gain separate floating variants, never bit-pattern integers.
- Typed load/store/materialization/argument/return work preserves caller-first
  argument evaluation, reference transport, cleanup, unknown objects, forbidden
  existing-object loads, aggregate cells and nonobject one-past markers.
- Scalar assignments retain RHS-before-LHS, single compound LHS evaluation,
  common operation type and actual-cell unsequenced access tracking. Integer-
  only operators continue to require integer operands.
- Foundation range/resource/invalid/divide-by-zero flags are numerical outcomes;
  frontend diagnostics/required-constant policy are explicit. Inexact alone is
  successful. Ordinary const floating variables do not acquire the existing
  potentially-constant const-integral import privilege.

All accepted 136 groups and current source provenance, Unicode, replay,
array/pointer/reference, scoped-type ownership and owner rollback contracts must
remain intact. New tests will include a changed-stream macro/template/frozen
overload/reference/store/loop/required-constant gate, numerical failures,
retained graph/storage facts, resource recovery and isolation. Full PLAN and
remaining target/literal/object/template language branches remain required.

## Authored integration checkpoint

The supervisor imported the accepted held foundation without replacing central
files: `FloatingValues.cs`, `FloatingLiterals.cs`, `FloatingValueTests.cs`, its
eleven fixtures and separate foundation handoff. This integration applies only
the nineteen additive foundation registrations, alongside nine authored frontend
groups and the accepted136: expected164 groups. No execution is claimed here.

New central modules are `FloatingSemantics.cs` and `ConstantFloating.cs`; new
test modules are `FloatingIntegrationTests.cs` and `FloatingCombinedTests.cs`.
Additive hooks are in Types, CompilationSession, Tokens, Parser, Expressions,
SemanticGraph, OverloadResolution, References, StringLiterals, ConstantStorage,
ConstantArrays, ConstantEvaluation and ScalarControlFlow. Existing tests change
only typed integer casts after generalized public scalar types and obsolete
unsupported-float expectations. All remaining unsupported expectations remain.

`ConstantCell.Type`, storage snapshot Type, reference binding SourceType and
TargetType, and compound assignment OperationType are now SemanticType. The
integer fields remain integer-only; floating cells/results/assignment
observations expose a separate FloatingValue. Assignment observations provide
nullable IntegerValue; the existing integer Value accessor is preserved and
throws on a floating observation instead of inventing an integer. Actual cell
identity, lifetime owners, unknown-object null facts, NoteAccess sequencing and
cleanup remain shared. Numeric conditions are retained in literal/evaluation
snapshots. `FloatingIntermediateBitLimit` is immutable session configuration,
default32768, allowed64–65536; token/digit/exponent foundation limits also apply.

The frontend fixes FLI-01 at the actual number-token boundary: no-dot e/E or p/P
without exponent digits denotes a potential integer user-defined suffix, while
signed missing exponents and leading exponent separators remain malformed.
Unknown integer suffixes share Unicode identifier validation; malformed
underscore suffixes are not silently accepted as unsupported. Full literal-
operator lookup remains required. Active number conversion remains after PP
rescan; inactive, unused and dropped numeric arguments are not evaluated.

Exact new fixtures are `m4-floating-frontend-gate.h/.cpp`, its separate witness,
and overload/divide/global/range controls. The combined source retains macro
STEP/STORE/RET provenance, two canonical Float/Double template instances and
reuse, earlier const-reference echo candidates despite a later void echo and
caller-local hiding, real floating array/reference cell stores and loop work,
and required computed values. Tests retain payload/cell/origin queries after
session disposal and prove independent/repeated analyses and evaluator recovery.

The initial required-constant policy was corrected after actual pinned native
evidence (FLI-02). `AnalysisOptions.FloatingExceptions` is an immutable validated
Ieee/Strict enum, default Ieee for the witnessed target/profile. IEEE arithmetic
preserves infinity/NaN results and divide-by-zero/invalid flags; Strict rejects
those exceptional operations during required evaluation. Both modes reject
overflow, resource exhaustion, out-of-range/nonfinite integer conversion and
current arithmetic/conversion underflow that loses a nonzero result to zero.
Tiny inexact nonzero results remain accepted. Literal conversion is separate:
hexadecimal zero underflow remains accepted with its warning, while decimal
nonzero literal underflow to zero is FLT002, including inside sizeof. Inexact
alone succeeds. Floating-to-bool is a numerical zero test, including real NaN
and infinity values. Ordinary const floating globals do not become constant-readable through
optional initializer results; constexpr globals and evaluation-local storage
use their original typed recipes and actual storage. Integer-only bitwise,
remainder and unsupported shift/cast/declarator forms retain their real bounds.

The foundation's root-authorized FLT-02 narrow exception for the demonstrated
pinned hexadecimal subnormal carry/exponent-reset defect is inherited exactly:
correct target nearest values are preserved, not rewritten to reproduce that
defect. Native mismatches/exclusions and unavailable bit-cast remain separate
foundation evidence, not frontend passes. Long double80, extended formats,
rounding/excess-precision modes, builtin/nonfinite construction, full UDLs,
general dependent references/arrays, initialization and remaining language/ABI
branches remain required. No wider parity or full M1–M6 completion is claimed.

## FLI-02 / FLI-03 finite follow-up

The former divide fixture is now a positive under default IEEE policy and a
negative under explicit Strict constant evaluation. New exact fixtures are
`m4-floating-frontend-ieee.cpp`, `-runtime.cpp`, `-underflow.cpp` and
`-conversion-underflow.cpp`. `FloatingPolicyTests.cs` extends the existing
frontend failure group; the registration count stays164. It checks actual
Inf/NaN payloads through cells/references/arrays/calls, unordered comparison and
bool, both modes, nonfinite integer conversion, decimal/hex literal timing,
arithmetic/format-conversion loss to zero versus nonzero tiny outcomes, runtime
warnings, no warning for constant/constant IEEE division or sizeof operations,
resource/failure recovery and concurrent policy isolation.

Source inspected for this repair: `cmd_line.c:13656` initializes IEEE handling
from TARG_HAS_IEEE_FLOATING_POINT;2637 sets a distinct Microsoft policy (Microsoft
language support is not claimed); `float_pt.c:3993–4030` returns real Inf/NaN and
mode dependence;759 onward/907–915 distinguish finite nonzero lost-to-zero
conversion;2859–3008 decimal conversion differs from hexadecimal conversion;
`folding.c:4434` handles non-IEEE division separately; `expr.c:31755–31766`
warns known-zero RHS with nonconstant LHS,37850 warns compound division, both
outside unevaluated contexts. Actual supervisor native outcomes refine source
severity: ordinary constant arithmetic under/overflow warns while required
evaluation fails. Constant/constant IEEE division has no invented warning.

The first optional-warning implementation reinterpreted growing expression
prefixes and was replaced before its acceptance (FLI-03). Closed immutable
numeric facts now use a parser-owned capped memo and iterative worklist. Each
new operation consumes already-completed child values through ephemeral
`NumericalFoldLeaf` nodes and the existing typed interpreter. Leaves never enter
the returned graph; no reference/call/store is followed, body replay is never
requested, and no whole-prefix effects/dependency query occurs. Memo entries
are bounded by the evaluation step policy; saturation disables optional
folding, not valid source. The cache has parser/TU lifetime and contains no
mutable binding/storage fact, so it needs no owner journal or session cache.
A controlled2048-term source verifies runtime parsing, required ResourceLimit,
stack-safe graph queries and repeated same-interpreter recovery. Actual required
execution still traverses the original graph under its own budget.

No numerical foundation source changed. This follow-up requires a fresh held
supervisor execution/review; the earlier164-group passes predate these policy
and resource repairs and are not acceptance of the new snapshot.
