# picotls adapter contract review

The actual adapter passes 20 cases in each raw/optimized × JIT/NativeAOT variant
on Linux x64, recorded by `scripts/test-tls-adapter.py` under
`artifacts/tls-adapter/results.json`. This establishes the typed TLS services,
packet-key integration and ownership. Complete QUIC transport scheduling and UDP
interoperability are separate gates. The earlier raw picotls feasibility matrix
establishes only the handshake-message API.

## Input and result mapping

`CxPlatTlsProcessData` receives bytes from the current MsQuic CRYPTO encryption
level. Its input/output `BufferLength` is the number consumed, as used by
`QuicCryptoProcessDataComplete` in the pinned `src/core/crypto.c`. The adapter
must account for each byte exactly once, including fragments retained by picotls.
Use `ptls_get_read_epoch` to select the corresponding raw input epoch. Epoch 1
remains disabled. Only the initial client call supplies a null input pointer to
`ptls_handle_message`; a later empty call must not restart ClientHello.

Reset the picotls output buffer and all five epoch offsets for each call. Copy
output into MsQuic-owned allocation, preserving absolute `BufferTotalLength`,
`BufferOffsetHandshake`, and `BufferOffset1Rtt` across calls and across core
compaction of the pending output buffer. A failed grow must retain the existing
allocation and its valid contents. Key derivation uses the original translated
`QuicPacketKeyDerive` and its configured version-specific HKDF labels.

Traffic-key callbacks can run inside message processing. Record the resulting
read/write keys and flags without advancing the core's input level before
picotls has accepted the corresponding handshake transition. Handshake complete,
negotiated ALPN, resumption, and output readiness are separate results.

The QUIC adapter must reject TLS KeyUpdate messages before picotls consumes them:
the pinned raw TLS handlers otherwise accept them after the handshake. Preserve
the four-byte handshake framing across fragmented and concatenated input; do
not interpret handshake bodies or implement TLS transitions in the adapter.
Return `CXPLAT_TLS_RESULT_ERROR` and `AlertCode = 10`. This produces QUIC close
code `0x010a` through the unchanged core's `QUIC_ERROR_CRYPTO_ERROR` path, as
required by [RFC 9001 section 6](https://datatracker.ietf.org/doc/html/rfc9001#section-6).

## Ticket integration constraint

The pinned picotls implementation generates NewSessionTicket in
`server_finish_handshake`, before the client Finished. Its public raw API has no
post-handshake ticket-generation operation. MsQuic later supplies application
and transport state through `CXPLAT_TLS_TICKET_DATA`. Passing those bytes to the
TLS handshake parser is incorrect. Calling picotls's private
`send_session_ticket` after handshake completion is also incorrect: that routine
temporarily computes a hypothetical client Finished transcript.

The authored adapter discards the automatic pre-Finished tickets and invokes
`dotcc_ptls_send_quic_ticket` for each application request. This small authored C
boundary includes the unchanged reference core in the same translation unit,
checks the server post-handshake state, and calls the existing
`encode_session_identifier` with the completed transcript and a fresh 32-byte
ticket nonce. It uses the existing buffer macros with a null transcript argument
to encode NewSessionTicket. It does not change handshake state, traffic keys,
epochs, or the transcript. Each request receives its own PSK, age-add, issue time,
and full lifetime, including requests on long-lived connections. Its temporary
session secret is cleared before returning. `picotls/config/core-wrappers.json`
and the translation provenance record this explicit authored input.

The reused BCL ticket protector authenticates one versioned envelope containing
the fresh TLS session identifier, its expiry, and the MsQuic application and
transport state. Original picotls algorithms generate the session identifier;
the adapter handles the opaque ticket identity only.

The server authenticates and bounds the entire envelope before passing its
application bytes to the original core ReceiveTicket callback and its session
identifier to picotls. The latter still checks issue time, context, cipher, ALPN,
and the PSK binder. Every accepted ticket rejects early data and requires fresh
DHE. The envelope uses the currently configured protector ring at release, so
shared imported keys support cross-instance resumption without a stateful
ticket lookup. Replacing the ring removes omitted decrypt keys immediately.
The native helper control passes both ciphers and both certificate types,
including issuance two hours after a 60-second original lifetime, two distinct
nonce/PSK pairs, actual resumption of both tickets with fresh P256 exchanges,
invalid-state rejection, and failed-protector output rollback. The actual adapter
also passes delayed ticket delivery, distinct ticket PSKs, cross-configuration
imported-key resumption, key removal, authenticated application rejection and
corrupted-identity fallback. Real transport acceptance remains a separate gate.

Transport-parameter collection validates repeated extensions. Only the client
calls the core ReceiveTP callback: the server's core has already parsed the
ClientHello parameters before selecting the TLS configuration. The managed
profile requires nonempty ALPN identifiers without NUL bytes, matching the
picotls negotiated-protocol accessor's string representation.

## Ownership and verification

Use one immutable picotls server context per security configuration. The reused
BCL ticket protector binds to exactly one context, which may serve multiple TLS
connections. Keep its owner and certificate owners alive until all connections
release their leases. Per-connection callback state uses a rooted handle in
`ptls_get_data_ptr`; do not store managed references in C storage.

MsQuic local transport-parameter and resumption input buffers, output buffers,
and packet keys keep their original allocator ownership. Picotls temporary
buffers use the picotls allocator. Security configuration creation, connection
initialization, and callback failure must clean up partial ownership transfers.

The passing adapter cases include fragmented inputs, exact consumed lengths and
result flags, both roles and both ciphers with ECDSA/RSA certificates,
HelloRetryRequest including bytewise fragments and a separate raw-peer bridge,
transport parameters, certificate/name/trust failures, ALPN failure, TLS KeyUpdate
alert 10, ticket lifecycle, and disposal of original certificate/credential/
configuration owners before the handshake. The core maps alert 10 to close
`0x010a`; the actual transport close remains an integration gate.
Packet tests call translated derivation helpers and compare Initial secrets,
keys, IVs, header protection, Retry integrity, and key updates with RFC/native
vectors. Decryption authenticates into private storage before exposing output.

## Credential callbacks

The authored loader accepts the selected managed flags only. Asynchronous load
invokes completion inline and returns PENDING, matching the pinned OpenSSL
loader's observable ordering. Certificate indication is client-only in this
profile and requires portable certificates: a DER leaf and PKCS7 chain are
borrowed only during the callback. The owning facade must copy them if needed
after return. The reused provider still validates explicit trust, hostname,
certificate purpose, and the CertificateVerify signature. Deferred application
approval cannot override a failed trust/name check. Original core callbacks own
pending approval and completion; an application rejection releases the provider's
pending signature verification key. Portable DER/PKCS7 acceptance/rejection,
deferred failed-trust rejection, inline asynchronous completion and invalid flag
combinations pass the adapter matrix. Pending approval through the actual core
and owning facade remains a P6/P8 integration gate.
