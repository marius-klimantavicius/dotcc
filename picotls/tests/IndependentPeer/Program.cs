using System.Buffers.Binary;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

// Independent test oracle only. This executable never replaces translated TLS.
internal static class Program
{
    private const int MaximumFrame = 8 * 1024 * 1024;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args is ["credentials", var directory])
            {
                Credentials.Create(directory);
                Console.WriteLine("Created disposable local TLS credentials");
                return 0;
            }
            if (args.Length == 0 || args[0] is not ("server" or "client"))
                throw new ArgumentException("Use credentials DIRECTORY, or server/client --credentials DIRECTORY [--port N --identity NAME --ready FILE --bytes N --alpn NAME --target HOST --require-client-cert true --timeout N]");
            var options = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 1; i < args.Length; i += 2)
            {
                if (i + 1 == args.Length || !args[i].StartsWith("--", StringComparison.Ordinal)
                    || !options.TryAdd(args[i][2..], args[i + 1]))
                    throw new ArgumentException("Options must be unique --name value pairs");
            }
            foreach (string key in options.Keys)
                if (key is not ("credentials" or "port" or "identity" or "ready" or "bytes" or "alpn" or "target" or "require-client-cert" or "timeout"))
                    throw new ArgumentException($"Unknown option: {key}");
            string Get(string key, string fallback) => options.GetValueOrDefault(key, fallback);
            string credentials = options["credentials"];
            bool server = args[0] == "server";
            int port = int.Parse(Get("port", server ? "0" : "-1"));
            int byteCount = int.Parse(Get("bytes", "65537"));
            int seconds = int.Parse(Get("timeout", "30"));
            if (seconds is < 1 or > 300 || byteCount is < 0 or > MaximumFrame)
                throw new ArgumentOutOfRangeException(nameof(args));
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            var token = deadline.Token;
            using var root = X509CertificateLoader.LoadCertificateFromFile(Path.Combine(credentials, "ca.cer"));
            string identity = Get("identity", server ? "server-ecdsa" : "");
            using var certificate = identity.Length == 0 ? null : X509CertificateLoader.LoadPkcs12FromFile(
                Path.Combine(credentials, identity + ".pfx"), "", X509KeyStorageFlags.EphemeralKeySet);
            string target = Get("target", "localhost");
            string alpn = Get("alpn", "dotcc-picotls");
            bool requireClient = bool.Parse(Get("require-client-cert", "false"));

            using var socket = new TcpClient(AddressFamily.InterNetwork);
            TcpClient connection = socket;
            if (server)
            {
                if (certificate is null) throw new ArgumentException("Server requires an identity");
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start(1);
                try
                {
                    var ready = new Ready(((IPEndPoint)listener.LocalEndpoint).Port);
                    string json = JsonSerializer.Serialize(ready, PeerJson.Default.Ready);
                    if (options.TryGetValue("ready", out string? readyPath))
                    {
                        await File.WriteAllTextAsync(readyPath + ".tmp", json, token);
                        File.Move(readyPath + ".tmp", readyPath, overwrite: true);
                    }
                    else Console.WriteLine(json);
                    connection = await listener.AcceptTcpClientAsync(token);
                }
                finally { listener.Stop(); }
            }
            else await socket.ConnectAsync(IPAddress.Loopback, port, token);

            using (connection)
            using (var tls = new SslStream(connection.GetStream(), false, (_, peer, suppliedChain, errors) =>
                Validate(peer, suppliedChain, errors, root, server, requireClient, target)))
            {
                if (server)
                    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificateContext = SslStreamCertificateContext.Create(certificate!, new X509Certificate2Collection(root), offline: true),
                        ClientCertificateRequired = requireClient,
                        EnabledSslProtocols = SslProtocols.Tls13,
                        ApplicationProtocols = [new SslApplicationProtocol(alpn)],
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        AllowTlsResume = false
                    }, token);
                else
                    await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        TargetHost = target,
                        ClientCertificates = certificate is null ? null : new X509CertificateCollection { certificate },
                        EnabledSslProtocols = SslProtocols.Tls13,
                        ApplicationProtocols = [new SslApplicationProtocol(alpn)],
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        AllowTlsResume = false
                    }, token);
                if (tls.SslProtocol != SslProtocols.Tls13 || tls.NegotiatedApplicationProtocol != new SslApplicationProtocol(alpn))
                    throw new AuthenticationException("TLS 1.3 and expected ALPN are required");

                byte[] payload;
                if (server)
                {
                    payload = await ReadFrame(tls, token);
                    await WriteFrame(tls, payload, token);
                }
                else
                {
                    payload = new byte[byteCount];
                    for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i * 31 + 7);
                    await WriteFrame(tls, payload, token);
                    var echoed = await ReadFrame(tls, token);
                    if (!payload.AsSpan().SequenceEqual(echoed)) throw new IOException("Echo payload mismatch");
                }
                await tls.ShutdownAsync().WaitAsync(token);
                if (await tls.ReadAsync(new byte[1], token) != 0) throw new IOException("Unexpected data after echo");
                Console.WriteLine(JsonSerializer.Serialize(new Result(server ? "server" : "client",
                    tls.SslProtocol.ToString(), tls.NegotiatedCipherSuite.ToString(), tls.NegotiatedApplicationProtocol.ToString(),
                    payload.Length, Convert.ToHexString(SHA256.HashData(payload)), tls.IsMutuallyAuthenticated), PeerJson.Default.Result));
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"{error.GetType().Name}: {error.Message}");
            return 1;
        }
    }

    private static bool Validate(X509Certificate? peer, X509Chain? suppliedChain, SslPolicyErrors errors,
        X509Certificate2 root, bool server, bool requireClient, string target)
    {
        if (peer is null) return server && !requireClient;
        if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0) return false;
        using var leaf = X509CertificateLoader.LoadCertificate(peer.GetRawCertData());
        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(root);
        chain.ChainPolicy.DisableCertificateDownloads = true;
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Disposable offline fixtures only.
        chain.ChainPolicy.ApplicationPolicy.Add(new Oid(server ? "1.3.6.1.5.5.7.3.2" : "1.3.6.1.5.5.7.3.1"));
        if (suppliedChain is not null)
            foreach (var element in suppliedChain.ChainElements)
                if (!element.Certificate.RawData.AsSpan().SequenceEqual(leaf.RawData)) chain.ChainPolicy.ExtraStore.Add(element.Certificate);
        return chain.Build(leaf) && (server || leaf.MatchesHostname(target, allowWildcards: true, allowCommonName: false));
    }

    private static async Task<byte[]> ReadFrame(SslStream tls, CancellationToken token)
    {
        byte[] header = new byte[4];
        await tls.ReadExactlyAsync(header, token);
        uint length = BinaryPrimitives.ReadUInt32BigEndian(header);
        if (length > MaximumFrame) throw new IOException("Application frame exceeds limit");
        byte[] payload = new byte[(int)length];
        await tls.ReadExactlyAsync(payload, token);
        return payload;
    }

    private static async Task WriteFrame(SslStream tls, byte[] payload, CancellationToken token)
    {
        byte[] header = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)payload.Length);
        await tls.WriteAsync(header, token);
        // Split writes ensure consumers cannot assume application frames map to records.
        for (int offset = 0; offset < payload.Length; offset += 4093)
            await tls.WriteAsync(payload.AsMemory(offset, Math.Min(4093, payload.Length - offset)), token);
    }
}

internal sealed record Ready(int Port);
internal sealed record Result(string Role, string Protocol, string Cipher, string Alpn, int Bytes, string Sha256, bool MutualAuthentication);
[JsonSerializable(typeof(Ready))]
[JsonSerializable(typeof(Result))]
internal partial class PeerJson : JsonSerializerContext;
