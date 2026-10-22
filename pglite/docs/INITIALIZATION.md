# Translated initialization and owning adapters

The coherent 928-unit backend and complete original frontend emit, strict-link,
process and build. Original initdb passes BKI/bootstrap, post-bootstrap SQL,
required PL/pgSQL/Snowball setup and final disk sync through the concrete owning
adapters. All four final canonical raw/processed × JIT/normally trimmed NativeAOT
rows pass 36 assertions, three clean same-process lifetimes and two persistence
reopens each. The ordinary solution, selected boundary suites and final P6 audits
also pass. Combined selected acceptance: `artifacts/final-qualification.json`.

Current publication is `artifacts/campaign/20261022-211008-fe98b7c0/receipt.json`.
Actual canonical executions and the separately corrected read-only evidence
qualification are in
`artifacts/product-owner-r3/canonical-final/20261022-211359-8ba5a1f9/`.
The original observer-failed Context remains failed and byte-identical; all four
actual execution commands exit zero. [VALIDATION.md](VALIDATION.md) explains the
preserved failed observer and corrected runtime-pack check.

Earlier private initialization/raw-mode passes and the 1942/2008 failures remain
historical evidence. Pending/open language in intermediate checkpoints below
is superseded by final selected acceptance above; no failed receipt is rewritten.
References and configured native stages remain preserved. No emitted C# is
manually edited, and no native PostgreSQL engine is a product backend.

## Host services and complete fork libc

`DotCC.Libc/PgliteProcessServicesLib.cs` provides owner-scoped, ref-returning
`optind` storage initially one, and the actual BCL host page-size query.
It deliberately supplies no replacement getopt algorithm. The exact read-only
`popen("locale -a", "r")` (also `"re"`) uses the real host locale utility as
described below. Other commands return NULL with ENOTSUP; invalid/null arguments
report EINVAL/EFAULT. `pclose` returns the native wait status for an owner-local
locale process, or -1 with EBADF for unknown handles or ECHILD for an ordinary
FILE, without closing that FILE. The actual fork's
`pgl_set_system_fn`, `pgl_set_popen_fn` and `pgl_set_pclose_fn` remain the
in-process transport extension points.

`StrchrnulLib.cs` implements the real byte search used by upstream snprintf:
first unsigned-byte match, otherwise the original NUL terminator, without
allocation, errno mutation or reading beyond that terminator.

`PglitePosixIoLib.cs` implements positioned scalar/vector I/O on owned file
descriptors. Linux uses actual pread/pwrite/preadv/pwritev on the FileStream's
retained SafeFileHandle; synthetic descriptor numbers never reach the kernel.
The other-platform path uses RandomAccess and a bounded vector gather/scatter
buffer. Neither implementation seeks. Linux retains its host O_APPEND behavior.
Invalid offsets, access modes, pointers, vector lengths and nonseekable streams
fail with errno. Linux syncfs and sync_file_range call the real filesystem
services; other platforms reject them with ENOTSUP. A missing Linux entrypoint
reports ENOSYS. Range writeback is not a replacement for PostgreSQL's final
fsync/durability algorithm.

The existing compiler embeds new runtime sources through its ordinary glob;
an immutable rebuilt compiler is needed before generated code includes them.
`scripts/libc-build.py` reads the already acquired reference, uses the shared
guarded `libc-input.py` compatibility adaptation, emits the complete actual fork
unit, builds privately, and runs its real callbacks with an independently
emitted C byte-I/O fixture. It performs no acquisition or native process launch.
The poll signature adaptation is owned by the supervisor: the fork's signed
ssize_t count becomes the poll.h-compatible unsigned-long count. Its original
body and LP64 width are preserved.

## Actual frontend source and data closure

[`frontend-closure.json`](../src/Initialization/frontend-closure.json) retains
the native library boundaries and the selected translation units. Two private
native relinks, using only existing readonly objects/archives, produce
`initdb-link.map` and `libpq-link.map` in this task's artifact root. They establish
35 frontend C units (3 direct plus 32 archive members), and 40 units in the
required libpq shared closure (15 direct plus 25 archive members). The raw
Makefile-derived contexts are explicitly reconstructed, rather than claimed as
captured compiler invocations. Configured native codegen flags remain evidence
of the native build, not claims about DotCC capabilities.

