# PGlite campaign-start validation

## Current full-source and managed execution gate, 2026-10-22

Coherent receipt `artifacts/campaign/20261022-194223-e06ea2a0/receipt.json`
emits and strict-links all 876 selected backend/module/libc/adapter units. The
complete raw backend C# build passes with zero errors and 12 warnings; the
separate 73-unit original initdb companion also emits, links and builds. The
receipt is failed because the explicit publication guard remains enabled, not
because source emission or the raw build failed. A separate private full-backend
processed diagnostic passes semantic processing and C# build with zero errors
and 12 warnings (`artifacts/supervisor-processed1942/report.json`). This is
backend build evidence only; processed initializer and SQL qualification remain open.

The actual API and canonical consumer build against these complete libraries
with zero errors. Two independent, isolated actual initdb invocations both run
the unchanged 973545-byte BKI bootstrap successfully, then execute the original
251748-byte post-bootstrap SQL. The first runtime failure is original
`SELECT pg_import_system_collations('pg_catalog')`: `locale -a` returns
`Operation not supported`, so single-user mode exits 1. No managed exception or
consumer SQL success is inferred. The coordinator is repairing real locale
enumeration; the import and SQL inputs are retained unchanged. Calendar `%Z`
formatting is an independently observed host defect under repair.

Evidence: `artifacts/initdb-owner-20261022-r3/actual1942/initialization/execution/`
and `artifacts/product-owner-r3/raw1942-consumer-{build,run}.log`; original
backend inputs, outputs and operation/status records are retained in private
diagnostic directories. All required SQL, shutdown/reopen and four product
matrix rows remain open. The latest full shared compiler suite has 3420 passes
and the same 16 failures reproduced in the untouched baseline; current
comparison is `artifacts/supervisor-portable/regression-comparison-current9.json`.


## Runtime repair retry, immutable19

The real locale inventory service now launches only the fixed OS locale utility
with `-a`, using native posix_spawn descriptor actions and real waitpid status.
It does not launch a shell or database executable, manufacture/filter the locale
list, or remove original collation import. Owner-local pipes preserve real
stdout/stderr and are closed/killed/reaped on abandonment. Direct JIT and trimmed
NativeAOT each pass 53 host process assertions; actual emitted C also compares
locale bytes to native across three owners in both modes. Evidence:
`artifacts/initdb-owner-20261022-r3/locale-process-repair.json`.

[Calendar timezone](CALENDAR.md) formatting passes native/emitted JIT/AOT in all
five selected zones. Immutable19 at `build/tools/supervisor-20261022-2008/`
contains both fixes. The complete coherent1942 object sets (876 backend and 75
frontend objects) strictly relink with these runtime resources into private
projects; `artifacts/supervisor-runtime2008/report.json` records original
commands and object/tool hashes. This is a resource-only diagnostic, not fresh C
regeneration or publication. Real original initdb and the canonical consumer
are running against these projects with separate fresh data/output directories.
No SQL success or final matrix row is inferred before their actual results.

## Active implementation continuation

The coordinator continues through the full product and P6; historical baseline
receipts below are intermediate evidence. Current work is tracked in
[IMPLEMENTATION.md](IMPLEMENTATION.md).

Receipt `20261022-170306-5fe5032a` passes the complete real fork libc managed
build and the native/emitted portable CRC32C/popcount suite (771 length/alignment
cases plus known vectors). This removes the four unresolved libc host names;
process fallback returns documented errors and is not an embedded initializer.

The shared GNU statement-expression loop reduction fails on previous tools,
passes native and actual emitted direct/object execution after repair, and
retains rejection of outward transfers. Eighteen focused syntax checks pass.
Full-source receipt `20261022-170525-3774461c` advances through the brin units
and fails at a separate for-condition comma parser gap in actual `tupdesc.c`.
No complete product or managed SQL acceptance is claimed.

Updated 2026-10-22. This is an implementation start, not acceptance of a complete
PostgreSQL port. Current evidence uses Linux x64, .NET SDK 10.0.112 and the exact
source/configuration pins in `config/`. Logs, private source stages and receipts
are ignored outputs beneath `artifacts/`, `build/` and `ref/`.

## Passed observations

- Native `embedding-native` PGlite variant: configure, generated inputs, full
  native build/install, then 21 SQL protocol cases passed with exact row values,
  command completions, SQLSTATEs and transaction states. Cases cover UTF-8 text,
  NULL, commit/rollback, duplicate/aborted-transaction/syntax errors, recovery,
  clean shutdown and persistence over three fresh native test processes.
  Source-owner receipt: `artifacts/native-control/20261022-162405-b42aad97/receipt.json`;
  detailed report `native-control-report.json` and original `*.wire` transcripts.
  Wrong-row negative assertions are rejected; invalid harness arguments produce
  exit 24. Initdb uses native helper subprocesses for control setup only.
