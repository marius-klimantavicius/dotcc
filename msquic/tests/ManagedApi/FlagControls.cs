using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Staged outside the ManagedApi compile glob until the current matrix is done.
// All notifications and credit changes below come from actual endpoints.
internal static class FlagControls
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);
    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("selected-flag-controls");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck);
        int finalValidation = 0;
        var serverOptions = new QuicConfigurationOptions
        {
            DelayAcceptedStreamCreditUntilClose = true,
            ServerResumptionValidation = (state, _) =>
            {
                Check(state.Span.SequenceEqual("final-state"u8), "final ticket lost its application state");
                Interlocked.Increment(ref finalValidation); return ValueTask.FromResult(true);
            }
        };
        var serverSettings = new QuicSettings
        {
            PeerBidiStreamCount = 8, PeerUnidiStreamCount = 0, SendBufferingEnabled = false,
            ServerResumptionLevel = QuicServerResumption.Tickets, IdleTimeoutMs = 20000, DatagramReceiveEnabled = true
        };
        await using var serverConfig = await registration.CreateConfigurationAsync(["flag-controls"u8.ToArray()], serverCredentials,
            serverSettings, options: serverOptions);
        await using var clientConfig = await registration.CreateConfigurationAsync(["flag-controls"u8.ToArray()], clientCredentials,
            new() { PeerBidiStreamCount = 8, PeerUnidiStreamCount = 8, SendBufferingEnabled = false, IdleTimeoutMs = 20000, DatagramReceiveEnabled = true });
        await using var listener = await registration.ListenAsync(serverConfig, new(IPAddress.Loopback, 0));
        await DelayedAcceptedCredit(registration, clientConfig, listener);
        var pair = await Connect(registration, clientConfig, listener);
        QuicResumptionTicket finalTicket;
        await using (pair.Client)
        await using (pair.Server)
        {
            await FailedStart(pair.Client, shutdownOnFail: false);
            await FailedStart(pair.Client, shutdownOnFail: true);
            await CreditUnblocksPeerAccept(pair.Client, pair.Server);
            await DelayedSendFlushedByOtherStream(pair.Client, pair.Server);
            await DatagramPriority(pair.Client, pair.Server);
            await InvalidFlagClassification(pair.Client);
            finalTicket = await FinalTicket(pair.Server, pair.Client);
        }
        var resumed = await Connect(registration, clientConfig, listener, finalTicket.Bytes);
        await using (resumed.Client)
        await using (resumed.Server)
        {
            Check(resumed.Client.GetStatistics().ResumptionSucceeded && resumed.Server.GetStatistics().ResumptionSucceeded &&
                Volatile.Read(ref finalValidation) == 1, "FINAL ticket was not independently usable for fresh-1RTT resumption");
        }
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0,
            "selected flag controls leaked host ownership");
        Console.WriteLine("PASS facade flag controls: delayed credit, blocked start, peer acceptance, priority datagram, delayed send and final ticket");
    }

    private static async Task DelayedAcceptedCredit(QuicRegistration registration, QuicConfiguration configuration, QuicListener listener)
    {
        var pair = await Connect(registration, configuration, listener);
        await using var client = pair.Client;
        await using var server = pair.Server;
        server.SetSettings(new() { PeerUnidiStreamCount = 1 });
        await Until(() => client.GetCapabilities().AvailableUnidirectionalStreams == 1);
        ulong grant = server.GetMaximumStreamIds().ClientUnidirectional;
        await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional);
        byte[] data = Payload(4096, 81);
        Task send = writer.SendAsync(data, QuicSendOptions.Fin).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        var closed = NewNotification();
        reader.SetCallbackHandler((_, value, _) =>
        { if (value.Kind == QuicStreamNotificationKind.Closed) closed.TrySetResult(value); });
        await ReadAll(reader, data); await send.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        await closed.Task.WaitAsync(Limit);
        // This queued getter executes after the Closed callback has returned.
        // It reads the server's actual granted count, without depending on when
        // a MAX_STREAMS UDP packet would reach the client. stream.c:648 retains
        // the closed stream while DelayIdFcUpdate is set and HandleClosed=false.
        Check(server.GetMaximumStreamIds().ClientUnidirectional == grant,
            "accepted-stream close notification released ID credit before owning StreamClose");
        await FailedStart(client, shutdownOnFail: true);
        await reader.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(server.GetMaximumStreamIds().ClientUnidirectional == grant + 4,
            "owning StreamClose failed to release exactly one delayed stream-ID credit");
        await Until(() => client.GetCapabilities().AvailableUnidirectionalStreams == 1);
        await using var next = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional);
        await next.StartAsync(QuicStreamStartOptions.FailBlocked | QuicStreamStartOptions.Immediate).AsTask().WaitAsync(Limit);
        byte[] nextData = Payload(100, 82); Task nextSend = next.SendAsync(nextData, QuicSendOptions.Fin).AsTask();
        await using var nextPeer = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        await ReadAll(nextPeer, nextData); await nextSend.WaitAsync(Limit);
        await next.CompleteWritesAsync().AsTask().WaitAsync(Limit);
    }

    private static async Task DatagramPriority(QuicConnection client, QuicConnection server)
    {
        await Until(() => client.MaximumDatagramSendLength >= 256);
        await Throws<NotSupportedException>(() => client.SendDatagramAsync(new byte[1], options: (QuicDatagramSendOptions)1).AsTask());
        await Throws<ArgumentException>(() => client.SendDatagramAsync(new byte[1], options: (QuicDatagramSendOptions)0x80000000u).AsTask());
        await using var trigger = await client.CreateStreamAsync();
        var queued = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sends = new List<Task<QuicDatagramSendResult>>();
        byte[][] expected = [Payload(256, 91), Payload(256, 92), Payload(256, 93)];
        trigger.SetCallbackHandler((_, notification, _) =>
        {
            if (notification.Kind != QuicStreamNotificationKind.Started) return;
            // These actual sends are admitted during one native callback; the
            // core drains its API queue after returning. datagram.c:435 inserts
            // the flagged request into its priority prefix. UDP/QUIC DATAGRAM
            // does not promise peer delivery order, so compare payload identity,
            // not wall-clock or receive order.
            sends.Add(client.SendDatagramAsync(expected[0]).AsTask());
            sends.Add(client.SendDatagramAsync(expected[1], options: QuicDatagramSendOptions.Priority).AsTask());
            sends.Add(client.SendDatagramAsync(expected[2]).AsTask());
            queued.TrySetResult();
        });
        await trigger.StartAsync().AsTask().WaitAsync(Limit);
        await queued.Task.WaitAsync(Limit);
        var observed = new HashSet<string>();
        using var timeout = new CancellationTokenSource(Limit);
        for (int i = 0; i < 3; ++i)
        {
            var bytes = await server.ReceiveDatagramAsync(timeout.Token);
            Check(expected.Any(candidate => candidate.AsSpan().SequenceEqual(bytes.Span)) && observed.Add(Convert.ToHexString(bytes.Span)),
                "priority datagram was duplicated or changed");
        }
        var results = await Task.WhenAll(sends).WaitAsync(Limit);
        Check(results.All(result => result is QuicDatagramSendResult.Acknowledged or QuicDatagramSendResult.AcknowledgedAfterLoss),
            "priority datagram did not receive actual terminal acknowledgement");
        Check(trigger.CallbackFailure is null, "priority datagram callback failed");
    }

    private static async Task InvalidFlagClassification(QuicConnection connection)
    {
        int contexts = QuicObject.LiveContextCount;
        await Throws<NotSupportedException>(async () => { await using var unexpected = await connection.CreateStreamAsync((QuicStreamOpenOptions)2); });
        await Throws<ArgumentException>(async () => { await using var unexpected = await connection.CreateStreamAsync((QuicStreamOpenOptions)0x80000000u); });
        Check(QuicObject.LiveContextCount == contexts, "rejected open flags published a native callback root");
        await using var stream = await connection.CreateStreamAsync();
        int starts = 0;
        stream.SetCallbackHandler((_, value, _) => { if (value.Kind == QuicStreamNotificationKind.Started) Interlocked.Increment(ref starts); });
        await Throws<NotSupportedException>(() => stream.StartAsync((QuicStreamStartOptions)16).AsTask());
        await Throws<ArgumentException>(() => stream.StartAsync((QuicStreamStartOptions)0x80000000u).AsTask());
        await Throws<ArgumentException>(() => stream.SendAsync(new byte[1], (QuicSendOptions)8).AsTask());
        await Throws<NotSupportedException>(() => stream.SendAsync(new byte[1], (QuicSendOptions)1).AsTask());
        await Throws<ArgumentException>(() => stream.SendAsync(new byte[1], (QuicSendOptions)0x80000000u).AsTask());
        Check(Volatile.Read(ref starts) == 0, "invalid flags dispatched a start");
        await stream.StartAsync().AsTask().WaitAsync(Limit);
        Check(stream.Id != ulong.MaxValue, "invalid flags mutated the later valid start");
    }

    private static async Task FailedStart(QuicConnection client, bool shutdownOnFail)
    {
        await using var stream = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional);
        var started = NewNotification(); var closed = NewNotification();
        stream.SetCallbackHandler((_, value, _) =>
        {
            if (value.Kind == QuicStreamNotificationKind.Started) started.TrySetResult(value);
            if (value.Kind == QuicStreamNotificationKind.Closed) closed.TrySetResult(value);
        });
        var flags = QuicStreamStartOptions.FailBlocked |
            (shutdownOnFail ? QuicStreamStartOptions.ShutdownOnFail : QuicStreamStartOptions.None);
        // stream_set.c:628–638 uses actual peer stream credit and returns the
        // LP64 ESTRPIPE status86 without assigning a stream ID when blocked.
        try { await stream.StartAsync(flags).AsTask().WaitAsync(Limit); throw new InvalidOperationException("Blocked start succeeded"); }
        catch (QuicTransportException error) { Check(error.Status == 86, "blocked start returned a different status"); }
        Check((await started.Task.WaitAsync(Limit)).Status == 86, "actual START_COMPLETE lost blocked status");
        if (shutdownOnFail) await closed.Task.WaitAsync(Limit);
        // No timing assertion claims the non-SHUTDOWN_ON_FAIL case cannot later
        // close. Its owning DisposeAsync below supplies explicit shutdown.
        Check(stream.CallbackFailure is null, "blocked start observer faulted");
    }

    private static async Task CreditUnblocksPeerAccept(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional);
        var accepted = NewNotification(); int events = 0;
        writer.SetCallbackHandler((_, value, _) =>
        {
            if (value.Kind != QuicStreamNotificationKind.PeerAccepted) return;
            Interlocked.Increment(ref events); accepted.TrySetResult(value);
        });
        await writer.StartAsync(QuicStreamStartOptions.Immediate | QuicStreamStartOptions.IndicatePeerAccept).AsTask().WaitAsync(Limit);
        Check(!accepted.Task.IsCompleted && server.GetSettings().PeerUnidiStreamCount == 0,
            "blocked stream was accepted before peer credit existed");
        byte[] data = Payload(4096, 7); Task send = writer.SendAsync(data, QuicSendOptions.Fin).AsTask();
        // Original stream_set.c:481–516 moves this actual waiting stream after
        // receipt of MAX_STREAMS and then indicates PEER_ACCEPTED exactly once.
        server.SetSettings(new() { PeerUnidiStreamCount = 8 });
        await accepted.Task.WaitAsync(Limit);
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        Check(reader.Id == writer.Id, "credit release accepted a different stream");
        await ReadAll(reader, data); await send.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        Check(Volatile.Read(ref events) == 1, "peer acceptance was not exactly once");
    }

    private static async Task DelayedSendFlushedByOtherStream(QuicConnection client, QuicConnection server)
    {
        await using var delayed = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional);
        await using var trigger = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional);
        byte[] first = Payload(8192, 11), second = Payload(4096, 29);
        Task delayedSend = delayed.SendAsync(first, QuicSendOptions.DelaySend | QuicSendOptions.Fin).AsTask();
        Task immediateSend = trigger.SendAsync(second, QuicSendOptions.Fin).AsTask();
        var reads = new List<Task>(); var peers = new List<QuicStream>();
        try
        {
            for (int n = 0; n < 2; ++n)
            {
                var peer = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit); peers.Add(peer);
                Check(peer.Id == delayed.Id || peer.Id == trigger.Id, "unexpected stream in delayed-send control");
                reads.Add(ReadAll(peer, peer.Id == delayed.Id ? first : second));
            }
            await Task.WhenAll(reads.Append(delayedSend).Append(immediateSend)).WaitAsync(Limit);
            await delayed.CompleteWritesAsync().AsTask().WaitAsync(Limit);
            await trigger.CompleteWritesAsync().AsTask().WaitAsync(Limit);
            Check(delayed.GetObservedEarlyDataLength() == 0 && trigger.GetObservedEarlyDataLength() == 0,
                "delayed send used early data");
        }
        finally { foreach (var peer in peers) await peer.DisposeAsync(); }
        // DELAY_SEND is a batching hint (stream_send.c:651–659), not a promise
        // about inter-stream delivery order or a minimum elapsed delay.
    }

    private static async Task<QuicResumptionTicket> FinalTicket(QuicConnection server, QuicConnection client)
    {
        server.SendResumptionTicket("before-final"u8);
        using var timeout = new CancellationTokenSource(Limit);
        var before = await client.WaitForResumptionTicketAsync(cancellationToken: timeout.Token);
        server.SendResumptionTicket("final-state"u8, QuicResumptionTicketFlags.Final);
        var final = await client.WaitForResumptionTicketAsync(before.Sequence, timeout.Token);
        Check(final.Sequence > before.Sequence && !final.Bytes.Span.SequenceEqual(before.Bytes.Span), "FINAL did not deliver its own ticket");
        // Receipt of the real ticket is the processing barrier: connection.c:
        // 7926 clears ResumptionEnabled after issuing it, and api.c:622 rejects
        // later issue requests. No elapsed-time assumption substitutes for it.
        try { server.SendResumptionTicket("after-final"u8); throw new InvalidOperationException("Ticket issuance continued after FINAL"); }
        catch (QuicTransportException error) { Check(error.Status == 1, "post-FINAL rejection did not preserve INVALID_STATE"); }
        return final;
    }

    private static async Task<(QuicConnection Client, QuicConnection Server)> Connect(
        QuicRegistration registration, QuicConfiguration configuration, QuicListener listener, ReadOnlyMemory<byte> ticket = default)
    {
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accept = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        QuicConnection? client = null;
        try
        {
            client = await registration.ConnectAsync(configuration, "localhost", listener.LocalEndPoint,
                new QuicConnectOptions { ResumptionTicket = ticket }, timeout.Token);
            return (client, await accept);
        }
        catch { if (client != null) await client.DisposeAsync(); throw; }
    }
    private static async Task ReadAll(QuicStream stream, byte[] expected)
    {
        byte[] buffer = new byte[997]; int offset = 0;
        while (true)
        {
            int count = await stream.ReadAsync(buffer).AsTask().WaitAsync(Limit);
            if (count == 0) break;
            Check(count <= expected.Length - offset && buffer.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)), "flagged send changed stream bytes");
            offset += count;
        }
        Check(offset == expected.Length, "flagged send lost bytes before FIN");
    }
    private static byte[] Payload(int length, int seed)
        => Enumerable.Range(0, length).Select(i => unchecked((byte)(i * 31 + seed))).ToArray();
    private static TaskCompletionSource<QuicStreamNotification> NewNotification()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static async Task Throws<T>(Func<Task> operation) where T : Exception
    {
        try { await operation().WaitAsync(Limit); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static async Task Until(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(Limit);
        while (!condition()) await Task.Delay(1, timeout.Token);
    }
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}