The frontend cannot be reduced to just pqexpbuffer: the original
fe_utils/string_utils.o also references PQclientEncoding, PQescapeStringConn,
PQmblen and PQserverVersion. The translated closure retains those original
libpq sources and their common/port dependencies. It does not load native libpq.

The initial coalesced translation selection contains 67 distinct original
sources, plus the two original PostgreSQL getopt units. The latest shared
portable_sources list adds the actual portable string and bit utilities as
needed; `frontend_units()` merges/deduplicates that list. Per-attempt
`initialization/frontend-units.json` is the authoritative expanded source,
ordered include and define inventory. `FRONTEND` common code stays in its own
assembly/class, separate from backend variants.

All frontend callers use `USE_PRIVATE_ENCODING_FUNCS` in this assembly, avoiding
native ELF public/private encoding-name interposition. They also consistently
select `JSONAPI_USE_PQEXPBUFFER`, matching the actual shared JSON implementation.
Without the latter, native client/shared headers declare JsonLexContext pointer
fields with different opaque types, and strict object linking rejects the
incompatible declarations. No JSON or encoding algorithm is replaced.

`prepare_frontend_overlay(root, source, destination)` copies configured headers
to a fresh private destination, validates exact original anchors, invokes the
shared portability helper once, and checks the frontend's required outcomes.
Its explicit option-parser expectations are:

- HAVE_GETOPT, HAVE_GETOPT_H and HAVE_GETOPT_LONG: `#define NAME 1` becomes undef.
- HAVE_STRUCT_OPTION and HAVE_INT_OPTERR: `#define NAME 1` becomes undef.

The resulting program uses untouched src/port/getopt.c and getopt_long.c.
Their real source globals resolve ahead of runtime surfaces. Tests execute
parsing, restart, source/runtime optind separation, and two state-context owners.
The supervisor independently established these same capability corrections
are also necessary for actual backend sources; this helper itself never edits
the backend's configured source.

HAVE_DECL_STRLCPY and HAVE_DECL_STRLCAT also become zero. Native configuration
advertised declarations supplied by native string.h, while DotCC's string.h
supplies neither. PostgreSQL's port.h gates its fallback prototypes on these
declaration macros. Advertising them caused eleven signed-to-size_t C# call
errors after linking original portable strlcpy. The corrected capabilities
expose the original size_t declarations and all calls build. Native, direct and
two-object controls establish this configuration mismatch, without a demonstrated
compiler coercion defect for declared C calls.

`stage_bootstrap_data()` copies only the selected BKI, SQL, sample configuration,
text-search and timezone files, COPYRIGHT, and both authored static-module
manifests to a fresh private prefix. It does not initialize PGDATA or copy a
native library. `initialization/bootstrap-data.json` records the actual selected
paths, sizes and SHA-256 observations for the run. Files are copied byte-for-byte;
the original initdb still performs BKI token substitution and all configuration,
catalog, privilege, template and SQL work.

The prefix also contains JSON entrypoint descriptors at bin/initdb and
bin/postgres, executable on Unix for original find_my_exec/find_other_exec
regular-file/access checks. They name managed entrypoints and contain no native
executable or shell implementation. Original exec.c's version request invokes
the real translated backend through popen. The historical r2 standalone prefix has
650 declared files plus COPYRIGHT; bootstrap-prefix.json records their bytes.

## In-process process transport

`InitializationProcessTransport` handles only the actual commands generated
by initdb/common/exec.c for its exact configured backend path:

- `-V`: invoke the translated backend first, then expose its actual output FILE.
- `--check`: invoke the original resource/configuration test and return its exit.
- `--boot`: spool original BKI bytes until pclose supplies EOF, then invoke the
  original backend bootstrap entrypoint with those bytes as stdin.
- `--single`: do the same with initdb's original post-bootstrap SQL.