- Complete real fork source objects `pglitec.c`, `pg_lzcompress.c`, `bitmapset.c`
  emit after shared repairs. Receipt:
  `artifacts/campaign/20261022-162338-b253238e/receipt.json`.
  This four-unit diagnostic selection is not the complete backend/initdb product.
- Actual native/emitted LP64 boundary fixture, wide signed callbacks, pinned
  UTF-8 bytes across GC, allocation/free and lexical nested jumps pass. Real
  fork callback and cleanup function bodies pass as guarded unmodified source
  slices; actual full fork native shared-memory and error-exit controls pass.
- Native actual PostgreSQL-header layouts have been measured. The final
  actual native/emitted PostgreSQL-header comparison passes all 13 reported
  fields. The fixture provides only the real declared CurrentMemoryContext
  pointer storage; it implements no memory-context behavior.
- Canonical layout/discovery passes. Offline fetch with `--hashes off` works
  from `/tmp` through the absolute wrapper path, using the existing local input.

## Shared repairs and regression scope

`d597dcd9a` adds Linux LP64 IPC data declarations and opaque sigjmp_buf storage,
and repairs volatile array typedef decay. It provides no kernel shared memory
or signal-mask implementation. Native layout reduction agrees with emitted
48/112-byte records and relevant offsets. Twelve POSIX/jump fixtures and nine
volatile/member tests pass, including direct/object-linked C# execution.

`25f84c9dd` accepts valid C11 `_Noreturn extern` declarations while preserving
external linkage and the noreturn contract. Parser tables are regenerated by the
full build. Ten focused unit tests, emitted execution and native fixture pass.
The full current unit suite has 3,257 passes / 16 failures. An isolated archive
of untouched repository baseline (`config/campaign.json`) has 3,256 passes /
the identical 16 failures; no new failing names. Original logs and comparison:
`artifacts/discovery/dotcc-tests-{all,baseline}.log`, `baseline-comparison.json`.

The shared framework suite has 69 passes / one pre-existing failure in Blink's
checksum-literal metadata check. No Blink files were edited. See
`artifacts/baseline/pglite-framework-tests.log` and BASE-001 in BLOCKERS.md.

## Required failed, blocked and unrun gates

- `postgres.c` initially fails atomic compiler-port selection. The reviewed
  C11 port is integrated and its native/emitted JIT controls pass; the next
  actual full-TU failure is separate spinlock-port selection. See ATOMICS.md. Atomics/fences must remain real operations.
- Complete pglitec private managed-library build fails on missing host services
  `popen`, `pclose`, `optind`, `getpagesize`, despite successful object emission.
- PGlite synthetic exit 99/100 needs a resumable host boundary. Current owner
  termination is terminal; tested typed `exit(int)` override is rejected.
- Signal masks, signal old-action behavior, timers, complete initdb/source/import
  closure, actual managed startup/SQL/error recovery and same-process reset are
  incomplete. Required full product raw/processed × JIT/NativeAOT execution,
  consumer and clean shutdown/reopen remain unrun.
- Missing-product consumer build fails explicitly. That is expected negative
  packaging behavior, not a passing useful workflow. No product is published.
- Upstream wasm32/Emscripten profile and its builder are investigated/pinned but
  not built. No cross-ABI database-file interchange or profile parity is claimed.

## Reproducible entrypoints

```bash
bash Scripts/campaign.sh layout pglite --check
bash pglite/scripts/fetch.sh --fetch never --hashes off
bash pglite/scripts/test.sh --suite inventory --fetch never
bash pglite/scripts/test.sh --suite native-control --fetch never
bash pglite/scripts/probe.sh --unit pg_lzcompress.c --fetch never --tools reuse --jobs 1
bash pglite/scripts/translate.sh --fetch never --tools reuse --jobs 1
bash pglite/scripts/test.sh --suite libc-build --fetch never --tools reuse
bash pglite/scripts/test.sh --suite boundary --fetch never --tools reuse
bash pglite/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
```

Probe performs object emission only. Normal translation attempts the current
selected source set and refuses publication of an incomplete product. Suite
failures preserve receipts and logs. Ordinary test commands snapshot existing
tools without rebuilding root outputs. Final integrated receipts and exact
milestone states are recorded below after the combined review run.

## Final integrated receipts and milestone status

All paths below are beneath `artifacts/campaign/` and contain `receipt.json`,
exact command arrays/tool snapshots, and original logs. Overall failed receipts
remain failures even where their smaller probes pass.

- `20261022-163944-c778dbf6`: **passed** normal recipe `native-control`; 21 SQL
  assertions, clean native lifetimes, negative controls and the authoritative
  863-unit native backend/common/port/timezone source manifest.
- `20261022-163657-6b1f4069`: **failed boundary feasibility**; actual-header native/
  emitted ABI comparison and callback/memory/jump probes pass, while exit/signal/
  timer requirements reproduce four blockers.
