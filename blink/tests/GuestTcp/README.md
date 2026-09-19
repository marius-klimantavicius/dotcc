# Finite guest TCP syscalls

Native and all four managed modes passed at
`artifacts/guest-tcp/attempt-8r2oj_2k/receipt.json`
(SHA256 `ad31914a360345f527ae55fcff7dcb669b8e8b86b706971953585e447549a03d`).
Raw JIT, raw NativeAOT, postprocessed JIT and postprocessed NativeAOT matched
the native five-line state transcript with empty runtime stderr. All endpoint
sidecars passed their independent invariants; each managed owner retained only
its three original descriptors and no pending socket operations after close.
The canonical assembly was `73428887715220efa9380f0dc8632efa11c202dbe67fe72630abda978cd4db75`;
108 producers were retained and only the authored driver was replaced. Source,
producer, raw generated file, Host snapshot, log and execution binary hashes
were independently rechecked after completion. The earlier native-only gate is
`artifacts/guest-tcp/attempt-rco6bg8m/receipt.json`
(SHA256 `3a99ed636fc913bbd84be9c31bdc894a6fdd160cbbdec502761922a8ed37d647`).

This fixture executes the real `0f 05` instruction and pinned Linux syscall
dispatcher with valid nonlinear guest page tables. It creates one TCP listener,
accepts exactly one connection, receives a known 257-byte request and orderly
EOF, sends a known 263-byte response, shuts down writes, closes both guest
descriptors and frees guest mappings. There is no guest ELF, HTTP, service,
execution worker, cancellation loop or injected failure.

The finite syscall set is socket41, setsockopt54, getsockopt55, bind49,
getsockname51, listen50, poll7, accept43, recvfrom45, sendto44, shutdown48 and
close3. The fixture sets and queries SO_REUSEADDR and TCP_NODELAY, requiring
value1 and length4 with preserved canaries. IPv4 sockaddr storage and both
payloads cross valid page boundaries. Guest ABI registers, actual host fd
callbacks, exact payload bytes, surrounding canaries, descriptor counts,
syscall scratch cleanup and final guest page counts are asserted.

Setup observes a nonready listener with poll timeout0. Only after an independent
peer has connected, sent the complete request and shut down its write side does
the exchange observe POLLIN, accept, and consume the request. Poll readiness is
checked with timeout0; this does not qualify blocking guest poll or stop logic.
Positive stream transfer prefixes are accumulated until the finite byte count
is reached; the fixture does not require one syscall to transfer a whole stream.
Sendto uses MSG_NOSIGNAL, which the pinned dispatcher strips before its ordinary
HostMessages callback. Recvfrom uses null source-address outputs, as permitted
for connected TCP.

The native reference links the pinned native Blink archive and a separate
native-only pthread/libc peer. Managed forms retain 108 canonical producers,
replace only the authored driver with this fixture and copy the exact canonical
Host/bridge snapshot. Their peer is an independent real BCL Socket connecting to
the existing explicit `InstanceIo.Publish` loopback endpoint. No model, compiler,
upstream algorithm or generated C# is edited by this fixture.

All translated Setup/Exchange/Destroy calls and host bindings remain on the
same thread. Only peer operations are asynchronous. The native thread is joined
and the BCL peer task is completed after exact response and EOF verification.
No peer remains active on a successful result. Each managed mode uses a fresh
process and disposes its private owner; no translated code runs afterward.

Native guest ports are physical socket ports. Managed guest ports belong to a
private namespace and explicit publication exposes separate physical loopback
ports. Each mode writes a hashed endpoint sidecar retaining all four raw ports;
the runner checks their valid ranges, native identity and peer results. Neither
port numbers nor partial-transfer call counts are cross-platform equality
targets. Exact deterministic state transcripts are compared across native, raw
JIT, raw NativeAOT, postprocessed JIT and postprocessed NativeAOT. Managed
sidecars additionally require only the three original host descriptors and no
pending socket operations after guest close.

```sh
python3 blink/tests/GuestTcp/run.py --assembly-receipt <qualified-assembly-receipt>
```

Run only after source review and build-slot release. `--native-only` preserves
an independent native gate before managed execution. The runner pins the
upstream revision/archive/headers, canonical producer sources/receipts/objects,
fixture and native peer sources, copied Host and bridges, compiler/postprocessor
and native/dotnet tool identities. It hashes execution binaries before and
after each mode, rejects unexpected runtime stderr and checks raw generated
source immutability. Every command receives an isolated TMPDIR. Timeouts stop
the command process group with a ten-second grace; all failed attempts and logs
remain available. Installed SDK and NuGet assets are explicit nonhermetic inputs.
Failed native fixtures may exit immediately; a stalled process is bounded by
the outer timeout. Failure cleanup is test orchestration, not a qualification
of guest cancellation or a production owner stop contract.
