# Recovery parity follow-up — 2026-10-23

**No compiler/translation divergence has been demonstrated. Full semantic parity
has not been demonstrated either.** The 87 strict recovery failures are not 87
compiler discrepancies. Equivalent native unsuccessful outcomes are relevant to
parity, while remaining unsuccessful recovery.

This follow-up preserves the original campaign and fixed-sampling receipts. It
adds independently checked historical case identities, an explicit mapping from
current failures to native outcome witnesses, and a lifecycle source audit.
Detailed evidence is in the [history audit](recovery-history-audit-20261023.md),
[runtime audit](recovery-runtime-audit-20261023.md), and
`artifacts/recovery-parity-investigation-20261023/`. The separate bounded runtime
controls below preserve every attempt and do not rewrite historical outcomes.

## What the failure count means

The frozen compiler-source `123b3e76e` campaign has 576 cases: 489 strict
successes and 87 recognized warnings that retain `passed=false`. Its ten positive
scenarios pass all 480 cases. Dual-live rebinding passes 9/48 and fails 39/48;
decreasing payload ceilings fail all 48 transfers. The warning-policy command
exit of zero is not a successful stronger-recovery result.

The full matrices use the same 576 case keys and normalized fault configurations:

| Complete collection | Strict successes | Strict failures | Rebinding failures | Ceiling failures |
| --- | ---: | ---: | ---: | ---: |
| September 24 | 495 | 81 | 33 | 48 |
| September 27 | 488 | 88 | 40 | 48 |
| October 18 | 491 | 85 | 37 | 48 |
| October 23 frozen | 489 | 87 | 39 | 48 |

All strict pass/fail transitions between these collections are rebinding. Against
October 18, nine cases changed to failure and seven to success, giving two more
failures overall. Against September 27 there is one fewer failure. These different
products, native binaries, certificates and schedules do not isolate a regression
or establish equal failure probabilities. The evidence does establish that this
scale of strict recovery failure predates the latest compiler.

Earlier 480-case positive-only runs excluded both stronger scenarios. The first
declared matrix had 528 cases and lacked the expired-mapping control; an early
native rebinding "pass" checked delivery without actual native new-path
validation. Later native path snapshots made that gate meaningful. Also,
September 27's 87 warnings accompanied one unexpected failure: its strict failure
count was 88. Those are concrete coverage/reporting differences, not compiler
discrepancies.

## Native outcome witnesses and the limits of the comparison

The new read-only [accounting audit](../artifacts/recovery-parity-investigation-20261023/accounting/audit.py)
verifies the retained archive and member hashes, actual endpoint JSON records,
actual proxy statistics, unique executions, shared receipt bindings and fault
configurations. It invokes the unchanged per-case classifier; it does not grant
official qualification.

Of the original 87 failures, **84 have a same-profile/configuration native witness
for the checked failure kind**: all 39 unvalidated rebound paths and 45/48 ceiling
failures with both endpoints idle `62/1/0`. The other three are the managed/managed
and two mixed-role ceiling cases in raw/AOT/IPv6/AES-128. They share the one
profile whose fixed native control has an unclassified terminal. They are three
missing exact witnesses, not three demonstrated translation defects.

The immutable 288-observation sample contains 80 logical groups: 160 rebinding
attempts, 32 ceiling failures and 96 positive controls. Its original official
classifier aborts before comparisons on the unsupported native case. The earlier
diagnostic **79/80** counts groups with native witnesses for *managed failure
kinds*. It is one-way support, not 79 parity passes. Comparing both observed
success and failure kind sets gives **72/80** equal sets. Seven rebinding groups
contain success on only one side of five attempts; the eighth group is the
unsupported native ceiling terminal. Finite sampling gaps neither prove
divergence nor establish that the absent outcomes are possible on both sides.

The checked failure kinds include negotiation, lifecycle, actual faults, cleanup
and managed ownership gates. They allow partial byte counts, packetization and
timing to differ and do not compare complete callback/packet/timer histories or
establish integrity of every failed transfer's application prefix. Thus even the
84 supported failure rows establish scoped observational agreement, not universal
semantic equality. Both sides failing and similar totals alone establish neither.

## Supported mechanisms and the unresolved native terminal

The pinned C has one pending PATH_RESPONSE token per path. Two challenges
delivered before a response can overwrite the first token. The old/new proxy
mapping arrangement can expose this ordering; historical worker/frame traces
capture it. Expiring the old mapping avoids that specific challenge interaction
and passes its stronger control. This supports an upstream mechanism, not an
attribution of every current rebinding failure to an uncaptured trace.

