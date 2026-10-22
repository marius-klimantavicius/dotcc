# Source and native control progress

Implementation owner: source/configuration/preparation/native control only.
Shared compiler outputs, recipe, root integration and commits belong to supervisor.

## Source pin (2026-10-22)

Live `git ls-remote --symref https://github.com/electric-sql/postgres-pglite.git HEAD`
and GitHub repository API agree on default branch `REL_18_6-pglite` and commit
`29b6424aa312ec3556247db069939006dd27f878`. Metadata is available now in
`pglite/config/sources.json`. Reference destination:
`pglite/ref/postgres-pglite-29b6424aa312ec3556247db069939006dd27f878`.

Fork instructions are `README-PGLITE-DEV.md`, `build-pglite.sh`,
`build-with-docker.sh`. Embedding requires `__PGLITE__`, unrolled backend
entrypoints, libc interception and Emscripten-specific support; a normal native
PostgreSQL configuration is only a baseline. Wrapper image declared by the pin:
`electricsql/pglite-builder:3.1.74-7`. Resolving its immutable metadata separately.

## Integration interface (implementation in progress)

`scripts/prepare.py:prepare(ctx)` will preserve `ctx.sources['product']`, prepare
a private native/source stage beneath `pglite/build/native-control/`, and return
stage/build/generated-header information and a source manifest to the parent
recipe. `scripts/native-control.py:run(ctx, suite=None)` will expose the native
workflow with framework command logs. No DotCC invocation occurs in these helpers.

Acquisition/configure/build/native assertions are pending. No native or managed
success is claimed yet. Source licenses will remain in acquired and staged trees
and be identified in the generated manifest.

## Current implementation interface

`scripts/prepare.py` now exports `sources(root)`, `prepare(ctx)` and
`build_native(ctx, info)`. `prepare` returns a JSON-compatible dict with
`source`, `build`, `prefix`, `libc_object`, ordered `include`, `defines`, `flags`,
`generated_inputs`, local `cache_identity`, and explicit classification. Source
inputs come from `ctx.sources['product']` or framework acquisition. Native stage
is private under `pglite/build/native-control/<identity>/source`; all commands use
`ctx.run`; native make is fixed `-j2`. `native-control.py:run(ctx, suite=None)` is
available for recipe suites. Standalone specialist runner creates its own
framework receipts under `artifacts/native-control/` and does not update the
supervisor's campaign receipt pointer.

The current native variant sets `__PGLITE__` and the upstream libc macro overrides,
links the original `pglitec.c`, uses Linux LP64 headers and excludes ICU/XML/TLS/
compression/extensions. It is not the upstream wasm32 profile. Forced stdbool.h
and stdlib.h includes supply standard declarations missing from upstream
pglitec.c on a native GNU C11 build. No upstream source text has been patched.
Native configure/build launched; meaningful SQL assertions remain pending.

## Build evidence available

`python3 -B pglite/scripts/native-control.py --build-only --fetch never`
passed configure, generation, complete native make and install using framework
commands. Receipt: `artifacts/native-control/20261022-161556-2ed58248/receipt.json`.
Stage: `build/native-control/7fb2c283d42041d6f91e/source`.
This is an actual __PGLITE__ native variant build, not a normal PostgreSQL build.
No SQL behavior has passed yet. The upstream script's `--disable-spinlocks`
option is unrecognized by this 18.6 configure, so effective native settings must
be inspected rather than assuming that option worked. Native native setup uses
upstream initdb with explicitly test-only helper subprocess support; embedded
initialization in the delivered managed product remains unproved.

Parent interface clarification: `include` currently contains ordered string
paths and can be used as `include_dirs`; `prepared.json` in the stage's parent
is the detailed preparation manifest. Aliases will be added in final integration.

## Native embedding harness findings

Native initdb setup completed successfully in
`artifacts/native-control/20261022-162119-6b6860d5/native-initdb.log`.
A test-only constructor supplies upstream pgl_system callback through real native
system() for initdb's helper processes. It is not product initialization.
The first actual embedded startup yielded 99 as expected, then `pgl_startPGlite`
raised synthetic exit 100 / errno EPERM. Native epoll rejected upstream's fake
socket descriptor 1 because framework stdout is a regular log file. Test host
now supplies a real pollable AF_UNIX socket at descriptor 1; SQL data still uses
upstream pgl_read/pgl_write callbacks. Retry is running. This native descriptor
requirement is a host-service distinction from Emscripten, not an SQL engine fix.

## Actual native embedding workflow passed (16:24 local)

