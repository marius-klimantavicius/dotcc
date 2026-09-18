# Managed.Net.Quic messages in one app

Run from the repository root with .NET 10 and the existing generated MsQuic and
PicoTLS sources:

```sh
dotnet run --project msquic/samples/ManagedNetQuicRoundTrip -c Release
```

The app starts a loopback listener on an available port, connects a client in
the same process, and sends a UTF-8 message in each direction over a bidirectional
QUIC stream. Each sender completes its writes (FIN) to mark the end of its
message. Both connections remain alive until the exchange finishes, then close.

Example output (the port and negotiated cipher can vary):

```text
Listener: listening on 127.0.0.1:45678
Client: connected using TLS_AES_128_GCM_SHA256
Client -> listener: Hello from the client!
Listener received: Hello from the client!
Listener -> client: Hello from the listener!
Client received: Hello from the listener!
Done: both messages received and connection closed.
```

`Program.cs` contains the listener, client, and message exchange. A normal
project reference and `using Managed.Net.Quic;` select the translated API.
`SampleCertificates.cs` creates an in-memory CA and localhost server certificate;
the client trusts only that CA and checks the server name. No certificate files,
trust-store installation, or second process are needed. The exchange has a
30-second timeout.
[ManagedNetQuicRoundTrip.slnx](ManagedNetQuicRoundTrip.slnx)