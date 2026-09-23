using static Managed.Smb.LibSmb2;
using Kerberos.NET.Dns;

namespace Managed.Smb;

/// <summary>Resolves DFS UNC paths and owns the referral and target connections it creates.</summary>
public sealed class SmbDfsClient : IDisposable, IAsyncDisposable
{
    private const uint MaxReferralResponseBytes = 65535;
    private const int MaxReferralHops = 16;
    private readonly SemaphoreSlim _gate = new(1);
    private readonly Func<string, string, bool, CancellationToken, Task<SmbConnection>> _connect;
    private readonly bool _encryptTargets;
    private readonly Dictionary<string, SmbConnection> _connections = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<DfsCacheEntry> _cache = new();
    private bool _disposed;

    private SmbDfsClient(Func<string, string, bool, CancellationToken, Task<SmbConnection>> connect, bool encryptTargets)
    {
        _connect = connect;
        _encryptTargets = encryptTargets;
    }

    public static SmbDfsClient CreateKerberosWithExistingCredentials(string? user = null,
        ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => new((server, share, useEncryption, token) => SmbConnection.ConnectKerberosWithExistingCredentialsAsync(
            server, share, user, dialect, useEncryption, timeoutSeconds, token), encrypt);

    public static SmbDfsClient CreateKerberos(string principal, string password, string realm,
        ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        return new((server, share, useEncryption, token) => SmbConnection.ConnectKerberosAsync(server, share,
            principal, password, realm, dialect, useEncryption, timeoutSeconds, token), encrypt);
    }

    public static SmbDfsClient CreateKerberosWithKeytab(string principal, string realm,
        string keytabPath, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentException.ThrowIfNullOrWhiteSpace(keytabPath);
        return new((server, share, useEncryption, token) => SmbConnection.ConnectKerberosWithKeytabAsync(
            server, share, principal, realm, keytabPath, dialect, useEncryption,
            timeoutSeconds, token), encrypt);
    }

    public static SmbDfsClient CreateKerberosWithCredentialCache(string credentialCachePath,
        string? user = null,
        ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialCachePath);
        return new((server, share, useEncryption, token) =>
            SmbConnection.ConnectKerberosWithCredentialCacheAsync(server, share,
                credentialCachePath, user, dialect, useEncryption, timeoutSeconds, token), encrypt);
    }

    public static SmbDfsClient Create(string user, string password, string domain = "WORKGROUP",
        ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return new((server, share, useEncryption, token) => SmbConnection.ConnectAsync(server, share, user,
            password, domain, dialect, useEncryption, timeoutSeconds, token), encrypt);
    }

