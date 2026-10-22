# PGlite blocker ledger

## Current — original initialization and private raw JIT/AOT pass

Coherent receipt `20261022-202228-8f7e10f1` emits and strict-links all 928 selected
backend/module/libc/adapter units and raw-builds with zero errors and 12 warnings.
The fresh original frontend and original-main controls pass. That receipt's
explicit publication guard remains a failure; it is not a published-product pass.

Original translated initdb now returns zero, including real bootstrap,
post-bootstrap SQL and final disk sync. The successful receipt is
`artifacts/initdb-owner-20261022-r3/actual2020/receipt.json`; complete spools and
hashes are retained. The fresh cluster contains an 8192-byte `global/pg_control`
and `PG_VERSION=18`. The canonical consumer separately passes 36 exact SQL/
ownership assertions in private raw JIT and normally trimmed NativeAOT runs
against coherent build `20261022-202228-8f7e10f1`. Each runs three clean
same-process lifetimes and two persistence reopens. Evidence under
`artifacts/product-owner-r3/raw928/`: `consumer-run.{log,json}` and
`aot-{publish,run}.log` plus `aot-run.json`. The AOT run exits zero in 29.84 seconds.
The 21 unchanged native-inventory rows and 15 ownership/recovery checks include
UTF-8, NULL, transactions, SQL errors/recovery and retained results after reopen.

The locale utility transport, calendar timezone service and required Snowball
closure resolve the earlier initialization failures. All 51 original Snowball
sources and both real module catalogs are linked into the backend context through the
shared PL/pgSQL/Snowball registry. Independent native/JIT/trimmed-AOT stemmer
controls also pass. Original collation import and SQL are retained; no native
database engine or native module replaces translated code.

These raw passes are prepublication evidence for 202228, not the resumed 203609
closure or final promoted assets. All four final canonical raw/processed ×
JIT/NativeAOT rows, paired delivery and P6 acceptance remain pending. Active receipt
`artifacts/campaign/20261022-205650-7472ba65/receipt.json` resumes retained raw
emissions from canceled attempt `20261022-203609-4cdd2a15`. The shared asset-bin
and source ZIP/verbatim-notice packaging repairs add no new execution claims.
A further data-only transactional correction `20261022-211008-fe98b7c0` preserves
168 prepared C/header inputs and generated Bison terms/exceptions plus referenced
GPL text. Generated raw/processed C# remains byte-identical to the passed205650
publication. Its final project builds and asset hashes remain required.
See [VALIDATION.md](VALIDATION.md) for current evidence. The latest compiler
comparison is
`artifacts/supervisor-portable/regression-comparison-current10.json`: 3420 passes
and the same 16 untouched-baseline failures.

## Historical blocker checkpoints

The entries below retain campaign-start and intermediate observations from
2026-10-22. Their older pending states are superseded by the current gate above;
planned checks are not successes and every failed receipt remains failed.

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
That finding was active at this checkpoint and was later repaired by the integer
width/sign conversion described below. The original combined unit log remains
failed. Logs: `artifacts/supervisor-portable/designators-{native,
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
condition was the next reproduced parser defect; PGL-009 records its repair.
Software codegen selection and continuation are recorded in IMPLEMENTATION.md.

## BASE-001 — Existing framework metadata regression

Classification: unrelated repository baseline. Command:
`python3 -B -m unittest discover -s Scripts/campaigns/tests -v`.
Observed: 70 cases, one failure in
`test_campaign_helpers_keep_hash_literals_in_metadata`, which finds an existing
64-hex literal in `blink/scripts/campaign_delivery.py`. Other 69 cases pass.
Expected: helpers keep provenance literals in JSON. No PGlite code is involved;
Blink was not changed. Transcript: `/tmp/pglite-framework-tests.log` (copied into
campaign artifacts for retained evidence). This does not close PGlite validation.

## PGL-001 — Embedding profile, host and ABI closure (historical feasibility)

Classification at campaign start: open feasibility, not a reproduced compiler
defect.
The fork's PGlite path uses `__PGLITE__`, Emscripten and intercepted libc. A
normal x64 PostgreSQL build cannot prove that profile. Experiments: inspect
pinned build scripts and generated inputs, native-control profile, actual fork
entrypoint emission and native/emitted ABI checks. These were P1 requirements;
the private raw JIT/AOT workflow now passes its real host/state/error boundaries.
Remaining product matrix and P6 gates are recorded above.

## PGL-002 — POSIX declarations and volatile array decay (fixed)

Classification: shared header coverage and compiler lowering. Full source
`pglitec.c` initially reported missing `sys/shm.h` and failed parsing its
`volatile sigjmp_buf postgresmain_sigjmp_buf` declaration. Native reductions
passed. Adding the opaque signal-jump storage alias exposed C# CS0306: a volatile
array typedef decayed through `Volatile.Read<ulong*>`, an illegal pointer generic
argument. Array decay computes an address; volatility belongs to the elements.
The backend now retains that distinction. LP64 `ipc_perm`/`shmid_ds` data records
were supplied by that declaration-only repair without kernel IPC implementations.
Signal-mask operations were unimplemented at that checkpoint; later owner-scoped
signal services supersede that limitation, not the opaque storage alias alone.

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

## PGL-004 — Host resumable exit, signals, timers and state reset (historical failure)

Classification: reproduced host/runtime contract mismatch at campaign start.
PGlite startup yields 99 and query-error recovery yields 100. The then-current
bound RuntimeContext recorded its first exit as terminal; reusing that owner
after catching the exception was not implemented. Minimal translated probes also
observed unsupported setitimer and sigaction returning success without populating
the previous action. See the historical boundary reports and BOUNDARIES.md.
The required implementation was typed PGlite exit interception with owned
exception/status handling and no leaked host
exceptions, real signal/error cleanup decisions, and drain/reset at shutdown.
No hidden subprocess or global lock can prove independent instances. Subsequent
owned boundaries now pass raw JIT error/recovery and three clean same-process
lifetimes; the remaining matrix is still required.

PGL-003 full retry: receipt `20261022-162338-b253238e` emits complete pglitec,
pg_lzcompress and bitmapset objects. postgres.c stops at PGL-005 below. The full
unit suite reports 3,257 passing and 16 failing cases. An isolated archive of
repository baseline from `config/campaign.json` reports 3,256 passing and the
identical 16 failing test names (no new failure). The added noreturn case passes.
Logs: `artifacts/discovery/dotcc-tests-{all,baseline}.log`.

## PGL-005 — Missing PostgreSQL atomic compiler port (historical; port integrated)

Classification: project platform integration. Receipt
`20261022-162338-b253238e/003-postgres.log` stops at upstream
`port/atomics.h`: `#error "could not find an implementation of pg_atomic_uint32"`.
Native GCC-generated headers advertise GCC builtin capabilities; DotCC does not
pretend to be GCC. Defining __GNUC__ would select asm/intrinsics that have not
been proven at that checkpoint. The implemented repair is an explicit C11 port
using the shared Interlocked-backed atomics, retaining real PostgreSQL generic
wrappers and native/emitted widths, CAS expected-value behavior and ordering. See ATOMICS.md
for the integrated native/emitted controls. No synchronization operations
may be removed or replaced with successful stubs.

