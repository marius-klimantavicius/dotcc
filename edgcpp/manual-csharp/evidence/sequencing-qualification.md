# Scalar sequencing qualification

This supersedes any inference that a false static_assert proves native detection
of an unsequenced conflict. It does not change the retained isolated receipts.

The C++20 [N4861 intro.execution/10](https://timsong-cpp.github.io/cppwp/n4861/intro.execution#10)
defines conflicting unsequenced scalar modification/read behavior as undefined.
[N4861 expr.const/5.7](https://timsong-cpp.github.io/cppwp/n4861/expr.const#5.7)
excludes undefined operations from core constant evaluation. Paragraph11 of the
former separately describes argument-before-body and function-invocation order;
caller argument effects must not be confused with separate callee-body effects.
The primary draft text was inspected by supervisor and independent reviewer.

Exact pinned native probes with correct computed-value assertions accept0:

- id(i=1)+i, assertion2: native-controlflow-contract-scalar-call-sequence-round1.
- i+(i=1), assertion1: native-controlflow-contract-scalar-direct-sequence-round1.
- pass("abc",i=1)+i, resulting element98: native-controlflow-contract-pointer-unsequenced-round1.

The latter two receipts originally proposed reject and are matchedfalse; they
remain retained/excluded as negative evidence. A rejected assertion0 on a
nonzero computed result cannot establish undefined-behavior detection. Native
acceptance is likewise not proof that a source expression has defined behavior.

Actual EDG exprutil.c2390ff/2934ff supplies source diagnostic sequencing walks,
including completed argument effects before invocation. That mechanism is
distinct from the managed interpreter's finite cell-access observations. No
profile option or interpreter parity is inferred solely from absent diagnostics.

The implemented finite direct-scalar conflict check is an intentional stricter
constant-evaluation adaptation justified by the C++20 rules above. It is not
claimed native accept/reject parity. Callee-body invocation identities, permitted
argument ordering, explicit assignment/subscript/logical sequence boundaries and
source operand order retain separate checks. The pointer-arithmetic candidate
was provisional and produced no new finding or forced rejection policy.

Complete language/dialect/user-defined operator sequencing and diagnostic
configuration remain required in the full feature inventory. Actual main
integration still requires produced-code review and matching execution receipts.
