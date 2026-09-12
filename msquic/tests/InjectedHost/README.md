# Deterministic host failure controls

`scripts/test-injected-host.py` builds a separate source-linked host consumer with
the unchanged generated MsQuic and picotls libraries. The first optimized JIT
IPv4 run passes all five cases with zero build warnings or errors, recorded in
`artifacts/injected-host/results.json`. IPv6 and the full raw/optimized ×
JIT/NativeAOT matrix remain pending. These tests do not qualify all of P7.

Each isolated process creates one real client connection and a bound loopback UDP
sink. No native transport or replacement handshake participates. The test copies
the typed host table and delegates to the real host except for the selected
clock, allocation, or socket-creation callback. Five cases run on IPv4/IPv6 under
raw/optimized × JIT/NativeAOT (40 total):

| Case | Required core and ownership observation |
| --- | --- |
| Virtual timeout | A frozen clock does not expire the handshake. Advancing one second causes an Initial PTO retransmission; advancing eleven more causes exactly one connection-idle and shutdown-complete callback. |
| Send allocation | The first send-context allocation returns null. A later allocation sends the Initial, then virtual PTO and idle expiration work. |
| Send completion error | The actual socket operation completes before the test substitutes a network-unreachable result. The production completion records the error, frees the send, queues unreachable, and the core closes with that exact status. |
| Receive error | After renting a receive owner and before submitting an OS operation, the test throws one network-down exception. The real catch returns the lease and restarts receiving; virtual PTO and idle expiration still work. |
| Socket creation | The table returns address-not-available and a null output. The core reports that status and closes without retaining partially initialized owners. |

The clock is held at a fixed monotonic value and advanced explicitly. A table
wrapper caps genuine queue waits at ten milliseconds so unchanged workers
reconsider their deadlines after an advance. It does not fabricate completions,
rewrite connection state, or change the OS clock. Wall-clock watchdogs remain
independent and bound each wait to three seconds.

The send-completion hook already exists as a test-only partial method. The new
`ObserveBeforeDatagramReceive` declaration and call are erased from production
builds because no implementation is present there. Its test implementation runs
before OS submission: it never returns a buffer still owned by a pending receive.

Every case requires zero host resources, platform allocations, and receive leases
after real API shutdown. Receipts hash authored host/provider inputs, generated
libraries, the closure manifest, the test, and published binaries. Selected runs
set only `targeted_passed`; all 40 cases are required for this subset's `passed`.

```sh
python3 msquic/scripts/test-injected-host.py --variants optimized --jit-only
```

The native-peer setup must have supplied the default trust certificate, or pass
`--certificate PATH`. The sink does not authenticate; the certificate is only
input for constructing the real client credential.
