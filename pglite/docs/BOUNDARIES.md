# PGlite embedding and ABI boundaries

The first managed SQL workflow is **blocked**. Native boundary controls and a
small emitted ABI fixture pass, but they do not establish embedded initialization,
query execution, shutdown or reopen. The ordinary consumer fails clearly until a
complete translated product supports that lifecycle. No native database import,
subprocess engine, JavaScript/Wasm backend or replacement SQL engine is delivered.

This implementation follows baseline v4. The source selection is owned by
[`config/sources.json`](../config/sources.json); native preparation and profile
policy are owned by the source agent. Detailed incremental evidence lives in
[`boundary-progress.md`](boundary-progress.md). Shared compiler repairs and
integration remain the supervisor's responsibility.

## Final integrated workflow

The actual 928-unit backend executes original initdb and the canonical SQL
workflow under raw/processed JIT and normally trimmed Linux x64 NativeAOT.
Every row passes 36 assertions across three same-process lifetimes and two
persistence reopens. Original error/transaction cleanup and shutdown are exercised
by the translated core. Current boundary suites and import/resource/ELF audits
pass; `artifacts/final-qualification.json` and [VALIDATION.md](VALIDATION.md)
record final selected acceptance through P6. Signal-aware lexical jumps and their
bounded controls are documented in [SIGNAL-JUMPS.md](SIGNAL-JUMPS.md).

## Historical owner implementation and integration

The following investigation retains its snapshot-specific failures and probes;
current integrated results above supersede its present-tense open-gate statements.


The owned runtime now supplies a deliberately scoped exit transport.
`RuntimeContext.InterceptPgliteExits()` intercepts only `exit(99)` and
`exit(100)` in its original thread/binding and throws `PgliteYieldException`
with that owner. It leaves `Termination` and `TerminationTask` unset. Ordinary
exit codes, unscoped 99/100, `_exit`/`_Exit`, abort and worker failures retain
terminal semantics. Real terminal requests and runtime disposal cancel owned
timers, pending signals and handler references. Runtime disposal also calls the
locale owner's existing `DisposePosixLocales(this)` before heap/descriptor release. The rejected typed exit override
is retained as an unsupported-alternative diagnostic, not a required route.

`src/Host/PgliteOwner.cs` uses the actual `--state-context` factory
`PGlite.__DotCcCreateContext()`. Every entry binds that generated context;
callbacks, error-recovery calls and cleanup use the same runtime identity. The
host permits one owner on its creating thread and rejects reentry. It scopes
yields only around startup/query dispatch; after 100 it calls the supplied real
`dpg_recover_query_error` delegate, which must still execute upstream
`PostgresMainLongJmp`. It does not implement transaction cleanup. Shutdown runs
without yield interception, requires a real normal `exit(0)`, then drains the
real fork callbacks before disposing storage and releasing callback roots. A
failed shutdown quarantines the owner and prevents reopen.

Reopen creates a fresh generated state context, preserving the compiler's
original global and function-static initializer recipes. It never calls main
again against retired globals or resets PostgreSQL fields by hand. Two emitted
probe lifetimes now prove that mechanism (including globals initialized to 37
and 71, repeated 99/100 yields and actual fork atexit draining). This is **not**
proof of PostgreSQL backend shutdown, persisted rows or successful full-backend
reopen; those remain required in QUALIFICATION.md.

The real signal implementation file is `DotCC.Libc/ProcessSignalLib.cs` (there
is no SignalLib.cs). `PgliteSignalTimerLib.cs` implements an explicit opt-in,
single-thread guest service. It supplies old-action writes, masks, pending
coalescing, ignored/default dispositions, handler masks and restoration, one-shot
SA_RESETHAND, owner-local raise/self-kill, ITIMER_REAL remaining/interval values,
cancellation and cooperative delivery. SIGKILL/SIGSTOP registration,
SA_SIGINFO/SA_NODEFER, CPU timers and process stop operations fail explicitly.
No host process disposition is installed. Without opt-in, sigaction/mask/timer
requests fail instead of returning a successful no-op. The existing standalone
SIGINT backend remains separate. An expired blocked timer remains pending, as
on POSIX; cancellation prevents future expirations rather than erasing already
pending delivery.

