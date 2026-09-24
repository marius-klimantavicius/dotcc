# Owning managed server API

The public API is `Managed.Valkey.ValkeyServer`, with options in
`Managed.Valkey.ValkeyOptions`. Its project references the real translated
`Managed.Database.ValkeyCore` library. See the [API guide](../src/Managed.Valkey/README.md)
for a minimal example and the [sample](../samples/ManagedConsumer/Program.cs)
for a complete TCP client and restart flow.

`DataDirectory` is required. Bind address defaults to loopback, port to 6379,
foreground RDB persistence to `dump.rdb`, AOF to disabled, and disposal to SAVE.
AOF can be enabled at startup with `AppendOnly`; `AppendFsync` selects Always,
EverySecond or No. Password and data-directory strings are encoded as strict
UTF-8. Invalid ports, path components, enum values or embedded NULs fail before
starting translated code. Validation does not reserve a port; bind failure
faults readiness.

The constructor starts an owned executor thread. `Ready` and `Endpoint` complete
after configuration, listeners, static Lua and persistence loading succeed.
`StartAsync` awaits that readiness. `IsRunning` reports serving state;
`Completion` completes after terminal cleanup, including worker joins and owner
disposal. Each instance has an `InstanceId`, separate globals, descriptors,
allocation ownership and data-directory context. The process working directory
does not change.

`StopAsync(Save)` requests upstream foreground save and shutdown.
`StopAsync(NoSave)` explicitly omits the RDB save. A failed SAVE faults the stop
operation and retains the live server; callers can correct the cause and retry.
Cancellation stops waiting for a queued stop request; it does not revoke an
operation already submitted. A token already cancelled prevents submission.
`DisposeAsync` uses `DisposeMode`, including the same failed-SAVE behavior.
Concurrent stop requests settle after the chosen successful shutdown.

No runtime binding crosses an `await`. Executor entry and translated workers
bind the appropriate runtime owner synchronously. If worker cleanup fails, the
API retains the owner instead of releasing memory under a live worker;
`IsQuarantined` and `ValkeyCleanupException.InstanceId` identify that state.
Terminal failures propagate through readiness/completion tasks. Upstream logs
currently use the shared process console; a per-instance logging callback is
not exposed.

Commands use ordinary RESP clients over TCP. The public facade does not expose
unsafe translated pointers or permit direct calls into its private core.
Configuration and capability limits are described in
[managed-profile.md](managed-profile.md) and [persistence.md](persistence.md).
