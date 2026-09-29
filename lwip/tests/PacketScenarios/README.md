# Packet fixtures

The current executable exercises four finite behavior families over copied
Ethernet frames between independent generated owners:

- IPv4 and IPv6 UDP payloads of 4,500 bytes must produce at least three MTU-sized
  fragments and deliver an exact reassembled datagram.
- IPv4 and IPv6 UDP-Lite use wire coverage 16 (eight header and eight data bytes).
  Both original and repeated datagrams preserve their complete payload; the
  emitted coverage field is observed without modifying the wire bytes.

The fixture drains ingress through translated core barriers. A bounded queue
rejects transport overload.
Pass a PCAP path as the executable's first argument to retain original and
repeated Ethernet frames with deterministic timestamps.

`NativeOracle/packets.c` covers the corresponding fragment/checksum semantics
using captured/reinjected multicast Ethernet on a native stack, with matched
product options and the original lwIP algorithms. It preserves a separate PCAP.
The native and managed fixtures use different endpoint topologies; their byte
traces are not asserted identical. They are not independent protocol peers.

Reassembly expiry, reordered/duplicate valid fragments, TCP half-close/reset,
raw IP protocols and other planned packet/recovery cases remain required work.
These initial cases do not close the entire `packets` or `recovery` suite.