## PGL-006 — Full fork libc C# host closure (historical failure; resolved)

Classification: missing host/runtime services at the recorded checkpoint. The
complete pglitec object linked to a private managed feasibility project, but C#
build failed on unresolved `popen`, `pclose`, `optind` and `getpagesize`. Reproducer:
`bash pglite/scripts/test.sh --suite libc-build --fetch never --tools reuse`.
Original logs: `artifacts/discovery/full-pglitec-{link,build}.log`.
No generated C# is repaired. Process fallback must be explicitly rejected or
served by translated initdb orchestration, never a native backend. Reviewed host
bindings and full pglitec controls subsequently passed. Original initdb now uses
its concrete translated backend transport; the exact locale inventory command
uses a real OS utility. See INITIALIZATION.md. The original failed build receipt
is retained; callback/cleanup slices alone did not close that whole-unit gate.


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


## Compiler repairs found by initializer closure

Original getopt's `*(place = argv[optind])` exposed a C# nint dereference error.
The compiler now restores the logical pointer type around nint-backed assignment
results, including discarded-expression handling. Native/direct/object tests
exercise dereference, indexing, function pointer invocation and compound storage
updates. No emitted C# is edited.

Unsigned BSD aliases needed by original inet_net_ntop exposed constant casts
that retained the unbounded source value: `(unsigned char)-1` incorrectly stayed
-1. Explicit comptime casts now wrap at the target width and interpret its sign;
_Bool uses truth conversion. Native/direct/object tests include signed and
unsigned truncation and promotion after conversion.

All six original PL/pgSQL translation units exposed `union YYSTYPE;`. The
file-scope grammar now accepts union forward declarations through the existing
tag-forward path. Actual six-unit source retry passes emission. These repairs
removed local compiler failures; initializer execution and SQL acceptance were
still open at that checkpoint. The current raw JIT passes are recorded above.

## Independent aggregate linkage and original extern-array declarations

The complete 876-unit backend emitted under 1812, then strict linking rejected
unrelated regex and timezone `struct state` graphs. Aggregate exposure alone
did not identify the actual external symbol connecting translation units.
Additive `aggregate-linkage:2` records real external declaration/definition roots
and their typed graph edges, partitions independent compatible components, and
keeps conflicting shared-object/function contracts as errors, including unused
extern declarations. Old/mixed metadata retains its conservative gate.

The first rooted retry emitted all 876 units but rejected `archive_mode_options`: an
original `extern const struct config_enum_entry archive_mode_options[]` had
already decayed to a pointer in execution IR. The metadata producer now preserves
the original omitted-bound declaration before that lowering; explicit zero and
known bounds stay distinct, and definitions retain their completed bounds.
No existing emitted object is reinterpreted or edited. Exact relocation origins
also permit compiler-generated private aggregate names in owning contexts while
continuing to reject authored reserved names.

