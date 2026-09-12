namespace Managed.Transport.Api;

public enum QuicCongestionControl : ushort { Cubic = 0 }
public enum QuicServerResumption : byte { Disabled = 0, Tickets = 1 }

/// <summary>Typed selected settings. Unspecified values retain upstream defaults.</summary>
public sealed record QuicSettings
{
    public ulong? MaxBytesPerKey { get; init; }
    public ulong? HandshakeIdleTimeoutMs { get; init; }
    public ulong? IdleTimeoutMs { get; init; }
    public ulong? MtuDiscoverySearchCompleteTimeoutUs { get; init; }
    public uint? TlsClientMaxSendBuffer { get; init; }
    public uint? TlsServerMaxSendBuffer { get; init; }
    public uint? StreamRecvWindowDefault { get; init; }
    public uint? StreamRecvBufferDefault { get; init; }
    public uint? ConnFlowControlWindow { get; init; }
    public uint? MaxWorkerQueueDelayUs { get; init; }
    public uint? MaxStatelessOperations { get; init; }
    public uint? InitialWindowPackets { get; init; }
    public uint? SendIdleTimeoutMs { get; init; }
    public uint? InitialRttMs { get; init; }
    public uint? MaxAckDelayMs { get; init; }
    public uint? DisconnectTimeoutMs { get; init; }
    public uint? KeepAliveIntervalMs { get; init; }
    public ushort? PeerBidiStreamCount { get; init; }
    public ushort? PeerUnidiStreamCount { get; init; }
    public ushort? MaxBindingStatelessOperations { get; init; }
    public ushort? StatelessOperationExpirationMs { get; init; }
    public ushort? MinimumMtu { get; init; }
    public ushort? MaximumMtu { get; init; }
    public bool? SendBufferingEnabled { get; init; }
    public bool? PacingEnabled { get; init; }
    public bool? MigrationEnabled { get; init; }
    public bool? DatagramReceiveEnabled { get; init; }
    public byte? MaxOperationsPerDrain { get; init; }
    public byte? MtuDiscoveryMissingProbeCount { get; init; }
    public uint? DestCidUpdateIdleTimeoutMs { get; init; }
    public bool? GreaseQuicBitEnabled { get; init; }
    public bool? HyStartEnabled { get; init; }
    public uint? StreamRecvWindowBidiLocalDefault { get; init; }
    public uint? StreamRecvWindowBidiRemoteDefault { get; init; }
    public uint? StreamRecvWindowUnidiDefault { get; init; }
    public QuicCongestionControl? CongestionControlAlgorithm { get; init; }
    public QuicServerResumption? ServerResumptionLevel { get; init; }
    public bool? EncryptionOffloadAllowed { get; init; }
    public bool? ReliableResetEnabled { get; init; }
    public bool? OneWayDelayEnabled { get; init; }
    public bool? NetStatsEventEnabled { get; init; }
    public bool? StreamMultiReceiveEnabled { get; init; }
    public bool? XdpEnabled { get; init; }
    public bool? QTIPEnabled { get; init; }
    public bool? ReservedRioEnabled { get; init; }
    public bool? EcnEnabled { get; init; }

