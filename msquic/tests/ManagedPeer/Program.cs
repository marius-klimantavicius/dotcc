using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Managed.Transport;
using Managed.Transport.Hosting;
using static Managed.Transport.QUIC_CONNECTION_EVENT_TYPE;
using static Managed.Transport.QUIC_STREAM_EVENT_TYPE;

// Test-only, one external connection per process. All transport storage and
// callbacks are the compiler's generated types; no mirrored event/handle ABI.
internal static unsafe class Program
{
    private const int PayloadSize = 65537;
    private static QUIC_API_TABLE* api;
    private static QUIC_HANDLE* configuration;
    private static Peer peer = null!;
    private static int failed;
    private static readonly ConcurrentQueue<string> errors = new();
    private static readonly ConcurrentQueue<string> events = new();
    private static readonly ConcurrentDictionary<nint, SendLease> sends = new();
    private static long nextSend;
    private static int settleMilliseconds;
    private static int finalRemotePort;
    private static bool finalActivePathValidated;
    private static bool listenerPreflightPassed;
    private static QUIC_STATISTICS_V2 finalStatistics;
    private static uint statisticsStatus = uint.MaxValue;
    private static int finalHostResources, finalHostAllocations, finalHostReceiveLeases;
    private static long finalSendErrors, finalReceiveErrors, finalTruncations;

    private sealed class Peer(bool server)
    {
        internal readonly bool Server = server;
        internal QUIC_HANDLE* Connection;
        internal QUIC_HANDLE* Stream;
        internal ulong Received, Sent;
        internal int Connected, Finished, Closed, SendCompleted;
        internal uint Version;
        internal QUIC_HANDSHAKE_INFO Handshake;
        internal uint TransportStatus;
        internal ulong TransportError, PeerError;
        internal GCHandle Root;
        internal void* Context => (void*)GCHandle.ToIntPtr(Root);
    }

    private sealed class SendLease
    {
        internal readonly byte[] Payload = GC.AllocateUninitializedArray<byte>(PayloadSize, pinned: true);
        internal readonly QUIC_BUFFER[] Descriptors = GC.AllocateArray<QUIC_BUFFER>(1, pinned: true);
        internal SendLease(bool server)
        {
            for (int i = 0; i < Payload.Length; i++) Payload[i] = Pattern((ulong)i, server);
            fixed (byte* data = Payload) Descriptors[0] = new QUIC_BUFFER { Length = PayloadSize, Buffer = data };
        }
    }

