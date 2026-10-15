# Implementation restart state

Updated 2026-10-15. Approved plan commit: `e40280bea`.

The coordinator owns shared contracts, integration, serialized build/export
scheduling, exact-path Git mutations and this ledger. Root supervises read-only.
Two `gpt-6.1-sol` xhigh implementers have disjoint ownership:

- `inventory`: the NEW typed planner files
  `Emission/ExpressionPlan.cs`, `Emission/CSharpEmission.ExpressionPlans.cs`,
  NEW `Emission/CSharpEmission.Program.cs`, plus sequencing/program fixture
  and checker paths. Completed inventory/semantic assembly files remain stable.
- `emission`: existing Emission partials, runtime/host, Projection, T2 fixtures
  and execution drivers. Next target: genuine SysV primitive variadics and
  addressed variadic ABI adapters.

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

T2 bounded semantic assembly was verified and committed at
`60e24dddbbef36cb6fd425d89733fa70ad47e22a`: 16 genuine inputs, C/C++ native
oracles and explicit unsupported link-policy diagnostics. This does not yet
provide generated cross-unit execution. See [T2-ASSEMBLY.md](T2-ASSEMBLY.md).
All builds/exports remain coordinator-scheduled. Reviewed representation and ABI
contracts are in [T2-CONTRACT.md](T2-CONTRACT.md).

The coordinator's `acceptance/sqlite/prepare-native.py` built the separate
full-feature native oracle under `sqlite/native-oracle`; its identity/recipe/
build report is committed at `acceptance/sqlite/native-oracle-build.json`.
The build receipt remains build-only. The separate native-only SQL runner now
passes 43 statements twice with identical typed observations; its receipt is
`acceptance/sqlite/native-sql-smoke.json`. This baseline does not cover all T4
rows or prove generated execution. The oracle cannot be a generated-runtime
provider.

T2 storage/callback checkpoint passes all 46 phases at
`t2/storage-callback-reproduction/t2-storage-callbacks-report.json`; a copy is
committed at `acceptance/storage-callbacks/report.json`. Storage 147423,
aggregate copy 45 and callbacks 751 match native in JIT and NativeAOT. All 32
storage observations, real physical layout/alignment checks and exact seven
callback events pass in both modes. Repeated generated files are identical;
projects depend only on Generated and Runtime. The relevant T1 regression passes
all 23 phases at `t2/t1-regression-storage-callbacks/t1-report.json`, also copied
beside the checkpoint report. See [T2-STORAGE-CALLBACKS.md](T2-STORAGE-CALLBACKS.md).

Production CLI native-function bindings now pass the same genuine callback input
via manifest/translate in a sparse isolated snapshot (`t2/manifest-bindings/verified-snapshot`).
The final receipt is `acceptance/manifest-bindings/report.json`: nine successful
phases plus four expected fail-closed configuration rejections, JIT/AOT751 and
exact seven events. Bindings carry explicit provider path/hash and complete
captured-signature/convention/ownership/error/thread/callback contracts; required
data/intrinsic providers remain unsupported. See [MANIFEST-BINDINGS.md](MANIFEST-BINDINGS.md).

Typed sequencing now passes its frozen 54-outcome receipt at
`t2/sequencing-reproduction/t2-sequencing-report.json`, copied to
`acceptance/sequencing/report.json`. All four native/JIT/NativeAOT results
41/15/18/248 and every event/value match, each generated checker twice. The
effective typeref 4/16 versus unwrapped 4/4 fixture rejects before output.
Root accepted this bounded gate; ordinary callback regression passes 751 and
seven exact events. See [T2-SEQUENCING.md](T2-SEQUENCING.md).

Two-unit direct and production CLI execution now pass the frozen 51-outcome
receipt at `t2/program-reproduction-2/t2-program-report.json`, copied to
`acceptance/program/report.json`. C 1150/all ten observations and C++ 1212/all
eight match native in direct and CLI JIT/NativeAOT checkers, twice each. Exact
canonical allocation/address selection, deterministic emission, completion
hashes, budget rejection and dependencies pass. First `program-reproduction`
remains incomplete from a harness artifact-path assumption, corrected without
semantic changes. Root accepted this bounded self-contained scalar/pointer
slice. See [T2-PROGRAM.md](T2-PROGRAM.md).

Primitive variadic evidence remains `acceptance/variadics/report.json` (62 phases,
1870/314/216/627 plus physical/native entry and complete outbound observations).
The formerly pending effective/raw guard is now implemented and exercised by
the rejection above. No additional full T0 inventory repeat was needed.

Both workers were completely frozen through the integration receipts. After
this checkpoint, emitter owns pure native aggregate ABI mirrors/wrappers and
new aggregate fixtures; inventory worker supports planner integration. The narrow
readability improvement now passes all 54 expected outcomes at
`t2/sequencing-readability`, copied to `acceptance/readability/report.json`.
Only branches changes (161 to 155 lines); other accepted C# examples are
byte-identical, with all JIT/AOT observations preserved. Runtime has not changed
since the primitive varargs checkpoint. Aggregate fixtures under `tests/Fixtures/T2/aggregate-abi` are now exported and
native-verified under `t2/aggregate-abi`; public probes show ordinary signatures,
no explicit ReturnSlot/hidden/copy parameters, including memory return 24/8.
Host hidden sret remains a native wrapper detail. Generated execution and
physical ABI proof are still required. Packing/unions
and each ordinary shape remain separately gated. See [T2-REMAINING.md](T2-REMAINING.md).

Preserve the original aggregate producer/native differential at
`acceptance/semantic-differentials/aggregate.json` and its exact public trace:
retained roots predict 248, GCC observes 600; initial 430 prediction was wrong.
Never force GCC's result over retained execution. The separate explicit named
snapshot is the accepted 248 input. No process remains active at checkpoint.

Genuine fixture objects/native oracles remain under `t2/export`, `t2/native`,
`t2/assembly-export`, `t2/sequencing` and `t2/program-execution`. Preserve all
artifacts, including failed/incomplete attempts; no cleanup is authorized.
T2 remains open for native aggregate boundary/readability; SQLite T3/T4 has not run.
