# Source and native reference provenance

Implementation uses the immutable latest-main snapshot requested by the user,
`80a065112426bce68c1da42d026478d3e40fd45e` (version header 2.7.0), resolved
2026-09-12. [source.json](../config/source.json) records its archive hash and URL.
`python3 msquic/scripts/fetch.py` verifies the archive and archived source files.
The MsQuic input is MIT licensed (`LICENSE` in the unchanged reference tree).

The required picotls input remains
`3598470df01264da85157025ed10db0f7e103790`; its archive, MIT notices and native
test-only dependency are recorded in [picotls inputs](../../picotls/config/inputs.json)
and [source documentation](../../picotls/docs/source.md).

`python3 msquic/scripts/inventory.py` regenerates
[source-inventory.json](../config/source-inventory.json) and
[public-api-inventory.json](../config/public-api-inventory.json) from the pinned
input. The 39 core files and five portable-platform candidates are a compiler
survey, not an accepted product closure. The public API/parameter inventory
explicitly records unimplemented and unreviewed entries. The product closure,
settings policy and managed CxPlat ABI remain P0/P1 work; Linux POSIX diagnostic
headers do not define the product host contract.

[native-inputs.json](../config/native-inputs.json) records every upstream gitlink
from the pinned GitHub tree, including the exact quictls archive checksum.
GoogleTest, CLOG, XDP, native OpenSSL and quictls are not product dependencies.
The separate native MsQuic reference uses only the pinned quictls input; logging,
performance tools and upstream GoogleTest suites are disabled for this build.
Installed system OpenSSL 3.0.13 cannot satisfy the latest upstream OpenSSL
provider's 3.5.0 requirement, as specified in the pinned `CMakeLists.txt`.

```sh
python3 msquic/scripts/native-oracle.py --jobs 4
python3 msquic/scripts/test-native-sample.py
python3 msquic/scripts/test-native-peer.py
```

The recipe verifies the MsQuic input, verifies quictls SHA-256, and builds
upstream `quicsample` in `build/native-oracle/`. A separate source working copy
protects `ref/` from CMake/Perl generated files. Full commands and logs are kept in
`artifacts/native-oracle/`. This native executable is a test peer only and never
enters the translated product. Building the peer alone does not prove an
authenticated stream exchange or the required cipher/group profile. Native
The independently pinned [aioquic peer](independent-peer.md) passes both-role
interoperability in16 positive and4 authentication-negative cases. The native sample
smoke passes certificate-verified connection and stream events but does not check
payload contents, negotiated group or cipher.

The separate [NativePeer harness](../tests/NativePeer/peer.c) subsequently validates
65,537 deterministic bytes in each direction with FIN and clean shutdown. It
passes ECDSA/RSA × AES-128/AES-256 × IPv4/IPv6 (eight cases), querying and checking
QUIC v1, exact cipher suite, P256 group23 and ALPN `dotcc-probe`. Unrelated trust
and wrong server-name cases reject authentication before application data.
`artifacts/native-oracle/peer-results.json` records exact commands, source hash,
test-only trust files and OpenSSL configuration. Tests set both the credential
CA file and `SSL_CERT_FILE` explicitly; insecure verification is never enabled.

The oracle build defines `IS_OPENSSL_3=1` to enable upstream's negotiated-group
query for its pinned quictls3.1.7 dependency. Without that flag the upstream
getter leaves `TlsGroup` at0 even after a successful handshake. The native test
OpenSSL configuration restricts groups to P256; actual queried group23 verifies
the restriction. IPv6 uses an explicit loopback remote address while preserving
`localhost` SNI, avoiding native address-configuration resolver filtering on a
host whose IPv6 interface is loopback-only. The upstream input is never edited.
