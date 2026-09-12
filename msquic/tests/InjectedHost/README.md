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

## Optional actual keepalive control

The new `keepalive` scenario is authored and wired, **not yet built or run**.
It does not change the five default scenarios or their `passed` meaning. Run it
separately after the serialized test slot is released:

```sh
python3 msquic/scripts/test-injected-host.py --scenarios keepalive --output msquic/artifacts/keepalive-host
```

It source-links the existing `TlsAdapter/Credentials.cs` certificate recipe and
uses two real authenticated loopback peers, without a native transport substitute.
For each AES suite and IP family it compares a disabled baseline with client-only
and server-only keepalive. Every case negotiates the same 3,000 ms idle timeout;
positive cases set a 250 ms interval through actual connection settings. MTU min
and max are equal, excluding DPLPMTUD probes as an explanation for recurring
packets, and neither peer sends application streams or datagrams.

The existing typed clock/queue seam advances virtual time in bounded steps.
Each active round requires actual sent/received packet counter growth and a
received valid ACK before proceeding. These aggregate counters establish recurring
acknowledged traffic; they do not identify every packet as a keepalive PING. Both
peers must survive more than twice their idle timeout. Clearing keepalive must then permit exactly one idle status 62 and one
shutdown completion on both peers, just as in the disabled baseline. The disabled
baseline and disable-after-survival controls both advance in 50 ms steps for at most ten virtual seconds, allowing a final ACK to refresh the peer
deadline before requiring expiry. Only callback terminal counters are read after
a peer closes. Wall-clock watchdogs bound active waits; passing settings
validation or waiting alone cannot pass the control. Every case closes typed
handles and checks zero host resources, allocations, receive leases and OS errors.

This follows unchanged `src/core/connection.c` at the pinned source: lines
6202–6251 negotiate/reset idle and keepalive deadlines; 6257–6269 expire idle
silently; 6274–6289 schedule PING and restart keepalive; 7812–7817 apply a live
interval or cancel its timer; 8047–8060 dispatch the real expired timer. Actual
PING encoding is in `src/core/send.c:953–969`. Setup confirms the client handshake
through the original serialized local-address setter: an invalid address family
returns state 1 before confirmation and argument 22 after confirmation, before
any mutation (`connection.c:6453–6468`); a getter verifies the endpoint is unchanged.
Server CONNECTED already implies confirmation (`crypto.c:1594–1610`). The test
never invokes those timer handlers directly and never changes connection fields.

Each process returns six completed controls (two AES suites × three settings).
The full raw/optimized × JIT/NativeAOT × IPv4/IPv6 run therefore requires 48
controls. `keepalive_qualified` records that finite timer matrix independently
of the original five-case `passed` and the still-false `entire_p7_qualified`.
Per-control observations, source hashes and binary hashes remain in its separate
receipt and logs. This proves the core/host timer behavior, not a separate facade
`KeepAliveIntervalMs` ownership surface.
