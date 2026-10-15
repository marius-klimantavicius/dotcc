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

T1 final reproduction passes all 23 phases with exact source/build hashes,
deterministic repeat and public layout/template proof. Its immutable report is
committed at `acceptance/t1/report.json`; root independently reviewed and accepted
this first fixture gate. No more T1 reruns are needed absent relevant changes.

The CLI/manifest and focused storage/cleanup-context tests pass. Two earlier
SQLite attempts were gracefully cancelled for inventory-context corrections;
their incomplete reports remain preserved. All T0/T1 source is now frozen.
Next: build the CLI from this snapshot and complete the SQLite inventory under
the pinned manifest budgets, then audit imports/forms to order T2 work. T0
remains open; SQLite translation/execution is unimplemented.

The emitter implementer may draft NEW T2 C/C++ fixtures/native oracle sources
without changing existing source/project files. Root reviewed the planned
canonical-address wrappers, managed internal indirect calls, storage/fixups and
SysV variadic packs. Incoming variadic native boundaries may use concrete
fixed-prefix C wrappers limited to `va_*` capture and synchronous forwarding.
All such capabilities require genuine JIT/NativeAOT evidence before acceptance.
