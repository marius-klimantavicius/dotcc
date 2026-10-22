# Manual port of the PGlite TypeScript API

Status: implementation and selected Linux x64 qualification complete. Updated 2026-10-23.

The user requested the TypeScript API be manually ported into the managed API
libraries after the compiler failures are fixed. This extends the previously
qualified embedded engine; it does not retroactively claim wrapper API parity.
Keep `src/Api/PGlite.csproj` as the reusable public library and
`samples/ManagedConsumer` as an ordinary consumer and acceptance harness.
Separate supporting libraries only where dependencies and reusable contracts
justify them. Do not put reusable API implementation in the sample.

## Start dependency and coordinator responsibility

Complete the active compiler repair task first:
`repair-baseline-compiler-failures-20261022-r1`. Its original 16 compiler failures
and framework metadata failures must be resolved, full suites green, and the
affected downstream behavior validated. Planning and source inventory can happen
before that gate; API implementation starts afterward. The parent coordinator
must automatically launch the API work when those results are accepted, without
requiring another user prompt. A partial repair report does not release the gate.

The gate was accepted at `ddc9faf08`: all 3,436 compiler tests and 88 framework
tests pass, with the original 16 assertions preserved and 99 selected semantic
checks passing. The parent verified all 12 evidence hashes in the
[repair receipt](../../docs/validation/compiler-framework-repair-20261023.json).
Downstream validation was scoped; the API qualification below must rerun the
complete PGlite consumer matrix with the repaired toolchain.

Use `gpt-6.1-sol` with `high` reasoning for the supervisor and `xhigh` for
implementation agents. Split independent work between multiple implementation
agents with explicit file ownership and coordinated tool builds. Follow the
completion responsibility in [baseline v4](../../docs/plans/translation-baseline.md):
integrate results, repair failures and continue through all required API gates.

## Source and intended compatibility

