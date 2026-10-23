# Native input and control evidence

The native controls establish source availability, native assertions and real
TLS exchanges. They do not establish translated C# behavior, the managed provider,
managed interoperability, JIT/NativeAOT delivery or campaign completion.

The current full native receipt is
[`20261023T171931Z/report.json`](../artifacts/native/20261023T171931Z/report.json):
**238 passed, zero failed, zero skipped**, using the product compatibility
adapter. All sixteen selected upstream executables passed, with **590,537**
runtime assertions. The deterministic DH probe fails on the pristine pin and
passes on the reviewed staged repair, for both client and server helpers.

## Inputs and source closure

[`config/inputs.json`](../config/inputs.json) is the source declaration authority.
All revisions, archive roots and SHA-256 values live there. The campaign recipe
yields shared-framework `Source` declarations directly from these entries.

- `s2n-tls`: v1.7.12, `6459002cd61e4bf3bbc53db23f068b21da9ff6bd`.
  The actual C engine and same-pin native oracle share this source input.
- `openssl-legacy`: OpenSSL 1.1.1w,
  `e04bd3433fd84e1861bf258ea37928d9845e6a86`. This supplies pinned reference
  headers and the separate native oracle backend. Native libcrypto is not a
  permitted runtime dependency of the managed product.
- `openssl-peer`: OpenSSL 1.1.1v,
  `9398e4038c27d5ef15111c61e984d4d2be8a52a8`. Independent TLS engine for TLS 1.0–1.3.
- `openssl-peer-sslv3`: OpenSSL 1.0.2u,
  `e818b74be2170fbe957a07b0da4401c2b694b3b8`. Independent TLS engine for all
  fourteen SSLv3 suites. OpenSSL 1.1.1 rejects six of these ECDHE combinations
  through its cipher-version table, so merely enabling SSLv3 there is insufficient.
- `provider-managed`: BouncyCastle.Cryptography 2.7.0,
  `4007498b13582d90ee1eda5d9920c324428b98b3`. Product-group source declaration
  copied from `config/provider.json#managed_dependency`; acquired at
  `ref/bouncycastle`, with its MIT license at `ref/bouncycastle/LICENSE.md`.
  The provider uses selected managed primitives and parsing, not its TLS engine.
  The recorded native matrix predates this acquisition-metadata addition and
  makes no managed-provider qualification claim.

References remain under `ref/<directory>` and are preserved. Explicit s2n archive
selectors acquire the C engine, build system and complete `tests/` subtree,
without selecting unrelated Rust-binding certificate symlinks. The initial fetch
correctly rejected a selected symlink; the extractor was not weakened.
Successful framework acquisition receipts include
[`20261023-200313-154db7b3`](../artifacts/campaign/20261023-200313-154db7b3/receipt.json)
and [`20261023-200932-7845ebbd`](../artifacts/campaign/20261023-200932-7845ebbd/receipt.json).

[`source-inventory.py`](../scripts/source-inventory.py) checks the actual CMake
`S2N_SRC` closure against [`source-manifest.json`](../config/source-manifest.json):
29 crypto, 1 error, 6 stuffer, 124 recursive TLS and 14 utility files, totaling
174 upstream C units. The native build additionally checks all 174 entries
against CMake's exported compilation commands. Authored policy code is recorded
separately from this upstream count.

[`compatibility.json`](../config/compatibility.json) records all 36 names, IANA IDs,
key exchanges, authentication methods and version relationships. Fourteen have
SSLv3 record algorithms; the resulting matrix has 78 version/suite pairs.
`TLS_NULL_WITH_NULL_NULL` remains an internal initialization suite.
The inventory also records 29 compatibility/lifecycle/security controls and
three environment controls with their actual semantics. This includes certificate
time/intent/chain/hostname exceptions, IP-in-CN fallback, blinding, DSS request
compatibility, NPN, client authentication, key logging, serialization and the
additional certificate-validation callback. RC4 connection serialization is
explicitly unsupported upstream. `s2n_rand_set_callbacks` and `s2n_cleanup` are
retained no-ops; actual entropy and finalization must use their real paths.

## Native builds and policy

Native prerequisites are Linux x64, Python 3.11+, a C compiler, CMake, Make, Perl
and ordinary POSIX sockets. Native builds use two jobs and private outputs.
No shared DotCC tool build is performed.

OpenSSL sources are copied to `build/native/openssl-{legacy,peer,peer-sslv3}`
before configuration. The 1.1.1 builds enable `ssl3`, `ssl3-method` and
`weak-ssl-ciphers` and use `no-shared`; 1.0.2 uses its SSLv3-capable native
configuration. Generated product-reference headers are available at
`build/native/openssl-legacy/include/openssl/opensslconf.h` and the surrounding
include tree. The original pinned header tree remains at `ref/openssl-legacy/include`.

