# Named locals and readable byte initialization

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

Literal-pool addresses retain pointer identity and offsets, with an added
navigation comment for valid UTF-8, for example:

```csharp
(__edg_literal_base + 128 /* "message" */)
```

Previews stop after 80 Unicode scalars, omit one terminal NUL, escape embedded
control characters and cannot terminate their surrounding comment. Binary
literals receive no misleading textual preview. Comments do not affect bytes
or runtime evaluation.

Focused tests are `named_local_storage_verify.py` and
`byte_data_readability_verify.py` in `backend/tests/`. They compare behavior
with native fixtures and cover JIT/NativeAOT, static/instance modes, alignment,
exception cleanup, large frames, byte preservation, mutable copies and TLS.
