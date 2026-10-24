# Translate s2n-tls to C# with DotCC

Status: implementation authorized; P0/P2 and captured provider/application/delivery snapshots have passing evidence. Reviewed ASN.1, blinded raw-RSA and EC state repairs are frozen; current full50 provider groups and emitted183 ABI pass JIT/AOT. Downstream qualification must use the current source epoch. P4/P6 remain in progress. Product acceptance remains incomplete.
Campaign: `s2n-tls/`. Updated: 2026-10-24.

Final-provider main1076, delivery12, native238 and recorded warn/off reproduction
gates pass for their identified snapshots. Required original certificate
assertions exposed a production ASN.1 string-conversion defect. Repair5f0b8ff3d,
48 provider groups, ABI all4 and unchanged certificate488 assertions per variant
now pass. All18 native Python families qualify22,186 applicable original cases.
Raw private RSA boundary inputs subsequently exposed a second production defect.
A narrow input conversion override retaining BC blinding and CRT checks is
committed as e2876b1a6. Subsequent native comparisons exposed EC public/private
state and encoding differences; repair322d75bc1 is frozen in receipt202209f4.
All50 provider groups and retained emitted183-binding ABI probes now pass in
both JIT/AOT. A deterministic CBC fixture retains the original error assertion.
Current original-C qualification has16 cases passed in all four modes; complete
257-case/18-family matrices and integrated acceptance remain required.

This plan adopts [translation baseline v4](../../docs/plans/translation-baseline.md)
and the [shared campaign framework](../../docs/campaigns.md). The user requested
the project plan first, then authorized translation, and explicitly requires unsafe features, including SSLv3
and old cipher suites, to be enabled. Legacy support is a completion requirement,
not a later extension or an optional diagnostic build.

Paths below are relative to `s2n-tls/` unless stated otherwise. Proposed files,
commands and suites describe implementation work until backed by current receipts.
No milestone is passed by the source inspection used for planning.

## Agent orchestration

For an authorized implementation campaign, follow the baseline's supervisor
selection of `gpt-6.1-sol` with `high` reasoning and implementation agents on
`gpt-6.1-sol` with `xhigh` reasoning. Set model and effort explicitly and report
unavailable settings. The parent session runs on `gpt-6-astra` with `high`
reasoning and delegates campaign supervision to the specified model. The requested
supervisor and implementation settings are available in the live T3 catalog.

The supervisor owns the full authorized campaign through P6, integration,
review and current acceptance evidence. Under the applicable delegation
instructions, launch independent implementation packages in parallel, with
bounded file ownership and checks. Coordinate shared compiler edits and build
outputs. A child result, probe, intermediate build or milestone commit is not
campaign completion. Repair observed compiler/runtime/host failures and retry
the complete source set without reducing the required legacy scope.

Planned independent packages after source/configuration selection:

| Package | Owned files | Dependencies and acceptance |
| --- | --- | --- |
| Inputs and native controls | `config/inputs.json`, source inventory, `tests/NativeOracle/` | P0 pin; native suite inventory, assertions, SSLv3 and RC4 actually execute. |
| Crypto capability and bindings | `src/CryptoProvider/`, `tests/Provider/`, provider contract | Shared ABI declarations coordinated first; prove every required primitive and legacy operation before provider selection closes. |
| Translation and compiler repair | Recipe, translation policy, reduced shared regressions | P0 manifest; complete emitted units, linkage, raw/processed builds and full-source retries. |
| Owning API and qualification | `src/Managed.S2nTls/`, consumer, peer/lifecycle tests | Design can run early; execution waits for translated core and provider. |

The coordinator owns `docs/PLAN.md` and the blocker/evidence ledgers. Once
implementation is authorized, continue through every required gate unless the
user limits or pauses work, or an external dependency/necessary decision prevents
all further useful progress. Record exact impediments and attempted resolutions.
The user has authorized implementation through the plan's acceptance gates and
explicitly reaffirmed the supervisor plus implementation-agent arrangement.
Keep milestone commits focused on validated work; pushing is not authorized.

## Outcome and scope

