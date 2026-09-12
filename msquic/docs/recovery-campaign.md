# Packet recovery and independent interop campaign

These drivers have targeted optimized JIT results and do not yet qualify P7.
The translated-pair IPv4/AES-128 baseline, Initial loss, periodic loss, reordering,
duplication, delay, and combined-fault exchanges pass in
`artifacts/recovery-checked/results.json`. Rebinding delivers both payloads and
updates the server's remote port, but the required active-path validation remains
false even with a two-second client settling interval
(`artifacts/recovery-settled/results.json`). Exact native private-path diagnostics
reproduce this failure (`artifacts/recovery-native-private-rebinding/results.json`).
With the old mapping explicitly expired, strict new-path validation passes for
native pairs, translated pairs, and both mixed roles in the targeted optimized
JIT IPv4/AES-128 run (`artifacts/recovery-expired-mapping/results.json`). The
original dual-live-mapping case remains a strict failure. Full MTU and
build/runtime matrices remain pending.

Both managed endpoint roles now pass positive exchange and the three
authentication-negative rows in the refreshed eight-case optimized JIT
IPv4/ECDSA/AES-128 run
(`artifacts/managed-independent-refreshed/results.json`). The corrected
peer-specific ALPN expectation described below is confirmed. Each receipt binds its own checkpoint;
subsequent baseline source changes require a new baseline and dependent rerun.

The UDP fault proxy's nine isolated controls pass; those controls exercise the
proxy with opaque datagrams rather than QUIC.

Both drivers reuse executables from `scripts/test-managed-peer.py`. Before and
after execution they verify its source, generated output, executable, and closure
hashes. A selected baseline case must already have passed for each requested
variant, runtime, role, cipher, certificate, and IP family. No native transport
library is loaded by the managed application; oracle peers run separately.

## Packet recovery

```sh
python3 msquic/scripts/test-recovery.py
```

The complete packet recovery subset contains 576 exchanges: raw/optimized,
JIT/NativeAOT, IPv4/IPv6, AES-128/AES-256, translated pairs and both roles against
native MsQuic, across twelve scenarios:

| Scenario | Required observation |
| --- | --- |
| Baseline | Both peers exchange the exact payload through the proxy. |
| Handshake loss | The first client datagram is dropped and the connection recovers. |
| Loss | Periodic drops occur while both stream payloads arrive intact. |
| Reordering | Original packets are actually forwarded out of order. |
| Duplication | Duplicate datagrams are actually forwarded. |
| Delay | Configured bounded delay and seeded jitter are applied. |
| Combined | Loss, reordering, duplication, and delay all occur. |
| Rebinding | The server-facing source port changes during the exchange. |
| Rebinding with expired mapping | The source port changes and replies arriving on the old mapping are explicitly discarded; the new server path must validate. |
| MTU probe loss | Oversized probes are dropped while baseline-sized data can pass. |
| Payload ceiling increase | Initial probe drops occur before the ceiling rises. |
| Payload ceiling decrease | Oversized datagrams are dropped after the ceiling changes. |

Each direction carries 65,537 deterministic bytes and FIN. Peer assertions cover
authenticated QUIC v1/P256 negotiation, exact payloads, send completion, shutdown,
and drained host ownership. Core stream-byte statistics include retransmitted
frames, so they may exceed the exact application payload count; the application
checks every byte and offset independently. Proxy assertions cover effective faults and bounded
queues. Counters distinguish intentional faults from queue overflow, truncation,
foreign traffic, socket errors, and shutdown abandonment.

For both translated and native servers, source-port cases require the final
remote port to match the proxy's new port and the actual active path to be
validated. Native snapshots use the exact pinned native core headers and
compilation defines, with their provenance bound in the baseline receipt.
Earlier native rows with public counters alone establish continued delivery but
cannot identify the validated path. Likewise the payload ceiling models loss above a
threshold; it does not synthesize ICMP or by itself establish MTU discovery.
The pinned upstream discovery algorithm increases MTU monotonically. A ceiling
decrease may produce a bounded failure in native MsQuic as well; that requires
a native-to-native control before diagnosing a compiler or host regression.
`--roles native` selects such an additional test-only control. The current driver
still requires successful delivery and records a failure otherwise; a bounded
native-equivalent failure policy has not yet been qualified or implemented.

The original rebinding case preserves both reply mappings, which causes the
server to challenge both paths while the client observes one proxy endpoint.
Both translated diagnostics and exact native core-header diagnostics show the
old path validated and the new active path unvalidated. This establishes a
pinned native behavior rather than a translated-core divergence. Source review
suggests that two challenges arriving through one perceived client path overwrite
its single pending response; that causal explanation remains an inference.
The separate expired-mapping case models disappearance of the old NAT mapping
and counts those discarded replies explicitly. Its new-path validation assertion
is identical and passes the targeted four-role comparison. The original failing
case remains in the campaign and is never relabeled as validated migration.

Targeted optimized JIT IPv4/AES-128 MTU controls now pass probe loss and a ceiling
increase for both native pairs and translated pairs. A ceiling decrease fails
delivery in both (`artifacts/recovery-native-mtu/results.json` and
`artifacts/recovery-managed-mtu/results.json`). Translated peers report actual
connection-idle status 62 and drain all host ownership. Refreshed native
terminal diagnostics (`artifacts/recovery-native-mtu-terminal/results.json`)
also show both endpoints connected, unfinished, then closed with status 62 and
transport error 1. The strict delivery test remains failed for both, as expected
from the pinned monotonically increasing discovery algorithm.

For a diagnostic subset:

```sh
python3 msquic/scripts/test-recovery.py --variants optimized --jit-only \
  --roles both --families ipv4 --ciphers 128 --scenarios baseline handshake-loss
```

Only a complete subset sets `passed`; selected runs set `targeted_passed`.
Both retain `entire_p7_qualified: false` because other mandatory feature gates
remain outside this driver. A configured fault that never occurs fails its case.

## Independent transport and authentication

```sh
python3 msquic/scripts/test-managed-independent.py
```

The independent peer is the pinned aioquic test process described in
[independent-peer.md](independent-peer.md). Before using its environment, the
driver checks every installed Python module and native extension against its
hash-pinned wheel. Its optional `--alpn` argument changes only the test peer's
configured application protocol; the default remains `dotcc-probe`.

The full subset contains 64 positive exchanges and 24 authentication negatives.
Positive rows cover both endpoint roles, IPv4/IPv6, AES-128/AES-256, ECDSA/RSA
certificates, raw/optimized, and JIT/NativeAOT. Each runtime/variant/role also
rejects an unrelated trust root, a wrong hostname, and a disjoint ALPN.

Negative cases require the expected QUIC TLS alert, no application data, and
drained managed host resources. An unrelated setup failure or timeout cannot
satisfy the expected-alert check. Rejection before listener admission does not
invent an application connection or require a callback for a nonexistent handle.
The pinned aioquic server uses TLS `handshake_failure` (40) for a disjoint ALPN;
the translated picotls server uses `no_application_protocol` (120). The driver
checks the actual peer-specific alert rather than treating either as a timeout.

This subset does not establish tickets, key updates, Retry, stateless reset,
DATAGRAM, CID rotation, resource exhaustion, or the complete upstream test suite.
Those remain separate requirements in [PLAN.md](PLAN.md).
