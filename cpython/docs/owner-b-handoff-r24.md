# B24 socket resolver implementation

The real shared resolver closure is implemented in `DotCC.Lib/include/netdb.h`,
`DotCC.Libc/NetDbLib.cs`, `ResolverLib.cs`, and `RuntimeContextLib.cs`.
The public C surface adds `getnameinfo`, `gethostbyname`, the actual LP64
`hostent`, NI constants, and a declared per-thread/per-context `h_errno`.
No reentrant C API or feature macro is exposed. A owns policy selection.

Linux x64 host libc supplies real NSS aliases, reverse names, local-domain
rules, transport-specific service names, and native buffer/flag/error behavior.
The private host `gethostbyname_r` bridge fills stable unmanaged storage held by
the existing runtime thread state. The next lookup, including failure,
invalidates the previous borrowed graph. Context disposal releases live holders;
retired thread holders have finalizer cleanup. No borrowed pointer references a
moving managed field or writable process-global host result. The native ordinary
and reentrant numeric lookup paths differ in error-state preservation; fifteen
paired native forms establish the implemented distinction.

Host `getaddrinfo` results are copied into the existing context-owned heap and
always released with host `freeaddrinfo`. Each returned node has its own address
and optional canonical bytes. Ownership is checked before reading a node during
free, so a foreign-context or repeated free rejects before dereferencing it.
The actual C header controls measure unsigned four-byte lengths, pointer order,
all relevant offsets and alignment, rather than aggregate size alone.

The original C fixture and its native/RED outputs remain byte-identical to A18's
independent pre-edit copies. The first broad local-header RED also contains
unrelated GCC/DotCC dialect errors. A separate actual-header reduction supplies
only opaque `timespec` and native builtin `offsetof` syntax bindings: its before
fails on missing resolver contracts; its after passes. Wrong length width,
addr/canonical pointer order and hostent pointer order all reject.

Current Libc JIT and NativeAOT each pass 411 baseline assertions: 59 native
reverse cases, fifteen paired numeric forms, 47 forward cases, four genuine
workers, compacting GC, nested contexts, invalidation, owner disposal and retired
thread finalization. All 90 focused runtime/socket/heap units pass with zero
skips. The old localhost test now compares the exact original NULL hints,
`localhost`/`445` node sequence against native flags, families, socket types,
protocols, lengths, addresses and canonical values. Its old invalid-port
assumption is replaced by a genuine invalid-service rejection; a dedicated
native-backed test retains `65536` → port zero, all three native socket/protocol
entries, errno, NULL-on-error and owned-node/free assertions. Original source and
the failed unit log remain preserved.

Coordinator review found that a local clone allocation failure returned
`EAI_MEMORY` while restoring the successful host call's errno. The R1 source,
PE/PDB proofs, RED logs and source-ready record remain retained. A narrowly
authorized test-only calloc interposer independently reproduces first and second
64-byte clone failure in both JIT and NativeAOT: native/unfaulted lookup succeeds
with three nodes; the exact armed/fault OS thread and allocation count agree;
the fault caller belongs to managed JIT/test NativeAOT code, outside host libc
and native CLR allocation. R1 returns errno123; R2 returns ENOMEM12, NULL and an
empty owner allocation set, including partial-list cleanup. Host failure and
success errno behavior remains native-equivalent. Native allocation-size overflow
also returns NULL/ENOMEM12. The checked local size/OOM path uses this convention.
The interposer has no production runtime reference, build input or environment
setting; it is used only by explicit disposable test invocations.

[R2 source-ready](../artifacts/owner-b-r24-20261018/source-ready-r2.json) binds the
four exact source hashes, native bridge ABI, actual current Libc/test PE/MVID/PDB
checksums, ILC references/object/executable, and results. Unchanged B22/B23 byte
copies are reused. B23's three repairs, Pthread, V13 tools, upstream source and
accepted A handoffs remain preserved. No compiler build, resource patch,
generated-C# repair, product publication, commit or source/case exclusion ran.

This is shared-runtime evidence. New emitted direct/object × raw/processed ×
JIT/NativeAOT controls require the separately coordinated normal tool capture.
Actual unchanged socketmodule/asyncio and full required-suite execution, fresh
193/286 products and each consumer's D10/ABI/lifetime proof, all interpreter
matrices and P6 remain open. GNU flag bits32/64/128 have ASCII controls only;
non-Linux resolver qualification and exhaustive DNS fault injection are not
claimed. The new APIs report failure outside the proven Linux x64 ABI.

The [final handoff](../artifacts/owner-b-r24-20261018/b24-final-handoff.json) and
[release](../artifacts/owner-b-r24-20261018/serial-slot-release-all.json) govern
the next ownership boundary. Main PLAN/IMPLEMENTATION/ledger updates belong to A
and the coordinator.
