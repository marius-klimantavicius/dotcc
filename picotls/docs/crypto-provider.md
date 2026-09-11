# P1 BCL feasibility and provider boundary contract

Status on 2026-09-11: standalone P1 probes pass on Linux x64 using SDK
10.0.111 / runtime .NET 10.0.11 (Zorin OS 18.1, little endian). This is
capability and ownership evidence, not an implemented provider or translated TLS.
The public contract is the unmodified pinned `include/picotls.h` at revision
`3598470df01264da85157025ed10db0f7e103790`.

Run from the repository root or any working directory:

```sh
/path/to/dotcc/picotls/scripts/probe-boundaries.sh
```

The script fetches/verifies pinned inputs using `scripts/fetch.sh`, then uses the system C
compiler for the native layout oracle, then runs the dependency-free .NET 10
`tests/BoundaryProbes` executable. Builds/tests must remain serial with other
campaign work. Temporary files use `artifacts/tmp/p1`; retained evidence is
`artifacts/p1/boundaries/native-layout.txt` and `managed.log`. No public network
endpoint, system trust-store modification, persistent private key, or native
crypto import is used by the managed executable.

## Executed capability and encoding checks

| Boundary | Executed evidence | Remaining provider work |
| --- | --- | --- |
| SHA-256 / SHA-384 | Independent `IncrementalHash.Clone` after a shared prefix; diverging suffixes checked against one-shot BCL hashes. SNAPSHOT preserves state; RESET returns digest and resets; FREE returns digest and releases handle/allocation. All modes also accept null output, including cleanup-only FREE. | Actual translated HMAC/HKDF and transcript integration; independent published hash/HKDF vectors. |
| Hash callback/lifetime | Static `unmanaged[Cdecl]` function pointers in an unmanaged header; appended `GCHandle` roots crypto state across forced collection; 1,000 allocate/free cycles leave zero counted handles. Oversized `size_t` raises a captured overflow, observed after callback return. | Register callbacks with actual emitted signatures; full connection abort, concurrent independent connections, allocation-failure injection. |
| AES-128/256-GCM | Zero-key/zero-IV 16-byte known-answer ciphertext/tag vectors; decrypt and empty input; TLS-style nonce XOR with big-endian sequence; AAD and tag corruption rejected. This target clears failed-decrypt output. | Mandatory vector/one-shot/decrypt callbacks, IV get/set, bounds and overlap handling, supplementary encryption after ciphertext exists, AES block/mode adapter, key updates and usage limits. |
| P-256 ECDHE | SEC1 uncompressed `04 || X[32] || Y[32]`; fixed private scalar 1 with peer 2G yields exactly the 32-byte big-endian X coordinate of 2G using `DeriveRawSecretAgreement`. Fresh peers agree after export/import. Wrong point lengths/prefix and off-curve (0,0) rejected. | Provider allocation/release/error mapping, further independent key-agreement vectors including leading-zero secrets, negotiated curve restrictions. |
| ECDSA | P-256/SHA-256 `SignData` and `VerifyData` explicitly use `Rfc3279DerSequence`; DER parser checks exactly two positive integers; altered signature rejected. | TLS signature-scheme/key restrictions, native-peer verification and malformed DER corpus. |
| RSA-PSS | RSA-2048/SHA-256 with `RSASignaturePadding.Pss`; altered signature and PKCS#1-v1.5 verification rejected. | TLS RSA key/OID restrictions, independent salt-length/encoding evidence and native interoperability. |
| X.509 and names | Disposable RSA root signs an ECDSA leaf. Explicit custom root trust and server-auth EKU pass; untrusted, expired, future and client-only EKU fail. DNS/IP SAN and one-label wildcard pass; wrong DNS/IP, CN fallback and multi-label wildcard fail. Leaf key verifies a separate signature; altered signing input fails. | Real certificate callback, returned verification state/cleanup, intermediate-chain policies, production revocation decisions and platform matrix. |
| AEAD appended state | Core-owned unmanaged header+handle allocation survives forced GC; success and simulated setup failure dispose BCL state and handle before the simulated core frees memory; algorithm pointer is preserved; handle counts return to zero. | Actual translated allocation pairing and provider callbacks; realistic failure injection. |

Signature round trips use the BCL on both sides and establish API/encoding
feasibility. They are not independent protocol interoperability. Certificate
fixtures set `RevocationMode=NoCheck`, disable certificate downloads and fix chain
verification to the test's captured current time. That choice is an isolated
fixture policy, not a production revocation policy. No Windows, macOS, arm64 or
NativeAOT execution is claimed.

## ABI result and its explicit limit

The native program includes the pinned public header and prints 92 sizes,
alignments and field offsets for 12 provider-related structures and two appended
context structures. Every value matched the hand-authored sequential C# mirror
on this host. Selected sizes (all align to 8 bytes):

| Native structure | Size | Relevant offset |
| --- | ---: | ---: |
| `ptls_iovec_t` | 16 | `len=8` |
| `ptls_buffer_t` | 32 | `is_allocated=24`, `align_bits=25` |
| `ptls_hash_context_t` | 24 | `clone_=16` |
| `ptls_hash_algorithm_t` | 96 | `empty_digest=32` |
| `ptls_cipher_context_t` | 32 | `do_transform=24` |
| `ptls_cipher_algorithm_t` | 48 | `context_size=32` |
| `ptls_aead_context_t` | 80 | `do_decrypt=72` |
| `ptls_aead_algorithm_t` | 104 | `align_bits=81`, `context_size=88` |
| `ptls_aead_supplementary_encryption_t` | 32 | `output=16` |
| `ptls_key_exchange_context_t` | 32 | `on_exchange=24` |
| `ptls_key_exchange_algorithm_t` | 40 | `create=8` |
| `ptls_verify_certificate_t` | 16 | `algos=8` |
| Probe hash header + handle | 32 | `handle=24` |
| Probe AEAD header + handle | 88 | `handle=80` |

