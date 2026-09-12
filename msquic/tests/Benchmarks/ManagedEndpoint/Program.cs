using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

internal static class Program
{
    private const byte ReadyMarker = 0xa7;
    private static readonly byte[] Alpn = "dotcc-bench-v1"u8.ToArray();
    private sealed record Options(bool Server, string Certificate, string KeyOrName, IPEndPoint Endpoint,
        string ReadyFile, int Bytes, int Warmups, int Iterations, int Chunk, ushort Cipher);
    private readonly record struct Resources(long CpuUs, long RssBytes, long PeakRssBytes, long AllocatedBytes, long HeapBytes);
    private sealed class ShutdownMeasurements
    {
        internal long ConnectionWallUs;
        internal long OwnerDisposalBegin;
    }

    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = Parse(args);
            PrintConfiguration(options);
            var shutdown = new ShutdownMeasurements();
            await RunAsync(options, shutdown);
            // RunAsync's enclosing await-using scopes have now also drained
            // configuration, registration and runtime, followed by credentials.
            long disposalUs = ElapsedUs(shutdown.OwnerDisposalBegin);
            Console.WriteLine("{\"metric\":\"shutdown\",\"scope\":\"" +
                (options.Server ? "server_peer_close_wait" : "client_requested_close") +
                "\",\"wall_us\":" + shutdown.ConnectionWallUs + "}");
            Console.WriteLine("{\"metric\":\"owner_disposal\",\"scope\":\"complete_local_owners_after_shutdown\",\"wall_us\":" + disposalUs + "}");
            PrintResources("drained", -1, SampleResources());
            Console.WriteLine("{\"passed\":true,\"clean_close\":true}");
            return 0;
        }
        catch (QuicTransportException error)
        {
            Console.Error.WriteLine($"QUIC status={error.Status} transport_error={error.TransportError} tls_alert={error.TlsAlert}");
            Console.Error.WriteLine(error);
        }
        catch (Exception error) { Console.Error.WriteLine(error); }
        Console.WriteLine("{\"passed\":false,\"clean_close\":false}");
        return 1;
    }

    private static Options Parse(string[] args)
    {
        if (args.Length != 12 || args[0] is not ("client" or "server"))
            throw new ArgumentException("endpoint client|server certificate-or-root key-or-server-name ip port ready-file bytes warmups iterations chunk-bytes pipeline cipher128|256");
        int Number(int index) => int.Parse(args[index], CultureInfo.InvariantCulture);
        bool server = args[0] == "server";
        int port = Number(4), bytes = Number(6), warmups = Number(7), iterations = Number(8), chunk = Number(9);
        if (port is < 0 or > 65535 || (!server && port == 0) || bytes is < 16777216 or > 268435456 ||
            warmups is < 1 or > 4 || iterations is < 1 or > 16 || chunk is < 4096 or > 4194304 || Number(10) != 1 || args[11] is not ("128" or "256"))
            throw new ArgumentOutOfRangeException(nameof(args), "Require16..256MiB,1..4warmups,1..16iterations,4KiB..4MiBchunks,pipeline1,cipher128/256 and a valid port.");
        return new(server, args[1], args[2], new(IPAddress.Parse(args[3]), port), args[5], bytes, warmups, iterations, chunk,
            args[11] == "128" ? (ushort)0x1301 : (ushort)0x1302);
    }

    private static async Task RunAsync(Options options, ShutdownMeasurements shutdown)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        CancellationToken cancellation = timeout.Token;
        using var certificate = options.Server ? X509Certificate2.CreateFromPemFile(options.Certificate, options.KeyOrName)
            : X509CertificateLoader.LoadCertificateFromFile(options.Certificate);
        using var credentials = options.Server ? QuicCredentials.Server(certificate, cipherSuite: options.Cipher)
            : QuicCredentials.Client([certificate], X509RevocationMode.NoCheck, options.Cipher);
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 }, cancellation);
        string revision = runtime.GetLibrarySourceRevision();
        string? required = Environment.GetEnvironmentVariable("DOTCC_REQUIRED_SOURCE_REVISION");
        if (required != null && revision != required) throw new IOException("Compiled source revision mismatch.");
        Console.WriteLine("{\"metric\":\"identity\",\"implementation\":\"managed-owning\",\"aot\":" +
            (!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false") + ",\"source_revision\":\"" +
            System.Text.Json.JsonEncodedText.Encode(revision) + "\",\"tls_provider\":\"" + runtime.GetTlsProvider() + "\"}");
        await using var registration = await runtime.OpenRegistrationAsync("dotcc-benchmark", cancellation);
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 32, PeerUnidiStreamCount = 0,
            StreamRecvWindowDefault = 1048576, StreamRecvBufferDefault = 1048576,
            ConnFlowControlWindow = 8388608, SendBufferingEnabled = false,
            PacingEnabled = true, EcnEnabled = false, EncryptionOffloadAllowed = false,
            IdleTimeoutMs = 60000, HandshakeIdleTimeoutMs = 10000,
            ServerResumptionLevel = QuicServerResumption.Disabled
        };
        await using var configuration = await registration.CreateConfigurationAsync([Alpn], credentials, settings, cancellation);
        QuicListener? listener = null;
        QuicConnection? connection = null;
        try
        {
            long begin;
            if (options.Server)
            {
                listener = await registration.ListenAsync(configuration, options.Endpoint, cancellation);
                await File.WriteAllTextAsync(options.ReadyFile, listener.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture) + "\n", cancellation);
                begin = Stopwatch.GetTimestamp();
                connection = await listener.AcceptConnectionAsync(cancellation);
            }
            else
            {
                begin = Stopwatch.GetTimestamp();
                connection = await registration.ConnectAsync(configuration, options.KeyOrName, options.Endpoint, cancellation);
            }
            long connectUs = ElapsedUs(begin);
            var handshake = connection.GetHandshakeInformation();
            if (connection.GetProtocolVersion() != 1 || handshake.NamedGroup != QuicTlsNamedGroup.Secp256R1 ||
                (ushort)handshake.CipherSuite != options.Cipher || !connection.NegotiatedApplicationProtocol.Span.SequenceEqual(Alpn))
                throw new IOException("Negotiated benchmark profile mismatch.");
            Console.WriteLine("{\"metric\":\"handshake\",\"scope\":\"" + (options.Server ? "server_accept_wait" : "client_connect_api") +
                "\",\"wall_us\":" + connectUs + ",\"quic_version\":1,\"group\":23,\"cipher\":" + options.Cipher + "}");
            PrintResources("idle", -1, SampleResources());
            for (int index = 0; index < options.Warmups + options.Iterations; index++)
                await TransferAsync(connection, options, index, cancellation);
            long shutdownBegin = Stopwatch.GetTimestamp();
            if (options.Server)
            {
                while (connection.CloseInfo == null) await Task.Delay(1, cancellation);
                var close = connection.CloseInfo!;
                if (close.Status != 0 || close.ErrorCode != 0 || !close.ApplicationInitiated)
                    throw new IOException("Peer did not close the benchmark connection cleanly.");
            }
            await connection.ShutdownAsync(cancellationToken: cancellation);
            shutdown.ConnectionWallUs = ElapsedUs(shutdownBegin);
            shutdown.OwnerDisposalBegin = Stopwatch.GetTimestamp();
        }
        finally
        {
            if (connection != null) await connection.DisposeAsync();
            if (listener != null) await listener.DisposeAsync();
        }
    }

    private static async Task TransferAsync(QuicConnection connection, Options options, int index, CancellationToken outer)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(outer);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        CancellationToken cancellation = timeout.Token;
        // Payload generation and allocation precede the timed region on both
        // endpoints. Each managed SendAsync still copies and pins its slice.
        byte[] payload = new byte[options.Bytes];
        for (int i = 0; i < payload.Length; i++) payload[i] = Pattern(i, options.Server, index);
        byte[] receiveBuffer = new byte[options.Chunk];
        long openBegin = Stopwatch.GetTimestamp();
        await using var stream = options.Server ? await connection.AcceptStreamAsync(cancellation)
            : await connection.OpenStreamAsync(cancellationToken: cancellation);
        long openUs = ElapsedUs(openBegin);
        var before = SampleResources();
        PrintResources("load_begin", index, before);
        long allocatedBefore = GC.GetTotalAllocatedBytes(precise: false);
        long begin = Stopwatch.GetTimestamp();
        // Start reception before waiting for the unbuffered marker's send
        // completion. Peer marker/payload may be offered in the same callback.
        var markerReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task receive = ReceiveAsync(stream, receiveBuffer, options, index, markerReceived, cancellation);
        try
        {
            await stream.SendAsync(new byte[] { ReadyMarker }, cancellationToken: cancellation);
            await markerReceived.Task.WaitAsync(cancellation);
            for (int offset = 0; offset < payload.Length; offset += options.Chunk)
            {
                int count = Math.Min(options.Chunk, payload.Length - offset);
                await stream.SendAsync(payload.AsMemory(offset, count), offset + count == payload.Length ? QuicSendOptions.Fin : QuicSendOptions.None, cancellation);
            }
            await receive;
            await stream.CompleteWritesAsync(cancellation);
        }
        catch
        {
            await stream.DisposeAsync();
            try { await receive; } catch { /* Preserve the original failed operation. */ }
            throw;
        }
        long elapsedUs = ElapsedUs(begin);
        long allocatedAfter = GC.GetTotalAllocatedBytes(precise: false);
        var after = SampleResources();
        // Only a verified transfer gets a timing record. Never report a rate
        // for a partial payload, rejected stream, missing FIN, or canceled send.
        Console.WriteLine("{\"metric\":\"transfer\",\"index\":" + index + ",\"warmup\":" + (index < options.Warmups ? "true" : "false") +
            ",\"payload_sent\":" + options.Bytes + ",\"payload_received\":" + options.Bytes + ",\"fin_received\":true,\"wall_us\":" + elapsedUs +
            ",\"stream_open_or_accept_us\":" + openUs + ",\"cpu_us\":" + (after.CpuUs - before.CpuUs) +
            ",\"managed_allocated_bytes\":" + (allocatedAfter - allocatedBefore) + "}");
        PrintResources("load_end", index, after);
    }

    private static async Task ReceiveAsync(QuicStream stream, byte[] buffer, Options options, int index,
        TaskCompletionSource markerReceived, CancellationToken cancellation)
    {
        try
        {
            int wireOffset = 0;
            while (true)
            {
                int count = await stream.ReadAsync(buffer, cancellation);
                if (count == 0) break;
                for (int i = 0; i < count; i++, wireOffset++)
                {
                    if (wireOffset > options.Bytes || buffer[i] != (wireOffset == 0 ? ReadyMarker : Pattern(wireOffset - 1, !options.Server, index)))
                        throw new IOException("Benchmark payload or readiness marker mismatch.");
                    if (wireOffset == 0) markerReceived.TrySetResult();
                }
            }
            if (wireOffset != options.Bytes + 1) throw new IOException("Benchmark FIN arrived with incomplete payload.");
        }
        catch (Exception error) { markerReceived.TrySetException(error); throw; }
    }

    private static byte Pattern(int offset, bool server, int transfer) => unchecked((byte)(offset * 31 + (server ? 83 : 17) + transfer * 7));
    private static long ElapsedUs(long begin) => (long)(Stopwatch.GetElapsedTime(begin).TotalMicroseconds);
    private static Resources SampleResources()
    {
        using var process = Process.GetCurrentProcess();
        process.Refresh();
        return new(process.TotalProcessorTime.Ticks / 10, process.WorkingSet64, process.PeakWorkingSet64,
            GC.GetTotalAllocatedBytes(precise: false), GC.GetTotalMemory(forceFullCollection: false));
    }
    private static void PrintResources(string phase, int index, Resources sample) => Console.WriteLine(
        "{\"metric\":\"memory\",\"phase\":\"" + phase + "\",\"index\":" + index + ",\"rss_bytes\":" + sample.RssBytes +
        ",\"peak_rss_bytes\":" + sample.PeakRssBytes + ",\"managed_heap_bytes\":" + sample.HeapBytes + ",\"process_cpu_us\":" + sample.CpuUs + "}");
    private static void PrintConfiguration(Options options) => Console.WriteLine(
        "{\"metric\":\"configuration\",\"role\":\"" + (options.Server ? "server" : "client") + "\",\"bytes\":" + options.Bytes +
        ",\"warmups\":" + options.Warmups + ",\"iterations\":" + options.Iterations + ",\"chunk_bytes\":" + options.Chunk +
        ",\"pipeline\":1,\"transport_workers\":1,\"stream_window\":1048576,\"connection_window\":8388608,\"send_buffering\":false,\"pacing\":true,\"ecn\":false,\"encryption_offload\":false,\"alpn\":\"dotcc-bench-v1\",\"cipher\":" + options.Cipher + "}");
}
