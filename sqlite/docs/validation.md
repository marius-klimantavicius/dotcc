# Validation evidence

Campaign commands run from `sqlite/`; scripts resolve their own absolute roots.
Evidence is recorded in milestone order; later checkpoints supersede earlier
pending statements. Reproduction commands are in `usage.md`.

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
`scripts/check-layout-metadata.py generated/TranslatedSqlite/Program.cs` repeats
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
