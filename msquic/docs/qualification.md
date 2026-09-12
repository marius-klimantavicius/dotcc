# Linux x64 qualification

Final qualification is in progress. The completed rows below use the frozen
compiler and regenerated product closure; pending rows are not credited by
historical targeted receipts. See [PLAN](PLAN.md) for the selected profile and
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

| Gate | Current result | Receipt |
| --- | --- | --- |
| Compiler and final SQLite | PASS: 2,146 compiler unit and 447 functional tests; fresh SQLite/native/managed/VFS/threading/corpus and raw/optimized JIT/AOT checks. Postprocess 65, analyzers 31 and opt-in SQLite parity 1 also pass. | `sqlite/artifacts/msquic-final-20260912-120752/results.json` and companion unit receipts |
| Normal optimized SQLite output | PASS: normal consumer JIT/AOT; all 42 emitted and five linked host sources match the qualified optimized snapshot. | Same directory, `normal-optimized-product/results.json` |
| Reused picotls | PASS: fresh translation, complete raw/optimized JIT/AOT campaign and dependency audit. | `picotls/artifacts/tests/PASS.json` → `run-3ln2j9l8` |
| Core and public ABI | PASS: 60 host/core plus 29 public observations match native, generated JIT and NativeAOT. | `host-contract/results.json`, `abi/results.json` |
| Complete generated product | PASS: all 47 objects linked; raw/optimized libraries and whole-assembly-rooted NativeAOT controls. | `product-build/results.json`, `config/product-closure.json` |
| Platform host | PASS in four forms, including actual workers and eleven allocation rollback positions. | `platform-host/results.json` |
| Packet cryptography | PASS: 162 checks in each form, plus native controls. | `packet-crypto/results.json` |
| TLS adapter | PASS: 20 cases in each form, with provider/resource drain. | `tls-adapter/results.json` |
| UDP host | PASS in four forms, including buffer/completion/address and shutdown contracts. | `datapath-host/results.json` |
| Basic translated/native transport | PASS: 80 pairs, exact 65,537 bytes and FIN each direction, authentication/profile and ownership checks. Peer binaries include the test-only CID shared-binding control; ordinary rows explicitly disable it. | `managed-peer/results.json` |
| Owning API | PASS: all 17 modes in four forms, 68 mode executions, actual pinned metadata and final source hashes. | `managed-api-all/results.json` |
| Separate public consumer | PASS: 32 cases across four forms and both IP families, roundtrip/resumption and authentication negatives, ordinary project references, no extra roots, actual pinned metadata. | `public-consumer/results.json` |
| Upstream-derived malformed corpus | Pending final 75-case native comparison in four forms. | `malformed-corpus/` |
| Injected host failures | Pending final 40-case campaign. | `injected-host/results.json` |
| Actual keepalive/idle behavior | Targeted 12 controls pass; full 48-control campaign pending. | `keepalive-host-refreshed/results.json` |
| Independent interoperability and CID rotation | Targeted paired rotation passes both roles; full profile matrices pending. | `cid-rotation-shared-binding/results.json` |
| Recovery and native observations | Final positive and matched native-limitation campaigns pending. | [Recovery scope](recovery-campaign.md) |
| Performance | Targeted equivalent transfers pass; final matrix pending. | `benchmarks/` |
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

Dual-live-mapping rebinding and a decreasing validated UDP payload ceiling have
strict failures reproduced by the exact pinned native core. They remain failed
stronger checks. Positive expired-mapping path validation and increasing-ceiling
or probe-loss recovery are separate controls; native-equivalence classification
does not relabel a strict failure as a passing path-validation or delivery test.

This evidence qualifies only the selected Linux x64 profile. Other platforms,
0-RTT, QUIC v2, additional cryptographic profiles and optional native facilities
remain outside the qualified scope. The upstream inventory contains 590 literal
gtest declarations; the reused decoder subset is not the full upstream suite.
