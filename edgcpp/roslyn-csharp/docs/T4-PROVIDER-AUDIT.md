# Published SQLite provider audit

The [binding and contract audit](../acceptance/sqlite/provider-contract-audit.json)
reconciles all 103 required imports of the actual readable production publication:
88 explicit native bindings and 15 runtime intrinsics. No mapping is missing.
This is complete selection/binding reconciliation with bounded execution evidence;
whole-provider coverage remains false as an explicit scope limit. The configured
finite T4 matrix is now [accepted separately](../acceptance/sqlite/t4-acceptance.json).

The audited source is
`/mnt/storage/roslynobj-csharp-20261015/t4/sqlite-readable-generated-1`, with its real
completion/program/import reports and the actual request at
`t4/readability-production-preparation-1/manifest.json`. The complete source-file
inventory and hashes match completion.json. The immutable input is still SHA256
`e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c`,
artifact `9097468216bba3077ff4a24753d09245`. The fresh readable publication's
19-phase JIT/AOT bind and 43-statement proof is linked in the audit. No native
SQLite application library was loaded by those generated processes.

The audit keeps three evidence levels separate:

| Level | Established scope |
| --- | --- |
| Selected, resolved, hash verified | Every native binding matches its exact captured linkage/signature/convention, explicit requested symbol/library and emitted import. Accepted preparation resolved its export; current provider files and observed loaded images retain their hashes. |
| Exercised contract family | Named finite errno/loader/callback/variadic/aggregate/intrinsic/SQL proofs have their own receipt and limits. A family reference does not prove every import or ABI/error/ownership/thread variant. |
| Observed invocation | Seventeen selected-import implementations have explicit calls in genuine focused generated fixtures: 16 host targets plus the canonical logical-errno substitute. No full-SQLite per-import invocation trace is asserted; the other 71 native selections receive no exact implementation invocation claim. |

The 16 focused exact host targets are `getpid`, `qsort`,
`pthread_create`, `pthread_join`, the three mutex-attribute operations
`init/settype/destroy`, the five mutex operations `init/lock/trylock/unlock/destroy`,
and `dlopen/dlerror/dlsym/dlclose`. The seventeenth selected implementation is
the canonical `__roslynobj_application_errno_location` substitute for the
`__errno_location` import. Its fixture invocation proves the selected logical
application slot, not invocation of the raw libc entry. Raw libc
`__errno_location` calls inside ABI gateway bookkeeping are separate evidence.
The focused invocation evidence comes from the accepted
[errno fixture](../acceptance/errno-provider/report.json) and
[loader fixture](../acceptance/loader-provider/report.json), with native, JIT and
NativeAOT observations. It does not count imports from successful symbol lookup,
canonical-address creation, or captured `directCalls`/`addressReferences` fields.
Those fields are retained static context counts.

The errno fixture calls libc `open`. The SQLite object selects the exact captured
identity `__USER_LABEL_PREFIX__open64`, explicitly bound to `open64`. The former
proves a finite open-family/two-fixed-argument variadic protocol, with **no exact
open64 invocation credit**. All nine explicit aliases stay intact:

| Captured linkage identity | Explicit host entry |
| --- | --- |
| `__USER_LABEL_PREFIX__open64` | `open64` |
| `__USER_LABEL_PREFIX__stat64` | `stat64` |
| `__USER_LABEL_PREFIX__fstat64` | `fstat64` |
| `__USER_LABEL_PREFIX__ftruncate64` | `ftruncate64` |
| `__USER_LABEL_PREFIX__fcntl64` | `fcntl64` |
| `__USER_LABEL_PREFIX__pread64` | `pread64` |
| `__USER_LABEL_PREFIX__pwrite64` | `pwrite64` |
| `__USER_LABEL_PREFIX__mmap64` | `mmap64` |
| `__USER_LABEL_PREFIX__lstat64` | `lstat64` |

No generic prefix stripping or source-name rebinding occurs. Historical
[provider-plan.json](../acceptance/sqlite/provider-plan.json) remains unchanged
with its preparation-only status. Native rows in the public program report still
say `declared-unverified`; this separate audit records later manifest, file and
receipt checks without rewriting those original facts.

The selected logical errno provider intentionally substitutes a native TLS
application `int*` for the raw libc slot. Direct, indirect and address-exposed
`__errno_location` use one canonical generated accessor. Outgoing native calls
seed/capture raw errno before managed transitions. Incoming generated C entries
capture incoming state before CLR entry, update the application slot, and publish
its final value on return. The focused proof covers application writes, native
success leaving errno untouched, failure, nested qsort and a foreign pthread.
Its public-use audit classifies 28 unique signed32 value dereferences; three
explicit-initializer overlaps explain the inventory's 31 contexts. It establishes
only that selected terminal-consumer pattern, not general alias analysis.

