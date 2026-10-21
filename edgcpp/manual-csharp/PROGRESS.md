# Manual frontend progress

## Campaign state

Resumed 2026-10-21 after accepted checkpoint `f27031ffc`. Implementation and
independent review agents are running for scalar free-function overloads
and then the early M4 gate. Prior agents are not running. Supervisor retains
validation/Git/ledger ownership; implementation owns `src/EdgSharp/`,
`tests/EdgSharp.Tests/`, `fixtures/` and standalone manifests. Reviewer owns
only new `evidence/review-m3-*.md` / `review-m4-*.md` reports and otherwise reads.

The next binding contract separates an ordered overload binding from canonical
function entity Symbols; compatible redeclarations still share one entity.
Candidates retain per-argument conversion descriptors; selection uses EDG's
pairwise standard-conversion comparison, never a summed score. N1's direct
definition-link/current-function checkpoint tests must close before activating
speculative/replay paths. Production remains fully handwritten and in process.
Per the explicit user instruction and PLAN's continuous execution rule,
checkpoints never stop the campaign. The complete actionable backlog is
`docs/FEATURE-INVENTORY.md`.

The first handwritten semantic checkpoint is implemented, independently reviewed
and accepted: M0 baseline, partial M1 lexical foundation, and a bounded integrated
M2 slice. R1–R7 are closed. Release build, 25/25 managed groups and 25/25 Linux
x64 NativeAOT groups pass with no failures or skips. The full frontend roadmap
remains active; preprocessing, overload/class semantics and templates/constexpr
are not completed by this checkpoint.

The full objective is a handwritten .NET C++ parser and semantic analyzer.
Milestones M0–M6 and acceptance criteria are defined in `PLAN.md`.

## Environment observed by root

- Workspace: `/home/marius/p/dotcc`
- Branch at launch: `sqlite`
- Working tree was clean before the plan was created.
- .NET SDK: `10.0.112`
- Native EDG path exists: `edgcpp/build/bin/cpfe`
- Native compiler execution/profile validation: accepted M0 receipts retained;
  resumed checks will record new receipt names rather than overwrite old evidence.
- Configured model available: `gpt-6.1-sol`, efforts `high` and `xhigh`.

## Ownership

- Root: `PLAN.md`, initial `AGENTS.md`, campaign monitoring.
- Supervisor: assignments, ongoing progress/support ledgers, integration and
  serialized validation/commits; record worker-owned paths before dispatch.
- Implementation worker: `src/EdgSharp/`, `tests/EdgSharp.Tests/`, `fixtures/`,
  and the standalone solution/project manifests below this directory. Authors
  all production frontend code and implementation tests; does not run builds
  or commit. Supplies source correspondence notes for the supervisor ledger.
- Independent reviewer: read-only source/code inspection and review reports in
  `evidence/review-*.md`; does not edit implementation or run concurrent builds.
- Supervisor additionally owns `SUPPORT.md`, `docs/`, `tools/`, and validation
  receipts under `evidence/` other than the assigned review reports.
- Current implementation agent: `/root/manual_edg_supervisor/implementation_m3`,
  `gpt-6.1-sol` with `xhigh` reasoning, started with a clean-context brief.
- Current review agent: `/root/manual_edg_supervisor/review_m3`, `gpt-6.1-sol`
  with `xhigh` reasoning, started independently with the full review brief.
- Prior checkpoint agents `/implementation` and `/review` are historical and
  not running. Their accepted review/evidence remains retained.

## Initial interface contract

- A compilation/session owns mutable symbols, canonical types, source buffers,
  token cursors and diagnostics; independent sessions share no mutable state.
- Analysis accepts source text and explicit C++20/LP64 options and returns a
  typed graph with source ranges, identity-bearing declarations/types, resolved
  references, structured bodies and distinct invalid/unsupported diagnostics.
- Parsing and semantic analysis are interleaved, following named EDG routines.
  Replayable tokens and checkpoints are retained for later template replay.
- The first integrated slice covers aliases, scalar declarations, declarations
  followed by definitions, lexical scopes/shadowing, ordinary calls and returns.
  Scalar conversions are explicit and target-aware; other forms are diagnosed.
- Production code has no native/compiler/generated-code dependencies. Tests
  run without EDG; the supervisor invokes EDG separately as an explicit oracle.

## Milestone state

- M0: accepted; source/native provenance, session/API contracts, independent
  ownership/concurrency tests, standalone build/harness and native controls.
- M1: partial accepted lexical/replay foundation; preprocessing and documented
  lexical-validation gaps remain unsupported/unqualified.
- M2: first bounded integrated semantic slice accepted; typed source graph,
  alias/function identity, scopes/calls/returns/conversions and negative recovery
  checked by 25 named managed+AOT groups, native fixtures and independent review.
