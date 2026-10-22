# Linux x64 qualification

## Post-repair verification, 2026-10-23

Verification work is complete, with **stronger recovery and observation
qualification failed**. The [aggregate validation receipt](../../docs/validation/four-campaign-post-repair-20261023.json)
indexes the exact commands, matrix rows, source overlays, hashes and retained
failures. The private campaign source starts at `ddc9faf08`; frozen generation 2
uses compiler source `123b3e76e`. `DotCC.Lib.dll` SHA-256 is
`08a0ecd70213b342bb763a8bd381de64387953a8ba1c587fd8451d8d505d298d`;
the postprocessor SHA-256 is
`e1867eca1f516ba8a2ea900cbbc378b4d92179ef1b8adf19409dbc7e71c520dc`.
The full 22-file compiler and 33-file postprocessor closures remain unchanged.
Both compiler assemblies report informational version `1.0.0`; the qualified
closure records `compiler_commit=null` / `not_reported`, and the external frozen
tool receipt supplies source identity.

Fresh full framework verification completed all 14 suites and exited 0 under
its existing warning policy. Recovery executed all 576 configured cases:
489 passed, 87 warned and none were unexpected. All 480 positive cases passed.
The 87 warning rows retain strict failure: 39 dual-live rebinding failures and
48 decreasing-ceiling failures. `verification_passed=true` does not qualify them.

| Recovery form/runtime | Cases passed | Strict failures retained |
| --- | ---: | ---: |
| Raw JIT | 120 / 144 | 24 |
| Raw NativeAOT | 125 / 144 | 19 |
| Processed JIT | 120 / 144 | 24 |
| Processed NativeAOT | 124 / 144 | 20 |

The fixed observation campaign retained five rebinding attempts per role/profile,
one decreasing-ceiling attempt, and three positive controls. Its 288 relevant
observations comprise 80 rows from the full matrix and 208 fresh executions.
Rebinding retained managed 12 successes / 68 failures and native 11 / 69;
all 32 decreasing-ceiling observations failed. All 48 additional native positive
controls passed. These counts do not establish equal frequency or common cause.

The unchanged official classifier exited 1 before emitting any comparisons;
`complete_evidence_validated=false` and `observation_validated=false`. A fresh
native/native control labelled `raw/aot/ipv6/128` ended with client terminal
0/0/0 and server 62/1/0. That label selects its matching managed profile; both
executed peers are native C. The client log has no explicit deadline marker,
so elapsed time does not establish a deadline cause. This is an unsupported
native terminal of unknown cause. A read-only inventory retained all 288 rows:
287 pass the unchanged per-case validators, and 79 logical groups have matching
shapes/configurations; the remaining group lacks a validated exact native
witness. This diagnostic inventory receives no qualification credit. No extra
sampling, classifier expansion or baseline compiler rerun was used.

The other fresh suite results include 167 packet checks per cell, 20 TLS-adapter
cases per cell, 80 transport pairs, 68 API mode executions, 32 public-consumer
cases, 368 facade case executions, 300 native-matched malformed cases,
64 endpoint profiles, 40 injected failures, 88 independent-peer cases and
152 CID cases. The 60 host/core and 29 public ABI observations match native,
JIT and NativeAOT. All 47 units link and pass whole-assembly-rooted boundaries.
Ordinary sample consumption and the focused real registry regression pass all
four cells; the latter also rejects all 12 invalid-token probes. UDP feasibility
passes 14 cases in each runtime; QUIC-TLS feasibility passes 11 in each cell.
All 48 targeted keepalive controls pass. Cases, checks, observations and explicit
assertions are separate units; these MsQuic harnesses do not report an overall
assertion total.

The full 20-pair benchmark passed its unchanged 16 MiB per direction, warmup,
three measurements, payload/FIN, source metadata, shutdown and drain checks.
Client-interval rates were native 9,115–9,587 Mbit/s, raw/processed JIT
410–556 / 357–480, and raw/processed NativeAOT 795–941 / 754–956.
These small sequential measurements compare whole implementations, with different
TLS, buffering and runtime costs; they do not isolate compiler overhead or prove
a performance regression. The 47-unit native symbol audit passed, and the
product audit reported zero violations and missing prerequisites. The latest
MsQuic picotls republish has byte-identical generated/project/host inputs and
frozen tools to the independently qualified picotls product.

Campaign-only repairs update stale fixture imports/project references and
current provenance selection; the informational commit label is nullable without
relaxing binary/source checks. No compiler, runtime or production host behavior
changed. Other platform rows and the PLAN's excluded native/API capabilities
remain unqualified. The full upstream test suite was not executed.

## Historical checkpoint

The remainder records earlier checkpoints and their original inputs and outcomes;
it does not establish passes for the post-repair source above.

The subsequent [nested-layout regeneration](nested-layout.md) has its own refreshed
ABI, consumer and service evidence. The compiler hashes and full campaign ledger
below describe the preceding checkpoint; they are historical evidence, not a
claim that every matrix was rerun after changing the generated layout.

