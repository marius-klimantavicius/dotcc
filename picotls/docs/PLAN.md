# Translate picotls to C# with dotcc

Status: **Linux x64 implementation and all available validation complete;
Windows/macOS and additional architectures remain unverified** (2026-09-11).
The unchanged core, BCL-only provider, owning API and consumer pass the complete
raw/optimized × JIT/NativeAOT matrix. Each variant passes 92 actual ABI checks,
2033 provider checks (including 124 real-handshake allocation boundaries and two
ticket-clone failures), eight upstream utility cases/232 checks, all TLS scenarios
and 56 independent peer cases: 224 peer executions total. Public results match.
The dependency audit passes with zero violations and zero missing inputs.
Shared compiler/libc and complete preserved SQLite regressions also pass.
The remaining unchecked items below require execution machines for other
platforms; none were available in this environment. See [validation.md](validation.md),
[blockers.md](blockers.md), [crypto-provider.md](crypto-provider.md), and
[usage.md](usage.md).
Campaign working directory: `<repo>/picotls/`.

## Objective and boundaries

Translate upstream picotls's real TLS protocol implementation with dotcc into a
reusable unsafe C# library. Keep its state machine, record processing, transcript,
key schedule, handshake parsing, and protocol validation translated from C.
Supply external cryptographic operations through an explicitly registered C#
provider using `System.Security.Cryptography` and other BCL APIs. `SslStream` can
be an interoperability peer; replacing picotls with it does not satisfy the goal.

Start from the pinned latest source in [source.md](source.md). There is no upstream
amalgamation requirement: compile the actual core translation units and their
headers. Preserve original downloads. Configuration and small host/provider
adapters belong to this campaign; generic parser, IR, emission, runtime, or
source-linker fixes belong in dotcc with regression tests. Do not patch generated
C# or rewrite TLS algorithms to bypass compiler bugs.

Dotcc now has source/object linking, custom class names and namespaces, split
output, macro constants, layout computation, cached function pointers, and span
varargs. These are useful starting capabilities, **not evidence that picotls can
already parse, compile, or run**. Expect to discover and repair missing C and
emission cases, then retry the same actual upstream input after every fix.

Use dotcc's supplied headers, libc implementations, and existing ports for
noncryptographic dependencies. Do not pull OpenSSL, minicrypto/cifra/micro-ecc,
fusion assembly, or libaegis into the managed product to obtain crypto. Normal
BCL use of platform crypto libraries is acceptable; a separate application-owned
P/Invoke crypto layer or native picotls dependency is outside this plan. Native
picotls/OpenSSL are allowed as test oracles in separate processes.

Unsafe pointers and function-pointer tables are valid APIs. Keep callback pointers
in canonical static fields and register providers explicitly; no dynamic loading
or reflection-based dispatch. Target .NET 10/C# 14 and preserve NativeAOT support.
Commit locally after each coherent tested change; do not push. Keep this plan and
picotls implementation on the `sqlite` branch, preserving existing SQLite work.
Do not create or switch to a separate picotls branch. The user authorized a coordinator and sub-agents to continue all remaining
phases, starting with generic BCL-only, NativeAOT-compatible pthread and
`posix_memalign` support. Continue until the acceptance criteria pass; record any
unavailable execution targets or external blockers honestly.

## Workspace and repeatable inputs

```text
picotls/
  README.md
  ref/                       unmodified upstream archives, sources, test dependencies
  docs/
    PLAN.md                  milestones and acceptance criteria
    source.md                immutable revisions, URLs, hashes, licenses, provenance
    configuration.md         source closure, defines, ABI and negotiated capabilities
    crypto-provider.md       callback ownership, encodings, error and platform matrix
    blockers.md              reduced failing cases, compiler fixes, real-source retries
    validation.md            exact commands, results, exclusions and evidence
    usage.md                 translation/build instructions and managed API examples
  config/                    source list, feature definitions, provider profile
  scripts/                   fetch, preprocess, translate, build-only, test, oracle
  src/                       C boundary declarations and BCL provider/managed facade
  tests/                     provider vectors, ABI, managed consumers, TLS interoperability
  generated/                 source/project output, ignored
  build/                     native oracle and publish output, ignored
  artifacts/                 logs, preprocessed C, reproducers, measurements, ignored
```

Keep every campaign-specific file inside `picotls/`. Scripts resolve their own
location and support invocation from `picotls/`. Reference inputs, generated C#,
and artifacts remain ignored; commit recipes, tests, docs, and authored adapters.
Pin native test dependencies and record compiler/.NET versions too. Production
keys and arbitrary public network endpoints are unnecessary: use disposable test
certificates, local peers, and deterministic vectors where appropriate.

