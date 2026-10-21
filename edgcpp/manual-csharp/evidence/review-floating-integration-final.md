# Independent main floating integration acceptance

**ACCEPTED within the bounded binary32/binary64 frontend scope below.**
FLI-01, FLI-02 and FLI-03 are closed by actual source repairs, direct assertions
and matching final held execution. No unresolved critical/high finding is known
in these reviewed bytes. This is main integration acceptance, separate from
the isolated numerical foundation; it does not complete M1, M3, M4 or PLAN.

The reviewer inspected source, tests, fixtures, upstream branches and receipts;
ran read-only hash/JSON/inventory/diff audits only; and changed only the assigned
review reports. No build, test, native frontend, Git operation or implementation
edit was performed by the reviewer.

## Final evidence independently audited

- `floating-integrated-{build,tests,nativeaot-publish,nativeaot-tests}-round3.json`:
  all exit 0, actual main cwd, identical **175-file maps matching current disk**.
  Release reports zero warnings/errors. JIT and Linux-x64 NativeAOT each execute
  **164/164 groups, zero failures/skips**. The 164 unique emitted PASS names
  exactly match the 164 unique registrations and retain all 136 prior accepted
  names. The additions are 19 numerical and nine actual frontend groups.
- Maps explicitly include the three oracle/check tools and three Unicode data/
  provenance files. All earlier mapped files remain present. Earlier test
  changes were independently read against byte-verified accepted copies:
  integer/char16 assertions keep their meaning through explicit type casts;
  obsolete unsupported-floating expectations become real positive behavior.
- `floating-integrated-built-artifacts-round3.json`: independently matched size
  and SHA-256 of the actual library DLL, managed test DLL and published native
  executable (1,500,160; 372,224; 6,117,592 bytes). No earlier output is substituted.
- `floating-integrated-held-snapshot-round3.json`: actual writer-hold statement
  and current handoff fingerprints match. Integration handoff SHA is
  `838568e42834c3224754816804a31e40b33b767c6a9b8f5388dfe240b96d50c8`;
  accepted foundation handoff SHA is
  `36f43e26121a0401c06210654e82a14a25d8333be381304faa442d996529f234`.
  All **14 imported non-Program foundation files** match the accepted isolated
  manifest; no numerical foundation source was changed by the central repairs.
- `source-floating-integration.json`: all **nine** actual consulted upstream
  hashes match. Foundation source/configuration correspondence remains separate
  in its accepted handoff and `review-floating-isolated-round1.md`.

Read all ten `native-floating-integrated-*-round3.json` commands, exact source/
header hashes, statuses and diagnostic text. All match: **six accept 0, four
language reject 2**. Compiler SHA independently matches
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
Profile explicitly disables the predefined macro file and selects strict
C++20, no-codegen, instantiate-used and two-argument UTF-8 source handling.

| Exact final native input | Result / discriminating fact |
| --- | --- |
| gate + header | accept; macros, frozen helpers, two floating template instances and actual computed values |
| witness including unchanged gate/header | accept; float/double result selection is distinguished by overload witnesses |
| overload | accept; float-to-double promotion wins over integral conversion, and integer compound multiplication converts after floating operation |
| divide | accept with warning; default pinned IEEE outcome, not a negative |
| ieee | accept; nonfinite arithmetic, transport, unordered predicates and bool |
| runtime | accept; optional range warnings and variable-LHS zero warning; direct constant IEEE division and sizeof do not gain invented warnings |
| global | reject; ordinary const floating value is unavailable as a required constant |
| range | reject; floating value does not fit the required integral destination |
| underflow | reject; arithmetic finite nonzero result is lost to zero |
| conversion-underflow | reject; required double-to-float conversion loses a nonzero value to zero |

The old round-two divide expected-reject receipt is retained **excluded**.
Additional native contracts have exact retained copies matching their recorded
input hashes. Disproved expected-accept lL/overflow hypotheses and expected-
reject ordinary-runtime range hypotheses remain excluded. They are not counted
as matching negatives, successful skips or numerical parity. The independent
binary80 layout preview is native-only evidence, not managed long-double support.

## Canonical finding closure

