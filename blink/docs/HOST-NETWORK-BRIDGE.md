# Translated C TCP callbacks

`HostNetworkBridge.cs` implements the explicitly named socket declarations in
`managed-host/sys/socket.h` and shares the private descriptor binding from
`HostIoBridge.cs`. It implements IPv4 TCP socket, bind, listen, accept, connect,
getsockname, send, recv, shutdown and selected socket options. Family and integer
storage use the measured host C ABI; ports and addresses use network byte order.
Address outputs honor caller capacity and report the full required length.

Socket-only send/recv reject file descriptors with ENOTSOCK. Transfers use bounded
64 KiB owned arrays and return real short counts. Socket options currently cover
reuse-address, TCP no-delay and bounded receive/send buffer sizes. Unsupported
domains, socket types, flags and options fail explicitly. No hostname resolution
or external destination fallback is performed. Connection policy remains the
instance's private IPv4 namespace; publication returns a separate physical
loopback endpoint to the controller.

The C callback runs on the dedicated interpreter worker. Blocking operations
wait for actual BCL async I/O; a nonblocking connect returns immediately with
success, an immediate error, or `EINPROGRESS`. Its pending task belongs to the
socket and outlives the initiating syscall's wake-token scope. A repeated connect
returns `EALREADY` while pending and `EISCONN` after establishment.
The upstream socket syscall strips `SOCK_NONBLOCK` and applies `fcntl` through
the descriptor layer; subsequent `F_SETFL` changes the same shared description.
Completion is observed through poll/select/epoll and `SO_ERROR`. Instance disposal closes and drains the underlying socket operations,
which makes a blocked translated C accept return a failure. Managed exceptions
are contained at the callback boundary. The existing trusted-host-pointer scope
of HOST-IO-BRIDGE.md also applies here; complete guest-address validation awaits
upstream syscall integration.

`tests/HostNetwork/run.py` builds a native C oracle contacted by a real Python
loopback client, then raw/optimized JIT/NativeAOT consumers contacted by real BCL
clients. All compare fragmented requests, exact 128 KiB responses and EOF, plus
IPv4 record invariants and truncated sockaddr output. The managed consumer runs
two simultaneous C servers on guest port 8080 with distinct published endpoints,
rejects an external destination, forces GC, rejects send on stdout and disposes
a worker blocked in C accept. All modes pass without build warnings. The native
shared test library is used only by the oracle; managed consumers have no native
emulator or native socket adapter.

This original fixture calls translated C server code, not the x86 guest
interpreter. Later socket/readiness fixtures and the public-machine/Kestrel gates
qualify their respective extensions; see [VALIDATION.md](VALIDATION.md). The
[nonblocking-connect sub-plan](P6-NONBLOCKING-CONNECT.md) records the outbound
NativeAOT HTTP-client qualification.
