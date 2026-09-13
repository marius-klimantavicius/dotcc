using static Managed.Transport.MsQuic;
using System.Net;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed record QuicConnectOptions
{
    /// <summary>An opaque ticket previously received from this server and ALPN.
    /// The connection factory copies it before asynchronous work begins.</summary>
    public ReadOnlyMemory<byte> ResumptionTicket { get; init; }
    /// <summary>Early data is outside the selected profile and is rejected.</summary>
    public bool EnableEarlyData { get; init; }
    public IPEndPoint? LocalEndPoint { get; init; }
    public uint LocalInterfaceIndex { get; init; }
    public bool ShareUdpBinding { get; init; }
    public QuicSettings? Settings { get; init; }
}

public enum QuicTlsProtocol { Unknown = 0, Tls13 = 0x3000 }
public enum QuicTlsCipherSuite : ushort { Aes128GcmSha256 = 0x1301, Aes256GcmSha384 = 0x1302 }
public enum QuicTlsNamedGroup : ushort { Unknown = 0, Secp256R1 = 23, Secp384R1 = 24, X25519 = 29 }
public enum QuicStreamScheduling { Fifo = 0, RoundRobin = 1 }
public enum QuicParameterPriority { Normal = 0, High = 1 }
/// <summary>Both clocks use monotonic microseconds in the selected BCL host.
/// Platform selects the actual upstream *_PLAT query, not Stopwatch ticks.</summary>
public enum QuicStatisticsClock { Microseconds = 0, Platform = 1 }
internal static class QuicParameterDispatch
{
    internal static uint Apply(uint parameter, QuicParameterPriority priority) => priority switch
    {
        QuicParameterPriority.Normal => parameter,
        QuicParameterPriority.High => parameter | (uint)MsQuic.QUIC_PARAM_HIGH_PRIORITY,
        _ => throw new ArgumentOutOfRangeException(nameof(priority))
    };
}
[Flags] public enum QuicResumptionTicketFlags { None = 0, Final = 1 }

public sealed record QuicHandshakeInformation(QuicTlsProtocol Protocol, QuicTlsCipherSuite CipherSuite,
    QuicTlsNamedGroup NamedGroup, int CipherStrengthBits, int HashStrengthBits, int KeyExchangeStrengthBits);
public sealed record QuicConnectionCapabilities(bool DatagramReceiveEnabled, bool DatagramSendEnabled,
    ushort AvailableBidirectionalStreams, ushort AvailableUnidirectionalStreams);
public sealed record QuicNetworkStatistics(uint BytesInFlight, ulong PostedBytes, ulong IdealBytes,
    ulong SmoothedRttMicroseconds, uint CongestionWindowBytes, ulong BandwidthBytesPerMicrosecond);
public sealed record QuicResumptionTicket(long Sequence, ReadOnlyMemory<byte> Bytes);
/// <summary>Upstream boundaries encoded as (maximum stream count &lt;&lt; 2) | type.
/// They are exclusive bounds for each type, not the last opened stream IDs.</summary>
public sealed record QuicMaximumStreamIds(ulong ClientBidirectional, ulong ServerBidirectional,
    ulong ClientUnidirectional, ulong ServerUnidirectional);

/// <summary>Actual translated core counters. Clock fields use monotonic
/// microseconds; zero remains an observation, not an inferred capability.</summary>
public record QuicLegacyConnectionStatistics
{
    public QuicStatisticsClock Clock { get; init; }
    public ulong CorrelationId { get; init; }
    public bool VersionNegotiation { get; init; }
    public bool StatelessRetry { get; init; }
    public bool ResumptionAttempted { get; init; }
    public bool ResumptionSucceeded { get; init; }
    public uint RttMicroseconds { get; init; }
    public uint MinimumRttMicroseconds { get; init; }
    public uint MaximumRttMicroseconds { get; init; }
    public ulong StartTimeMicroseconds { get; init; }
    public ulong InitialFlightEndMicroseconds { get; init; }
    public ulong HandshakeFlightEndMicroseconds { get; init; }
    public uint HandshakeClientFlight1Bytes { get; init; }
    public uint HandshakeServerFlight1Bytes { get; init; }
    public uint HandshakeClientFlight2Bytes { get; init; }
    public ushort PathMtu { get; init; }
    public ulong SentPackets { get; init; }
    public ulong RetransmittablePackets { get; init; }
    public ulong SentBytes { get; init; }
    public ulong SentStreamBytes { get; init; }
    public ulong SuspectedLostPackets { get; init; }
    public ulong SpuriousLostPackets { get; init; }
    public uint CongestionEvents { get; init; }
    public uint PersistentCongestionEvents { get; init; }
    public ulong ReceivedPackets { get; init; }
    public ulong ReceivedBytes { get; init; }
    public ulong ReceivedStreamBytes { get; init; }
    public ulong DroppedPackets { get; init; }
    public ulong ReorderedPackets { get; init; }
    public ulong DuplicatePackets { get; init; }
    public ulong DecryptionFailures { get; init; }
    public ulong ValidAckFrames { get; init; }
    public uint KeyUpdates { get; init; }
}

