# picotls adapter contract review

This is the implementation contract for P4, not a passing runtime receipt. The
raw picotls feasibility matrix establishes the handshake-message API only.

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

The adapter must resolve this ordering explicitly. A possible bounded stateful
design retains generated ticket output until the application requests release
and associates the protected ticket with the later application/transport state.
This requires a reviewed lifetime, capacity, expiry, rotation, replay, and
multiple-ticket policy before implementation. No ticket-resumption gate is
satisfied by the existing raw feasibility test alone.

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

P4 validation must include fragmented inputs and output growth, exact consumed
lengths and result flags, both roles and both ciphers, HelloRetryRequest,
transport parameters, certificate/name/trust failures, ALPN failure, wrong
epochs, TLS KeyUpdate close `0x010a`, ticket lifecycle, and repeated disposal.
Packet tests call translated derivation helpers and compare Initial secrets,
keys, IVs, header protection, Retry integrity, and key updates with RFC/native
vectors. Decryption authenticates into private storage before exposing output.