The selected loader protocol uses the exact registered error-provider address,
copies native error bytes before CLR activity, and replaces pending state after
each selected loader call, including clearing it on successful glibc calls.
NULL symbol values are distinguished from errors. Reading clears pending status
while keeping the returned buffer alive until the next selected loader call.
Direct, canonical and unknown-indirect selected loader routes have finite proof.
Native TLS-destructor reentry and arbitrary incoming foreign loader state remain
unexercised.

Known generated function addresses dispatch to ordinary managed bodies. Unknown
native scalar, variadic and aggregate gateways preserve the selected error
protocol, but their combined variant coverage is finite. The publication emits
1,014 scalar incoming entries, 362 scalar outgoing shapes, seven addressed
variadic entries and nine variadic outgoing shapes. These are generated code
inventory counts, not measured calls. Its variadic native imports are exact
`open64`, `fcntl64` aliases and `mremap`; it emits no aggregate-by-value native
import or aggregate gateway. The separate
[primitive variadic proof](../acceptance/variadics/report.json) covers register and
stack exhaustion, interleaving, fixed prefixes, independent va_copy, borrowed
cursors and unknown native outbound entry. The
[aggregate proof](../acceptance/aggregate-abi/report.json) covers six concrete
layouts/signatures. Neither establishes every SQLite variant or all combinations
with logical errno.

All 15 recorded intrinsic requirements select Runtime.TargetIntrinsics and have
accepted [direct](../acceptance/intrinsics/report.json) and
[production CLI](../acceptance/intrinsics/cli-report.json) JIT/AOT receipts. They
are not native bindings or attempted symbol lookups. Their exercised scope is
captured relaxed order 0; load widths 1/2/4 and store widths 1/2/4/8; fence;
bswap16/32/64; defined nonzero clzll; and signed64 add/sub/mul overflow with a
signed64 output and normalized bool8 result. No wider/unsigned/order variant is
credited by the family count.

The actual provider files remain:

| Provider file | SHA256 |
| --- | --- |
| `/usr/lib/x86_64-linux-gnu/libc.so.6` | `3a15d66867d83762c7f2f1e37359cb8f6c5743edb369c65285cb0b1c4f7498bf` |
| `/usr/lib/x86_64-linux-gnu/libm.so.6` | `fce00b6f25f459cf4ae0b7fae4257909a1a8a86f9149e7c029a5d617baf1ccd0` |

Resolution used glibc 2.39 on Linux x64, kernel `6.8.0-138-generic`. The actual
provider preparation resolves `time` and `gettimeofday` through the normal kernel
`linux-vdso.so.1` image. IFUNC/vDSO resolution is valid host provenance; the audit
does not require hashes for arbitrary resolved executable addresses or a
nonexistent vDSO backing file. Measured runtime information records .NET SDK
10.0.112 and runtime 10.0.12, commit `95017c711e`; the audit hashes loaded JIT
runtime images and records native compiler/linker attribution. The generated ABI
source and binary, matching JIT/AOT copies, loaded libraries and dependencies are
linked with exact hashes. Its native code forwards ABI/error/TLS only; application
algorithms stay generated C#.

The three finite ProviderContracts cases now pass native/JIT/NativeAOT, all
16 invariants. [Their receipt](../acceptance/sqlite/provider-contracts-report.json)
records immediate clock endpoint bounds/Julian rounding, requested random-buffer
extent/guards and allocator grow/preserve/msize/free. Nondeterministic values
remain audit data; direct SQLite API counts add no raw host-export invocation
credit. The audit records `boundedSelectedProviderAuditComplete=true`.

Descriptor/stat/locking/mapping, borrowed TLS/static-storage and other
error/ownership/thread variants remain outside the finite receipts. Current SQL,
persistence, callback and math successes retain their own bounded provenance;
they do not imply invocation of all 88 exports. Exact focused implementation
evidence stays 16 host targets plus one logical-errno substitution, with 71
native selections untraced. Whole-provider coverage remains false without
requiring a new campaign to force every retained variant. The original
preparation and public declared-unverified inventory facts remain unchanged.
Preparing this reconciliation required no new build, test, export or resolver;
the referenced execution fixtures have their independent frozen receipts.
