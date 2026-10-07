# Selected musl network algorithms

This component supplies `htons`, `ntohs`, `htonl`, `ntohl`, `inet_ntop`,
`in6addr_any`, and `in6addr_loopback` from the pinned musl 1.2.6 release.
It has no OS calls. The byte-order routines detect host endianness; presentation
formatting uses the selected musl printf and the existing memory/string runtime.
The previously implemented strict `inet_pton` remains in `LibcInet.cs`.

`network_abi.h` is the authored shared ABI declaration used by this component
and the portable POSIX `netinet/in.h`. It fixes address family values to the
translator's portable Linux/musl ABI, independently of the host socket ABI.
This component can also be parsed when the POSIX profile is disabled; enabling
POSIX exposes the public networking headers to application input.

The algorithms are unchanged. Adaptations redirect includes to the shared ABI
and local byteswap header, remove the unused musl `features.h` include, and
remove C-only `restrict` plus add an explicit void-pointer cast for C++ parsing.
`source.json` pins original/adapted/patch hashes. `originals/`, `patches/`, and
`COPYRIGHT` preserve provenance and license. Default provider reachability pruning
keeps only definitions actually referenced by the translated application.

`backend/tests/libc_network_verify.py` links these same pinned algorithms into a
native C oracle and compares translated C static/instance and C++ instance
execution, including optional NativeAOT. It exercises exhaustive uint16 byte-order
round trips, randomized uint32 round trips, loopback/unspecified constants, IPv4,
IPv6 zero compression and mapped addresses, capacities 0–47 with canaries,
unsupported families, and success/error errno behavior. The oracle deliberately
uses musl's formatting policy, which need not match glibc's permitted choices for
IPv4-compatible IPv6 spellings or partially written short buffers.
