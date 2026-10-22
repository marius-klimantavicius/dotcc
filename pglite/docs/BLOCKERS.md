# PGlite blocker ledger

Campaign-start ledger, 2026-10-22. Planned checks are not successes.

## PGL-011 — Multidimensional array designators (fixed)

The original common/jsonapi.c parser table initializes struct cells with
`[nonterminal][terminal] = { ... }`. The grammar previously allowed one index,
and the array interpreter explicitly rejected multidimensional designation.
Index chains now retain each target dimension, validate every bound, fill sparse
scalar/aggregate cells, preserve continuation and later-write replacement, and
zero-fill explicitly selected rows. Irregular partial-row bracing is rejected
rather than silently relocated. Native fixture output matches direct/object
emission, including three dimensions, repeated writes and whole-row replacement.
Actual complete jsonapi.c emits: receipt `20261022-172314-cb2ce1cd`.

Four direct/object control-flow/designator fixture executions pass. Nineteen
affected aggregate checks pass; an additional new BSD typedef check independently
reveals existing compile-time unsigned-small-integer cast truncation behavior
(native unsigned char/short -1 differs from DotCC static assertion folding).
That new finding remains active; the overall combined unit log is failed, not
reported as passing. Logs: `artifacts/supervisor-portable/designators-{native,
before,functional,unit}.log`.

## PGL-009 — Comma expressions in for conditions (fixed)

Actual `tupdesc.c:1072` expands `forfour` into four assignments and a final
boolean condition joined with commas. The grammar accepted only the assignment
tier here, rejecting the first comma. ForCond now uses the existing full Expr
tier; existing IR preserves sequential side effects and the final value.
Native/emitted/object fixture `for-comma-condition` checks declaration, expression
and empty init forms, final false-condition side effects, and continue/update.
Full-source receipt `20261022-170809-7535d470` emits tupdesc and reaches a
separate adjacent-string-message failure in gindatapage.

## PGL-010 — Adjacent static-assert message literals (fixed)

Actual `gindatapage.c:501` uses `dlist_container` and PostgreSQL's fallback
type assertions, whose stringified operand is adjacent to explanatory message
literals. File/block static assertions previously accepted one STRING token;
they now accept the existing StringSeq grammar. Failure diagnostics retain all
original segments. Positive file/block and negative assertion tests pass.
Actual full gindatapage emits in receipt `20261022-171127-7f751d14`.

Combined control-flow/static-assert syntax regressions: 38 pass, log
`artifacts/supervisor-portable/control-unit-combined.log`; direct for-comma and
both loop object fixtures pass. Diagnostic remaining-source receipt
`20261022-171159-54a6f245` advances beyond 50 backend units and reaches missing
`locale_t` in actual `utils/pg_locale.h`; POSIX locale implementation is active.
These diagnostic selections do not replace final complete-source regeneration.

## PGL-008 — GNU statement-expression local loops (fixed, full retry advanced)

Actual `brin.c` reached a `do ... while (0)` inside PostgreSQL's
`StaticAssertExpr` after real spinlock selection. The shared builder rejected
every loop in a statement expression. Internal structured loops now keep their
own break/continue targets, including deferred conditional evaluation; returns
and transfers to enclosing scopes remain rejected. The native reduction prints
`4 3 1 0`; prior immutable tools reject its DoWhile. Eighteen syntax checks and
three emitted/direct/object fixture executions pass. Logs:
`artifacts/supervisor-portable/statement-{before,native,unit2,functional,object}.log`.

Normal full-source receipt `20261022-170525-3774461c` emits the earlier brin
units and reaches `tupdesc.c`'s real `forfour` macro. Its comma-separated for
condition is the next reproduced parser defect, currently under repair. Software
codegen selection and current continuation are recorded in IMPLEMENTATION.md.

## BASE-001 — Existing framework metadata regression

