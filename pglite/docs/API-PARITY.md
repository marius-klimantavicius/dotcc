# PGlite API mapping and qualification

The manually ported API follows `@electric-sql/pglite` 0.5.11, pinned in
[api-source.json](../config/api-source.json). The translated PostgreSQL engine
keeps its separate [source pin](../config/sources.json). The product executes
the translated backend and original translated initdb in process. JavaScript,
Wasm and native PostgreSQL are not delivered execution engines; the upstream
Wasm package is a test oracle only.

**The stated .NET API contract is qualified on Linux x64 LP64.** All current
raw/processed × JIT/normally trimmed NativeAOT rows pass the original 36 and 391
new real-engine API assertions each. The authoritative
[qualification receipt](../../docs/validation/pglite-api-port-20261023.json)
links exact tools, source hashes, commands and logs. Other OS/architecture targets
and omitted upstream mechanisms remain unqualified.

| Product | Execution | Original checks | API checks |
| --- | --- | --- | --- |
| Raw | JIT | 36 | 391 |
| Raw | Normally trimmed NativeAOT | 36 | 391 |
| Processed | JIT | 36 | 391 |
| Processed | Normally trimmed NativeAOT | 36 | 391 |

## Ledger and evidence

[api-parity.json](../config/api-parity.json) records upstream source/line,
precise managed mapping, implementation paths, existing test selectors,
upstream test sources, limitations and status. All 608 original inventory rows
were reviewed. The repaired source scanner currently reports 537 symbol rows;
71 mistaken member/type-continuation/helper rows remain separately documented
in `inventory_artifacts`. No managed member is invented to match those artifacts.
Constructor properties and exported object children not individually emitted by
the scanner are described in their owning type mappings.

`tests` references source checks; resolved evidence labels in the ledger point to
passing receipts. Empty tests mean no applicable behavioral check is recorded,
not an implicit pass. Source inventory establishes coverage only. The 341
`qualified-adaptation` rows qualify their stated managed contracts; 162
`platform-specific` rows record deliberate host/runtime choices, and 34
`platform-unavailable` rows identify absent contrib modules. Those statuses do
not prove omitted upstream behavior or every variant of a referenced type.

Current gates pass 185 wire/transport assertions and 237 codec assertions under
both JIT and NativeAOT, 1,833 assertions against 21 actual native transcripts,
22 pinned upstream oracle cases, 3,436 compiler tests, 39 affected functional
tests, 89 campaign framework tests and 34 API host-contract checks. All 14 engine
suites pass again. Ordinary Release solution build and no-argument Debug run
pass; the Debug run repeats all 36 + 391 checks. The separate example runs both
normally and from its framework-dependent publish output in `/tmp`.

## Export mappings

| Upstream surface | Managed API and contract |
| --- | --- |
| Root `PGlite` and interfaces | `PgliteDatabase.Create/Open`, `PgliteOptions`, synchronous owner-thread lifetime; `PgliteWorker` supplies an async caller queue. |
| Query, exec, descriptions | `Query`, `Exec`, `DescribeQuery`, `PgliteQueryResult`, `PgliteResultSet`, `PgliteQueryDescription`. |
| Types, codecs and arrays | `PgliteTypeOids`, independent session/query `PgliteCodecs`, `PgliteParameter`, `RefreshArrayTypes`. |
| `./template` | `PgliteTemplate.Sql/Query/Identifier/Raw`, immutable `PgliteSql.Text/Parameters`; `Join/Value` are managed helpers. |
| Protocol, parse and messages | `PgliteProtocol.Encode*`, incremental `PgliteProtocolDecoder`, `PgliteBackendMessage` and typed result payloads. |
| Transactions, callbacks | `Transaction`, scoped `PgliteTransaction`, `RunExclusive`, `Listen/Unlisten`, `OnNotification/OffNotification`, `OnNotice`. |
| `./live` | `PgliteDatabase.Live.Query/Changes/IncrementalQuery`, result/change records and disposable subscriptions. |
| `./worker` | Dedicated `Thread` through `PgliteWorker.Create/Open`, `WaitReady`, async convenience methods and `InvokeAsync`. |
| `./nodefs`, `./basefs`, root filesystem exports | `PgliteStorage.Persistent/Temporary`, real `System.IO` services, explicit checkpoint/archive/COPY operations; no Node/Emscripten filesystem facade. |
| `./opfs-ahp`, IndexedDB | Browser storage is unavailable; choose durable host files or disposable temporary host files. Temporary storage uses disk. |
| `./contrib/*` | All 34 exported descriptors are individually unavailable in the unchanged static engine profile. |

