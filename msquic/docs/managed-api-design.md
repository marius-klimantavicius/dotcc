# Owning managed API contract

The API is implemented, with optimized JIT configuration, stream lifetime and
resumption controls passing. This document describes its contract; individual
receipts and the phase ledger define the qualified scope. The selected contract remains [`api-profile.json`](../config/api-profile.json);
its pending requirements must not disappear merely because a first sample does
not exercise them. P8 requires separate consumers without internal APIs, and P9
requires the final raw/optimized, JIT/NativeAOT and shared regression campaigns.

## Public types and ownership

Use `Managed.Transport.Api` for the facade, keeping generated C types in
`Managed.Transport` and host services in `Managed.Transport.Hosting`. Public
signatures expose managed values, not `QUIC_HANDLE*`, API table pointers, native
credential structures, generated anonymous types or arbitrary parameter blobs.

| Type | Public operations | Owns or retains |
| --- | --- | --- |
| `QuicRuntime : IAsyncDisposable` | `CreateAsync`, `OpenRegistrationAsync`, immutable `Capabilities` and typed global queries/settings | One installed `MsQuicHost`, one `MsQuicOpenVersion(2)` result, all registrations and a cleanup executor |
| `QuicRegistration : IAsyncDisposable` | `CreateConfigurationAsync`, `ListenAsync`, `ConnectAsync`, `ShutdownAsync` | Registration handle; configuration, listener and connection children |
| `QuicConfiguration : IAsyncDisposable` | Immutable ALPN/settings/credential snapshots; typed permitted updates; ticket-key replacement | Configuration handle and credential registration; leases held by listeners, connecting and accepted connections |
| `QuicListener : IAsyncDisposable` | `AcceptConnectionAsync`, `StopAsync`, actual `LocalEndPoint`, statistics | Listener handle, configuration lease, bounded pending-accept admissions |
| `QuicConnection : IAsyncDisposable` | Open/accept bidirectional or unidirectional streams, datagrams, ticket operations, typed parameters, `ShutdownAsync`, negotiated metadata | Connection handle, parent/configuration leases, stream children, pending validation and callback contexts |
| `QuicStream : IAsyncDisposable` | `ReadAsync`, `ReceiveAsync`, `SendAsync`, `CompleteWritesAsync`, `AbortRead`, `AbortWrite`, statistics | Stream handle, connection lease, send operations and at most one deferred receive offer |
| `QuicReceiveLease : IDisposable` | `AbsoluteOffset`, `Buffers`, `HasFin`, `Complete(consumed, resume)`, `Dispose` | A single receive offer and its stream lease; managed payload copies plus deferred core ownership until completion |
| `QuicClientCredentials`, `QuicServerCredentials` | Owned trust/identity construction and selected validation options | Copied trust/chain data and retained private-key owner; no insecure validation bypass |
| `QuicTransportException`, `QuicCloseInfo` | Actual status, transport/application error, TLS alert and initiator | Immutable copied diagnostics |

Configuration dependencies are leases, not an assumed strict tree: one
configuration may serve several listeners/connections. A configuration's close
request prevents new leases and waits for existing leases. Registration shutdown
first closes its users of configurations, then releases the configurations.
Stopping a listener stops admission; accepted connections remain registration
children and are not silently destroyed by listener disposal.

The initial implementation should support one runtime per generated-library
instance, matching `MsQuicHost.Install`. A second active runtime fails before
opening upstream state. Do not imply isolation between registrations when they
share generated global state. Runtime construction/teardown is serialized.

## Validation before dispatch

Every entry point builds and validates a complete temporary request before
calling the generated table. Validation also applies to global, configuration
and connection settings updates, not only construction.

1. Validate object ownership, non-closing state and required parent leases.
   Identify the allowed operation scope and getter/setter direction. Unknown
   identifiers/reserved bits/sizes are invalid; known excluded options are
   unsupported. No raw `SetParam` public escape hatch is provided.