Translate the actual s2n-tls C implementation into `TranslatedS2nTls`, preserving
handshake state machines, negotiation, record processing, transcript handling,
SSLv3/TLS PRFs, HMAC/HKDF, session behavior, alerts and protocol validation.
Supply external execution services and crypto operations through explicit managed
boundaries. A native s2n wrapper or a replacement TLS engine does not satisfy this
product. `SslStream` is usable as an independent peer for protocols it supports.

The separate managed consumer must create a client and server, load credentials,
negotiate a requested protocol/suite, exchange a binary payload in both directions
over loopback sockets, verify the negotiated values and payload, and shut down
cleanly. Include a forced SSLv3/RC4 run, an SSLv3/3DES run and a modern TLS 1.3 run.
The broader qualification matrix covers every applicable suite/version pair.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required protocols | SSLv3, TLS 1.0, TLS 1.1, TLS 1.2 and TLS 1.3, client and server. Preserve upstream SSLv2-format ClientHello compatibility. |
| Required cipher coverage | Every negotiable suite in the pinned `s2n_all_cipher_suites` table: currently 36. This includes RC4-MD5, RC4-SHA, 3DES-CBC, AES-CBC, AES-GCM and ChaCha20-Poly1305 with their upstream RSA, DHE-RSA, ECDHE-RSA or ECDHE-ECDSA combinations and TLS 1.3 suites. |
| Required legacy semantics | Static RSA key exchange, finite-field DHE, MD5/SHA-1 and combined legacy hashes/signatures, SSLv3 MAC/PRF and record rules, TLS 1.0 CBC chaining, legacy signature/certificate policies and key sizes accepted by the selected upstream policy. Inventory all applicable compatibility controls at P0. |
| Required application behavior | Certificate authentication and explicit upstream verification bypass controls; SNI, ALPN where the protocol supports it; client authentication; session IDs/tickets and resumption; upstream-supported client renegotiation; TLS 1.3 key update; partial/nonblocking I/O, errors, connection reuse and cleanup. |
| Required managed delivery | Owning configuration/credential/connection API; low-level translated API; ordinary project references; concurrent independent connections; raw/processed JIT/NativeAOT on Linux x64. |
| Outside this first product | Full SSLv2 handshakes/records and cipher suites absent from the pinned implementation; resurrection of removed policy implementations; QUIC transport, kTLS/offload, PQ/ML-DSA features, 0-RTT, Rust bindings, native command-line applications and FIPS qualification. These are distinct from the required legacy features. |
| Later qualification | Additional execution targets, optional upstream features above, extended fuzz/stress and performance campaigns. |
| Baseline departures | None in required execution forms or completion gates. D4 selects BCL plus pinned BouncyCastle.Cryptography2.7.0; no BCL-only restriction is inherited from picotls. |

### Default compatibility policy

The default campaign profile is `compat`; it builds and enables all required
legacy algorithms together with modern ones. The owning API's default
configuration permits SSLv3 through TLS 1.3 and every applicable suite. Legacy
features must not require another build, test-mode environment variables or an
additional enablement gate. Applications can narrow the version/suite policy.
Tests and the consumer must be able to force individual versions and suites.

Prefer a small authored policy adapter using upstream structures and selectors;
preserve upstream named policy meanings and raw API defaults. The adapter selects
the compatibility policy explicitly when creating the managed default config.
The upstream `test_all` policy is useful evidence for the table layout and SSLv3
minimum, but also selects all KEM/group/signature preferences. Do not ship it
unchanged without auditing those dependencies. If exact version selection needs
an internal adapter, check its native/emitted layout and behavior rather than
inventing offsets or exposing unit-test machinery in the product.

Expose upstream verification-disabling, legacy-policy and blinding controls with
their actual semantics. Enabling old algorithms does not itself set all unrelated
options to arbitrary values: certificate bypass remains a selectable setting,
and both validating and explicitly bypassed configurations are required tests.
Retain protocol error checks and mitigations. Internal test-only helpers are not
production configuration APIs. Any additional unsafe/compatibility option found
during P0 must receive a required behavior row or an explicit explanation that
the pin does not implement it; it cannot disappear because a provider rejects it.

## Decisions and feasibility work

`Decided` identifies this plan's requirements. `Provisional` names a proposed
implementation choice needing evidence. `Open` identifies a specific unresolved
choice; only dependent work waits for it.