The upstream wrapper is [electric-sql/pglite](https://github.com/electric-sql/pglite).
The exact revision, package version and source locations are in
[`config/api-source.json`](../config/api-source.json). Preserve source licenses,
attribution and applicable notices for the manual port. Keep the current engine
pin separate; verify that wrapper assumptions match its exported hooks and
protocol behavior. Do not silently upgrade the PostgreSQL fork.

Acquire and inspect the pinned implementation, exports, protocol dependency and
tests. Build a symbol-by-symbol parity ledger mapping upstream behavior to .NET
API, implementation files, tests and status. Cover the public package API and
exported subpaths, rather than treating the current Simple Query wrapper as
sufficient. Track browser/WASM-only mechanisms separately: choose explicit .NET
equivalents where their behavior applies, and document platform-specific
exceptions with reasons. Do not silently omit an API or advertise stubs as parity.

The initial work areas are:

- Readiness, construction/configuration, shutdown and resource ownership.
- Parameterized queries and extended protocol, descriptions, result metadata,
  value codecs, custom codecs, arrays and safe SQL interpolation.
- Multi-statement execution, callback transactions, rollback and exclusive
  execution; preserve ordering, error recovery and transaction boundaries.
- Raw/decoded/streamed protocol access, notices and notification subscriptions.
- Data import/export, binary/blob/COPY behavior, storage abstractions and
  durability controls.
- Extension lifecycle, exported template/live functionality and the .NET
  equivalents or explicit platform decisions for worker/browser integration.

Use idiomatic C# types and async/disposal contracts while preserving observable
upstream behavior. JavaScript promises do not justify entering the translated
core concurrently or resuming a thread-bound owner on arbitrary pool threads.
Design serialized execution and callback ownership before adding async methods.
The core remains the actual translated engine; do not introduce JS/WASM execution
as the delivered implementation. An upstream JS/WASM build may serve as a test
oracle. Keep internal initialization command paths out of public configuration
unless callers have a meaningful supported choice.

## Implementation and acceptance

1. Freeze the source/dependency inventory, parity ledger and .NET contract.
   Resolve platform adaptations explicitly and derive cases from upstream tests.
2. Establish protocol and typed value support; then implement query, transaction,
   lifecycle and independent higher-level features in coordinated parallel work.
3. Exercise each API against the real translated engine. Compare applicable
   behavior with the pinned upstream wrapper and reuse its meaningful cases.
   Include parameter boundaries, NULL/type conversion, errors and recovery,
   transaction ownership, notifications and storage lifetimes.
4. Integrate the public project graph and consumer. Preserve the existing SQL,
   shutdown/reopen and ownership acceptance tests. Run the required API cases
   through raw/processed products, JIT and normally trimmed NativeAOT on the
   established Linux x64 target; record any additional target separately.
5. Complete the parity ledger with current evidence, reproduce the ordinary
   build/run workflow, document usage and explicit platform differences, and
   verify affected shared regressions. Implementation and qualification must
   both pass before declaring the manual API port complete.

The full pinned archive and all public package subpaths have been inspected.
`config/api-parity.json` currently inventories 537 exported symbols/members; its
per-symbol status remains authoritative and is not an automatic parity claim.
The protocol dependency path is `packages/pg-protocol`, version 0.0.4. Its package
metadata says Apache-2.0 while its retained LICENSE is MIT (Brian Carlson); both
are recorded, with the original notices delivered in the reusable library.

Three parallel gpt-6.1-sol xhigh implementation tasks delivered
protocol/query, codecs/templates, and lifecycle/services. A second core integration
round exercises the actual translated engine and hardens exchange ownership.
The supervisor owns source acquisition, contract review, examples, acceptance
integration, configuration, complete regeneration and final evidence. No child
commits or shared builds are permitted; the coordinator serializes those outputs.
Parent bootstrap staging fix `8e8bb390a` is preserved, including ordinary Debug
build/run and publish behavior.

The npm 0.5.11 differential oracle passes 22 upstream-derived parameter/type/array/
error/recovery cases. It is test-only. Final protocol contracts pass 185 assertions
under JIT and NativeAOT; codec contracts pass 237 in each mode. Retained actual
native transcripts pass 1,833 assertions. Current shared gates pass 3,436 compiler,
39 affected functional and 89 framework tests; API host contracts pass 34.

Full regeneration passed with accepted compiler fix `123b3e76e` using its
immutable frozen-v2 closure (receipt `20261023-003454-1ac22a3d`): all 928 selected
backend/module/libc/bridge units and 73 initializer/frontend units emitted and
linked. All 14 engine suites pass again (`20261023-010738-3673914b`).

All four current consumer rows pass the original 36 and 391 API assertions each
(`20261023-011914-08221e49`). Normally trimmed NativeAOT succeeds without new
native/trimming diagnostics; both executables depend only on libc, libm and the
Linux loader. Ordinary five-project Release build, no-argument Debug consumer,
normal/published JIT example execution from `/tmp`, layout check and offline
inventory with hashes warn/off pass. Every delivery checked contains 659 matching
bootstrap assets, descriptor mode 0700, and matching API notices.

Implementation commit `5c49e6924` and lexical review/fix commit `2c6471c7e` are
followed by final qualification evidence. Five bounded review findings have
real-engine regressions. The delegated final auditor was stopped by its provider;
no completed independent final audit is claimed. Supervisor review and the actual
matrix remain explicit. The strict UTF-8 encoder already rejects malformed
UTF-16 SQL; a fixture bypassing that encoder was a rejected hypothesis, and two
additional wire tests preserve the guarantee.

[API-PARITY.md](API-PARITY.md), [API-PLATFORMS.md](API-PLATFORMS.md) and
[API-USAGE.md](API-USAGE.md) describe the stated adapted contract, including the
conservative transaction-token guard, buffered protocol streams, host storage,
local live refresh and unavailable browser/contrib mechanisms. The
[final receipt](../../docs/validation/pglite-api-port-20261023.json) is authoritative.
Earlier failed/cancelled runs and engine-only P6 evidence remain historical.
