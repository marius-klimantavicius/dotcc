# Translation coverage and evidence

Updated 2026-10-16. The design at `e40280bea` is approved. Implementation starts
from the public RoslynObj 2.0 reader (with its exact published 1.0 admission).
EDG is the semantic authority. No prior dotobj implementation or evidence is used.

| Gate | Current state | Evidence | Remaining requirements |
| --- | --- | --- | --- |
| T0 | Closed for contract/inventory | [Complete SQLite inventory](SQLITE-INVENTORY.md); 4,212 bodies/592,512 body operations; 103 required imports; genuine storage/cleanup/address closure tests | T0 is inventory only; execution is proved separately under T2/T3 |
| T1 | Closed for first fixture slice | [Reproduced report](../acceptance/t1/report.json): 23 phases pass; native/JIT/NativeAOT 66/38; deterministic repeat; public layout/template and source review | Broader forms remain explicit below and under T2/T5 |
| T2 | Closed for bounded fixture contracts | [Assembly](T2-ASSEMBLY.md), [storage/callbacks](T2-STORAGE-CALLBACKS.md), [variadics](T2-VARIADICS.md), [sequencing](T2-SEQUENCING.md), [two-unit CLI](T2-PROGRAM.md), [native aggregate boundaries](T2-AGGREGATE-ABI.md) and source review pass | Broader forms remain explicitly unsupported/unexercised; broader C++ and target profiles remain later work |
| T3 | Closed for complete source and first execution | [Production audit](../acceptance/sqlite/production-audit.json): all 4,212 bodies, 103 requirements, five completion hashes; 19 JIT/AOT execution phases pass | Broader configured behavior/provider audit/readability belongs to T4 |
| T4 | Closed for configured finite SQLite acceptance | [Consolidated receipt](../acceptance/sqlite/t4-acceptance.json), [row table](T4-PROGRESS.md): native/JIT/AOT behavior, providers, readability/resources and restart pass | Explicit unexercised variants remain; broader C++/portability/scale belong to T5/T6 |
| T5 | Open; bounded lifetimes and ordinary/virtual-base dispatch exercised | [Normal proof](T5-NORMAL-LIFETIMES.md) and [generated-only int EH](T5-EH-CONTRACT.md) and [constructor member cleanup](T5-CONSTRUCTOR-FAILURE.md) and [automatic partial arrays](T5-PARTIAL-ARRAY.md) and [integer rethrow](T5-RETHROW.md) and [default noexcept termination](T5-TERMINATION.md), plus [single-inheritance virtual dispatch](T5-VIRTUAL-DISPATCH.md) and [multiple inheritance/fixed adjustor](T5-MULTIPLE-INHERITANCE.md) and [virtual bases/VTT](T5-VIRTUAL-INHERITANCE.md) and [exact class payload](T5-CLASS-PAYLOAD.md): production CLI, native/JIT/AOT complete twice-run observations, first-pass selection, captured cleanup and balanced payload ownership | Exact int payload with bounded original/rethrow profiles and one successful noexcept16/8 class-copy/const-reference handler; bounded member/automatic-array shapes and const-record references only; pending class-copy failure, class hierarchy/polymorphic matching, mutable-reference generalization, full-array exceptional activation, failed-member construction, further rethrow chains, uncaught/custom termination, general diamonds/repeated virtual bases, covariant return adjustments, virtual destructors, RTTI queries/member pointers, guards/TLS and broader ABI forms remain open |
| T6 | Open; bounded next-target reader facts only | [DuckDB four-root trace](DUCKDB-SLICE-TRACE.md): actual class throws, reference roles and one-hop dependencies; no translation/link acceptance | Additional ABIs/platforms, a runnable selected C++ library slice and bounded multi-unit scale with dependency closure |

Capability rows will record captured form, projection/provider, activation,
execution evidence and limits. Inventory occurrence counts establish what is
present; they do not establish implemented execution support.

The pinned SQLite input is immutable:
`/mnt/storage/roslynobj-sqlite-20261014/export/sqlite3.roslynobj`, SHA256
`e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c`.
Generated outputs and long logs use `/mnt/storage/roslynobj-csharp-20261015`.
Generated SQLite execution is proven in JIT and NativeAOT. Its loaded-library
inventories contain generated ABI/system/runtime libraries and exclude native
SQLite application libraries; the native reference runs separately.

## First T1 execution evidence