- M3: scalar free-function overload checkpoint independently accepted; remaining
  references/classes/advanced-conversion/ADL branches stay open in the inventory.
- M4: early coupled gate independently accepted: real token replay/deduction/
  instances/frozen lookup/overload/constexpr source path. General templates,
  constraints, compound/dependent forms and full constexpr remain required gaps.
- M5: not started.
- M6: not started.

## Review and evidence

## Native baseline verification

The supervisor ran `tools/oracle.py` for `tools/oracle-inputs/profile.cpp`
(accept), `positive.cpp` (accept), `unresolved.cpp` (reject), `positive.cpp`
with `--preprocess` (accept), and `semantic-witness.cpp` (accept with explicit
additional-input hash for `positive.cpp`). Commands and executable/source
SHA-256 hashes, expected status and full output are in `evidence/native-*.json`.

The invocation uses `--clear_flag use_predefined_macro_file --c++20 --strict
--no_code_gen`; the built-in target is separately witnessed as LP64. The native
semantic witness proves alias/call result types and three integer conversion
results. These are native baseline facts only, not managed implementation passes.
`--help` was not supported by this compiler (exit 4); invocation flags were
established from local EDG command-line source/docs and successful executions.

`evidence/source-baseline.json` records exact hashes of the initial consulted
EDG source files and repository HEAD. Routine mappings and omissions remain to
be reconciled with produced code and independent review.

Review baseline identified parameter/body redeclaration scope, canonical
prototype/definition identity, public collection immutability, replay-state
boundaries and equal-width/different-rank integer conversions as required
adversarial checks. These were forwarded to implementation before acceptance.
First executed managed results and exact commands are recorded in
`evidence/validation-initial.md`; native exact-byte fixture receipts are
`evidence/native-m2-*.json`. These are checkpoint evidence, not final acceptance.

## Review issue tracking

Final status: R1–R7 are closed by produced code, assertions, executed final
managed/AOT validation and native controls. Details below retain the finding/
response history. `evidence/review-final-m0-m2.md` is the final acceptance record.
N1 was the initial nonblocking medium coverage deferral; the resumed M3
checkpoint closes it with direct managed/AOT nested-definition/function tests.

- R1, high if assignment uses `BinaryExpression`: the initial graph constructor
  makes all binary expressions prvalues, but built-in assignment yields an
  lvalue. Forwarded to implementation before the first build, with nested
  assignment `(x=1)=2` as an acceptance probe. Worker reports assignments are
  deliberately unsupported and adds an unsupported regression. Thus no accepted
  assignment semantics are claimed; produced-parser/test review remains pending.
- R2, high, independently confirmed: semantic checkpoint validity based only on
  undo length permits an abandoned branch checkpoint to restore a detached scope
  after divergent work recreates that length. Diagnostic truncation can also
  fail after partial mutation. Forwarded exact reproduction and required stale,
  divergent, nested/repeated and cross-state checkpoint tests. Fix/review pending;
  acceptance is blocked while this finding remains open.
- R3, high, independently confirmed: scalar function call prvalues retain the
  declared return's top-level `const`. Worker must retain declared function
  return qualification but remove it from scalar call/result expression types.
- R4, high, independently confirmed: `main` is treated as an ordinary scalar
  function, permitting invalid signatures and forbidden references. Worker must
  implement the relevant branch or explicitly diagnose global `main` unsupported.
- R5, high contract issue, independently confirmed: alternative operators,
  spaceship and function-designator decay/contextual-bool conversions can receive
  only invalid-source diagnostics despite being valid unimplemented forms.
  Worker must diagnose these missing branches as unsupported or implement them.
  R3–R5 were forwarded with exact source inputs and required regressions; fixes
  and independent recheck remain pending.
- Worker reports R2 fixed with registered checkpoint identity/lineage (including
  diagnostic-only branches), R3 with scalar call unqualification, R4 with global
  `main` explicitly unsupported, R5 with function decay unsupported/alternative
  operators implemented or explicitly unsupported. First integrated tests pass;
  independent final recheck remains outstanding.
- Worker audit: binary operands need explicit promotion nodes before conversion
  to a wider common type (for example short/bool + long). Fix and source-graph
  assertions are in progress. Preprocessing directives within a body also need
  explicit unsupported diagnostics. AOT publish is deferred until these fixes.
- R6, high contract issue: preprocessing inside declaration/parameter positions
  can evade a statement-only unsupported check. Worker now diagnoses Hash and
  HashHash centrally during `Analyze`, with three directive-position regressions;
  raw `Lex` retains its documented tokenization-only boundary.
