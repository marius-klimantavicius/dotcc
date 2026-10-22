# Managed PGlite service contracts

The manual API port follows `@electric-sql/pglite` 0.5.11 at the revision in
`config/api-source.json`. It uses the existing translated PostgreSQL backend and
original translated initdb. It does not change the engine pin or execute a
JavaScript, Wasm, native PostgreSQL, or subprocess database engine.

These are .NET adaptations of the upstream services. Runtime qualification is
owned by the supervisor's API matrix; implementation alone is not parity evidence.

`PgliteDatabase.Create/Open` are synchronous. They return after an actual SQL
readiness round trip and managed extension initialization. `Ready`, `Closed`,
`WaitReady`, `Close`, `Dispose` and `DisposeAsync` describe that same lifetime.
`WaitReady` and database async disposal complete synchronously on the owner
thread. They never schedule translated entry onto a pool thread. One database
owner is permitted per process; closing and reopening are supported sequentially.
A failed core shutdown keeps its resources/exclusion lease quarantined.

## Transactions and callbacks

`Transaction<T>(Func<PgliteTransaction,T>)` and its synchronous action overload
issue real BEGIN/COMMIT/ROLLBACK. The token exposes query, SQL template, exec,
listen, unlisten and rollback. SQL failures throw; a caught failure cannot silently
commit an aborted transaction. Callback failure rolls back. Explicit rollback
allows the callback to return a value. The token closes on every scope exit and
rejects later calls, nested ownership and direct database access during its
callback. Transaction-control SQL belongs to the wrapper and is rejected in token
query/exec/template text; token rollback uses `Rollback()`. The preflight scanner
handles comments, quoted text, dollar quoting and the actual
`standard_conforming_strings` setting; PostgreSQL remains the SQL parser.

Callbacks must be synchronous. Task/ValueTask-returning and async state-machine
delegates are rejected before invocation. `RunExclusive` executes a synchronous
owner-thread callback and defers service callbacks until it returns. It does not
turn the synchronous database into a cross-thread queue.

Notices and notifications are decoded from real PostgreSQL responses and delivered
after unmanaged entry and transport cleanup. Managed callback failures are
reported as `AggregateException` after execution; a command may already have
committed. They do not quarantine a healthy core. Service callbacks cannot
synchronously issue SQL, close the owner or enter another transaction. Schedule
the next caller operation after they return. Notifications and notices from a
scoped transaction are delivered after the token is revoked. On a failing callback
transaction, queued notices are retained for the next successful operation so
callback delivery does not mask the original SQL/user exception.

`Listen`/`Unlisten` issue actual LISTEN/UNLISTEN. Channel input is one PostgreSQL
identifier, with ASCII case folding for unquoted input, doubled quotes for quoted
input and the selected engine's 63-byte UTF-8 identifier limit. Multiple callbacks
share a channel subscription. `OnNotification`/`OffNotification` observe channels
already listened to; registering a global callback does not itself execute LISTEN.
Subscriptions are disposed on the owner thread. A raw protocol caller controls
its own protocol exchanges and callback bypass behavior; complete its Sync/frame
sequence before mixing ordinary services with raw operations.

## Extensions

`PgliteOptions.Extensions` accepts `PgliteExtension` descriptors. Each managed setup
runs after the engine readiness check, before Create/Open returns; setup results
supply an optional namespace object, initializer and closer. All setup functions
run before initializers. `GetExtension<T>(name)` accesses a namespace without
reflection, dynamic property generation or runtime code generation. This is the
.NET replacement for JavaScript object augmentation and Emscripten options.

Closers run in registration order on the owner thread before engine shutdown.
Closer errors are collected; the engine still closes and resources are released.
Cleanup errors are reported only afterward. Setup/init failure unwinds the ready
engine and already-registered closers. Extension code must keep its lifecycle
callbacks synchronous.

`PgliteExtension.Plpgsql` and `.Snowball` describe modules already statically
translated and registered by the bootstrap path. They do not dynamically load a
payload or imply every PostgreSQL contrib module is delivered. Native shared
libraries, arbitrary Wasm bundles and unknown translated built-ins are explicitly
rejected during option validation. The supervisor's source/parity ledger records
individual exported contrib availability.

## Storage, archives and COPY

Existing positional `PgliteOptions(data, prefix, work)` remains supported.
`BackendExecutable` is a hidden, obsolete compatibility check; it must identify
the internal `prefix/bin/postgres` bootstrap descriptor. `BootstrapPrefix` selects
the staged translated bootstrap assets.

`PgliteStorage.Persistent(path)` describes durable host directory storage.
`PgliteStorage.Temporary(parent)` owns a unique temporary host directory. It uses
real files, including WAL, and makes no RAM-only or memory-filesystem claim.
Construct options with the storage descriptor, bootstrap prefix and a diagnostic
work directory outside PGDATA. The database owns the descriptor while open;
disposing it while attached fails. Temporary storage is deleted after successful
engine shutdown and exclusion-lease release. Persistent directories are retained.
Core startup/shutdown failure does not erase a quarantined cluster. A caller can
dispose an unattached temporary descriptor to clean up construction failures.

