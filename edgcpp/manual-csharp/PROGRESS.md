# Manual frontend progress

## Campaign state

Active work: remaining M1 character literals/PP character operands and predefined
operators after accepted integrated preprocessing (`review-m1-final.md`, 60/60
managed and Linux x64 NativeAOT groups). Implementation and
independent review agents remain running on this next assignment. Supervisor retains
validation/Git/ledger ownership; implementation owns `src/EdgSharp/`,
`tests/EdgSharp.Tests/`, `fixtures/` and standalone manifests. Reviewer owns
only new `evidence/review-*.md` reports and otherwise reads.

Accepted scalar overloads separate ordered name bindings from canonical entities;
early template/constexpr paths preserve cached-token rescanning and explicit
evaluation. N1 direct definition-link/current-function checkpoint tests are closed.
Current preprocessing contracts separate physical spelling, expansion/include
history and presumed locations; native-only controls are not managed acceptance.
Production remains fully handwritten and in process.
Per the explicit user instruction and PLAN's continuous execution rule,
checkpoints never stop the campaign. The complete actionable backlog is
`docs/FEATURE-INVENTORY.md`.

Earlier accepted checkpoints are initial M0/partial M1/integrated scalar M2
`f27031ffc` (25 groups) and scalar M3 `8b32f7c60` (35 groups). Their receipts and
reviews remain retained. General preprocessing, overload/class semantics,
templates/constexpr and product qualification remain outstanding beyond the
precise accepted bounds.

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
- M1: active integrated preprocessing/provenance implementation after accepted
  lexical/replay foundation; all remaining lexical/PP inventory gaps stay required.
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

## Historical next assignment after the initial checkpoint

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

## M4 implementation and review history (checkpoint accepted)

Current status: bounded gate accepted in `evidence/review-m4-final.md` after
48/48 managed and Linux x64 NativeAOT groups, zero failures/skips, and the native
used-profile controls. Canonical M4-01..13 findings are closed. Older provisional
R numbers in the implementation history below map explicitly in
`docs/TEMPLATES-AND-CONSTANTS.md`; they are not remaining open blockers.

Historical dispatch: worker `/root/manual_edg_supervisor/implementation_m3` and independent reviewer
`/root/manual_edg_supervisor/review_m3` remain active at their requested models/
efforts for the next M4 assignment; the scalar M3 checkpoint commit is
`8b32f7c60a18184476248e759c085b5a86e4588c`. Actual template/dependent-type/cache/
constexpr implementation was underway at that point; final acceptance and
canonical finding closure are recorded at the start of this section.

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

## Active M1 assignment after accepted M4 checkpoint

M4 commit: `eadd29a6efcb2565ddf359463c387c6d3fa20fb8`. Implementation source holds
were released immediately after the owned verified commit. The same worker and
independent reviewer now implement/review integrated source preprocessing; there
is no milestone stop. Their existing disjoint ownership and no-worker-build/Git
rules remain in force; reviewer reports are `evidence/review-m1-*.md`.

Architecture contract: SourceRange stays one immutable buffer. Physical spelling
contributors, macro invocation/argument chains, include activations and presumed
logical #line locations are separate retained facts. Changed streams use an
explicit presentation source buffer and origin map; authoritative PP token
identity is never accidentally retokenized by serialization. AnalysisResult.Source
remains primary physical source; token/template buffers may be presentation
sources. Diagnostics expose logical primary location, physical anchors and notes.
Include search uses physical including directory plus configured ordered roots,
not presumed #line names; provider/search/state/limits are explicit and per
analysis. Guarded re-entry is not rejected as a blanket include cycle.

The macro algorithm uses input/replacement frames with caller-tail visibility,
raw versus expanded arguments and persistent suppression/inertness, following
EDG's strict branch. Comments/line splicing/logical BOL, keyword-eligible PP
identifiers, #/##/placemarkers/variadics, short-circuit target-width #if and
include/#line/predefined origins have direct native/graph acceptance probes.
Remaining M1/M2/M3/general M4/M5/M6 rows stay visible until actually implemented
and validated; this assignment cannot be called full-plan completion.