public sealed record QuicConnectionStatistics : QuicLegacyConnectionStatistics
{
    public bool GreaseBitNegotiated { get; init; }
    public bool EcnCapable { get; init; }
    public bool EncryptionOffloaded { get; init; }
    public uint CongestionWindowBytes { get; init; }
    public uint DestinationCidUpdates { get; init; }
    public uint EcnCongestionEvents { get; init; }
    public byte HandshakeHopLimit { get; init; }
    public uint RttVarianceMicroseconds { get; init; }
    public uint AverageConnectionQueueDelayMicroseconds { get; init; }
    public uint MaximumConnectionQueueDelayMicroseconds { get; init; }
    public uint AverageSendQueueDelayMicroseconds { get; init; }
    public uint MaximumSendQueueDelayMicroseconds { get; init; }
    public uint AverageReceiveQueueDelayMicroseconds { get; init; }
    public uint MaximumReceiveQueueDelayMicroseconds { get; init; }
}

public sealed partial class QuicConnection
{
    private readonly object parameterGate = new();
    private bool parametersStarted;
    private byte[] latestTicket = [];
    private long ticketSequence;
    private TaskCompletionSource ticketChanged = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Exception? ticketNotificationsClosed;

    // Main factory snapshots options before its first await, then installs this
    // copied ticket while the newly opened handle is still unpublished.
    internal static byte[] SnapshotResumptionTicket(QuicConnectOptions? options)
    {
        if (options?.EnableEarlyData == true)
            throw new NotSupportedException("0-RTT application data is outside the selected QUIC profile.");
        if (options == null || options.ResumptionTicket.IsEmpty) return [];
        if (options.ResumptionTicket.Length > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(options), "A resumption ticket must fit the upstream 16-bit envelope length.");
        return options.ResumptionTicket.ToArray();
    }

