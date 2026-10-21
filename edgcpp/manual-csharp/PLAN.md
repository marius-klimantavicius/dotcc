# Handwritten EDG frontend for .NET

## Objective and authority

Implement an extensible C++ parser and semantic analyzer entirely in authored
C#, using EDG's implementation and documentation as the behavioral reference.
The user explicitly selected a fully manual port. This means understanding and
handwriting the implementation, including its data structures and algorithms.
It does not mean generating C# from EDG, editing generated C#, porting generated
C output, or wrapping a native compiler as the product.

The product must eventually expose resolved declarations, symbols, types,
expressions, statements, template instances, diagnostics and source information
through an in-process .NET API. Code generation is a later consumer, not an
initial requirement. A successful initial slice is not completion of C++ support.

This is a separate campaign from the existing EDG C# emitter and relational
exporter. Those implementations, their artifacts and upstream sources must be
preserved. Their existing tests are not evidence for the new manual frontend.
The parent directory's exporter-only semantic-authority rules describe that
older campaign; this user request authorizes implementing semantics in C# here.
Follow EDG's semantics, and document any intentional implementation adaptation.

## Source baseline and traceability

- Source reference: `../upstream/src/`, with documentation in
  `../upstream/doc/source/`.
- Imported base: `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`, recorded in
  `../upstream-origin.json`. This working tree has subsequent changes; that
  identifier alone does not identify the exact files being ported.
- Record the actual SHA-256 of each consulted implementation file in a
  correspondence ledger. Record the native oracle executable hash, profile and
  arguments separately. Retain the corresponding source revision when available.
- Each implemented subsystem must identify the upstream routines, structures,
  invariants and relevant conditional branches it follows. Record omissions
  and tests. Do not claim a whole source file is ported because a few functions
  have counterparts.
- Preserve applicable upstream copyright and Apache-2.0 WITH LLVM-exception
  attribution for derived code and fixtures. Do not relabel EDG-derived files
  as MIT merely because the surrounding repository uses MIT.

## Architectural decisions

### Execution and dependencies

Use .NET 10 / C# 14 and support NativeAOT. Production analysis runs in process
without native EDG, libclang, compiler subprocesses, generated EDG assemblies,
the existing semantic exporter, or dynamic code generation. Native EDG is
allowed only in explicitly selected oracle/testing tools.

Start with a standalone project tree:

- `src/EdgSharp/`: handwritten frontend library.
- `tests/EdgSharp.Tests/`: deterministic unit and semantic integration tests.
- `tools/`: explicitly invoked native-oracle and inspection tools.
- `fixtures/`: source inputs and reviewed expectations with provenance.
- `docs/`: design decisions and subsystem correspondence.
- `evidence/`: small verification receipts and review records.
- `PROGRESS.md`: milestone status, exact checks, assignments and next steps.
- `SUPPORT.md`: implemented, partial, unsupported and unverified behavior.

Keep the new projects independently buildable initially. Reuse repository
package versions and build conventions where suitable. Do not edit shared
solution/build files concurrently or add them merely to make scaffolding look
integrated. Every new dependency needs an actual reason and AOT compatibility.

### State, graph and ownership

Use a compilation/session owner for mutable frontend state. Distinguish
compilation-wide identity and configuration from translation-unit state,
temporary parse state and immutable shared tables. Separate analysis completion
from disposal. Two simultaneous independent sessions must not share mutable
symbols, diagnostics, token cursors or template caches.

Use managed identity-bearing symbols, declarations and types, or typed IDs
where justified. Specify identity, canonicalization and structural equivalence
separately. Avoid record-generated structural equality for mutable graph nodes.
Keep compact tokens and source buffers in arrays; avoid per-character objects.
Use explicit variants for node payloads instead of replicating native unions
and allocator prefixes. Define copying versus aliasing at every mutable record
boundary. Keep public consumers from corrupting internal invariants.

The session must retain the information needed by promised queries, including
source/macro origins and retained template definitions. EDG can discard some
frontend symbol state before backend execution; the managed API must not inherit
that lifecycle accidentally. An incremental IDE engine is not an initial claim.

