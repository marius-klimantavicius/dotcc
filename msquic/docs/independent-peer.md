# Independent QUIC reference

The test-only independent implementation is **aioquic 1.3.0**, revision
`cb103537126bbcc58752fd5bc7c8ac88bf552c01`. It provides its own QUIC transport and
TLS handshake implementation; its packet primitives use native dependencies.
It runs in a separate Python process and is never loaded into the translated
product. [Upstream source](https://github.com/aiortc/aioquic/tree/cb103537126bbcc58752fd5bc7c8ac88bf552c01).

`config/independent-inputs.json` pins the immutable source archive and ten Python
wheels by URL and SHA-256. The setup script verifies every archived source file,
installs only the hash-locked wheels into a campaign virtual environment, checks
dependency consistency, and compares all installed aioquic Python source files
with the Git source pin. Packet-primitive extension binaries are identified by
their official distribution wheel hashes; they are not locally rebuilt.

These wheel pins qualify **CPython 3.12 / Linux x64 / glibc 2.34 or newer**.
They do not claim portable wheel availability. The upstream license is
BSD-3-Clause and remains in the unchanged reference snapshot.

## Reproduce

First build the native reference with `scripts/native-oracle.py`, as described
in [source.md](source.md). Then, from the repository root:

```sh
python3 msquic/scripts/independent-peer.py
python3 msquic/scripts/test-independent-peer.py
```

The setup uses the system Python's `pip` to populate an isolated environment;
it does not install packages into the system environment or require `ensurepip`.
The interop runner builds its native test peer against the pinned native MsQuic
library, generates short-lived test certificates, and controls both processes
with bounded deadlines. Each server binds an ephemeral loopback port and reports
it before the client starts. Logs, commands, source and binary hashes, certificate
hashes, and per-case results are under `artifacts/independent-peer/`.

## Verified on the first runner

The matrix passes **16 positive cases**:

- aioquic client / native MsQuic server and native MsQuic client / aioquic server;
- IPv4 and IPv6;
- AES-128-GCM/SHA-256 and AES-256-GCM/SHA-384;
- ECDSA P256 and RSA server certificates.

Every connection negotiates QUIC v1, TLS group 23 (P256), and ALPN `dotcc-probe`.
Both endpoints verify the selected cipher and group. Each client sends 65,537
deterministically patterned bytes and FIN; the server verifies the complete
payload, replies with a distinct 65,537-byte pattern and FIN, and the client
verifies it before closing. Clients enable certificate-chain and hostname
validation using explicit test trust. Server certificates contain localhost
and loopback address SANs; client certificate authentication is outside this
probe. No session tickets are supplied and no early data or resumed handshake
is accepted by the case assertions.

Four negative cases cover unrelated trust and wrong server name with each stack
as client. They require an observed QUIC TLS certificate alert (42 or 48 as
appropriate), failed validation, and zero application bytes at both endpoints.
A timeout or arbitrary setup failure cannot satisfy a negative case.

The native oracle restricts groups through its OpenSSL configuration and queries
the negotiated group through MsQuic's handshake-info API. The pinned aioquic
release exposes cipher configuration publicly, but the harness sets its TLS
group list immediately after connection initialization through a private hook.
It checks the actual ECDH key curve and absence of X25519/X448 keys after the
handshake. These test-only private hooks are tied to the source pin; no upstream
source file is edited. The native handshake query independently confirms P256.

The native peer's original same-process mode remains covered by
`scripts/test-native-peer.py`: eight positive cases and two authentication
negative cases still pass after adding external client/server modes.

This completes the independent-reference pin and baseline interoperability
subgate. **It is not translated MsQuic interoperability evidence.** Recovery,
fault injection, Retry, key update, DATAGRAM, resumption, and translated peers
remain downstream campaign work.