    internal unsafe QUIC_SETTINGS ToNative(bool initialize = false)
    {
        if (EncryptionOffloadAllowed == true) throw new NotSupportedException("EncryptionOffloadAllowed is outside the qualified QUIC profile.");
        if (ReliableResetEnabled == true) throw new NotSupportedException("ReliableResetEnabled is outside the qualified QUIC profile.");
        if (OneWayDelayEnabled == true) throw new NotSupportedException("OneWayDelayEnabled is outside the qualified QUIC profile.");
        if (NetStatsEventEnabled == true) throw new NotSupportedException("NetStatsEventEnabled is outside the qualified QUIC profile.");
        if (StreamMultiReceiveEnabled == true) throw new NotSupportedException("StreamMultiReceiveEnabled is outside the qualified QUIC profile.");
        if (XdpEnabled == true) throw new NotSupportedException("XdpEnabled is outside the qualified QUIC profile.");
        if (QTIPEnabled == true) throw new NotSupportedException("QTIPEnabled is outside the qualified QUIC profile.");
        if (ReservedRioEnabled == true) throw new NotSupportedException("ReservedRioEnabled is outside the qualified QUIC profile.");
        if (EcnEnabled == true) throw new NotSupportedException("EcnEnabled is outside the qualified QUIC profile.");
        if (CongestionControlAlgorithm is not (null or QuicCongestionControl.Cubic)) throw new NotSupportedException("Only CUBIC is qualified.");
        if (ServerResumptionLevel is not (null or QuicServerResumption.Disabled or QuicServerResumption.Tickets)) throw new NotSupportedException("Early data is not qualified.");
        QUIC_SETTINGS value = default;
        var present = value.IsSet;
        if (MaxBytesPerKey is { } MaxBytesPerKeyValue) { present.MaxBytesPerKey = 1; value.MaxBytesPerKey = (ulong)MaxBytesPerKeyValue; }
        if (HandshakeIdleTimeoutMs is { } HandshakeIdleTimeoutMsValue) { present.HandshakeIdleTimeoutMs = 1; value.HandshakeIdleTimeoutMs = (ulong)HandshakeIdleTimeoutMsValue; }
        if (IdleTimeoutMs is { } IdleTimeoutMsValue) { present.IdleTimeoutMs = 1; value.IdleTimeoutMs = (ulong)IdleTimeoutMsValue; }
        if (MtuDiscoverySearchCompleteTimeoutUs is { } MtuDiscoverySearchCompleteTimeoutUsValue) { present.MtuDiscoverySearchCompleteTimeoutUs = 1; value.MtuDiscoverySearchCompleteTimeoutUs = (ulong)MtuDiscoverySearchCompleteTimeoutUsValue; }
        if (TlsClientMaxSendBuffer is { } TlsClientMaxSendBufferValue) { present.TlsClientMaxSendBuffer = 1; value.TlsClientMaxSendBuffer = (uint)TlsClientMaxSendBufferValue; }
        if (TlsServerMaxSendBuffer is { } TlsServerMaxSendBufferValue) { present.TlsServerMaxSendBuffer = 1; value.TlsServerMaxSendBuffer = (uint)TlsServerMaxSendBufferValue; }
        if (StreamRecvWindowDefault is { } StreamRecvWindowDefaultValue) { present.StreamRecvWindowDefault = 1; value.StreamRecvWindowDefault = (uint)StreamRecvWindowDefaultValue; }
        if (StreamRecvBufferDefault is { } StreamRecvBufferDefaultValue) { present.StreamRecvBufferDefault = 1; value.StreamRecvBufferDefault = (uint)StreamRecvBufferDefaultValue; }
        if (ConnFlowControlWindow is { } ConnFlowControlWindowValue) { present.ConnFlowControlWindow = 1; value.ConnFlowControlWindow = (uint)ConnFlowControlWindowValue; }
        if (MaxWorkerQueueDelayUs is { } MaxWorkerQueueDelayUsValue) { present.MaxWorkerQueueDelayUs = 1; value.MaxWorkerQueueDelayUs = (uint)MaxWorkerQueueDelayUsValue; }
        if (MaxStatelessOperations is { } MaxStatelessOperationsValue) { present.MaxStatelessOperations = 1; value.MaxStatelessOperations = (uint)MaxStatelessOperationsValue; }
        if (InitialWindowPackets is { } InitialWindowPacketsValue) { present.InitialWindowPackets = 1; value.InitialWindowPackets = (uint)InitialWindowPacketsValue; }
        if (SendIdleTimeoutMs is { } SendIdleTimeoutMsValue) { present.SendIdleTimeoutMs = 1; value.SendIdleTimeoutMs = (uint)SendIdleTimeoutMsValue; }
        if (InitialRttMs is { } InitialRttMsValue) { present.InitialRttMs = 1; value.InitialRttMs = (uint)InitialRttMsValue; }
        if (MaxAckDelayMs is { } MaxAckDelayMsValue) { present.MaxAckDelayMs = 1; value.MaxAckDelayMs = (uint)MaxAckDelayMsValue; }
        if (DisconnectTimeoutMs is { } DisconnectTimeoutMsValue) { present.DisconnectTimeoutMs = 1; value.DisconnectTimeoutMs = (uint)DisconnectTimeoutMsValue; }
        if (KeepAliveIntervalMs is { } KeepAliveIntervalMsValue) { present.KeepAliveIntervalMs = 1; value.KeepAliveIntervalMs = (uint)KeepAliveIntervalMsValue; }
        if (PeerBidiStreamCount is { } PeerBidiStreamCountValue) { present.PeerBidiStreamCount = 1; value.PeerBidiStreamCount = (ushort)PeerBidiStreamCountValue; }
        if (PeerUnidiStreamCount is { } PeerUnidiStreamCountValue) { present.PeerUnidiStreamCount = 1; value.PeerUnidiStreamCount = (ushort)PeerUnidiStreamCountValue; }
        if (MaxBindingStatelessOperations is { } MaxBindingStatelessOperationsValue) { present.MaxBindingStatelessOperations = 1; value.MaxBindingStatelessOperations = (ushort)MaxBindingStatelessOperationsValue; }
        if (StatelessOperationExpirationMs is { } StatelessOperationExpirationMsValue) { present.StatelessOperationExpirationMs = 1; value.StatelessOperationExpirationMs = (ushort)StatelessOperationExpirationMsValue; }
        if (MinimumMtu is { } MinimumMtuValue) { present.MinimumMtu = 1; value.MinimumMtu = (ushort)MinimumMtuValue; }
        if (MaximumMtu is { } MaximumMtuValue) { present.MaximumMtu = 1; value.MaximumMtu = (ushort)MaximumMtuValue; }
        if (SendBufferingEnabled is { } SendBufferingEnabledValue) { present.SendBufferingEnabled = 1; value.SendBufferingEnabled = (byte)(SendBufferingEnabledValue ? 1 : 0); }
        if (PacingEnabled is { } PacingEnabledValue) { present.PacingEnabled = 1; value.PacingEnabled = (byte)(PacingEnabledValue ? 1 : 0); }
        if (MigrationEnabled is { } MigrationEnabledValue) { present.MigrationEnabled = 1; value.MigrationEnabled = (byte)(MigrationEnabledValue ? 1 : 0); }
        if (DatagramReceiveEnabled is { } DatagramReceiveEnabledValue) { present.DatagramReceiveEnabled = 1; value.DatagramReceiveEnabled = (byte)(DatagramReceiveEnabledValue ? 1 : 0); }
        if (MaxOperationsPerDrain is { } MaxOperationsPerDrainValue) { present.MaxOperationsPerDrain = 1; value.MaxOperationsPerDrain = (byte)MaxOperationsPerDrainValue; }
        if (MtuDiscoveryMissingProbeCount is { } MtuDiscoveryMissingProbeCountValue) { present.MtuDiscoveryMissingProbeCount = 1; value.MtuDiscoveryMissingProbeCount = (byte)MtuDiscoveryMissingProbeCountValue; }
        if (DestCidUpdateIdleTimeoutMs is { } DestCidUpdateIdleTimeoutMsValue) { present.DestCidUpdateIdleTimeoutMs = 1; value.DestCidUpdateIdleTimeoutMs = (uint)DestCidUpdateIdleTimeoutMsValue; }
        if (GreaseQuicBitEnabled is { } GreaseQuicBitEnabledValue) { present.GreaseQuicBitEnabled = 1; value.GreaseQuicBitEnabled = (byte)(GreaseQuicBitEnabledValue ? 1 : 0); }
        if (HyStartEnabled is { } HyStartEnabledValue) { present.HyStartEnabled = 1; value.HyStartEnabled = (ulong)(HyStartEnabledValue ? 1 : 0); }
        if (StreamRecvWindowBidiLocalDefault is { } StreamRecvWindowBidiLocalDefaultValue) { present.StreamRecvWindowBidiLocalDefault = 1; value.StreamRecvWindowBidiLocalDefault = (uint)StreamRecvWindowBidiLocalDefaultValue; }
        if (StreamRecvWindowBidiRemoteDefault is { } StreamRecvWindowBidiRemoteDefaultValue) { present.StreamRecvWindowBidiRemoteDefault = 1; value.StreamRecvWindowBidiRemoteDefault = (uint)StreamRecvWindowBidiRemoteDefaultValue; }
        if (StreamRecvWindowUnidiDefault is { } StreamRecvWindowUnidiDefaultValue) { present.StreamRecvWindowUnidiDefault = 1; value.StreamRecvWindowUnidiDefault = (uint)StreamRecvWindowUnidiDefaultValue; }
        if (initialize || CongestionControlAlgorithm.HasValue) { present.CongestionControlAlgorithm = 1; value.CongestionControlAlgorithm = 0; }
        if (ServerResumptionLevel is { } ServerResumptionLevelValue) { present.ServerResumptionLevel = 1; value.ServerResumptionLevel = (byte)ServerResumptionLevelValue; }
        if (initialize || EncryptionOffloadAllowed.HasValue) { present.EncryptionOffloadAllowed = 1; value.EncryptionOffloadAllowed = 0; }
        if (initialize || ReliableResetEnabled.HasValue) { present.ReliableResetEnabled = 1; value.ReliableResetEnabled = 0; }
        if (initialize || OneWayDelayEnabled.HasValue) { present.OneWayDelayEnabled = 1; value.OneWayDelayEnabled = 0; }
        if (initialize || NetStatsEventEnabled.HasValue) { present.NetStatsEventEnabled = 1; value.NetStatsEventEnabled = 0; }
        if (initialize || StreamMultiReceiveEnabled.HasValue) { present.StreamMultiReceiveEnabled = 1; value.StreamMultiReceiveEnabled = 0; }
        if (initialize || XdpEnabled.HasValue) { present.XdpEnabled = 1; value.XdpEnabled = 0; }
        if (initialize || QTIPEnabled.HasValue) { present.QTIPEnabled = 1; value.QTIPEnabled = 0; }
        if (initialize || ReservedRioEnabled.HasValue) { present.ReservedRioEnabled = 1; value.ReservedRioEnabled = 0; }
        if (initialize || EcnEnabled.HasValue) { present.EcnEnabled = 1; value.EcnEnabled = 0; }
        value.IsSet = present;
        if (initialize) ValidateInitial(value);
        return value;
    }