The parser decodes POSIX quoting and the specific /dev/null redirections emitted
by upstream. It rejects shell operators, expansions, other executables and
unrecognized command/direction combinations. There is no shell execution.
Successful/failed backend exits are converted to POSIX wait statuses exactly
once. Unexpected 99/100 session yields and host exceptions become explicit
callback errors. Flush/close failure must prevent executing partial input.
Abandoned streams close without invoking bootstrap during disposal.

`InitializationFrontend` implements `IInitdbFiles` with actual emitted
fopen/fflush/fclose/errno, binding its context on every operation. It installs
original pgl_set_system_fn/popen/pclose callbacks through the forwarding C
bridge, calls original main(argv), catches only its own normal exit, drains real
fork atexit callbacks and removes callback pointers before context disposal.
Each frontend owner runs once. It contains callback exceptions and restores
environment variables changed by original startup.

`InitializationBackend.Invoke` creates a fresh PGlite.__DotCcContext per command,
enables its signals, redirects actual stdio using original pgl_freopen and real
dup2 for merged stderr, registers both real module catalogs through
RegisterModules and calls original
backend main(argv). It catches only matching-owner normal exits, rejects 99/100
yields, retains argv through original atexit draining and disposes runtime
storage afterward. Child environment changes are restored before returning to
initdb. Frontend/backend FILE pointers are never interchangeable. Flush
exceptions still close frontend streams; close exceptions leave them tracked
for disposal rather than executing partial BKI/SQL or dropping cleanup.

PgliteOwner.PollSignals first asks InitializationBackend.TryPollSignals; other
calls retain the SQL-owner checks. The signal-only partial is independently
compilable for existing boundary fixtures. Backend products compile both
InitializationBackend*.cs files, InitializationArguments.cs and
InitializationProcessTransport.cs beside PgliteOwner. The owning API compiles
InitializationFrontend.cs and its own internal InitializationArguments.cs and
references both generated libraries. It uses the backend assembly's transport
types to preserve one bootstrap signal owner.

## PL/pgSQL required by unchanged bootstrap

The original initdb unconditionally sends `CREATE EXTENSION plpgsql` during
post-bootstrap initialization. It requires upstream plpgsql.control,
plpgsql--1.0.sql, and the six original PL/pgSQL C units, including generated
pl_gram.c and scanner keyword/error headers. These are recorded in the owned
metadata; excluding the module would remove required initialization behavior.

`run_modules(ctx)` compiles natively and emits the original module units and
forwarding catalogs using private reviewed atomic/spinlock/portable headers.
The handler wrapper changes only the magic-function and _PG_init names so
their real exported addresses can coexist with statically linked modules.
Its export catalog retains original magic, initializer, fmgr info and three
language-handler functions.

`dotcc_static_modules.c` is a bounded host loader provider. It registers the two
exact PL/pgSQL and Snowball module paths with rooted symbol catalogs, validates
owner-local handles, tracks separate opens/closes, and reports real errors for
unknown modules/symbols. It returns
actual managed function addresses and never invokes a native dynamic loader.
Remapping the original dfmgr's dlopen/dlsym/dlclose/dlerror to these exported
services preserves its path/inode checks, ABI magic comparison, _PG_init call,
function lookup and caching algorithms. The real frontend/backend C types and
managed calling convention must be retained when integrating these remaps.

The static provider manifest is staged as `$libdir/plpgsql`, an ordinary file
for dfmgr's unchanged stat/path lookup. It is explicit JSON identifying the
translated module. Register that exact expanded path with the original
`dpg_plpgsql_module()` catalog inside each backend owner before original main.
Link all six module units/catalog/provider into the same backend program
context, so memory contexts, executor, fmgr and module globals belong to the
same lifetime. The coherent 928-unit build and original initdb now pass actual
CREATE EXTENSION execution through this catalog. Native plpgsql.so is not used;
the final canonical product matrix and P6 still require validation.

`backend_module_units(ctx, info, include)` returns 60 Unit entries: six original
PL/pgSQL sources, 51 original Snowball sources, two export catalogs and the
shared static provider. The private handler
wrapper guards original magic/initializer declarations.
`backend_module_configuration(root)` supplies dfmgr-only dl* remaps and exact
include/path requirements. Call InitializationBackend.RegisterModules(prefix)
for both modules in each subsequent fresh SQL session context too; initialization's loader cache cannot survive disposal.

