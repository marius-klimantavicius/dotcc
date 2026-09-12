# P1: BCL UDP feasibility on Linux x64

The standalone [UdpFeasibility](../tests/UdpFeasibility/Program.cs) consumer uses
BCL `Socket` APIs and `NativeMemory`, with no MsQuic/picotls/compiler references,
authored native imports, or external packages. It establishes a usable initial
UDP substrate; it does not implement the CxPlat datapath or complete P5.

Run:

```sh
python3 msquic/scripts/test-udp-feasibility.py
```

The script builds JIT and publishes/runs an actual Linux x64 NativeAOT executable.
The AOT program checks its runtime identity through `RuntimeFeature` and uses
source-generated JSON metadata. Detailed outputs are
`artifacts/udp-feasibility-{jit,aot}.json`; the script records commands and source
hashes in `artifacts/udp-feasibility-run.json`. Failed or unavailable cases do
not produce a success manifest.

## Measured capabilities

| Case | Observed behavior on this runner |
| --- | --- |
| Wildcard IPv4 receive | Actual destination 127.0.0.1 / 127.0.0.2 and interface index 1 are returned; remote port is preserved. |
| Wildcard IPv6 receive | Actual destination ::1, interface index 1, and remote endpoint are returned. |
| Link-local IPv6 scope | Sending to a locally assigned link-local address with scope ID 2 preserves destination/interface and remote scope. This is local routing, not a test against a remote link-local peer. |
| Dual-stack IPv6 socket | IPv4 and IPv6 packets both arrive; IPv4 addresses are mapped IPv6 values and need normalization. |
| Explicitly bound source | Binding a sending socket to 127.0.0.2 selects that source address. |
| Datagram boundaries | Exact content/length for 0, 1, 1200, 1472, 4000, and 65507 bytes in each IP family. |
| Truncation | 2048-byte datagram into a 64-byte buffer returns 64 bytes plus `SocketFlags.Truncated`; the next datagram is intact. |
| Pending receive / GC | Receive into caller-owned native memory remains valid through forced GC; the owner is retained until completion. |
| Queued receive | A packet queued before the call completes synchronously on this runner. The same completion path consumes it correctly. |
| Cancellation / close | 64 iterations race send, token cancellation, and socket disposal; every receive drains before the buffer is released, with no observed writes after completion. |
| Oversized IPv4 send | 65508-byte payload with `DontFragment` reports `SocketError.MessageSize`. |
| Unreachable peer | Connected UDP to a closed loopback port reports `SocketError.ConnectionRefused`. |

The custom native memory owner reports zero outstanding pins after completion.
The Linux socket path did not call its `MemoryManager.Pin` method in these runs;
zero pin count is not a promise that all runtime/socket paths avoid pinning.
The owner itself must still outlive every pending operation.

Completion outcome counts can vary across race runs; required invariants are
bounded completion, correct successful payloads, no premature release, and
no observed access after completion. Time limits prevent hangs. The closed-port
case has a small OS port-reuse race and is a feasibility experiment, not a
hermetic unit test.

## Remaining host design and qualification

- `SocketReceiveMessageFromResult` exposes packet address/interface and flags,
  but no ECN/TOS ancillary value. Do not advertise ECN receive capability based
  on this API; a separate BCL-supported path would need evidence.
- `SendToAsync` provides a remote endpoint, with no per-datagram local source
  selector. The bound-source case uses a separate ephemeral sender port. A
  wildcard binding that must reply from each received destination at the same
  listener port needs a qualified per-address socket/binding design. This
  experiment has not established that behavior.
- Loopback/local delivery does not establish path MTU discovery, multi-interface
  routing, remote link-local behavior, or network-change handling. Receiving
  interface indices must be preserved instead of fabricated.
- No segmentation/coalescing, RSS, raw datapath, or offload capability is claimed.
- This probe does not measure buffer pools, sustained load, pending-send
  backpressure, or translated worker ordering/shutdown. Those remain P3/P5/P6
  integration work.
- Windows, macOS, and arm64 have not been run. A runner without a configured
  link-local IPv6 address reports that case unverified/failing rather than
  silently treating it as a pass.

The first adapter can use BCL asynchronous datagram operations with explicitly
retained native buffers, process both immediate and deferred completions, and
cancel/close then drain before freeing contexts. These observations support the
P1 UDP feasibility sub-gate only; the full P1 gate also needs the raw picotls QUIC
bridge and ABI evidence.