2. Validate the entire settings mask and reserved fields against the checked
   profile, including disabled-only fields and enum restrictions. Then preserve
   stricter upstream range, versioned-size and current-state validation. A
   profile failure must cause no partial upstream mutation.
3. Initialize effective settings explicitly: CUBIC, no ECN/offload/preview
   options, no early data and v1-only acceptable/offered/fully-deployed version
   lists. Do this before the first binding/connection can observe defaults.
   The global compiled-version query is not the qualified capability list.
4. Snapshot ALPN values as immutable byte strings of length 1–255, reject NUL
   for the selected picotls adapter and validate aggregate bounds. Validate SNI,
   endpoint family, port, IPv6 scope, interface index and applicable bind state.
   Retain or copy strings through the actual C ownership transfer; never keep a
   pointer to a temporary `fixed` span after its scope ends.
5. Check operation-specific flag masks and combinations. A send flag admitted
   for streams is not automatically admitted for QUIC DATAGRAM. Unidirectional
   streams cannot use the disallowed direction. Enforce negotiated datagram
   support and the current payload limit before admission.
6. Map owned credentials through the real host helpers
   `CreateClientCredential`, `CreateServerCredential` and `LoadCredential`.
   The managed type/provider extension `0x10000` remains internal. Native
   credential handles, validation bypasses, unsupported algorithms and excluded
   flags are rejected before configuration mutation.

The host provides the tested overload
`LoadCredential(configuration, credential, additionalFlags, asyncHandler)` with
the exact `delegate*<QUIC_HANDLE*, void*, uint, void>` completion signature. Its
asynchronous mode can invoke completion inline and then return `PENDING`.
Install the configuration's pending operation/token **before** dispatch and
settle it once from callback plus return status; a `PENDING` return must not reset
a state already completed inline. Do not force a callback to be deferred merely
to simplify the facade. Indication requires the client/portable combination;
the adapter supplies a DER leaf and PKCS7 chain and preserves actual trust/name
failure under deferred application approval. The direct TLS adapter matrix covers these seams under raw/optimized JIT and
NativeAOT; facade and separate-consumer qualification remains distinct. A checked-in profile-to-validator
coverage test must prevent missing entries and accidental future-bit acceptance.

## Handle and callback states

Transitions below are facade states; core transport state remains authoritative.
Each raw call acquires a short operation lease under the owner gate. Close stops
new leases first. Never hold that gate while calling C: calls may invoke a
callback inline or wait for a worker which needs the gate.

| Owner | Normal transitions | Failure and close obligations |
| --- | --- | --- |
| Runtime | `Creating → Open → Closing → Closed` | Reverse each completed startup step on failure. `Closed` requires all children, API close, host resource drain and successful uninstall. |
| Registration | `Opening → Open → Closing → Closed` | Stop child admission, shutdown connections, close children on the independent executor, then `RegistrationClose`. |
| Configuration | `Opening → LoadingCredential → Ready → Closing → Closed` | No listener/connect dispatch before real load completion. Failure closes the C handle and credential lease; asynchronous completion retains input/context until fired or canceled and drained. |
| Listener | `Opening → Starting → Listening → Stopping → Stopped → Closing → Closed` | `STOP_COMPLETE` settles `StopAsync`; raw `ListenerClose` and callback drain settle disposal. Pending accepts complete with the actual stop/error. Restart is allowed after actual STOP_COMPLETE and is covered by the owning transport controls. |
| Connection | `Opening/Accepting → Handshaking → Connected → ShutdownRequested → ShutdownComplete → Closing → Closed` | A failed handshake retains its actual transport/TLS status. A shutdown event may arrive without `Connected`. Rooting lasts through raw close and active callback drain, not merely task cancellation. |
| Stream | `Opening → Starting → Active → ShutdownRequested → ShutdownComplete → Closing → Closed` | Track read and write directions separately: FIN on one does not close the other. Close waits for application receive leases and send/callback ownership before retiring the handle. |