## Historical initial evidence

These entries retain their original checkpoint observations and artifact paths.
They do not describe the current initdb or product status above:

- `host-tests.log`: 19 focused cases pass: eight process services, four strchrnul,
  seven positioned I/O/synchronization. These include 800 concurrent offset
  writes, sparse offsets above 2^32, owner descriptor separation, short vector
  reads and comparison to actual Linux syncfs/sync_file_range calls.
- `libc-current/receipt.json`: full actual pglitec emission/build and 31 assertions
  through original fork callbacks, emitted byte-I/O/positioned-vector code and
  Linux filesystem synchronization pass with immutable compiler 1743 and the
  shared guarded poll adaptation. `full-pglitec-contracts-run.log` is the output.
- `contracts-1735/receipt.json`: original getopt/getopt_long execute natively and
  in emitted code; owner/source cursor separation, restart and real static
  managed function-address transport pass.
- `static-provider-attempt/receipt.json`: emitted static symbol lookup, managed
  calli, real errors/reference counts and two-owner registry separation pass.
- `initializer-python-tests.log`: five source/overlay/data-staging contract
  tests pass, including missing/duplicate-anchor and readonly-path rejection.
- `plpgsql-current/plpgsql/report.json`: all six original PL/pgSQL units plus the
  export catalog compile natively and emit with zero failures. Execution was
  unrun at that checkpoint.
- `closure-unified/initialization/report.json`: all 69 initial frontend units
  emit/link; C# build fails with 96 diagnostics summarized in
  `closure-unified/build-error-summary.json`. This snapshot predates the new
  positioned services, corrected portable-source closure and poll declaration.
- `pointer-assignment-*` and `union-forward-*` preserve native success and the
  reduced compiler failures; the supervisor's later snapshot fixes both.

The historical complete frontend retry is `frontend-1743/receipt.json`, using the
supervisor's immutable 1743 compiler, shared libc input helper and 73 expanded
sources. Original option-parser/static-symbol contracts pass. **72/73 objects
emit; the overall attempt fails** on src/common/pg_get_line.c:129:

```c
if (prompt_ctx && sigsetjmp(*((sigjmp_buf *) prompt_ctx->jmpbuf), 1) != 0)
```

The 1743 signal-jump lowering did not support that short-circuit expression. `tests/Initialization/short_circuit_sigjmp.c` reduces it; the
native control passes disabled/nonlocal-jump cases, while 1743 reproduces the
unsupported sigsetjmp shape. Logs are `short-circuit-{native,emission}.log`.
The shared compiler subsequently repaired conditional arming and the lexical
jump continuation. The subsequent r2 retries regenerated every object; old objects are
not used to claim this repair. Historical `bootstrap-data.json`
records 648 selected data/manifest files (about 1.7 MB); COPYRIGHT is also copied.
Failed historical receipts remain failures after later repairs.

At that checkpoint the concrete harness awaited the real backend. Original
initdb and private raw JIT/NativeAOT SQL, error recovery and shutdown/reopen now
pass on the coherent 928-unit backend. Final canonical matrix/P6 acceptance
remains supervisor-owned; no SQL behavior is inferred from smaller fixtures.

## Framework integration

The new helper exports `run(ctx, suite=None)` and `run_modules(ctx, suite=None)`;
both use existing ctx.run/managed logging and private work/artifact paths.
Neither calls acquire(), make, prepare.py, tools(), or product publication.
`PGLITE_INITIALIZATION_PREPARATION` can select an existing prepared.json;
otherwise the matching source/profile stage is selected read only.

The supervisor can register dedicated initializer and module suites with these
functions and ensure an immutable ctx.compiler exists. For structural checks:

```bash
PYTHONPATH=Scripts python3 -B -m unittest discover \
  -s pglite/tests/Initialization -p 'test_*.py' -v
bash pglite/scripts/test.sh --suite libc-build --fetch never --tools reuse
```

