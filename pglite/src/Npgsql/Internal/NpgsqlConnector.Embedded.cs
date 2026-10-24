using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Managed.PGlite;
using Backend = Managed.PGlite.Translated.PGlite;

namespace Npgsql.Internal;

public sealed partial class NpgsqlConnector
{
    internal bool IsEmbedded => DataSource is NpgsqlEmbeddedDataSource;
    internal Exception? EmbeddedCleanupError { get; private set; }
    internal bool EmbeddedEngineClosed { get; private set; }

    async Task OpenEmbedded(CancellationToken cancellationToken)
    {
        EmbeddedPolicy.CheckCancellation(cancellationToken);
        if (Settings.Pooling || Settings.Multiplexing || Settings.KeepAlive != 0 || Settings.Enlist ||
            Settings.Timeout != 0 || Settings.CommandTimeout != 0)
            throw new NotSupportedException("Embedded sessions require pooling, multiplexing, enlistment, keepalive and all client timeouts to be disabled.");
        var stream = await ((NpgsqlEmbeddedDataSource)DataSource).OpenStream(cancellationToken).ConfigureAwait(false);
        _stream = stream;
        await stream.Worker.InvokeAsync(db => db.InvokeBackend(() =>
        {
            EmbeddedCopyGuard.Install();
            // --single startup predates remote protocol attachment. Enable the
            // real engine's ParameterStatus reporting now, so later SET/RESET
            // updates Npgsql's parser and session metadata through raw frames.
            Backend.BeginReportingGUCOptions();
            Backend.dpg_flush();
        })).ConfigureAwait(false);
        TextEncoding = NpgsqlWriteBuffer.UTF8Encoding;
        RelaxedTextEncoding = NpgsqlWriteBuffer.RelaxedUTF8Encoding;
        ReadBuffer = new NpgsqlReadBuffer(this, stream, null, Settings.ReadBufferSize, TextEncoding, RelaxedTextEncoding);
        WriteBuffer = new NpgsqlWriteBuffer(this, stream, null, Settings.WriteBufferSize, TextEncoding);

        // Read values from the real initialized backend. Do not send a fake
        // startup/authentication exchange or manufacture BackendKeyData.
        var parameters = await stream.Worker.InvokeAsync(db => db.Query(
            "SELECT name, pg_catalog.current_setting(name, true) FROM unnest(ARRAY[" +
            "'server_version','server_encoding','client_encoding','application_name','is_superuser'," +
            "'integer_datetimes','DateStyle','TimeZone','standard_conforming_strings'," +
            "'session_authorization','in_hot_standby','default_transaction_read_only','scram_iterations']) AS settings(name)")
            .EnsureSuccess()).ConfigureAwait(false);
        foreach (var row in parameters.ResultSets[0].Rows)
            if (row[1] != null)
                ReadParameterStatus(Encoding.UTF8.GetBytes(row[0]!), Encoding.UTF8.GetBytes(row[1]!));
        var identity = await stream.Worker.InvokeAsync(db => db.Query(
            "SELECT session_user::text, current_user::text").EnsureSuccess()).ConfigureAwait(false);
        string actualUser = identity.ResultSets[0].Rows[0][0]!;
        if (actualUser != Settings.Username)
            throw new NpgsqlException($"Configured embedded Username must match the existing trusted session_user ({actualUser}).");
        InferredUserName = actualUser;
        State = ConnectorState.Ready;
    }
}
