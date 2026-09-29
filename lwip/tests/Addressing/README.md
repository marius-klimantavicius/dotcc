Finite deterministic DNS peer cases exercise translated A/AAAA lookup, cache and
NXDOMAIN delivery across two owners over Ethernet. The peer validates query
shape and emits explicit wire responses; PCAP retains transport evidence. Native
C repeats the same query/response cases through one stack's loopback. Callback
roots remain live until completion or owner teardown. This suite does not claim
DHCPv4/v6, AutoIP, SLAAC/DAD or multicast addressing qualification.
