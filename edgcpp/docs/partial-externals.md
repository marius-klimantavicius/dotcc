# Runtime implementations through partial methods

`dotnet.py --unbound-externals partial` emits required C# partial methods for
functions that have no existing runtime binding. Known libc/runtime bindings
continue to use their current implementations. The default remains `reject`;
`declarations` remains the separate review mode with throwing stubs.

For example, a native declaration `extern int host_add(int);` produces:

```csharp
private static partial int host_add(int arg0);
```

Supply its implementation in another file with the same namespace and owner:

```csharp
namespace Example;
public static unsafe partial class Core
{
    private static partial int host_add(int arg0) => arg0 + 10;
}
```

```sh
python3 edgcpp/dotnet.py input.c --language c --emit managedlib \
  --namespace Example --class-name Core --unbound-externals partial \
  --runtime-source HostRuntime.cs --output-dir output
```

`--runtime-source` is repeatable. The driver copies the C# files into the
generated project and records their original paths and hashes in
`dot-gen.json.externalRuntimeSources`. Implementations compile into the same
assembly as the generated partial class. An implementation in a referenced
assembly cannot complete a partial method. Missing implementations produce C#
compile errors, including for `void` methods: the explicit `private` modifier
prevents optional-partial-call erasure.

In `--instance-methods` mode, methods are instance partial methods and the owner
is a sealed partial class. Runtime state can then belong to that instance.
Managed function-pointer adapters call the same implementations. Signatures
use translated types and the generated managed ABI, including the typed
`VaList*` representation for variadic functions; they are not automatic native
P/Invoke declarations. Use the actual generated declaration when implementing
overloads, C++ symbols or collision-renamed identifiers.

External variables have a required pointer-returning partial method such as
`private static partial int* __edg_unbound_address_host_value();`. The generated
ref property and address expressions both use this method. Its implementation
must return stable storage with the source type's alignment, lifetime, sharing
and TLS/instance behavior. Pinned arrays suit persistent globals/TLS;
`NativeMemory.Alloc`/`Free` suit explicitly owned native allocations. Providers
remain responsible for platform behavior and C/C++ error conventions; BCL
implementations are preferred.

This mechanism does not implement every outstanding EDG or project import.
`dot-gen.json.unboundExternals.requiresImplementation` records required hooks,
even when provider files were supplied; only a successful build proves they
are all implemented. Providers also need runtime and NativeAOT verification.

The focused `backend/tests/partial_externals_verify.py` test checks missing
implementation diagnostics, direct/indirect calls, a BCL native-memory provider,
external-variable address identity, and independent owner state under both JIT
and NativeAOT.
