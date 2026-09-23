# Four-library regression run — 2026-09-23

Fresh Linux x64 verification of SQLite, picotls, MsQuic and libsmb2, starting
from `f387a71` on `sqlite`, using .NET SDK 10.0.111. No compiler or runtime
source changes were made during this run. The two intervening commits below
repair SQLite verification code; later tool rebuilds identify `c18a34d`.

## Results

| Suite | Result |
| --- | --- |
| Compiler unit tests | 2,441 passed |
| Compiler functional tests | 619 passed; 1,081 optional cases skipped |
| Postprocessor unit tests | 101 passed |
| SQLite | All default campaign stages passed after two harness fixes; additional original/postprocessed JIT/NativeAOT comparisons passed |
| picotls | Full raw/optimized JIT/NativeAOT campaign and product audit passed, including 224 independent-peer cases |
| libsmb2 | Fresh generation and implemented Linux x64 qualification passed, including raw/processed JIT/NativeAOT Samba matrices |
| MsQuic | Builds and ordinary runtime matrices passed; API passed on full retry after one intermittent assertion; stronger recovery gates remain failing |

No new compiler/runtime regression was confirmed. This is not an all-green
MsQuic qualification: the failures and retry below remain part of the result.

## SQLite harness repairs

- `6e404cc`: the product-offset verifier did not recognize emitted
  `public unsafe partial struct` declarations. The same diagnostic was already
  present in the preserved September 18 log. The corrected verifier passed
  41 metadata contracts, 67 field offsets and eight pointer-array checks under
  JIT and NativeAOT.
- `c18a34d`: the isolated host-VFS platform fixture needed a project reference
  to the generated SQLite constants used by its linked authored source.
  The platform fixture and complete `SQLITE_AOT=1 test-host-vfs.sh` rerun passed,
  including native/managed process differentials under both runtimes.

The initial fail-fast `verify.sh` invocation did not finish. Its successful
prefix was retained, the remaining stages were executed, and failed stages were
rechecked after repair. This is complete composite coverage of the default
campaign, not a claim that the original invocation exited zero. The additional
`test-postprocess.py --aot --corpora` gate passed consumer, host VFS, threading,
core, API, upstream JSONB and FTS5 comparisons against fresh raw/optimized output.

## MsQuic findings

Fresh translation emitted all 48 objects and passed ABI/build qualification for
raw/optimized JIT and NativeAOT. Runtime gates passed for platform hosting,
packet crypto, TLS, UDP hosting, 80 basic peer exchanges, 32 public-consumer
profiles, 184 `Managed.Net.Quic` cases, malformed packets, endpoint controls,
injected hosting, 88 independent aioquic cases and 152 connection-ID cases.

The first `test-managed-api.py --all` run stopped in raw NativeAOT flags testing:
`rejected open flags published a native callback root`. Three fixed follow-up
runs of the unchanged executable passed, followed by a successful complete
raw/optimized JIT/NativeAOT rerun of all 17 API modes. The assertion compares a
global live-root count while other endpoint activity can occur; a test race is
suspected, not proven. The original failure is retained and must not be treated
as an unconditionally clean first run. No API test assertion was weakened.

The normal recovery command passed seven exchanges, then failed dual-live
rebinding: both peers transferred their payloads and exited successfully, but
the server did not validate the new source port. A fresh native/native control
failed the same assertion. A selected decreasing-payload-ceiling check failed
with incomplete delivery and idle status 62 / transport error 1 for both
managed/managed and native/native endpoints. These are already documented in
[the recovery campaign](../msquic/docs/recovery-campaign.md). The fresh controls
cover raw JIT, IPv4 and AES-128; they do not establish equivalence across every
profile or prove a common cause. Both stronger gates remain failures.

The separate ten-scenario recovery matrix passed **480/480 exchanges** across
raw/optimized, JIT/NativeAOT, IPv4/IPv6, AES-128/AES-256 and all three
managed/native role combinations. It covers baseline, handshake loss, loss,
reordering, duplication, delay, combined faults, expired-mapping rebinding,
MTU probe loss and increasing payload ceiling. Its receipt correctly records
`targeted_passed: true`, `passed: false`: the two excluded stronger scenarios
are not silently credited as passes. The full 576-case strict matrix was not
completed after the normal command stopped at its first failure.

## Scope and reproduction

All four libraries were regenerated using the current compiler and their pinned
upstream sources. Campaign stages ran serially because generation shares build
outputs. The unchanged compiler unit/functional results from SQLite were reused
for libsmb2 verification rather than rerunning them; postprocessor tests ran once.

- SQLite: `SQLITE_AOT=1 bash sqlite/scripts/verify.sh`, resumed as recorded above;
  also a fresh raw emission and `test-postprocess.py <snapshot> --aot --corpora`.
- picotls: `oracle.sh`, `translate.sh`, `build-only.sh`,
  `test.sh --all --aot --runtime linux-x64`, and `audit-product.py`.
- MsQuic: every stage from `scripts/test-all.sh`, with the outer driver continuing
  to independent stages after a failure. Individual suites retain fail-fast
  behavior. Additional API retry and separate recovery controls are recorded.
- libsmb2: `translate.sh` invoked outside the repository directory, followed by
  `test.sh` and the repository test suites listed above.

libsmb2's upstream runner records 35 passes, 60 explicit skips, one native
baseline failure and four corresponding blocked managed cases. The blocked
program is the unchanged `ntlmssp_generate_blob` vector. Its qualification
receipt does not claim full upstream coverage or completion of every planned
feature. Optional SQLite Lua/Chibi/WAT port regressions and other operating
systems were not run. Functional-test skips are not passes.

## Evidence

The local run directory is
`sqlite/artifacts/cross-campaign-20260923T070729Z/`. It contains `status.json`,
`followups.json`, all stage logs, the runner scripts and preserved receipts.
The initial SQLite logs and failed MsQuic API attempt are kept separately from
their successful rechecks. Build artifacts are ignored by Git.

Additional receipts:

- `picotls/artifacts/tests/run-z0q6m112/PASS.json`
- `libsmb2/artifacts/qualification/result.json`
- `libsmb2/artifacts/upstream-tests/cases/result.json`
- `msquic/artifacts/managed-api-all/results.json`
- `msquic/artifacts/recovery/results.json` (strict failure)
- `msquic/artifacts/recovery-regression-20260923-positive/results.json`

The regenerated `msquic/config/product-closure.json` records the current compiler,
generated-source and ABI/build evidence hashes. Its separate shared-SQLite flags
remain false because the specialized closure-freezing SQLite receipt was not
issued; the executed SQLite campaign evidence is recorded here instead.
