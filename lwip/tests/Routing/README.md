Finite three-interface IPv4 forwarding fixture: synthetic Ethernet/ARP/UDP inputs
are processed by translated lwIP and outputs checked for egress, decremented TTL,
IPv4/ICMP checksums, payload, same-interface forwarding prohibition and link
recovery. Independent native C runs the same frames/configuration/assertions.
IPv6 forwarding, static-routing addon and bridge learning/aging remain separate
required cases; this suite is not the full routing gate.
