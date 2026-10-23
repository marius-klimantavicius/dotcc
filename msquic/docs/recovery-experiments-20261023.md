# Bounded recovery experiments, 2026-10-23

A new native control demonstrates an incomplete transfer ending with
`transport_status/transport_error/peer_error=0/0/0`: its FIN wait explicitly
exhausted the harness budget, then the application initiated the first close.
The managed control also records a client FIN timeout followed by zero-error
cleanup. This establishes a lifecycle possibility in new experiments. The
historical unsupported native terminal remains unclassified; its elapsed
16.0775 seconds still supplies no causal trace. No historical witness count is
changed, including the existing 84-witness inventory.

The [declaration](../artifacts/recovery-parity-investigation-20261023/experiments/declaration.json)
was persisted before builds or peer execution. All 14 pairs ran exactly once,
in the declared order, with seed 7381, IPv6, AES-128-GCM, P256, fresh common
credentials, the ordinary unshared client binding, and unchanged transfer gates:

| Ordinals | Pairs | Strict exchange | Client / server terminal status |
| --- | --- | --- | --- |
| 01–04 | Native, managed, native, managed baselines | 4 passed | All `0 / 0`, full payload and FIN |
| 05–12 | Four alternating native/managed ceiling pairs | 8 failed | All `62 / 62`, error `1` at both ends |
| 13 | One late-packet native pair | Failed | `0 / 0`, both unfinished |
| 14 | One late-packet raw NativeAOT managed pair | Failed | `0 / 62`, both unfinished |

All ten negative pairs exercised the client-direction UDP ceiling change:
1472 to 1300 bytes after ordinal 20, with actual oversize drops. The unchanged
full bidirectional-success fault validator separately failed all ten with
`Traffic ended before the payload ceiling changed`: the reverse direction
received only 9–20 packets and never crossed its change ordinal. This failure is
retained in every case. No claim is made that both directional ceiling changes
were exercised. The server responds only after receiving the entire request.
The existing classifier's unchanged negative criteria accept all eight original
ceiling pairs as `transport_idle_62_1`; their actual client-direction black hole
is evidenced. Neither late pair is accepted by that classifier, and its
rejections are preserved. No new classifier kind was added.

In the four original native ceiling pairs, the exact historical asymmetric
client-zero/server-idle shape did not recur: all four were idle/idle. This is
non-reproduction within the fixed inventory; there were no extra attempts.

For pair 13, the client trace records the following sequence. Event times use
the same monotonic clock; the original sleep-count budget remains unchanged.

1. `client.finished` enters its wait with failure clear. After 15,000
   one-millisecond sleeps, it exits explicitly for **budget**, with FIN absent
   and `failure_before=0`. Actual wait time is **15.787666396 seconds**.
2. The first failure store is `wait budget exhausted`. There is no earlier
   failure store, first close, transport shutdown-begin, or peer shutdown-begin.
3. The recovery-transfer helper immediately exits for that failure. The owner
   calls `ConnectionShutdown(NONE, 0)`; the worker processes it next.
4. The worker's first close entry has flags **4** (`QUIC_CLOSE_APPLICATION`),
   both prior local/remote close bits clear, and error zero. Its first-close
   decision is true; committed status and error remain zero. At that entry the
   idle deadline is still **2.294938231 seconds in the future**, with MTU 1440,
   smoothed RTT 564 microseconds and RTT variance 456 microseconds. No idle
   expiration/enqueue/process event precedes this close. This trace supports
   future idle expiry; it supplies no queued-idle-timer race example.
5. The client receives `SHUTDOWN_COMPLETE` with `HandshakeCompleted=1`,
   `PeerAcknowledgedShutdown=1`, `AppCloseInProgress=0`. Neither transport nor
   peer shutdown-begin callback occurs anywhere in its complete trace. Its
   closed wait exits on the flag; stream and connection handles close afterward.
   Final state is `connected=1, finished=0, closed=1`, FIN unacknowledged,
   with all three error scalars zero.

The native server received 18,098 verified request bytes and no FIN. Its first
close is remote application close, flags 14, status/error zero; its peer-begin
callback explicitly carries error zero. Thus the two zero-valued endpoints have
different close origins, now observable in callbacks and core events. The native
client retained no outstanding application payload buffer at cleanup. The
[timestamped trace](../artifacts/recovery-parity-investigation-20261023/experiments/late-native-client-timeline.json)
and full endpoint logs retain every event, including later close attempts.

Both late controls use the exact September selection predicate: the first
scheduled server short-header datagram after at least 20 client datagrams have
arrived. Each adds 8,000 ms, once. Native selected server ordinal 6, 1,312 bytes,
with 36 client datagrams observed; it released the same SHA-256 payload after
8,000.629548 ms. Managed selected server ordinal 5, 27 bytes, with 34 client
datagrams observed; release followed after 8,000.500482 ms. Both controls were
exercised. Selection/release monotonic timestamps, first bytes, lengths, digests
and actual delays are in `proxy.stats.json`. These are different encrypted
packets; neither identical plaintext frame content nor identical accepted-packet
or timer histories is established. Instrumentation can also alter scheduling.
Here, release means a successful proxy UDP send. It does not independently
timestamp authenticated endpoint acceptance: native ciphertext digests are not
directly bound to logged packet numbers, and managed acceptance timing is absent.

