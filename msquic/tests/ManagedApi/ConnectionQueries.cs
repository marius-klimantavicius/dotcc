using Managed.Transport.Api;

internal static class ConnectionQueries
{
    internal static void Check(QuicConnection connection)
    {
        var high = QuicParameterPriority.High;
        if (connection.GetProtocolVersion(high) != 1 || connection.GetIdealProcessor(high) >= connection.Runtime.Host.ProcessorCount)
            throw new InvalidOperationException("Incorrect version or logical worker query.");
        if (connection.GetLocalEndPoint(high).Port == 0 || connection.GetRemoteEndPoint(high).Port == 0)
            throw new InvalidOperationException("Connected endpoint query lost its bound port.");
        var ids = connection.GetMaximumStreamIds(high);
        if ((ids.ClientBidirectional & 3) != 0 || (ids.ServerBidirectional & 3) != 1 ||
            (ids.ClientUnidirectional & 3) != 2 || (ids.ServerUnidirectional & 3) != 3)
            throw new InvalidOperationException("Stream-ID boundary types changed.");
        if (connection.GetOriginalDestinationConnectionId(high).Length is < 1 or > 20)
            throw new InvalidOperationException("Invalid original destination CID observation.");
        _ = connection.GetUdpBindingShared(high);
        _ = connection.GetNetworkStatistics(high);
        _ = connection.GetCapabilities(high);
        if (connection.GetHandshakeInformation(high).Protocol != QuicTlsProtocol.Tls13)
            throw new InvalidOperationException("Handshake information does not report TLS1.3.");
        var v2 = connection.GetStatistics(priority: high);
        var platform = connection.GetStatistics(QuicStatisticsClock.Platform, high);
        var legacy = connection.GetLegacyStatistics(priority: high);
        var legacyPlatform = connection.GetLegacyStatistics(QuicStatisticsClock.Platform, high);
        foreach (var snapshot in new QuicLegacyConnectionStatistics[] { platform, legacy, legacyPlatform })
            if (snapshot.CorrelationId != v2.CorrelationId || snapshot.StartTimeMicroseconds != v2.StartTimeMicroseconds ||
                snapshot.InitialFlightEndMicroseconds != v2.InitialFlightEndMicroseconds ||
                snapshot.HandshakeFlightEndMicroseconds != v2.HandshakeFlightEndMicroseconds ||
                snapshot.HandshakeClientFlight1Bytes != v2.HandshakeClientFlight1Bytes ||
                snapshot.HandshakeServerFlight1Bytes != v2.HandshakeServerFlight1Bytes ||
                snapshot.HandshakeClientFlight2Bytes != v2.HandshakeClientFlight2Bytes)
                throw new InvalidOperationException("Legacy/V2/platform immutable handshake observations disagree.");
        connection.SetCloseReasonPhrase("typed query control"u8, high);
        if (connection.GetCloseReasonPhrase(high) is not { } phrase || !phrase.Span.SequenceEqual("typed query control"u8))
            throw new InvalidOperationException("Close reason bytes did not round trip.");
        try { connection.SetCloseReasonPhrase(new byte[512], high); throw new InvalidOperationException("Oversized reason accepted."); }
        catch (ArgumentException) { }
        if (connection.GetCloseReasonPhrase(high) is not { } unchanged || !unchanged.Span.SequenceEqual("typed query control"u8))
            throw new InvalidOperationException("Rejected close reason changed state.");
        var settings = connection.GetSettings(high);
        try
        {
            connection.SetSettings(new() { IdleTimeoutMs = 50000, StreamRecvWindowDefault = 3 }, high);
            throw new InvalidOperationException("Invalid connection settings accepted.");
        }
        catch (ArgumentException) { }
        if (connection.GetSettings(high) != settings) throw new InvalidOperationException("Rejected connection settings changed state.");
    }
}
