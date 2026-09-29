# lwIP host contract

The campaign's authored C files use upstream architecture, application callback
and netif APIs. Protocol parsing, TCP state, netconn/socket behavior, PPP crypto,
checksums, memory pools and timeouts remain translated upstream code.

`LwipSysArch` stores synchronization handles in a weakly keyed registry for each
generated owner. Handles use the LP64 C `intptr_t`/managed `long` ABI. Mailboxes
carry null messages, enforce bounded FIFO capacity and return the upstream
`ERR_MEM`/`SYS_ARCH_TIMEOUT` statuses. Semaphore waits use monotonic elapsed time
and BCL monitor deadlines; lwIP semaphores are binary. Recursive protection
returns the previous nesting depth and rejects out-of-order restoration. Core
locking uses a separate recursive monitor with actual thread ownership checks.

Each worker captures its explicit owner, retains a generated callback lease,
enters that owner's runtime context and releases both after the C entrypoint
returns. Requested positive stack sizes are passed to the BCL `Thread`
constructor. lwIP priorities remain logical hints: BCL threads use their normal
platform scheduling priority. Core callbacks execute synchronously on the real
translated `tcpip_thread`; no OS sockets implement the protocol stack.

Normal worker shutdown queues an ordinary translated `tcpip_callback` marker.
The marker sets an owner-specific stop flag. Only the `tcpip.c` translation unit
receives the authored `DOTCC_TCPIP_WORKER` define; its upstream
`LWIP_TCPIP_THREAD_ALIVE` hook unlocks and returns at the next top-level loop.
The message has already been freed by the original upstream dispatch loop.
`sys_check_timeouts` is compiled without that macro, so it cannot accidentally
release the core lock. Native controls use the same per-unit selection.

The host then joins workers before retiring host resources and owner backing.
A separate exception cancellation path can wake blocked host waits during
failed initialization/forced reclamation; this is not a replacement for closing
active sockets, protocol callbacks or pending app operations. Initialization
contexts remain rooted through timeout until the worker has joined. Repeated
initialization attempts, including after failure, are rejected.

`LwipStack` owns startup and netifs, serializes facade operations, dispatches core
operations using upstream callback APIs, copies inbound frames into owned pbufs,
and removes its interfaces before stopping the worker. `Core` is an explicitly
low-level borrowed owner: callers must establish `__DotCcEnter`, honor upstream
threading rules and finish all calls/resources before disposing the facade.
Higher-level cancellable sockets and full pending-protocol teardown are not yet
qualified. Clearing the final host registry is not evidence of protocol cleanup.

Ethernet `linkoutput` copies the pbuf chain before returning. `LwipFrames` copies
again into a managed byte array owned by the receiving transport. The sample
uses a bounded 1,024-frame queue and rejects admission when full; it does not
block the TCP/IP core waiting for queue capacity. Exceptions from supplied frame
outputs are captured and reported via `ThrowErrors` outside translated callbacks.
Serial channels likewise copy output and retain input until read or abort.

HTTP custom files provide copied, independently owned buffers until `fs_close`.
POST callbacks collect bounded bodies, free incoming pbufs through translated
lwIP and invoke configured storage handlers. Handler failures return an empty
response URI and are exposed through `LwipApplications.ThrowErrors` outside the
translated callback. SNMPv3 application callbacks provide per-owner volatile
engine/user state and only configured `noAuthNoPriv` users. No cryptographic
provider is substituted or restored by these callbacks.

The standalone `tests/SysArch` executable uses a deliberately small simulated
owner to test host synchronization/storage contracts. Its checks do **not** prove
instance callback ABI or translated-worker correctness. NativeOracle builds the
matching product profile's core/API/PPP closure, performs 256 KiB TCP and UDP over
IPv4/IPv6 loopback and verifies a deliberate assertion failure. It is an early
oracle, not an independent Ethernet peer or a two-owner isolation witness.
