# Reference constexpr temporal-policy review, round 2

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. No implementation
edits, builds/tests, native/compiler execution or Git operations performed.
The worker is still editing; no snapshot is accepted.

## Canonical dispositions

**RCE-01: disproved/closed.** The ordinary const-reference late-body control
accepts0 natively, despite the originally assumed rejection. The mismatched
receipt remains excluded. No blanket declaration-time eligibility restriction
is justified. A required `constexpr const int&` declaration before the needed
body is available rejects2; those are distinct contexts.

**RCE-02: high, open pending produced repair/execution.** The actual scalar
path still requires a successful cached declaration-time ConstantEvaluation.
The following exact optional const scalar control accepts0 natively but that
path currently treats its load as NotConstant:

```cpp
constexpr int later();
const int value=later();
constexpr int later(){return 3;}
static_assert(value==3);
```

`native-reference-constexpr-value-late-body-contract-round1.json` retains the
disproved expected-reject receipt. Its accept0 result establishes the positive
contract, but the mismatched expectation does not count as a passing control.

## Actual EDG source policy

- `exprutil.c:is_potentially_constant_valued_variable` (17519ff) admits
  nonvolatile const integral/enum variables and reference-typed variables, with
  broader literal/template/dialect branches conditional.
- `exprutil.c:var_constant_value_full` (17681) calls `initializer_constant`.
- `il.c:initializer_constant` (17431ff) handles a dynamic initializer by calling
  `fold_dynamic_var_init_if_possible`, instead of requiring a cached successful
  earlier optional evaluation.
- `il.c:fold_dynamic_var_init_if_possible` (17344–17407) temporarily marks the
  variable uninitialized to prevent runaway recursion, interprets the original
  initializer, and restores state. It rejects folding a temporary address and
  restricts static/local addresses; C++26 local-address branches are conditional.
- `decl_inits.c:6070–6155` separates an optional failed interpretation from a
  required constexpr/constinit initializer failure. `interpret.c:32234` obtains
  `var_constant_value` for a non-frame variable value load.

Acceptance for RCE-02 requires replay of eligible original typed const scalar
initializers with bounded self/cycle handling, preserved selected entity and
source-order lookup facts, and fresh evaluation-owned storage. No lookup
reparsing, mutable-cell import or mutable-global load is justified. Exact
optional late-body positive and required constexpr negative controls must run,
with existing mutable/uninitialized/lifetime regressions. Full dynamic/static
initialization, volatile/enum/class/pointer/constinit/P2280/C++26 coverage is not
implied by this bounded adaptation.
