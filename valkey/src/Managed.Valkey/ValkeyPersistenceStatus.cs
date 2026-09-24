namespace Managed.Valkey;

/// <summary>A snapshot taken on the server executor after an event-loop turn.
/// PrimaryOffset and FsyncedOffset are replication-stream positions, not file
/// byte counts. FsyncedOffset is upstream's published completion position and
/// may be -1 when unavailable. It does not promise directory-entry durability.</summary>
public sealed record ValkeyPersistenceStatus(
    bool AppendOnlyEnabled,
    long AofCurrentBytes,
    long PrimaryOffset,
    long FsyncedOffset,
    bool BackgroundFsyncFailed);
