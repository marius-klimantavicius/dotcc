# Lexical signal jumps and pending guest signals

The bounded signal-jump boundary passes native, emitted JIT, object-linked JIT
and normally trimmed Linux x64 NativeAOT execution. This does not establish a
translated PostgreSQL SQL workflow. The supervisor retains complete-source
integration, actual startup/error cleanup, all product matrix rows and P6.

## Lowering and lifetime

`<setjmp.h>` declares `sigsetjmp(env, savemask)` and `siglongjmp(env, value)`.
The builder recognizes the existing lexical guard, switch, declaration capture,
assignment capture and negated/assignment-guard forms. A compiler-only `Call`
environment inside the existing jump IR carries both arguments through ordinary
IR walkers. The backend materializes the buffer and save request once, then
creates a managed `SignalJumpActivation` around the lexical region.

`if (prefix && sigsetjmp(env, save) != 0)` also works, including the actual
`pg_get_line.c` pointer prefix and cast/dereference buffer expression. A terminal
bare, negated or zero-comparison jump test is supported on the right of `&&`.
The prefix truth value is cached once. False evaluates neither jump argument
and creates no activation; true arms once. The existing capture-tail rewrite
keeps that handler active across the remainder of the same lexical block.
Recovery reuses the cached prefix rather than rerunning its side effects.

The C buffer remains one unmanaged numeric identity slot. Mask, owner and thread
state stay in the managed activation, with no global registry retaining old
frames. `siglongjmp` uses the existing tagged numeric jump exception and maps
zero to one. Distinct and rearmed buffers retain fresh identities. A matching
catch validates the activation and throwing owner/thread, restores the saved
mask, then executes recovery or writes the result and restarts the region.
Checking the throwing owner matters because a foreign binding may already have
unwound before the matching catch runs. Ordinary `setjmp` still calls
`ArmJumpBuffer` and captures no signal mask.

Nonzero `savemask` requires the opted-in live cooperative signal service. Zero
requires no signal-mask capability and leaves the current mask alone. A jump
escaping a signal callback skips the dispatcher's normal handler-return mask
restoration: the saving lexical catch restores its own snapshot, while a
non-saving jump retains the handler's mask. Restoration itself never invokes
pending callbacks; delivery waits for the next cooperative safe point.

Disposal retires the lexical activation without dereferencing guest storage.
An expired target has no matching live handler and the jump exception escapes;
foreign-owner/thread or terminal-owner recovery is explicitly rejected. This is
not arbitrary nonlexical resumption. Basic guards retain their existing bounded
try/catch extent; value captures and conditional-prefix guards cover their block
tail. Unsupported loop/ternary/arithmetic uses, mixed boolean tests and
side-effecting capture lvalues remain errors. Function-pointer shadows of
`sigsetjmp` remain ordinary calls, with no jump recognition.

The fork's top-level `postgresmain_sigjmp_buf` interception is separate.
`pgl_longjmp` still turns that particular target into exit 100, and actual
`PostgresMainLongJmp` transaction/error cleanup remains a translated-core product
requirement. Lexical lowering does not replace that cleanup with host code.

## Supervisor backend hooks

The supervisor applied these two hooks in its owned `CSharpBackend.cs`:

```diff
 case SetjmpGuard sj:
 {
+    if (TryEmitSignalJumpGuard(sb, sj, ind, pad)) break;
     // existing ordinary setjmp guard emission follows
 case SetjmpCapture sc:
 {
+    if (TryEmitSignalJumpCapture(sb, sc, ind, pad)) break;
     // existing ordinary setjmp capture emission follows
```

All new emission lives in `CSharpBackend.SignalJumps.cs`. No additional main
backend hook is required for conditional-prefix guards.

## Pending and synchronous consumption

`sigpending` observes owned timer expiry and reports blocked pending guest bits
without consuming them or calling handlers. It returns 0 or -1 with errno.
`sigwait` consumes one selected blocked bit, including coalesced SIGPIPE used by
actual libpq, or waits for a selected active owned ITIMER_REAL expiration. It
writes the selected signal only on success, returns positive errors, and
preserves errno. Masks, pending bits and timers belong to one live owner thread.

Without signal opt-in these services report ENOTSUP. Invalid pointers, masks,
thread access and retired owners fail explicitly. This serialized cooperative
service has no asynchronous foreign-thread sender: a wait without a selected
pending bit or selected active timer returns ENOTSUP. It does not return a
successful invented signal or install host process signal dispositions.

## Current evidence

