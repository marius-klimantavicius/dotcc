using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Managed.PGlite;
using Npgsql.Internal;
using Npgsql.Util;

namespace Npgsql;

/// <summary>A single exclusively leased embedded PostgreSQL session, backed by
/// the translated Managed.PGlite engine on its dedicated owner thread.</summary>
public sealed class NpgsqlEmbeddedDataSource : NpgsqlDataSource
{
    readonly PgliteOptions _options;
    readonly object _leaseLock = new();
    sealed class Lease { internal bool Started, Closed; }
    Lease? _lease;
    bool _create;

    NpgsqlEmbeddedDataSource(PgliteOptions options, bool create,
        NpgsqlConnectionStringBuilder settings, NpgsqlDataSourceConfiguration config)
        : base(settings, config, reportMetrics: true)
    { _options = options; _create = create; }

    /// <summary>Create a data source. Engine initialization is deferred until
    /// the first connection opens. Closing that connection destroys its session;
    /// later opens use the persistent cluster in a fresh engine lifetime.</summary>
    public static NpgsqlEmbeddedDataSource Create(PgliteOptions options, bool create = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        var builder = new NpgsqlSlimDataSourceBuilder();
        var settings = builder.ConnectionStringBuilder;
        settings.Host = "embedded";
        settings.Username = options.Configuration.Username;
        settings.Database = options.Configuration.Database;
        settings.Pooling = false;
        settings.Multiplexing = false;
        settings.Enlist = false;
        settings.IncludeFailedBatchedCommand = true;
        settings.Timeout = 0;
        settings.CommandTimeout = 0;
        settings.SslMode = SslMode.Disable;
        settings.GssEncryptionMode = GssEncryptionMode.Disable;
        builder.EnableArrays();
        var (validated, config) = builder.PrepareConfiguration();
        return new(options, create, validated, config);
    }

    internal override (int Total, int Idle, int Busy) Statistics
    { get { lock (_leaseLock) return _lease != null ? (1, 0, 1) : (0, 0, 0); } }
    internal override bool OwnsConnectors => true;

    internal async Task<EmbeddedStream> OpenStream(CancellationToken cancellationToken)
    {
        EmbeddedPolicy.CheckCancellation(cancellationToken);
        Lease lease;
        lock (_leaseLock)
        {
            lease = _lease ?? throw new InvalidOperationException("No embedded lease is active.");
            lease.Started = true;
        }
        var worker = _create ? PgliteWorker.Create(_options) : PgliteWorker.Open(_options);
        try
        {
            await worker.WaitReady().ConfigureAwait(false);
            _create = false;
            return new EmbeddedStream(worker, () => ReleaseLease(lease));
        }
        catch
        {
            // Drain/close before releasing exclusivity. Failed core shutdown
            // keeps the underlying process owner quarantined in Managed.PGlite.
            await worker.CloseAsync().ConfigureAwait(false);
            lease.Closed = true;
            throw;
        }
    }

    internal override async ValueTask<NpgsqlConnector> Get(
        NpgsqlConnection connection, NpgsqlTimeout timeout, bool async, CancellationToken cancellationToken)
    {
        var lease = new Lease();
        lock (_leaseLock)
        {
            CheckDisposed();
            if (_lease != null) throw new InvalidOperationException("The embedded data source already has an open connection. Close it before opening another session.");
            _lease = lease;
        }
        try
        {
            var connector = new NpgsqlConnector(this, connection);
            await connector.Open(timeout, async, cancellationToken).ConfigureAwait(false);
            return connector;
        }
        catch
        {
            lock (_leaseLock)
                if (_lease == lease && (!lease.Started || lease.Closed)) _lease = null;
            throw;
        }
    }
    void ReleaseLease(Lease lease)
    {
        lock (_leaseLock)
        {
            lease.Closed = true;
            if (_lease == lease) _lease = null;
        }
    }
    internal override bool TryGetIdleConnector([NotNullWhen(true)] out NpgsqlConnector? connector)
    { connector = null; return false; }
    internal override ValueTask<NpgsqlConnector?> OpenNewConnector(NpgsqlConnection connection,
        NpgsqlTimeout timeout, bool async, CancellationToken cancellationToken) => new((NpgsqlConnector?)null);
    internal override void Return(NpgsqlConnector connector) => connector.Close();
    /// <summary>No pooled sessions exist. An active connection retains ownership
    /// until it closes, including when its data source has been disposed.</summary>
    public override void Clear() { }
}
