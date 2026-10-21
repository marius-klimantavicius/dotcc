# Isolated scalar assignment/control-flow implementation

Status: **READY / OWNED-FILES HOLD for the early isolated Release/JIT gate**.
All files listed below are frozen pending an explicit scoped release. Source is
UNEXECUTED: no build, managed/AOT test, native invocation or acceptance is claimed.
This bounded module is not M0–M6 completion. Supervisor owns all validation,
Git, independent finding closure and main integration.

- Worktree: `/home/marius/.t3/worktrees/dotcc/t3-c8f7a1e2`.
- Base/HEAD: `44640803bf5670e332a347a5836567e2be588eec` (accepted102 groups).
- No builds/tests/native execution/Git mutations/commits or main-checkout edits.
- Owned central paths: `Parser.cs`, `Expressions.cs`, `ConstantEvaluation.cs`,
  `ConstantStorage.cs`, `SemanticGraph.cs`; `References.cs` only if necessary
  (currently untouched). New `ScalarControlFlow.cs`, `ControlFlowTests.cs`,
  fixtures, Program registrations and this handoff. No token expansion is needed.
- Main Unicode acceptance `6c3848ca5` is separate. Preserve main's raw/canonical
  identity, character/encoding and Unicode additions when merging central hunks.

## Agreed interfaces for the strings/arrays module

- `ConstantCell` remains the stable integer scalar/array-element object. The same
  array/index must yield the same cell. The existing scalar `(Integer, Location)`
  runtime-value constructor and `Scalar`/`Address` factories are preserved here.
- Internal immutable `ConstantCell.ReadOnly` uses actual allocated object
  qualification with trailing optional ctor `bool? readOnly = null` and
  `ReadOnly = readOnly ?? type.Qualifiers.HasFlag(Const)`; binding
  a const reference to mutable storage never changes the target cell flag.
  Public `ConstantStorageObject.IsReadOnly` is `bool?`, null for unknown objects.
- Independent `ConstantCell.BeganInEvaluation { get; init; }` and public nullable
  `LifetimeBeganInEvaluation` distinguish local/value-parameter lifetimes from
  preexisting global/imported automatic objects. Scalar stores check known,
  alive, readonly and evaluation lifetime, then converted integer/access ordering.
- `Allocate(..., bool readable, CallFrame? frame, bool? readOnly = null,
  bool beganInEvaluation = false)` is
  additive. Existing frame `Owned`/`LifetimeOwners`, scalar/reference slots and
  full-expression owners remain; cleanup expires original owned cells. Aliases
  transport a location, never load or re-extend lifetime.
  Origin is explicit: local/value-parameter allocations pass true; materialization
  passes `!isStatic`; imported scalar storage defaults false, independently of
  whichever frame is active. Strings' newly created local elements must pass true.
- `Load(ConstantRuntimeValue)` becomes an instance method to record reads against
  the same cell identity as writes. New immutable assignment/control observations
  are per-evaluation and cleared from the interpreter after snapshot creation.
- Strings main owns distinct `ConstantArray` storage and
  `ConstantArrayAddress(Array, Index)` pointer payloads, `Array`/`Pointer`/
  `PointerLocation` runtime-value extensions and pointer slots. They are absent
  here and must survive the later three-way merge. No integer surrogate pointer.
- `Expression.HasSideEffects` is virtual. Scalar composites use an iterative
  worklist; unknown child nodes dispatch their property override, with unknown
  base implementations conservatively returning true. New subscript base/index
  and pointer operands must override/extend evaluated traversal on integration;
  `SizeOfExpression` must explicitly return false for its unevaluated operand.
  Scalar composite dependency queries are also iterative; new graph variants
  must preserve their dependency overrides when merged.

## Initial central hunk contract

- Expressions: right-associative assignment precedence, destination lvalue/cv
  checks, explicit RHS conversions, compound arithmetic type and single target
  evaluation; no folded value for a store.
- Parser: structured dispatch and outermost for-body redeclaration restriction.
  New control parser retains explicit scopes and resolves loop transfer identity.
- Graph: additive assignment, if, while, do, for, break/continue and loop targets.
- Interpreter: additive work dispatch, frame transfer state, `LeaveScope` replacing
  `LeaveBlock`, condition full-expression cleanup before branch/body execution.
- Storage: additive readonly/evaluation-lifetime facts, instance load and immutable
  observation construction; original initializer replay/cycles stay intact.

## Implemented source behavior, pending execution

