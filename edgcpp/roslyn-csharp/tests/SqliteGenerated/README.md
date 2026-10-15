# Generated SQLite execution harness

This harness calls the generated C# application methods directly. It has no native
SQLite binding. In diagnostic mode, `SurfaceValidator.py` checks a zero-error unpublished
diagnostic candidate, every emitted file hash, the exact accepted public inventory,
and the candidate's 88 native system-provider mappings. It then derives only the
two C# aliases from actual generated declarations and verifies all 23 used API
signatures against retained ordinary scalar/pointer facts. The build repeats this
check. Candidate binding configuration can be prepared independently of successful
emission with `acceptance/sqlite/prepare-generated-manifest.py`.

The initial mode exercises open/prepare/bind/step/reset/clear/finalize/close, including
signed 64-bit integers, binary64, embedded-NUL UTF-8, nonempty and empty BLOBs,
empty text, and NULL. Destructor pointers use the captured `nint` representation;
SQLITE_STATIC buffers remain owned until finalization. Borrowed column bytes are
copied before the next step. `sql-smoke` runs the unchanged five-case/43-statement
workload, preserving exact typed rows and column names. JSON reports use explicit
primitive JSON nodes so NativeAOT needs no reflection serializer metadata.

`run-generated-sqlite.py` runs the existing stages as one measured campaign against
an already genuine candidate and its exact emission receipt:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteGenerated/run-generated-sqlite.py \
  --candidate /absolute/path/to/diagnostic-candidate \
  --candidate-receipt /absolute/path/to/emission-diagnostics-report.json \
  --artifacts /absolute/path/to/new-sql-test-artifacts
```

The driver snapshots the harness, validator, workload, oracle runners, provider
plan, build properties and measurement tool. It verifies candidate file hashes,
freezes attributed runtime/system-provider inputs, rebuilds the ABI-only helper
with the recorded C compiler, and audits JIT/publish copies. It disables MSBuild
servers and shared compilation, executes the separate native oracles, and runs
both generated modes under JIT and NativeAOT with exact typed JSON comparison.
Compiler, ABI export/dependency, managed dependency, native library and observation
identities are recorded in `generated-sqlite-report.json`, including failures.
NativeAOT compiler package identities are recorded from publish assets after
publication of the test executable; this does not publish the application.

Default phase budgets are build `300s/8GiB`, NativeAOT `900s/16GiB`, and execution
`120s/4GiB`, with `64MiB` of logs per phase. Override them using
`--build-seconds/--build-memory-bytes`, `--aot-seconds/--aot-memory-bytes`,
`--execution-seconds/--execution-memory-bytes` and `--log-bytes`. The selected tools
can be set with `--dotnet`, `--cc`, and `--aot-cc`; the default AOT compiler is
`clang`. This driver adds no workloads and never exports, retranslates or creates
a production completion marker. The individual commands below remain useful for
first execution or focused diagnosis without waiting for the complete driver.

Production mode consumes the actual CLI output and explicit request manifest. It
requires the real `completion.json` and `program-report.json`; it never fabricates
a diagnostic receipt or replaces CLI-emitted files:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteGenerated/run-generated-sqlite.py \
  --mode production \
  --generated-project /absolute/path/to/cli-output/Generated.csproj \
  --completion /absolute/path/to/cli-output/completion.json \
  --program-report /absolute/path/to/cli-output/program-report.json \
  --request-manifest /absolute/path/to/explicit-cli-request.json \
  --artifacts /absolute/path/to/new-production-source-tests
```

The validator checks completion file hashes/sizes, input SHA/artifact/ordinal,
`translationComplete`, retained definitions and library roots, the exact runtime
project reference, and all 103 requirements. It reconciles the 88 native bindings,
provider file hashes, nine explicit aliases, exact signatures and declared
ownership/error/thread/callback contracts. The other 15 requirements must retain
their selected runtime-intrinsic lowering and have no native binding. The 23 API
representations still come from the accepted T0 public facts and actual generated
declarations. A valid production API/test report records `sourcePublished: true`
and `completionValidated: true`; source publication is distinct from runtime,
whole-provider and complete T4 acceptance.

An optional `--compare-diagnostic-receipt /actual/diagnostic-report.json` records
byte-hash comparisons and both actual runtime project paths. It preserves and
tests the CLI files even when a runtime path or another file differs.

