# Managed host boundary

This is the implementation contract selected during P1. The host header overlay,
typed callback adapter and staging recipe are implemented. Binding, TLS layout,
and the complete selected core ABI harness pass native/JIT/NativeAOT controls.
Reusable raw/optimized libraries and their rooted JIT/NativeAOT consumers also
pass. Host runtime services remain under implementation.

The core will call a statically registered, versioned C callback table. A minimal
C adapter provides the original `CxPlat*` entry points and forwards host-owned
operations to that table. dotcc's existing `-l` mode dynamically loads native
symbols and is unsuitable here. Authored C# installs managed `delegate*` callbacks
whose signatures use the generated C types, before invoking upstream library
initialization. It must never reinterpret managed and unmanaged function-pointer
calling conventions.

`MsQuicHostInstall` validates table size/version and every required slot,
copies the table into stable translated storage, and rejects replacement while the
library is active. There is one host instance per generated library instance;
registration and teardown serialize with upstream initialization. The adapter
marks the table active before the `CxPlatSystemLoad` callback and keeps it active
until `CxPlatSystemUnload` completes. All core work and I/O must be drained before
unload; individual service calls do not acquire a table lifetime lease. Table context
tokens identify a strongly rooted, checked registry. Monotonically unique tokens
avoid interpreting stale values as recycled handles; managed object references
are never embedded in C memory. Missing operations fail binding;
there is no success-returning fallback table for runtime validation.

## Source and ABI selection

The first managed ABI is LP64/little-endian/Linux x64. Public `QUIC_BUFFER`,
`QUIC_ADDR`, API callback tables and protocol headers retain upstream definitions.
Native/translated probes measure their actual layouts. Opaque PAL locks, events,
threads and completion queues use a separately documented managed host ABI;
they are not native pthread/epoll handles. The same host header must compile in
the native ABI harness and dotcc to validate that boundary.

Product staging preserves source bytes and replaces only explicit host headers
through a recorded overlay. In particular, the `quic_platform_posix.h` selection
must resolve to the managed header, rather than compiling POSIX event-loop inline
implementations. A staged source/header tree is needed because quoted upstream
includes otherwise find their neighboring original header before `-I` overlays.
The staging manifest records every replaced header and verifies every unchanged
file against the pinned input. Diagnostic `probe-source` transformations are
excluded from this recipe.

The staged closure contains all 39 core files plus `crypt.c`, `hashtable.c`,
`platform_worker.c` and `toeplitz.c`. Retaining the BBR implementation as a compiled
core candidate does not expose BBR as a supported congestion setting. The separate
PCP client is a survey input; it is not required for QUIC NAT rebinding and is
omitted from staging. The native symbol audit finds no missing PCP references.
Four adapter units add typed forwarding, table lifecycle, and unchanged extracted
upstream reference/rundown and route-copy implementations. The 47-unit native
closure compiles with only the expected libc/compiler helper symbols unresolved.
All 47 product objects also emit and link with the core ABI harness, and that
executable builds and runs under JIT and NativeAOT. Reusable managed-library
packaging also passes for raw and optimized variants, with the complete generated
assembly rooted in each AOT consumer. `config/product-closure.json` freezes the
validated source, table, compiler and output hashes.

`scripts/stage-product.py` verifies the complete source archive before staging.
It records file hashes and three explicit replacements in
`config/managed-host/overlay.json`: the PAL header, private platform include
aggregation, and the event-queue tail of `msquic_posix.h`. The latter makes public
event-queue types opaque aliases of the managed host types and removes native
epoll includes. Public address/status definitions are unchanged. The system
header additions supply an opaque `addrinfo` alias and IP address declarations;
they provide no native DNS/socket/event-loop implementations. The reference and
rundown fragment and route-copy fragment are checked byte-for-byte against their
pinned upstream bodies.

`config/managed-host/operations.json` inventories 99 provisional typed slots
derived from the actual native closure and portable inline helper dependencies.
`scripts/generate-host-contract.py` generates the C header, required-slot checks,
and forwarding bodies. Native compilation checks those definitions against the
upstream declarations. There are no default service callbacks. Product host
implementations must supply an explicit rejection for unsupported optional
operations, including offload, RSS and persistent storage, and implement every
required operation before runtime phase gates can pass.

