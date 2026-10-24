# Embedded Npgsql supported surface

The copied library retains Npgsql v10.0.1 command, reader, parameter,
transaction, SQL parser, prepared-statement, catalog-loading and type-converter
machinery. New embedded files provide session leasing, worker transport and
engine policy. [NPGSQL-PLAN.md](NPGSQL-PLAN.md) records the agreed scope;
[NPGSQL-USAGE.md](NPGSQL-USAGE.md) describes consumption.

Qualification passed Linux x64 raw/processed JIT and normally trimmed NativeAOT:
5,151 assertions in 42 cases per row. The table defines the qualified acceptance
surface; commands and hashes are recorded in the final validation receipt.
Retained upstream source alone does not qualify every public member.

| Surface | Embedded contract and acceptance |
| --- | --- |
| Data source and connection | `NpgsqlEmbeddedDataSource.Create(options, create)`; deferred create/open, one exclusive physical session, sequential persistence reopens. |
| Commands | Sync/async `ExecuteNonQuery`, `ExecuteScalar`, `ExecuteReader`; actual extended protocol with typed parameters, no SQL reconstruction. |
| Readers | Typed getters, NULL, metadata/schema, multiple results, ordinary early disposal and sequential access over buffered engine responses. |
| Preparation | Real PostgreSQL preparation, repeated values, unprepare and reset after session close. |
| Batches | Upstream batch parser/state machine, typed parameters, multiple results, records affected and error recovery. |
| Transactions | Actual BEGIN/COMMIT/ROLLBACK, disposal rollback and session-close rollback; familiar Npgsql transaction wrappers. |
| Errors | Actual PostgreSQL SQLSTATE and message fields; ordinary protocol Sync recovery leaves the connection usable. |
| Types | Slim-builder built-in ADO codecs plus arrays; exact tested CLR/type variants are mapped in the acceptance source inventory. |
| Startup | Trusted local owner, real engine settings/version and Npgsql catalog bootstrap; no socket authentication or fabricated cancellation key. |
| Async ownership | Engine entry stays on one dedicated worker thread. Connector state transitions reject overlapping use of a connection. |
| Close/reopen | Fresh backend context resets session settings, temporary objects and prepared statements; committed caller-owned PGDATA persists. |
| Native oracle | Same-pin native PostgreSQL runs only in tests. It is not loaded, spawned or contacted by embedded product execution. |

## Explicit exclusions

| Feature | Deterministic behavior |
| --- | --- |
| Simultaneous connections | A second active lease fails; independently named data sources do not create independent engine owners. |
| Pooling/multiplexing | Disabled in the embedded factory and rejected if inconsistent embedded settings reach connector opening. There is no hidden shared-session pool. |
| Streaming COPY APIs | Import/export/raw streaming entrypoints throw `NotSupportedException` before taking command ownership. |
| SQL COPY STDIN/STDOUT | An owner-installed PostgreSQL utility hook rejects the actual streaming `CopyStmt` with SQLSTATE `0A000` before COPY starts, including dynamic/stored routines. |
| Ordinary file COPY | Remains engine SQL, subject to the existing filesystem/platform behavior. It is not streaming COPY through Npgsql. |
| Running cancellation | `Cancel()` throws; it does not open a cancellation socket or concurrently close the engine. |
| Live cancellable tokens | Unsupported command/reader operations reject them before advancing/executing; pre-cancelled tokens cancel before entry. Use `CancellationToken.None`. |
| Client command/batch timeout | Only zero (infinite) is accepted. A nonzero value fails instead of silently ignoring a deadline. |
| Notification waits | `Wait`/`WaitAsync` reject before ownership; a buffered exchange does not provide an unsolicited notification pump. |
| Connection reconfiguration | `ChangeDatabase`, connection-string reassignment and `CloneWith` variants reject before mutation. Ordinary `ICloneable.Clone` keeps the embedded source and its exclusive lease rule. Create another embedded source after closing the owner to change configuration. |
| Distributed enlistment | Manual and automatic ambient enlistment are disabled; local PostgreSQL transactions remain supported. |
| Result backpressure | The core response is buffered and bounded by `PgliteOptions.MaximumResponseBytes`; sequential reader methods do not promise bounded-memory streaming. |
| Dynamic type mapping/plugins | Reflection-dependent JSON/POCO/composite/plugin profiles are not enabled or qualified by the embedded Slim factory. |
| Server ecosystem | Replication, network authentication/TLS, EF Core, Dapper, multiple hosts and full-server PostgreSQL delivery are outside this qualification. |

The retained stock network connector is upstream source, not the embedded
execution path. Tests may use it as an independent native-oracle client; that
does not make a native server a product dependency or qualify the network mode.

The embedded hook changes only the Npgsql-owned worker session. The original
Managed.PGlite API retains its separate buffered COPY contract.

## Type qualification boundary

The acceptance cases cover `bool`, `short`, `int`, `long`, `float`, `double`,
`decimal`, `string`, `byte[]`, `Guid`, `DateOnly`, `TimeOnly`, unspecified
`DateTime` for timestamp, UTC `DateTime` for timestamptz, and month-free
`TimeSpan` intervals. All sixty upstream decimal vectors retain exact decimal
bits; overflow rejects without losing the following field. Array cases cover
integer, nullable integer, Boolean and text values, empty arrays, NULL elements
and multidimensional shape. Large values cross Npgsql's normal 8 KiB buffers.

Retaining other upstream converters does not qualify every conversion variant.
BigInteger, temporal infinities/out-of-range dates, timetz/DateTimeOffset,
month-bearing interval structures, JSON/XML, network/geometric/range types,
arbitrary enum/composite mapping and optional plugins are outside the tested
type matrix. Dynamic JSON/POCO/composite profiles are not enabled by this
factory. These omissions do not replace the original Npgsql codecs with a
homegrown conversion layer.

Tracing with an `ActivityListener` requesting all data is covered, using an
embedded tag and actual database metadata without network endpoint/PID tags.
Type reload rejects cancellation or a busy connection before changing the
working codec catalog. Managed extension cleanup failures are reported after
logical connection cleanup; only proven engine closure releases the lease.

Reproduction commands, review repairs and current matrix evidence are recorded
in [NPGSQL-QUALIFICATION.md](NPGSQL-QUALIFICATION.md) and the
[validation receipt](../../docs/validation/pglite-npgsql-20261024.json).
