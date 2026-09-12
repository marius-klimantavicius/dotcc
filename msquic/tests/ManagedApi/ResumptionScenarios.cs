using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

internal static class ResumptionScenarios
{
    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 });
        await using var serverRegistration = await runtime.OpenRegistrationAsync("resumption-server");
        await using var clientRegistration = await runtime.OpenRegistrationAsync("resumption-client");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck);
        int callbacks = 0;
        string mode = "initial";
        TaskCompletionSource<byte[]> entered = NewEntered();
        TaskCompletionSource<bool> decision = NewDecision();
        TaskCompletionSource canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var options = new QuicConfigurationOptions
        {
            ServerResumptionValidation = (state, token) =>
            {
                Interlocked.Increment(ref callbacks);
                entered.TrySetResult(state.ToArray());
                if (mode == "sync-accept") return ValueTask.FromResult(true);
                if (mode == "sync-reject") return ValueTask.FromResult(false);
                if (mode == "close") token.Register(() => canceled.TrySetResult());
                return new(decision.Task);
            }
        };
        await using var serverConfiguration = await serverRegistration.CreateConfigurationAsync(
            ["resumption-control"u8.ToArray()], serverCredentials,
            new() { ServerResumptionLevel = QuicServerResumption.Tickets, PeerBidiStreamCount = 4 }, options: options);
        await using var clientConfiguration = await clientRegistration.CreateConfigurationAsync(
            ["resumption-control"u8.ToArray()], clientCredentials, new() { PeerBidiStreamCount = 4 });
        await using var listener = await serverRegistration.ListenAsync(serverConfiguration, new(IPAddress.Loopback, 0));

        var first = await Connect(clientRegistration, clientConfiguration, listener, default);
        QuicResumptionTicket ticket;
        await using (first.Client)
        await using (first.Server)
        {
            if (first.Client.GetStatistics().ResumptionSucceeded || callbacks != 0)
                throw new InvalidOperationException("Fresh handshake unexpectedly resumed.");
            first.Server.SendResumptionTicket("policy-state"u8);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            ticket = await first.Client.WaitForResumptionTicketAsync(cancellationToken: timeout.Token);
        }

        foreach (string selected in new[] { "sync-accept", "sync-reject", "async-accept", "async-reject" })
        {
            mode = selected; entered = NewEntered(); decision = NewDecision();
            int before = callbacks;
            Task<(QuicConnection Client, QuicConnection Server)> connecting = Connect(clientRegistration,
                clientConfiguration, listener, ticket.Bytes);
            byte[] observed = await entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            if (!observed.AsSpan().SequenceEqual("policy-state"u8))
                throw new InvalidOperationException("Server policy lost copied ticket application state.");
            bool accept = selected.EndsWith("accept", StringComparison.Ordinal);
            if (selected.StartsWith("async", StringComparison.Ordinal))
            {
                // Hold validation across the provider callback and worker turn.
                await Task.Delay(50);
                if (connecting.IsCompleted) throw new InvalidOperationException("Handshake escaped pending ticket validation.");
                decision.SetResult(accept);
            }
            var pair = await connecting;
            await using (pair.Client)
            await using (pair.Server)
            {
                if (callbacks != before + 1 || pair.Client.GetStatistics().ResumptionSucceeded != accept ||
                    pair.Server.GetStatistics().ResumptionSucceeded != accept || pair.Client.GetProtocolVersion() != 1 ||
                    pair.Server.CallbackFailure != null || pair.Client.CallbackFailure != null)
                    throw new InvalidOperationException("Incorrect ticket validation/resumption result: " + selected);
                await ExchangeFreshData(pair.Client, pair.Server);
                ConnectionQueries.Check(pair.Client);
                ConnectionQueries.Check(pair.Server);
            }
        }

        mode = "close"; entered = NewEntered(); decision = NewDecision();
        Task<(QuicConnection Client, QuicConnection Server)> closing = Connect(clientRegistration,
            clientConfiguration, listener, ticket.Bytes);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
        // Closing the owner must not wait for a policy that ignores cancellation.
        await serverRegistration.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(15));
        await canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        decision.SetResult(true); // Late approval cannot use a retired native handle.
        bool published = false;
        try
        {
            var unexpected = await closing;
            await unexpected.Client.DisposeAsync(); await unexpected.Server.DisposeAsync();
            published = true;
        }
        catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
        if (published) throw new InvalidOperationException("A closed pending-validation connection was published.");
        Console.WriteLine("PASS managed API configuration control: server resumption sync/async approve/reject, full-handshake fallback, copied appstate, fresh 1-RTT data, pending close/cancel");
    }

    private static TaskCompletionSource<byte[]> NewEntered() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static TaskCompletionSource<bool> NewDecision() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task<(QuicConnection Client, QuicConnection Server)> Connect(QuicRegistration registration,
        QuicConfiguration configuration, QuicListener listener, ReadOnlyMemory<byte> ticket)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        QuicConnection? client = null;
        try
        {
            client = await registration.ConnectAsync(configuration, "localhost", listener.LocalEndPoint,
                new QuicConnectOptions { ResumptionTicket = ticket, Settings = new() { IdleTimeoutMs = 30000 } }, timeout.Token);
            return (client, await listener.AcceptConnectionAsync(timeout.Token));
        }
        catch { if (client != null) await client.DisposeAsync(); throw; }
    }

    private static async Task ExchangeFreshData(QuicConnection client, QuicConnection server)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await using var outbound = await client.OpenStreamAsync(cancellationToken: timeout.Token);
        await outbound.SendAsync("fresh-1rtt"u8.ToArray(), QuicSendOptions.Fin, timeout.Token);
        await using var inbound = await server.AcceptStreamAsync(timeout.Token);
        byte[] received = new byte[10]; int length = 0;
        while (length < received.Length)
        {
            int count = await inbound.ReadAsync(received.AsMemory(length), timeout.Token);
            if (count == 0) throw new InvalidOperationException("Early FIN in resumed stream.");
            length += count;
        }
        if (!received.AsSpan().SequenceEqual("fresh-1rtt"u8))
            throw new InvalidOperationException("Resumption fresh-1RTT stream payload mismatch.");
    }
}
