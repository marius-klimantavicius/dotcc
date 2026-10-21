# Faithful manual EDG-to-C# port

## Authority, reset and current execution state

The user's correction on 2026-10-21 is authoritative: this must be a **port of
EDG**, not a newly designed C++ frontend using EDG as a reference. Preserve
one-to-one method semantics and semantic identity. Unsafe code is explicitly
allowed. Idiomatic C# and managed abstractions are subordinate to fidelity and
side-by-side readability of the original and ported code.

**Execution RESUMED by explicit user instruction on 2026-10-21.** The user
revoked the prior stop for this new P0–P6 plan and authorized implementation,
review and validation beginning at P0, not the old condition/cast/binary80 queue.
The earlier planning-only stop is preserved as historical authority in
`faithful/coordination/RESUME-20261021.md`.

The default restart strategy is a fresh implementation in `faithful/` beneath
this directory. Existing `src/EdgSharp`, tests and isolated contributions are a
superseded prototype. They may be excluded entirely from the new implementation;
there is no obligation to retain their architecture, APIs, algorithms or test
expectations. Preserve the physical files and evidence for now. This planning
change does not delete, reset, move or overwrite existing implementation work.

The former plan is retained in
`docs/HISTORICAL-PLAN-before-parity-reset-20261021.md`. Previous milestone
acceptances, test counts and commit `2705ea856` describe the prototype only.
**No routine or subsystem is accepted under the new parity contract yet.**
`evidence/RESTART-20261021.md` and its manifests remain historical recovery records;
their instructions to continue the old implementation sequence are superseded.

## Objective and scope

Manually port the pinned EDG parser and semantic analyzer into in-process C#.
The original implementation is the implementation specification, not merely an
oracle. Understand each routine and handwrite its counterpart while preserving
its algorithm, dependencies, state and effects. No mechanical source translation,
generated EDG C# reuse, replacement parser, native frontend runtime fallback or
fixture-specific shortcuts.

Expose EDG's resolved declarations, symbols, types, expressions, templates,
diagnostics and source information to .NET consumers. A friendly facade can follow
qualification of the core; it must not force the original semantic graph into
an independently invented model. Code generation is outside the initial frontend
deliverable. Preserve the separate existing emitter/exporter and upstream files.

The full frontend remains required: preprocessing, declarations, lookup,
expressions, overloads, initialization, classes, templates, constexpr, layout and
other frontend facilities in the pinned inventory. Begin with a pinned profile,
initially strict C++20/LP64 where supported. This qualification boundary does not
authorize simplification of reachable semantics. Other dialect/configuration
branches remain explicit backlog. Do not quietly disable difficult enabled
branches to obtain a passing subset.

## Non-negotiable semantic parity contract

1. **Routine correspondence.** Each EDG semantic routine has an identifiable C#
   counterpart with equivalent inputs, outputs, state and side effects. Preserve
   source names, file grouping, parameter roles and locals where C# permits.
   Do not replace several routines with a smaller independently designed
   algorithm. Language-required splitting/helpers need explicit source-block
   mappings and independent equivalence review; they are exceptions.
2. **Control and state correspondence.** Preserve branch predicates and order,
   loops, call order, short-circuiting, deferred work, retries, lexical rescans,
   speculative state, rollback, recovery and diagnostic timing/suppression.
   Retain labels/gotos and explicit status propagation where appropriate. C#
   exception unwinding or a generic transaction is not automatically equivalent
   to EDG's state protocols.
3. **Semantic identity.** Preserve which symbols, types, declarations, IL nodes,
   caches and storage locations are the same entity and which are distinct.
   Preserve aliasing, canonicalization, linked order, sharing, mutation visibility
   and lifetime. Equal values or structurally equal graphs do not prove identity.
   Keep active/inactive/other-symbol and scope distinctions, not just lookup
   results on selected fixtures.
4. **Representation semantics.** Preserve discriminants, flags, union meanings,
   arithmetic widths, signedness, truncation, bit operations, rounding stages
   and storage rules wherever EDG observes them. A mathematically cleaner
   exact-rational algorithm is not a replacement for EDG's numerical algorithm.
   Host C# semantics are not automatically source or target semantics.
5. **Errors are behavior.** Preserve acceptance/rejection, semantic diagnostic
   conditions, order, severity, locations and recovery, including IDs/arguments
   where applicable. Enumerate formatting/host-path differences separately.
   Unsupported placeholders do not implement native invalid-input branches.
6. **No silent improvements.** Do not correct suspected EDG bugs, prefer a
   standard interpretation, change rounding, add stricter checks or import the
   prototype's native-defect exceptions into the parity core. Investigate a
   discrepancy as a port/configuration/instrumentation problem first. Preserve
   reproducible defined behavior of the pinned implementation, including quirks.
   Any intentional semantic divergence requires an explicit user decision and
   separate labeling; it cannot count as exact parity. Neither root nor supervisor
   may silently authorize it as an ordinary implementation adaptation.