### Preserve semantic algorithms

EDG parses and analyzes together. Template instantiation uses token caches and
rescanning. Preserve these algorithmic choices initially. Do not replace them
with a new parse-all-then-bind architecture, generic grammar generator, or a
small independently invented C++ checker.

Preserve EDG's active/inactive/other symbol distinctions, ordering rules,
scope transitions, deferred work, speculative parsing and diagnostic suppression.
Host containers may change only when their required identity, ordering,
mutation and iteration behavior is explicit and tested.

Checkpoint/rollback helpers must restore token position, scope/lookup state,
pending work and diagnostics as required by the specific upstream algorithm.
C# exception unwinding does not automatically implement these state changes.

### Target semantics and API boundary

Represent target integer widths, signedness, floating formats, alignment and
layout explicitly. CLR arithmetic, `System.Type`, and C# overload resolution
are not implementations of C++ target arithmetic, types or overload resolution.
`BigInteger`, if used, still needs explicit target-width and overflow rules.

Expose a typed semantic result with diagnostics, declarations, resolved symbol
references, expression types/categories, selected conversions/callees, constant
values and structured bodies as those features are implemented. Queries must
state whether they inspect existing facts or trigger additional instantiation.
Do not force the result into dotcc's current C IR and lose C++ information.

## Scope and compatibility policy

The roadmap targets a useful modern C++ frontend, ultimately expanding toward
the pinned EDG feature surface. No initial subset is a replacement for that
goal. The first tested language profile is C++20 with an explicit LP64 target;
that is a staged test profile, not a claim of C++20 completeness or host ABI.
Keep target configuration separable so it can grow.

Begin with header-free inputs, then controlled include fixtures, then real
headers. Stage vendor extensions, modules, PCH, multi-TU processing and other
large subsystems explicitly. Record them as outstanding, not silently disabled
and counted as supported. Compiler command-line compatibility, native emitters,
runtime libraries and prelinking are not initial deliverables.

For input outside the implemented subset, produce a diagnostic identifying an
unsupported feature or syntax. Never silently skip bodies/declarations, create
success-shaped placeholder nodes, fall back to native EDG, or substitute fixture
answers. Keep unsupported-feature results distinct from diagnosed invalid C++.
Incomplete API functionality must be explicit rather than returning plausible
default semantic facts.

## Milestones and acceptance

### M0 — Baseline, contracts and test harness

Inspect relevant EDG routines, establish source hashes and native-oracle
availability, create the library/tests, and record initial API/data contracts.
Create support/correspondence ledgers. Verify a native header-free positive and
negative fixture with an explicit target/dialect profile.

Acceptance: reproducible build/test commands; independent session ownership
tests; named source references; native invocation receipts; no runtime native
dependency. Missing oracle availability is a reported verification gap, never a
silently skipped pass. Scaffolding alone does not finish the first assignment.

### M1 — Source handling, lexical state and preprocessing foundation

Handwrite source ranges, source-buffer ownership, token kinds, identifiers,
punctuation, comments, literals, lexical diagnostics and replayable token cursors,
following EDG lexical/preprocessor routines. Add controlled preprocessing:
object/function macros, expansion suppression/rescanning, includes and conditional
directives incrementally. Record each directive/operator still outstanding.

Acceptance: exact token spelling/kind/location fixtures; replay/checkpoint
restoration; EOF and malformed-input diagnostics; nested expansion and include
provenance tests for implemented features; native preprocessing comparisons.
Do not let a lexer-only milestone stand in for a semantic frontend.

### M2 — First complete semantic slice

Implement foundational types, declarations, lexical scopes, aliases and name
lookup; fundamental expressions, function declarations/definitions, parameters,
locals, calls and return statements. Maintain declaration identity, canonical
types, qualifiers and source correspondence. Support a small, documented set
of scalar conversions and diagnose the rest explicitly.

Suggested acceptance inputs include function declaration followed by definition,
an alias used in a declaration, a shadowed local in a nested block, an ordinary
resolved function call, and independent analyses of the same source text.
Negative cases include unresolved identifiers, conflicting declarations and
unsupported syntax embedded inside an otherwise supported function.