Managed pair 14's first printed client failure is `client payload FIN timed out`.
Its final client state is likewise unfinished but closed with `0/0/0`; its
callback sequence has shutdown-complete and no transport/peer shutdown-begin.
The bound harness source unconditionally calls local application shutdown after
this wait. The server instead reports transport idle `62/1/0`, with 35,603
verified request bytes. Managed telemetry has no new timestamped first-close
record or shutdown-complete flag payload, so its causal resolution is lower.
The native and managed whole-pair late lifecycles differ in their observed
server terminals; whole-pair parity is **not established**. This observation
alone does not demonstrate a semantic implementation discrepancy.

Every baseline delivered exactly 65,537 bytes each way, with the original byte
pattern, both received FINs, actual send-FIN acknowledgments before close,
negotiation and cleanup gates satisfied. All 28 endpoints negotiated QUIC v1,
AES-128-GCM and group 23. Every failed endpoint remained incomplete, with no
payload-corruption, offset, or early-FIN assertion observed. Native original
ceiling servers each received 18,098 bytes; managed counts varied from 18,093
to 35,603. These are verified received prefixes, not equal complete payloads.
Native sends have balanced allocation/release events, zero outstanding payload
buffers, and shutdown-complete before handle close at all 14 native endpoints;
this is not a general native allocator leak census. All 14 managed endpoints
report zero final host resources, allocations, receive leases, I/O errors and
truncations. There were no crashes, proxy I/O errors or ring overflows. Native
rings are bounded at 8,192 harness and 32,768 core events per process and flush
after cleanup. Core snapshots occur on the worker, with no live owner-thread
polling. Wait budgets, protocol/timer policy and transfer assertions were not
changed. Both baseline pairs per implementation passed, making the narrow new
budget-cause control usable under its declared criteria.

The native library was rebuilt privately from the locally verified MsQuic
`80a065112426bce68c1da42d026478d3e40fd45e` and quictls
`ff36838bb69801cad56823159a036977bcbe5c75` archives, with observation-only
`connection.c` additions and a private peer. Exact commands, defines, CMake
configuration, compiler/SDK identities, input hashes and patches are retained.
GCC is 13.3.0; observed installed .NET SDK is 10.0.112. That SDK observation does
not identify the compiler/SDK that originally produced the reused AOT peer.
The fresh certificate is valid October 23–25, 2026 UTC; both peers use the same
P256 OpenSSL configuration. No product/compiler/runtime/shared-bin build or edit,
commit, push or PR was performed by this experiment agent.

Distinct binary SHA-256 identities are recorded in full in the manifest:

- Executed private native peer: `4db6386a5a4d75b8040e23d0ff46ad6949ed02aeb3901e4b321d701103f81a17`.
- Executed private native library: `277bf7f2a3f392076a0289fe72d760a8938f7fd88b281f12799d533473a33b2f`.
- Executed canonical raw NativeAOT copy: `4243ba969a149d35f49d077a7d8112f8af76f6b761be88bfdf8e38fb979c8cb1`.

The canonical native library `0fdf728c…` was only hashed, not executed; it differs
from deleted October library `03a0c330…` and deleted peer `e121ad9f…`. The new
native binaries are not claimed identical to either. The selected managed AOT
binary matches the retained baseline, as do 111/111 source hashes, all ten raw
MsQuic generated files and five raw picotls files. The baseline closure digest
`7fccec02…` **differs** from current closure `c35a6346…`. The old 460-entry binary
table has 20 matches and 440 missing unused artifacts, principally deleted
publish/JIT intermediates; the selected executed AOT binary is present and
matches. Actual selected execution binding therefore passes the declared
binding criterion. Full current-closure qualification is not claimed. This is
prior-generation diagnostic evidence and does not requalify the deleted frozen
compiler `123b3e76e` outputs.

[Results](../artifacts/recovery-parity-investigation-20261023/experiments/results.json),
[analysis](../artifacts/recovery-parity-investigation-20261023/experiments/analysis.json),
[managed binding](../artifacts/recovery-parity-investigation-20261023/experiments/managed-binding.json)
and the [manifest](../artifacts/recovery-parity-investigation-20261023/experiments/manifest.json)
retain the full inventory and raw evidence. Two preparation anchor-guard failures
occurred before compilation or peer execution; both scripts, outcomes and traces
remain retained. The subsequent private native build and final harness build
passed. All ten strict exchange exceptions and both late classifier rejections
remain visible. Essential native/managed binaries and credentials are durable
under `experiments/`; temporary build objects remain exclusively in the declared
unique private directory on `/mnt/secondary` until evidence preservation.
Integration subsequently verified every experiment manifest entry and removed
that temporary tree; the
[cleanup record](../artifacts/recovery-parity-investigation-20261023/cleanup.json)
records the removal. Essential binaries,
sources, commands, patches, logs and receipts remain retained.
