# Finite nonblocking async socket contract

Native Linux and all four raw/optimized JIT/NativeAOT forms pass in
`artifacts/host-async-sockets/attempt-n7spb0qb/receipt.json`, SHA-256
`5557d1877e7fba91399618d5767c00b91b78fabe290d7c3eb9e9479dc80e405c`.
All 21 commands exit successfully. An independent follow-up rechecked 211
source, tool, log and executed-artifact identities. No failed attempt preceded
this boundary result.

The shared C fixture runs against native Linux and translated raw/optimized
JIT/NativeAOT host forms. It uses ordinary TCP listener/accepted socket traffic,
nonblocking accept/receive returning EAGAIN, initial actual writable readiness,
new connections/data after draining, MSG_PEEK preservation, opaque 64-bit event
data, two interests returned with maxEvents=1, duplicate descriptor lifetime,
final close and descriptor-number reuse. SO_LINGER is disabled normally and read
back; enabled zero linger is checked on the idle listener at ordinary shutdown,
matching Kestrel without deliberately resetting a connected peer. Managed-only lifecycle checks request cancellation and dispose the owner
while a registered listener is idle, requiring waits and descriptors to drain.

The selected contract matches the observed Kestrel transport's drain-to-EAGAIN
loop. It is not complete arbitrary Linux EPOLLET behavior: unobserved kernel
transitions while the caller leaves data unread are outside this finite profile.
Events must come from actual readiness and must not be repeated level events
with EPOLLET merely accepted. No one-shot, MOD, RDHUP, fault injection, race
injection or forced-backpressure scenario is included.

When the matching Host implementation and bridges are ready, run from the root:

```bash
python3 blink/tests/HostAsyncSockets/run.py
```

Each attempt snapshots authored sources, headers and tools and retains native,
raw/optimized JIT/NativeAOT command logs, hashes and execution closure identities.
The original Host implementation and raw generated sources are rechecked after
postprocessing. This boundary test does not replace actual Kestrel guest execution.

The implementation stores read/write generations on the shared socket entry.
A real nonblocking accept/receive/send returning EAGAIN advances its direction's
generation while holding the socket lock. An epoll interest reports actual
initial readiness or readiness for a newer generation once, with per-interest
delivery state and round-robin maxEvents selection. The waiter scans at bounded
intervals and owns its complete cancellation/disposal drain; it creates no
independent observer tasks or speculative socket transfers. Reusing a descriptor
number does not retarget an old interest, and final description close removes its
interests. Nonblocking outgoing connect, positive linger durations, one-shot,
MOD and registrations for other object types remain explicitly unsupported.
