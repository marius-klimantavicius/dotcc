# Produced parser/expression review, round 1 — 2026-10-20

Status: **changes required; no milestone approved**. Reviewed actual
`Parser.cs`, `Expressions.cs`, updated `ParseState.cs`, initial test harness and
five authored M2 fixtures. No managed build/test or NativeAOT result existed at
this review point. Reviewer executed no build/test/native processes.

The implementation interleaves declaration entry, lexical lookup, expression
parsing, conversion construction and graph construction. Its fixed LP64 integer
literal candidate arrays preserve radix/suffix ordering, and its arithmetic
conversions preserve rank/signedness distinctions. It withholds signed-overflow,
division-by-zero and minimum/-1 undefined folds, and wraps unsigned arithmetic.
These are substantive produced algorithms rather than fixture answer branches.

## R3 — High — scalar call prvalues retain return-type const

Affected code: initial `SemanticGraph.cs:148–150` and
`Expressions.cs:134–154`.

Reproduction: `const int f(); int g(){return f();}`. CallExpression takes the
declared FunctionType.ReturnType directly, exposing a const-int PRValue.
Convert's same-integer-kind fast path then returns that already-qualified
PRValue unchanged. In C++ the declared function return type retains const, but
the scalar call expression has unqualified int type.

EDG correspondence: `declarator.c:report_bad_return_type_qualifier` (~743 onward)
retains the declared qualifier; `exprutil.c:func_call_expr` (~19516) obtains
`il_return_type_of`; `il.c:prvalue_type` (14807), `return_type_of` (14835), and
`il_return_type_of` (14857) drop scalar prvalue cv separately from declaration
type preservation.

Acceptance: preserve const on declared ReturnType; expose canonical unqualified
int on the call and returned expression; preserve const on the definition's
own parameter object while the signature parameter is unqualified. Add a graph
regression and native decltype witness. **Open; sent promptly to supervisor.**

## R4 — High — main is silently accepted with ordinary-function semantics

Affected code: initial `Parser.FunctionDeclaration` and expression name lookup;
no main-specific semantic branch exists.

Reproductions accepted by code inspection: `void main(){}`;
`bool main(){return true;}`; `int main(){return main();}`. Strict C++20 forbids
these return types/use of main. EDG `decls.c:check_main_function` (17699 onward)
and recognition in `function_declaration` (~18288) are explicit semantic work.

Acceptance: implement the bounded main constraints/reference rules with exact
EDG correspondence, or explicitly diagnose global main as unsupported until
those branches are ported. Silent success is not an acceptable scope exclusion.
Native negative receipts and managed regressions must cover all three forms.
**Open; sent promptly to supervisor.**

## R5 — High — valid unsupported conversions/operators are labeled invalid

Affected code: initial `Expressions.ScanOperand`, `Convert`, `Binary` and
`Parser.IsUnsupportedExpressionContinuation`.

Valid strict C++20 reproductions currently reach InvalidSource diagnostics:

- `int f(); bool g(){return f;}` requires function-to-pointer and bool conversion.
- `int f(); bool g(){return !f;}` requires function-designator transformation.
- `int g(){return 1 bitand 2;}` and `int g(){return 1 and 2;}` use alternative
  operator tokens currently left as Keyword and missed by continuation checks.
- Three-way comparison token handling has the same missing-continuation risk;
  its result/type machinery is unimplemented and must remain explicit.

EDG `exprutil.c:do_operand_transformations` (~24924 onward) and function/pointer
helpers (~23474, 23768 onward) handle function transformation separately from
ordinary integer conversion. Keyword operator spellings denote ordinary C++
operators even if this implementation chooses to exclude them initially.

Acceptance: implement the forms or classify them UnsupportedFeature. Preserve
InvalidSource for impossible supported conversions such as void-returning call
to int. Graphs from unsupported forms must remain diagnosed and unsuccessful.
Review numeric user-defined suffix treatment separately: current
`IntegerLiteral` mixes malformed/unsupported suffixes into LEX010; no claim of
user-defined literal support is justified. **Open; sent promptly to supervisor.**

## Prior findings revisited in produced code

- **R1 code-level scope resolution confirmed:** ScanExprFull and expression
  semicolon checking explicitly diagnose assignment continuations unsupported.
  The authored `(x=1)=2` test checks ContainsUnsupportedFeatures. BinaryExpression
  therefore does not represent accepted assignment in this slice. Executed
  regression remains pending before closure at acceptance.
- **R2 fix inspected:** ParseState now tracks actual checkpoint object identity
  in an ancestry list, removes descendants on rollback, validates diagnostic
  counts before mutation and rejects stale entries regardless of undo count.
  The harness includes equal-count scope branches and diagnostic-only stale
  checkpoints plus nested/repeated/cross-state controls. This addresses the
  reported mechanism. Execution remains pending.

The supervisor's suggestion that `int x; int x;` should be accepted was rejected
by independent review: C++ namespace variables do not use C tentative-definition
rules. The current rejection is appropriate for that exact strict C++20 input.

## Remaining review/validation obligations

The initial harness has meaningful tests for identity, explicit conversions,
source-order lookup, parameter-body scope, replay, stale checkpoint atomicity,
concurrent independent sessions, collection mutation rejection, arithmetic
rank/width, malformed recovery and unsupported constructs. It is authored code,
not evidence of execution. Tests currently cover selected literal/suffix
branches rather than all candidate-array combinations. Resource/recursion bounds,
full preprocessing, parser speculation, template replay and constant-required
contexts remain future work and cannot be inferred from these tests.

A full review must re-read actual fixes and final harness, executed managed
inventory, native comparisons, dependency audit and NativeAOT smoke. M1 remains
partial while preprocessing is unsupported. Default Lex currently recognizes
numeric token shapes without validating all integer grammar; its IsSuccess
boundary must be documented or strengthened before lexical-validity claims.

## Additional consulted source hashes

| File under `../upstream/src/` | SHA-256 |
| --- | --- |
| `declarator.c` | `26680a8059cb338336b31d68ab40b25e9d1660bb5ba6192aea16ae15a9651552` |
| `il.c` | `535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0` |
