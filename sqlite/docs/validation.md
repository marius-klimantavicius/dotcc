# Validation evidence

Campaign commands run from `sqlite/`; scripts resolve their own absolute roots.
Evidence is recorded in milestone order; later checkpoints supersede earlier
pending statements. Reproduction commands are in `usage.md`.

## Nested C-only output and cursor boundary (2026-09-12)

The product now uses `--nest-types --runtime=c`. Aggregates, runtime and helpers
live under `Managed.Database.Sqlite`; split-file aliases are local. One
`SqliteFunctionPointers` cache replaces the consecutive partial declarations,
and globals live in `SqliteGlobals`. Two copied translations compile together
without project references and preserve separate runtime/global state under JIT
and NativeAOT. Compiler suites (2,096 unit / 394 functional, 969 opt-in skips) and
65 postprocessor tests pass. Raw/postprocessed SQLite and its NativeAOT consumer
pass with zero product build warnings.

The priority offset audit found that upstream intentionally clears only the
32-byte BtCursor prefix before `pBt`; `.Value` is correct, while `.Size` describes
all 296 bytes. ManagedConsumer now verifies that every trailing byte survives.
All 41 native offset contracts and the actual size/alignment/field/storage tests
also pass under JIT and NativeAOT. Details and commands:
[output-isolation.md](output-isolation.md).

## Generic runtime endian overrides (2026-09-11)

The product now selects SQLite's original BIGENDIAN/LITTLEENDIAN pointer probes
using a generic JSON macro profile. They lower through `_Bool` typed IR to
`BitConverter.IsLittleEndian`, with BIGENDIAN negated. The generated engine has
26 BCL reads and no `sqlite3one` address reads. Upstream source is unchanged.

Raw and postprocessed ManagedConsumer pass with zero build warnings. Dedicated
UTF-16LE/BE database exchange passes against unmodified native SQLite under JIT
and NativeAOT; both byte orders, native UTF-16 APIs, non-ASCII/supplementary text,
WAL checkpoint/reopen and integrity are covered. The full host VFS independent
process campaign passes under JIT and NativeAOT, including forced-kill recovery,
stale/missing WAL indexes, native/managed reads and writes, and checkpoint stress.
Product layout checks pass: 41 offsetof contracts, 67 actual field offsets,
48 aggregate size/alignment checks and 8 inline-array storage checks.

Commands, generic compiler evidence and Linux x64 portability limits are in
[macro-overrides.md](macro-overrides.md). Profile and source dependencies are
recorded in `artifacts/engine.d`; selection/expansion trace is in
`artifacts/engine-overrides.jsonl`. The campaign now includes `test-endian.sh`.

## GNU bit-field profile migration (2026-09-11)

Shared compiler work for the unchanged picotls core repaired ordinary-member
bit-field tail reuse. Rechecking SQLite exposed its historical `-mms-bitfields`
oracle workaround and two remaining GNU leading-byte placement gaps. The generic
repair now uses one bit-position map for metadata and emitted accessors. Native
GNU reducer byte images, 28 focused units and 13 functional/layout cases pass.
Fresh actual SQLite JIT storage matches the complete native GNU transcript:
39 required metadata contracts, 42 aggregate sizes/alignment wrappers and eight
inline-array storage checks. Evidence is retained under
`artifacts/gnu-layout/{native.out,translated.out,fresh-storage-build.log}`.

`config/native-flags.txt` now explicitly selects `-mno-ms-bitfields` and
`tests/layout-native.expected` contains that fresh measured transcript. The
previous expected transcript is preserved at `tests/layout-native-ms.reference`;
`SQLITE_MS_BITFIELDS=1 scripts/layout-native.sh` retains the historical comparison.
No expected offsets were guessed or substituted to suppress a compiler mismatch.
The standard native and translated layout scripts now pass with this profile
under JIT and NativeAOT (`artifacts/campaign-layout.log`), and all eight native
corpus checks pass. The complete rebuilt repository passes 2017 units and 380
functional cases (967 opt-in oracle skips), plus 64 postprocessor and 31 analyzer
checks (one optional analyzer oracle skip). The complete final campaign passes with `SQLITE_AOT=1`: all eight native and
seven translated corpora, owning consumer, threaded product ABI (41 metadata
contracts, 48 actual aggregate layouts, 67 field offsets and eight arrays),
threading/restart/failure cleanup, host VFS independent-process/crash/WAL cases,
source/object function-pointer identity and all three image-exchange directions.
The applicable executables pass JIT and NativeAOT with no IL/trim/AOT warnings in
the current publish logs. The source preparation checks also pass.

Exact orchestration is saved at
`../picotls/artifacts/translation/sqlite-verify-with-postprocess.sh`: it runs the
unchanged verification sequence and adds the two postprocessor suites after the
repository build/tests. Final logs are `artifacts/campaign-*.log` and
`../picotls/artifacts/translation/sqlite-final-regression.log` (exit zero).
Eleven pre-manifest compiler-generated `Program.cs` files were preserved outside
regenerated project directories to avoid stale SDK-glob duplicate definitions;
paths and SHA-256 hashes are recorded in
`../picotls/artifacts/sqlite-stale-generated/preserved-files.json`. No generated
contents or upstream sources were patched. Earlier MS-profile results below
remain historical evidence.

- `dotnet build ../dotcc.sln -c Release`: passed, zero warnings/errors, 9 seconds.
- Baseline unit suite: **1,723 passed, zero failed/skipped**, 35 seconds, with isolated TMPDIR.
  The first default `/tmp` run was stopped after eight minutes without completion;
  compiler include discovery repeatedly traversed an unrelated 18 GB temporary tree.
  `scripts/test-repository.sh` isolates TMPDIR for repeatable test timing.
- Baseline functional suite: **224 passed, zero failed, 819 opt-in skips**, 1 minute
  16 seconds, with isolated TMPDIR. Skips include external GCC/Zig/WAT oracle tests;
  they are not counted as executed translated tests.
- `scripts/native.sh`: 36-case deterministic core/JSON/JSONB native corpus completes;
  expected transcript checked in at `tests/native-corpus.expected`. Translated comparison pending.
- VFS worker strict GCC native suite passes; ASan+UBSan suite passes. Reproduce with
  `scripts/test-vfs-native.sh` and `SQLITE_SANITIZE=1 scripts/test-vfs-native.sh`.
  Covers direct I/O/locking/failure contracts, journal cleanup, transaction recovery,
  reopen/read-only, WAL fallback, JSONB, image export/import and resource cleanup.
- `python3 scripts/fetch.py`: archive SHA-256 verified; upstream source unchanged.
- `scripts/test-api-native.sh`: native API corpus passes; matching full-source
  ASan+UBSan execution passes. Transcript at `tests/native-api.expected`.
  Covers prepare/bind/reset/finalize, embedded NULs, destructor ownership, UTF16,
  scalar/aggregate/collation callbacks, busy/progress/transaction hooks, backup,
  incremental blobs, extended errors and 256 deterministic random mutations
  compared with an independent C model (seed `0x51a17e`).
- `scripts/preprocess.sh > artifacts/sqlite3.i`: passed.
- `scripts/translate.sh`: failed at nested callback declarator; see B001.
- GCC C17 reduced nested-callback fixture: expected output `7` confirmed.

Generated artifacts/logs are ignored under `generated/`, `build/`, `artifacts/`.
No translated SQLite execution or corpus pass is claimed yet.

## First integrated compiler increment

Commits `4db77b5`, `c679ee4`, `0071282`:

- Release solution build: zero warnings/errors, 8 seconds.
- Full unit suite: 1,740 passed, zero failed, 41 seconds.
- Full functional suite: 239 passed, zero failed, 833 opt-in skips, 75 seconds.
  The not-yet-implemented managed-library regression was deliberately excluded;
  it remains uncommitted until its API implementation lands.
- All new parser/macro/layout fixtures have native GCC expected-output evidence.
- Full SQLite retry advances to the nested flexible array B005. Preprocessing now
  registers exactly one `->` and one `->>` with no corrupted spaced operator.
- Raw logs: `artifacts/stringify-*-integrated.log`, earlier offset logs under
  `artifacts/offsetof/`. Generator actual storage/native fixture is documented
  in `generators/README.md`. Full SQLite execution remains pending.

## Public JSONB tests

`scripts/test-upstream-native.sh` passes all 37 adapted SQL assertions and setup
from the matching public `jsonb01.test`, retaining upstream expected bytes and
error behavior. `tests/upstream-jsonb.expected` records the native transcript;
`docs/upstream-tests.md` documents the adaptation and coverage boundary.
Translated execution remains pending.

## Native ABI and single-unit adapter validation

All native core, API, VFS and upstream JSONB suites pass with the explicit matching
`-mms-bitfields` profile. Core/API/upstream transcripts remain byte-for-byte equal
to their saved expected results. Actual SQLite layout probes and raw default-GCC
comparison are recorded in `tests/layout-native.expected` and
`tests/layout-native-sysv.reference`.

The one-unit layout probe exposed our adapter's private `MemFile` name collision
with SQLite's built-in memdb VFS. Adapter types and private symbols are now
prefixed `DotccMem`/`dotcc_mem_`; native VFS contracts pass after the rename.
SQLite source remains untouched. Probe logs live under `artifacts/layout-native-*`
and profile rerun logs under `artifacts/native-matched-*`.

## Tagged definitions and flexible tails

Commit `c32dd74` adds the tagged-definition variable regression and updates the
old flexible-tail assertion. Functional suite: 242 passed, 839 opt-in skips,
zero failures, 76 seconds. The updated FAM assertion and 55 focused macro/unit
tests pass. Full SQLite advances from the definition of `sqlite3StatType` to the
GLOBAL/vfsList rescan issue B007. Managed-library work was tested separately.

## Explicit virtual-table module contract

`scripts/test-vtable-native.sh` passes against the matching native profile;
`SQLITE_SANITIZE=1 scripts/test-vtable-native.sh` also passes ASan and UBSan.
The portable C test registers its module through `sqlite3_create_module_v2`;
it covers table/cursor callbacks, index constraints, numeric affinity, NULLs,
rowids, JSON joins, rejection of writes, absent FTS5, and module/context cleanup.
It preserves a virtual-table extension seam for future FTS while FTS remains
disabled. `tests/native-vtable.expected` records the transcript. This is native
oracle coverage; translated execution and a C# SQLite module remain pending.

## Bounded allocator failure contract

`scripts/test-allocation-native.sh` and its `SQLITE_SANITIZE=1` ASan/UBSan run
pass 128 deterministic one-shot allocator failures. Sixty-four target
prepare/execute through `sqlite3_exec`; another 64 begin after preparing a
statement, targeting VM execution, recursive inserts, and JSONB construction.
The test disables connection lookaside to route these requests through SQLite's
configured allocator callbacks. Every selected failure returns `SQLITE_NOMEM`,
then rollback when needed, integrity checks, new JSONB writes/reads, handle
cleanup, and shutdown memory accounting succeed. The allocator is restored.
This bounded fault corpus does not claim exhaustive OOM coverage. Its portable C
and `tests/native-allocation.expected` will also drive translated validation.

## Closed database-image transport

`scripts/test-image-native.sh` writes a closed VFS database image to
`artifacts/native-exchange.db`, then a separate process imports and validates it.
The portable driver accepts `write PATH` and `read PATH`, ready to exchange
images in both directions between native and translated executables. The native
round trip passes integrity checks, JSONB with Unicode/embedded NUL, a 64KiB
overflow-page blob with an incremental write, user metadata, and cleanup.
Host `fopen`/read/write calls only transport the closed image; SQLite itself
continues to use the memory VFS. Cross-engine exchange remains pending.

