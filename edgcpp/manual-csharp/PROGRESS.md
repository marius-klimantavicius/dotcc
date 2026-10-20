# Manual frontend progress

## Campaign state

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
- Native compiler execution/profile validation: pending M0.
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
- Native implementation agent: `/root/manual_edg_supervisor/implementation`,
  `gpt-6.1-sol` with `xhigh` reasoning, started with a clean-context brief.
- Native review agent: `/root/manual_edg_supervisor/review`, `gpt-6.1-sol`
  with `xhigh` reasoning, started independently with the full review brief.

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
- M3: not started.
- M4: not started.
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
N1 remains a documented nonblocking medium test-coverage deferral as stated below.

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

## Final acceptance evidence

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
