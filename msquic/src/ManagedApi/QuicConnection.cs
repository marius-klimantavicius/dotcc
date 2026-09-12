using System.Net;
using System.Text;
using System.Threading.Channels;
using Managed.Transport.Hosting;
using static Managed.Transport.QUIC_CONNECTION_EVENT_TYPE;

namespace Managed.Transport.Api;

public sealed partial class QuicConnection : QuicObject
{
    private readonly QuicRegistration registration;
    private readonly QuicConfiguration configuration;
    private IDisposable? configurationLease;
    private readonly HashSet<QuicStream> streams = [];
    private readonly Channel<QuicStream> accepted = Channel.CreateBounded<QuicStream>(new BoundedChannelOptions(256) { FullMode = BoundedChannelFullMode.Wait });
    private readonly TaskCompletionSource connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource shutdown = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource validationLifetime = new();
    private readonly CancellationToken validationToken;
    private long bufferedBytes;
    private byte[] negotiatedAlpn = [];
    private QuicHandshakeInformation? negotiatedHandshake;
    private QuicCloseInfo? closeInfo;
    private QuicConnection(QuicRegistration registration, QuicConfiguration configuration) : base(registration.Runtime)
    { this.registration = registration; this.configuration = configuration; validationToken = validationLifetime.Token; }
    internal unsafe QUIC_HANDLE* StreamParentHandle => Handle;
    internal Task ConnectedCompletion => connected.Task;
    internal Task ShutdownCompletion => shutdown.Task;
    public ReadOnlyMemory<byte> NegotiatedApplicationProtocol => negotiatedAlpn;
    public QuicCloseInfo? CloseInfo { get { lock (Gate) return closeInfo; } }
    public bool IsServer => configuration.IsServer;

