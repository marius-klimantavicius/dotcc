using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Managed.Security.S2n;

namespace ManagedConsumer;

public sealed record ExchangeObservation(string engine, string role, string version, string iana_id,
    int sent, int received, bool shutdown, long callbacks);

[JsonSerializable(typeof(ExchangeObservation))]
internal partial class ConsumerJson : JsonSerializerContext;

public static class Exchange
{
    public const int PayloadLength = 1027;
    public static byte[] Payload(int direction)
    {
        byte[] data = new byte[PayloadLength];
        for (int i = 0; i < data.Length; i++) data[i] = (byte)((i * 131 + (i >> 3) + direction * 73) & 255);
        data[0] = 0; data[513] = 255; data[^1] = 0;
        return data;
    }
    public static S2nProtocol Protocol(string name) => name switch
    {
        "S2N_SSLv3" => S2nProtocol.Ssl3, "S2N_TLS10" => S2nProtocol.Tls10,
        "S2N_TLS11" => S2nProtocol.Tls11, "S2N_TLS12" => S2nProtocol.Tls12,
        "S2N_TLS13" => S2nProtocol.Tls13, _ => throw new ArgumentException("Unknown protocol: " + name)
    };
    public static string ProtocolName(S2nProtocol version) => version switch
    {
        S2nProtocol.Ssl3 => "S2N_SSLv3", S2nProtocol.Tls10 => "S2N_TLS10", S2nProtocol.Tls11 => "S2N_TLS11",
        S2nProtocol.Tls12 => "S2N_TLS12", S2nProtocol.Tls13 => "S2N_TLS13", _ => throw new ArgumentOutOfRangeException(nameof(version))
    };
    public static S2nConfiguration Configuration(S2nRuntime runtime, S2nRole role, string fixtures, string source,
        string validation = "validate", bool clientCertificate = false)
    {
        var config = new S2nConfiguration(runtime);
        try
        {
            config.EnableSessionTickets(false);
            if (role == S2nRole.Client)
            {
                config.ClearTrustStore();
                if (!validation.Contains("untrusted", StringComparison.Ordinal))
                { config.AddTrustPem(File.ReadAllText(Path.Combine(fixtures, "rsa.pem"))); config.AddTrustPem(File.ReadAllText(Path.Combine(fixtures, "ecdsa.pem"))); }
                if (validation.Contains("bypass", StringComparison.Ordinal)) config.DisableCertificateVerification();
            }
            if (role == S2nRole.Server || clientCertificate)
            {
                foreach (string kind in new[] { "rsa", "ecdsa" })
                {
                    using var credential = new S2nCredentials(runtime, File.ReadAllText(Path.Combine(fixtures, kind + ".pem")), File.ReadAllText(Path.Combine(fixtures, kind + ".key")));
                    config.AddCredentials(credential);
                }
                config.AddDhParameters(File.ReadAllText(Path.Combine(source, "tests/pems/dhparams_2048.pem")));
            }
            return config;
        }
        catch { config.Dispose(); throw; }
    }
    public static async Task<(Socket Client, Socket Server)> SocketsAsync(CancellationToken token)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            Task<Socket> accepted = listener.AcceptSocketAsync(token).AsTask();
            await client.ConnectAsync(listener.LocalEndpoint, token);
            return (client, await accepted);
        }
        catch { client.Dispose(); throw; }
        finally { listener.Stop(); }
    }
    public static async Task<ExchangeObservation[]> LoopbackAsync(S2nProtocol protocol, ushort suite, string cipherName,
        string fixtures, string source, Action<S2nConfiguration, S2nConfiguration>? configure = null,
        Func<Socket, IS2nTransport>? transport = null)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var sockets = await SocketsAsync(timeout.Token);
        using var clientSocket = sockets.Client; using var serverSocket = sockets.Server;
        using var runtime = new S2nRuntime();
        using var clientConfig = Configuration(runtime, S2nRole.Client, fixtures, source);
        using var serverConfig = Configuration(runtime, S2nRole.Server, fixtures, source);
        configure?.Invoke(clientConfig, serverConfig);
        var policy = new S2nPolicy(protocol, protocol, suite);
        await using S2nConnection client = transport == null ? new(clientConfig, S2nRole.Client, clientSocket, policy) : new(clientConfig, S2nRole.Client, transport(clientSocket), policy);
        await using S2nConnection server = transport == null ? new(serverConfig, S2nRole.Server, serverSocket, policy) : new(serverConfig, S2nRole.Server, transport(serverSocket), policy);
        client.SetServerName("localhost");
        async Task<ExchangeObservation> Guard(S2nConnection connection)
        {
            try { return await RunAsync(connection, protocol, suite, cipherName, timeout.Token); }
            catch { timeout.Cancel(); throw; }
        }
        return await Task.WhenAll(Guard(client), Guard(server));
    }
    public static async Task<ExchangeObservation> EndpointAsync(S2nRole role, Socket socket, S2nProtocol protocol, ushort suite,
        string cipherName, string fixtures, string source, string validation, CancellationToken token)
    {
        using var runtime = new S2nRuntime();
        using var config = Configuration(runtime, role, fixtures, source, validation);
        await using var connection = new S2nConnection(config, role, socket, new(protocol, protocol, suite));
        if (role == S2nRole.Client) connection.SetServerName("localhost");
        return await RunAsync(connection, protocol, suite, cipherName, token);
    }
    public static async Task<ExchangeObservation> RunAsync(S2nConnection connection, S2nProtocol protocol, ushort suite,
        string cipherName, CancellationToken token)
    {
        await connection.NegotiateAsync(token);
        S2nNegotiated negotiated = connection.Negotiated;
        if (negotiated.Protocol != protocol || negotiated.CipherSuite != suite || negotiated.CipherName != cipherName)
            throw new IOException($"Wrong negotiation: {negotiated}, expected {protocol}/{suite:x4}/{cipherName}.");
        if (connection.Role == S2nRole.Client) { await SendAsync(connection, 0, token); await ReceiveAsync(connection, 1, token); }
        else { await ReceiveAsync(connection, 0, token); await SendAsync(connection, 1, token); }
        await connection.ShutdownAsync(token);
        if (!connection.ShutdownComplete || !connection.PeerClosed) throw new IOException("Bidirectional close_notify was not observed.");
        S2nIoSnapshot io = connection.InspectIo();
        if (io.CallbackCalls == 0 || io.EncryptedSent <= PayloadLength || io.EncryptedReceived <= PayloadLength)
            throw new IOException("The translated engine did not use the owned I/O callbacks.");
        return new("translated-s2n", connection.Role == S2nRole.Client ? "client" : "server", ProtocolName(protocol),
            "0x" + suite.ToString("X4"), PayloadLength, PayloadLength, true, io.CallbackCalls);
    }
    public static async Task SendAsync(S2nConnection connection, int direction, CancellationToken token)
    {
        byte[] data = Payload(direction);
        for (int offset = 0; offset < data.Length;)
        {
            int count = await connection.SendAsync(data.AsMemory(offset, Math.Min(389, data.Length - offset)), token);
            if (count <= 0) throw new IOException("No TLS send progress.");
            offset += count;
        }
    }
    public static async Task ReceiveAsync(S2nConnection connection, int direction, CancellationToken token)
    {
        byte[] data = new byte[PayloadLength];
        for (int offset = 0; offset < data.Length;)
        {
            int count = await connection.ReceiveAsync(data.AsMemory(offset), token);
            if (count <= 0) throw new EndOfStreamException("TLS closed before the complete binary payload.");
            offset += count;
        }
        if (!data.AsSpan().SequenceEqual(Payload(direction))) throw new IOException("Binary payload differs.");
    }
}