`=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=` are authored for integer
scalar destinations. The parser checks lvalue, const and integer conversion
requirements and retains assignment result lvalue identity. Compound nodes
carry the arithmetic operation type without duplicating destination evaluation.
RHS evaluation precedes the destination, and the store result is an address;
only explicit conversion nodes load it. There is no folded assignment value.
Simple C++20 assignment may initialize an uninitialized cell; compound
assignment loads first and rejects an uninitialized value. Arithmetic checks
operation-width overflow/division before the final destination conversion.

`if`/`else`, expression-condition `while`, `do` and ordinary `for` have actual
structured graph/parser/interpreter paths. Both source branches/bodies are
semantically checked; evaluation selects only the needed branch/iteration.
Unbraced substatements get real scopes. For initializers have retained scopes,
with the outermost-body redeclaration restriction and cleanup on every exit.
Break/continue nodes retain the innermost `LoopTarget` identity. Per-frame
pending transfers skip subsequent executable work while `LeaveScope` still
runs; only the owning loop consumes the transfer. For-continue runs increment,
do-continue runs the condition, and return retains its value before cleanup.
Condition temporaries expire before dependent statements execute.

The explicit work budget charges loop conditions, bodies, transfers and stores;
empty infinite loops still exhaust it. The existing call-depth limit remains.
Per-evaluation assignment/control snapshots contain immutable facts and selected
graph nodes, never mutable cells. The interpreter clears new effect/trace state
after success/failure, alongside existing stack/storage cleanup.

Unsequenced operand conflicts in the implemented arithmetic/comparison/bitwise
binary expressions are checked using actual cell identity. A permitted fixed
order is used for indeterminately sequenced function arguments. Nested function
executions have distinct invocation identities. This is a bounded scalar
adaptation of EDG's sequencing checks, not general pointer/class sequencing.
Existing logical short circuit is reused unchanged.

## Review responses awaiting supervisor execution and independent closure

- **CF-R1-01 HIGH:** imported static reference temporaries must not become
  writable because a caller frame is active. `Allocate` now accepts explicit
  allocation origin; new locals/value parameters pass true, materialization
  passes `!isStatic`, and imported scalar cells default false. The no-load global
  write, runtime-only complement and local rvalue-reference temporary success
  are authored in `ScalarAssignmentFailures` and three named fixtures below.
  Supervisor reports native strict-profile statuses2/0/0 in main receipts
  `native-controlflow-contract-{global-temporary-write,global-temporary-runtime,local-temporary-write}-round1.json`.
  Those native receipts were not generated in this checkout; managed closure
  and exact-byte requalification of this checkout's fixtures remain pending.
- **CF-R1-02 MEDIUM:** `if (; true)` is valid unsupported syntax. Selection
  parsing now recognizes the leading semicolon before scanning an operand,
  emitting Unsupported. `ControlBoundaries` includes this classification and
  also distinguishes a stray `else` as actual invalid source. Supervisor reports
  native accept0 in `native-controlflow-contract-empty-selection-initializer-round1.json`.
- **CF-R1-03 HIGH:** eager assignment dependency recursion could exhaust the CLR
  stack ahead of work limits. Assignment scheduling no longer walks dependency
  graphs; real operand work reaches dependent failures. Scalar effects and
  composite dependency queries use explicit stacks, including left-deep sums.
  `ControlLimits` authors a bounded2048-term flat sum with a12-step interpreter,
  safe public effects/dependency queries and failed/successful/failed reuse.
  No crash repro or managed execution was performed. New expression overrides
  must extend this stack-safe traversal contract during strings integration.

## EDG provenance and branch correspondence

Imported source base is `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`; actual consulted
implementation hashes below identify the files, not whole-file port coverage.
Apache-2.0 WITH LLVM-exception notices are retained in authored source/fixtures.

```text
056f8e7416d6f948846abbe17c3401e9eb4ca8ed1ce0ae7673cd573b314df3c2  edgcpp/upstream/src/expr.c
1b01609700d2a72f64f94e218a3f210e7e9503394bc3cf16168e8a1a50e50812  edgcpp/upstream/src/exprutil.c
a4b9476b67189ea0ed2034239f4494bc32a4f50a885bb258ab01eb8e01bac703  edgcpp/upstream/src/statements.c
d3f34d737a6690e482942a52a463e406b767a0d5fc06b3609c8cb9c79b27cb4d  edgcpp/upstream/src/interpret.c
535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0  edgcpp/upstream/src/il.c
```