Use private --artifacts-path for managed harnesses. Do not run shared compiler
builds concurrently or treat copied native data/setup as translated initialization.

## Historical r2 evidence and current invocation helpers

Paths below are under `artifacts/initdb-owner-20261022-r2/` unless stated
otherwise. Historical failed receipts above remain failed.

- `frontend-1802/receipt.json`: 73 objects and link pass; eleven C# build errors
  identify the missing strlcpy/strlcat declaration capabilities.
- `strlcpy-configured/receipt.json`: native and direct-source execution pass;
  old-profile two-object build reproduces two signed argument errors; corrected
  declarations make that build and execution pass.
- `frontend-1820/receipt.json`: all 73 original units, full fork libc and bridge
  regenerate/link/build. Thirteen original initdb main assertions cover help,
  version, failed option, true exit, stdio, environment and fresh-owner cleanup.
  The earlier `frontend-aot-{publish,run}.log` records ten option assertions on
  immutable 1802 through normally trimmed Linux x64 NativeAOT.
- `backend-adapter2/receipt.json`: separated-output invocation passes; merged
  output exposes real dup2's unconditional EPERM. The supervisor repaired it.
- `backend-adapter-1820/receipt.json`: complete actual fork libc, explicit
  byte/exit fixture and static provider emit/link/build. Eighteen assertions
  pass for large UTF-8/NUL input, merged/separate stderr, real exits, original
  atexit draining, retained argv, fresh initialized state and child environment.
  `backend-adapter-aot-{publish,run}.log` establishes the same normally trimmed
  Linux x64 NativeAOT behavior. The fixture cannot answer PostgreSQL -V,
  --check, --boot or --single and never backs original initdb.
- `libc-1820/receipt.json`: 37 real-fork/runtime/emitted-I/O assertions pass,
  including flush/close exception draining without executing partial input.
- `plpgsql-helper-1812/plpgsql/report.json`: eight module/helper units compile
  natively and emit. Module execution was unrun at that checkpoint.
- `initializer-python-tests-current.log`: six readonly source/config/data/
  module-wrapper checks pass. `boundary-dispatch-{build,run}.log` preserves
  the existing ABI/callback/yield/cleanup fixture with the dispatch addition.

`run` includes original option execution after frontend build.
`run_frontend_entrypoints(ctx, project, aot=True)` additionally publishes and
runs the Linux x64 option harness. `run_actual_initialization(ctx,
frontend_project, backend_project, prefix, data_directory=None)` builds
tests/Initialization/Initialize.csproj against the actual generated libraries,
invokes original initdb and retains each command's original BKI/SQL input,
stdout/stderr, argv, exit and host exception. Success requires --boot and
--single execution and original pg_control/PG_VERSION files. The complete
backend/module closure and authored backend adapter are required at build;
there is no missing-product or native fallback.

Historical standalone staged data: `build/initdb-owner-20261022-r2/bootstrap-prefix`.
Its `bootstrap-prefix.json` manifest records 650 files plus separate COPYRIGHT.
The frontend project from 1820 is
`build/initdb-owner-20261022-r2/frontend-1820/initialization/frontend/frontend.csproj`.
The later coherent 928-unit attempt regenerated the frontend with the final
creation-mode services. The old staged prefix is not current product evidence.

## Historical backend aggregate linkage repairs

All 876 actual backend inputs emitted on the supervisor's complete retry, but
strict linking rejected pg_tz because its state dependency shared a tag with
the unrelated regex state. Old metadata marked both externally exposed without
identifying the actual external declarations. It cannot safely distinguish
these APIs from an incompatible unused shared extern declaration.

The shared repair adds aggregate-linkage:2 metadata for every external symbol,
including unused function and global declarations. Canonical type contracts
retain qualifiers, tag references, pointer/callback structure and known array
bounds; unknown bounds can complete. Matching external symbols force graph
identity and recursively check complete child shapes. Structural partition
refinement then coalesces compatible cyclic graphs within components and
completes opaque nodes only into a unique graph. It preserves incompatible
layout/kind/field/prototype rejections and never selects a largest layout or
erases a pointer. Old or mixed metadata retains the conservative prior gate.