For product integration, the supervisor owns these recipe changes:

1. Link with `--state-context` and include authored `src/Host/PgliteOwner.cs`
   at its original location in the generated project. The diagnostic
   `tests/Boundary/BoundaryProduct.csproj` demonstrates compiling untouched
   emitted files alongside that authored source; no emitted C# is edited.
2. After the existing atomic/CPU private include overlays, call
   `boundary.prepare_signal_overlay(Path(port["include"]), ctx.work)`. This
   guards the exact selected `miscadmin.h` macro and inserts only
   `dpg_host_poll_signals()` before the original
   `INTERRUPTS_PENDING_CONDITION()`/`ProcessInterrupts()` logic. Reference and
   configured stages are rejected as destinations.
3. Pass `--overrides-file src/Host/signal-overrides.json` for every object
   translation. It binds the poll hook to `PgliteOwner.PollSignals` and the
   real declared `setitimer` to `PGlite.Libc.PgliteSetitimer`. The generated
   wrapper keeps the declared signature; this ordinary typed override is
   accepted by the real compiler. No shared timer-file edit is needed for this
   profile. If another override profile is needed, merge its function rules
   explicitly; do not silently drop these bindings.
4. Supply actual recovery, shutdown/Terminate input and cleanup exports to the
   owner, retaining callback roots, argv and buffers through cleanup. Entry-point
   host pumps supplement the real translated safe points; they do not replace
   them.

The native and emitted diagnostic now exercise an actual copied/adapted
`CHECK_FOR_INTERRUPTS` macro inside a bounded C loop with a real timer. Managed
delivery occurs before that C call returns, through the typed owner poll hook.
Its ProcessInterrupts callee is explicitly a **test event observer**, not a
product implementation. The product overlay retains the upstream callee and
condition unchanged. Full-backend startup/query timer coverage and the complete
SQL workflow remain unrun; this experiment does not qualify missing safe-point
coverage in other required backend paths. Blocking-query cancellation is outside
the selected first workflow.

`CaptureGuestSignalMask(save)` and `RestoreGuestSignalMask(snapshot)` supply the
runtime half of signal-aware lexical jumps. A saved snapshot belongs to its
capturing owner/thread; restoration does not execute pending callbacks during
unwind. Save=0 intentionally does nothing. The compiler still needs actual
sigsetjmp/siglongjmp declaration and lowering support: the new native probe
passes both mask modes, while emitted C# fails CS0103 on these unresolved names.
Required shared repair: add the POSIX declarations to setjmp.h, recognize
sigsetjmp in all supported lexical guard/capture shapes, evaluate both arguments
once, capture the mask at initial arm, and restore it inside the matching catch
before recovery/goto. Map siglongjmp to the existing tagged jump transport and
retain 0-to-1 conversion. Do not replace it with a macro that ignores save-mask.
No jump into an expired frame is promised.

Current evidence: `artifacts/boundary-owner-20261022/runtime-tests.log` reports
38 passing focused tests (owned lifecycle plus existing termination/binding/
process-signal regressions). The immutable supervisor 1713 snapshot ran the
entire boundary adapter under private `build/boundary-owner-20261022/suite-1713`.
Commands and original logs are in
`artifacts/boundary-owner-20261022/suite-1713/commands.json` and its sibling logs;
`boundary/report.json` is **blocked** solely on the lexical sigjmp C# build.
The actual-header ABI, complete pglitec object emission, scoped repeated yields,
real fork callback/cleanup slices, timers/cancellation and emitted safe-point
loop pass. A normally trimmed Linux x64 NativeAOT publication of that same
boundary fixture also runs successfully; its observations match JIT exactly.
Logs are `artifacts/boundary-owner-20261022/aot-{publish,run}.log` and
`aot-managed.json`. This is an unprocessed diagnostic fixture, not a completed
raw/processed product matrix. Four real-source overlay checks reject changed
and duplicate contexts and configured-stage destinations without mutation;
`overlay-checks.json` records them. The SQL/backend/matrix gates remain unrun. The source snapshot
predates the final snapshot-helper additions, which pass the separate current
runtime tests and need the supervisor's next compiler rebuild.

## Actual fork entrypoints

