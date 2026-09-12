# Translate MsQuic with dotcc

Status: P0–P2 and P3/P4/P5 service gates passed on the current translated closure; full transport, injected failure paths, owning API and final qualification remain in progress.
Created 2026-09-11; execution started 2026-09-12.
Branch: `sqlite`. Campaign directory: `<repo>/msquic/`.
Current authorization: the user requested implementation of all phases through a
coordinator and sub-agents on 2026-09-12 and subsequently authorized committing
the work. Commit completed, validated changes as implementation proceeds.
After the translation and compiler fixes are complete, verify SQLite correctness
with the final compiler as well as the MsQuic phase gates.

## Objective and fixed constraints

Translate MsQuic's actual C transport core into a reusable unsafe C# library with
an owning managed API. Preserve upstream packet processing, connection and stream
state machines, recovery, congestion control, and scheduling policy. Use the
already translated picotls library as the **required TLS backend**.

Carry forward the picotls campaign's BCL-only and NativeAOT requirements:

- Target the repository's .NET 10 toolchain; validate both JIT and NativeAOT.
- Supply platform services and cryptographic primitives through BCL APIs and
  dotcc's managed runtime. Normal .NET implementation dependencies beneath BCL
  APIs are allowed; application-owned native shims and imports are not.
- Do not use native MsQuic, `System.Net.Quic`, OpenSSL, quictls, or Schannel as
  product backends. Native implementations may run as separate test peers.
- Reuse translated picotls and its BCL provider. Do not translate a second TLS
  implementation, write a replacement TLS state machine, or pass TLS records
  through the existing stream-oriented TLS facade to simulate QUIC TLS.
- Preserve upstream inputs. Put feature selection and necessary host adaptation
  in documented configuration/header overlays and adapter files. Fix generic C
  translation problems in dotcc; never hand-edit generated C#.
- Keep callback registration static and compatible with trimming/AOT. No runtime
  code generation, reflection-based dispatch, or dynamic provider discovery.

SQLite is an existing compiler regression workload, not an MsQuic dependency.
The branch name does not change the translation target.

## Source baseline and existing capabilities

