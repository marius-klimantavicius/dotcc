# Recovery lifecycle source audit — 2026-10-23

The unmatched native client `0/0/0, connected=1, finished=0, closed=1` remains
unclassified. The pinned core supports an application-initiated close with no
shutdown-begin notification; the native harness can initiate that close after a
bounded wait. Source review establishes this mechanism, not that the historical
wait actually expired. The historical log contains neither its exit reason nor
the first-close event. There is no evidence here of a lost atomic publication or
a DotCC miscompilation causing this native/native result.

This audit covers source and retained-evidence review only. It did not run
peers, build products or native binaries, change the classifier, or change the
compiler/runtime. Experiments below are proposals for the supervisor, not results.
The source excerpts, hashes, archive comparisons, and derived observations are
under `artifacts/recovery-parity-investigation-20261023/runtime/`.

The subsequent [bounded experiment report](recovery-experiments-20261023.md)
records a separately declared and executed inventory. Its new native budget-close
control does not supply the missing historical wait/first-close trace.

## Evidence and scope

- Upstream: `80a065112426bce68c1da42d026478d3e40fd45e`, selected by
  `config/source.json`. Reviewed source files are compared with that cached,
  checksum-verified archive in `runtime/source-audit.json`.
- Native harness: `tests/NativePeer/peer.c`, SHA-256
  `c36f604686563743eff5c75e6a016479547055bab557e6947ded9dd1ac14b51d`.
  This equals the source binding in the immutable finding. Its executable and
  native library from the four-campaign private workspace were deleted; this
  review does not execute or pretend to reconstruct those binaries.
- Managed harness: `tests/ManagedPeer/Program.cs`, SHA-256
  `aff5080a5bc0256af9de669ab2047d314876b06deab60b7f0e3fa75e27b9cfe7`.
  Its hash also equals the uninstrumented source recorded by the September 25
  diagnostic build. Equal harness source does not establish equal product DLLs.
- Historical unmatched case: retained
  `/tmp/dotcc-four-campaign-20261023/evidence/msquic/native-unsupported-terminal/`.
  The retained file hashes are checked against `finding.json`; originals remain
  untouched. `runtime/source-audit.json` records the checks.
- Current generated raw/processed C# was inspected for the close, idle, PTO,
  disconnect and MTU paths. This is a bounded source review of the available
  files, whose hashes are retained. Those local files are not asserted to be the
  deleted four-campaign output emitted by compiler `123b3e76e`.

The qualification accounting remains 576 full recovery observations: 489 strict
passes and 87 recognized warnings (39 dual-live rebinding, 48 ceiling-down).
Neither these warnings nor 287/288 accepted supplemental shapes establish full
semantic parity. The exact unmatched native result differs from the same-profile
managed `62/1` idle result and remains an explicit unresolved observation.

## What the native harness actually observes

`peer.c:152` waits up to 15,000 one-millisecond `nanosleep` calls. It returns early
if the requested atomic flag is set, or if `stop_on_failure` is enabled and the
process-global atomic `failure` is set. It neither uses an absolute monotonic
deadline nor logs which exit occurred. Interrupted sleeps are not retried in this
loop. Scheduling overhead can lengthen the interval; signals can shorten the
accumulated sleep. The historical 16.0775-second total is consistent with this
loop but is not its execution trace.

The client ignores the return value (`peer.c:343`), optionally waits for the
recovery transfer barrier, then unconditionally calls
`ConnectionShutdown(NONE, 0)` at line 351. The transfer barrier returns immediately
if the process already failed or the connection closed (`peer.c:201`). It is not
a second independent observation window after a failed FIN wait. The client then
waits for `closed` with failure checking disabled.

`finished` means receipt of the peer's stream FIN, not connection shutdown or
successful delivery of the local FIN. `send_fin_acknowledged` has a separate
graceful-send callback. A failed transfer followed by orderly handle cleanup can
therefore have `finished=0, closed=1`. Neither flag should be repurposed into a
successful-recovery assertion.

The final error fields start at zero. The callback writes transport fields only
for `SHUTDOWN_INITIATED_BY_TRANSPORT`, and peer error only for
`SHUTDOWN_INITIATED_BY_PEER` (`peer.c:124`). No event-presence field distinguishes
an absent notification from an observed zero-valued notification. `closed` records
`SHUTDOWN_COMPLETE`; its handshake, peer-acknowledgment and app-close bits are
discarded. The scalar `passed=false` also does not identify which assertion first
failed: payload mismatches, canceled sends and some other failures set the same
global bit without printing a reason.

## Why zero status is a supported lifecycle shape

The following control flow is present in the pinned C and in the inspected raw
and processed C#:

1. `connection.c:438` maps public application shutdown to
   `QUIC_CLOSE_APPLICATION` (`0x04`), with no
   `QUIC_CLOSE_SEND_NOTIFICATION` (`0x02`). The latter flag belongs to internal
   transport shutdown (`connection.h:17`).
2. `QuicConnTryClose`, `connection.c:1470`, stores the first close status, cancels
   non-shutdown timers, and indicates shutdown begin only when
   `QUIC_CLOSE_SEND_NOTIFICATION` is set (`connection.c:1648`). Local application
   close with error zero thus requires no transport/peer begin callback.