### Query and conversion differences

`Query(string)` retains the original managed text/error-returning contract.
Parameterized `Query` describes and binds values through PostgreSQL, returns
text `Rows` together with `TypedRows` and `TypedObjectRows`, and also returns SQL
errors. Call `EnsureSuccess()` to throw. `Exec`, transaction-token operations and
decoded protocol methods with their default `throwOnError` throw
`PgliteSqlException`; error results remain available on the exception.

`ResultSets` represents multiple statements. `Command` preserves the complete
command tag; `CommandName` matches the upstream first-word command.
`RowCount` is per statement; `AffectedRows` accumulates DML/COPY/MERGE counts
within that response. `Columns` contains name/OID plus table, attribute, length,
modifier and wire-format metadata. Object rows use the last duplicate column
name; live queries require unique aliases.

Both row projections are available without a `rowMode` flag. NULL bypasses codecs.
Integer widths follow CLR types; numeric values use lossless `PgliteNumeric`,
JSON uses owned `JsonElement`, bytea uses `byte[]`, and temporal values use
`DateOnly`, `DateTime`, `DateTimeOffset`, `TimeOnly`, `PgliteTimeWithTimeZone`
and `PgliteInterval`. Unrepresentable temporal values and unknown scalar OIDs
remain text. Nested arrays preserve NULL/quoted NULL, shape and delimiters;
PostgreSQL lower bounds normalize to nested arrays. Arbitrary POCO serialization
and reflection-based row mapping are not provided. Exact conversions and source
tests are in [SOURCE-MAPPING.txt](../tests/ApiCodecs/SOURCE-MAPPING.txt).

`FormatQuery` uses the server's described types and `quote_nullable`. It replaces
actual repeated/reordered `$n` positions while retaining lexical text, comments,
quoted identifiers and strings, including the session's
`standard_conforming_strings` choice. It requires an open database and performs
SQL round trips. Upstream pretty-printing is not reproduced. Prefer bound
parameters/templates for execution. `Raw` trusts its input; `Identifier` safely
doubles embedded quotes and can quote schema/name components separately.

### Configuration and lifetime choices

`PgliteOptions.Configuration` exposes `Username`, `Database`, `Debug`,
`RelaxedDurability` and `Settings`. Username selects the bootstrap superuser when
creating a cluster; opening uses the cluster's existing role without network
login. Database must already exist; `template1` is the default and `postgres`
also exists after initialization. Debug accepts 0..5. Relaxed durability sets
`synchronous_commit=off`; `Settings` can subsequently override that value.

Upstream `startParams` and `postgresqlconf` map to supported built-in settings,
passed as individual `-c` arguments. Arbitrary argv, config-file text/includes,
custom GUC names and profile-owned path/module/multiprocess settings are not
accepted. `initDbStartParams` is adapted to fixed C-locale/UTF8/trust-auth
initialization plus configurable username. Other initdb flags and ICU-data
injection are unsupported. Internal executable paths are not public choices.
`BootstrapPrefix` selects the translated assets. Stdout/stderr go to files under
`DiagnosticsDirectory`; stream interception/filter callbacks are unavailable.

Only one database owner may be open per process. Root operations, callbacks and
disposal stay on the creating thread. Root `WaitReady`/`DisposeAsync` complete
synchronously. Worker callbacks remain synchronous even though callers await
their queue tasks. Closing drains accepted worker items. Cancellation can skip
queued work; it cannot interrupt running PostgreSQL. Core shutdown failures keep
resources and exclusion quarantined.

### Storage, service and platform limits