The execution baseline is the latest upstream `main` snapshot resolved on
2026-09-12, **`80a065112426bce68c1da42d026478d3e40fd45e`**, dated
2026-09-09 (version header 2.7.0). The user requested the latest source after the
initial release-based plan; this immutable snapshot supersedes its v2.6.1 pin.
[Source manifest](../config/source.json) records its archive SHA-256 and URL.
Use the pinned revision, never a floating branch URL, for reproduction.
[Source commit](https://github.com/microsoft/msquic/commit/80a065112426bce68c1da42d026478d3e40fd45e).

The pinned build separates `src/core` from platform sources. The platform build
selects OS datapaths and native TLS providers; a managed picotls provider is new
integration work. Treat the source lists as inventory inputs, not a build recipe
that can be reused unchanged for the managed product.
[Core build](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/core/CMakeLists.txt),
[platform build](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/platform/CMakeLists.txt).

Reuse the picotls revision already pinned by this repository:
`3598470df01264da85157025ed10db0f7e103790`. Its translated core and BCL provider
already cover P256, both AES-GCM TLS suites, ECDSA/RSA-PSS authentication,
certificate checks, and protected session tickets. Its current consumer is not
a QUIC transport. The handshake-message API and traffic-secret callbacks must
be exercised through a new adapter.
[Current picotls status](../../picotls/README.md),
[provider contract](../../picotls/docs/crypto-provider.md).

dotcc already has managed-library emission, source/object linking, static
function-pointer support, GNU layout support, aligned integer bitfield storage,
atomics, pthread services, and aligned allocation. These are foundations, not
proof that MsQuic's full source and concurrency contracts work. The existing libc
socket implementation is not a complete MsQuic datapath; in particular, its
documented limitations include IPv6 I/O and mixed-descriptor polling.
[C support](../../docs/C-SUPPORT.md),
[socket runtime](../../DotCC.Libc/SocketLib.cs).

## Intended product profile

All phases below target this bounded profile. Completion means this profile
passes its gates, not that every optional native MsQuic facility is implemented.

| Area | Required by completion |
| --- | --- |
| Transport | QUIC v1 client and server over real UDP; IPv4 and IPv6 |
| Streams | Concurrent bidirectional/unidirectional streams; flow control, backpressure, FIN, reset, cancellation |
| TLS | Translated picotls TLS 1.3; P256; AES-128-GCM/SHA-256 and AES-256-GCM/SHA-384; ECDSA and RSA-PSS certificates |
| Authentication | ALPN/SNI, explicit trust and name validation, failure propagation; no insecure default. ALPN is 1..255 bytes without embedded NUL because pinned picotls serializes it as a C string; reject unsupported identifiers before dispatch. |
| Lifecycle | Listener/connection/stream ownership, asynchronous completion, reentrant callbacks, orderly shutdown |
| Recovery | Loss, duplication, reordering, congestion control using CUBIC initially, idle timeout and keepalive |
| Additional transport behavior | Retry/address validation, stateless reset, QUIC key updates, connection-ID rotation, NAT rebinding/path validation |
| Application features | QUIC DATAGRAM and ticket resumption with fresh 1-RTT data |
| Delivery | Reproducible generated library, owning C# facade, separate consumer samples, raw/optimized × JIT/NativeAOT validation |

Defer 0-RTT and anti-replay policy, QUIC v2, additional ciphers/groups, client
certificate authentication, full credential-provider/API parity, HTTP/3,
WebTransport, multipath, BBR qualification, kernel mode, XDP, io_uring, hardware
offloads, and native binary exports. Exercise the chosen upstream congestion
algorithm; make no performance-equivalence claim about the native datapath.

Audit every exposed setting, parameter, flag, and capability. Unsupported options
must fail clearly or be disabled through the documented configuration; they must
not silently succeed. A BCL gap affecting required behavior is a blocker, not
permission to add P/Invoke or silently shrink the required profile.

Linux x64 is the first execution target. Plan Windows x64, macOS arm64, and Linux
arm64 qualification after it. A platform is supported only after actual runners
pass its matrix; unavailable platforms remain explicitly unverified.

## Translation and host boundaries

### C core and managed platform layer

Inventory every translation unit reachable under the selected defines. Translate
the upstream core plus reusable platform-independent helpers such as cryptographic
key derivation and hash tables. Audit `platform_worker.c` and related execution
helpers individually: retain portable scheduling logic and replace OS execution
services at a narrow, documented boundary.

Define a managed CxPlat host contract for allocation, synchronization, time,
entropy, execution, UDP I/O, credentials, TLS, and packet cryptography. Do not
pretend that a POSIX profile supplies working epoll or that a Windows profile
supplies IOCP. Keep semantic inline helpers; replace only platform-specific
definitions and implementations. Disable optional tracing/storage/offload paths
explicitly where they are outside the product profile.

Use one documented C data model for the first target, checked by a native C
harness built with the same host headers. Distinguish that host ABI from native
Linux/Windows MsQuic binary ABIs, especially LP64 versus LLP64 and status values.
Measure sizes, alignment, field addresses, unions, bitfields, buffers, callback
tables, and actual generated storage. Retain native-aligned integral bitfield
backing where possible; reduce and fix any new generic layout failures.

The generated API table and `HQUIC` handles are a managed consumption surface.
They do not imply that native applications can call the generated library using
the upstream shared-library ABI.
[Public API definitions](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/inc/msquic.h).

### picotls QUIC TLS adapter

Implement MsQuic's `CxPlatTls*` contract against the existing translated picotls
core and provider. Map security configuration, per-connection state, results,
alerts, negotiated ALPN, credentials, and ticket callbacks explicitly.
[Pinned TLS contract](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/inc/quic_tls.h).

Use `ptls_handle_message`, `ptls_get_read_epoch`, `epoch_offsets`,
`update_traffic_key`, and the custom-extension hooks. picotls requires input at
the current read epoch; account for this when consuming MsQuic's CRYPTO stream
data. Map Initial, Handshake, and application handshake output to the correct
MsQuic offsets, keys, consumed lengths, and result flags. Epoch 1 early data stays
disabled. Process fragmented and post-handshake messages without adding TLS
record framing.
[Pinned picotls API](https://github.com/h2o/picotls/blob/3598470df01264da85157025ed10db0f7e103790/include/picotls.h).

Exchange QUIC transport parameters using the extension type supplied by MsQuic;
preserve the core's validation and callback timing. Handle both roles, a
zero-input client start, HelloRetryRequest, ALPN disagreement, certificate
failure, and TLS alerts. Disable TLS middlebox compatibility mode and
EndOfEarlyData emission. Reject received TLS KeyUpdate messages; QUIC key-phase
updates belong to the transport, separate from TLS record updates.
[QUIC TLS specification](https://www.rfc-editor.org/rfc/rfc9001).

Tickets need an explicit envelope mapping: picotls ticket protection plus the
MsQuic application/transport state and lifetime contract. Existing TLS ticket
tests alone do not establish QUIC resumption. Reject 0-RTT consistently and test
ticket expiry, rejection, key rotation, and fallback to a full handshake.

Keep generated picotls types and callback conventions intact. Use managed
`delegate*` callbacks with rooted state, static registrations, and stable context
headers/GCHandle tokens. Never cast between managed and unmanaged calling
conventions or store movable managed references in C memory.

Document ownership across the two translated libraries. MsQuic's TLS output and
transport-parameter buffers must be freed by their originating allocator;
picotls buffers retain picotls ownership. Copy across the boundary when necessary.
Do not assume two generated libraries share libc state or allocator bookkeeping.
Test exceptions, partial initialization, cancellation, and repeated disposal.

### Packet cryptography

Translate the reusable MsQuic `crypt.c` derivation logic; implement the primitive
`CxPlatKey*`, hash, AEAD, header-protection, and entropy contracts using BCL
services, reusing suitable picotls provider primitives without duplicating TLS.
Inventory all other crypto entry points reachable from token/ticket code.
[Derivation implementation](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/platform/crypt.c),
[crypto contracts](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/inc/quic_crypt.h).

Validate Initial secrets, directional packet keys/IVs, AES-GCM tags, AES header
protection, packet-number nonces, Retry integrity, and current/next/retiring keys
against RFC vectors and native results. Cover in-place buffers, malformed sample
lengths, tag failure, key discard, and zeroization. Do not release unauthenticated
plaintext. Secret logging must be an explicit test-only option.

### BCL execution and UDP datapath

Implement the required CxPlat locks, events, reference/rundown operations, pools,
thread-local state, execution wakeups, and monotonic clocks with BCL primitives.
Reuse libc pthread/atomic/allocation operations where their semantics match;
audit memory ordering, alignment, timeout units, and shutdown behavior instead of
assuming equivalence from function names. Preserve upstream worker ordering and
timer policy; the host supplies execution rather than a competing scheduler.

Build a UDP adapter over BCL `Socket` APIs, choosing its asynchronous model after
a measured feasibility spike. Define receive-chain and send-buffer ownership,
datagram boundaries, completion delivery, synchronous-completion handling,
cancellation, and close/drain behavior. A pending socket operation must retain its
buffers and context until completion. Exceptions must become explicit statuses
without escaping into translated callbacks.

Probe destination-address/interface metadata, IPv6 scope, wildcard listeners,
source-address selection, dual-stack behavior, socket errors, MTU discovery, and
ECN observability on each target. Use conservative supported behavior where the
protocol permits it, and test it. Do not fabricate packet metadata or advertise
RSS, segmentation/coalescing, raw sockets, or other capabilities without support.
[Datapath contract and capability flags](https://github.com/microsoft/msquic/blob/80a065112426bce68c1da42d026478d3e40fd45e/src/inc/quic_datapath.h).

## Workspace and reproducibility

Implementation is authorized. Campaign layout:

```text
msquic/
  README.md
  docs/         PLAN, source, configuration, blockers, tls, datapath, validation, usage
  config/       source manifests, explicit feature defines, managed host headers
  scripts/      fetch, oracle, translate, build, test, dependency audit
  src/          minimal C adapters, BCL PAL, picotls bridge, owning managed facade
  tests/        ABI, TLS/crypto vectors, transport, concurrency, peers, consumer
  ref/          pinned unchanged sources and oracle inputs (ignored)
  generated/    reproducible raw/optimized C# and metadata (ignored)
  build/        native and managed binaries (ignored)
  artifacts/    diagnostics, traces, comparisons, validation receipts (ignored)
```

Keep compiler/libc fixes and generic tests in their existing projects; keep
reusable picotls changes under `picotls/`. Reference its projects/artifacts rather
than copying generated TLS code into MsQuic. Record both source revisions,
toolchains, defines, input hashes, generated hashes, and runtime identifiers in
each validation receipt. Resolve paths relative to scripts and isolate campaign
temporary directories. Never execute native oracle code inside the product.

## Phases and completion gates

Current execution status (2026-09-12):

| Phase | Status and evidence |
| --- | --- |
| P0 | Passed: immutable source integrity verified; native44-unit syntax control passes. Native peer validates8 certificate/cipher/IP cases and2 authentication negatives. Pinned independent aioquic cross-connects both roles in16 positive and4 negative cases. The validated47-unit source/host closure is pinned in `config/product-closure.json` with exact source, compiler, generated and evidence hashes. |
| P1 | Passed: [UDP feasibility](datapath-feasibility.md) passes14 cases in JIT and AOT; [raw picotls QUIC TLS](tls-feasibility.md) passes11 cases in each raw/optimized × JIT/AOT combination. Public upstream ABI matches29 records in JIT/AOT. The managed99-slot host contract, TLS and complete core ABI match all60 native records under JIT/AOT. Feasibility and ABI subgates pass. |
| P2 | Passed: [compiler implementation](compiler-implementation.md) records validated preprocessing, declarations, anonymous members, alignment storage, high-bit conversions, and GNU intrinsics. The complete staged core emits, links and passes JIT/AOT ABI checks. Fullunit2146 and executedfunctional447 pass (1005 optionaloracle skips); Raw and optimized managed libraries and wholeassembly-rooted JIT/NativeAOT consumers pass. Runtime host services remain separate gates. Generic promoted-member value accessors are committed as `c786b9c`; the new compiler and complete raw/optimized closure pass refreshed60 host/core and29 public native ABI records plus whole-assembly JIT/AOT consumers. The prior checkpoint remains preserved. |
| P3 | Service gate passed on the current compiler and generated closure: actual translated worker execution, synchronization, all 11 allocation rollback positions and shutdown drain pass raw/optimized × JIT/NativeAOT. See `artifacts/platform-host/results.json` and [worker lifetime](worker-lifetime.md). |
| P4 | Service gate passed: packet crypto passes 162 checks in each raw/optimized × JIT/NativeAOT combination plus native controls; the actual CxPlatTls adapter passes 20 cases in each combination, including imported-key resumption and credential paths. Fresh translated picotls full JIT/AOT regression passes. See `artifacts/packet-crypto/results.json`, `artifacts/tls-adapter/results.json` and [TLS adapter contract](tls-adapter-contract.md). Transport and facade policy approval remain separate gates. |
| P5 | Real UDP service gate passed on the current closure in raw/optimized × JIT/NativeAOT: exact listener flags, source/interface routing, retained asynchronous sends, cancellation/drain, GC lifetime and callback-delete controls pass. See [datapath host](datapath-host.md) and `artifacts/datapath-host/results.json`. The typed host table supplies an injection seam; virtual time and injected send/receive failures remain unqualified until exercised with P7. |
| P6 | Typed actual-core peer harness authored, including ListenerStart preflight, 8 managed-to-managed and 64 native interoperability cases. The prerequisite service gates now pass; execution is next. No translated transport success is claimed yet. |
| P7 | Fault proxy and bounded controls are authored; advanced transport, resumption, migration, deterministic loss and injected failure gates remain mandatory. |
| P8 | Owning runtime, configuration, stream and credential wrappers are being implemented. Complete facade build, connection/listener integration, deferred/partial receive completion, canceled sends and asynchronous disposal remain required. |
| P9 | Final clean regeneration, complete regression/audit/performance campaign and fresh SQLite JIT/AOT correctness validation remain pending. |

The initial diagnostic experiment emits 44/44 adjusted translation units but its
managed library does not build. Its modified source copies and declaration-only
headers are evidence tools, not product inputs. Compiler work may overlap P0/P1
while those feasibility gates remain open; no downstream phase is accepted on
that basis. Update status with concrete commands and evidence. Compilation alone
does not complete a runtime phase, and stubs that return success satisfy no gate.

### P0 — Pin sources, profile, and native references

- Record exact MsQuic/picotls inputs, archive checksums, licenses, and required
  submodule gitlinks. Separate native-oracle dependencies from product sources.
- Freeze the feature/source manifest and managed ABI assumptions. Inventory public
  APIs/settings as supported, required later, or explicitly deferred.
- Build a pinned native MsQuic peer and establish a QUIC v1 client/server stream
  exchange with the selected certificates, ALPN, groups, and cipher suites.
- Select and pin an independent QUIC implementation for later interop. A
  `System.Net.Quic` peer alone does not provide independent transport coverage.

**Gate:** reproducible provenance and native reference commands, explicit product
scope, and a source/dependency map. Upstream C++ tests remain native references;
do not make translating GoogleTest or a C++ compiler a prerequisite.

### P1 — Establish feasibility and reduce blockers

- Preprocess/translate the selected closure and classify failures by compiler,
  headers, runtime, TLS, datapath, and optional feature. Save reduced reproducers.
- Compile minimal host ABI/callback probes and compare native layouts to actual
  generated storage, including MsQuic/picotls boundary structures.
- Spike the picotls handshake-message bridge: both roles, transport parameters,
  epoch-separated output, and matching traffic secrets without TLS records.
- Probe real BCL UDP IPv4/IPv6, metadata, completion lifetime, and close behavior
  under NativeAOT. Record a platform capability table and required gaps.

**Gate:** demonstrated raw picotls QUIC integration and usable UDP substrate, plus
an actionable blocker ledger. Do not substitute native TLS or OS imports to pass.

### P2 — Close compiler gaps and emit the complete selected core

- Fix evidenced preprocessing, C semantics, intrinsics, atomics, layout, function
  pointers, initialization, and linking issues generically in dotcc.
- Add reduced differential tests for substantive compiler fixes, including actual
  storage checks for unions/bitfields and callback invocation where implicated.
- Emit raw and optimized managed libraries with the explicit host imports. Audit
  unresolved symbols and initializer ordering; distinguish temporary build-only
  placeholders from usable behavior and prevent their use in runtime validation.

**Gate:** the complete source manifest emits and builds for JIT/AOT, native ABI
probes pass, and relevant compiler/runtime regression suites pass.

### P3 — Implement allocation, synchronization, and worker services

- Supply the required BCL PAL with correct allocation domains, alignment,
  memory-ordering semantics, monotonic time, events, and execution wakeups.
- Connect translated queues/timers/workers and implement callback-safe shutdown.
- Test contention, timeout/wakeup races, reference exhaustion/error paths,
  allocation failure, forced GC, repeated initialization, and final cleanup.

**Gate:** real worker work items and timers run and drain without leaked handles,
use-after-free, deadlock, or callbacks after context destruction in JIT and AOT.

### P4 — Complete picotls TLS and packet crypto providers

- Turn the P1 spike into the full required `CxPlatTls*` adapter and BCL packet
  crypto provider; retain translated MsQuic key derivation.
- Cover credentials, ALPN/SNI, parameter ownership, key transitions, error mapping,
  certificate rejection, HelloRetryRequest, ticket lifecycle, and buffer limits.
- Run RFC/native vectors and fragmented handshake harnesses for both roles and
  both cipher suites through raw/optimized × JIT/AOT.

**Gate:** the TLS contract and packet protection pass independently, all provider
callbacks execute under AOT, and the existing picotls campaign still passes.

### P5 — Complete the BCL UDP datapath

- Implement binding, address resolution, send/receive ownership, completion and
  error handling, IPv4/IPv6 metadata, capability reporting, and shutdown drains.
- Add an injectable test datapath and monotonic clock alongside the real adapter.
- Test bursts, truncated/oversized datagrams, cancellation races, unreachable
  destinations, wildcard/multiple listeners, and bounded buffer retention.

**Gate:** real UDP contract tests pass on the first target, with measured feature
limits and no accidental dependency on native MsQuic/POSIX event loops.

### P6 — Run translated QUIC end to end

- Link the actual core, PAL, packet provider, and translated picotls bridge.
- Connect generated client/server pairs over real UDP and cross-connect each role
  with the native reference. Verify authenticated ALPN negotiation and stream data.
- Exercise multiple streams/connections, large fragmented transfers, bidirectional
  backpressure, FIN/reset, timeout, cancellation, and shutdown callbacks.

**Gate:** real transport traffic and verified payloads in all four build/runtime
combinations, including a separately published NativeAOT consumer.

### P7 — Qualify recovery, remaining profile features, and interop

- Complete Retry/anti-amplification, stateless reset, connection-ID rotation,
  rebinding/path validation, key updates, DATAGRAM, and 1-RTT ticket resumption.
- Inject loss, delay, duplication, reordering, MTU changes, and receive/send
  failures using deterministic harnesses and a real UDP fault proxy.
- Exercise malformed packets/frames/transport parameters, wrong epochs, invalid
  tags, certificate/name/ALPN failures, limits, resource exhaustion, and shutdown
  races. Reuse suitable upstream corpora with recorded provenance and coverage.
- Run both roles against pinned native MsQuic and an independent QUIC stack;
  validate the selected P256/cipher profile with each peer before comparison.

**Gate:** every required profile behavior has passing positive/negative cases;
interop and fault campaigns pass with bounded runtime and resource use. Inventory
upstream test coverage by case/category; do not equate a small ported subset with
the entire upstream suite.

### P8 — Deliver the managed API and consumer documentation

- Provide owning registration/configuration/listener/connection/stream APIs with
  explicit send/receive buffer lifetime and exception-to-status boundaries.
- Define cancellation, asynchronous shutdown, callback reentrancy, deferred
  receive completion, and disposal ordering. Do not block a worker waiting for
  work that requires that same worker to complete.
- Publish build recipes and client/server samples using the generated library
  and picotls projects. Document supported settings and deliberate exclusions.

**Gate:** separate applications consume the library with streaming, errors,
resumption, and clean shutdown under JIT/AOT without repository-internal APIs.

### P9 — Qualify delivery, portability, and regressions

- Run the full raw/optimized × JIT/NativeAOT campaign on Linux x64; qualify the
  additional platform targets on actual runners and record unverified rows.
- Audit authored code, generated output, package closure, and published artifacts
  for native backends/imports, dynamic code, missing trim roots, and TLS duplication.
- Measure throughput, latency, allocations, memory under idle/load, and shutdown
  against the native reference using equivalent settings. Investigate regressions
  and report tradeoffs; do not invent a performance threshold before measuring.
- Run full compiler/libc tests and existing SQLite and picotls validation after
  shared changes. Preserve reproducible logs and case-level pass/fail/skip counts.
- The user's final SQLite acceptance explicitly requires fresh translation with
  the final compiler. Run `SQLITE_AOT=1 sqlite/scripts/verify.sh`, covering native
  corpora, managed consumers, real-file VFS/WAL interoperation, threading, layout,
  callback identity and JIT/NativeAOT. Then regenerate a raw engine, postprocess
  an isolated optimized snapshot, and run
  `sqlite/scripts/test-postprocess.py SNAPSHOT --aot --corpora` for both forms.
  Record the exact compiler hashes; the SQLite NuGet compiler build can replace
  shared CLI outputs, so keep the MsQuic frozen compiler receipts and recheck
  the product if its validated compiler inputs change.

**Gate:** clean regeneration, documented consumption, passing required feature
matrix, dependency audit, and regression evidence. Skipped or unavailable gates
must not produce an unconditional success receipt.

Dependencies: P0 → P1 → P2; P3/P4/P5 build on those results; P6 requires all three;
P7 → P8 → P9 follow. API design can inform earlier ownership work. Resolve any
newly discovered compiler blockers as they arise, with their own reduced tests.

## Validation rules and final acceptance

Compare deterministic layouts, vectors, payloads, negotiated parameters, errors,
and lifecycle events exactly where defined. For randomized handshakes and live
networks, compare protocol outcomes and invariants rather than ciphertext or
timestamps. Keep normalization explicit; never hide failed authentication,
incorrect offsets, dropped callbacks, or corrupted payloads in a comparison.

Track the full role × IP family × cipher × generated variant × runtime matrix,
plus targeted feature/fault cases. Keep resource and concurrency stress tests
bounded and reproducible by seed. A successful echo is an integration milestone,
not a substitute for authentication, recovery, ownership, and negative testing.

The campaign is complete only when the required profile is implemented by the
translated MsQuic core, TLS is supplied by the existing translated picotls,
separate consumers work over real UDP under NativeAOT, all mandatory validation
passes, and limitations/platform evidence are published. No native TLS/QUIC
fallback, handwritten replacement transport, generated-source patch, or success
stub can satisfy that outcome.
