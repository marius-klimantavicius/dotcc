# Finite ownership, preupdate, session and backup checks

This slice invokes only the existing CLI-generated SQLite C# application. Native
SQLite is confined to `run-native.py`, a separate oracle process loading the
audited library with SHA256
`39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a`.
No native test application/provider body is linked into the generated checker.

`workload.json` is pinned at SHA256
`b5d18a84906ae21cf2c75e04f9ade60cba35afcdbdd45e0f84d1970e68e7d8db`.
It contains four bounded, synchronous phases:

- Three custom bound BLOB owners are released by rebind, clear and finalize.
  Reset retains the first owner. IDs and counters outlive every released buffer.
- Four preupdate events copy exact typed values during the callback. Only the
  defined old/new rowid and value sides are observed. Hook removal verifies the
  returned userdata and absence of events from a subsequent insert.
- A session captures one update, delete and insert. A protected iterator copies
  typed old/new values; a NULL `sqlite3_value*` for an unchanged update field is
  recorded as `undefined`, distinct from SQL NULL. The changeset is applied to a
  second database, then applied again with an OMIT conflict callback.
- An in-memory backup takes one page per step, capped at 64 steps, and compares
  final rows and `integrity_check`. It has no concurrent writers.

The first native observation records release counts `[0,1,2,3]`, destructor IDs
`[101,102,103]`, four preupdate events, no first-apply conflicts, and three second
apply conflicts in actual observed order (DATA/NOTFOUND/CONFLICT). Backup steps
observed `(OK,remaining1,pageCount2)` and `(DONE,remaining0,pageCount2)`.
The generated result must match every observation, including actual changeset
bytes and iterator/callback order; the checker does not fabricate or reorder them.

The generated test uses fixed native userdata and concrete Cdecl
`UnmanagedCallersOnly` callbacks. Each callback catches managed exceptions and
records a failure flag; none crosses the C boundary. Borrowed SQLite values are
copied during their valid lifetime. Sessions, iterators, changeset allocations,
backup handles, statements, database connections and custom owners are explicitly
released. JSON uses primitive nodes and the non-generic `JsonArray.Add(JsonNode)`
overload for NativeAOT.

`ValidateApi.py` first replays the exact frozen production API validator referenced
by the existing production API receipt. It then compares all 51 used API shapes
against accepted T0 public facts and actual emitted C# declarations. Complete
database/value/session/iterator/backup names are derived from their actual API
declarations and mapped back to selected public type identities. Four exact Cdecl
callback shapes are checked. The original 23-API validator and generated files are
never modified.

The coordinator schedules all execution. From the repository root, using fresh
artifact paths:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/run-native.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --output /absolute/new/native-api-callbacks.json

python3 edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/ValidateApi.py \
  --generated-project /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1/Generated.csproj \
  --base-api-report /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-generated-production-run-1/api-surface.json \
  --aliases /absolute/new/ApiAliases.cs --report /absolute/new/api-surface.json

dotnet build edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/Checks.csproj -c Release \
  --disable-build-servers -p:UseSharedCompilation=false -nodeReuse:false \
  -p:GeneratedProject=/mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1/Generated.csproj \
  -p:BaseApiReport=/mnt/storage/roslynobj-csharp-20261015/t3/sqlite-generated-production-run-1/api-surface.json \
  -p:ApiAliases=/absolute/new/ApiAliases.cs -p:ApiReport=/absolute/new/api-surface.json \
  -o /absolute/new/jit

dotnet /absolute/new/jit/Checks.dll \
  edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/workload.json \
  /absolute/new/generated-jit.json /absolute/new/native-api-callbacks.json \
  /absolute/new/api-surface.json

dotnet publish edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/Checks.csproj -c Release \
  --disable-build-servers -p:UseSharedCompilation=false -nodeReuse:false \
  -r linux-x64 --self-contained true -p:PublishAot=true \
  -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject=/mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1/Generated.csproj \
  -p:BaseApiReport=/mnt/storage/roslynobj-csharp-20261015/t3/sqlite-generated-production-run-1/api-surface.json \
  -p:ApiAliases=/absolute/new/ApiAliases.cs -p:ApiReport=/absolute/new/api-surface.json \
  -o /absolute/new/aot

/absolute/new/aot/Checks \
  edgcpp/roslyn-csharp/tests/SqliteApiCallbacks/workload.json \
  /absolute/new/generated-aot.json /absolute/new/native-api-callbacks.json \
  /absolute/new/api-surface.json
```

The test compares only the finite `observations` object with the separate oracle;
execution reports also retain source/workload/report identities, resource release
and loaded native library hashes. Foreign threads, callback-triggered SQL writes,
fault injection, arbitrary conflict policies, broader destructor APIs, concurrent
backup and the remaining T4/provider matrices are outside this slice.

The API usage follows SQLite's documented [binding ownership](https://sqlite.org/c3ref/bind_blob.html),
[preupdate lifetime and defined sides](https://sqlite.org/c3ref/preupdate_blobwrite.html),
[new changeset values](https://sqlite.org/session/sqlite3changeset_new.html),
[old changeset values](https://sqlite.org/session/sqlite3changeset_old.html), and
[OMIT conflict policy](https://sqlite.org/session/c_changeset_abort.html). These
documents guide test API usage; retained public model facts remain the translator's
input authority.

The frozen generated execution gate now passes in JIT and NativeAOT. The compact
[receipt](../../acceptance/sqlite/api-callbacks-report.json) pins the separate
native oracle, all source bytes, API validation, commands, loaded providers and
observations. Five measured phases pass; both generated observation objects are
exactly equal to the native object, with no callback error and all tracked
resources released. No native SQLite library appears in generated process maps.
The first build correctly rejected a validator path-provenance mismatch; its
receipt is preserved. Fresh metadata came from the exact frozen validator.
The accepted snapshot also clears finalizer handles before checking results to
avoid cleanup retries after a native finalizer has released its handle.

The 51 validated API signatures are a surface check, not a claim that all 51
APIs or every argument/ownership variant was exercised. The four specific
callback signatures and finite behavior above are the execution scope. RBU,
snapshot/carray/normalize, cross-process locking, controlled failures and broader
provider/ownership contracts remain separate acceptance work.
