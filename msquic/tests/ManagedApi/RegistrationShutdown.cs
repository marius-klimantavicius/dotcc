using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Actual owning facade, translated callbacks and loopback UDP. Internal access
// below only observes final callback/resource accounting; no callback is injected.
internal static class RegistrationShutdown
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(25);
    private const ulong ApplicationError = 0x1234_5678_9abc;
    private const int ConnectionCount = 3;
    private sealed record Pair(QuicConnection Client, QuicConnection Server);
    private sealed record Transfer(QuicStream Writer, QuicStream Reader, QuicReceiveLease Offer,
        ReadOnlyMemory<byte> Saved, byte[] Expected, Task Send, Task<QuicReceiveLease?> Receive);

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        foreach (bool serverInitiates in new[] { false, true })
        foreach (bool disposeParent in new[] { false, true })
            await RunCase(certificates, address, serverInitiates, disposeParent);
    }

    private static async Task RunCase(Credentials certificates, IPAddress address, bool serverInitiates, bool disposeParent)
    {
        int contexts = QuicObject.LiveContextCount;
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var clients = await runtime.OpenRegistrationAsync("registration-shutdown-clients");
        await using var servers = await runtime.OpenRegistrationAsync("registration-shutdown-servers");
        using var serverCredential = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
        using var clientCredential = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: 0x1301);
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 8, SendBufferingEnabled = false,
            StreamRecvWindowDefault = 16384, StreamRecvBufferDefault = 16384, ConnFlowControlWindow = 65536,
            HandshakeIdleTimeoutMs = 15000, IdleTimeoutMs = 30000
        };
        await using var serverConfig = await servers.CreateConfigurationAsync(["registration-shutdown"u8.ToArray()], serverCredential, settings);
        await using var clientConfig = await clients.CreateConfigurationAsync(["registration-shutdown"u8.ToArray()], clientCredential, settings);
        await using var listener = await servers.ListenAsync(serverConfig, new(address, 0));
        var pairs = new List<Pair>();
        var transfers = new List<Transfer>();
        var owners = new List<QuicObject> { clients, servers, serverConfig, clientConfig, listener };
        var marker = new object();
        using var timeout = new CancellationTokenSource(Limit);
        QuicRegistration initiating = serverInitiates ? servers : clients;
        try
        {
            for (int i = 0; i < ConnectionCount; i++)
            {
                Task<QuicConnection> accepted = listener.AcceptConnectionAsync(timeout.Token).AsTask();
                var client = await clients.ConnectAsync(clientConfig, "localhost", listener.LocalEndPoint, timeout.Token);
                owners.Add(client);
                var server = await accepted;
                owners.Add(server); pairs.Add(new(client, server));
                // Opposite-direction blocked writes ensure both registrations
                // have an actual application-held native receive offer.
                transfers.Add(await HoldTransfer(client, server, i * 2, owners, timeout.Token));
                transfers.Add(await HoldTransfer(server, client, i * 2 + 1, owners, timeout.Token));
            }
            foreach (var owner in owners) owner.ApplicationContext = marker;
            Check(listener.GetStatistics().AcceptedConnections == ConnectionCount, "not all real connections were accepted");
            Check(transfers.All(value => !value.Send.IsCompleted && !value.Receive.IsCompleted),
                "shutdown did not begin with pending native sends and read waits");

            // Invalid application errors must not shut down any active child.
            await Throws<ArgumentOutOfRangeException>(initiating.ShutdownAsync(1UL << 62));
            Check(pairs.All(value => value.Client.CloseInfo == null && value.Server.CloseInfo == null),
                "invalid registration shutdown error reached a connection");

            // Multiple admitted calls use the same code; first-writer ordering
            // cannot alter the expected wire application error.
            Task first = initiating.ShutdownAsync(ApplicationError);
            Task second = initiating.ShutdownAsync(ApplicationError);
            await Until(() => pairs.All(pair => Remote(pair, serverInitiates).CloseInfo != null), timeout.Token);
            foreach (var pair in pairs) CheckPeerError(Remote(pair, serverInitiates));

            if (disposeParent)
            {
                // Observe the actual peer close before racing disposal's
                // default-zero shutdown. This preserves an exact error oracle
                // while cleanup still competes with outstanding receive owners.
                Task admittedShutdown = initiating.ShutdownAsync(ApplicationError);
                Task parentClose = initiating.DisposeAsync().AsTask();
                Check(ReferenceEquals(parentClose, initiating.DisposeAsync().AsTask()), "parent disposal is not one shared operation");
                var closes = owners.OfType<QuicStream>().Select(value => value.DisposeAsync().AsTask()).ToList();
                closes.AddRange(pairs.Select(pair => Local(pair, serverInitiates).DisposeAsync().AsTask()));
                Check(!parentClose.IsCompleted, "parent close completed while children retained application receive offers");
                foreach (var transfer in transfers) transfer.Offer.Dispose();
                await Task.WhenAll(closes.Append(parentClose).Append(first).Append(second).Append(admittedShutdown)).WaitAsync(timeout.Token);
                AssertDisposed(initiating);
                AssertDisposed(serverInitiates ? serverConfig : clientConfig);
                if (serverInitiates) AssertDisposed(listener);
                foreach (var pair in pairs) AssertDisposed(Local(pair, serverInitiates));
                foreach (var transfer in transfers) { AssertDisposed(transfer.Writer); AssertDisposed(transfer.Reader); }
                await Throws<ObjectDisposedException>(initiating.ShutdownAsync(ApplicationError));
            }
            else
            {
                foreach (var transfer in transfers) transfer.Offer.Dispose();
                await Task.WhenAll(first, second).WaitAsync(timeout.Token);
                // Shutdown finishes transport work but does not dispose any
                // child/configuration/listener or its managed association.
                foreach (var owner in owners)
                    Check(ReferenceEquals(owner.ApplicationContext, marker), "ShutdownAsync disposed a retained owner");
                Check(clientConfig.GetSettings().IdleTimeoutMs == settings.IdleTimeoutMs &&
                    serverConfig.GetSettings().IdleTimeoutMs == settings.IdleTimeoutMs,
                    "shutdown disposed configuration handles");
                Check(listener.GetStatistics().AcceptedConnections == ConnectionCount, "shutdown disposed listener handle");
                foreach (var pair in pairs) { _ = pair.Client.GetStatistics(); _ = pair.Server.GetStatistics(); }
                await initiating.ShutdownAsync(ApplicationError).WaitAsync(timeout.Token);
            }
            foreach (var transfer in transfers)
            {
                await AbortedOperation(transfer.Send, disposeParent);
                await AbortedReceive(transfer.Receive, disposeParent);
                Check(transfer.Saved.Span.SequenceEqual(transfer.Expected), "held managed receive copy changed during shutdown");
            }
            foreach (var pair in pairs) CheckPeerError(Remote(pair, serverInitiates));
        }
        finally
        {
            // A failed assertion must not leave the test itself holding the
            // native receive ownership that parent disposal needs to drain.
            foreach (var transfer in transfers) transfer.Offer.Dispose();
            await Task.WhenAll(owners.OfType<QuicStream>().Select(value => value.DisposeAsync().AsTask())).WaitAsync(Limit);
            await Task.WhenAll(owners.OfType<QuicConnection>().Select(value => value.DisposeAsync().AsTask())).WaitAsync(Limit);
        }
        await Task.WhenAll(clients.DisposeAsync().AsTask(), servers.DisposeAsync().AsTask()).WaitAsync(Limit);
        Check(owners.All(value => value.CallbackFailure == null), "a native callback recorded a lifetime or duplicate-completion failure");
        Check(pairs.All(value => value.Client.PacketScenarioOwnership == (0, 0L) && value.Server.PacketScenarioOwnership == (0, 0L)),
            "connection copied-buffer reservations did not drain");
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        foreach (var transfer in transfers)
            Check(transfer.Saved.Span.SequenceEqual(transfer.Expected), "saved public receive memory became invalid after complete close");
        Check(QuicObject.LiveContextCount == contexts && runtime.Host.OutstandingResources == 0 &&
            runtime.Host.OutstandingPlatformAllocations == 0 && runtime.Host.OutstandingDatagramReceives == 0,
            "registration shutdown leaked facade/core/host ownership");
        Console.WriteLine($"PASS facade registration shutdown family={address.AddressFamily} initiator={(serverInitiates ? "server" : "client")} " +
            $"mode={(disposeParent ? "concurrent-parent-disposal" : "keep-children")} peers={pairs.Count} " +
            $"application_error=0x{ApplicationError:x} blocked_sends={transfers.Count} blocked_reads={transfers.Count} ownership_drained=true");
    }

    private static async Task<Transfer> HoldTransfer(QuicConnection sender, QuicConnection receiver, int seed,
        List<QuicObject> owners, CancellationToken cancellation)
    {
        var writer = await sender.OpenStreamAsync().AsTask().WaitAsync(cancellation); owners.Add(writer);
        byte[] payload = new byte[1024 * 1024];
        for (int i = 0; i < payload.Length; i++) payload[i] = unchecked((byte)(i * 31 + seed * 17));
        Task send = writer.SendAsync(payload, QuicSendOptions.Fin).AsTask();
        var reader = await receiver.AcceptStreamAsync(cancellation); owners.Add(reader);
        QuicReceiveLease offer = await reader.ReceiveAsync(cancellation) ?? throw new InvalidOperationException("missing blocked transfer offer");
        try
        {
            ReadOnlyMemory<byte> saved = offer.Buffers[0];
            byte[] expected = payload.AsSpan(checked((int)offer.AbsoluteOffset), saved.Length).ToArray();
            Check(saved.Span.SequenceEqual(expected) && offer.Length > 0 && offer.Length < (ulong)payload.Length,
                "initial held receive offer differs from sent payload");
            Task<QuicReceiveLease?> receive = writer.ReceiveAsync().AsTask();
            Check(!send.IsCompleted && !receive.IsCompleted, "transfer is not flow-control blocked with a pending opposite read");
            return new(writer, reader, offer, saved, expected, send, receive);
        }
        catch { offer.Dispose(); throw; }
    }

    private static QuicConnection Local(Pair pair, bool server) => server ? pair.Server : pair.Client;
    private static QuicConnection Remote(Pair pair, bool server) => server ? pair.Client : pair.Server;
    private static void CheckPeerError(QuicConnection peer)
    {
        var close = peer.CloseInfo;
        Check(close is { Status: 0, PeerInitiated: true, ApplicationInitiated: true } && close.ErrorCode == ApplicationError,
            "peer did not receive the exact registration-wide application error");
    }
    private static async Task AbortedOperation(Task operation, bool disposed)
    {
        try { await operation.WaitAsync(Limit); }
        catch (OperationCanceledException) { return; }
        catch (ObjectDisposedException) when (disposed) { return; }
        catch (QuicTransportException) { return; }
        throw new InvalidOperationException("blocked operation succeeded despite connection shutdown");
    }
    private static async Task AbortedReceive(Task<QuicReceiveLease?> operation, bool disposed)
    {
        try
        {
            using var unexpected = await operation.WaitAsync(Limit);
            throw new InvalidOperationException("pending read reported data/FIN which the peer never sent");
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (disposed) { }
        catch (QuicTransportException) { }
    }
    private static void AssertDisposed(QuicObject owner)
    {
        try { _ = owner.ApplicationContext; }
        catch (ObjectDisposedException) { return; }
        throw new InvalidOperationException("parent disposal retained an open child");
    }
    private static async Task Throws<T>(Task operation) where T : Exception
    {
        try { await operation.WaitAsync(Limit); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static async Task Until(Func<bool> condition, CancellationToken cancellation)
    { while (!condition()) await Task.Delay(1, cancellation); }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Registration shutdown: " + message); }
}
