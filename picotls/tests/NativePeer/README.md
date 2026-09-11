# Native picotls test peer

`main.c` is a small native test adapter linked against the pinned picotls core
and upstream OpenSSL provider. It is never part of the managed product. Both
roles use the four-byte big-endian length-prefixed echo protocol documented in
`../IndependentPeer/README.md` and the same disposable credential files.

The adapter implements blocking local socket transport, preserving the number
of bytes consumed by each `ptls_handshake`/`ptls_receive` call. It buffers and
flushes TLS output, bounds application frames to 8 MiB, fragments application
writes, and requires an actual received `close_notify` before successful exit.
An alarm bounds the whole native process to 30 seconds. It binds/connects IPv4
loopback only; a server writes an atomic `{"Port":12345}` readiness file and
accepts one connection.

Example commands after building:

```sh
NativePeer server --credentials DIRECTORY --identity server-ecdsa --ready ready.json --cipher TLS_AES_128_GCM_SHA256
NativePeer client --credentials DIRECTORY --port PORT --bytes 65537 --cipher TLS_AES_256_GCM_SHA384
```

Only P-256 ECDHE and the selected AES-GCM suite are registered. The default is
AES-256-GCM/SHA-384. ALPN is `dotcc-picotls`; client SNI/endpoint name defaults
to `localhost` (`--target NAME` overrides it). Certificate verification uses
upstream picotls's OpenSSL callback with an explicit fixture root store; the
upstream callback validates chain, server/client purpose and endpoint name,
and verifies CertificateVerify. It does not use system roots or public endpoints.

`--identity server-rsa` selects RSA server authentication. The server's
`--require-client-cert true` requests client authentication; clients can select
`--identity client-rsa` or `client-ecdsa`. `--update-key true` requests a traffic
key update after handshake completion. Session tickets and early data are not
enabled by this adapter. The process prints a JSON result containing negotiated
protocol/cipher/ALPN, byte count and SHA-256 of the authenticated application
payload, or an explicit operation/error and exit 1.

`picotls/scripts/test-native-peer.sh` verifies the inputs, builds the adapter with
warnings treated as errors, and exercises both native-client and native-server
pairings against the independent BCL peer. Each direction tests both AES suites
with RSA/ECDSA server keys, empty and large fragmented payloads, key update,
RSA/ECDSA client authentication, and rejection of wrong-name, expired, untrusted
and missing/untrusted-client certificates. `--aot-peer` publishes and runs the BCL peer
with Linux x64 NativeAOT. The native side remains an ordinary native C process.
Run builds/tests serially with the campaign's other work.

These tests establish interoperability of the reference peers. They do not
establish any translated picotls execution; P4 must include the translated
provider and core in both client/server roles using the same peer contract.

Executed on 2026-09-11: all 26 native/SslStream pairings passed with
`test-native-peer.sh --aot-peer` on Linux x64, using the pinned native oracle and
SDK 10.0.111's NativeAOT publication of IndependentPeer. Results independently
agree on both negotiated AES suites, payload counts and hashes. Negative cases
require a handshake/authentication error from the peer rejecting the certificate;
timeouts or startup failures do not qualify. Evidence is retained under
`artifacts/native-peer/run-9nb158sg/`. GCC uses `-Wall -Wextra -Werror` for the
adapter; pinned upstream headers are system includes, as their disabled logging
inlines contain unused parameters. The upstream core and headers are unchanged.
