# Using Managed.PGlite

Reference [PGlite.csproj](../src/Api/PGlite.csproj) from an ordinary .NET 10
application. Build the translated backend/initdb and stage their bootstrap assets
through the campaign first. The public namespace is `Managed.PGlite`.
The current qualification target is Linux x64; the new API matrix is still
pending as recorded in [API-PARITY.md](API-PARITY.md).

## Run the separate example

[ApiExample](../samples/ApiExample/Program.cs) demonstrates a dedicated worker,
safe interpolation, a callback transaction and a parameterized typed query.
It has a normal `ProjectReference` to the reusable API and no acceptance sources.
Run from the repository root after the supervisor's translated products are
available:

```bash
dotnet build pglite/samples/ApiExample/ApiExample.csproj -c Release
dotnet run --project pglite/samples/ApiExample/ApiExample.csproj -c Release --no-build
```

Expected application output:

```text
id=7, name=Žąsis
```

The project imports the existing `ManagedConsumer/BootstrapAssets.targets` and
`NativeAotNotices.targets`. Bootstrap data and notices follow the selected
translated project into build/publish output. Nothing is copied into this
example's source tree. Missing product/assets cause a build error.

`TranslatedProject` and `InitdbTranslatedProject` can select the paired raw
products. Supply both absolute paths to avoid selecting different forms:

```bash
dotnet run --project pglite/samples/ApiExample/ApiExample.csproj -c Release \
  -p:TranslatedProject="$PWD/pglite/generated/TranslatedPGlite.Raw/TranslatedPGlite.csproj" \
  -p:InitdbTranslatedProject="$PWD/pglite/generated/TranslatedInitdb.Raw/TranslatedInitdb.csproj"
```

For a normally trimmed NativeAOT consumer, with the target toolchain installed:

```bash
dotnet publish pglite/samples/ApiExample/ApiExample.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o pglite/build/api-example-publish
pglite/build/api-example-publish/ApiExample
```

These commands describe usage, not completed final qualification.

## Async application ownership

The root database is thread-affine. A caller using `await` should use
`PgliteWorker` so every core operation stays on its dedicated owner thread:

```csharp
using Managed.PGlite;

// Application owns these directories; keep work outside the cluster directory.
var options = new PgliteOptions(dataDirectory, bootstrapPrefix, workDirectory);
await using var worker = PgliteWorker.Create(options);
await worker.WaitReady();
await worker.ExecAsync("CREATE TABLE entries (id integer PRIMARY KEY, name text)");

int id = 7;
string name = "Žąsis";
await worker.TransactionAsync(tx =>
{
    tx.Sql(PgliteTemplate.Sql($"INSERT INTO entries VALUES ({id}, {name})"));
});

var response = await worker.QueryAsync(
    "SELECT id, name FROM entries WHERE id = $1::integer", new object?[] { id });
var row = response.EnsureSuccess().ResultSets.Single().TypedObjectRows.Single();
Console.WriteLine($"id={(int)row["id"]!}, name={(string)row["name"]!}");
await worker.CloseAsync();
```

Transaction and `InvokeAsync` delegates must be synchronous. Do not return
Task/ValueTask, use an `async` lambda, enqueue another worker operation from its
callback or let a database/token escape for later caller-thread use. Top-level
`await` resumes only the caller; the engine stays on the worker thread. Use
`InvokeAsync(db => ...)` for synchronous APIs without an async convenience
method. Cancellation can skip queued work and cancel waiting; it does not stop
a query already executing. `CloseAsync` drains accepted work and closes the owner.

The complete example creates unique caller-owned temporary data/work paths,
explicitly closes in `finally` and removes its root only after successful close.
Startup/shutdown failure retains the files for diagnostics. `await using` alone
does not authorize deleting a cluster whose shutdown failed.

## Synchronous configuration and query results

In an entirely synchronous workflow, create/use/close `PgliteDatabase` on the
same thread:

```csharp
var options = new PgliteOptions(dataDirectory, bootstrapPrefix, workDirectory)
{
    Configuration = new PgliteConfiguration
    {
        Username = "app_owner",     // Bootstrap role for a new cluster.
        Database = "postgres",      // Must already exist when opening.
        Debug = 0,
        RelaxedDurability = false,
        Settings = new Dictionary<string, string>
        {
            ["application_name"] = "my-app",
            ["TimeZone"] = "UTC"
        }
    }
};
using var db = PgliteDatabase.Create(options);
var result = db.Query("SELECT $1::integer AS answer", new object?[] { 42 })
    .EnsureSuccess().ResultSets.Single();
int answer = (int)result.TypedRows.Single()[0]!;
```

