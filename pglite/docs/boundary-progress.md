# Boundary implementation progress

Status: investigation in progress; no PGlite engine workflow has run.

Ownership: `src/`, `samples/ManagedConsumer/`, `tests/Boundary/`,
`scripts/boundary.py`, `docs/BOUNDARIES.md`, and this file only.
Shared compiler outputs, source refs, recipe/config, plan and root integration
are owned by the supervisor/source agent. No compiler invocation is authorized
until the supervisor supplies an immutable private tool snapshot.

Read baseline version 3 and `docs/campaigns.md`. No applicable `AGENTS.md` was
found in the root or assigned subtree. The existing uncommitted common planning
documents are preserved and will not be staged.

Initial observations: the existing shared Libc has pointer-based exception
lowering for setjmp/longjmp, managed function pointers for callbacks, and context
ownership APIs. These are starting points, not proof that the selected fork
builds or runs. Actual fork source selection and ABI comparison are pending.

Next: inspect selected PGlite fork C exports and TypeScript lifecycle; author
native/managed ABI probes under private boundary output directories; make the
consumer fail explicitly when the translated lifecycle is unavailable.

Supervisor action needed after tool build: provide the absolute immutable DotCC
DLL and closure directory for a private boundary fixture emission. Requested
command shape will be recorded here after matching the actual CLI contract.

## First actual experiment (2026-10-22)

Supervisor allocation read from `integration-progress.md`: using private compiler
`build/tools/boundary/DotCC/dotcc.dll` only. `tests/Boundary/abi_fixture.c` emitted
to `build/boundary/manual/emitted`, its authored managed harness built privately
and ran successfully. Native/emitted LP64 widths, sample aggregate offsets,
64-bit signed callback returns and lengths above 2^32, pinned UTF-8 bytes across
GC, allocation/free, and actual nested lexical setjmp/longjmp all passed.
Native jmp_buf=200 versus DotCC jmp_buf=8 is an intentional opaque-ABI difference,
not matching raw records. Evidence: `artifacts/boundary/manual/{native,managed}.json`.
No PostgreSQL SQL execution is claimed by this minimal fixture.

Actual selected fork source is now available and matches the read-only inspection:
`pglitec.c` has process globals, callbacks `ssize_t (*)(void *, size_t)`,
`pgl_setPGliteActive`, `pgl_set_rw_cbs`, and registered atexit callbacks;
`postgres.c` exposes `pgl_startPGlite`, `PostgresMainLoopOnce`,
`PostgresMainLongJmp`, and protocol flushing. Native/Emscripten startup enters
main and expects synthetic exit 99; a top-level error exits 100 for host recovery.

High-priority risk for parent: shared Libc `RuntimeTerminationLib.cs` treats the
first exit of an explicitly bound owner as terminal and retains that first
status. PGlite needs repeated yield/error exits. Legacy unbound exit terminates
the managed process. An authored typed interception or shared capability is
needed; catching terminal exceptions and resuming under the same owner is not a
valid existing runtime contract. Also the shared setjmp.h has no sigjmp_buf/
sigsetjmp declarations, and no sys/shm.h is present. Actual fork native tests
and direct emission probes are next. Consumer will fail clearly until real
initialization/shutdown/reopen support exists.

## Integrated boundary suite result

`DOTCC_COMPILER=/home/marius/p/dotcc/pglite/build/tools/boundary/DotCC/dotcc.dll bash pglite/scripts/test.sh --suite boundary --fetch never --tools reuse`
ran with receipt `artifacts/campaign/20261022-161640-8e7f7534/receipt.json`.
Native controls and the actual emitted managed harness passed, now including
exact source slices of real `pgl_set_rw_cbs` / recv / send and atexit bodies.
The suite intentionally returns failure for complete boundary feasibility:
full pglitec source emission failed, sigjmp declarations/lowering failed,
synthetic exit 99 records permanent owner termination, and clear_setitimer
returns unsupported. Read detailed report under that run's `boundary/report.json`.

Consumer authored now with ordinary `TranslatedProject` reference and planned
class alias from parent `config/translation.json`: `Managed.PGlite.Translated.PGlite`.
It has an explicit missing-project build error and an unconditional runtime
workflow-unavailable guard. Neither help nor a diagnostic subset can claim SQL
success. No handwritten database implementation or subprocess path exists.

`src/Host/dotcc_pglite_boundary.{h,c}` defines forwarding-only proposed lifecycle/
protocol operations against inspected upstream exports. It is NOT linked as a
product yet. Native syntax and DotCC object emission succeeded. This adapter
accepts an actual main function pointer rather than inventing a renamed main;
startup still requires a complete initdb/main closure. It intentionally offers
no instance handle or promised reset/reopen capability. PostgreSQL
`pq_buffer_remaining_data` returns ssize_t (LP64 long), and the adapter retains
that width.

Parent integration: `scripts/boundary.py:run(ctx,suite=None)` is ready. Test-only
runs need explicit immutable `DOTCC_COMPILER`; translate/verify can use the
provided `ctx.compiler`. It never builds root tools. Output stays beneath the
current campaign's private boundary work/log directories. Additional full ABI
and remaining provider audit findings are still being documented.

