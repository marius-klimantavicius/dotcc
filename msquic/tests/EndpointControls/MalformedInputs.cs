using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Finite source-derived endpoint controls, not a general fuzzer or a claim that
// upstream packet tests were ported. Pin:80a065112426bce68c1da42d026478d3e40fd45e.
// packet.c117–140 validates invariant length/DCID/SCID extents before connection
// lookup; binding.c1185 calls it on actual UDP input. connection.c4344–4351
// records real AEAD decryption failure and drop before returning without frames.
// Attribution hashes below identify the unchanged files these inputs target.
internal static class MalformedInputs
{
    private const string PacketSourceSha256 = "0b80b81fff3f63265817293346073ca6dc288d1fdfc5b6643c0b09ca44adf682";
    private const string ConnectionSourceSha256 = "7bf1eb16839824c35f3e5d03a3165bee5e03aa2e6b1e9ef036dc59ddfd24e036";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);
    private sealed record HeaderVector(string Name, byte[] Bytes, int SourceLine);
    private static readonly HeaderVector[] HeaderVectors =
    [
        new("truncated-long-one", [0xc0], 117),
        new("truncated-long-six", [0xc0, 0, 0, 0, 1, 0], 117),
        new("truncated-destination-cid", [0xc0, 0, 0, 0, 1, 20, 0], 128),
        new("truncated-source-cid", [0xc0, 0, 0, 0, 1, 0, 20], 137)
    ];

    internal static async Task RunAsync()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign, true));
        var names = new SubjectAlternativeNameBuilder(); names.AddDnsName("localhost");
        request.CertificateExtensions.Add(names.Build());
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(1));
        foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
        foreach (IPAddress address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        foreach (bool serverReceives in new[] { true, false })
            await RunCase(certificate, cipher, address, serverReceives);
    }

    private static async Task RunCase(X509Certificate2 certificate, ushort cipher, IPAddress address, bool serverReceives)
    {
        string receiverName = serverReceives ? "server" : "client";
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        var baseline = runtime.GetPerformanceCounters();
        Require(baseline[QuicPerformanceCounter.ActiveConnections] == 0 && baseline[QuicPerformanceCounter.ActiveStreams] == 0,
            "fresh runtime already owns active transport objects");
        await using var registration = await runtime.OpenRegistrationAsync("malformed-endpoint");
        using var serverCredential = QuicCredentials.Server(certificate, cipherSuite: cipher);
        using var clientCredential = QuicCredentials.Client([certificate], X509RevocationMode.NoCheck, cipher);
        var settings = new QuicSettings
        {
            PeerUnidiStreamCount = 8, SendBufferingEnabled = false,
            IdleTimeoutMs = 20000, HandshakeIdleTimeoutMs = 10000,
            MinimumMtu = 1280, MaximumMtu = 1280
        };
        await using var serverConfiguration = await registration.CreateConfigurationAsync(["malformed-endpoint"u8.ToArray()], serverCredential, settings);
        await using var clientConfiguration = await registration.CreateConfigurationAsync(["malformed-endpoint"u8.ToArray()], clientCredential, settings);
        await using var listener = await registration.ListenAsync(serverConfiguration, new(address, 0));
        await using var relay = new Relay(listener.LocalEndPoint);
        using var timeout = new CancellationTokenSource(Limit);
        var accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        await using var client = await registration.ConnectAsync(clientConfiguration, "localhost", relay.LocalEndPoint, timeout.Token);
        await using var server = await accepting.WaitAsync(timeout.Token);
        Require((ushort)client.GetHandshakeInformation().CipherSuite == cipher &&
            (ushort)server.GetHandshakeInformation().CipherSuite == cipher, "actual negotiated cipher differs");
        // Complete real 1-RTT data in both directions before fault injection.
        await Transfer(client, server, 4097, 3, timeout.Token);
        await Transfer(server, client, 4099, 5, timeout.Token);
        QuicConnection receiver = serverReceives ? server : client;
        QuicConnection sender = serverReceives ? client : server;
        var originalData = receiver.GetStatistics();
        foreach (HeaderVector vector in HeaderVectors)
        {
            long before = runtime.GetPerformanceCounters()[QuicPerformanceCounter.DroppedPackets];
            await relay.Inject(vector.Bytes, serverReceives, timeout.Token);
            await Until(() => runtime.GetPerformanceCounters()[QuicPerformanceCounter.DroppedPackets] > before,
                relay, timeout.Token, "malformed invariant header was not counted as dropped");
            var current = receiver.GetStatistics();
            Require(client.CloseInfo is null && server.CloseInfo is null &&
                current.ReceivedStreamBytes == originalData.ReceivedStreamBytes,
                "malformed header closed an established peer or delivered stream data");
            long delta = runtime.GetPerformanceCounters()[QuicPerformanceCounter.DroppedPackets] - before;
            Console.WriteLine(FormattableString.Invariant($"EVIDENCE endpoint {{\"kind\":\"malformed-header\",\"cipher\":{cipher},\"family\":\"{address.AddressFamily}\",\"receiver\":\"{receiverName}\",\"vector\":\"{vector.Name}\",\"input_hex\":\"{Convert.ToHexString(vector.Bytes)}\",\"core_drop_delta\":{delta},\"source\":\"src/core/packet.c\",\"source_line\":{vector.SourceLine},\"source_sha256\":\"{PacketSourceSha256}\"}}"));
        }
        await BadTag(runtime, relay, sender, receiver, serverReceives, cipher, receiverName, address, timeout.Token);
        // Fresh exact traffic after the protected-packet failure demonstrates
        // that dropping unauthenticated input did not poison live transport.
        await Transfer(client, server, 65537, 17, timeout.Token);
        await Transfer(server, client, 65539, 29, timeout.Token);
        Require(client.CloseInfo is null && server.CloseInfo is null, "live peer closed after malformed input recovery");
        await client.ShutdownAsync(0x345, timeout.Token).WaitAsync(timeout.Token);
        await Until(() => server.CloseInfo is { PeerInitiated: true, ApplicationInitiated: true, Status: 0, ErrorCode: 0x345 },
            relay, timeout.Token, "normal application shutdown did not reach peer");
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await Until(() =>
        {
            var counters = runtime.GetPerformanceCounters();
            return counters[QuicPerformanceCounter.ActiveConnections] == baseline[QuicPerformanceCounter.ActiveConnections] &&
                counters[QuicPerformanceCounter.ActiveStreams] == baseline[QuicPerformanceCounter.ActiveStreams];
        }, relay, timeout.Token, "public active connection/stream counters did not drain");
        ThrowsDisposed(() => client.GetStatistics()); ThrowsDisposed(() => server.GetStatistics());
        // Public runtime disposal executes the owning implementation's host
        // resource/buffer checks. No private registry or host field is inspected.
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        relay.ThrowIfFailed();
        Console.WriteLine(FormattableString.Invariant($"EVIDENCE endpoint {{\"kind\":\"public-lifetime\",\"cipher\":{cipher},\"family\":\"{address.AddressFamily}\",\"receiver\":\"{receiverName}\",\"active_connections\":0,\"active_streams\":0,\"public_runtime_disposal_complete\":true}}"));
        Console.WriteLine($"PASS endpoint malformed cipher=0x{cipher:x4} family={address.AddressFamily} receiver={receiverName}");
    }

    private static async Task BadTag(QuicRuntime runtime, Relay relay, QuicConnection sender, QuicConnection receiver,
        bool serverReceives, ushort cipher, string receiverName, IPAddress address, CancellationToken token)
    {
        var receiveStream = receiver.AcceptStreamAsync(token).AsTask();
        await using var writer = await sender.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional, token);
        var before = receiver.GetStatistics();
        long globalBefore = runtime.GetPerformanceCounters()[QuicPerformanceCounter.DecryptionFailures];
        var captured = relay.Arm(serverReceives);
        Task send = writer.SendAsync(Bytes(8193, 11), QuicSendOptions.Start | QuicSendOptions.Fin, token).AsTask();
        try
        {
            byte[] original = await captured.WaitAsync(token);
            Require(original.Length >= 64 && (original[0] & 0x80) == 0, "capture was not a sufficiently long short protected packet");
            byte[] corrupted = original.ToArray(); corrupted[^1] ^= 1;
            // Last byte is inside the16-byte AES-GCM tag and outside any possible
            // short-header sample (maxCID20 + PN-offset/sample extent <=41).
            // The original packet has NEVER been forwarded; duplicate packet
            // filtering cannot mask the tag verification being exercised.
            await relay.Inject(corrupted, serverReceives, token);
            await Until(() =>
            {
                var current = receiver.GetStatistics();
                return current.DecryptionFailures > before.DecryptionFailures && current.DroppedPackets > before.DroppedPackets &&
                    runtime.GetPerformanceCounters()[QuicPerformanceCounter.DecryptionFailures] > globalBefore;
            }, relay, token, "tag corruption did not reach actual AEAD failure and drop counters");
            var after = receiver.GetStatistics();
            Require(receiver.CloseInfo is null && sender.CloseInfo is null && !receiveStream.IsCompleted &&
                after.ReceivedStreamBytes == before.ReceivedStreamBytes,
                "corrupted protected packet delivered application bytes or closed the peer");
            Console.WriteLine(FormattableString.Invariant($"EVIDENCE endpoint {{\"kind\":\"bad-tag\",\"cipher\":{cipher},\"family\":\"{address.AddressFamily}\",\"receiver\":\"{receiverName}\",\"bytes\":{original.Length},\"original_sha256\":\"{Hash(original)}\",\"corrupted_sha256\":\"{Hash(corrupted)}\",\"decryption_delta\":{after.DecryptionFailures - before.DecryptionFailures},\"connection_drop_delta\":{after.DroppedPackets - before.DroppedPackets},\"source\":\"src/core/connection.c\",\"source_line\":4344,\"source_sha256\":\"{ConnectionSourceSha256}\"}}"));
        }
        finally { await relay.Release(token); }
        await using var reader = await receiveStream.WaitAsync(token);
        await ReadExact(reader, Bytes(8193, 11), token);
        await send.WaitAsync(token); await writer.CompleteWritesAsync(token).AsTask().WaitAsync(token);
        Require(writer.GetObservedEarlyDataLength() == 0, "unexpected early data in malformed-input control");
    }

    private static async Task Transfer(QuicConnection sender, QuicConnection receiver, int length, int seed, CancellationToken token)
    {
        var accepting = receiver.AcceptStreamAsync(token).AsTask();
        await using var writer = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional, cancellationToken: token);
        byte[] expected = Bytes(length, seed);
        Task send = writer.SendAsync(expected, QuicSendOptions.Fin, token).AsTask();
        await using var reader = await accepting.WaitAsync(token);
        await ReadExact(reader, expected, token); await send.WaitAsync(token);
        await writer.CompleteWritesAsync(token).AsTask().WaitAsync(token);
        Require(writer.GetObservedEarlyDataLength() == 0, "unexpected early data");
    }
    private static async Task ReadExact(QuicStream reader, byte[] expected, CancellationToken token)
    {
        byte[] buffer = new byte[4093]; int offset = 0;
        while (true)
        {
            int count = await reader.ReadAsync(buffer, token).AsTask().WaitAsync(token);
            if (count == 0) break;
            Require(count <= expected.Length - offset && buffer.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)), "stream byte mismatch");
            offset += count;
        }
        Require(offset == expected.Length, "FIN preceded exact payload");
    }
    private static byte[] Bytes(int length, int seed)
    { var bytes = new byte[length]; for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)(i * 31 + seed)); return bytes; }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static void Require(bool value, string reason)
    { if (!value) throw new InvalidOperationException("Malformed endpoint: " + reason); }
    private static async Task Until(Func<bool> predicate, Relay relay, CancellationToken token, string reason)
    {
        long started = Stopwatch.GetTimestamp();
        while (!predicate())
        {
            relay.ThrowIfFailed(); token.ThrowIfCancellationRequested();
            if (Stopwatch.GetElapsedTime(started) > Limit) throw new TimeoutException(reason);
            await Task.Delay(2, token);
        }
    }
    private static void ThrowsDisposed(Action action)
    { try { action(); } catch (ObjectDisposedException) { return; } throw new InvalidOperationException("Closed public owner accepted a query"); }

    private sealed class Relay : IAsyncDisposable
    {
        private sealed class Held(bool serverReceives)
        {
            internal readonly bool ServerReceives = serverReceives;
            internal readonly TaskCompletionSource<byte[]> First = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly List<byte[]> Packets = [];
            internal int Bytes;
        }
        private readonly Socket socket;
        private readonly IPEndPoint server;
        private readonly CancellationTokenSource stop = new();
        private readonly Task forwarding;
        private readonly object gate = new();
        private IPEndPoint? client;
        private Held? held;
        private bool disposed;
        internal IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint!;
        internal Relay(IPEndPoint server)
        {
            this.server = server;
            socket = new Socket(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(server.Address, 0));
            forwarding = Task.Run(Forward);
        }
        internal Task<byte[]> Arm(bool serverReceives)
        {
            lock (gate)
            { Require(held == null, "relay capture already armed"); held = new Held(serverReceives); return held.First.Task; }
        }
        internal async Task Inject(byte[] payload, bool serverReceives, CancellationToken token)
        {
            IPEndPoint destination;
            lock (gate) destination = serverReceives ? server : client ?? throw new InvalidOperationException("Client endpoint is not established");
            int written = await socket.SendToAsync(payload.AsMemory(), SocketFlags.None, destination, token);
            Require(written == payload.Length, "UDP injection was not submitted completely");
        }
        internal async Task Release(CancellationToken token)
        {
            Held? capture;
            lock (gate) { capture = held; held = null; }
            if (capture == null) return;
            foreach (byte[] packet in capture.Packets) await Inject(packet, capture.ServerReceives, token);
        }
        internal void ThrowIfFailed()
        { if (forwarding.IsFaulted) forwarding.GetAwaiter().GetResult(); }
        private async Task Forward()
        {
            byte[] buffer = new byte[65535];
            EndPoint any = new IPEndPoint(server.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    var result = await socket.ReceiveFromAsync(buffer.AsMemory(), SocketFlags.None, any, stop.Token);
                    var remote = (IPEndPoint)result.RemoteEndPoint; bool fromServer = remote.Equals(server); bool retain = false;
                    IPEndPoint destination;
                    lock (gate)
                    {
                        if (!fromServer)
                        {
                            client ??= remote; Require(remote.Equals(client), "relay received an unexpected client endpoint");
                        }
                        destination = fromServer ? client ?? throw new InvalidOperationException("Server packet preceded client") : server;
                        if (held is { } capture && capture.ServerReceives == !fromServer)
                        {
                            Require(capture.Packets.Count < 128 && capture.Bytes + result.ReceivedBytes <= 256 * 1024, "relay held packet budget exhausted");
                            byte[] packet = buffer.AsSpan(0, result.ReceivedBytes).ToArray();
                            capture.Packets.Add(packet); capture.Bytes += packet.Length; retain = true;
                            if (packet.Length >= 64 && (packet[0] & 0x80) == 0) capture.First.TrySetResult(packet);
                        }
                    }
                    if (!retain)
                    {
                        int written = await socket.SendToAsync(buffer.AsMemory(0, result.ReceivedBytes), SocketFlags.None, destination, stop.Token);
                        Require(written == result.ReceivedBytes, "relay UDP send length mismatch");
                    }
                }
            }
            catch (Exception error) when (stop.IsCancellationRequested && error is (OperationCanceledException or ObjectDisposedException or SocketException)) { }
            catch (Exception error)
            { lock (gate) held?.First.TrySetException(error); throw; }
        }
        public async ValueTask DisposeAsync()
        {
            if (disposed) return; disposed = true; stop.Cancel(); socket.Dispose();
            try { await forwarding.WaitAsync(Limit); }
            finally { lock (gate) held = null; stop.Dispose(); }
        }
    }
}
