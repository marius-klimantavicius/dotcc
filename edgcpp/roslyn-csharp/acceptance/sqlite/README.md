# Pinned SQLite translation input

`input.json` pins the unchanged full-feature SQLite 3.54.0 object, its SHA256,
artifact identity, producer revision/version and captured target. The identity
and target values were obtained through the public CLI `inspect` command, not
database queries. The source Linux x64/glibc contract is explicit and separate
from the captured Itanium/layout facts and output RID.

The immutable producer recipe and all 19 feature definitions remain documented
in [the export report](../../../roslyn/docs/SQLITE.md). No reduced export is
permitted. Native SQLite may only be a separate oracle process; generated SQL
execution must come from emitted C#.

Build and inspect from the repository root:

```sh
dotnet build edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Cli/RoslynObj.CSharp.Cli.csproj -c Release
dotnet edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Cli/bin/Release/net10.0/roslynobj-csharp.dll inspect \
  /mnt/storage/roslynobj-sqlite-20261014/export/sqlite3.roslynobj --report /mnt/storage/sqlite-inspect.json
```

For a complete inventory, choose a new attempt directory and run:

```sh
python3 edgcpp/roslyn-csharp/tools/run-measured.py \
  --output-dir /mnt/storage/sqlite-inventory-new \
  --timeout-seconds 1800 --memory-bytes 8589934592 -- \
  dotnet edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Cli/bin/Release/net10.0/roslynobj-csharp.dll \
  inventory edgcpp/roslyn-csharp/acceptance/sqlite/input.json \
  --report /mnt/storage/sqlite-inventory-new/program-report.json \
  --metrics /mnt/storage/sqlite-inventory-new/metrics.json
```

The manifest additionally limits output to 512 MiB, opens one unit, and fixes
reader caches to 4,096 rows/128 pages of 128. Measured process-tree RSS and waited
process high-water RSS have different meanings; the receipt labels both.

The [complete inventory](../../docs/SQLITE-INVENTORY.md) passes and the
[import audit](import-audit.json) lists all 103 requirements, including 15
recorded compiler intrinsics. `input.json` intentionally has no executable
bindings. The preparation tool below adds the 88 explicit system bindings and
retains the 15 recorded intrinsic lowerings. The unchanged full object now emits,
builds and passes initial generated JIT/NativeAOT SQL tests; broader provider and
T4 coverage remains separate.

`prepare-native.py --attempt NEW_DIRECTORY` builds a separate native oracle
from the exact unchanged source and all 19 feature definitions, after checking
source/object/recipe identity. The [build receipt](native-oracle-build.json)
records the successful build, not SQL execution. Its `nativeLibraries` field
lists requested native link libraries, not a measured loaded-provider audit.
The generated runtime must never load this SQLite library. See
[coverage](../../docs/COVERAGE.md) for current evidence.

The separate native oracle now passes the initial 43-statement deterministic
SQL workload twice. [The receipt](native-sql-smoke.json) records exact loaded
libraries, typed rows (including embedded-NUL text/blob bytes and binary64 bits),
source/tool hashes and measured process high-water RSS. This is native-only
baseline evidence; that native-only receipt does not prove generated execution.
The separate [generated receipt](generated-candidate-report.json) supplies the
initial JIT/NativeAOT proof.
`sql-smoke.json` is the shared workload definition, with the remaining T4 areas
listed explicitly. Bind/ownership APIs, persistence/WAL, concurrency and failure
paths are not replaced by this SQL-only baseline.

Run this driver only as a separate native-reference process:

```sh
python3 edgcpp/roslyn-csharp/acceptance/sqlite/run-native-sql.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --output /mnt/storage/roslynobj-csharp-20261015/sqlite/NEW-OBSERVATIONS.json
```

The driver validates the exact oracle library hash and SQLite version before
execution, opens a fresh in-memory database for each case, and refuses to
overwrite evidence. The first development workload attempt failed because
Geopoly provides `_shape` implicitly; its preserved failure is recorded in the
final receipt. The source/object/configuration stayed unchanged.


## Production request and generated execution

Use a fresh artifact root. The helper verifies the accepted inventory, exact
input and provider files, every captured signature and the nine explicit asm-name
aliases. It writes a retain-all library request, preserving all 19 source feature
definitions. Providers are explicit libc/libm/system bindings; a native SQLite
application binding is never supplied.

```sh
sqlite_attempt=/mnt/storage/roslynobj-csharp-20261015/sqlite-NEW
python3 edgcpp/roslyn-csharp/acceptance/sqlite/prepare-generated-manifest.py \
  --manifest "$sqlite_attempt/request.json" \
  --report "$sqlite_attempt/binding-reconciliation.json" \
  --output-directory "$sqlite_attempt/generated"

python3 edgcpp/roslyn-csharp/tools/run-measured.py \
  --output-dir "$sqlite_attempt/translate-run" \
  --timeout-seconds 1800 --memory-bytes 8589934592 -- \
  dotnet edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Cli/bin/Release/net10.0/roslynobj-csharp.dll \
  translate "$sqlite_attempt/request.json" \
  --report "$sqlite_attempt/program-report.json" --metrics "$sqlite_attempt/metrics.json"

python3 edgcpp/roslyn-csharp/tests/SqliteGenerated/run-generated-sqlite.py \
  --mode production --generated-project "$sqlite_attempt/generated/Generated.csproj" \
  --completion "$sqlite_attempt/generated/completion.json" \
  --program-report "$sqlite_attempt/generated/program-report.json" \
  --request-manifest "$sqlite_attempt/request.json" \
  --artifacts "$sqlite_attempt/execution" --aot-cc clang-18
```

For an isolated source snapshot, pass `--runtime-project ABSOLUTE_PROJECT` to the
preparation helper. The generated project references only that runtime, never the
reader or its SQLite package. The execution driver validates the real completion
hashes and public requirements, builds JIT/NativeAOT, records exact compiler and
provider identities, and compares bind/reset/clear plus all 43 SQL statements to
separate pinned native-oracle processes. It never substitutes the oracle for
generated application methods.

See [current execution evidence and remaining gates](../../docs/SQLITE-EXECUTION.md).
The native-only baseline, published source completion, generated execution and
whole T4 acceptance are separate results. No failed attempt or artifact should be
overwritten or removed to make a receipt appear successful.