## Managed library and explicit callback API

The managed library increment passes a clean Release solution build (zero
warnings/errors, 6.88 seconds) and seven focused managed/native-library tests.
Separate library/consumer assemblies cover source and object-link output,
public callback structs/inline arrays/enums/globals, static callback table
initialization, context pointers, GC stress, cleanup, and rejected native binds.
An existing native-library test now supplies its framework references explicitly
so it also passes in isolation without depending on other test execution order.

Actual CLI `--emit=managedlib -c --offset-generator ...` builds the generated
library with the analyzer (zero warnings/errors). A separate project-reference
consumer prints `42 8` under both JIT and NativeAOT: the managed callback result
and generated offset. Both `-shared`/`--shared` conflicts and `-lexample` are
rejected with exit code 2. Logs/projects: `artifacts/managed-api/`.
This validates the compiler/library/generator seam, not a translated SQLite
engine. The actual SQLite library, C# extension, and engine AOT test remain open.
See `docs/managed-api.md` for usage and ownership rules.

## Macro rescan, register, and managed integration

Commits `fba0378`, `ba0b0e2`, and `c630f7d` pass the coherent full snapshot:
1,742 unit tests (38 seconds), 249 functional tests with 843 opt-in skips
(77 seconds), zero failures. This includes all managed/native-library tests,
the macro invocation-boundary correction, and register locals/for initializers.
The next array-leading and physical-source-mapping tests were added afterward
as separate reduced failures and are not claimed by these counts.
Full SQLite retry reaches B009 in 0.80 seconds with 123632 KiB peak RSS.

## Every active offsetof request

`scripts/layout-native.sh` now preprocesses the entire pinned profile, extracts
every active `offsetof` request, and generates a C include containing probes.
There are 30 distinct aggregate/member requests. Unexpected designator syntax
fails explicitly; the generator contains no offset values. Native C computes
size/alignment, constant offset, and actual address difference for each request.
All 30 agree under the matching profile, alongside the existing representative
public/JSON/bit-field/FAM probes. Default-GCC comparison is retained separately.
The wrapper produces exactly the same transcript as the captured preprocessor
input; both updated transcripts are committed. The corresponding translated
layout comparison remains pending full lowering/emission.

## Array declarations, conditional commas, physical source mapping

Commits `6ae3c5f`, `b8c11e1`, and `50308f9`: clean build, 1,750 unit tests
passed (37 seconds), 251 functional tests passed with 847 opt-in skips (78
seconds), zero failures. Native-verified expression tests check selected and
unselected conditional side effects, nested ternaries, mixed array declarations,
and pointer typedefs. Eight physical-position tests cover continuation splicing,
LF/CRLF, UTF-8 byte offsets, includes, builtins, and macro invocation positions.
The complete SQLite retry now reports physical `139841:10`, byte `5075023`,
at a pointer-to-function-pointer field (B011). Source mapping limitations are
recorded in `docs/source-mapping.md`.

## Callback arrays and inline aggregate initialization

Commits `3d7d65e`, `04555ea`, and `037a1ab`: build with zero warnings/errors
(7.59 seconds), 1,750 unit tests passed (40 seconds), 255 functional tests passed
with 853 opt-in skips (78 seconds). Tests cover native-verified callback storage,
const/mutable/zero-filled direct callback arrays, null comparisons, and typed
inline-array factories for global/local/static/compound initialization. Separate
object linking checks deterministic helper identity and callback initialization.
Actual full SQLite parsing reaches the local typedef at physical183277. The new
local typedef fixture is a separate pending regression, not part of these counts.

## Unsigned plain-char native profile

The actual emitted char ABI probe builds and prints `255 0 0 255`, matching
GCC with `-funsigned-char` and dotcc's supplied limits. The native core, API,
VFS, 37-case upstream JSONB, virtual-table, 128-case allocator-fault and closed
image suites all pass after adding this explicit ABI flag. Every saved corpus
transcript remains byte-for-byte identical. Logs: `artifacts/native-unsigned-*`.

## Full amalgamation emission

Commits `ccf6ab9` and `65cd92d`: 1,753 unit tests pass (37 seconds),
257 functional tests pass with 857 opt-in skips (79 seconds), zero failures.
The full unchanged configured amalgamation parses, lowers to typed IR, and emits
106,296 lines / 3,641,990 bytes of C#: 3.09 seconds and 771,092 KiB peak RSS.
Logs: `artifacts/parse-probes/local-typedef-string-{unit,functional}-all.log`.
This establishes emission only. The combined engine/VFS translation exposes B014;
the standalone amalgamation's managed-library build exposes B015 syntax errors.

## Combined engine emission and all active offset contracts

Commits `840ab96`, `4678f4e`, and `304eff4`: clean rebuild, 1,756 unit tests
pass (43 seconds), 260 functional tests pass with 863 opt-in skips (81 seconds),
zero failures. Focused coverage includes 99 unit cases and the new include,
nested-switch and complete-sizeof runtime fixtures. The combined unchanged
SQLite + memory VFS emits 115,010 lines / 4,457,992 bytes of managed-library C#
in 3.22 seconds, peak RSS 947,232 KiB. All 30 compiler offset contracts now agree
with native sizes, alignments and offsets, including corrected WhereInfo storage.
`scripts/check-layout-metadata.py generated/TranslatedSqlite/Sqlite.cs` repeats
that static check; actual generated C# storage still requires execution.
Logs: `artifacts/include-order/checkpoint-*`.

All seven native harnesses also compiled and executed as single translation
units including the exact engine; core, API, VFS, virtual-table, allocation,
image and upstream JSONB outputs equal their saved transcripts byte-for-byte.
Logs: `artifacts/combined-*-native.{out,log}` and build logs. Core/API/VFS/upstream
combined harnesses now emit C#; virtual-table/allocation parser gaps B019/B020
are being fixed. The reusable C# build remains at opaque/tentative declarations.

## Opaque declarations, global storage and complete harness emission

Commits `7fdf9aa`, `a6ff6dc`, `dd1636b`, and `529a68c`: 1,775 unit tests
and 264 functional tests pass, 871 opt-in skips, zero failures. Native-verified
regressions cover opaque callback pointers, incomplete-type rejection, canonical
tentative globals, external initializers, designated callback tables and mixed
initialized arrays. Runtime-owned calendar/locale tags retain their dotcc library
definitions. All seven engine/harness translation units and the complete layout
probe now emit C#. The generated layout header uses `.h`, matching the compiler's
header catalog; native output remains identical after this campaign-only rename.
Logs: `artifacts/opaque-aggregates/`, `artifacts/translated-*-emission.log`.

## C# compiler control-flow performance

Commit `905c8b3`: clean build, 1,775 unit and 265 functional tests pass,
871 opt-in skips, zero failures. A native-verified 80-case control-flow stress
test passes with a 60-second compiler cancellation bound. The actual full SQLite
C# compilation now completes in 7.32 seconds and reports 170 remaining semantic
diagnostics, compared with the prior attempt cancelled after 5:14.68 without
diagnostics. The improvement preserves structured regions without entry labels.
The before/after profile and VDBE statement counts are in `switch-lowering.md`;
logs and read-only trace: `artifacts/switch-structure/` and
`artifacts/compiler-profile/`. This is a compilation progress result, not yet a
working SQLite assembly or runtime validation.

## Switch storage, runtime names and source filename checkpoint

Commits `8c45a24`, `ea32962`, and `cb266d8`: 1,782 unit and 267 functional
tests pass, 875 optional oracle skips, zero failures. Native/red regressions
cover prelude storage, skipped array initialization and the runtime DateTime
collision; five unit cases verify physical included-file provenance.
Actual complete engine emission takes 3.99 seconds / 1,048,676 KiB peak RSS;
its C# build completes in 7.29 seconds with 66 errors, down from 170. All 30
compiler offset contracts still match native. Actual C# storage and engine
execution remain pending. Logs: `artifacts/source-filenames/`.

## Complete managed-library compilation

Commits `9003712`, `d738f6f`, `1687ce7`, `0b4d96d`, and `6a1700d`:
1,785 unit tests pass (49 seconds), 273 functional tests pass (83 seconds),
887 optional oracle skips, zero failures. All six native-verified runtime
regressions pass, as do 405 focused pointer/Zig cases. The first variadic patch
failed four existing Zig saturation checks; its corrected C-only scope passes
the complete suites.

The unchanged combined engine emits in 4.13 seconds / 1,055,276 KiB peak RSS
and compiles into TranslatedSqlite.dll with zero errors (7.73 seconds wall,
7.57 seconds MSBuild). All 30 compiler offset contracts still match native.
The archive SHA-256 and all four extracted amalgamation files were rechecked
against the pinned ZIP with no changes. Logs:
`artifacts/pointer-conditionals/*scope*`. Engine execution, independent storage
checks and full native differentials are the next gate.


## First actual managed execution and independent storage checks

Commits `be6ab96`, `4fc82b1`, and `a54d816`: all 1,793 unit tests pass
(48 seconds), 276 functional tests pass (90 seconds), 893 optional skips, zero
failures. Character-conditional, array-address and row-stride reductions all pass.
The unchanged engine now selects ASCII correctly and builds with zero errors and
47 warnings. The separate C# consumer passes actual SQLite 3.50.4 SQL, JSONB,
explicit callback registration, nested SQL, pointer identity/GC stress and cleanup.

The expanded JIT layout probe independently checks 33 actual aggregate sizes and
alignments, eight pointer-inline-array sizes and storage spans, and all 30 member
address differences against native results. Compiler/generator constants also
match. These are actual storage checks, not only comparisons of folded constants.
Logs: `artifacts/preprocessor-char/{build-final,unit-all-final,functional-all-final,
engine-final-build,layout-jit-final}.log` and
`artifacts/managed-consumer-ascii-final-run.log`. Full native differential suites,
NativeAOT and clean reproducibility remain required before completion.


The actual full-engine C# consumer also publishes and executes under linux-x64
NativeAOT: publish 12.00 seconds, peak RSS 307,808 KiB; runtime under 0.01 seconds,
peak RSS 10,496 KiB, exit zero. Its SQL/JSONB, explicit callback, nested SQL,
function identity/GC and cleanup assertions all pass. Logs:
`artifacts/managed-consumer-aot-build.log`, `managed-consumer-aot.out`,
`managed-consumer-aot.err`, and `managed-consumer-aot-runtime.time`.


The complete layout pipeline also passes NativeAOT (22.60 seconds total, peak RSS
1,065,872 KiB). The 71-line native, JIT and AOT transcripts are byte-identical;
all 33 actual aggregate size/alignment checks, eight pointer-array storage checks
and 30 active offsets pass. Neither full-engine AOT publish reports ILxxxx
analysis warnings. ELF `DT_NEEDED` lists only libm, libc and the Linux loader;
there is no native SQLite dependency. Ordinary .NET native runtime imports are
not SQLite extension loading. Logs: `artifacts/layout-aot-validation.log`,
`layout-aot-total.time`, and `translated-layout-aot.{out,build.log}`.


## Broad translated integration checkpoint

The translated API, VFS, explicit virtual-table and all 37 adapted upstream
JSONB cases match their native transcripts exactly. API coverage includes UTF-16,
ownership/destructors, scalar/aggregate/collation callbacks, hooks, backup,
incremental blobs and 256 deterministic transaction operations (seed `0x51a17e`).
The complete memory-VFS contract and multi-connection recovery checks pass.
Allocator harness emission exposed B034; core/image reruns and port regressions
remain pending. Logs: `artifacts/translated-{api,vfs,vtable,upstream}.out`.

