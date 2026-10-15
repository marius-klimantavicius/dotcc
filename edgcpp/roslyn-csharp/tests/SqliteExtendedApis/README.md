This finite T4 slice exercises carray values/ownership, normalized statement
execution, column metadata and controlled constraint/extended-error recovery.
It uses the actual CLI-published generated library. The independent native
oracle passed all four cases at
`/mnt/storage/roslynobj-csharp-20261015/t4/native-extended-apis-1/extended-api-report.json`.
Generated JIT/NativeAOT execution is a separate pending gate.

[availability.json](availability.json) records the accepted public inventory
and pinned header/source basis. Seven selected APIs have retained executable
definitions and actual generated declarations: `sqlite3_carray_bind`,
`sqlite3_carray_bind_v2`, `sqlite3_normalized_sql`,
`sqlite3_table_column_metadata`, `sqlite3_errcode`,
`sqlite3_extended_errcode`, `sqlite3_extended_result_codes`. The initial source
availability snapshot is not an execution or native-export probe claim.

The shared [workload](workload.json) SHA-256 is
`1ad62ff2001a6630e52f0fb7a9fa730209574e9c03ec6b3f219e56db5bdb72e7`.
The native loader pins the existing build receipt SHA-256
`c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435`
and library SHA-256
`39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a`.
It loads only that absolute library in a separate process, with no Python
sqlite3 import or system-library fallback. Generated code calls `Api` methods
statically, rejects a loaded native SQLite library, and never forwards
application SQL to the oracle.

The carray cases check INT32 static storage, INT64 transient copying after the
caller mutates its original array, and v2 callbacks with contexts distinct from
the data. DONE and reset retain a binding; clear_bindings and finalize each
release one binding, and an out-of-range bind releases its context immediately.
Three exact callback events are checked. All callbacks use Cdecl and catch
errors; owners stay alive through statement/destructor completion, then the
harness releases its allocations. The callback is a release notification,
not a forwarding implementation. DOUBLE/TEXT/BLOB carray layouts, deep-copy
cases and broader ownership failures remain untested.

Normalized SQL is borrowed until statement finalization. Both harnesses copy
it before later calls, execute two literal-differing statements with a bound
value, and compare exact native text. Normalization spelling is pinned-version
behavior, not a portable promised format. Metadata copies both borrowed
strings before the next SQLite call; failure outputs are not read. Constraint
failures capture the documented preserving errcode/extended_errcode/errmsg
APIs immediately after step and before finalize. Step/finalize19, extended1299,
UNIQUE2067, explicit rollback and later valid writes are checked separately.

The new validator replays the unchanged production23 API validator using its
recorded frozen path, then checks the seven additional selected signatures,
ordinary parameter roles, scalar/pointer dimensions, the exact void(void*)
destructor and actual generated C# declarations. It preserves the same concrete
`Api`/`CapturedDatabase` aliases. It performs no object database scan or type
inference from source spelling.

Coordinator preparation command, choosing fresh alias/report paths:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteExtendedApis/SurfaceValidator.py \
  --generated-project /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1/Generated.csproj \
  --candidate-receipt /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-production-generated-1/completion.json \
  --base-report /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-generated-production-run-1/api-surface.json \
  --aliases /absolute/new/CapturedAliases.cs --report /absolute/new/api-surface.json
```

Build [Checks.csproj](Checks.csproj) with `GeneratedProject`, `CandidateReceipt`
(the actual completion), `SqliteApiAliases` and `SqliteApiReceipt` above.
`SqliteSurfaceValidator` optionally selects the independently frozen new
validator path. The target repeats `--verify-only` before build. Publish the
same project for linux-x64 NativeAOT with clang-18 and the same properties.
The coordinator schedules all builds/executions.

Run the generated JIT checker as `dotnet Checks.dll WORKLOAD_JSON
NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT`, or the AOT `Checks` with the
same arguments. It compares the exact typed `cases` object and connection
codes against the pinned native report, using explicit JSON nodes for AOT.
Each result records borrowed values before advancing/finalizing statements,
all cleanup, and loaded provider identities.

Snapshots and RBU remain available and unexercised in this slice. All five
snapshot APIs are retained; snapshot storage is captured48/1. Fourteen
`sqlite3rbu_*` definitions, including open/step/close/savestate, are genuinely
amalgamated and generated. RBU must not be classified as unavailable or as only
internal hooks. Its staged update/resumption and the snapshot lifecycle need
separate workloads. See [T4-EXTENDED-APIS.md](../../docs/T4-EXTENDED-APIS.md).

The generated counterpart now passes JIT and NativeAOT against the exact native
four-case observation array. All five measured phases and immutable source/API/
provider evidence are in [the receipt](../../acceptance/sqlite/extended-apis-report.json).
Connections close, all three finite release callbacks match without callback
failure, and generated maps exclude native SQLite. NativeAOT pins clang 18.
The availability snapshot records facts at preparation time; it is separate from
these executed API cases and is not a whole-provider or wider-datatype proof.
