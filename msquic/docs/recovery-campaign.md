# Packet recovery and independent interop campaign

The current verification driver continues after recognized upstream recovery
limitations and emits a warning stating that these tests are timing dependent
and the outcomes are also observable in upstream MsQuic. This covers the
dual-live-mapping rebinding snapshot (new path unvalidated, old path validated)
and the previously classified decreasing-MTU idle/deadline outcomes. The existing
observation classifier must validate the endpoint, proxy and cleanup evidence;
an arbitrary failure in either scenario is still fatal. Expired-mapping rebinding
remains a required positive control.

Warning cases retain `passed: false`, the original error and the recognized
outcome in `results.json`. `verification_passed: true` means the selected matrix
finished with no unexpected failure; `cases_warned` counts known limitations
separately from `cases_passed`. The campaign records and displays these test
warnings regardless of its hash policy. Use `--strict-recovery` on
`scripts/test-recovery.py` to restore fail-fast recovery qualification. Historical
strict results described below remain unchanged.

The [2026-09-25 investigation](recovery-investigation-20260925.md) captures the
pending PATH_RESPONSE overwrite, oversized retransmissions and separate harness
deadline behavior without changing the core or relabeling strict failures.

The [2026-09-24 compiler refresh](verification-20260924.md) records a fresh
execution against the latest regenerated sources. The checkpoints below retain
their historical inputs and outcomes.

The complete ordinary managed/aioquic matrix has passed all 88 cases, and the
separate CID matrix has passed all 152 cases. Their receipts before the proxy
lifetime-evidence change are preserved at
`artifacts/managed-independent-before-io-lifetime/results.json` and
`artifacts/cid-rotation-before-io-lifetime/results.json`. Both drivers import the
recovery helper module. Fresh complete executions now also pass with the current
helper hash at `artifacts/managed-independent/results.json` (88 cases) and
`artifacts/cid-rotation/results.json` (152 cases). The historical receipts remain
preserved separately; they were not relabeled as runs of the changed helper.

The full ten-scenario positive recovery campaign now passes all 480 cases at
`artifacts/recovery-final-positive/results.json` on the current FIN/cleanup-barrier
peer baseline. Its `targeted_passed=true` records that complete selected matrix;
top-level `passed=false` does not credit the two excluded stronger scenarios.
The dependent ordinary 88-case, CID 152-case and endpoint 64-profile matrices
also pass fresh executions with this baseline. The fixed repeated observation
campaign is complete, with the unmatched terminal outcome described below. The separate full 20-pair benchmark, including shutdown durations, passes;
[performance investigation](performance.md) documents the measured tradeoffs and
limits of the successful diagnostic CPU trace. The first complete positive
campaign passed 36 cases before an unclassified proxy receive error stopped it;
both peers in that case had completed exact payload/FIN and clean teardown.
The lifetime evidence and strict classification below distinguish that situation
from live or unknown-process errors. No transport assertion was removed.

Earlier targeted optimized JIT recovery receipts establish baseline, Initial
loss, periodic loss, reordering, duplication, delay and combined faults
(`artifacts/recovery-checked/results.json`). Expired-mapping rebinding also passes
strict new-path validation in all four native/managed role combinations. The
original dual-live-mapping and decreasing-payload-ceiling failures remain
preserved native-equivalent observations, described below; they do not become
passing stronger recovery checks.

All 14 isolated UDP proxy controls pass. In addition to opaque-datagram fault,
queue and mapping controls, real IPv4/IPv6 kernel refusals distinguish a closed
socket in a still-live subprocess, a process already observed exited through a
pidfd, and missing process observation. Pipe acknowledgments establish socket
closure; the checks do not guess its timing with sleeps. Validator controls
reject wrong process identity, other errno, omitted events, raw-count mismatch,
unknown/live lifetime and unsuccessful endpoint evidence. These controls qualify
the test harness rather than QUIC recovery.

Both drivers reuse executables from `scripts/test-managed-peer.py`. Before and
after execution they verify its source, generated output, executable, and closure
hashes. A selected baseline case must already have passed for each requested
variant, runtime, role, cipher, certificate, and IP family. No native transport
library is loaded by the managed application; oracle peers run separately.

## Packet recovery

```sh
python3 msquic/scripts/test-recovery.py
```

The complete packet recovery subset contains 576 exchanges: raw/optimized,
JIT/NativeAOT, IPv4/IPv6, AES-128/AES-256, translated pairs and both roles against
native MsQuic, across twelve scenarios:

| Scenario | Required observation |
| --- | --- |
| Baseline | Both peers exchange the exact payload through the proxy. |
| Handshake loss | The first client datagram is dropped and the connection recovers. |
| Loss | Periodic drops occur while both stream payloads arrive intact. |
| Reordering | Original packets are actually forwarded out of order. |
| Duplication | Duplicate datagrams are actually forwarded. |
| Delay | Configured bounded delay and seeded jitter are applied. |
| Combined | Loss, reordering, duplication, and delay all occur. |
| Rebinding | The server-facing source port changes during the exchange. |
| Rebinding with expired mapping | The source port changes and replies arriving on the old mapping are explicitly discarded; the new server path must validate. |
| MTU probe loss | Oversized probes are dropped while baseline-sized data can pass. |
| Payload ceiling increase | Each direction starts at 1,300 bytes, drops an actual oversized probe, then raises its ceiling to 1,472 bytes and forwards subsequent traffic. |
| Payload ceiling decrease | Oversized datagrams are dropped after the ceiling changes. |

Each direction carries 65,537 deterministic bytes and FIN. Peer assertions cover
authenticated QUIC v1/P256 negotiation, exact payloads, send completion, shutdown,
and drained host ownership. Core stream-byte statistics include retransmitted
frames, so they may exceed the exact application payload count; the application
checks every byte and offset independently. Proxy assertions cover effective faults and bounded
queues. Counters distinguish intentional faults from queue overflow, truncation,
foreign traffic, socket errors, and shutdown abandonment.

The Linux proxy also records up to 64 I/O-error events with errno, operation,
direction, socket port, monotonic observation time, and the server's process
lifetime observed through a pidfd. Raw error counters are never reduced. A
connected-backend `ECONNREFUSED` observed after that same server process exited
can be classified separately only when its actual endpoint result establishes
successful payload/FIN completion, clean shutdown, and managed ownership drain.
This establishes the time of observation, not when the kernel generated an
ICMP error. Errors observed while the server is alive, unknown process lifetime,
other errors, incomplete event evidence, and mismatched counters still fail.
The original positive campaign stopped after 36 passing cases because it had
one unclassified proxy receive error despite successful endpoints; its receipt
is preserved in `artifacts/recovery-final-positive-before-io-lifetime`.

The first run with this evidence then rejected a refusal observed while the
server process was still alive, after both endpoints had completed successfully
(`artifacts/recovery-final-positive-before-proxy-drain-barrier`). Process exit
is later than UDP binding disposal, so process lifetime alone cannot prevent
delayed proxy traffic from reaching an intentionally closed socket. The test
peers now use a bounded shutdown barrier: after connection shutdown the server
retains its listener binding, the driver drains and stops the proxy, and only
then releases server cleanup. Every ordinary endpoint, exact payload/FIN,
terminal-status and ownership assertion still runs after cleanup. A missing
barrier, timeout, proxy failure or unexpected I/O error remains a failure.

The increasing-ceiling scenario originally raised its ceiling after twelve
received packets. A fast handshake could reach that ordinal before an oversized
probe, producing no actual loss; the strict gate correctly rejected that run
after 79 passing cases (`artifacts/recovery-final-positive-before-drop-trigger`).
Its trigger now depends on the first observed oversized-packet drop in each
direction. Before/after counters establish that traffic exercised both ceilings.
The mutually exclusive packet-ordinal trigger remains available for the
decreasing-ceiling scenario. The corrected increasing-ceiling control passes all
64 variant/runtime/IP/cipher/native-managed role profiles before the full run.

The subsequent run completed 362 cases before a periodic-loss pair delivered
both payloads and FINs but its server later closed with idle status 62. The
client's final packet ordinal was dropped; loss of its connection-close packet
is consistent with the evidence, but the encrypted packet was not decoded to
prove that attribution. The original failed outcome remains at
`artifacts/recovery-final-positive-before-fin-barrier`.

Recovery peers now keep the connection open until both incoming FIN and actual
graceful `SEND_SHUTDOWN_COMPLETE` have arrived. The pinned `stream_send.c`
emits the latter only when all queued bytes and FIN are acknowledged. Each peer
publishes its process identity at that barrier; the driver releases local
connection shutdown only after observing both identities. The existing client
path-validation settling interval precedes this release. This qualifies reliable
stream recovery without making success depend on delivery of an unacknowledged
connection-close packet. Both exact payloads, FIN acknowledgments, clean terminal
status and complete ownership drain remain mandatory. An incomplete/idle pair
never receives this release and remains a strict failure.

For both translated and native servers, source-port cases require the final
remote port to match the proxy's new port and the actual active path to be
validated. Native snapshots use the exact pinned native core headers and
compilation defines, with their provenance bound in the baseline receipt.
Earlier native rows with public counters alone establish continued delivery but
cannot identify the validated path. Likewise the payload ceiling models loss above a
threshold; it does not synthesize ICMP or by itself establish MTU discovery.
The pinned upstream discovery algorithm increases MTU monotonically. A ceiling
decrease may produce a bounded failure in native MsQuic as well; that requires
a native-to-native control before diagnosing a compiler or host regression.
`--roles native` selects such an additional test-only control. The driver records
incomplete delivery as failed recovery, but recognized upstream outcomes now
warn and continue unless `--strict-recovery` is selected.
The separate `scripts/classify-native-recovery.py` observation classifier
previously validated an optimized JIT IPv4/AES-128 set against its recorded
historical baseline in
`artifacts/recovery-native-observations/results.json`. It does not change the
strict driver's failures or claim successful recovery.