All eight native baseline runners also pass with the explicit mmap-disabled
profile. Only the reviewed core `PRAGMA mmap_size` zero row differs from the
previous platform-dependent profile. Exact source preservation and feature scope
are unchanged. Logs: `artifacts/mmap-profile/`.


## Complete native differential corpus

After the standard-header NULL and callback-null coercion correction, all six
translated suites were regenerated and pass against the exact native transcripts:
39 core SQL/JSON/JSONB cases, the API corpus with 256 seeded operations, full VFS
contracts, explicit virtual tables, 128 allocator failures (64 execution and 64
step failures with recovery), and all 37 adapted public JSONB assertions.

Independent-process database-image exchange also passes native→managed,
managed→native and managed→managed. Each direction checks integrity, JSONB,
Unicode/embedded NUL, a 64-KiB incremental blob, metadata and cleanup. No generated
engine source was patched. Logs: `artifacts/null-macro/runtime-*.log`,
`allocation-retry.log`, `image-exchange.log`, and `artifacts/exchange-*.out`.
M5 is complete for this documented corpus; shared port regression and clean
reproduction gates remain. This does not claim the complete SQLite test suite.


The final shared-header fix is committed as `a3a7b5f`. Full repository verification
passes 1,800 unit tests (51 seconds) and 277 functional tests (92 seconds), with
895 optional oracle skips and zero failures. The same snapshot re-emits/builds
and passes every SQLite runtime corpus, all image exchanges and the separate C#
consumer. The full managed library again has zero compile errors; all 30 metadata
contracts match. Logs: `artifacts/null-macro/{unit-all,functional-all,
managed-consumer}.log`. Existing Lua/Chibi/WAT execution is the next shared gate.

## Shared-port gate

Lua's complete existing upstream runner reaches `final OK !!!`. All 146 WAT
execution-oracle cases pass with zero skips (10 seconds). The port runner's
existing VSTest arguments work on this SDK; no argument workaround was needed.
Chibi emission succeeds but its C# build exposes two generic regressions: external
switch-label scope (B035) and unsigned constant arithmetic context (B036). Their
reductions and corrections are required before clean reproduction. Logs:
`artifacts/regression-{lua,chibi,wat}*` and `artifacts/ports-validation.log`.


Commit `147d26e` fixes unsigned constant arithmetic, primitive sizeof typing and
unsigned comparisons through 64 bits. Native/red reductions and 56 focused unit
plus five functional cases pass. Full SQLite emission/build passes (zero errors,
47 warnings; 7.61 seconds build); Chibi now reports only the two B035 switch-label
scope errors. Full shared suites will repeat after that remaining correction.
Logs: `artifacts/unsigned-constant-wrap/{unit-focus-after,functional-final,
engine-final-build,chibi-build-after}.log`.


## Final repository and shared-port checkpoint

Commits `1110b45` and `f9b19bc`: Release solution build passes in 9.30 seconds
with zero warnings/errors; all 1,814 unit tests pass (46 seconds), and all 280
functional tests pass (86 seconds), with 901 optional oracle skips. The first full
unit run found one stale Zig emission-string assertion; its slice length remains
three, and the corrected assertion passes the repeated complete unit suite.

Lua's upstream runner, Chibi's 1,225 tests/18 subgroups against its exact native
baseline, and all 146 WAT execution tests pass (35.95 seconds total). The actual
SQLite engine still builds in 7.68 seconds with zero errors and 47 warnings;
its separate consumer passes SQL/JSONB/callback/reentry/GC checks (12.13 seconds
including emission/build). Full clean-checkout reproduction is the remaining gate.
Logs: `artifacts/external-switch-{unit-final,functional-all,sqlite}.log`,
`external-switch-entry-final-build.log`, and `ports-final-validation.log`.


## Final clean-checkout reproduction — M6 complete

Implementation commit `53c4a06869885aa3f98c76f42da4836774587223` passed the
complete campaign from a fresh detached worktree at
`sqlite/artifacts/clean-checkout`. Initial tracked status was clean and no fetched
sources, generated output, `bin/` or `obj/` directories existed. From that
checkout's `sqlite/` directory, the exact command was:

```sh
SQLITE_AOT=1 scripts/verify.sh --with-ports
```

The command exited zero in **8 minutes 6.97 seconds**, with peak RSS
**1,096,696 KiB**. Both pinned archives were downloaded anew and their SHA-256
hashes verified. All four extracted amalgamation files match their archive bytes.
The build used NuGet `SharpAstro.LALR.CC` 4.7.0, with no local sibling dependency
or copied generated sources/binaries. No source, runner or expected-output changes
were needed during this reproduction.

- Release solution build: zero errors, 8.69 seconds.
- Unit suite: **1,814 passed**, zero failures/skips, 41 seconds.
- Functional suite: **280 passed**, zero failures, 901 optional oracle skips,
  1 minute 26 seconds. These skips are not counted as executed tests.
- All seven native baselines match their committed expected transcripts.
- Actual translated layout passes JIT and NativeAOT: **30 offsets, 33 aggregate
  size/alignment checks and eight pointer-array storage checks**.
- The separate C# consumer passes JIT and NativeAOT, including SQL/JSONB,
  explicit managed function pointers, nested SQL, identity/GC stress and cleanup.
- All six translated core/API/VFS/virtual-table/allocation/upstream suites match
  native, including 39 core cases, 256 seeded transaction operations, 128 injected
  allocator failures and 37 adapted public JSONB assertions.
- All three independent-process image exchanges pass: native→managed,
  managed→native and managed→managed.
- Lua reaches `final OK !!!`; Chibi passes **1,225/1,225** tests and **18/18**
  subgroups against its exact native baseline; WAT passes **146/146**, zero skips.

Top-level evidence in the original campaign directory:
`artifacts/clean-reproduction.log`, `clean-reproduction.time`,
`clean-reproduction-initial.json`, `clean-reproduction-input-audit.json`, and
`clean-reproduction-final-audit.json`.
Stage logs and regenerated outputs remain under
`artifacts/clean-checkout/sqlite/artifacts/`, including `campaign-repository.log`,
`campaign-layout.log`, `campaign-managed-consumer.log`,
`campaign-translated-*.log`, `campaign-image-exchange.log`, and `regression-*`.
The final documentation-only commit records this verified implementation snapshot.
The detached checkout remains clean after all stages. Both AOT publish logs have
zero ILxxxx analysis warnings. Both ELF executables list only libm, libc and the
Linux loader as direct dependencies, with no SQLite import. Ordinary native .NET
runtime dependencies are distinct from native SQLite interop or extension loading.

The supported profile remains Linux x64, LP64, little endian, unsigned plain char,
matching whole-unit bit-field layout, serialized calls and a process-local memory
VFS. There is no native SQLite dependency or dynamic extension loader. FTS remains
deferred; concurrent hosting, WAL shared memory, mmap and disk durability remain
outside this profile. This is the documented corpus, not all upstream SQLite tests
or TH3. Shared compiler limitations outside these verified inputs remain listed in
`../docs/C-SUPPORT.md`. The CI workflow is committed but remote CI was not run;
no branch was pushed. Future FTS/profile work is described in `usage.md`.


## Pointer inline-array element wrappers — 2026-09-10

Raw pointer and function-pointer fields inside an InlineArray caused CS9184 and
prevented managed consumers from using normal indexing and generic spans. New
source/object-link regressions first reproduced that diagnostic with CS9184
promoted to an error. Generated inline arrays now contain one-field unmanaged
structs holding the pointer in `Value`, preserving storage size and alignment.
The same regressions now pass C# indexing, span mutation, pointer-to-pointer and
callback access, zero initialization, layout checks, and translated C calls.

Full repository validation passes **1,814 unit tests and 282 functional tests**
(901 optional oracle skips). The regenerated SQLite library builds with zero
errors and **38 warnings, with no CS9184** (previously 47 warnings). Actual SQLite
layout checks and the managed SQL/JSONB/callback consumer pass under both JIT and
NativeAOT. Layout transcripts still match the native baseline.

Evidence: `artifacts/pointer-inline-before.log`, `pointer-inline-after.log`,
`pointer-inline-units.log`, `pointer-inline-functional.log`,
`pointer-inline-layout.log`, `pointer-inline-consumer.log`, and `build-only.log`.
The new `scripts/build.sh` was also executed from the repository root; it fetches
pinned inputs, builds dotcc/the generator, and regenerates/builds SQLite without
running tests, consumers, native oracles, or AOT publishing.


## Offset source generator removed — 2026-09-10

Commit `5a4c7ad` removes the optional Roslyn offset generator, its CLI/project
settings, conditional source emission, and the functional test GeneratorDriver.
The shared layout implementation now belongs to `DotCC.Lib/Layout/OffsetLayout.cs`;
dotcc emits all offset/size/alignment constants directly. Model/metadata tests
were retained as `DotCC.FunctionalTests/OffsetLayoutTests.cs`, with added ordinary
file/project tests for constant contexts and flexible headers. The object-link
regression first failed on the old conditional generator switch and now passes.
Historical entries above describe the former analyzer path; it is no longer used.

After deleting the obsolete generator directory including its ignored build
outputs, `scripts/build.sh` regenerated and built SQLite with zero errors and 38
unrelated warnings. Generated source/project files contain no offset-generator
symbol or analyzer reference; dotcc, functional tests, and TranslatedSqlite
runtime dependency manifests contain no OffsetGenerator dependency.

`scripts/verify.sh` completed with exit zero and AOT enabled:

- **1,814 unit tests and 284 functional tests passed**, with 901 optional oracle
  skips and no failures.
- All seven native baselines and six translated core/API/VFS/virtual-table/
  allocation/upstream-JSONB suites matched.
- All 30 offsets, 33 actual aggregate layouts, and eight pointer-array layouts
  matched native under JIT and NativeAOT.
- The separate managed SQL/JSONB/callback/GC/cleanup consumer passed under JIT
  and NativeAOT.
- Native-to-managed, managed-to-native, and managed-to-managed independent-process
  database-image exchanges passed.

Evidence: `artifacts/offset-removal-before.log`, `offset-removal-focused.log`,
`offset-removal-build-only.log`, `offset-removal-campaign.log`, and the refreshed
`campaign-repository.log`, `campaign-layout.log`, `campaign-managed-consumer.log`,
`campaign-translated-*.log`, and `campaign-image-exchange.log`.
The optional `--with-ports` campaign was not repeated for this removal.

## FTS5 profile: native baseline and first translated boundary

The shared profile now adds `SQLITE_ENABLE_FTS5`; FTS3 and FTS4 remain disabled.
The first full FTS5 engine emission on unchanged SQLite 3.50.4 reached typed IR
then failed on the tokenizer's external entry into the `non_ascii_tokenchar`
loop. It exited 2 in 4.90 seconds; C# compilation was not reached. Evidence:
`artifacts/fts5/engine-baseline-emission.log` and `engine-baseline.time`.

