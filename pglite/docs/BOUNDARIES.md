# PGlite embedding and ABI boundaries

The first managed SQL workflow is **blocked**. Native boundary controls and a
small emitted ABI fixture pass, but they do not establish embedded initialization,
query execution, shutdown or reopen. The ordinary consumer fails clearly until a
complete translated product supports that lifecycle. No native database import,
subprocess engine, JavaScript/Wasm backend or replacement SQL engine is delivered.

This investigation follows baseline v3. The source selection is owned by
[`config/sources.json`](../config/sources.json); native preparation and profile
policy are owned by the source agent. Detailed incremental evidence lives in
[`boundary-progress.md`](boundary-progress.md). Shared compiler repairs and
integration remain the supervisor's responsibility.

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
host lifecycle remain unproved. Its shutdown functions expose distinct upstream
steps; they do not promise a reset or reopen operation.

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
main after shutdown remains unqualified even with one owner at a time.

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

Time/signals: wall/monotonic clock helpers exist, but `setitimer` returns -1 and
ENOTSUP, including the all-zero shutdown request. Actual managed `sigaction`
queries return success without writing oldact; `sigprocmask` is also currently
accepted and ignored. PostgreSQL timeout and error code relies on those services.
Neither a timer no-op nor a plain sigsetjmp-to-setjmp alias is an acceptable
success claim. The initial profile must either implement needed owner-aware
services or prove a reviewed reduced contract against real source paths.

Termination: unbound shared `exit` terminates the containing managed process.
With an explicit Libc owner it records the first termination permanently and
throws `RuntimeTerminationException`. The fixture confirms exit 99 sets that
terminal status. Catching it and resuming the same context is outside that
contract. A proposed typed `exit(int)` managed-method override was tested, but
the compiler rejects special system termination overrides. The small
[`exit_fixture.c`](../tests/Boundary/exit_fixture.c) and its JSON override retain
that exact capability reproducer. No implementation using an unsupported
binding is delivered. A reviewed host transport must distinguish repeated
99/100 yields from real close/fatal exits without dropping cleanup.

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
repeatable exit and real shutdown/reopen remain open.

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

## Final supervisor integration

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
