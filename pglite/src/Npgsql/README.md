# Managed.PGlite.Npgsql

This directory contains copied Npgsql v10.0.1 source at
`12d792ac2163596385152dcea5c68ba99769731f`, adapted to the actual translated
Managed.PGlite engine pinned at `29b6424aa312ec3556247db069939006dd27f878`.
It retains Npgsql commands, batch commands, data readers, parameters, PostgreSQL
codecs, catalog discovery, prepared statements, and local transaction machinery.
Source provenance is in `../../config/npgsql-source.json`; adaptation reasons
are in `PATCHES.md`. Preserve `Notices/` and the engine/bootstrap notices.

Reference `Npgsql.csproj` from a .NET 10 consumer, then use:

```csharp
using Managed.PGlite;
using Npgsql;

var options = new PgliteOptions(dataDirectory, bootstrapPrefix, workDirectory);
await using var source = NpgsqlEmbeddedDataSource.Create(options, create: true);
await using var connection = await source.OpenConnectionAsync();
await using var command = new NpgsqlCommand("SELECT $1::integer + 1", connection);
command.Parameters.AddWithValue(41);
Console.WriteLine(await command.ExecuteScalarAsync()); // 42
```

The assembly and package identity is **Managed.PGlite.Npgsql**, while the public
namespace remains **Npgsql**. Stock Npgsql defines the same public type names;
do not reference both assemblies in an ordinary consumer. A distinct assembly
name does not establish binary compatibility with stock Npgsql plugins or its
Entity Framework provider. The copied network-related source remains upstream
code; only connections obtained from this embedded factory select the qualified
embedded transport. Constructing a connection from its display connection string
does not restore the embedded binding: use the factory or its CreateConnection.

`Create` defers engine startup until the first connection opens. `create: true`
requires a new/empty PGDATA for that first lifetime; `false` opens an existing
cluster. Only one connection can be leased from a data source, and the engine
allows one owner per process across all data sources. A second open fails
immediately. Connection close drains transport work and destroys the worker and
backend session. Reopening uses a fresh session against persistent PGDATA:
prepared statements, temporary objects, transaction state and session settings do
not survive close. Use persistent storage for repeated opens; the API's temporary
storage descriptor deletes its cluster on successful close. Disposing a data
source with an active connection prevents future opens; that connection owns its
engine until it closes. Failed engine shutdown retains its quarantine/lease.
A rejected open releases its own logical attempt lease after cleanup, so the
same source can retry once a competing owner closes. This does not release any
actual process-owner quarantine retained by the PGlite API after failed startup.

Startup reads actual `current_setting` values and `session_user`; Npgsql then
performs its original PostgreSQL catalog/type bootstrap over raw protocol bytes.
There is no socket authentication or cancellation key. Username describes the
trusted cluster owner: it selects the initdb role on creation and must match the
actual session user when opening. `ProcessID` has no network-backend meaning.
Failed batched commands retain the originating `NpgsqlBatchCommand` in error
metadata (`IncludeFailedBatchedCommand = true`).
`NpgsqlSlimDataSourceBuilder` provides its AOT-compatible basic codecs plus array
resolvers. Broader upstream feature claims below are upstream documentation, not
an embedded compatibility promise; current evidence belongs to the project
qualification ledger.

The embedded profile fixes pooling, multiplexing, keepalive and automatic ambient
transaction enlistment off. Connection and command timeout defaults are zero
(infinite); nonzero command/batch timeouts are rejected before engine entry.
`Cancel` is unsupported. An already cancelled token produces cancellation before
entry; an uncancelled but cancellable token is rejected for engine operations,
including command preparation/execution, transactions and reader operations.
No client timer claims to interrupt a running translated query.

Streaming COPY APIs fail before binding a COPY operation. SQL COPY with
STDIN/STDOUT fails as an ordinary recoverable PostgreSQL `0A000` error before the
core COPY operation starts. A real PostgreSQL utility hook checks the parsed
CopyStmt, including COPY from stored routines and computed dynamic SQL. Quoted
text/comments remain SQL, and ordinary server-file COPY remains available with
PostgreSQL's file privileges and paths. Command readers can expose Stream values;
responses and raw protocol exchanges are still buffered, not streaming COPY.

Notification `Wait`/`WaitAsync`, distributed enlistment, ChangeDatabase,
CloneWith/CloneWithAsync and ConnectionString reassignment are explicitly
unsupported on embedded connections. `ICloneable.Clone` retains its embedded
data source and remains subject to exclusive leasing. Use another factory after
closing the current owner to select different settings or a different database.

## Original upstream README (unmodified below)

Npgsql is the open source .NET data provider for PostgreSQL. It allows you to connect and interact with PostgreSQL server using .NET.

## Quickstart

Here's a basic code snippet to get you started:

```csharp
var connString = "Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase";

await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

// Insert some data
await using (var cmd = new NpgsqlCommand("INSERT INTO data (some_field) VALUES (@p)", conn))
{
    cmd.Parameters.AddWithValue("p", "Hello world");
    await cmd.ExecuteNonQueryAsync();
}

// Retrieve all rows
await using (var cmd = new NpgsqlCommand("SELECT some_field FROM data", conn))
await using (var reader = await cmd.ExecuteReaderAsync())
{
while (await reader.ReadAsync())
    Console.WriteLine(reader.GetString(0));
}
```

## Key features

* High-performance PostgreSQL driver. Regularly figures in the top contenders on the [TechEmpower Web Framework Benchmarks](https://www.techempower.com/benchmarks/).
* Full support of most PostgreSQL types, including advanced ones such as arrays, enums, ranges, multiranges, composites, JSON, PostGIS and others.
* Highly-efficient bulk import/export API.
* Failover, load balancing and general multi-host support.
* Great integration with Entity Framework Core via [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL). 

For the full documentation, please visit [the Npgsql website](https://www.npgsql.org).

## Related packages

* The Entity Framework Core provider that works with this provider is [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL).
* Spatial plugin to work with PostgreSQL PostGIS: [Npgsql.NetTopologySuite](https://www.nuget.org/packages/Npgsql.NetTopologySuite)
* NodaTime plugin to use better date/time types with PostgreSQL: [Npgsql.NodaTime](https://www.nuget.org/packages/Npgsql.NodaTime)
* OpenTelemetry support can be set up with [Npgsql.OpenTelemetry](https://www.nuget.org/packages/Npgsql.OpenTelemetry)