Strict native `scripts/test-fts5-native.sh` passes 34 SQL assertions plus explicit
FTS5 auxiliary/tokenizer callback and lifetime checks. Coverage is enumerated in
`fts5-inventory.md`; transcript is `tests/native-fts5.expected`.
The core corpus now has 41 cases: FTS5 option flag becomes one, its former
negative case now creates/inserts/queries/drops a table, and two new negative
cases retain FTS3/4 exclusion. FTS5 shadow-table writes account for the increase
of ten in later `total_changes` counters. All pre-existing data and error rows
remain unchanged. The explicit-vtable corpus now verifies FTS5 presence and
FTS3/4 absence. Reviewed transcript deltas are retained under `artifacts/fts5/`.
The same 34-assertion corpus passes with AddressSanitizer and UndefinedBehaviorSanitizer (`SQLITE_SANITIZE=1`), with an identical transcript. Translated FTS5 completion remains pending this checkpoint.

Native API, VFS, allocation (128 failures), and upstream JSONB (37 cases)
transcripts remain byte-for-byte unchanged with FTS5 enabled. Native layout
adds nine FTS5 offsetof contracts (30 to 39); every previous layout row is
unchanged. The expanded native expected transcript is saved, with the exact
nine-row delta at `artifacts/fts5/native-layout.diff`.

## FTS5 and canonical function identity checkpoint

The static FTS5 profile (`7cf1e34`) passes 34 native SQL assertions plus explicit
auxiliary/tokenizer callback lifetimes, including ASan/UBSan execution. Core now
has 41 cases with positive FTS5 and negative FTS3/4 checks. API/VFS/allocation/
public-JSONB baseline transcripts remain unchanged. The layout inventory grows to
39 active offsets, 42 actual aggregate size/alignment checks and eight arrays.

The translated FTS5 corpus matches native exactly (14.72 seconds including emit,
build and execution); core's 41 cases pass (14.55 seconds), and expanded layout
JIT checks pass (15.52 seconds). Both native-backed external-loop fixtures pass.
Logs and timings: `artifacts/fts5/{loop-fixtures.log,translated-fts5.*,
translated-core.*,layout-jit.*}`. The loop fixes are `9afe0eb` and `b6e1502`.

The expanded separate C# consumer (`574e1d9`) passes JIT and NativeAOT: prepared
CRUD, joins/correlated queries, CTE/windows, JSONB, savepoints/rollback, triggers,
FTS5 queries, explicit C# auxiliary/tokenizer callbacks, canonical pointer reuse,
GC stress and ownership/cleanup. Logs: `artifacts/managed-consumer-{jit,aot}.log`.
The runtime allocator cache (`cd89af1`, warning cleanup `7775e59`) has a structural
red/green regression and 18 passing allocator tests, including 30,000 cached-table
reuses, collections, independent contexts and indirect allocation.

Canonical emitted fields (`be7e05b`) and final static/owner/address-cancellation
corrections (`8d3df53`) pass focused source/object/runtime tests and full FTS5
build retries. The final retry has zero errors and 51 warnings (10.35 seconds).
All four source/object JIT/NativeAOT identity executions match native output,
including distinct addresses for identical static functions and actual calls.
`06b09cf` adds that reproducible AOT gate to `scripts/verify.sh`.
Logs: `artifacts/function-identity-validation.log` and per-route
`function-identity-{source,objects}-{jit,aot}.out`.

Full repository/port regression review and final campaign reproduction remain
active. These intermediate passes do not replace the final combined snapshot.

## M7–M8 complete — final FTS5 and canonical identity evidence

The verified implementation snapshot is
`3d4dbc0bac11405e426c8972c186c9904db02317`. The final documentation-only commit
records these results; M9/M10 remain planning-only and were not implemented.

Shared validation passed before the fresh packaging run:

- Release solution build: zero warnings/errors, 8.46 seconds.
- **1,816 unit tests passed**, zero failures/skips, 55 seconds. Thirteen old
  emitted-text expectations were updated to require canonical fields; production
  code was unchanged during that assertion-only correction.
- **299 functional tests passed**, zero failures, 913 optional oracle skips,
  1 minute 42 seconds. Skips are not counted as executed tests.
- Lua's upstream runner passed; Chibi passed **1,225/1,225** against its native
  transcript; WAT passed **146/146**, zero skips. Total port time: 40.75 seconds.
- Distinct identical-body static functions retained separate callable identities
  in **source and object routes under both JIT and NativeAOT**. All four outputs
  match the native-proven `1 42 42` fixture output.

Logs: `artifacts/function-identity-{unit-all,functional-all,ports}.log`,
`function-identity-ports.time`, and `function-identity-validation.log` with its
per-route emission/build/JIT/AOT logs. The first full unit log records the stale
assertion failures; the passing retry is `function-identity-unit-final.log`.

A fresh detached checkout at `artifacts/fts5-clean-checkout` began with empty
tracked status and no downloaded inputs, generated files, `bin/` or `obj/`
directories. Both pinned archives were downloaded again and SHA-256 verified;
all four amalgamation files remain byte-identical to their archive entries.
The clean Release solution build used NuGet `SharpAstro.LALR.CC` 4.7.0 and passed
with zero warnings/errors in 8.82 seconds. No copied inputs/binaries, local sibling
parser project, source edits or expected-output changes were needed.

From that checkout's `sqlite/`, the bounded packaging/integration sequence was:

```sh
python3 scripts/fetch.py
dotnet build ../dotcc.sln -c Release -p:UseLocalLalrCc=false
SQLITE_AOT=1 scripts/test-layout-translated.sh
SQLITE_AOT=1 scripts/test-managed-consumer.sh
scripts/test-translated.sh fts5
scripts/test-translated.sh core
scripts/test-image-exchange.sh
for suite in api vfs vtable allocation upstream; do
  scripts/test-translated.sh "$suite"
done
```

Every stage passed. Layout verifies **39 offsets, 42 actual aggregate layouts and
eight pointer arrays** under JIT and NativeAOT. The separate managed consumer
passes expanded core/JSONB SQL, FTS5 CRUD/search, explicitly registered C# auxiliary
and tokenizer callbacks, canonical pointer identity, GC stress and cleanup under
both runtimes. **All seven FTS5-enabled translated suites match native**: core
(41 cases), API (including 256 seeded operations), VFS, explicit virtual tables,
allocation (128 failures), public JSONB (37 assertions), and FTS5 (34 assertions
plus callback/tokenizer lifecycle checks).

All three independent-process database-image directions pass with persisted FTS5
index pages and Unicode documents: native→managed, managed→native and
managed→managed. Tests include index update/delete, bound term/phrase/Unicode/
boolean MATCH queries and content/index integrity, alongside the existing core,
JSONB, 64-KiB incremental blob, metadata and cleanup assertions (`aeaac2b`).

The initial packaging sequence exited zero in **2 minutes 12.25 seconds**, peak
RSS **1,262,064 KiB**; the five supplemental translated suites exited zero in
**1 minute 8.19 seconds**, peak RSS **1,250,076 KiB**. Already-passing repository,
port and generic identity-AOT suites were reused rather than repeated. The two
clean AOT publish logs have no ILxxxx analysis warnings; their ELF dependencies
are only libm, libc and the Linux loader, with no SQLite import or dependency.
The detached worktree remains clean after validation.

Original-directory evidence:
`artifacts/fts5-clean-reproduction.{log,time}`,
`fts5-clean-supplemental.{log,time}`, and
`fts5-clean-reproduction-{initial,input-audit,aot-audit,final-audit}.json`.
Stage logs are under `artifacts/fts5-clean-checkout/sqlite/artifacts/clean-*`.
The complete reusable campaign command remains `SQLITE_AOT=1 scripts/verify.sh
--with-ports`; the evidence above records which stages were run freshly and which
already-green gates were reused, rather than claiming one new full-script run.

The supported profile remains Linux x64, LP64, little endian, unsigned plain char,
matching bit-field storage, serialized calls and a process-local memory VFS.
FTS5 is statically compiled; FTS3/4 remain deferred. Native SQLite is only the
separate oracle. Extensions register explicitly in C#, with no dynamic loading.
WAL shared memory, mmap, concurrent hosting and disk durability remain outside
this profile. This is the documented corpus, not the complete upstream Tcl/TH3
suite. `offsetof` remains direct compiler emission; no generator was restored.
CI configuration is committed, but remote CI was not run and nothing was pushed.


## M11: Real host files, OS locks and hot-journal recovery

The managed library now defaults to `dotcc-host`, using BCL random-access I/O and
OS-specific locking/durability services. This supersedes the preceding product
memory-only limitation; deterministic C fixtures still use `dotcc-memory`.
SQLite core, JSONB and FTS5 remain C#, with no native SQLite dependency or dynamic
extension loading. See [host VFS](host-vfs.md) for the integration and platform
contract. M9/M10 were not implemented.

Local validation ran on Linux x64 with .NET 10 against code through `be77292`:

```sh
SQLITE_AOT=1 scripts/test-host-vfs.sh
SQLITE_AOT=1 scripts/test-managed-consumer.sh
scripts/test-translated.sh vfs
scripts/build.sh
```

All four commands exited zero. The last command still only fetches/builds and
emits the reusable library; it does not execute tests or publish AOT. The new
host-VFS gate is also part of `scripts/verify.sh`. This phase did not rerun the
entire repository/port campaign: generic compiler/runtime code did not change,
and the preceding phase's broader results remain historical evidence.

The initial default-VFS regression failed with `expected dotcc-host, got
dotcc-memory` before integration. Passing raw contracts cover sparse offset I/O,
short-read zero fill, readonly writes/truncation, exclusive creation, temporary
and delete-on-close files, UTF-8/canonical paths, no-follow symlinks, flushes,
real clocks/randomness and handle cleanup. SQL tests verify real database file
headers, close/reopen persistence, rollback, JSONB, FTS5 and integrity, plus
shutdown/reinitialization. The ordinary managed consumer now creates a temporary
disk database and passes its full SQL/JSONB/FTS5 and C# callback workload under both
JIT and NativeAOT.

Independent processes pass in all three directions: managed→managed,
native→managed and managed→native. The native oracle compiles the pinned
amalgamation with its actual OS VFS and runs in a separate process. Tests verify
simultaneous shared locks, reserved contention, pending admission, exclusive
upgrade/downgrade and readonly reserved-lock probes. A writer spills pages into
a real transaction, the controller verifies the synced hot-journal magic, then
forcibly kills it. The surviving engine recovers the original data, passes
integrity and writes again. Hardlink aliases coordinate locks, and symlink paths
use the canonical target's journal, including symlink-before-`..` resolution.
The complete process campaign passes for both JIT and NativeAOT.

Two standalone regression cases simulate macOS unlock failures with real safe
handles. They verify immediate close and deferred close while peers retain
locks, removal of disposed leases, poisoned-state refusal and eventual drain.
These validate ownership/error handling only; they do not execute macOS syscalls.
Windows and macOS implementations have source review and dedicated JIT/NativeAOT
CI configuration, but neither operating system was run locally and remote CI was
not triggered. Linux ARM64 is also unexecuted; BSD is explicitly unsupported.

Evidence under `artifacts/`:

- `host-vfs-default-red.log`: initial failing default-VFS assertion.
- `host-vfs-final.log`: passing platform failure models and JIT/AOT host campaign.
- `host-vfs-aot-build.log`: NativeAOT publish; no host-adapter or ILxxxx warnings.
- `host-vfs-managed-consumer-final.log`: managed consumer succeeds twice, JIT/AOT.
- `host-vfs-memory-regression.log`: existing memory VFS matches the native fixture.
- `host-vfs-build-only.log`: dotcc and generated library build, zero errors.

Remaining generated SQLite warnings include CS8909; cached canonical callback
addresses remain in use. The Linux AOT executable depends only on libm, libc and
the ELF loader, with no SQLite dependency. This is a rollback-journal VFS with
version-1 I/O methods: WAL/mmap are not advertised, and SQLite calls must still
be serialized in each process. Process-kill recovery verifies the journal/OS
contract, not physical power-loss behavior. Changes are committed locally only.

