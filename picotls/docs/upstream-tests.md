# Upstream test coverage

This is a case-level inventory of the pinned `t/picotls.c` suite, not a claim
that its original runner has been translated. Its runner includes private core
implementation and minicrypto/FFX fixtures directly. The product continues to
translate the three unchanged selected core units. Named ports call that product
through its real ABI; independent provider vectors and TLS harness cases are
identified separately below.

Status on 2026-09-11: **all selected-profile mappings below pass all four Linux
x64 variants (raw/optimized × JIT/NativeAOT)**. This includes eight utility ports
with 232 checks, both upstream handshake-port groups within the TLS suite, 2033
provider checks and 56 independent peer scenarios per variant. Evidence:
`artifacts/tests/run-lqwhjcso` and [validation.md](validation.md). A mapped
replacement is not automatically equivalent to every assertion in the upstream
case; exclusions and narrower coverage remain explicit.

| Upstream case | Managed coverage or explicit remaining work |
| --- | --- |
| `is_ipaddr` | Direct seven-input port in `UpstreamVectors`. |
| `extension_bitmap` | UpstreamHandshakePorts mutates a real ClientHello to duplicate supported_versions or add disallowed ech_outer_extensions and requires illegal_parameter; this exercises the bitmap through the parser. |
| `select_cipher` | AdditionalUpstreamPorts directly calls actual select_cipher for both AES orderings, client/server preferences, ignored ChaCha offers, empty/no-overlap cases. ChaCha selection is excluded by the advertised profile. The additional port passes all four variants. |
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
| `fragmented-message` | TlsTests passes fragmented/coalesced TLS exchanges. AdditionalUpstreamPorts ports exact private-core fragment bytes/end-of-record flags and both 14-byte buffer-overflow paths; the additional port passes all four variants. |
| `hrr-cipher-suite-mismatch` | UpstreamHandshakePorts generates a real SHA384 HRR, rewrites the final ServerHello to the still-offered SHA256 suite and requires illegal_parameter. |
| `hrr-selected-group-matches-ch1-key-share` | UpstreamHandshakePorts sends the exact pinned HRR packet selecting the P256 key share already sent in ClientHello and requires illegal_parameter. |
| `handshake/full-handshake` | TlsTests plus process interoperability in both client/server directions with both suites and RSA/ECDSA authentication. |
| `handshake/send-fails-with-handshake-traffic-key` | UpstreamHandshakePorts ports the raw-core sequence through ServerHello only, then requires ptls_send to return IN_PROGRESS without emitting bytes while only handshake traffic keys exist. |
| `handshake/full-handshake+client-auth` | TlsTests and independent process mTLS success/rejection cases. |
| `handshake/hrr-handshake`, `enforce-retry-stateful` | TlsTests exercises stateful enforced HRR and client deferred key exchange. |
| `handshake/resumption`, `resumption-with-client-authentication` | TlsTests passes actual PSK-DHE ticket resumption, rotation/retention/eviction, corruption fallback and expiry. The exact mTLS policy passes: offered tickets fall back to full certificate reauthentication, as pinned upstream requires; a missing client certificate still fails. |
| `handshake/resumption-different-preferred-key-share` | Excluded from initial single-P256-group profile. |
| `handshake/resumption-with-grease` | AdditionalUpstreamPorts passes actual full/GREASE PSK-DHE resumed handshakes and authenticated payload checks in all four variants. A test-only null-operation KEM descriptor supplies metadata read by the pinned random-GREASE path, while actual BCL AES-GCM protects the dummy payload. This neither advertises X25519 nor tests real ECH/HPKE. |
| `handshake/async-sign-certificate` | Excluded: public async signing is outside this initial profile; synchronous failure/cleanup remains required. |
| `handshake/hrr-stateless-handshake`, `enforce-retry-stateless`, `stateless-hrr-aad-change` | Excluded from the initial owning API: stateless cookie HRR and its application AAD/cookie policy are not exposed or advertised. Stateful HRR is tested; translated raw-core presence does not establish the deferred stateless profile. |
| `handshake/key-update` | TlsTests plus native process key-update exchanges. |
| `handshake/pre-shared-key` | External PSKs are outside the initial certificate-authenticated ticket-resumption profile. |
| `handshake/handshake-api` | Full QUIC message-level handshake harness remains follow-up work; TLS record API is exercised. |
| `quic/varint` | Direct published decode patterns and round-trip boundaries in `UpstreamVectors`, with additional bounded truncation cases. Codec coverage does not imply QUIC transport support. |
| `quic/block` | UpstreamVectors ports the exact 3-byte and 123-byte block cases through macro wrappers; QUIC transport remains excluded. |
| `legacy-ch` | UpstreamHandshakePorts rewrites the supported_versions extension of a real ClientHello to TLS1.2-only and requires protocol_version. AdditionalUpstreamPorts passes all six exact pinned legacy packets (including SSL2 rejection before callback) and legacy incompatible-version/raw/SNI metadata in all four variants. TLS1.2 compatibility remains disabled. |
| `ptls_escape_json_unsafe_string` | Direct ASCII escapes, Unicode and control-byte ports in `UpstreamVectors`. |
| `signature-algorithms-overflow` | UpstreamHandshakePorts inserts all 35 pinned Erlang schemes into a real ClientHello; both RSA/ECDSA servers must parse the full offer and produce a signing flight. The intentionally changed transcript is not claimed as a complete round trip. |

Provider-specific `t/openssl.c`, `t/minicrypto.c`, `t/mbedtls.c` and `t/fusion.c`
exercise implementations excluded from the managed product. Their relevant
algorithm and authentication contracts are covered by BCL vectors and independent
TLS peers, not reported as direct ports. `t/hpke.c` and `t/quiclb.c` cover follow-up
profiles. The selected-profile ports above pass all four variants. Excluded
profiles are not inferred from
their raw-core presence or the native oracle’s larger algorithm list.