The original rebinding case preserves both reply mappings, which causes the
server to challenge both paths while the client observes one proxy endpoint.
Some translated and exact native core-header diagnostics show the old path
validated and the new active path unvalidated. A later raw JIT IPv4/AES-256
native attempt passed the original strict gate with both paths validated and
`paths_validated=2`; that success remains unchanged in
`artifacts/recovery-final-native-observations/raw-jit-ipv4-256-rebinding-native/results.json`.
These observations establish that the failure can occur in the pinned native
core, not that it always occurs or has the same frequency in both implementations. Source review
suggests that two challenges arriving through one perceived client path overwrite
its single pending response; that causal explanation remains an inference.
The separate expired-mapping case models disappearance of the old NAT mapping
and counts those discarded replies explicitly. Its new-path validation assertion
is identical and passes the targeted four-role comparison. The original failing
case remains in the campaign and is never relabeled as validated migration.

Targeted optimized JIT IPv4/AES-128 MTU controls now pass probe loss and a ceiling
increase for both native pairs and translated pairs. A ceiling decrease fails
delivery in both (`artifacts/recovery-native-mtu/results.json` and
`artifacts/recovery-managed-mtu/results.json`). Translated peers report actual
connection-idle status 62 and drain all host ownership. Refreshed native
terminal diagnostics (`artifacts/recovery-native-mtu-terminal/results.json`)
also show both endpoints connected, unfinished, then closed with status 62 and
transport error 1. The strict delivery test remains failed for both, as expected
from the pinned monotonically increasing discovery algorithm.

The observation classifier accepts recovery receipt paths as positional
arguments. Every receipt must bind the same current, qualified peer baseline,
executed binaries, certificates and unchanged driver/proxy sources. The full
sixteen-profile campaign requires exactly five independent dual-live-mapping
attempts per native/native and managed/managed role, one decreasing-ceiling
attempt per role, and passing expired-mapping, probe-loss and increasing-ceiling
controls. Existing executions, including the native success, count as attempt
zero; unique output directories retain all subsequent outcomes. Repetition count
is fixed before execution and is not increased until a desired result appears.

Successful rebinding observations require actual new-path validation, exact
payload/FIN acknowledgment, clean status and ownership drain. Failures retain
the exact old-validated/new-unvalidated path shape, no pending challenges or
responses, and the original strict error. Every managed failure needs at least
one same-profile/configuration native failure witness. The classifier rejects
duplicate receipt paths, copied receipt content and repeated underlying proxy
execution evidence; it does not infer identical failure rates or a common cause.
Its 80 logical scenario/profile comparisons retain each native and managed
success/failure count and all receipt/case links. Top-level `strict_passed` and
`entire_p7_qualified` remain false even when `observation_validated` becomes true.
The full fixed campaign is complete: 160 rebinding observations and 32
decreasing-ceiling observations, plus 48 native positive controls and the
separate 480 passing selected recovery exchanges. All 80 logical comparisons
are retained at `artifacts/recovery-final-native-observations/classification.json`;
`controller.json` records completion and intentional exit status 1.

| Stronger observation | Native | Managed |
| --- | --- | --- |
| Dual-live-mapping new-path validation | 19 strict successes; 61 strict failures | 13 strict successes; 67 strict failures |
| Decreasing-ceiling delivery | 16 failures ending in idle 62 / error 1 | 15 failures ending in idle 62 / error 1; one harness FIN deadline |

Every managed rebinding failure has a same-profile/configuration native witness.
These counts do not establish equal frequencies or a common cause. The sole
unmatched comparison is decreasing-ceiling delivery under optimized NativeAOT,
IPv4 and AES-256. The managed client explicitly logs its 15-second FIN deadline,
then requests local cleanup; the server observes peer close. Both remain
unfinished, exit 1 and report status/error 0 with complete ownership drain. The
matching native observation instead ends with actual idle 62 / error 1. The
classifier binds both endpoint logs, final JSON and callback evidence and keeps
`managed_harness_fin_deadline` distinct from `transport_idle_62_1`. Neither is a
successful transfer. Missing markers, unrelated errors or crashes cannot satisfy
this recorded shape. No extra repetitions were added after observing it.

The classifier emits `complete_evidence_validated=true` but
`observation_validated=false` and `strict_passed=false`. All strict receipts stay
unchanged. Evidence completeness does not qualify these stronger recovery gates
or establish why the terminal kinds differ.