Classification: unrelated repository baseline. Command:
`python3 -B -m unittest discover -s Scripts/campaigns/tests -v`.
Observed: 70 cases, one failure in
`test_campaign_helpers_keep_hash_literals_in_metadata`, which finds an existing
64-hex literal in `blink/scripts/campaign_delivery.py`. Other 69 cases pass.
Expected: helpers keep provenance literals in JSON. No PGlite code is involved;
Blink was not changed. Transcript: `/tmp/pglite-framework-tests.log` (copied into
campaign artifacts for retained evidence). This does not close PGlite validation.

## PGL-001 — Embedding profile, host and ABI closure

Classification: open feasibility, not yet a reproduced compiler defect.
The fork's PGlite path uses `__PGLITE__`, Emscripten and intercepted libc. A
normal x64 PostgreSQL build cannot prove that profile. Experiments: inspect
pinned build scripts and generated inputs, native-control profile, actual fork
entrypoint emission and native/emitted ABI checks. Resolution belongs to P1;
the full embedded workflow and raw/processed JIT/AOT product remain blocked
until the required host/state/error boundaries work.

## PGL-002 — POSIX declarations and volatile array decay (fixed)

Classification: shared header coverage and compiler lowering. Full source
`pglitec.c` initially reported missing `sys/shm.h` and failed parsing its
`volatile sigjmp_buf postgresmain_sigjmp_buf` declaration. Native reductions
passed. Adding the opaque signal-jump storage alias exposed C# CS0306: a volatile
array typedef decayed through `Volatile.Read<ulong*>`, an illegal pointer generic
argument. Array decay computes an address; volatility belongs to the elements.
The backend now retains that distinction. LP64 `ipc_perm`/`shmid_ds` data records
are supplied without kernel IPC implementations. Signal-mask operations are
still unimplemented; the alias does not claim them.

Regression: `Fixtures/sysv-shm-layout`, `Fixtures/sigjmp-buffer-type`, and
`ManagedLibraryTests.VolatileArrayDecay` direct/object execution. Native record
sizes 48/112 and offsets 48/56/80/88 match emitted observations; 64-bit size and
signed key/time storage are exercised. Twelve POSIX/jump plus nine volatile/member
checks pass. Commit `d597dcd9a`. Full pglitec object emission passes in framework
receipt `20261022-161942-b7ddf824`; that retry then stops in PostgreSQL c.h.
Logs: `artifacts/discovery/{sysv-before,sysv-shm-native,sysv-regression,
posix-regressions,posix-regressions-after,volatile-array-regressions}.log`.

## PGL-003 — C11 `_Noreturn extern` declaration ordering (fixed, retrying closure)

Classification: parser grammar. Actual PostgreSQL c.h line 890 declares
`pg_noreturn extern void ExceptionalCondition(...)`, where C11 pg_noreturn is
`_Noreturn`. DotCC rejected `extern` after the function specifier. Reproduction:
`tests/Translation/noreturn-extern.c`; native C11 succeeds, old DotCC fails.
The shared grammar now accepts this function specifier before external storage;
IR retains both external linkage and the noreturn contract. Full project builds
regenerate parser tables. Ten NoreturnCallTests and an emitted execution fixture
pass; native fixture prints `value=42`. The real `src/common/pg_lzcompress.c`
object probe now passes, receipt `20261022-162319-d66e5d12`.
Logs: `artifacts/discovery/{noreturn-before,noreturn-unit,noreturn-functional,
noreturn-fixture-native,pglz-probe}.log`. Complete diagnostic-set retry and
actual PostgreSQL layout emission remain tracked separately from this repair.

## PGL-004 — Host resumable exit, signals, timers and state reset (open)

Classification: reproduced host/runtime contract mismatch. PGlite startup yields
99 and query-error recovery yields 100. The current bound RuntimeContext records
its first exit as terminal. Reusing that owner after catching the exception is
not an implemented lifecycle. Minimal translated probes also observe unsupported
setitimer and sigaction returning success without populating the previous action.
See boundary suite reports and BOUNDARIES.md. Required next experiment: typed
PGlite exit interception with owned exception/status handling and no leaked host
exceptions, real signal/error cleanup decisions, and drain/reset at shutdown.
No hidden subprocess or global lock can prove independent instances.

