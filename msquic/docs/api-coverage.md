# Selected API coverage audit

The [machine-readable inventory](../config/api-coverage.json) maps 31 selected
API slots, 49 selected parameter identifiers (including the priority modifier),
24 operation flags, seven credential flags and ordinary settings to authored
methods and test groups. It records exact source lines, current source hashes,
receipt hashes and differences between recorded receipt inputs and this source
snapshot. Method presence does not grant qualification or change profile policy.

The coherent [full facade receipt](../artifacts/managed-api-all/results.json) now
records **PASS for all 68 executions**: 17 modes × raw/optimized × JIT/NativeAOT
on Linux x64. Its recorded authored inputs match the current workspace, all
current facade source inputs are present, and both generated-source manifests
and hashes match. Every mode checks the actual runtime's source revision against
pin `80a065112426bce68c1da42d026478d3e40fd45e` before its control runs.

The inventory retains earlier targeted receipts with their actual hashes and
input mismatches; they are historical observations, not the basis for current
qualification. Each facade test group links its modes to the full receipt.
Separate TLS adapter and independent-peer evidence retains its own scope.

| Behavior | Evidence in the full facade matrix | Scope and limits |
| --- | --- | --- |
| Configuration and connection version-policy read/write | `VersionPolicies` exercises actual getters/setters, priority, independently owned snapshots, caller mutation, invalid-update no-mutation and lifetime rejection. | Each list is restricted to QUIC v1. The surface is implemented; the mapped controls pass all four build/runtime combinations. |
| Public callbacks, context and INLINE shutdown | `CallbackControls` exercises actual stream callbacks, INLINE abort, self-replacement, retained context, clearing observers and listener DoS transitions. | The new `ReceiveFailures` controls also pass external replacement during an in-flight receive observer and exceptions after actual lease delivery, with held-lease backpressure, exception propagation, exactly-once completion and drain. |
| Accepted-stream delayed credit and blocked start flags | `FlagControls` observes withheld server stream credit until owning close, actual credit advancement, blocked status, `ShutdownOnFail` and `IndicatePeerAccept`. | Uses actual peer-stream flags and core credit processing, not synthesized notifications. `ReceiveFailures` now passes standalone receive abort with exact peer error and an intact opposite send direction while the original receive lease stays held. |
| DATAGRAM Priority, DelaySend and ticket FINAL | `FlagControls` forwards Priority on actual datagrams, flushes a delayed stream send using another stream, and proves a final ticket resumes while subsequent issuance rejects. | Priority belongs to DATAGRAM; StreamSend rejects it as operation-inapplicable. No UDP arrival-order guarantee is inferred. |
| Invalid operation flags | `FlagControls` exercises unknown/reserved bits versus known excluded bits before dispatch. | Unknown bits produce argument errors; known excluded bits produce unsupported-feature errors. The prior classification discrepancy is resolved. |
| Owning certificate policy | `CertificatePolicies` passes 24 facade cases covering sync/async approval, rejection, exceptions, trust/name failures, pending close/cancel, late approval and owned certificate/peer-chain data. | These facade cases use ECDSA/AES-128 on IPv4/IPv6. Broader cipher/certificate combinations have separate TLS adapter evidence. Application approval cannot override provider validation failure. |
| Imported ticket-key rotation | `TicketRotation` passes 28 actual connections across two configurations, both AES suites and IP families: copied inputs, retained/deleted decrypt keys, first-key encryption, cross-configuration resumption, fallback and atomic invalid imports. | This is not a facade expiry-clock test; expiry/tamper controls remain separately attributed to the provider/TLS layer. |
| Network setters, scheduling and statistics | `NetworkParameters` proves local bind/interface/shared source port, invalid interface status, live remote-update rejection without mutation, supported client local rebinding with validated PATH_RESPONSE, both scheduler settings and real backpressure/counter evolution. | Live remote updates correctly reject under the pinned core contract. Exact application bytes and PostedBytes drain are checked; CUBIC bandwidth is integer bytes per microsecond. Individual queue averages/maxima are not each independently forced. |
| Copied handshake information | `HandshakeSnapshots` exercises both AES suites and IP families after the server raw TLS query has become unavailable, with resumption disabled. | The facade returns its immutable snapshot captured during actual CONNECTED; priority and lifetime checks still apply. |
| DATAGRAM acknowledgment after loss | `DatagramLateAck` passes both AES suites and IP families by holding one actual encrypted packet, forwarding eight later payloads, then releasing after suspected loss. | Requires actual `AcknowledgedAfterLoss`, suspected/spurious-loss counters, exact-once payload and ownership drain; no synthetic callback. |

