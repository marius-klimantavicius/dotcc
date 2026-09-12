# BCL UDP datapath host

`src/BclHost/MsQuicHost.Datapath*.cs` implements the UDP slots in the frozen
`MSQUIC_HOST_TABLE`. It consumes the generated C types and calls the unchanged
translated worker pool. It contains no native imports and does not replace the
transport core with `System.Net.Quic`.

The first implementation matrix passed against raw and optimized generated
libraries under JIT and Linux x64 NativeAOT. Its preserved receipts are in
`artifacts/datapath-host-initial/`. The expanded delayed-send, callback-local
close, listener flags and explicit interface tests also passed all four runtime
variants. Their current source-bound receipt is
`artifacts/datapath-host/results.json`.

## Ownership and completion

A datapath holds an upstream worker-pool reference until all sockets, borrowed
receive descriptors and pooled slabs drain. Opaque datapath, socket and send
handles use the host's typed, monotonic token registry. No managed object address
is stored in C memory.

Each receive slab comes from the platform's pinned, aligned allocation domain.
It contains the exact `CXPLAT_RECV_DATA`, the caller-requested client context
immediately after that descriptor, an aligned `CXPLAT_ROUTE`, and a 1500-byte
payload region. At most 64 returned slabs remain in the reuse pool. The next
rental clears the descriptor and client context. A pending socket operation owns
its slab until completion; a delivered packet remains rooted by its actual C
address until `CxPlatRecvDataReturn` returns it. Socket deletion can finish while
core still owns a delivered packet; datapath destruction waits for those returns.

Each physical socket has one receive pump. The pump uses BCL asynchronous
`ReceiveMessageFromAsync` over retained memory and waits for that operation on
its own dedicated thread. Socket disposal cancels pending I/O, and deletion joins
all receive pumps before reclaiming internal state. Datagram payloads enter a
bounded queue of 256 items. Queue pressure drops complete datagrams with an error
counter; it never combines packets or hands a truncated payload to the core.

A registered worker SQE announces readiness. Its coalesced notification drains
individual queued packets and unreachable reports on the original translated
worker. No queue or resource lock spans the translated callback. Receive
callbacks may immediately return their buffers or retain them for later return.

A send owns one pinned payload and an actual `QUIC_BUFFER`. Allocation reports
full after one buffer because segmentation is not advertised. `SocketSend`
transfers ownership to a real BCL `SendToAsync` operation. Both immediate and
deferred completions release the send token and buffer exactly once. Socket
close disposes the physical sockets and waits for admitted send operations to
complete. UDP has no send-completion slot in the upstream UDP callback table:
ordinary send failures are counted and their buffers are released; appropriate
network-unreachable failures additionally notify the upstream unreachable handler.

The deterministic pending-send test submits the real BCL send and then gates
completion delivery with a test-only partial method. It proves retained ownership,
forced GC and close waiting on the deferred path. That partial method has no
implementation in production and is omitted by the C# compiler. This test does
not claim that the kernel send queue naturally filled on loopback.

## Close from a worker batch

A socket is first marked closing, which prevents further receive upcalls and
send admission. Its pumps and admitted sends drain before normal deletion
returns. Pending packet descriptors are returned without an upcall.

A different callback on the same worker may delete a socket whose SQE appears
later in the current batch. In that case queue cleanup closes the registration
immediately, but retains the internal C notification and socket token through
`CxPlatEventQReturn`. A later notification in that batch observes closing and
returns without reading the borrowed callback context. Batch return then reclaims
the notification and token outside the queue lock. Datapath shutdown waits for
this internal reclamation too.

Synchronous deletion from the socket's own receive/unreachable callback is a
contract violation. It fails at the host invariant boundary instead of joining
itself or reporting a false drain. The owning facade must defer that close to an
independent owner, as required by the worker lifetime contract.

## Address and capability profile

IPv4 and IPv6 addresses use the measured POSIX `QUIC_ADDR` layout, network-order
ports and real IPv6 scope IDs. IPv4-mapped IPv6 addresses normalize to IPv4.
Receive packet information supplies the actual local destination and interface;
the interface is retained in `LocalAddress.Ipv6.sin6_scope_id` for both families,
matching the native datapath convention. Link-local scope is preserved for BCL
endpoints; non-link-local interface metadata is not mistaken for an endpoint
scope.

