using System.Buffers.Binary;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Managed.Transport.Api;

// Ordinary public-library consumer. The relay observes complete encrypted UDP
// datagrams; it never decrypts, changes a CID, or writes a native layout/callback.
internal static class AmplificationControls
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(25);
    private const int PayloadSize = 65537;
    // Pinned quicdef.h: QUIC_MIN_SEND_ALLOWANCE. A plateau alone is insufficient:
    // require the flight to exhaust its byte credit below this native threshold.
    private const int MinimumSendAllowance = 76;

    internal static async Task RunAsync()
    {
        using var certificates = new LargeCertificate();
        string? native = Environment.GetEnvironmentVariable("DOTCC_AMPLIFICATION_NATIVE_PEER");
        if (native != null) Check(Path.IsPathFullyQualified(native) && File.Exists(native), "native peer path must name an existing absolute executable");
        foreach (string? executable in native == null ? new string?[] { null } : new string?[] { null, native })
        foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
            await RunCase(certificates, address, cipher, executable);
    }

    private static async Task RunCase(LargeCertificate certificates, IPAddress address, ushort cipher, string? nativePath)
    {
        await using var runtime = await QuicRuntime.CreateAsync(new()
        {
            ProcessorCount = 2, Parameters = new() { RetryMemoryLimit = ushort.MaxValue }
        });
        Check(runtime.GetRetryMemoryLimit() == ushort.MaxValue, "ordinary handshake Retry threshold was not applied");
        await using var registration = await runtime.OpenRegistrationAsync("amplification-controls");
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipherSuite: cipher);
        using var serverCredentials = QuicCredentials.Server(certificates.Leaf, cipherSuite: cipher);
        var settings = new QuicSettings
        {
            PeerBidiStreamCount = 4, SendBufferingEnabled = false, InitialWindowPackets = 64,
            PacingEnabled = false, MinimumMtu = 1280, MaximumMtu = 1280,
            HandshakeIdleTimeoutMs = 15000, IdleTimeoutMs = 30000
        };
        await using var clientConfiguration = await registration.CreateConfigurationAsync(["dotcc-probe"u8.ToArray()], clientCredentials, settings);
        await using var serverConfiguration = await registration.CreateConfigurationAsync(["dotcc-probe"u8.ToArray()], serverCredentials, settings);
        await using QuicListener? listener = nativePath == null
            ? await registration.ListenAsync(serverConfiguration, new(address, 0)) : null;
        await using NativeServer? native = nativePath == null ? null : await NativeServer.Start(nativePath, certificates, address, cipher);
        await using var relay = new InitialGate(listener?.LocalEndPoint ?? native!.LocalEndPoint);
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection>? accepting = listener?.AcceptConnectionAsync(timeout.Token).AsTask();
        Task<QuicConnection> connecting = registration.ConnectAsync(clientConfiguration, "localhost", relay.LocalEndPoint, timeout.Token).AsTask();
        QuicConnection? client = null, server = null;
        Flight first = default, second = default;
        try
        {
            await relay.FirstInitial.WaitAsync(timeout.Token);
            Check(certificates.Leaf.RawData.Length > 6L * relay.InitialLength,
                "certificate does not exceed both pre-validation budgets");
            first = await Plateau(relay, connecting, accepting, expectedInitials: 1, timeout.Token);
            // Explicitly replay the SAME authenticated Initial UDP bytes. The
            // pin credits UDP receipt before packet deduplication (connection.c
            // QuicConnRecvDatagrams). No Handshake packet reaches the server,
            // so this adds receive credit without validating the source path.
            await relay.ReplayInitial(timeout.Token);
            second = await Plateau(relay, connecting, accepting, expectedInitials: 2, timeout.Token);
            Check(second.ServerBytes > first.ServerBytes &&
                second.ServerBytes - first.ServerBytes >= 3L * relay.InitialLength - MinimumSendAllowance,
                "second Initial did not release another substantial real server flight");
            Check(relay.InitialCopiesForwarded == 2 && relay.OtherClientPacketsForwarded == 0,
                "non-Initial client traffic escaped before validation gate release");
            await relay.Open(timeout.Token);
            client = await connecting;
            if (accepting != null) server = await accepting;
            Check(!client.GetStatistics().StatelessRetry, "Retry validated the address instead of exercising amplification protection");
            if (server != null)
            {
                Check(!server.GetStatistics().StatelessRetry, "unexpected server Retry statistic");
                await Task.WhenAll(ClientExchange(client, timeout.Token), ServerExchange(server, timeout.Token));
            }
            else await ClientExchange(client, timeout.Token);
            await client.ShutdownAsync().WaitAsync(timeout.Token);
            if (native != null) await native.VerifyExit(cipher, address, timeout.Token);
            relay.ThrowIfFailed();
        }
        finally
        {
            timeout.Cancel();
            // Adopt a connection that finished between an assertion and cleanup.
            // Registration disposal also owns unclaimed accepted connections.
            try { client ??= await connecting; } catch (Exception) when (timeout.IsCancellationRequested) { }
            if (client != null) await client.DisposeAsync().AsTask().WaitAsync(Limit);
            if (server != null) await server.DisposeAsync().AsTask().WaitAsync(Limit);
            if (accepting != null && accepting.IsCompleted)
            {
                try { var orphan = await accepting; if (server == null) await orphan.DisposeAsync(); }
                catch (OperationCanceledException) { }
            }
        }
        await relay.DisposeAsync();
        if (native != null) await native.DisposeAsync();
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        // Public disposal itself checks the host drain; this consumer has no
        // friend/internal access to resource registries or native path flags.
        EmitEvidence(address, cipher, nativePath, native, certificates, relay, first, second);
        Console.WriteLine($"PASS endpoint amplification cipher=0x{cipher:x4} family={address.AddressFamily} server={(nativePath == null ? "managed" : "native")}");
    }

    private static async Task<Flight> Plateau(InitialGate relay, Task connecting, Task? accepting, int expectedInitials, CancellationToken cancellation)
    {
        while (true)
        {
            relay.ThrowIfFailed();
            if (connecting.IsFaulted) await connecting;
            if (accepting?.IsFaulted == true) await accepting;
            Check(!connecting.IsCompleted && accepting?.IsCompleted != true, "handshake completed/failed before opening the validation gate");
            Flight flight = relay.Snapshot();
            Check(flight.ClientBytes == (long)expectedInitials * relay.InitialLength, "unexpected credited client byte count");
            if (flight.ServerBytes > 0 && 3 * flight.ClientBytes - flight.ServerBytes < MinimumSendAllowance)
            {
                // This observation is bounded and supports the exhausted-credit
                // and released-flight proof; quiet time alone never earns PASS.
                await Task.Delay(150, cancellation);
                Flight settled = relay.Snapshot();
                relay.ThrowIfFailed();
                Check(settled == flight && !connecting.IsCompleted && accepting?.IsCompleted != true,
                    "unvalidated server flight did not plateau at its exhausted credit");
                return settled;
            }
            await Task.Delay(1, cancellation);
        }
    }

    private static async Task ClientExchange(QuicConnection client, CancellationToken cancellation)
    {
        await using var stream = await client.OpenStreamAsync().AsTask().WaitAsync(cancellation);
        Task send = stream.SendAsync(Payload(server: false), QuicSendOptions.Fin).AsTask();
        await ReadExact(stream, Payload(server: true), cancellation);
        await send.WaitAsync(cancellation);
        await stream.CompleteWritesAsync().AsTask().WaitAsync(cancellation);
    }
    private static async Task ServerExchange(QuicConnection server, CancellationToken cancellation)
    {
        await using var stream = await server.AcceptStreamAsync(cancellation);
        await ReadExact(stream, Payload(server: false), cancellation);
        await stream.SendAsync(Payload(server: true), QuicSendOptions.Fin).AsTask().WaitAsync(cancellation);
        await stream.CompleteWritesAsync().AsTask().WaitAsync(cancellation);
    }
    private static async Task ReadExact(QuicStream stream, byte[] expected, CancellationToken cancellation)
    {
        byte[] buffer = new byte[4096]; int offset = 0;
        while (true)
        {
            int length = await stream.ReadAsync(buffer, cancellation);
            if (length == 0) break;
            Check(length <= expected.Length - offset && buffer.AsSpan(0, length).SequenceEqual(expected.AsSpan(offset, length)), "payload corruption/duplication");
            offset += length;
        }
        Check(offset == expected.Length, "FIN arrived without the exact payload");
    }
    private static byte[] Payload(bool server)
    {
        byte[] bytes = new byte[PayloadSize];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)(i * 31 + 17 + (server ? 29 : 0)));
        return bytes;
    }

    private static void EmitEvidence(IPAddress address, ushort cipher, string? nativePath, NativeServer? native, LargeCertificate certificates,
        InitialGate relay, Flight first, Flight second)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer))
        {
            json.WriteStartObject(); json.WriteString("control", "pre-validation-amplification");
            json.WriteString("family", address.AddressFamily.ToString()); json.WriteNumber("cipher", cipher);
            json.WriteString("server", nativePath == null ? "managed" : "native");
            json.WriteNumber("certificate_der_bytes", certificates.Leaf.RawData.Length);
            json.WriteString("certificate_sha256", Hex(certificates.Leaf.RawData));
            json.WriteString("initial_sha256", relay.InitialHash); json.WriteNumber("initial_udp_bytes", relay.InitialLength);
            json.WriteNumber("first_received_udp_bytes", first.ClientBytes); json.WriteNumber("first_sent_udp_bytes", first.ServerBytes);
            json.WriteNumber("second_received_udp_bytes", second.ClientBytes); json.WriteNumber("second_sent_udp_bytes", second.ServerBytes);
            json.WriteNumber("initial_network_copies", 2); json.WriteBoolean("second_initial_is_unchanged_network_duplicate", true);
            json.WriteNumber("non_initial_client_datagrams_before_release", 0); json.WriteNumber("plateau_observation_ms", 150);
            json.WriteNumber("bounded_client_queue_peak", relay.PeakQueue);
            json.WriteBoolean("retry_used", false); json.WriteBoolean("handshake_incomplete_before_release", true);
            json.WriteBoolean("three_times_bound_every_observation", true); json.WriteNumber("payload_bytes_each_direction", PayloadSize);
            json.WriteBoolean("fin_each_direction", true); json.WriteBoolean("public_disposal_completed", true);
            json.WriteString("comparison_scope", "3x pre-validation byte bound and credit release; flight layout and timing are not compared");
            json.WriteStartObject("managed_client_settings");
            json.WriteNumber("initial_window_packets", 64); json.WriteBoolean("pacing_enabled", false);
            json.WriteNumber("minimum_mtu", 1280); json.WriteNumber("maximum_mtu", 1280); json.WriteEndObject();
            json.WriteStartObject("server_settings");
            json.WriteString("source", native == null ? "explicit managed configuration" : "existing native peer configuration and defaults, unchanged by this harness");
            if (native == null)
            {
                json.WriteNumber("initial_window_packets", 64); json.WriteBoolean("pacing_enabled", false);
                json.WriteNumber("minimum_mtu", 1280); json.WriteNumber("maximum_mtu", 1280);
            }
            json.WriteEndObject();
            if (nativePath != null && native != null)
            {
                json.WriteString("native_binary_sha256", Hex(File.ReadAllBytes(nativePath)));
                json.WriteString("native_stdout", native.RecordedStandardOutput);
                json.WriteString("native_stderr", native.RecordedStandardError);
                json.WriteString("native_terminal_json", native.RecordedTerminalJson);
                json.WriteString("native_stdout_sha256", Hex(Encoding.UTF8.GetBytes(native.RecordedStandardOutput)));
                json.WriteString("native_stderr_sha256", Hex(Encoding.UTF8.GetBytes(native.RecordedStandardError)));
                json.WriteString("native_terminal_json_sha256", Hex(Encoding.UTF8.GetBytes(native.RecordedTerminalJson)));
            }
            json.WriteEndObject();
        }
        Console.WriteLine("EVIDENCE endpoint " + Encoding.UTF8.GetString(buffer.ToArray()));
    }
    private static string Hex(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Amplification control: " + message); }
    private readonly record struct Flight(long ClientBytes, long ServerBytes);

    private sealed class InitialGate : IAsyncDisposable
    {
        private readonly Socket socket;
        private readonly IPEndPoint server;
        private IPEndPoint? client;
        private readonly CancellationTokenSource stop = new();
        private readonly object gate = new();
        private readonly SemaphoreSlim sends = new(1, 1);
        private readonly Queue<byte[]> queued = new();
        private readonly TaskCompletionSource first = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Task forwarding;
        private byte[]? initial;
        private string? initialHash;
        private long clientBytes, serverBytes;
        private bool opened, validationReleased, disposed;
        internal int InitialCopiesForwarded { get; private set; }
        internal int OtherClientPacketsForwarded { get; private set; }
        internal int PeakQueue { get; private set; }
        internal int InitialLength => initial?.Length ?? 0;
        internal string InitialHash => initialHash!;
        internal Task FirstInitial => first.Task;
        internal IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint!;
        internal InitialGate(IPEndPoint server)
        {
            this.server = server; socket = new(server.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(server.Address, 0)); forwarding = Forward();
        }
        internal Flight Snapshot() { lock (gate) return new(clientBytes, serverBytes); }
        internal void ThrowIfFailed() { if (forwarding.IsFaulted) forwarding.GetAwaiter().GetResult(); }
        private async Task Forward()
        {
            byte[] bytes = new byte[65536];
            EndPoint any = new IPEndPoint(server.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
            try
            {
                while (true)
                {
                    var received = await socket.ReceiveFromAsync(bytes, SocketFlags.None, any, stop.Token);
                    Check(received.ReceivedBytes is > 0 and <= 4096, "unexpected UDP size");
                    var source = (IPEndPoint)received.RemoteEndPoint;
                    if (source.Equals(server))
                    {
                        lock (gate)
                        {
                            serverBytes += received.ReceivedBytes;
                            Check(validationReleased || serverBytes <= 3 * clientBytes, "server exceeded 3x amplification limit");
                        }
                        Check(client != null, "unsolicited server traffic before Initial");
                        await Send(bytes.AsMemory(0, received.ReceivedBytes), client!, stop.Token);
                        continue;
                    }
                    bool sendFirst = false;
                    byte[] copy = bytes.AsSpan(0, received.ReceivedBytes).ToArray();
                    lock (gate)
                    {
                        Check(client == null || client.Equals(source), "relay received an unrelated source");
                        client = source;
                        if (initial == null)
                        {
                            Check(copy.Length >= 1200 && InitialOnly(copy), "first client UDP is not exclusively v1 Initial packets");
                            initial = copy; initialHash = Hex(copy); sendFirst = true;
                        }
                        else if (!opened)
                        {
                            Check(queued.Count < 64, "client validation gate queue overflow");
                            queued.Enqueue(copy); PeakQueue = Math.Max(PeakQueue, queued.Count); continue;
                        }
                    }
                    await CreditAndSend(copy, sendFirst, stop.Token);
                    if (sendFirst) first.TrySetResult();
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
            catch (Exception error) { first.TrySetException(error); throw; }
        }
        private async Task CreditAndSend(byte[] bytes, bool initialCopy, CancellationToken cancellation)
        {
            await sends.WaitAsync(cancellation);
            try
            {
                // Account before the send can make a peer response observable.
                // Any failed/partial UDP send fails the complete control.
                lock (gate)
                {
                    clientBytes += bytes.Length;
                    if (initialCopy) InitialCopiesForwarded++; else OtherClientPacketsForwarded++;
                }
                await Send(bytes, server, cancellation);
            }
            finally { sends.Release(); }
        }
        internal async Task ReplayInitial(CancellationToken cancellation)
        {
            byte[] bytes;
            lock (gate)
            {
                Check(!opened && InitialCopiesForwarded == 1 && initial != null, "invalid Initial replay state");
                bytes = initial!; Check(InitialOnly(bytes) && Hex(bytes) == initialHash, "retained Initial UDP bytes changed");
            }
            await CreditAndSend(bytes, initialCopy: true, cancellation);
        }
        internal async Task Open(CancellationToken cancellation)
        {
            lock (gate)
            {
                Check(!validationReleased && InitialCopiesForwarded == 2 && OtherClientPacketsForwarded == 0,
                    "invalid validation gate release state");
                validationReleased = true;
            }
            while (true)
            {
                byte[] bytes;
                lock (gate)
                {
                    if (queued.Count == 0) { opened = true; return; }
                    // Validation is now intentionally allowed. Keep new client
                    // arrivals queued while draining the existing FIFO.
                    bytes = queued.Dequeue();
                }
                await CreditAndSend(bytes, initialCopy: false, cancellation);
            }
        }
        private async Task Send(ReadOnlyMemory<byte> bytes, IPEndPoint target, CancellationToken cancellation)
        { Check(await socket.SendToAsync(bytes, SocketFlags.None, target, cancellation) == bytes.Length, "partial UDP forwarding send"); }
        public async ValueTask DisposeAsync()
        {
            if (disposed) return; disposed = true;
            stop.Cancel(); socket.Dispose();
            try { await forwarding.WaitAsync(Limit); }
            finally { lock (gate) queued.Clear(); stop.Dispose(); sends.Dispose(); }
        }
        private static bool InitialOnly(ReadOnlySpan<byte> bytes)
        {
            int offset = 0;
            try
            {
                while (offset < bytes.Length)
                {
                    if (bytes.Length - offset < 7 || (bytes[offset] & 0xf0) != 0xc0 ||
                        BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset + 1, 4)) != 1) return false;
                    offset += 5;
                    int destination = bytes[offset++]; if (destination > 20) return false; offset += destination;
                    int source = bytes[offset++]; if (source > 20) return false; offset += source;
                    ulong tokenLength = VarInt(bytes, ref offset); if (tokenLength != 0) return false;
                    ulong length = VarInt(bytes, ref offset);
                    if (length < 17 || length > (ulong)(bytes.Length - offset)) return false;
                    offset += checked((int)length);
                }
                return offset == bytes.Length;
            }
            catch (ArgumentOutOfRangeException) { return false; }
            catch (IndexOutOfRangeException) { return false; }
        }
        private static ulong VarInt(ReadOnlySpan<byte> bytes, ref int offset)
        {
            byte first = bytes[offset++]; int length = 1 << (first >> 6); ulong value = (uint)(first & 63);
            for (int i = 1; i < length; i++) value = (value << 8) | bytes[offset++];
            return value;
        }
    }

    private sealed class LargeCertificate : IDisposable
    {
        internal X509Certificate2 Root { get; }
        internal X509Certificate2 Leaf { get; }
        internal LargeCertificate()
        {
            using var rootKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var root = new CertificateRequest("CN=Amplification root", rootKey, HashAlgorithmName.SHA256);
            root.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            root.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
            Root = root.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(2));
            using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var leaf = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256);
            leaf.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            leaf.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            leaf.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
            var names = new SubjectAlternativeNameBuilder(); names.AddDnsName("localhost");
            names.AddIpAddress(IPAddress.Loopback); names.AddIpAddress(IPAddress.IPv6Loopback);
            leaf.CertificateExtensions.Add(names.Build());
            var extension = new AsnWriter(AsnEncodingRules.DER); extension.WriteOctetString(RandomNumberGenerator.GetBytes(16384));
            leaf.CertificateExtensions.Add(new X509Extension("1.3.6.1.4.1.55555.7.1", extension.Encode(), critical: false));
            using var signed = leaf.Create(Root, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), RandomNumberGenerator.GetBytes(16));
            Leaf = signed.CopyWithPrivateKey(key);
        }
        public void Dispose() { Leaf.Dispose(); Root.Dispose(); }
    }

    private sealed class NativeServer : IAsyncDisposable
    {
        private readonly string directory;
        private readonly Process process;
        private readonly Task<string> stdout, stderr;
        private bool disposed;
        internal string RecordedStandardOutput { get; private set; } = "";
        internal string RecordedStandardError { get; private set; } = "";
        internal string RecordedTerminalJson { get; private set; } = "";
        internal IPEndPoint LocalEndPoint { get; private set; } = null!;
        private NativeServer(string executable, LargeCertificate certificates, IPAddress address, ushort cipher)
        {
            directory = Path.Combine(Path.GetTempPath(), "dotcc-amplification-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "leaf.pem"), certificates.Leaf.ExportCertificatePem());
            File.WriteAllText(Path.Combine(directory, "root.pem"), certificates.Root.ExportCertificatePem());
            using (var key = certificates.Leaf.GetECDsaPrivateKey()!)
            {
                var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write };
                if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                using var stream = new FileStream(Path.Combine(directory, "leaf.key"), options);
                using var writer = new StreamWriter(stream); writer.Write(key.ExportPkcs8PrivateKeyPem());
            }
            var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (string argument in new[] { Path.Combine(directory, "leaf.pem"), Path.Combine(directory, "leaf.key"),
                cipher == 0x1301 ? "128" : "256", address.AddressFamily == AddressFamily.InterNetwork ? "ipv4" : "ipv6",
                Path.Combine(directory, "root.pem"), "localhost", "server", "0", Path.Combine(directory, "ready") }) start.ArgumentList.Add(argument);
            start.Environment.Remove("SSLKEYLOGFILE"); start.Environment.Remove("DOTCC_PEER_PROXY_DRAIN");
            start.Environment["DOTCC_PEER_SETTLE_MS"] = "0";
            string? openssl = Environment.GetEnvironmentVariable("DOTCC_AMPLIFICATION_NATIVE_OPENSSL_CONF");
            if (openssl != null) start.Environment["OPENSSL_CONF"] = openssl;
            process = Process.Start(start) ?? throw new InvalidOperationException("native process did not start");
            stdout = process.StandardOutput.ReadToEndAsync(); stderr = process.StandardError.ReadToEndAsync();
        }
        internal static async Task<NativeServer> Start(string executable, LargeCertificate certificates, IPAddress address, ushort cipher)
        {
            var server = new NativeServer(executable, certificates, address, cipher);
            try
            {
                using var timeout = new CancellationTokenSource(Limit);
                string ready = Path.Combine(server.directory, "ready");
                while (!File.Exists(ready) || string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(ready, timeout.Token)))
                {
                    Check(!server.process.HasExited, "native server exited before readiness: " + (server.process.HasExited ? await server.stderr : ""));
                    await Task.Delay(10, timeout.Token);
                }
                int port = int.Parse(await File.ReadAllTextAsync(ready, timeout.Token));
                Check(port is > 0 and <= 65535, "invalid native server ready port");
                server.LocalEndPoint = new(address, port); return server;
            }
            catch { await server.DisposeAsync(); throw; }
        }
        internal async Task VerifyExit(ushort cipher, IPAddress address, CancellationToken cancellation)
        {
            await process.WaitForExitAsync(cancellation);
            string output = await stdout; string errors = await stderr;
            RecordedStandardOutput = output; RecordedStandardError = errors;
            Check(process.ExitCode == 0, "native server failed: " + errors);
            string[] rows = output.Split('\n').Where(line => line.StartsWith("{\"passed\":", StringComparison.Ordinal)).ToArray();
            Check(rows.Length == 1, "missing/ambiguous native server result");
            RecordedTerminalJson = rows[0];
            using var document = JsonDocument.Parse(rows[0]); var result = document.RootElement;
            Check(result.GetProperty("passed").GetBoolean() && result.GetProperty("server_bytes").GetInt32() == PayloadSize &&
                result.GetProperty("core_sent_stream_bytes").GetUInt64() >= PayloadSize &&
                result.GetProperty("connected").GetInt32() == 1 && result.GetProperty("finished").GetInt32() == 1 && result.GetProperty("closed").GetInt32() == 1 &&
                result.GetProperty("transport_status").GetUInt32() == 0 && result.GetProperty("transport_error").GetUInt64() == 0 &&
                result.GetProperty("peer_error").GetUInt64() == 0 && result.GetProperty("statistics_status").GetUInt32() == 0 &&
                result.GetProperty("cipher").GetInt32() == cipher && result.GetProperty("group").GetInt32() == 23 && result.GetProperty("quic_version").GetInt32() == 1 &&
                result.GetProperty("family").GetString() == (address.AddressFamily == AddressFamily.InterNetwork ? "ipv4" : "ipv6"),
                "native server payload/FIN/lifecycle/profile mismatch");
        }
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync().WaitAsync(Limit); process.Dispose();
            Directory.Delete(directory, recursive: true);
        }
    }
}