    // Validate with the pinned implementation, not a second table of numeric
    // limits. These stack-owned records never borrow a live VersionSettings
    // allocation: QUIC_SETTINGS conversion only copies public scalar fields.
    internal static unsafe void ValidateInitial(QUIC_SETTINGS requested)
    {
        QUIC_SETTINGS_INTERNAL destination = default, source = default;
        try
        {
            MsQuic.QuicSettingsSetDefault(&destination);
            QuicError.ThrowIfFailed(MsQuic.QuicSettingsSettingsToInternal((uint)sizeof(QUIC_SETTINGS), &requested, &source), "Validate initial settings");
            if (MsQuic.QuicSettingApply(&destination, 1, 1, &source) == 0)
                throw new ArgumentException("The requested initial QUIC settings are invalid.");
        }
        finally
        {
            MsQuic.QuicSettingsCleanup(&source);
            MsQuic.QuicSettingsCleanup(&destination);
        }
    }

    // Call under the same mutation gate used to read effective settings and
    // dispatch SetParam. Preserve IsSet flags: upstream MTU validation uses them
    // to distinguish an explicitly configured bound from its default value.
    internal static unsafe void ValidateUpdate(QUIC_SETTINGS requested, QUIC_SETTINGS effective,
        bool allowMtuAndEcnChanges = true)
    {
        QUIC_SETTINGS_INTERNAL destination = default, source = default;
        try
        {
            QuicError.ThrowIfFailed(MsQuic.QuicSettingsSettingsToInternal((uint)sizeof(QUIC_SETTINGS), &effective, &destination), "Read effective settings for validation");
            QuicError.ThrowIfFailed(MsQuic.QuicSettingsSettingsToInternal((uint)sizeof(QUIC_SETTINGS), &requested, &source), "Validate settings update");
            if (MsQuic.QuicSettingApply(&destination, 1, (byte)(allowMtuAndEcnChanges ? 1 : 0), &source) == 0)
                throw new ArgumentException("The requested QUIC settings update is invalid for the effective settings and connection state.");
        }
        finally
        {
            MsQuic.QuicSettingsCleanup(&source);
            MsQuic.QuicSettingsCleanup(&destination);
        }
    }