- R7, high: a sole unnamed alias bound to void was incorrectly treated as one
  invalid parameter. Worker now follows the C++11+ EDG declarator branch and
  preserves zero-arity identity with the matching empty-parameter definition;
  named/mixed void remain invalid. Final review pending.
- R4 global-object hole fixed by explicit unsupported global variable `main`;
  local variables named `main` remain supported. Direct initialization through
  a parenthesized integer value is now explicitly unsupported rather than
  diagnosed as a malformed function parameter.

## Next concrete assignment after this checkpoint

Begin M3 with bounded by-value scalar free-function overload sets. A scope's
overload binding must be separate from each function entity's canonical Symbol.
Represent each candidate's argument conversion descriptors, collect viable
candidates and compare per-argument quality rather than summing heuristic scores.
Worker inspected `overload.c:add_function_to_candidate_functions_list`,
`determine_arg_match_level`, `compare_standard_conversions`,
`select_best_candidate_functions`, and `match_is_better_on_at_least_one_arg`.

Acceptance probes: exact versus conversion, short-to-int promotion versus
short-to-long conversion, indistinguishable integer conversions, crossed
multi-argument quality ambiguity, candidate-order independence, wrong arity,
local nonfunction hiding, and compatible prototype/definition identity. References,
classes/member functions/user-defined conversions/ADL remain later M3 branches.
Proceed to the early M4 retained-token deduction/instantiation gate without
perfecting peripheral lexing first. This is a proposed next bounded assignment,
not an implemented or accepted overload/template claim.

Early M4 source inspection identifies `templates.c:cache_function_template_body`,
`function_prototype_instantiation`, `instantiate_template_function_full`,
`wrapup_function_template_argument_deduction` and the overload deduction pair
routines. Preserve actual token-cache rescanning and a definition-context
nondependent binding/order snapshot; final scope dictionaries must not expose
later declarations or caller locals during replay. First forms can be
`template<class T> T id(T)` and repeated-T deduction failure, with distinct
canonical argument-keyed instances and non-template preference on equal
conversions. Read-only next-gate source hashes are separate in
`evidence/source-next-gates.json`; none of those subsystems is implemented yet.
Constant-required evaluation must add real explicit call/work state following
`interpret.c`, not claim existing optional integer folding is constexpr.

Reviewer nonblocking validation limit: SetDefinition and CurrentFunction
checkpoint restoration are source-inspected but not directly exercised by the
current helper tests. Add those branch tests before activating speculation or
template replay; no production parser branch currently uses the helper.

## Initial checkpoint acceptance evidence

- `dotnet build EdgSharp.slnx -c Release --nologo`: exit 0, no warnings/errors.
- `dotnet run --project tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release
  --no-build`: 25 named groups passed, 0 failed, 0 skipped; no native tool needed.
- `dotnet publish tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release -r
  linux-x64 -p:PublishAot=true -o tests/EdgSharp.Tests/bin/aot-smoke --nologo`:
  exit 0; published executable runs the same 25/25 groups successfully.
- Five managed/AOT receipts (`managed-build/tests/positive-inspect.json` and
  `nativeaot-publish/tests.json`) identify the same 16-file source/test/fixture
  snapshot; three retained build artifacts have size/SHA receipts.
- Seventeen native receipts match source/compiler hashes and expectations.
  Exact-byte M2 positives/negatives and valid-managed-unsupported boundary are
  explicit. Native static assertions establish selected alias/call/function types,
  not pointer identity or presumed graph-byte equality.
- Twenty-two baseline upstream source hashes match the reviewed files;
  next-gate read-only sources have a separate four-file receipt.
- Independent reviewer `/root/manual_edg_supervisor/review`,
  `gpt-6.1-sol`/`xhigh`, reviewed actual produced code, fixes, assertions and
  executed receipts and approved only this bounded checkpoint. It does not
  approve full M1, complete C++/EDG, M3/M4 or M6.

Git record: the supervisor's initial handwritten semantic checkpoint commit
contains this ledger and reviewed snapshot; its exact identifier is reported at
handoff and is discoverable with `git log --oneline -- edgcpp/manual-csharp`.

## Resumed scalar M3 checkpoint — 2026-10-21

The implementation worker authored ordered NameBinding/entity separation,
source-visible overload snapshots, viable/rejected candidate facts, scalar
exact/promotion/conversion descriptors and pairwise selection following the
named EDG overload routines. No width/distance/signedness/bool/declaration-order
heuristic is used. Compatible signatures retain canonical Symbol identity;
nearest nonfunction bindings hide outer overloads. Public single-symbol queries
never silently choose the first overload. New owner-mediated overload membership
rollback and N1 preexisting definition/current-function restoration are tested.