Acceptance: actual C++ text -> handwritten frontend -> typed resolved graph;
inspect selected declaration, result type and body relationships. Compare
acceptance/diagnostic behavior with native EDG and validate semantic facts with
reviewed native witnesses or an explicit test-only inspector. No emitter needed.

### M3 — Overload and class semantics

Grow conversion sequences/ranking, overload candidate collection, references,
member lookup, simple records/classes and member functions. Follow EDG rather
than selecting candidates from a heuristic score. Make value category and
implicit conversions explicit. Add constructors and initialization in reviewed
steps; distinguish declared support from tested support.

Acceptance: exact-match versus conversion selection, ambiguity, overloads across
scopes, reference binding, member lookup and matching invalid cases; inspect
selected callees and conversions. Review at least one adversarial case per
algorithmic branch added.

### M4 — Early difficult-path feasibility gate

Implement a bounded but real path through function-template declaration,
token retention/replay, deduction, instantiation, overload participation and
constant evaluation. Keep dependent entities and instantiation contexts explicit.
Exercise nondependent lookup at definition and dependent lookup at instantiation
as the relevant features arrive. Track constraints/SFINAE/partial specialization
as separate work rather than implying coverage from simple substitution.

Acceptance: header-free function-template deduction with distinct concrete
instances; template/non-template overload selection; declaration-context lookup
case; an instantiated constexpr call used in a constant-required context;
deduction failure and ambiguity/error controls. All facts must come from the
handwritten frontend. Document the precise supported forms and the next gaps.

This gate tests whether the chosen architecture survives actual C++ semantic
coupling. Do not spend a long campaign perfecting peripheral lexer features
before attempting it. It is not full template or constexpr support.

### M5 — Language breadth and realistic headers

Expand initialization, object lifetime facts, special members, inheritance,
virtual semantics, exceptions, templates, constraints/concepts and constexpr.
Add target layout and arithmetic coverage where each feature needs them.
Introduce header fixtures and selected real library headers with exact compiler
profile/macros. Add modules/vendor features according to the explicit backlog.

Acceptance: subsystem-specific upstream regressions, semantic differential
checks, malformed/negative coverage and real-header results. Record every
unsupported feature encountered rather than patching away source semantics.

### M6 — Library product and broader qualification

Stabilize the query API, independent/repeated session lifecycle, cancellation,
diagnostic recovery and resource bounds. Qualify NativeAOT and supported platforms.
Measure startup, throughput, allocations and retained graph memory against
representative inputs; optimize only with behavioral parity tests.

Acceptance: documented API examples, reproducible reference comparisons,
published coverage/exclusions, and explicit upstream-update procedure. Full EDG
compatibility requires a separate coverage audit; neither compiling nor a small
passing fixture corpus establishes it.

## Verification protocol

1. Unit tests verify invariants: identity, ordered lookup, checkpoint restoration,
   width-aware constants, ownership and source mapping.
2. Semantic integration tests start with source text and inspect actual results.
   Test both valid and invalid forms and unsupported input boundaries.
3. Native EDG comparisons use the same input bytes, include files, language mode,
   target and macro configuration. Retain executable/source hashes and commands.
4. Semantic comparisons use selected types/callees/conversions/constants/layouts,
   not raw pointer values or presumed byte equality of implementation graphs.
   Where direct inspection is unavailable, use reviewed discriminating C++
   witnesses such as static assertions. Report exactly what each proves.
5. An oracle adapter may call native EDG for tests; it must never answer runtime
   queries for the managed frontend. Default library/tests do not require EDG.
6. Run focused checks after changes, an integrated new-project suite at milestone
   boundaries, and a NativeAOT smoke once an executable probe exists. Serialize
   build/test/native preparation processes through the supervisor.
7. Record expected test inventory, passes, failures and skips. A missing tool,
   skipped case, unexecuted branch or placeholder is never a pass.

## Agent organization and review gates

