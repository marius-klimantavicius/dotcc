This finite suite exercises owning socket, netconn and resolver APIs from the
ordinary `Managed.Lwip` project reference. `LwipProduct.props` imports capabilities
from the actual selected generated project; the runner deliberately omits a
`LwipProfile` hint. NO_SYS builds expose resolver/address services without
references to socket/netconn symbols that the original profile does not emit.
The finite promotion passed raw/processed JIT/NativeAOT in all applicable product
profiles; no broader API gate is closed. Verification-policy metadata records
exact receipt boundaries, including later scheduling admission qualification.

Socket/netconn async scheduling retains the owner before queueing and registers
the returned Task in disposal accounting. A controlled scheduler holds a real
public receive call before original admission; owner disposal must wait until
that Task settles, while an independent owner continues.

Socket operations hold per-call owner leases, bind on the calling thread,
initialize/clean up the original per-thread semaphore, and copy user buffers.
Original `get_socket`/`done_socket` references pin descriptor lifetime across
close and prevent an admitted call from targeting a reused descriptor. The
include-wrapper exposes these private references without changing algorithms.

Each socket/netconn permits one reader or acceptor and one writer or connector,
concurrently with close, matching upstream FULLDUPLEX's contract. Bind/listen
require exclusive directional admission. The original u8 socket pin counter
also has a defensive bound; this is not a promise of arbitrary same-direction
concurrency. Cancellation closes the entire affected handle, including another
operation on that handle, and is not per-operation protocol cancellation.

Registries close all children before joining admitted calls. Accepted children
are registered before possible rejection; failed release retains their backing
for disposal retry. Netconns use original `netconn_prepare_delete`, wait for
callers, then `netconn_delete`. Socket timeout accounting is monotonic.

Address DTOs copy IP bytes and numeric owner-relative lwIP interface indices
(0..255). They never map OS interface names or scope indices. Resolver calls
normalize IDNA names and use original lwIP DNS, with copied A/AAAA/cache/NXDOMAIN
results. Canceling a DNS Task retains its still-callable original callback root;
the real callback or the owning stack's post-core-join phase releases it.
Tests cover pending queries across GC, failed disposal/retry and two owners.

Ordinary Ethernet interfaces now stop and drain on the core while backing stays
alive through real queued FIFO barriers, then free. Native ASan controls compare
premature original removal against the exact product stop/barrier/free boundary,
using a coherent IPv6 packet to populate the pending loopback queue. No malformed
placeholder packet or custom corruption probe is used.

Native socket-reference and netconn controls preserve ordinary descriptor reuse,
blocked receive wakeup and natural pool accounting. Managed qualification reports
and the verification-policy inventory record which source snapshots have run.

The local-project integration control selects NO_SYS using a relative property
inside its project, forwards normalized facade-reference metadata, and verifies
both the actual product constant and absence of socket types in the facade.
An unrelated two-hop project reference remains usable. The import uses static
graph restore and removes only the duplicate selected-product transitive edge.
Archived prototypes live outside compilation under
`artifacts/owned-api-prototype-history-20260929/manifest.json`.