| ID | Decision | Choice and reason | Status and resolving gate |
| --- | --- | --- | --- |
| D1 | Source version and inputs | Start from release `v1.7.12`, commit `6459002cd61e4bf3bbc53db23f068b21da9ff6bd`, published 2026-10-16. Keep s2n product, provider/header dependencies, native oracle and independent peers separately identified in `config/inputs.json`. | Selected and acquired through framework JSON metadata; native/header, independent peers and managed dependency have separate pins/roles. |
| D2 | Source/configuration closure | Select all 174 C files in the pinned upstream library build groups, retaining every required legacy branch. Explicit manifests, ordered include/define lists and per-unit flags; C99 plus the actual required GNU extensions. Default `compat`, non-FIPS, no native autodetection deciding managed capabilities. | Selected: actual CMake174-unit manifest plus authored adapter; final183-binding profile includes proved SHAKE and retains all36 suites. |
| D3 | ABI and encodings | Linux x64 LP64 first: 64-bit pointers/long/size_t, little endian; preserve wire network byte order, byte-oriented PEM/DER and upstream text contracts. Measure layouts, unions, bitfields, callbacks and thread-local errors against the selected native build. | Provisional until actual native/emitted probes at P1. |
| D4 | Host/provider and dependencies | Reuse DotCC runtime/libc and BCL execution services. Prefer BCL crypto where it satisfies the exact operation; allow a deliberately selected managed or translated crypto dependency for missing legacy operations. No native s2n/libcrypto wrapper in the proposed managed product. | Selected BCL plus pinned BC2.7.0,183 exact typed entries covering178 selected native imports. Current EC freeze passes50 native differential groups in both runtimes, including Base64, ASN.1, blinded private-RSA and EC public/private state contracts; complete original-caller qualification remains open. |
| D5 | State and lifetime | One explicitly owned library initialization domain, shared immutable configurations after setup, separate connections; serialize entry per connection while allowing parallel connections. Final cleanup follows draining all users. No independent library-global isolation claim. | Provisional; audit init/cleanup, globals, TLS and reentrancy at P1/P3. |
| D6 | Translation/delivery | Multi-unit object emission/linking into class `S2nTls`, namespace `Managed.Security.S2n`; .NET 10/C# 14. Shared `link_flags` with 256 KB splitting, literal pool and inline deduplication. Original authored host/API references. | Provisional route; complete raw and processed products at P2, including address identity checks. No default exceptions proposed. |
| D7 | Native and independent controls | Same-pin native s2n using a legacy-capable non-FIPS libcrypto. OpenSSL 1.1.1 is a candidate oracle backend; a separately pinned SSLv3/RC4-capable OpenSSL build is a candidate independent peer. Modern peers may supplement coverage. | Selected OpenSSL1.1.1w backend, separately pinned1.1.1v and1.0.2u independent peers. All78pair/role native exchanges and214654 managed matrix execute; additional Python test peers are separately recorded. |
| D8 | Execution matrix | `compat` × raw/processed × JIT/NativeAOT × Linux x64, both roles and every protocol-applicable suite. Whole-library-rooted AOT and a separately trimmed ordinary consumer. | Decided proposed acceptance target; P4/P6 execution evidence required. Windows/macOS/arm64 are unqualified until separately executed. |
| D9 | Test scope | Applicable upstream cases, exhaustive required suite negotiation, native differential checks, independent peers, authentication/error/lifetime cases and bounded allocation/I/O failures. No long-running custom fuzz, saturation stress or performance threshold in this campaign. | Decided selection; freeze case inventory and explicit exclusions at P0/P1. |

Use `config/inputs.json` for URLs, immutable revisions, archive roots and optional
checksums; `config/translation.json` for ABI/features; `config/source-manifest.json`
for selected units; `config/compatibility.json` for versions, suites and controls;
and `config/adaptations.json` for exact adaptation anchors. These are planned
implementation artifacts. Revision/checksum literals must not enter scripts.
Hash policy defaults to `warn`, supports `off`, and offers optional `strict`.
Every mode preserves usable local source trees and requires unique patch context.

## Source observations and contracts