The frozen producer exported `tests/Fixtures/T1/arithmetic.c` (810 entities) and
`resolved-template.cpp` (868 entities). Independent GCC/G++ native harnesses
observe 66 and 38. The in-tree `tests/Projection/Projection.csproj` Release build
passes with zero warnings/errors. It emits two C functions plus `Pair`, and five
C++ functions including the selected signed/unsigned `add` and `select_value`
instances. No C# generics or source-language lookup is involved.

Fresh generated projects under
`/mnt/storage/roslynobj-csharp-20261015/t1/final/generated-{c,cpp}` compile and
produce exactly `66\n` and `38\n` under both JIT and NativeAOT. The generated
dependency manifests contain only the generated project and
`RoslynObj.CSharp.Runtime`. NativeAOT ELF dependencies are libc, libm and the
system loader. No native application provider or ABI shim is used in these
fixtures. The fixture-only option prints the entry result; ordinary executable
mode returns the recorded entry result as its process exit code.

The authoritative final reproduction is
`/mnt/storage/roslynobj-csharp-20261015/t1/final-reproduction/t1-report.json`,
copied unchanged into `acceptance/t1/report.json` (SHA256
`3e38b4a34db983c7ab5844f5425e958df2fe95ae9494bb7d60f4af9655ecddb6`).
All 23 phases pass, including repeat emission with identical source/project
hashes. The report freezes 20 implementation inputs and four translator/reader/
runtime build artifacts, plus fixture and generated source/binary hashes.
Final AOT publishes took 4.52/4.62 seconds with waited process high-water RSS
203,976,704/202,063,872 bytes, within frozen 300-second/2-GiB budgets. Each phase
also records sampled live process-tree RSS (which can double-count shared pages
and miss short peaks). These are small-fixture measurements, not scale acceptance.

Readability review confirms named pointer/record operations, explicit Pair
offsets 0/4 and size 8, ordinary loops/conditions and concrete C++ calls.
Remaining local casts preserve captured narrow integer conversions. Earlier
bootstrap outputs are retained separately and are not the final milestone.

| Capability | Status | Activation and limits |
| --- | --- | --- |
| Integer conversions/local storage | Exercised | C fixture uses signed 32-bit, unsigned 8/16-bit storage, narrowing and promotions; 64-bit integer arithmetic remains unexercised |
| Pointer/record layout/direct calls | Exercised | Named Pair fields, stable local pointer, address/dereference, captured selected calls; no globals, indirect or host calls |
| Structured control flow | Exercised | For/while/if/return; switch/goto remain unsupported |
| Concrete C++ instantiations | Exercised | Four EDG-selected signed/unsigned instances; no generic/type/overload reanalysis |
| Other scalar operators | Implemented, partly unexercised | Shifts, logical operators and do-loops need their own activation before coverage claims |
| T2 storage/calls | Partly exercised | Single-TU stable globals, arrays/strings, aggregate copies, nonvolatile bitfields, canonical function addresses and scalar/pointer native callbacks pass the separate T2 receipt; Primitive variadics and bounded self-contained two-unit execution now pass separate receipts; TLS, dynamic initialization and hidden/native aggregate ABI remain open |
| Broader scalar/C++ forms | Unsupported or unexercised | Primitive double behavior and selected effectful sequencing are exercised by later fixtures; full floating/extended values, reference adaptation, cleanup/EH, inheritance and richer initialization remain open |

## T0 focused evidence and open work

The real `storage.c` object proves current `sc_extern` imports separately from
tentative, redeclared, static and initialized definitions. Its console suite
passes deterministic reports, unsupported target/duplicate-artifact diagnostics,
unresolved-import publication failure and cancellation. The fresh `cleanup.cpp`
object proves exactly two actual constructor-call operations, metadata catch
parameter recipes excluded from normal roots, required exceptional destructor
dependencies/actions and the actual `may_throw` import. Same-entity independent
metadata-association/context promotion is not activated by this fixture.

Focused logs are `t0/{storage-tests-final,cleanup-tests-4}`. Two preliminary
SQLite runs were cancelled after review found context classification fixes;
their reports explicitly retain `InventoryComplete=false` and
`TranslationComplete=false` with ROC0002. The old tableless reader-development
artifact was rejected by exact contract admission; it was never modified or
used as accepted evidence. The corrected full pass from `bcb86a08e` now closes
T0: 97.00 seconds, zero diagnostics, all 4,212 executable bodies inventoried,
2,226 required string values and 103 required imports/intrinsics. See
[the inventory report](SQLITE-INVENTORY.md).

