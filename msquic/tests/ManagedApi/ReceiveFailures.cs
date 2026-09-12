using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Real facade callbacks and receive leases over authenticated loopback UDP.
// No generated callback is invoked by the test, and no product seam is added.
internal static class ReceiveFailures
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(15);
    private const int Length = 2 * 1024 * 1024;
    private static void Check(bool value, string reason)
    { if (!value) throw new InvalidOperationException("Receive failures: " + reason); }
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static byte[] Payload(int count, int seed) => Enumerable.Range(0, count).Select(i => unchecked((byte)(i * 31 + seed))).ToArray();

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await Pair(certificates, address, "replace", ReplaceDuringPendingReceive);
            await Pair(certificates, address, "throw", ThrowDuringPendingReceive);
            await Pair(certificates, address, "abort", AbortReceiveOnly);
        }
        Console.WriteLine("PASS facade receive failures: concurrent observer replacement, observer throw with delivered pending lease, standalone AbortReceive and surviving send direction; IPv4/IPv6; drained owners");
    }

    private static async Task Pair(Credentials certificates, IPAddress address, string scenario,
        Func<QuicConnection, QuicConnection, Task> exercise)
    {
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("receive-failures");
        using var serverCredential = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
        using var clientCredential = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, 0x1301);
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 4, PeerUnidiStreamCount = 4, SendBufferingEnabled = false,
            StreamRecvWindowDefault = 16384, StreamRecvBufferDefault = 16384,
            ConnFlowControlWindow = 65536, IdleTimeoutMs = 20000
        };
        await using var serverConfig = await registration.CreateConfigurationAsync(["receive-failures"u8.ToArray()], serverCredential, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["receive-failures"u8.ToArray()], clientCredential, settings);
        await using var listener = await registration.ListenAsync(serverConfig, new(address, 0));
        using var timeout = new CancellationTokenSource(Limit);
        var accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        await using var client = await registration.ConnectAsync(clientConfig, "localhost", listener.LocalEndPoint, timeout.Token);
        await using var server = await accepting.WaitAsync(Limit);
        await exercise(client, server);
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0 &&
            runtime.Host.OutstandingDatagramReceives == 0, "host owners or receive leases leaked");
        Check(runtime.Host.DatagramSendErrors == 0 && runtime.Host.DatagramReceiveErrors == 0, "unexpected OS error");
        Console.WriteLine($"PASS facade receive failure scenario={scenario} family={address.AddressFamily}");
    }

    private static async Task<(QuicStream Writer, QuicStream Reader)> Open(QuicConnection client, QuicConnection server, bool bidirectional = false)
    {
        var writer = await client.OpenStreamAsync(bidirectional ? QuicStreamOpenOptions.None : QuicStreamOpenOptions.Unidirectional);
        QuicStream? reader = null;
        try
        {
            Task initial = writer.SendAsync(new byte[] { 73 }).AsTask();
            reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
            var first = new byte[1];
            Check(await reader.ReadAsync(first).AsTask().WaitAsync(Limit) == 1 && first[0] == 73, "bootstrap stream byte");
            await initial.WaitAsync(Limit);
            // SEND_COMPLETE publishes its Task before notifying the observer.
            // Real serialized parameter calls fence both worker callbacks before
            // a scenario installs its observer and starts counting new events.
            _ = writer.GetPriority(); _ = reader.GetPriority();
            return (writer, reader);
        }
        catch
        {
            if (reader != null) await reader.DisposeAsync();
            await writer.DisposeAsync(); throw;
        }
    }

    private static async Task ReplaceDuringPendingReceive(QuicConnection client, QuicConnection server)
    {
        var pair = await Open(client, server);
        await using var writer = pair.Writer; await using var reader = pair.Reader;
        object originalContext = new(), replacementContext = new();
        using var entered = new ManualResetEventSlim(); using var replaced = new ManualResetEventSlim();
        var originalReturned = Signal(); var replacementReceived = Signal(); var closed = Signal();
        int oldReceives = 0, newReceives = 0, closedCount = 0, sendCompletions = 0;
        Exception? replacementFailure = null;
        QuicStreamNotification? copied = null;
        writer.SetCallbackHandler((_, value, _) =>
        { if (value.Kind == QuicStreamNotificationKind.SendCompleted) Interlocked.Increment(ref sendCompletions); });
        QuicStreamCallback replacement = (_, value, context) =>
        {
            Check(ReferenceEquals(context, replacementContext), "replacement used original context");
            if (value.Kind == QuicStreamNotificationKind.ReceiveOffered)
            { Interlocked.Increment(ref newReceives); replacementReceived.TrySetResult(); }
            if (value.Kind == QuicStreamNotificationKind.Closed)
            { Interlocked.Increment(ref closedCount); closed.TrySetResult(); }
        };
        reader.SetCallbackHandler((stream, value, context) =>
        {
            if (value.Kind != QuicStreamNotificationKind.ReceiveOffered) return;
            Interlocked.Increment(ref oldReceives); copied = value;
            Check(ReferenceEquals(context, originalContext), "original context mismatch before replacement");
            entered.Set();
            // A dedicated external thread performs only SetCallbackHandler,
            // whose implementation changes managed state without a core call.
            // Never wait for transport work, task continuations or disposal on
            // this callback thread. The test-only barrier is bounded.
            Check(replaced.Wait(Limit), "external replacement blocked behind in-flight observer");
            Check(ReferenceEquals(context, originalContext) && ReferenceEquals(stream.ApplicationContext, replacementContext),
                "in-flight snapshot or published replacement context was lost");
            originalReturned.TrySetResult();
        }, originalContext);
        var replacementThread = new Thread(() =>
        {
            try
            {
                Check(entered.Wait(Limit), "receive observer never entered");
                reader.SetCallbackHandler(replacement, replacementContext);
            }
            catch (Exception error) { replacementFailure = error; }
            finally { replaced.Set(); }
        }) { IsBackground = true, Name = "receive observer replacement control" };
        replacementThread.Start();
        try
        {
            byte[] expected = Payload(Length, 11), input = expected.ToArray();
            using var cancelReceive = new CancellationTokenSource();
            var receiving = reader.ReceiveAsync(cancelReceive.Token).AsTask();
            Task sending = writer.SendAsync(input, QuicSendOptions.Fin).AsTask();
            using var held = await receiving.WaitAsync(Limit) ?? throw new InvalidOperationException("Missing pending lease");
            await originalReturned.Task.WaitAsync(Limit);
            Check(replacementThread.Join(Limit), "replacement thread did not end");
            if (replacementFailure != null) throw new InvalidOperationException("Replacement thread failed", replacementFailure);
            Check(!sending.IsCompleted && held.Length < (ulong)Length, "pending receive did not retain actual backpressure");
            Check(Volatile.Read(ref oldReceives) == 1 && Volatile.Read(ref newReceives) == 0,
                "replacing observer consumed or redelivered the outstanding offer");
            Check(copied is { Kind: QuicStreamNotificationKind.ReceiveOffered, AbsoluteOffset: 1 } && copied.ByteCount == held.Length,
                "copied actual receive notification changed");
            var saved = held.Buffers[0]; byte[] savedBytes = saved.ToArray();
            Check(saved.Span.SequenceEqual(expected.AsSpan(0, saved.Length)), "held lease bytes before replacement");
            cancelReceive.Cancel(); // Delivery already won; cancellation cannot silently release its lease.
            Throws<OperationCanceledException>(() => reader.ReceiveAsync(cancelReceive.Token));
            input.AsSpan().Clear(); GC.Collect(2, GCCollectionMode.Forced, true, true);
            Check(!sending.IsCompleted && saved.Span.SequenceEqual(savedBytes), "cancel/replacement released pending ownership");
            int consumed = checked((int)held.Length); held.Complete(held.Length);
            Throws<ObjectDisposedException>(() => held.Complete(0));
            await ReadExact(reader, expected.AsMemory(consumed), eof: true);
            await sending.WaitAsync(Limit); await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
            await replacementReceived.Task.WaitAsync(Limit); await closed.Task.WaitAsync(Limit);
            await reader.DisposeAsync().AsTask().WaitAsync(Limit);
            Check(oldReceives == 1 && newReceives > 0 && closedCount == 1 && sendCompletions == 1 &&
                writer.CallbackFailure is null && reader.CallbackFailure is null && saved.Span.SequenceEqual(savedBytes),
                "replacement/receive completion or retained bytes violated lifetime");
        }
        finally
        {
            entered.Set(); replaced.Set();
            Check(replacementThread.Join(Limit), "replacement cleanup did not join");
        }
    }

    private static async Task ThrowDuringPendingReceive(QuicConnection client, QuicConnection server)
    {
        var pair = await Open(client, server);
        await using var writer = pair.Writer; await using var reader = pair.Reader;
        var injected = new InvalidOperationException("intentional actual RECEIVE observer failure");
        int receives = 0, closes = 0, sendCompletions = 0;
        writer.SetCallbackHandler((_, value, _) =>
        { if (value.Kind == QuicStreamNotificationKind.SendCompleted) Interlocked.Increment(ref sendCompletions); });
        reader.SetCallbackHandler((_, value, _) =>
        {
            if (value.Kind == QuicStreamNotificationKind.Closed) Interlocked.Increment(ref closes);
            if (value.Kind != QuicStreamNotificationKind.ReceiveOffered) return;
            Interlocked.Increment(ref receives); throw injected;
        });
        byte[] expected = Payload(Length, 23);
        Task<QuicStream> pendingAccept = server.AcceptStreamAsync().AsTask();
        // Install the owning receive waiter first. ProcessEvent delivers the
        // lease before invoking the public observer and preserves native PENDING
        // if that observer throws (QuicStream.cs333–345,376–401).
        var receiving = reader.ReceiveAsync().AsTask();
        Task sending = writer.SendAsync(expected, QuicSendOptions.Fin).AsTask();
        using var held = await receiving.WaitAsync(Limit) ?? throw new InvalidOperationException("Missing throwing-observer lease");
        var saved = held.Buffers[0]; byte[] savedBytes = saved.ToArray();
        Check(saved.Span.SequenceEqual(expected.AsSpan(0, saved.Length)), "throwing observer lost delivered bytes");
        await Until(() => ReferenceEquals(reader.CallbackFailure, injected) && ReferenceEquals(server.CallbackFailure, injected) && IsClosing(reader));
        try
        {
            await using var unexpected = await pendingAccept.WaitAsync(Limit);
            throw new InvalidOperationException("Observer failure did not fault pending public stream acceptance");
        }
        catch (Exception error) when (ReferenceEquals(error, injected) || ReferenceEquals(error.InnerException, injected)) { }
        Task firstClose = server.DisposeAsync().AsTask(), secondClose = server.DisposeAsync().AsTask();
        Check(!firstClose.IsCompleted && !secondClose.IsCompleted,
            "observer fault closed native owner while delivered receive was still pending");
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        Check(saved.Span.SequenceEqual(savedBytes) && held.Length > 0, "fault/close invalidated a delivered lease");
        held.Dispose(); held.Dispose(); // Idempotent Dispose must complete native borrow exactly once.
        Throws<ObjectDisposedException>(() => held.Complete(0));
        await Task.WhenAll(firstClose, secondClose).WaitAsync(Limit);
        await CanceledSend(sending);
        await writer.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(receives == 1 && closes == 1 && sendCompletions == 1 && saved.Span.SequenceEqual(savedBytes),
            "fault did not preserve PENDING/terminal callback cardinality or cached memory");
        Throws<ObjectDisposedException>(() => reader.SetCallbackHandler(null));
    }

    private static async Task AbortReceiveOnly(QuicConnection client, QuicConnection server)
    {
        var pair = await Open(client, server, bidirectional: true);
        await using var clientStream = pair.Writer; await using var serverStream = pair.Reader;
        const ulong code = 0x214365;
        var peerAborted = Signal(); var clientClosed = Signal(); var serverClosed = Signal();
        int aborts = 0, clientCloses = 0, serverCloses = 0, clientSendCompletions = 0;
        clientStream.SetCallbackHandler((_, value, _) =>
        {
            if (value.Kind == QuicStreamNotificationKind.PeerReceiveAborted)
            { Check(value.ErrorCode == code, "STOP_SENDING changed exact peer error"); Interlocked.Increment(ref aborts); peerAborted.TrySetResult(); }
            if (value.Kind == QuicStreamNotificationKind.SendCompleted) Interlocked.Increment(ref clientSendCompletions);
            if (value.Kind == QuicStreamNotificationKind.Closed) { Interlocked.Increment(ref clientCloses); clientClosed.TrySetResult(); }
        });
        serverStream.SetCallbackHandler((_, value, _) =>
        { if (value.Kind == QuicStreamNotificationKind.Closed) { Interlocked.Increment(ref serverCloses); serverClosed.TrySetResult(); } });
        byte[] expected = Payload(Length, 41);
        var receiving = serverStream.ReceiveAsync().AsTask();
        Task sending = clientStream.SendAsync(expected).AsTask();
        using var held = await receiving.WaitAsync(Limit) ?? throw new InvalidOperationException("Missing abort receive lease");
        var saved = held.Buffers[0]; byte[] savedBytes = saved.ToArray();
        Check(!sending.IsCompleted && saved.Span.SequenceEqual(expected.AsSpan(0, saved.Length)), "receive-abort lacks held backpressure");
        // Original stream.c586–598 routes flag4 only to QuicStreamRecvShutdown.
        // The peer's actual STOP_SENDING event preserves the application code
        // (stream_recv.c358–359); the opposite send direction remains usable.
        serverStream.Shutdown(QuicStreamShutdownOptions.AbortReceive, code);
        await peerAborted.Task.WaitAsync(Limit); await CanceledSend(sending);
        byte[] reverse = Payload(65537, 67);
        Task reply = serverStream.SendAsync(reverse, QuicSendOptions.Fin).AsTask();
        await ReadExact(clientStream, reverse, eof: true);
        await reply.WaitAsync(Limit); await serverStream.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        Check(saved.Span.SequenceEqual(savedBytes), "opposite send or receive abort invalidated held lease");
        Task closing = serverStream.DisposeAsync().AsTask();
        await Until(() => IsClosing(serverStream));
        Check(!closing.IsCompleted, "standalone receive abort released delivered pending lease");
        held.Dispose(); Throws<ObjectDisposedException>(() => held.Complete(0));
        await closing.WaitAsync(Limit);
        await clientClosed.Task.WaitAsync(Limit); await serverClosed.Task.WaitAsync(Limit);
        await clientStream.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(aborts == 1 && clientCloses == 1 && serverCloses == 1 && clientSendCompletions == 1 &&
            clientStream.CallbackFailure is null && serverStream.CallbackFailure is null && saved.Span.SequenceEqual(savedBytes),
            "standalone receive abort damaged surviving direction, callbacks or owners");
        // Connection remains healthy after the half-stream failure.
        var health = await Open(client, server);
        await health.Reader.DisposeAsync().AsTask().WaitAsync(Limit);
        await health.Writer.DisposeAsync().AsTask().WaitAsync(Limit);
    }

    private static async Task ReadExact(QuicStream stream, ReadOnlyMemory<byte> expected, bool eof)
    {
        byte[] data = new byte[4093]; int offset = 0;
        while (offset < expected.Length)
        {
            int count = await stream.ReadAsync(data).AsTask().WaitAsync(Limit);
            Check(count > 0 && count <= expected.Length - offset && data.AsSpan(0, count).SequenceEqual(expected.Span.Slice(offset, count)), "exact stream bytes");
            offset += count;
        }
        if (eof) Check(await stream.ReadAsync(data).AsTask().WaitAsync(Limit) == 0, "missing FIN after exact bytes");
    }
    private static async Task CanceledSend(Task sending)
    {
        try { await sending.WaitAsync(Limit); }
        catch (Exception error) when (error is OperationCanceledException or QuicStreamAbortedException or QuicTransportException) { return; }
        throw new InvalidOperationException("Aborted backpressured send succeeded");
    }
    private static bool IsClosing(QuicStream stream)
    { try { _ = stream.ApplicationContext; return false; } catch (ObjectDisposedException) { return true; } }
    private static async Task Until(Func<bool> predicate)
    {
        var start = System.Diagnostics.Stopwatch.GetTimestamp();
        while (!predicate())
        {
            if (System.Diagnostics.Stopwatch.GetElapsedTime(start) > Limit) throw new TimeoutException("Receive ownership transition timed out");
            await Task.Delay(1);
        }
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