Evidence collection is complete for this campaign. The rows below use the frozen
compiler and regenerated product closure. This is not unconditional qualification:
the stronger recovery checks retain strict failures, and one managed terminal
outcome lacks a matching native observation. Historical receipts are not credited
as executions of later sources. See [PLAN](PLAN.md) for the selected profile and
[API coverage](api-coverage.md) for the owning surface's exact scope.

## Frozen inputs

- MsQuic: `80a065112426bce68c1da42d026478d3e40fd45e` (2.7.0).
- picotls: `3598470df01264da85157025ed10db0f7e103790`.
- Product closure: `7439fb111285863904055f525882f63431605039dafd8d07c859eb58afe9fb78`.
- Compiler build revision: `ce54ee2`; `DotCC.Lib.dll` SHA-256:
  `4fe035206c4f7cad69a80de48da2f334a38d3627557e29f0be0b1e812431a855`.
- Postprocessor SHA-256:
  `d7306947639d5762773ff5385dcff7add0699a9e7b4abc6c5dddc2d018ac3474`.

Later documentation and harness commits do not change the recorded compiler
build identity. The closure binds the complete compiler dependency snapshot,
47 staged translation units, generated raw/optimized sources and prerequisite
receipts. Both native and translated libraries compile the exact source revision;
the public API tests query those actual bytes.

## Evidence ledger

“Four forms” below means raw/optimized × JIT/actual linux-x64 NativeAOT.
Receipts and logs are ignored build artifacts under `msquic/artifacts/` unless
another path is shown; their source and binary hashes bind the tested snapshot.
`final-qualification/results.json` indexes the final receipts and explicitly
retains `full_profile_qualified=false`. The final binding check confirms SQLite's
42 emitted sources, five linked host sources and six compiler files, all 389
product-audit bindings, and the benchmark's 152 inputs, 55 binaries and 142 native
dependency bindings. The frozen postprocessor and product closure also match.

| Gate | Current result | Receipt |
| --- | --- | --- |
| Compiler and final SQLite | PASS: 2,146 compiler unit and 447 functional tests; fresh SQLite/native/managed/VFS/threading/corpus and raw/optimized JIT/AOT checks. Postprocess 65, analyzers 31 and opt-in SQLite parity 1 also pass. | `sqlite/artifacts/msquic-final-20260912-120752/results.json` and companion unit receipts |
| Normal optimized SQLite output | PASS: normal consumer JIT/AOT; all 42 emitted and five linked host sources match the qualified optimized snapshot. | Same directory, `normal-optimized-product/results.json` |
| Reused picotls | PASS: fresh translation, complete raw/optimized JIT/AOT campaign and dependency audit. | `picotls/artifacts/tests/PASS.json` → `run-3ln2j9l8` |
| Native and independent references | PASS fresh native 8 positive + 2 negative cases and native/aioquic 16 positive + 4 negative cases. Separate test processes; no product dependency. | `native-oracle/peer-results.json`, `independent-peer/interop-results.json` |
| Core and public ABI | PASS: 60 host/core plus 29 public observations match native, generated JIT and NativeAOT. | `host-contract/results.json`, `abi/results.json` |
| Complete generated product | PASS: all 47 objects linked; raw/optimized libraries and whole-assembly-rooted NativeAOT controls. | `product-build/results.json`, `config/product-closure.json` |
| Platform host | PASS in four forms, including actual workers and eleven allocation rollback positions. | `platform-host/results.json` |
| Packet cryptography | PASS: 162 checks in each form, plus native controls. | `packet-crypto/results.json` |
| TLS adapter | PASS: 20 cases in each form, with provider/resource drain. | `tls-adapter/results.json` |
| UDP host | PASS in four forms, including buffer/completion/address and shutdown contracts. | `datapath-host/results.json` |
| Basic translated/native transport | PASS: 80 pairs, exact 65,537 bytes and FIN each direction, authentication/profile and ownership checks. Peer binaries include the test-only CID shared-binding control; ordinary rows explicitly disable it. | `managed-peer/results.json` |
| Owning API | PASS: all 17 modes in four forms, 68 mode executions, actual pinned metadata and final source hashes. | `managed-api-all/results.json` |
| Separate public consumer | PASS: 32 cases across four forms and both IP families, roundtrip/resumption and authentication negatives, ordinary project references, no extra roots, actual pinned metadata. | `public-consumer/results.json` |
| Upstream-derived malformed corpus | PASS: 75 native-matched decoder cases in each of four forms. This is a selected decoder corpus, not endpoint fuzzing or the full upstream suite. | `malformed-corpus/results.json` |
| Live endpoint input and amplification | PASS: 64 profiles across four forms, both IP families and AES suites. Malformed headers and captured-packet tag mutations reach both endpoint roles; actual drop/decryption counters, subsequent exact data/FIN and owner drain are required. Managed/native server flights obey the pre-validation 3× UDP-byte bound, stop for lack of credit and resume after a duplicate Initial supplies credit. Native settings differ, so only the invariant and credit-release behavior are compared. | `endpoint-controls/results.json` |
| Injected host failures | PASS: 40 cases across four forms, both IP families and five actual clock/send/receive/socket failure scenarios. | `injected-host/results.json` |
| Actual keepalive/idle behavior | PASS: 48 controls across four forms, both ciphers/IP families and disabled/client/server keepalive profiles. `keepalive_qualified=true`; top-level `passed=false` deliberately does not credit the omitted default failure scenarios. | `keepalive-host/results.json` |
| Managed/independent interoperability | PASS: all 88 cases across four forms, both roles/IP families/ciphers/certificate types and authentication negatives. | `managed-independent/results.json` |
| Independent CID rotation | PASS: 152 cases (64 matched ordinary profiles, 64 actual rotations, 24 authentication negatives), with actual peer CID change and translated destination-CID counter growth. | `cid-rotation/results.json` |
| Positive recovery | PASS: all 480 cases across four forms, both IP families/ciphers, three translated/native roles and ten exercised fault scenarios. Both stream FINs are acknowledged before local shutdown; proxy and host ownership drain. `targeted_passed=true`; top-level `passed=false` retains the two excluded stronger checks. | `recovery-final-positive/results.json` |
| Repeated native observations | COMPLETE evidence, not a matching-outcome pass: 192 fixed observations and 48 native positive controls produce 80 logical comparisons. Dual-live rebinding: native 19 successes / 61 failures, managed 13 / 67. Decreasing ceiling: native 16 idle closures; managed 15 idle closures and one 15-second harness deadline. The latter is unmatched in optimized NativeAOT / IPv4 / AES-256. Strict outcomes remain unchanged; no equal-frequency or common-cause claim. | `recovery-final-native-observations/controller.json`, `classification.json`; [scope](recovery-campaign.md) |
| Performance measurements and investigation | PASS: all 20 pairs with actual pinned metadata, exact 16 MiB per direction, one warmup and three measurements, plus shutdown and complete owner-disposal durations. Aggregate client-interval rates: native 4,648–8,756 Mbit/s; raw/optimized JIT 420–446 / 567–629; raw/optimized AOT 904–1,051 / 971–1,112. A separate successful 30-second client trace identifies scheduling, ownership and packet-processing candidates without assigning CPU percentages or a single cause. | `benchmarks/results.json`; [measurements and limits](performance.md) |
| Product dependency audit | PASS: zero violations and missing prerequisites, bound to all 32 fresh public-consumer cases and published dependencies. | `product-audit/results.json`; [scope and limits](product-audit.md) |

