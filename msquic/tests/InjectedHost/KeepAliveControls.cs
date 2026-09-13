using static Managed.Transport.MsQuic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Managed.Transport;
using Managed.Transport.Hosting;

// Optional InjectedHost scenario; the five existing defaults remain unchanged.
// No product clock/API changes.
// Actual owning typed API + real authenticated UDP peers. Only the host clock
// and maximum queue wait use the existing InjectedHost table seam.
internal static unsafe class KeepAliveControls
{
    private static MSQUIC_HOST_TABLE services;
    private static QUIC_API_TABLE* api;
    private static QUIC_HANDLE* serverConfiguration;
    private static nint clientHandle, serverHandle;
    private static long now;
    private static readonly int[] connected = new int[2], closed = new int[2], shutdowns = new int[2];
    private static readonly uint[] shutdownStatus = new uint[2];
    private static string? callbackFailure;
    private const uint IdleMilliseconds = 3000, IntervalMilliseconds = 250;
    private const long ClockOrigin = 1_000_000_000;

    internal static int Run(string family)
    {
        var wall = Stopwatch.StartNew(); bool passed = false; int completed = 0;
        try
        {
            using var credentials = new Credentials();
            IPAddress address = family == "ipv4" ? IPAddress.Loopback : IPAddress.IPv6Loopback;
            foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
            {
                // Identical idle negotiation in all three controls. The negative
                // control has no keepalive; each positive has one initiator.
                RunCase(credentials, cipher, address, -1); completed++;
                RunCase(credentials, cipher, address, 0); completed++;
                RunCase(credentials, cipher, address, 1); completed++;
            }
            passed = true;
        }
        catch (Exception error) { Console.Error.WriteLine(error); }
        Console.WriteLine("{\"passed\":" + (passed ? "true" : "false") + ",\"scenario\":\"keepalive\",\"family\":\"" + family +
            "\",\"completed_controls\":" + completed + ",\"required_controls\":6,\"wall_elapsed_ms\":" + wall.ElapsedMilliseconds +
            ",\"aot\":" + (!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false") + "}");
        return passed ? 0 : 1;
    }

    private static ulong Clock(void* context) => (ulong)Interlocked.Read(ref now);
    private static uint Dequeue(void* context, CXPLAT_EVENTQ* queue, CXPLAT_CQE* result, uint count, uint timeout)
        => services.CxPlatEventQDequeue(context, queue, result, count, Math.Min(timeout, 10));
    private static void Advance(uint milliseconds) => Interlocked.Add(ref now, milliseconds * 1000L);
    private static QUIC_HANDLE* Handle(int side) => (QUIC_HANDLE*)(side == 0 ? Volatile.Read(ref clientHandle) : Volatile.Read(ref serverHandle));
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException("Keepalive: " + message); }
    private static void Check(uint status, string operation)
    { Require(!Status.Failed(status), operation + " status=" + status); }
    private static void Wait(Func<bool> predicate, string description, bool inspectCallback = true)
    {
        long start = Stopwatch.GetTimestamp();
        while (!predicate())
        {
            if (inspectCallback && Volatile.Read(ref callbackFailure) is { } failure)
                throw new InvalidOperationException(failure);
            Require(Stopwatch.GetElapsedTime(start) < TimeSpan.FromSeconds(5), description + " exceeded wall watchdog");
            Thread.Sleep(1);
        }
    }
    private static long WaitWithVirtualProgress(Func<bool> predicate, string description)
    {
        long virtualStart = Interlocked.Read(ref now), wallStart = Stopwatch.GetTimestamp();
        while (!predicate())
        {
            if (Volatile.Read(ref callbackFailure) is { } failure) throw new InvalidOperationException(failure);
            Require(Volatile.Read(ref closed[0]) == 0 && Volatile.Read(ref closed[1]) == 0,
                description + " encountered a closed peer");
            Require(Interlocked.Read(ref now) - virtualStart < 1_000_000 &&
                Stopwatch.GetElapsedTime(wallStart) < TimeSpan.FromSeconds(5),
                description + " exceeded bounded virtual/wall progress");
            // Receiving valid traffic restarts KEEP_ALIVE from its actual
            // arrival time (connection.c5961,6246–6250); pacing may also schedule
            // a future send (send.c1352–1356). Do not freeze that new deadline
            // merely because the preceding fixed interval already elapsed.
            Step(5);
        }
        return Interlocked.Read(ref now) - virtualStart;
    }

