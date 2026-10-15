# Translation coverage and evidence

Updated 2026-10-15. The design at `e40280bea` is approved. Implementation starts
from the public RoslynObj 2.0 reader (with its exact published 1.0 admission).
EDG is the semantic authority. No prior dotobj implementation or evidence is used.

| Gate | Current state | Evidence | Remaining requirements |
| --- | --- | --- | --- |
| T0 | Closed for contract/inventory | [Complete SQLite inventory](SQLITE-INVENTORY.md); 4,212 bodies/592,512 body operations; 103 required imports; genuine storage/cleanup/address closure tests | All required executable providers/projections remain T2/T3 work |
| T1 | Closed for first fixture slice | [Reproduced report](../acceptance/t1/report.json): 23 phases pass; native/JIT/NativeAOT 66/38; deterministic repeat; public layout/template and source review | Broader forms remain explicit below and under T2/T5 |
| T2 | In progress | [Bounded assembly selection](T2-ASSEMBLY.md) passes 16 genuine inputs; storage physical JIT/NativeAOT slice passes pending final receipt | Integrated multi-unit emitted execution, callbacks, varargs, complete storage/ABI fixtures and provider contracts |
| T3 | Not implemented | Existing full-feature object is input evidence only | Complete SQLite emission, imports/providers, first SQL in JIT and NativeAOT |
| T4 | Not implemented | None | All configured workload rows, differential execution, resource/restart and readability evidence |
| T5 | Not implemented | None | Broader C++ lifetime/EH/dispatch/RTTI/initialization/TLS fixtures |
| T6 | Not implemented | None | Additional ABIs/platforms and bounded multi-unit scale with dependency closure |

Capability rows will record captured form, projection/provider, activation,
execution evidence and limits. Inventory occurrence counts establish what is
present; they do not establish implemented execution support.

The pinned SQLite input is immutable:
`/mnt/storage/roslynobj-sqlite-20261014/export/sqlite3.roslynobj`, SHA256
`e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c`.
Generated outputs and long logs use `/mnt/storage/roslynobj-csharp-20261015`.
No generated application currently loads native SQLite, and no SQLite execution
has been claimed.

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
| T2 storage/calls | Unsupported | Globals/static/TLS, arrays/strings, aggregate copies, bitfields, function addresses/callbacks, native/hidden-ABI calls, variadics, multi-unit linking |
| Broader scalar/C++ forms | Unsupported | Floating/extended values, reference adaptation, cleanup/EH, inheritance, richer initialization and effectful recorded right-to-left sequencing |

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
