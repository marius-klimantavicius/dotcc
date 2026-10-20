# Produced parser/expression review, round 2 — 2026-10-20

Status: **changes required; no milestone approved**. This round reviews actual
R1–R5 fixes and the first executed managed checkpoint. The supervisor's
`validation-initial.md` records a successful Release build with zero warnings
and errors, 25/25 passing test groups, zero skips, exact-byte native M2 fixture
comparisons and a successful managed analysis of the positive source. Reviewer
read code, assertions, fixtures and receipts; no reviewer build/test/native
execution occurred. NativeAOT execution is still pending.

## Findings after rechecking fixes

- **R1:** assignment continuations diagnose UnsupportedFeature, and `(x=1)=2`
  regression checks failure plus unsupported classification. Accepted binary
  nodes therefore need only PRValue for the declared subset. Initial managed
  validation passed this regression.
- **R2:** checkpoint identity ancestry invalidates removed descendants before
  state mutation; equal undo counts and diagnostic-only stale branches have
  regressions, along with nested/repeated/cross-state controls. Initial managed
  validation passed those cases. The helper is not advertised as a port of all
  future parser/template speculative states.
- **R3:** calls now receive the unqualified scalar expression result type;
  function declarations retain their declared return qualifier. The regression
  inspects both facts and verifies canonical signature versus const definition
  parameter. The native review-valid witness's `decltype(source())` assertion
  independently establishes unqualified int. Initial managed validation passed.
- **R4:** global function main declarations/uses now explicitly diagnose
  UnsupportedFeature. The three function repros have managed regressions; native
  main-void/main-bool/main-call receipts separately reject each. However the
  same special-name branch still silently accepts **global object main**:
  `int main;` / `int main=1;`. EDG `decls.c:7916–7941` explicitly diagnoses this.
  **R4 remains open** until global variables are covered and tested. Local
  variables named main must not be accidentally rejected.
- **R5:** function-designator conversions/operations now explicitly diagnose
  unsupported; bitwise/not alternative tokens are mapped to their operators;
  logical and spaceship tokens are explicitly unsupported. Native review-valid
  receipt accepts the discriminating valid forms; managed regressions verify
  the selected support/exclusion. `_custom` integer suffixes now receive an
  unsupported diagnostic. Initial managed validation passed these regressions.
  This does not establish all user-defined literal grammar support.

## R6 — High — preprocessing taxonomy depends on parser position

Affected code: `Parser.Statement`, `ScanOperand`, generic `Expect`, and places
where type/declarator parsing expects its next token.

Initial reproduction:

```cpp
int f(){
#define V 1
return 0;
}
```

Hash reaches ScanOperand and receives only InvalidSource. The program is valid
C++ with an unsupported preprocessing directive. Worker has added Hash operand
handling, but that alone misses preprocessing between type/declarator tokens:

```cpp
int
#define V 1
x;
```

and

```cpp
int f(int
#define V 1
x);
```

Acceptance: Analyze diagnoses unimplemented preprocessing consistently at body,
operand, declaration-interior and parameter-interior positions. Generic token
expectation or a central analysis preprocessing boundary can enforce this;
position-specific patching must not leave valid syntax labeled invalid. Lex's
documented tokenization-only boundary can remain distinct from Analyze.
**Open; sent promptly to supervisor.**

## R7 — High — alias to void does not form an empty parameter list

Affected code: `Parser.FunctionDeclaration`'s special literal `void)` test and
the later blanket SEM008 for a void parameter.

Reproduction:

```cpp
using V=void;
int f(V);
int f(){return 1;}
```

This is valid strict C++20, but produced code diagnoses V as an invalid void
parameter and constructs a one-parameter function type. EDG
`declarator.c:3481–3528` explicitly accepts a sole unnamed alias-bound void type
as an empty parameter list in C++11 and later. This is inside the claimed
alias/void/function subset, not template or pointer scope expansion.

Acceptance: after resolving the parameter type, treat a sole unnamed,
unqualified void type before `)` as empty; f(V) and f() must share canonical
zero-parameter function type and symbol. Keep named void and void combined with
other parameters invalid. Add a native control and graph regression.
**Open; sent promptly to supervisor.**

## Promotion representation clarification

The worker separately chose to expose both integral promotion and subsequent
common-type conversion for short/bool + long. This is a useful explicit managed
API adaptation. It must not be described as an EDG IL node-parity defect: EDG
`determine_arithmetic_conversions_full` (~11876–11884) computes promoted types
without mutating operands, then `change_binary_operand_types` (13583–13628)
casts directly to the selected common type (`expr.c:31753–31755`). Tests should
assert the promised managed conversion structure; fixed-LP64 result values and
types were already correct. Supervisor notified of this distinction.

## Remaining gate

Recheck R4/R6/R7 fixes and final tests, source hashes and full stable managed
validation. Inspect final exact-byte native comparisons and NativeAOT receipt.
Reconcile SUPPORT/CORRESPONDENCE claims with actual bounds. M1 remains partial;
no macro/conditional/include implementation or template/constexpr gate follows
from this scalar slice.