The idle-terminal subset of decreasing-ceiling failures requires both endpoints
to close with actual idle status 62 / transport error 1, incomplete payloads and
clean ownership. The explicit harness-deadline terminal kind above is separate. Actual
client-direction drops can prevent the request from completing, so the response
direction never reaches its change ordinal. The classifier requires the real
triggering decrease and drops; it does not claim both directions decreased.

Focused classifier checks pass against 55 recorded cases (48 native positive
controls, four strict failures, the native rebinding success, a managed success
with an aggregate path-failure counter and the explicit harness deadline), with 32
acceptance/adversarial checks recorded in
`artifacts/repeated-observation-controls/results.json`. They reject relabeled
outcomes, wrong status/counters, missing FIN acknowledgment or cleanup, crashes,
stale baselines, duplicate evidence and an incomplete campaign. An initial test
assertion expected a different rejection message for incomplete coverage; that
failed assertion receipt is preserved, and the classifier itself was unchanged.

A later strict success retained a validated new active path while `PATH_FAILURE`
was one. The original classifier incorrectly required that aggregate counter to
be zero. Pinned `connection.c` increments it for any unvalidated path that times
out before removing that path; an old-path timeout therefore need not invalidate
a new-path success. The receipt and classifier failure remain archived. The
corrected check still requires actual new-path validation, clean terminal state,
FIN acknowledgments and ownership drain; negative-counter and unvalidated-path
controls reject altered evidence. The specific old-path attribution is consistent
with the snapshot, but no per-path timeout trace was collected.

For a diagnostic subset:

```sh
python3 msquic/scripts/test-recovery.py --variants optimized --jit-only \
  --roles both --families ipv4 --ciphers 128 --scenarios baseline handshake-loss
```

Only a complete subset sets `passed`; selected runs set `targeted_passed`.
Both retain `entire_p7_qualified: false` because other mandatory feature gates
remain outside this driver. A configured fault that never occurs fails its case.

## Independent transport and authentication

```sh
python3 msquic/scripts/test-managed-independent.py
```

The independent peer is the pinned aioquic test process described in
[independent-peer.md](independent-peer.md). Before using its environment, the
driver checks every installed Python module and native extension against its
hash-pinned wheel. Its optional `--alpn` argument changes only the test peer's
configured application protocol; the default remains `dotcc-probe`.

The full subset contains 64 positive exchanges and 24 authentication negatives.
All 88 pass in the fresh source-bound rerun after the helper update as well as
in the separately preserved earlier execution.
Positive rows cover both endpoint roles, IPv4/IPv6, AES-128/AES-256, ECDSA/RSA
certificates, raw/optimized, and JIT/NativeAOT. Each runtime/variant/role also
rejects an unrelated trust root, a wrong hostname, and a disjoint ALPN.

Negative cases require the expected QUIC TLS alert, no application data, and
drained managed host resources. An unrelated setup failure or timeout cannot
satisfy the expected-alert check. Rejection before listener admission does not
invent an application connection or require a callback for a nonexistent handle.
The pinned aioquic server uses TLS `handshake_failure` (40) for a disjoint ALPN;
the translated picotls server uses `no_application_protocol` (120). The driver
checks the actual peer-specific alert rather than treating either as a timeout.

This subset does not establish tickets, key updates, Retry, stateless reset,
DATAGRAM, CID rotation, resource exhaustion, or the complete upstream test suite.
Those remain separate requirements in [PLAN.md](PLAN.md).


## Independent connection-ID rotation

```sh
python3 msquic/scripts/test-managed-independent.py --rotate-cid \
  --output msquic/artifacts/cid-rotation
```

This complete matrix passed 152 cases: 64 matched ordinary profiles, 64 actual
rotations, and 24 authentication negatives. It covers both roles, IP families,
AES suites, certificate types and raw/optimized JIT/NativeAOT. As with ordinary
interop, a fresh complete source-bound rerun passes after the recovery helper
change. The earlier completed receipt remains preserved separately.

The pinned aioquic peer performs its real `change_connection_id()` operation
between acknowledged transport barriers, then finishes the exact payload/FIN
exchange. The test records the old/new CID and sequence, and requires the
translated core's destination-CID update counter to increase against its matched
ordinary profile. These are actual packet/state observations, not synthetic
callbacks or a counter-only model.

A dedicated MsQuic client binding intentionally uses a zero-length source CID,
so client-role rotation comparisons enable and query the actual shared-binding
parameter before connection start. Both ordinary and rotation rows use that same
setting. The ordinary P6/recovery baseline explicitly keeps it disabled. The
initial zero-CID setup failure remains in `artifacts/cid-rotation-initial/`; no
product or pinned source change was needed. CID qualification is separate from
Retry, key updates, ticket resumption, DATAGRAM and full recovery gates.