## Initial feature profile

The following is the first complete, useful profile. Keep other upstream code
translatable where practical, but do not advertise unverified protocol features.

| Area | Initial target |
| --- | --- |
| TLS | TLS 1.3 client and server; authenticated full handshakes, record send/receive, fragmentation, alerts and orderly shutdown. Do not enable TLS 1.2 compatibility yet. |
| Algorithms | P-256 ECDHE; TLS_AES_128_GCM_SHA256 and TLS_AES_256_GCM_SHA384; ECDSA P-256/SHA-256 and RSA-PSS/SHA-256 authentication, with compatible certificates. |
| Application API | Low-level translated API plus a small owning C# connection/context facade; SNI, ALPN, exporter and key-update tests. |
| Credentials | BCL-loaded certificate chains/private keys; explicit trust policy, chain and endpoint-name validation, CertificateVerify verification. Cover optional client authentication after the basic client/server path. |
| Resumption | Session tickets and PSK-DHE resumption after full-handshake validation. Implement secure ticket protection and rotation. Early data is off by default; it needs a separate replay-policy gate. |
| Transport | Picotls consumes/produces bytes; use memory queues first, then BCL streams/sockets. No fake transport or crypto in the final consumer. |
| Threads | Independent connections may run concurrently. Serialize access to each connection; document shared configuration/provider and disposal lifetimes. |
| Platforms | Linux x64 first, then Windows and macOS on supported 64-bit .NET targets. Track x64/arm64 separately; do not claim support without execution evidence. |
| Diagnostics | Initially `PTLS_HAVE_LOG=0`, `PICOTLS_USE_DTRACE=0`, certificate compression disabled. Audit actual remaining headers, time and OS references rather than impersonating another OS/compiler. |

ChaCha20-Poly1305 and additional NIST curves can be added after capability probes
and vectors. Do not assume that X25519, Ed25519, AEGIS, or hybrid/PQ groups are
available through the selected BCL APIs on every platform. Omit unsupported groups
and signature schemes from advertised lists and record the limitation; do not
substitute algorithms or silently add another crypto dependency.

Full QUIC transport, raw public keys, ECH, optional HPKE suites, certificate
compression, FFX/QUIC-LB ciphers, async signing, and 0-RTT are follow-up profiles.
Translate core `hpke.c` as part of the selected source closure, but distinguish
its presence from a tested public feature. Handshake-message APIs for a future
QUIC consumer should remain usable; full QUIC packet protection is a separate
contract, especially where BCL exposes an AEAD but no raw ChaCha20 cipher.

## Provider design to implement and verify

Implement the provider against the translated `ptls_*` structs and callback
signatures, rather than emulating OpenSSL's API. The pinned header is the contract;
use upstream backend code as reference and independent vectors as validation.

| Picotls requirement | Proposed BCL implementation and required checks |
| --- | --- |
| Secure random bytes | `RandomNumberGenerator.Fill`; support arbitrary lengths, allocation errors and concurrent calls. Deterministic RNG only in isolated test fixtures. |
| SHA-256/SHA-384 | `IncrementalHash.CreateHash`, `AppendData`, `GetCurrentHash`, `GetHashAndReset`, `Clone`, and `Dispose`. Map SNAPSHOT/RESET/FREE exactly, including null-output cleanup calls and independent clones. No transcript replay buffer is needed if clone support passes on the target. |
| HMAC and HKDF | Keep upstream translated HMAC/HKDF and TLS HKDF-Expand-Label over the hash provider. Check byte-for-byte against vectors and independent BCL HMAC/HKDF results; do not change TLS's label encoding or schedule. |
| AES-GCM | `AesGcm` with TLS's nonce, tag and AAD rules. Implement required `do_encrypt`, `do_encrypt_v`, `do_decrypt`, and IV get/set. Validate nonce XOR/sequence handling, key updates, limits, empty input and rejected tags. |
| Vector/supplementary encryption | Gather bounded iovecs when BCL requires contiguous input; handle overlap deliberately. Mandatory one-shot supplementary encryption must obey upstream ordering, including input pointing into freshly written ciphertext. Implement needed AES cipher callbacks using BCL AES block operations and a tested mode adapter. |
| Deprecated incremental AEAD API | Do not route mandatory callbacks through an unimplemented init/update/final trio. Initially document these deprecated entry points as unsupported, leaving them unselected; add compatible buffering/semantics only if a supported consumer requires them. |
| ECDHE P-256 | `ECDiffieHellman`, explicit curve/key import/export and `DeriveRawSecretAgreement`. Validate SEC1 uncompressed points, coordinate widths, invalid points, raw-secret length and byte order against upstream vectors. Do not use a method that adds a KDF. |
| Signatures | `ECDsa` with explicit TLS-compatible DER signature encoding; `RSA` with PSS padding and the negotiated hash. Respect TLS signature-scheme/key restrictions and the exact provided signing input, avoiding an extra transcript hash. |
| Certificate verification | BCL X.509 loading and `X509Chain`, explicit trust roots, validity/EKU/revocation policy, and separate DNS/IP endpoint-name checks. Verify CertificateVerify with the leaf key; a successful chain build alone is insufficient. |
| Time and tickets | BCL time callbacks with picotls's required units; authenticated ticket protection, independent ticket keys, rotation/lifetime and explicit replay policy. Test ticket failures and expiry. |