## M12: SQLite 3.53.4 and host WAL

The pinned inputs are now the user-requested SQLite **3.53.4** release. Both
archives are SHA-256 pinned, and the extracted `sqlite3.c` SHA3-256 matches the
upstream release page; see [source provenance](source.md). This release includes
the upstream WAL-reset race fix. The amalgamation remains unchanged.

The real host VFS advertises version-2 I/O methods, maps the actual `-shm` file
through BCL `MemoryMappedFile`, and implements native-compatible shm range and
dead-man locks, barriers and cleanup. WAL is enabled with
`PRAGMA journal_mode=WAL`. The managed consumer now selects it explicitly before
its full SQL/JSONB/FTS5 and C# callback workload. Database mmap remains disabled;
M9/M10 remain plan-only and the offset generator remains removed.

Two generic dotcc compatibility fixes were required by the updated amalgamation:

- Disabled Tcl source inside `#if 0` contained non-C preprocessing tokens such as
  `$`. The lexer now preserves such tokens through preprocessing and diagnoses
  those that survive into C parsing; macro stringification and discarded arguments
  are covered too. The initial actual-amalgamation failure and reduced regression
  are recorded. Full unit tests passed **1,826/1,826** and functional tests passed
  **300**, with **915** optional oracle skips (`2b31d89`).
- The supplied `stdint.h` lacked integer-constant macros, producing **54** missing
  `UINT64_C` errors in generated 3.53.4 C#. All ten standard constant macros now
  have LP64-appropriate suffixes/promotions. **13** relevant unit tests and **four**
  focused functional fixtures pass (eight optional skips), as do actual engine
  emission and library compilation (`4ab79be`). The broad exploratory fixture
  also exposed existing `_Generic`, literal-expression `sizeof`, and unsigned
  comparison constant-folding gaps. These are explicitly open in
  `docs/plans/deferred.md`; they did not block the SQLite build or runtime corpus.

The initial VFS capability regression failed because the old method table had no
WAL methods (`wal-contract-red.log`). Linux x64 raw and SQL contracts now pass
under **JIT and NativeAOT**. They verify shared mapping visibility, stable addresses
across index growth, shared/exclusive range conflicts, atomic failed acquisitions,
sibling cleanup, invalid ranges and unmap. SQL checks cover reader snapshots,
`SQLITE_BUSY_SNAPSHOT`, one-writer contention, savepoints, JSONB/FTS5, all five
checkpoint modes (including NOOP doing no work), 4,500-page multi-region index
growth, readonly reopen, integrity, final sidecar cleanup and return to DELETE.
New JSONB table functions and JSON/JSONB array insertion are exercised too.

The independent-process campaign passes under both runtimes for all three pairs:
managed→managed, native→managed and managed→native. It includes the earlier
rollback-journal cases and adds WAL snapshots, checkpoint blocking/completion,
multiple mapped regions, readonly-media reads and recovery, and actual forced
termination with committed and spilled uncommitted frames. Both stale and missing
shm indexes recover committed data and discard the uncommitted tail. A stress case
overlaps 120 writer transactions with 120 passive checkpoints in separate processes,
then verifies all row values and integrity after checkpoint/reopen. This is bounded
stress, not an exhaustive scheduler or power-loss simulation.

The readonly-media regression first passed native→native and failed managed with
`SQLITE_CANTOPEN`: SQLite opens WAL read/write even for readonly databases and
expects a readonly fallback. The host adapter originally restricted fallback to
the main database. Extending it to existing WAL files fixes all three mixed/managed
pairs without changing the test (`8b84480`). Permissions tests require a non-root
POSIX account; Windows/root runs explicitly skip that permission-specific case.

Fresh native 3.53.4 baselines pass for core, API, VFS, virtual tables, allocation,
public JSONB, FTS5 and layouts. Exactly two expected lines changed: the reported
SQLite version and the `Fts5TokenDataIter.apIter` layout (size/offset 56→72).
All other native baselines are unchanged (`9ccd69e`).

Evidence under `artifacts/`:

- `inactive-script/`: reduced lexer failures, native comparison, full build/unit/
  functional results and actual 3.53.4 emission.
- `stdint-constants/`: missing-macro regression, focused results, preserved open
  expression-type findings and actual engine build.
- `wal-host-contracts.log`, `wal-aot-contracts.log`: passing raw/SQL contracts.
- `wal-process-jit-final.log`, `wal-process-aot.log`: complete passing process
  campaigns, including native interoperation and concurrent stress.
- `wal-readonly-diagnostic.log`, `wal-readonly-fixed.log`: native-proven failure
  and all three passing pairs after the WAL readonly fallback fix.
- `wal-aot-build.log`: NativeAOT publish with no adapter or ILxxxx warnings.
- `wal-native-baselines.log` and `wal-native-*.{out,diff}`: fresh native evidence
  and the two reviewed baseline changes.

The final regression sequence (`artifacts/wal-regressions.sh`, invoked from the
repository root) also exited zero against code through `4ab79be`:

- All seven translated corpora match their fresh native 3.53.4 baselines: core,
  API, memory VFS, virtual tables, allocation, public JSONB and FTS5.
- Layout checks pass under JIT and NativeAOT: 39 offsets, 42 actual aggregate
  layouts and eight pointer-array layouts, including the changed FTS structure.
- The separate managed consumer passes its WAL SQL/JSONB/FTS5, cached callbacks,
  nested SQL, identity/GC and cleanup workload under JIT and NativeAOT.
- All three independent database-image exchange directions pass.
- Both platform failed-close ownership models pass; these simulate Darwin errors
  and do not claim execution of macOS system calls.
- The ordinary `scripts/build.sh` fetches/verifies the new pins, builds dotcc,
  emits and builds the reusable host-VFS library with zero errors, and remains
  build-only.

Final-stage logs: `wal-regressions.log`, `wal-translated-*.log`,
`wal-layout-jit-aot.log`, `wal-managed-consumer-jit-aot.log`,
`wal-image-exchange.log`, `wal-platform-models.log` and `wal-build-only.log`.
The reusable host gate remains `SQLITE_AOT=1 scripts/test-host-vfs.sh`; this phase
ran its build/contract/process components and the regression sequence explicitly,
not a newly claimed full `verify.sh --with-ports` invocation. Unchanged port and
standalone function-identity AOT gates retain their preceding-phase evidence.

Linux x64 is the locally executed platform. Windows/macOS implementations and
JIT/AOT CI are included and source-reviewed, but no remote CI or other OS execution
is claimed. BSD remains unsupported. SQLite calls remain serialized within each
process; WAL enables concurrent processes on the same host/local filesystem.
The Linux NativeAOT executable depends only on libm, libc and the ELF loader,
with no native SQLite dependency. Changes are committed locally without pushing.

## M10 — Borrowed span varargs (2026-09-10)

The emitted API is now `params ReadOnlySpan<VaArg>` and `VaList` is a ref struct
with a readonly borrowed span and an independent mutable cursor on copies.
Variadic managed function pointers carry an explicit final span and use the
canonical static readonly address fields. The compiler diagnoses unsupported
cursor storage and escaping local borrows, and scopes locally started cursors
and their aliases. See [API and limitations](varargs-span.md).

Regression-first evidence includes the original array-signature assertion failure
(`m10-span-red.log`) and the variadic callback fixture's missing span-tail/signature
errors. After the fixes, 19 lifetime cases and source/object execution cases pass.
A full fresh solution build has zero warnings/errors; the repository gate passes
**1,857 unit tests** and **307 functional tests**, with **919 optional tests skipped**
(`campaign-repository.log`). The skipped platform/oracle suites are not claimed
as executed by this run. The explicit WAT gate is recorded separately below.

`scripts/test-varargs-span.sh` freshly emits its C fixture and builds a separate
consumer. Both JIT and linux-x64 NativeAOT pass all **15 cases**, each measured for
20,000 calls after 4,096 warmup calls. All span cases allocate **zero measured
bytes**, including translated C calls, expanded arguments, explicit storage,
forward/copy, callbacks and promotions. The benchmark build has zero warnings,
and its NativeAOT publish has no IL warnings or new span lifetime warnings.

| Expanded pack size | Span bytes/call, JIT and AOT | Array baseline bytes/call |
| --- | ---: | ---: |
| 0 | 0 | 0 |
| 1 | 0 | 40 |
| 8 | 0 | 152 |
| 64 | 0 | 1,048 |

Representative expanded-call times for 20,000 calls (span / array baseline):
JIT 1 argument **1.208 / 1.002 ms**, 8 arguments **4.666 / 4.283 ms**, and
64 arguments **31.153 / 23.627 ms**; AOT **0.141 / 0.453 ms**,
**0.285 / 1.651 ms**, and **2.118 / 5.062 ms**, respectively. These are reported
observations, not timing thresholds or a general speedup claim. The baseline
isolates array allocation and directly sums values, while the translated path
also forwards a VaList cursor. Runtime optimization and sampling order affect
these short timings. Allocation removal is the demonstrated improvement.
A 64-carrier pack has 1,024 bytes of payload; arbitrary arity/recursive stack use
is not established. Existing caller-owned arrays remain available for large packs.
Full outputs are `varargs-span-{jit,aot}.out`, with emission/build/publish logs
under the same prefix.

The complete `scripts/verify.sh --with-ports` invocation finishes with exit zero
and **AOT enabled** (`m10-campaign.log`). It freshly verifies the pinned SQLite
3.53.4 archives and passes:

- Eight native baseline comparisons, all seven translated SQL/API/VFS/vtable/
  allocation/JSONB/FTS5 corpora, and layout comparisons under JIT and NativeAOT.
- The separate managed consumer's SQL CRUD, joins, CTE/windows, JSONB, FTS5,
  registered C# callbacks, nested SQL/GC and WAL workloads under JIT and NativeAOT.
- Host VFS contract/platform-model checks and independent process locking,
  crash/recovery, readonly-media, WAL snapshot/checkpoint/growth and integrity
  campaigns under JIT and NativeAOT, including native-process interoperability.
- Canonical function identity under source/object linking, JIT and NativeAOT,
  plus all three native/managed database-image exchange directions.
- Lua's upstream runner (`final OK !!!`), Chibi's **1,225/1,225 R7RS tests** with
  unchanged baseline, and the explicit **146/146 WAT execution tests**.

Campaign logs use `campaign-*.log`; port evidence is in `regression-lua.out`,
`regression-chibi.out` and `regression-wat.log`. Actual SQLite compilation has
zero errors and the existing 57 warnings; all 28 span-related CS9080 warnings
from the first intermediate build are removed. Final engine/consumer/host-VFS,
Lua and Chibi build logs contain no CS9080. The benchmark gate now treats CS9080
as an error in both build and publish; its final rerun is `m10-varargs-final.log`.

M10 is complete for the documented Linux x64 profile. Other OS execution is not
claimed. M9 remains plan-only, the offset generator remains removed, and all
changes are committed locally without pushing.

## M13 — Math, percentile and column metadata (2026-09-10)

Enabled `SQLITE_ENABLE_MATH_FUNCTIONS`, `SQLITE_ENABLE_PERCENTILE` and
`SQLITE_ENABLE_COLUMN_METADATA` in the shared configuration. The first actual
SQLite build failed with three CS0103 errors for missing `acosh`, `asinh` and
`atanh` callback targets (`features-first-build.log`). A reduced C fixture
reproduced missing runtime functions (`features-math-red.log`); BCL Math/MathF
wrappers and header declarations fix direct calls and cached double/float
callbacks. The fixture passes and matches strict GCC output, including domain
NaNs, poles and signed zero (`features-math-green.log`, `features-math-native.out`).

