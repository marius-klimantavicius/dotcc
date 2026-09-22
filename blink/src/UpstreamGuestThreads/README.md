# Managed guest thread boundaries

This adapter connects Blink's internal thread boundaries to the authored C#
execution owner selected by the threaded product. Four whole-function ownership
boundaries now use typed semantic overrides to authored managed methods; this
conversion is source-ready and pending a new product and runtime qualification.
Earlier lifecycle receipts do not qualify the converted source state. The adapter
adds no C execution loop, guest workload, native thread fallback, or success stub.

## Exact derivation

`stage.py` consumes the reviewed `UpstreamGuestRuntime` syscall output and its
receipt, reproduces the stop/runtime chain from immutable upstream, and checks
all predecessor frozen inputs. It also pins upstream `memorymalloc.c` and
`signal.c`. The
deterministic unified diff must match `guest-threads.patch`. Base and overlay
headers, callbacks, scripts, source and predecessor receipt hashes are recorded.
Outputs must be fresh paths under campaign `generated/` or `artifacts/`.
`config/managed-boundaries.json` also pins the original implementation files and
the original `machine.h`/`syscall.h` declarations, with exact C signatures and
managed targets. Its identity and required producer units are recorded by this
stage. The compiler profile must select those rules; this source stage alone
does not remove or replace the original function bodies.

```
python3 blink/src/UpstreamGuestThreads/stage.py \
  --predecessor <reviewed-runtime-output>/syscall.c \
  --predecessor-receipt <reviewed-runtime-receipt.json> \
  --output <fresh-campaign-artifact-directory>/staged \
  --receipt <fresh-campaign-artifact-directory>/receipt.json
```

This is a new derivative of the exact single-thread runtime output. It replaces
that file's single-thread membarrier guard with a distinct explicit managed
thread profile guard. It does not change the existing runtime stage or authorize
reuse of its single-thread C# membarrier owner in a threaded profile.

## Ownership boundaries

| Boundary | Required managed behavior |
| --- | --- |
| `blink_host_guest_thread_start(child)` | Return zero only after transferring the already-created Machine to a bounded registered worker. A nonzero return guarantees no worker started and leaves upstream responsible for `FreeMachine`. Never throw after starting a worker. |
| `SysExit` → `blink_host_guest_exit(machine,status)` | Call unchanged generated `IsOrphan` exactly once, then select existing group or thread exit. The thread path records status and unwinds on that worker before native free. |
| `SysExitGroup` → `blink_host_guest_group_exit(machine,status)` | Latch the first group exit status, request cooperative stops, then unwind the calling worker. Do not join while its translated syscall or page locks remain held. |
| `KillOtherThreads` → `blink_host_guest_stop_other_threads(system)` | Reject unsupported competing callers explicitly. The current owner coordinates group shutdown through its group-exit path; this target does not native-kill or silently free the caller. |
| `SignalActor` → `blink_host_guest_signal_actor(machine)` | Run nested guest signal instructions through the C# owner's ordinary instruction budget, trace and stop checks. |
| `ClearChildTid(machine)` | Newly exported, otherwise unchanged upstream zero-and-futex-wake implementation; invoke during exactly-once worker cleanup while guest memory remains valid. |

`SysSpawn` keeps clone flags, pointer validation, `NewMachine`, register state,
TLS, child TID writes, saved mask, parent TID writes and failure cleanup. Only
the pthread launch block and its now-unused locals are replaced. `OnSpawn` is
removed because the managed owner initializes each child and executes it.
The original C bodies of `SignalActor`, `KillOtherThreads`, `SysExitGroup` and
`SysExit` remain byte-for-byte unchanged. Their declarations bind to managed
methods through `functionOverrides`, with external linkage and exact physical
upstream header selectors. The two exit rules explicitly require
`target.doesNotReturn: true`; their authored adapters also carry
`DoesNotReturn` and throw if an owner unexpectedly returns. `SysExit` preserves
the selected `HAVE_THREADS` orphan decision by calling generated upstream
`IsOrphan`, including its existing lock discipline. No replacement struct layout
or external type registration is used. The C shim declarations for these four
boundaries are removed; their authored C# targets remain.

