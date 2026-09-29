Finite deterministic DNS peer cases exercise translated A/AAAA lookup, cache and
NXDOMAIN delivery across two owners over Ethernet. The peer validates query
shape and emits explicit wire responses; PCAP retains transport evidence. Native
C repeats the same query/response cases through one stack's loopback. Callback
roots remain live until completion or owner teardown. This suite does not claim
DHCPv4/v6, AutoIP, SLAAC/DAD or multicast addressing qualification.

Separate `dhcp-fixtures` uses a deterministic Ethernet DHCP server and the actual
client/ACD timer functions. It verifies DORA and ACK options, T1 renewal, NAK,
lease expiry, repeated ARP conflict/DECLINE and recovery. Native C repeats these
phases with a captured-frame peer; no client state is assigned by the test.

Separate `autoip-fixtures` observes real ARP probes and announcements, verifies
first-conflict defense and repeated-conflict address replacement. With DHCP
cooperation enabled, a link cycle intentionally stops AutoIP; the fixture checks
that behavior and explicitly restarts, preserving the selected address. Caller
owned AutoIP/ACD storage is detached before release. Each finite suite retains
its own native negative control and managed PCAP; IPv6 addressing remains open.