## Actual PostgreSQL header ABI probe

`tests/Boundary/postgres_layout.c` now uses real `postgres.h`, StringInfo,
ItemPointer, block, timestamp and WAL-pointer types. Native compile/run against
the source agent's configured LP64 stage passed. Actual native values:
Datum/Size/TimestampTz/XLogRecPtr=8, Oid=4, StringInfoData=24 with data/len/maxlen/
cursor offsets 0/8/12/16; ItemPointerData=6, BlockIdData=4.
`artifacts/boundary/manual/postgres-native.json` retains those observations.
DotCC object emission of that exact fixture failed in upstream c.h line 890:
`extern` after `pg_noreturn` (`_Noreturn`). Detailed log:
`artifacts/boundary/manual/postgres-layout-emission.log`. This is a real-header
reproducer and no native/emitted PostgreSQL layout match is claimed. Parent may
reuse this fixture to check the full-header repair. No shared compiler edits
were made by boundary owner.

## Parent repairs acknowledged and next independent work

Read `integration-progress.md`: parent commit d597dcd9a provides LP64 SysV IPC
headers, opaque sigjmp storage alias and volatile-array-typedef decay repair;
parent's full pglitec object emission now succeeds. Our 16:22 suite still uses
the originally allocated immutable snapshot and retains its historical failures.
Do not interpret old missing-header/type failures as current parent failures.
The parent already owns the `_Noreturn extern` grammar reduction/repair; our
`tests/Boundary/noreturn_declaration.c` independently reproduces native compile
success and the original snapshot's parser failure.

Latest integrated boundary receipt: `20261022-162206-cee01373`; actual native
PostgreSQL-header values now included, emitted header match still blocked in the
old snapshot. Managed probe also confirms sigaction(NULL, oldact) returns
success without writing oldact. This signal contract is a blocker independently
of parsing. Actual sigsetjmp mask semantics and repeatable-exit remain unproved.

I am testing an authored, typed managed-method override for `exit(int)` as a
narrow alternative to the shared terminal runtime path. It will classify and
unwind 99/100 without terminating the Libc owner, retain ordinary terminal exits,
and reject unbound/concurrent dispatch. This is host execution transport only;
no SQL, storage, or PostgreSQL cleanup is replaced. It stays out of parent config
and the product until the supervisor integrates the tested policy deliberately.

Typed interception result: the actual compiler rejects an override of exit(int)
with `special system termination functions are not supported`, before C# target
binding. Reproducer: `tests/Boundary/exit_fixture.c` plus
`tests/Boundary/exit-overrides.json`. No host implementation using that unsupported
binding has been retained, and no successful interception is claimed. This is
another concrete capability blocker for parent integration. A future explicitly
reviewed project-local service name/remap or shared runtime capability needs its
own emitted proof; catching a recorded terminal exception is still insufficient.

16:26 fixture closure update: parent latest-header retry reached C# compilation
and identified emitted unused MemoryContextSwitchTo needing the real global
pointer storage. `postgres_layout.c` now defines
`MemoryContext CurrentMemoryContext = NULL;` as a test-only data provider.
It supplies no memory context operation and claims no allocator behavior.
Parent can rerun its latest snapshot. Our authorized boundary snapshot is still
the original pre-repair allocation, so please supply a refreshed immutable
snapshot path for final independent reruns if desired.

Consumer missing-product build check ran and fails with the explicit authored
message as intended. This is negative packaging evidence, not a successful SQL
consumer build or workflow. `boundary.py` syntax compilation passes.

## Owned implementation handoff

Owned authored files are ready for integration. `BOUNDARIES.md` documents actual
entrypoints, LP64/Wasm ABI separation, global ownership, allocation/FS/entropy/
signal/time/initdb/provider dependencies, cleanup/reset risks, evidence and all
unrun gates. Native adapter compilation/object emission and Python/XML syntax
checks pass. No unsupported typed exit host implementation remains; its failing
compiler reproducer is retained.

The latest attempted suite run (`20261022-163023-8ac969d0`) stopped in the parent's
new before-suite preparation hook because the source owner's prepare.lock was
held. It ran no boundary controls; do not replace the earlier complete boundary
receipt with a success claim. It is safe for the supervisor to retry after the
source preparation work finishes. The boundary suite itself never builds root
tools or mutates refs. The fixture global provider is in place for the newer
compiler's actual-header match retry.

Native source owner independently reports 21 actual SQL/control assertions and
persistence over fresh-process reopen cycles under
`artifacts/native-control/20261022-162405-b42aad97/receipt.json`. That does not
prove managed SQL, in-process initdb or same-process instance reset, and is not a
boundary-owner execution claim. Parent integration should retain that distinction.

No files have been staged or committed by this owner. Shared planning edits,
recipe/config/root solution, source refs and compiler outputs remain outside
owned edits. Final parent reruns and raw/processed JIT/AOT product qualification
are still required.
