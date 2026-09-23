# Explicit managed C program state

For true instance methods and explicit-owner function pointers, use
[instance translation](instance-translation.md). This older option retains
static methods and its existing callback convention.

`--emit=managedlib --runtime=c --state-context` emits a library with explicitly
owned C globals. It is a link-time option; the same object files can still link
without it for the existing static-library behavior.

```csharp
using var first = Api.__DotCcCreateContext();
using var second = Api.__DotCcCreateContext();
using (first.Enter()) { Api.initialize(); Api.step(); }
using (second.Enter()) { Api.initialize(); Api.step(); }
```

Each context owns zeroed, pinned ordinary global storage, global initializers,
aligned/flexible backing, and per-context/per-thread C TLS including arrays.
Its runtime ownership includes global array roots, function-pointer array pins,
errno and private pthread registries/keys/thread identities. Generated method
and literal storage can remain shared. `pthread_create` captures the binding
and retains the context through thread completion; C pthread callbacks therefore
resolve the creating program's globals.

Bindings are thread-affine and stack ordered. Bind on every thread entering
translated code, and leave the binding before returning that thread to unrelated
work. They intentionally do not flow through `await`. Multiple threads may bind
the same context for a C multithreaded program; independent contexts may execute
concurrently. C synchronization/data-race rules still apply inside each program.
No implicit default context exists for generated C globals. `__DotCc` is reserved
for the emitted context API and helper members.

Dispose refuses live bindings or created pthreads and leaves backing intact on
that refusal. After quiescence, it releases global roots/pins and registry state;
a retired context cannot be entered again. Raw pointers borrowed from a context
are invalid after disposal. The caller must also release C-owned allocations and
host resources: this option is not a memory-safe C implementation or an automatic
`free` of every allocation made by arbitrary guest C code.

This is program-state ownership, not automatic isolation of all host facilities.
Applications must provide per-instance file/network/environment/stdio bindings
where those are required. Other embedded libc facilities (including ambient
host IO and the separate C11 thread implementation) retain their existing
contracts; this option does not silently virtualize them or impose OS isolation.
The Blink host adapters provide its selected private host contracts separately.

Focused functional coverage exercises direct compilation and object linking,
monolithic and split/nested output, global addresses across GC, over-alignment,
TLS switching/reuse, nested bindings, concurrent contexts, pthread callback
inheritance, fresh initialization and disposal refusal while bound. NativeAOT
and actual Blink lifecycle qualification are separate delivery gates.
