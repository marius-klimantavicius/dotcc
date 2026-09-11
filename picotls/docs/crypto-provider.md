# BCL provider contract

The product is the unchanged pinned picotls core at revision
`3598470df01264da85157025ed10db0f7e103790`, translated to C# and referenced by
`src/BclProvider/BclProvider.csproj`. The provider and owning TLS facade live in
`Managed.Security`. Protocol state machines, HMAC and HKDF remain translated core
code; application cryptographic operations use .NET 10 BCL APIs.

## Current execution evidence

The complete Linux x64 raw/optimized × JIT/NativeAOT matrix passes. Receipt:
`artifacts/tests/PASS.json`, run `artifacts/tests/run-lqwhjcso`.

| Evidence in each variant | Result |
| --- | --- |
| Actual provider vectors | 2033 checks, including 124 real-handshake allocation boundaries and two failed ticket-clone paths |
| Actual TLS facade | All scenarios pass; final assertion counts are 9967/9961/9961/9965 for raw JIT/raw AOT/optimized JIT/optimized AOT, varying only with fragment iterations |
| Actual emitted ABI | 92 layout comparisons match the native oracle |
| Direct upstream utilities | Eight cases, 232 checks |
| Translated-peer interoperability | 56 cases with native picotls and independent BCL SslStream peers in both directions; 224 total executions |
| Product dependency audit | Zero violations and zero missing inputs; `artifacts/dependencies/report.json` |

Public results match across variants. All inspected suite and nested peer publish
logs have zero IL/trim/AOT warnings. Ordinary CS8981 lowercase native-type naming
warnings remain. Windows, macOS and arm64 product execution is unavailable and
unverified; Linux evidence does not establish those targets. See [usage.md](usage.md),
[dependencies.md](dependencies.md) and [upstream-tests.md](upstream-tests.md).

The provider project enables `IsAotCompatible`. Its callback registrations are
statically bound, state types are known at compile time, and it does not depend on
dynamic assembly loading or runtime code generation. These are NativeAOT design
constraints; successful compilation, warning review and execution of the actual
published product remain separate requirements.

## Advertised profile and BCL operations

| Surface | Implemented contract |
| --- | --- |
| TLS cipher suites | TLS 1.3 `TLS_AES_128_GCM_SHA256` (0x1301) and `TLS_AES_256_GCM_SHA384` (0x1302). |
| Hash | SHA-256/SHA-384 through `IncrementalHash`: independent clone, update, snapshot, reset and free; final modes accept null output. |
| Symmetric encryption | `AesGcm` one-shot/vector encryption, authenticated decryption and IV get/set. ECB uses BCL AES block operations; CTR adapts those operations with a big-endian counter and partial-block state. |
| Key exchange | Group 23 P-256 only. SEC1 uncompressed 65-byte public points; BCL `DeriveRawSecretAgreement` supplies the 32-byte raw secret without an added KDF. Malformed and off-curve peer points are rejected. |
| CertificateVerify | P-256 ECDSA/SHA-256 (0x0403), explicitly DER encoded; RSA-PSS with SHA-256 (0x0804), `rsaEncryption` certificate keys of at least 2048 bits. The supplied signing input is hashed once using `SignData`. |
| Certificate validation | Explicit custom trust roots, caller-selected revocation mode, appropriate server/client EKU, key-usage validation, and SAN endpoint matching for servers. |
| Entropy and time | `RandomNumberGenerator.Fill` and UTC wall-clock milliseconds through scoped callbacks. |
| Session tickets | Explicitly configured owning AES-256-GCM ticket protector; fresh DHE required on resumption, early data disabled. |

Algorithm lists are terminated and immutable after publication. Initialization
probes required BCL hash-clone, AES and P-256 capabilities before publishing
complete tables; table memory intentionally has process lifetime. AEAD advertises
confidentiality and integrity limits of 2^25 and 2^54 respectively to the core.
The provider does not configure TLS 1.2, X25519, Ed25519, ChaCha20, AEGIS,
hybrid/PQ, ECH or QUIC. Translated helper code existing in the assembly does not
advertise those features through the facade.

BCL-only describes the authored API and dependency boundary, not the absence of
native platform libraries. .NET cryptography and certificate validation use
platform implementations; see Microsoft's [cross-platform cryptography
documentation](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography).
The product provider has no authored P/Invoke crypto layer, dynamic library
loading, runtime code generation or reflection-based callback binding.

## Actual ABI and allocation ownership

The provider uses the actual emitted `st_ptls_*` structs and
`en_ptls_hash_final_mode_t` enum. LP64 `size_t` is `ulong`. Callback pointers are
ordinary managed `delegate*<...>` pointers with exact emitted parameter types,
held in canonical static readonly fields. They are not `unmanaged[Cdecl]`
pointers; casting between those conventions is unsupported. No hand-authored
mirror ABI types are used in the product.

Appended provider contexts have an actual emitted unmanaged header prefix and an
opaque pointer-sized GCHandle token. Retained keys and state remain managed
objects rooted through that handle. Retained certificate bytes and pointer arrays
have stable owned storage. Allocations passed back to the core use its embedded
`Libc.malloc`/`free` allocator, with checked conversions; standalone probe
`NativeMemory` allocations are not mixed into these lifetimes.

- Hash contexts and clones are provider-owned. Ordinary FREE releases BCL state,
  handle and allocation exactly once, including null-output cleanup. RESET resets
  even without output; SNAPSHOT preserves the hash. An ordinary freed hash cannot
  be reused.
- Cipher/AEAD context allocations are core-owned. Setup initializes the undefined
  tail, preserves `algo`, and unwinds partial BCL/handle ownership on failure. The
  core frees failed setup allocations without calling provider disposal. After
  success, provider disposal releases only appended state; the core frees memory.
