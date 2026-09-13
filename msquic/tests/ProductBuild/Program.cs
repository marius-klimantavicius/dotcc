using System;
using System.Runtime.CompilerServices;
using Managed.Transport;

internal static unsafe class Program
{
    private sealed class SettingsOwner { public MsQuic.QUIC_SETTINGS Settings; }

    private static int Main()
    {
        MsQuic.QUIC_SETTINGS settings = default(MsQuic.QUIC_SETTINGS);
        settings.IsSet.PeerUnidiStreamCount = 1;
        settings.PeerUnidiStreamCount = (ushort)10;
        if (settings.IsSetFlags != (1UL << 19) || settings.PeerUnidiStreamCount != 10)
            return 1;
        var copy = settings;
        copy.IsSet.PeerUnidiStreamCount = 0;
        if (settings.IsSetFlags != (1UL << 19) || copy.IsSetFlags != 0)
            return 1;
        var owner = new SettingsOwner();
        ref var flags = ref owner.Settings.IsSet;
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        flags.PeerUnidiStreamCount = 1;
        if (owner.Settings.IsSetFlags != (1UL << 19))
            return 1;

        // Stable field-type names must cross an assembly boundary in both
        // raw/postprocessed JIT and NativeAOT consumers.
        MsQuic.QUIC_CONNECTION_EVENT connectionEvent = default;
        ref MsQuic.QUIC_CONNECTION_EVENT_CONNECTED_DATA connected = ref connectionEvent.CONNECTED;
        ref MsQuic.QUIC_CONNECTION_EVENT_LOCAL_ADDRESS_CHANGED_DATA local_address_changed = ref connectionEvent.LOCAL_ADDRESS_CHANGED;
        ref MsQuic.QUIC_CONNECTION_EVENT_PEER_ADDRESS_CHANGED_DATA peer_address_changed = ref connectionEvent.PEER_ADDRESS_CHANGED;
        ref MsQuic.QUIC_CONNECTION_EVENT_PEER_CERTIFICATE_RECEIVED_DATA peer_certificate_received = ref connectionEvent.PEER_CERTIFICATE_RECEIVED;
        ref MsQuic.QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED_DATA peer_stream_started = ref connectionEvent.PEER_STREAM_STARTED;
        ref MsQuic.QUIC_CONNECTION_EVENT_STREAMS_AVAILABLE_DATA streams_available = ref connectionEvent.STREAMS_AVAILABLE;
        ref MsQuic.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE_DATA shutdown_complete = ref connectionEvent.SHUTDOWN_COMPLETE;
        ref MsQuic.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER_DATA shutdown_initiated_by_peer = ref connectionEvent.SHUTDOWN_INITIATED_BY_PEER;
        ref MsQuic.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT_DATA shutdown_initiated_by_transport = ref connectionEvent.SHUTDOWN_INITIATED_BY_TRANSPORT;
        connected.SessionResumed = 1;
        if (connectionEvent.CONNECTED.SessionResumed != 1) return 1;
        connected = default;
        MsQuic.QUIC_LISTENER_EVENT listenerEvent = default;
        ref MsQuic.QUIC_LISTENER_EVENT_NEW_CONNECTION newConnection = ref listenerEvent.NEW_CONNECTION;
        ref MsQuic.QUIC_LISTENER_EVENT_STOP_COMPLETE stopComplete = ref listenerEvent.STOP_COMPLETE;
        newConnection = default;
        stopComplete = default;

        // These wrappers use typedef/primitive casts and function-like macros
        // upstream. Their public exports must be usable as C# constants.
        const uint pending = MsQuic.QUIC_STATUS_PENDING;
        const uint addressInUse = MsQuic.QUIC_STATUS_ADDRESS_IN_USE;
        const uint badCertificate = MsQuic.QUIC_STATUS_BAD_CERTIFICATE;
        if (!pending.Equals(4294967294U) || !addressInUse.Equals(98U) || !badCertificate.Equals(200000298U))
            return 1;
        // Selected C inline functions are callable by their original names.
        if (MsQuic.CxPlatEwma(100, 200, 4) != 125)
            return 1;
        MsQuic.QUIC_ADDR address = default;
        MsQuic.QuicAddrSetFamily(&address, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_INET);
        MsQuic.QuicAddrSetPort(&address, 443);
        if (MsQuic.QuicAddrGetPort(&address) != 443 || (int)MsQuic.QuicAddrIsWildCard(&address) != 1)
            return 1;
        MsQuic.QuicAddrSetFamily(&address, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_INET6);
        MsQuic.QuicAddrSetPort(&address, 8443);
        if (MsQuic.QuicAddrGetPort(&address) != 8443 || (int)MsQuic.QuicAddrIsWildCard(&address) != 1)
            return 1;
        // The constant-backed inline helper has one shared managed name.
        MsQuic.QuicAddrSetToLoopback(&address);
        byte* ipv6 = (byte*)&address.Ipv6.sin6_addr;
        for (int i = 0; i < 15; ++i)
            if (ipv6[i] != 0) return 1;
        if (ipv6[15] != 1 || MsQuic.QuicAddrGetPort(&address) != 8443)
            return 1;
        address = default;
        MsQuic.QuicAddrSetFamily(&address, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_INET);
        MsQuic.QuicAddrSetToLoopback(&address);
        byte* ipv4 = (byte*)&address.Ipv4.sin_addr;
        if (ipv4[0] != 127 || ipv4[1] != 0 || ipv4[2] != 0 || ipv4[3] != 1)
            return 1;
        // This checks generated assembly reachability and a real rejecting boundary.
        // Runtime host services and transport behavior have separate phase gates.
        if (MsQuic.MsQuicHostInstall(null) == 0 || MsQuic.MsQuicHostUninstall() != 0)
            return 1;
        // Public ABI types must be usable through the wrapper in a separate
        // project; generated file-local aliases cannot supply this name.
        MsQuic.MSQUIC_HOST_TABLE table = default;
        table.Size = (uint)sizeof(MsQuic.MSQUIC_HOST_TABLE);
        if (MsQuic.MsQuicHostInstall(&table) == 0)
            return 1;
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported ? "jit: boundary rejection passed" : "nativeaot: boundary rejection passed");
        return 0;
    }
}