The selected `postgres-pglite` source supplies `pglite/src/pglitec/pglitec.c`,
`src/backend/tcop/postgres.c`, `src/backend/libpq/pqcomm.c`, and
`README-PGLITE-DEV.md`. These establish the following contract:

- `pgl_setPGliteActive(int)` toggles global embedding mode. Disabling it calls
  `clear_setitimer()`, which invokes `setitimer(ITIMER_REAL, zero, NULL)`.
- `pgl_set_rw_cbs` takes two callbacks of type
  `ssize_t (*)(void *buffer, size_t length)`. `pgl_recv` and `pgl_send` forward
  synchronously. They do not own or copy the host's callback roots.
- Startup still enters the real backend `main`, using its single-user dispatch
  and data-directory configuration. It initializes the core and reaches a
  synthetic `exit(99)` when the input loop yields. There is no inspected C
  `pglite_init(path)` or `pglite_exec_sql(string)` API.
- `pgl_startPGlite()` attaches a dummy port, switches output to `DestRemote`,
  adjusts backend flags and loads HBA configuration. It is called after startup;
  calling it alone does not initialize a PostgreSQL cluster or establish readiness.
- `PostgresMainLoopOnce()` consumes PostgreSQL frontend protocol input.
  `pq_buffer_remaining_data()` returns **ssize_t**, and `pgl_pq_flush()` flushes
  backend protocol output. A Simple Query frame uses protocol network byte order
  for its length and a NUL-terminated UTF-8 SQL payload; host results must parse
  actual backend frames, not infer rows from submitted SQL.
- `pgl_longjmp` compares a jump record against `postgresmain_sigjmp_buf` and
  intercepts the active top-level error as `exit(100)`. The host is then expected
  to call the real `PostgresMainLongJmp()` to abort/clean up the transaction and
  restore protocol state. Ordinary nested jumps still use longjmp.
- Disabling embedding and processing a Terminate frame takes the real
  `proc_exit` path. `pgl_run_atexit_funcs()` drains registered callbacks in reverse
  order. Callback draining alone is not PostgreSQL shutdown or durability.

[`src/Host/dotcc_pglite_boundary.h`](../src/Host/dotcc_pglite_boundary.h) and its
C implementation are forwarding-only proposed adapters. `dpg_start` accepts an
actual `int (*)(int, char **)` backend entrypoint rather than assuming a generated
rename for main. Native compilation and DotCC object emission have succeeded.
The adapter is excluded from product publication while the complete closure and
host lifecycle remain unproved. Its C shutdown functions expose distinct upstream
steps. The authored managed owner supplies context retirement/recreation; a full
backend reopen is still unqualified.

Some client-facing startup behavior is additionally conditional on Emscripten.
For example, `ProcessStartupPacket` in `backend_startup.c` is static in a native
build and exported only under `__EMSCRIPTEN__`; `postinit.c` contains
Emscripten-specific session-user selection. Defining only `__PGLITE__` is not
proof of upstream Wasm parity. The proposed adapter does not call a nonexistent
native export for that startup packet function.

## Ownership and global state

The inspected C program uses process globals for embedding mode, input/output
callbacks, streams, atexit registration, emulated shared-memory segment list and
IDs, the top-level jump record, protocol flags, and PostgreSQL's backend state.
The initial host contract therefore permits one owner, one entering execution
thread, serialized calls and no callback reentrancy. Multiple instances and
parallel queries are unqualified. A directory and the corresponding static
program must have an exclusive owner.

Callbacks must catch managed exceptions and report a documented C error return.
The owner must retain callback roots, argv strings, request/result storage and
runtime resources until upstream cleanup completes. Read callbacks must copy
only the available input bytes; write callbacks must copy borrowed bytes before
returning if those bytes outlive the call. C size_t lengths cannot be narrowed
to int without checking range. A managed callback address uses the emitted
managed calling convention, not `UnmanagedCallersOnly` or a Wasm table index.

The real emitted callback type in the allocated LP64 tool snapshot is
`delegate*<void*, ulong, long>`. The actual nested runtime exposes
`Libc.RuntimeContext`, `Enter()`, `RetainLease()` and
`RuntimeTerminationException`. No unverified generic `BindHost` API has been
invented. A runtime context owns heap/descriptors and binding identity; it does
not, by itself, relocate or reset all static PostgreSQL program globals.

