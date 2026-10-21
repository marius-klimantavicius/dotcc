# Floating integration round two — exception policy

Status: **changes required / final acceptance pending**. Combined round two
reported 164/164 JIT and NativeAOT on 170-file maps, but those receipts are
historical after the required FLI-02 policy repair. No numerical foundation
change is requested. Source editing and final matching execution remain with
the implementation worker and supervisor.

## FLI-01 source closure pending final rerun

Rechecked the exact final suffix repair and tests: no-dot decimal e/E and hex
p/P ambiguity selects integer UDL tokens; signed missing exponents/separators
remain invalid; all unknown whole integer suffixes use Unicode validation.
Whole builtin-shaped mixed l/L remains invalid, while llL requires unsupported
literal-operator lookup. Direct Lex and active analysis assertions cover both.
The `1lL` proposed native positive was disproved and remains excluded; actual
`1llL` cooked-ULL positive is accepted. No production UDL support is inferred.

## FLI-02 — numerical flags are not the pinned C++ exception policy

Severity: high integration correctness; open until actual policy repair and
matching final execution. The held CheckFloating unconditionally rejects
DivideByZero and InvalidOperation, and accepts every Underflow flag. This
differs in both directions from the actual pinned target policy.

Read exact native `native-floating-integrated-divide-round2.json` and source
fixture: `constexpr double divide(double x){return x/0.0;}` used in a positive
comparison accepts 0 with division-by-zero warning. Its old expected-reject
receipt is **excluded**, not a passing negative. This is not the separately
root-authorized FLT-02 hexadecimal carry defect.

Actual upstream branches read:

- `cmd_line.c:13656` initializes IEEE_handling_on_float_operation_exceptions
  from TARG_HAS_IEEE_FLOATING_POINT; `2637` has an explicit Microsoft-mode false
  branch. The runtime profile and target mode must not be inferred just from
  the word strict on the command line.
- `folding.c:4434` guards the early zero-divisor failure with the IEEE handling
  flag. `interpret.c:29003` calls fp_divide and fails only on returned err.
- `float_pt.c:3993–4030` permits NaN/infinity with IEEE target support and marks
  mode dependence. `store_host_fp_value:1338` dispatches target conversions;
  `conv_host_fp_to_float:759`, especially `887–915`, excludes existing
  nonfinite values from finite-range tests but reports finite nonzero values
  rounded to zero. Corresponding double conversion follows the same target
  range distinction. These are actual source branches, not CLR authority.

Supervisor separately confirmed exact native controls:

| Control | Actual pinned result |
| --- | --- |
| zero/zero and infinity-minus-infinity comparisons | accept 0 |
| NaN/infinity propagation, comparisons and bool | accept 0 |
| constant/constant ordinary runtime global division by zero | accept 0, no warning |
| nonconstant left operand divided by known zero | accept 0, warning |
| finite floating arithmetic overflow | reject 2; original expected-accept hypothesis excluded |
| nonfinite floating-to-integer required conversion | reject 2, matched |
| minimum float subnormal divided by two, rounded to zero | reject 2, matched |
| three minimum float subnormal units divided by two, rounded to two units | accept 0, matched |
| double 2^-150 converted to float zero | reject 2, matched |

The underflow pairs are necessary: numerical Underflow can accompany a nonzero
inexact subnormal, so rejecting every Underflow bit would also be wrong.
Successful direct hex-literal underflow to zero was witnessed earlier and
uses a separate literal conversion path; do not infer arithmetic acceptance
from that literal. The baseline's initial generic underflow wording is
superseded by this context/result-aware finding.

Root explicitly authorized the finite repair: immutable selectable exception
policy, pinned IEEE behavior by default, explicit strict alternative; preserve
all flags and typed nonfinite values without allowing finite overflow or
invalid floating-to-integer conversion. Review must inspect actual handling of
rounded-zero underflow in operation/conversion results, separate literal
handling, numeric equality/bool, shared cells, cleanup and reusable-evaluation
state. Require paired managed controls and final held JIT/AOT/native evidence.
No broad IEEE environment, alternate rounding mode or NaN payload parity is
implied. Long double and the full campaign remain required.

## Further context controls and correction