The proposed pin's [CMake source selection][upstream-build] includes
`crypto/*.c` (29), `error/*.c` (1), `stuffer/*.c` (6), recursive `tls/*.c` (124)
and `utils/*.c` (14): 174 units before authored adapters. Include public/unstable
API and internal headers. Capture feature-test output explicitly; do not borrow
host OpenSSL headers or feature flags accidentally. Keep reference trees intact
and prepare any generated headers/adaptations in private staging.

The [cipher table][upstream-ciphers] has 36 negotiable suites. Fourteen have a
non-null `sslv3_record_alg`. `TLS_NULL_WITH_NULL_NULL` is an internal initial-state
suite outside that negotiable table; preserving it is not a promise to negotiate
NULL encryption. Record every IANA ID and valid protocol relationship in the
compatibility inventory; do not assume all 36 suites apply to all five versions.

The [SSLv3 unit test][upstream-sslv3] skips unavailable record algorithms and
only requires a positive number to run. That assertion is insufficient here:
the campaign must account for all 14 SSLv3 entries and fail if any required one
is unavailable. Other upstream capability-based skips need the same review.

The [RC4 implementation][upstream-rc4] checks both EVP RC4 availability and the
OpenSSL version, explicitly returning unavailable for OpenSSL 3 or later.
The [build guide][upstream-build-guide] also records provider differences for
RC4, RSA-PSS and ChaChaPoly. A system OpenSSL build is therefore not automatically
a valid complete oracle. Do not fake a vendor/version macro to bypass the
managed provider decision; use truthful compatibility declarations or a narrowly
guarded adaptation with tests for its capability contract.

The [public API][upstream-api] marks `s2n_rand_set_callbacks` as a retained no-op.
Entropy must bind the actual RAND/OS path selected in `utils/s2n_random.c`.
Registering the old callback API does not establish managed entropy ownership.
Allocation, I/O and clock callback APIs remain candidates for ordinary host hooks.

### Host and ownership boundaries

| Boundary | Translated responsibility | Managed responsibility and validation |
| --- | --- | --- |
| Crypto | Preserve s2n protocol logic, SSLv3 MAC/PRF, HMAC/HKDF and record rules; retain s2n crypto wrappers where practical. | Map the exact external libcrypto operations through typed bindings or a documented provider adapter; vectors, native comparisons, lengths, encodings, copy/reset semantics and error contracts. |
| Allocation | Preserve object/record/stuffer ownership and zeroization calls. | Stable unmanaged storage, matching allocator/free, checked lengths and allocation failure; no movable address retained by C. |
| Entropy and time | Preserve random-use sites, ticket lifetimes and timeout decisions. | Real entropy at the selected entry points; wall/monotonic clocks in upstream units; deterministic replacements confined to test fixtures. |
| I/O | Upstream negotiate/send/receive/shutdown state machines and blocked status. | BCL sockets plus memory queues for probes; short reads/writes, would-block, EOF and errors mapped exactly; retain callback context until drain. |
| Credentials | Preserve wire parsing and signature/policy decisions at the translated boundary. | Own keys/certificate handles, expose trust and verification controls, check DER/signature representations and requested legacy certificates against native behavior. |
| Global state and errors | Preserve initialization, suite tables, thread-local error semantics and cleanup ordering. | Reference-count the owning runtime domain; prevent cleanup while active; static callback identities and rooted contexts; contain exceptions at all C boundaries. |

Provider feasibility must cover cloneable MD5/SHA-1/SHA-2 state, combined
MD5+SHA-1 operations, SSLv3 and TLS PRFs, RC4 state continuing across records,
3DES/AES CBC with upstream padding and IV ownership, AEAD tag rejection,
RSA PKCS#1 v1.5 encryption/decryption and legacy signing, RSA-PSS, finite-field
DH and ECDHE/ECDSA. Test the older key sizes and certificate signatures actually
accepted by the chosen upstream policy. Modern BCL success does not prove these
legacy contracts. Reuse picotls provider techniques only after checking semantic
compatibility; its provider is not an s2n libcrypto implementation.