Incomplete extern arrays retain their declaration bounds before executable
lowering decays them to pointers or uses zero for an omitted outer row bound.
An omitted bound can complete against a known definition; an explicit GNU
zero bound remains zero. Definitions take precedence over a preceding header
declaration in the same translation unit. Version 1 linkage metadata lacks this
provenance and therefore uses the conservative legacy gate.

`aggregate-final-unit-tests.log` has 21 passes and
`aggregate-final-functional-tests.log` has 56 passes: direct/object routes,
both unit orders, unused shared extern
object/function conflicts, transitive/callback conflicts, opaque completion,
pointer-depth negatives, array/pointer/zero/known-bound conflicts, exact
generated-name provenance, authored reserved-name rejections, fresh owning
state contexts and legacy fallback. The five-TU native component/callback/opaque
control passes (`aggregate-native.json`), as do two native header/definition
array controls (`aggregate-array-native.json`). The multidimensional probe uses
an explicitly bounded definition: inferred multidimensional struct-array
definitions retain their existing unsupported diagnostic. The supervisor owns
the subsequent compiler snapshots, full regeneration and relink/build. Those
checkpoints led to the later coherent 928-unit build and actual initdb passes;
the final canonical matrix and P6 remain open.

The fresh 1852 retry emitted all 876 units and then exposed a separate false
kind conflict for YYSTYPE. Both grammar productions for `struct Tag;` and
`union Tag;` use the same syntax node; its IR case always recorded a struct.
The repaired case preserves the actual keyword. The native two-TU union
control exits zero, while immutable 1852 reproduces the false link failure
(`union-forward-native.json`). Existing incorrect object metadata is never
reinterpreted; affected inputs require re-emission.

There is also a real source-level namespace collision: the replication grammar
includes the synchronous parser's `syncrep_yylex(union YYSTYPE *)` declaration
and then completes YYSTYPE with its different generated semantic union. The
shared unused-extern ABI gate continues to reject that incompatible contract.
The profile adaptation assigns BootYYSTYPE, ReplicationYYSTYPE and SyncRepYYSTYPE
to the three exported parser interfaces. Each private common API header has
two exact identifier replacements. Private copies of the six original C units
are byte-identical; their three copied generated grammar headers each replace
four YYSTYPE identifier tokens, retaining original include and declaration
guards. The matching per-pair `-DYYSTYPE=Tag` changes references without defining
Tag as a macro, so the renamed generated union/typedef is still declared.
This also covers the direct union reference in syncrep_scanner.c's YY_DECL.

All six actual parser/scanner copies compile natively with incompatible-pointer
and implicit-declaration errors enabled (`parser-type-scoping/report.json`).
`aggregate-parser-functional-tests.log` has 66 passes, adding direct/object
union completion, true kind conflicts, unchanged rejection of the unscoped
unused parser declaration and execution with scoped interfaces. The supervisor
integrated the guarded profile adaptation in subsequent complete retries.

The fresh 1908 full retry resolved parser/union conflicts and next rejected
regex vars. The exact shared root is `pg_reg_getcolor -> colormap -> vars`:
regguts.h's compile-error pointer binds to the compiler's complete vars type in
regcomp.c and the executor's different complete type in regexec.c. This is a
real transitive source contract conflict; the linker remains unchanged.
The private header names both compile-only pointers (colormap.v and nfa.v)
RegCompVars. Only regcomp.c defines vars=RegCompVars; only regexec.c defines
vars=RegExecVars. Original algorithms and pointer types remain intact.

Exactly six backend units include the affected header and contain those graph
edges: regex/regcomp.c, regerror.c, regexec.c, regexport.c, regfree.c and
regprefix.c. All six compile natively with strict pointer/declaration diagnostics
(`regex-type-scoping/report.json`), and a three-TU shared-container control
executes (`regex-type-scoping/shared-container-native.json`).
`aggregate-regex-functional-tests.log` has 74 passes. The added direct/object,
both-order cases preserve rejection of the unscoped transitive child conflict
and execute a real shared-container call chain with scoped types. No compiler
or metadata-format change is involved.

