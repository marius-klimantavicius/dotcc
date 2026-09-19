# Linux x64 host contract

The selected upstream `socket.c`, request queues, framing, crypto,
and authentication remain translated C. Shared dotcc libc supplies host services;
the managed facade adds ownership, serialization and a readiness pump over the
upstream asynchronous API. The translated synchronous entry points remain in the
low-level library but are not used by the facade. No campaign-specific SMB
implementation or native libsmb2 import is used by the consumer.

| Boundary | Contract |
| --- | --- |
| Descriptors | Stable dotcc descriptor-table entries own BCL sockets. Close releases the socket and entry. `fcntl` changes real nonblocking state and tracks supported descriptor flags. |
| Transport | BCL IPv4/IPv6 TCP, real connect progress and `SO_ERROR`, short send/receive counts, zero for orderly read EOF, and Linux-shaped errno failures. `poll` reports actual readiness, invalid descriptors, and timeout expiration. |
| Socket options | Supported options include keepalive, TCP_NODELAY and linger. Unsupported options fail. Linger has the Linux two-int layout and actual BCL semantics, including abortive close. |
| Resolver | BCL DNS or numeric-address parsing returns an owned Linux LP64 addrinfo chain. Each node owns its sockaddr and optional canonical-name storage; `freeaddrinfo` frees that chain. Supported service names are explicit; numeric ports serve the campaign. |
| Vector I/O | Linux LP64 iovec entries are validated, gathered/scattered through a pooled buffer, and forwarded to a single descriptor read/write. Actual short counts are preserved. Datagram boundaries are not split into one send per vector. |
| Entropy | `arc4random_buf`, `getrandom`, and `getentropy` use BCL cryptographic randomness. The selected libsmb2 configuration takes this secure path. Separate `random`/`srandom` compatibility functions are noncryptographic and do not supply session entropy. |
| Identity | `gethostname` uses BCL DNS host identity. `getlogin_r` maps to `Environment.UserName`, not terminal utmp identity; the facade always supplies an explicit authentication user afterward. |
| Allocation | C objects and copied strings use translated libc allocation/free pairs. Resolver nodes, PDUs, contexts and handles must remain in their originating allocation domain. Managed read/write arrays are pinned for the complete translated asynchronous request and its servicing/drain. |
| Errors | errno is thread-local. Each scheduled synchronous call consumes its status/error text on that same worker before returning to managed code. Resolver and login functions retain their direct error-number conventions. |
| Concurrency | The facade locks each context across mutation and servicing. A separate registry lock covers upstream global context-list creation/destruction. The low-level API requires callers to provide equivalent synchronization. |

The generated project embeds the shared runtime source. That runtime also contains
unrelated filesystem/process native declarations and dynamic-library utilities;
their presence is not a libsmb2 protocol dependency. The final reachability and
published-dependency audit remains an acceptance gate. BCL implementation-level
native dependencies are expected.

`scripts/test-host-services.py` runs matching native, generated JIT and NativeAOT
fixtures for these host boundaries. It records tool hashes and fails if they
change during the run. These fixtures do not substitute for the separate Samba
interoperability and lifecycle campaign.
