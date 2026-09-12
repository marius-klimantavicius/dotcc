# Actual TLS adapter harness

`python3 msquic/scripts/test-tls-adapter.py` compiles the authored platform,
packet-crypto and TLS partials against existing frozen MsQuic and picotls
translations. All 99 host-table slots have real selected callbacks or typed
test-only fail-fast bodies. Tests call the translated `CxPlatTls*` entry points;
they do not replace TLS callbacks with success stubs.

The matrix covers both cipher suites and RSA/P-256 credentials, full and
byte-fragmented HRR exchanges, a separate unchanged feasibility RawPeer server,
trust/name/ALPN/TP rejection, exact KeyUpdate alert 10, typed result flags,
consumption, absolute output epoch offsets and overlapping buffer compaction.
Directional packet keys must actually encrypt/decrypt successfully. Credential
and security-config owners are released before handshaking to test retained
ownership. TP callback counts follow the core role contract (client only).

Ticket tests use actual delayed TICKET_DATA issuance, distinct ticket PSKs,
independently configured protectors sharing imported keys, retained/removed keys,
authenticated application data and application rejection, and corrupted ticket
identities. No 0-RTT keys may appear. The provider vectors separately verify
authenticated expiry and counter limits; adapter clock-boundary coverage is
pending the new on-demand ticket helper integration.

`--variants optimized --jit-only` is an initial probe and never marks the full
gate passed. The full receipt at `artifacts/tls-adapter/results.json` binds both
raw/optimized JIT/NativeAOT case lists to generated sources, provider/host inputs,
and frozen closure hashes. A source change during execution invalidates the run.
This is a TLS adapter gate; complete QUIC/UDP/native-peer interoperability belongs
to the separate transport campaign.
