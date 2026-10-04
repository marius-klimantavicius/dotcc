# Runtime implementations through partial methods

`dotnet.py --unbound-externals partial` emits required C# partial methods in
**`Managed.Runtime`**, independent of `--namespace`, `--class-name`, and file
splitting. Contracts live in `Managed.Runtime.generated.cs`. Known libc/runtime
bindings continue to use their current implementations. The default remains
`reject`; `declarations` remains the review mode with throwing stubs.

For example, `extern int host_add(int);` produces this contract:

```csharp
namespace Managed;
public unsafe partial class Runtime
{
    internal partial int host_add(int arg0);
}
```

Supply its implementation in a common runtime source file:

```csharp
namespace Managed;
public unsafe partial class Runtime
{
    internal partial int host_add(int arg0) => arg0 + 10;
}
```

```sh
python3 edgcpp/dotnet.py input.c --language c --emit managedlib \
  --namespace Example --class-name Core --unbound-externals partial \
  --runtime-source HostRuntime.cs --output-dir output
```

The same provider file works when the translated owner's name, namespace or
static/instance mode changes. `Managed.Runtime` has a fixed identity; its methods
are instance methods in both modes. A static translated owner holds one runtime
instance; `--instance-methods` gives every translated owner its own runtime.
Providers should put isolated state in instance fields. This does not make an
intentionally static provider field independent per owner.

`--runtime-source` is repeatable. It copies files into the generated project and
records paths and hashes in `dot-gen.json.externalRuntimeSources`. Partial bodies
must compile into the **same assembly** as their declarations; a referenced DLL
cannot complete them. Missing implementations are compile errors, including
`void` methods, because the contracts have explicit `internal` accessibility.
Existing providers that implemented owner-class partial methods must move those
bodies to `Managed.Runtime` and use the generated contract signatures.

A shared provider may implement more partial methods than this project imports.
The driver uses Roslyn to add their missing defining declarations in
`Managed.Runtime.SupplementNNN.cs`, leaving the provider source unchanged. It
processes the small contract/provider set, not the translated program's bodies.
Ordinary compiler checks still reject incorrect required signatures. For custom
generated types, use fully qualified names or explicit aliases; unresolved bare
types inferred only from namespace imports are rejected. When linking provider
files manually instead of using `--runtime-source`, their extra partial methods
also need defining declarations.

Calls retain source-named forwarding methods in the translated owner, including
routine-address adapters. `Managed.Runtime` names have their own allocation
domain, so changing the owner's class name does not rename a runtime import.
Signatures use the translated managed ABI, including `VaList*` for variadic
functions. These are not automatic native P/Invoke declarations. The manifest
records each import's `runtimeOwner` and `runtimeMethod`.

## Generated types and callbacks

Record, enum and callback signatures name their exact generated types. Moving a
record between namespaces or changing type nesting therefore needs a small
project-specific alias/adapter file; a common namespace does not make distinct
C# struct types interchangeable. C++ overloads may also require disambiguated
names. Use the generated contract as the ABI reference.

The runtime exposes `internal object? Owner`: it is the translated owner in
instance mode and null in static mode. An incoming managed callback in instance
mode still takes its typed owner as its first argument. A provider can invoke it
with `(OwnerAlias)Owner!`, where `OwnerAlias` names that generated class. Static
callbacks omit that argument. These mode-specific adapters preserve the existing
function-pointer ABI and owner isolation; primitive-only providers need none.

## Storage and verification

External variables have required pointer-returning hooks such as
`internal partial int* __edg_unbound_address_host_value();`. Generated ref
properties and address expressions use the same hook. It must return stable
storage with the source type's alignment, lifetime, sharing and TLS behavior.
Pinned arrays suit persistent globals/TLS; `NativeMemory.Alloc`/`Free` suit
explicitly owned native allocations. Providers own resource cleanup and C/C++
error conventions. Prefer BCL services or runtime platform selection.

This mechanism does not implement outstanding project imports.
`dot-gen.json.unboundExternals.requiresImplementation` remains true when
providers are supplied: a successful build establishes that the contracts were
completed. NativeAOT and runtime behavior still need verification.

`backend/tests/partial_externals_verify.py` exercises the same primitive provider
under different namespaces/classes and both owner modes, unused provider
methods, missing-body errors, direct/indirect calls, BCL native allocation,
stable pinned storage, records/enums, incoming callbacks and independent owners.
The generated hosts are checked under JIT and NativeAOT.
