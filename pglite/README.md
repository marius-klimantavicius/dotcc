# Managed embedded PGlite

This ports the pinned PostgreSQL 18.6 PGlite fork through DotCC for Linux x64
LP64. The actual translated backend initializes a database, executes SQL and
transactions, recovers from ordinary SQL errors, shuts down cleanly and reopens
persisted data in the same process. Initialization includes original PL/pgSQL
and Snowball modules through a translated static registry.

The reusable API manually adapts `@electric-sql/pglite` 0.5.11 at the separate
[wrapper pin](config/api-source.json). It includes parameterized queries, typed
results/codecs/arrays, safe SQL templates, transactions, protocol access,
notifications, COPY, physical archives, managed extensions, local live queries
and a dedicated-thread async worker. [API-USAGE.md](docs/API-USAGE.md) provides
examples; [API-PARITY.md](docs/API-PARITY.md) maps every exported surface and
records platform adaptations, including unavailable contrib modules.

The repaired compiler regenerated all 928 selected backend and 73 initializer
units. All four raw/processed JIT/normally trimmed NativeAOT rows pass the original
36 plus 391 API assertions each. All 17 selected suites, the ordinary five-project
solution build, no-argument Debug run, and normal/published API example pass.
[VALIDATION.md](docs/VALIDATION.md) links current evidence and historical lineage.

The public owning API is [src/Api/PGlite.csproj](src/Api/PGlite.csproj).
Reference that project from a separate application. It references both generated
libraries through ordinary project references. For example, using a fresh data
directory and a separate work directory:

```csharp
using Managed.PGlite;

var options = new PgliteOptions(
    "/tmp/my-pglite-data",
    Path.GetFullPath("pglite/generated/TranslatedPGlite/bootstrap-prefix"),
    "/tmp/my-pglite-work");

using (var database = PgliteDatabase.Create(options))
{
    database.Query("CREATE TABLE items(id integer PRIMARY KEY, note text)").EnsureSuccess();
    database.Query("INSERT INTO items VALUES (1, 'Vilnius Žąsis')").EnsureSuccess();
}
using (var database = PgliteDatabase.Open(options))
{
    var result = database.Query("SELECT note FROM items WHERE id = 1").EnsureSuccess();
    Console.WriteLine(result.ResultSets[0].Rows[0][0]);
}
```

Use one owner on its creating thread, with serialized calls and no callback
reentry. Results own managed copies. SQL errors are returned in `Errors` after
real core recovery; `EnsureSuccess()` optionally throws `PgliteSqlException`.
`Dispose()` performs real backend shutdown and preserves caller-owned data and
diagnostics. See [PRODUCT.md](docs/PRODUCT.md) for lifetime and failure contracts.

Build and run the canonical consumer against existing generated products:

```bash
dotnet build pglite/ManagedConsumer.slnx -c Release
dotnet run --project pglite/samples/ManagedConsumer/ManagedConsumer.csproj -c Release
dotnet run --project pglite/samples/ApiExample/ApiExample.csproj -c Release
```

Ordinary builds and publishes copy the selected backend's complete
`bootstrap-prefix` into the consumer output, including catalogs, descriptors and
notices. Debug/IDE runs also work without arguments. The sample creates a fresh
work directory under `/tmp/dotcc-pglite-consumer`; use `--work PATH` or
`--bootstrap-prefix PATH` after `--` to override these defaults.

To reproduce translation and the complete selected qualification:

```bash
bash Scripts/campaign.sh layout pglite --check
bash pglite/scripts/verify.sh --fetch missing --tools build --form all --mode all --rid linux-x64 --jobs 4
```

The source pin and configuration are in `config/`. Preparation needs Linux x64,
.NET SDK 10, Python 3.11+, a C toolchain, make, Bison, Flex, Perl and binutils.
Native database executables are controls only. The product uses in-process
callbacks and OS primitives; its narrowly bounded locale inventory service runs
the system `locale -a` utility. No native PostgreSQL engine is loaded or spawned.

This target is the Linux LP64 `__PGLITE__` variant. It does not claim upstream
Wasm32/Emscripten equivalence, server/multiprocess delivery or broad extension
qualification. Selected boundaries and exclusions are in
[QUALIFICATION.md](docs/QUALIFICATION.md).

Each backend's bootstrap prefix contains the exact upstream data, JSON
entrypoint/module descriptors and complete upstream/generated notice archives.
NativeAOT publish also copies the resolved runtime pack's license and third-party
notices beside the executable. Keep these notice assets when redistributing.
Generated libraries, acquired sources, build outputs and receipts remain ignored
beneath `generated/`, `ref/`, `build/` and `artifacts/`.
