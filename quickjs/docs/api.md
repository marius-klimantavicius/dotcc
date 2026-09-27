# Owning embedding API

Qualified on Linux x64 in raw/processed JIT and NativeAOT; consult
[validation](validation.md) for the passing build and execution matrix. The API assembly is
`Managed.QuickJs`, namespace `Managed.Interpreters`.

`QuickJsRuntime` owns the engine, allocation callbacks and in-memory module
resolver. `CreateContext()` creates a realm. `QuickJsContext.Evaluate()` returns
an owning `QuickJsValue`; dispose values when finished. Disposing a context also
releases its outstanding facade values. The runtime disposes all contexts and
retains callback roots until the underlying engine finishes teardown.

```csharp
using var runtime = new QuickJsRuntime();
using var context = runtime.CreateContext();
context.RegisterFunction("double", arguments => arguments[0] * 2);
using var value = context.Evaluate("double(21)");
Console.WriteLine(value.ToDouble());
```

`RegisterFunction` accepts a numeric managed callback. Arguments are copied to a
managed `double[]`; its numeric return becomes a JavaScript number. A managed
exception becomes a catchable JavaScript error. Registration arity is limited
to 256; callback invocations accept up to 4096 arguments. The callback cannot
reenter or dispose its active runtime. Retained JavaScript functions remain
callable after their original facade context has been disposed.

`GetGlobal`, `GetProperty` and `Duplicate` return new owned values. `SetGlobal`
borrows its input and duplicates the underlying reference; it never consumes
the caller's handle. Values can move between contexts in the same runtime, but
cannot cross runtimes. Disposed handles reject subsequent use; repeated disposal
is harmless.

Strings returned by `ToString()` preserve embedded NUL, supplementary characters
and lone UTF-16 surrogates. Source is encoded as UTF-8 with lone-surrogate
preservation. Property names, module keys and filenames supplied through this
facade must not contain NUL; those parameters use upstream NUL-terminated APIs.

Supply a dictionary of module names to source strings to the runtime constructor.
Evaluation with `module: true` uses the engine's ES-module parser/linker and the
in-memory resolver. Unknown modules fail. Call `DrainJobs()` explicitly to run
Promise reactions. It enforces a job-count budget and reports unhandled
rejections after the queue drains. The embedding has no background event loop
or implicit file/network access.

The default memory limit is 64 MiB. `AllocatedBytes` includes allocator headers;
`AllocatedBlocks` counts live owned blocks. Disposal checks that all blocks have
been released. The default stack budget is 256 KiB, with accepted settings from
64 to 512 KiB; host stack availability and requested temporary allocation space
are both checked. Evaluation and job draining accept cancellation tokens, polled
by the engine's interrupt hooks. The required JIT/NativeAOT limit, recovery and cleanup tests pass in all four
qualification cells.

Low-level host integrations that register custom classes must allocate IDs with
`QuickJsHost.NewClassId()`. It calls the engine allocator, whose upstream `CONFIG_ATOMICS` mutex
protects registration. The owning
numeric-callback facade uses the existing built-in function class.

JavaScript `Atomics` and `SharedArrayBuffer` are enabled. Owning runtimes retain
upstream's default `can_block=false`: blocking `Atomics.wait` rejects until a
low-level host explicitly opts in using `JS_SetCanBlock`. Such hosts must arrange
wait completion/notification before disposal; an engine interrupt does not wake
a pthread wait.

Calls into one runtime must not overlap; concurrent calls and callback reentry
are rejected. Separate runtimes may execute on separate threads. Disposal from
another thread asks active evaluation to interrupt and waits for it before
freeing native storage. It cannot interrupt a managed host callback that ignores
cancellation or blocks forever. Unsafe translated code is not a security
isolation boundary for hostile scripts.

The separate [consumer](../samples/ManagedConsumer/Program.cs) demonstrates
modules, JSON, numeric callbacks and Promise jobs. Set the `QuickJsProject`
MSBuild property to choose the raw generated project; its default selects the
processed project. Authored host sources are linked into both generated products
from their original paths, so host edits take effect on an ordinary rebuild.