For a wildcard listener, replies use an explicitly bound socket for the received
local destination at the listener's existing port. These sockets share the
wildcard binding through BCL `ReuseAddress`; each has its own receive pump so
packets selected by the kernel for the specific binding remain covered. The
qualified cases include 127.0.0.1 and 127.0.0.2, a dual-stack listener receiving
IPv4 and IPv6, and an actual configured local link-local IPv6 address. New remote
ports are taken from each receive route, covering server-side NAT rebinding.
This is local address and lifetime qualification, not remote multi-interface
migration or a claim of exclusive port ownership against other processes.

The socket accepts the actual `SHARE | SERVER_OWNED` flags that upstream
`ListenerStart` always supplies. Binding-object sharing remains in the core.
The typed datapath fixture uses that exact combination; the managed peer also
runs an actual `ListenerOpen/ListenerStart/GetParam/ListenerClose` preflight.

Explicit interface selection uses BCL `Socket.SetRawSocketOption` before binding
or connecting every physical socket, including source-specific sockets created
later. This BCL method accepts OS option identifiers absent from
`SocketOptionName`; it requires no authored native imports. The selected Linux
profile uses `IP_UNICAST_IF=50` at level 0 and `IPV6_UNICAST_IF=76` at level 41.
Both Linux options consume a 32-bit **network-order** index, including IPv6.
Dual-mode sockets receive both options. The kernel validates interface existence
and chooses the outgoing interface; the host does not substitute a guessed local
address for interface routing. Unsupported platforms return `NOT_SUPPORTED`.
The passing tests read the actual kernel option back and exercise IPv4, mapped
IPv4 on dual-mode sockets, IPv6 loopback, scoped IPv6 and invalid indices.
Broader remote routing and changing-interface qualification remain pending.
Sources: [BCL API contract](https://source.dot.net/System.Net.Sockets/System/Net/Sockets/Socket.cs.html),
[Linux IPv4 option implementation](https://github.com/torvalds/linux/blob/v6.12/net/ipv4/ip_sockglue.c),
[Linux IPv6 option implementation](https://github.com/torvalds/linux/blob/v6.12/net/ipv6/ipv6_sockglue.c).

BCL DNS and connected-UDP local-route lookup preserve the requested family/port
and do not expose the lookup socket's ephemeral port. Interface enumeration uses
BCL network-interface data and returns memory in the expected MsQuic allocation
domain.

The advertised feature mask is zero: no RSS, coalescing, segmentation, port
reservation/sharing feature, TCP, raw/XDP, TTL, DSCP, ECN, QTIP or encryption
offload capability is claimed. The supported MTU ceiling is the upstream 1500
bytes; send allocations are bounded to its maximum IPv4 UDP payload of 1472.
Truncated incoming datagrams are dropped and counted. Closed connected UDP peers
produce the typed unreachable callback.

Optional raw/TCP/DSCP initialization, PCP, partitioned socket flags, CIBIR and QTIP requests return
`QUIC_STATUS_NOT_SUPPORTED`; ECN/DSCP send allocation requests return null through
the allocator's failure contract. Nonzero busy-polling requests are outside the
selected profile and must be rejected by the facade before reaching its void
host slot. No success fallback fabricates those facilities.

## Validation

Run `python3 msquic/scripts/test-datapath-host.py`. The driver checks source and
closure hashes, then builds and executes raw and optimized variants under JIT and
NativeAOT. Its isolated service bootstrap installs the exact 99 typed callbacks;
unused subsystems fail fast if accidentally invoked. Required tests cover:

- Mapped-address conversion, local-route lookup, DNS family/port preservation,
  real interface metadata and allocator-domain return.
- Exact 0/1/1200/1472-byte datagrams, wildcard destination selection, same-port
  replies, dual-stack normalization, configured IPv6 scope and remote-port changes.
- Client context placement and clearing on reuse, buffer retention through forced
  GC, truncation followed by an intact datagram, and unreachable notification.
- Repeated socket close with pending receive I/O, retained deferred sends, and
  no receive upcall after deletion.
- A control SQE closing a socket whose notification is proven to be later in the
  same batch, plus a child-process negative test for callback-local deletion.
- Zero outstanding host resources, platform allocations and receive leases after
  full datapath and translated-worker teardown.

A machine without a configured link-local IPv6 address fails the required scope
case instead of silently treating it as covered. General external network
migration, sustained saturation throughput and physical-interface PMTU discovery
remain transport-integration qualification work.
