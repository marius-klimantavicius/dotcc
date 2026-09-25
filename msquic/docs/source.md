# Source and native reference provenance

The active snapshot is recorded in [source.json](../config/source.json).
Select another upstream version with one command, then regenerate:

```sh
python3 msquic/scripts/fetch.py --ref stable  # latest published release
# Alternatives: --ref main, --ref v2.6.1, --ref release/2.6, --ref <commit>
msquic/scripts/translate.sh
```

Selection resolves the ref to a commit, downloads its source, updates the native
reference's quictls dependency from that commit's gitlinks, and regenerates the
source/API/test inventories. It does not translate or validate the new version.
`fetch.py` without `--ref` uses the recorded selection without following updates.
For translation without validation, use `translate.sh --fast` after fetching.

When GitHub is unavailable, supply the source manually at
`msquic/ref/<directory>` (the `directory` in `config/source.json`) and run:

```sh
msquic/scripts/translate.sh --no-fetch
```

Keep the source selection metadata consistent with the tree supplied. This mode
uses the unpacked tree directly, without fetching or requiring its original
tarball. ABI validation and closure/audit evidence record the actual local input
files rather than claiming archive verification. The option also works with
`--fast` (which already skips fetching) and `--no-build-tools`. It controls source
fetching; .NET builds still require their SDK and NuGet dependencies to be available.

The three managed platform headers are reused across versions without checking
hashes of the upstream headers they replace. Staging discovers the core C files
from the selected source's CMake list and extracts the reference/rundown and
route-copy functions into `src/Host/portable.c` and `src/Host/route.c` anew. There
are no manually maintained source hashes to update for these inputs. Missing
files or function boundaries are reported; compilation and ABI/runtime tests
determine compatibility, including semantic differences that compilation misses.

Archive checksums still verify downloads. Generated manifests and test receipts
still identify the inputs actually built/tested; they are not compatibility
allowlists for new upstream versions. Regenerate and rerun the relevant tests
after switching: historical passing receipts do not validate a new source.
The MsQuic input is MIT licensed (`LICENSE` in the unchanged reference tree).

The required picotls input remains
`3598470df01264da85157025ed10db0f7e103790`; its archive, MIT notices and native
test-only dependency are recorded in [picotls inputs](../../picotls/config/inputs.json)
and [source documentation](../../picotls/docs/source.md).

`python3 msquic/scripts/inventory.py` regenerates
[source-inventory.json](../config/source-inventory.json) and
[public-api-inventory.json](../config/public-api-inventory.json) from the pinned
input. These are descriptive compiler survey inventories, not staging gates.
Their historical support labels are not the current product qualification;
see the [qualification ledger](qualification.md) for executed verification.

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
protects `ref/` from CMake/Perl generated files. Switching either MsQuic or quictls
recreates that working copy and build directory on the next native build. Full commands and logs are kept in
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
