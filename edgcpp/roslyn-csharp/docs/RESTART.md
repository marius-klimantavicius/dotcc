# Implementation restart state

Updated 2026-10-15. Approved plan commit: `e40280bea`.

The coordinator owns shared contracts, integration, serialized build/export
scheduling, exact-path Git mutations and this ledger. Root supervises read-only.
Two `gpt-6.1-sol` xhigh implementers have disjoint ownership:

- `inventory`: `src/RoslynObj.CSharp/` excluding `Emission/`,
  `src/RoslynObj.CSharp.Cli/`, and `tests/Inventory/`.
- `emission`: `src/RoslynObj.CSharp/Emission/`, runtime/host projects, and other
  `tests/` paths. Its first target is genuine C/C++ T1 fixtures.

The coordinator (`gpt-6.1-sol` high) owns root project configuration and docs.
Workers must request build/export scheduling and must not mutate Git.

Use .NET SDK 10.0.112/C# 14, unsafe output. The reader is
`../../roslyn/managed/RoslynObj`; generated programs reference runtime components
only, never the reader or its SQLite dependency. Access semantics only through
the public reader, never raw SQL, reflection or internal IDs.

The frozen producer is
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe`.
Reuse it through the actual Roslyn driver after inspecting its invocation. Do not
rebuild it or modify EDG core. Provenance and exact SQLite configuration are in
`/mnt/storage/roslynobj-sqlite-20261014/producer-provenance.json` and
`export/recipe.json`; current backend is `edgcpp/roslyn/backend/`.

Large artifacts go under `/mnt/storage/roslynobj-csharp-20261015`. Freeze budgets
before each measured acceptance run. The initial environment has 406 GiB free
there, 100 GiB workspace free, and about 98 GiB available RAM. No cleanup is
authorized without coordination with root.

Preserve all unrelated dirty/untracked user files. Never stage all. In
particular, leave the known untracked Roslyn pause/restart files and
`tests/fixtures/name_reference_overloads.cpp` alone. Ignore all dotobj and old
object-linker implementation, findings and evidence, including AGENTS links.

Genuine T1 objects and independent native oracles are now under `t1/export/` and
`t1/native-{c,cpp}`. The in-tree Projection Release build passes. Corrected fresh
generated projects under `t1/final/generated-{c,cpp}` compile and execute 66/38 in
JIT and NativeAOT, matching the native oracles. Final measured attempts are under
`t1/final/`; preliminary bootstrap outputs are separate retained artifacts.

T1 milestone commit: `b5a096226e0e521a85f01e2f262bcf4b192c4b22`.
T1 final reproduction passes all 23 phases with exact source/build hashes,
deterministic repeat and public layout/template proof. Its immutable report is
committed at `acceptance/t1/report.json`; root independently reviewed and accepted
this first fixture gate. No more T1 reruns are needed absent relevant changes.

T0's corrected full SQLite inventory now passes from commit
`bcb86a08e24d983cd654904c4833f46c62d15c87` in the sparse detached checkout
`/mnt/storage/roslynobj-csharp-20261015/t0/verified-snapshot`. Its immutable report
and build hashes are under `t0/sqlite-inventory-address-closure`; root accepted
T0. It inventories 4,212 executable bodies/592,512 body operations and 103
required external/intrinsic requirements, with no diagnostics. Constant-address
closure now reaches 2,226 required strings. Two earlier context-fix attempts are
cancelled/incomplete; the b5 pass is complete but superseded by this closure fix.
See [SQLITE-INVENTORY.md](SQLITE-INVENTORY.md) and the compact committed audit.
No additional full inventory repeat is needed absent semantic-dependency changes.

No inventory process is running now. The sparse frozen checkout preserves
authoritative T0 binaries independently of current T2 edits; keep it available.

Root authorized disjoint T2 implementation while that frozen binary runs.
The inventory worker owns NEW `ProgramAssembly*`/`AbiCompatibility*` files (or
`Assembly/`) plus `tests/Assembly`; the emitter worker owns its existing
Emission/runtime/host/Projection paths and NEW T2 fixtures. Existing T0
Inventory/Translator/Contracts/Translation files remain unchanged until
integration. All builds/exports remain coordinator-scheduled. T2 representation
directions and required evidence are recorded in [T2-CONTRACT.md](T2-CONTRACT.md).

The coordinator's `acceptance/sqlite/prepare-native.py` built the separate
full-feature native oracle under `sqlite/native-oracle`; its identity/recipe/
build report is committed at `acceptance/sqlite/native-oracle-build.json`.
This is build-only evidence, with no SQL execution claimed. The oracle cannot
be a generated-runtime provider.

T2 genuine fixture objects/native oracles are under `t2/export`, `t2/native` and
`t2/assembly-export`. First storage emission reached a compilable-projection
stage then exposed narrow compound-assignment emission; the worker has a focused
fix ready for next build. Native physical layout checks remain required. First
assembly harness passed initial strong/local/opaque/qualifier/link diagnostics
then exposed alias definitions without bodies; the corrected alias/ifunc logic
and three extra ABI/inline fixtures await the next scheduled build. No T2 gate
is closed. Next: complete these fixtures in JIT/NativeAOT and integrate selected
program identities; then implement callbacks and variadics before SQLite T3.
