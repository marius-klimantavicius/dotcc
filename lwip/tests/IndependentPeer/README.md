The independent peer is Linux kernel TCP/IP. `provision.py` creates a disposable
user+network namespace and TAP, assigns 10.9.0.1/fd09::1, then runs the consumer.
The translated stack owns 10.9.0.2/fd09::2. Namespace setup, kernel/iproute2 versions,
checksummed frames and explicit TUNSETOFFLOAD(0) configuration are retained.
Nothing changes host routes or interfaces; no sudo is used.

Each family runs 256 KiB TCP echo with lwIP both active and passive, plus UDP in
both directions. Threaded profiles use translated BSD APIs; NO_SYS uses raw TCP
and UDP callbacks with explicit owners and rooted state. Only the peer side uses
BCL kernel sockets. The test project alone imports libc ioctl/poll to attach and
read the TAP. It is never referenced by the product or owning facade, which
continue to expose managed frame interfaces without a TAP adapter.
