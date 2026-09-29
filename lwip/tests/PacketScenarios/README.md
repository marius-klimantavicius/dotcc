# Packet fixtures

The current executable exercises four finite behavior families over copied
Ethernet frames between independent generated owners:

- IPv4 and IPv6 UDP payloads of 4,500 bytes must produce at least three MTU-sized
  fragments and deliver an exact reassembled datagram.
- IPv4 and IPv6 UDP-Lite use wire coverage 16 (eight header and eight data bytes).
  Corruption outside coverage is accepted with the expected changed payload;
  corruption inside coverage is rejected, and the next valid datagram succeeds.

The negative case drains ingress before enqueueing translated core barriers;
absence is checked after the affected input has executed. It does not infer
success from an arbitrary sleep. A bounded queue rejects transport overload.
Pass a PCAP path as the executable's first argument to retain original and
modified Ethernet frames with deterministic timestamps.

`NativeOracle/packets.c` covers the corresponding fragment/checksum semantics
using captured/reinjected multicast Ethernet on a native stack, with matched
product options and the original lwIP algorithms. It preserves a separate PCAP.
The native and managed fixtures use different endpoint topologies; their byte
traces are not asserted identical. They are not independent protocol peers.

Reassembly expiry, reordered/duplicate/overlapping fragments, TCP half-close/reset,
raw IP protocols and other planned packet/recovery cases remain required work.
These initial cases do not close the entire `packets` or `recovery` suite.