The four latest groups close the remaining identified selected behavior gaps.
Their initial targeted receipts remain recorded; their corresponding modes now
pass in the same coherent full matrix:

| Gap | Evidence in the full facade matrix | Limit |
| --- | --- | --- |
| G01 and G04 | `ReceiveFailures`: six IPv4/IPv6 cases prove external observer replacement with retained in-flight context, cancellation after lease delivery, observer throw propagated to a pending public accept, close blocked by the delivered lease, standalone receive-only abort with exact STOP_SENDING error, surviving opposite-direction bytes/FIN, later healthy stream and owner drain. | ECDSA/AES-128 facade scope. Callback/completion counts are checked; no callback is synthesized. |
| G07 | `RegistrationShutdown`: eight role/family/disposal profiles, three connections and six blocked sends/reads each, exact peer application error `0x123456789abc`, repeated shutdown, invalid error rejection, concurrent disposal and retained-memory/drain checks. | The expected wire error is observed before racing disposal's default-zero shutdown; no ordering guarantee is invented for competing distinct error codes. |
| G05, partial resolution | `SettingsStates`: concurrent sparse configuration and live connection setters preserve both fields; compound rejection leaves prior settings intact; configured MTU bounds and started-connection MTU rejection follow the original apply path. | Dedicated settings/version-list allocation-failure injection is not claimed. The broader allocation and budget controls have separate scopes below. |
| G11 | `SettingsStates`: direct post-start datagram receive setter rejects without mutation; aggregate settings disable and restore actual capability, followed by exact datagram payload and acknowledgment. | Confirms the pinned direct-versus-aggregate state distinction rather than imposing one rule on both operations. |
| G09 | `StatelessSecrets`: four AES/family profiles verify actual emitted Retry/reset binding to copied provisioned keys, Retry key epoch rotation, recovered original CID, real traffic/reset status and counter, plus owner drain. Independent BCL authentication rejects the wrong/prior key and a changed Retry tag. | Wrong-key/tag negatives operate on captured packets; this is not a claim of a separate corrupted-token replay campaign over the network. |

The remaining `G05` and `G06` entries describe coverage breadth, not newly added
PLAN acceptance requirements. `G05` does not claim every settings allocation site
has an OOM injection control. P3's PlatformHost eleven-position allocation rollback
matrix and the facade's actual buffer-budget exhaustion controls remain separately
attributed; neither is relabeled as settings-specific failure coverage. `G06`
records that every mutable legacy/stream counter and V2 queue average/maximum is
not independently forced against an oracle. Selected query surfaces, units, actual
backpressure/counter evolution and credit advancement already have mapped controls;
priority acceptance does not promise global scheduling order.

This bounded inventory review found no further substantive selected facade behavior
gap within these mapped controls. The source-linked facade matrix is qualified;
separate public-consumer, P7 recovery/interop/fault and P9 delivery/platform gates
retain their own PLAN requirements and receipts. This result does not promote
those phases by itself.

The [first full attempt](../artifacts/managed-api-all-before-reset-race/results.json)
and its [failed optimized-AOT log](../artifacts/managed-api-all-before-reset-race/optimized-handshake-faults-aot.log)
remain preserved. The final reset test had assumed `CloseInfo` was still null
after silent server disposal. An already queued packet can reach the removed
connection and elicit the required real stateless reset before disposal returns.
`HandshakeFaults` now requires an open client before shutdown and handles either
an already-observed reset or an explicit trigger afterward. `StatelessSecrets`
also arms capture before that reset can arrive. Both still require exact status
125/error 1, the actual reset counter, and owner drain; the secret control also
requires captured packet/key binding. The old/new receipt input comparison shows
only these two test files changed, with no product-source change. The corrected
complete matrix then passed; the original assertion failure is not erased.

Resolved gaps retain their groups and scope in `resolved_gaps`. The JSON records
current source lines/hashes, full-receipt qualification, and historical missing
or mismatching inputs separately. Optional allocation-injection and exhaustive
counter breadth remain explicitly unclaimed rather than being mistaken for full
native-suite parity.
