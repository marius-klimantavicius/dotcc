# Test-only UDP fault proxy

`proxy.py --config CONFIG --ready READY --stats STATS` runs a separate loopback
process. READY is atomically written JSON containing the client-facing port.
SIGTERM drains process ownership, counts queued packets abandoned at shutdown,
and atomically writes final statistics. A stopped proxy is not a transport pass.

CONFIG selects `ipv4` or `ipv6`, a required `server_port`, optional loopback
addresses/listen port, a seed, packet/byte queue bounds, and optional
`rebind_after_client_packets`. Rebinding changes the source port seen by the
server while retaining old sockets for replies. It never changes packet bytes.

The `client_to_server` and `server_to_client` objects independently accept
`drop_first`, `drop_every`, `reorder_every`, `duplicate_every`, `delay_ms`,
`jitter_ms`, `reorder_hold_ms`, and `duplicate_delay_ms`. Periodic rules use the
received packet ordinal, starting at one. Jitter is derived from seed,
direction and ordinal; cross-direction event interleaving does not consume a
shared random stream. Live QUIC retransmission timing can still change which
packet occupies an ordinal, so complete packet traces are not claimed identical.

`mtu_bytes` drops UDP payloads above a configured ceiling (zero disables it).
After `mtu_change_after` received packets, `mtu_bytes_after` replaces that ceiling.
This models a changing path payload limit through loss; it does not synthesize
ICMP or claim that a peer has discovered the new MTU. Separate `mtu_drops`
counters distinguish this from periodic loss. Endpoint recovery must prove it.

Counters distinguish intentional loss, queue overflow, truncation, foreign
traffic, send errors and shutdown abandonment. Scheduled delay/reorder counts
are separate from packets actually forwarded out of order. Duplicate copies
have their own scheduled and forwarded counters. Queue peaks and final ownership
counts are recorded. Endpoint payload, negotiation and lifetime assertions must
remain in the real peer driver; this process never decodes or implements QUIC.

Run `python3 msquic/tests/UdpFaultProxy/test_proxy.py` from the repository root.
All seven controls passed on Linux x64: IPv4/IPv6 packet preservation, loss,
reordering, duplication, source-port changes, queue bounds, changing payload
ceilings, and reproducible jitter. These controls validate the proxy; they do
not qualify QUIC recovery, path validation, or NAT rebinding by themselves.

`msquic/scripts/test-recovery.py` orchestrates qualified ManagedPeer and native
peer binaries through this proxy. It checks the baseline source/executable
hashes before reuse, requires successful payload/FIN/shutdown results, and
rejects a case when its requested fault did not actually occur. Selected runs
produce `targeted_passed`; only the complete packet recovery subset can produce
`passed`. Neither qualifies the entire P7 feature matrix. The recovery driver
is authored and awaits successful baseline transport qualification.
