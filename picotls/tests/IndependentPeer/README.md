# Independent TLS peer

This standalone .NET 10 executable uses BCL `SslStream` as an independent local
TLS 1.3 client/server oracle. It is test infrastructure, not the translated
picotls implementation or its provider. It uses source-generated JSON metadata
for NativeAOT and no application-owned native imports.

Build with `dotnet build -c Release`. Run the DLL with these commands:

```sh
dotnet IndependentPeer.dll credentials /path/to/empty-directory
dotnet IndependentPeer.dll server --credentials /path/to/credentials --identity server-ecdsa --ready /path/to/ready.json
dotnet IndependentPeer.dll client --credentials /path/to/credentials --port PORT --bytes 65537
```

The server binds only IPv4 loopback; by default it chooses a free port and writes
`{"Port":12345}` atomically to `--ready`. It accepts one connection and exits.
The client connects only to IPv4 loopback, validates the default target name
`localhost`, and sends deterministic bytes `(index * 31 + 7) mod 256`.

Each application frame is a four-byte unsigned big-endian payload length followed
by exactly that many bytes, capped at 8 MiB. The client sends one frame and the
server echoes it. Both send TLS shutdown and expect end of input after the echo.
Writes split payloads at 4,093 bytes, so application frames cross TLS records.
Successful processes print one JSON result with role, protocol, cipher, ALPN,
payload count/hash and mutual-authentication status. A failure prints its type
and message to stderr and returns 1. The default total deadline is 30 seconds.

Both sides require TLS 1.3 and ALPN `dotcc-picotls`; `--alpn NAME` changes the
expected protocol, and `--target NAME` changes client endpoint-name validation.
`--identity server-rsa` chooses RSA instead of the default ECDSA server key.
For mutual authentication, pass `--require-client-cert true` to the server and
`--identity client-rsa` or `--identity client-ecdsa` to the client.

Credential generation refuses a nonempty directory. It creates a disposable RSA
root (`ca.cer`, `ca.pem`), RSA/ECDSA server and client credentials, plus expired
and wrong-name server fixtures. Leaf identities have a certificate `.pem`, a
PKCS#8 private `.key.pem`, and an empty-password `.pfx`. These local test keys are
not production credentials. X.509 validation uses explicit custom roots, SAN
endpoint matching, server/client EKUs and validity checks. Revocation and network
certificate downloads are disabled only for these offline fixtures.

From any working directory, `picotls/scripts/test-independent-peer.sh` builds and
tests the peer against itself using separate local processes. `--aot` additionally
publishes and executes a Linux x64 NativeAOT binary. The script covers empty and
fragmented/large payloads, RSA/ECDSA and mutual authentication, and rejection of
untrusted, expired, wrong-name and missing-client certificates. It retains
per-process logs and credentials under ignored `picotls/artifacts/independent-peer/`.
Coordinate the shared serial build/test slot before running it.

Passing these checks validates the oracle harness. P4 still requires translated
picotls in each client/server pairing, both planned AES suites, and the remaining
protocol and negative cases in the campaign plan.

Executed on 2026-09-11 with SDK 10.0.111 / runtime 10.0.11 on Linux x64:
all nine process cases passed under JIT and again under NativeAOT (18 total).
NativeAOT publication had zero warnings. Negative cases require an explicit
`AuthenticationException` from the peer rejecting the certificate, so timeouts
and unrelated failures do not satisfy rejection tests. Evidence from the final
run is `artifacts/independent-peer/run-tif85kcv/`. These runs negotiated
`TLS_AES_256_GCM_SHA384`; other cipher suites/platforms are not established here.