    private static void PrintDiagnostics(string phase, MsQuicHost? host, bool includeConnection)
    {
        try
        {
            if (host != null)
            {
                finalHostResources = host.OutstandingResources; finalHostAllocations = host.OutstandingPlatformAllocations;
                finalHostReceiveLeases = host.OutstandingDatagramReceives; finalSendErrors = host.DatagramSendErrors;
                finalReceiveErrors = host.DatagramReceiveErrors; finalTruncations = host.DatagramTruncations;
                // Test-only JIT inspection of a failed drain. Product code has
                // no reflective diagnostics or dependency on this private field.
                Console.Error.WriteLine($"host_diagnostics phase={phase} resources={host.OutstandingResources} platform_allocations={host.OutstandingPlatformAllocations} receive_leases={host.OutstandingDatagramReceives} send_errors={host.DatagramSendErrors} receive_errors={host.DatagramReceiveErrors} truncations={host.DatagramTruncations} load_refs={MsQuicGlobals.MsQuicLib.LoadRefCount} open_refs={MsQuicGlobals.MsQuicLib.OpenRefCount} cleanup_rundown_event={(nint)MsQuicGlobals.MsQuicLib.RegistrationCloseCleanupRundown.RundownComplete.Handle}");
            }
            if (includeConnection && api != null && peer.Connection != null)
            {
                QUIC_STATISTICS_V2 stats = default; uint size = (uint)sizeof(QUIC_STATISTICS_V2);
                uint status = api->GetParam(peer.Connection, MsQuic.QUIC_PARAM_CONN_STATISTICS_V2, &size, &stats);
                finalStatistics = stats;
                QUIC_ADDR remote = default; uint remoteLength = (uint)sizeof(QUIC_ADDR);
                if (api->GetParam(peer.Connection, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, &remoteLength, &remote) == 0)
                    finalRemotePort = MsQuicHost.DatagramEndpoint(&remote).Port;
                // Test-only actual core storage, sampled after SHUTDOWN_COMPLETE
                // and before close: the active path is kept in slot zero upstream.
                var path = ((QUIC_CONNECTION*)peer.Connection)->Paths[0];
                finalActivePathValidated = path.InUse != 0 && path.IsActive != 0 && path.IsPeerValidated != 0; statisticsStatus = status;
                Console.Error.WriteLine($"connection_statistics phase={phase} status={status} mtu={stats.SendPathMtu} sent_packets={stats.SendTotalPackets} lost_packets={stats.SendSuspectedLostPackets} sent_bytes={stats.SendTotalBytes} sent_stream_bytes={stats.SendTotalStreamBytes} recv_packets={stats.RecvTotalPackets} recv_dropped={stats.RecvDroppedPackets} recv_bytes={stats.RecvTotalBytes} recv_stream_bytes={stats.RecvTotalStreamBytes} decrypt_failures={stats.RecvDecryptionFailures} ack_frames={stats.RecvValidAckFrames}");
            }
        }
        catch (Exception error) { Console.Error.WriteLine("diagnostic_failure " + phase + ": " + error); }
    }

    private static byte Pattern(ulong offset, bool server) => unchecked((byte)(offset * 31 + 17 + (server ? 29UL : 0)));
    private static void Fail(string error) { errors.Enqueue(error); Console.Error.WriteLine(error); Volatile.Write(ref failed, 1); }
    private static void Require(bool condition, string error) { if (!condition) Fail(error); }
    private static bool Check(uint status, string operation)
    {
        if (!Status.Failed(status)) return true;
        Fail(operation + " failed: 0x" + status.ToString("x")); return false;
    }
    private static Peer FromContext(void* context)
        => GCHandle.FromIntPtr((nint)context).Target as Peer ?? throw new InvalidOperationException("Missing peer callback owner");

    private static void SendPayload(Peer owner)
    {
        var lease = new SendLease(owner.Server);
        nint token = checked((nint)Interlocked.Increment(ref nextSend));
        if (!sends.TryAdd(token, lease)) throw new InvalidOperationException("Duplicate send owner");
        uint status;
        fixed (QUIC_BUFFER* buffers = lease.Descriptors)
            status = api->StreamSend(owner.Stream, buffers, 1, QUIC_SEND_FLAGS.QUIC_SEND_FLAG_FIN, (void*)token);
        if (!Check(status, "StreamSend")) sends.TryRemove(token, out _);
        else owner.Sent += PayloadSize;
        // The dictionary roots both pinned arrays through SEND_COMPLETE, even if
        // MsQuic deferred reading the descriptor or the payload after this call.
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }

    private static uint StreamCallback(QUIC_HANDLE* stream, void* context, QUIC_STREAM_EVENT* notification)
    {
        try
        {
            var owner = FromContext(context);
            events.Enqueue("stream:" + notification->Type);
            switch (notification->Type)
            {
                case QUIC_STREAM_EVENT_START_COMPLETE:
                    Check(notification->START_COMPLETE.Status, "StreamStart completion");
                    break;
                case QUIC_STREAM_EVENT_RECEIVE:
                    var received = notification->RECEIVE;
                    Require(received.AbsoluteOffset == owner.Received, "Unexpected stream receive offset");
                    ulong total = 0;
                    for (uint b = 0; b < received.BufferCount; b++)
                    {
                        QUIC_BUFFER buffer = received.Buffers[b];
                        for (uint i = 0; i < buffer.Length; i++)
                        {
                            if (owner.Received >= PayloadSize || buffer.Buffer[i] != Pattern(owner.Received, !owner.Server))
                                Fail("Peer payload mismatch at " + owner.Received);
                            owner.Received++;
                        }
                        total += buffer.Length;
                    }
                    Require(total == received.TotalBufferLength, "Receive descriptor length mismatch");
                    break;
                case QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN:
                    Require(owner.Received == PayloadSize, "FIN arrived before the complete payload");
                    if (owner.Server && Volatile.Read(ref failed) == 0) SendPayload(owner);
                    Volatile.Write(ref owner.Finished, 1);
                    break;
                case QUIC_STREAM_EVENT_SEND_COMPLETE:
                    var completed = notification->SEND_COMPLETE;
                    Require(sends.TryRemove((nint)completed.ClientContext, out _), "Unknown or repeated send completion");
                    Require(completed.Canceled == 0, "Stream send was canceled");
                    Interlocked.Increment(ref owner.SendCompleted);
                    break;
                case QUIC_STREAM_EVENT_PEER_SEND_ABORTED:
                    Fail("Peer aborted send: " + notification->PEER_SEND_ABORTED.ErrorCode); break;
                case QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED:
                    Fail("Peer aborted receive: " + notification->PEER_RECEIVE_ABORTED.ErrorCode); break;
            }
            return Status.Success;
        }
        catch (Exception error) { Fail("Stream callback: " + error); return Status.InternalError; }
    }

    private static uint ConnectionCallback(QUIC_HANDLE* connection, void* context, QUIC_CONNECTION_EVENT* notification)
    {
        try
        {
            var owner = FromContext(context);
            events.Enqueue("connection:" + notification->Type);
            switch (notification->Type)
            {
                case QUIC_CONNECTION_EVENT_CONNECTED:
                    QUIC_HANDSHAKE_INFO handshake = default;
                    uint size = (uint)sizeof(QUIC_HANDSHAKE_INFO), version = 0;
                    Check(api->GetParam(connection, MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, &size, &handshake), "handshake info");
                    Require(size == sizeof(QUIC_HANDSHAKE_INFO), "Handshake info extent mismatch");
                    size = sizeof(uint);
                    Check(api->GetParam(connection, MsQuic.QUIC_PARAM_CONN_QUIC_VERSION, &size, &version), "QUIC version");
                    owner.Handshake = handshake; owner.Version = version;
                    var connected = notification->CONNECTED;
                    Require(new ReadOnlySpan<byte>(connected.NegotiatedAlpn, connected.NegotiatedAlpnLength).SequenceEqual("dotcc-probe"u8), "Negotiated ALPN mismatch");
                    Volatile.Write(ref owner.Connected, 1);
                    if (!owner.Server && Volatile.Read(ref failed) == 0)
                    {
                        QUIC_HANDLE* opened = null;
                        if (Check(api->StreamOpen(connection, QUIC_STREAM_OPEN_FLAGS.QUIC_STREAM_OPEN_FLAG_NONE, &StreamCallback, context, &opened), "StreamOpen"))
                        {
                            owner.Stream = opened;
                            if (Check(api->StreamStart(opened, QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_IMMEDIATE), "StreamStart")) SendPayload(owner);
                        }
                    }
                    break;
                case QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED:
                    if (!owner.Server || owner.Stream != null) { Fail("Unexpected peer stream"); return Status.ConnectionRefused; }
                    owner.Stream = notification->PEER_STREAM_STARTED.Stream;
                    api->SetCallbackHandler(owner.Stream, (void*)(delegate*<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, uint>)&StreamCallback, context);
                    break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
                    var shutdown = notification->SHUTDOWN_INITIATED_BY_TRANSPORT;
                    owner.TransportStatus = shutdown.Status; owner.TransportError = shutdown.ErrorCode;
                    Check(shutdown.Status, "transport shutdown (error " + shutdown.ErrorCode + ")");
                    break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER:
                    owner.PeerError = notification->SHUTDOWN_INITIATED_BY_PEER.ErrorCode;
                    Require(owner.PeerError == 0, "Peer closed with an application error");
                    break;
                case QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                    Volatile.Write(ref owner.Closed, 1); break;
            }
            return Status.Success;
        }
        catch (Exception error) { Fail("Connection callback: " + error); return Status.InternalError; }
    }