A BCL capability failure leaves the corresponding requirement open. Resolve it
with a pinned existing managed implementation or translated dependency, including
its source/license/test inventory. The exact dependency is intentionally not
chosen without an operation inventory. No success stubs, handwritten replacement
TLS core, generated-C# repair or silent algorithm substitution is permitted.

The host API must define cancellation at nonblocking entry boundaries, socket
wakeup/close behavior, callback drain and disposal ordering. Upstream blinding
may use its self-service interface, with the managed owner honoring the returned
delay and exposed upstream controls. A cancellation flag alone is not evidence
that a blocked call terminates.

### Highest-risk experiments

| Risk | Smallest useful experiment | Required observation and dependent gate |
| --- | --- | --- |
| Legacy provider holes | Actual emitted RC4/3DES/hash/RSA/DH wrappers against the proposed provider and native control. | Exact vectors, state continuity, legacy signature and key exchange success; selects D4 at P1. |
| Policy silently filters legacy | Enumerate initialized suites, force SSLv3 with each of the 14 applicable entries, and force every other valid version/suite row. | Negotiated IDs/version and bidirectional bytes; unavailable required entries fail P1/P4. |
| ABI and cleanup extensions | Emit actual stuffer/blob/cipher/config/connection layouts and representative upstream cleanup-attribute/error paths. | Native/emitted sizes, offsets, callback identity and cleanup behavior agree; P1/P2. |
| Library/global ownership | Init, concurrent distinct connections, forced GC, failed initialization, dispose during pending I/O, final drain. | No lost roots, double frees, cross-connection error corruption or live callbacks after cleanup; P1/P3. |
| Native peer lacks SSLv3/RC4 | Build isolated candidate peer and perform forced native-s2n exchanges. | Successful real exchanges with recorded provider/build features; peer absence cannot count as P4 coverage. |
| Host certificate policy differs | Legacy certificates/signatures/key sizes under validating and bypass configurations. | Match the specified upstream outcomes or resolve provider/policy mismatch; P1/P4. |

## Framework integration and deliverables

Planned canonical layout:

```text
s2n-tls/
  README.md
  ManagedConsumer.slnx
  docs/{PLAN,blockers,validation,provider-contract,usage}.md
  config/
  scripts/campaign.py
  scripts/{common,fetch,translate,build,test,verify,probe}.sh
  src/Boundary/
  src/CryptoProvider/
  src/Managed.S2nTls/
  samples/ManagedConsumer/ManagedConsumer.csproj
  tests/
  ref/
  generated/TranslatedS2nTls/TranslatedS2nTls.csproj
  generated/TranslatedS2nTls.Raw/TranslatedS2nTls.csproj
  build/campaign/<run-id>/
  artifacts/campaign/<run-id>/
```

Export `Recipe("s2n-tls", "TranslatedS2nTls", ("compat",), ...)`. The default
profile uses canonical generated paths; any future alternate profile uses
`generated/profiles/<profile>/`. The consumer selects artifacts through the
`S2nTlsProject` property. Its project reference reaches the owning API and selected
translated product through original authored project/file paths. Regeneration
must preserve those sources and builds must pick up authored edits immediately.

Use the framework's fetch/prepare/emit/raw-build/postprocess/processed-build/
transactional-publish sequence. Declare selected translated dependencies in the
recipe rather than copying their generated output. No central project registry
or duplicated runner. Supply metadata, recipe and authored consumer before
running the layout scaffold. Add project-local ignores for references, generated
output, build work and artifacts during implementation.

Prerequisites: repository .NET 10/C# 14 toolchain, shared runtime, Bash and Python
3.11+; native C/CMake/build tools and explicit libcrypto/peer sources for control
suites; platform NativeAOT prerequisites. Exact native/provider versions and
commands are P0 outputs. Keep native test prerequisites out of ordinary managed
translation unless required source headers actually depend on them. Package
upstream Apache-2.0 license/NOTICE and selected dependency notices.

Shared Bash dispatch, portable copies and project references must work under an
ordinary Windows account without symlinks/elevation. Linux native/peer execution
is a separate prerequisite, not a claim that Windows runtime qualification passed.
Use shared Python resolution and preserve invocation from unrelated directories.

## Milestones and evidence

