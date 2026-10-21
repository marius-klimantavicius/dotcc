# Scalar reference constant storage

This bounded extension is independently accepted in
`../evidence/review-reference-constexpr-final.md`: Release has no warnings/errors;
JIT and Linux-x64 NativeAOT each pass102/102 groups with zero skips. Four round2
receipts share96 current source/test/fixture/tool hashes; three output artifacts
are fingerprinted. Native strict C++20 LP64 uses `--instantiate used`, no predefined
macro file and no code generation:21 exact fixture/witness runs match12 accepts
(status0) and9 language rejections(status2). Native is an oracle only.

| EDG routine/branch | Preserved handwritten contract |
| --- | --- |
| interpret.c storage/live sets70–241, address/reference27322ff and loads33874ff | Evaluation-local scalar cells and reference locations; only explicit lvalue-to-rvalue nodes load. Binding and materialization invent no folded scalar value |
| process_call_work22952ff and return9356ff | Evaluate caller arguments before callee mapping; value parameters copy into distinct cells, references alias original locations; install return before cleanup |
| global/unknown32234ff/32262ff | Unknown addresses may travel without reads; unknown lifetime/identity/liveness stays explicit. Initializing scalar addresses can form while premature loads fail |
| exprutil.c17519ff/17681ff; il.c17431ff/17344ff | Replay original typed initializer into fresh storage for potentially constant scalar imports, retaining selected entities and cycle guards; no stale optional fold or later lookup substitutes for evaluation |
| expr.c35086ff; interpret.c31659ff | Both logical operands receive semantic/contextual-bool checks, but evaluation schedules only the needed RHS |

Caller argument temporaries survive nested calls until the caller full expression.
Callee locals, value parameters and return-created temporaries expire before caller
loads. Global const-reference direct binding can extend a materialized temporary;
aliases do not re-extend lifetime. Unknown reference facts have no fabricated object
identity, symbol, owner, initialization or liveness; UnknownReference separately
identifies the symbolic reference parameter. Required persistent unknown references
and unknown/mutable/expired reads fail. Immutable retained observations expose no
mutable evaluator cells. Every evaluation resets work/depth budgets and storage;
failed/early-return cleanup, reuse and live-session collection execute in tests.

Optional declaration-time failures remain historical observations. A later required
use can evaluate the original ordinary-const initializer once its selected function
has a body; mandatory constexpr initialization still fails at its source point.
RCE-01's proposed optional-late-body rejection was disproved by native evidence;
its mismatched receipt is retained and excluded. RCE-02–05 repaired real initializer
replay, unknown transport, initializing addresses and unknown public facts. Round1's
101/102 run passed all seven new groups; only an obsolete `and` Unsupported test
was corrected before the green round2 snapshot, with positive graph/value assertions.

The combined macro/character/template/frozen-overload/reference/storage fixture
checks selected identity, distinct instances, reused replay, required values and
retained origins after disposal. Its native type/category witness includes the
unchanged source bytes and discriminates later overload contamination.

These are scalar bounds. Assignment, branches/loops, general void execution,
pointers/arrays/classes, aggregate cleanup, dependent-reference templates, general
constant-expression classification and complete lifetime semantics remain required.
Full PLAN M1–M6 coverage is unchanged. Consulted source hashes are in
`source-reference-constexpr.json`; exact source mapping is also in the implementation
handoff. No whole-file port is claimed.
