# Snapshot retention and resumable RBU

This is a finite maintenance API slice for the unchanged, published SQLite
application. The native process is only a pinned test oracle. The generated
counterpart calls translated C# bodies and compares exact typed rows and the
native state/progress trace. The workload SHA256 is
`8f056be066db0b291929a750dae021202f32a2c30a96be9bb0102c6fafd221eb`.

The snapshot test creates a real WAL, disables automatic checkpoints and pins an
old read transaction while another connection commits changes. It captures a
new snapshot, compares only the documented chronology signs and self equality,
and opens both historical views in a third connection. Statements finish before
each snapshot transition. `snapshot_recover` is called without a read transaction
on that initialized WAL connection. Both owned snapshots are freed before the
WAL generation ends; only then are readers released and WAL truncated. This does
not exercise orphaned/PERSIST_WAL recovery or expired snapshots.

The RBU test creates a rollback-mode target with three rows and a secondary
index, plus a genuine `data_item` update database containing update/delete/insert
operations and a correct `rbu_count` table. It saves after observable staged work,
closes, verifies unpublished rows, reopens the same target/update/state paths,
and steps to completion under a finite cap. Step counts and permyriad progress
come from the native observations. RBU-owned connection pointers are borrowed
only until the next RBU call, never closed by the test. Each RBU handle is cleared
before close; returned error strings are copied and freed with `sqlite3_free`.

The retained public shapes are ordinary fixed C signatures: `sqlite3_snapshot`
is 48 bytes/alignment 1, `sqlite3rbu` is 416 bytes/alignment 8, and all pointers are
8/8. The test treats handles opaquely. API usage preconditions come from the
pinned `sqlite3.h` snapshot comments (lines 11022–11216) and the embedded RBU
public API documentation in `sqlite3.c` (lines 227035–227575). These comments are
not a source of translated application bodies.

The coordinator schedules the separate native oracle:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteMaintenance/run-native.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --artifacts /absolute/new/native-maintenance
```

The runner validates the exact accepted native library hash and build role,
creates fresh files through SQLite APIs, and records resources and file hashes.
The first measured native observation passed at
`/mnt/storage/roslynobj-csharp-20261015/t4/native-maintenance-1/native-maintenance.json`:
two snapshots freed, chronology signs -1/1/0, both replayed views correct,
checkpoint 0/0/0, and RBU saved/reopened progress 1. Its 12 completion steps visit
OAL/MOVE/CHECKPOINT/DONE; final step, repeated step and complete close return 101.
Those are observations of this pinned finite input, not generic step guarantees.

`ValidateApi.py` replays the frozen production validator and compares the extra
14 API declarations to the accepted public inventory. It derives C# handle
aliases from the actual published declarations and checks explicit handle sizes
and packing. It does not inspect or rebuild SQLite source semantics. Prepare
fresh aliases/report files before the coordinator builds:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteMaintenance/ValidateApi.py \
  --generated-project "$GENERATED_PROJECT" --base-api-report "$BASE_API_REPORT" \
  --aliases "$API_ALIASES" --report "$API_REPORT"

dotnet build edgcpp/roslyn-csharp/tests/SqliteMaintenance/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$JIT_OUTPUT"

dotnet "$JIT_OUTPUT/Checks.dll" \
  edgcpp/roslyn-csharp/tests/SqliteMaintenance/workload.json \
  "$FRESH_JIT_ARTIFACTS" "$NATIVE_REPORT" "$API_REPORT"

dotnet publish edgcpp/roslyn-csharp/tests/SqliteMaintenance/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$AOT_OUTPUT"

"$AOT_OUTPUT/Checks" edgcpp/roslyn-csharp/tests/SqliteMaintenance/workload.json \
  "$FRESH_AOT_ARTIFACTS" "$NATIVE_REPORT" "$API_REPORT"
```

Both generated modes create their own fresh databases, call generated bodies
directly, compare the complete semantic observations with the same separate
native report and audit loaded shared-library hashes. The generated process
rejects any loaded native SQLite library. JsonArray calls explicitly select the
JsonNode overload, with no reflection serializer or warning suppression.

Cross-process recovery, concurrent writers, failure injection, RBU vacuum,
custom VFS/rename and delta/virtual-table/constraint paths remain separate.
Only the eight RBU APIs used here are executed; other retained RBU entrypoints
remain unexercised by this slice. Generated execution is pending coordinator
build/run evidence.

The bounded maintenance gate now passes native, translated JIT and NativeAOT.
[The compact receipt](../../acceptance/sqlite/maintenance-report.json) records all
five successful measured phases, exact frozen sources/workload/API/provider
identities, command lines and loaded libraries. Both generated observation
objects are exactly equal to the native reference; all owned snapshot, RBU,
statement and database handles release, and generated maps exclude native SQLite.
NativeAOT explicitly pins clang 18. This is same-WAL retained-history and normal
RBU save/reopen/completion evidence; invalidated snapshots, RBU VACUUM/custom VFS
and failure variants are outside this finite proof.