The new API contracts pass against native SQLite and translated JIT/NativeAOT:
math families and domain NULLs; all four percentile functions with aggregate,
sliding-window, grouped, empty/NULL, sorting and invalid-input recovery cases;
and all six UTF-8/UTF-16 column-origin APIs through aliases, views, joins,
attached databases, expressions and statement reset. The native expected
transcript adds exactly three PASS lines; existing results remain unchanged.
The separate managed consumer directly exercises the public APIs under JIT/AOT.

The complete `scripts/verify.sh` campaign finishes with exit zero and AOT enabled
(`features-campaign.log`). It passes 1,857 unit tests, 308 functional tests
(921 optional skips), the span allocation gate, all eight native comparisons,
all seven translated corpora, layout and managed-consumer JIT/AOT, host VFS/WAL
contract and independent-process recovery tests, canonical function identity,
and all three image-exchange directions. API NativeAOT is now a repeatable part
of `SQLITE_AOT=1 scripts/test-translated.sh api`; its transcript matches
`tests/native-api.expected` (`translated-api-aot.out`). Lua/Chibi/WAT were not
rerun for this phase. No new native SQLite dependency or dynamic loading is added.

M13 is complete on Linux x64. The following requested phase adds multithreading
and database mmap; the M13 validation above uses the preceding serialized,
disabled-database-mmap profile.

## M14 — Multithreading and database mmap (2026-09-10)

The managed product applies `config/host-defines.txt` after the deterministic
corpus definitions: `SQLITE_THREADSAFE=1`, `SQLITE_MUTEX_APPDEF=1`, host VFS,
64 MiB default database mmap and 256 MiB maximum per file. The corpus profile
remains explicitly single-threaded and unmapped. The host uses BCL monitors,
initialization memory barriers and read-only `MemoryMappedFile` views from existing
file handles. SQLite still implements transactions, page management and WAL.
See [threading and mmap](threading-mmap.md) for ownership and configuration.

Upstream `SQLITE_OS_OTHER` otherwise selects no-op mutexes even when THREADSAFE
is enabled. `prepare-host-source.py` permits APPDEF in exactly one selection guard
of a generated input copy, supplies the default mutex/barrier hooks from
`host_mutex.c`, and preserves the downloaded references byte-for-byte. Source
preparation tests verify the exact single substitution and rejection of changed
reference inputs before output. The original source SHA-256 is
`b1dd5d74ec7f29055a6684fa06fb3c2f6821c87dd38f9a458dfd2e8a1db28189`;
the adapted source SHA-256 is
`497cc4d6de14a548553a1508c57a8b88bcf02dfe28735e1b7771d281212b77e1`.
The generated manifest records the header hash and exact guard as well.

The allocation audit found that `NativeMemory` OOM escaped through C allocation
APIs, which could bypass the memory VFS's C unlock paths. Ten deterministic
impossible-size cases first failed (`allocator-before-tests.log`). The shared
libc now returns null, guards calloc/debug-overhead overflow, and preserves the
old allocation on failed realloc. Fourteen new cases plus existing related tests
pass (89 total), and the actual SQLite product then emits/builds with zero errors.
This changes the shared runtime, without introducing SQLite-specific compiler
rules. Arbitrary managed extension exceptions remain outside the C callback
contract; callbacks should return SQLite errors.

The dedicated threading executable passes nine groups under JIT and linux-x64
NativeAOT (`threading-{jit,aot}.out`): concurrent cold initialization, quiescent
shutdown/restart and configuration modes; static/dynamic mutex identity,
recursion/ownership/try; shared FULLMUTEX updates and callback reentry; distinct
NOMUTEX host rollback, host WAL and named-memory connections; cross-thread
recovery after injected OPEN NOMEM/IOERR; concurrent allocation/status/randomness;
and joined workers with no leaked VFS handles or dynamic mutexes. Static mutex
identities intentionally persist across shutdown. Collectible hosting is not
supported. Named-memory contention uses an elapsed-time retry bound because its
deterministic sleep callback intentionally does not wait.

An independent native layout gate uses the exact host definitions and adapted
input with the existing LP64/GCC ABI flags. It compares 48 aggregate sizes and
alignments, all 41 offset contracts (39 active source requests plus two flexible
array contracts), 67 actual field addresses and eight pointer-array first/last
storage checks with the public generated product types. JIT and NativeAOT
transcripts match (`product-layout-{jit,aot}.out`); no IL warnings appear in its
AOT publish. Native link stubs abort if called and exist only to link this layout
probe; they are not a native runtime or mutex oracle. The generator produces test
consumer assertions, not an OffsetGenerator analyzer or a product postprocessor.

All six host VFS contract groups pass under JIT and NativeAOT in the full
campaign (`campaign-host-vfs.log`). New raw mmap assertions cover old-value/query
and maximum-cap semantics, duplicate borrows, pointer stability across GC and
rejected changes, invalid releases, overflow/EOF checks, the 256-byte safety
margin, ordinary-write visibility, growth with live borrows, remapping,
truncation, readonly handles, disable/fallback and complete cleanup. SQL tests
assert a positive mapped-read counter increase (not just an enabled PRAGMA) in
rollback and WAL modes, then exercise snapshots, concurrent writes, checkpoints,
new file data, disable/re-enable, VACUUM with another connection open, integrity
and readonly reopen. No mapped views or references remain afterward.

The same gate passes independent managed/managed, native/managed and
managed/native process pairs under both managed runtimes: lock admission,
rollback recovery after forced death, WAL snapshots/checkpoints/index growth,
committed-frame preservation, readonly media, stale/missing index recovery,
overlapping writers/checkpointers and inode/path aliases. These separate native
processes use SQLite's normal OS VFS and single-threaded harness configuration;
they establish storage interoperability, not a native multithreading benchmark.
The product mmap defaults remain enabled during these runs. Actual product
compilation has zero errors and the existing 57 warnings; the host and threading
AOT logs contain no IL warnings or span-lifetime CS9080 warnings.

The dedicated Linux/Windows/macOS workflow now prepares the guarded host input,
merges host definitions by their case-sensitive names and includes ThreadingTests
alongside the VFS/mmap JIT/AOT gates. Its YAML and PowerShell blocks were parsed
locally; remote CI and Windows/macOS runtime results are not claimed. Local
execution evidence is Linux x64. Static mutex handles intentionally persist for
noncollectible hosting; shutdown/reconfiguration requires caller quiescence.

The final `scripts/verify.sh --with-ports` run exits zero with NativeAOT enabled
(`m14-campaign.log`). Both preparation tests, 1,871 unit tests and 308 functional
tests pass (921 optional functional skips). The span benchmark retains zero
warmed allocations in all 15 cases. All eight native comparisons and seven
translated corpora pass, including API NativeAOT math/percentile/metadata
contracts. Corpus/product layouts, managed SQL/JSONB/FTS5 consumers, threading,
VFS/mmap/WAL and canonical source/object function identity pass under JIT/AOT.
All three database-image exchange directions pass. The shared runtime change
also passes Lua's upstream `final OK !!!`, Chibi's unchanged 1,225/1,225 R7RS
baseline, and all 146 WAT execution tests.

M14 is complete for the documented Linux x64 profile. The build-only helper
continues to fetch, emit and build without running tests. M9 remains plan-only;
the OffsetGenerator remains removed, and no native SQLite dependency or dynamic
extension loading is introduced. Changes are committed locally without pushing.

## M9 — Standalone Roslyn Cond.B optimization (2026-09-10)

The follow-up request authorizes M9 implementation as a standalone command after
all normal dotcc actions. `DotCC.PostProcess` consumes an emitted project and
writes separate original/optimized snapshots; neither dotcc nor the SQLite build
helper invokes it. All transformations use semantic models and syntax-tree nodes,
then serialize and revalidate. Only Cond.B calls and proven redundant CBool
conversions within their arguments are rewritten. Runtime CBool stores/arithmetic
and compiler emission remain unchanged. Roslyn comes from the .NET 10 SDK and
is referenced only by the standalone tool and its tests.

Reduced tests first exposed target-typed `new()` losing its context and pointer
null conversions failing the helper proof. Review added tests for competing
implicit/explicit user conversions, constructor/indexer caller-argument capture,
query-provider expression trees, CBool constructor effects and optional helper
arguments. Actual SQLite then exposed a separate representation case: its embedded
CBool lives in the global namespace, while the library/test runtime uses
`DotCC.Libc`. Both are now covered by structural proofs. Semantic-model/proof
caching fixes the first slow large-source attempt. The final tool rewrites all
**24,123 calls, with zero skips**, and validates the rewritten compilation in
about **15 seconds**, using roughly **600 MiB** peak RSS on this host.

All **38 semantic regressions** pass, comparing original execution, rewritten
execution, serialized/reparsed execution and idempotence. They cover every helper
overload, native-width integers, NaN/signed zero, typed/function pointers, CBool,
assignments/increments, short-circuiting, volatile/atomic reads, checked overflow,
contextual typing, aliases/shadowing, comments and retained observable contexts.
The final solution build has zero warnings/errors (`m9-repository-build.log`),
and `m9-core-tests.log` records the final test result. The three standard platform
CI jobs now include these tests; remote CI has not run in this session.

The standalone CLI contracts also pass (`m9-cli-smoke.log`): isolated
original/optimized builds preserve implementation assembly references, resource
names/bytes, Release preprocessor symbols even when built as Debug, caller-file
paths and portable PDBs. They verify deterministic manifests, unchanged inputs,
existing-output rejection, symlink path guards and no partial output on invalid
C#. These tests caught and fixed incremental MSBuild evaluation returning no
compiler arguments, missing resource preparation and an inadequate file-based
path map; copied source files now use one mapped directory per input.

The explicit command
`python3 sqlite/scripts/test-postprocess.py sqlite/generated/postprocess-m9-final --aot --corpora`
finishes with exit zero (`m9-differentials.log`). Original and optimized managed
SQL/JSONB/FTS5 consumers, threading, mmap/WAL contracts and independent native
process interoperability all pass under JIT and linux-x64 NativeAOT. The separate
core, API, public JSONB and FTS5 executable corpora match their committed native
baselines in both original/optimized variants under both runtimes. No IL warnings
or CS9080 span-lifetime warnings appear in the publish logs. Optimized compilation
exposes six additional CS0162 unreachable-code warnings (63 warnings versus 57);
all other warning counts remain unchanged, including canonical-pointer CS8909.

| Measurement | Original | Optimized |
| --- | ---: | ---: |
| All product C# source bytes | 6,538,635 | 6,320,439 |
| Managed engine assembly bytes | 1,946,624 | 1,694,208 |
| NativeAOT managed-consumer executable bytes | 3,915,304 | 3,681,832 |
| First snapshot Release build elapsed | 8.11 s | 5.29 s |
| Prepared-query median, JIT | 319.11 ms | 99.33 ms |
| Prepared-query median, NativeAOT | 337.35 ms | 103.70 ms |
| Measured managed allocation per query round, JIT/AOT | 0 bytes | 0 bytes |