Supervisor executed, serially:

- Release solution build: 0 warnings/errors (`m3-build-initial.json`).
- Managed harness: 35/35 named groups, 0 failures/skips (`m3-tests-initial.json`).
  Inventory is prior 25 + N1 direct checkpoint group + nine M3 semantic groups.
- Linux x64 NativeAOT publish to separate retained `bin/aot-m3` output and its
  executable: 35/35 groups, 0 failures/skips (`m3-nativeaot-*-initial.json`).
- Six exact M3 fixtures: two native accepts, four actual language rejections;
  all expectations matched. Distinct-return included-fixture static assertions
  discriminate exact int/long and short/bool/ushort promotion; separate native
  staged lookup assertions discriminate before/after visibility.
- Four build/test receipts have the same 23-file source/test/fixture SHA map,
  matching held files; eight native receipts and three M3 artifact hashes match.
  Initial checkpoint artifact bytes were relocated to ignored retained storage
  before default outputs changed (`retained-f27031ffc.json`).

Independent reviewer `/root/manual_edg_supervisor/review_m3` inspected actual
production/test code, all assertions/fixtures, EDG algorithm branches and matching
executed receipts. `evidence/review-m3-final.md` approves only bounded scalar M3,
closes N1, and records no identified critical/high blocker. Full M3/M4–M6 remain
outstanding. Source mapping/adaptations are in `docs/OVERLOADS.md`.

Next worker assignment is already active: actual early M4 template declarations,
dependent representation, definition-time checks (including unused templates),
body token retention/rescan/deduction/instances, frozen definition lookup, overload
participation, explicit constexpr execution and combined constant-required source
test. Supervisor commits this checkpoint then immediately releases source edits;
reviewer is reassigned to M4 independently. No user reconfirmation or milestone
stopping boundary applies.

## Active M4 implementation and review

Current status: bounded gate accepted in `evidence/review-m4-final.md` after
48/48 managed and Linux x64 NativeAOT groups, zero failures/skips, and the native
used-profile controls. Canonical M4-01..13 findings are closed. Older provisional
R numbers in the implementation history below map explicitly in
`docs/TEMPLATES-AND-CONSTANTS.md`; they are not remaining open blockers.

Worker `/root/manual_edg_supervisor/implementation_m3` and independent reviewer
`/root/manual_edg_supervisor/review_m3` remain active at their requested models/
efforts for the next M4 assignment; the scalar M3 checkpoint commit is
`8b32f7c60a18184476248e759c085b5a86e4588c`. Actual template/dependent-type/cache/
constexpr implementation is underway; no M4 acceptance has occurred.

The combined native contract and losing-template-body controls accept under
strict C++20; unused-template nondependent-name/call errors and crossed
ordinary/template candidates reject with actual language exit 2. Receipts are
`native-m4-*-contract.json`. They define behavioral controls, not managed passes.
The native adapter now requires exact normal/error statuses; catastrophe/command
failure/signal never matches a negative. Four adapter unit controls pass, with
source mapping/evidence in `evidence/oracle-status-control.md`.

Open M4-R1, high ownership blocker, independently source-confirmed: session-lived
FunctionType interning captures TemplateParameterType.ParameterScope and thus
prior TU/source/cache graphs across repeated Analyze calls. Worker must scope
dependent signature identity/canonicalization to analysis/template lifetime while
retaining immutable primitive/concrete session canonicalization. Required direct
non-inlined weak-reference collection and repeated-analysis isolation tests keep
the session alive; M6 inventory records broader repeated-template memory work.

Worker source fix for M4-R1 now keeps dependent/scope-owning function signatures
out of session interning; direct collection/isolation tests remain required.
M4-R2 template-parameter redeclaration checks cover initial signature/body and
must also cover the function's own name after popping parameter scope.
M4-R3 separates expression type-dependency from value-dependency: a known bool
result of `!dependent` or concrete target conversion still cannot be evaluated
at prototype time. Reviewer also requires shared replay/checkpoint lineage or an
explicit child-checkpoint exclusion until supported; old N1 does not cover new
mutation states automatically. These are active produced-code review findings,
not accepted behavior.

Native profile correction: external template instantiation defaults to none in
EDG, so the initial selected-false-assert expected-rejection probe returned 0.
That failed expectation is retained, excluded from pass counts, and explained in
`oracle-status-control.md`. The adapter now explicitly selects `--instantiate
used`; selected false assertion and selected invalid dependent return reject,
while unused false assertion and losing-template body accept. The combined gate
and all nondependent/error controls match under this used profile. Seven adapter
unit controls plus explicit native integration controls guard the profile; raw
acceptance with default-none must never prove selected-body semantics.