P0/P2 pass source/native/build gates. P1 has passing RSA boundary probes but a newly confirmed EC state/role gap;
P3/P5 have passing captured0158 evidence and require refreshed application/delivery
checks. P4/P6 remain open. Commit coherent
milestones after required validation, respecting current user prerequisites.
Do not push without authorization.

| Phase | Concrete exit gate | Status | Planned evidence and remaining work |
| --- | --- | --- | --- |
| P0 — Scope/native baseline | Exact pins, 174-unit manifest reconciled to build, complete legacy inventory, recipe/layout, working native SSLv3/RC4 + TLS 1.3 controls and first full-source translation attempt. | Passed | Framework acquisition `20261023-200313-154db7b3`; source inventory 174/36/14/78; native `20261023T171931Z` passes 238 cases and 590,537 upstream assertions; full-source attempts have concrete compiler findings. |
| P1 — Boundary feasibility | Real emitted/native ABI checks; legacy provider operations and truthful capability configuration; default compatibility policy; ownership and peer capability probes. | EC repair in progress | Capability255, final49 native differential groups in JIT/AOT and post-ASN1 canonical ABI all4 pass.178 selected crypto imports are covered by183 declarations. Base64 repair08796c070 matches1,659 native vectors and ASN.1 repair5f0b8ff3d matches3,131 vectors per runtime. Full upstream caller qualification remains P4. |
| P2 — Complete translation | All selected units emit/link; raw and processed libraries build at canonical paths; no unresolved required imports or manual output repair. | Passed | Full175-unit raw/processed publication223542 passes with183 exact bindings/SHAKE/correct DHE callers and array-lifetime compiler repair. No generated edits. Behavioral gates remain separate. |
| P3 — Useful execution | Managed client/server execute forced SSLv3 RC4, SSLv3 3DES and TLS 1.3 round trips through translated code, with clean lifecycle. | Passed0158 snapshot; RSA refresh required | All4 ordinary consumer variants execute all3 forced exchanges with exact IDs/versions/binary payloads/shutdown;30 lifecycle cases pass pervariant. Final-source main matrix1076/1076 and combined5,023-file audit pass. |
| P4 — Behavioral qualification | Every required suite/version/role row, applicable upstream cases, compatibility controls, errors and independent interop pass. | In progress | Recorded223542/base64 matrix1076/1076 passes for its pre-ASN1 provider. Final caller-only DHE native238/0/0 and matching-profile native257/257 pass. Final ASN1 source48 groups, ABI all4 and certificate488each pass; captured0158 required257C×4 controller has46passed/2rawfailed/209unrun at the recorded checkpoint. RSA repair e2876b1a6 and full49 groups/ABI JIT+AOT pass; no new C/application/delivery pass is claimed. Final-provider main1076/delivery12 and strict native18 Python families pass; complete translated C/Python qualification remains open. Original buffered-send overflow node passes all4. Failed/skipped/retried attempts remain preserved. |
| P5 — Consumer delivery | Root solution builds; separate ordinary consumer runs JIT and trimmed AOT; whole-library AOT and package/import/notices audits pass. | Passed0158 snapshot; RSA refresh required | Final-source delivery12/12 covers copied ordinary consumer all4variants plus whole-library TrimmerRootAssembly AOT bothforms, actual3roundtrips and notices/import/original-reference/runtime identity audits. Combined main/delivery deep audit verifies5,023 files and rooted XML derivation. Final-provider locked root solution build passes. |
| P6 — Reproduction/acceptance | Final-tool regeneration, complete verification matrix, affected shared regressions and reproduction checks pass. | In progress | Final-tool223542 publication, current ABI, affected shared regressions and reproduction233001-6f4afff7 pass. Reproduction exercises two full175-unit regenerations, warn/off, fetch-disabled/unrelated cwd, authored edit pickup and failed publication in an isolated destination. Full upstream and final integrated acceptance remain open. |

Dependency order is P0 → P1 → P2 → P3 → P4 → P5 → P6, with independent input,
provider, API and harness design work as described above. Split implementation
work into smaller validated changes without changing these completion gates.