Use exact static generated callback signatures compatible with NativeAOT.
Opaque facade context tokens map to strong owner references in a checked registry;
they are not movable object addresses. Keep this registry separate from the
host's resource registry, which owns PAL/TLS/datapath service objects. Maintain
callback entry/exit counts and a retired state. A late/wrong token is a contract
failure, not an empty callback that reports success.

Callbacks update owner state and complete task sources created with
`RunContinuationsAsynchronously`. They must not run arbitrary user continuations
under a core worker, owner gate or host queue lock. Copy callback-scoped event
data before asynchronous publication. User callback replacement, if exposed,
updates the managed association behind the stable C trampoline; replacement
must retain the old association through callbacks already in progress.

The listener's NEW_CONNECTION path reserves a bounded accept slot and roots the
new connection before installing its callback and applying configuration. It
cannot await user acceptance on the core worker. Failure returns the appropriate
rejection and unwinds exactly the ownership the upstream callback contract gave
the application. P6 must pin the rejection/handle-transfer boundary. Accept tasks
return a connected connection; incomplete handshakes still consume backlog slots.

## Send memory and cancellation

`SendAsync(ReadOnlyMemory<byte>, options, cancellation)` uses a bounded facade
send budget and copies admitted bytes into facade-owned pinned storage. The
descriptor array is retained with that storage. Copying is the initial public
contract; callers do not have to keep arbitrary external memory pinned after
their operation is canceled. A future explicit ownership-transfer send buffer
can add a qualified copy-avoidance path without weakening this contract.

An operation moves through
`WaitingForBudget → Prepared → Submitting → InFlight → Completed → Released`.
Reserve its token and install the pending operation before calling `StreamSend`,
because completion may race the return. A synchronous failed status releases the
operation if C did not accept ownership. After successful/pending admission,
`SEND_COMPLETE` owns final release, even when `Canceled` is true, the application
task was canceled or shutdown was requested. A per-operation atomic completion
claim prevents the synchronous-failure path and callback from both releasing.

Cancellation before admission prevents submission and releases the budget. The
proposed default after admission is to cancel the caller's wait only: it does
not promise to retract bytes already handed to QUIC. The internal operation and
copied memory remain until completion. `AbortWrite(errorCode)` explicitly aborts
the write direction and can affect all queued writes. Do not simulate per-send
wire cancellation, which the selected upstream stream API does not provide.
This public cancellation choice requires coordinator acceptance before coding.

`CompleteWritesAsync` submits/requests FIN in the upstream-supported way and
distinguishes send-buffer release from peer acknowledgment/write-shutdown
completion. A SEND_COMPLETE callback alone must not be described as delivery
acknowledgment. QUIC DATAGRAM tracks its own send-state callbacks and terminal
cancellation/loss/acknowledgment states; UDP host send completion is a different
internal lifetime and cannot release an application datagram operation by itself.

## Deferred receive and partial consumption

The initial profile disables multi-receive and application-provided receive
buffers. Permit one active read operation/receive lease per stream. A copying
`ReadAsync(Memory<byte>)` and a deferred `ReceiveAsync` share this state machine;
mixing them concurrently is rejected before altering C receive state.

On RECEIVE, validate offset, lengths and flags and copy the payload into
managed-owned buffers immediately, charging both the native offer and the managed
copy against bounded admission. Read the `QUIC_BUFFER` descriptors only during
the callback. The descriptor array itself is callback-scoped:
`stream_recv.c` uses stack or temporary descriptor storage and frees it after
dispatch. Retain the core offer through pending completion and a live stream
handle, without exposing its payload pointers to application code. Never retain `event*`, `event.RECEIVE.Buffers` or a copied property value
that merely contains the expired descriptor-array pointer.

For deferred delivery, publish an owned `QuicReceiveLease`, return
`QUIC_STATUS_PENDING` and leave its entire offered length pending. The lease
moves through `Offered → Pending → Completing → Released`; only one completion
claim is permitted. Publication and completion can race callback return; the
core has an explicit active-receive completion flag for this case. P6 must test
the inline/concurrent completion path, not rely on a scheduling delay.

