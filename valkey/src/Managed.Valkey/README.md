# Managed Valkey API

This project references the real translated `generated/TranslatedValkey` project.
Generate it with `valkey/scripts/translate.sh` before building `valkey/ManagedConsumer.slnx`.
The generated core and its authored C# host use namespace `Managed.Database`;
the public lifecycle API uses `Managed.Valkey`.

```csharp
await using var server = await ValkeyServer.StartAsync(new ValkeyOptions
{
    DataDirectory = "/absolute/path/to/data",
    Port = 6379,
});
var endpoint = await server.Endpoint;
// Connect any RESP client to endpoint.
await server.StopAsync(ValkeyShutdownMode.Save);
```

Each server has a dedicated executor thread, an isolated translated owner, and
an explicitly bound libc context on every core entry. `Ready`/`Endpoint` publish
successful startup; `Completion` reports terminal failure or completion after
worker joins and owner disposal. Startup does not call the CLI entry point.
Ports must be positive: upstream port zero disables the TCP listener.

`DisposeAsync` requests `DisposeMode` (SAVE by default). A failed final SAVE
faults that operation and keeps the server running. Retry after correcting the
cause, or explicitly call `StopAsync(ValkeyShutdownMode.NoSave)` when acceptable.
Cancellation of a Stop task cancels the wait, not a queued shutdown operation.
Failed worker cleanup retains the owner; `ValkeyCleanupException.InstanceId`
identifies that retained instance. It never frees memory underneath a worker.

`GetPersistenceStatusAsync` queues an AOF progress snapshot on the owner executor.
It reports enabled state, current AOF bytes, primary and completed-fsync
replication offsets, and background fsync failure. The offsets are positions in
the replication stream, not file byte counts. To observe completion of preceding
writes, capture `PrimaryOffset` after their replies and wait for `FsyncedOffset`
to reach it while checking `BackgroundFsyncFailed`. This does not promise
directory-entry crash durability. Shutdown faults unserved snapshot requests;
cancellation cancels waiting without interrupting the executor.

The BCL-only TCP sample exercises two owners, binary values, lists/hashes,
transactions, Lua, and a persistence restart:

```
dotnet run --project valkey/samples/ManagedConsumer -c Release -- --port 6379 --peer-port 6380
dotnet run --project valkey/samples/ManagedConsumer -c Release -- --port 6379 --peer-port 6380 --appendonly
```

Each run creates distinct data directories by default and prints their location.
`--directory`, `--password`, and the two port options are also supported. AOF is
selected at startup; runtime enablement, background rewrite/snapshot, replication,
clustering, native modules, and process-level features remain outside this profile.