## Preserved failures and limits

The first final SQLite invocation stopped at stale Lua generated outputs after
its SQLite stages had passed. The repaired port driver and resumed qualification
bind that original failure and the already completed prefix explicitly. SQLite's
normal generated product was then restored to optimized output and compared to
the separately qualified snapshot.

The first full API attempt passed 67 mode executions and failed the final mode's
assumption that no reset could have arrived immediately after silent shutdown.
A queued packet can already provoke the surviving listener's stateless reset.
Two test controls now handle either arrival order while still requiring exact
125/1 transport closure, reset-counter growth, packet authentication where
captured, and complete owner drain. The failed receipt is preserved at
`managed-api-all-before-reset-race/`; only those two test files changed before
the passing coherent rerun.

The earlier CID diagnostic used a dedicated client binding, for which the pinned
core deliberately generates a zero-length source CID. Rotation qualification now
uses and queries the actual shared-binding parameter before connection start,
with the same setting in each ordinary/rotation comparison pair. No product or
upstream transport changes were made.

Dual-live-mapping rebinding retains 61 native and 67 managed strict failures
across the fixed five-attempt-per-role/profile campaign, alongside 19 native and
13 managed strict successes. These are descriptive counts, not evidence of equal
failure probability or a common cause. Every managed rebinding failure has a
same-profile/configuration native failure witness. Positive expired-mapping
validation and increasing-ceiling/probe-loss recovery are separate passing gates.

Decreasing-ceiling delivery fails all 32 observations. Native peers report idle
status 62 / transport error 1 in all 16 profiles. Managed peers match that terminal
kind in 15 profiles; optimized NativeAOT / IPv4 / AES-256 instead reaches the
explicit 15-second client FIN deadline, then closes locally with status/error 0
and incomplete payloads. Its actual logs, callbacks and clean ownership are
bound in the classifier receipt. It is not an idle closure or successful recovery.
No additional attempts were added to obtain a preferred outcome. The complete
80-comparison classifier therefore records `complete_evidence_validated=true`,
`observation_validated=false` and `strict_passed=false`. The stronger gates remain
unqualified; the evidence does not establish the cause of the unmatched outcome.

The passing gates apply only within the selected Linux x64 profile and the
explicit limitations above. Other platforms,
0-RTT, QUIC v2, additional cryptographic profiles and optional native facilities
remain outside the qualified scope. The upstream inventory contains 590 literal
gtest declarations; the reused decoder subset is not the full upstream suite.