`python3 -B pglite/scripts/native-control.py --fetch never` PASSED all 21 SQL
case assertions, including result rows, command tags, SQLSTATEs, transaction
ReadyForQuery states, UTF-8, NULL, rollback/commit, duplicate key + aborted
transaction recovery, syntax-error recovery, clean shutdown and persistence over
two fresh-host reopen cycles. Actual upstream backend objects run in-process
inside `tests/native/embedding_control.c`; callbacks carry PostgreSQL wire frames.
No hand-written query engine or native wrapper is delivered as the product.

Evidence: `artifacts/native-control/20261022-162405-b42aad97/receipt.json`,
`native-control-report.json`, per-phase logs and original `.wire` transcripts
under `build/native-control/runs/20261022-162405-b42aad97/native-control/`.
Wrong-row negative assertion was rejected; invalid native harness args propagated
exit 24. Synthetic query errors 100 recover through PostgresMainLongJmp; startup
99 retains backend state. Shutdown uses upstream proc_exit(0) and cleanup.

Native host fix: pq_init requires both a pollable dummy fd 1 and a live
postmaster-death pipe on Linux. Test host supplies those OS primitives, then
uses unchanged upstream pgl_startPGlite and callback overrides. No engine text
was modified. Initdb has native helper subprocesses (setup oracle only), and each
reopen is a new native test process. Embedded product initialization, same-process
reinitialization, upstream wasm32 profile equivalence, managed SQL and managed
JIT/AOT remain unproved. These caveats must accompany the passed native claims.

Finalizing reproducible backend/common/port source and generated-header manifests
and SOURCE.md; parent recipe can already run the native-control suite.

## Final source-owner handoff (16:39 local)

Final GNU C11 variant invocation:
`python3 -B pglite/scripts/native-control.py --fetch never`.
PASSED receipt and SQL report:
`artifacts/native-control/20261022-163448-222180a4/{receipt.json,native-control-report.json}`.
21 cases passed, 0 failed; two fresh-process reopen cycles; actual error recovery,
callback bytes, clean shutdown; negative wrong-row and exit-24 controls passed.
All 94 logged commands are framework commands. Native builds remained `-j2`.

Final current native stage:
`build/native-control/14abb02f16cfb8013f7c/source`.
`native-source-manifest.json` in that stage's parent and final artifact directory
resolves 863 actual backend/timezone/common/port C TUs through upstream link
inputs and archive members, plus separately compiled original pglitec.c (864
C inputs including libc). `native-build-manifest.json` records 169 generated or
changed C/header/config inputs, actual tools, 8-byte long/pointer/size_t/off_t,
source content identity and notices. The acquired source fingerprint was checked
unchanged after generation/build. No source engine text was adapted.

Effective generated features include SSE4.2/AVX512 CRC and AVX512 popcount with
runtime selection, SysV shared memory behind pglitec interception, and unnamed
POSIX semaphores. Parent must make an explicit translation-profile decision for
native CPU intrinsic units; this manifest is actual native evidence, not a claim
that those intrinsics are translated. Complete embedded-init product closure
remains false even though the native backend linker closure is complete.

Final helpers return JSON-compatible `source`, `build`, `prefix`, `libc_object`,
`include_dirs` and alias `include`, `defines`, `flags`, `manifest`, ABI/config
and provenance information. `build_native(ctx, info)` adds `source_manifest`,
`backend_unit_count` and refreshed generated inputs. `native-control.py:run(ctx,
suite=None)` is ready for the parent recipe. Standalone mode uses framework
Context with specialist-owned receipts and does not overwrite the supervisor's
campaign pointer. --prepare-only/--build-only receipts explicitly state SQL
unrun. Invocation from /tmp passed: prepare-only receipts
`20261022-163648-aeb248d4` and `20261022-163832-268a3c19`. The latter includes final
explicit scope metadata. Owned Python/JSON syntax, strict C host syntax
(-Wall -Wextra -Werror), and whitespace checks passed.

Failed manifest-collector experiments are retained as receipts
`20261022-162644-7833394a` (stopped configure-remake dry-run) and
`20261022-163013-8499cb92` (GNU make intermediate-object elision). Final collector
asks each owning Makefile for explicit object targets while parking/restoring
only private objects, avoiding both behaviors. Final 863-unit closure validation
passed; no native objects remain parked.

Deliverables owned here: config/sources.json, config/profile.json,
scripts/prepare.py, scripts/native-control.py, tests/native/{cases.json,
embedding_control.c,initdb_host.c}, docs/SOURCE.md and this progress file.
The .gitignore was created only when absent and is now shared with supervisor.
No commits, staging, pushes, PRs, delegation, DotCC invocation or shared compiler
builds were performed. Both pre-existing shared plan edits remain untouched.

Remaining product boundaries: embedded initialization without setup subprocesses,
same-process reinitialization and independent instances, upstream WASM/ILP32
profile parity, translated exit/signal-mask services and SQL, raw/processed
JIT/AOT qualification. Native success is separately established above.