- `20261022-163734-05c8ab0d`: **failed** `libc-build` C# build; full fork libc
  emission passes, four host names remain unresolved.
- `20261022-164001-838140a2`: **failed complete-input gate** in `atomics`; original
  native, C11 native and emitted JIT atomic/layout controls agree. Four workers
  reach 20,000 increments and 2,000 payload handshakes pass. Actual postgres.c
  and its reduced actual-header fixture reproduce missing spinlock support.
- `20261022-164034-40490bc6`: **failed** normal default translation of the whole
  observed backend selection: 863 backend C units plus actual libc and authored
  forwarding adapter = 865 selected inputs. libc emits; first backend brin.c
  reaches the same spinlock error. No output is published. This is an attempt
  against the full observed backend selection, not a claim that all units emitted
  or that the separate initializer closure is complete.
- `20261022-164133-d738135d`: **passed** three-unit diagnostic probe using final
  tools/preparation/atomic-overlay integration: complete pglitec, pg_lzcompress,
  bitmapset objects. Scope is object emission only.
- `20261022-164226-fa7a6ae2`: **passed inventory**; source selection has 863 unique
  backend units. `config/backend-units.json` is a portable frozen conversion of
  actual native compile/link inputs, including per-context includes/defines.
- `20261022-164227-7558bda9`: **failed/blocked consumer prerequisite**, missing
  complete generated library. Requested matrix was raw/processed × JIT/AOT;
  no runtime row executed. Direct root solution build fails MSB3202 on that same
  absent project (`artifacts/discovery/solution-final.log`).

The normal recipe discovers correctly and layout, Python/Bash syntax and
whitespace checks pass. From `/tmp`, the absolute fetch wrapper succeeds offline
with hashes disabled (`artifacts/discovery/fetch-off-unrelated-cwd.log`). The two
pre-existing shared plan files retain their original bytes and are excluded from
campaign commits. Ref/source stages, tools, generated libraries and all logs
remain ignored. No push or PR was created.

P0 is complete for the scoped campaign baseline: exact inputs/profile, native
useful control, actual source/ABI inventory, canonical recipe/consumer, real
translation attempts and reduced/repaired findings. P1 is **partial**: actual
PostgreSQL ABI and C11 atomic feasibility pass; spinlocks, host imports,
resumable exit/signals/timers and init/reset remain blocked. P2–P6 remain open.
The next concrete step is a reviewed one-byte PostgreSQL spinlock port retaining
real TAS and S_LOCK contention semantics, followed by full-source retry. Host
initdb, resumable-yield/error and resource reset remain separate required work.

To refresh the observed source selection after an intentional upgrade, run the
native suite and convert its source manifest with:

```bash
python3 -B pglite/scripts/update-manifest.py PATH/native-source-manifest.json \
  --stage PATH/configured/source
```

The converter retains native codegen flags as evidence and separates them from
DotCC's declared dialect and include/define selection. It never treats the
backend closure as complete embedded initialization.


## Continuing implementation — compiler and source closure

Immutable tools `supervisor-20261022-1735` through `1749` include structural
repairs discovered in original initdb/getopt and PL/pgSQL inputs: pointer-valued
assignments retain their C type despite nint-backed storage; explicit integer
constant casts apply width/sign conversion before promotion; file-scope union
forward declarations parse. Native controls and seven direct/object functional
cases pass (`artifacts/supervisor-portable/pointer-cast-functional.log`). All six
original PL/pgSQL units then emit, and original getopt executes in two managed
state contexts (initializer-owner evidence in INITIALIZATION.md).

Current shared unit run: 3,355 pass, 17 fail in
`artifacts/supervisor-portable/unit-current2.log`. Sixteen failure names exactly
match the already recorded untouched baseline; the seventeenth is the new
POSIX-host owner's NSS assertion and remains assigned for repair. No new
compiler regression is hidden in the baseline count.

Diagnostic backend suffix receipt `20261022-174825-772d7ead` emits another
42 units through execExpr.c before execExprInterp.c selects GCC computed goto.
The profile now disables that unimplemented compiler capability and retains
PostgreSQL's original switch interpreter. The full current source regeneration,
managed SQL, persistence and all four product matrix rows remain required.


Current shared regression run `artifacts/supervisor-portable/unit-current3.log`
has **3,380 passes and exactly the same 16 baseline failures**, with no new
failure names. The new NSS test now accepts the actual native provider's numeric
no-entry outcome instead of assuming one universal outcome. Comparison is in
`regression-comparison-current3.json`; no baseline reinvestigation was repeated.
Array-member and multiple string-array declaration native/direct/object controls
pass; existing statement-expression object cases remain passing. Signal and
structural declaration integration is committed with focused and full-suite
evidence; complete backend generation and product execution remain active.
