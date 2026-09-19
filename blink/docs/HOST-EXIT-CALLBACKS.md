# Private exit callbacks

`src/Host/HostExitCallbacks.c` implements real `atexit`
registration and explicitly invoked cleanup for one worker context. Its header
includes `<stdlib.h>` and redirects the public name to `blink_host_atexit`.
Both direct calls and registered C callback pointers use the translated C
implementation. There is no OS `atexit` registration or process exit call in
the product module.

At pinned upstream revision `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`,
the native-selected closure retains these registrations:

| Source | Callback | Dependencies and effect |
| --- | --- | --- |
| `startdir.c:37` | `FreeStartDir` | Frees the cached current-directory string and clears its global pointer. |
| `log.c:186` | `FreeLogPath` | Frees the expanded log-path string and clears its global pointer. |
| `overlays.c:141` | `FreeOverlays` | Frees the overlay string list and clears its global pointer; also called synchronously when replacing overlays. |
| `demangle.c:148` | `CloseCxxFilt` | Registered after successful child setup; closes descriptors, waits for the child, and brackets this work with the virtual signal mask. |

The demangler's process creation, waiting, and child lifecycle remain separate
dependencies. Qualifying registration does not qualify that subprocess path.
The other three registration sites ignore allocation failure from `atexit`;
their behavior is unchanged, so the embedding must retain the documented
bounded context lifetime and treat unexpected resource failures as failures.

## State and limits

`BlinkHostExitCallbacksBegin()` starts an empty context on the current thread.
The table holds 128 callback pointers in pinned, translated thread-local C
storage, with no managed references in C slots. Duplicate registrations are
independent entries. Registration returns zero on success. Null callbacks fail
with `EINVAL`, an unbound context with `ENODEV`, and a full table with `ENOMEM`.
Successful calls do not clear `errno`.

`BlinkHostExitCallbacksRun()` pops and invokes callbacks in reverse registration
order. Registrations made by a callback are accepted and run next. Nested Run
or Begin calls fail with `EBUSY`. An invocation is counted and its slot cleared
before calling it. At most 256 callbacks may run in one context. If more remain,
Run returns `ELOOP`, preserving the pending table and invocation count; later
Run or registration calls fail with `ECANCELED`, and Begin remains `EBUSY`.
The owner must explicitly End/discard this failed context. The invocation limit
does not bound time spent inside one callback, and cannot interrupt a callback
that loops forever.

After a successful drain, Run is idempotent and new registrations fail with
`ECANCELED`. `BlinkHostExitCallbacksEnd()` clears all slots and counters without
calling anything, and is idempotent. A generation counter prevents an old Run
from modifying a context that a callback explicitly ended and replaced: the
old Run returns `ECANCELED`, leaving the new context intact.

A callback exception or `longjmp` escapes Run normally. The popped callback is
not replayed; the remaining entries and invocation count remain observable,
and the phase remains Running, so Run/Begin cannot resume it. There is no
portable C catch/finally mechanism here. The embedding must catch an escape at
its boundary and End/discard the whole context; it must not register more work
in that abandoned Running state. Native staged tests exercise a real POSIX
`setjmp`/`longjmp`, while managed tests exercise both the generic numeric jump
intrinsic and a CLR exception from an actual callback function pointer.

## Embedding order

Include `HostExitCallbacks.h` in the binding preamble and add
`HostExitCallbacks.c` to the C closure. No C# bridge is needed. Begin must run
before any upstream initialization that might register cleanup. After upstream
execution stops, perform ordinary machine/system destruction that may still
log, then Run callbacks while descriptor services, virtual masks, malloc and
mapping storage, and cached environment values remain valid. Discard upstream
state and cached pointers before End and final host-owner teardown. Immediate
exit/abort paths skip Run and explicitly discard registrations.

These operations are thread-affine and must remain on the bound worker.
Clearing callbacks does not reset every upstream global/once flag or slab
cache and does not qualify reuse of arbitrary upstream state on that worker.

## Evidence

`tests/HostExitCallbacks/run.py` builds a native libc oracle in a subprocess,
which registers duplicate callbacks and registers a new callback during exit;
both it and the staged native adapter produce the exact `ABCA` trace. The
private native checks additionally cover slot overflow, invocation exhaustion,
nested Run/Begin refusal, state replacement from inside a callback, repeated
End/Run behavior, and escaping native `longjmp`. Native staged compilation uses
the real system `<setjmp.h>`, rather than opaque managed jump storage.

The runner translates the same C fixture and adapter and compares output in
raw/optimized JIT/AOT. Its C# consumer forces compacting GC between registration
and invocation, tests two simultaneous worker tables, and checks a callback
that throws through the translated function pointer. `--object-link` emits the
two C translation units separately before linking. Generated raw source is
hashed and retained unchanged; only a copy is postprocessed, and `CS8500` is
treated as an error. Each attempt records source/header/compiler hashes,
commands, logs, and AOT binary hashes under `artifacts/host-exit-callbacks/`.
This is host-boundary evidence, not a claim that the complete Blink core or its
demangler process path has executed.

Final direct receipt: `artifacts/host-exit-callbacks/attempt-p9g8tj4y/receipt.json`.
Final separate-object receipt:
`artifacts/host-exit-callbacks/attempt-_nkm9dcr/receipt.json`. Both passed native,
staged native, and all four managed modes, including context replacement from
inside a callback.
