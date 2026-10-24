# Using the embedded Npgsql source fork

This library copies Npgsql v10.0.1 at
`12d792ac2163596385152dcea5c68ba99769731f`. The exact acquisition metadata and
original source hashes are in [npgsql-source.json](../config/npgsql-source.json).
The [upstream release](https://github.com/npgsql/npgsql/releases/tag/v10.0.1)
is the source selection; it is separate from the pinned PGlite engine.

Reference [Npgsql.csproj](../src/Npgsql/Npgsql.csproj) from a .NET 10 project.
The public namespace remains `Npgsql`; the assembly is `Managed.PGlite.Npgsql`.
Remove a stock Npgsql package reference when using this fork: the shared public
type names make mixing the two libraries ambiguous. No EF Core or Dapper
compatibility is claimed. Normal builds consume checked-in Npgsql source and
restore ordinary managed build dependencies; they do not fetch Npgsql source.

The execution path is Npgsql commands/readers/codecs → buffered frontend
transport → dedicated `PgliteWorker` → actual generated PostgreSQL backend.
The original generated initdb initializes new clusters. Native PostgreSQL,
JavaScript, Wasm and socket proxies are not product execution dependencies.

## Ordinary sample

[NpgsqlExample](../samples/NpgsqlExample/Program.cs) creates a private cluster,
inserts Unicode text with binary typed parameters, commits, rolls back a delete,
closes/reopens, prepares a query and reads typed values. Its bootstrap and notice
assets are staged by the existing consumer targets.

```bash
dotnet build pglite/ManagedConsumer.slnx -c Release
dotnet run --project pglite/samples/NpgsqlExample/NpgsqlExample.csproj -c Release
```

Expected output:

```text
id=7, name=Žąsis
```

The example deliberately deletes its own temporary cluster after clean shutdown.
The library retains caller-owned persistent PGDATA. For a durable application,
choose stable data/work directories and keep the work directory outside PGDATA:

```csharp
using Managed.PGlite;
using Npgsql;
using NpgsqlTypes;

var options = new PgliteOptions(dataDirectory, bootstrapPrefix, workDirectory);
await using var source = NpgsqlEmbeddedDataSource.Create(options, create: true);
await using var connection = await source.OpenConnectionAsync();
await using var command = new NpgsqlCommand("SELECT $1::integer + 1", connection);
command.Parameters.AddWithValue(NpgsqlDbType.Integer, 41);
int answer = (int)(await command.ExecuteScalarAsync())!;
```

Use `create: false` to open an already initialized cluster. See
[API-USAGE.md](API-USAGE.md) for `PgliteOptions`, engine settings and asset
selection. Only one connection can own the engine at a time. Close it before
opening the next connection. Successive leases use fresh backend lifetimes,
retaining committed database files and resetting session state.

## Build variants

Both translated projects must use the same form. For raw products, supply
absolute `TranslatedProject` and `InitdbTranslatedProject` paths to the paired
`.Raw` projects, as described in API-USAGE. The default is processed.

```bash
dotnet publish pglite/samples/NpgsqlExample/NpgsqlExample.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o /absolute/private/publish-directory
```

Launch the published executable from any directory. Bootstrap assets and
licenses travel with the publish output. Normally trimmed NativeAOT support
applies to the qualified embedded Slim-builder type profile; it does not imply
reflection-based dynamic JSON, composite or arbitrary plugin support.

## Boundaries

The transport buffers engine responses, subject to `MaximumResponseBytes`.
`SequentialAccess` does not establish bounded-memory database streaming.
Multiple simultaneous sessions, pooling, multiplexing, streaming COPY and
running-query cancellation are excluded. The exact deterministic rejection
policy and qualified surface are recorded in [NPGSQL-PARITY.md](NPGSQL-PARITY.md)
with the acceptance results; do not
infer support from untested upstream APIs retained in the copied source.

This document describes the delivery interface. Current test results and
limitations are recorded in [NPGSQL-QUALIFICATION.md](NPGSQL-QUALIFICATION.md)
and the [validation receipt](../../docs/validation/pglite-npgsql-20261024.json).
