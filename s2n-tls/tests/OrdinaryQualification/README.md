# Isolated ordinary original-body qualification

The 2026-10-24 result is **14 native controls and 56 managed executions passed**,
with zero failures or skips in the completed attempt. Each raw/processed ×
JIT/NativeAOT mode executed 857,712 assertions, matching the preserved native
original inventories exactly. See [evidence-summary.json](evidence-summary.json).

This evidence covers unchanged original test bodies with **memory-only startup**.
It is not full original execution, provider qualification, or canonical257
progress. The historical 62 passed / 10 failed / 185 unrun ledger is unchanged.
Full original initialization, cleanup registration, and the complete campaign
remain unqualified by this lane.

`selection.json` contains two explicit path batches. They cover error strings and
types; allocator/free callbacks and their bookkeeping; hexadecimal, integer and
text buffers; IPv4/IPv6 formatting; return/error safety macros; byte-to-integer
conversion; stack blobs; and connection layout bounds. Original assertion counts
include startup: the connection-size case has just one counted startup assertion
plus its original conditional layout failure, and stacktrace has two assertions
with `S2N_STACKTRACE` absent. These are not active stacktrace functionality tests.

The external machine inventory classifies every required C case: 62 already
qualified, one separately qualified preferences case, 14 runnable isolated bodies,
179 held cases, and one ordinary I/O host gap. Every row includes original source
identity, direct calls, assertion sites, native counts, prior status and rationale.
Mixed utility/protocol cases remain intact. The map and map-iterator cases call
SHA-256; Base64 requests random data; timer/configuration cases create crypto
state. None was executed.

The remaining ordinary I/O case requires real `fork`/`waitpid` and signals. The
preserved runtime returns `-1/EPERM` for both process functions. Its original
parent loop can bypass child assertions after a failed fork, so running that
body would not provide faithful qualification. The exact source and next action
are recorded in the summary; no success substitute or partial-body pass is used.

## Harness and call closure

The normal `BEGIN_TEST` calls `s2n_init`, which initializes crypto and protocol
tables. The private wrapper includes the original test header, replaces only
`BEGIN_TEST` with original `BEGIN_TEST_NO_INIT` plus real `s2n_mem_init`, then
includes the original test file unchanged. Assertions and original body branches
are preserved. Distinct main/counter/color names allow one isolated executable
to dispatch only the selected paths, with a fresh process for every case.

Three small original functions are copied verbatim into a labelled bootstrap:
`s2n_enable_atexit` only sets a flag, `s2n_cleanup` is upstream's compatibility
no-op, and `s2n_disable_tls13_in_test` checks the test flag and assigns version
flags. There is no global initialization or atexit cleanup registration. Native
controls use the identical startup, wrappers and original source bodies.

Transitive bodies use only the original error, buffer, blob, memory, checked
arithmetic/copy, address-formatting and byte-order implementations. Memory callback
targets come from those same implementations and original memory-test support.
Original safety macros route failures into error/debug strings, not crypto.
The entire safety TU retains an unused PKCS1 helper; review checks that it has
no callers in either emitted tree. No held main is called to reach a utility.

The preserved product objects embed the complete unused provider binding
catalogue. Attempt2 therefore stopped at C# compilation with 169 missing-binding
errors, before managed execution. Attempt3 freshly emits only the 11 unchanged
ordinary core TUs without that catalogue. No product input or object is edited.
The original immutable compiler and semantic postprocessor are used; generated
C# is never manually repaired. Both emitted trees have no provider calls,
randomness calls, global library initialization call, configuration/connection constructor, or
negotiation call. Native executables need only libc and the dynamic loader.

`review.py` independently checks all 70 result keys, source/log/output hashes,
original/native assertion counts, generated call exclusions and native dynamic
dependencies. It verified 766 files, plus recorded intermediate/project hashes.
Both initial failed attempts and their native outcomes remain external and are
linked from the summary. A final two-line controller exit-status correction was
checked separately with synthetic failed/passed receipts; the exact assertion-run
script snapshot remains at `attempt3/harness.py`.

## Commands and evidence

Everything new is under the private external root:

`/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/ordinary-c/`

`attempt3/receipt.json` records all 132 actual commands, timeouts, exit statuses,
logs, assertions and provenance. `attempt3/inventory.json` contains the full257
classification. `attempt3/review.json` contains the read-only review. The summary
records their hashes and the two earlier failures.

Reproduce using a new external output path, from the repository root:

```bash
python3 -B - <<'PY'
import json, pathlib, subprocess
h = pathlib.Path('s2n-tls/tests/OrdinaryQualification')
selection = json.loads((h / 'selection.json').read_text())
command = ['python3', '-B', str(h / 'qualify.py'),
           '--campaign', 's2n-tls/build/campaign/20261023-223542-f7e7054e',
           '--output', '/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/ordinary-c/new-attempt']
for batch in selection['batches']:
    for case in batch:
        command += ['--case', case]
raise SystemExit(subprocess.call(command))
PY
```

Review a completed attempt with:

```bash
python3 -B s2n-tls/tests/OrdinaryQualification/review.py \
  /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/ordinary-c/attempt3 \
  --output /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/ordinary-c/attempt3/review.json
```

Execution is serial, with a 2 GiB minimum filesystem reserve, 90-second case
timeouts, bounded command timeouts/CPU/file sizes, no core dumps, two .NET
processors and private outputs/temp directories. No shared tool build, cache
restoration, shared-runner edit, central-ledger edit, provider/candidate apply,
protocol/Python suite, delegation or push is part of this harness.
