# Compiler refresh verification — 2026-09-24

MsQuic and PicoTLS were freshly translated and postprocessed with dotcc at
`1abb6a813ee4a8e8880a103abc8558f3b09dbf85`, using `--literal-pool` and
`--nest-types`. This campaign runs on Linux x64 with both raw and optimized
output under JIT and NativeAOT. The previously qualified closure and its
artifacts were preserved at `artifacts/closure-85402e92e04196d3/`.

The PicoTLS matrix passes completely. MsQuic passes all stages before recovery
in `scripts/test-all.sh`; the complete script exits 1 at the historical
dual-live-mapping rebinding failure. The separate decreasing-MTU delivery
check also fails. Native-to-native controls reproduce both categories, which
are documented in the earlier [recovery campaign](recovery-campaign.md).
These observations do not establish a common cause or equal failure rates.

## Executed gates

| Gate | Result |
| --- | --- |
| PicoTLS native oracle | PASS: two files, 24 tests |
| PicoTLS complete matrix | PASS in all four forms: copied-source consumer, 92 ABI checks, 2,962 provider checks, eight upstream cases / 232 checks, TLS tests, 56 peer cases each (224 executions) |
| PicoTLS dependency audit | PASS: zero violations and zero missing prerequisites |
| MsQuic translation and ABI | PASS: 47 product units plus ABI probe, 60 host/core and 29 public ABI observations |
| Generated libraries and complete assembly consumers | PASS: raw/optimized JIT and NativeAOT |
| Platform and UDP host | PASS in all four forms |
| Packet crypto | PASS: 167 checks in each form |
| TLS adapter | PASS: 20 cases in each form |
| Translated/native transport peers | PASS: 80 cases |
| Owning managed API | PASS: 17 modes in each form (68 executions) |
| Public consumer | PASS: 32 cases |
| Managed.Net.Quic | PASS: 23 cases × JIT/NativeAOT × IPv4/IPv6 × cached/uncached (184 executions); TLS secret consistency checked |
| Malformed decoder corpus | PASS: 75 native-matched cases in each form |
| Live malformed input and amplification | PASS: 64 profiles |
| Injected host failures | PASS: 40 cases |
| Independent interoperability | PASS: 88 cases |
| CID rotation | PASS: 152 cases |
| MsQuic product dependency audit | PASS: zero violations and zero missing prerequisites |

## Recovery observations

The ordinary script stops at its first failure, so a separate serial observation
driver imports the unchanged `scripts/test-recovery.py` and calls its original
`exchange` function for every matrix case. It retains each exception and the
original peer/proxy logs, then continues to the next case. All original
assertions, source/binary/closure binding checks, settings and deadlines remain
in place. Twelve native-to-native controls cover both IP families and ciphers
for dual-live rebinding, expired-mapping rebinding and decreasing MTU.

All 576 original matrix cases and 12 additional native controls executed. The
strict result remains failed; completing evidence collection is not a pass.

| Recovery selection | Passed | Failed |
| --- | ---: | ---: |
| Ten ordinary scenarios, all translated profiles | 480 | 0 |
| Dual-live-mapping rebinding, translated profiles | 15 | 33 |
| Decreasing MTU, translated profiles | 0 | 48 |
| Native expired-mapping controls | 4 | 0 |
| Native dual-live-mapping controls | 0 | 4 |
| Native decreasing-MTU controls | 0 | 4 |

The ten ordinary scenarios are baseline, handshake loss, periodic loss,
reordering, duplication, delay, combined faults, expired-mapping rebinding,
MTU probe loss and increasing payload ceiling. There are 495 passing and 81
failing translated cases; including native controls gives 499 passes and 89
failures. No ordinary scenario failed. The strict rebinding failures retain
`Server did not validate the new source port`.

Decreasing-MTU delivery failures must not all be classified as native-equivalent
idle closures. Two translated-pair clients log `client payload FIN timed out`
and clean up with incomplete payloads and status/error 0:

- Raw NativeAOT / IPv6 / AES-128: both endpoints finish with status/error 0.
- Optimized NativeAOT / IPv6 / AES-256: the server instead reports idle status
  62 / transport error 1.

The corresponding native controls close with idle status 62 / transport error 1
at both endpoints. The earlier campaign also recorded a harness-deadline
outcome, in a different profile. This rerun retains these terminal differences
and does not claim successful recovery or prove a common cause.

The other 46 translated decreasing-MTU cases and all four native controls end
with idle status 62 / transport error 1 at both endpoints. All translated cases,
including failures, report zero remaining host resources, allocations and receive
leases, and zero host send/receive errors or truncations.

## Evidence and scope

Exact commands, exit codes and compiler source hashes are retained in
`artifacts/reverify-20260924/results.json`. Tracked compiler/runtime/postprocessor
sources and the repository HEAD stayed unchanged throughout execution. This
receipt has SHA-256
`8720d3da36d962d206ceeab4e54544d2fa7ee90168f28d02995feb1e65de07e8`
and indexes 35 preserved receipts, facade logs and driver files under its
`receipts/` directory. Main logs are `picotls-oracle.log`,
`picotls-translate.log`, `picotls-matrix.log`, `picotls-audit.log`,
`msquic-all.log` and `msquic-audit.log` in that directory. PicoTLS's bound receipt is
`../picotls/artifacts/tests/PASS.json`, referring to `run-_d0wplxf`.

The first strict rebinding failure remains at `artifacts/recovery/results.json`.
The supplemental early-stop run, including its decreasing-MTU failure, remains
at `artifacts/reverify-20260924/recovery-other/results.json`. The complete
serial observation driver and receipt are retained as
`artifacts/reverify-20260924/recovery-continue.py` and
`artifacts/reverify-20260924/recovery-complete/results.json`.

The refreshed ABI/build closure SHA-256 is
`c35a63469c17bf5799d60e73f68dcb5ee5c4a55436cf9f1489ca7e702e95d35f`.
It does not mark the failed recovery campaign or separate shared-regression
gates as passed. This rerun does not repeat SQLite, performance benchmarks,
15,000-connection load testing, or other platform qualification. No compiler,
host, upstream source or test assertion was changed for this verification.
