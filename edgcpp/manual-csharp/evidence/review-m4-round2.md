# Independent M4 review — evaluator and parser findings

Date: 2026-10-21. Read-only inspection of in-progress produced code; no reviewer
execution. These findings were sent promptly to the supervisor. Final severity
closure requires reviewing fixed code and matching executed regressions.

## M4-R3 — High: discarded nonvolatile lvalues are interpreted as reads

`ConstantInterpreter.EvaluateStatement` currently sends every expression
statement to normal value evaluation. `EvaluateNode(ReferenceExpression)` then
reads its variable's storage. A discarded nonvolatile lvalue does not undergo
lvalue-to-rvalue conversion. The typed parser graph already distinguishes a
plain reference from explicit lvalue-to-rvalue conversion, so interpreting that
reference as a read incorrectly rejects valid C++20 constexpr execution.

Native/managed acceptance probes requested:

```cpp
constexpr int f() { int x; x; return 1; }
static_assert(f() == 1);
```

Also test `(x);` and `int h(); constexpr int f(){h; return 1;}` with a required
call to f. Merely discarding a function designator must not call it or require
it to be constexpr. A paired `+x;` control must still perform the actual read and
fail for an uninitialized x. Fix through explicit discarded-value execution or
an equivalent bounded treatment of the supported reference/parenthesized forms;
do not skip expressions whose operands require evaluation.

## M4-R4 — High: constexpr is silently ignored on aliases/typedefs

`Parser.Declaration` consumes leading `constexpr` before its alias/typedef
branches, then those branches create an ordinary alias without a diagnostic.
Consequently `constexpr using I=int;` and `constexpr typedef int I;` currently
have success paths despite being invalid source.

Acceptance: reject illegal specifier combinations before alias creation, with
native negative controls and managed invalid-source assertions. Valid supported
constexpr variables/functions must retain their behavior.

## M4-R5 — High contract issue: static_assert skips contextual bool conversion

Current `StaticAssertion` evaluates the raw condition directly. For
`int f(); static_assert(f);`, the function designator's pointer/bool conversion
is valid C++ but outside the implemented subset. The current interpreter instead
returns NotConstant and the parser emits only CEX001/InvalidSource. This repeats
the previously fixed function-decay taxonomy hole in a newly introduced context.

Acceptance: apply the explicit contextual-bool conversion, preserving dependent
value/type behavior and graph facts, or explicitly diagnose unsupported function
designators in this context. Required-context false/nonconstant/void controls
must remain diagnosed appropriately. Confirm with native EDG before closure.

## Additional source guards and interim resolutions

- R1's produced `TypeTable.Function` now bypasses session interning when result
  or parameters are dependent/scope-owning. The reference path identified in
  round 1 is removed. Repeated-template/count/WeakReference execution is pending.
- R2's initial parameter/body check now walks lexical template ancestors.
  The separately reported template function-name branch after the template
  scope is popped still requires explicit checking and regression review.
- New dependent expression nodes expose value dependency separately from their
  result type; the interpreter directly reports Dependent for those nodes.
  Concrete bool result types do not establish a known value. Review propagation
  through conversions, calls and const locals in the stable snapshot.
- Replay states share the root's mutation journal. Their independent token
  cursors do not provide a correct cross-state checkpoint lineage automatically.
  The worker reports child Checkpoint/Restore will be explicitly prohibited;
  root checkpoints can then journal child graph/cache mutations without claiming
  general child speculation. Verify this bound and new-state rollback tests.

## Material native-oracle profile correction

The supervisor discovered and preserved a failed expected-rejection receipt:
with the old `--no_code_gen` invocation, EDG's external-template instantiation
default is `none`. `ext_intf.rst:1649` documents `none` and `used` modes. A native
acceptance under default none cannot prove that a selected template body was
checked. Required constexpr evaluation happened to force the combined gate's
bodies, but does not establish nonconstexpr selected-body checking.

The adapter now sends explicit `--instantiate used` by default, permits explicit
none for a documented purpose, and records the policy in its profile/command.
Supervisor reports corrected used-profile controls reject selected false
assertion/invalid sink body while accepting unused false assertion/losing body;
integration controls and exact receipts must be independently checked at the
final gate. Earlier mismatched evidence must remain retained and excluded from
acceptance counts. No production fallback or semantic exception is authorized.