The user explicitly requested this hierarchy:

- Root/monitor: owns this plan and campaign scope, monitors supervisor progress
  and resolves scope/architecture deviations.
- Supervisor/coordinator: `gpt-6.1-sol`, reasoning **high**. Owns assignment,
  interfaces, integration, validation scheduling, progress records and commits.
- Implementation agents: `gpt-6.1-sol`, reasoning **xhigh**. Write actual
  production code and implementation tests within disjoint owned files.
- Independent review agent: `gpt-6.1-sol`, reasoning **xhigh**. Reviews produced
  work and evidence against EDG and this plan; does not approve its own changes.

The supervisor must launch implementation and review agents, not implement the
frontend itself or merely return another plan. Root remains the monitor.
Respect actual concurrency limits: one implementation worker plus one reviewer
is sufficient alongside root and supervisor; add workers only when capacity
and disjoint contracts permit it.

The user explicitly authorizes multiple `gpt-6.1-sol` **xhigh** implementation
agents when work can run independently. The supervisor should use that capacity
with explicit file ownership and integration contracts. Native and T3-owned
children are both supervised work; retain delegated task IDs and do not create
top-level conversations for these tasks. A shared-checkout validation hold must
cover every implementation writer before recording hashes or running checks.
Independent review and serialized builds/tests/Git remain required.

Before implementation, the supervisor records file ownership and interface
contracts. A reviewer may first examine those contracts and upstream invariants,
then review the actual diff and executed tests. Every accepted code milestone
needs a review of the produced work, not just an early design review.

Review checks: fully manual implementation; fidelity to the named EDG routines;
aliasing/identity/order; parser-state rollback; diagnostics and invalid inputs;
numeric/target semantics; real semantic results; no native fallback; honest
support claims; AOT-compatible dependencies; maintainability and attribution.
Findings identify severity, affected code, reproduction and acceptance criteria.

Implementation workers fix findings; the reviewer rechecks changed code and
unresolved objections. No critical/high correctness finding may remain open at
acceptance. Track other findings and any justified deferral explicitly.
If T3 delegation is used, each review round is a new delegated task with the
original brief, previous findings/responses and unresolved objections.

Only the supervisor performs Git mutations, and only for owned, verified
milestones. Preserve unrelated work; do not reset the tree, overwrite existing
campaign files, delete retained evidence, or push/publish without user direction.
No worker silently changes scope, model, reasoning effort, or manual-only rule.

## Continuous execution and completion rule

The user explicitly instructed: "You must make sure that the plan is fully
implemented before stopping - do not stop on every milestone."

Milestones, reviews and commits are checkpoints, not stopping boundaries. Root
continues monitoring and the supervisor automatically gives implementation and
review agents their next ready assignments. Do not wait for the user to ask
whether agents are running or authorize continuation already covered here.
When a child finishes a bounded task, reassign it or launch the next required
child while preserving the requested models and independent review.

Continue until all M0-M6 requirements are implemented and validated against the
explicit feature inventory. Do not mark a partial milestone, unsupported feature,
unexecuted platform check or deferred review finding as complete. Maintain a
coverage inventory that accounts for the full stated plan; do not silently
reduce scope to make the campaign appear finished. A genuine blocker must be
reported with evidence and escalated to root while independent work continues.
Only full completion, an explicit user pause/stop, or an unavoidable external
blocker ends active campaign work. Progress reports do not end the campaign.

## Execution sequence

Start M0 and M1 immediately, using an implementation worker and independent
reviewer. Continue to an integrated M2 slice rather than stopping at project
creation. Advance into M3/M4 as dependencies allow, with reviewed checkpoints and
specific next assignments. The full roadmap remains active beyond those gates.

At each checkpoint report: implemented behavior, exact files, source correspondence,
executed commands/results, review findings and resolutions, commit if made,
remaining unsupported cases, and the next concrete task. Report blockers with
evidence; do not treat the size of the full port as a reason to stop useful work.
The supervisor issues the next bounded assignments automatically; root monitors
the full roadmap and intervenes on scope, correctness or execution failures.
