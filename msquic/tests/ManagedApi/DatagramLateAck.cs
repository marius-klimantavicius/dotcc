using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Pending outside the active ManagedApi compile/receipt glob. This uses real
// encrypted UDP packets and public DATAGRAM operations, never injected callbacks.
internal static class DatagramLateAck
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(25);
    private const int PayloadLength = 1100;
    private const int LaterPackets = 8;

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
            await RunCase(certificates, cipher, address);
    }

    private static async Task RunCase(Credentials certificates, ushort cipher, IPAddress address)
    {
        int contexts = QuicObject.LiveContextCount;
        string evidence;
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("datagram-late-ack");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: cipher);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: cipher);
        // loss_detection.c retains LostPackets for two PTOs, each including the
        // peer transport parameter MaxAckDelay. This provides >=2 seconds to
        // observe real loss and release the held packet before it is forgotten.
        // Fixed MTU excludes large discovery probes from the relay's selector.
        var settings = new QuicSettings
        {
            DatagramReceiveEnabled = true, SendBufferingEnabled = false,
            MinimumMtu = 1280, MaximumMtu = 1280, InitialWindowPackets = 32,
            MaxAckDelayMs = 1000, HandshakeIdleTimeoutMs = 15000, IdleTimeoutMs = 30000
        };
        await using var serverConfig = await registration.CreateConfigurationAsync(["datagram-late-ack"u8.ToArray()], serverCredentials, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["datagram-late-ack"u8.ToArray()], clientCredentials, settings);
        await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
        await using var relay = new HoldRelay(listener.LocalEndPoint);
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        await using (var client = await registration.ConnectAsync(clientConfig, "localhost", relay.LocalEndPoint, timeout.Token))
        await using (var server = await accepting)
        {
            await Until(() => client.MaximumDatagramSendLength >= PayloadLength && server.MaximumDatagramSendLength >= PayloadLength, relay, timeout.Token);
            // Both directions actually send/receive/ACK 1-RTT application data
            // before the selector is armed. No earlier large application sends.
            await Exchange(client, server, Payload(32, 201), timeout.Token);
            await Exchange(server, client, Payload(32, 202), timeout.Token);
            Check(client.PacketScenarioOwnership == (0, 0L) && server.PacketScenarioOwnership == (0, 0L), "warm-up ownership did not drain");
            var before = client.GetStatistics();
            byte[] original = Payload(PayloadLength, 91);
            byte[] expected = original.ToArray();
            relay.Arm();
            Task<QuicDatagramSendResult> target = client.SendDatagramAsync(original).AsTask();
            await relay.Held.WaitAsync(timeout.Token);
            // The real facade's copied/pinned send owner, not this caller array,
            // must survive both interim loss and a collection until final ACK.
            Array.Fill(original, (byte)0xff);
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Check(!target.IsCompleted, "held target completed before any ACK could pass");

            var sends = new List<Task<QuicDatagramSendResult>>();
            for (int i = 0; i < LaterPackets; i++)
                sends.Add(client.SendDatagramAsync(Payload(PayloadLength, i)).AsTask());
            var seen = new HashSet<byte>();
            for (int i = 0; i < LaterPackets; i++)
            {
                ReadOnlyMemory<byte> received = await server.ReceiveDatagramAsync(timeout.Token);
                byte id = received.Span[0];
                Check(id < LaterPackets && seen.Add(id) && received.Span.SequenceEqual(Payload(PayloadLength, id)),
                    "later payload missing/duplicated, or relay held the wrong packet");
            }
            // Two 1100-byte payloads cannot share a 1280-byte UDP packet. Eight
            // later delivered packets exceed core's strict PN+3<largest-ACK
            // threshold. The test does not decode or manufacture their ACKs.
            await Until(() => relay.BufferedAcknowledgments > 0, relay, timeout.Token);
            await relay.ReleaseAcknowledgmentsAsync(timeout.Token);
            await Until(() =>
            {
                Check(!target.IsCompleted, "target reached a final state before held packet release");
                return client.GetStatistics().SuspectedLostPackets > before.SuspectedLostPackets;
            }, relay, timeout.Token);
            // Existing read-only source-linked harness observation, absent from
            // the product: an interim loss must retain send/copy reservations.
            var retained = client.PacketScenarioOwnership;
            Check(retained.Sends >= 1 && retained.Bytes >= PayloadLength && !target.IsCompleted,
                "suspected loss released the DATAGRAM owner prematurely");
            await relay.ReleaseHeldAsync(timeout.Token);
            ReadOnlyMemory<byte> late = await server.ReceiveDatagramAsync(timeout.Token);
            Check(late.Span.SequenceEqual(expected), "late DATAGRAM payload differs from its original copied bytes");
            Check(await target.WaitAsync(timeout.Token) == QuicDatagramSendResult.AcknowledgedAfterLoss,
                "actual final callback was not AcknowledgedAfterLoss");
            Check((await Task.WhenAll(sends).WaitAsync(timeout.Token)).All(IsAcknowledged), "later DATAGRAM was not acknowledged");
            // An additional actual message must be next, not a duplicate target.
            await Exchange(client, server, Payload(37, 203), timeout.Token);
            var after = client.GetStatistics();
            Check(after.SpuriousLostPackets > before.SpuriousLostPackets, "core did not count an ACK of a suspected-lost packet");
            Check(relay.HeldCount == 1 && relay.ReleasedCount == 1 && relay.BufferedAcknowledgments == 0,
                "relay did not release exactly one original held packet and drain its bounded queue");
            Check(client.PacketScenarioOwnership == (0, 0L) && server.PacketScenarioOwnership == (0, 0L),
                "final DATAGRAM send/receive ownership did not drain");
            relay.ThrowIfFailed();
            evidence = $"EVIDENCE datagram-late-ack cipher=0x{cipher:x} family={address.AddressFamily} " +
                $"held_udp_bytes={relay.HeldLength} held_sha256={relay.HeldSha256} held=1 released=1 " +
                $"later_payloads={seen.Count} suspected_loss_delta={after.SuspectedLostPackets - before.SuspectedLostPackets} " +
                $"spurious_loss_delta={after.SpuriousLostPackets - before.SpuriousLostPackets} final=AcknowledgedAfterLoss payload_once=true";
        }
        await relay.DisposeAsync();
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(QuicObject.LiveContextCount == contexts && runtime.Host.OutstandingResources == 0 &&
            runtime.Host.OutstandingPlatformAllocations == 0 && runtime.Host.OutstandingDatagramReceives == 0,
            "late-ACK control leaked context or host resources");
        Console.WriteLine(evidence + " ownership_drained=true");
        Console.WriteLine($"PASS facade DATAGRAM late ACK cipher=0x{cipher:x} family={address.AddressFamily}: actual AcknowledgedAfterLoss, exact payload and ownership drain");
    }

    private static bool IsAcknowledged(QuicDatagramSendResult value) =>
        value is QuicDatagramSendResult.Acknowledged or QuicDatagramSendResult.AcknowledgedAfterLoss;

    private static async Task Exchange(QuicConnection sender, QuicConnection receiver, byte[] expected, CancellationToken cancellation)
    {
        Task<QuicDatagramSendResult> send = sender.SendDatagramAsync(expected).AsTask();
        ReadOnlyMemory<byte> received = await receiver.ReceiveDatagramAsync(cancellation);
        Check(received.Span.SequenceEqual(expected), "DATAGRAM exchange payload mismatch or duplicate");
        Check(IsAcknowledged(await send.WaitAsync(cancellation)), "DATAGRAM exchange was not actually acknowledged");
    }

    private static async Task Until(Func<bool> condition, HoldRelay relay, CancellationToken cancellation)
    {
        while (!condition())
        {
            relay.ThrowIfFailed();
            await Task.Delay(1, cancellation);
        }
        relay.ThrowIfFailed();
    }

    private static byte[] Payload(int length, int identity)
    {
        byte[] bytes = new byte[length]; bytes[0] = (byte)identity;
        for (int i = 1; i < bytes.Length; i++) bytes[i] = (byte)((i * 31 + identity * 17) % 251);
        return bytes;
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private sealed class HoldRelay : IAsyncDisposable
    {
        private readonly Socket socket;
        private readonly IPEndPoint server;
        private readonly CancellationTokenSource stop = new();
        private readonly object gate = new();
        private readonly List<byte[]> acknowledgments = [];
        private readonly TaskCompletionSource held = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Task forwarding;
        private IPEndPoint? client;
        private byte[]? heldBytes;
        private bool armed, gateServer, disposed;
        internal int HeldCount { get; private set; }
        internal int ReleasedCount { get; private set; }
        internal int HeldLength { get; private set; }
        internal string? HeldSha256 { get; private set; }
        internal Task Held => held.Task;
        internal IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint!;
        internal int BufferedAcknowledgments { get { lock (gate) return acknowledgments.Count; } }

        internal HoldRelay(IPEndPoint server)
        {
            this.server = server;
            socket = new(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(server.Address, 0));
            forwarding = ForwardAsync();
        }
        internal void Arm()
        {
            lock (gate)
            {
                Check(client != null && !armed && HeldCount == 0 && !gateServer, "relay arm state is invalid");
                armed = gateServer = true;
            }
        }
        internal void ThrowIfFailed()
        { if (forwarding.IsFaulted) forwarding.GetAwaiter().GetResult(); }

        private async Task ForwardAsync()
        {
            byte[] buffer = new byte[65536];
            EndPoint any = new IPEndPoint(server.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
            try
            {
                while (true)
                {
                    var received = await socket.ReceiveFromAsync(buffer, SocketFlags.None, any, stop.Token);
                    Check(received.ReceivedBytes > 0 && received.ReceivedBytes <= 4096, "unexpected UDP packet size in fixed-MTU control");
                    var source = (IPEndPoint)received.RemoteEndPoint;
                    bool fromServer = source.Equals(server);
                    IPEndPoint? destination;
                    bool captured = false;
                    lock (gate)
                    {
                        if (!fromServer)
                        {
                            Check(client == null || client.Equals(source), "relay received an unrelated client");
                            client = source;
                        }
                        destination = fromServer ? client : server;
                        if (fromServer && gateServer)
                        {
                            Check(acknowledgments.Count < 64, "reverse UDP hold queue exceeded 64 packets");
                            acknowledgments.Add(buffer.AsSpan(0, received.ReceivedBytes).ToArray());
                            continue;
                        }
                        if (!fromServer && armed && received.ReceivedBytes > 1000 && (buffer[0] & 0x80) == 0)
                        {
                            heldBytes = buffer.AsSpan(0, received.ReceivedBytes).ToArray();
                            HeldLength = heldBytes.Length;
                            HeldSha256 = Convert.ToHexString(SHA256.HashData(heldBytes)).ToLowerInvariant();
                            HeldCount++; armed = false; captured = true;
                        }
                    }
                    if (captured) { held.TrySetResult(); continue; }
                    if (destination != null)
                        await Send(buffer.AsMemory(0, received.ReceivedBytes), destination, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
            catch (Exception error) { held.TrySetException(error); throw; }
        }

        internal async Task ReleaseAcknowledgmentsAsync(CancellationToken cancellation)
        {
            // Keep gating while flushing. Reverse each snapshot so later actual
            // ACK ranges can reach the sender first; ciphertext stays unchanged.
            while (true)
            {
                byte[][] batch; IPEndPoint destination;
                lock (gate)
                {
                    Check(gateServer && HeldCount == 1, "reverse gate released in invalid state");
                    if (acknowledgments.Count == 0) { gateServer = false; return; }
                    batch = acknowledgments.ToArray(); acknowledgments.Clear();
                    destination = client!;
                }
                for (int i = batch.Length - 1; i >= 0; i--)
                    await Send(batch[i], destination, cancellation);
                ThrowIfFailed();
            }
        }
        internal async Task ReleaseHeldAsync(CancellationToken cancellation)
        {
            byte[] bytes;
            lock (gate)
            {
                Check(heldBytes != null && ReleasedCount == 0 && !gateServer, "held UDP release state is invalid");
                bytes = heldBytes!; heldBytes = null;
                Check(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() == HeldSha256,
                    "held UDP ciphertext was modified");
            }
            await Send(bytes, server, cancellation);
            ReleasedCount++;
        }
        private async Task Send(ReadOnlyMemory<byte> bytes, IPEndPoint destination, CancellationToken cancellation)
        {
            int sent = await socket.SendToAsync(bytes, SocketFlags.None, destination, cancellation);
            Check(sent == bytes.Length, "relay UDP send was truncated");
        }
        public async ValueTask DisposeAsync()
        {
            lock (gate)
            {
                if (disposed) return;
                disposed = true;
            }
            stop.Cancel(); socket.Dispose();
            try { await forwarding.WaitAsync(Limit); }
            finally
            {
                lock (gate) { acknowledgments.Clear(); heldBytes = null; }
                stop.Dispose();
            }
        }
    }
}
