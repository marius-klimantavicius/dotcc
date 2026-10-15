This is a bounded repeat-generation and restart test for the unchanged pinned
SQLite production input. It is prepared, not execution evidence. The test uses
the public `Translator` pipeline and actual CLI-published output as its
baseline. It does not execute application SQL or add another acceptance
framework.

The translator has a synchronous phase observer overload:
`Translate(request, cancellationToken, phaseObserver)`. The real phases are
`InventoryCompleted`, `EmissionCompleted` and `BeforePublication`. The last
notification occurs after all staging files and the real staged
`completion.json` have been written, immediately before the cancellation check
and atomic output-directory move. The original overload remains available.

The full-object procedure is deliberately only two translation attempts:

1. Derive a request from the actual production manifest, changing only
   `output.directory` to a fresh dedicated output parent. Keep every input,
   provider, root, budget and the snapshot-11 runtime project path unchanged.
2. In a separate process, call `Translate` with an observer that cancels its
   CTS synchronously at `BeforePublication`. The observer records the actual
   staged completion before cancelling. Require `Cancelled`, ROC0002, no
   published output, no generated-file success result, and removal of staged
   completion. Preserve all incomplete source/report files.
3. After that process exits, start a new process with the exact same request
   file and requested output path. The incomplete stage remains untouched.
   Recompute inventory/emission and publish normally; no phase cache or staged
   files are reused.
4. Compare `Generated.cs`, `Generated.csproj`, `Generated.Abi.c` and
   `Generated.NativeImports.json` directly as bytes against the actual CLI
   baseline, also recording lengths and SHA-256. Verify every completion file
   entry against its actual file. Compare the program reports after removing
   only `output.directory`, retaining both original report/completion hashes
   and output paths. Record metrics independently.

The callback records newly created staging by comparing the dedicated parent
directory's before/after contents. This happens only after the synchronous
phase notification; it does not infer a phase by polling filenames, guessing a
GUID or waiting an estimated duration. No other artifact or process writes
inside that parent. Failed staging remains explicitly incomplete even though
its representation report describes already emitted source. No surviving
completion is accepted there.

Public cancellation does not currently interrupt the emitter midway through a
body, and the product has no incremental resume/checkpoint-cache API. This test
proves clean restart from immutable input, not partial-stage recovery. Returned
cancelled-result metrics currently inherit inventory metrics; the measured
process phase is authoritative for full cancelled-attempt elapsed time and
peak memory.

The coordinator can build the small harness against independently frozen
product source:

```sh
dotnet build edgcpp/roslyn-csharp/tests/SqliteRestart/Checks.csproj -c Release \
  -p:TranslatorProject=/mnt/storage/roslynobj-csharp-20261015/t3/value-storage/snapshot-12/edgcpp/roslyn-csharp/src/RoslynObj.CSharp/RoslynObj.CSharp.csproj \
  -p:UseSharedCompilation=false -o /absolute/new/checker
```

Direct harness commands are `Checks cancel REQUEST NEW_RESULT [PHASE]` and
`Checks complete REQUEST NEW_RESULT`, under separate measured processes. The
default cancellation phase is `BeforePublication`. Other phase checks must use
an independently genuine small-object manifest, not extra full SQLite scans.
The harness writes the returned public program report separately from its
phase/status/staging/metrics summary.

The finite reproduction runner builds the harness and runs those two measured
full-object attempts against the actual CLI baseline:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteRestart/run-restart.py \
  --manifest /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-preparation-1/manifest.json \
  --baseline /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1 \
  --translator-project /mnt/storage/roslynobj-csharp-20261015/t3/value-storage/snapshot-12/edgcpp/roslyn-csharp/src/RoslynObj.CSharp/RoslynObj.CSharp.csproj \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t4/restart-reproduction-1
```

An optional `--small-manifest` adds only the two early-phase cancellation
checks, each in a separate dedicated output parent and process. Each genuine
small input and provider is hash-checked. The full request retains its fixed
8GiB, 1800-second, 512MiB output, one-open-unit and reader-cache budgets; the
existing measured runner also enforces process time/memory/log limits. Builds
use 300 seconds/8GiB. All tool/compiler, source, baseline, input/provider,
build, manifest, cancelled-stage and restarted output identities are recorded.
Source/input changes or resource excess leave an incomplete receipt.

The coordinator schedules all builds and full-object runs. These files do not
run compilers or workloads merely by being prepared. This gate neither closes
the enabled-feature matrix nor broadens provider, C++, scale or incremental
resume claims. See [T4-RESTART.md](../../docs/T4-RESTART.md).