Use `Open(options)` for an existing cluster; `Create` requires a new/empty one.
Default database is `template1`. Create a custom database with SQL before opening
it by name. One process can own one database at a time, even when using workers.
There is no socket login, arbitrary initdb flag array, executable selection,
Wasm-module injection or config-file parser. `Settings` accepts selected built-in
PostgreSQL settings; process/module/path choices belong to the embedding profile.
`BackendExecutable` is an obsolete compatibility check, not a supported choice.

`Query(string)` retains text rows and returns SQL errors. Parameterized `Query`
also returns typed projections and SQL errors. `EnsureSuccess()` converts them
to `PgliteSqlException`; `Exec`, token operations and decoded protocol calls
normally throw SQL errors directly. Read `exception.Result` for full details.
Ordinary SQL errors leave the owner usable after protocol recovery.

`Rows` contains original text, `TypedRows` positional values and `TypedObjectRows`
named values. Use unique column aliases when names matter. `Columns` gives OIDs
and metadata. `DescribeQuery` returns parameter OIDs/result columns without
executing the statement. Customize session codecs with
`db.Codecs.RegisterParser/RegisterSerializer`, or supply
`new PgliteQueryOptions { Codecs = registry, ParamTypes = oids }` for a query.
`PgliteParameter(value, oid)` supplies an explicit per-value type. Call
`RefreshArrayTypes()` after creating new enum/custom array types.

Templates bind ordinary interpolated values. Compose nested templates with
`PgliteTemplate.Sql`, `Join`, and `Identifier("schema", "table")`. `Raw` is for
trusted SQL only. `FormatQuery` produces server-quoted literal SQL for display
or export, with SQL round trips; use bound templates/parameters for execution.

## Transactions, notifications and extensions

`db.Transaction(tx => ...)` supplies a token valid only inside that synchronous
callback. Success commits, callback/SQL failure rolls back and `tx.Rollback()`
explicitly rolls back. Use token methods inside the callback; direct database
access, nested ownership and transaction-control SQL are rejected. A token
cannot be reused after return. `RunExclusive` groups synchronous owner operations
and defers normal service callbacks.

`Listen(channel, callback)` and `OnNotification(callback)` return disposable
subscriptions; dispose them on the owner thread, or through `InvokeAsync` when
using a worker. Global notification observers do not themselves issue LISTEN.
Use `OnNotice` events/options for session notices and
`PgliteQueryOptions.OnNotice` for query-specific notices. Callbacks execute after
translated entry and must return before further SQL/close operations. A callback
exception may be reported after its command has committed.

Add `PgliteExtension` descriptors through `options.Extensions`. Managed setup,
initialize and close callbacks run synchronously on the owner. Retrieve namespace
objects with `GetExtension<T>(name)`. `PgliteExtension.Live`, `.Plpgsql` and
`.Snowball` name supported managed/static descriptors. Native/Wasm payloads and
the 34 unavailable contrib modules are not runtime-loadable plugins.

## Storage, COPY and live queries

`PgliteStorage.Persistent(path)` retains a real cluster directory.
`PgliteStorage.Temporary(parent)` owns a disposable disk directory and deletes it
after successful engine shutdown. Construct options with that descriptor,
bootstrap prefix and an external work path. Dispose an unattached descriptor
after construction failure; never delete files retained by a failed core shutdown.
No RAM-only, IndexedDB or OPFS backend is provided.

On the owner thread, real COPY can transfer byte arrays or caller-owned streams:

```csharp
var csv = db.CopyTo(file => $"COPY entries TO {file} WITH (FORMAT csv)");
db.Exec("CREATE TABLE imported (LIKE entries INCLUDING ALL)");
db.CopyFrom(file => $"COPY imported FROM {file} WITH (FORMAT csv)", csv.Data);
```

The callback receives an already quoted private transport filename. PostgreSQL
performs CSV/text/binary conversion and reports COPY errors. Use the Stream
overloads for caller I/O. `DumpDataDir(PgliteDumpCompression.Gzip)` exports a
checkpointed physical tar/gzip snapshot. Close the source before calling
`Restore(newOptions, sourceStream)` into a new/empty directory. Archives require
matching engine/platform format. Streams remain caller-owned.

`db.Live.Query/Changes/IncrementalQuery` returns initial data separately from
subsequent callback delivery. Dispose subscriptions on their owner thread.
Changes require a unique non-NULL result key. Local commits trigger read-only
query refresh and full managed comparison; external writes and incremental SQL
cost are outside this adaptation. `CancellationToken` stops future delivery
when the owner next observes it; it cannot interrupt a running engine query.

Raw/decoded protocol APIs and stream callbacks are available through the root
database or worker `InvokeAsync`. Callbacks receive owned buffers after core
entry; output is buffered. Finish extended exchanges with Sync before mixing
ordinary queries. See [API-PLATFORMS.md](API-PLATFORMS.md) for the complete raw
COPY, callback, quarantine and restore restrictions.
