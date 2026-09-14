# Literal storage

By default, translated literals use `Libc.L(...)` as before. Pass `--literal-pool`
to opt into rooted storage, short names (`S0`, `S1`, …), and usage-site previews:

```sh
dotcc main.c --literal-pool -o generated
dotcc a.cs b.cs --literal-pool -o linked
```

For object compilation, select the flag at link time. The same object files can
produce either storage mode. The library API option is
`new CSharpOutputOptions(LiteralPool: true)`. The SQLite, PicoTLS, and MsQuic product
generation scripts enable it.

With the flag, generated narrow string pointers address a single pinned GC byte array owned by
a translation-specific holder (for example, `SqliteLiterals`). Its static
constructor allocates the array, copies the translated strings, then publishes
the pointer. Libc owns a separate pool for its fixed strings; neither pool depends
on the other, allowing runtime code to be shared independently.
The array stays rooted in a static field. No pointer escapes from the managed
initializer span. Default emission retains the legacy `Libc.L` lifetime requirements.

The pointer remains valid while the owning library is kept alive. Callers must
stop using its pointers before permitting assembly unloading. Pinning prevents
movement, not collection; the pool is reclaimed when its owning assembly and
other managed references are gone. This change does not make unrelated pointers
to managed static fields safe in collectible assembly contexts.

The compiler decodes C strings into exact bytes, including embedded NULs and the
implicit final NUL. Identical strings share an offset. Valid UTF-8 sequences are
combined into a UTF-8 initializer; arbitrary byte sequences use a binary section
of the same array. Offsets are C# constants, so
using a literal requires only a pointer load and addition.

Usage sites include a quoted comment preview of the literal. The implicit final
NUL is omitted; embedded NULs, line breaks, quotes, backslashes, and invalid UTF-8
bytes are escaped. Slashes next to stars are escaped so comment delimiters cannot
terminate or nest the preview. Previews longer than 160 escaped characters end
with an ellipsis outside the quotes, keeping long strings from producing giant
usage-site lines.

UTF-8 initializers wrap at approximately 500 source characters with `u8 + u8`.
These additions fold at compile time into one blob. Splits never divide an escape
or a Unicode scalar, and no newlines are inserted into the data. Binary
initializers and object-record payloads are also wrapped.

Objects retain content-addressed literal records. The linker coalesces records
and assigns deterministic short names and offsets before generating the final pool.
Full hashes appear only in intermediate object records. The holder
and initialization stay in the main generated file when functions are split.
The object format is version 2 with `literal-pool:2` capability; objects without
that capability marker must be regenerated.

With the flag, declared constant arrays use `GlobalArrayFrom<T>` (or the existing aligned
variant), which copies into rooted pinned storage. Separate declarations retain
separate allocations and their element representation. Without it, constant
primitive arrays retain the legacy `Libc.L`/`Libc.L<T>` path. UTF-16/UTF-32 strings
continue to use the existing rooted `L16`/`L32` pools.

Validation covers exact UTF-8 and binary bytes, long wrapped literals, global
initialization, array identity, object linking, split/nested output, concurrent
first access, compacting GC, and collection of both the pool and its assembly.