    private unsafe void ApplyResumptionTicket(byte[] ticket)
    {
        if (ticket.Length == 0) return;
        if (IsServer || parametersStarted) throw new InvalidOperationException("Tickets must be installed before starting a client connection.");
        fixed (byte* bytes = ticket)
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_RESUMPTION_TICKET,
                (uint)ticket.Length, bytes), "Set resumption ticket");
        // The upstream ticket decoder also installs its remembered version.
        // Reject a non-v1 ticket while the handle is still private to the factory.
        if (ReadParameter<uint>(MsQuic.QUIC_PARAM_CONN_QUIC_VERSION) != 1)
            throw new NotSupportedException("The resumption ticket is for a QUIC version outside the selected v1 profile.");
    }

    private sealed record PreparedConnectOptions(IPEndPoint? LocalEndPoint, uint InterfaceIndex, bool Share, QuicSettings? Settings);
    private static PreparedConnectOptions SnapshotConnectionOptions(QuicConnectOptions? options)
    {
        IPEndPoint? endpoint = options?.LocalEndPoint;
        if (endpoint != null)
        {
            if (endpoint.AddressFamily is not (System.Net.Sockets.AddressFamily.InterNetwork or System.Net.Sockets.AddressFamily.InterNetworkV6))
                throw new ArgumentException("An IPv4 or IPv6 local endpoint is required.", nameof(options));
            var address = endpoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                ? new IPAddress(endpoint.Address.GetAddressBytes(), endpoint.Address.ScopeId) : new IPAddress(endpoint.Address.GetAddressBytes());
            endpoint = new IPEndPoint(address, endpoint.Port);
        }
        return new(endpoint, options?.LocalInterfaceIndex ?? 0, options?.ShareUdpBinding ?? false, options?.Settings);
    }
    private unsafe void ApplyConnectionOptions(PreparedConnectOptions options)
    {
        if (options.LocalEndPoint != null)
        {
            QUIC_ADDR local = default; MsQuicHost.DatagramAddress(options.LocalEndPoint, &local);
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, (uint)sizeof(QUIC_ADDR), &local), "local endpoint");
        }
        if (options.InterfaceIndex != 0)
        {
            uint index = options.InterfaceIndex;
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_LOCAL_INTERFACE, sizeof(uint), &index), "local interface");
        }
        if (options.Share)
        {
            byte enabled = 1;
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_SHARE_UDP_BINDING, 1, &enabled), "shared UDP binding");
        }
        if (options.Settings != null)
        {
            QUIC_SETTINGS requested = options.Settings.ToNative();
            QuicSettings.ValidateUpdate(requested, ReadParameter<QUIC_SETTINGS>(MsQuic.QUIC_PARAM_CONN_SETTINGS));
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONN_SETTINGS,
                (uint)sizeof(QUIC_SETTINGS), &requested), "Set initial connection settings");
        }
    }
    /// <summary>Forwards the pinned core's remote-address setter. It rejects started
    /// connections with InvalidState; the initial address is selected by ConnectAsync.</summary>
    public unsafe void SetRemoteEndPoint(IPEndPoint endpoint, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        using var operation = EnterOperation();
        QUIC_ADDR remote = default; MsQuicHost.DatagramAddress(endpoint, &remote);
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, priority), (uint)sizeof(QUIC_ADDR), &remote), "remote endpoint update");
    }

    /// <summary>Requests actual client rebinding after handshake confirmation.
    /// The core preserves role/state validation and owns binding/path transitions.
    /// Port zero selects an ephemeral port; read GetLocalEndPoint afterward.</summary>
    public unsafe void SetLocalEndPoint(IPEndPoint endpoint, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        using var operation = EnterOperation();
        QUIC_ADDR local = default; MsQuicHost.DatagramAddress(endpoint, &local);
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, priority),
            (uint)sizeof(QUIC_ADDR), &local), "local endpoint update");
    }

    // Called before client ConnectionStart and before accepting a server handle.
    private void MarkParametersStarted() { lock (parameterGate) parametersStarted = true; }

    public unsafe uint GetProtocolVersion(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        // Unlike VERSION_SETTINGS, this pinned getter already swaps to host order.
        return ReadParameter<uint>(MsQuic.QUIC_PARAM_CONN_QUIC_VERSION, priority);
    }

    public IPEndPoint GetLocalEndPoint(QuicParameterPriority priority = QuicParameterPriority.Normal) => ReadEndPoint(MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, priority);
    public IPEndPoint GetRemoteEndPoint(QuicParameterPriority priority = QuicParameterPriority.Normal) => ReadEndPoint(MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, priority);
    private unsafe IPEndPoint ReadEndPoint(uint parameter, QuicParameterPriority priority)
    {
        using var operation = EnterOperation();
        QUIC_ADDR address = ReadParameter<QUIC_ADDR>(parameter, priority);
        return MsQuicHost.DatagramEndpoint(&address);
    }

    /// <summary>Returns the immutable negotiated handshake snapshot captured during
    /// CONNECTED, including when the core has retired its temporary TLS context.</summary>
    public unsafe QuicHandshakeInformation GetHandshakeInformation(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        _ = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, priority);
        lock (Gate) if (negotiatedHandshake is { } snapshot) return snapshot;
        return ConvertHandshakeInformation(ReadParameter<QUIC_HANDSHAKE_INFO>(MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, priority));
    }
    private unsafe void CaptureHandshakeInformation()
    {
        var info = ReadParameter<QUIC_HANDSHAKE_INFO>(MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, QuicParameterPriority.Normal);
        lock (Gate) negotiatedHandshake = ConvertHandshakeInformation(info);
    }
    private static QuicHandshakeInformation ConvertHandshakeInformation(QUIC_HANDSHAKE_INFO info)
        => new((QuicTlsProtocol)info.TlsProtocolVersion, (QuicTlsCipherSuite)info.CipherSuite,
            (QuicTlsNamedGroup)info.TlsGroup, info.CipherStrength, info.HashStrength, info.KeyExchangeStrength);

    public unsafe QuicConnectionCapabilities GetCapabilities(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        return new(ReadParameter<byte>(MsQuic.QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED, priority) != 0,
            ReadParameter<byte>(MsQuic.QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED, priority) != 0,
            ReadParameter<ushort>(MsQuic.QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT, priority),
            ReadParameter<ushort>(MsQuic.QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT, priority));
    }

    public unsafe QuicSettings GetSettings(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        lock (parameterGate) return QuicSettings.FromNative(ReadParameter<QUIC_SETTINGS>(MsQuic.QUIC_PARAM_CONN_SETTINGS, priority));
    }

    public unsafe void SetSettings(QuicSettings settings, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(settings);
        QUIC_SETTINGS requested = settings.ToNative();
        using var operation = EnterOperation();
        lock (parameterGate)
        {
            // Published connections have already started. Exposing updates on an
            // unstarted handle would also invoke fallible crypto reinitialization
            // after the core had changed its settings (connection.c:7721).
            if (!parametersStarted) throw new InvalidOperationException("Connection settings may be updated after starting the connection.");
            QuicSettings.ValidateUpdate(requested, ReadParameter<QUIC_SETTINGS>(MsQuic.QUIC_PARAM_CONN_SETTINGS, priority),
                allowMtuAndEcnChanges: false);
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_SETTINGS, priority),
                (uint)sizeof(QUIC_SETTINGS), &requested), "Set connection settings");
        }
    }

    public unsafe QuicStreamScheduling GetStreamScheduling(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        return (QuicStreamScheduling)ReadParameter<QUIC_STREAM_SCHEDULING_SCHEME>(MsQuic.QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME, priority);
    }
    public unsafe void SetStreamScheduling(QuicStreamScheduling scheduling, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        if (scheduling is not (QuicStreamScheduling.Fifo or QuicStreamScheduling.RoundRobin))
            throw new ArgumentOutOfRangeException(nameof(scheduling));
        using var operation = EnterOperation();
        var native = (QUIC_STREAM_SCHEDULING_SCHEME)scheduling;
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME, priority),
            (uint)sizeof(QUIC_STREAM_SCHEDULING_SCHEME), &native), "Set stream scheduling");
    }

    public unsafe QuicNetworkStatistics GetNetworkStatistics(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        var value = ReadParameter<QUIC_NETWORK_STATISTICS>(MsQuic.QUIC_PARAM_CONN_NETWORK_STATISTICS, priority);
        return new(value.BytesInFlight, value.PostedBytes, value.IdealBytes, value.SmoothedRTT, value.CongestionWindow, value.Bandwidth);
    }

    public unsafe QuicConnectionStatistics GetStatistics(QuicStatisticsClock clock = QuicStatisticsClock.Microseconds,
        QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        var value = ReadParameter<QUIC_STATISTICS_V2>(StatisticsParameter(clock,
            MsQuic.QUIC_PARAM_CONN_STATISTICS_V2, MsQuic.QUIC_PARAM_CONN_STATISTICS_V2_PLAT), priority);
        return new()
        {
            Clock = clock, CorrelationId = value.CorrelationId, VersionNegotiation = value.VersionNegotiation != 0,
            StatelessRetry = value.StatelessRetry != 0, ResumptionAttempted = value.ResumptionAttempted != 0,
            ResumptionSucceeded = value.ResumptionSucceeded != 0, EcnCapable = value.EcnCapable != 0,
            EncryptionOffloaded = value.EncryptionOffloaded != 0, RttMicroseconds = value.Rtt,
            MinimumRttMicroseconds = value.MinRtt, MaximumRttMicroseconds = value.MaxRtt,
            StartTimeMicroseconds = value.TimingStart, InitialFlightEndMicroseconds = value.TimingInitialFlightEnd,
            HandshakeFlightEndMicroseconds = value.TimingHandshakeFlightEnd, PathMtu = value.SendPathMtu,
            SentPackets = value.SendTotalPackets, SentBytes = value.SendTotalBytes, SentStreamBytes = value.SendTotalStreamBytes,
            SuspectedLostPackets = value.SendSuspectedLostPackets, SpuriousLostPackets = value.SendSpuriousLostPackets,
            CongestionEvents = value.SendCongestionCount, CongestionWindowBytes = value.SendCongestionWindow,
            ReceivedPackets = value.RecvTotalPackets, ReceivedBytes = value.RecvTotalBytes,
            ReceivedStreamBytes = value.RecvTotalStreamBytes, DroppedPackets = value.RecvDroppedPackets,
            ReorderedPackets = value.RecvReorderedPackets, DuplicatePackets = value.RecvDuplicatePackets,
            DecryptionFailures = value.RecvDecryptionFailures, KeyUpdates = value.KeyUpdateCount,
            GreaseBitNegotiated = value.GreaseBitNegotiated != 0,
            HandshakeClientFlight1Bytes = value.HandshakeClientFlight1Bytes,
            HandshakeServerFlight1Bytes = value.HandshakeServerFlight1Bytes,
            HandshakeClientFlight2Bytes = value.HandshakeClientFlight2Bytes,
            RetransmittablePackets = value.SendRetransmittablePackets,
            PersistentCongestionEvents = value.SendPersistentCongestionCount,
            ValidAckFrames = value.RecvValidAckFrames, DestinationCidUpdates = value.DestCidUpdateCount,
            EcnCongestionEvents = value.SendEcnCongestionCount, HandshakeHopLimit = value.HandshakeHopLimitTTL,
            RttVarianceMicroseconds = value.RttVariance,
            AverageConnectionQueueDelayMicroseconds = value.ConnectionQueueDelayAvgUs,
            MaximumConnectionQueueDelayMicroseconds = value.ConnectionQueueDelayMaxUs,
            AverageSendQueueDelayMicroseconds = value.SendQueueDelayAvgUs,
            MaximumSendQueueDelayMicroseconds = value.SendQueueDelayMaxUs,
            AverageReceiveQueueDelayMicroseconds = value.ReceiveQueueDelayAvgUs,
            MaximumReceiveQueueDelayMicroseconds = value.ReceiveQueueDelayMaxUs
        };
    }

    public unsafe QuicLegacyConnectionStatistics GetLegacyStatistics(QuicStatisticsClock clock = QuicStatisticsClock.Microseconds,
        QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        var value = ReadParameter<QUIC_STATISTICS>(StatisticsParameter(clock,
            MsQuic.QUIC_PARAM_CONN_STATISTICS, MsQuic.QUIC_PARAM_CONN_STATISTICS_PLAT), priority);
        return new()
        {
            Clock = clock, CorrelationId = value.CorrelationId, VersionNegotiation = value.VersionNegotiation != 0,
            StatelessRetry = value.StatelessRetry != 0, ResumptionAttempted = value.ResumptionAttempted != 0,
            ResumptionSucceeded = value.ResumptionSucceeded != 0, RttMicroseconds = value.Rtt,
            MinimumRttMicroseconds = value.MinRtt, MaximumRttMicroseconds = value.MaxRtt,
            StartTimeMicroseconds = value.Timing.Start, InitialFlightEndMicroseconds = value.Timing.InitialFlightEnd,
            HandshakeFlightEndMicroseconds = value.Timing.HandshakeFlightEnd,
            HandshakeClientFlight1Bytes = value.Handshake.ClientFlight1Bytes,
            HandshakeServerFlight1Bytes = value.Handshake.ServerFlight1Bytes,
            HandshakeClientFlight2Bytes = value.Handshake.ClientFlight2Bytes,
            PathMtu = value.Send.PathMtu, SentPackets = value.Send.TotalPackets,
            RetransmittablePackets = value.Send.RetransmittablePackets, SentBytes = value.Send.TotalBytes,
            SentStreamBytes = value.Send.TotalStreamBytes, SuspectedLostPackets = value.Send.SuspectedLostPackets,
            SpuriousLostPackets = value.Send.SpuriousLostPackets, CongestionEvents = value.Send.CongestionCount,
            PersistentCongestionEvents = value.Send.PersistentCongestionCount,
            ReceivedPackets = value.Recv.TotalPackets, ReceivedBytes = value.Recv.TotalBytes,
            ReceivedStreamBytes = value.Recv.TotalStreamBytes, DroppedPackets = value.Recv.DroppedPackets,
            ReorderedPackets = value.Recv.ReorderedPackets, DuplicatePackets = value.Recv.DuplicatePackets,
            DecryptionFailures = value.Recv.DecryptionFailures, ValidAckFrames = value.Recv.ValidAckFrames,
            KeyUpdates = value.Misc.KeyUpdateCount
        };
    }

    private static uint StatisticsParameter(QuicStatisticsClock clock, uint normal, uint platform) => clock switch
    {
        QuicStatisticsClock.Microseconds => normal,
        QuicStatisticsClock.Platform => platform,
        _ => throw new ArgumentOutOfRangeException(nameof(clock))
    };

    /// <summary>Actual logical core worker assignment; this is not OS affinity.</summary>
    public unsafe ushort GetIdealProcessor(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        return ReadParameter<ushort>(MsQuic.QUIC_PARAM_CONN_IDEAL_PROCESSOR, priority);
    }

    public unsafe bool GetUdpBindingShared(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        return ReadParameter<byte>(MsQuic.QUIC_PARAM_CONN_SHARE_UDP_BINDING, priority) != 0;
    }

    public unsafe QuicMaximumStreamIds GetMaximumStreamIds(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        ulong* values = stackalloc ulong[4]; uint length = 4 * sizeof(ulong);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle,
            QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_MAX_STREAM_IDS, priority), &length, values), "Read maximum stream IDs");
        if (length != 4 * sizeof(ulong)) throw new InvalidOperationException("Unexpected stream ID boundary count.");
        return new(values[0], values[1], values[2], values[3]);
    }

    public unsafe ReadOnlyMemory<byte> GetOriginalDestinationConnectionId(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        byte* bytes = stackalloc byte[MsQuic.QUIC_MAX_CONNECTION_ID_LENGTH_V1];
        uint length = MsQuic.QUIC_MAX_CONNECTION_ID_LENGTH_V1;
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle,
            QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_ORIG_DEST_CID, priority), &length, bytes), "Read original destination CID");
        if (length > MsQuic.QUIC_MAX_CONNECTION_ID_LENGTH_V1) throw new InvalidOperationException("Invalid QUIC v1 CID length.");
        return new ReadOnlySpan<byte>(bytes, (int)length).ToArray();
    }

    /// <summary>Copies the reason bytes without the C terminator. Embedded NUL is
    /// invalid. The complete encoded value is limited to the upstream 512 bytes.</summary>
    public unsafe void SetCloseReasonPhrase(ReadOnlySpan<byte> phrase,
        QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ValidateCloseReasonPhrase(phrase);
        uint parameter = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_CLOSE_REASON_PHRASE, priority);
        using var operation = EnterOperation();
        byte* bytes = stackalloc byte[phrase.Length + 1];
        phrase.CopyTo(new Span<byte>(bytes, phrase.Length)); bytes[phrase.Length] = 0;
        lock (parameterGate)
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, parameter, (uint)phrase.Length + 1, bytes), "Set close reason phrase");
    }

    internal static void ValidateCloseReasonPhrase(ReadOnlySpan<byte> phrase)
    {
        if (phrase.Length >= MsQuic.QUIC_MAX_CONN_CLOSE_REASON_LENGTH || phrase.Contains((byte)0))
            throw new ArgumentException("Close reason must contain at most 511 non-NUL bytes.", nameof(phrase));
    }

    /// <summary>Returns copied reason bytes, or null when the core has no phrase.
    /// Empty is a configured empty phrase; it is distinct from an absent phrase.</summary>
    public unsafe ReadOnlyMemory<byte>? GetCloseReasonPhrase(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        byte* bytes = stackalloc byte[MsQuic.QUIC_MAX_CONN_CLOSE_REASON_LENGTH];
        uint length = MsQuic.QUIC_MAX_CONN_CLOSE_REASON_LENGTH;
        uint status;
        lock (parameterGate)
            status = Runtime.Api->GetParam(Handle,
                QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_CLOSE_REASON_PHRASE, priority), &length, bytes);
        if (status == Status.NotFound) return null;
        QuicError.ThrowIfFailed(status, "Read close reason phrase");
        if (length is 0 or > MsQuic.QUIC_MAX_CONN_CLOSE_REASON_LENGTH || bytes[length - 1] != 0)
            throw new InvalidOperationException("Invalid upstream close reason termination.");
        return new ReadOnlySpan<byte>(bytes, (int)length - 1).ToArray();
    }

    private unsafe T ReadParameter<T>(uint parameter, QuicParameterPriority priority = QuicParameterPriority.Normal) where T : unmanaged
    {
        T value = default; uint length = (uint)sizeof(T);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, QuicParameterDispatch.Apply(parameter, priority), &length, &value), "Read connection parameter");
        if (length != sizeof(T)) throw new InvalidOperationException("Unexpected pinned connection parameter size.");
        return value;
    }

    /// <summary>Queues an actual post-handshake ticket. Successful return means
    /// accepted by the core; it does not acknowledge delivery to the peer.</summary>
    public unsafe void SendResumptionTicket(ReadOnlySpan<byte> applicationData = default,
        QuicResumptionTicketFlags flags = QuicResumptionTicketFlags.None)
    {
        if (applicationData.Length > MsQuic.QUIC_MAX_RESUMPTION_APP_DATA_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(applicationData));
        if (flags is not (QuicResumptionTicketFlags.None or QuicResumptionTicketFlags.Final))
            throw new ArgumentOutOfRangeException(nameof(flags));
        if (!IsServer) throw new InvalidOperationException("Only servers issue resumption tickets.");
        using var operation = EnterOperation();
        // The actual API copies applicationData into its queued operation before
        // returning; it does not retain this borrowed span.
        fixed (byte* bytes = applicationData)
            QuicError.ThrowIfFailed(Runtime.Api->ConnectionSendResumptionTicket(Handle,
                (QUIC_SEND_RESUMPTION_FLAGS)flags, (ushort)applicationData.Length, bytes), "Send resumption ticket");
    }

    /// <summary>Returns a copied ticket newer than the supplied sequence. Only
    /// the latest ticket is retained; intermediate tickets may be coalesced.</summary>
    public async ValueTask<QuicResumptionTicket> WaitForResumptionTicketAsync(long afterSequence = 0,
        CancellationToken cancellationToken = default)
    {
        if (afterSequence < 0) throw new ArgumentOutOfRangeException(nameof(afterSequence));
        if (IsServer) throw new InvalidOperationException("Only clients receive resumable connection tickets.");
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Task changed;
            lock (Gate)
            {
                if (ticketSequence > afterSequence) return new(ticketSequence, latestTicket.ToArray());
                if (ticketNotificationsClosed != null) throw ticketNotificationsClosed;
                ObjectDisposedException.ThrowIf(IsClosing, this);
                changed = ticketChanged.Task;
            }
            await changed.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private unsafe void OnResumptionTicketReceived(byte* bytes, uint length)
    {
        if (bytes == null || length is 0 or > ushort.MaxValue)
            throw new InvalidOperationException("Invalid upstream resumption ticket notification.");
        byte[] snapshot = new ReadOnlySpan<byte>(bytes, (int)length).ToArray();
        lock (Gate)
        {
            if (IsClosing || ticketNotificationsClosed != null) return;
            var signal = ticketChanged;
            ticketChanged = new(TaskCreationOptions.RunContinuationsAsynchronously);
            latestTicket = snapshot; ticketSequence = checked(ticketSequence + 1);
            signal.TrySetResult();
        }
    }

    private void CompleteParameterNotifications(Exception? error = null)
    {
        lock (Gate)
        {
            ticketNotificationsClosed ??= error ?? new IOException("The connection closed before another ticket arrived.");
            ticketChanged.TrySetResult();
        }
    }
}
