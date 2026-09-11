using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Security;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is not ("client" or "server"))
                throw new ArgumentException("Use client/server --credentials DIRECTORY [--port N --identity NAME --ready FILE --bytes N --alpn NAME --target HOST --require-client-cert true --cipher TLS_AES_128_GCM_SHA256 --update-key true --revocation Offline --timeout N]");
            var options = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 1; i < args.Length; i += 2)
                if (i + 1 == args.Length || !args[i].StartsWith("--", StringComparison.Ordinal) || !options.TryAdd(args[i][2..], args[i + 1]))
                    throw new ArgumentException("Options must be unique --name value pairs.");
            foreach (string key in options.Keys)
                if (key is not ("credentials" or "port" or "identity" or "ready" or "bytes" or "alpn" or "target" or "require-client-cert" or "cipher" or "update-key" or "revocation" or "timeout"))
                    throw new ArgumentException("Unknown option: " + key);
            string Get(string key, string fallback) => options.GetValueOrDefault(key, fallback);
            bool server = args[0] == "server";
            int port = int.Parse(Get("port", server ? "0" : "-1"));
            int size = int.Parse(Get("bytes", "65537"));
            int seconds = int.Parse(Get("timeout", "30"));
            if (port is < 0 or > 65535 || seconds is < 1 or > 300 || size is < 0 or > 8 * 1024 * 1024)
                throw new ArgumentOutOfRangeException(nameof(args));
            ushort suite = Get("cipher", "TLS_AES_128_GCM_SHA256") switch
            {
                "TLS_AES_128_GCM_SHA256" => 0x1301,
                "TLS_AES_256_GCM_SHA384" => 0x1302,
                _ => throw new ArgumentException("Unsupported cipher suite.")
            };
            string directory = options["credentials"];
            string identity = Get("identity", server ? "server-ecdsa" : "");
            string target = Get("target", "localhost");
            bool requireClient = bool.Parse(Get("require-client-cert", "false"));
            using var root = X509CertificateLoader.LoadCertificateFromFile(Path.Combine(directory, "ca.cer"));
            using var certificate = identity.Length == 0 ? null : X509CertificateLoader.LoadPkcs12FromFile(
                Path.Combine(directory, identity + ".pfx"), "", X509KeyStorageFlags.EphemeralKeySet);
            using var signer = certificate == null ? null : new BclCryptoProvider.SigningIdentity(certificate);
            using var verifier = new BclCryptoProvider.CertificateVerifier([root], Enum.Parse<X509RevocationMode>(Get("revocation", "Offline")));
            using var configuration = new PicotlsContext(server, verifier, signer, [Get("alpn", "dotcc-picotls")], suite, requireClient);
            using var connection = configuration.CreateConnection(server ? null : target);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            using var socket = new TcpClient(AddressFamily.InterNetwork);
            TcpClient connected = socket;
            if (server)
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start(1);
                try
                {
                    string ready = JsonSerializer.Serialize(new Ready(((IPEndPoint)listener.LocalEndpoint).Port), ConsumerJson.Default.Ready);
                    if (options.TryGetValue("ready", out string? path))
                    {
                        await File.WriteAllTextAsync(path + ".tmp", ready, deadline.Token);
                        File.Move(path + ".tmp", path, overwrite: true);
                    }
                    else Console.WriteLine(ready);
                    connected = await listener.AcceptTcpClientAsync(deadline.Token);
                }
                finally { listener.Stop(); }
            }
            else await socket.ConnectAsync(IPAddress.Loopback, port, deadline.Token);
            using (connected)
            using (deadline.Token.Register(static state => ((TcpClient)state!).Dispose(), connected))
            {
                connected.NoDelay = true;
                using var stream = connected.GetStream();
                var transport = new TlsTransport(connection, stream);
                transport.Handshake();
                if (bool.Parse(Get("update-key", "false"))) stream.Write(connection.UpdateKey());
                byte[] payload;
                if (server) { payload = transport.ReadFrame(); transport.WriteFrame(payload); }
                else
                {
                    payload = new byte[size];
                    for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i * 31 + 7);
                    transport.WriteFrame(payload);
                    if (!payload.AsSpan().SequenceEqual(transport.ReadFrame())) throw new IOException("Echo payload mismatch.");
                }
                transport.Shutdown();
                Console.WriteLine(JsonSerializer.Serialize(new Result(server ? "server" : "client", "Tls13",
                    connection.CipherSuite == 0x1301 ? "TLS_AES_128_GCM_SHA256" : "TLS_AES_256_GCM_SHA384",
                    connection.NegotiatedProtocol, payload.Length, Convert.ToHexString(SHA256.HashData(payload)),
                    server && requireClient, connection.IsResumed), ConsumerJson.Default.Result));
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine($"{error.GetType().Name}: {error.Message}"); return 1; }
    }
}

