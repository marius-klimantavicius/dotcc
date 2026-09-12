using System.Net;
using Managed.Transport.Api;

// Calls only the owning public facade. No native notification is manufactured.
// Stream INLINE uses the public synchronous observer while callbackOwner is
// active. Task continuations are never substituted for a native callback.
internal static class CallbackControls
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(15);

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new()
            {
                ProcessorCount = 2, Parameters = new() { RetryMemoryLimit = ushort.MaxValue }
            });
            await using var registration = await runtime.OpenRegistrationAsync("listener-callback-controls");
            using var credentials = QuicCredentials.Server(certificates.EcLeaf);
            await using var configuration = await registration.CreateConfigurationAsync(["callback-controls"u8.ToArray()], credentials);
            await using var listener = await registration.ListenAsync(configuration, new(address, 0));
            IPEndPoint endpoint = listener.LocalEndPoint;
            Check(!listener.GetDosModeEventsEnabled() && listener.LastDosModeChange is null,
                "listener did not begin with notifications disabled");
            listener.SetDosModeEventsEnabled(true);
            Check(listener.GetDosModeEventsEnabled() && listener.LastDosModeChange is null,
                "enabling notifications in normal mode manufactured a transition");

            // library.c:1107–1122 applies the actual threshold and calls
            // QuicLibraryEvaluateSendRetryState. At :2827 even zero current
            // handshake usage meets a zero threshold, producing a true change.
            // listener.c:1068 delivers the typed DOS_MODE_CHANGED callback.
            runtime.SetRetryMemoryLimit(0);
            await Until(() => listener.LastDosModeChange is { Sequence: 1, RetryModeEnabled: true });
            QuicListenerDosState first = listener.LastDosModeChange!;
            Check(runtime.GetRetryMemoryLimit() == 0, "actual Retry threshold did not change");

            Throws<ArgumentOutOfRangeException>(() => listener.SetDosModeEventsEnabled(false, (QuicParameterPriority)2));
            Check(listener.GetDosModeEventsEnabled(QuicParameterPriority.High) && listener.LastDosModeChange == first,
                "invalid dispatch modifier mutated the notification setting or snapshot");
            runtime.SetRetryMemoryLimit(0);
            Check(listener.LastDosModeChange == first, "unchanged Retry state produced a duplicate event");

            listener.SetDosModeEventsEnabled(false);
            runtime.SetRetryMemoryLimit(ushort.MaxValue);
            Check(!listener.GetDosModeEventsEnabled() && listener.LastDosModeChange == first,
                "disabled listener received the normal-mode transition");
            listener.SetDosModeEventsEnabled(true);
            Check(listener.LastDosModeChange == first,
                "normal-mode enable unexpectedly synthesized a state event");
            runtime.SetRetryMemoryLimit(0);
            await Until(() => listener.LastDosModeChange is { Sequence: 2, RetryModeEnabled: true });
            runtime.SetRetryMemoryLimit(ushort.MaxValue);
            await Until(() => listener.LastDosModeChange is { Sequence: 3, RetryModeEnabled: false });

            // listener.c:899–901 also reports current active Retry mode when
            // notifications are enabled after that mode already became active.
            listener.SetDosModeEventsEnabled(false);
            runtime.SetRetryMemoryLimit(0);
            Check(listener.LastDosModeChange is { Sequence: 3, RetryModeEnabled: false },
                "disabled listener snapshot was changed");
            listener.SetDosModeEventsEnabled(true);
            await Until(() => listener.LastDosModeChange is { Sequence: 4, RetryModeEnabled: true });
            Check(listener.LocalEndPoint.Equals(endpoint), "mode changes replaced the listener binding");
            var statistics = listener.GetStatistics();
            Check(statistics.AcceptedConnections == 0 && statistics.RejectedConnections == 0,
                "DoS notification controls unexpectedly admitted a connection");

            QuicListenerDosState retired = listener.LastDosModeChange!;
            await listener.DisposeAsync().AsTask().WaitAsync(Limit);
            runtime.SetRetryMemoryLimit(ushort.MaxValue);
            runtime.SetRetryMemoryLimit(0);
            Check(listener.LastDosModeChange == retired, "retired listener received a later callback");
            Throws<ObjectDisposedException>(() => listener.SetDosModeEventsEnabled(true));
            await registration.DisposeAsync().AsTask().WaitAsync(Limit);
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0,
                "listener callback control leaked host ownership");
            Console.WriteLine($"PASS facade listener callback controls family={address.AddressFamily}");
        }
        await StreamObservers(certificates);
        Console.WriteLine("PASS facade callback controls: listener DoS transitions and public stream observers");
    }

    private static async Task StreamObservers(Credentials certificates)
    {
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("stream-callback-controls");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf);
        using var clientCredentials = QuicCredentials.Client([certificates.Root],
            System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck);
        var settings = new QuicSettings { PeerBidiStreamCount = 8, PeerUnidiStreamCount = 8, SendBufferingEnabled = false, IdleTimeoutMs = 20000 };
        await using var serverConfig = await registration.CreateConfigurationAsync(["stream-callbacks"u8.ToArray()], serverCredentials, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["stream-callbacks"u8.ToArray()], clientCredentials, settings);
        await using var listener = await registration.ListenAsync(serverConfig, new(IPAddress.Loopback, 0));
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accepted = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        await using var client = await registration.ConnectAsync(clientConfig, "localhost", listener.LocalEndPoint, timeout.Token);
        await using var server = await accepted.WaitAsync(Limit);
        foreach (QuicObject owner in new QuicObject[] { registration, serverConfig, clientConfig, listener, client, server })
        {
            object association = new(); owner.ApplicationContext = association;
            Check(ReferenceEquals(owner.ApplicationContext, association), "owner context association was not retained");
            owner.ApplicationContext = null;
            Check(owner.ApplicationContext is null, "owner context clear did not take effect");
        }
        await ReplaceAndClearObserver(client, server);
        await InlineAbort(client, server);
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        foreach (QuicObject owner in new QuicObject[] { registration, serverConfig, clientConfig, listener, client, server })
        {
            Throws<ObjectDisposedException>(() => _ = owner.ApplicationContext);
            Throws<ObjectDisposedException>(() => owner.ApplicationContext = new object());
        }
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0,
            "stream observer controls leaked host ownership");
        Console.WriteLine("PASS facade stream callback controls: actual INLINE abort, replacement, clear and context lifetime");
    }

    private static async Task ReplaceAndClearObserver(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional);
        object firstContext = new(), secondContext = new();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cleared = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int firstCalls = 0, secondCalls = 0;
        QuicStreamNotification? copiedStart = null;
        writer.SetCallbackHandler((stream, notification, context) =>
        {
            if (notification.Kind != QuicStreamNotificationKind.Started) return;
            Interlocked.Increment(ref firstCalls);
            Check(ReferenceEquals(stream, writer) && ReferenceEquals(context, firstContext), "original callback/context pair changed");
            Check(notification.Status == 0 && notification.StreamId == stream.Id, "Started observer ran before owner state publication");
            copiedStart = notification;
            stream.SetCallbackHandler((current, next, replacementContext) =>
            {
                Interlocked.Increment(ref secondCalls);
                Check(ReferenceEquals(replacementContext, secondContext), "replacement callback used the old context");
                if (next.Kind != QuicStreamNotificationKind.SendCompleted) return;
                Check(!next.Canceled, "healthy stream send was canceled");
                current.SetCallbackHandler(null);
                Check(current.ApplicationContext is null && ReferenceEquals(replacementContext, secondContext),
                    "clearing the observer changed an in-flight invocation's retained context");
                cleared.TrySetResult();
            }, secondContext);
            Check(ReferenceEquals(context, firstContext) && ReferenceEquals(stream.ApplicationContext, secondContext),
                "self-replacement did not retain old invocation and publish new context independently");
            started.TrySetResult();
        }, firstContext);
        Check(ReferenceEquals(writer.ApplicationContext, firstContext), "callback registration did not publish its context");
        await writer.StartAsync(QuicStreamStartOptions.Immediate).AsTask().WaitAsync(Limit);
        await started.Task.WaitAsync(Limit);
        byte[] first = Enumerable.Range(0, 4096).Select(i => unchecked((byte)(i * 31))).ToArray();
        Task send = writer.SendAsync(first).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        await ReadPart(reader, first);
        await send.WaitAsync(Limit);
        await cleared.Task.WaitAsync(Limit);
        int callsAtClear = Volatile.Read(ref secondCalls);
        object contextWithoutObserver = new(); writer.ApplicationContext = contextWithoutObserver;
        byte[] second = Enumerable.Range(0, 4097).Select(i => unchecked((byte)(i * 17 + 5))).ToArray();
        Task secondSend = writer.SendAsync(second, QuicSendOptions.Fin).AsTask();
        await ReadPart(reader, second);
        Check(await reader.ReadAsync(new byte[1]).AsTask().WaitAsync(Limit) == 0, "cleared observer lost FIN ownership");
        await secondSend.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        Check(Volatile.Read(ref firstCalls) == 1 && Volatile.Read(ref secondCalls) == callsAtClear,
            "retired observer received a later callback");
        Check(copiedStart is { Status: 0, Kind: QuicStreamNotificationKind.Started } && copiedStart.StreamId == writer.Id,
            "copied Started notification changed after later events");
        Check(ReferenceEquals(writer.ApplicationContext, contextWithoutObserver) && writer.CallbackFailure is null,
            "managed context replacement disturbed native callback ownership");
        await writer.DisposeAsync().AsTask().WaitAsync(Limit);
        Throws<ObjectDisposedException>(() => writer.SetCallbackHandler(null));
        Throws<ObjectDisposedException>(() => _ = writer.ApplicationContext);
        Throws<ObjectDisposedException>(() => writer.ApplicationContext = firstContext);
    }

    private static async Task InlineAbort(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional);
        object association = new();
        var invoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int starts = 0;
        writer.SetCallbackHandler((stream, notification, context) =>
        {
            Check(ReferenceEquals(context, association), "INLINE observer lost its managed context");
            if (notification.Kind == QuicStreamNotificationKind.Started)
            {
                Interlocked.Increment(ref starts);
                // This is the actual translated START_COMPLETE callback, not a
                // Task continuation or a test-only wrapper around a notification.
                stream.Shutdown(QuicStreamShutdownOptions.Abort | QuicStreamShutdownOptions.Inline, 0x321);
                invoked.TrySetResult();
            }
            if (notification.Kind == QuicStreamNotificationKind.Closed) closed.TrySetResult();
        }, association);
        await writer.StartAsync(QuicStreamStartOptions.Immediate).AsTask().WaitAsync(Limit);
        await invoked.Task.WaitAsync(Limit);
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        try
        {
            await reader.ReadAsync(new byte[1]).AsTask().WaitAsync(Limit);
            throw new InvalidOperationException("INLINE abort did not reach the peer as an application reset");
        }
        catch (QuicStreamAbortedException error)
        { Check(error.ApplicationErrorCode == 0x321, "INLINE reset changed the application error code"); }
        await closed.Task.WaitAsync(Limit);
        Check(Volatile.Read(ref starts) == 1 && writer.CallbackFailure is null,
            "INLINE shutdown failed inside the actual public observer");
    }

    private static async Task ReadPart(QuicStream stream, byte[] expected)
    {
        byte[] actual = new byte[expected.Length]; int offset = 0;
        while (offset < actual.Length)
        {
            int count = await stream.ReadAsync(actual.AsMemory(offset)).AsTask().WaitAsync(Limit);
            Check(count > 0, "stream ended before all observer-control bytes arrived"); offset += count;
        }
        Check(actual.AsSpan().SequenceEqual(expected), "observer replacement changed actual stream bytes");
    }

    private static async Task Until(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow + Limit;
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline) throw new TimeoutException("Actual listener callback was not observed");
            await Task.Delay(1);
        }
    }
    private static void Throws<T>(Action operation) where T : Exception
    {
        try { operation(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}
