Finite IPv6 wire fixtures inject checksum-valid router advertisements, neighbor
advertisements and DHCPv6 replies into the actual Ethernet input path. They check
link-local DAD, invalid RA hop-limit/checksum rejection, SLAAC tentative/preferred/
deprecated/expired states, duplicate-address rejection and stateless DHCPv6
transaction matching/DNS options. The tests drive original nd6 timer callbacks
and inspect emitted netif state; they do not assign SLAAC state transitions.
Native C mirrors the wire controls with an intentional assertion-failure control.
Stateful DHCPv6 is not implemented by the pinned upstream library. This finite
suite does not yet close all multicast, routing or asynchronous shutdown gates.

DHCPv6 also covers same-transaction retransmission after timeout and information
refresh after a new RA. Native and raw/processed JIT/AOT passed all three profiles:
threaded `20260929-103455-c49324c6`, nosys `20260929-103633-09b9abb5`,
threaded-locking `20260929-103831-2b08f372` (campaign receipt directories).
