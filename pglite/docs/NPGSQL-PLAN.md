# Embedded Npgsql implementation plan

This campaign follows translation-baseline version 4 (2026-10-22), adapted to
a copied managed source fork. The existing generated backend/initdb remain the
engine; no additional PostgreSQL translation is authorized. Supervisor:
`gpt-6.1-sol` high. Independent implementation agents: `gpt-6.1-sol` xhigh,
explicitly selected through T3 orchestration.

## Product and source boundary

Deliver a reusable library copied from Npgsql v10.0.1
(`12d792ac2163596385152dcea5c68ba99769731f`), retaining upstream commands,
readers, parameters, codecs, transactions and catalog discovery. Preserve the
upstream license and record acquisition hashes and changes. Normal builds use
checked-in source and project references, without fetching Npgsql source.
The engine stays pinned to `29b6424aa312ec3556247db069939006dd27f878`.

The intended workflow is an ordinary ADO.NET application creating/opening a
persistent embedded cluster, binding typed parameters, reading typed rows,
committing and rolling back transactions, then closing and reopening it.
`Npgsql` remains the public namespace; the fork assembly is
`Managed.PGlite.Npgsql`. Mixing this fork and stock Npgsql in one application is
outside the contract. Dapper and EF Core compatibility require separate tests.

Use `PgliteWorker` for thread affinity and raw PostgreSQL frontend/backend bytes
for transport. Do not reconstruct SQL from parameters or decode/re-encode typed
rows through the high-level PGlite API. Startup metadata and type catalog
discovery must come from the real engine. There is no socket authentication.

Only one physical session may be active per process. A connection owns its
worker while leased; closing must complete engine shutdown and retain caller
PGDATA. Reopening creates a fresh backend session, resetting transactions,
settings, prepared statements and temporary tables while preserving committed
data. No pooling or multiplexing is promised.

Streaming COPY, running-query cancellation, simultaneous sessions and bounded
memory result streaming are excluded. Unsupported calls/options must fail
deterministically before entering a blocking exchange and leave the engine
usable. No successful flags may suggest those features work. Qualify the exact
timeout and cancellation-token policy before documenting it as supported.

The target is Linux x64 LP64, .NET 10, little-endian host with standard
PostgreSQL wire encoding. Existing managed host/filesystem services remain in
use. Native PostgreSQL is permitted only as a test oracle; native PostgreSQL,
JavaScript, Wasm and socket proxies are forbidden product engines.

## Ownership and gates

The fork agent owns `src/Npgsql/**` and `config/npgsql-source.json`. The test
agent owns `tests/NpgsqlAcceptance/**`, `scripts/npgsql-*` and the upstream test
mapping. The supervisor owns shared integration, original API changes, sample,
solution, documentation, receipt, review and commits. Builds use distinct
artifacts/intermediate directories under `/mnt/secondary/dotcc-npgsql-20261024`.
Concurrent `s2n-tls/**` files are excluded from every edit and staging operation.

1. P0/P1: inspect release source and highest-risk startup/framing/ownership/AOT
   boundaries. Obtain a matching native control and reproducible provenance.
2. P2/P3: compile the copied library and execute useful parameterized work against
   the real translated core, including bootstrap, prepared statements,
   transactions, errors and clean reopen.
3. P4: qualify upstream-derived codecs and command/reader cases, large fragmented
   messages, multi-result/batch execution, metadata, early disposal, misuse and
   all explicit exclusions. Compare selected cases against native PostgreSQL.
4. P5: normal project-referenced sample, solution build, default run and publish
   from an unrelated working directory; bootstrap and notice asset audit.
5. P6: raw/processed × JIT/normally trimmed NativeAOT execution, original 36+391
   acceptance checks, relevant affected framework/host gates, independent final
   review and repairs, evidence receipt and coherent scoped commits.

Compiler, host and integration failures require classification, repair and retry.
A first SELECT, a successful child task or a build does not complete this plan.
Counts, commands, source/tool hashes, limitations and current outcomes belong in
the final validation receipt. Historical API receipts remain historical evidence,
not substitutes for acceptance of changed artifacts.

## Working library milestone

The copied library and ordinary solution/sample now build and execute against
the real translated engine. The sample passes typed insertion, commit/rollback,
prepared read and persistent reopen from `/tmp`, including framework-dependent
publish output. The processed JIT acceptance run passed 5,045 assertions in 36
cases; the same-pin native transcript control passed 101 assertions in 25 cases.
Focused probes additionally passed two uses of prepared 2 MiB Unicode text,
131,077-byte bytea and arrays, a twelve-command batch, savepoints and dynamic
COPY rejection/recovery. These are milestone results, not final acceptance.

Raw/processed NativeAOT execution, the complete current regression matrix and
independent final review remain required. The initial source-generator publish
property failure was repaired in its build-time project; a PublishAot-configured
managed build passes. This does not substitute for native executable execution.

The first independent review reproduced four defects: missing live GUC reports,
tracing's assumption of a network endpoint, swallowed embedded close failures,
and type reload rejecting after codec-state mutation. Each was repaired and
independently exercised through the actual engine. The current processed JIT
suite passes 5,144 assertions in 41 cases. A separate process probe exercises
response-limit quarantine through public options/SQL; it deliberately retains
the failed owner and PGDATA rather than permitting a false successful close.
Full native matrix acceptance is still required.
