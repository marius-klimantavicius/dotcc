# Semantic C function overrides

`--overrides-file` can replace a selected C function implementation with a known
compiler intrinsic or an authored static C# method. Selection happens after C
preprocessing and declaration binding. The original source must still parse,
but the selected implementation body is not lowered.

This is an explicit substitution chosen by the profile author. The compiler
checks the declared signature; it does not prove that the replacement preserves
every effect of the original function.

## Little-endian byte loads and stores

For a source declaration such as `int32_t read_int32(const uint8_t *p)`, use:

```json
{
  "version": 1,
  "functionOverrides": [
    {
      "name": "read_int32",
      "signature": {
        "returnType": "int32_t",
        "parameterTypes": ["const uint8_t *"],
        "variadic": false
      },
      "target": { "kind": "intrinsic", "name": "load.i32.le" },
      "requireMatch": true
    }
  ]
}
```

Pass the profile when translating C, including when producing object fragments:

```sh
dotcc --overrides-file overrides.json --emit=managedlib library.c -o generated/
dotcc --overrides-file overrides.json --emit=obj library.c -o library.o
```

`load.i32.le` reads four ordinary readable bytes and returns a signed 32-bit
integer in little-endian order, independently of host byte order. Unaligned
addresses are supported. Its C# implementation calls
`System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian` with a four-byte
`ReadOnlySpan<byte>` over the pointer. The pointer must identify four readable
bytes; constructing the span cannot validate an arbitrary pointer's allocation.
Volatile and atomic accesses are not supported by this intrinsic.

The same profile syntax supports these unsigned targets:

| Target | Required C signature | Managed operation |
| --- | --- | --- |
| `load.u16.le` | unsigned 16-bit result, one unsigned-byte pointer | `ReadUInt16LittleEndian` |
| `load.u32.le` | unsigned 32-bit result, one unsigned-byte pointer | `ReadUInt32LittleEndian` |
| `load.u64.le` | unsigned 64-bit result, one unsigned-byte pointer | `ReadUInt64LittleEndian` |
| `store.u16.le` | `void`, unsigned-byte pointer, unsigned 16-bit value | `WriteUInt16LittleEndian` |
| `store.u32.le` | `void`, unsigned-byte pointer, unsigned 32-bit value | `WriteUInt32LittleEndian` |
| `store.u64.le` | `void`, unsigned-byte pointer, unsigned 64-bit value | `WriteUInt64LittleEndian` |

For example, a `store.u64.le` rule can select
`void write_word(uint8_t *p, uint64_t value)`. Signature matching uses actual C
types after typedef resolution; the intrinsic then checks integer signedness
and width. Both `unsigned long` and `unsigned long long` are 64-bit types in
the current C model. A rule must still match the original declaration exactly.
Loads permit const source bytes. Stores require writable bytes and reject
const, volatile or atomic destinations. Every operation accesses exactly its
width (2, 4 or 8 bytes) and supports unaligned addresses. Stores use a
`Span<byte>` over the destination; the caller must supply writable memory.

The generated function keeps its original signature and identity. Direct calls,
exported calls and function-pointer calls use the replacement. Arguments retain
normal C-call evaluation semantics and are evaluated once. The load remains a
memory read, so an intervening write must remain observable. Stores retain call
effects as well: neither loads nor stores become pure expressions, and both
store arguments are evaluated once.

## Population count and intrinsic registration

`popcount.u64` requires a signed 32-bit C result and one unsigned 64-bit integer
parameter, such as `int count_bits(uint64_t value)`. It calls
`System.Numerics.BitOperations.PopCount` directly. Zero returns 0 and an all-one
64-bit value returns 64; the input expression is evaluated once.

The compiler's closed intrinsic registry supplies the fully qualified managed
method, its C signature predicate and an optional argument adapter. The adapter
receives rendered arguments and retains their order, using each once. Endian
operations adapt pointers to spans; population count needs no adapter. The C#
backend invokes this description without selecting a BCL class itself, so a
new managed intrinsic does not require another backend class-name branch.

## Authored managed methods

Use a qualified static method name to bind a compatible managed implementation:

```json
"target": {
  "kind": "managedMethod",
  "method": "global::MyHost.ByteIo.ReadInt32"
}
```

Add the implementation to the generated C# project or a referenced assembly:

```csharp
namespace MyHost;

public static unsafe class ByteIo
{
    public static int ReadInt32(byte* p) =>
        System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
            new System.ReadOnlySpan<byte>(p, 4));
}
```

The normal C# build resolves and validates the method. dotcc does not load the
assembly or invoke the method while translating. The target accepts a method
identifier, not arbitrary C# expression text. It does not automatically marshal
values, adapt pointers to spans, reorder parameters, supply optional arguments,
or turn synchronous signatures into tasks. Author a bridge method when needed.

A prototype-only C function may be selected. dotcc then generates its managed
implementation rather than treating the declaration as a native-library import.
Missing or incompatible C# methods fail the generated project's build.