M1 native contracts executed: eight exact source/header inputs with explicit used
instantiation, strict C++20 and preprocessing-only where needed. Six accept and
two language-status-2 reject; macro, multiline and include-tail outputs are additionally
asserted by `tools/test-oracle-preprocessing.py`. Receipts are
`oracle-m1-pp-control-round3-*.json` and `oracle-m1-preprocessing-controls-round3.json`.
Eight adapter profile/status/configuration controls pass in
`oracle-m1-adapter-tests-round1.json`. No managed M1 pass is claimed yet.
Observed profile facts: include EOF is a macro-lookahead barrier; a spliced
logical line's `__LINE__` uses its final physical line, and a multiline argument's
`__LINE__` uses the invocation's closing-parenthesis line, while spelling locations
remain distinct. Actual consulted source/document hashes are retained in
`source-m1-preprocessing-round2.json` (including shift-folding correspondence).
Accepted M4 output bytes are retained and verified
before M1 rebuilds in `retained-eadd29a6e.json`.

Independent produced-code review is active in `review-m1-round1.md`, canonical
M1-01..20: half-open/clipped source maps, slash-root include lookup, eager
variadic prescan, lost nested expansion ancestry, stringized VA_OPT, composite
range histories, inactive/unused literal diagnostic timing, macro-redefinition
parameter spelling/original identity, VA_OPT operand paste edges, required
object-macro whitespace, ordinary #line filename escapes, multiline-comment
directive grouping, target-width signed shifts, alternative-operator macro
eligibility before classification, strict-C++20 #warning rejection, replacement
whitespace, phase-2-created comment delimiters, raw-closure qualification and
nested VA_OPT substitution/paste ordering. Source repairs
are under inspection; no finding is closed merely because a patch is visible.
Direct regressions and serialized managed/AOT/native evidence remain pending.

Pinned native #line anomaly: L-prefixed filenames reject, but u8/u/U/R forms
accept and retain prefix fragments in the resulting __FILE__ value. Exact
per-prefix receipts are `native-m1-line-*-filename-contract-round1.json`.
The earlier combined expected-reject receipt
`native-m1-line-other-prefixed-filenames-contract-round1.json` returned 0,
is retained as a disproved expectation and is excluded from pass counts.
Ordinary escaped filename decoding is required now; nonordinary compatibility
remains explicitly bounded/unimplemented until deliberately qualified.

Additional native profile correction: an alternative operator spelling can be
defined as a macro in this pinned EDG and expands before operator classification
(`native-m1-operator-macro-expansion-contract-round1.json`). The assumed-reject
unused-definition receipt returned 0 and is retained/excluded from pass counts.
Strict-C++20 #warning rejects with language status 2, rather than accepting a
warning. Worker now prioritizes actual session wiring and adversarial/integrated
test groups, followed by the first serialized managed build/test cycle; oracle
exploration alone does not advance managed acceptance.

Early integrated execution: `m1-build-round1.json` and `m1-tests-round1.json`
hold identical 44-file maps; Release has zero warnings/errors and all 56 managed
groups pass, zero failures/skips. This is early evidence, not final M1 acceptance.
Exact simple integrated fixture/header, three PP fixtures and a native included
type/value witness also pass (`native-m1-*-fixture-round1.json`,
`native-m1-semantic-witness-round1.json`). Source edits are released for the full
upstream VA_OPT matrix (32 invocation labels, preserving all source/options),
empty/EOF/splice-boundary maps, paste/stringize contributor assertions, repeated
include remaps, conflicting concurrent PP sessions and a changed-stream M4
difficult-path fixture. Final held managed/AOT/native/code review is pending.

Raw-control correction: the no-newline malformed-unused-raw rejection diagnosed
EOF newline policy only. The newline-terminated unused form is native-accepted;
its expected-reject receipt is retained/excluded. M1-19 is a local active raw
closure invariant plus an explicit unused-definition compatibility qualification,
not proof EDG rejects that unused macro. No stronger claim is accepted.

Final integrated preprocessing acceptance: `review-m1-final.md` independently
closes M1-01..18/20/21 within tested bounds and explicitly qualifies M1-19's
native-accepted unused raw Unsupported gap. Four round-3 receipts match identical
47-file held maps and current bytes; Release zero warnings/errors and 60/60 JIT
plus Linux x64 NativeAOT groups, zero failures/skips. Three artifact fingerprints,
17 consulted source hashes, exact fixture/witness inputs, full upstream matrix,
and final adapter/profile/status controls are independently audited. This is a
bounded checkpoint, not full M1 or campaign completion.

Next automatic assignment: target-aware character literal decoding/graph values
and PP character operands (`literals.c:conv_single_char/conv_char_literal`,
expr/preproc controlling expressions), deterministic standard date/time/hosted
predefined values and configured `__has_include`. Unicode/UCN, complete
prefixed/raw/adjacent string/floating/UDL semantics, filesystem encoding/include
provider, then remaining references/classes/general templates and M5/M6 remain
required. Supervisor commits this verified checkpoint and immediately releases
source edits; no root/user approval or stopping boundary applies.
