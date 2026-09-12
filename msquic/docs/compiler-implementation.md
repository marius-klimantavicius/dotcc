# Compiler implementation evidence

Implementation authorized 2026-09-12; completed compiler fixes are committed. The
[initial scope report](compiler-scope.md) describes the compiler before these
changes; its receipts are preserved in `artifacts/initial-compiler-scope.tar.gz`.

The first implementation tranche passes a clean full Release solution build,
76 selected compiler/runtime unit cases and14 functional checks. Twenty optional
MSVC/GCC-oracle cases are explicitly skipped; committed output sidecars were
generated directly with local GCC. Exact filters, commands and logs are in
`artifacts/compiler-implementation-validation.json`.

| Changed behavior | Validation |
| --- | --- |
| Arbitrary include suffixes, nested GNU variadic comma handling, ASCII multichar integer constants |16 reduced unit cases and3 GCC-derived executable fixtures; all70 actual frame tracing and2 crypt tracing token sequences match GCC after normalizing only file-path prefixes |
| Function typedefs and declarations, GNU annotation lists, empty file-scope declarations, MS anonymous members and recursive bitfield promotion | Declaration/attribute unit cases and executable callback/member fixtures match GCC |
| Requested aggregate/member/object alignment | Structural layout metadata and real aligned stack/global/array backing; C11 and GNU fixtures match GCC in JIT and NativeAOT, including nested offsets, global addresses, local arrays and by-value parameters |
| High-bit signed/enum conversions | Explicit and implicit conversions preserve selected C low-bit semantics; executable receive-mask/send-flags fixture matches GCC |
| GNU byte swaps, legacy sync and atomic builtins |13 reduced cases, same-width narrow atomic storage/canaries and contention checks; full intrinsic fixture matches GCC in JIT and NativeAOT |

All three NativeAOT publications in the first tranche have zero warnings/errors
and exact expected output. Runtime-owned aggregate metadata and initializer work
has a separate receipt in `artifacts/runtime-aggregate-results.json`.

Alignment is enforced on generated C storage. A CLR `Pack=16` or overlapping
Vector128 field alone did **not** guarantee actual16-byte addresses; standalone
JIT and AOT probes disproved that shortcut. The backend uses rounded stack
buffers and rooted, overallocated pinned-byte storage for globals. Directly
allocating generated value types in arbitrary consumer-owned managed arrays
does not inherit that storage guarantee. The future owning API must allocate
through its managed CxPlat memory contract.

At the intermediate diagnostic checkpoint, the full upstream ABI and selected-core
gates remained open. The original-source probes emitted all44 units without any source rewrites, using only the explicitly
diagnostic host declarations. The frozen compiler and dependency hashes are in
`artifacts/unchanged-core/results.json`; linking uses that exact compiler snapshot.
That intermediate Roslyn build failed:6,841 unique diagnostics comprise6,721 unresolved
host/runtime references,91 missing members,27 returned-aggregate fixed-buffer
storage errors and2 promoted-member initializer errors. These are fewer than the
initial8,182 diagnostics but remain a failed library build. Those three remaining compiler categories now have generic fixes: structural
object metadata selects complete definitions independent of object order and
rejects incompatible declarations; anonymous identities use resolved source
identity and physical declaration locations; returned aggregate array members
receive addressable temporary storage; promoted initializer designators target
actual nested storage. A clean solution build,20 targeted unit cases and16
functional cases pass (4 optional oracle skips). Five new/expanded fixtures match
fresh GCC output under NativeAOT, and a separate-object namespace-linked AOT
fixture validates opaque completion and shared/private anonymous identities.
Receipts are `artifacts/compiler-integration-*.log` and
`artifacts/compiler-object-native/`. The reduced passes did not replace the complete selected-core and regression
gates recorded below.

Missing POSIX rwlock
and event-loop header declarations are host-boundary work; declaration-only probe
headers cannot establish their ABI. A subsequent pointer-return attribute form
and `#pragma pack` state were fixed from actual upstream headers. Pack/structural
layout functional checks pass26 cases with8 explicitly gated oracle skips. Packing
state propagates through includes and macro declarations; mid-aggregate pack
changes fail explicitly. Public upstream ABI now matches29 records in JIT and AOT,
including callback invocation and actual field addresses. TLS/core original-POSIX
ABI probes were blocked on missing real pthread header contracts; the managed
host overlay needs its own matched native/generated ABI. The initial12 grouped
gaps were not a bound on total scope.

No modified reference input, manually patched generated C#, success stub, native
product TLS provider or replacement transport was introduced. Later phase gates
still require the actual complete core, host contracts, upstream ABI evidence,
transport traffic, independent interoperability and full regression campaigns.

## Complete selected-core gate

The completed compiler checkpoint (`4f625631a75897990d35ec44dfa5e6af50b68d2526925d4729856709a6e75d17`
for DotCC.Lib.dll) passes2146 unit tests and439 executed functional tests. The
1003 skipped cases are explicitly gated native/oracle campaigns, itemized in
`artifacts/compiler-full-isolated-receipt.json`. The complete47-unit product
closure emits and links as individual objects. All60 native host/TLS/core ABI
observations match JIT and NativeAOT. Both raw and postprocessed managed libraries
build, and consumers that root the entire generated assembly publish and run
under NativeAOT. `config/product-closure.json` binds every source, compiler,
generated-file and gate receipt hash.

The final missing libc additions were six Linux errno values and bounded
`strnlen`; its64-bit bounds, unterminated storage and NUL behavior match GCC in
JIT and NativeAOT. The existing Zig128 wrapping-arithmetic text assertion was
updated for actual aligned parameter copies and backed by a runtime wrapping
regression. The first broad unit attempt was stopped because test helpers
repeatedly scanned the shared temporary directory. No partial pass count was
claimed; a fresh isolated temporary directory completed all2146 cases in82seconds.

These are P0–P2 compiler, ABI and feasibility gates. The BCL platform, TLS and UDP
host services, owning API and end-to-end transport remain separate runtime work.

## Typed managed anonymous-member access

The runtime adapters exposed a public projection problem: the C core could access
promoted anonymous members, while authored C# saw only generated physical field
names. Explicit anonymous-container metadata now emits typed value forwarding
properties in managed-library and object output. These preserve physical fields,
union overlap and ABI; const members are getter-only, and atomic/volatile scalar
and pointer properties use fenced primitives inside a fixed owner scope. Nested
aggregate values use explicit read-modify-write. Arrays, qualified aggregate or
bit-field copies and a member equal to its enclosing managed type retain physical
access instead of an invalid convenience property. C ambiguity is rejected.

Seven direct/object-linked managed-consumer tests pass, including function pointers,
const-pointee versus const-pointer behavior, nested bit-fields, atomic/volatile
access, and a valid C member matching its enclosing tag. A native C control agrees
on all values and the 48-byte layout with offsets8/16/40. Two standalone consumers
pass JIT and NativeAOT with no publication warnings. Receipts are
`artifacts/promoted-managed-final/results.json`. Final full-suite validation passes
2146 units and447 functional cases, with1005 explicitly gated oracle skips; see
`artifacts/compiler-promoted-final-results.json`. The old validated P2 receipts
and object provenance are retained in `artifacts/checkpoint-4f625631/`. The new
compiler `6bd81abfd99e00e605067b3f75260449475ab7b243377aa8c6c470f0709e284a`
and committed fix `c786b9c` pass all60 host/core and29 public native ABI records
in JIT/AOT. Both refreshed raw/optimized libraries and whole-assembly-rooted
JIT/AOT consumers pass; `config/product-closure.json` binds the new receipts.
