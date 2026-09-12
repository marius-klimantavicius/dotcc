using System.Buffers;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Actual facade -> BCL host -> translated core -> loopback UDP, with no callback
// replacement or simulated send/receive completion.
internal static class Transport
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException("Facade transport: " + message); }
    private static byte[] Payload(int length, ulong stream, bool server = false)
    {
        var bytes = new byte[length];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)((ulong)i * 31 + stream * 17 + (server ? 83UL : 19UL)));
        return bytes;
    }
    internal static async Task RunAsync()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign, true));
        var names = new SubjectAlternativeNameBuilder(); names.AddDnsName("localhost"); names.AddIpAddress(IPAddress.Loopback); names.AddIpAddress(IPAddress.IPv6Loopback);
        request.CertificateExtensions.Add(names.Build());
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(1));
        using var serverCredentials = QuicCredentials.Server(certificate, cipherSuite: 0x1301);
        using var clientCredentials = QuicCredentials.Client([certificate], X509RevocationMode.NoCheck, cipherSuite: 0x1301);
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var registration = await runtime.OpenRegistrationAsync("facade-transport");
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 32, PeerUnidiStreamCount = 32, SendBufferingEnabled = false,
            StreamRecvWindowDefault = 16384, StreamRecvBufferDefault = 16384,
            ConnFlowControlWindow = 65536, IdleTimeoutMs = 15000
        };
        await using var serverConfig = await registration.CreateConfigurationAsync(["facade-transport"u8.ToArray()], serverCredentials, settings);
        await using var clientConfig = await registration.CreateConfigurationAsync(["facade-transport"u8.ToArray()], clientCredentials, settings);
        QuicConnection? closedClient = null;
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var listener = await registration.ListenAsync(serverConfig, new IPEndPoint(address, 0));
            listener.SetDosModeEventsEnabled(true);
            Check(listener.GetDosModeEventsEnabled(), "listener DoS notification option round trip");
            Check(listener.GetStatistics(QuicParameterPriority.High).AcceptedConnections == 0, "fresh listener statistics");
            var endpoint = listener.LocalEndPoint;
            await listener.StopAsync().AsTask().WaitAsync(Limit);
            await listener.StartAsync(endpoint).AsTask().WaitAsync(Limit);
            Check(listener.LocalEndPoint.Equals(endpoint), "listener restarts on the same selected endpoint");
            using (var cancelAccept = new CancellationTokenSource())
            {
                var pending = listener.AcceptConnectionAsync(cancelAccept.Token).AsTask(); cancelAccept.Cancel();
                await ThrowsAsync<OperationCanceledException>(pending, "canceled accept");
            }
            var accepted = listener.AcceptConnectionAsync().AsTask();
            await using var client = await registration.ConnectAsync(clientConfig, "localhost", endpoint).AsTask().WaitAsync(Limit);
            await using var server = await accepted.WaitAsync(Limit);
            closedClient = client;
            Check(listener.GetStatistics().AcceptedConnections == 1, "actual listener accepted counter");
            await ExplicitStartAsync(client, server);
            await MultipleBidirectionalAsync(client, server);
            await PartialUnidirectionalAsync(client, server);
            await CancelAdmittedSendAsync(client, server);
            await GracefulShutdownAsync(client, server);
            await AbortAsync(client, server);
            await ParentCloseWithLeaseAsync(client, server);
            Console.WriteLine("PASS facade stream/lifetime " + address.AddressFamily);
        }
        await ParentOpenCloseRaceAsync(registration, serverConfig, clientConfig);
        await runtime.DisposeAsync();
        int roots = QuicObject.LiveContextCount;
        await ThrowsAsync<ObjectDisposedException>(registration.ListenAsync(serverConfig, new IPEndPoint(IPAddress.Loopback, 0)).AsTask(), "listener factory after full runtime close");
        await ThrowsAsync<ObjectDisposedException>(closedClient!.OpenStreamAsync().AsTask(), "stream factory after full runtime close");
        Check(QuicObject.LiveContextCount == roots, "failed factories retire newly rooted contexts without dispatching to closed cleanup queue");
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0 &&
            runtime.Host.OutstandingDatagramReceives == 0, "all facade/core/host ownership drained");
    }

    private static async Task ExplicitStartAsync(QuicConnection client, QuicConnection server)
    {
        await using (var stream = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit))
        {
            Throws<InvalidOperationException>(() => _ = stream.Id, "unstarted stream has no published ID");
            using (var alreadyCanceled = new CancellationTokenSource())
            {
                alreadyCanceled.Cancel();
                Throws<OperationCanceledException>(() => stream.StartAsync(cancellationToken: alreadyCanceled.Token), "pre-admission start cancellation");
            }
            await stream.StartAsync(QuicStreamStartOptions.Immediate).AsTask().WaitAsync(Limit);
            byte[] expected = Payload(4096, stream.Id);
            Task send = stream.SendAsync(expected, QuicSendOptions.Fin).AsTask();
            await using var peer = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
            await Task.WhenAll(send, ReadAndVerifyAsync(peer, expected)).WaitAsync(Limit);
            Throws<InvalidOperationException>(() => stream.StartAsync(), "explicit start is admitted once");
        }
        await using (var stream = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit))
        {
            // START and FIN act on this genuinely unstarted native handle.
            byte[] expected = Payload(4096, 77);
            Task send = stream.SendAsync(expected, QuicSendOptions.Start | QuicSendOptions.Fin).AsTask();
            await using var peer = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
            await Task.WhenAll(send, ReadAndVerifyAsync(peer, expected)).WaitAsync(Limit);
            Check(stream.Id == peer.Id, "Send START publishes the actual native stream ID");
            Throws<InvalidOperationException>(() => stream.StartAsync(), "Send START prevents a duplicate explicit start");
        }
        await using (var stream = await client.CreateStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit))
        {
            using var release = new ManualResetEventSlim(false);
            using var cancellation = new CancellationTokenSource();
            var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            int completions = 0;
            QuicStream.StartCompletionControl = value =>
            {
                if (!ReferenceEquals(value, stream)) return;
                Interlocked.Increment(ref completions); reached.TrySetResult();
                if (!release.Wait(Limit)) throw new TimeoutException("Actual START_COMPLETE callback gate was not released");
            };
            try
            {
                try
                {
                    Task wait = stream.StartAsync(QuicStreamStartOptions.Immediate, cancellation.Token).AsTask();
                    await reached.Task.WaitAsync(Limit);
                    cancellation.Cancel();
                    await ThrowsAsync<OperationCanceledException>(wait, "cancel wait after actual native start completion enters callback");
                }
                finally { release.Set(); }
                await UntilAsync(() => HasId(stream));
                Check(Volatile.Read(ref completions) == 1, "exactly one actual native start completion");
                Console.WriteLine("PASS canceled start wait: gated delivery of one actual START_COMPLETE callback; native scheduling is not simulated");
                byte[] expected = Payload(4096, stream.Id);
                Task send = stream.SendAsync(expected, QuicSendOptions.Fin).AsTask();
                await using var peer = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
                await Task.WhenAll(send, ReadAndVerifyAsync(peer, expected)).WaitAsync(Limit);
            }
            finally { release.Set(); QuicStream.StartCompletionControl = null; }
        }
    }
    private static bool HasId(QuicStream stream)
    {
        try { _ = stream.Id; return true; }
        catch (InvalidOperationException) { return false; }
    }

    private static async Task MultipleBidirectionalAsync(QuicConnection client, QuicConnection server)
    {
        var locals = new Dictionary<ulong, QuicStream>();
        var remotes = new List<QuicStream>();
        var transfers = new List<Task>();
        try
        {
            for (int i = 0; i < 4; i++)
            {
                var stream = await client.OpenStreamAsync().AsTask().WaitAsync(Limit); locals.Add(stream.Id, stream);
                stream.SetPriority((ushort)(17 + i)); Check(stream.GetPriority(QuicParameterPriority.High) == 17 + i, "stream priority round trip");
                Throws<ArgumentOutOfRangeException>(() => stream.SetPriority(0, (QuicParameterPriority)7), "invalid priority modifier");
                Check(stream.GetIdealSendBufferSize() > 0, "actual ideal send-buffer query");
                Throws<ArgumentException>(() => stream.Shutdown(QuicStreamShutdownOptions.None), "empty shutdown flags");
                Throws<ArgumentException>(() => stream.Shutdown(QuicStreamShutdownOptions.Graceful | QuicStreamShutdownOptions.AbortSend), "graceful/abort flags");
                Throws<ArgumentException>(() => stream.Shutdown(QuicStreamShutdownOptions.Immediate), "immediate requires both abort directions");
                Throws<InvalidOperationException>(() => stream.Shutdown(QuicStreamShutdownOptions.Abort | QuicStreamShutdownOptions.Inline), "inline outside native callback");
                transfers.Add(stream.SendAsync(Payload(65537, stream.Id), QuicSendOptions.Fin).AsTask());
                transfers.Add(ReadAndVerifyAsync(stream, Payload(65537, stream.Id, server: true)));
            }
            for (int i = 0; i < 4; i++)
            {
                var stream = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit); remotes.Add(stream);
                Check(locals.ContainsKey(stream.Id) && stream.CanRead && stream.CanWrite, "matching bidirectional stream ID");
                transfers.Add(ReadAndVerifyAsync(stream, Payload(65537, stream.Id)));
                transfers.Add(stream.SendAsync(Payload(65537, stream.Id, server: true), QuicSendOptions.Fin).AsTask());
            }
            await Task.WhenAll(transfers).WaitAsync(Limit);
            foreach (var stream in locals.Values.Concat(remotes))
            {
                await stream.CompleteWritesAsync().AsTask().WaitAsync(Limit);
                Check(stream.GetObservedEarlyDataLength() == 0, "selected profile observed zero early-data bytes");
                _ = stream.GetStatistics();
            }
            Check(locals.Values.Any(stream => stream.LastIdealSendBufferRecommendation is > 0), "copied real ideal-buffer recommendation callback");
        }
        finally
        {
            await Task.WhenAll(locals.Values.Concat(remotes).Select(stream => stream.DisposeAsync().AsTask())).WaitAsync(Limit);
        }
    }
    private static async Task PartialUnidirectionalAsync(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        byte[] expected = Payload(1024, writer.Id);
        var send = writer.SendAsync(expected, QuicSendOptions.Fin).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        Check(!writer.CanRead && writer.CanWrite && reader.CanRead && !reader.CanWrite, "unidirectional ownership");
        await ThrowsAsync<InvalidOperationException>(reader.SendAsync(new byte[1]).AsTask(), "receive-only stream send");
        using var first = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing first offer");
        Check(first.AbsoluteOffset == 0 && first.Length > 1, "first deferred offer");
        int prefix = Math.Min(17, checked((int)(first.Length / 2)));
        ReadOnlyMemory<byte> saved = first.Buffers[0]; byte[] savedExpected = saved.ToArray();
        first.Complete(0, resume: false);
        Throws<ObjectDisposedException>(() => first.Complete(0), "one completion per offer");
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel();
            Throws<OperationCanceledException>(() => reader.ReceiveAsync(canceled.Token), "canceled read cannot consume or resume paused offer");
        }
        using (var repeated = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing repeated offer"))
        {
            Check(repeated.AbsoluteOffset == 0 && repeated.Buffers[0].Span.SequenceEqual(expected.AsSpan(0, checked((int)repeated.Length))), "zero completion preserves exact prefix");
            repeated.Complete((uint)prefix, resume: false);
        }
        using (var suffix = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing partial suffix"))
        {
            Check(suffix.AbsoluteOffset == (ulong)prefix && suffix.Buffers[0].Span.SequenceEqual(expected.AsSpan(prefix, checked((int)suffix.Length))), "partial completion redelivers exact suffix");
            ulong consumed = suffix.Length; suffix.Complete(consumed);
            await ReadAndVerifyAsync(reader, expected.AsMemory(checked(prefix + (int)consumed)));
        }
        await send.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
        Check(writer.GetObservedEarlyDataLength() == 0, "unidirectional early data observation");
        await Task.Run(() => CachedViewsSurviveClose(reader, saved, savedExpected)).WaitAsync(Limit);
        Check(saved.Span.SequenceEqual(savedExpected), "cached managed memory after close");
    }
    private static void CachedViewsSurviveClose(QuicStream stream, ReadOnlyMemory<byte> memory, byte[] expected)
    {
        ReadOnlySpan<byte> span = memory.Span;
        using var pin = memory.Pin();
        stream.DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        Check(span.SequenceEqual(expected) && PinnedMatches(pin, expected), "cached span and pin remain safe after native stream close");
    }
    private static unsafe bool PinnedMatches(MemoryHandle pin, byte[] expected)
        => new ReadOnlySpan<byte>(pin.Pointer, expected.Length).SequenceEqual(expected);

    private static async Task CancelAdmittedSendAsync(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        byte[] expected = Payload(1024 * 1024, writer.Id); byte[] application = expected.ToArray();
        using var cancellation = new CancellationTokenSource();
        Task send = writer.SendAsync(application, QuicSendOptions.Fin, cancellation.Token).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        using var held = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing admitted-send offer");
        Check(!send.IsCompleted, "unbuffered large send remains owned while peer receive window is held");
        cancellation.Cancel(); await ThrowsAsync<OperationCanceledException>(send, "post-admission send wait cancellation");
        application.AsSpan().Fill(0); GC.Collect(2, GCCollectionMode.Forced, true, true);
        Check(held.Buffers[0].Span.SequenceEqual(expected.AsSpan(0, checked((int)held.Length))), "admitted send copied input before caller mutation");
        int consumed = checked((int)held.Length); held.Complete(held.Length);
        await ReadAndVerifyAsync(reader, expected.AsMemory(consumed));
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
    }
    private static async Task GracefulShutdownAsync(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        byte[] expected = Payload(4096, writer.Id);
        Task send = writer.SendAsync(expected).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        Task read = ReadAndVerifyAsync(reader, expected);
        await send.WaitAsync(Limit);
        writer.Shutdown(QuicStreamShutdownOptions.Graceful);
        // Duplicate completion observers must share graceful shutdown ownership.
        await Task.WhenAll(writer.CompleteWritesAsync().AsTask(), writer.CompleteWritesAsync().AsTask(), read).WaitAsync(Limit);
        Check(writer.GetObservedEarlyDataLength() == 0, "graceful native shutdown acknowledged without early data");
    }
    private static async Task AbortAsync(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        Task send = writer.SendAsync(new byte[] { 31 }).AsTask();
        await using var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        using (var offer = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing pre-abort offer")) offer.Complete(offer.Length);
        await send.WaitAsync(Limit);
        var read = reader.ReadAsync(new byte[1]).AsTask();
        writer.Shutdown(QuicStreamShutdownOptions.AbortSend, 0x123);
        try { await read.WaitAsync(Limit); throw new InvalidOperationException("Abort reported EOF instead of application error"); }
        catch (QuicStreamAbortedException error) { Check(error.ApplicationErrorCode == 0x123, "application abort code preserved independently of TLS alerts"); }

        await using var both = await client.OpenStreamAsync().AsTask().WaitAsync(Limit);
        Task initial = both.SendAsync(new byte[] { 7 }).AsTask();
        await using var remote = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        using (var offer = await remote.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing immediate-abort offer")) offer.Complete(offer.Length);
        await initial.WaitAsync(Limit);
        Task remoteRead = remote.ReadAsync(new byte[1]).AsTask();
        both.Shutdown(QuicStreamShutdownOptions.Abort | QuicStreamShutdownOptions.Immediate, 0x456);
        await ThrowsAsync<OperationCanceledException>(both.ReadAsync(new byte[1]).AsTask(), "immediate abort ends local read");
        try { await remoteRead.WaitAsync(Limit); throw new InvalidOperationException("Immediate abort reported EOF"); }
        catch (QuicStreamAbortedException error) { Check(error.ApplicationErrorCode == 0x456, "immediate abort reaches peer with actual application code"); }
    }
    private static async Task ParentCloseWithLeaseAsync(QuicConnection client, QuicConnection server)
    {
        await using var writer = await client.OpenStreamAsync().AsTask().WaitAsync(Limit);
        byte[] bytes = Payload(1024, writer.Id);
        Task send = writer.SendAsync(bytes).AsTask();
        var reader = await server.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        using var lease = await reader.ReceiveAsync().AsTask().WaitAsync(Limit) ?? throw new InvalidOperationException("Missing borrowed close offer");
        ReadOnlyMemory<byte> saved = lease.Buffers[0]; byte[] expected = saved.ToArray();
        Task close = server.DisposeAsync().AsTask();
        try
        {
            await UntilAsync(() => IsClosing(reader));
            Check(!close.IsCompleted, "parent close retains a delivered receive lease");
            Check(saved.Span.SequenceEqual(expected), "held lease remains safe during parent close");
        }
        finally { lease.Dispose(); }
        await close.WaitAsync(Limit);
        await client.DisposeAsync().AsTask().WaitAsync(Limit);
        try { await send.WaitAsync(Limit); } catch (OperationCanceledException) { }
        Check(saved.Span.SequenceEqual(expected), "held managed view remains safe after parent close");
    }
    private static bool IsClosing(QuicStream stream)
    {
        try { _ = stream.GetPriority(); return false; }
        catch (ObjectDisposedException) { return true; }
    }
    private static async Task ParentOpenCloseRaceAsync(QuicRegistration registration, QuicConfiguration serverConfig, QuicConfiguration clientConfig)
    {
        await using var listener = await registration.ListenAsync(serverConfig, new IPEndPoint(IPAddress.Loopback, 0));
        var accept = listener.AcceptConnectionAsync().AsTask();
        var client = await registration.ConnectAsync(clientConfig, "localhost", listener.LocalEndPoint).AsTask().WaitAsync(Limit);
        await using var server = await accept.WaitAsync(Limit);
        await using var unstarted = await client.CreateStreamAsync().AsTask().WaitAsync(Limit);
        var opens = Enumerable.Range(0, 8).Select(_ => OpenDuringCloseAsync(client)).ToArray();
        await client.DisposeAsync().AsTask().WaitAsync(Limit);
        await Task.WhenAll(opens).WaitAsync(Limit);
        Throws<ObjectDisposedException>(() => unstarted.GetPriority(), "parent closes an unstarted native stream");
    }
    private static async Task OpenDuringCloseAsync(QuicConnection connection)
    {
        try { await using var stream = await connection.OpenStreamAsync().AsTask().WaitAsync(Limit); }
        catch (Exception error) when (error is ObjectDisposedException or OperationCanceledException or QuicTransportException) { }
    }
    private static async Task ReadAndVerifyAsync(QuicStream stream, ReadOnlyMemory<byte> expected)
    {
        var buffer = new byte[4093]; int offset = 0;
        while (true)
        {
            int length = await stream.ReadAsync(buffer).AsTask().WaitAsync(Limit);
            if (length == 0) break;
            Check(length <= expected.Length - offset && buffer.AsSpan(0, length).SequenceEqual(expected.Span.Slice(offset, length)), "stream payload order/content");
            offset += length;
        }
        Check(offset == expected.Length, "FIN follows every expected byte");
    }
    private static async Task UntilAsync(Func<bool> predicate)
    {
        var deadline = DateTime.UtcNow + Limit;
        while (!predicate())
        {
            if (DateTime.UtcNow >= deadline) throw new TimeoutException("Facade state transition did not occur");
            await Task.Delay(1);
        }
    }
    private static void Throws<T>(Action operation, string description) where T : Exception
    {
        try { operation(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + description);
    }
    private static async Task ThrowsAsync<T>(Task operation, string description) where T : Exception
    {
        try { await operation.WaitAsync(Limit); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + description);
    }
}

// This partial implementation is compiled only by the source-linked test
// project. Production QuicStream omits both hook call and implementation.
namespace Managed.Transport.Api
{
    public sealed partial class QuicStream
    {
        internal static Action<QuicStream>? StartCompletionControl;
        partial void ObserveStartCompletion() => Volatile.Read(ref StartCompletionControl)?.Invoke(this);
    }
}