    private static uint ListenerCallback(QUIC_HANDLE* listener, void* context, QUIC_LISTENER_EVENT* notification)
    {
        try
        {
            events.Enqueue("listener:" + notification->Type);
            if (notification->Type != QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_NEW_CONNECTION) return Status.Success;
            if (peer.Connection != null) return Status.ConnectionRefused;
            peer.Connection = notification->NEW_CONNECTION.Connection;
            api->SetCallbackHandler(peer.Connection, (void*)(delegate*<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, uint>)&ConnectionCallback, peer.Context);
            uint status = api->ConnectionSetConfiguration(peer.Connection, configuration);
            // Returning failure rejects the connection; the core then closes
            // its handle. Do not retain it for the application's final cleanup.
            if (unchecked((int)status) > 0) peer.Connection = null;
            Check(status, "ConnectionSetConfiguration"); return status;
        }
        catch (Exception error) { Fail("Listener callback: " + error); return Status.InternalError; }
    }

    private static bool Wait(ref int flag, string operation, bool stopOnFailure = true)
    {
        long start = Stopwatch.GetTimestamp();
        while (Volatile.Read(ref flag) == 0)
        {
            if (stopOnFailure && Volatile.Read(ref failed) != 0) return false;
            if (Stopwatch.GetElapsedTime(start) > TimeSpan.FromSeconds(15)) { Fail(operation + " timed out"); return false; }
            Thread.Sleep(1);
        }
        return true;
    }

    private static uint PreflightListenerCallback(QUIC_HANDLE* listener, void* context, QUIC_LISTENER_EVENT* notification)
        => notification->Type == QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_NEW_CONNECTION ? Status.ConnectionRefused : Status.Success;

    private static void ListenerPreflight(QUIC_HANDLE* registration, QUIC_BUFFER* alpn, bool ipv6)
    {
        QUIC_ADDR address = default;
        MsQuicHost.DatagramAddress(new IPEndPoint(ipv6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, 0), &address);
        QUIC_HANDLE* listener = null;
        try
        {
            if (!Check(api->ListenerOpen(registration, &PreflightListenerCallback, null, &listener), "preflight ListenerOpen")) return;
            // This executes unchanged listener.c, including its mandatory
            // SHARE|SERVER_OWNED request, with the real installed host services.
            if (!Check(api->ListenerStart(listener, alpn, 1, &address), "preflight ListenerStart")) return;
            uint length = (uint)sizeof(QUIC_ADDR);
            if (!Check(api->GetParam(listener, MsQuic.QUIC_PARAM_LISTENER_LOCAL_ADDRESS, &length, &address), "preflight listener address")) return;
            Require(length == sizeof(QUIC_ADDR) && MsQuicHost.DatagramEndpoint(&address).Port != 0, "Preflight listener did not bind");
            listenerPreflightPassed = Volatile.Read(ref failed) == 0;
        }
        finally { if (listener != null) api->ListenerClose(listener); }
    }