    private static uint ConnectionCallback(QUIC_HANDLE* connection, void* context, QUIC_CONNECTION_EVENT* notification)
    {
        int side = context == (void*)1 ? 0 : 1;
        switch (notification->Type)
        {
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_CONNECTED:
                Interlocked.Increment(ref connected[side]); break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
                shutdownStatus[side] = notification->SHUTDOWN_INITIATED_BY_TRANSPORT.Status;
                Interlocked.Increment(ref shutdowns[side]); break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                Interlocked.Increment(ref closed[side]); break;
        }
        return Status.Success;
    }
    private static uint ListenerCallback(QUIC_HANDLE* listener, void* context, QUIC_LISTENER_EVENT* notification)
    {
        if (notification->Type != QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_NEW_CONNECTION) return Status.Success;
        if (Volatile.Read(ref serverHandle) != 0) return Status.ConnectionRefused;
        var connection = notification->NEW_CONNECTION.Connection;
        api->SetCallbackHandler(connection,
            (void*)(delegate*<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, uint>)&ConnectionCallback, (void*)2);
        uint status = api->ConnectionSetConfiguration(connection, serverConfiguration);
        if (!Status.Failed(status)) Volatile.Write(ref serverHandle, (nint)connection);
        else Volatile.Write(ref callbackFailure, "Keepalive accepted configuration status=" + status);
        // On rejection upstream owns and closes the unretained handle.
        return status;
    }
    private static QUIC_STATISTICS_V2 Statistics(int side)
    {
        Require(Volatile.Read(ref closed[side]) == 0, "statistics requested after shutdown on side " + side);
        QUIC_STATISTICS_V2 value = default; uint length = (uint)sizeof(QUIC_STATISTICS_V2);
        Check(api->GetParam(Handle(side), MsQuic.QUIC_PARAM_CONN_STATISTICS_V2, &length, &value), "statistics");
        Require(length == sizeof(QUIC_STATISTICS_V2), "statistics returned wrong size");
        return value;
    }
    private static void KeepAlive(int side, uint interval)
    {
        QUIC_SETTINGS update = default; var present = update.IsSet;
        present.KeepAliveIntervalMs = 1; update.IsSet = present; update.KeepAliveIntervalMs = interval;
        Check(api->SetParam(Handle(side), MsQuic.QUIC_PARAM_CONN_SETTINGS, (uint)sizeof(QUIC_SETTINGS), &update), "keepalive setting");
    }
    private static QUIC_HANDLE* Configuration(QUIC_HANDLE* registration, QUIC_BUFFER* alpn)
    {
        QUIC_SETTINGS settings = default; var present = settings.IsSet;
        present.IdleTimeoutMs = 1; present.HandshakeIdleTimeoutMs = 1; present.InitialRttMs = 1;
        present.KeepAliveIntervalMs = 1; present.MinimumMtu = 1; present.MaximumMtu = 1;
        settings.IsSet = present; settings.IdleTimeoutMs = IdleMilliseconds;
        settings.HandshakeIdleTimeoutMs = 10000; settings.InitialRttMs = 100;
        settings.KeepAliveIntervalMs = 0;
        // Avoid attributing DPLPMTUD probes to keepalive.
        settings.MinimumMtu = 1280; settings.MaximumMtu = 1280;
        QUIC_HANDLE* result = null;
        Check(api->ConfigurationOpen(registration, alpn, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, &result), "configuration");
        return result;
    }

    private static void Step(uint milliseconds)
    {
        // The same small steps drive active and disabled controls. A queued
        // delayed ACK may refresh the other peer before its next idle deadline.
        while (milliseconds != 0)
        {
            uint amount = Math.Min(milliseconds, 50);
            Advance(amount); milliseconds -= amount; Thread.Sleep(10);
        }
    }

    private static void AwaitConfirmedHandshake()
    {
        // There is no public read-only HandshakeConfirmed parameter. The pinned
        // original client local-address operation checks Started/Confirmed before
        // validating the address, and validates before any mutation (connection.c
        // 6453–6468). An invalid family therefore returns state1 until confirmed,
        // then argument22. This uses the real serialized connection operation;
        // it neither reads mutable core fields nor requests a valid rebind.
        QUIC_ADDR before = default; uint size = (uint)sizeof(QUIC_ADDR);
        Check(api->GetParam(Handle(0), MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, &size, &before), "pre-confirmation local address");
        QUIC_ADDR invalid = default; invalid.Ip.sa_family = ushort.MaxValue;
        long started = Interlocked.Read(ref now);
        while (true)
        {
            Require(Volatile.Read(ref closed[0]) == 0 && Volatile.Read(ref closed[1]) == 0, "closed before handshake confirmation");
            uint status = api->SetParam(Handle(0), MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, (uint)sizeof(QUIC_ADDR), &invalid);
            if (status == Status.InvalidParameter) break;
            Require(status == Status.InvalidState, "confirmation barrier status=" + status);
            Require(Interlocked.Read(ref now) - started < 1_000_000, "HANDSHAKE_DONE did not reach the client within setup bound");
            Step(25);
        }
        QUIC_ADDR after = default; size = (uint)sizeof(QUIC_ADDR);
        Check(api->GetParam(Handle(0), MsQuic.QUIC_PARAM_CONN_LOCAL_ADDRESS, &size, &after), "post-confirmation local address");
        Require(MsQuicHost.DatagramEndpoint(&before).Equals(MsQuicHost.DatagramEndpoint(&after)), "rejected confirmation operation changed the endpoint");
        // Server CONNECTED already implies confirmation (crypto.c1594–1610).
    }