7. **Undefined/platform-sensitive behavior.** Do not invent portable semantics
   from a crash, arbitrary address or undefined native operation. Identify the
   source contract, build flags and host assumptions; qualify the environment or
   record the unresolved blocker. A crash is not a passing language-negative test.
   Do not hide uncertainty behind a blanket exception policy.

## C# policy: fidelity before idiom

Use .NET 10 / C# 14; retain NativeAOT qualification. Unsafe C# is explicitly
authorized: pointers, pointer arithmetic, fixed buffers, explicit-layout structs
and unions, function pointers, pinned storage and native allocations may preserve
EDG more directly.

Do not require records, managed class hierarchies, dictionaries, tagged object
variants, LINQ, exceptions, BigInteger or per-session redesigns. Native-style
structures, flags, globals/context objects, linked lists, arenas and allocation
lifetimes are acceptable. Preserve source names and shape for side-by-side reading.

Unsafe is permitted, not mandatory. Record pointer widths, layout/alignment,
padding, bool/enum representation, ownership, allocation/free, pinning duration
and alias lifetimes. Map bitfields to explicit masks/accessors where necessary.
Keep source evaluation order and overflow behavior explicit. Do not introduce
memory corruption to mimic an unsupported host accident.

Port portability/library shims separately from semantics, with explicit native
contracts. Filesystem, memory or OS shims must not decide C++ semantics.
No P/Invoke to native EDG/libclang or production compiler subprocess. Native EDG
and test-only instrumentation are for verification only. Compare meaningful raw
layouts where needed; do not treat arbitrary padding/addresses as semantic data.

Concurrency, retained graphs after disposal and convenient queries must not drive
an early redesign of state or lifetimes. Preserve EDG first; add wrappers or
reviewed equivalent context lifting afterward. State initial restrictions honestly.

## Baseline and routine coverage ledger

Pin source/header/configuration/table hashes, provenance, build flags, native
compiler identity, ABI assumptions, feature macros, dialect and target before
implementation. The imported base revision alone does not identify the modified
local EDG sources. Keep upstream read-only and preserve applicable
Apache-2.0 WITH LLVM-exception attribution.

The fresh routine-level ledger must record:

- Original file, routine/macro/type, source range and exact source hash.
- Managed counterpart, parameters, return/out values and source-block mapping.
- Global/context/cache reads and writes; identity-bearing fields and aliases.
- Called routines, macros, allocation/cleanup and failure dependencies.
- Every branch/label/block and compile-time alternative, including untested paths.
- Representation/language-required adjustments and equivalence arguments.
- Implementation state, unresolved dependencies, tests/traces, reviewer verdict
  and the exact reviewed managed snapshot.

States distinguish unstarted, translated but dependency-blocked, review-pending,
reviewed with tests pending, and parity-qualified for a named configuration.
Do not mark partial routines complete. A branch excluded by configuration must
also be excluded in the pinned native build. Temporary stubs fail visibly and
cannot lie on a qualified path. Feature checklists cannot replace this ledger.

## Restart phases and acceptance

### P0 — Fresh baseline and dependency plan

Preserve the stopped prototype and inventories. After explicit restart, create
the separate `faithful/` project tree; do not transplant its semantic architecture.
Inventory actual EDG entry points, routines, structures, macros and dependencies.
Choose a dependency-closed port batch from the original call graph, not a desired
small C++ grammar. Review ABI/shim contracts and mapping format before acceptance.

Historical inputs, harness ideas and research can be reused only after inspection.
Any semantic code proposed for reuse receives the same one-to-one audit as fresh
code. Expectations based on deliberate native deviations are not parity tests.

Acceptance: pinned reproducible baseline, initial complete source inventory,
dependency plan, review template and fresh project without the custom frontend
architecture. Previous test totals provide no acceptance credit.

### P1 — Native data and runtime foundations

Translate required structures, discriminants, allocation/storage helpers, globals,
configuration, diagnostics and portability dependencies. Preserve identity,
aliasing, observed offsets, arena lifetimes and source state protocols. Do not
invent a replacement graph.

Acceptance: per-type/per-routine review, ABI/arithmetic and ownership tests, and
no unresolved helper on any claimed qualified execution path.

### P2 — Source and preprocessing call paths

Translate actual source-input, lexical/cache/rescan, macro, include and directive
routines with their branch structure and dependencies. Preserve physical/logical
source relationships and diagnostic timing. Do not build a small new lexer or
macro engine and then attach EDG references.

Acceptance: fully mapped routines per batch, side-by-side branch review, identical
input/configuration comparisons and identity/state traces where outputs alone
cannot establish equivalence. Remaining routines stay explicitly open.