    private static void ValidateConfiguration(QuicRegistration owner, QuicConfiguration config, bool server)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!config.BelongsTo(owner)) throw new ArgumentException("Configuration belongs to another registration.", nameof(config));
        if (config.IsServer != server) throw new ArgumentException("Configuration has the wrong credential role.", nameof(config));
    }

    internal static async ValueTask<QuicConnection> ConnectAsync(QuicRegistration owner, QuicConfiguration config,
        string serverName, IPEndPoint endpoint, CancellationToken cancellationToken, QuicConnectOptions? options = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateConfiguration(owner, config, false);
        ArgumentException.ThrowIfNullOrEmpty(serverName); ArgumentNullException.ThrowIfNull(endpoint);
        if (serverName.Contains('\0')) throw new ArgumentException("Server name contains NUL.", nameof(serverName));
        byte[] name = Encoding.UTF8.GetBytes(serverName + "\0");
        if (name.Length > 256 || endpoint.Port == 0) throw new ArgumentOutOfRangeException(nameof(endpoint));
        byte[] ticket = SnapshotResumptionTicket(options);
        var preparedOptions = SnapshotConnectionOptions(options);
        var connection = new QuicConnection(owner, config);
        try
        {
            using (owner.AdmitConnection(connection))
            using (connection.EnterOperation())
            {
                connection.configurationLease = config.AcquireLease();
                connection.OpenAndStart(name, endpoint, ticket, preparedOptions);
            }
            await connection.connected.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch { await connection.DisposeAsync().ConfigureAwait(false); throw; }
    }

    private unsafe void OpenAndStart(byte[] name, IPEndPoint endpoint, byte[] ticket, PreparedConnectOptions options)
    {
        QUIC_HANDLE* handle = null;
        QuicError.ThrowIfFailed(Runtime.Api->ConnectionOpen(registration.ConnectionParentHandle, &Callback, Context, &handle), "ConnectionOpen");
        Handle = handle;
        QuicRuntime.SetVersionOne(Runtime.Api, Handle, MsQuic.QUIC_PARAM_CONN_VERSION_SETTINGS);
        QUIC_ADDR address = default; MsQuicHost.DatagramAddress(endpoint, &address);
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, (uint)sizeof(QUIC_ADDR), &address), "remote address");
        ApplyConnectionOptions(options);
        ApplyResumptionTicket(ticket);
        MarkParametersStarted();
        fixed (byte* hostname = name)
            QuicError.ThrowIfFailed(Runtime.Api->ConnectionStart(Handle, configuration.Handle, address.Ip.sa_family, hostname, (ushort)endpoint.Port), "ConnectionStart");
    }

    internal static unsafe QuicConnection Accept(QuicRegistration owner, QuicConfiguration config, QUIC_HANDLE* handle)
    {
        ValidateConfiguration(owner, config, true);
        var connection = new QuicConnection(owner, config);
        try
        {
            using var admission = owner.AdmitConnection(connection);
            using var initialization = connection.EnterOperation();
            connection.configurationLease = config.AcquireLease();
            void* callbackContext = connection.Context;
            // All allocating admission steps precede this ownership transfer.
            connection.Handle = handle;
            owner.Runtime.Api->SetCallbackHandler(handle, (void*)(delegate*<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, uint>)&Callback, callbackContext);
            connection.MarkParametersStarted();
            uint status = owner.Runtime.Api->ConnectionSetConfiguration(handle, config.Handle);
            if (QuicError.Failed(status)) connection.Fault(QuicError.FromStatus(status, "ConnectionSetConfiguration"));
            return connection;
        }
        catch (Exception error)
        {
            if (connection.Handle != null) { connection.Fault(error); return connection; }
            connection.configurationLease?.Dispose(); owner.ConnectionClosed(connection); connection.RetireContext(); connection.validationLifetime.Dispose();
            throw;
        }
    }

    internal IDisposable AdmitStream(QuicStream child)
    { var lease = EnterOperation(); try { lock (Gate) streams.Add(child); return lease; } catch { lease.Dispose(); throw; } }
    internal void StreamClosed(QuicStream child) { lock (Gate) streams.Remove(child); }
    internal bool TryReserveBufferBytes(long count, out IDisposable? reservation)
    {
        reservation = null;
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        lock (Gate)
        {
            if (IsClosing || count > Runtime.Options.ConnectionBufferBudget - bufferedBytes) return false;
            if (!Runtime.TryReserveBufferBytes(count, out var runtimeReservation)) return false;
            try
            {
                reservation = new QuicBudgetLease(() => { lock (Gate) bufferedBytes -= count; runtimeReservation!.Dispose(); });
                bufferedBytes += count; return true;
            }
            catch { runtimeReservation!.Dispose(); throw; }
        }
    }

    /// <summary>Creates an owned local stream without starting it. Call StartAsync
    /// or send with Start to admit it to the peer.</summary>
    public ValueTask<QuicStream> CreateStreamAsync(QuicStreamOpenOptions options = 0, CancellationToken cancellationToken = default)
        => QuicStream.CreateAsync(this, options, cancellationToken);
    public ValueTask<QuicStream> OpenStreamAsync(QuicStreamOpenOptions options = 0,
        QuicStreamStartOptions startOptions = QuicStreamStartOptions.Immediate, CancellationToken cancellationToken = default)
        => QuicStream.OpenAsync(this, options, startOptions, cancellationToken);
    public ValueTask<QuicStream> AcceptStreamAsync(CancellationToken cancellationToken = default)
        => accepted.Reader.ReadAsync(cancellationToken);
    public async Task ShutdownAsync(ulong applicationError = 0, CancellationToken cancellationToken = default,
        QuicConnectionShutdownOptions options = QuicConnectionShutdownOptions.None)
    {
        QuicError.ErrorCode(applicationError);
        if (options is not (QuicConnectionShutdownOptions.None or QuicConnectionShutdownOptions.Silent))
            throw new ArgumentOutOfRangeException(nameof(options));
        using (EnterOperation()) await Runtime.RunCleanup(() => ShutdownNative(applicationError, options)).ConfigureAwait(false);
        await shutdown.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    private unsafe void ShutdownNative(ulong error, QuicConnectionShutdownOptions options = QuicConnectionShutdownOptions.None)
    { if (Handle != null) Runtime.Api->ConnectionShutdown(Handle, (QUIC_CONNECTION_SHUTDOWN_FLAGS)options, error); }
    internal void Fault(Exception error)
    {
        RecordCallbackFailure(error); connected.TrySetException(error); accepted.Writer.TryComplete(error);
        _ = DisposeAsync().AsTask();
    }

    private static unsafe uint Callback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* notification)
    {
        var owner = FromContext<QuicConnection>(context);
        try
        {
            switch (notification->Type)
            {
                case QUIC_CONNECTION_EVENT_CONNECTED:
                    var value = notification->CONNECTED;
                    owner.negotiatedAlpn = new ReadOnlySpan<byte>(value.NegotiatedAlpn, value.NegotiatedAlpnLength).ToArray();
                    // The pinned server retires TLS immediately after CONNECTED
                    // when resumption is disabled. Snapshot before publishing.
                    owner.CaptureHandshakeInformation();
                    owner.connected.TrySetResult(); break;
                case QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED:
                    var offered = notification->PEER_STREAM_STARTED;
                    if (owner.configuration.DelayAcceptedStreamCreditUntilClose)
                    {
                        offered.Flags |= QUIC_STREAM_OPEN_FLAGS.QUIC_STREAM_OPEN_FLAG_DELAY_ID_FC_UPDATES;
                        notification->PEER_STREAM_STARTED = offered;
                    }
                    QuicStream stream;
                    try { stream = QuicStream.Accept(owner, offered.Stream, offered.Flags); }
                    catch { return Status.ConnectionRefused; } // Core still owns and closes the rejected raw stream.
                    if (!owner.accepted.Writer.TryWrite(stream)) _ = stream.DisposeAsync().AsTask();
                    break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
                    var transport = notification->SHUTDOWN_INITIATED_BY_TRANSPORT;
                    lock (owner.Gate) owner.closeInfo = new(transport.Status, transport.ErrorCode, false, false);
                    owner.connected.TrySetException(new QuicTransportException("Transport closed", transport.Status, transport.ErrorCode));
                    break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER:
                    var peer = notification->SHUTDOWN_INITIATED_BY_PEER;
                    lock (owner.Gate) owner.closeInfo = new(0, peer.ErrorCode, true, true);
                    owner.connected.TrySetException(new IOException("Peer closed before connection establishment.")); break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                    owner.connected.TrySetException(new IOException("Connection shut down before establishment."));
                    owner.accepted.Writer.TryComplete(); owner.CloseResumptionValidation(); owner.CompleteParameterNotifications(); owner.shutdown.TrySetResult(); break;
                case QUIC_CONNECTION_EVENT_RESUMED:
                    var resumed = notification->RESUMED;
                    return owner.ValidateResumptionState(resumed.ResumptionState, resumed.ResumptionStateLength);
                case QUIC_CONNECTION_EVENT_RESUMPTION_TICKET_RECEIVED:
                    var ticket = notification->RESUMPTION_TICKET_RECEIVED;
                    owner.OnResumptionTicketReceived(ticket.ResumptionTicket, ticket.ResumptionTicketLength); break;
                case QUIC_CONNECTION_EVENT_DATAGRAM_STATE_CHANGED:
                    var state = notification->DATAGRAM_STATE_CHANGED;
                    lock (owner.Gate) { owner.datagramSendEnabled = state.SendEnabled != 0; owner.maximumDatagramLength = state.MaxSendLength; }
                    break;
                case QUIC_CONNECTION_EVENT_DATAGRAM_RECEIVED:
                    owner.ReceiveDatagram(notification->DATAGRAM_RECEIVED.Buffer); break;
                case QUIC_CONNECTION_EVENT_DATAGRAM_SEND_STATE_CHANGED:
                    var sent = notification->DATAGRAM_SEND_STATE_CHANGED;
                    owner.DatagramState(sent.ClientContext, sent.State); break;
                case QUIC_CONNECTION_EVENT_PEER_CERTIFICATE_RECEIVED:
                    var certificate = notification->PEER_CERTIFICATE_RECEIVED;
                    var observation = new QuicCertificateValidation(CopyCertificate((QUIC_BUFFER*)certificate.Certificate),
                        CopyCertificate((QUIC_BUFFER*)certificate.Chain), certificate.DeferredStatus);
                    _ = Task.Run(() => owner.ValidateCertificateAsync(observation));
                    return Status.Pending;
            }
            return Status.Success;
        }
        catch (Exception error) { owner.Fault(error); return Status.InternalError; }
    }

    private static unsafe byte[] CopyCertificate(QUIC_BUFFER* buffer)
    { if (buffer == null) return []; return new ReadOnlySpan<byte>(buffer->Buffer, checked((int)buffer->Length)).ToArray(); }
    private async Task ValidateCertificateAsync(QuicCertificateValidation observation)
    {
        bool providerFailed = QuicError.Failed(observation.ValidationStatus);
        if (providerFailed)
        {
            // The core pauses TLS result processing while certificate approval
            // is pending. Release it immediately with the provider's real alert;
            // arbitrary delayed application observation cannot override trust.
            uint status = observation.ValidationStatus;
            ushort alert = status switch
            {
                Status.CertificateExpired => 45,
                Status.CertificateUntrustedRoot => 48,
                Status.CertificateMissing => 116,
                >= 200000256 and <= 200000511 => (ushort)(status - 200000256),
                _ => 80
            };
            FinishCertificateApproval(false, alert);
        }
        bool approve = false;
        try
        {
            var policy = configuration.CertificateValidation;
            bool applicationApproved = policy != null && await policy(observation, validationToken).ConfigureAwait(false);
            approve = applicationApproved && !providerFailed;
        }
        catch (Exception error) { RecordCallbackFailure(error); }
        if (!providerFailed) FinishCertificateApproval(approve, 42);
    }
    private void FinishCertificateApproval(bool approve, ushort alert)
    {
        try { using var operation = EnterOperation(); CompleteCertificate(approve, alert); }
        catch (ObjectDisposedException) { }
        catch (Exception error) { Fault(error); }
    }
    private async Task CancelCertificateValidationAsync()
    {
        try { await validationLifetime.CancelAsync().ConfigureAwait(false); }
        catch (Exception error) { RecordCallbackFailure(error); }
        finally { validationLifetime.Dispose(); }
    }
    private unsafe void CompleteCertificate(bool approve, ushort alert)
        => QuicError.ThrowIfFailed(Runtime.Api->ConnectionCertificateValidationComplete(Handle, approve ? (byte)1 : (byte)0,
            (QUIC_TLS_ALERT_CODES)alert), "certificate validation completion");
    private unsafe void CloseNative()
    { if (Handle != null) { Runtime.Api->ConnectionClose(Handle); Handle = null; } }
    public override ValueTask DisposeAsync()
    {
        // Wake pending receives before waiting for operation leases; handed-off
        // items release their reservation before their operation can drain.
        datagrams.Writer.TryComplete(new ObjectDisposedException(nameof(QuicConnection)));
        CloseResumptionValidation();
        return new(CloseOnce(async () =>
    {
        _ = CancelCertificateValidationAsync();
        CompleteParameterNotifications(new ObjectDisposedException(nameof(QuicConnection)));
        accepted.Writer.TryComplete(new ObjectDisposedException(nameof(QuicConnection)));
        if (HasNativeHandle) await Runtime.RunCleanup(() => ShutdownNative(0)).ConfigureAwait(false);
        QuicStream[] children; lock (Gate) children = streams.ToArray();
        await Task.WhenAll(children.Select(child => child.DisposeAsync().AsTask())).ConfigureAwait(false);
        if (HasNativeHandle) await Runtime.RunCleanup(CloseNative).ConfigureAwait(false);
        DrainDatagrams();
        connected.TrySetException(new ObjectDisposedException(nameof(QuicConnection))); shutdown.TrySetResult();
        RetireContext(); configurationLease?.Dispose(); configurationLease = null; registration.ConnectionClosed(this);
        lock (Gate) if (bufferedBytes != 0) throw new InvalidOperationException("Connection closed with outstanding copied buffers.");
        // Policies may ignore cancellation; they retain only managed copied certificate data.
    }));
    }
}
