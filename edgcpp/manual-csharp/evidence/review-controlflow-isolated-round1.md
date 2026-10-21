# Independent isolated scalar assignment/control-flow review — round 1

Status: **ACCEPTED — BOUNDED ISOLATED SNAPSHOT ONLY**. CF-R1-01/02/03 are closed
against actual held source, authored regressions and executed evidence. No known
unresolved critical/high finding remains in this reviewed scalar slice.
Independently recomputed hashes of all 26 listed code/test/fixture files match
the author's terminal OWNED-FILES HOLD manifest. Handoff SHA-256:
`fe063c1121c9c6988ea8eeb79df645bee00b5ca5561d0da2d9cc8cacd5defffe`.
Historical findings below describe the original live source and subsequent
source responses. Final closure uses the held executions audited below.

## Authority and boundaries

Independent GPT-6.1-sol/xhigh review of
`/home/marius/.t3/worktrees/dotcc/t3-c8f7a1e2/edgcpp/manual-csharp`, branch
`t3/scalar-assignment-control-flow`, supplied accepted base/HEAD
`44640803bf5670e332a347a5836567e2be588eec` (102 groups). No Git command was used
to independently inspect that supplied identity. The only reviewer write is
this report in the main checkout. No build, test, native invocation, source edit,
implementation fix, or subagent was performed by the reviewer. Shell use is
limited to reading/searching files and computing read-only fingerprints.

The full PLAN M0–M6 remains required. This is a bounded isolated module review;
it does not accept the unseen Unicode/strings/arrays integration, a checkpoint
does not finish the campaign, and required Unsupported/unverified branches
remain outstanding. Native results below were executed serially by the
supervisor, not by the reviewer.

Read: main AGENTS.md, PLAN.md, PROGRESS.md, docs/FEATURE-INVENTORY.md and accepted
docs/CONSTANT-STORAGE.md; actual isolated implementation handoff; actual scalar
production graph/parser/interpreter/storage and authored tests/fixtures. Native
collaboration `/root/manual_edg_supervisor` is unavailable from this T3 child;
material findings have been sent to the T3 parent/root for relay.

## Findings

### CF-R1-01 — high — imported static temporary is misclassified as evaluation-local

Affected original source: `ConstantStorage.cs:102–106`, `:163–173`; the resulting
store is permitted by `ScalarControlFlow.cs:243–248`.

`Allocate` derives `BeganInEvaluation` from `frame != null` or a full-expression
temporary. `Materialize` passes the active call frame even when `isStatic` is
true for a global reference's lifetime-extended temporary. Importing that
reference during a constexpr call therefore marks preexisting static storage
as having begun its lifetime in this evaluation. The write guard then permits
the forbidden mutation.

Discriminator:

```cpp
int&& global = 1;
constexpr int f() { global = 2; return 1; }
static_assert(f() == 1);
```

There is deliberately no load of `global` after the write; an independent
readability guard must not hide the missing store-origin guard. The same
runtime syntax without the assertion is valid, and a local `int&& value = 1`
inside the evaluated call can be modified.

Actual EDG correspondence: `interpret.c:30218ff` forbids modification of runtime
storage whose lifetime began outside the evaluation; the separate expired and
const-storage checks follow it. This is independent of which interpreter frame
happens to import the object.

Supervisor native controls have been read in
`native-controlflow-contract-global-temporary-write-round1.json` (reject 2),
`native-controlflow-contract-global-temporary-runtime-round1.json` (accept 0),
and `native-controlflow-contract-local-temporary-write-round1.json` (accept 0).
All three have explicit strict C++20/used/UTF-8/no-code-generation profile,
source and compiler hashes, exact status and `matched: true`. Their receipt paths
refer to the held Unicode checkout where they were executed; exact inputs are
copied to main `tools/oracle-inputs/m4-controlflow-contracts/`.

Required closure: origin-based lifetime classification, preserving allowed
local/value-parameter/full-expression temporaries; direct managed source and
storage-fact regressions for global/local cases; matching held JIT/AOT and
native evidence.

