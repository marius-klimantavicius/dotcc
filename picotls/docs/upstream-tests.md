# Upstream test coverage

This is a case-level inventory of the pinned `t/picotls.c` suite, not a claim
that its original runner has been translated. Its runner includes private core
implementation and minicrypto/FFX fixtures directly. The product continues to
translate the three unchanged selected core units. Named ports call that product
through its real ABI; independent provider vectors and TLS harness cases are
identified separately below.

Status on 2026-09-11: optimized Linux x64 JIT executes all eight `UpstreamVectors`
ports (232 checks), the `UpstreamHandshakePorts` cases within the passing TLS
suite, 321 provider checks, and 56 independent peer scenarios. Raw and NativeAOT
replays remain pending. Native oracle results are recorded separately in
[validation.md](validation.md). A mapped replacement case is not automatically
equivalent to every assertion in the upstream case.

| Upstream case | Managed coverage or explicit remaining work |
| --- | --- |
| `is_ipaddr` | Direct seven-input port in `UpstreamVectors`. |
| `extension_bitmap` | UpstreamHandshakePorts mutates a real ClientHello to duplicate supported_versions or add disallowed ech_outer_extensions and requires illegal_parameter; this exercises the bitmap through the parser. |
| `select_cipher` | Both supported suites are exercised by TLS tests; the complete upstream client/server preference matrix remains pending. |
| `sha256`, `sha384` | ProviderVectors: published digest, clone independence, empty digest, reset/free/snapshot modes, allocation and GC behavior. |
| `hmac-sha256` | Direct RFC 4231 vector port with two resets and final free in `UpstreamVectors`. |
| `hkdf` | ProviderVectors uses the same RFC 5869 case 1 PRK/OKM and actual translated HKDF. |
| `aes128gcm`, `aes256gcm` | ProviderVectors tests both algorithms, known answers, independent BCL checks, bad tag/AAD/sequence, overlapping input/output, vectors and supplementary encryption. |
| `aes128ecb`, `aes256ecb`, `aes128ctr` | ProviderVectors uses FIPS 197 and NIST SP 800-38A vectors through the provider callbacks, including AES256CTR additionally. |
| `chacha20poly1305`, `chacha20` | Excluded from the declared initial algorithm profile; no table advertises these algorithms. |
| `aegis-128l`, `aegis-256` | Excluded: optional AEGIS provider is outside the BCL-only selected profile. |
| `ffx` | Excluded: FFX/QUIC-LB follow-up profile, not part of selected source closure. |
| `base64-decode` | Direct valid text, invalid ASCII alphabet and non-ASCII byte ports in `UpstreamVectors`. |
| `tls-block8`, `tls-block16` | UpstreamVectors ports exact 255/65535-byte success and one-byte overflow cases through thin host wrappers over the unchanged C length-block macros; also tests truncation/trailing bytes. |
| `ech` and ECH handshake variants | Excluded: ECH is disabled and no ECH/HPKE algorithms are advertised. |
| `fragmented-message` | TlsTests exercises fragmented/coalesced TLS messages; upstream exact malformed-fragment scenarios still need mapping. |
| `hrr-cipher-suite-mismatch` | UpstreamHandshakePorts generates a real SHA384 HRR, rewrites the final ServerHello to the still-offered SHA256 suite and requires illegal_parameter. |
| `hrr-selected-group-matches-ch1-key-share` | UpstreamHandshakePorts sends the exact pinned HRR packet selecting the P256 key share already sent in ClientHello and requires illegal_parameter. |
| `handshake/full-handshake` | TlsTests plus process interoperability in both client/server directions with both suites and RSA/ECDSA authentication. |
| `handshake/send-fails-with-handshake-traffic-key` | UpstreamHandshakePorts ports the raw-core sequence through ServerHello only, then requires ptls_send to return IN_PROGRESS without emitting bytes while only handshake traffic keys exist. |
| `handshake/full-handshake+client-auth` | TlsTests and independent process mTLS success/rejection cases. |
| `handshake/hrr-handshake`, `enforce-retry-stateful` | TlsTests exercises stateful enforced HRR and client deferred key exchange. |
| `handshake/resumption`, `resumption-with-client-authentication` | TlsTests passes actual PSK-DHE ticket resumption, rotation/retention/eviction, corruption fallback and expiry. Exact mTLS resumption remains pending. |
| `handshake/resumption-different-preferred-key-share` | Excluded from initial single-P256-group profile. |
| `handshake/resumption-with-grease` | Pending GREASE ticket-resumption port. |
| `handshake/async-sign-certificate` | Excluded: public async signing is outside this initial profile; synchronous failure/cleanup remains required. |
| `handshake/hrr-stateless-handshake`, `enforce-retry-stateless`, `stateless-hrr-aad-change` | Stateless cookie HRR is not exposed by the initial owning facade; stateful HRR is supported. Raw API coverage pending. |
| `handshake/key-update` | TlsTests plus native process key-update exchanges. |
| `handshake/pre-shared-key` | External PSKs are outside the initial certificate-authenticated ticket-resumption profile. |
| `handshake/handshake-api` | Full QUIC message-level handshake harness remains follow-up work; TLS record API is exercised. |
| `quic/varint` | Direct published decode patterns and round-trip boundaries in `UpstreamVectors`, with additional bounded truncation cases. Codec coverage does not imply QUIC transport support. |
| `quic/block` | UpstreamVectors ports the exact 3-byte and 123-byte block cases through macro wrappers; QUIC transport remains excluded. |
| `legacy-ch` | UpstreamHandshakePorts rewrites the supported_versions extension of a real ClientHello to TLS1.2-only and requires protocol_version. Exact upstream SSL2 and fallback callback metadata cases remain pending; TLS1.2 compatibility is disabled. |
| `ptls_escape_json_unsafe_string` | Direct ASCII escapes, Unicode and control-byte ports in `UpstreamVectors`. |
| `signature-algorithms-overflow` | UpstreamHandshakePorts inserts all 35 pinned Erlang schemes into a real ClientHello; both RSA/ECDSA servers must parse the full offer and produce a signing flight. The intentionally changed transcript is not claimed as a complete round trip. |

Provider-specific `t/openssl.c`, `t/minicrypto.c`, `t/mbedtls.c` and `t/fusion.c`
exercise implementations excluded from the managed product. Their relevant
algorithm and authentication contracts are covered by BCL vectors and independent
TLS peers, not reported as direct ports. `t/hpke.c` and `t/quiclb.c` cover follow-up
profiles. Keep pending rows visible when reporting initial-profile completion;
each applicable pending row needs an executed case or a precise remaining blocker.