These mirrors are **test-only handwritten declarations**. They do not establish
that dotcc emits correct layouts. The real-header preprocessing blocker recorded
in `blockers.md` currently prevents that comparison. P2 must replace or supplement
these checks with emitted types and layout metadata, including bitfield semantics
(the current oracle validates fields around `non_temporal`, not bit writes).
Full `ptls_context_t` and handshake-property layout remain untested. No offset
source generator has been introduced.

The C# frontend's ordinary translated callbacks use managed function pointers.
The standalone hash experiment deliberately exercises a stricter unmanaged Cdecl
boundary and exception containment; its pointer types must not be blindly copied
into the translated provider. P2 must use precisely the actual emitted calling
convention and signatures. Do not cast between managed and unmanaged function
pointers to bypass a compiler mismatch.

## Chosen registration and ownership contract (design for P3)

Register the provider explicitly before constructing a connection. Keep canonical
static callback pointer fields and stable unmanaged algorithm/callback tables for
the lifetime of all referring contexts. Build only fully implemented and
successfully probed algorithm lists. Retained strings, chains and pointer arrays
need stable owned storage; movable managed arrays and stack-backed tables cannot
be retained by translated C. Algorithm registrations are immutable after use.
The owning context roots shared provider configuration; each connection is used
serially, while independent connections may run concurrently.

Use translated headers as the unmanaged prefix and a pointer-sized opaque
`GCHandle` token for managed state. Never store a managed object reference in a
C-copied structure. Allocate `context_size` using the actual emitted header size,
alignment and tail layout, and pair allocation/free with the translated runtime's
allocator. The standalone probes use `NativeMemory` only within their own paired
allocations; they do not prove cross-allocator compatibility.

Ownership follows the pinned implementation:

- Hash contexts are provider-owned. FREE always disposes crypto state, releases
  its handle and frees the context, whether or not output is requested. RESET
  resets even without output; SNAPSHOT never changes state. Cloning owns a fresh
  hash and handle. Finalization after FREE is invalid, not an idempotent operation.
- Cipher and AEAD allocations are core-owned. In `lib/picotls.c`, creation
  initializes only the public prefix; setup must initialize the undefined tail.
  On setup error the core directly frees memory without invoking disposal, so
  setup must unwind every partial provider resource before returning failure.
  Successful disposal frees provider state/handles only, then the core frees the
  allocation. Preserve the core-owned `algo` pointer.
- Key exchange contexts/public-key storage are provider-owned until release.
  `on_exchange` with null secret is cleanup-only; `release` frees state and sets
  `*keyex` to null. Returned secret/public-key byte buffers must be compatible
  with the core's `free`. Failure leaves outputs unchanged or zero-cleared as the
  public header requires.
- Certificate verification returns one signature callback plus opaque state.
  That state owns the leaf verification key and required temporary resources.
  Normal signature verification consumes/disposes it; a call with both input and
  signature empty is the upstream abort-cleanup path and must release it without
  attempting signature verification. Setup failure releases unpublished state.
  The empty-buffer protocol is a design decision here, not yet an executed
  provider/handshake test.

## Failure and encoding contract (design for P3)

Every pointer length must undergo checked `size_t` to span conversion; validate
null/length pairs, bound gathered iovec totals and scratch allocations, and clear
secret scratch in `finally`. Validate P-256 point length/prefix before import and
use the raw agreement, never a BCL KDF. ECDSA uses DER; RSA uses negotiated PSS.
Sign the supplied CertificateVerify input once with `SignData` and its negotiated
hash, avoiding an extra transcript hash before that call.

AEAD decryption maps authentication failure to `SIZE_MAX` and does not expose
unauthenticated plaintext. Use private scratch or explicitly clear output on all
failure paths; do not generalize the observed Linux bad-tag buffer-clearing
behavior to every exception or platform. Respect IV XOR sequence encoding and
run supplementary cipher input only after the ciphertext it may reference exists.
Deprecated incremental AEAD entries remain unselected; mandatory callbacks must
not delegate through an unimplemented incremental path.

Catch BCL exceptions inside every externally callable ABI callback. Map ordinary
failure to the header's nonzero PTLS error, null pointer, or `SIZE_MAX` as
appropriate; populate output pointers only after successful initialization.
Callbacks without error returns latch the first failure in the owning managed
entry scope. That scope checks the latch on return, discards buffered output and
tears down the failed connection. The probe demonstrates a captured exception
without crossing an unmanaged boundary, but uses a simple thread-local latch;
production requires scoped nesting and a context association, with no async
migration during a translated call. Validate failure paths after translation:
continued internal execution after a latched void-callback failure must neither
publish output nor violate memory invariants. Cleanup must not replace the first
failure. Unrecoverable process exceptions are not converted into TLS success.

Unsupported algorithms and incomplete callback tables are never advertised.
The initial provider will require P-256, both AES-GCM/SHA suites, and the selected
ECDSA/RSA schemes. X25519, Ed25519, ChaCha20, AEGIS, hybrid/PQ, tickets, early data,
HPKE/ECH and QUIC are not established by these probes. Existing core HMAC/HKDF
must remain translated. Randomness/time, cipher supplementary semantics and
all TLS-level behavior remain P3/P4 work.