    private static void LibraryLifetimePreflight(MsQuicHost host)
    {
        var table = host.CreateTable();
        CXPLAT_EVENT unrelated = default;
        table.CxPlatEventInitialize(table.Context, &unrelated, 1, 0);
        try
        {
            for (int iteration = 0; iteration < 3; iteration++)
            {
                void* opened = null; QUIC_HANDLE* registration = null;
                Check(MsQuic.MsQuicOpenVersion(2, &opened), "lifetime OpenVersion");
                var lifetimeApi = (QUIC_API_TABLE*)opened;
                try
                {
                    fixed (byte* name = "lifetime-preflight\0"u8)
                    {
                        QUIC_REGISTRATION_CONFIG config = new() { AppName = name, ExecutionProfile = QUIC_EXECUTION_PROFILE.QUIC_EXECUTION_PROFILE_LOW_LATENCY };
                        Check(lifetimeApi->RegistrationOpen(&config, &registration), "lifetime RegistrationOpen");
                    }
                }
                finally
                {
                    if (registration != null) lifetimeApi->RegistrationClose(registration);
                    if (opened != null) MsQuic.MsQuicClose(opened);
                }
                Require(host.OutstandingResources == 1 && host.OutstandingPlatformAllocations == 0, "Repeated library close must retain only the unrelated event");
                Require(MsQuicGlobals.MsQuicLib.RegistrationCloseCleanupRundown.RundownComplete.Handle == 0, "Global cleanup rundown event did not retire");
                bool rejected = false;
                try { host.Dispose(); } catch (InvalidOperationException) { rejected = true; }
                Require(rejected, "Host close must still reject unrelated live ownership");
            }
        }
        finally { table.CxPlatInternalEventUninitialize(table.Context, &unrelated); }
        Require(host.OutstandingResources == 0, "Lifetime preflight did not drain");
    }