- `expr.c:12948–12974 change_assignment_result_to_lvalue`: preserve original
  destination type and lvalue result; represented by distinct `AssignmentExpression`.
- `expr.c:37121–37327 process_simple_assignment`: scalar non-overload path,
  suppress destination load, modifiable-lvalue check and RHS assignment
  conversion. `37330ff scan_simple_assignment_operator` recurses at assignment
  precedence. Braced initialization, classes/operators and vendor branches remain.
- `expr.c:37400ff scan_compound_assignment_operator`, especially37560ff
  modifiable destination and37720–37865 operation/result types: scalar integer
  arithmetic/bitwise forms only. The node records operation type separately and
  evaluates the destination once. Shift/pointer/class/vector branches remain.
- `exprutil.c:14086–14181 check_modifiable_lvalue_operand`: lvalue/complete
  integer/nonconst requirements; C/GNU/vendor and class-member branches remain.
  `2447ff merge_seq_pt_info_lists` and2950ff assignment side-effect merging guide
  the cell-identity sequencing checks. Existing `TypeTable` arithmetic conversion
  routines are reused rather than introducing CLR arithmetic types.
- `statements.c:3734ff dependent_statement`: explicit/generated substatement
  blocks and scopes. `4056–4135 scan_condition`: expression-to-bool path and
  explicit Unsupported for condition declarations/selection initializers,
  including empty init. `4182–4541 if_statement`: ordinary then/else semantic
  checking; no discarded-token implementation of `if constexpr` is claimed.
- `statements.c:4657–4721 while_statement`, `4724–4819 do_statement`,
  `5540–5914 for_statement` ordinary init/condition/increment branch: retained
  source structure, scoped bodies and correct continuation point. Range-for,
  condition variables, coroutines and vendor forms remain.
- `statements.c:6587–6637 continue_statement`, `6713–6777 break_statement`:
  innermost structured owner resolution; managed identity targets replace the
  native generated labels. Switch/goto paths remain unsupported.
- `interpret.c:30206–30323 eok_assign`: reject runtime/expired/const storage,
  permit C++20 initialization by assignment, return location. `30324ff`
  arithmetic compound branches and31023ff remainder/31286ff bitwise branches:
  typed arithmetic plus final destination conversion; scalar bounds only.
- `interpret.c:27260–27315 eval_right_to_left` scheduling: RHS before LHS
  without duplicate target evaluation. `8518ff do_constexpr_for_statement`:
  init once, condition/body/increment, continue increment and saved storage.
- `interpret.c:9195–9684 process_stmt_work`: explicit resume phases for if,
  while/do, return, break/continue flags and owner cleanup. Managed work records
  replace native phase unions while retaining evaluation/control separation.
- Existing initializer replay, reference transport/load and call-argument owner
  contracts from `docs/CONSTANT-STORAGE.md` remain. `il.c`'s initializer-folding
  contracts are consulted as context; this module adds no replacement folding
  or new whole-routine claim there.

## Exact central merge hunk inventory against base446

Old-file line anchors below refer to the unchanged base commit, not main's
Unicode/string line numbers. Integrate these hunks additively; do not replace
central files wholesale.

- `Parser.cs`: old148 `NewLocalSymbol` adds outermost for-body conflict check;
  after370 statement dispatch adds if/loops/transfers and invalid stray-else
  diagnosis. All identifier construction/spelling/canonicalization hunks from
  Unicode must remain as they exist in main.
- `Expressions.cs`: old13 precedence comment;24–27 right-associative assignment
  dispatch and unsupported-continuation unwind; after32 assignment precedence;
  after43 `IsScalarAssignment`/`Assignment`. Preserve main's literal/array/
  pointer/postfix/sizeof scanning, raw spellings and conversion branches.
- `SemanticGraph.cs`: after151 virtual effects property; old186/195/203/212/225
  parenthesis/conversion/unary/binary/call dependency getters use the iterative
  helper; after215 assignment node; after277 structured control nodes/targets.
  Preserve all main string/array/pointer/sizeof/subscript nodes and metadata.
- `ConstantEvaluation.cs`: old18/23 adds result observations; old34/101 removes
  obsolete `BinaryValue` work;40 scope-based cleanup work;50 return work carries
  its statement; after59 per-frame transfer state;83 names popped work;119/124/
  134 block/statement transfer handling; after161 return observation; after163
  additive control work fallback;182 clears new state;209 binary sequencing;
  after211 assignment dispatch;233/247 explicit local/parameter allocation origin;
  254–255 return/if/loop/transfer statement scheduling. Preserve main array/
  pointer work dispatch and cleanup. The new fallback must follow existing
  primary-module handlers rather than replacing them.
