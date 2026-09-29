Finite IPv6 wire fixtures inject checksum-valid router advertisements, neighbor
advertisements and DHCPv6 replies into the actual Ethernet input path. They check
link-local DAD, SLAAC tentative/preferred/
deprecated/expired states, duplicate-address rejection and stateless DHCPv6
same-transaction retry/DNS options. The tests drive original nd6 timer callbacks
and inspect emitted netif state; they do not assign SLAAC state transitions.
Native C mirrors the wire controls with an intentional assertion-failure control.
Stateful DHCPv6 is not implemented by the pinned upstream library. This finite
suite does not yet close all multicast, routing or asynchronous shutdown gates.

DHCPv6 also covers same-transaction retransmission after timeout and information
refresh after a new RA. Historical pre-policy native and raw/processed JIT/AOT
receipts (which also included now-removed custom corruption probes) were:
threaded `20260929-103455-c49324c6`, nosys `20260929-103633-09b9abb5`,
threaded-locking `20260929-103831-2b08f372` (campaign receipt directories).
