# Copied-source adaptation ledger

Upstream: Npgsql v10.0.1, commit `12d792ac2163596385152dcea5c68ba99769731f`.
Engine: PGlite PostgreSQL fork `29b6424aa312ec3556247db069939006dd27f878`.

The complete upstream Npgsql directory and connection-string source generator were
copied, including the embedded generator template and original PostgreSQL license.
Per-file original and adapted hashes are in `../../config/npgsql-source.json`.
No source download occurs during ordinary builds. No Npgsql NuGet package supplies
commands, readers, codecs, transactions or protocol machinery.

## Modified upstream files

| File | Reason |
| --- | --- |
| `Internal/NpgsqlConnector.cs` | Select managed embedded startup without sockets/authentication; owner-controlled shutdown instead of frontend Terminate; reject unsupported timeouts/COPY operation state/notification wait/cancellation; lock embedded StartUserAction and EndUserAction state transitions. |
| `Internal/NpgsqlReadBuffer.cs` | Accept embedded infinite read timeout without dereferencing a nonexistent socket; reject finite timeout. |
| `Internal/NpgsqlWriteBuffer.cs` | Accept embedded infinite write timeout without dereferencing a nonexistent socket; reject finite timeout. |
| `Npgsql.csproj` | Distinct assembly/package identity and .NET10 project wiring to actual Managed.PGlite; retain logging runtime dependency and copied source generator; omit upstream packaging/analyzer-only infrastructure; private prebuilt API/backend override for focused isolated probes; analyzer project excludes runtime NativeAOT/publish global properties. |
| `NpgsqlCommand.cs` | Reject live running-cancellation tokens before ownership on execute/prepare/unprepare and reject embedded Cancel even when idle. |
| `NpgsqlConnection.cs` | Reject streaming COPY APIs before binding; preserve embedded data source by rejecting ConnectionString reassignment, CloneWith variants, ChangeDatabase and distributed enlistment. Ordinary ICloneable.Clone retains upstream embedded binding. |
| `NpgsqlDataReader.cs` | Enforce cancellation-token policy in buffered fast paths for ReadAsync, NextResultAsync, GetFieldValueAsync and IsDBNullAsync, as well as connector nested operations. |
| `NpgsqlSlimDataSourceBuilder.cs` | Expose PrepareConfiguration internally so embedded data source reuses AOT-compatible original slim builder/configuration. |
| `NpgsqlTransaction.cs` | Enforce cancellation-token policy for SaveAsync which otherwise bypasses normal connector registration. |
| `NpgsqlActivitySource.cs` | Emit embedded tracing tags without assuming a socket endpoint or inventing a network backend PID; active diagnostic listeners must not break commands. |
| `NpgsqlDataSource.cs` | Acquire connector ownership before replacing the working codec catalog during reload; restore that catalog if loading fails. |
| `README.md` | Document embedded factory, distinct identity/conflict with stock Npgsql, real startup/type bootstrap, supported lifetime and explicit exclusions; retain original upstream README below. |
| `SourceGenerators/Npgsql.SourceGenerators.csproj` | Standalone copied build-time analyzer project with explicit package versions; Scriban.Signed6.5.1 deliberately upgraded to7.2.1 to address restore advisories; generator source/template unchanged and not shipped as runtime dependencies. Treat NativeAOT/publish/runtime properties as local and explicitly disable them so static-graph restore cannot native-publish the netstandard compiler plugin. |

## Authored transport and lifecycle files

- `NpgsqlEmbeddedDataSource.cs`: original slim configuration, basic/array codecs, exclusive session lease, persistent reopen and lease release only after shutdown.
- `Internal/NpgsqlConnector.Embedded.cs`: dedicated worker startup, actual current_setting/session_user metadata, real Npgsql catalog bootstrap, no authentication/cancel-key fiction.
- `Internal/EmbeddedStream.cs`: serialize original frontend frames to worker.ExecProtocolRaw, retain fragmented headers/payloads, queue original backend bytes, wake synchronous/asynchronous readers for producer output/errors/disposal.
- `Internal/EmbeddedCopyGuard.cs`: owner/context-bound PostgreSQL utility hook, chain prior hook, reject parsed streaming CopyStmt before standard_ProcessUtility using normal SQLSTATE0A000 recovery, including stored/dynamic SQL.
- `Internal/EmbeddedPolicy.cs`: pre-cancelled-token behavior, live-token exclusion and explicit streaming COPY API error.

The supervisor-owned API adds only an internal context dispatch bridge and friend
assembly permission, so hook installation uses the same existing owner dispatch.
Those API changes are outside this directory's ownership.

After remote attachment, embedded startup also enables the engine's actual GUC
reporting and flushes its initial status messages into the configuration
exchange. Startup values come from real SQL settings; subsequent SET/RESET
ParameterStatus frames pass unchanged through the raw transport. This keeps
Npgsql's named-parameter parser synchronized with standard_conforming_strings.

Embedded disposal captures failures through complete connector cleanup and
closes/unbinds the logical connection before throwing. A managed cleanup error
after actual engine closure releases the lease; genuine failed shutdown retains
quarantine and continues to reject close/open retries. ReloadTypesAsync rejects
unsupported cancellation before any catalog mutation.

A failed physical open releases only its own logical attempt lease after worker
cleanup/drain, allowing retry after another owner closes. This does not release
the PGlite API's actual process-owner exclusion: failed-startup quarantine remains
owned there. Established connections retain their stricter proven-close rule.

## Build and identity

Assembly/package: `Managed.PGlite.Npgsql`; namespace: `Npgsql`. This is a distinct
assembly with overlapping type names, not a binary-compatible stock Npgsql drop-in.
The original generator runs only during compilation. Its Scriban dependency was
deliberately upgraded to 7.2.1; the trusted embedded template still generates the
original connection-string builder. Runtime logging packages and their notices are
retained. A normally trimmed consumer uses original slim basic/array resolvers;
there is no whole-library rooting or reflection-based embedded integration.

## Execution evidence

Focused private probes use actual translated backend/initdb assemblies and the
ordinary Managed.PGlite API, never a native/JavaScript/Wasm/socket substitute.
The supervisor owns the complete raw/processed JIT/NativeAOT acceptance evidence.
See the project validation ledger for the current qualified matrix.