| ID | Severity | Actual closure |
| --- | --- | --- |
| FLI-01 | High | Actual Lexer classifies no-dot decimal e/E and hex p/P UDL ambiguity as integer tokens when no exponent digits occur. Signed missing exponents/separators remain malformed. Whole unknown suffixes use Unicode validation; the pinned lL-invalid versus llL-UDL restoration split is explicit. Direct Lex, active invalid/unsupported and lazy PP assertions pass both runtimes; cooked native decimal/hex/llL controls discriminate literal-operator lookup. Full UDL semantics are not claimed. |
| FLI-02 | High | Immutable validated Ieee/Strict policy defaults to the witnessed IEEE target. Real Inf/NaN and flags can survive arithmetic, calls, references and array cells. Both modes reject finite overflow, nonfinite/out-of-range integer conversion and current operation/conversion underflow-to-zero, while nonzero tiny results succeed. Decimal literal zero-underflow is a source range error; hexadecimal zero-underflow remains an inexact warning success. Optional/runtime versus required contexts and sizeof are separately tested. Conditions/counters reset and current result flags, rather than sticky earlier observations, govern failure. |
| FLI-03 | High | Optional diagnostics use a capped parser-owned iterative memo. Each operation consumes already-completed child values through constant-size ephemeral numerical leaves; no growing-prefix interpreter replay or full effect/dependency query remains. Leaves never replace the original graph, and references/calls/stores/dependent forms are not followed. The 2048-term source, low-budget required failure, iterative queries and repeated recovery controls execute in JIT/AOT. |

The initial claim that direct constant/constant IEEE division should warn was
withdrawn after reading the actual empty-stderr receipt and `expr.c:31755`.
Final separate tests preserve that no-warning case and the nonconstant-LHS
warning case. No production repair is credited to that incorrect hypothesis.
The report history records this correction and the lL refinement explicitly.

## Produced integration reviewed

Canonical FloatingType identities retain target widths/precision and const
qualification, without CLR type/arithmetic authority. FloatingLiteralExpression,
runtime values, actual cells, results and assignment observations carry separate
immutable floating payloads; integer Value fields remain absent for floating
values. Floating-to-integer truncates before representability checking. Common
floating arithmetic precedes destination conversion, including integer compound
assignment. Numeric equality/bool use numerical predicates, not bit equality.

Overload matching keeps explicit per-argument conversion descriptors and pairwise
selection. Float-to-double is Promotion; other supported mixed changes are
Conversion. Exact matches, crossed ambiguity, repeated deduction conflict,
canonical instance reuse and definition-time candidate visibility are asserted.
Actual references preserve materialization/category and one shared object cell;
readonly comes from the allocated object, not the reference view. Unknown,
uninitialized, existing-global, expired and nonobject paths retain their guards.

The difficult source gate actually joins changed-stream macros/includes,
float/double deduction and cached-token replay, earlier const-reference helpers
despite a later overload/caller hiding, typed array/alias writes, loop work and
required values. Graph assertions inspect selected entities, conversions,
instances/replay counts, shared cell/assignment identity, payloads, flags,
cleanup and retained STEP/STORE/RET provenance after disposal. Reversed subscript
evaluation, failure recovery and repeated/concurrent sessions have direct controls.
The source follows general typed operations; no fixture spelling/value dispatch,
generated frontend or native production fallback was introduced.

Source branches reviewed include literals.c literal-kind/range/inexact handling;
lexical.c integer/floating suffix restoration; exprutil.c floating common types;
overload.c conversion matching; interpret.c typed casts, arithmetic and compound
assignment; float_pt.c IEEE nonfinite versus finite-range/zero-underflow paths;
folding.c exception/optional folding distinctions; cmd_line.c target IEEE default
and distinct Microsoft setting; expr.c evaluated-context zero-divisor warning.
Omitted dialect, mode and compound-language branches remain explicit.

## Bounds retained at acceptance

Optional warning folding is deliberately partial: closed numeric syntax only,
parser-local entries capped by ConstantEvaluationStepLimit; saturation abstains
from further optional folding. It is not complete native diagnostic parity.
Required evaluation still executes the original typed graph under its own
budgets. Strict is an explicit constant-evaluation alternative; it is not a
claim of a separately qualified full native dialect configuration.

The foundation's **FLT-02** root-authorized hexadecimal subnormal carry/reset
exception is distinct from integration **FLI-02**. Exact nearest payloads remain
correct rather than reproducing that proved pinned defect. Its native mismatches,
rational proof and unavailable native builtin-bit-cast proposal stay explicit;
managed signed-zero/NaN payload tests do not prove native bit-cast/payload parity.
The earlier finite scalar sequencing adaptation likewise keeps its separate
qualification; passing false assertions do not prove native UB detection.

Long double/binary80 integration, extended formats, alternate rounding/excess
precision, broader IEEE environment/NaN payload/builtin policy, complete warning
folding, UDLs, casts/initialization/declarators/classes/templates, ABI/layout,
real headers and broader product/platform/resource qualification remain required.
Full PLAN continues immediately beyond this accepted checkpoint.
