using static Managed.Transport.MsQuic;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport;
using Managed.Transport.Api;

// Observe real packets from the translated core. Independent BCL calculations
// check secret binding; no callback, protocol packet, or token is synthesized.
internal static class StatelessSecrets
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);
    private const uint RotationMs = 2000;

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
        foreach (IPAddress address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
            await RunCase(certificates, suite, address);
    }

    private static async Task RunCase(Credentials certificates, ushort suite, IPAddress address)
    {
        int contexts = QuicObject.LiveContextCount;
        await using var runtime = await QuicRuntime.CreateAsync(new()
        { ProcessorCount = 2, Parameters = new() { RetryMemoryLimit = 0 } });
        await using var registration = await runtime.OpenRegistrationAsync("stateless-secret-controls");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: suite);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: suite);
        var settings = new QuicSettings
        {
            PeerUnidiStreamCount = 8, IdleTimeoutMs = 20000,
            DisconnectTimeoutMs = 10000, SendBufferingEnabled = false
        };
        await using var serverConfig = await registration.CreateConfigurationAsync(["stateless-secrets"u8.ToArray()], serverCredentials, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["stateless-secrets"u8.ToArray()], clientCredentials, settings);
        await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
        int length = suite == 0x1301 ? 16 : 32;
        var cipher = suite == 0x1301 ? QuicPacketCipher.Aes128Gcm : QuicPacketCipher.Aes256Gcm;
        byte[] priorRetry = Material(length, 19), priorReset = Material(32, 23);
        long previousIndex = -1;
        for (int round = 0; round < 2; round++)
        {
            byte[] retry = Material(length, 71 + round * 17), reset = Material(32, 83 + round * 19);
            byte[] callerRetry = retry.ToArray(), callerReset = reset.ToArray();
            runtime.ConfigureStatelessRetry(cipher, RotationMs, callerRetry);
            runtime.ProvisionStatelessResetKey(callerReset);
            Array.Fill(callerRetry, (byte)0xff); Array.Fill(callerReset, (byte)0xff);
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            // Existing cached keys intentionally follow upstream's epoch policy.
            // Advance to a fresh epoch before testing a changed base secret.
            if (round != 0)
            {
                long next = Math.Max(previousIndex, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / RotationMs) + 1;
                long remaining = next * RotationMs + 100 - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (remaining > 0) await Task.Delay(checked((int)remaining));
            }
            await using var relay = new PacketRelay(listener.LocalEndPoint);
            using var timeout = new CancellationTokenSource(Limit);
            Task<QuicConnection> accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
            await using var client = await registration.ConnectAsync(clientConfig, "localhost", relay.LocalEndPoint, timeout.Token);
            await using var server = await accepting;
            Check(client.GetStatistics().StatelessRetry, "connection did not consume a real Retry");
            var observed = VerifyRetry(await relay.Retry.WaitAsync(timeout.Token), retry, priorRetry,
                client.GetOriginalDestinationConnectionId().ToArray());
            Check(server.GetOriginalDestinationConnectionId().Span.SequenceEqual(client.GetOriginalDestinationConnectionId().Span),
                "server did not recover original CID from its authenticated Retry token");
            if (round != 0) Check(observed.KeyIndex > previousIndex, "Retry key epoch did not rotate");
            previousIndex = observed.KeyIndex;
            // Arm before the final live traffic. An in-flight ACK/STREAM packet
            // can elicit a valid reset as soon as the server handle is removed.
            relay.ObserveReset(observed.CidLength, reset, priorReset);
            await Transfer(client, server, 31 + round, timeout.Token);
            await Transfer(server, client, 47 + round, timeout.Token);
            await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(timeout.Token);
            Check(client.CloseInfo is null, "client closed before silent server shutdown");
            long before = runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessResetsSent];
            await server.ShutdownAsync(options: QuicConnectionShutdownOptions.Silent).WaitAsync(timeout.Token);
            await server.DisposeAsync().AsTask().WaitAsync(timeout.Token);
            Task? send = client.CloseInfo is null
                ? writer.SendAsync(Material(1024, 113 + round), QuicSendOptions.Fin).AsTask() : null;
            await relay.Reset.WaitAsync(timeout.Token);
            await client.ShutdownCompletion.WaitAsync(timeout.Token);
            Check(client.CloseInfo is { Status: 125, ErrorCode: 1, PeerInitiated: false, ApplicationInitiated: false },
                "observed reset did not produce the actual pinned-core reset close");
            Check(runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessResetsSent] > before,
                "reset observation did not come from the core");
            try { if (send != null) await send.WaitAsync(timeout.Token); }
            catch (OperationCanceledException) { }
            catch (QuicTransportException error) when (error.Status == 125) { }
            relay.ThrowIfFailed();
            priorRetry = retry; priorReset = reset;
        }
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(QuicObject.LiveContextCount == contexts && runtime.Host.OutstandingResources == 0 &&
            runtime.Host.OutstandingPlatformAllocations == 0 && runtime.Host.OutstandingDatagramReceives == 0,
            "stateless-secret controls leaked ownership");
        Console.WriteLine($"PASS facade stateless secrets cipher=0x{suite:x} family={address.AddressFamily}: copied keys, real Retry epoch rotation, emitted Retry/reset key binding, independent wrong-key/tag controls, payload and reset drain");
    }

    private static unsafe (long KeyIndex, int CidLength) VerifyRetry(byte[] packet, byte[] secret, byte[] wrongSecret, byte[] originalCid)
    {
        Check(packet.Length > 23 && (packet[0] & 0xf0) == 0xf0 && BinaryPrimitives.ReadUInt32BigEndian(packet.AsSpan(1, 4)) == 1,
            "observed packet is not QUIC v1 Retry");
        int offset = 6 + packet[5];
        Check(offset < packet.Length, "Retry DCID extent is invalid");
        int cidLength = packet[offset++];
        Check(cidLength is > 0 and <= 20 && offset + cidLength < packet.Length - 16, "Retry SCID extent is invalid");
        ReadOnlySpan<byte> cid = packet.AsSpan(offset, cidLength); offset += cidLength;
        ReadOnlySpan<byte> token = packet.AsSpan(offset, packet.Length - offset - 16);
        // binding.h owns this private native layout, already translated with the
        // product. Use its field extents, not a hand-written replacement struct.
        QUIC_TOKEN_CONTENTS layout = default;
        int encryptedOffset = checked((int)((byte*)&layout.Encrypted - (byte*)&layout));
        int tagOffset = checked((int)((byte*)layout.EncryptionTag - (byte*)&layout));
        int originalOffset = checked((int)((byte*)layout.Encrypted.OrigConnId - (byte*)&layout.Encrypted));
        int originalLengthOffset = checked((int)((byte*)&layout.Encrypted.OrigConnIdLength - (byte*)&layout.Encrypted));
        Check(token.Length == sizeof(QUIC_TOKEN_CONTENTS) && encryptedOffset == 8 && tagOffset > encryptedOffset,
            "Retry token has an unexpected private layout");
        ulong authenticated = BinaryPrimitives.ReadUInt64LittleEndian(token);
        Check((authenticated & 1) == 0, "Retry carried a NEW_TOKEN flag");
        long keyIndex = checked((long)(authenticated >> 1) / RotationMs);
        Span<byte> context = stackalloc byte[8]; BinaryPrimitives.WriteInt64LittleEndian(context, keyIndex);
        Span<byte> nonce = stackalloc byte[12]; nonce.Clear();
        for (int i = 0; i < cid.Length; i++) nonce[i % nonce.Length] ^= cid[i];
        byte[] derived = DeriveKey(secret, context), wrong = DeriveKey(wrongSecret, context);
        byte[] plaintext = new byte[tagOffset - encryptedOffset];
        try
        {
            using var aead = new AesGcm(derived, 16);
            aead.Decrypt(nonce, token.Slice(encryptedOffset, plaintext.Length), token.Slice(tagOffset, 16), plaintext, token[..encryptedOffset]);
            Check(plaintext[originalLengthOffset] == originalCid.Length &&
                plaintext.AsSpan(originalOffset, originalCid.Length).SequenceEqual(originalCid), "Retry key decrypted the wrong original CID");
            using var wrongAead = new AesGcm(wrong, 16);
            bool rejected = false;
            try { wrongAead.Decrypt(nonce, token.Slice(encryptedOffset, plaintext.Length), token.Slice(tagOffset, 16), plaintext, token[..encryptedOffset]); }
            catch (AuthenticationTagMismatchException) { rejected = true; }
            Check(rejected, "Retry token authenticated under the wrong/retired secret");
            byte[] badTag = token.Slice(tagOffset, 16).ToArray(); badTag[0] ^= 1; rejected = false;
            try { aead.Decrypt(nonce, token.Slice(encryptedOffset, plaintext.Length), badTag, plaintext, token[..encryptedOffset]); }
            catch (AuthenticationTagMismatchException) { rejected = true; }
            Check(rejected, "mutated Retry tag was accepted");
        }
        finally { CryptographicOperations.ZeroMemory(derived); CryptographicOperations.ZeroMemory(wrong); CryptographicOperations.ZeroMemory(plaintext); }
        return (keyIndex, cidLength);
    }

    private static byte[] DeriveKey(byte[] secret, ReadOnlySpan<byte> context)
    {
        // SP800-108 counter mode, independently assembled rather than calling
        // the host's KDF callback: [i]32 || label || 0 || context || [L]32.
        ReadOnlySpan<byte> label = "QUIC Stateless Retry Key"u8;
        byte[] input = new byte[4 + label.Length + 1 + context.Length + 4];
        BinaryPrimitives.WriteUInt32BigEndian(input, 1); label.CopyTo(input.AsSpan(4));
        context.CopyTo(input.AsSpan(4 + label.Length + 1));
        BinaryPrimitives.WriteUInt32BigEndian(input.AsSpan(input.Length - 4), checked((uint)secret.Length * 8));
        byte[] digest = HMACSHA256.HashData(secret, input);
        byte[] result = digest.AsSpan(0, secret.Length).ToArray(); CryptographicOperations.ZeroMemory(digest);
        return result;
    }

    private static async Task Transfer(QuicConnection sender, QuicConnection receiver, int seed, CancellationToken cancellation)
    {
        await using var writer = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(cancellation);
        byte[] payload = Material(4096, seed);
        Task send = writer.SendAsync(payload, QuicSendOptions.Fin).AsTask();
        await using var reader = await receiver.AcceptStreamAsync(cancellation);
        byte[] buffer = new byte[1021]; int offset = 0;
        while (true)
        {
            int count = await reader.ReadAsync(buffer, cancellation);
            if (count == 0) break;
            Check(count <= payload.Length - offset && buffer.AsSpan(0, count).SequenceEqual(payload.AsSpan(offset, count)), "secret control payload mismatch");
            offset += count;
        }
        Check(offset == payload.Length, "secret control FIN preceded complete payload");
        await send.WaitAsync(cancellation);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(cancellation);
    }

    private static byte[] Material(int length, int seed)
    {
        byte[] result = new byte[length];
        for (int i = 0; i < length; i++) result[i] = unchecked((byte)(seed + i * 37));
        return result;
    }

    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private sealed class PacketRelay : IAsyncDisposable
    {
        private readonly Socket socket;
        private readonly IPEndPoint server;
        private readonly CancellationTokenSource stop = new();
        private readonly Task forwarding;
        private readonly object gate = new();
        private readonly TaskCompletionSource<byte[]> retry = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource reset = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private IPEndPoint? client;
        private byte[]? resetKey, wrongResetKey, destinationCid;
        private int cidLength;
        private bool disposed;
        internal Task<byte[]> Retry => retry.Task;
        internal Task Reset => reset.Task;
        internal IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint!;

        internal PacketRelay(IPEndPoint server)
        {
            this.server = server;
            socket = new(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(server.Address, 0));
            forwarding = ForwardAsync();
        }

        internal void ObserveReset(int length, byte[] secret, byte[] wrong)
        { lock (gate) { cidLength = length; resetKey = secret.ToArray(); wrongResetKey = wrong.ToArray(); } }

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
                    Check(received.ReceivedBytes > 0, "empty relay datagram");
                    var remote = (IPEndPoint)received.RemoteEndPoint;
                    bool fromServer = remote.Equals(server);
                    if (!fromServer)
                    {
                        client ??= remote;
                        Check(remote.Equals(client), "unexpected relay client endpoint");
                    }
                    else Check(client != null, "server packet preceded client endpoint");
                    ReadOnlySpan<byte> packet = buffer.AsSpan(0, received.ReceivedBytes);
                    if (fromServer && (packet[0] & 0xf0) == 0xf0) retry.TrySetResult(packet.ToArray());
                    lock (gate)
                    {
                        if (resetKey != null && (packet[0] & 0x80) == 0)
                        {
                            if (!fromServer && packet.Length >= 1 + cidLength + 16)
                                destinationCid = packet.Slice(1, cidLength).ToArray();
                            if (fromServer && destinationCid != null && packet.Length is >= 39 and < 100)
                            {
                                byte[] expected = HMACSHA256.HashData(resetKey, destinationCid);
                                if (packet[^16..].SequenceEqual(expected.AsSpan(0, 16)))
                                {
                                    byte[] wrong = HMACSHA256.HashData(wrongResetKey!, destinationCid);
                                    Check(!packet[^16..].SequenceEqual(wrong.AsSpan(0, 16)), "reset bound to the wrong/retired key");
                                    reset.TrySetResult();
                                }
                            }
                        }
                    }
                    await socket.SendToAsync(buffer.AsMemory(0, received.ReceivedBytes), SocketFlags.None,
                        fromServer ? client! : server, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
            catch (Exception error) { retry.TrySetException(error); reset.TrySetException(error); throw; }
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true; stop.Cancel(); socket.Dispose();
            try { await forwarding.WaitAsync(Limit); }
            finally
            {
                if (resetKey != null) CryptographicOperations.ZeroMemory(resetKey);
                if (wrongResetKey != null) CryptographicOperations.ZeroMemory(wrongResetKey);
                stop.Dispose();
            }
        }
    }
}
