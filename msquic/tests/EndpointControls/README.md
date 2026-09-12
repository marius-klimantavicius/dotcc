# Live endpoint input and amplification controls

This standalone executable references the actual owning API project with unsafe
code disabled and no additional trimming roots. It is separate from the already
qualified 17-mode API harness. The full campaign passes all 64 profiles across
raw/optimized JIT and trimmed linux-x64 NativeAOT, recorded in
`artifacts/endpoint-controls/results.json`. The receipt binds the external native
peer baseline; changing that test executable requires a fresh campaign.

`MalformedInputs` delivers finite malformed packet-header vectors and a tag-only
mutation of an actual captured protected packet through real UDP input. It checks
actual drop/decryption observations before releasing held valid packets, then
requires exact stream payloads, FIN and fresh bidirectional traffic. The selected
matrix covers both receiving roles, IP families and AES suites. This is not an
unbounded fuzzer or a replacement for the separately native-matched frame and
transport-parameter decoder corpus.

`AmplificationControls` uses a valid oversized server certificate flight. The
relay delivers one unchanged Initial datagram and gates subsequent client input,
so the server cannot validate the path with a Handshake packet. It counts real
UDP datagram bytes and requires the server flight to stop within three times the
received-byte budget while the handshake is incomplete. A network duplicate of
the same Initial adds receive credit without validating the path; the test must
observe a further bounded flight. Releasing the gate must complete authentication
and exact payload/FIN transfer. The same relay and public client can compare the
unchanged native peer executable, which runs only as a separate test process.
The managed control uses InitialWindowPackets=64, disabled pacing and fixed1280
MTU; the native oracle retains its existing settings. The comparison is the
3× amplification invariant and blocked/released credit behavior, not identical
flight layouts, congestion settings or timing.

```sh
env UseLocalLalrCc=false python3 msquic/scripts/test-endpoint-controls.py
```

The default campaign runs raw/optimized × JIT/trimmed linux-x64 NativeAOT, with
native amplification comparisons required. Each form expects eight malformed
profiles (two receiving roles × two IP families × two AES suites) and eight
amplification profiles (managed/native servers × the same families/suites).
Diagnostic selectors `--variants`, `--modes`, `--jit-only` and `--skip-native`
cannot promote the full gate. `entire_p7_qualified` stays false because recovery,
interop and the other P7 features have their own receipts.

The driver verifies actual runtime/provider/pinned metadata, generated manifests,
qualified tool hashes, authored input hashes and executed binary hashes. Native
comparisons require the current qualified P6 executable/library/source/config
receipt. Dynamic packet and timing evidence is retained verbatim; only lines
explicitly prefixed `EVIDENCE endpoint ` are excluded from deterministic
cross-runtime transcript equality. Each control also emits exact profile PASS
identities, which the driver checks for omissions and duplicates.