`SyncToFs` executes a real CHECKPOINT. `DumpDataDir` requires an idle session,
checkpoints and writes tar using `System.Formats.Tar`, with optional BCL gzip.
The single process owner remains quiescent while all cluster files and WAL are
copied. Engine entry from a destination-stream callback is rejected. Archives omit
postmaster PID/options files. External tablespaces, symbolic links and special
files are unsupported and rejected. A running snapshot retains the engine's
control/WAL state and is reopened through PostgreSQL recovery.

`RestoreDataDirectory` recognizes gzip automatically or accepts an explicit
compression choice. It extracts into private staging, bounds total extracted
bytes/entry count, rejects absolute/traversal/duplicate paths, links and devices,
requires PG_VERSION and global/pg_control, and installs into a new or empty target.
It never overlays an existing cluster. `Restore(options, stream)` then opens that
cluster through the translated engine. Streams remain owned by the caller.
Physical archives require the same engine, platform/ABI and disk format. They are
not logical pg_dump archives, browser filesystem bundles, or a portable format
across engine revisions. The one-owner process rule means closing the source
before opening a restored clone.

COPY has explicit stream and byte-array APIs. The SQL-builder callback receives a
safe SQL literal for a private managed FILE transport path:

```csharp
var csv = db.CopyTo(file => $"COPY my_table TO {file} WITH (FORMAT csv)");
db.CopyFrom(file => $"COPY other_table FROM {file} WITH (FORMAT csv)", csv.Data);
```

Overloads copy directly to/from caller-owned streams. PostgreSQL executes COPY,
including CSV/text/binary conversion and errors, and must return exactly one COPY
command. Output bytes come from its real file writes. Temporary transport files
are removed after execution or failure. This replaces the upstream `/dev/blob`
Emscripten device; it does not reserve a host `/dev/blob` or rewrite arbitrary SQL.
Raw protocol COPY support is provided separately by the protocol layer.

## Live and worker adaptations

`db.Live.Query`, `.Changes` and `.IncrementalQuery` run real SQL in read-only
transactions. They expose initial results and subscription/refresh/disposal
methods. Live queries refresh automatically after successful local operations
return the session to idle, including explicit and callback transaction commits.
The initial result is returned in `InitialResults`/`InitialChanges`; callback
registration receives subsequent changes. Results equal to the previous response
are suppressed. Explicit `Refresh` delivers the refreshed response even if equal.

This port uses a full local result comparison instead of upstream temporary views,
table triggers, LISTEN invalidation and SQL diff tables. It does not detect writes
by external processes, provide polling, or claim incremental SQL execution cost.
Read-only enforcement prevents an adapter refresh from repeatedly executing a
write. Views, joins and user SELECTs remain PostgreSQL operations. Windowed query
options require offset and limit together, return total count and slice results
in managed memory. A refresh can change an existing window's offset and limit.

Changes/incremental queries require a unique non-NULL named result key. Changes
include INSERT/DELETE/UPDATE, changed column names and the preceding row's key
(`After`, null for the first row). Reordering reports `__after__` in the changed
columns. Schema changes issue RESET plus new inserts. Result column names must be
unique; alias duplicate columns. SQL/refresh/callback errors are reported after
the triggering local commit and leave a healthy engine usable. Dispose a query
before removing the tables it depends on. Close disposes all live subscriptions.

`PgliteWorker.Create/Open` constructs a dedicated .NET thread and queues operations
using asynchronous task completion sources. `WaitReady` awaits startup.
`QueryAsync`, `ExecAsync`, `SqlAsync`, `TransactionAsync`, `SyncToFsAsync`,
`DumpDataDirAsync` and `InvokeAsync` enter only on that thread. Worker callbacks,
transactions, extension lifecycle and notification delivery stay there as well.
Task continuations run asynchronously on caller-side schedulers. `InvokeAsync`
and transaction delegates must be synchronous and cannot return Task/ValueTask;
queuing another operation from the worker owner callback is rejected to prevent
deadlocks. A worker does not create a second permissible process owner.

Cancellation can skip an item before it starts or cancel awaiting close/readiness;
it does not interrupt translated PostgreSQL while executing. `CloseAsync` stops
accepting work, drains accepted items, then disposes on the owner thread.
`Dispose` waits for that same close; `DisposeAsync` awaits it. Worker failure is
reported to queued promises and shutdown completion, preserving core quarantine.
`Closed` confirms successful engine closure; a failed startup does not claim that
any quarantined engine resources were closed.
Browser leader election, multi-tab BroadcastChannel and Web Worker transport have
no .NET process equivalents here.

## Reusable qualification cases

`tests/ApiServices/ApiServicesCases.cs` is intended to be linked into the consumer.
`Run(database, report)` exercises service SQL in an existing ready lifetime.
`RunLifetimes(seedOptions, report)` must run after all other owners have closed;
it exercises extension setup/init/close, temporary cleanup, checkpointed tar/gzip
restore through the actual engine, invalid archive handling and worker lifetimes.
The supervisor owns builds and raw/processed × JIT/NativeAOT execution. An isolated
source-contract compilation does not qualify runtime archive recovery or workers.