Outputs are private and ignored under `build/signal-jump-owner-20261022` and
`artifacts/signal-jump-owner-20261022`. Immutable compiler snapshot
`build/tools/supervisor-20261022-1812/dotcc.dll` includes the final signal,
pending/wait, conditional-prefix and subsequent structural implementations.

- `unit-conditional.log`: 70 focused jump, lifecycle, signal and existing setjmp
  checks pass, including stale/foreign/thread negatives, exact masks, pending
  coalescing/selection, timer consumption and unsupported-shape diagnostics.
- `functional-conditional.log`: four direct/object and translation-unit-order
  permutations pass. Each executes two fresh generated owner contexts, lazy
  prefix/operand checks, nested/repeated jumps, shadowed functions, ordinary
  setjmp mask behavior and pending/timer consumption.
- `final1812/report.json`: native, actual direct/object JIT and normally trimmed
  NativeAOT boundary probes pass with matching native observations. Managed
  probes additionally reject expired and foreign-owner recovery.
- `suite1812/boundary/report.json`: complete existing boundary adapter passes
  with no blockers. Actual-header ABI, real fork callback/cleanup slices,
  repeated 99/100 yields, fresh generated contexts and the actual adapted
  CHECK_FOR_INTERRUPTS timer loop remain covered.
- `suite1812/boundary-nativeaot-{publish,run}.log` and
  `suite1812/boundary/aot-managed.json`: the same complete boundary fixture publishes
  and executes with normal NativeAOT trimming; all observations equal JIT.
  `suite1812/all-modes.json` records the comparison. The first current refresh
  caught a missing authored project reference after initializer integration;
  the supervisor added the actual `InitializationBackend.Signals.cs` source and
  the refreshed JIT/AOT gate passes. The initial error log remains retained.
- `retry-commands.json`: original complete `postgres.c` and `elog.c` objects
  emit successfully with private guarded overlays and no unresolved includes.
  The original nested PG_TRY/PG_CATCH/PG_END_TRY macros execute natively and in
  emitted code (`pg-try-final-run.log`). That diagnostic supplies an explicit
  abort trap for its unused PG_FINALLY-only rethrow reference; it does not
  substitute for executing the real complete elog/error path.
- `pg-get-line-1802-command.json` and `pg-get-line-1802.log`: the actual complete
  frontend unit now emits. The initializer owner owns the full frontend retry;
  a duplicate private attempt was cancelled and is not a successful receipt.

No reference/configured source or generated C# was edited. Shared tool builds
belong to the supervisor. No commit, push or PR was made by this owner.

## Additional actual-source compiler repairs

The supervisor expanded this owner's scope to two subsequent failures. A new
block declaration rule accepts `extern Type declarators`, including the real
`extern char **environ;` in `postmaster.c`. The builder binds a lexical alias to
the canonical external object symbol or visible global object; it defines no
local or global storage. The canonical target name survives local-name shadows
and separate object emission, and later definitions share the same symbol and
address-taken facts. Conflicting declarations and block extern initializers
are rejected. Existing block function prototypes remain functions.

The real `bufmgr.c` cast `(BlockNumber (*)[MAX_FORKNUM + 1]) palloc(...)` now
builds the existing pointer-to-array IR type, retaining complete row dimensions
for indexing, arithmetic and sizeof. Runtime variable bounds remain an explicit
unsupported case. No backend hook or flattening of row strides was added.

`structural-build.log` reports a successful private parser/compiler build.
`structural-unit-final.log` passes 61 focused structural/global/signal tests;
the earlier seven failures were corrected test-harness assertions, with their
original log retained. `structural-functional.log` passes six structural native-
matched direct/object executions plus the four signal execution permutations;
`functional-final1812.log` refreshes all ten successfully with the final source.
The C fixtures cover external object address equality across translation units,
outer local visibility, pointer assignment, visible internal storage, 2D/3D row
strides, const views, unsigned elements and one-time cast-operand evaluation.
Native observations are `17 7 60 8` and `17 29 43 24 24 1` in the corresponding
`extern-native-run.log` and `rowcast-native-run.log`.

The supervisor owns the independent `inet_net_pton.c` multiple string-array
declarator repair and has resumed grammar ownership. Snapshot 1812 successfully
emits the actual complete `postmaster.c` and `bufmgr.c` units with no unresolved
includes; `structural-retry-1812.json` records exact commands and logs. Postmaster
retains three existing qualifier warnings. `unit-final1812.log` passes all 93
selected structural/global/jump/lifecycle checks. Complete product regeneration
and execution remain the supervisor's responsibility.
