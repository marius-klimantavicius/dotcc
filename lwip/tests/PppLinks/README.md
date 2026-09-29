# Qualified finite PPP link controls

`serial.c` and `Program.cs` drive actual PPPoS links through a bounded byte queue,
splitting every emitted chunk into 1–7 byte deliveries. The managed fixture owns two
separate `LwipStack` instances; the native control uses two PPP PCBs under the
original native stack. Both run PAP, CHAP-MD5, MSCHAPv1, MSCHAPv2 and EAP-MD5 with
valid and wrong credentials, then MSCHAPv2 with required 128-bit MPPE. Each successful
link must finish IPCP and IPv6CP, receive one IPv4 UDP payload and three TCP payloads,
close normally and restore heap/pbuf/timeout usage. Wire FCS is observed on
unaltered successful frames; no custom invalid-FCS input is injected.
The independently written RFC1662 decoder checks frame FCS and observes VJ full and
compressed protocols. MPPE must be negotiated, use the encrypted protocol on wire,
exclude the known plaintext, and deliver the correct decrypted UDP/TCP bytes.

The locking profile explicitly requests and acknowledges the upstream LQR option.
That proves LCP option negotiation only: this pin does not implement a periodic LQR
report engine. Native injected clocks are advanced while real `sys_check_timeouts`
runs under the core callback. This is necessary for the locking profile, whose
synchronous core call does not wake the timer thread's real-time mailbox wait.
The first locking attempt without this explicit virtual-clock driver timed out; it
was a fixture scheduling error, not a protocol or compiler repair.

The native serial helper links an additional binary using the exact same fixture
and objects except for original `eap.c`. Wrong EAP credentials must reproduce exit92
at the heap baseline assertion; the staged local pbuf release must make the full
fixture pass. The allocation trace is preserved under `artifacts/native-eap-leak`.
No upstream reference file is changed. Managed qualification uses products freshly
published with the same staged source.

`ethernet.c` and `Ethernet.cs` are an independently encoded RFC2516/RFC1661 peer.
They check PADI service/concentrator names, echo the AC cookie in PADR,
negotiate an actual session, reject LCP
options and observe a fresh request, complete IPCP/IPv6CP, exchange independently
encoded/decoded UDP payloads, and acknowledge termination through PADT. This is a
scripted access concentrator, not lwIP's unimplemented PPPoE server branch. Native controls and managed raw/processed JIT/NativeAOT cells pass in all three
product profiles. Receipts: threaded102218-436a6eb5, nosys102323-8af6d3ee and
threaded-locking102423-3e0a37ed.

These finite controls do not close the broad PPP/link gates. L2TP tunnel challenge
and data cases, broader IPv6 data, reconnect/timer failures, and owning service
registration with one owner disposed while another continues remain required.
