# Recovery failure investigation — 2026-09-25

The investigation found a pending PATH_RESPONSE overwrite in the pinned core,
no downward MTU recovery in its discovery algorithm, and a test deadline that
can expire before the transport's own timers. No dotcc miscompilation was
identified. The original strict recovery failures remain failures.

The diagnostics reused the exact raw/optimized MsQuic, PicoTLS and host DLLs
qualified on September 24. Only isolated copies of the test peer changed:
wrappers around the host encryption/decryption callbacks recorded plaintext
test frames and worker-side path/timer snapshots. Queued records were written
after cleanup. The translated sources, host, pinned reference and normal test
drivers were not edited. Instrumentation can change timing; these observations
do not establish failure frequencies.

## Rebinding: a pending response is overwritten

The dual-live-mapping proxy forwards replies from both old and new server-facing
ports through one client-visible endpoint. The server challenges both paths.
The client therefore receives both challenges on its one existing path.

In the pinned `src/core/connection.c`, the PATH_CHALLENGE handler at line 5205
sets `Path->SendResponse` and copies the token into the single eight-byte
`Path->Response` buffer. There is no queue. `src/core/send.c:619` later encodes
one response from that buffer.

All three instrumented raw JIT rebinding runs captured this sequence. The first
run provides a concrete example:

| Server challenge | Packet | Client receives | Client response | Packet acknowledged |
| --- | ---: | ---: | --- | --- |
| New path, `DC9DBBB6157C69CD` | 199 | 243.56 ms | None | Yes |
| Old path, `3DB03FD686E89C6A` | 200 | 243.82 ms | Token echoed at 246.67 ms | Yes |

Times are relative to the client's first diagnostic record. Before processing
the second challenge, the snapshot still contains the first token and
`SendResponse=1`. The next response packet contains only the second token.
The server consequently validates the old path while its new active path stays
unvalidated. Both challenge packets are acknowledged, so the packet-loss retry
in `loss_detection.c:829` does not rescue the missing response. The separate
path-validation timer abandons expired paths rather than retrying an acknowledged
challenge whose response never arrived.