The query benchmark scans 1,000 rows with an integer filter and sum. After 1,000
warmup queries, each of three measured rounds executes 3,000 queries and verifies
the same total (501,501,000). Timings above are the median of those rounds; the
observed improvement is about 3.2× for this workload. This is not a universal SQL
speedup or a statement that SQLite makes no unmanaged allocations. Builds are
single observations affected by caches/order. Detailed timings, sizes and all
query rounds are under `artifacts/postprocess/`; source-size collection traverses
the snapshot's per-input subdirectories.

M9 is complete on the locally tested Linux x64 platform. It remains opt-in and
standalone. The initial CLI supports emitted single-target net10.0 projects and
explicitly rejects custom generators/analyzers, response-file inputs and other
unsupported project features described in [the tool guide](../../docs/postprocess.md).
Feed the original emitted project to subsequent CLI runs; the pure semantic
rewriter is idempotent, while generated snapshot projects use internal response
files. No Rider analyzer, offset generator, native SQLite dependency or automatic
optimization hook was added. All changes are committed locally without pushing.

## Standalone empty-block cleanup (2026-09-10)

The standalone post-processor now runs a syntax-tree cleanup after Cond.B
inlining. The current translated engine snapshot at
`generated/postprocess-empty-blocks` removes **2,208 standalone empty blocks**;
**24,123 Cond.B calls** still inline with **zero skips**
(`artifacts/empty-block-sqlite-rewrite.log`). Required bodies, labels and nonempty
scopes remain intact. Comments and line breaks survive serialization, while
blocks containing directives or captured caller-argument text are retained.

All **49 postprocessor tests** pass (`artifacts/empty-block-tests.log`), including
11 new cases covering nested removal, statement/declaration bodies, switch
sections, labels, comments, caller line numbers and argument text, disabled
source, top-level entry points, combined inlining and repeated processing.
Execution tests compare original, transformed and serialized/reparsed source.
The CLI smoke also passes (`artifacts/empty-block-cli-smoke.log`), with empty-block
counts now included in the manifest. Repeated runs exposed nondeterministic
warning-option enumeration in snapshot project files; sorting the diagnostic IDs
now makes those properties reproducible without changing their values.

`python3 sqlite/scripts/test-postprocess.py sqlite/generated/postprocess-empty-blocks --aot`
passes from the repository root on Linux x64. Original and optimized managed SQL,
JSONB and FTS5 consumers, host VFS, threading and prepared-query workloads pass
under both JIT and NativeAOT. The independent native-process VFS campaign also
passes for both variants and runtimes, covering locking, WAL, crash recovery and
file aliases. Transcripts match (`artifacts/empty-block-differentials.log`). This
follow-up reruns the consumer/VFS/threading gate; the separate executable corpus
campaign was last run for M9 above.

## Rider analyzer and in-place code fixes (2026-09-10)

The optional `DotCC.PostProcess.Analyzers` and `DotCC.PostProcess.CodeFixes`
assemblies target .NET Standard 2.0 and Roslyn 4.14. They compile the same
condition proof, observable-context and empty-block rewrite source as the
standalone tool. `DCCPP001` offers Cond.B inlining; `DCCPP002` offers standalone
empty-block removal. Both expose individual fixes and document/project/solution
Fix All through Roslyn code actions. The SQLite targets add the built assemblies
only for design-time compilation; normal builds remain analysis-free for these
rules, and the standalone CLI retains its existing input contract.

All **31 IDE regressions** pass (`artifacts/rider-analyzer-tests.log`): all current
argument overloads, typed/function pointers, CBool and nested calls, source
positions after nested block removal, required bodies, caller line/argument
information, directives, generated sources, suppressions, unknown helpers,
invalid documents, Fix All scopes and rule isolation, and MEF discovery. The
existing **49 standalone tests** and CLI smoke also pass after extracting the
shared implementation (`artifacts/rider-standalone-tests.log` and
`artifacts/rider-standalone-cli-smoke.log`).

The separately enabled full SQLite IDE test passes against
`generated/postprocess-empty-blocks`: **24,123 Cond.B diagnostics and 2,208
empty-block diagnostics** are fixed through the code-action API. Every resulting
source file **exactly matches the standalone optimized snapshot** previously
verified under JIT and NativeAOT. The final IDE compilation emits successfully
and reports no remaining postprocessor diagnostics. The test takes **31.34 s**
including repeated diagnostic runs, both Fix All passes, exact comparisons,
semantic revalidation and assembly emission (`artifacts/rider-sqlite-fixall.log`).
It edits an in-memory workspace, preserving the on-disk SQLite sources.

The local `DotCC.PostProcess.Rider.0.1.0.nupkg` is built under
`artifacts/packages/`, with only the two analyzer/code-fix DLLs and no runtime
assets or dependency packages. The package smoke uses a fresh NuGet cache,
loads both diagnostics through the real compiler, checks unchanged input source
and application output, and verifies no analyzer/Roslyn application dependency.
It also evaluates SQLite with ordinary, design-time and explicitly disabled
IDE settings, finding respectively zero, two and zero tooling references
(`artifacts/rider-package-smoke.log`). No package is published.