The BCL provider may contain unmanaged context structs with a translated header
prefix and an opaque handle to managed crypto state. Never put managed references
inside structs copied by C. Define `context_size`, alignment, allocation/free
ownership, rooting and handle disposal per callback. Core-owned AEAD/cipher memory
and provider-owned hash/key-exchange memory can have different release rules.
Use dotcc layout metadata and native layout oracles; add no offset source generator
unless a demonstrated gap actually requires one.

Model certificate verification's returned callback/state and its empty-input
cleanup invocation. Make aborts, failed handshakes, partial initialization,
concurrent use, disposal, and forced GC part of the ownership tests. Static
callback pointers must outlive every referring connection; no stack-lived table
or movable managed buffer can back a retained C pointer.

Map authentication failure to upstream failure results (AEAD uses `SIZE_MAX`),
never success with junk plaintext. Define propagation for BCL exceptions, including
callbacks without error return values, at the owning managed entry wrapper; ensure
failed connections are torn down and no exception escapes a native ABI boundary.
Use checked `size_t`/span conversions, bound vector totals and allocations, clear
secret scratch storage, and never expose unauthenticated plaintext. Disable
unsupported algorithms before negotiation, and fail clearly if the required
initial profile is unavailable on the running platform.

## Milestones

### P0 — Freeze inputs and establish the campaign

- [x] Confirm the current latest upstream revision at implementation start; keep
      or deliberately replace the recorded snapshot, with archive hash/provenance.
- [x] Stay on the `sqlite` branch and create a repeatable verified fetch recipe, including
      exact test-submodule revisions and licenses. Preserve original sources.
- [x] Record the effective C source list, includes, feature definitions, LP64
      assumptions, endianness, and .NET/platform matrix. Avoid accidental optional
      dependencies caused by CMake discovering system libraries.
- [x] Build a pinned native picotls oracle with a real crypto backend and run its
      relevant upstream tests. Record enabled algorithms and oracle dependencies.

### P1 — Prove the compiler and BCL boundaries

- [x] Attempt preprocessing of the core files separately with dotcc; inventory
      unresolved libc, OS and header declarations, crypto symbols, exported APIs
      and callback types. All three initially produced invalid token-paste output;
      see [compiler-boundary.md](compiler-boundary.md) and saved diagnostics.
- [x] Prototype BCL capability/encoding checks: cloneable hashes, AES-GCM,
      raw P-256 agreement, ECDSA DER signatures, RSA-PSS and X.509/name checks.
      These are provider feasibility probes, not completion of TLS translation.
- [x] Validate ABI layouts and how appended native context storage holds managed
      handles. Decide the provider registration and callback failure contracts.
      Native/C# mirrors and actual emitted runtime types each pass 92 comparisons;
      actual-header metadata passes 64 checks. Appended handle lifetime and
      callback failure/registration checks pass against the actual provider.
- [x] Save every observed real-source blocker with command, reduced reproducer and expected
      native behavior. Select an initial source/object linking approach; preserve
      multi-translation-unit boundaries for static and inline semantics. Source
      linking is selected; actual-core linked execution requires P2.

### P2 — Repair dotcc until the real core emits and compiles

Chained token pasting, diagnostic format annotations, GNU thread-storage,
extern qualified callbacks, qualified specifier runs, inline enum types and
const pointer tails have been repaired with regressions. The bitfield-tail
layout repair passes actual-header metadata and a native/emitted/AOT storage
reducer. All unchanged core sources now emit, source-link, postprocess and compile;
see [blockers.md](blockers.md) for B1–B20 repairs and regression evidence. All remaining
phases are authorized.

- [x] For each parse, IR, emission or runtime failure: reduce it; add a failing
      compiler/functional regression; fix the shared implementation; run relevant
      tests; retry the same real picotls files; record the result; commit locally.