This is a transport defect, not merely an overly strict success assertion:
[RFC 9000 §8.2.2–8.2.3](https://www.rfc-editor.org/rfc/rfc9000.html#section-8.2.2)
requires echoing challenge data and distinguishes a valid response from a
packet acknowledgment. If the first response is sent before the second
challenge is processed, the overwrite does not occur; that explains why the
same scenario can pass in other schedules.

The expired-mapping control passed: the old path's challenge did not reach the
client, the new-path token was echoed, and the server validated the new path.
This control changes the simulated network and is not a repair for the
dual-live-mapping case.

An appropriate core repair would preserve pending response tokens until they
can be sent, rather than replacing one unsent token with another. Validation
retries independent of packet acknowledgments would provide additional
robustness. Such changes belong in an explicit core patch followed by native
and translated comparison, rather than in C# emission.

## Decreasing MTU: retransmission keeps the oversized packet size

The proxy changes its UDP payload ceiling from 1472 to 1300 bytes after the
twentieth received datagram, independently in each direction. It silently
drops larger datagrams; it does not produce an ICMP packet-too-big message.

`src/core/mtu_discovery.c` explicitly implements an increasing search.
`QuicMtuDiscoveryOnAckedPacket` raises `Path->Mtu`; exhausted probes stop the
search. It does not reduce the already selected MTU after a black hole appears.
`packet_builder.c:209` continues to size sends from that path MTU.

All eight instrumented default-MTU NativeAOT runs failed delivery. In a raw
IPv6 example, the selected IP MTU remained 1360 (1312-byte UDP payload), above
the new 1300-byte limit. The same stream offsets were retransmitted in
oversized packets at approximately 0.94, 1.86, 3.71 and 7.40 seconds. Their size
never fell below the limit; both endpoints eventually reported idle status 62.

Two separate controls set `QUIC_SETTINGS.MaximumMtu=1280` before connection
start. Both raw and optimized NativeAOT peers exchanged the full 65,537 bytes
in each direction, acknowledged both FINs and drained ownership. The original
strict test deliberately still rejects these controls because no MTU drops
occurred. The cap is a demonstrated mitigation for this simulated path, not
successful downward MTU discovery or a general throughput recommendation.

Full recovery requires a core change that detects an MTU black hole, confirms
a usable smaller size and updates the discovery/send state. Ordinary packet
loss alone must not be treated as proof that the MTU decreased.

## FIN deadlines: the harness can terminate a still-live connection

The managed peer's `Wait` uses a fixed 15-second wall-clock deadline. The native
peer uses 15,000 one-millisecond sleeps, which took about 16 seconds in the
control. Those budgets are not identical. Both peers configure a 10-second
idle timeout, but that is not a deadline measured from connection start:

- Valid received packets reset it (`connection.c:5961`).
- Its duration is at least three probe timeouts (`connection.c:6202`).
- A separate outstanding-packet disconnect timeout defaults to 16 seconds
  (`quicdef.h:313`, `loss_detection.c:1849`).

Small control packets and server MTU probes can still get through while client
stream packets are too large. The independently triggered directional ceilings
and packetization timing mean native and managed peers need not reach identical
terminal states at the same wall-clock time.

The eight instrumented default-fault repeats all ended with idle closures, so
they did not recapture the two original FIN-deadline cases. They did show late
server PING probes resetting the client idle timer: one raw client last received
a probe at 1.837 seconds and closed around 11.84 seconds. Another optimized
client last received one at 0.958 seconds and closed around 10.96 seconds.

Three explicit late-packet controls then held one real encrypted server packet
before delivery. They retained failed transfer assertions:

| Control | Observation |
| --- | --- |
| Managed, six-second packet hold, normal 15-second wait | FIN deadline; status/error 0 at both endpoints. At 14.873 seconds, the client idle timer still had about 1.220 seconds remaining. The harness initiated local close around 15 seconds. |
| Managed, six-second hold, diagnostic 30-second wait | Client transport timeout 110 around 16 seconds; server idle closure 62. A delayed ACK increased RTT variance and the three-PTO idle minimum, so the disconnect timer won. |
| Unmodified native peers, eight-second packet hold | Both endpoints unfinished, status/error 0 after the native harness deadline at about 16.08 seconds. |

Each control is a fresh connection. The held packets differ: a probe in the
first managed control, an ACK in the second. These are causal timing controls,
not a byte-for-byte replay of either September 24 failure. They demonstrate that
a harness deadline can preempt correct timer behavior in both implementations.
The exact packets behind the two historical deadlines were not captured, so
their individual timer histories remain unproven.

The harness should distinguish transfer failure from observation of the eventual
transport close, use comparable monotonic budgets for native and managed peers,
and record timer/packet timing when those outcomes differ. Increasing an
observation budget must not turn an incomplete transfer into a passing recovery
test. No deadline or assertion in the normal campaign was changed here.

## Retained evidence

All files are under `artifacts/recovery-investigation-20260925/`:

- `results.json`: three strict rebinding failures, one expired-mapping pass,
  eight decreasing-MTU delivery failures and two capped-MTU controls.
- `late-results.json`: the three additional timing controls.
- `summary.json`, `analysis.txt`: decoded frames, challenge acknowledgment/
  response correlation and terminal summaries.
- `Diagnostics.cs`, `Program.cs`, `build.py`, `run.py`, `analyze.py`,
  `proxy-late-packet.py`, `run-late-control.py`: isolated diagnostic sources.
- Per-case peer logs retain plaintext test frames and timer snapshots; proxy
  statistics retain fault counts. `build.json` records commands and source hashes.

`manifest.json` binds the evidence and diagnostic executables. Its SHA-256 is
`26108909881ae2876a267ad829dacf88fd9ad8492fdf84aa91f40fb452c8af08`.
All eight reused product DLLs were checked byte-for-byte against the qualified
raw/optimized peer baseline. Baseline source, generated output, executable and
closure bindings passed before and after the diagnostic campaigns.
