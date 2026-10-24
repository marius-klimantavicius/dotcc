# Embedded Npgsql qualification

The target is Linux x64 LP64 on .NET 10. The copied source is Npgsql v10.0.1,
commit `12d792ac2163596385152dcea5c68ba99769731f`; the translated engine remains
`29b6424aa312ec3556247db069939006dd27f878`. The [source manifest](../config/npgsql-source.json)
contains archive and original/adapted file hashes. [PATCHES.md](../src/Npgsql/PATCHES.md)
tracks the adaptations. See [NPGSQL-PARITY.md](NPGSQL-PARITY.md) for the supported
surface and exclusions.

Qualification passed on the selected target. Raw and processed JIT and normally
trimmed NativeAOT each passed **5,151 assertions in 42 cases**, with no failed or
skipped mapped cases. Commands, source/tool/artifact hashes and retained evidence
are recorded in the [validation receipt](../../docs/validation/pglite-npgsql-20261024.json).

The same-pin native differential control passed 105 assertions in 26 cases.
The upstream-source check passed 89 assertions over sixteen source files, all
42 mapped cases and sixty exact decimal vectors. Release and Debug solution
builds, the default sample and the framework-dependent published sample passed;
samples executed from `/tmp`. Every matrix/sample output retained all 659
bootstrap files, five Npgsql/Microsoft notices and mode `0700` entrypoint
descriptors. Native outputs also carry their selected .NET runtime notices.

## Reproduction

Build the paired generated products through the existing PGlite campaign when
they are absent. No Npgsql source download happens during ordinary builds.
Managed package restore is separate from source acquisition.

```bash
dotnet build pglite/ManagedConsumer.slnx -c Release
dotnet run --project pglite/samples/NpgsqlExample/NpgsqlExample.csproj
bash pglite/scripts/npgsql-test.sh --form all --mode all --rid linux-x64 \
  --work-root /absolute/private/npgsql-qualification
```

The matrix driver builds normal project references into private artifact paths,
executes from unrelated directories, validates all mapped case IDs and checks
source snapshots before and after builds. Normally trimmed NativeAOT uses the
embedded Slim-builder profile with arrays. No whole-library rooting substitutes
for consumer qualification. Build-time analyzer publication settings are kept
separate from the runtime application's AOT settings.

The [test mapping](NPGSQL-TEST-MAPPING.md) preserves the upstream fixture methods,
selected assertions and exact sixty decimal vectors. The source check verifies
the pinned upstream inputs independently of execution. A same-pin native engine
provides a differential control only; it is never a product dependency.

The response-limit quarantine reproducer is a separate process because a failed
owner intentionally remains quarantined until process exit:

```bash
dotnet run --project pglite/tests/NpgsqlQuarantine/NpgsqlQuarantine.csproj -c Release
```

It passes twelve checks through public configuration and SQL: an oversized
response fails, repeated close/open attempts report quarantine, a different
source cannot acquire the owner, and caller PGDATA remains present. Its current
execution evidence is processed JIT; it is separate from the 42-case matrix.

## Failure-driven repairs

| Finding | Reproducer and repair |
| --- | --- |
| Startup context | Installing an engine utility hook directly on the worker thread failed because generated globals were not context-bound. The internal friend-only bridge uses the existing guarded Dispatch. |
| Startup metadata | `pg_settings` omits some GUC_REPORT settings. Startup now reads actual `current_setting` values and validates `session_user`; no authentication or cancellation key is fabricated. |
| Async producer/reader | Npgsql can read while its send task is still producing bytes. Transport readers now wait for producer output, errors or disposal, preserving frame order. |
| Source-generator AOT graph | PublishAot propagated into the netstandard compiler plugin and failed NETSDK1207. Its local build-only publication properties now survive restore graph propagation. |
| Live GUC reports | SET changed the engine while Npgsql retained old parser settings, breaking escaped-string SQL with named parameters. Embedded startup now enables actual PostgreSQL ParameterStatus reporting. |
| Tracing | An all-data ActivityListener exposed an assumed socket endpoint. Embedded activities use real database metadata and an embedded tag without endpoint/PID claims. |
| Close failure | A throwing managed closer was swallowed and left an already closed engine's lease stuck. Close now completes client/logical cleanup, reports the error, and releases only a proven closed engine. Genuine engine failure stays quarantined. |
| Rejected type reload | Cancellation rejected after installing a temporary codec catalog. Ownership/token checks now precede mutation; arrays and an existing reader remain usable after rejection. |
| Failed-open retry | A rejected open retained its attempt lease after another source owned the engine. Cleanup now releases only that attempt's logical lease; genuine engine quarantine remains enforced. |

The independent reviewers reproduced the last five defects before repairs and
ran after-fix probes against the real engine. The public response-limit probe
also confirmed genuine quarantine. Fixture corrections preserved the selected
upstream assertions: text truncation restores the upstream value reset, nullable
array exceptions use the correct generic helper contract, and failed-command
metadata is explicitly enabled as in the upstream fixture.

## Existing engine and API regression boundary

The original 36 + 391 consumer suite remains unchanged. The original raw
NativeAOT consumer was compiled before a body-only update to the unused internal
bridge and executed freshly. An IL/native-symbol audit proves that bridge was
trimmed and all other API IL is identical; reuse is recorded with that proof,
not as a rebuild from later source. All four original rows passed 36 + 391
assertions. Protocol contracts passed 185 assertions in JIT and NativeAOT, codec
contracts passed 237 in each mode, and host/path contracts passed 34. These
results and their commands are retained in the original matrix linked by the
final receipt.

No compiler, generated engine, initializer or host-service implementation was
changed by this adaptation. The original 14 engine-suite qualification remains
historical evidence at the unchanged pin; it is not described as a fresh rerun.
Current acceptance includes the original consumer and appropriate API
protocol/codec/host regressions. Unrelated translation campaigns are excluded.
