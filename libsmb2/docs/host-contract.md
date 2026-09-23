# Managed socket host contract

The product profile uses [authored C# async sockets and fd-event callbacks](async-transport.md).
The selected upstream `socket.c`, request queues, framing, crypto and authentication
remain translated C. `LibSmb2.Bcl.cs` supplies synchronous, nonblocking adapters;
`HostSockets` owns `Socket.ConnectAsync`, `ReceiveAsync` and `SendAsync` operations.
The facade serializes translated calls on a short-turn executor and awaits actual
protocol callbacks. Idle connections own no waiting worker and have no recurring
readiness timer. No native libsmb2 import or additional C implementation is used.

| Boundary | Product contract |
| --- | --- |
| Descriptors | The authored unmanaged `t_socket` contains one four-byte int. Positive monotonically allocated tokens identify a separate registry; `-1` is invalid. Conversion to int is explicit, conversion from int implicit. Tokens are not native handles or shared Libc fds. Registry lookup validates current context ownership; retired tokens are never reused. |
| C declarations | Original bundled socket declarations keep their int signatures. Audited bridge adapters convert them to `t_socket`. Upstream context fields, socket arrays and fd callbacks use the strong type. The profile registers its size/alignment and overrides selected functions semantically; no generated source patch supplies networking. |
| Transport | One connect, one receive and one send operation at a time per socket, using BCL IPv4/IPv6 TCP. Completion latches state and enqueues a notification; it never invokes translated code inline. `SO_ERROR` consumes stored connect status on the translated caller's execution turn. |
| Buffer bounds | Each socket admits at most 256 KiB of pending send bytes and 256 KiB of completed receive bytes, plus a 64 KiB receive scratch array. Socket-system buffers are separate. Reads release receive capacity; receive scheduling stops at the cap. Accepted sends continue draining independently of upstream write interest. |
| Vector I/O | C calls gather/scatter bytes synchronously, preserve short accepted counts, and retain no borrowed vectors, stack prefixes or translated pointers across an await. A successful write means host-buffer acceptance, not server receipt or SMB success. Ordered buffered receive data precedes a later EOF/error. Empty buffers return would-block until actual EOF. |
| Socket options | TCP_NODELAY, reuse-address, linger, receive-buffer size and send-buffer size have BCL semantics. The nonblocking helper records logical O_NONBLOCK; all actual networking remains asynchronous. Unsupported options fail with a named errno. Half-close is unsupported; product teardown closes and drains owned sockets. |
| Resolver | The facade resolves DNS asynchronously, retaining the original server identity. A thread-scoped prepared result supplies all selected IPv4/IPv6 addresses to synchronous `getaddrinfo`; numeric service ports form owned Linux LP64 addrinfo/sockaddr chains. Unprepared calls fail rather than perform blocking DNS. `freeaddrinfo` releases the originating Libc allocations. |
| Servicing | fd ADD attaches completion state, including a connect that completed before registration. Interest changes and useful capacity/completion transitions schedule work; writable sockets do not continuously reschedule themselves. Buffered input is rearmed after a servicing turn. Happy Eyeballs and managed operation deadlines use one-shot timers. |
| Close/drain | Close immediately invalidates registry membership and interest, cancels pending host operations and closes the BCL socket. Context teardown awaits outstanding operations before relinquishing host ownership. Context addresses can be reused while an old private drain finishes; unique socket tokens prevent late completions from selecting a newer socket. |
| Errors | errno remains thread-local. Async continuations save error codes in socket state; only synchronous bridge calls set the consuming caller's errno. Connect/read/write failures follow the original C return conventions. |
| Allocation | Protocol objects and copied C strings use Libc allocation/free pairs. Managed operation buffers remain owned/pinned until the protocol callback or terminal context teardown and transport drain. |
| Entropy | `arc4random_buf`, `getrandom` and `getentropy` use BCL cryptographic randomness. The selected libsmb2 configuration takes this path. Separate random/srandom compatibility functions do not supply session entropy. |
| Identity | `gethostname` uses BCL host identity. `getlogin_r` maps to `Environment.UserName`; the facade supplies an explicit authentication user afterward. |
| Concurrency | One submitted operation per connection, serialized context mutation/service/destruction, and independent concurrent connections. A separate lock protects upstream's global context list. Low-level callback clients must provide equivalent ownership/scheduling. |

Product low-level C synchronous wait loops and server hosting are unsupported.
Their retained poll/bind/listen/accept entrypoints fail explicitly and never hand
a registry token to Libc. Shared Libc networking remains available to other
translated programs. An explicit legacy profile preserves it for unchanged
upstream C regression tests; those manual-poll tests do not qualify the product
async transport.

The generated project still embeds shared runtime source, including unrelated
filesystem/process declarations. Its presence alone is not a product protocol
dependency. Product call bindings and published dependencies are audited separately.
BCL implementation-level native dependencies are expected.

`tests/AsyncHost` exercises ordinary IPv4/IPv6 loopback transfers, full duplex,
short counts, byte ordering, EOF, independent contexts, ownership, prepared DNS,
retired handles and close/drain. It can target raw or processed output with
`-p:Libsmb2GeneratedProject=/absolute/path/TranslatedLibsmb2.csproj`, and supports
NativeAOT. `HostSockets.Snapshot()` reports cumulative completions/notifications,
registered contexts/sockets, current buffered bytes and per-socket peaks without
scheduling I/O. Windows execution must be recorded separately from Linux results.

`scripts/test-host-services.py` retains the earlier native/JIT/NativeAOT fixtures
for shared Libc services. Those fixtures and the legacy profile are distinct from
both `AsyncHost` and the Samba product interoperability/lifecycle campaign.