    public SmbConnection.SmbFile Open(string uncPath, bool create = false)
        => OpenAsync(uncPath, create).GetAwaiter().GetResult();
    public Task<SmbConnection.SmbFile> OpenAsync(string uncPath, bool create = false, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, (c, p) => c.OpenAsync(p, create, cancellationToken), cancellationToken);
    public SmbConnection.SmbFile OpenRead(string uncPath) => OpenReadAsync(uncPath).GetAwaiter().GetResult();
    public Task<SmbConnection.SmbFile> OpenReadAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, (c, p) => c.OpenReadAsync(p, cancellationToken), cancellationToken);
    public IReadOnlyList<SmbConnection.Entry> List(string uncPath) => ListAsync(uncPath).GetAwaiter().GetResult();
    public Task<IReadOnlyList<SmbConnection.Entry>> ListAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, (c, p) => c.ListAsync(p, cancellationToken), cancellationToken);
    public SmbConnection.Metadata Stat(string uncPath) => StatAsync(uncPath).GetAwaiter().GetResult();
    public Task<SmbConnection.Metadata> StatAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, (c, p) => c.StatAsync(p, cancellationToken), cancellationToken);
    public SmbConnection.SpaceInfo GetSpaceInfo(string uncPath) => GetSpaceInfoAsync(uncPath).GetAwaiter().GetResult();
    public Task<SmbConnection.SpaceInfo> GetSpaceInfoAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, (c, p) => c.GetSpaceInfoAsync(p, cancellationToken), cancellationToken);
    public void Delete(string uncPath) => DeleteAsync(uncPath).GetAwaiter().GetResult();
    public Task DeleteAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, async (c, p) => { await c.DeleteAsync(p, cancellationToken).ConfigureAwait(false); return true; }, cancellationToken);
    public void CreateDirectory(string uncPath) => CreateDirectoryAsync(uncPath).GetAwaiter().GetResult();
    public Task CreateDirectoryAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, async (c, p) => { await c.CreateDirectoryAsync(p, cancellationToken).ConfigureAwait(false); return true; }, cancellationToken);
    public void RemoveDirectory(string uncPath) => RemoveDirectoryAsync(uncPath).GetAwaiter().GetResult();
    public Task RemoveDirectoryAsync(string uncPath, CancellationToken cancellationToken = default)
        => ExecuteAsync(uncPath, async (c, p) => { await c.RemoveDirectoryAsync(p, cancellationToken).ConfigureAwait(false); return true; }, cancellationToken);

    public void Rename(string uncPath, string newUncPath) => RenameAsync(uncPath, newUncPath).GetAwaiter().GetResult();
    public async Task RenameAsync(string uncPath, string newUncPath, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RequireNotDisposed();
            var source = (await ResolveAsync(uncPath, cancellationToken).ConfigureAwait(false))[0];
            var destination = (await ResolveAsync(newUncPath, cancellationToken).ConfigureAwait(false))[0];
            if (!source.Server.Equals(destination.Server, StringComparison.OrdinalIgnoreCase) ||
                !source.Share.Equals(destination.Share, StringComparison.OrdinalIgnoreCase))
                throw new IOException("DFS rename cannot cross target shares");
            var connection = await GetConnectionAsync(source.Server, source.Share, cancellationToken).ConfigureAwait(false);
            await connection.RenameAsync(source.RelativePath, destination.RelativePath, cancellationToken).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }

    public IReadOnlyList<DfsTarget> ResolvePath(string uncPath) => ResolvePathAsync(uncPath).GetAwaiter().GetResult();
    public async Task<IReadOnlyList<DfsTarget>> ResolvePathAsync(string uncPath, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RequireNotDisposed();
            var targets = await ResolveAsync(uncPath, cancellationToken).ConfigureAwait(false);
            return targets.Select(t => new DfsTarget(t.Server, t.Share, t.RelativePath)).ToArray();
        }
        finally { _gate.Release(); }
    }

    private async Task<T> ExecuteAsync<T>(string uncPath, Func<SmbConnection, string, Task<T>> operation, CancellationToken token)
    {
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            RequireNotDisposed();
            string requested = DfsReferralCodec.NormalizeProtocolPath(uncPath);
            Exception? firstFailure = null;
            string? referralServer = null;
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int hop = 0; hop < MaxReferralHops; hop++)
            {
                bool needsDeeperReferral = false;
                foreach (var target in await ResolveAsync(requested, token, referralServer).ConfigureAwait(false))
                {
                    SmbConnection connection;
                    try { connection = await GetConnectionAsync(target.Server, target.Share, token).ConfigureAwait(false); }
                    catch (Exception error) when (CanTryAnotherServer(error))
                    { firstFailure ??= error; continue; } // No operation submitted; another target is safe.
                    try { return await operation(connection, target.RelativePath).ConfigureAwait(false); }
                    catch (SmbException error) when (error.NtStatus == SMB2_STATUS_PATH_NOT_COVERED)
                    {
                        firstFailure ??= error;
                        referralServer = target.Server;
                        if (!visited.Add(referralServer + "\0" + target.Share + "\0" + target.RelativePath))
                            throw new IOException("DFS referral cycle detected", error);
                        needsDeeperReferral = true;
                        break;
                    }
                    // Never replay an ambiguous operation failure on another server.
                }
                if (!needsDeeperReferral) throw firstFailure ?? new IOException("DFS referral returned no usable targets");
            }
            throw new IOException("DFS referral hop limit exceeded", firstFailure);
        }
        finally { _gate.Release(); }
    }

    private async Task<IReadOnlyList<ResolvedDfsTarget>> ResolveAsync(string uncPath, CancellationToken token, string? forcedReferralServer = null)
    {
        string requested = DfsReferralCodec.NormalizeProtocolPath(uncPath);
        DfsCacheEntry? cached = forcedReferralServer == null ? FindCached(requested) : null;
        if (cached != null) return cached.Resolve(requested);

        string namespaceServer = SplitUnc(requested).Server;
        IEnumerable<string> candidates = forcedReferralServer == null
            ? await FindReferralServersAsync(namespaceServer, token).ConfigureAwait(false) : [forcedReferralServer];
        Exception? firstFailure = null;
        foreach (string candidate in candidates)
        {
            try { return await ResolveFromServerAsync(requested, candidate, token).ConfigureAwait(false); }
            catch (SmbException error) when (forcedReferralServer == null &&
                error.NtStatus is SMB2_STATUS_NOT_SUPPORTED or SMB2_STATUS_INVALID_DEVICE_REQUEST or SMB2_STATUS_NOT_FOUND or SMB2_STATUS_OBJECT_NAME_NOT_FOUND)
            { return [SplitUnc(requested)]; }
            catch (Exception error) when (CanTryAnotherServer(error))
            {
                firstFailure ??= error;
                await EvictConnectionAsync(candidate, "IPC$").ConfigureAwait(false);
            }
        }
        throw new IOException("No DFS referral server was reachable", firstFailure);
    }

    private async Task<IReadOnlyList<ResolvedDfsTarget>> ResolveFromServerAsync(string requested, string referralServer, CancellationToken token)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int hop = 0; hop < MaxReferralHops; hop++)
        {
            if (!visited.Add(referralServer + "\0" + requested)) throw new IOException("DFS referral cycle detected");
            DfsReferralResponse response = await QueryReferralAsync(referralServer, requested, token).ConfigureAwait(false);
            DfsReferralEntry? nameList = response.Entries.FirstOrDefault(entry => entry.IsNameList);
            if (nameList != null)
            {
                if (nameList.ExpandedNames.Count == 0) throw new IOException("DFS referral returned no domain controllers");
                referralServer = TrimServer(nameList.ExpandedNames[0]);
                continue;
            }

            var targets = response.Entries.Where(entry => !string.IsNullOrWhiteSpace(entry.TargetPath)).ToArray();
            if (targets.Length == 0) throw new IOException("DFS referral returned no storage targets");
            string prefix = response.PathConsumedCharacters == 0
                ? targets[0].DfsPath ?? requested
                : requested[..response.PathConsumedCharacters];
            prefix = DfsReferralCodec.NormalizeProtocolPath(prefix);
            var cacheEntry = new DfsCacheEntry(prefix, response.Flags, targets, ExpiresAt(targets[0].TimeToLive));
            IReadOnlyList<ResolvedDfsTarget> resolved = cacheEntry.Resolve(requested);

            if (response.StorageServers || !response.ReferralServers)
            {
                Store(cacheEntry); // Referral-only intermediates must never become storage cache hits.
                return resolved;
            }
            referralServer = resolved[0].Server;
        }
        throw new IOException("DFS referral hop limit exceeded");
    }

    private static async Task<IEnumerable<string>> FindReferralServersAsync(string namespaceServer, CancellationToken token)
    {
        var servers = new List<string>();
        try
        {
            string[] discovered = (await DnsQuery.QuerySrv("_ldap._tcp.dc._msdcs." + namespaceServer).WaitAsync(token).ConfigureAwait(false))
                .OrderBy(record => record.Priority)
                .ThenByDescending(record => record.Weight)
                .Select(record => record.Target.TrimEnd('.'))
                .Where(target => !string.IsNullOrWhiteSpace(target)).ToArray();
            if (discovered.Length != 0)
            {
                string? logonServer = Environment.GetEnvironmentVariable("LOGONSERVER")?.TrimStart('\\');
                if (!string.IsNullOrWhiteSpace(logonServer)) servers.Add(logonServer);
                servers.AddRange(discovered);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch { } // Direct/standalone namespaces need no DC SRV record.
        servers.Add(namespaceServer);
        return servers.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<DfsReferralResponse> QueryReferralAsync(string server, string path, CancellationToken token)
    {
        byte[] request = DfsReferralCodec.EncodeRequest(path);
        var connection = await GetConnectionAsync(server, "IPC$", token).ConfigureAwait(false);
        byte[] response = await connection.IoctlAsync(
            unchecked((uint)SMB2_FSCTL_DFS_GET_REFERRALS), request, MaxReferralResponseBytes, token).ConfigureAwait(false);
        return DfsReferralCodec.Parse(path, response);
    }

    private async Task<SmbConnection> GetConnectionAsync(string server, string share, CancellationToken token)
    {
        string key = server + "\0" + share;
        if (_connections.TryGetValue(key, out SmbConnection? connection)) return connection;
        connection = await _connect(server, share, _encryptTargets, token).ConfigureAwait(false);
        _connections.Add(key, connection);
        return connection;
    }

    private async Task EvictConnectionAsync(string server, string share)
    {
        string key = server + "\0" + share;
        if (!_connections.Remove(key, out SmbConnection? connection)) return;
        try { await connection.DisposeAsync().ConfigureAwait(false); } catch { /* DisposeAsync destroys/drains in its finally. */ }
    }

    private DfsCacheEntry? FindCached(string path, int minimumPrefixLength = 0)
    {
        long now = Environment.TickCount64;
        _cache.RemoveAll(entry => entry.ExpiresAt <= now);
        return _cache.Where(entry => entry.Prefix.Length >= minimumPrefixLength && IsPathPrefix(entry.Prefix, path))
            .OrderByDescending(entry => entry.Prefix.Length).FirstOrDefault();
    }

    private void Store(DfsCacheEntry entry)
    {
        _cache.RemoveAll(existing => existing.Prefix.Equals(entry.Prefix, StringComparison.OrdinalIgnoreCase));
        if (entry.ExpiresAt > Environment.TickCount64) _cache.Add(entry);
    }

    private static long ExpiresAt(uint seconds)
    {
        long duration = Math.Min((long)seconds * 1000, int.MaxValue * 1000L);
        return Environment.TickCount64 + duration;
    }

    private static bool IsPathPrefix(string prefix, string path)
        => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            (path.Length == prefix.Length || path[prefix.Length] == '\\');

    private static ResolvedDfsTarget SplitUnc(string path)
    {
        string normalized = DfsReferralCodec.NormalizeProtocolPath(path);
        string[] parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts.Any(part => part is "." or ".."))
            throw new ArgumentException("UNC path must contain valid server and share components", nameof(path));
        return new ResolvedDfsTarget(parts[0], parts[1], string.Join('/', parts.Skip(2)));
    }

    private static string TrimServer(string path)
    {
        string[] parts = DfsReferralCodec.NormalizeProtocolPath(path)
            .Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) throw new IOException("DFS referral contains an invalid server name");
        return parts[0];
    }

    private static bool CanTryAnotherServer(Exception error)
        => error is not SmbException { Status: -Libc.EACCES or -Libc.EPERM } &&
            error is IOException or TimeoutException;

    private void RequireNotDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed) return;
            _disposed = true;
            Exception? firstFailure = null;
            foreach (SmbConnection connection in _connections.Values)
            {
                try { await connection.DisposeAsync().ConfigureAwait(false); }
                catch (Exception error) { firstFailure ??= error; }
            }
            _connections.Clear();
            _cache.Clear();
            if (firstFailure != null) throw firstFailure;
        }
        finally { _gate.Release(); }
    }

    private sealed class DfsCacheEntry
    {
        public string Prefix { get; }
        public uint Flags { get; }
        public IReadOnlyList<DfsReferralEntry> Entries { get; }
        public long ExpiresAt { get; }

        public DfsCacheEntry(string prefix, uint flags, IReadOnlyList<DfsReferralEntry> entries, long expiresAt)
        {
            Prefix = prefix;
            Flags = flags;
            Entries = entries;
            ExpiresAt = expiresAt;
        }

        public IReadOnlyList<ResolvedDfsTarget> Resolve(string path)
        {
            if (!IsPathPrefix(Prefix, path)) throw new IOException("DFS referral does not match the requested path");
            string suffix = path[Prefix.Length..];
            return Entries.Select(entry => SplitUnc((entry.TargetPath ?? throw new IOException(
                "DFS referral target is missing")).TrimEnd('\\') + suffix)).ToArray();
        }
    }
}

internal sealed class ResolvedDfsTarget
{
    public string Server { get; }
    public string Share { get; }
    public string RelativePath { get; }

    public ResolvedDfsTarget(string server, string share, string relativePath)
    {
        Server = server;
        Share = share;
        RelativePath = relativePath;
    }
}

public sealed class DfsTarget
{
    public string Server { get; }
    public string Share { get; }
    public string RelativePath { get; }

    public DfsTarget(string server, string share, string relativePath)
    {
        Server = server;
        Share = share;
        RelativePath = relativePath;
    }
}
