# Managed guest thread foundation (inactive)

This source-only adapter prepares Blink's internal thread boundaries for a
separate authored C# execution owner. It has not been compiled or runtime
qualified and is not selected by the active single-thread product. It adds no
C execution loop, guest workload, native thread fallback, or success stub.

## Exact derivation

`stage.py` consumes the reviewed `UpstreamGuestRuntime` syscall output and its
receipt, reproduces the stop/runtime chain from immutable upstream, and checks
all predecessor frozen inputs. It also pins upstream `memorymalloc.c`. The
deterministic unified diff must match `guest-threads.patch`. Base and overlay
headers, callbacks, scripts, source and predecessor receipt hashes are recorded.
Outputs must be fresh paths under campaign `generated/` or `artifacts/`.

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
| `blink_host_guest_thread_exit(machine,status)` | Record this thread's status and throw a private owner unwind on that same worker before any native free. |
| `blink_host_guest_group_exit(machine,status)` | Latch the first group exit status, request cooperative stops, then unwind the calling worker. Do not join while its translated syscall or page locks remain held. |
| `blink_host_guest_stop_other_threads(system)` | Only an owner-coordinated stop/join point may invoke it. Return after other workers are joined; reject unsupported competing callers explicitly. Never native-kill or silently free the caller. |
| `ClearChildTid(machine)` | Newly exported, otherwise unchanged upstream zero-and-futex-wake implementation; invoke during exactly-once worker cleanup while guest memory remains valid. |

`SysSpawn` keeps clone flags, pointer validation, `NewMachine`, register state,
TLS, child TID writes, saved mask, parent TID writes and failure cleanup. Only
the pthread launch block and its now-unused locals are replaced. `OnSpawn` is
removed because the managed owner initializes each child and executes it.
`SysExit` retains its orphan decision. `SysExitGroup` unwinds immediately;
upstream native exit/kill/free actions are replaced by managed ownership.

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

## Proposed profile overlay

The coordinator must review and select all of these together; this adapter
does not change profile configuration:

1. Put `config/managed-threaded` before `config/managed-host` and generic includes
   for every upstream and authored C TU. Snapshot its two headers together.
2. Define `BLINK_MANAGED_GUEST_THREADS`, `HAVE_THREADS`, `NOLINEAR`, `DISABLE_JIT`.
   Remove `DISABLE_THREADS`; forbid `HAVE_FORK`,
   `HAVE_PTHREAD_PROCESS_SHARED`, `HAVE_PTHREAD_SETCANCELSTATE`.
   Both staged source prefixes and the pthread overlay include the selected
   `config.h` before checking these definitions. The selected threaded config
   must therefore precede the existing disabled-thread config in include lookup.
3. Compose this stage after the exact execution-stop/runtime stages and before
   the common host preamble; override both `syscall.c` and `memorymalloc.c`.
   Record the full chain and regenerate every layout-dependent object.
4. Add `host-guest-threads.h` and qualified managed callbacks, the shared memory
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

## Qualification still required

No source receipt proves threaded execution. Before activation, validate both
header orders, scalar sizes/alignment and pointer signatures, zero initializers,
and the offsets/strides of all threaded Machine/System/Bus/Futex/Fds records.
Compare against a native probe using this managed storage ABI separately from
the ordinary native pthread semantic oracle. Verify direct and object-linked
raw/optimized JIT/AOT calls and compacting-GC pointer stability.

Generic condition waits use host realtime; their clock domain must match the
campaign realtime source. Futex timed waits must observe cooperative stops
without skipping upstream bookkeeping; untimed page-lock waits need orderly
cleanup/wakeup. Handle allocation/destruction and bus/global registry retention
need explicit accounting. No runtime or lifecycle acceptance is claimed here.
