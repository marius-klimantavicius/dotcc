extern alias ManagedQuic;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ManagedQuic::System.Net.Quic;

if (!QuicListener.IsSupported || !QuicConnection.IsSupported)
    throw new PlatformNotSupportedException("The translated QUIC implementation is unavailable.");

using var certificates = new SampleCertificates();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
CancellationToken cancellationToken = timeout.Token;
var protocol = new SslApplicationProtocol("dotcc-message-sample");

// Port zero chooses an available loopback port. Both endpoints live in this process.
await using var listener = await QuicListener.ListenAsync(new QuicListenerOptions
{
    ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
    ApplicationProtocols = [protocol],
    ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
    {
        DefaultCloseErrorCode = 0,
        DefaultStreamErrorCode = 0,
        MaxInboundBidirectionalStreams = 1,
        ServerAuthenticationOptions = new SslServerAuthenticationOptions
        {
            ApplicationProtocols = [protocol],
            ServerCertificate = certificates.Server
        }
    })
}, cancellationToken);
Console.WriteLine($"Listener: listening on {listener.LocalEndPoint}");

// Start accepting before connecting so the listener can complete the handshake.
Task<QuicConnection> accepting = listener.AcceptConnectionAsync(cancellationToken).AsTask();
await using var client = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions
{
    RemoteEndPoint = listener.LocalEndPoint,
    DefaultCloseErrorCode = 0,
    DefaultStreamErrorCode = 0,
    ClientAuthenticationOptions = new SslClientAuthenticationOptions
    {
        ApplicationProtocols = [protocol],
        TargetHost = "localhost",
        CertificateChainPolicy = new X509ChainPolicy
        {
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            CustomTrustStore = { certificates.Root },
            // The temporary sample CA has no revocation service.
            RevocationMode = X509RevocationMode.NoCheck,
            DisableCertificateDownloads = true
        }
    }
}, cancellationToken);
await using var server = await accepting;
Console.WriteLine($"Client: connected using {client.NegotiatedCipherSuite}");

// Keep both connections alive until both sides have received their complete message.
await Task.WhenAll(ReceiveAndReplyAsync(server, cancellationToken), SendAndReceiveAsync(client, cancellationToken));
await client.CloseAsync(0, cancellationToken);
Console.WriteLine("Done: both messages received and connection closed.");

static async Task SendAndReceiveAsync(QuicConnection connection, CancellationToken cancellationToken)
{
    await using var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken);
    const string message = "Hello from the client!";
    Console.WriteLine($"Client -> listener: {message}");
    // This sample sends one UTF-8 message per direction; FIN marks its end.
    await stream.WriteAsync(Encoding.UTF8.GetBytes(message), completeWrites: true, cancellationToken);

    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
    string reply = await reader.ReadToEndAsync(cancellationToken);
    if (reply != "Hello from the listener!") throw new InvalidDataException("Unexpected listener message.");
    Console.WriteLine($"Client received: {reply}");
    await stream.WritesClosed.WaitAsync(cancellationToken);
}

static async Task ReceiveAndReplyAsync(QuicConnection connection, CancellationToken cancellationToken)
{
    await using var stream = await connection.AcceptInboundStreamAsync(cancellationToken);
    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
    string message = await reader.ReadToEndAsync(cancellationToken);
    if (message != "Hello from the client!") throw new InvalidDataException("Unexpected client message.");
    Console.WriteLine($"Listener received: {message}");

    const string reply = "Hello from the listener!";
    Console.WriteLine($"Listener -> client: {reply}");
    await stream.WriteAsync(Encoding.UTF8.GetBytes(reply), completeWrites: true, cancellationToken);
    await stream.WritesClosed.WaitAsync(cancellationToken);
}
