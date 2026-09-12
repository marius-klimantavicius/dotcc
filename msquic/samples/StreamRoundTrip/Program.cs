using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// An ordinary external consumer: no generated types, native pointers, friend
// assembly name, source linking, reflection, or transport callback machinery.
internal static class Program
{
    private const int PayloadLength = 65537;
    private static byte[] Alpn = "dotcc-public-sample"u8.ToArray();
    private static string processRole = "unknown";
    private static long applicationBytesSent, applicationBytesReceived;

    private static async Task<int> Main(string[] args)
    {
        try
        {
            if (args is ["certificates", string directory])
            {
                SampleCertificates.Create(directory);
                return 0;
            }
            if (args.Length == 0) return Usage();
            if (args[^1].StartsWith("--alpn=", StringComparison.Ordinal))
            {
                string protocol = args[^1][7..];
                if (protocol.Length is < 1 or > 255 || protocol.Any(c => c is < '!' or > '~')) return Usage();
                Alpn = System.Text.Encoding.ASCII.GetBytes(protocol);
                args = args[..^1];
                if (args.Length == 0) return Usage();
            }
            bool server = args[0] == "server";
            if ((!server && args[0] != "client") ||
                (server ? args.Length is < 6 or > 7 : args.Length is < 5 or > 6)) return Usage();
            processRole = args[0];
            int port = int.Parse(args[4], CultureInfo.InvariantCulture);
            if (port is < 0 or > 65535 || (!server && port == 0)) return Usage();
            int roundsIndex = server ? 6 : 5;
            int rounds = args.Length > roundsIndex ? int.Parse(args[roundsIndex], CultureInfo.InvariantCulture) : 2;
            if (rounds is < 1 or > 32) return Usage();
            var endpoint = new IPEndPoint(IPAddress.Parse(args[3]), port);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
            Console.CancelKeyPress += cancel;
            try
            {
                if (server) await RunServerAsync(args[1], args[2], endpoint, args[5], rounds, cancellation.Token);
                else await RunClientAsync(args[1], args[2], endpoint, rounds, cancellation.Token);
            }
            finally { Console.CancelKeyPress -= cancel; }
            // Printed only after every owner (including the runtime) has drained.
            Console.WriteLine("{\"passed\":true,\"clean_close\":true,\"role\":\"" + args[0] + "\",\"connections\":" + rounds + "}");
            return 0;
        }
        catch (QuicStreamAbortedException error)
        {
            Console.Error.WriteLine($"Stream aborted: application_error={error.ApplicationErrorCode}");
            return 1;
        }
        catch (QuicTransportException error)
        {
            // TLS alerts retain their actual transport code; authentication
            // failure never becomes an accepted connection or a timeout pass.
            Console.Error.WriteLine($"QUIC failure: status={error.Status} transport_error={error.TransportError} tls_alert={error.TlsAlert}: {error.Message}");
            Console.WriteLine("{\"passed\":false,\"clean_close\":true,\"role\":\"" + processRole + "\",\"error_kind\":\"transport\",\"status\":" + error.Status +
                ",\"transport_error\":" + (error.TransportError?.ToString(CultureInfo.InvariantCulture) ?? "null") +
                ",\"tls_alert\":" + (error.TlsAlert?.ToString(CultureInfo.InvariantCulture) ?? "null") +
                ",\"application_bytes_sent\":" + applicationBytesSent + ",\"application_bytes_received\":" + applicationBytesReceived + "}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Canceled or timed out; pending owners were disposed.");
            Console.WriteLine("{\"passed\":false,\"clean_close\":true,\"role\":\"" + processRole + "\",\"error_kind\":\"canceled\",\"application_bytes_sent\":" +
                applicationBytesSent + ",\"application_bytes_received\":" + applicationBytesReceived + "}");
            return 1;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    private static int Usage()
    {
        Console.Error.WriteLine("PublicQuicSample certificates <new-directory>\n" +
            "PublicQuicSample server <certificate.pem> <private-key.pem> <ip> <port-or-0> <ready-file> [connections=2] [--alpn=protocol]\n" +
            "PublicQuicSample client <trust-root.pem> <server-name> <ip> <port> [connections=2] [--alpn=protocol]");
        return 2;
    }

    private static QuicSettings Settings(bool server) => new()
    {
        PeerBidiStreamCount = 4,
        PeerUnidiStreamCount = 4,
        IdleTimeoutMs = 30000,
        HandshakeIdleTimeoutMs = 10000,
        ServerResumptionLevel = server ? QuicServerResumption.Tickets : QuicServerResumption.Disabled
    };

    private static async Task RunServerAsync(string certificatePath, string keyPath, IPEndPoint endpoint,
        string readyFile, int rounds, CancellationToken cancellationToken)
    {
        using var certificate = X509Certificate2.CreateFromPemFile(certificatePath, keyPath);
        using var credentials = QuicCredentials.Server(certificate);
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 }, cancellationToken);
        CheckRuntime(runtime);
        await using var registration = await runtime.OpenRegistrationAsync("public-sample-server", cancellationToken);
        await using var configuration = await registration.CreateConfigurationAsync([Alpn], credentials, Settings(true), cancellationToken);
        await using var listener = await registration.ListenAsync(configuration, endpoint, cancellationToken);
        // The same configuration owns the ticket keys across all connections.
        // A real deployment can explicitly rotate them with SetTicketKeys.
        await File.WriteAllTextAsync(readyFile, listener.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture) + "\n", cancellationToken);
        for (int round = 0; round < rounds; round++)
        {
            await using var connection = await listener.AcceptConnectionAsync(cancellationToken);
            ValidateConnection(connection, round);
            connection.SendResumptionTicket("public-sample-v1"u8);
            await using var stream = await connection.AcceptStreamAsync(cancellationToken);
            await ReadPayloadAsync(stream, server: false, cancellationToken);
            applicationBytesSent += PayloadLength;
            await stream.SendAsync(Payload(server: true), QuicSendOptions.Fin, cancellationToken);
            await stream.CompleteWritesAsync(cancellationToken);
            // The client's FIN follows validation of our response and receipt
            // of its ticket, so neither can be cut short by local shutdown.
            await RequireFinAsync(stream, cancellationToken);
            while (connection.CloseInfo == null) await Task.Delay(5, cancellationToken);
            var close = connection.CloseInfo!;
            if (close.Status != 0 || close.ErrorCode != 0 || !close.ApplicationInitiated)
                throw new IOException($"Unexpected peer close: status={close.Status}, error={close.ErrorCode}");
            await connection.ShutdownAsync(cancellationToken: cancellationToken);
            ReportRound("server", round, connection);
        }
        await listener.StopAsync(cancellationToken);
    }

    private static async Task RunClientAsync(string rootPath, string serverName, IPEndPoint endpoint,
        int rounds, CancellationToken cancellationToken)
    {
        using var root = X509CertificateLoader.LoadCertificateFromFile(rootPath);
        // NoCheck is explicit for these locally issued, short-lived sample
        // certificates. Trust chain and server-name validation still apply.
        using var credentials = QuicCredentials.Client([root], X509RevocationMode.NoCheck);
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 }, cancellationToken);
        CheckRuntime(runtime);
        await using var registration = await runtime.OpenRegistrationAsync("public-sample-client", cancellationToken);
        await using var configuration = await registration.CreateConfigurationAsync([Alpn], credentials, Settings(false), cancellationToken);
        ReadOnlyMemory<byte> ticket = default;
        for (int round = 0; round < rounds; round++)
        {
            // Resumption ticket bytes stay in memory, scoped to this identity,
            // trust policy and ALPN. Early data is deliberately not requested.
            await using var connection = await registration.ConnectAsync(configuration, serverName, endpoint,
                new QuicConnectOptions { ResumptionTicket = ticket }, cancellationToken);
            ValidateConnection(connection, round);
            await using var stream = await connection.OpenStreamAsync(cancellationToken: cancellationToken);
            applicationBytesSent += PayloadLength;
            await stream.SendAsync(Payload(server: false), cancellationToken: cancellationToken);
            await ReadPayloadAsync(stream, server: true, cancellationToken);
            await RequireFinAsync(stream, cancellationToken);
            ticket = (await connection.WaitForResumptionTicketAsync(cancellationToken: cancellationToken)).Bytes;
            await stream.CompleteWritesAsync(cancellationToken);
            await connection.ShutdownAsync(cancellationToken: cancellationToken);
            ReportRound("client", round, connection);
        }
    }

    private static void ValidateConnection(QuicConnection connection, int round)
    {
        if (!connection.NegotiatedApplicationProtocol.Span.SequenceEqual(Alpn) || connection.GetProtocolVersion() != 1)
            throw new IOException("Unexpected ALPN or QUIC version.");
        if (connection.GetStatistics().ResumptionSucceeded != (round != 0))
            throw new IOException("The expected fresh/resumed handshake was not observed.");
    }

    private static void CheckRuntime(QuicRuntime runtime)
    {
        var version = runtime.GetLibraryVersion();
        string revision = runtime.GetLibrarySourceRevision();
        string? required = Environment.GetEnvironmentVariable("DOTCC_REQUIRED_SOURCE_REVISION");
        if (required != null && revision != required) throw new IOException("Compiled library source revision does not match the required pin.");
        var policy = runtime.GetVersionPolicy();
        if (version.Major != 2 || version.Minor != 7 || version.Patch != 0 ||
            runtime.GetTlsProvider() != QuicTlsProvider.Picotls ||
            !policy.AcceptableVersions.Span.SequenceEqual(new uint[] { 1 }) ||
            !policy.OfferedVersions.Span.SequenceEqual(new uint[] { 1 }) ||
            !policy.FullyDeployedVersions.Span.SequenceEqual(new uint[] { 1 }))
            throw new IOException("Unexpected library, TLS provider, or effective version policy.");
        Console.WriteLine("{\"metadata\":true,\"aot\":" + (!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false") +
            ",\"source_revision\":\"" + System.Text.Json.JsonEncodedText.Encode(revision) +
            "\",\"library_version\":\"" + version.Major + "." + version.Minor + "." + version.Patch +
            "\",\"tls_provider\":\"picotls\",\"effective_versions\":[1],\"alpn\":\"" +
            System.Text.Json.JsonEncodedText.Encode(System.Text.Encoding.ASCII.GetString(Alpn)) + "\"}");
    }

    private static void ReportRound(string role, int round, QuicConnection connection)
    {
        var statistics = connection.GetStatistics();
        Console.WriteLine("{\"role\":\"" + role + "\",\"round\":" + round +
            ",\"bytes_sent\":" + PayloadLength + ",\"bytes_received\":" + PayloadLength +
            ",\"fin_received\":true,\"resumed\":" + (statistics.ResumptionSucceeded ? "true" : "false") + "}");
    }

    private static byte[] Payload(bool server)
    {
        var bytes = new byte[PayloadLength];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)(i * 31 + (server ? 83 : 17)));
        return bytes;
    }

    private static async Task ReadPayloadAsync(QuicStream stream, bool server, CancellationToken cancellationToken)
    {
        byte[] expected = Payload(server), actual = new byte[PayloadLength];
        int received = 0;
        while (received < actual.Length)
        {
            int count = await stream.ReadAsync(actual.AsMemory(received), cancellationToken);
            if (count == 0) throw new IOException("FIN arrived before the complete payload.");
            received += count;
            applicationBytesReceived += count;
        }
        if (!actual.AsSpan().SequenceEqual(expected)) throw new IOException("Peer payload differs from the expected pattern.");
    }

    private static async Task RequireFinAsync(QuicStream stream, CancellationToken cancellationToken)
    {
        if (await stream.ReadAsync(new byte[1], cancellationToken) != 0)
            throw new IOException("Unexpected bytes after the payload.");
    }
}