Keep `docs/blockers.md` with classification, command/profile/tool identity,
native expectation, reproducer, regression, fix and complete-source retry.
Compiler fixes belong in shared structural code with focused and emitted/runtime
regressions. Preserve unrelated repository baseline failures separately. Existing
unrelated work, including the current MsQuic documentation edits, is outside scope.

## Validation selection

All suite names below are planned recipe declarations. `matrix` means
`compat` × raw/processed × JIT/NativeAOT on `linux-x64`. Native controls run in
separate processes; a native/native exchange alone is not managed interop evidence.

| Suite | Assertion/oracle | Required selection and prerequisites |
| --- | --- | --- |
| `native` | Native source/profile assertion inventory and forced legacy/modern exchanges. | Required P0/P6; explicitly pinned native backend and tools. |
| `abi` | Actual emitted sizes/alignment/offsets, calling conventions, canonical function pointers and thread-local errors vs native. | Required matrix; native observations plus emitted product. |
| `provider` | Exact primitive, PRF/MAC, state clone/reset, buffer/encoding/error and legacy key/signature vectors. | Required matrix; selected provider and native/vector inputs. |
| `compatibility` | Every valid version/suite pair, both roles; actual negotiation, records, resumption where supported; all legacy controls and policy behavior. | Required matrix; complete static inventory, credential/DH fixtures. No required capability skip. |
| `consumer` | Loopback binary request/response, asserted version/suite, forced RC4/3DES SSLv3 and TLS 1.3, shutdown and resource return. | Required matrix; generated test credentials and owning API. |
| `upstream` | Applicable translated upstream cases with preserved assertions, compared with matching native executions. | Required matrix; frozen `config/upstream-tests.json` and per-case counts. |
| `lifecycle` | Partial I/O, EOF, bad MAC/tag/padding, invalid cert/signature, validation bypass, OOM/initialization failures, cancellation, forced GC and concurrent distinct connections. | Required matrix; bounded deterministic fault cases and explicit expected statuses. |
| `interop` | Managed client/native peer and native peer/managed server for every required valid suite/version row, possibly across multiple peers. | Required matrix; peers proven to support each assigned row, negotiated values and payload checks. Missing peer coverage remains incomplete. |
| `rooted-aot` | Compile and execute a harness that roots the full selected library. | Required raw/processed AOT on Linux x64. |
| `delivery` | Build solution and copied external consumer, trimmed AOT execution, imports/dependency/notices audit, authored-source reference checks. | Required JIT/AOT and both forms; ordinary references, no native s2n/libcrypto product dependency. |
| `reproduction` | Final regeneration, fetch-disabled reuse, warn/off provenance behavior, unrelated cwd, authored edit pickup and failed-publication preservation. | Required P6; shared framework receipts plus focused project assertions. |
| `shared-regressions` | Focused compiler/runtime/emission/postprocessing tests and downstream campaigns affected by actual shared changes. | Required when shared code changes; include picotls/MsQuic only when a changed boundary affects them, not by historical inheritance. |

Planned `default_suites`: `consumer`.
Planned `verify_suites`: `native`, `abi`, `provider`, `compatibility`, `consumer`,
`upstream`, `lifecycle`, `interop`, `rooted-aot`, `delivery`, `reproduction`,
`shared-regressions`. `reproduction` must use non-recursive framework operations,
not invoke `verify` again.

Seed the upstream inventory with `s2n_sslv3_test`, `s2n_rc4_test`, `s2n_3des_test`,
`s2n_cbc_test`, `s2n_hash_all_algs_test`, `s2n_hmac_test`, `s2n_ssl_prf_test`,
`s2n_tls_prf_test`, `s2n_tls13_prf_test`, cipher selection/policy tests,
certificate/X.509 tests, handshake/record cases, resumption, renegotiation and
shutdown cases. This is a seed, not a fixed coverage ceiling. Inventory every
upstream case against the chosen product; classify unselected PQ/kTLS/FIPS,
formal verification and platform-specific harness cases explicitly. Adapt test
harness services without weakening upstream behavior assertions.

Compare deterministic bytes, statuses, negotiated values and layouts exactly.
For random-dependent handshakes, assert independent transcript/Finished/signature
verification, negotiated identities and exact application bytes, not identical
random transcripts. Preserve raw logs and specify any allowed normalization.
Do not normalize away failure, dropped data or provider unavailability. No timing
or constant-time security claim follows from functional correctness or AOT.

