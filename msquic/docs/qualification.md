# Linux x64 qualification

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
