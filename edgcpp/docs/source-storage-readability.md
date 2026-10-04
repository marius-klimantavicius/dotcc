# Named storage and readable initialization

Ordinary aggregate and array locals use named C# storage when their managed
layout supplies the native alignment. Constructor calls and cleanup helpers
take the address of those locals. EDG's lowered initialization and destruction
order remain unchanged. Generated record and inline-array packing retains
native size/alignment, including smaller lowered base-prefix views.

Explicitly over-aligned objects and layouts whose native alignment cannot be
established retain a frame. Large aggregate footprints retain the existing
native heap-frame allocation policy, avoiding an unbounded increase in managed
stack usage. Framed source locals have named ref views; address-taking uses
those names where the C# type permits `Unsafe.AsPointer<T>`. Pointer-typed refs
retain a raw address because pointer types cannot be generic type arguments.
Coroutine storage is unchanged.

## Empty noexcept bodies

The emitter checks the complete generated function body before adding the
`noexcept` catch/termination boundary. Bodies containing only whitespace, empty
statements/blocks and bare `return;` need no boundary. Calls, return expressions,
storage allocation and synthesized constructor/destructor work still retain it.
This includes source-empty destructors whose lowered body destroys a member.
The check does not remove cleanup or infer behavior from source braces alone.
`backend/tests/noexcept_empty_verify.py` covers both omitted and retained guards.

String and one-byte integer-array initializers use bulk copies:

```csharp
global::System.ReadOnlySpan<byte> __edg_initializer123 = "message\0"u8;
__edg_initializer123.CopyTo(new global::System.Span<byte>(destination,
    __edg_initializer123.Length));
```

Invalid UTF-8 and wide-character byte representations use numeric constant
spans instead. No replacement decoding or newline normalization occurs. Default
initialization preserves zero-filled tails; embedded NULs, explicit terminators,
signed byte patterns and independently mutable destinations retain their exact
bytes. Individual chunks are bounded to 16 KiB and UTF-8 chunks never split a
scalar. Constant spans avoid allocating a managed byte array for every copy.

## Other primitive tables

Arrays of signed/unsigned 16-, 32- and 64-bit integers now use static span
properties and bulk copies, including nested array rows and array fields:

```csharp
private static global::System.ReadOnlySpan<uint> __edg_initializer123 =>
    [1116352408U, 1899447441U, 3049323471U, 3921009573U];

// In the destination's initializer:
__edg_initializer123.CopyTo(new global::System.Span<uint>(destination,
    __edg_initializer123.Length));
```

The properties contain primitive constant expressions that compile to assembly
data. A property returning an arbitrary collection would not alone guarantee
that behavior. The verification tool inspects the generated assembly's getter
IL, checks that each property reaches a field with an RVA, verifies the exact
uint table bytes, and rejects heap allocation or unaudited calls.

Enum arrays use their underlying primitive data with a typed span view.
`Half`, `float` and `double` arrays use ushort/uint/ulong bit-pattern properties
and `MemoryMarshal.Cast` views, preserving negative zero, infinities,
subnormals and NaN payloads without decimal conversion. No per-element
`BitConverter` calls or temporary heap arrays are needed for these copies.

Data is split into properties of at most 4,096 elements. Identical property
data is shared, while each global/local/TLS/instance destination remains an
independently mutable copy. Omitted elements remain zero; long explicit zero
runs use the destination's existing default initialization instead of embedding
large zero tables. Tests cross chunk boundaries and zero ranges followed by
nonzero suffixes.

Pointer relocations remain runtime expressions; a struct table containing string
pointers cannot be embedded as final absolute pointer values. Primitive array
fields inside records can still use bulk copies. The typed-table path covers
the widths above, not 128-bit integers or extended 80-/128-bit floating types.

`backend/tests/primitive_data_readability_verify.py` compares native traces with
static/instance JIT and NativeAOT execution, including exact scalar bits,
multidimensional arrays, independent copies, TLS, relocation fallbacks and
compiled-data inspection.

## Record tables

Ordinary record tables use a source-named pointer alias and one value assignment
per row, for example:

```csharp
var builtin_table = (a_builtin_descr*)(__edg_base + 4550168);
builtin_table[15643] = new()
{
    name = __edg_literal(7343504 /* "#1_tum@43" */),
    cond_index = 169,
    type_index = 15029,
    kind = 1852,
};
```

The emitter retains a trailing comma after the last field. Plain nested records
can use nested object initializers. Omitted fields remain zero, and representable
narrow integer constants no longer need explicit casts in these assignments.
The `new()` creates a value-type struct, not a managed heap object. This is a
source-size/readability improvement; identical IL or native machine code is not
promised.

Union, bitfield, array-member and overlapping/lowered-base layouts retain the
existing initialization path. Global and TLS destinations retain their backing
arenas and isolation. Address constants refer to the final storage, including
self references, rather than an object-initializer temporary.

Literal-pool addresses retain pointer identity and offsets, with an added
navigation comment for valid UTF-8, for example:

```csharp
__edg_literal(128 /* "message" */)
```

The aggressively inlined helper returns a pointer into the same pinned literal
pool. Zero additional offsets are omitted; nonzero offsets retain byte units,
including when the resulting pointer addresses wide characters.

Previews stop after 80 Unicode scalars, omit one terminal NUL, escape embedded
control characters and cannot terminate their surrounding comment. Binary
literals receive no misleading textual preview. Comments do not affect bytes
or runtime evaluation.

Focused tests include `record_initializer_readability_verify.py`,
`named_local_storage_verify.py` and `byte_data_readability_verify.py` in
`backend/tests/`. They compare behavior
with native fixtures and cover JIT/NativeAOT, static/instance modes, alignment,
exception cleanup, large frames, byte preservation, mutable copies and TLS.