Status: **closed**. Held `ConstantStorage.cs:102–106` accepts explicit origin;
materialization passes `!isStatic` at171, imported scalar objects default false,
and value-parameter/local allocations explicitly pass true in
`ConstantEvaluation.cs:243/257`. `ScalarAssignmentFailures:135–143` checks the
global no-load failure, zero assignments and false lifetime-origin fact, local
temporary success/identity/readonly/expiration, and runtime-only acceptance.
Both JIT/AOT execute those assertions. Exact isolated global-temporary-write,
global-temporary-runtime and local-temporary-write receipts match reject2/
accept0/accept0 with current fixture hashes. The original four main contract
controls remain supplementary historical evidence rather than substitutes for
these exact held fixture runs.

### CF-R1-02 — medium — empty selection initializer becomes invalid syntax

Affected original source: `ScalarControlFlow.cs:34–39` (`ControlCondition`).

The selection-initializer semicolon is checked only after `ScanExprFull`.
A valid empty init-statement reaches `ScanOperand` first and produces `PAR003`
instead of the required Unsupported classification:

```cpp
int f() { if (; true) return 1; return 0; }
```

`statements.c:scan_condition` explicitly includes `curr_token == tok_semicolon`
when selection initializers are enabled. The original authored boundary test
covered `if(1;true)` and declarations, but missed the empty initializer.