The measured table size is 824 bytes with 8-byte alignment on the first runner.
Lock, RW-lock, event and event-queue structures each hold one 8-byte token;
`CXPLAT_THREAD` is an 8-byte scalar token because upstream tests and clears it as a
scalar. `CXPLAT_SQE` contains a token and a typed completion callback (16 bytes);
`CXPLAT_CQE` contains the originating SQE pointer (8 bytes). Rundown storage retains
the upstream event-plus-reference layout against these host definitions.

The host lock callback must provide recursive locking, matching the replaced
POSIX initializer. Both allocation callbacks must return at least 16-byte-aligned
storage: the retained pool header is 16 bytes and the returned payload follows
that header. `CxPlatAlloc` supplies zeroed bytes; `CxPlatAllocUninitialized` does
not promise zeroing. Pool bookkeeping and reference/rundown transitions remain
upstream C. Queue dequeue must return at most the supplied capacity; an error
cannot be represented by casting a negative native-style result to `uint32_t`.
SQE cleanup must account for all queued and in-flight completions before releasing
its context. These semantic requirements still need P3 runtime tests.

## Operation ownership

| Boundary | Retained translated behavior | Managed service |
| --- | --- | --- |
| Memory | Core allocation policy, pool/list bookkeeping and tags | Aligned allocation/free in the MsQuic allocator domain; failure returns propagate |
| Synchronization | Reference/rundown state transitions and lock ordering | Lock/event/thread objects, wakeups, waits and stable context ownership |
| Workers | `platform_worker.c` execution-context selection, queues, deadlines and core timer policy | Threads and completion queue primitives; no competing managed scheduler |
| Time | Upstream deadline calculations | Monotonic microseconds and explicit timeout conversion; processor/thread identifiers |
| UDP | Core binding/route/path validation, packets and retransmissions | BCL sockets, receive/send buffers, destination/interface metadata and completion drains |
| TLS | Existing translated picotls state machine plus MsQuic CRYPTO-stream policy | `CxPlatTls*` epoch/result/ownership adapter, credentials and BCL crypto callbacks |
| Packet protection | Translated `crypt.c` derivation/key transition policy | BCL hash/AEAD/AES header protection and entropy primitives |
| Configuration | Core parameter validation and settings logic | Explicit managed settings; persistent platform storage is disabled rather than fabricated |

Optional RSS, segmentation/coalescing, offloads, XDP, io_uring and ECN capabilities
are not advertised. The measured BCL receive result exposes destination/interface
metadata but no ECN value. Per-address send socket selection, wildcard listeners
and same-port binding need integration qualification before a source-selection
claim; see [UDP feasibility](datapath-feasibility.md).

The provider retains every pending socket buffer/context until its completion
has drained. Managed exceptions become explicit statuses before control returns
to translated callbacks. C callback reentrancy is allowed; disposal cannot hold a
worker thread waiting for work that requires that same thread. Global table and
context roots outlive all upstream work and pending I/O.

The TLS bridge keeps the two generated allocator domains separate. It copies
picotls handshake output/transport parameters into MsQuic-owned buffers when
ownership crosses the boundary, and frees each source buffer in its origin
library. The passing [raw TLS spike](tls-feasibility.md) demonstrates epochs,
parameters and secrets; it does not yet implement the `CxPlatTlsProcessData`
consumed-length/result-flags contract, QUIC ticket envelope or shutdown ownership.

## Remaining gates

`scripts/test-host-contract.py` compares native and generated storage using the
same overlays. The binding case passes 18 records in JIT and NativeAOT, covering
every missing slot, invalid size/version/count, copied table/context ownership,
replacement/uninstall rejection while loaded, forwarded 64-bit output and error
status, and callback/table layouts. Test callbacks for unexercised services abort;
they cannot make runtime validation succeed. The TLS case passes 25 records for
actual upstream TLS config/callback/state layouts, initialized bytes and a
transport-parameter callback. The core-header case adds 17 matching records for
the actual core types and storage, including pool alignment and packed fields;
it links all 47 product objects plus the harness object. These receipts live
under `artifacts/host-contract/`. The passing run uses compiler library SHA-256
`4f625631a75897990d35ec44dfa5e6af50b68d2526925d4729856709a6e75d17`;
all 48 object sidecars bind source, object, compiler and staged-input hashes.

The validated header/source closure and raw/optimized library build gates are
complete. A build-only host declaration has no runtime success status. The
[API profile](api-profile.md) records selected/deferred/rejected behavior for the
owning facade; its selected entries still require runtime implementation.