Twenty-one focused unit cases and 56 execution cases pass, including direct and
object routes, both orders, nested owners, transitive callback graphs, unused
extern conflicts and incompatible pointer/array/bound negatives. Native original
header/definition controls pass. Snapshot 1852 built without warnings/errors;
fresh full regeneration was then active. The corresponding compiler tests passed 3,387
cases with exactly the 16 established untouched-baseline failures (comparison:
`artifacts/supervisor-portable/regression-comparison-current5.json`).

## Forward union kinds and exported parser semantic tags

The fresh 876-object 1852 attempt rejected `YYSTYPE` between twophase and the
replication parser. Its first cause was incorrect forward-kind metadata: the
shared struct/union-forward syntax node always lowered as a struct. The lowering
now preserves the keyword. Sixty-six affected execution controls pass, including
forward completion in both orders and genuine union/struct conflicts.

The original bootstrap, replication and synchronous-replication interfaces also
export generic `union YYSTYPE` declarations that can bind to another parser's
complete union in an importing translation unit. Private type namespace
adaptations retain every original parser/scanner C byte and member layout.
Three public headers use distinct tags in exactly two token positions each;
three copied Bison headers use corresponding distinct names in four identifier
positions each. The per-pair YYSTYPE define applies only to the six matching
original sources. Merely defining YYSTYPE without adapting Bison's generated
guard would suppress its actual union declaration and is not used.

All six original units compile natively with incompatible-pointer and implicit
function diagnostics treated as errors. Both unscoped unused extern conflicts
and scoped execution are checked through direct and object routes. Nine guarded
initializer/staging checks pass, including missing/duplicate last-header anchors
and source/output preservation. Fresh full 1908 regeneration was then active; previous
objects with the wrong union-kind fact are not reinterpreted.

## Shared regex compile-error pointers and independent execution state

The coherent 1908 backend emits all 876 units and next rejects regex `vars`.
Tracing the exact constraint gives pg_reg_getcolor -> colormap -> vars. The
header's colormap.v and nfa.v are compile-error pointers, while regexec completes
the same generic tag with unrelated execution scratch state. This is a genuine
transitive source contract conflict; the compiler's strong check stays intact.
A guarded private regguts header names both compile-error pointees RegCompVars;
only regcomp defines vars=RegCompVars and regexec defines vars=RegExecVars. The
other four regex units retain the opaque compiler state. Fields, layouts and
algorithms remain unchanged. Six actual native units compile with strict pointer
and implicit-declaration diagnostics; 74 affected execution controls pass.

The ordinary six-unit scoped probe emits. Its diagnostic replacement in the
older full closure then fails LocalToUtf: anonymous typedef identities depend
on the two different private include paths. Metadata is not rewritten or erased
to permit that mixed closure. A coherent full normal retry under one include
overlay was then active. The corresponding compiler tests again passed 3,387 cases and
reproduce exactly the known 16 baseline failures (current6 comparison retained).

## Complete raw C# closure: intrinsics, enums and typed values

The first coherent 876-unit backend strict-links, then raw build reports 10,959
unique diagnostics. GNU unreachable accounts for 10,568; its unbound builtin
now lowers to the existing void unreachable operation, preserving explicit
termination/trap behavior and ordinary shadowed callback binding. Discarded
for-clause pointer assignments/comma values retain their effects, and addressed
file-scope struct literals now use stable owner storage instead of `&new`
expressions. Ten direct/object/lifetime execution controls and four native
programs pass, including independent live/fresh contexts and GC.

Parser enum collisions account for 337 missing-member diagnostics. Enums now
participate in semantic identity graph metadata3 with explicit kind, exact
member/value/underlying-type shape, and typed edges through external symbols
and container/callback fields. Private enums partition; incompatible shared
contracts remain errors. Fifty-six unit and 78 execution checks pass; thirteen
actual parser/scanner units emit/link with zero enum-member diagnostics in the
explicitly incomplete subset build. Old metadata is not reinterpreted.

Typed volatile CBool/enums/aggregates use actual unmanaged volatile operations;
member selection addresses volatile storage instead of loading a parent value.
Larger aggregate copies preserve every byte via volatile accesses without an
atomicity claim. Scalar conversions retain C integer promotions, CBool numeric
values and cast precedence. Exact original extern-array declaration facts
distinguish array storage from pointer slots for `&array`, sizeof and address
caching; invalid incomplete-array sizes/arithmetic stay rejected. Twenty
execution controls and 21 focused units plus three native programs pass.

The combined broad suite initially exposed one rvalue rendering regression.
It is repaired by retaining the ordinary rvalue path, with the old test unchanged.
Current8 passes 3,398 cases and exactly the same 16 baseline failures; current7's
extra failure remains accurately retained as intermediate evidence. Fresh full
metadata3 regeneration was then active and raw build/init/SQL were pending.
The later coherent 928-unit build, original initdb and raw JIT acceptance above
supersede those pending states; final matrix/P6 acceptance remains open.
