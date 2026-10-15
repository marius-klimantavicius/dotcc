# Implementation restart state

Updated 2026-10-15. Approved plan commit: `e40280bea`.

Earlier checkpoint `59d06b51d` uses exact snapshot 4 product files.
Subsequent immutable source snapshots 6–9, build receipts and fixtures are under
`/mnt/storage/roslynobj-csharp-20261015/t3/value-storage`.

The working source now has passing bounded native/JIT/NativeAOT evidence for
natural-width pointer compounds and shifts, signed32/binary64 volatile memory,
four direct static strings and one zero static record, and typed scalar `va_arg`.
The last-four fixture passes all 13 observations and six events twice in JIT/AOT:
`t3/last-four-reproduction/last-four-receipt.json`. Width proof has 23 expected
phase outcomes at `t3/memory-widths-reproduction/memory-widths-receipt.json`.
These extend the accepted checkpoint; they do not establish SQLite execution.
A normal loader regression includes clearing the cached native TLS pointer before
release; destructor reentry remains unexercised.

Full SQLite pass 4 has four first blockers after 346.96 seconds: three uses of a
top-level volatile formal hit a representation classifier, and one function needs
typed `va_arg`. Public facts are `t3/sqlite-operator-facts-4.json`; all 38 actual
`va_arg` identities are prvalues using the already admitted scalar cursor.
The classifier now inspects representation without fetching a value; execution
and native-boundary volatile guards remain. The typed plan captures its cursor
once and reuses the existing scalar adapter.

Full-object pass 5 completed from frozen snapshot 8: zero emitter errors, all
4,212 bodies and 486 records, 344.39 seconds. It retains the exact four-file
unpublished `t3/sqlite-emission-5-candidate`. First measured build took 22.65
seconds, compiled native ABI C and reported 169 C# errors: pointer constant and
global-address view casts, mixed-width assignments/compounds, and one ref-place projection.
No budget violation. Exact diagnostics are
`t3/sqlite-emission-5-compiler-diagnostics.json`, logs/receipt under
`t3/sqlite-emission-5-build`. All 23 harness API surfaces validate. The next
critical work is emitter correction followed by a new immutable generation/build.
No candidate file may be hand-edited to establish product acceptance.

The generated SQLite harness and explicit candidate manifest tooling are ready
under `tests/SqliteGenerated` and `acceptance/sqlite`. Their C# has not yet been
compiled against a buildable candidate. The preparation manifest is under
`t3/sqlite-generated-preparation`; it preserves all 88 explicit native binding
identities and leaves 15 intrinsics to the recorded runtime lowerings. All 88
bindings remain candidates, not accepted whole-provider coverage. T3/T4 generated
SQL execution has not run.

Latest committed checkpoint is `40a0803c8`: complete diagnostic emission and
focused width/cursor/static proofs, with first compiler failure preserved.
Subsequent pointer fixes pass native/JIT/NativeAOT result 57 and all five exact
observations, including canonical void*/routine-pointer identity and native
wrapper calls. Receipt is `t3/native-constants-receipt.json`, source snapshot 10.
These changes are not yet committed with the next compiler slice.

Ownership: coordinator integrates, schedules all builds/exports and owns Git.
The inventory worker implements mixed integer compound conversions and the
conditional aggregate-prvalue assignment. Its public facts are
`t3/compiler5-operator-facts.json` (46 exact bodies). Stock EDG
`il.c:18042–18107` selects the already-converted RHS type for ordinary compounds;
no usual-arithmetic analysis is repeated. The emission worker's pointer constant
and address-view fixes are frozen pending integration. Full SQLite pass 6 is running from immutable snapshot 11 with a 900-second/8-GiB
budget, report `t3/emission-diagnostics-6-report.json`, candidate
`t3/sqlite-emission-6-candidate`. Its immediate next stage is attributable compile. Root supervises read-only. Preserve all failed attempts
and immutable snapshots. No new full T0 inventory is needed.

## Earlier checkpoint history

The coordinator owns shared contracts, integration, serialized build/export
scheduling, exact-path Git mutations and this ledger. Root supervises read-only.
Two `gpt-6.1-sol` xhigh implementers have disjoint ownership:

- `inventory`: bounded intrinsic receipt passes. Next: actual pointer compounds,
  shifts, planned varargs and volatile memory execution, using its ready narrow
  20-body OperatorFacts probe. Coordinate core type hooks through coordinator.
- `emission`: existing Emission partials, runtime/host, Projection, T2 fixtures
  and execution drivers. The aggregate gate passes; next target is production
  Linux x64 provider ABI/error/TLS/callback proof passes. Next: ordinary static
  locals and expression constants in Storage.cs plus remaining loader/provider
  needs. Coordinator integrates small core/SelectedStorage hooks. Do not edit
  the intrinsic worker's planner/new intrinsic files.

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
and each ordinary shape are separately gated. The final frozen aggregate receipt
passes 97 expected outcomes: six layouts native/JIT/AOT 36/94/76/161/364/11,
all observations, twice-run physical/canonical checks, deterministic emission,
exact ABI-only exports/copies and three expected unsupported-input rejections.
Report SHA256 is
`9ed9392f61c17f1b779c922c27ea351900d5cd9e70d5e26e1a0de7c5f70e7f1b`.
The relevant addressed-varargs JIT/AOT regression preserves 216 and all accepted
physical/canonical/borrowed-cursor observations. Its harness-only phase-directory
and missing checker-provider-copy mistakes are preserved in
`t2/aggregate-variadic-regression{,-2}`. See [T2-AGGREGATE-ABI.md](T2-AGGREGATE-ABI.md).
Root accepted the bounded aggregate gate and directed continued production T3.
T2's fixture gate is closed with explicit limits in [T2-REMAINING.md](T2-REMAINING.md).

Coordinator preparation resolves all 88 candidate system exports while retaining
15 builtins as implementation requirements. Exact aliases/signatures, provider
file hashes and proposed per-function contracts are under
`t3/provider-preparation-3/provider-plan.json`. `time`/`gettimeofday` resolve to
kernel vDSO images, explicitly not hash-pinned files. No production binding is
enabled. First finite HostContracts native/JIT/AOT probe passes 58 checks each
under `t3/host-contracts-first`: baseline later errno happens to remain 20;
native protected scope observes deliberate callback clobber to 4 then restores
20, distinct foreign-thread TLS, nested managed callbacks and native mutex return
codes. This is preparation, not a production error-state implementation; see
[SQLITE-PROVIDERS.md](SQLITE-PROVIDERS.md). The formal receipt now passes 22
phases and four JIT/AOT runs with all 58 checks; source/toolchain/provider hashes
are committed at `acceptance/host-contracts/host-contracts-receipt.json`.
The first formal attempt stopped at missing unversioned clang after passing JIT;
the successful fresh attempt explicitly uses installed clang-18. Intrinsic
worker is now preparing actual public call facts and lowering the 15 required
builtins; no more broad host-probe matrix is needed.

Preserve the original aggregate producer/native differential at
`acceptance/semantic-differentials/aggregate.json` and its exact public trace:
retained roots predict 248, GCC observes 600; initial 430 prediction was wrong.
Never force GCC's result over retained execution. The separate explicit named
snapshot is the accepted 248 input. No process remains active at checkpoint.

Genuine fixture objects/native oracles remain under `t2/export`, `t2/native`,
`t2/assembly-export`, `t2/sequencing` and `t2/program-execution`. Preserve all
artifacts, including failed/incomplete attempts; no cleanup is authorized.
The first full-object diagnostic pass reports 596 first blockers; the second,
with the explicit logical errno profile and bounded intrinsics, reports 313 in
298.35 seconds, no output publication or budget violation. The reports and
current exact ownership are in [T3-PROGRESS.md](T3-PROGRESS.md). Counts are not
exhaustive unsupported families or a completion percentage. All 88 host bindings
remain candidates; the bounded provider fixture does not prove every contract.

Current checkpoint has accepted native/JIT/AOT statements 23/all 23 events,
intrinsics 34/all 34 plus 1000 exact handoffs (27 expected outcomes), and errno/
qsort/pthread 6223/all 24 with forced raw interference (17 phases). Frozen
receipts are under `acceptance/{statements,intrinsics,errno-provider}`. Product
sources remained held through these receipts; no additional baseline rerun is
needed absent relevant changes. Public CLI intrinsic fulfillment was subsequently verified; see current state above.