The pinned MTU discovery searches upward. After a silent payload-ceiling drop it
can keep retransmitting above the new limit. In the exact native case, IPv6 IP
MTU 1360 permits 1312 UDP bytes, exceeding the proxy's 1300-byte ceiling; the
proxy records actual drops. Earlier packet traces and capped-MTU controls
substantiate this black-hole mechanism. Capping MTU changes the workload and is
not successful downward MTU recovery. No upstream repair is proposed here.

The unsupported native/native client connected, remained unfinished, and closed
with status/error/peer-error `0/0/0`; its server reported `62/1/0`. Both endpoints
are native C; raw/AOT is only its matching managed-profile label. Its log lacks an
explicit wait-exit or deadline marker. Elapsed 16.0775 seconds does not identify
its cause.

Source review finds that the native client ignores its FIN wait's return and
unconditionally calls application shutdown. Such a close can complete without a
transport/peer shutdown-begin callback, leaving zero error fields. Its operation
is queued ahead of ordinary timer operations; first-close rules preserve the
winning reason. Server idle closure is silent and need not notify the client.
Atomic publication and the completed snapshot do not explain a lost preceding
status in this case. These facts provide supported possible mechanisms, not the
missing historical execution order. The exact event remains unclassified.

## New bounded lifecycle controls

The separately declared IPv6/AES-128 diagnostic set ran exactly **14 pairs**:
two baseline pairs per implementation, four unchanged ceiling-drop pairs per
implementation, and one eight-second late-packet control per implementation,
alternating native and managed. All four baselines passed. All eight unchanged
ceiling-drop pairs failed with both endpoints idle `62/1/0`; none reproduced the
historical asymmetric native terminal. Both late controls failed delivery: native
ended `0/0/0` at both endpoints; managed ended client `0/0/0`, server `62/1/0`.
No outcome-selected repeat was added.

The native diagnostic logs explicitly record the FIN wait exhausting 15,000
sleeps after 15.7877 seconds, with no earlier failure or shutdown-begin event.
The following owner shutdown request is processed as the first application close
(flags `0x04`, status/error zero). At processing, its idle deadline is still about
2.295 seconds away. These worker observations establish a harness-budget cause
for **this new control**, beyond inferring it from elapsed time. The managed
control explicitly reports its 15-second FIN deadline, then local client cleanup;
its server independently closes idle. This demonstrates supported lifecycle and
timing sensitivity, without proving why the old native client stopped waiting.

These controls use a private, observation-instrumented pinned native oracle and
the unchanged canonical raw NativeAOT managed executable from the earlier
baseline, not reconstructed frozen-generation-2 products. Commands, input and
binary hashes, instrumentation patches, preparation failures and all endpoint/
proxy records are retained under `experiments/`. They are diagnostic evidence,
not requalification of compiler `123b3e76e` or additions to the old fixed sample.
The original full bidirectional ceiling-exercise gate also rejects the negative
runs because reverse traffic ends before that direction changes its ceiling.
That failure is retained. Client-direction ordinal/drop evidence supports the
negative black-hole observation; it does not establish both directional changes.
See the [experiment report](recovery-experiments-20261023.md) for the full checks
and limitations.

## Reproduction and remaining evidence

Run the read-only accounting and source/history checks from the repository root:

```sh
python3 msquic/artifacts/recovery-parity-investigation-20261023/accounting/audit.py
python3 msquic/artifacts/recovery-parity-investigation-20261023/history/audit.py
python3 msquic/artifacts/recovery-parity-investigation-20261023/runtime/source-audit.py
```

The accounting JSON records every original failed case and its native references,
all 80 logical groups, terminal values and missing witnesses. Historical commands,
compiler/library/source identities, case transitions and source comparisons are
retained by the companion audits. The original four-campaign archive remains
unchanged. The source audit checks 12 pinned files and 21 retained evidence
bindings. No compiler, product runtime or normal recovery assertion was changed.
The [validation receipt](../../docs/validation/msquic-recovery-parity-investigation-20261023.json)
indexes the packaged evidence and its hashes. The retained text-evidence archive
is `artifacts/recovery-parity-investigation-20261023/evidence.tar.gz`; essential
executables remain locally retained and separately hash-indexed. Private test
keys are excluded from that archive.

Resolving the exact old native cause requires telemetry it did not retain: the
FIN wait's exit reason and first failure, local shutdown invocation, callback
presence, and worker first-close/timer order with accepted packet times and PTO
inputs. A new matching-looking run cannot reconstruct that history. Full parity
also needs a clearly scoped unsuccessful-outcome contract with prefix integrity,
reason/event provenance and schedule-sensitive comparison. The current evidence
does not justify a broad zero-status whitelist or a universal parity claim.