3. Later close attempts do not replace the first reason. Repeated local close
   returns early at `connection.c:1486`; a subsequent opposite-side close takes
   the completion branch with `IsFirstCloseForConnection=FALSE`.
4. `connection.c:1375` eventually indicates `SHUTDOWN_COMPLETE` independently of
   a begin callback. Once shutdown completes, the callback handler is cleared.

A native client locally closed by its harness can consequently finish with all
three error fields zero and `closed=1` after an incomplete transfer. A remote
application close with error zero is another indistinguishable shape in the
current JSON. The exact case needs event presence and first-close provenance to
select between possibilities; the final values alone are insufficient.

There is a further ordering issue relevant to future instrumentation.
`api.c:321` queues application shutdown at the front of the connection operation
queue through `QuicConnQueueHighestPriorityOper` (`connection.c:779`,
`operation.c:190`). Expiring idle/loss timers normally enqueue timer operations
at the tail (`connection.c:1271`). A harness close can be processed ahead of an
already queued expiration. The first-close rules can then suppress the transport
notification from that expiration. This is source-permitted ordering, not an
observed explanation of the historical case. Recording only an expired timer's
scheduled time would not establish which operation won.

## Publication and snapshots

Native transport fields are ordinary C storage, but are written on the connection
worker before the callback's sequentially consistent `atomic_store(closed,1)`.
The waiting/snapshot owner performs atomic loads. For a completed connection this
publishes preceding callback writes. The exact case also has
`private_paths_snapshot=true`, whose guard at `peer.c:245` requires observing
`closed` before private storage is read. Native result fields are not reset to
zero later. No missing release/acquire pair was identified that explains losing
a preceding transport error in this case.

The normal native snapshot first uses synchronous `GetParam` for remote address
and statistics. From the external owner thread, `api.c:1883` queues this work on
the same worker and waits for completion. The connection remains handle-owned
until after the snapshot. `QuicConnOnShutdownComplete` removes packet lookup,
removes the timer-wheel entry and uninitializes loss/send state before its final
callback. Subsequent receive, timer, unreachable and route-completion operations
are ignored when `ShutdownComplete` is set (`connection.c:8166`). These facts
support the final path snapshot's intended quiescent use in this case.

This does not justify polling live private structures from another thread.
Historical `closed=1` does not reveal *when* its callback occurred relative to the
FIN deadline, local close request or server timeout. A failed wait followed by a
forced blocking `ConnectionClose` can also generate cleanup callbacks. New
evidence should distinguish the first observation before local intervention from
state obtained during cleanup.

Managed callbacks use `Volatile.Write` for Connected/Finished/Closed/FIN flags.
Transport fields precede Closed publication (`ManagedPeer/Program.cs:283`). The
owning main thread closes handles after callbacks and reads the final result.
The managed FIN wait uses `Stopwatch` elapsed time with a 15-second budget
(`Program.cs:318`), whereas native counts sleeps. These are different harness
budgets even if their loops use similarly named constants.

## Timers, MTU and directional asymmetry

Both test peers configure 10,000 ms idle timeout. Pinned
`QuicConnResetIdleTimeout` (`connection.c:6202`) takes the nonzero negotiated
minimum, then raises it to at least three PTOs. PTO is
`smoothed_rtt + 4*rtt_variance + max_ack_delay`, multiplied by the requested count
(`loss_detection.c:226`). It is not a fixed 10-second connection-age timer.

Receive processing resets idle after accepted packet processing
(`connection.c:5961`). Sending the first ack-eliciting packet with no packets in
flight can also reset it (`loss_detection.c:439`). Valid ACK/PING/control traffic
may extend lifetime without progressing the application stream. A delayed ACK
can also change RTT/variance and hence the minimum idle duration. Knowing only
the last proxy datagram arrival is insufficient: authenticated acceptance and
the resulting worker timer update matter.

The separate disconnect check uses the oldest outstanding ack-eliciting packet
and the configured default 16,000 ms timeout, evaluated in the loss timer handler
(`loss_detection.c:1849`, `quicdef.h:313`). It is neither a 16-second process
deadline nor guaranteed to run exactly at connection age 16 seconds. An extended
observation can end with status 110 rather than 62 without changing the
application failure. The inspected generated C# preserves these formulas,
millisecond/microsecond conversions and the 62/110 close constants.

Idle closure uses `QUIC_CLOSE_INTERNAL_SILENT | QUIC_CLOSE_QUIC_STATUS`
(`connection.c:6263`). `62/1` means local idle status 62 plus the core's generic
internal-error placeholder when a QUIC_STATUS is supplied (`connection.c:1632`).
It does not prove a CONNECTION_CLOSE frame carrying error 1 was sent. In
particular, the server's silent idle shutdown need not tell the client to close.
Different timer histories at the endpoints can therefore produce an asymmetric
terminal pair. This mechanism does not prove the exact client's history.

