# Shared C++ exception provider

The linker includes `CppExceptions.cs` when reachable code uses source throw,
rethrow or handlers. Its source is shared by all translated targets; generated
modules copy the embedded resource. `--exception-provider external` emits the
same partial contracts without an implementation, for hosts supplying their own
provider (and for independent ABI tests). This requires `--allow-externs`.

The provider uses BCL APIs and managed function pointers. It supports trimming
and NativeAOT without reflection, runtime code generation or native C++ exception
APIs. Exception payloads use `NativeMemory.AlignedAlloc`/`AlignedFree`, with the
recorded EDG size/alignment. Completed payloads arm the exact translated complete
object destructor. Failed construction releases storage only after generated
rollback has run; it never calls the destructor of the incomplete object.

A thread-local linked handler stack tracks active catches. A payload has separate
pending-propagation and active-handler counts. Each `throw;` adds a propagation,
including when a destructor rethrows and catches the same object during another
rethrow's unwind. The exception stays uncaught through catch parameter
initialization; the observed `MarkCaught` marker consumes that propagation.
Handler locals are destroyed before `LeaveCatch` relinquishes ownership. The last
handler destroys/frees the payload only when no propagation remains.

Matching consumes original C++ type identities, pointer cv qualifiers and public,
unambiguous base conversions recorded in the object. It performs no numeric
promotion. Adjusted pointer catches get a separate native pointer cell; modifying
that cell does not alter the original pointer preserved for rethrow. Class base
references point into the actual payload. Typed catches do not match foreign
managed exceptions. Catch-all can handle them, except transfers excluded by
`ObjectProgram.ClassifyTransfer` (for example a runtime-owned forced thread exit).

Allocation/bookkeeping failures, invalid provider state, failed catch-parameter
construction, escaping throw-operand copy failure, and exception-object destructor
failure terminate through `Environment.FailFast`. Forced runtime transfers retain
separate classification and pass through the generated cleanup paths. An uncaught
C++ exception at the generated executable boundary terminates. There is currently
no binding to C++ `set_terminate`, `exception_ptr`, cross-thread payload transfer,
native C++ exception interoperability, or platform lifecycle release of a pending
C++ payload abandoned by a forced runtime transfer. These need their own owned contracts;
this provider does not claim those unsupported source/runtime features.

## Managed library callers

A translated library can propagate a `CppException` to its managed caller. The
caller must participate in ownership if it consumes the exception, just as a
translated catch does:

```csharp
catch (Edg.Runtime.CppException error)
{
    var handler = Edg.Runtime.CppExceptions.EnterCatch(error);
    try
    {
        Edg.Runtime.CppExceptions.MarkCaught(handler);
        // Read the payload while the handler is active, or use Rethrow().
    }
    finally { Edg.Runtime.CppExceptions.LeaveCatch(handler); }
}
```

Use `EnterTypedCatch` after a successful `FilterTypedCatch` to obtain a validated,
adjusted address. Always leave the handler after its translated locals die.
Payload pointers become invalid after release; the carrier's `ObjectAddress`
then returns null. Do not throw a caught/released carrier using managed `throw
error`: use `CppExceptions.Rethrow()` while its handler is active. There is no
finalizer that invokes arbitrary C++ destructors on the GC finalizer thread.
Handler and payload ownership are synchronous and thread-affine; passing them
across `await` or threads is outside this contract.
