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

Providers and bindings are empty until the complete import inventory is audited.
This is an inventory input, not a runnable SQLite configuration. T3/T4 execution
is not implemented. See [coverage](../../docs/COVERAGE.md) for current evidence.