## Bounded T2 storage and callback execution

[The checkpoint report](../acceptance/storage-callbacks/report.json) records all
46 phases, frozen source/build/input/provider hashes, deterministic output,
physical storage observations and callback event order. Native/JIT/NativeAOT
results are 147423/45/751 for storage/copy/callback fixtures. The separate
[T1 regression](../acceptance/storage-callbacks/t1-regression.json) passes all
23 phases after these emitter/runtime changes. At that checkpoint typed expression planning and
SysV cursors were compiled baselines; subsequent primitive variadic acceptance is
recorded below. Later sequencing/program receipts establish their bounded integration. This original checkpoint left T2 open; the later bounded gate is now closed. See the explicit limits in
[T2-STORAGE-CALLBACKS.md](T2-STORAGE-CALLBACKS.md).

The separate pinned native SQLite oracle passes 43 SQL statements across core
values/transactions, JSON/JSONB, FTS3/4/5, RTree/Geopoly and selected inspection,
math and STAT4 observations. Two runs produce identical typed observations;
[the native-only receipt](../acceptance/sqlite/native-sql-smoke.json) records
all providers and leaves the broader T4 API/persistence/concurrency/failure rows
open. This is future comparison evidence, not generated execution.

[Production manifest bindings](MANIFEST-BINDINGS.md) are exercised through the
public CLI with the same genuine callback fixture: nine successful phases,
four expected configuration rejections, JIT/NativeAOT751 and exact seven events.
Provider path/hash verification and complete declared contracts are distinct from
this fixture's exercised native ABI. External data/intrinsic provider execution
and the complete SQLite import audit remain open.

[Primitive variadics](T2-VARIADICS.md) now pass 62 phases: direct 1870,
interleaved 314, addressed 216 and unknown outbound 627 match native in JIT and
NativeAOT. Physical cursor/native borrowed entry/canonical identity and all ten
outbound value pairs are exercised. The subsequently exercised effective/raw typeref guard rejects unsupported
alignment. Typed effect planning and bounded multi-unit execution pass the later
receipts below; the original aggregate fixture
is preserved as an explicit producer/native differential, not forced to GCC.

[Typed sequencing](T2-SEQUENCING.md) passes 54 expected outcomes with exact
41/15/18/248 native/JIT/NativeAOT traces and exercised effective-alignment
rejection. [Two-unit execution and production CLI](T2-PROGRAM.md) pass 51 expected
outcomes, C 1150 and C++ 1212 with complete observations in direct/CLI JIT/AOT.
Canonical selected identities, one shared external allocation, separate internal
names and address identity are exercised. Native aggregate boundaries and narrow
readability improvements now pass separately; later unsupported forms remain explicit.

The [initial readability review](READABILITY.md) passes the relevant 54-outcome
sequencing revalidation. Only branches changes (six fewer lines, 407 fewer bytes),
with direct discarded local increment and typed Boolean seeds. Other places and
value-used effects retain full plans. [Aggregate output](T2-AGGREGATE-ABI.md) retains
ordinary application record methods with explicit ABI forwarding helpers.

The native aggregate receipt passes 97 expected outcomes: 94 successful phases
and three expected unsupported-input rejections. Six independent layouts match
native in JIT/NativeAOT with all observations, canonical callback identities,
physical size/offset/alignment/native-byte checks and deterministic output.
The relevant existing addressed-variadic regression also preserves all accepted
JIT/AOT observations after the shared native-ABI hook changes.


T3 diagnostic emission now reaches all 4,212 full SQLite bodies with zero emitter
diagnostics. This is emission evidence only: the first full C# build has 169
projection errors, and generated SQL execution and production publication remain
open. See [the exact progress and compiler evidence](T3-PROGRESS.md).


T3 now closes with the unchanged full-feature object published through the
production CLI and executed in fresh JIT/NativeAOT processes. The
[19-phase production proof](../acceptance/sqlite/generated-production-report.json)
validates real completion/request/public requirements, exact 46 initial statements
across two modes, four hashes identical to the independent diagnostic emission,
source/input freezes and reader-independent dependencies. See
[the detailed execution boundary](SQLITE-EXECUTION.md); T4 is not complete.