- `ConstantStorage.cs`: after27 public readonly/origin snapshots;30/33 tuple;
  53 nullable readonly constructor argument; after61 immutable readonly and init
  origin;98/100 optional Allocate flags and explicit origin;133 instance `Load`;
  after139 missing-value/access record;163 materialization origin;234 assignment/
  control snapshot construction. Preserve main `ConstantArray` storage,
  pointer/reference slots and all extended runtime-value payloads. Scalar
  `Store(ConstantSlot,...)` remains initialization-only, distinct from assignment.
- `Program.cs`: old75 group label reflects newly supported historical M2 if
  fixture; after120 sixteen registrations;412–414 actual if-graph assertions for
  that unchanged fixture;1115–1116 lvalue/computed nested assignment assertions;
  1152 obsolete assignment-Unsupported case replaced by still-unsupported shift
  assignment. These strengthen newly implemented behavior; no expected failure
  was removed to hide a still-unimplemented case.
- New modules `ScalarControlFlow.cs` and `ControlFlowTests.cs` are whole authored
  additions. `References.cs`, `Templates.cs`, `Types.cs`, tokens, Unicode,
  character/PP/provider sources and all support/progress ledgers are untouched.

## Authored test inventory and supervisor gate

Exactly16 new named groups are registered in Program. The isolated base102
therefore has118 expected groups, zero intentional skips. This is an inventory
count only. Main's separate Unicode/string groups are not included in118.

1. `ScalarAssignmentGraph`: lvalue/type, explicit RHS/result loads, reference
   binding and address identity, right associativity and nested stores.
2. `ScalarAssignmentConversions`: promotion/operation/destination types,
   narrowing, unsigned wrap, bool arithmetic, every authored compound and UB.
3. `ScalarAssignmentCells`: aliases share value-parameter storage, parameters
   copy, uninitialized-to-initialized store and readonly-vs-const-access facts.
4. `ScalarAssignmentFailures`: actual invalid bindings/conversion/nonexecuted
   branch, global/unknown/imported-auto/expired/uninitialized failures, static
   vs local rvalue-reference temporary origin and runtime hazard acceptance.
5. `ScalarAssignmentOrder`: discriminating RHS-before-single-target values
   `11,1,11,12`, with repeated stores to one observed location.
6. `ScalarAssignmentSequencing`: unsequenced read/write conflicts, permitted
   assignments/logical/call argument ordering and skipped store effects.
7. `ControlIfSelection`: selected branch observations, dangling-else binding,
   skipped division and semantic checking of unexecuted invalid bodies.
8. `ControlIfScopes`: lexical hiding, implicit scopes, sibling lookup isolation
   and temporary/local cleanup.
9. `ControlWhile`: actual totals/condition side effects, zero iterations,
   iteration counts and distinct per-iteration temporary cells.
10. `ControlTransfers`: resolved inner-loop targets, outer surviving owners,
    break/continue counts and crossed-block cleanup; computed nested result33.
11. `ControlReturnCleanup`: return through loop/if/block, condition and local
    temporary cleanup, expired returned stores and missing-return outcome.
12. `ControlDoFor`: do continuation, for increment/exit, omitted conditions,
    early return, for scope ownership and declaration conflict boundaries.
13. `ControlBoundaries`: invalid transfers, valid unsupported selection/condition/
    range/shift branches, stray else and restoration after malformed loop body.
14. `ControlLimits`: finite/infinite while/do/for cost, recursion depth,
    failure recovery, bounded flat2048-sum dependency/effects/resource reuse.
15. `ControlLifetime`: repeated failed/successful execution, stable old immutable
    snapshots, different new snapshot identity, live-session/interpreter release
    of dropped graphs and retained post-disposal queries.
16. `ControlCombined`: macro expansion origins, retained template replay/reuse,
    int/long instances, frozen const-reference overloads despite later void
    overload, alias store identity, branch/transfer observations and values.

Proposed first checks, supervisor only, from this campaign directory:

```sh
dotnet build EdgSharp.slnx -c Release --nologo
dotnet run --project tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release --no-build
```

An early gate is not acceptance; repair actual failures with scoped release,
then obtain matching-source NativeAOT/native receipts and independent review.

