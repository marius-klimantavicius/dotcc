using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Uses the public owning facade for every transport operation. Core source
// restrictions are tested as observed statuses, never bypassed with raw handles.
internal static class NetworkParameters
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(25);

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
            await FamilyAsync(certificates, address);
        Console.WriteLine("PASS facade network parameters: explicit endpoint/interface/shared binding; pinned live remote rejection and actual local rebinding; FIFO/RoundRobin multistream delivery; real posted-byte backpressure/counter evolution and CUBIC units; IPv4/IPv6");
    }

    private static async Task FamilyAsync(Credentials certificates, IPAddress address)
    {
        int initialContexts = QuicObject.LiveContextCount;
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("network-parameter-controls");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: 0x1301);
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 16, PeerUnidiStreamCount = 32,
            StreamRecvWindowDefault = 16384, StreamRecvBufferDefault = 16384,
            ConnFlowControlWindow = 65536, SendBufferingEnabled = false,
            IdleTimeoutMs = 30000, HandshakeIdleTimeoutMs = 10000,
            CongestionControlAlgorithm = QuicCongestionControl.Cubic
        };
        await using var serverConfiguration = await registration.CreateConfigurationAsync(["network-parameters"u8.ToArray()], serverCredentials, settings);
        await using var clientConfiguration = await registration.CreateConfigurationAsync(["network-parameters"u8.ToArray()], clientCredentials, settings);
        await using var listener = await registration.ListenAsync(serverConfiguration, new(address, 0));
        uint index = LoopbackIndex(address);
        var requested = new IPEndPoint(address, 0);
        var first = await Connect(registration, clientConfiguration, listener,
            new() { LocalEndPoint = requested, LocalInterfaceIndex = index, ShareUdpBinding = true });
        await using var firstClient = first.Client;
        await using var firstServer = first.Server;
        var local = firstClient.GetLocalEndPoint(QuicParameterPriority.High);
        Check(local.Address.Equals(address) && local.Port != 0 && firstServer.GetRemoteEndPoint().Equals(local),
            "Explicit local address/interface did not reach the actual peer");
        Check(requested.Port == 0, "Connection factory mutated the caller's requested endpoint");
        Check(firstClient.GetUdpBindingShared() && firstClient.GetRemoteEndPoint().Equals(listener.LocalEndPoint),
            "Explicit shared binding/remote endpoint options were not applied");
        await Batch(firstClient, firstServer, 2);

        var second = await Connect(registration, clientConfiguration, listener,
            new() { LocalEndPoint = local, LocalInterfaceIndex = index, ShareUdpBinding = true });
        await using var secondClient = second.Client;
        await using var secondServer = second.Server;
        Check(secondClient.GetLocalEndPoint().Equals(local) && secondServer.GetRemoteEndPoint().Equals(local) && secondClient.GetUdpBindingShared(),
            "Second simultaneous connection did not use the same selected source port");
        await Task.WhenAll(Batch(firstClient, firstServer, 2), Batch(secondClient, secondServer, 2));
        Check(firstClient.GetOriginalDestinationConnectionId().Span.SequenceEqual(secondClient.GetOriginalDestinationConnectionId().Span) == false,
            "Shared source binding reused the original destination connection identity");
        await firstClient.DisposeAsync().AsTask().WaitAsync(Limit);
        await firstServer.DisposeAsync().AsTask().WaitAsync(Limit);
        // Closing one binding user must preserve the other connection's socket,
        // route and callback ownership, demonstrated by fresh data and FIN.
        await Batch(secondClient, secondServer, 2);

        var remoteBefore = secondClient.GetRemoteEndPoint();
        var serverRemoteBefore = secondServer.GetRemoteEndPoint();
        var differentRemote = new IPEndPoint(address, remoteBefore.Port == 65535 ? 1 : remoteBefore.Port + 1);
        // connection.c:6424,6544 rejects Started || ClosedLocally before role or
        // address checks. There is no live remote-migration setter in this pin.
        RequireStatus(() => secondClient.SetRemoteEndPoint(differentRemote, QuicParameterPriority.High), 1);
        RequireStatus(() => secondServer.SetRemoteEndPoint(differentRemote), 1);
        Check(secondClient.GetRemoteEndPoint().Equals(remoteBefore) && secondServer.GetRemoteEndPoint().Equals(serverRemoteBefore),
            "Rejected live remote-address update changed a route");
        await Batch(secondClient, secondServer, 2);
        await Batch(secondServer, secondClient, 2);

        foreach (var scheme in new[] { QuicStreamScheduling.Fifo, QuicStreamScheduling.RoundRobin })
        {
            secondClient.SetStreamScheduling(scheme, QuicParameterPriority.High);
            secondServer.SetStreamScheduling(scheme);
            Check(secondClient.GetStreamScheduling() == scheme && secondServer.GetStreamScheduling(QuicParameterPriority.High) == scheme,
                "Stream scheduling option did not round trip");
            try { secondClient.SetStreamScheduling((QuicStreamScheduling)99); throw new InvalidOperationException("Unknown scheduling scheme accepted"); }
            catch (ArgumentOutOfRangeException) { }
            Check(secondClient.GetStreamScheduling() == scheme, "Rejected scheduler value mutated the selected scheme");
            // All streams carry different IDs/payload lengths and run together.
            // Correct delivery and bounded completion are required in both
            // modes; arrival order is not falsely equated with scheduler order.
            await Task.WhenAll(Batch(secondClient, secondServer, 4), Batch(secondServer, secondClient, 4));
        }

        await StatisticsUnderBackpressure(runtime, secondClient, secondServer);
        await LiveLocalRebinding(runtime, secondClient, secondServer, address);
        await InvalidInterface(registration, clientConfiguration, listener, address);
        await Batch(secondClient, secondServer, 2); // Failed startup left the existing shared socket intact.
        await secondClient.ShutdownAsync().WaitAsync(Limit);
        RequireStatus(() => secondClient.SetRemoteEndPoint(differentRemote), 1);
        await secondClient.DisposeAsync().AsTask().WaitAsync(Limit);
        try { secondClient.SetRemoteEndPoint(differentRemote); throw new InvalidOperationException("Disposed route setter accepted"); }
        catch (ObjectDisposedException) { }
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(QuicObject.LiveContextCount == initialContexts && runtime.Host.OutstandingResources == 0 &&
            runtime.Host.OutstandingPlatformAllocations == 0 && runtime.Host.OutstandingDatagramReceives == 0,
            "Network parameter controls left context/host/buffer ownership live");
        Console.WriteLine($"PASS facade network parameters {address.AddressFamily}: explicit interface/shared binding, rejected remote update, local rebinding with public PATH_RESPONSE counter, scheduling and statistics");
    }

    private static uint LoopbackIndex(IPAddress address)
    {
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback) continue;
            var properties = adapter.GetIPProperties();
            if (!properties.UnicastAddresses.Any(item => item.Address.Equals(address))) continue;
            int index = address.AddressFamily == AddressFamily.InterNetwork ? properties.GetIPv4Properties()!.Index : properties.GetIPv6Properties()!.Index;
            if (index > 0) return checked((uint)index);
        }
        throw new InvalidOperationException("Required actual loopback interface is unavailable for " + address.AddressFamily);
    }

    private static async Task<(QuicConnection Client, QuicConnection Server)> Connect(QuicRegistration registration,
        QuicConfiguration configuration, QuicListener listener, QuicConnectOptions options)
    {
        using var cancellation = new CancellationTokenSource(Limit);
        Task<QuicConnection> accepted = listener.AcceptConnectionAsync(cancellation.Token).AsTask();
        QuicConnection? client = null;
        try
        {
            client = await registration.ConnectAsync(configuration, "localhost", listener.LocalEndPoint, options, cancellation.Token);
            return (client, await accepted);
        }
        catch
        {
            cancellation.Cancel();
            if (client != null) await client.DisposeAsync();
            try { await using var orphan = await accepted.WaitAsync(Limit); }
            catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
            throw;
        }
    }

    private static async Task InvalidInterface(QuicRegistration registration, QuicConfiguration configuration,
        QuicListener listener, IPAddress address)
    {
        using var timeout = new CancellationTokenSource(Limit);
        try
        {
            await using var unexpected = await registration.ConnectAsync(configuration, "localhost", listener.LocalEndPoint,
                new() { LocalEndPoint = new(address, 0), LocalInterfaceIndex = uint.MaxValue }, timeout.Token);
        }
        catch (QuicTransportException error)
        {
            // The qualified BCL socket layer rejects nonexistent Linux
            // interface indices with EADDRNOTAVAIL99; no fake connect result.
            Check(error.Status == 99, "Nonexistent interface lost address-not-available status: " + error.Status);
            return;
        }
        throw new InvalidOperationException("A nonexistent interface established a connection");
    }

    private static async Task LiveLocalRebinding(QuicRuntime runtime, QuicConnection client, QuicConnection server, IPAddress address)
    {
        var oldLocal = client.GetLocalEndPoint();
        var serverLocal = server.GetLocalEndPoint();
        var remote = client.GetRemoteEndPoint();
        RequireStatus(() => server.SetLocalEndPoint(new(address, 0)), 1);
        Check(server.GetLocalEndPoint().Equals(serverLocal), "Rejected server local update changed its binding");
        long validated = runtime.GetPerformanceCounters()[QuicPerformanceCounter.ValidatedPaths];
        // Several completed data+FIN exchanges above ensure handshake traffic
        // has progressed before the real setter's HandshakeConfirmed gate.
        client.SetLocalEndPoint(new(address, 0), QuicParameterPriority.High);
        var changed = client.GetLocalEndPoint();
        Check(changed.Address.Equals(address) && changed.Port != 0 && changed.Port != oldLocal.Port &&
            client.GetRemoteEndPoint().Equals(remote), "Successful local rebind did not select a new source port with the same remote");
        await Batch(client, server, 2);
        using var timeout = new CancellationTokenSource(Limit);
        while (!server.GetRemoteEndPoint().Equals(changed) ||
            runtime.GetPerformanceCounters()[QuicPerformanceCounter.ValidatedPaths] <= validated)
            await Task.Delay(1, timeout.Token);
        // The public counter is incremented only when the actual core accepts a
        // PATH_RESPONSE (connection.c). No private IsPeerValidated storage is
        // queried or inferred from a getter-only address change.
        await Batch(server, client, 2);
        Check(server.GetRemoteEndPoint().Equals(changed), "Rebound peer route reverted during return traffic");
    }

    private static async Task Batch(QuicConnection sender, QuicConnection receiver, int count)
    {
        using var cancellation = new CancellationTokenSource(Limit);
        CancellationToken token = cancellation.Token;
        var writers = new List<QuicStream>();
        var sends = new List<Task>();
        var readers = new List<Task<ulong>>();
        async Task ReceiveAll()
        {
            for (int i = 0; i < count; i++)
            {
                var stream = await receiver.AcceptStreamAsync(token);
                readers.Add(ReadOne(stream, token));
            }
        }
        Task accepting = ReceiveAll();
        try
        {
            for (int i = 0; i < count; i++)
            {
                var stream = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional, cancellationToken: token);
                writers.Add(stream);
                sends.Add(stream.SendAsync(Payload(stream.Id), QuicSendOptions.Fin, token).AsTask());
            }
            await accepting;
            ulong[] receivedIds = await Task.WhenAll(readers);
            await Task.WhenAll(sends);
            foreach (var writer in writers) await writer.CompleteWritesAsync(token);
            Check(receivedIds.Distinct().Count() == count && receivedIds.Order().SequenceEqual(writers.Select(stream => stream.Id).Order()),
                "Concurrent scheduling/shared binding mixed or lost stream identities");
        }
        finally
        {
            cancellation.Cancel();
            foreach (var writer in writers) await writer.DisposeAsync();
            try { await accepting; } catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
            try { await Task.WhenAll(readers); } catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
            try { await Task.WhenAll(sends); } catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
        }
    }
    private static async Task<ulong> ReadOne(QuicStream stream, CancellationToken token)
    {
        await using (stream)
        {
            ulong id = stream.Id;
            byte[] expected = Payload(id), buffer = new byte[8179];
            int offset = 0;
            while (true)
            {
                int count = await stream.ReadAsync(buffer, token);
                if (count == 0) break;
                Check(count <= expected.Length - offset && buffer.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)),
                    "Multistream payload differs from its stream ID pattern");
                offset += count;
            }
            Check(offset == expected.Length, "Multistream FIN arrived before complete payload");
            return id;
        }
    }
    private static byte[] Payload(ulong id)
    {
        byte[] bytes = new byte[65537 + (int)(id % 3) * 8192];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)((ulong)i * 31 + id * 17));
        return bytes;
    }

    private static async Task StatisticsUnderBackpressure(QuicRuntime runtime, QuicConnection sender, QuicConnection receiver)
    {
        using var cancellation = new CancellationTokenSource(Limit);
        var token = cancellation.Token;
        var before = sender.GetNetworkStatistics(); CheckUnits(before);
        var countersBefore = runtime.GetPerformanceCounters();
        await using var writer = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional, cancellationToken: token);
        byte[] expected = new byte[2 * 1024 * 1024];
        for (int i = 0; i < expected.Length; i++) expected[i] = unchecked((byte)(i * 43 + 19));
        Task sending = writer.SendAsync(expected, QuicSendOptions.Fin, token).AsTask();
        await using var reader = await receiver.AcceptStreamAsync(token);
        int offset = 0;
        using (var offer = await reader.ReceiveAsync(token) ?? throw new InvalidOperationException("Missing held receive offer"))
        {
            foreach (var buffer in offer.Buffers)
            {
                Check(buffer.Span.SequenceEqual(expected.AsSpan(offset, buffer.Length)), "Held receive data changed");
                offset += buffer.Length;
            }
            Check(offset > 0 && offset <= 65536 && !sending.IsCompleted, "Receive window did not hold an admitted large send");
            var loaded = sender.GetNetworkStatistics(QuicParameterPriority.High); CheckUnits(loaded);
            Check(loaded.PostedBytes > before.PostedBytes && loaded.PostedBytes >= (ulong)(expected.Length - 65536),
                "Actual outstanding send bytes did not appear while receive ownership blocked flow control");
            offer.Complete(offer.Length);
        }
        byte[] scratch = new byte[16381];
        while (true)
        {
            int count = await reader.ReadAsync(scratch, token);
            if (count == 0) break;
            Check(count <= expected.Length - offset && scratch.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)),
                "Backpressure statistics transfer corrupted or duplicated payload");
            offset += count;
        }
        Check(offset == expected.Length, "Backpressure FIN truncated the transfer");
        await sending;
        await writer.CompleteWritesAsync(token);
        var drained = sender.GetNetworkStatistics(); CheckUnits(drained);
        Check(drained.PostedBytes == before.PostedBytes, "Completed send remained in actual PostedBytes");
        var countersAfter = runtime.GetPerformanceCounters();
        Check(countersAfter[QuicPerformanceCounter.ApplicationSentBytes] - countersBefore[QuicPerformanceCounter.ApplicationSentBytes] == expected.Length &&
            countersAfter[QuicPerformanceCounter.ApplicationReceivedBytes] - countersBefore[QuicPerformanceCounter.ApplicationReceivedBytes] == expected.Length,
            "Application counters did not count exactly the delivered payload bytes");
        Check(countersAfter[QuicPerformanceCounter.SentDatagrams] > countersBefore[QuicPerformanceCounter.SentDatagrams] &&
            countersAfter[QuicPerformanceCounter.ReceivedDatagrams] > countersBefore[QuicPerformanceCounter.ReceivedDatagrams],
            "Network counters did not evolve during actual UDP transfer");
    }
    private static void CheckUnits(QuicNetworkStatistics sample)
    {
        // Actual CUBIC source computes bytes / microseconds with integer
        // truncation. This is not bits/sec or a guessed throughput multiplier.
        ulong bandwidth = sample.SmoothedRttMicroseconds == 0 ? 0 : sample.CongestionWindowBytes / sample.SmoothedRttMicroseconds;
        Check(sample.BandwidthBytesPerMicrosecond == bandwidth && sample.CongestionWindowBytes > 0 && sample.IdealBytes > 0,
            "Typed network statistics changed CUBIC byte/time units or omitted live state");
    }
    private static void RequireStatus(Action operation, uint status)
    {
        try { operation(); }
        catch (QuicTransportException error) { Check(error.Status == status, "Wrong upstream parameter status: " + error.Status); return; }
        throw new InvalidOperationException("Expected upstream parameter rejection");
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