For an explicitly `_Noreturn` C declaration, the managed target must opt into
the same termination contract:

```json
"target": {
  "kind": "managedMethod",
  "method": "global::MyHost.Process.Exit",
  "doesNotReturn": true
}
```

The flag must agree with the actual selected C declaration. It is not allowed
on intrinsic targets, and it does not grant a termination contract to an
ordinary returning C function. Omitted or false retains the existing returning
target behavior and profile identity. The generated wrapper retains its original
signature, invokes the target once, and throws `UnreachableException` if the
target unexpectedly returns. A target's actual exception or process termination
propagates normally. This also covers indirect calls through the wrapper's
function pointer. The explicit flag participates in object contract
compatibility and is recorded in the typed selection report; older objects
without the flag retain the false contract.

## Selection

Every rule requires an exact original C function `name`, a `signature`, and a
`target`. The signature specifies `returnType` and `parameterTypes`, with optional
`variadic` (default `false`). Typedef spellings are resolved in the selected C
scope. Parameter names do not participate in selection. Variadic replacements
are unsupported.

Optional selectors narrow a rule:

| Field | Meaning |
| --- | --- |
| `linkage` | `external` or `internal`. |
| `translationUnit` | The physical C translation-unit path. |
| `declarationFile` | The physical file containing the selected declaration. |
| `requireMatch` | Fail if this compilation invocation selects no function. |

Relative selector paths in JSON resolve against the profile's directory.
Diagnostic `#line` names do not replace physical source identity. Redeclarations
of one logical function count as one selection. A rule matching different static
functions in different translation units is ambiguous; select a translation
unit explicitly. Two rules selecting the same function are also an error.

A matching name and scope with a changed signature is an error, including when
`requireMatch` is false. An absent optional rule is permitted. Local variables,
function-pointer variables and members with the same name are not rewritten.
Unknown profile fields, unknown intrinsics and invalid method names are rejected.
Function rules coexist with `macroOverrides`, `fieldTypeNames` and `externalTypes`.

## Separate compilation and limits

Apply profiles when translating each C source. Object fragments preserve the
selected replacement and its signature so linking can reject incompatible
replacements, including a replacement conflicting with an original definition.
A profile hash alone is not the compatibility check. Object-only linking cannot
apply a new function override: rebuild the relevant objects from C.

Use `--override-report` to inspect rule selection and target provenance. Matching
state belongs to each compilation invocation, so reusing the same options object
does not carry successful matches into a later invocation.

The initial targets support C input and C# output. WAT rejects these replacements
instead of falling back to the original implementation. This feature does not
include automatic recognition of equivalent C bodies, body-hash guards, byte
widths or byte orders beyond the listed targets, or arbitrary C# templates.

## Qualification

The compiler test suites cover schema validation, typedef resolution, physical
selectors, invocation isolation, wrapper identity, object conflicts, argument
effects and authored managed methods/types. Run the focused cases with:

```sh
dotnet test DotCC.Tests --filter 'FullyQualifiedName~SemanticFunctionOverrideTests|FullyQualifiedName~ExternalTypeTests'
dotnet test DotCC.FunctionalTests --filter 'FullyQualifiedName~ManagedLibraryTests'
```

On 2026-09-22, the full unit run passed 2,386 cases; the managed-library/object
regression run passed 173 cases. Subsequent focused runs passed all 25 semantic
override cases and 32 external-type cases, including additional path/link checks.
A Linux x64 differential probe also matched native C against raw and
postprocessed generated C# under both JIT and NativeAOT. It covered signed
boundaries, an unaligned byte offset, direct/function-pointer calls, pointer
identity, postincrement arguments and reads after writes. Windows execution was
not part of this local qualification.

The unsigned-target tests additionally cover width/signedness and access
qualifier rejection, all six emitted operations, unaligned reads and writes,
exact byte order and adjacent guards, zero and all-one values, direct and
function-pointer calls, separate object linking, split output, independently
side-effecting store arguments and reads after stores. On 2026-09-23 the focused
Release run passed all 47 semantic override unit cases and 12 functional cases
(including four unsigned direct/object and single/split combinations). Logs and
TRX results are retained in `blink/artifacts/attempt-ligxcg0g/`. These in-process
checks do not claim NativeAOT or postprocessed unsigned-target qualification.

Functions with special compiler lowering remain excluded: `__builtin_*`,
`__atomic_*`, `__sync_*`, `__dotcc_*`,
`setjmp`/`longjmp`, allocation/free operations, `dlsym`, variadic format functions,
and `va_*`. These are rejected explicitly because their specialized lowering
requires a separate replacement contract.

The combined intrinsic-registry and explicit nonreturning-target run on
2026-09-23 passed 55 focused unit cases and 14 functional cases (four unsigned
endian cases, two population-count cases and eight nonreturning-target cases).
The latter cover direct/object output, void/value signatures, direct and
function-pointer calls, actual target unwind and the unexpected-return guard.
The actual Release CLI build completed with zero warnings and errors.