## Exact proposed native inputs/statuses

All names below are files in this checkout's `fixtures/`. Proposed strict
C++20 LP64 profile: predefined macro file disabled, `--no_code_gen`,
`--instantiate used`, using the existing separately invoked oracle adapter.
Only actual normal0 or language-error2 count; unexpected compiler statuses,
resource catastrophes or command failures are never a passing rejection.

- Accept0: `m4-controlflow-positive.cpp` (eight computed assertions, including
  RHS-first12, alias7, initialization4, branches3/8, while8, for4, do3).
- Accept0: `m4-controlflow-combined.cpp` (combined25, int6, long3).
- Accept0: `m4-controlflow-native-witness.cpp`, which includes the exact two
  preceding files; register both with oracle `--input`. It additionally proves
  simple/compound/nested assignment `decltype` is `int&`, template result int/
  long and the later ordinary mutable-reference overload's void result. The
  unchanged template's successful values discriminate frozen earlier lookup.
- Accept0: `m4-controlflow-runtime-hazards.cpp`. Mutable-global writes and
  possible expired/uninitialized runtime paths are not language rejections.
  The constexpr functions also have valid safe paths; no always-invalid
  constexpr-function assumption is used as the hazard control.
- Accept0: `m4-controlflow-global-temporary-runtime.cpp` and
  `m4-controlflow-local-temporary-write.cpp` (local value2).
- Accept0, managed Unsupported: `m4-controlflow-empty-selection-initializer.cpp`.
- Reject2, actual ordinary-language error: `m4-controlflow-const-target.cpp`,
  `m4-controlflow-non-lvalue.cpp`, `m4-controlflow-invalid-conversion.cpp`,
  `m4-controlflow-unselected-invalid.cpp`, `m4-controlflow-invalid-transfer.cpp`.
- Reject2, required constant-expression failure: `m4-controlflow-global-mutation.cpp`
  and `m4-controlflow-unknown-mutation.cpp` (NotConstant),
  `m4-controlflow-global-temporary-write.cpp` (NotConstant, no load required),
  `m4-controlflow-expired-mutation.cpp` (ExpiredObject),
  `m4-controlflow-uninitialized-read.cpp` (UninitializedRead),
  `m4-controlflow-infinite.cpp` (ResourceLimit; native cost threshold need not
  equal the managed limit). There is no runtime execution of the hazard files.

The18 proposed runs comprise7 accepts and11 language rejections. No result
is asserted for these exact fixture bytes until the supervisor executes them.
Example adapter shape (choose a new receipt path, never overwrite history):

```sh
python tools/oracle.py fixtures/m4-controlflow-positive.cpp --expect accept --receipt evidence/native-controlflow-positive-round1.json
python tools/oracle.py fixtures/m4-controlflow-native-witness.cpp --expect accept --input fixtures/m4-controlflow-positive.cpp --input fixtures/m4-controlflow-combined.cpp --receipt evidence/native-controlflow-witness-round1.json
```

## Remaining limitations and next tasks, not completion credit

Outstanding within these scalar/control families: `<<=`/`>>=`, shifts,
prefix/postfix increments, comma/conditional/casts, condition declarations,
selection init statements, `if constexpr`, switch/case/default, goto/labels,
range-for and general void interpreter execution. Complete sequencing across
future pointer/class/operator operations and full constant-expression
classification still need their own branches and controls. Deeply nested grammar,
general parse resource limits/cancellation and memory budgets remain M6 work;
CF-R1-03 only bounds the authored flat-expression/evaluator query path.

Keep all remaining PLAN/inventory work active: full strings/raw/prefixed/
concatenated encodings, arrays/pointers and layout, floating/UDL/extended targets,
classes/members/initialization/special members/inheritance/virtual semantics,
general templates/specialization/packs/constraints/ADL, full references and
lifetimes, exceptions/lambdas/coroutines/modules, realistic/real library headers
and projects/multi-TU, complete product API/recovery/cancellation/limits,
NativeAOT/platform/performance/corpus/update qualification and the final M0–M6
coverage audit. Main's separately authored Unicode/string work is not qualified
by this checkpoint. No support inventory or Unsupported row was marked done.

## Frozen owned files and fingerprints

READY/HOLD covers the six production files, two test files and all18 fixtures
listed below, plus this handoff (27 files total). `References.cs` remains
unchanged and unowned edits elsewhere are absent. These are source-inspection
hashes only, not execution receipts. The handoff's own hash is reported separately
after saving; it is excluded from its self-referential manifest.

