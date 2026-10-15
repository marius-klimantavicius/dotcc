# Generated concurrency worker

`Program.cs` implements the same finite JSONL protocol as the separate native
worker. It invokes generated SQLite bodies directly, uses fixed native callback
contexts/UCO Cdecl entrypoints, and copies borrowed column bytes before another
step. Busy and unlock callbacks contain exceptions and never call SQLite.
Registrations are canceled before closing connections; callback contexts remain
allocated until all handles close. Thread reads use separate connections and
ordinary Threads/Barrier, with joins before cleanup. JsonArray entries explicitly
select the JsonNode overload for NativeAOT.

The first native observation is
`/mnt/storage/roslynobj-csharp-20261015/t4/native-concurrency-1/concurrency-report.json`:
all 29 commands passed, BUSY primary/extended/finalize codes 5 with callback `[0]`,
passive checkpoint 0/4/3 while pinned, released truncate 0/0/0,
LOCKED_SHAREDCACHE step/extended/finalize 262 with primary 6, one actual
notification carrying context 2, successful retry, and both three-round readers
joined. These values belong to this input/provider run, not general guarantees.
The complete native report is the comparison authority.

The coordinator supplies the forthcoming readable production publication and its
validated baseline API report. `ValidateApi.py` replays that publication validator,
checks 23 baseline plus 6 additional signatures against accepted T0 public facts,
and verifies the exact busy/unlock Cdecl shapes. It emits the actual C# aliases.

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteConcurrency/ValidateApi.py \
  --generated-project "$GENERATED_PROJECT" --base-api-report "$BASE_API_REPORT" \
  --aliases "$API_ALIASES" --report "$API_REPORT"

dotnet build edgcpp/roslyn-csharp/tests/SqliteConcurrency/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$JIT_OUTPUT"

dotnet publish edgcpp/roslyn-csharp/tests/SqliteConcurrency/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$AOT_OUTPUT"
```

Prepare a fresh command JSON with absolute argv for either generated mode:

```json
["/absolute/dotnet", "/absolute/jit-output/Checks.dll", "--api-surface", "/absolute/api-surface.json"]
```

For NativeAOT use `[/absolute/aot-output/Checks, --api-surface, REPORT]` as properly
quoted JSON strings. Run each mode in its own fresh artifact directory:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteConcurrency/run-concurrency.py \
  --mode generated --worker-command-json "$WORKER_COMMAND_JSON" \
  --native-report "$NATIVE_REPORT" --artifacts "$FRESH_GENERATED_ARTIFACTS"
```

The orchestrator compares every semantic acknowledgment/error/callback/thread
observation, waits for both process exits, and validates closure reports. Each
generated process hashes its loaded shared libraries and rejects native SQLite.
Watchdogs abort stalled protocols; timing does not establish success. Generated
execution remains pending measured coordinator builds/runs.

The generated gate now passes JIT and NativeAOT against the final readable
publication. [The receipt](../../acceptance/sqlite/concurrency-report.json)
records five measured phases and exact comparison of all 29 observations.
Both workers exit with resourcesReleased=true and empty cleanup/callback errors;
source/binary freezes pass and generated loaded-library lists exclude native
SQLite. NativeAOT explicitly pins clang 18. Three barrier-controlled read rounds
per thread demonstrate the tested permitted use; a start barrier permits overlap
but does not prove simultaneous in-engine execution on every scheduler run.