The supervisor's diagnostic hybrid relink (six fresh whole objects plus 870
retained 1908 objects) passed the regex vars conflict but failed LocalToUtf's
external type contract. Independently staged include-overlay paths gave its
anonymous typedefs different identities. The old printsimple and old regcomp
contracts match exactly; the new probe regcomp contract names the other
overlay's identities. This hybrid is a failed diagnostic, not a coherent full
closure or a new demonstrated compiler defect. Exact contracts, object hashes,
log and receipt are recorded in `regex-type-scoping/repair.json`. No generated
text or metadata is rewritten and the ABI gate remains strict. The supervisor
started a normal fresh 876-unit retry with one coherent include overlay and
compiler 1908 (`artifacts/campaign/20261022-191828-3a8ebaa0`). Its actual link/
build result and original initialization were pending at that checkpoint. The
928-unit result below supersedes those pending states; final selected P6 now passes as recorded above.


## Locale process service and historical first bootstrap failure

The coherent 1942 normal build emits all 876 backend units and the complete
frontend, strictly links them, and builds the real backend with zero errors.
`artifacts/initdb-owner-20261022-r3/actual1942/receipt.json` is a failed real
initialization attempt, not a qualified product. Original initdb dispatches
version, two checks, bootstrap and single-user commands. Original bootstrap
returns zero after consuming its unchanged 973545-byte BKI. The unchanged
251748-byte post-bootstrap SQL reaches `SELECT
pg_import_system_collations('pg_catalog');` and fails because `locale -a` was
unsupported. All original input/output/error spools are retained with that
receipt. The native PostgreSQL engine is never invoked by these adapters.

`DotCC.Libc/LocaleProcessLib.cs` provides the required host service for the exact
`locale -a` command in read modes only. Linux x64/glibc uses the actual
`/usr/bin/locale` (or `/bin/locale`) via `posix_spawn`, fixed `-a` argv and a
snapshot of the real host environment. There is no shell or PATH execution.
The native stdout pipe backs an ordinary owner-local FILE. A separate worker
captures native stderr bytes without borrowing a runtime binding or FILE;
`pclose` forwards those bytes to the calling owner's stderr. Native `waitpid`
provides the exact wait status, including signal termination. Closing a process
stream removes its owner registration; disposing an owner closes abandoned
streams, kills any still-running locale child, waits for it, and releases its
stderr drain. Other commands and write modes still return ENOTSUP.

The inventory is unfiltered OS output. It does not advertise arbitrary installed
locales as supported by the managed provider: original PostgreSQL locale checks
and the provider's existing rejection remain unchanged. No SQL statement or
collation import is skipped and no catalog entries or locale names are invented.

The private `LocaleProcess.csproj` gate compares output with an independent real
locale utility invocation. Its explicit native byte/status fixture is a host
boundary control and cannot execute PostgreSQL. JIT and normally trimmed Linux
x64 NativeAOT each pass 56 assertions, covering r/re descriptor flags, foreign,
stale and ordinary handles, command/mode rejection, real ENOENT, binary stdout,
100000 stderr bytes without pipe deadlock, exit-seven status 1792, native
SIGTERM status 15, and three abandoned live children killed and reaped without
zombies. A native host that already reaped a child also cannot cause disposal
of its old owner to change another currently bound owner's errno. Logs are
`locale-process-{build,run,aot-publish,aot-run}.log` under the r3 artifact root. The failed 2006 resource probe records a missing qualified
DictionaryEntry reference; the authored helper now qualifies that reference
and Task explicitly for the compiler's stripped-using resource shell. The
supervisor supplied the corrected immutable snapshots and relinked the retained
objects before the successful fresh 928-unit initialization.


## Required original Snowball module

The historical attempt `actual2008/receipt.json` built the owning graph with
zero errors and passed locale import. Bootstrap returned zero, while
single-user SQL exited one at the original `CREATE FUNCTION dsnowball_init`
statement because `$libdir/dict_snowball` was not yet installed. Its complete spools
remain intact. The BKI is identical to the 1942 attempt; the post-bootstrap SQL
only differs in the original initdb-generated absolute sql_features.txt COPY
path for the new prefix. Neither input is edited by the transport.