    private static void ExpireIdle()
    {
        long started = Interlocked.Read(ref now);
        while (Volatile.Read(ref closed[0]) == 0 || Volatile.Read(ref closed[1]) == 0)
        {
            Require(Interlocked.Read(ref now) - started < 10_000_000, "disabled keepalive did not idle-expire both peers within virtual bound");
            if (Volatile.Read(ref callbackFailure) is { } failure) throw new InvalidOperationException(failure);
            Step(50);
            // Only callback-owned terminal counters are read after a peer closes.
            // No GetParam/Statistics poll runs on a closed connection here.
        }
    }

    private static void RunCase(Credentials certificates, ushort cipher, IPAddress address, int activeSide)
    {
        Array.Clear(connected); Array.Clear(closed); Array.Clear(shutdowns); Array.Clear(shutdownStatus);
        callbackFailure = null; clientHandle = serverHandle = 0; serverConfiguration = null; api = null;
        Interlocked.Exchange(ref now, ClockOrigin);
        using var host = new MsQuicHost(2);
        MsQuicHost.CredentialRegistration? serverCredential = null, clientCredential = null;
        QUIC_HANDLE* registration = null, listener = null, clientConfiguration = null;
        bool installed = false;
        ulong sentDelta = 0, ackDelta = 0; long progressWaitMicroseconds = 0;
        try
        {
            services = host.CreateTable(); var table = services;
            table.CxPlatTimeUs64 = &Clock; table.CxPlatEventQDequeue = &Dequeue;
            Check(MsQuic.MsQuicHostInstall(&table), "host install"); installed = true;
            void* opened = null; Check(MsQuic.MsQuicOpenVersion(2, &opened), "library open"); api = (QUIC_API_TABLE*)opened;
            fixed (byte* name = "keepalive-control\0"u8)
            {
                QUIC_REGISTRATION_CONFIG config = new() { AppName = name };
                Check(api->RegistrationOpen(&config, &registration), "registration");
            }
            fixed (byte* protocol = "keepalive-control"u8)
            {
                QUIC_BUFFER alpn = new() { Buffer = protocol, Length = 17 };
                serverConfiguration = Configuration(registration, &alpn);
                clientConfiguration = Configuration(registration, &alpn);
                serverCredential = host.CreateServerCredential(certificates.EcLeaf, cipherSuite: cipher);
                clientCredential = host.CreateClientCredential(new[] { certificates.Root }, cipher);
                Check(host.LoadCredential(serverConfiguration, serverCredential), "server credential");
                Check(host.LoadCredential(clientConfiguration, clientCredential), "client credential");
                Check(api->ListenerOpen(registration, &ListenerCallback, null, &listener), "listener");
                QUIC_ADDR endpoint = default; MsQuicHost.DatagramAddress(new(address, 0), &endpoint);
                Check(api->ListenerStart(listener, &alpn, 1, &endpoint), "listener start");
                uint size = (uint)sizeof(QUIC_ADDR);
                Check(api->GetParam(listener, MsQuic.QUIC_PARAM_LISTENER_LOCAL_ADDRESS, &size, &endpoint), "listener endpoint");
                QUIC_HANDLE* client = null;
                Check(api->ConnectionOpen(registration, &ConnectionCallback, (void*)1, &client), "client open");
                clientHandle = (nint)client;
                Check(api->SetParam(client, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, (uint)sizeof(QUIC_ADDR), &endpoint), "remote address");
                fixed (byte* name = "localhost\0"u8)
                    Check(api->ConnectionStart(client, clientConfiguration, endpoint.Ip.sa_family, name,
                        (ushort)MsQuicHost.DatagramEndpoint(&endpoint).Port), "connection start");
            }
            Wait(() => Volatile.Read(ref connected[0]) == 1 && Volatile.Read(ref connected[1]) == 1 && Volatile.Read(ref serverHandle) != 0, "authenticated peers");
            AwaitConfirmedHandshake();
            // Drain handshake ACK deadlines while allowing actual OS packets and
            // workers to run. This is setup, not keepalive success evidence.
            for (int i = 0; i < 8; i++) Step(25);
            Require(closed[0] == 0 && closed[1] == 0, "connection closed during setup");
            long startTime = Interlocked.Read(ref now);
            if (activeSide >= 0)
            {
                var initial = Statistics(activeSide);
                KeepAlive(activeSide, IntervalMilliseconds); // Original core sends the first PING immediately.
                // Recurring packet/ACK progression must sustain BOTH peers beyond
                // two idle windows. Counters do not identify individual PINGs:
                // occasional ACK/control/PTO packets may also contribute. The
                // identically stepped disabled control must actually expire.
                for (int step = 0; step < 20; step++)
                {
                    var before = Statistics(activeSide);
                    var peerBefore = Statistics(1 - activeSide);
                    Step(IntervalMilliseconds + 50);
                    progressWaitMicroseconds += WaitWithVirtualProgress(
                        () => Statistics(activeSide).SendTotalPackets > before.SendTotalPackets &&
                            Statistics(1 - activeSide).RecvTotalPackets > peerBefore.RecvTotalPackets,
                        "recurring packet reception while keepalive is enabled");
                    Step(50); // Release the real peer's delayed ACK deadline.
                    progressWaitMicroseconds += WaitWithVirtualProgress(
                        () => Statistics(activeSide).RecvValidAckFrames > before.RecvValidAckFrames,
                        "recurring traffic acknowledged while keepalive is enabled");
                    Require(closed[0] == 0 && closed[1] == 0, "unilateral keepalive failed to sustain both peers");
                }
                var after = Statistics(activeSide);
                sentDelta = after.SendTotalPackets - initial.SendTotalPackets;
                ackDelta = after.RecvValidAckFrames - initial.RecvValidAckFrames;
                Require(Interlocked.Read(ref now) - startTime >= 2 * IdleMilliseconds * 1000L && sentDelta >= 20 && ackDelta >= 20,
                    "keepalive did not produce recurring acknowledged packets beyond twice the negotiated idle timeout");
                Require(after.SendTotalStreamBytes == 0 && after.RecvTotalStreamBytes == 0 &&
                    Statistics(1 - activeSide).SendTotalStreamBytes == 0 && Statistics(1 - activeSide).RecvTotalStreamBytes == 0,
                    "application stream traffic contaminated idle control");
                KeepAlive(activeSide, 0);
            }
            // Both the disabled baseline and disable-after-survival case use
            // bounded small steps. A final delayed ACK may legitimately refresh
            // the peer's idle deadline, then both must expire without more data.
            ExpireIdle();
            for (int side = 0; side < 2; side++)
                Require(connected[side] == 1 && shutdowns[side] == 1 && closed[side] == 1 && shutdownStatus[side] == Status.ConnectionIdle,
                    "idle callback cardinality or exact status62 differs on side " + side);
        }
        finally
        {
            for (int side = 0; side < 2; side++)
            {
                var handle = Handle(side);
                if (handle == null) continue;
                if (Volatile.Read(ref closed[side]) == 0)
                {
                    api->ConnectionShutdown(handle, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_SILENT, 0);
                    int selected = side;
                    Wait(() => Volatile.Read(ref closed[selected]) != 0, "cleanup shutdown", inspectCallback: false);
                }
                api->ConnectionClose(handle);
            }
            clientHandle = serverHandle = 0;
            if (listener != null) api->ListenerClose(listener);
            if (clientConfiguration != null) api->ConfigurationClose(clientConfiguration);
            if (serverConfiguration != null) api->ConfigurationClose(serverConfiguration);
            if (registration != null) api->RegistrationClose(registration);
            if (api != null) MsQuic.MsQuicClose(api);
            clientCredential?.Dispose(); serverCredential?.Dispose();
            Require(host.OutstandingResources == 0 && host.OutstandingPlatformAllocations == 0 && host.OutstandingDatagramReceives == 0,
                "host resources, allocations or receive leases did not drain");
            Require(host.DatagramSendErrors == 0 && host.DatagramReceiveErrors == 0, "unexpected OS packet errors");
            if (installed) Check(MsQuic.MsQuicHostUninstall(), "host uninstall");
            api = null; serverConfiguration = null;
        }
        Console.WriteLine($"EVIDENCE keepalive cipher={cipher:x4} family={address.AddressFamily} initiator={activeSide} virtual_elapsed_us={Interlocked.Read(ref now) - ClockOrigin} recurring_packets={sentDelta} valid_ack_frames={ackDelta} progress_wait_virtual_us={progressWaitMicroseconds} idle_status=62,62 owners=0");
    }
}
