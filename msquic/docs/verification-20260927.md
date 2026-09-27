# Verification — 2026-09-27

The complete final packet-recovery matrix executed all 576 cases: **488 successful
exchanges, 87 existing-policy warnings, and one unclassified FIN-deadline failure**.
The collection exited **1** with `matrix_complete: true`, `verification_passed:
false`, `passed: false`, and `entire_p7_qualified: false`. This is complete evidence
of the selected matrix, not successful decreasing-MTU recovery.

The user chose to document the native-reproduced decreasing-MTU limitation rather
than repair the pinned upstream engine. That scope decision does not change the
classifier, transfer assertions, original failures, or the machine-readable
qualification result. The existing rebinding warnings retain the previously
documented policy in [recovery-campaign.md](recovery-campaign.md).

## Final tools and prerequisite gates

The final shared `DotCC.Lib.dll` SHA-256 was
`5e7f4c566aaa7f292fb104c8800866e5dc10b5bfa8b825630b19695adef111a7`.
PicoTLS and MsQuic ran serially through the actual shared campaign runner with a
shared dependency session, `--tools reuse --fetch never --hashes strict --form
all --mode all --rid linux-x64 --jobs 2`. PicoTLS was not regenerated during its
dependent MsQuic qualification. These native transport suites were run on Linux;
the audit selftest now simulates symlink metadata without creating privileged
filesystem links.

PicoTLS passed its full native oracle, raw/processed JIT/NativeAOT qualification,
and audit: `picotls/artifacts/campaign/20260927-060907-dca9e368/receipt.json`.
MsQuic passed host-contract and public ABI gates, all 47 translated units, both
product builds, rooted product-boundary checks and qualification freezing, and
all 13 suites before recovery: platform-host, packet-crypto, tls-adapter,
datapath-host, managed-peer, managed-api, public-consumer, managed-net-quic,
malformed-corpus, endpoint-controls, injected-host, managed-independent (88
cases), and cid-rotation (152 cases).

The framework receipt
`msquic/artifacts/campaign/20260927-061505-6ebabcee/receipt.json` intentionally
retains **cancelled** status. Its driver paused before recovery; after all earlier
suites passed, it was interrupted to avoid a duplicate recovery run. The complete
matrix ran separately with explicit `--keep-going` against that exact qualified
closure and peer baseline. The overlap with the last independent-peer suites
used immutable executable inputs and separate output directories, temporary
directories and assigned ports. Both receipts and the handoff are retained.

## Actual recovery outcomes

All ten positive fault scenarios passed in all 48 profiles (480 cases). Rebinding
also passed in eight profiles; its other 40 outcomes retain the existing
`rebinding_new_path_unvalidated` warning. All 48 decreasing-ceiling exchanges
failed delivery: 47 had the recognized idle 62/1 outcome and one hit the FIN
deadline below.

| Form | Runtime | Successful exchanges | Rebinding warning | Decreasing-ceiling idle warning | Unclassified deadline |
| --- | --- | ---: | ---: | ---: | ---: |
| Raw | JIT | 120 | 12 | 12 | 0 |
| Raw | AOT | 124 | 8 | 12 | 0 |
| Processed | JIT | 120 | 12 | 12 | 0 |
| Processed | AOT | 124 | 8 | 11 | 1 |

The unclassified case is
`optimized-aot-client-ipv6-256-payload-ceiling-down`. It explicitly logged
`client payload FIN timed out` at 15.27 seconds. Both endpoints connected and
closed but remained unfinished, with no FIN acknowledgment and zero transport
and peer errors. The native server received 35,609 of 65,537 bytes. The managed
client's resource, allocation, receive-lease and I/O error counters were zero
after cleanup. The proxy recorded 80 oversized drops after the client-direction
ceiling changed from 1472 to 1300 bytes, then stopped with an empty queue and no
I/O errors. The managed callback trace shows local shutdown completion without a
transport-initiated shutdown.

A fresh native/native control used the same final binaries, peer baseline,
IPv6/AES-256 configuration and seed 7381; only assigned ports differed. It also
failed delivery, with 18,098 partial bytes and transport idle 62/1 after 10.27
seconds. This establishes a native failure of the same required transfer under
the same fault configuration. It does **not** establish the same packet schedule
or timer history. The native endpoint lacks explicit deadline/callback markers,
so the unchanged classifier still rejects the mixed deadline instead of turning
it into a warning. The [earlier investigation](recovery-investigation-20260925.md)
describes the upstream increasing-only MTU discovery and separate harness timer
behavior; it is not substituted for missing evidence about this exact timer.

The preceding failed full attempt remains at
`msquic/artifacts/campaign/20260927-052731-cba0ebd4/receipt.json`: 228 attempted
cases, 192 successes, 35 warnings and one raw-AOT/IPv6/AES-128 mixed FIN deadline.
All its case logs and hash-verified driver/proxy/classifier bytes are preserved.
Six fixed mixed repeats and six native/native controls all failed with idle
62/1; they did not recapture that earlier deadline. The native peer and native
MsQuic library used by those controls are byte-identical to the final baseline.

## Evidence and checks

The final evidence directory is
`picotls/artifacts/verification-20260927-atomic-refresh/`:

- `final-evidence.json` and `final-recovery-summary.json`: exact counts, booleans,
  exit status, prerequisite receipts and per-form/runtime/scenario outcomes.
- `recovery-complete/results.json`: all 576 actual cases and their original errors.
- `run.py`, `run-recovery.py`, `recovery-command.json`, `parent-handoff.json`,
  `parent-before-recovery.json`, `parent-after-recovery.json`: exact commands,
  the deliberate cancellation and the composed evidence route.
- `native-final-comparison.json`, `native-final-control/results.json`: final
  native control, configuration equality and endpoint observations.
- `frozen-peer-baseline.json`, `frozen-qualified-closure.json`: frozen inputs.
  The peer baseline hash stayed
  `1b5c49c3e4633ad7b746eb7460fcfdcd04ff37dd5d7145c8f9c41c0cbf6cb778`;
  the closure hash stayed
  `a5191d3f5818597541dc8017881b0247b46fb93724f4cec131a6ed020a3e12d8`.
  The driver also rechecked executable/source bindings before and after the
  matrix. Shared tool and historical checkpoint hashes remained unchanged.
- `link-flags.json`: actual final PicoTLS and MsQuic delivery commands contain
  `--split-size=262144`, `--literal-pool`, and `--deduplicate-inline`.
- `collector-provenance-tests.log`: all 27 tests passed (6 diagnostic-collection
  tests and 21 qualification-provenance tests). Automatic failed/cancelled
  receipt selection still rejects; explicit diagnostics still require successful
  translation, boundary and freeze gates plus matching actual product hashes.
- `classifier-preservation.json`: the classifier is byte-identical to the prior
  version; exchange, existing warning handling and case assertions are unchanged.
  `--keep-going` only retains later outcomes after a failed case; unexpected
  failures still exit nonzero and cannot grant qualification.

The earlier complete failed logs and fixed controls remain under
`picotls/artifacts/verification-20260927-final/`; they were not overwritten or
relabeled. No upstream recovery repair or broad deadline-warning rule was added.