The upstream Wasm lifetime ends by discarding a module/memory instance. Native
static globals do not gain that reset by disposing a managed object. The fork's
`pgl_munmap` deliberately returns success without freeing a mapping because Wasm
instance termination reclaims memory. Its use in a static managed process needs
an explicit ownership/unmapping decision and leak/close tests. Reinitializing
main against the same retired state remains unsupported. Fresh generated state
is the implemented transport strategy; full backend qualification is pending.

## ABI separation and measured layouts

The intended initial host profile is little-endian Linux x64 LP64. Actual native
and emitted minimal fixtures agree on pointer, long, size_t, ssize_t and time_t
widths of 8, callback width 8, and a sample aggregate size/offset of 24/16.
The typed callback successfully passes a length above 2^32, preserves signed -1,
and accesses pinned UTF-8 bytes across a managed GC. The tested bytes include an
embedded NUL; boundary I/O uses explicit lengths.

The actual configured PostgreSQL-header native probe reports Datum=8, Size=8,
Oid=4, TimestampTz=8 and XLogRecPtr=8; StringInfoData=24 with data/len/maxlen/cursor
offsets 0/8/12/16; ItemPointerData=6 and BlockIdData=4. These values come from the
selected headers, not mirror structs. The probe supplies only the real declared
`CurrentMemoryContext` pointer storage needed by an emitted unused inline;
it implements no PostgreSQL memory management. Actual native/emitted header
matching remains a separate required gate and must be read from the latest
supervisor retry, not inferred from the minimal fixture.

The upstream build explicitly selects wasm32 and `-m32`; its pointers, C long,
size_t and ssize_t are 32-bit. Its JavaScript callback signature therefore cannot
be copied into the LP64 host. The Emscripten SDK and generated headers were not
built by this boundary owner; time_t widths, alignment, optional types and full
Wasm layouts remain unmeasured. PostgreSQL disk layout, catalog data, endian
conversion and host pointers are separate contracts. No cross-profile database
file interchange or copying a Wasm bootstrap directory into LP64 is qualified.

Jump storage is intentionally opaque: native libc jmp_buf is 200 bytes on this
host, while the tested DotCC jmp_buf is an 8-byte identity slot. The fixture proves
nested lexical jumps, including longjmp(value=0) becoming 1. It does not prove
sigsetjmp signal-mask restoration, jumping into a function that has already
returned, or PostgreSQL's whole error stack. A signal-aware typedef alone does
not implement the missing semantics.

## Host services and unresolved decisions

Memory: PostgreSQL memory contexts, buffer management, pages, catalogs and WAL
stay translated. Shared Libc malloc/free gives stable unmanaged storage and
per-context ownership. Minimal allocation/copy/free works. Full memory-context
alignment, nonlocal cleanup, global lifetime and exhaustion behavior are open.
The selected native config advertises ordinary 8-byte maximum alignment and
16-byte compiler int128 alignment; PostgreSQL explicitly adjusts its int128
aliases. Actual aggregate/alignment checks remain required for the complete ABI.

Files: real core code requires directory enumeration, path/cwd behavior, flags,
permissions, file positioning, vector/positioned reads/writes, truncation,
rename/unlink, locking, fsync/fdatasync and directory durability. The existing
shared Libc supplies many of these through BCL and targeted platform services;
its presence is not a PostgreSQL durability qualification. The initial product
must own its paths/descriptors and propagate errno/short I/O correctly. Catalog
and timezone data plus initdb's generated files are product inputs. No cache or
successful native workflow can replace emitted persistence testing.

Time/signals and termination: the former successful no-op action/mask stubs,
unsupported timer route and permanently terminal synthetic yields are replaced
for the explicit PGlite profile by the owned services and integration above.
The guest ABI is distinct from native sigaction. Required actual-backend timer
paths, signal-aware error jumps, SQL cleanup and full lifecycle execution remain
unqualified. Unbound exit still has standalone process semantics.