The local verification exercises Roslyn analysis, MEF discovery and actual code
actions; it does not drive the Rider UI. See the
[Rider usage instructions](../../docs/postprocess.md#rider-in-place-fixes) for
project reload and quick-fix settings. CI runs the IDE tests in all three
standard platform jobs and the package smoke on Linux; remote CI has not run in
this session.

The final solution Release build succeeds with zero errors
(`artifacts/rider-solution-build.log`). Its 14 xUnit1051 cancellation suggestions
are in the pre-existing standalone empty-block tests; the new IDE projects build
without warnings.

## Custom library API class (2026-09-10)

`--class-name` is supported by managed/shared C# emission and object linking;
`Compiler.EmitCSharp` and `Compiler.LinkObjects` expose the corresponding
optional `className` argument. Omission retains `DotCcLib`. The selected name is
used in the declaration, static imports, function-owner aliases and native export
wrapper calls. Invalid identifiers, incompatible modes and conflicting translated
symbols are rejected. Keywords are escaped without altering user string literals.

All **39 managed/shared library regressions** pass, including custom names,
object linking, globals initialized from callbacks, canonical/runtime pointers,
keyword names, separate consumer execution and validation errors
(`artifacts/class-name-functional.log`). CLI smoke covers both option spellings,
linking, the unchanged default and rejected modes/names. The compiler builds
without warnings and publishes as NativeAOT; its native executable successfully
emits a custom class (`artifacts/class-name-compiler-aot-smoke.log`).

SQLite's emission script now selects **Sqlite**, and the host VFS and consumer
imports match it. Fresh emission produces `public static class Sqlite` and
`using DotCcFunctions = global::Sqlite`. The actual SQLite consumer passes JIT
and linux-x64 NativeAOT with SQL/JSONB/FTS5/optional-feature and callback checks
(`artifacts/class-name-sqlite-consumer.log`). Host VFS and threading consumers
also rebuild and pass, including mmap/WAL, lock ranges, snapshots and concurrent
cleanup (`artifacts/class-name-HostVfsTests-run.log` and
`artifacts/class-name-ThreadingTests-run.log`). Old postprocessor snapshots retain
the API name they were emitted with; regenerate snapshots before testing them
against the current product consumers. The separate varargs fixture still uses
the default DotCcLib API.

## In-place postprocessing and SQLite emission integration (2026-09-10)

The CLI now accepts `--in-place` as an alternative to `--output`. Both use the
same semantic tree rewrites. In-place processing preserves source encoding/BOM,
trivia, file attributes and Unix modes, leaves unchanged files untouched, and
revalidates serialized source before replacing files. Hash checks detect input
changes; caught replacement failures/cancellation roll back prior replacements.
If concurrent edits prevent rollback, they are retained along with an explicitly
reported backup of the original source. Replacement is atomic per file, not a
crash-safe transaction across the whole project.

All **62 postprocessor tests** pass (`artifacts/in-place-tests.log`), including
encoding, read-only inputs, serialization failure, cancellation, injected I/O
failure, concurrent edits and symlink targets. The CLI smoke checks snapshot
compatibility, resources/dependencies/caller paths, mutually exclusive options,
in-place rewriting of a linked source, unchanged project/resources, idempotence
including timestamps, and invalid-source failure (`artifacts/in-place-cli-smoke.log`).
The CLI smoke is now included in Linux CI. Existing generated-source warnings
remain; the postprocessor itself builds without warnings.

`emit-engine.sh` now runs dotcc, builds the separate postprocessor tool, restores
the emitted project, and processes it in place. `--no-postprocess` retains raw
emission for baseline comparisons and Rider experiments. `build.sh` then compiles
the processed TranslatedSqlite; it still does not run tests.

The complete updated emission path rewrites **24,123 Cond.B calls**, skips none,
and removes **2,208 standalone empty blocks**, updating only `Program.cs` during
the rewrite. Its source text exactly matches the fresh comparison snapshot at
`generated/postprocess-in-place-validation/Optimized`, with no remaining Cond.B
calls or temporary replacement files (`artifacts/in-place-sqlite-comparison.log`).
MSBuild's `obj` metadata is regenerated separately as usual. The actual processed
engine passes `SQLITE_AOT=1 scripts/test-managed-consumer.sh` under JIT and
linux-x64 NativeAOT, including SQL CRUD/joins/windows, JSONB, FTS5, math/percentile/
metadata, WAL and cached managed callbacks (`artifacts/in-place-sqlite-consumer.log`).

ManagedConsumer’s solution now lists only ManagedConsumer and TranslatedSqlite;
SQLite's `Directory.Build.targets` retains the managed VFS sources and no longer
injects the optional IDE analyzer/code fix. The reduced solution builds in Release
(`artifacts/in-place-consumer-solution-build.log`). The package smoke still loads
the standalone IDE tooling for an explicitly configured project and confirms zero
DotCC postprocessor analyzers in SQLite’s ordinary and design-time evaluations
(`artifacts/in-place-analyzer-isolation.log`).


## Split generated sources for Rider (2026-09-10)

The compiler now supports project source layouts `--split=none|function|size`,
with a positive UTF-8 byte target (`--split-size`, default 262144) for size mode.
Function boundaries come directly from backend emission and are recorded in new
object fragments. No Roslyn dependency enters dotcc. Existing single-string APIs
and the general CLI default retain single-file output; old objects still link
without splitting. New named-source APIs support both emission and linking.

Partial classes share all translated methods. `Program.cs` keeps runtime, types,
globals and initialization together; aliases are emitted once as global usings.
A generated-file manifest removes obsolete split files on re-emission while
preserving unlisted sidecars. Size grouping appends a whole function before
checking whether the file first exceeds its target; it never splits a function.
The shared declaration and alias files are outside that target.

All **1,871 unit tests** pass (`artifacts/source-split-unit.log`). The full
functional suite passes **340 tests**, with **921 optional oracle tests skipped**
(`artifacts/source-split-all-functional.log`). The managed-library subset passes
**50 tests**, including direct/object-linked splitting, custom/escaped API names,
executable and shared-library compilation, calls across files, cached callback
identity, static-local state, UTF-8 grouping at an exact boundary, indivisible
large functions, determinism, obsolete-file cleanup and old-object diagnostics.
The full runs use an isolated `TMPDIR=artifacts/source-split-tmp`; an earlier unit
run was stopped after tracing excessive header discovery under the crowded system
`/tmp`, then passed in 46 seconds with that isolated directory.

SQLite’s script defaults to size grouping and then runs the existing in-place
postprocessor. The actual engine still rewrites **24,123 Cond.B calls**, skips
none, and removes **2,208 standalone empty blocks**, updating 17 source files.
The resulting **19 emitted C# files** comprise:

- **17 function groups**, 224,279–279,208 UTF-8 bytes after postprocessing.
- **Program.cs**, 1,678,195 bytes for shared declarations/runtime.
- **Dotcc.GlobalUsings.g.cs**, 287,458 bytes for shared aliases.

No Cond.B call sites or temporary replacement files remain. Measurements are in
`artifacts/source-split-sizes.log`. `SQLITE_AOT=1 scripts/test-managed-consumer.sh`
passes JIT and linux-x64 NativeAOT against this regenerated, processed engine,
including SQL/JSONB/FTS5/optional features, WAL and managed callback contracts
(`artifacts/source-split-sqlite-consumer.log`). The consumer solution still
contains only ManagedConsumer and TranslatedSqlite, without analyzer/code-fix
references. Use `SQLITE_SOURCE_SPLIT=none|function|size` and
`SQLITE_SOURCE_SPLIT_SIZE=<bytes>` to override SQLite’s layout.

The compiler itself also publishes as linux-x64 NativeAOT without warnings
(`artifacts/source-split-compiler-aot.log`). Its native executable passes CLI
smoke checks for all three layouts, automatic build/run, stale-file cleanup,
invalid flag combinations and split object linking
(`artifacts/source-split-cli-smoke.log`).


## Function filenames and SQLite default (2026-09-10)

Function mode now emits `<class>.<function>.cs`, such as
`Sqlite.sqlite3_open.cs`. Numeric suffixes appear only on filename collisions;
case-insensitive comparison makes the output portable (`Sqlite.Open.cs` and
`Sqlite.open.2.cs`). Size mode uses `<class>.<index>.cs`. Identifier escapes are
omitted from filenames. The manifest accepts legacy `Dotcc.Functions.*.g.cs`
entries so regeneration removes obsolete files, and cleanup precedes writing to
handle case-only class renames safely.

SQLite now defaults to `SQLITE_SOURCE_SPLIT=function`, producing **3,169 function
files** plus the shared Program.cs and alias file. No old `Dotcc.Functions.*`
files remain. In-place postprocessing rewrites the same **24,123 Cond.B calls**
and removes **2,208 empty blocks**, updating **2,511 files**. All **51 managed-library
tests** pass, including exact filenames, escaped class names, collisions and
legacy cleanup (`artifacts/class-file-names-tests.log`). The compiler builds
without warnings, and ManagedConsumer passes its JIT SQL/JSONB/FTS5/WAL/optional
feature and callback contracts on the freshly regenerated engine
(`artifacts/class-file-names-consumer.log`).


## Generated namespace (2026-09-10)

SQLite now emits `Managed.Database.Sqlite` and places its translated aggregate
and runtime types in `Managed.Database`. The script supplies `--namespace
Managed.Database`; function filenames remain `Sqlite.<function>.cs`. Consumer
and handwritten VFS imports, product layout checks, and usage documentation
follow the namespace. Existing generated snapshots need regeneration before
building against these updated consumers.

The compiler supports namespaced single-file/project executable output,
managed/shared libraries, all source layouts, and object linking. Names are
validated and C# keywords escaped. Objects retain symbolic aliases until link
time; older objects need regeneration for namespaced linking. The compiler and
runtime acquire no Roslyn dependency. The separate postprocessor and optional
IDE tools now prove the bound Cond.B/CBool helpers in their actual namespace.

Validation:

- **1,871 unit tests** and **354 functional tests** pass; **921 optional oracle
  tests** are skipped (`artifacts/namespace-unit.log`,
  `artifacts/namespace-all-functional.log`). The functional cases cover namespace
  validation, escaped components, source layouts, object linking, enum shadowing,
  cached callbacks, executable entry points, runtime types and string preservation.
- **64 postprocessor tests** and **31 analyzer/code-fix tests** pass, with one
  opt-in SQLite snapshot test skipped (`artifacts/namespace-postprocess-tests.log`,
  `artifacts/namespace-analyzer-tests.log`). Namespaced helper rewrites preserve
  execution and remain idempotent; unproven helpers are skipped.
- SQLite regeneration rewrites **24,123 Cond.B calls**, skips none, and removes
  **2,208 empty blocks**, updating **2,511 files**. ManagedConsumer passes JIT and
  linux-x64 NativeAOT SQL/JSONB/FTS5/optional-feature/WAL/callback checks
  (`artifacts/namespace-sqlite-consumer.log`).
- HostVfsTests and ThreadingTests pass, including WAL, mmap, concurrent connection
  and callback contracts. ProductLayout passes **41 offsetof contracts**, **67
  actual field offsets** and **8 pointer-array storage checks**
  (`artifacts/namespace-HostVfsTests-run.log`,
  `artifacts/namespace-ThreadingTests-run.log`,
  `artifacts/namespace-ProductLayout-run.log`).
- The compiler publishes as linux-x64 NativeAOT without warnings. Its native CLI
  passes executable project and file-based runs, split object linking, legacy
  object diagnostics/global linking, and invalid option checks
  (`artifacts/namespace-compiler-aot.log`, `artifacts/namespace-cli-smoke.log`).


## Host VFS namespace (2026-09-10)

Moved all HostVfs partial files, HostMutex and platform helpers into
`Managed.Database`, alongside the translated SQLite types. Updated static imports
and consumer references and removed the redundant global namespace import.
ManagedConsumer, HostVfsTests, ThreadingTests and HostVfsPlatformTests all build
and run successfully in Release on Linux. This covers SQL/JSONB/FTS5, WAL/mmap,
mutexes, concurrent connections and the simulated Darwin handle-close contracts.
Logs: `artifacts/hostvfs-namespace-<project>-build.log` and
`artifacts/hostvfs-namespace-<project>-run.log`.


## Upstream Unix VFS experiment and macro constants (2026-09-10)

The default product still uses HostVfs. A separate
`Managed.Database.UpstreamUnix.Sqlite` build enables the actual upstream
`os_unix.c` inside SQLite 3.53.4's amalgamation. Only the existing guarded APPDEF
adaptation changes the reference source. Dotcc translates the VFS algorithms;
P/Invoke plus a small Linux x64 OS shim supplies the system calls and ABI
structure conversion. No native SQLite engine is linked.

This exposed the unsupported file-scope `static T *(*const finder)(...)`
declarator. Added the `function-pointer-constant` execution fixture and extended
the parser/IR binding, preserving const pointer qualification. The callback-array
and new scalar callback fixtures pass. The real Unix engine then compiles.
JIT and NativeAOT runs pass disk CRUD, rollback and child-process writer locking,
WAL snapshots/checkpoints, actual mmap fetch/unfetch, JSONB, FTS5, integrity,
reopen persistence and mutex cleanup. Native and managed assertions agree on the
96-byte portable stat bridge and its 40-byte size-field offset. The OS shim is
copied transitively with the generated project into build/publish output.
Logs: `artifacts/upstream-vfs-probe/unix-final-build.log`, `unix-final-run.log`,
`unix-final-aot-build.log` and `unix-final-aot-run.log` in the same directory.

The Windows probe enables `os_win.c` but fails on absent `windows.h` declarations
(first downstream error at `HANDLE` in `winFile`, amalgamation line 49045).
This is recorded in `artifacts/upstream-vfs-probe/windows-emit.log` and
[the experiment document](upstream-vfs.md). Windows bindings and execution remain
unfinished; HostVfs is still available for all of its existing platforms.

Dotcc now emits **2,079 numeric/string macro constants** on the product's
`Managed.Database.Sqlite` class. ManagedConsumer uses exported result codes,
`SQLITE_VERSION`, `SQLITE_UTF8`, and `SQLITE_CHECKPOINT_TRUNCATE`, including an
actual truncate checkpoint API call. Product regeneration and in-place
postprocessing still rewrite **24,123 Cond.B calls**, skip none, and remove
**2,208 empty blocks** across **2,511 updated files**. ManagedConsumer builds
with zero errors and its 63 existing generated-code warnings, then passes all
SQL/JSONB/FTS5/optional-feature/WAL/callback checks.
Logs: `artifacts/macro-constants-sqlite-emit.log`,
`artifacts/macro-constants-consumer-build.log` and
`artifacts/macro-constants-consumer-run.log`.

All **1,871 unit tests** and **358 functional tests** pass; **923 optional oracle
tests** are skipped (`artifacts/macro-constants-unit.log`,
`artifacts/macro-constants-functional.log`). Macro regressions cover typed
literals, escapes/concatenation, casts, unsigned wrapping/conversions, aliases,
conditional expressions, contextual/undefined/unsafe replacement omission,
member collisions, source layouts and conflicting object/source definitions.
The compiler itself publishes as linux-x64 NativeAOT without warnings
(`artifacts/macro-constants-compiler-aot.log`).
Its native executable also emits and runs a namespaced file-based program using
macro constants and a const scalar callback (`artifacts/macro-constants-native-smoke.log`).


## Generated shared filenames follow the API class (2026-09-10)

The shared source is now `{class_name}.cs` for both split and unsplit output;
split aliases use `{class_name}.GlobalUsings.g.cs`. The product therefore emits
`Sqlite.cs` and `Sqlite.GlobalUsings.g.cs`, alongside its existing function files.
The output manifest migrates the previous filenames automatically and preserves
unlisted sidecars. Executable projects use `DotCcProgram.cs`.

All 67 managed-library tests pass, covering direct emission, object linking,
source layouts, escaped class names, namespace support and obsolete-file cleanup
(`artifacts/class-filenames-tests.log`). The compiler builds without warnings
(`artifacts/class-filenames-compiler-build.log`).

Product regeneration and in-place postprocessing succeed, preserving the
24,123 Cond.B rewrites and 2,208 empty-block removals. Product layout validation
passes all 41 offset contracts, 67 actual field offsets, aggregate size/alignment
checks and eight pointer-array storage checks. ManagedConsumer builds and passes
its SQL, JSONB, FTS5, optional-feature, WAL and callback workloads.
Logs: `artifacts/product-layout-emission.log`,
`artifacts/class-filenames-product-layout.log`,
`artifacts/class-filenames-consumer-build.log` and
`artifacts/class-filenames-consumer-run.log`.

The separate upstream Unix VFS experiment was regenerated with the same filenames
and passes its disk CRUD, cross-process locking, WAL, mmap, JSONB and FTS5 checks
(`artifacts/class-filenames-upstream-unix.log`).


## Preupdate hook (2026-09-12)

Enabled `SQLITE_ENABLE_PREUPDATE_HOOK` in the shared native/translated profile
and regenerated the managed product with its normal in-place postprocessing.
The newly active C paths translate and compile without compiler changes or
warnings. All six preupdate APIs are emitted on `Managed.Database.Sqlite`.

`SQLITE_AOT=1 scripts/test-managed-consumer.sh` passes under JIT and linux-x64
NativeAOT. The new callback test checks the compile option, retained managed
context across GC, connection/database/table identity, INSERT/UPDATE/DELETE
operation codes, old/new values (including NULL and UTF-8 text), changing rowids,
column counts, trigger depth, the ordinary-operation blob-write marker, and
unregistration with the returned context and no subsequent delivery. Existing
SQL, JSONB, FTS5, WAL, endian and callback workloads also pass.
Log: `artifacts/preupdate-consumer.log`.

`SQLITE_AOT=1 scripts/test-product-layout.sh` passes against the native profile
with the new flag: 41 offsetof contracts, 67 actual field offsets, 48 aggregate
size/alignment checks and eight inline-array storage checks under both JIT and
NativeAOT. Log: `artifacts/preupdate-product-layout.log`.