### P3 — Declarations, lookup and expression semantics

Port EDG's interleaved declaration, declarator, disambiguation, scope, symbol,
type, expression, conversion and overload machinery. Carry dependencies across
source units instead of collapsing them into a subset Parser class. Bring in
class/initialization/layout paths when required by the original dependency graph.

Acceptance: review every routine in each claimed closure, including recovery and
diagnostics; compare semantic identities and selected entities, not merely
acceptance or constant results. Unsupported guards do not replace dependencies.

### P4 — Deferred and difficult paths

Port template retention/rescan, deduction, instantiation, deferred lookup,
constraints/SFINAE and constant interpretation through their native call paths.
Exercise a difficult coupled path early to test the source-shaped foundations.
Do not build a separate toy template or constexpr engine. Preserve original
storage, evaluation, failed-instantiation, cleanup and reuse behavior.

Acceptance: routine parity review plus cross-subsystem tests of dependency,
identity, failed/repeated instantiation and rollback. Do not indefinitely perfect
peripheral modules while deferring all difficult coupling.

### P5 — Complete the frontend inventory

Complete enabled routines/branches for declarations, classes, special members,
inheritance, exceptions, templates, constexpr, layout and remaining frontend
features. Expand configurations/dialects and controlled/real headers under
matching native profiles. Account for every source file and distinguish required
frontend dependencies from out-of-scope backend facilities.

Acceptance: full stated scope accounted for, upstream regressions and realistic
corpus differential results. Missing/unsupported routines cannot be hidden behind
milestone labels or passing fixture counts.

### P6 — .NET facade and broader qualification

Add a thin query/session facade over ported EDG data and lifecycle. Qualify
NativeAOT, supported hosts/targets, repeatability, resources and performance.
Concurrency, cancellation and graph retention require explicit source-compatible
boundary contracts; do not silently change internal semantics to provide them.

Acceptance: documented API, platform evidence, remaining-difference ledger,
upstream-update procedure and final independent source-parity audit.

## Mandatory method-by-method independent review

Reviewers must open the exact original EDG routine and actual C# counterpart.
A source comment, hash ledger, self-review or green suite is not enough. Report
correspondence of every branch and meaningful operation: predicates/order, calls,
writes, aliases, out values, allocation/cleanup, errors, recovery, effects and
configuration alternatives. Check that native branches were not replaced with
convenient Unsupported exits.

Audit callers/callees and state representation as well as method bodies. Locally
similar code with a changed contract is not equivalent. Review unsafe layout,
ownership and native arithmetic. Every unexplained semantic mismatch blocks
acceptance even when tests pass. If equivalence cannot be established, leave the
method unqualified and state the missing evidence; do not substitute confidence.

Use native/managed instrumentation as needed to compare stable logical IDs,
equality/alias partitions, graph edges, selected declarations, canonical types,
ordered candidates, conversions, lifetime transitions and diagnostics. Addresses
need not match across processes. Normalization must preserve relationships rather
than erase meaningful differences. Static assertions/output alone do not prove
internal identity.

Differential tests use identical source/include bytes and options. Keep failed
predictions, crashes, mismatches and missing evidence visible. Run integration
and JIT/NativeAOT checks on the exact held reviewed snapshot. Tests complement
source review; passing examples do not establish universal equivalence. Do not
claim a formal proof where only review and finite testing have been performed.

## Supervisor, workers and execution discipline

- Root monitors the supervisor for stalls, scope drift and premature stopping;
  root does not routinely assign implementation work or coordinate each repair.
- Supervisor: `gpt-6.1-sol`, **high**. Owns routine-based assignments, dependency
  closure, communication, ownership, integration, serialized builds, review
  follow-ups, task IDs, progress and verified commits.
- Implementers: `gpt-6.1-sol`, **xhigh**. Handwrite mapped routines and tests.
  Parallelize disjoint source units/dependencies under explicit contracts.
- Independent reviewers: `gpt-6.1-sol`, **xhigh**. Audit each method against EDG
  and actual produced bytes; never approve their own implementations.

Every assignment must carry this parity contract, not just a feature request.
Review starts with source mapping and continues on produced code. No agent may
trade fidelity for idiomatic C#, throughput or passing milestones. Intentional
semantic departures require an explicit user decision. Only the supervisor
mutates Git for verified owned work; no unrelated changes/publishing. Hold all
shared writers during validation. Every T3 review round gets a fresh delegated
task with earlier findings/responses and its own tracked ID.

After explicit resume, continue this full plan without stopping at milestones.
Report parity-qualified routines/branches, dependencies and next work, not
inherited prototype feature counts. An explicit stop/pause overrides continuation.
**The explicit 2026-10-21 user resume is now in force. Continue P0–P6; do not
stop at milestone checkpoints. A future explicit user stop overrides it.**
