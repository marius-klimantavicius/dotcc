# Selected product qualification

This resolves the provisional test-scope decision before P4. The product remains
one serialized embedded owner on Linux x64 LP64, using actual translated
PostgreSQL and in-process initialization. It excludes the full server, extensions,
concurrent instances and cross-profile file interchange as already agreed in
PLAN.md. No required managed workflow or matrix row is deferred.

The canonical case inventory is `tests/native/cases.json`: 17 initial-lifetime
assertions, three first-reopen assertions and one second-reopen assertion. Both
native and managed runners must read the same inventory and check exact SQL
rows (including UTF-8 and NULL), command completions, SQLSTATE and transaction
state. They must reject malformed/truncated protocol and incorrect expected
rows. Managed reopen cycles run in the same process, with clean upstream
shutdown, callback draining and a fresh owned generated state for each lifetime.
Native controls remain independent evidence, never product implementation.

Required matrix: raw and processed libraries, each through JIT and a normally
trimmed separate NativeAOT consumer, on Linux x64. Every row initializes its own
directory, executes all 21 assertions and closes every lifetime. A build, help
command or missing-product rejection does not satisfy any row. The root solution
and ordinary project references must also build. Authored owning API and sample
sources remain outside generated output and survive regeneration.

Boundary qualification includes the real PostgreSQL-header ABI comparison,
callback width/sign/root lifetime, nested error jumps, resumable 99/100 yields
versus terminal exit, old signal action and masks, owned timer delivery/cancel,
real atomic/spinlock contention, native/emitted software CRC32C/popcount, and
required descriptor/mapping cleanup. PostgreSQL transaction/error cleanup stays
translated. Cooperative timer delivery must be wired to actual translated safe
points and must implement all paths required by this serialized workflow;
enabling a host timer without those connections is insufficient.

The selected SQL workload is checked against the same-revision native core.
The full upstream SQL regression runner, unrelated extension/platform cases,
custom fault injection, fuzzing, extended stress and performance thresholds are
outside this first product's scope. This follows the existing exclusion of broad
upstream regression qualification; it does not exclude ordinary SQL errors,
required rejection behavior, transaction cleanup or clean persistence lifetimes.
The actual upstream C atomic/spinlock algorithms and headers are exercised by
the boundary suites. Any additional required source defect gets a meaningful
shared regression and full-input retry.

P6 regenerates with final tools, reruns these suites and the full required
consumer matrix, checks offline warn/off provenance behavior and invocation from
an unrelated directory, audits imports/dependencies, and records current exact
commands, assertions, logs and receipts. Baseline failures already reproduced
on untouched sources remain separately identified. Historical receipts cannot
qualify changed artifacts.
