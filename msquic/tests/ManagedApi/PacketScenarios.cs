using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using Managed.Transport.Api;

// Real facade endpoints and a UDP forwarding socket. The relay only drops whole
// encrypted datagrams; it does not manufacture packets, keys or completion events.
internal static class PacketScenarios
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);
    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
            await using var registration = await runtime.OpenRegistrationAsync("packet-controls");
            using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: suite);
            using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: suite);
            // settings.c only bounds MaxBytesPerKey above by 2^38. 32 KiB is
            // comfortably above one MTU and forces repeated updates in this run.
            var settings = new QuicSettings
            {
                MaxBytesPerKey = 32768, DatagramReceiveEnabled = true, SendBufferingEnabled = false,
                PeerBidiStreamCount = 16, PeerUnidiStreamCount = 16, IdleTimeoutMs = 20000,
                DisconnectTimeoutMs = 10000, StreamRecvWindowDefault = 65536,
                StreamRecvBufferDefault = 65536, ConnFlowControlWindow = 262144
            };
            await using var serverConfig = await registration.CreateConfigurationAsync(["packet-controls"u8.ToArray()], serverCredentials, settings);
            await using var clientConfig = await registration.CreateConfigurationAsync(["packet-controls"u8.ToArray()], clientCredentials, settings);
            await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
            await using var relay = new Relay(listener.LocalEndPoint);
            var pair = await Connect(registration, clientConfig, listener, relay.LocalEndPoint);
            await using (pair.Client)
            await using (pair.Server)
            {
                await RepeatedKeyUpdates(pair.Client, pair.Server);
                await DatagramSizes(pair.Client, pair.Server);
                await DatagramSizes(pair.Server, pair.Client);
                await DroppedAndQueuedDatagrams(pair.Client, pair.Server, relay);
            }
            // A fresh direct connection isolates receive/close races from the
            // intentional preceding network partition and its loss recovery.
            var race = await Connect(registration, clientConfig, listener, listener.LocalEndPoint);
            await using (race.Client)
            await using (race.Server) await ReceiveCloseRace(race.Client, race.Server);
            await registration.DisposeAsync().AsTask().WaitAsync(Limit);
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0 &&
                runtime.Host.OutstandingDatagramReceives == 0, "packet campaign host ownership did not drain");
            await DatagramByteBudget(address, suite, certificates);
            Console.WriteLine($"PASS facade packet controls cipher=0x{suite:x} family={address.AddressFamily}");
        }
    }

    private static async Task DatagramByteBudget(IPAddress address, ushort suite, Credentials certificates)
    {
        await using var runtime = await QuicRuntime.CreateAsync(new()
        {
            ProcessorCount = 1, MaximumCopiedSendBytes = 4096, MaximumReceiveOfferBytes = 4096,
            ConnectionBufferBudget = 8192, RuntimeBufferBudget = 16384
        });
        await using var registration = await runtime.OpenRegistrationAsync("datagram-byte-budget");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: suite);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: suite);
        var settings = new QuicSettings { DatagramReceiveEnabled = true, IdleTimeoutMs = 20000 };
        await using var serverConfig = await registration.CreateConfigurationAsync(["byte-budget"u8.ToArray()], serverCredentials, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["byte-budget"u8.ToArray()], clientCredentials, settings);
        await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
        await using var relay = new Relay(listener.LocalEndPoint);
        var pair = await Connect(registration, clientConfig, listener, relay.LocalEndPoint);
        await using (pair.Client)
        await using (pair.Server)
        {
            await Until(() => pair.Client.MaximumDatagramSendLength > 0);
            relay.DropClient = true;
            int length = pair.Client.MaximumDatagramSendLength;
            Check(length <= 4096, "negotiated DATAGRAM exceeds this byte-budget test's send-copy limit");
            var pending = new List<Task<QuicDatagramSendResult>>();
            bool rejected = false;
            for (int i = 0; i < 32; i++)
            {
                Task<QuicDatagramSendResult> send = pair.Client.SendDatagramAsync(Payload(length, 601 + i)).AsTask();
                if (send.IsFaulted)
                {
                    await Throws<InvalidOperationException>(send); rejected = true; break;
                }
                pending.Add(send);
            }
            var ownership = pair.Client.PacketScenarioOwnership;
            Check(rejected && ownership.Sends < 64 && ownership.Bytes <= 8192 &&
                ownership.Bytes + length > 8192, "byte budget did not reject before the independent send-count bound");
            await pair.Client.DisposeAsync().AsTask().WaitAsync(Limit);
            var results = await Task.WhenAll(pending).WaitAsync(Limit);
            Check(results.All(value => value is QuicDatagramSendResult.Lost or QuicDatagramSendResult.Canceled),
                "partitioned byte-budget sends did not reach actual terminal states");
            Check(pair.Client.PacketScenarioOwnership == (0, 0L), "byte-budget reservations did not drain");
            await pair.Server.DisposeAsync().AsTask().WaitAsync(Limit);
        }
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0,
            "byte-budget control leaked host ownership");
    }

    private static async Task<(QuicConnection Client, QuicConnection Server)> Connect(QuicRegistration registration,
        QuicConfiguration configuration, QuicListener listener, IPEndPoint destination)
    {
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accept = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        QuicConnection? client = null;
        try
        {
            client = await registration.ConnectAsync(configuration, "localhost", destination, timeout.Token);
            return (client, await accept);
        }
        catch { if (client != null) await client.DisposeAsync(); throw; }
    }

    private static async Task RepeatedKeyUpdates(QuicConnection client, QuicConnection server)
    {
        var beforeClient = client.GetStatistics(); var beforeServer = server.GetStatistics();
        // Several acknowledged batches permit key-phase confirmation between
        // updates. Both endpoints send and verify independent nonrepeating data.
        for (int batch = 0; batch < 4; batch++)
        {
            await using var outgoing = await client.OpenStreamAsync().AsTask().WaitAsync(Limit);
            byte[] fromClient = Payload(262144, batch), fromServer = Payload(262144, 100 + batch);
            Task sendClient = outgoing.SendAsync(fromClient, QuicSendOptions.Fin).AsTask();
            Task receiveClient = ReadExact(outgoing, fromServer);
            await using var incoming = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
            Task sendServer = incoming.SendAsync(fromServer, QuicSendOptions.Fin).AsTask();
            await Task.WhenAll(sendClient, receiveClient, sendServer, ReadExact(incoming, fromClient)).WaitAsync(Limit);
            await outgoing.CompleteWritesAsync().AsTask().WaitAsync(Limit);
            await incoming.CompleteWritesAsync().AsTask().WaitAsync(Limit);
            Check(outgoing.GetObservedEarlyDataLength() == 0 && incoming.GetObservedEarlyDataLength() == 0,
                "key-update stream reported early data");
        }
        var afterClient = client.GetStatistics(); var afterServer = server.GetStatistics();
        Check(afterClient.KeyUpdates >= beforeClient.KeyUpdates + 2 && afterServer.KeyUpdates >= beforeServer.KeyUpdates + 2,
            $"missing repeated actual key updates client={afterClient.KeyUpdates} server={afterServer.KeyUpdates}");
        // Upstream counts retransmitted frame bytes here as well; exact unique
        // payload/FIN correctness is established by ReadExact above.
        Check(afterClient.SentStreamBytes - beforeClient.SentStreamBytes >= 1048576 &&
            afterServer.SentStreamBytes - beforeServer.SentStreamBytes >= 1048576,
            "key-update stream byte counters are smaller than transferred bytes");
        Check(afterClient.DecryptionFailures == 0 && afterServer.DecryptionFailures == 0, "key-update traffic failed decryption");
    }

    private static async Task DatagramSizes(QuicConnection sender, QuicConnection receiver)
    {
        await Until(() => sender.MaximumDatagramSendLength > 0);
        var capabilities = sender.GetCapabilities();
        Check(capabilities.DatagramReceiveEnabled && capabilities.DatagramSendEnabled, "DATAGRAM capability not negotiated");
        int maximum = sender.MaximumDatagramSendLength;
        foreach (int length in new[] { 0, 37, maximum })
        {
            byte[] expected = Payload(length, length + 71), caller = expected.ToArray();
            Task<ReadOnlyMemory<byte>> receive = receiver.ReceiveDatagramAsync().AsTask();
            Task<QuicDatagramSendResult> send = sender.SendDatagramAsync(caller).AsTask();
            caller.AsSpan().Fill(0xff); // Send owns a snapshot before its first await.
            ReadOnlyMemory<byte> observed = await receive.WaitAsync(Limit);
            Check(observed.Span.SequenceEqual(expected), "DATAGRAM size/caller ownership mismatch");
            var result = await send.WaitAsync(Limit);
            Check(result is QuicDatagramSendResult.Acknowledged or QuicDatagramSendResult.AcknowledgedAfterLoss,
                "delivered DATAGRAM did not receive a final acknowledgment");
        }
        int count = sender.PacketScenarioOwnership.Sends;
        await Throws<InvalidOperationException>(sender.SendDatagramAsync(new byte[maximum + 1]).AsTask());
        Check(sender.PacketScenarioOwnership.Sends == count, "oversized DATAGRAM allocated an outstanding send");
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        await Throws<OperationCanceledException>(sender.SendDatagramAsync(new byte[1], canceled.Token).AsTask());
        Check(sender.PacketScenarioOwnership.Sends == count, "pre-canceled DATAGRAM was admitted");
    }

    private static async Task DroppedAndQueuedDatagrams(QuicConnection sender, QuicConnection receiver, Relay relay)
    {
        relay.DropClient = true;
        long droppedBefore = relay.DroppedLargePackets;
        int length = sender.MaximumDatagramSendLength;
        var admitted = new List<Task<QuicDatagramSendResult>>
        {
            sender.SendDatagramAsync(Payload(length, 200)).AsTask()
        };
        await Until(() => relay.DroppedLargePackets > droppedBefore);
        using var cancelWait = new CancellationTokenSource();
        Task<QuicDatagramSendResult> canceledWait = sender.SendDatagramAsync(Payload(length, 201), cancelWait.Token).AsTask();
        cancelWait.Cancel();
        await Throws<OperationCanceledException>(canceledWait);
        Check(sender.PacketScenarioOwnership.Sends > 0, "canceling a DATAGRAM waiter released native send ownership early");
        int rejected = 0;
        for (int i = 0; i < 80; i++)
        {
            Task<QuicDatagramSendResult> operation = sender.SendDatagramAsync(Payload(length, 300 + i)).AsTask();
            if (operation.IsFaulted)
            {
                await Throws<InvalidOperationException>(operation); rejected++; break;
            }
            admitted.Add(operation);
        }
        Check(rejected != 0 && sender.PacketScenarioOwnership.Sends <= 64, "bounded DATAGRAM admission did not reject excess work");
        await Until(() => relay.DroppedLargePackets > droppedBefore);
        // Partition remains active through close. Already transmitted datagrams
        // retire as lost; work still in the real core send queue is canceled.
        await sender.DisposeAsync().AsTask().WaitAsync(Limit);
        var outcomes = await Task.WhenAll(admitted).WaitAsync(Limit);
        Check(outcomes.Contains(QuicDatagramSendResult.Canceled), "close did not cancel queued DATAGRAM work");
        Check(outcomes.Contains(QuicDatagramSendResult.Lost), "dropped transmitted DATAGRAM did not retire as lost");
        Check(outcomes.All(value => value is QuicDatagramSendResult.Canceled or QuicDatagramSendResult.Lost),
            "partitioned DATAGRAM unexpectedly acknowledged");
        Check(sender.PacketScenarioOwnership == (0, 0L), "DATAGRAM close left reservations or callback owners");
        await receiver.DisposeAsync().AsTask().WaitAsync(Limit);
        relay.DropClient = false;
    }

    private static async Task ReceiveCloseRace(QuicConnection sender, QuicConnection receiver)
    {
        await Until(() => sender.MaximumDatagramSendLength > 0);
        byte[] expected = Payload(31, 501);
        Task<ReadOnlyMemory<byte>>[] pending = Enumerable.Range(0, 32)
            .Select(_ => receiver.ReceiveDatagramAsync().AsTask()).ToArray();
        var sends = new List<Task<QuicDatagramSendResult>>();
        for (int i = 0; i < 32; i++) sends.Add(sender.SendDatagramAsync(expected).AsTask());
        Task closing = receiver.DisposeAsync().AsTask();
        foreach (var receive in pending)
        {
            try { Check((await receive.WaitAsync(Limit)).Span.SequenceEqual(expected), "receive/close handoff corrupted copied data"); }
            catch (Exception error) when (error is ObjectDisposedException or ChannelClosedException) { }
        }
        await closing.WaitAsync(Limit);
        await sender.DisposeAsync().AsTask().WaitAsync(Limit);
        foreach (var send in sends)
        {
            var final = await send.WaitAsync(Limit);
            Check(final is QuicDatagramSendResult.Canceled or QuicDatagramSendResult.Lost or
                QuicDatagramSendResult.Acknowledged or QuicDatagramSendResult.AcknowledgedAfterLoss,
                "non-final DATAGRAM result exposed");
        }
        Check(receiver.PacketScenarioOwnership == (0, 0L) && sender.PacketScenarioOwnership == (0, 0L),
            "receive/close race leaked ownership");
    }

    private static async Task ReadExact(QuicStream stream, byte[] expected)
    {
        byte[] buffer = new byte[4093]; int offset = 0;
        while (true)
        {
            int count = await stream.ReadAsync(buffer).AsTask().WaitAsync(Limit);
            if (count == 0) break;
            Check(count <= expected.Length - offset && buffer.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)),
                "key-update stream content/order mismatch");
            offset += count;
        }
        Check(offset == expected.Length, "key-update stream FIN truncated payload");
    }
    private static byte[] Payload(int length, int salt)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++) bytes[i] = unchecked((byte)(i * 31 + (i >> 8) * 17 + salt));
        return bytes;
    }
    private static async Task Until(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(Limit);
        while (!condition()) await Task.Delay(1, timeout.Token);
    }
    private static async Task Throws<T>(Task task) where T : Exception
    {
        try { await task.WaitAsync(Limit); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Packet controls: " + message); }

    private sealed class Relay : IAsyncDisposable
    {
        private readonly Socket socket;
        private readonly IPEndPoint server;
        private IPEndPoint? client;
        private readonly CancellationTokenSource stop = new();
        private readonly Task forwarding;
        private int dropClient;
        private long droppedLarge;
        internal IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint!;
        internal bool DropClient { set => Volatile.Write(ref dropClient, value ? 1 : 0); }
        internal long DroppedLargePackets => Interlocked.Read(ref droppedLarge);
        internal Relay(IPEndPoint server)
        {
            this.server = server;
            socket = new(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (server.AddressFamily == AddressFamily.InterNetworkV6) socket.DualMode = false;
            socket.Bind(new IPEndPoint(server.Address, 0));
            forwarding = Forward();
        }
        private async Task Forward()
        {
            byte[] bytes = new byte[65536];
            EndPoint any = new IPEndPoint(server.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
            try
            {
                while (true)
                {
                    var received = await socket.ReceiveFromAsync(bytes, SocketFlags.None, any, stop.Token);
                    bool fromServer = received.RemoteEndPoint.Equals(server);
                    if (!fromServer)
                    {
                        client = (IPEndPoint)received.RemoteEndPoint;
                        if (Volatile.Read(ref dropClient) != 0)
                        {
                            if (received.ReceivedBytes > 1000) Interlocked.Increment(ref droppedLarge);
                            continue;
                        }
                    }
                    IPEndPoint? destination = fromServer ? client : server;
                    if (destination != null)
                        await socket.SendToAsync(bytes.AsMemory(0, received.ReceivedBytes), SocketFlags.None, destination, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
        }
        public async ValueTask DisposeAsync()
        {
            stop.Cancel(); socket.Dispose();
            try { await forwarding; } finally { stop.Dispose(); }
        }
    }
}

namespace Managed.Transport.Api
{
    // Read-only source-linked test observation. No replacement callback or native
    // ownership mutation; production assembly does not include this partial.
    public sealed partial class QuicConnection
    {
        internal (int Sends, long Bytes) PacketScenarioOwnership
        { get { lock (Gate) return (datagramSends.Count, bufferedBytes); } }
    }
}