    internal static unsafe QuicSettings FromNative(QUIC_SETTINGS value) => new()
    {
        MaxBytesPerKey = (ulong)value.MaxBytesPerKey,
        HandshakeIdleTimeoutMs = (ulong)value.HandshakeIdleTimeoutMs,
        IdleTimeoutMs = (ulong)value.IdleTimeoutMs,
        MtuDiscoverySearchCompleteTimeoutUs = (ulong)value.MtuDiscoverySearchCompleteTimeoutUs,
        TlsClientMaxSendBuffer = (uint)value.TlsClientMaxSendBuffer,
        TlsServerMaxSendBuffer = (uint)value.TlsServerMaxSendBuffer,
        StreamRecvWindowDefault = (uint)value.StreamRecvWindowDefault,
        StreamRecvBufferDefault = (uint)value.StreamRecvBufferDefault,
        ConnFlowControlWindow = (uint)value.ConnFlowControlWindow,
        MaxWorkerQueueDelayUs = (uint)value.MaxWorkerQueueDelayUs,
        MaxStatelessOperations = (uint)value.MaxStatelessOperations,
        InitialWindowPackets = (uint)value.InitialWindowPackets,
        SendIdleTimeoutMs = (uint)value.SendIdleTimeoutMs,
        InitialRttMs = (uint)value.InitialRttMs,
        MaxAckDelayMs = (uint)value.MaxAckDelayMs,
        DisconnectTimeoutMs = (uint)value.DisconnectTimeoutMs,
        KeepAliveIntervalMs = (uint)value.KeepAliveIntervalMs,
        PeerBidiStreamCount = (ushort)value.PeerBidiStreamCount,
        PeerUnidiStreamCount = (ushort)value.PeerUnidiStreamCount,
        MaxBindingStatelessOperations = (ushort)value.MaxBindingStatelessOperations,
        StatelessOperationExpirationMs = (ushort)value.StatelessOperationExpirationMs,
        MinimumMtu = (ushort)value.MinimumMtu,
        MaximumMtu = (ushort)value.MaximumMtu,
        SendBufferingEnabled = value.SendBufferingEnabled != 0,
        PacingEnabled = value.PacingEnabled != 0,
        MigrationEnabled = value.MigrationEnabled != 0,
        DatagramReceiveEnabled = value.DatagramReceiveEnabled != 0,
        MaxOperationsPerDrain = (byte)value.MaxOperationsPerDrain,
        MtuDiscoveryMissingProbeCount = (byte)value.MtuDiscoveryMissingProbeCount,
        DestCidUpdateIdleTimeoutMs = (uint)value.DestCidUpdateIdleTimeoutMs,
        GreaseQuicBitEnabled = value.GreaseQuicBitEnabled != 0,
        HyStartEnabled = value.HyStartEnabled != 0,
        StreamRecvWindowBidiLocalDefault = (uint)value.StreamRecvWindowBidiLocalDefault,
        StreamRecvWindowBidiRemoteDefault = (uint)value.StreamRecvWindowBidiRemoteDefault,
        StreamRecvWindowUnidiDefault = (uint)value.StreamRecvWindowUnidiDefault,
        CongestionControlAlgorithm = (QuicCongestionControl)value.CongestionControlAlgorithm,
        ServerResumptionLevel = (QuicServerResumption)value.ServerResumptionLevel,
        EncryptionOffloadAllowed = value.EncryptionOffloadAllowed != 0,
        ReliableResetEnabled = value.ReliableResetEnabled != 0,
        OneWayDelayEnabled = value.OneWayDelayEnabled != 0,
        NetStatsEventEnabled = value.NetStatsEventEnabled != 0,
        StreamMultiReceiveEnabled = value.StreamMultiReceiveEnabled != 0,
        XdpEnabled = value.XdpEnabled != 0,
        QTIPEnabled = value.QTIPEnabled != 0,
        ReservedRioEnabled = value.ReservedRioEnabled != 0,
        EcnEnabled = value.EcnEnabled != 0,
    };
}
