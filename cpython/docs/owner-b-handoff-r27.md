# B27 SOMAXCONN source-ready handoff

Status: SOURCE-READY; narrow native/header/current-Libc controls PASS.
Terminal RELEASE-ALL records three independent free-lock checks and empty owned
active/queued lists. P0 remains in progress; P1–P6 remain open.

The sole production edit is `DotCC.Lib/include/sys/socket.h`: expose the Linux
header integer constant `SOMAXCONN 4096`, with a comment distinguishing the
public value from the host-controlled effective queue limit. No runtime,
compiler, other header, feature policy or CPython C input changes.

The original A23 reduction passes with native headers and fails with the actual
shared header before repair. Immutable A21 V15 independently rejects it at the
missing-constant `#error`. After repair the unchanged reduction compiles against
the actual shared header. A double-include/type/layout control reports native-exact
`SOMAXCONN=4096 type=int default=128 socklen=4 sockaddr=16`. The native syntax
bridge only forward-declares `tm`/`timespec`; no constant, layout or service is
substituted. Read-only upstream DotCC commit
`d3678b5c50f86bf65d831ee4efda6134a7894116` also lacks the constant; that reference
supplies no implementation or inherited pass.

All28 live listener cases pass: system-native, selected-header native, direct
current-Libc JIT and NativeAOT, each with default128, negative normalization0,
zero, one,4096,4097 and INT_MAX. Linux `ss`/INET_DIAG observes empty and
one-connection receive queues and effective backlog:128 by default,0/1 at the
lower boundary,4096 for larger positive requests on this host. Each case
authenticates a loopback peer, exchanges exact bytes, checks readiness/accept,
rejects bad descriptors with EBADF9 and UDP listen with EOPNOTSUPP95, and proves
the listening endpoint disappears after close. Only one queued connection is
used. Four wrong-payload executions reject. Native applies CPython's existing
negative normalization; direct Libc probes exercise its own clamp. No raw-native
negative-backlog parity is asserted.

The28-case group, including JIT/AOT builds, takes16.854s. All23 unchanged
socket/readiness/reuse unit cases pass with zero skips in an isolated Libc-only
project. Each actual entry/library PE has its own MVID, matching PDB ID and exact
physical Compile-source checksums. Actual IL binds Main→Libc.listen and
Libc.listen→Socket.Listen. The AOT response entry and every reference, native
object and final executable are separately bound.

Evidence: [source-ready](../artifacts/owner-b-r27-20261018/source-ready.json),
[native/header RED](../artifacts/owner-b-r27-20261018/before-result.json),
[live results](../artifacts/owner-b-r27-20261018/after-result.json),
[actual inputs](../artifacts/owner-b-r27-20261018/actual-current-libc-input-bindings.json)
and [release](../artifacts/owner-b-r27-20261018/serial-slot-release-all.json).
The original wrong GNU-dialect invocation and native fixture errno-redefinition
failure remain in their logs. Corrected commands use C11 and guard only the real
native errno accessor macro. Assertions and production sources are retained.
Builds report the existing two VectorIoLib CS0649 warnings and no errors.

V15 still embeds the old header. This is source readiness and direct current
runtime proof, not a new immutable-tool/emitted-C or CPython product pass. Fresh
A24 must normally capture the updated header, then run SAME-C native/direct/
object raw/processed JIT/AOT controls and the unchanged actual socket unit before
a separately granted whole286 retry. Canonical286hostname and oldV11 embedded193
remain unchanged. No tools or products were captured/published.

B22 remains1999 executed/1995 passed, one error, one failure, one required skip,
one interruption and2868 unrun. Expanded D10 readable bounded records and every
supported primitive class remain required implementation work for the next
suitable compiler round. Full new193/286 consumer/ABI/thread/GC/lifecycle/owning/
rooted/trimmed/delivery/audit/upstream and current-runtime production P6 gates
remain open. No case reclassification, cleanup, commit or nested agent occurred.
