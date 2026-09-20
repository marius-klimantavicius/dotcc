# Finite nonblocking async socket contract

Source preparation only: this fixture has not been built or executed yet.

The shared C fixture runs against native Linux and translated raw/optimized
JIT/NativeAOT host forms. It uses ordinary TCP listener/accepted socket traffic,
nonblocking accept/receive returning EAGAIN, initial actual writable readiness,
new connections/data after draining, MSG_PEEK preservation, opaque 64-bit event
data, two interests returned with maxEvents=1, duplicate descriptor lifetime,
final close and descriptor-number reuse. SO_LINGER is disabled normally and read
back. Managed-only lifecycle checks request cancellation and dispose the owner
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