## Reproduction commands and acceptance

The recipe and referenced scripts/projects are implemented. These are the
supported entry points; exact executed selections and passing receipts are in
`docs/validation.md`. Full `verify` acceptance is still pending and is not implied
by this command list:

```bash
bash Scripts/campaign.sh layout s2n-tls --write
bash Scripts/campaign.sh layout s2n-tls --check
bash s2n-tls/scripts/fetch.sh --fetch missing
bash s2n-tls/scripts/translate.sh --profile compat --fetch never
dotnet build s2n-tls/ManagedConsumer.slnx -c Release
bash s2n-tls/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash s2n-tls/scripts/test.sh --suite compatibility --form all --mode all --rid linux-x64
bash s2n-tls/scripts/verify.sh --fetch never --form all --mode all --rid linux-x64
```

Record actual successful commands and receipts during implementation. Distinguish
`--fetch never` from `--restore normal|locked|none`; native/peer acquisition belongs
to declared groups, not hidden downloads. Verify edited usable inputs under
`warn`/`off`, clean campaign-output reproduction, invocation from an unrelated
directory, and authored files surviving regeneration. Strict hashes are optional.

Final evidence belongs in `docs/validation.md`, with links to framework receipts
and specialist case reports. The current scoped native, translation, provider,
ABI and delivery gates have passing receipts. Exact provider and peer selections
are recorded in config JSON; full qualification and integrated acceptance remain
P4/P6 work. Reproduction passes for the recorded isolated final-tool snapshots.
Completion of the campaign requires the full specified legacy and modern
profile, consumer and reproduction gates. A working TLS 1.3 subset, a passing
build or an SSLv3 test that skips RC4 cannot close acceptance.

[upstream-build]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/CMakeLists.txt
[upstream-build-guide]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/docs/BUILD.md
[upstream-ciphers]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/tls/s2n_cipher_suites.c
[upstream-sslv3]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/tests/unit/s2n_sslv3_test.c
[upstream-rc4]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/crypto/s2n_stream_cipher_rc4.c
[upstream-api]: https://github.com/aws/s2n-tls/blob/6459002cd61e4bf3bbc53db23f068b21da9ff6bd/api/s2n.h


Current boundary follow-up: the EC repair is frozen at202209f4 and committed;
full50 provider groups and retained emitted183 ABI checks pass JIT/AOT. The
deterministic CBC fixture is separately frozen atc0aa38f7. Fresh application
main1076/1076 passes all four variants, including120 lifecycle executions.
Copied/rooted delivery remains queued behind its unchanged disk-admission
guard; original Python72 qualification continues. Required-C captured count
is32/257 cases all4, with no failed current cases. Retained linked projects
continue through batch006; a reviewed private placement derivation requires
one bounded original215-assertion case before general new-raw continuation.
Complete original C/Python and sealed P6 framework acceptance remain open.


2026-10-24 current-source checkpoint: the exact phase2 helpers are committed
in a900cc5b9 after actual original supported-versions215 assertions in each
of four variants and independent844/6195-file reviews. Same-epoch C controller
2335343 adopted only that bounded case, continued batch007, and published
62 passed /2 failed /193 unrun. The failures remain visible: connection
preferences fails C# compilation for `char no_curve[] = { "NONE" };`
(native132), and config fails assertion721 while default CA loading ignores
SSL_CERT_FILE (native3898). The compiler owner is proving a lowered-string
fix; the provider owner is preparing a private native differential. No live
provider/API source or immutable223542 objects/tools changed for these findings.

Current EC/CBC delivery actually passes12/12, with six app identities and
1717 retained files independently checked. Current main remains1076/1076 and
CLI8 passed. Closed historical/current-five Python source sharing preserves
145635 aliases and7380 independent private donors; supervisor postflight checks
174831 distinct paths and all four journal chains at
`artifacts/managed-contract/closed-python-source-sharing-supervisor-postflight/report.json`.
Sharing gives no test credit. Original Python72 remains5 passed,1 running,
66 unrun at this capture. Full257 C/all72 Python and sealed P6 stay open.
