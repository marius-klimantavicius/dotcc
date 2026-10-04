# Runtime implementations through partial methods

`dotnet.py --unbound-externals partial` places required C# partial methods by
the types in their emitted signatures:

- **`Managed.Runtime.generated.cs`**: shared `Managed.Runtime` contracts using
  primitives, primitive pointers, BCL types and shared runtime ABI types.
- **`Translated.Runtime.generated.cs`**: partial methods on the translated owner
  for signatures involving generated records, enums, opaque types or callbacks
  containing those types. Instance callbacks belong here because their ABI
  includes the translated owner as a hidden argument.

The decision follows types recursively, not a guess based on a function name.
Both files remain dedicated contract files regardless of `--split` mode. Known
libc/runtime bindings continue to use their current implementations. The default
remains `reject`; `declarations` remains the review mode with throwing stubs.

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
instance when shared imports are used; `--instance-methods` gives each such
translated owner its own runtime.
Providers should put isolated state in instance fields. This does not make an
intentionally static provider field independent per owner.

`--runtime-source` is repeatable. It copies files into the generated project and
records paths and hashes in `dot-gen.json.externalRuntimeSources`. Partial bodies
must compile into the **same assembly** as their declarations; a referenced DLL
cannot complete them. Missing implementations are compile errors, including
`void` methods, because the contracts have explicit accessibility.
Providers must follow the generated placement: shared signatures belong to
`Managed.Runtime`; project-specific signatures belong to the translated owner.

A shared provider may implement more partial methods than this project imports.
The driver uses Roslyn to add their missing defining declarations in
`Managed.Runtime.SupplementNNN.cs`, leaving the provider source unchanged. It
processes the small contract/provider set, not the translated program's bodies.
Ordinary compiler checks still reject incorrect required signatures. Unresolved
types in supplemental shared-provider signatures need fully qualified names or
explicit aliases; bare types inferred only from namespace imports are rejected. When linking provider
files manually instead of using `--runtime-source`, their extra partial methods
also need defining declarations.

Shared calls retain source-named forwarding methods in the translated owner, including
routine-address adapters. `Managed.Runtime` names have their own allocation
domain, so changing the owner's class name does not rename a runtime import.
Signatures use the translated managed ABI: C/C++ `...` becomes a final
`params System.ReadOnlySpan<Edg.Runtime.VaArg>` parameter. An explicit C/C++
`va_list` parameter retains the raw cursor ABI (normally `Edg.Runtime.VaList*`
after parameter adjustment). These are distinct interfaces; see
[variadic arguments](varargs-span.md) for provider migration and lifetime rules.
These are not automatic native P/Invoke declarations. The manifest
records each import's `runtimeOwner` and `runtimeMethod`.

## Project-specific contracts

Contracts referring to generated types remain on the same partial owner as the
translated code, with `private static partial` in static mode and `private
partial` in instance mode. For example:

```csharp
namespace Example;
public sealed unsafe partial class Core
{
    private partial Payload host_transform(Payload arg0);
}
```

Provide the body in another `Example.Core` partial declaration, using the exact
signature. It can directly access generated types, owner state and `this`.
Ordinary direct calls and function-pointer adapters use that partial method;
there is no detour through `Managed.Runtime`. `--runtime-source` accepts these
project-specific files alongside shared providers. Unused-method supplementation
applies only to shared `Managed.Runtime`, not project-specific partial classes.

An incoming instance callback still takes its typed owner as its first argument.
Its local provider passes `this`. Static primitive-only callbacks can remain in
`Managed.Runtime`; a callback containing a generated record remains local in
either mode. Shared `Runtime.Owner` remains available to provider code as an
object reference (null in static mode), but shared contract signatures never
mention the generated owner.

Each manifest symbol records `placement` (`shared` or `translated`),
`runtimeOwner`, `runtimeMethod` and `contractFile`; summary counts and both
contract filenames appear in `dot-gen.json.unboundExternals`. Source navigation
also includes both files.

## FILE and other platform records

The EDG host-header review contains a **complete 216-byte glibc FILE layout**.
It is a generated type, so its `fopen`, `fclose`, stream-global and other typed
contracts belong in `Translated.Runtime.generated.cs`. `stat`, `dirent`, C++
ABI records and other generated records follow the same rule. A platform API's
name alone is not evidence that its data layout is portable.

The existing portable libc path provides the intended shared implementation
pattern: `runtime/libc/include/stdio.h` declares an incomplete `__edg_FILE`,
`dot_gen_imports.h` recognizes that specific ABI, and
`runtime/libc-files-bindings.def` emits typed local adapters that pass `void*`
handles to the shared BCL-backed `Edg.Runtime.LibcFiles` services. Standard stream
slots use stable `void**` storage. The local pointer type remains useful for
source correspondence; its runtime handle need not expose a native libc layout.

For new partial providers, a thin project-specific FILE adapter can similarly
call a common opaque-handle service. That service must agree with the translated
program's FILE usage. The generator does not automatically substitute opaque
BCL tokens for a complete host FILE structure: layout-dependent member accesses
would require a compatible ABI or a portable-header translation. Records such
as `stat` whose fields are actually read need an explicit layout/conversion
adapter rather than an opaque cast.

## Storage and verification

External variables have required pointer-returning hooks such as
`internal partial int* __edg_unbound_address_host_value();` in shared Runtime, or
a private owner hook when the storage uses a generated type. Generated ref
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
stable pinned storage, records/enums/opaque handles, recursive callback
signatures, incoming callbacks and independent owners. It verifies shared contracts contain no generated types.
The generated hosts are checked under JIT and NativeAOT.