For manual production surface validation, invoke `SurfaceValidator.py` with
`--mode production`, `--generated-project`, `--completion`, `--program-report`,
`--request-manifest`, `--aliases` and `--report`. The existing harness MSBuild
property `CandidateReceipt` then points to the **actual CLI completion file**.
During `--verify-only`, the validator reads the explicitly recorded production
evidence paths from that API receipt and revalidates all of them. The historical
property name does not turn the CLI completion into a diagnostic receipt.

Initial candidate JIT and NativeAOT bind/reset/clear checks and the 43-statement
typed SQL workload are already proven by the frozen candidate execution receipt.
Production mode repeats that bounded coverage against CLI-published source.
Extended ownership/destructor callbacks, error/fault behavior, broader AOT and
provider contracts, and the other T4 workloads remain open. Prior frozen reports
are preserved; their broad remaining-area descriptions do not erase the initial
coverage they established.

From the repository root, after the coordinator has produced a genuine candidate,
set these paths to its exact diagnostic receipt and a fresh harness artifact folder:

```sh
sqlite_candidate=/absolute/path/to/diagnostic-candidate
sqlite_candidate_receipt=/absolute/path/to/emission-diagnostics-report.json
sqlite_checks=/absolute/path/to/new-harness-artifacts
```

Validate its API surface before building:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteGenerated/SurfaceValidator.py \
  --generated-project "$sqlite_candidate/Generated.csproj" \
  --candidate-receipt "$sqlite_candidate_receipt" \
  --aliases "$sqlite_checks/CapturedAliases.cs" \
  --report "$sqlite_checks/api-surface.json"
```

The coordinator schedules builds and executions. Use the same properties for JIT
and NativeAOT; the generated project builds its ABI-only native helper:

```sh
dotnet build edgcpp/roslyn-csharp/tests/SqliteGenerated/SqliteGenerated.csproj -c Release \
  -p:GeneratedProject="$sqlite_candidate/Generated.csproj" \
  -p:CandidateReceipt="$sqlite_candidate_receipt" \
  -p:SqliteApiAliases="$sqlite_checks/CapturedAliases.cs" \
  -p:SqliteApiReceipt="$sqlite_checks/api-surface.json" \
  -o "$sqlite_checks/jit"

dotnet publish edgcpp/roslyn-csharp/tests/SqliteGenerated/SqliteGenerated.csproj -c Release \
  -r linux-x64 -p:PublishAot=true \
  -p:GeneratedProject="$sqlite_candidate/Generated.csproj" \
  -p:CandidateReceipt="$sqlite_candidate_receipt" \
  -p:SqliteApiAliases="$sqlite_checks/CapturedAliases.cs" \
  -p:SqliteApiReceipt="$sqlite_checks/api-surface.json" \
  -o "$sqlite_checks/aot"
```

Generate native observations in separate processes using the existing audited
oracle build receipt. The first runner adds only the bind/reset/clear protocol;
the existing full SQL runner and its workload remain unchanged:

```sh
python3 edgcpp/roslyn-csharp/acceptance/sqlite/run-native-bind-smoke.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --output "$sqlite_checks/native-bind.json"

python3 edgcpp/roslyn-csharp/acceptance/sqlite/run-native-sql.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --output "$sqlite_checks/native-sql.json"
```

Run each generated executable mode against the separate typed observations:

```sh
dotnet "$sqlite_checks/jit/SqliteGenerated.dll" bind-smoke \
  edgcpp/roslyn-csharp/acceptance/sqlite/sql-smoke.json \
  "$sqlite_checks/generated-jit-bind.json" "$sqlite_checks/native-bind.json"

dotnet "$sqlite_checks/jit/SqliteGenerated.dll" sql-smoke \
  edgcpp/roslyn-csharp/acceptance/sqlite/sql-smoke.json \
  "$sqlite_checks/generated-jit-sql.json" "$sqlite_checks/native-sql.json"

"$sqlite_checks/aot/SqliteGenerated" bind-smoke \
  edgcpp/roslyn-csharp/acceptance/sqlite/sql-smoke.json \
  "$sqlite_checks/generated-aot-bind.json" "$sqlite_checks/native-bind.json"

"$sqlite_checks/aot/SqliteGenerated" sql-smoke \
  edgcpp/roslyn-csharp/acceptance/sqlite/sql-smoke.json \
  "$sqlite_checks/generated-aot-sql.json" "$sqlite_checks/native-sql.json"
```

These are source-validation tools and initial SQL tests. They do not publish source
or establish every native provider contract. The generated process
records loaded native library hashes and rejects a loaded native SQLite library.
The shared workload's remaining T4 areas stay open, including persistence/WAL,
concurrency, sessions/RBU, fault recovery, and broader API/destructor behavior.
