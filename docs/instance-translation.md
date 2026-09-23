# Instance managed libraries

`--instance-methods` is an opt-in C-to-managed calling convention. Static output
remains the default. Select this option when emitting each object and again at
managed-library link, with `--runtime=c`. Whole-source managed-library emission
also supports it. Executable, native shared-library, Zig runtime and native
function-pointer conventions are not supported in this mode. Do not combine it
with `--state-context`; instance mode already owns program state.

```sh
dotnet DotCC.dll input.c --emit=obj --instance-methods -o input.o.cs
dotnet DotCC.dll input.o.cs --emit=managedlib --instance-methods --runtime=c \
  --class-name=ProgramOwner --nest-types --split=function -o output
```

The corresponding API option is `CSharpOutputOptions.InstanceMethods`.
Objects record `calling-convention:instance-v1` or `static-v1`; the linker rejects
mixed inputs and a convention inconsistent with its options. Older objects with
no convention marker are static. Layout-only options remain link-time options.

The emitted sealed owner has a public parameterless constructor, instance C
methods and a public `Dispose()` method. Its ordinary globals, aligned backing,
function-local statics and C TLS belong to that owner. C TLS is further separated
by host thread. Addressable backing remains pinned and rooted until quiescent
owner disposal. Nested C structs retain their original unmanaged layout.

Use `using (owner.__DotCcEnter())` around synchronous entry from managed host code.
This binds the shared libc runtime ownership; the binding is thread-affine,
stack ordered and must not span an `await`. Instance global and TLS access always
uses the called owner, even when another owner is bound, but direct calls using
shared runtime services require the matching entry binding. Constructor
initialization establishes its own temporary binding. Disposing an owner with
active bindings, threads or retained callbacks fails and preserves its backing.
`__DotCcEnter()`, `__DotCcContext.Enter()` and `Libc.RuntimeContext.Enter()` return
the concrete `Libc.RuntimeBinding` struct, so normal `using` scopes do not allocate
or box a disposable object. Keep the concrete type (`var` is suitable); converting
it to `IDisposable` or capturing its `Dispose` method as a delegate can box it.
Dispose the scope on its entering thread in stack order. Repeated disposal of
the same mutable variable and disposal of a default scope are harmless; disposing
a stale copy is rejected rather than releasing a different active binding.
Deferred `__DotCcRetain()` leases remain reference objects because they support
shared, idempotent release across threads.

A C function pointer remains one pointer wide, with generated type
`delegate*<ProgramOwner, ..., TResult>`. It identifies shared code, not an owner.
Every indirect translated call supplies `this`. Taking a method address returns
a cached static adapter; the adapter binds its supplied owner and restores the
previous runtime binding on return or exception. Calling A's pointer with B uses
B's state. Struct fields, arrays, callback tables, function parameters/returns,
variadic calls and supported pointer casts use the same explicit convention.
Public pointer APIs expose the real signature.

A deferred registration must retain both the code pointer and its intended
owner. Hold `owner.__DotCcRetain()` until callbacks can no longer execute. This is
a thread-independent, idempotent reservation, not an ambient binding; the
callback adapter performs binding on the invoking thread. Merely retaining a
managed owner reference does not prevent explicit disposal. Do not call through
a pointer or use guest storage after owner disposal.

Shared libc `qsort`, `bsearch`, `pthread_create` and `pthread_once` have typed context-forwarding
overloads. Thread creation reserves the explicit origin before launch and keeps
it until completion, independent of the caller's ambient binding. Other external
callback boundaries need a typed semantic override:

```json
"target": {
  "kind": "managedMethod",
  "method": "global::Host.Register",
  "passInstance": true
}
```

The static managed target receives the generated owner first, followed by the C
parameters. Its callback types must include that owner argument. The target owns
registration, retention and cancellation semantics. The compiler rejects known
unadapted callback parameters, callback returns and callback-bearing aggregates;
unused declarations alone do not cause rejection. This checks available typed
signatures, not arbitrary erased `void*` payload protocols or opaque external
aggregate internals; hosts must explicitly define those protocols and cannot
reinterpret static-convention callback bits. Unsupported callback operations,
such as unadapted libc callback registrations, diagnose at translation/link time.

The `passInstance` flag participates in semantic profile hashes, object contract
matching and selected-override reports. It is invalid for intrinsic targets or
static translation. Existing noncallback managed overrides remain usable without
it. Function pointer callback signatures in managed overrides require this flag.

Focused unit/functional tests cover static compatibility, object ABI rejection,
managed boundaries, C data layout, owner/TLS separation and runtime callbacks.
`Scripts/instance-methods/run.py` adds an actual Linux native witness plus JIT and
NativeAOT execution using the same normal functional fixture. This calling
convention is a managed embedding ABI, not native C callback interoperability or
an OS security boundary.
