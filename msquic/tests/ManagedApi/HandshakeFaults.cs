using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Real translated endpoints only: Retry comes from the configured admission
// threshold, and reset comes from a listener receiving an unknown former CID.
internal static class HandshakeFaults
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new()
            {
                ProcessorCount = 2, Parameters = new() { RetryMemoryLimit = 0 }
            });
            await using var registration = await runtime.OpenRegistrationAsync("handshake-fault-controls");
            using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: suite);
            using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: suite);
            var settings = new QuicSettings
            {
                PeerBidiStreamCount = 8, PeerUnidiStreamCount = 8,
                IdleTimeoutMs = 20000, DisconnectTimeoutMs = 10000, SendBufferingEnabled = false
            };
            await using var serverConfig = await registration.CreateConfigurationAsync(["handshake-faults"u8.ToArray()], serverCredentials, settings);
            await using var clientConfig = await registration.CreateConfigurationAsync(["handshake-faults"u8.ToArray()], clientCredentials, settings);
            await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
            Check(runtime.GetRetryMemoryLimit() == 0, "forced Retry threshold was not applied");
            long retryBefore = runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessRetriesSent];
            var retried = await Connect(registration, clientConfig, listener);
            await using (retried.Client)
            await using (retried.Server)
            {
                Check(retried.Client.GetStatistics().StatelessRetry, "client did not process an actual Retry");
                // Pinned connection.c:3760 sets the public flag only in the
                // client Retry handler. The server instead records successful
                // token validation in HandshakeUsedRetryPacket at :4016.
                Check(!retried.Server.GetStatistics().StatelessRetry, "server Retry statistics diverged from pinned upstream semantics");
                Check(runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessRetriesSent] > retryBefore,
                    "core did not send a stateless Retry");
                var original = retried.Client.GetOriginalDestinationConnectionId();
                Check(original.Length != 0 && original.Span.SequenceEqual(retried.Server.GetOriginalDestinationConnectionId().Span),
                    "server did not recover the client's original CID from its Retry token");
                await Transfer(retried.Client, retried.Server, 101);
                await Transfer(retried.Server, retried.Client, 202);
            }

            // Keep this listener/binding and its reset secret alive while the
            // connection alone is silently removed. A fresh handshake below
            // proves the listener remains operational after the reset.
            runtime.SetRetryMemoryLimit(ushort.MaxValue);
            var reset = await Connect(registration, clientConfig, listener);
            await using (reset.Client)
            await using (reset.Server)
            {
                Check(!reset.Client.GetStatistics().StatelessRetry, "ordinary handshake unexpectedly retried");
                await Transfer(reset.Client, reset.Server, 303);
                await Transfer(reset.Server, reset.Client, 404);
                await using var writer = await reset.Client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
                long resetsBefore = runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessResetsSent];
                Check(reset.Client.CloseInfo is null, "client closed before silent shutdown");
                await reset.Server.ShutdownAsync(options: QuicConnectionShutdownOptions.Silent).WaitAsync(Limit);
                await reset.Server.DisposeAsync().AsTask().WaitAsync(Limit);
                // An already queued packet can reach the surviving listener
                // after the connection is removed and trigger the reset before
                // DisposeAsync returns. Both timings require the exact reset
                // status and counter below; a peer CONNECTION_CLOSE must fail.
                // Otherwise a real 1 KiB STREAM packet exceeds the minimum
                // reset-trigger size at binding.c:1143 (a bare keepalive may not).
                Task? send = reset.Client.CloseInfo is null
                    ? writer.SendAsync(Payload(1024, 505), QuicSendOptions.Fin).AsTask()
                    : null;
                await reset.Client.ShutdownCompletion.WaitAsync(Limit);
                var close = reset.Client.CloseInfo ?? throw new InvalidOperationException("Reset did not publish transport close information");
                // connection.c:4324 closes with QUIC_STATUS_ABORTED (LP64
                // ECANCELED=125); :1632 maps the status to transport error1.
                Check(close is { Status: 125, ErrorCode: 1, PeerInitiated: false, ApplicationInitiated: false },
                    $"reset close mismatch: {close}");
                Check(runtime.GetPerformanceCounters()[QuicPerformanceCounter.StatelessResetsSent] > resetsBefore,
                    "client closed without an actual core-generated stateless reset");
                try { if (send != null) await send.WaitAsync(Limit); }
                catch (OperationCanceledException) { }
                catch (QuicTransportException error) when (error.Status == 125) { }
            }

            var healthy = await Connect(registration, clientConfig, listener);
            await using (healthy.Client)
            await using (healthy.Server) await Transfer(healthy.Client, healthy.Server, 606);
            await registration.DisposeAsync().AsTask().WaitAsync(Limit);
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0 &&
                runtime.Host.OutstandingDatagramReceives == 0, "Retry/reset host ownership did not drain");
            Console.WriteLine($"PASS facade handshake fault controls cipher=0x{suite:x} family={address.AddressFamily}");
        }
    }

    private static async Task<(QuicConnection Client, QuicConnection Server)> Connect(
        QuicRegistration registration, QuicConfiguration configuration, QuicListener listener)
    {
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accept = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        QuicConnection? client = null;
        try
        {
            client = await registration.ConnectAsync(configuration, "localhost", listener.LocalEndPoint, timeout.Token);
            return (client, await accept);
        }
        catch { if (client != null) await client.DisposeAsync(); throw; }
    }

    private static async Task Transfer(QuicConnection sender, QuicConnection receiver, int seed)
    {
        await using var writer = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        byte[] expected = Payload(4096, seed);
        Task send = writer.SendAsync(expected, QuicSendOptions.Fin).AsTask();
        await using var reader = await receiver.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        byte[] buffer = new byte[1021]; int offset = 0;
        while (true)
        {
            int length = await reader.ReadAsync(buffer).AsTask().WaitAsync(Limit);
            if (length == 0) break;
            Check(length <= expected.Length - offset && buffer.AsSpan(0, length).SequenceEqual(expected.AsSpan(offset, length)),
                "handshake fault control changed stream payload");
            offset += length;
        }
        Check(offset == expected.Length, "handshake fault control lost stream bytes before FIN");
        await send.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        Check(writer.GetObservedEarlyDataLength() == 0 && reader.GetObservedEarlyDataLength() == 0,
            "selected Retry/reset profile unexpectedly sent early data");
    }

    private static byte[] Payload(int length, int seed)
    {
        byte[] result = new byte[length];
        for (int i = 0; i < length; ++i) result[i] = unchecked((byte)(i * 71 + seed * 37 + (i >> 8)));
        return result;
    }
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}