Supervisor's exact native decimal literal controls reject both 1e-46f and
1e-1000f as out-of-range literal values, while 1e-45f is accepted nonzero.
Hexadecimal literal underflow to zero is the separately accepted inexact
warning case. Decoder outcomes remain numerical; the parser must apply this
decimal-versus-hexadecimal language-policy distinction without changing the
accepted numerical module.

Ordinary runtime constant arithmetic underflow-to-zero/overflow accepts with
warnings, while the same operations fail required evaluation. Their initial
expected-reject native hypotheses remain excluded, not matched negatives.
Unevaluated sizeof suppresses these value diagnostics. Diagnostic folding
must not turn an optional numeric result into a core constant-expression fact.

The review initially misread the runtime division evidence and alleged that
direct constant/constant division should warn. **That allegation is withdrawn**:
actual `native-floating-contract-ieee-runtime-round1.json` has empty stderr;
`warning-variable-divide` warns only with a nonconstant LHS. Read actual
`expr.c:31755`'s `!is_constant_operand(operand_1)` guard. Preserve both direct
no-warning and nonconstant-LHS warning assertions; do not force an incorrect
warning from the earlier assumption. This is not a production finding.

## FLI-03 — repeated optional folding replays growing expression prefixes

Severity: high resource correctness; open on the produced optional-folding
implementation. Every FloatingBinary called DiagnoseFloatingOperation, which
ran full HasSideEffects and IsValueDependent walks on both operands, executed
fresh interpreters for both, then reinterpreted the whole expression. A flat
pure floating sum of n literals performs quadratic parse work. Each
interpreter's step limit cannot bound preceding whole-prefix queries or the
aggregate per-node replays. Iterative walks avoid stack exhaustion but do not
make this total work linear. No hostile unbounded repro was executed.

Acceptance requires incremental typed immutable fold facts, or a genuinely
fixed local inspection budget before any full query/evaluation with its
warning scope explicit. Preserve current-operation flags/result versus sticky
child conditions, no speculative calls/stores, no sizeof evaluation, no AST
retention in session/global caches, and actual required evaluation of the
original graph. Add a meaningful controlled flat-source regression. The
supervisor and author received this finding promptly; a repair is underway.

## Subsequent produced repair inspection, before execution

FLI-02 is source-repaired: validated immutable Ieee/Strict configuration;
default IEEE arithmetic may carry infinity/NaN and numerical flags, while
finite overflow and nonfinite/out-of-range integer conversion still fail.
CheckedFloating rejects only the **current result's** Underflow-to-zero;
nonzero inexact subnormal results remain successful. Same-format transport and
an earlier literal's sticky conditions cannot cause a later exact operation
to fail. The parser separately rejects decimal literal zero-underflow and
retains accepted hex zero-underflow/inexact warnings. Actual evaluator roots
still reset conditions/counters and clear per-evaluation state.

Read new FloatingPolicyTests and IEEE/runtime/underflow fixtures: both modes,
strict failures, default typed nonfinite values transported through actual
reference/array cells, NaN numeric comparisons/bool, mandatory integer failure,
ordinary runtime warning versus required failure, direct constant/constant
division with no warning, variable-LHS zero warning, sizeof suppression,
decimal/hex and zero/nonzero distinctions, lazy discarded PP input, reusable
failure/success and concurrent mode isolation. Payload expectations are the
managed deterministic policy, not native NaN sign/payload parity.

FLI-03 is source-repaired: the optional numeric memo uses an explicit iterative
stack, parser-local bounded entries and saturation. It follows only closed
numeric syntax. Each current operation uses already-computed child values in
a constant-size private NumericalFoldLeaf operation; it neither rewalks growing
prefixes nor substitutes those leaves into the original graph. References,
calls, stores and dependent forms are not speculatively followed. Facts retain
numeric values/status only; no session/global AST cache is introduced. The
optional warning budget/scope is partial and must be documented, rather than
claimed as complete native folding/diagnostic parity.

The controlled 2048-term source regression uses a low budget, verifies ordinary
source acceptance after optional memo saturation, actual required ResourceLimit,
iterative graph queries and repeated failure/success cleanup. Original required
evaluation still traverses the source graph under its own work budget. No huge
unguarded hostile repro was executed. All three canonical integration findings
remain pending final held execution/evidence closure; no further high source
defect is currently identified in this repaired path.