`Complete(consumed, resume)` accepts `0 ≤ consumed ≤ OfferedLength`, revokes the
**whole offer**, relinquishes native receive ownership and calls
`StreamReceiveComplete(consumed)` exactly once. In the selected single-receive
mode a partial completion clears the core's outstanding offer and leaves
receives paused. The suffix must be offered again after
`StreamReceiveSetEnabled(true)`; it is not a second independently completable
slice of the old lease. Completion and resume calls are serialized per stream.
The old lease cannot complete again after any completion, including completion
of zero bytes. Previously acquired managed buffer views remain safe to read.
The next offer's offset and bytes must equal the unconsumed suffix.

`Dispose` on an uncompleted lease means `Complete(0, resume:false)`: it releases
the borrow without falsely acknowledging bytes. `ReadAsync` resumes when a
consumer is ready, copies at most the destination capacity, and completes that
prefix. Its cancellation cannot return while a callback is still writing to the
caller's destination; gate ownership resolves that race before settling the
task. No receive callback reports an arbitrary failure status as backpressure:
the pinned core treats non-PENDING/non-CONTINUE returns as successful delivery.

A FIN-only receive may have zero descriptors and zero bytes. Handle it without
inventing a nonempty buffer. `HasFin` on an offered tail is an observation; EOF
is settled when all preceding bytes are consumed and the core indicates
PEER_SEND_SHUTDOWN. If a partial offer contains FIN, the unconsumed suffix still
belongs to the receive path and EOF cannot be published early.

Shutdown stops new receive publication but cannot free storage still borrowed
by application leases. Retain the stream handle until every lease is returned;
then close it. After a reset/abort, completing a lease releases the borrow and
must not claim new flow-control credit if the core ignores that completion.
Payload validity across remote reset and connection shutdown while the app
handle remains open is a required P6 test. Public leases always use managed
copies with internal deferred ownership: `ReadOnlyMemory` cannot revoke a span
that a caller already obtained. Retaining an old view after completion or raw
stream close must remain memory-safe and is a required ownership test. The
runtime stops accounting a completed copy as an outstanding receive; retained
managed views then have ordinary application-owned managed-memory lifetime.

## Deferred validation and tickets

Credential/certificate/ticket operations get their own once-only pending token
and a connection/configuration lease. Copy certificate chains, verification
results and application ticket bytes before leaving the callback. The portable
certificate representation must have owned DER data; it must not reinterpret a
native certificate context. Trust and hostname checks remain mandatory even if
an application policy callback runs asynchronously.

The current portable indication seam exposes a DER leaf and PKCS7 chain. Copy
these during the callback into the facade's owned certificate request rather
than exposing provider allocation addresses. Await its qualification before
advertising the selected credential flags.

For certificate validation, the real validator determines acceptance and the TLS
alert. `ConnectionCertificateValidationComplete` is dispatched only for that
pending token, once, while the connection permits it. Application cancellation
rejects/abandons the pending validation according to actual shutdown state; it
never converts failure into acceptance. Ticket validation similarly completes
the core's actual pending RESUMED event through
`ConnectionResumptionTicketValidationComplete`. Copy its application bytes:
the TLS ticket-decryption plaintext is zeroed/freed after the callback. Rejection
must follow the core's fallback/error path, with no early-data acceptance.

Ticket receipt and provisioning retain the upstream envelope, selected cipher,
ALPN/SNI/trust policy and key-rotation ownership. Fresh 1-RTT resumption must be
qualified separately from the full-handshake stream sample. A ticket is secret
material: no diagnostic output includes its contents or imported key bytes.

## Disposal, errors and failure cleanup

`DisposeAsync` is idempotent and returns the same underlying close task to all
callers. It first closes admission, then schedules the blocking work on an
executor independent of MsQuic workers. The proposed executor is one dedicated
BCL thread per runtime for blocking close calls; normal I/O and core scheduling
remain unchanged. Each executor item performs one already-eligible raw close;
the asynchronous orchestration waits for child/lease prerequisites outside that
thread. It must not queue a parent operation that blocks waiting for child close
work behind it on the same executor, or hold a parent lock while awaiting a child.

