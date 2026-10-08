# Managed semantic object linker

This is the executable `scalar-control-v2` implementation of the fresh
`.dotobj` semantic graph. It does not read the previous semantic-export format
or an emission-command stream.

Build with .NET 10:

```sh
dotnet build edgcpp/object-linker/managed/DotObj.Linker.csproj \
  --artifacts-path edgcpp/artifacts/dotobj-linker
```

The resulting `dotobj-link.dll` supports:

```sh
dotnet dotobj-link.dll verify provider.dotobj consumer.dotobj
dotnet dotobj-link.dll link --out generated \
  --namespace Example.Library --class Library --entry main \
  provider.dotobj consumer.dotobj
dotnet build generated/Translated.csproj
```

Omit `--entry` to emit a library. An executable entry must identify one external
zero-argument function returning `int` or `void`. Namespace and class are
presentation choices, not symbol identities. The output directory must be empty.
The linker does not invoke EDG, read original source files, or compile generated
C# itself. All compilation-unit inputs come from the object databases.

The linker requires the platform SQLite 3 library: `sqlite3.dll` on Windows,
`libsqlite3.so.0`/`libsqlite3.so` on Linux, or `libsqlite3.dylib` on macOS.
Selection uses runtime platform detection. SQLite is not referenced by generated
code. Each object is opened read-only inside a snapshot transaction. The reader
checks the application/format identifiers, integrity, foreign keys, logical
digest, source snapshot hashes, profile and ABI contracts. No extension loading
or arbitrary embedded query execution is used.
Version 1 requires the canonical table and index definitions embedded from
`schema.sql`; unknown columns/constraints/indexes are rejected even if a caller
recomputes the content digest. Primary-source metadata must match an available
root snapshot, and repeated include occurrences must agree on source bytes.

Entity/type/function summaries are read first. Definition signatures must agree
on exact external linker keys; duplicate strong definitions are rejected.
Internal identities include translation-unit identity and object-local entity
ID. Calls require a definition. Unused declaration-only API entities are retained
in the object but do not require definitions merely to link another function.
Bodies load one function at a time. The verifier checks typed payloads, operand
roles/counts, executable occurrence uniqueness, lexical binding ownership,
parameter agreement and supported evaluation order before emission.

The emitter preserves function/parameter/local spellings where C# permits them,
uses entity identities for every reference, and deterministically disambiguates
collisions. Reserved keywords use C# `@` escaping; ordinary names retain their
spelling without an escape prefix. Original files each get a generated
C# file; all files share one configurable partial class. Structured blocks,
`if`, `while`, `for`, `do`/`while`, `break`, `continue`, returns and ordinary
scalar expressions remain structured. Integer pre/post increment/decrement use
C# operators directly, including unchecked unsigned-byte wraparound. Loop exits
carry explicit target identities in the object and must address the nearest
lexically enclosing loop; malformed or cross-loop targets are rejected.
For-header declarations are currently normalized to declarations in an enclosing
block; the initializer expression, condition and step retain their execution
positions. Missing for conditions remain infinite-loop syntax.
Locals without explicit initializers receive `default` for C# definite assignment;
this does not model defined behavior for source reads of indeterminate values.
Non-void fallthrough receives a failure guard when needed for C# compilation;
the producer must explicitly normalize C/C++ `main`'s implicit return zero.

Generated code has no reflection, dynamic-code generation, SQLite dependency or
native runtime import. It is suitable for JIT and NativeAOT compilation for this
limited scalar profile. This does not establish cross-platform execution tests.

The link report preserves the missing capability flags: no pre-lowering source
view, macro capture or lifetime capture. Globals/TLS, pointers, records, function
addresses, exceptions/RAII, inline/COMDAT, templates and runtime providers are
not implemented in this profile. Unsupported input is rejected; scalar fixtures
do not demonstrate DuckDB readiness or full C++ coverage.

The current schema requires scalar-control-v2 objects. Earlier v1 objects remain
usable with their retained earlier tool build, or can be regenerated from source.