PGL-003 full retry: receipt `20261022-162338-b253238e` emits complete pglitec,
pg_lzcompress and bitmapset objects. postgres.c stops at PGL-005 below. The full
unit suite reports 3,257 passing and 16 failing cases. An isolated archive of
repository baseline from `config/campaign.json` reports 3,256 passing and the
identical 16 failing test names (no new failure). The added noreturn case passes.
Logs: `artifacts/discovery/dotcc-tests-{all,baseline}.log`.

## PGL-005 — Missing PostgreSQL atomic compiler port (reproduced; work active)

Classification: project platform integration. Receipt
`20261022-162338-b253238e/003-postgres.log` stops at upstream
`port/atomics.h`: `#error "could not find an implementation of pg_atomic_uint32"`.
Native GCC-generated headers advertise GCC builtin capabilities; DotCC does not
pretend to be GCC. Defining __GNUC__ would select asm/intrinsics that have not
been proven. The next experiment is an explicit C11 port using the shared
Interlocked-backed atomics, retaining real PostgreSQL generic wrappers and
native/emitted widths, CAS expected-value behavior and ordering. See ATOMICS.md
when the independent experiment is integrated. No synchronization operations
may be removed or replaced with successful stubs.

## PGL-006 — Full fork libc C# host closure (reproduced)

Classification: missing host/runtime services, not parser success. The complete
pglitec object links to a private managed feasibility project, but C# build fails
on unresolved `popen`, `pclose`, `optind` and `getpagesize`. Reproducer:
`bash pglite/scripts/test.sh --suite libc-build --fetch never --tools reuse`.
Original logs: `artifacts/discovery/full-pglitec-{link,build}.log`.
No generated C# is repaired. Process fallback must be explicitly rejected or
served by translated initdb orchestration, never a native backend. Next: define
reviewed host bindings/state for these services and execute full pglitec tests;
callback/cleanup slices do not close this whole-unit gate.


## PGL-007 — Spinlock compiler port (fixed and integrated)

The integrated C11 atomic port resolves PGL-005 for the actual PostgreSQL
wrappers. Original native, C11 native and emitted JIT layout/behavior/concurrency
controls pass. Full postgres.c now stops at storage/s_lock.h's unknown-compiler
`#error`. Actual-header reduction: `tests/Atomics/pg_spinlock_selection.c`;
native reports sizeof(slock_t)=1, DotCC reproduces the same preprocessing error.
Final atomic receipt `20261022-164001-838140a2` retains the reduced and full-TU
logs. Normal translation now selects all 865 observed backend/libc/adapter units;
receipt `20261022-164034-40490bc6` reaches this same failure in brin.c.

Next implementation must preserve one-byte storage, TAS result convention,
acquire/release semantics and real upstream S_LOCK contention/delay behavior.
Setting HAS_TEST_AND_SET without an implementation or bypassing the error is
invalid. This is a project port boundary, not an established compiler defect.
After a real port passes native/emitted probes, retry the whole backend and
continue the independent libc/initdb/exit/signal closure work.

Continuation: the real one-byte C11 port now preserves TAS/acquire/release and
the unchanged upstream S_LOCK contention, sleep/backoff and stuck-lock paths.
Integrated receipt `20261022-171904-fa0d4751` passes original-native/C11-native/
emitted-JIT controls, 20,000 protected increments, 64 forced sleeping contentions,
byte canaries/layout and seeded delay/timeout checks, plus existing atomic and
four overlay controls. Full postgres.c emits without unresolved include reports.
The complete backend still has other actionable source/compiler failures;
spinlock completion does not close P1–P6. See ATOMICS.md and IMPLEMENTATION.md.

## HAR-001 — Native manifest collection (fixed)

GNU make's forced dry run remade configure files, while the first recursive
plan omitted intermediate object compilation even with private objects parked.
The failing parent receipt is `20261022-163429-774b6941`. The final collector
retains the actual link closure, queries explicit object targets in each owning
Makefile while parking/restoring only private objects under the project lock,
and rejects any missing input. Integrated native receipt
`20261022-163944-c778dbf6` passes the 863-unit closure and all 21 SQL controls.
No references or shared compiler outputs are mutated by collection. This is a
harness repair, separate from SQL implementation or compiler progress.