Shutdown order is: stop listeners/admission; request connection shutdown;
resolve/cancel pending application work; wait for borrowed receives and terminal
send/callback ownership; close streams; close connections; close listeners;
release configuration/credential leases; close configurations; close
registrations; close the API table; verify host allocations/resources drain;
uninstall and dispose the host. Configuration/registration close cannot run ahead
of a connection that still uses them. Host disposal currently checks for live
resources rather than draining them; the facade supplies this ordering.

Calling `DisposeAsync` from a callback schedules work and returns without waiting.
Never expose a convenience synchronous wait on that worker. A canceled or timed
out caller waiting for disposal does not cancel the underlying cleanup or free
contexts. A receive lease retained forever can keep close pending; provide
diagnostics identifying the owner/operation without reclaiming unsafe storage.
Finalizers do not block or invoke raw close and must not drop the last roots of
live callbacks. Explicit asynchronous disposal is the required contract.

Callback trampolines catch exceptions, record a single failure and schedule the
appropriate abort. For RECEIVE, preserve correct pending/completion semantics
even when allocation/publication fails; returning a generic error is insufficient.
For void APIs, use managed prevalidation and the documented exception boundary;
do not invent a C status or return successful no-ops. Diagnostics preserve actual
upstream status, TLS alert/error and cancellation origin. Handle construction
failure unwinds completed acquisitions in reverse order, including credentials,
callback token, parent admission slot and any accepted raw handle.

## Implementation gates and open decisions

Before P8 implementation, P6 must pin callback/close transfer on listener
rejection, pending receive storage across reset/shutdown, partial-prefix
redelivery, completion racing callback return, and accepted/canceled sends during
close. Then add isolated public-consumer tests for every state transition and
allocation failure boundary, raw/optimized under JIT/NativeAOT. Profile tests
cover all selected/excluded identifiers and mixed valid/invalid settings payloads.
No internal-friend test application substitutes for the P8 consumer gate.

Decisions sent to the coordinator: accept copied-send/cancel-wait semantics;
accept one-shot listeners or implement upstream restart explicitly; choose the
public pending-validation policy surface while preserving mandatory trust/name
checks; and set bounded accept/send/receive budgets based on measurements rather
than an undocumented unbounded queue. None is an authorization to drop selected
profile operations.

Source anchors: [receive return/temporary descriptors](../ref/msquic-80a065112426bce68c1da42d026478d3e40fd45e/src/core/stream_recv.c#L1000),
[single-receive partial completion](../ref/msquic-80a065112426bce68c1da42d026478d3e40fd45e/src/core/stream_recv.c#L1133),
[concurrent receive completion](../ref/msquic-80a065112426bce68c1da42d026478d3e40fd45e/src/core/api.c#L1432),
[pending ticket validation](../ref/msquic-80a065112426bce68c1da42d026478d3e40fd45e/src/core/connection.c#L2225),
[host lifetime contract](worker-lifetime.md), and [current host disposal](../src/BclHost/MsQuicHost.cs#L55).

## Synchronous stream observers

`ApplicationContext` associates an arbitrary managed object with an owner under
its lifetime checks; the native callback token remains the stable rooted owner.
`QuicStream.SetCallbackHandler` atomically replaces a synchronous typed observer
and its context. Each invocation holds its delegate/context snapshot until it
returns, including when the handler replaces itself. Clearing the observer keeps
the owning trampoline and asynchronous stream operations active.

Observers receive copied event values after internal owner state updates. They
may request `Inline` stream shutdown while that actual callback is executing.
They must not block on QUIC work or disposal; ordinary asynchronous continuations
remain outside this callback context. Exceptions fault the connection without
escaping into C or changing an outstanding receive's `PENDING` ownership result.
These new observer controls are authored and await runtime qualification.