The unadapted native library is `build/native/s2n/lib/libs2n.a`. Reviewed
[`adaptations.json`](../config/adaptations.json) replacements are applied with
unique exact anchors to `build/native/s2n-source`; its CMake output is
`build/native/s2n-adapted/lib/libs2n.a`. Managed-only adaptations retain their
native branches. Native controls do not define the managed-provider macro.

The endpoint compiles the **same** authored
[`dotcc_s2n_compat.c`](../src/Boundary/dotcc_s2n_compat.c) used by the product.
It selects compat before loading credentials, narrows each connection through
`dotcc_s2n_policy_new` / `dotcc_s2n_connection_use_policy`, and frees the owning
policy after its connection. It does not copy `security_policy_test_all` or write
connection version fields. All 36 suites, fourteen SSLv3 entries and seventeen
classical signatures are retained, without inheriting PQ KEM preferences.

Executables are `build/native/s2n-endpoint`, `openssl-endpoint` and
`openssl-sslv3-endpoint`. They statically link the declared native TLS/crypto
libraries; recorded `ldd` checks reject unexpected dynamic s2n/libssl/libcrypto
dependencies. These executables are native test controls, not delivered product
backends.

## Assertions and deterministic upstream repair

Each positive exchange runs separate client and server processes over an actual
POSIX stream socket pair. Both assert the forced protocol and IANA cipher ID,
send and verify 1,027 binary bytes in each direction over three application
writes, exchange close-notify, and free connection/configuration/credential
objects. s2n calls `s2n_cleanup_final` after draining its objects. The payload
contains zero bytes and `0xff`; the multi-record traffic exercises RC4 state
continuity and CBC chaining.

The 78 pairs run as s2n/s2n, s2n-client/independent-server and
independent-client/s2n-server: 234 exchanges. Four further assertions require
untrusted-certificate rejection and explicit verification bypass with that same
untrusted credential, for SSLv3/RC4-MD5 and TLS 1.3/AES-128-GCM. Positive matrix
rows validate generated RSA/ECDSA credentials and the `localhost` identity.
No required unavailable cipher or protocol is skipped.

The pristine pin contains a finite-field DHE interoperability defect: it pads
`DH_compute_key` output before the legacy PRF. [RFC 5246 §8.1.2](https://www.rfc-editor.org/rfc/rfc5246#section-8.1.2)
requires removal of leading zero bytes. This produces occasional independent
Finished/record failures that self-talk can mask. The preserved failing full run
[`20261023T171208Z`](../artifacts/native/20261023T171208Z/report.json) has 236 passing
and two failing rows; it is not a successful oracle receipt.

[`dh_leading_zero.c`](../tests/NativeOracle/dh_leading_zero.c) fixes exponents only
inside its native vector executable using a test-only linker interception of
`DH_generate_key`. With a real 2048-bit group and secret `g^4`, the pristine
client **and** server helpers return 256 bytes where the unpadded backend returns
one byte. The pristine executable returns 1. The staged S2N-006 repair preserves
the actual computed bytes and length; both helpers then return one byte and the
executable returns 0. The full interop matrix runs with ordinary cryptographic
randomness, without this interception. No reference file is edited.

## Reproduce and inspect

From the repository root:

```bash
bash s2n-tls/scripts/fetch.sh --fetch missing
python3 s2n-tls/scripts/source-inventory.py
python3 s2n-tls/scripts/native-control.py --fetch never --selection all
```

The native command works from an unrelated working directory when invoked with
its absolute script path. Its `REPORT` line names
`artifacts/native/<UTC-run>/report.json`. That report contains command arguments,
exit codes, log paths, source declarations, adaptations, both deterministic DH
observations, all 174 native C compilation units, feature flags, file identities,
per-exchange negotiated values and per-upstream-case runtime assertion counts.

`--build-only` stops after native builds and deterministic boundary controls.
`--selection required` limits exchange diagnostics to all fourteen SSLv3 and
three TLS 1.3 suites. `--no-build` and `--no-upstream` are explicit diagnostic
omissions and are recorded as such; they do not establish the full native gate.
`--output PATH` chooses a specialist report directory. Native acquisition uses
the shared framework declarations; it does not download undeclared inputs.

Invocation from `/tmp` with the absolute script path, `--fetch never` and
`--build-only` passed in
[`20261023T172109Z/report.json`](../artifacts/native/20261023T172109Z/report.json).
That receipt covers source reuse, native builds and both deterministic DH probes;
the full exchange/assertion receipt remains `20261023T171931Z` above.

The frozen [`upstream-tests.json`](../config/upstream-tests.json) contains 644
source-file/case-family entries: 276 required for translation (258 unit, 18 integration),
286 explicit exclusions and 82 support files. It records assertion sites,
Python test functions and conditional/skip sites. These are static inventory
counts, not executed parameter combinations. The selected native control list
runs sixteen upstream executables and requires a positive `PASSED N tests`
assertion count with no whole-test skip. Their current native execution reports
590,537 assertions. The remaining applicable upstream cases, translated versions
of these tests and all managed product gates still require execution.