- [x] Cover encountered callback macros, nested function pointers, aggregates,
      bitfields, compound literals, inline functions and cross-unit symbols based
      on actual failures, rather than speculative rewrites.
- [x] Resolve remaining platform needs using dotcc runtime/BCL adapters, including
      default time helpers even when a custom context callback is supplied.
- [x] Emit `TranslatedPicotls` with class `PicoTls`, namespace `Managed.Security`,
      and approximately 100 KiB function groups. Use `--nest-types --runtime=c`:
      shared `PicoTls.cs` contains nested types and runtime helpers, with
      file-local aliases and no global-usings sidecar.
      Whole functions may exceed the byte target.
- [x] Build the unoptimized generated project with typed provider scaffolding.
      Temporary failure-returning scaffolds are test-only and cannot satisfy P3/P4.

### P3 — Complete the BCL provider

- [x] Implement registration, stable function pointers, context ownership, errors,
      secure randomness and time; test allocation/abort/disposal/GC behavior.
- [x] Implement hash cloning/finalization, AEAD/cipher callbacks, P-256 exchange,
      signing and certificate verification. Verify every advertised algorithm.
- [x] Run independent known-answer and negative vectors for hashes/HMAC/HKDF,
      AEAD, raw key agreement and signatures; cover split buffers and overlaps.
- [ ] Prove capability detection and failure behavior on each target. Record
      unsupported algorithms explicitly; never advertise a partially implemented
      provider table or skip certificate authentication to make a handshake pass.
      **Linux x64 passes all four variants.** Windows/macOS/arm64 execution is
      unavailable here and remains open; no support claim is made for them.

### P4 — Real TLS and interoperability

- [x] Run translated-client ↔ translated-server exchanges with disposable trusted
      certificates: both AES suites, ECDSA/RSA authentication, ALPN and SNI.
- [x] Exercise fragmented/coalesced handshakes and records, empty and large payloads,
      exporter agreement, HelloRetryRequest, key updates, alerts and close_notify.
- [x] Run both translated/native client/server pairings, then an independent
      TLS 1.3 peer such as BCL `SslStream` or OpenSSL. Use local transports.
- [x] Test untrusted, expired, wrong-name and incompatible certificates, invalid
      CertificateVerify/Finished, corrupt tags, truncated/oversized messages,
      unsupported algorithms and failure cleanup; authentication must fail closed.
- [x] Add session tickets, PSK-DHE resumption, expiry/rotation and rejection tests;
      confirm early data stays disabled in the default profile.
- [x] Port applicable upstream tests with a case-level pass/skip/blocker inventory.
      No blanket skip for the provider's missing features and no claim of full
      upstream coverage from a single managed round trip.

### P5 — Packaging, platforms and sustained validation

- [x] Add a ManagedConsumer solution containing the translated project/provider
      and a practical client/server example; expose raw callbacks as well as the
      owning managed API. It must actually exchange and authenticate application data.
- [x] Add separate translation, build-only, test and oracle scripts. Translation
      runs dotcc first, then the existing semantic postprocessor in place; compare
      raw versus optimized behavior and preserve manifest-based cleanup.
- [ ] Publish and run NativeAOT tests, then validate Windows/macOS and additional
      architecture targets independently. Record unrun targets honestly.
      **Linux x64 raw/optimized JIT/NativeAOT passes.** Windows/macOS and other
      architectures remain unrun because no execution targets were available.
- [x] Add bounded malformed-input/fuzz corpus runs, parallel independent connections,
      cancellation/abort/GC stress, and memory/handle-leak checks with timeouts.
- [x] Run appropriate compiler/runtime suites and SQLite regression checks after
      shared fixes. Keep builds/tests serial and use an isolated campaign TMPDIR.
- [x] Record reproducible source-to-consumer commands, exact negotiated capabilities,
      retained exclusions, dependency inventory, logs, and upgrade procedure.

## Completion criteria

P0–P5 are complete only when an unchanged pinned picotls core is reproducibly
translated by dotcc, the BCL provider performs real advertised cryptography and
certificate authentication, independent TLS peers interoperate in both directions,
negative tests fail correctly, and the documented supported targets pass JIT and
NativeAOT validation. A parse, a successful C# build, an unauthenticated handshake,
or a wrapper around native TLS does not count as completion.

Follow-up capabilities remain separate, tracked work: 0-RTT with replay protection,
BCL-supported additional algorithms, QUIC integration/packet protection, optional
certificate compression via BCL Brotli, ECH/HPKE profiles, raw public keys and async
signing. Unsupported BCL cryptography must be resolved explicitly before promising
those features; no hand-written cryptographic primitive is a planned fallback.