Paths in this manifest are relative to `edgcpp/manual-csharp/`.

```text
3e6409702c2f68fea250a8e1679cfa8646808f44605a86850e601f06a0a13eaa  src/EdgSharp/Parser.cs
eb79d258468af435fd765acb5c7e20f97d7dc54d09bae28042002acb61b0d505  src/EdgSharp/Expressions.cs
49e78b634e5f6bbedf593a5960a960d5b704b6741c111dca9c0ea33deac9d397  src/EdgSharp/ConstantEvaluation.cs
e7b0d702a028239d96660cafe497ef856d6c4da183062b0dd95ce212df53e825  src/EdgSharp/ConstantStorage.cs
42c167c553adc9ed55d0d3845f668980d46aaedfa9f77fe5cb3556bc701bd889  src/EdgSharp/SemanticGraph.cs
14944222ec5f5bb033f1af6e4d83649d9a8763e8a83a5e85aee85e6900e76805  src/EdgSharp/ScalarControlFlow.cs
a93f8553b07573fa60ab95127c4fe7a0868eb358519b5df71607e3b2cac65acb  tests/EdgSharp.Tests/Program.cs
37aebeec5a92162de23efcf2402331d8c00d2eeb72fe24790e1b593b814cd288  tests/EdgSharp.Tests/ControlFlowTests.cs
8278a9d5ddc46a72e9dffcf48c317712ea0643c2a80c0d6f07c540e363ef24fa  fixtures/m4-controlflow-combined.cpp
8e1babda7ce601f0b86f4339971fcb08bd2c4f51e5c40b61e5a636c1b202ac22  fixtures/m4-controlflow-const-target.cpp
f87573ce6f31d90088029a65dffee748541c1b9d8a3c0afda6ad2b3f2aa81f1c  fixtures/m4-controlflow-empty-selection-initializer.cpp
7945c99305a8f0141559f1dd2853d0c895d20aa2b5a3c57854837317b79b4aea  fixtures/m4-controlflow-expired-mutation.cpp
bde82f604de0202c5374bbba8171c515696edc14dad2873115fe66c94e178d49  fixtures/m4-controlflow-global-mutation.cpp
c432a6a555419859724051f3763ae038004f29276b43be568eed0f75ba6daf11  fixtures/m4-controlflow-global-temporary-runtime.cpp
3d390c18295383a6bbccc2b75941f814832b6493aca7b031349d12b977ad69f1  fixtures/m4-controlflow-global-temporary-write.cpp
c8c464fdceb4bf65df4242e157d0d489d3da433d3727539f9f8cfcf647897a5b  fixtures/m4-controlflow-infinite.cpp
81e08581343f87fbc39fc08e6cbcb68885d9c8a4cc5bd6cff95ad51f84fa0682  fixtures/m4-controlflow-invalid-conversion.cpp
df91065e4d7ceb5a712f25949ba226f9dec7481886ed551564c52d8fec2d2cdd  fixtures/m4-controlflow-invalid-transfer.cpp
cc4beb78c3274d33ad2885b8e1030f5c7dd838068406117ab8168b6f9205ea01  fixtures/m4-controlflow-local-temporary-write.cpp
35383de5cfd116b847f93cda2083f3ee3aebeef47ac56a57ba5bf61667b994d9  fixtures/m4-controlflow-native-witness.cpp
ffacc7cead33fc4e3f6719c38c43bfa11e256cfb06bc0c2a29713a5f339145e2  fixtures/m4-controlflow-non-lvalue.cpp
af13471ff40bad3a26caa42e14f299e55f2c78b5a4cd1799b81b327c654daba1  fixtures/m4-controlflow-positive.cpp
74110b0ff56b77e50a471f766dcb98c3c035639702034522b9acc9a13f6609ff  fixtures/m4-controlflow-runtime-hazards.cpp
e65dfc0afd4269779b498f8ceef2715a14026a7fffc75d06d3d0224c04a0367a  fixtures/m4-controlflow-uninitialized-read.cpp
111a70fe32b54e5a7d1deca5d71a7fca845adc89263641ad42f98e2dccaf8029  fixtures/m4-controlflow-unknown-mutation.cpp
1994fa69351d6cb87916663dfcf8d613fabe2e9c7001d71207eb9262275a55cb  fixtures/m4-controlflow-unselected-invalid.cpp
```