The owner must bind each worker's Host contexts, `g_machine`, virtual mask and a
fresh jump identity before execution. Child cleanup must release syscall/page
locks and temporaries before joining or freeing. Exactly one worker owns each
`FreeMachine`; the main worker retains its Machine until children have joined,
because the final `FreeMachine` also frees System. Capturing status, clearing
child TID, callback lifetime and shared memory teardown remain owner obligations.

The selected library caller of `KillOtherThreads` is the replaced
`SysExitGroup`. Immutable upstream `blink.c` also calls it during native frontend
execution/cleanup; that frontend is not part of this product. The retained
export is not permission to run those native lifecycle paths.

## Selected profile overlay

The threaded profile selects these together; this adapter alone does not
change profile configuration:

1. Put `config/managed-threaded` before `config/managed-host` and generic includes
   for every upstream and authored C TU. Snapshot its two headers together.
2. Define `BLINK_MANAGED_GUEST_THREADS`, `HAVE_THREADS`, `NOLINEAR`, `DISABLE_JIT`.
   Remove `DISABLE_THREADS`; forbid `HAVE_FORK`,
   `HAVE_PTHREAD_PROCESS_SHARED`, `HAVE_PTHREAD_SETCANCELSTATE`.
   Both staged source prefixes and the pthread overlay include the selected
   `config.h` before checking these definitions. The selected threaded config
   must therefore precede the existing disabled-thread config in include lookup.
3. Compose this stage after the exact execution-stop/runtime stages and before
   the common host preamble; override `syscall.c`, `memorymalloc.c` and `signal.c`.
   Record the full chain and regenerate every layout-dependent object.
4. Compose the four rules in `config/managed-boundaries.json` into every relevant
   translation unit's semantic profile, resolving declaration files to the
   pinned upstream physical headers. Units without these declarations report
   absence; `syscall.c` must select `SignalActor`, `SysExitGroup` and `SysExit`,
   and `memorymalloc.c` must select `KillOtherThreads`. All selected and absent
   reports remain part of the public producer evidence.
5. Add `host-guest-threads.h` and qualified managed callbacks, the shared memory
   context, per-worker stop/binding support, and the C# owning group lifecycle.
   Qualify the process-wide memory barrier against that lifecycle before use.

`pthread.h` derives from the pinned generic BCL-only runtime header. Internal
`pthread_t` is signed 64-bit long; mutex, condition, attributes, once and key
types are four-byte ints. Initializers are zero. Mutex/condition/once and
identity primitives can use the actual generic implementations. These types
describe interpreter internals, not guest musl pthread objects or native glibc
pthread layouts. The generic create/exit names deliberately redirect to
unresolved names to prevent bypassing managed execution ownership.

The companion `signal.h` derives from the pinned campaign header, replacing only
its retained unsigned thread identity / mutex-attribute union aliases with the
managed pthread header. Campaign signal records and bindings are preserved.
`abi.h` and `retained-thread-types.h` are unchanged. Include order must not
silently choose incompatible aliases.

The overlay declares private `pthread_sigmask`, `pthread_kill`, and
`pthread_atfork` bindings through `host-guest-threads.h`:

- Mask changes use the virtual per-worker mask and return pthread error numbers
  directly, preserving errno.
- Kill requests use managed worker lookup and cooperative guest signal/wakeup
  semantics, never host POSIX signal delivery.
- Atfork explicitly returns `ENOTSUP`, preserves errno, and registers nothing.
  Retained demangler code can fail its upstream assertion if it reaches that
  call. The selected service does not invoke the demangler's fork setup; the
  profile makes no successful external demangling/fork claim.

## Qualification scope

No source receipt proves threaded execution. Runtime qualification must cover both
header orders, scalar sizes/alignment and pointer signatures, zero initializers,
and the offsets/strides of all threaded Machine/System/Bus/Futex/Fds records.
Compare against a native probe using this managed storage ABI separately from
the ordinary native pthread semantic oracle. Verify direct and object-linked
raw/optimized JIT/AOT calls and compacting-GC pointer stability.

