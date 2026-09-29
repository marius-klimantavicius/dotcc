Finite group-specific IGMP and MLD fixtures observe membership reports, deliver
UDP to a joined group, inject a general membership query and advance the original
protocol timer, then leave and confirm later group traffic is rejected. Report
and leave assertions match the exact tested group address, so unrelated automatic
solicited-node memberships cannot satisfy the witness. The managed fixture uses
two translated owners; native C uses captured output and authored ingress frames.
The finite suite remains separate from the broader addressing/lifecycle gates.

Native and raw/processed JIT/AOT passed threaded `20260929-103455-c49324c6`,
nosys `20260929-103633-09b9abb5`, threaded-locking `20260929-103831-2b08f372`.
The initial managed failure `20260929-103007-aea434fd` used `udp_bind(NULL)`,
which upstream defines as IPv4 ANY even on an IPv6 PCB. The fixture now binds
an explicit family-matched wildcard. Its failed transcript and PCAP are retained.