`mtu_discovery.c:82` explicitly requires an increasing search. Probe ACKs assign
the larger probe size to `Path->Mtu`; exhausted probes stop searching and do not
lower the established MTU. The packet builder starts from that path MTU
(`packet_builder.c:209`). Path initialization/settings can assign a minimum MTU;
this review's conclusion concerns a silent black hole on the existing validated
path, not every way a different path or configuration can set an MTU.

In the exact retained case the client's final IP MTU is 1360. IPv6 plus UDP
headers consume 48 bytes, so a full ordinary datagram is 1312 UDP bytes, above
the new 1300-byte proxy ceiling. The proxy recorded 80 client-direction MTU
drops and 38 forwarded datagrams. The reverse direction had 21 forwarded
datagrams, zero MTU drops, and only one packet after its own twentieth-packet
threshold. The server had received only 35,609 application bytes and sent no
application response. These observations support the MTU black-hole mechanism
and directional asymmetry; aggregate counters provide no timestamp for the
client's last accepted packet or the first close.

The BCL host supplies monotonic microseconds from `Stopwatch` with integer
conversion (`MsQuicHost.Platform.cs:121`); finite event/queue waits preserve a
monotonic deadline and round the remaining microseconds upward to milliseconds
(`Platform.cs:427`, `MsQuicHost.Queue.cs:205`). No competing 15-second recovery
timer was found in these services. The translated worker retains upstream timer
policy. UDP delivery/queue scheduling, packetization and TLS backend still
distinguish managed and native peers. A fixed proxy seed fixes fault choices,
not identical accepted-packet or timer histories. No host clock, queue or MTU
service defect was proved by this review.

## What September diagnostics establish

The retained September 25 controls establish a causal *possibility*: holding a
real reverse-direction packet can let the harness deadline precede transport
shutdown in managed and native peers. They are fresh connections with altered
delivery timing; the 15/30-second managed controls held different packet types.
They do not replay the exact October native observation, prove its deadline
cause, bind the October compiler closure, or establish failure frequencies.

The useful method to carry forward is recording plaintext test-frame identity
and timer/path snapshots on the connection worker, with logs flushed after
cleanup. Independent-thread reads of a live connection should not be used as
causal evidence. Instrumentation overhead must remain an acknowledged change
to scheduling even when protocol bytes/state policy are preserved.

## Proposed bounded experiments and fixed decision criteria

These proposals came from the source-only review and are not results. The
separate executed inventory is linked above. Declare a final inventory before
starting and retain all attempts, including readiness, compilation,
instrumentation and runner failures.

First add observation-only instrumentation in private native harness/core
copies. Record both endpoints, bounded in-memory event rings with explicit
overflow counts, monotonic timestamps, event sequence and worker identity:

- Per-wait target, entry/exit, iteration count, actual elapsed time and reason
  (`flag`, `failure`, `budget`); assign every failure store a reason.
- Owner-thread invocation of application shutdown/handle close and error code;
  worker first-close decision with flags/status/error and local/remote origin.
- Presence and payload of each shutdown-begin callback; all three public
  SHUTDOWN_COMPLETE bits; canceled send completion versus graceful FIN ACK.
- Worker timer set/reset/expire/process events, accepted receive packet/frame
  identity, PTO inputs, oldest outstanding packet time, MTU, and path state.
  Distinguish queued expiration from actual timer operation processing.

A fixed first inventory could be two baseline IPv6/AES128 native pairs and six
unmodified-ceiling IPv6/AES128 native pairs with observation only. Do not add
repetitions based on whether the desired terminal appears. A baseline regression,
ring overflow, native crash, unrelated host/proxy error or binary/source binding
failure makes the affected evidence unusable for a cause claim, while its raw
result stays retained. If the exact asymmetric shape does not recur, report
non-reproduction without assigning the old result a cause.

For a new occurrence, a **harness-budget cause** requires the unfinished-FIN
wait's explicit `budget` exit, no preceding other failure or transport/peer
shutdown-begin, and a following local application first close. Remaining idle
time greater than zero or queued-versus-processed timer ordering explains why
no transport reason was observed; elapsed duration alone never qualifies.

An optional separately declared diagnostic control can retain the 15-second
transfer failure but observe the same live connection for at most another 20
seconds before local intervention. It must not reset that failure or turn later
FIN receipt into a strict recovery pass. If idle/disconnect closes first, retain
its real reason; if no terminal appears, retain a censored observation. This
control changes harness intervention and is not the original test result.

A controlled late-packet experiment should use the same selection predicate,
delay, budgets and inventory for both endpoint implementations, record the
selected packet identity and actual release/acceptance times, and retain all
controls. It can establish sensitivity to timing. It cannot retroactively
identify the historical packet. A capped-MTU control similarly demonstrates a
mitigation while changing the workload; no-drop transfer success does not meet
the strict decreasing-ceiling fault gate.

Full parity still requires qualified observations across the required profiles,
including unsuccessful outcomes: valid negotiation, exact application prefix
integrity, actual FIN/send lifecycle, first close provenance, bounded drain, no
unrelated I/O/ownership failure, and the exercised fault. Equal failure booleans
or equal final scalar errors cannot replace those checks. No new general native
or managed failure kind is justified by this source-only audit.
