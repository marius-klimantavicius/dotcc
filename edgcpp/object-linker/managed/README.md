# Managed semantic object linker

This is the executable `scalar-control-v5` implementation of the fresh
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
C# operators directly, including unchecked unsigned wraparound. Loop exits
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

The link report records `sourceDeclarations=true` for pre-lowering scalar type
and declaration provenance. `source-map.json` records original builtin type
kinds (including bool, long and long long), typedef/qualification chains,
function signatures, parameter/local identities and source positions, connected
to execution identities. These facts are checked against execution signatures
and bindings; they do not replace the authoritative executable graph. Original
parameter spelling can differ from the canonical routine signature when a
prototype used a different typedef or top-level qualifier. The reader rejects
missing declaration capture, type-reference cycles and inconsistent mappings.
Source declarations stream through validation and map output; local provenance
does not remain resident for the whole program.

The separate `macro-map.json` records `macroDefinitions=true`: canonical macro
text decoded by EDG's preprocessor, definition identities, function parameter
names, effective define/undef events, source locations and command-line versus
predefined origins. Dynamic builtins have an empty definition text and an
explicit dynamic flag. The reader verifies chronological installation identity,
matching event/definition origins and names, parameter ownership and ordinals.
Undefined-name `#undef` is allowed. Definitions and events stream through
validation and map output without retaining replacement text for the program.
This is inspection metadata, never executable code. It does not provide a
replacement-token IR, expansion/argument history, or automatic C# macro APIs.
Discarded identical redefinitions and inactive directives do not appear;
comments and whitespace in definition text can be canonicalized by EDG.
Push/pop and precompiled/module macro import are rejected for this profile.

The link report still preserves the missing capability flags: `prelowerSource`,
`macros`, and `lifetimes` are false. A complete source expression/statement view,
all redeclarations, macro expansion history and lifetime capture remain absent. Globals/TLS, pointers, records, function
addresses, exceptions/RAII, inline/COMDAT, templates and runtime providers are
not implemented in this profile. Unsupported input is rejected; scalar fixtures
do not demonstrate DuckDB readiness or full C++ coverage.

The current schema requires scalar-control-v5 objects. Earlier v1/v2/v3/v4 objects
remain usable with their retained earlier tool build, or can be regenerated.

The integer profile supports signed/unsigned 8/16/32/64-bit values, exact decimal
constants (including ulong maximum and long minimum), typed conversions and
calls, bitwise operators, shifts and value conditionals. EDG performs integer
promotions; the graph records them explicitly. Arithmetic emission uses
unchecked expressions where constant or runtime overflow would otherwise
conflict with source unsigned wraparound. Unsigned negation emits typed zero
minus the operand. Shifts convert non-int counts explicitly to int. Conditional
expressions and short-circuit operators evaluate only their selected operands.
This does not add floating point, pointers, compound assignment or comma nodes.

The definition-text capture profile rejects actual `__VA_OPT__` replacement
operators: the stock inspection formatter is not a complete round-trip format
for trailing optional text. Quoted occurrences remain ordinary macro text.
EDG preprocessing semantics are unchanged; a future token-based macro graph can
represent this construct without relying on inspection formatting.