The module inventory now includes the 51 original sources from Snowball's
Makefile: dict_snowball.c, api.c, utilities.c and all 48 generated stemmer
algorithms. The handler wrapper includes postgres.h/fmgr.h first, overrides
only PG_MAGIC_FUNCTION_NAME to dpg_snowball_magic and includes the unchanged
original source. Snowball has no load-time initializer. Its forwarding catalog
exports the real module magic, dsnowball_init, dsnowball_lexize and both original
pg_finfo functions. Its staged ordinary JSON descriptor is
lib/postgresql/dict_snowball; no native module is copied or loaded.

`backend_module_units` returns 60 units, including both original modules and
their two catalogs plus shared provider. All units use the same supplied
backend overlay and state context. Snowball adds its two actual upstream
include directories. The complete backend selection consequently grows from
876 to 928 units. `InitializationBackend.RegisterModules(bootstrapPrefix)`
registers both catalogs in the current fresh backend binding and is shared by
initdb invocation and API SQL startup. Both descriptors must exist.

The static provider permits two path registrations, with distinct owner-local
handles and separate reference counts. It rejects duplicate paths, duplicate
symbol names, full registration storage and unknown/native/foreign handles.
Unregister checks every module before releasing any path. The native boundary
control passes all 27 checks. All three authored catalogs/provider units compile
natively with implicit-declaration and incompatible-pointer errors enabled.
Ten source/staging checks pass. The original option parser plus emitted
multi-module registry pass JIT and normally trimmed NativeAOT checks across two
owners, including foreign-handle rejection. The updated current adapter passes
18 JIT and 18 NativeAOT assertions using its explicit byte fixture. That fixture
still cannot answer original PostgreSQL startup commands and never initializes
a cluster. Exact evidence is snowball-module-repair.json, registry-contracts2020
and backend-adapter2020 in the r3 artifact root.

The complete coherent 928-unit build now executes both modules during original
initialization and the private raw JIT/NativeAOT product workload. Independent
original Snowball native/emitted algorithm controls also pass. These local
results do not close the final canonical product matrix or P6 acceptance.


## Actual coherent initialization passes

`artifacts/initdb-owner-20261022-r3/actual2020/receipt.json` passes. The harness
builds the actual coherent 928-unit backend and fresh original frontend graph
with zero errors (14 warnings), then original initdb returns zero. Its five
original backend commands return zero without managed callback exceptions:
version, two configuration checks, bootstrap and single-user post-bootstrap.
The real bootstrap consumes 973545 bytes of original BKI; the real single-user
backend consumes 251748 bytes of original initdb-generated SQL, including
unchanged system-collation import, Snowball setup and PL/pgSQL extension setup.
Original initdb reports final disk sync complete; no no-sync flag is supplied.

The surviving fresh private cluster is
`build/initdb-owner-20261022-r3/actual2020/initialization/execution/pgdata`.
Original code creates an 8192-byte global/pg_control and PG_VERSION containing
18 plus newline, both mode 0600. Full argv, statuses, BKI/SQL and stdio spools
remain under the attempt's `initialization/execution` artifacts directory.
`actual-initialization-result.json` records their exact hashes, the compiler
hash and owning-build provenance. Earlier actual1942 and actual2008 receipts
remain failed historical evidence; neither is rewritten as a success.

The independent product owner records private raw JIT and normally trimmed
NativeAOT success for coherent build `20261022-202228-8f7e10f1` in
`artifacts/product-owner-r3/raw928/{consumer-run,aot-run}.log` and matching JSON
command/exit records. Each passes all 21 unchanged native inventory rows and
15 ownership/recovery checks, with three clean same-process lifetimes and two
persistence reopens. The AOT run exits zero in 29.84 seconds. That broader
consumer is not replaced by the initialization helper or any explicit byte fixture.
The original engine/modules execute throughout; no native database engine,
replacement bootstrap, SQL edits or emitted C# edits occur. All four final
canonical raw/processed JIT/AOT rows, paired delivery and P6 acceptance
remain pending; private 202228 passes do not qualify the resumed 203609 closure.