- Key-exchange contexts/public keys are provider-owned until release. Cleanup-only
  `on_exchange` accepts a null secret output. Release nulls the caller's context
  pointer; returned public and secret buffers use the core allocator.
- Certificate verification publishes a signature callback and opaque leaf-key
  state only after validation. Signature verification consumes that state. The
  upstream abort callback with both data and signature lengths zero releases it
  without attempting a signature check.
- A usable empty core buffer has a stable non-null base, zero capacity and zero
  offset. Use `PicotlsBuffer.Create()` for raw callers. A default zeroed buffer
  does not satisfy the upstream reserve contract.

`SigningIdentity`, `CertificateVerifier` and `TicketProtector` own their callback
storage. The facade retains leases on these owners; disposing an owner defers
resource destruction while contexts retain it. `PicotlsContext` is immutable and
shareable. Disposal prevents new connections while existing connections retain
its resources. Each `PicotlsConnection` serializes its synchronous operations;
independent connections can run concurrently. Direct raw `ApplyTo` use requires
the caller to retain owners and synchronize mutation/disposal for the whole
referring context lifetime; it does not acquire a facade lease automatically.

## Callback failure and output contract

Every raw translated call that can invoke the provider requires
`CallbackScope.Enter()`. The caller must call `ThrowIfFailed()` before publishing
any result bytes. Facade entry points supply this boundary automatically. Scopes
stay on their creating thread and are disposed in stack order. Nested scopes
preserve and propagate the first captured exception; cleanup cannot replace it.
Unscoped callbacks fail before cryptographic work and latch an orphan error,
which can be explicitly drained or is carried into the next scope.

Provider exceptions map to the callback's error convention and are latched.
Authentication rejection is an ordinary protocol result: for example, AEAD bad
authentication returns `SIZE_MAX`, and rejected tickets return
`PTLS_ERROR_SESSION_NOT_FOUND`. Private AEAD scratch prevents unauthenticated
plaintext publication; checked error paths clear output and scratch. One callback
or gathered message is limited to 16 MiB, with at most 1,024 iovecs. Gather totals,
pointer/length pairs, output lengths and integer conversions are checked. Scratch
allows overlapping input/output. Supplementary encryption reads its input after
AEAD ciphertext has been written, as required by the pinned core.

Hash clone failure has one special ownership rule. The pinned
`send_session_ticket` stores an unchecked clone result as its transcript, and
later cleanup dereferences that pointer. A failed clone therefore returns an
immutable process-lifetime poison hash after recording the original exception.
It owns no handle, produces no digest, clones only itself and permits cleanup
without freeing table memory. Any attempt to use it in a fresh or unscoped call
records another failure; it cannot become a working hash by resetting the scope.
Ordinary hash creation still returns null on failure. This preserves cleanup
invariants while the facade rejects output and aborts the connection.

`HandshakeAllocationVectors` first completes an actual authenticated handshake
with ticket issuance, then injects failure at every measured provider ownership
allocation boundary using fresh contexts/connections. Its scoped hook records
exception identity and failed-clone hits, checks no result is returned by the
failing facade call, and checks handle/key cleanup after disposal and finalizer
drains. It is capped at 512 measured boundaries. All four variants pass all 124
measured boundaries, including two actual ticket-clone failures. This does not claim exhaustive failure injection into BCL
internals or every translated-core allocation.

## Certificate and ticket policy

`CertificateVerifier` requires at least one explicit trust root and an explicit
`X509RevocationMode`. It uses `CustomRootTrust`, peer-supplied intermediates, the
appropriate TLS EKU, revocation checking excluding the root, disabled certificate
downloads and a two-second URL retrieval timeout. Platform chain behavior still
applies, and an online revocation policy can require network access. Fixtures use
`NoCheck` for isolated generated credentials; that fixture choice is not a
production revocation recommendation.

Server identity matching uses DNS/IP SAN with optional wildcard matching and no
common-name fallback. Present KeyUsage must permit digital signatures. Named
P-256 and supported RSA leaf keys are validated independently of chain building.
Mutual TLS validates the client-auth EKU and trust chain; server endpoint-name
matching applies on the client side. Trust, name and signature failures never
become a successful handshake.

`TicketProtector` binds permanently to one immutable server context to keep keys
from crossing authentication policies. AES-256-GCM authenticates the complete
version/key-id/issue-time/expiry/nonce header. Each key has independent random key
material and a 128-bit identifier; nonces combine a random prefix and a monotonic
per-key counter, with at most 2^32 encryptions per key. Lifetime is an explicit
whole number of seconds from 1 to 604800. Rotation retains a configured zero to
eight prior keys and discards expired retired keys; extra rotations can invalidate
otherwise unexpired tickets early. Disposal clears owned material after context
leases end. Invalid, tampered, unknown-key, future-issued and expired tickets
leave the destination unchanged. Successful ticket decryption explicitly rejects
early data, and the context requires fresh DHE for resumed sessions.

Client tickets are connection-local owning `SavedSessionTicket` objects.
`TakeSessionTickets` transfers them to the caller; disposal clears their stored
PSK-bearing bytes. `Export` returns a separate caller-owned copy. The facade
rejects application writes before handshake completion, including resumed
connections, and never offers early data.

## Historical P1 evidence

`tests/BoundaryProbes` and `scripts/probe-boundaries.sh` are the earlier standalone
BCL feasibility experiment. Their hand-authored mirror structs and deliberately
unmanaged Cdecl callback probe established API/encoding feasibility only. They
are not product ABI declarations, provider execution or TLS interoperability.
Historical logs remain under `artifacts/p1/boundaries/`; the executed product ABI,
provider and peer runs above supersede the old “provider not implemented” status.
