# P1: raw picotls QUIC handshake feasibility

The [QuicTlsFeasibility](../tests/QuicTlsFeasibility/Program.cs) consumer executes
the existing translated picotls core and existing BCL crypto/certificate/ticket
provider directly through `ptls_handle_message`. It does not use the
stream-oriented `PicotlsConnection` facade or add TLS record framing.

On Linux x64, all **11 cases pass in each of raw/optimized × JIT/NativeAOT**:
44 case executions. Both actual published executables identify their runtime
as NativeAOT. The only build warnings are the existing CS8981 lowercase C type
name warnings from generated picotls; there are no IL/trim/AOT warnings.

```sh
python3 msquic/scripts/test-tls-feasibility.py
```

The run manifest at `artifacts/tls-feasibility-run.json` records exact commands,
source/provider/generated-code hashes, case counts, and limits. Each detailed
receipt is `artifacts/tls-feasibility-{raw,optimized}-{jit,aot}.json`. Builds use
isolated SDK artifact directories beneath `build/tls-feasibility/`.

## Positive coverage

Eight cases combine AES-128-GCM/SHA-256 and AES-256-GCM/SHA-384, ECDSA/P-256 and
RSA-PSS signing identities, and ordinary/HelloRetryRequest handshakes. Ordinary
handshakes use 137-byte input fragments; retry handshakes use one-byte fragments.
The pump is bounded and periodically forces GC while callback state is live.

Each case verifies:

- A zero-input client start emits raw ClientHello; HelloRetryRequest produces
  exactly two ClientHellos rather than one.
- Every input fragment is supplied at `ptls_get_read_epoch()`; wrong-epoch input
  returns TLS alert 10 (`unexpected_message`).
- Output offsets are monotonic and cover the complete raw output. Parsing each
  output segment as handshake messages succeeds, with no TLS record header.
- Output contains epochs **0, 2, and 3**. Epoch 3 carries a protected
  NewSessionTicket, delivered to the client's save-ticket callback. Epoch 1 is
  absent, early-data fields remain zero, and no early traffic secret is emitted.
- The custom QUIC transport-parameter extension (`0x39`) is received with exact
  byte equality in both roles. Client/server use distinct test parameter bytes.
- Both peers finish with the selected cipher, matching ALPN `dotcc-quic`, and
  server-observed SNI `localhost`.
- All four directional secrets agree: client read/server write and client
  write/server read at epochs 2 and 3. Secret lengths are 32/48 bytes for the
  selected hash. The harness compares secrets in constant time, does not log
  them, and zeroes its copies on disposal.
- All provider-owned managed contexts return to their pre-case count after
  disposal. Raw configuration/registration buffers are freed after `ptls_free`.

Certificates are created with BCL APIs for each process: a private test CA,
ECDSA and RSA leaves with SAN `localhost`, server-auth EKU, and digital-signature
key usage. Verification uses explicit custom-root trust with certificate
downloads disabled by the existing provider. Revocation is deliberately disabled
for these short-lived test certificates.

## Negative coverage

| Case | Observed alert |
| --- | --- |
| No shared ALPN | 120 (`no_application_protocol`) |
| Wrong expected hostname | 42 (`bad_certificate`) |
| Different trusted root | 48 (`unknown_ca`) |

Every negative case verifies that the handshake does not complete at both peers
and that provider contexts are released. Test outcomes and negotiated values
agree across the four build/runtime combinations; randomized bytes and
fragment-step counts are not compared exactly.

## Ownership and boundaries

[RawPeer.cs](../tests/QuicTlsFeasibility/RawPeer.cs) owns raw contexts and extension
buffers allocated through translated picotls's `Libc` allocator. It retains its
signer, verifier, and ticket-protector owners through each test's `using` scopes.
A stable `GCHandle` token is stored in picotls's application data pointer.
Callbacks are static managed `delegate*` values matching the generated ABI;
they capture exceptions into a `CallbackScope` and return explicit errors.

No generated source or BCL provider source was modified for this spike. Provider
operations stay inside callback scopes on one thread, and no managed object
reference is stored directly in C memory. Crossing into actual MsQuic allocation
domains remains future adapter work.

## What remains

This establishes the P1 handshake-message feasibility sub-gate, not the complete
`CxPlatTls*` implementation. The core still needs MsQuic's security configuration,
per-connection state, TLS-result flags, consumed-byte accounting, encryption-level
offsets, alert mapping, and ownership contract wired to these calls.

The transport-parameter bytes are structurally encoded test values, processed
opaquely by picotls. No MsQuic validation or connection-ID relationship is tested.
The protected TLS ticket does not implement MsQuic's application/transport ticket
envelope, resumption policy, expiry/rejection/rotation matrix, or 1-RTT resumption.

QUIC packet protection, rejection of received TLS KeyUpdate messages, transport
key-phase updates, real UDP integration, native/independent interop, fault
injection, partial initialization failures, and the wider P4/P6/P7 matrix remain
open. Other OS/architecture runners are unverified.
