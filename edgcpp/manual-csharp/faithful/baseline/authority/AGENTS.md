# Faithful EDG port: controlling instructions

Read `PLAN.md`. The user corrected the campaign on 2026-10-21: implement a
faithful manual EDG-to-C# port with one-to-one method semantics and semantic
identity, not an independently designed C++ frontend based on EDG ideas.

CURRENT STATE: RESUMED by explicit user instruction on 2026-10-21. The user
revoked the prior stop for the new P0–P6 plan. Begin fresh-port P0 in faithful/;
do not resume the old condition/cast/binary80 queue. Prior restart records and
acceptance claims are historical; none qualifies the new port.

Default fresh implementation location: `faithful/` under this directory. Existing
prototype source/tests/worktrees/evidence are superseded and may be excluded
entirely. Do not delete or overwrite them during this planning reset. Do not
transplant the custom Parser/type/graph architecture. Reuse requires the same
full source-equivalence review as newly ported code.

Preserve EDG names, source-unit grouping, calls, branch predicates/order, state
transitions, diagnostics/recovery, symbol/type/IL identity, aliasing, ordering,
lifetime, target arithmetic and configuration behavior. Each method maps directly
to its original routine and every meaningful branch/operation. Document necessary
language transformations with equivalence arguments. No semantic simplification,
replacement algorithms, native frontend fallback, mechanical source translation,
generated EDG C# reuse or fixture shortcuts.

Unsafe C# is explicitly allowed: pointers, fixed buffers, explicit-layout unions,
native-style structures/globals/arenas and unmanaged storage. Fidelity and
side-by-side readability outrank idiomatic C#. Do not impose managed containers,
records, exceptions, new graphs or rational arithmetic on EDG. Preserve and review
source ownership, alias/lifetime and ABI contracts.

Do not fix suspected EDG bugs or import earlier standard-correct/native-defect
exceptions into the parity core. Preserve reproducible pinned behavior; flag
undefined/platform-sensitive uncertainty. Intentional semantic divergence needs
an explicit user decision and cannot count as exact parity. Prior root exceptions
do not authorize new-port deviations.

Root monitors the supervisor; supervisor (`gpt-6.1-sol` high) owns delegation,
communication, integration, serialized execution/Git and progress. Implementers
and independent method-by-method reviewers use `gpt-6.1-sol` xhigh. Parallel work
is authorized with disjoint ownership/dependency contracts. Track native/T3 tasks;
use a fresh delegated task for every T3 review round.

Reviewers compare original/produced methods branch by branch, including state,
identity, diagnostics and dependencies. Comments and passing tests are not enough.
Unexplained semantic differences block acceptance. Hold shared writers during
validation. Preserve provenance and Apache-2.0 WITH LLVM-exception attribution.
Do not modify upstream or unrelated emitter/exporter products to make tests pass.

After explicit resume, milestones are checkpoints; continue the full PLAN.
Unported/unsupported/unverified work is not complete. Explicit future user stop takes precedence. The previous stop is historical;
this explicit resume authorizes implementation, review and validation.