Read the actual native accept-0 receipt
`native-controlflow-contract-empty-selection-initializer-round1.json`. The main
copied exact input SHA-256 is
`8a74859776f7d98aa2f38e48e005db1a489dd24281d3adfdfb8104e375896b96`, matching
the receipt. All four current native contract inputs and the actual compiler
have independently recomputed hashes matching their receipts; compiler SHA-256
is `e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.

Required closure: classify the empty initializer as Unsupported before scanning
an expression, with a source-based taxonomy regression and held execution.
Status: **closed**. Held `ScalarControlFlow.cs:85` checks the leading semicolon
before expression scanning. `ControlBoundaries:304–307` directly executes the
empty-init Unsupported classification in both JIT/AOT. The exact isolated
empty-selection-initializer fixture is native accept0 with a matching held hash.

### CF-R1-03 — high — eager recursive queries bypass the interpreter work bound

Affected original source: `ScalarControlFlow.cs:ScheduleAssignment` (initial line232,
then233), `SemanticGraph.cs:214/226`; the new effect getter also recursively
walks children in `ScalarControlFlow.cs:12–23`.

Before scheduling any assignment work, `ScheduleAssignment` queries
`expression.IsValueDependent`. The assignment delegates to its RHS;
`BinaryExpression.IsValueDependent` recursively descends the full left-deep
arithmetic graph. A flat supported sum parses into such a graph without deep
parser recursion. A source of the following shape with tens of thousands of
terms therefore triggers unbounded CLR recursion at the assignment, before the
explicit interpreter work limit can stop evaluation:

```cpp
constexpr int f() { int x; x = 1 + 1 + /* many more terms */ 1; return x; }
static_assert(f() == /* number of terms */);
```

With a small `ConstantEvaluationStepLimit`, this must return ResourceLimit
through ordinary work dispatch and remain reusable. The original dependency
walk is independent of that budget. `HasSideEffects` on the same pure graph is
also an unbounded new recursive API query. The source establishes the unbounded
paths; no reviewer execution or particular stack-overflow threshold is claimed.

Required closure: avoid/bound the eager recursive dependency traversal, provide
stack-safe effect queries while preserving the agreed virtual extension
behavior for new expression kinds, and execute a bounded adversarial long-sum
assignment/effect/resource/reuse regression under supervisor control.
Status: **closed within the held flat-expression bounds**. `ControlLimits`
executes the 2048-term flat sum under a 12-step budget, checks iterative public
dependency/effect queries and reuses the same interpreter across resource
failure/success/failure. This group passes both actual JIT and NativeAOT.
No general nested-parser resource qualification or old-crash threshold is claimed.

## Completed held source/algorithm audit

In the held responses for CF-R1-01/02/03, an explicit origin
argument replaces frame-derived lifetime classification, an empty selection
initializer guard precedes expression scanning, the assignment dispatcher no
longer walks dependencies eagerly, and scalar effect/dependency queries use
explicit stacks. Authored regressions include the global/local temporary
controls and a 2048-term flat sum with a small work budget plus same-interpreter
resource-failure/success/repeated-failure reuse. These responses have now been
inspected at HOLD and executed by the supervisor under matching JIT/AOT maps.
Exact isolated native and artifact audits also pass below.

The current implementation has dedicated assignment nodes returning locations,
explicit RHS conversions, a separate compound operation type and no scalar fold
for a store. Its work stack schedules RHS before the single destination
evaluation and performs the compound load afterward. Storage keeps object
readonly qualification separate from reference qualification and lifetime
origin. Nullable public readonly/lifetime facts preserve unknown objects.

Control statements retain distinct scopes and loop-target identities. The
interpreter schedules condition full-expression cleanup before branch/body
selection and carries return/break/continue through intervening `LeaveScope`
work. Loops use work items and step limits. Produced tests inspect actual graph,
storage, selected execution and cleanup facts; both execution modes pass them.
The 27 owned files, including the handoff, remain held. Exactly 16 new groups
are registered, giving 118 isolated groups over the accepted 102-group base;
there are 18 new exact native fixtures.

Audited actual upstream branches include `expr.c:37121ff/37382ff` (modifiable
lvalue, source conversion, compound operation type, assignment result lvalue),
`statements.c:3734ff/4056ff/4657ff/4724ff` (dependent scopes, conditions,
while/do), and `interpret.c:9195ff/9560ff/27299ff/30206ff` (work transfer,
condition/return cleanup, RHS-first operands, store guards).

The unsequenced-access audit follows `exprutil.c:2444ff/2650ff/2870ff`, including
ordered operands and explicit call handling. `statements.c:5540ff/6587ff/6713ff`
supplies for and nearest structured jump contracts. Current parser loop state
is restored with finally blocks; template definition/instance rescans create
separate Parser objects. New graph targets are immutable identity objects, and
mutable jump/observation/access state is evaluation-local and cleared in finally.
No additional shared parse-state mutation requiring a new journal operation was
identified in this slice. The malformed-loop recovery, existing checkpoint/
replay rollback, repeated independent analyses and live-session/interpreter
collection tests pass in the held suite. No recursive production cleanup walk
was introduced. The bounded long-sum query repair does not qualify arbitrary
deeply nested parser input or general cancellation/memory limits.

The native witness includes unchanged positive and combined inputs. It proves
simple/compound/nested assignment result types are `int&`, int/long template
result types and the later ordinary mutable-reference overload's `void` result.
The included template's successful computed values discriminate its frozen
earlier overload set. The positive order fixture discriminates RHS-before-LHS
and single destination evaluation with native result12; managed assertions
additionally inspect assignment observations `11,1,11,12`. The combined source
tests inspect macro origins, separate/reused instances, alias
cell identity, selected conversions and actual loop transfers. Unsupported
template/class/pointer branches are not credited as implemented.

`il.c:17344ff/17431ff` was read as the existing initializer-replay/folding
context: the original dynamic recipe and cycle guard are retained, and a
temporary address is not replaced with a misleading folded initializer. This
module adds no whole-routine implementation claim there. `interpret.c:8518ff`
was also checked for for-init once, continuation increment and owner cleanup.

Read-only SHA-256 of consulted actual main upstream files:

- `expr.c`: `056f8e7416d6f948846abbe17c3401e9eb4ca8ed1ce0ae7673cd573b314df3c2`
- `statements.c`: `a4b9476b67189ea0ed2034239f4494bc32a4f50a885bb258ab01eb8e01bac703`
- `interpret.c`: `d3f34d737a6690e482942a52a463e406b767a0d5fc06b3609c8cb9c79b27cb4d`
- `exprutil.c`: `1b01609700d2a72f64f94e218a3f210e7e9503394bc3cf16168e8a1a50e50812`
- `il.c`: `535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0`

All five match the finished handoff and actual files in both main and isolated
upstream paths. No generated C# implementation was used as review input.

## Executed evidence and bounded acceptance

The explicit author HOLD, finished handoff/test/fixture inventory and matching
owned-file manifest are available. Actual
`controlflow-isolated-build-round1.json` records Release exit0 with no warnings
or errors. `controlflow-isolated-tests-round1.json` records direct-dotnet JIT
118/118, zero failures/skips. `controlflow-isolated-nativeaot-publish-round1.json`
records successful Linux-x64 NativeAOT publication, and
`controlflow-isolated-nativeaot-tests-round1.json` records its actual executable
118/118, zero failures/skips. All four complete maps are identical 116 files,
including `tools/oracle.py`, `tools/test-oracle.py`, `tools/record-check.py`.
The reviewer independently recomputed all 116 current file hashes: no mismatch.
Specific finding regressions, scalar order/identity/arithmetic, both-source-branch
checking, nested transfers/cleanup, repeated evaluation, immutable retained
observations and collection groups were inspected in produced tests and their
actual named PASS lines; acceptance does not rest on the aggregate group count.

All 18 `native-controlflow-isolated-*-round1.json` receipts were read, including
diagnostics, exact command/profile, source/compiler hashes and expectations:
7 accepts at normal0 and 11 language rejections at2, with no unexpected status
or skipped run. Every source hash matches the 116-file held map. All use
strict C++20, used instantiation, explicit UTF-8, predefined macro file disabled
and no code generation; the prior LP64 target qualification remains the profile.
The witness's two additional input hashes independently match the held positive
and combined fixtures. Runtime hazards accept with native warnings; the expired/
uninitialized/global/unknown negatives reject in required constant contexts.
An infinite required evaluation rejects for excessive constexpr complexity,
while bounded managed tests inspect ResourceLimit and later reuse explicitly.
Native and managed warning parity or identical cost thresholds are not claimed.

`controlflow-isolated-held-snapshot.json` matches all four receipt maps and the
handoff hash. Independently read actual artifact sizes and SHA-256 match:

- `src/EdgSharp/bin/Release/net10.0/EdgSharp.dll`: 270336 bytes,
  `554c6f2561dab75b2515fa7c460ac21738253e95674dd352f2f51bdf87ea3d66`.
- `tests/EdgSharp.Tests/bin/Release/net10.0/EdgSharp.Tests.dll`: 256512 bytes,
  `f4cf308503b527da7a421c31130386a2228dfbebe515e6051a527b388b00fd48`.
- `tests/EdgSharp.Tests/bin/aot-controlflow-isolated/EdgSharp.Tests`: 4432312 bytes,
  `19a3c4b96f8bb595656d0f01f899d3acbb98071cd7a9a7a8276c43dc40fc56c1`.

Accept only this isolated handwritten integer assignment and expression-condition
if/while/do/ordinary-for/break/continue slice, with the documented scalar storage,
sequencing and cleanup bounds. No unresolved finding is deferred under this
acceptance. Shifts/shift assignment, increments, comma/conditional/casts,
condition declarations, selection initializers, if-constexpr, switch/goto/labels,
range-for, general void/object/class/pointer execution and all other open PLAN
rows remain required. Broader resource limits remain M6 work.

Main integration is **not accepted**. Preserve main's Unicode canonical identity,
distinct array/pointer/pointer-location payloads, stable element cells, readonly
and lifetime-origin metadata, import/frame ownership and new expression override
semantics during the additive merge. Combined held execution, native witnesses
and independent review are still required there. Source repairs needing a later
review round must use a fresh delegated task carrying prior findings/responses.
The supervisor continues the full M0–M6 campaign without a checkpoint stop.