Host storage is not a pluggable Emscripten filesystem or RAM filesystem.
`SyncToFs` performs a real idle-session CHECKPOINT; protocol `syncToFs` options
become an explicit call after a completed exchange. `DumpDataDir` produces tar
or gzip bytes/streams; callers choose filenames. Restore validates into private
staging and accepts only a new/empty target. Links, devices, external tablespaces,
unsafe paths and duplicate entries are rejected. Physical snapshots require the
same engine/platform disk format; they are not logical dumps.

There is no direct `Clone` wrapper. The supported adaptation is snapshot,
successful source close, then restore/open with options/extensions explicitly
reapplied. Simultaneous independent source/clone owners are unsupported.
`copyToFS` becomes ordinary caller `System.IO` for host files; do not modify
live PGDATA. `callMain`, external-command handlers and stream descriptors remain
private initialization mechanisms, not a public subprocess API.

`QueryOptions.blob`/`Results.blob` become explicit `CopyFrom`/`CopyTo` byte-array
or stream operations using real PostgreSQL COPY and a quoted private FILE path.
There is no automatic `/dev/blob` device or arbitrary-query substitution.
Raw COPY FROM must bundle query/Execute, CopyData and CopyDone/CopyFail because
the synchronous engine cannot suspend inside COPY. Protocol stream callbacks
receive owned chunks/messages after translated entry; exchanges are buffered,
without bounded-memory streaming or backpressure. Complete extended exchanges
with Sync before mixing high-level calls. Close owns Terminate.

Managed extension setup/initialize/close is synchronous; namespaces are retrieved
with `GetExtension<T>`. Dynamic native/Wasm bundles and arbitrary preload modules
are rejected. PL/pgSQL and Snowball are existing statically translated modules.
Live queries use read-only SQL and full local result comparison after successful
local commit. They do not track external writes, poll, install the upstream
trigger/view diff machinery or promise incremental SQL cost. Windows are sliced
in managed memory. Live cancellation is observed by the next owner operation;
it never calls the engine from a cancellation callback thread.

Wasm memory/modules/environment objects, socket startup/SSL/password/SASL/cancel
serializers, browser multi-tab leader election and `LeaderChangedError` have no
public equivalent here. Generic authentication/version-negotiation frames retain
raw payload bytes; this does not implement network authentication. Replication
message recognition does not deliver a replication server. See
[API-PLATFORMS.md](API-PLATFORMS.md) for detailed callback, ownership, archive,
worker and live contracts.

Unavailable contrib descriptors: `amcheck`, `auto_explain`, `bloom`, `btree_gin`,
`btree_gist`, `citext`, `cube`, `dict_int`, `dict_xsyn`, `earthdistance`, `file_fdw`,
`fuzzystrmatch`, `hstore`, `intarray`, `isn`, `lo`, `ltree`, `moddatetime`,
`pageinspect`, `pg_buffercache`, `pg_freespacemap`, `pg_stat_statements`,
`pg_surgery`, `pg_trgm`, `pg_visibility`, `pg_walinspect`, `pgcrypto`, `seg`,
`tablefunc`, `tcn`, `tsm_system_rows`, `tsm_system_time`, `unaccent`, `uuid_ossp`.
Their upstream cases remain linked individually in the ledger.

## Qualification sources and notices

The reusable actual-engine cases are
[PgliteApiCases](../tests/ApiAcceptance/PgliteApiCases.cs),
[CoreProtocolCases](../tests/ApiProtocolEngine/CoreProtocolCases.cs),
[PgliteCodecCases](../tests/ApiCodecs/PgliteCodecCases.cs) and
[ApiServicesCases](../tests/ApiServices/ApiServicesCases.cs). Protocol/codec
contract programs and the upstream differential oracle establish narrower
boundaries. Current acceptance and ordinary example execution are linked in the
qualification receipt. Code presence and source-only compilation alone are
not runtime acceptance.

The reusable API copies [upstream notices](../src/Api/Notices/ATTRIBUTION.txt),
including wrapper Apache-2.0, PostgreSQL and pg-protocol MIT notices. Protocol
package metadata separately declares Apache-2.0; the retained MIT license and
the postgres.js/Unlicense codec attribution are preserved explicitly.