    private static int Main(string[] arguments)
    {
        if (arguments.Length != 9 || arguments[2] is not ("128" or "256") || arguments[3] is not ("ipv4" or "ipv6") ||
            arguments[6] is not ("client" or "server") || !ushort.TryParse(arguments[7], out ushort port) || (arguments[6] == "client" && port == 0))
        {
            Console.Error.WriteLine("ManagedPeer certificate key cipher128|256 ipv4|ipv6 trust-file server-name client|server port ready-file");
            return 2;
        }
        string? settling = Environment.GetEnvironmentVariable("DOTCC_PEER_SETTLE_MS");
        if (settling != null && (!int.TryParse(settling, out settleMilliseconds) || settleMilliseconds is < 0 or > 5000))
        { Console.Error.WriteLine("DOTCC_PEER_SETTLE_MS must be in 0..5000."); return 2; }
        peer = new Peer(arguments[6] == "server"); peer.Root = GCHandle.Alloc(peer);
        ushort suite = arguments[2] == "128" ? (ushort)0x1301 : (ushort)0x1302;
        QUIC_HANDLE* registration = null; QUIC_HANDLE* listener = null;
        MsQuicHost? host = null; MsQuicHost.CredentialRegistration? credential = null;
        try
        {
            host = new MsQuicHost(2); host.Install();
            LibraryLifetimePreflight(host);
            void* openedApi = null;
            if (!Check(MsQuic.MsQuicOpenVersion(2, &openedApi), "MsQuicOpenVersion")) goto Complete;
            api = (QUIC_API_TABLE*)openedApi;
            fixed (byte* appName = "dotcc-managed-peer\0"u8)
            {
                var registrationConfig = new QUIC_REGISTRATION_CONFIG { AppName = appName, ExecutionProfile = QUIC_EXECUTION_PROFILE.QUIC_EXECUTION_PROFILE_LOW_LATENCY };
                if (!Check(api->RegistrationOpen(&registrationConfig, &registration), "RegistrationOpen")) goto Complete;
            }
            QUIC_SETTINGS settings = default;
            var isSet = settings.IsSet;
            isSet.PeerBidiStreamCount = 1; isSet.IdleTimeoutMs = 1;
            settings.IsSet = isSet; settings.PeerBidiStreamCount = 1; settings.IdleTimeoutMs = 10000;
            QUIC_HANDLE* openedConfiguration = null;
            fixed (byte* protocol = "dotcc-probe"u8)
            {
                QUIC_BUFFER alpn = new() { Length = 11, Buffer = protocol };
                if (!Check(api->ConfigurationOpen(registration, &alpn, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, &openedConfiguration), "ConfigurationOpen")) goto Complete;
                configuration = openedConfiguration;
                if (peer.Server)
                {
                    using var certificate = X509Certificate2.CreateFromPemFile(arguments[0], arguments[1]);
                    credential = host.CreateServerCredential(certificate, cipherSuite: suite);
                }
                else
                {
                    using var trust = X509Certificate2.CreateFromPem(File.ReadAllText(arguments[4]));
                    credential = host.CreateClientCredential(new[] { trust }, suite);
                }
                if (!Check(host.LoadCredential(configuration, credential), "LoadCredential")) goto Complete;
                ListenerPreflight(registration, &alpn, arguments[3] == "ipv6");
                if (!listenerPreflightPassed) goto Complete;
                QUIC_ADDR address = default;
                MsQuicHost.DatagramAddress(new IPEndPoint(arguments[3] == "ipv4" ? IPAddress.Loopback : IPAddress.IPv6Loopback, port), &address);
                if (peer.Server)
                {
                    if (!Check(api->ListenerOpen(registration, &ListenerCallback, peer.Context, &listener), "ListenerOpen") ||
                        !Check(api->ListenerStart(listener, &alpn, 1, &address), "ListenerStart")) goto Complete;
                    uint size = (uint)sizeof(QUIC_ADDR);
                    if (!Check(api->GetParam(listener, MsQuic.QUIC_PARAM_LISTENER_LOCAL_ADDRESS, &size, &address), "listener address")) goto Complete;
                    // Atomic publication prevents the orchestrator observing an empty ready file.
                    File.WriteAllText(arguments[8] + ".tmp", MsQuicHost.DatagramEndpoint(&address).Port + "\n");
                    File.Move(arguments[8] + ".tmp", arguments[8], overwrite: true);
                    Wait(ref peer.Closed, "server connection shutdown");
                }
                else
                {
                    QUIC_HANDLE* openedConnection = null;
                    if (!Check(api->ConnectionOpen(registration, &ConnectionCallback, peer.Context, &openedConnection), "ConnectionOpen")) goto Complete;
                    peer.Connection = openedConnection;
                    if (!Check(api->SetParam(openedConnection, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, (uint)sizeof(QUIC_ADDR), &address), "remote address")) goto Complete;
                    byte[] hostname = Encoding.UTF8.GetBytes(arguments[5] + "\0");
                    fixed (byte* name = hostname)
                        if (Check(api->ConnectionStart(openedConnection, configuration, address.Ip.sa_family, name, port), "ConnectionStart")) Wait(ref peer.Finished, "client payload FIN");
                    if (Volatile.Read(ref peer.Finished) != 0 && settleMilliseconds != 0) Thread.Sleep(settleMilliseconds);
                    api->ConnectionShutdown(openedConnection, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
                    Wait(ref peer.Closed, "client connection shutdown", stopOnFailure: false);
                }
            }
        }
        catch (Exception error) { Fail("Peer main: " + error); }
        finally
        {
            // Blocking close occurs only on this independent owner thread. Callback
            // contexts and send buffers remain rooted through all close callbacks.
            bool callbacksDrained = api == null;
            try
            {
                // Stop admission before reading the final connection owner. A
                // listener callback can publish a connection while main wakes.
                if (listener != null) { api->ListenerClose(listener); listener = null; }
                if (peer.Connection != null && Volatile.Read(ref peer.Closed) == 0)
                {
                    api->ConnectionShutdown(peer.Connection, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
                    Wait(ref peer.Closed, "cleanup shutdown", stopOnFailure: false);
                }
                PrintDiagnostics("before_handles_close", host, includeConnection:true);
                if (peer.Stream != null) api->StreamClose(peer.Stream);
                if (peer.Connection != null) api->ConnectionClose(peer.Connection);
                if (configuration != null) api->ConfigurationClose(configuration);
                if (registration != null) api->RegistrationClose(registration);
                if (api != null) MsQuic.MsQuicClose(api);
                PrintDiagnostics("after_api_close", host, includeConnection:false);
                callbacksDrained = true;
                credential?.Dispose();
                PrintDiagnostics("after_credential_close", host, includeConnection:false);
                Require(sends.IsEmpty, "Outstanding send buffers after API close");
                host?.Dispose();
            }
            catch (Exception error) { Fail("Peer cleanup: " + error); }
            // A failed close must not invalidate a context still used by workers.
            // The process runner terminates failed peers; it cannot count as drain.
            if (callbacksDrained) peer.Root.Free();
        }
    Complete:
        Require(peer.Connected != 0 && peer.Finished != 0 && peer.Closed != 0, "Connection/FIN/shutdown lifecycle incomplete");
        Require(peer.Received == PayloadSize && peer.Sent == PayloadSize && peer.SendCompleted == 1, "Bidirectional payload/send completion totals mismatch");
        Require((int)peer.Handshake.CipherSuite == suite && (int)peer.Handshake.TlsGroup == 23 && peer.Version == 1, "Negotiated cipher/group/version mismatch");
        return Report(arguments);
    }

    private static int Report(string[] arguments)
    {
        // Hand-written scalar JSON keeps NativeAOT free of reflection serialization.
        // Callback/error strings go in stderr; event names are enum identifiers.
        Console.Error.WriteLine("callback_sequence=" + string.Join(",", events));
        Console.WriteLine("{\"passed\":" + (Volatile.Read(ref failed) == 0 ? "true" : "false") +
            ",\"family\":\"" + arguments[3] + "\",\"role\":\"" + arguments[6] + "\",\"cipher\":" + (int)peer.Handshake.CipherSuite +
            ",\"group\":" + (int)peer.Handshake.TlsGroup + ",\"quic_version\":" + peer.Version +
            ",\"client_bytes\":" + (peer.Server ? 0 : peer.Received) + ",\"server_bytes\":" + (peer.Server ? peer.Received : 0) +
            ",\"sent_bytes\":" + peer.Sent + ",\"send_completions\":" + peer.SendCompleted +
            ",\"connected\":" + peer.Connected + ",\"finished\":" + peer.Finished + ",\"closed\":" + peer.Closed +
            ",\"transport_status\":" + peer.TransportStatus + ",\"transport_error\":" + peer.TransportError + ",\"peer_error\":" + peer.PeerError +
            ",\"statistics_status\":" + statisticsStatus + ",\"udp_sent_packets\":" + finalStatistics.SendTotalPackets +
            ",\"udp_received_packets\":" + finalStatistics.RecvTotalPackets + ",\"decrypt_failures\":" + finalStatistics.RecvDecryptionFailures +
            ",\"dropped_packets\":" + finalStatistics.RecvDroppedPackets + ",\"suspected_lost_packets\":" + finalStatistics.SendSuspectedLostPackets +
            ",\"udp_sent_bytes\":" + finalStatistics.SendTotalBytes + ",\"udp_received_bytes\":" + finalStatistics.RecvTotalBytes +
            ",\"core_sent_stream_bytes\":" + finalStatistics.SendTotalStreamBytes + ",\"core_received_stream_bytes\":" + finalStatistics.RecvTotalStreamBytes +
            ",\"valid_ack_frames\":" + finalStatistics.RecvValidAckFrames + ",\"path_mtu\":" + finalStatistics.SendPathMtu +
            ",\"settle_ms\":" + settleMilliseconds + ",\"remote_port\":" + finalRemotePort + ",\"active_path_validated\":" + (finalActivePathValidated ? "true" : "false") + ",\"dest_cid_updates\":" + finalStatistics.DestCidUpdateCount +
            ",\"host_resources\":" + finalHostResources + ",\"host_allocations\":" + finalHostAllocations + ",\"host_receive_leases\":" + finalHostReceiveLeases +
            ",\"host_send_errors\":" + finalSendErrors + ",\"host_receive_errors\":" + finalReceiveErrors + ",\"host_truncations\":" + finalTruncations +
            ",\"listener_preflight\":" + (listenerPreflightPassed ? "true" : "false") +
            ",\"certificate_validation\":" + (!peer.Server ? "true" : "false") + ",\"aot\":" + (!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false") + "}");
        return Volatile.Read(ref failed) == 0 ? 0 : 1;
    }
}