Coordinator next: flexible-tail address representation, value-only unsigned128
with guarded addressable alignment, CLI intrinsic fulfillment and checkpoint
integration. Reader-only StorageFacts reports all 132 constant static recipes,
26 actual SQLite flexible tails and the two actual unsigned128 locals. Preserve
`t3/statement-snapshot` and all incomplete attempts. No build/process remains
running at this checkpoint. SQLite T3/T4 SQL execution has not run.


Latest execution evidence: full pass 6 candidate builds with zero C# errors and
passes all 19 initial SQL phases in JIT/NativeAOT, receipt
`t3/sqlite-generated-candidate-run-2/generated-sqlite-report.json` SHA
`8609110735c363217ad59a3c5194009cacafdafbaba181991c65e54cf6f143de`.
Attempt 1 is preserved: both JIT modes passed; harness-only generic JsonArray Add
calls blocked AOT. Attempt 2 corrects those overloads and all-thread memory sampling.
The actual generated application source remains unchanged.
Production CLI translation is now running from snapshot 11:
`t3/sqlite-production-cli-translate-2`, manifest
`t3/sqlite-production-preparation-1/manifest.json`, output
`t3/sqlite-production-generated-1`. The immediate wrong-DLL-path invocation is
preserved as a tooling attempt, not a translator failure. Source publication and
production execution verification are next, followed by remaining T4.
Emission worker owns explicit production validation mode in harness metadata;
inventory worker owns NEW finite stateful WAL/locking/interchange workload and
native oracle paths. No product source changes are in flight.


T3 is now CLOSED: production CLI translation/publication and fresh JIT/NativeAOT
bind/reset/clear plus 43 SQL statements pass. Production report is
`t3/sqlite-generated-production-run-1/generated-sqlite-report.json` SHA
`dafaa011dae26c81d3c9f0b6ac7f441e848696873ad9ce1f0f86778a695e7c9a`.
All 19 phases/source freezes pass; all four emitted files match diagnostic pass 6.
Compact audit is `acceptance/sqlite/production-audit.json`; broader T4 remains open.
No builder/translation process is running. Worker inventory implements the NEW
Stateful C# counterpart; finite native40/13/5 modes pass in `t4/native-stateful-2`,
`native-append-1`, `native-verify-1`. Attempt1 sidecar guard failure remains
preserved; final verification uses readwrite open without create, no deletion.
Emission worker production validator/driver is frozen pending checkpoint.
Next: compile generated stateful counterpart against actual production output,
run JIT/AOT snapshot/BUSY/persistence and both closed-file interchange directions.


T4 stateful persistence slice now passes all 15 phases: JIT/NativeAOT each match
40 native snapshot/BUSY/rollback/checkpoint actions, plus both closed-file
interchange chains (13 append / 5 verify actions). Compact evidence is
`acceptance/sqlite/stateful-report.json`; broader simultaneous cross-process
locking, ownership/error/failure and enabled-feature rows remain open.
Emission worker owns NEW SqliteApiCallbacks harness/validator; native oracle
passes destructor/preupdate/session/conflict/backup observations. Coordinator
is building its frozen counterpart against unchanged production output.
Inventory worker owns NEW SqliteRestart harness/driver. Coordinator added the
minimal synchronous TranslationPhase observer overload, preserved existing
Translate overload, and built snapshot 12 successfully. Queue full-object
BeforePublication cancellation then fresh-process restart from identical input
and request, preserving incomplete staging and already-published production
output. No partial-stage reuse is claimed. Runtime stays frozen snapshot 11.

The API callback slice now passes native/JIT/NativeAOT: destructor counts and
owner IDs, preupdate events/unregister, session changesets/apply/conflicts and
incremental backup match exactly. Five phases and immutable source/API/provider
identities are in `acceptance/sqlite/api-callbacks-report.json`. First metadata
path mismatch is preserved. AOT explicitly pins clang 18. Emission worker now
prepares NEW SqliteFeatureMatrix SQL workload; frozen API sources remain held.