/// <summary>Small bounded framing example; all TLS bytes pass through the
/// translated core. NetworkStream supplies only transport reads and writes.</summary>
internal sealed class TlsTransport(PicotlsConnection connection, NetworkStream transport)
{
    private readonly byte[] incoming = new byte[8191];
    private byte[] plaintext = [];
    private int offset;
    public void Handshake()
    {
        transport.Write(connection.Process([]).Outbound);
        while (!connection.HandshakeComplete) ReadMore();
    }
    private void ReadMore()
    {
        if (offset != plaintext.Length) throw new IOException("Consume buffered application bytes before reading more.");
        int count = transport.Read(incoming);
        if (count == 0) { connection.CompleteInput(); return; }
        PicotlsStep step;
        try { step = connection.Process(incoming.AsSpan(0, count)); }
        catch (PicotlsException error)
        {
            if (!error.AlertBytes.IsEmpty) transport.Write(error.AlertBytes.Span);
            throw;
        }
        if (step.Consumed != count) throw new IOException("TLS did not consume transport bytes.");
        transport.Write(step.Outbound);
        plaintext = step.Plaintext; offset = 0;
    }
    private void ReadExactly(Span<byte> destination)
    {
        while (!destination.IsEmpty)
        {
            if (offset == plaintext.Length)
            {
                if (connection.PeerClosed) throw new EndOfStreamException("TLS closed before the complete application frame.");
                ReadMore();
            }
            int count = Math.Min(destination.Length, plaintext.Length - offset);
            plaintext.AsSpan(offset, count).CopyTo(destination);
            offset += count; destination = destination[count..];
        }
    }
    public byte[] ReadFrame()
    {
        Span<byte> header = stackalloc byte[4]; ReadExactly(header);
        uint count = BinaryPrimitives.ReadUInt32BigEndian(header);
        if (count > 8 * 1024 * 1024) throw new IOException("Application frame exceeds limit.");
        byte[] output = new byte[(int)count]; ReadExactly(output); return output;
    }
    public void WriteFrame(ReadOnlySpan<byte> payload)
    {
        Span<byte> header = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(header, (uint)payload.Length);
        transport.Write(connection.Send(header));
        while (!payload.IsEmpty)
        {
            int count = Math.Min(4093, payload.Length);
            transport.Write(connection.Send(payload[..count])); payload = payload[count..];
        }
    }
    public void Shutdown()
    {
        transport.Write(connection.CloseNotify());
        if (offset != plaintext.Length) throw new IOException("Unexpected data after echo.");
        while (!connection.PeerClosed)
        {
            ReadMore();
            if (plaintext.Length != 0) throw new IOException("Unexpected data during shutdown.");
        }
        connection.CompleteInput();
    }
}

internal sealed record Ready(int Port);
internal sealed record Result(string Role, string Protocol, string Cipher, string Alpn, int Bytes, string Sha256, bool RequiredClientCertificate, bool Resumed);
[JsonSerializable(typeof(Ready))]
[JsonSerializable(typeof(Result))]
internal partial class ConsumerJson : JsonSerializerContext;
