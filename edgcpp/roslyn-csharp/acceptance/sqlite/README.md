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
recorded compiler intrinsics. Candidate providers remain unimplemented;
`input.json` has no executable bindings. This is an inventory input, not a
runnable SQLite configuration. T3/T4 execution is not implemented.

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
baseline evidence; no generated SQLite execution or T3/T4 gate is claimed.
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