Entropy/providers: without OpenSSL the selected `pg_strong_random` opens and
reads `/dev/urandom`; its exact-length/EINTR/error behavior needs host testing.
The default native variant excludes TLS, ICU, XML/XSLT, compression providers and
extensions. Upstream Wasm enables several of those dependencies and preloads
locale/ICU/share data. Exclusion is a profile decision, not proof those APIs are
implemented. Native configuration also selects x86 CRC/popcount implementations
at runtime; the translated source closure must audit that selection rather than
claim unsupported compiler intrinsics or atomics. The supervisor/atomics owner
handles that separate port investigation.

Processes: `pgl_system` returns 123 when its callback is absent. `pgl_popen` and
`pgl_pclose` fall back to native subprocess services. The delivered product must
not use that fallback. initdb is a separate frontend closure and invokes backend
bootstrap operations through these hooks; the upstream client orchestrates
those calls. A translated in-process initializer or a qualified same-profile
bootstrap-data policy is required. The native helper's subprocess support is an
oracle-only facility, never an embedded product backend.

Shared-memory emulation: the fork owns its malloc-backed segments and a global
list. Positive create/attach/detach/remove behavior passes natively. The source
reuses a matching key before checking flags; IPC_PRIVATE/IPC_EXCL and allocation
failure/large-size behavior are not qualified. Core SysV/mmap/locking use and
cleanup must be validated under the selected host policy.

## Evidence and reproduction

The latest boundary-owner suite used its explicitly allocated immutable compiler
snapshot. Its receipt is
`artifacts/campaign/20261022-162206-cee01373/receipt.json`; native and managed
layout/behavior reports are in that run's `boundary/`. Native actual-fork
callbacks, atexit LIFO/reset, shared-memory lifecycle and top-level exit 100
passed. Emitted exact callback/atexit source slices, minimal LP64 ABI, allocation
and nested jumps passed. Signal, timer and termination behavior was
characterized as blocked, not converted into SQL success.

That original tool snapshot predates the supervisor's IPC/sigjmp-storage,
volatile-array-decay and declaration-order repairs. The supervisor has since
reported full pglitec object-emission success; old missing-header/type/parser
failures are retained historical evidence, not assertions about current tools.
The actual-header retry reached a missing fixture global; the fixture now
supplies its pointer storage for a fresh retry, which now passes under final tools. `sigsetjmp` mask behavior,
repeatable exits are now covered by the current owned implementation above;
real backend shutdown/reopen remains open.

The suite adapter is `scripts/boundary.py:run(ctx, suite=None)`. A standalone
framework test requires existing immutable `DOTCC_COMPILER` and configured-header
metadata via `PGLITE_BOUNDARY_PREPARATION`; translate/verify can use ctx.compiler
and the preparation data already recorded in its receipt. It never builds
shared tools. Use the actual paths allocated for the run:

```bash
DOTCC_COMPILER=/absolute/immutable/DotCC/dotcc.dll \
PGLITE_BOUNDARY_PREPARATION=/absolute/native-control/prepared.json \
  bash pglite/scripts/test.sh --suite boundary --fetch never --tools reuse
```

Required unrun gates: complete product source/import closure; signal-aware PostgreSQL error recovery and subsequent
query; real in-process initialization; create/insert/exact query; rollback/commit;
shutdown/reopen with persisted rows; required raw/processed JIT/NativeAOT product
matrix; instance reset/leak/lifetime tests. Adapter syntax, native controls and
negative consumer packaging checks close none of those workflow gates.

## Historical supervisor integration before owned runtime implementation

Receipt `artifacts/campaign/20261022-163657-6b1f4069/receipt.json` passes the
actual native/emitted PostgreSQL-header layout comparison, including all 13
reported fields. It also passes the minimal emitted ABI and actual fork
callback/cleanup-slice execution. The overall boundary suite still **fails** on
four reproduced requirements: rejected typed exit override, permanent owner
termination for synthetic exit 99, unsupported setitimer, and sigaction failing
to populate the old-action output. Full signal-mask semantics remain unproved.

The recipe now snapshots existing tools and prepares configured headers for an
ordinary `test --suite boundary --fetch never --tools reuse` invocation. Explicit
`DOTCC_COMPILER` and `PGLITE_BOUNDARY_PREPARATION` remain optional independent
selection mechanisms. A complete full-pglitec library is separately tested by
`libc-build`: object emission succeeds, but its C# build exposes missing
popen/pclose/optind/getpagesize services. See VALIDATION.md and BLOCKERS.md.