Generic condition waits use host realtime; their clock domain must match the
campaign realtime source. Futex timed waits must observe cooperative stops
without skipping upstream bookkeeping; untimed page-lock waits need orderly
cleanup/wakeup. Handle allocation/destruction and bus/global registry retention
need explicit accounting. Prior runtime receipts cover the existing lifecycle;
they do not qualify the new signal callbacks or their combined source state.

## Thread-directed signal staging

The Kestrel activation signal exposed two additional owner boundaries. The
threaded semantic profile replaces `SignalActor`'s C interpreter loop with the
typed managed target `blink_host_guest_signal_actor(Machine*)`; no C body patch
is used. The authored owner must use its normal
instruction accounting, tracing and cooperative stop checks during nested signal
execution. `DeliverSignalRecursively`, signal selection, frame construction,
mask changes and `SigRestore` remain upstream algorithms. There is no new C
execution loop, runtime-handler substitute or signal-number special case.

`blink_host_guest_signal_checkpoint(Machine*)` runs at `ConsumeSignal` entry,
before the metal-mode check and signal lock. On the actual owning worker it
acknowledges the transient private wake generation before inspecting pending,
masked or ignored signals. It must not clear guest pending bits or turn a signal
wake into a permanent execution-stop request. This allows subsequent operation
tokens to be fresh after the pending notification was observed.

Three metadata callbacks preserve actual thread-directed sender identity:

| Callback | Contract |
| --- | --- |
| `blink_host_guest_signal_enqueue_info(machine, signal, pid, uid)` | Under `System.sig_lock`, record the actual `SysTkill` sender before `EnqueueSignal`. Record only when the pending bit is clear; coalesced signals retain the first sender. Keep at most 64 pending entries per live Machine. |
| `blink_host_guest_signal_deliver_tkill(machine, signal, pid, uid)` | For immediate self delivery, scope sender metadata separately and call unchanged `DeliverSignal` with SI_TKILL. Restore the scope on unwind; do not consume a separately pending signal's metadata. |
| `blink_host_guest_signal_apply_info(machine, signal, info)` | Apply scoped immediate metadata, otherwise consume queued metadata only after the pending bit was cleared by `ConsumeSignalImpl`. Set only code/PID/UID; absent metadata leaves the upstream frame untouched. A null pointer discards queued metadata before default/ignored delivery. |

`SysTkill` supplies its actual private `System.pid` and bound `getuid()` result.
Both masked-self enqueue and cross-thread enqueue run under the target signal
lock. After releasing that lock, cross-thread delivery calls
`blink_host_guest_signal_wake(targetMachine)` in place of the upstream
`pthread_kill(target->thread, SIGSYS)` notification. The callback returns a POSIX
error number directly and preserves errno. It identifies the exact live target
Machine and latches a private wake even before that worker binds. Using the
original pthread identity here is ambiguous during clone startup because
`NewMachine` initializes `Machine.thread` with the creator's identity; the
child's owning dispatcher replaces it only when that child starts execution.
Signal-zero pthread existence probes remain a separate unchanged operation.
This wake is not the guest signal number and never emits a native host signal.
The managed owner must qualify a real wake/bounded delivery path and remove
metadata before releasing a Machine. A successful notification alone does not
prove handler completion or general host pthread signal support.

`signal.c` applies metadata after the upstream fault-address branch so an actual
TKILL of a normally fault-associated signal retains its sender union fields.
An unrelated synchronous fault must not consume metadata for a signal whose
pending bit remains set. Default and ignored pending signals discard metadata
before returning. Static assertions pin guest siginfo code/PID/UID offsets
8/16/20, four-byte field sizes and SI_TKILL=-6 for the authored bridge. No Machine
layout changes or global fabricated sender identity are introduced.

The deterministic checked patch includes the narrow launch, signal metadata and
profile-guard changes in all three pinned source files. Whole-function boundary
replacements and the former `SysExit` exit-arm patch are absent.
`UpstreamMremap` replays this exact new predecessor and records the signal source
pin while retaining only its existing source-range validation change. Profile
staging admits the three-file replacement set and records each staged hash.
These signal additions are source preparation; actual execution qualification
requires a new coherent generated product and separately reviewed owner.
