# Managed TLS consumer

This example uses the unchanged, dotcc-translated picotls core and the BCL crypto
provider. `NetworkStream` transports ciphertext; the owning `PicotlsConnection`
performs TLS. The product references no native picotls/OpenSSL adapter or SslStream.

From `picotls/`, translate and build before using the example:

```sh
scripts/translate.sh
dotnet build ManagedConsumer/ManagedConsumer.slnx -c Release
dotnet run --project tests/IndependentPeer -c Release -- credentials artifacts/demo-credentials
```

Start a server and client in separate terminals (choose a free local port):

```sh
dotnet run --project ManagedConsumer -c Release -- server --credentials artifacts/demo-credentials --port 44330 --revocation NoCheck
dotnet run --project ManagedConsumer -c Release -- client --credentials artifacts/demo-credentials --port 44330 --revocation NoCheck
```

`NoCheck` is explicit for the disposable offline certificates; the example defaults
to the BCL's `Offline` revocation policy. The sample binds/connects to IPv4 loopback,
requires TLS 1.3 and ALPN, authenticates the server against `ca.cer` and its DNS/IP
SAN, sends a bounded framed payload, validates the echo and exchanges close_notify.
`--require-client-cert true` on the server and `--identity client-rsa` on the
client enable mutual certificate authentication. The default cipher is
`TLS_AES_128_GCM_SHA256`; `--cipher TLS_AES_256_GCM_SHA384` selects the other suite.

Every connection is serialized. Contexts can be shared by independent connections;
disposing a context prevents new connections while existing connections retain it.
Credential registrations are leased by contexts, so they may be disposed by their
owners after registration. Always dispose connections, contexts, credentials and
saved session tickets. The low-level translated API requires an explicit
`CallbackScope`, valid retained native storage and application-managed lifetimes.

`PicotlsConnection.Process` consumes input and returns ciphertext to forward and
authenticated plaintext. Call it with empty input to start a client, feed all
post-handshake records too, and call `CompleteInput` when transport EOF occurs.
Forward `PicotlsException.AlertBytes` when available. Other failures invalidate the
connection. Each call is bounded to 8 MiB, handshake buffering defaults to 1 MiB,
and early data is disabled. `UpdateKey` emits its update immediately.

Client ticket capture is opt-in with `saveSessionTickets: true`. Call
`TakeSessionTickets` after processing records and keep the resulting PSK credentials
confidential. A ticket's `Export` returns a caller-owned byte array to pass to
`CreateConnection`; clear that copy after use. Server tickets require a
`BclCryptoProvider.TicketProtector` leased to one context.

NativeAOT and process-pair validation recipes (execution evidence is tracked in
`docs/validation.md`; these instructions alone do not claim a passing build):

```sh
dotnet publish ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true -o build/managed-consumer-aot
scripts/test-managed-peer.sh
scripts/test-managed-peer.sh --aot
scripts/test-managed-peer.sh --raw
```
