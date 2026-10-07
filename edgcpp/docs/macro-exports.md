# Opt-in public macro APIs

`dotnet.py --emit managedlib --export-macros REGEX` exports selected macros into
the generated owner class, using the original macro names. Selection uses Python
regular-expression **full matches**; repeat the option to combine selectors.
Nothing is exported implicitly.

```sh
python3 edgcpp/dotnet.py path/to/sqlite3.c --language c --emit managedlib \
  --libc dotcc --export-macros 'SQLITE_(OK|ERROR|BUSY|ROW|DONE|OPEN_READONLY)' \
  --output-dir path/to/generated
```

This command illustrates selection, not a claim that this minimal SQLite build
configuration provides all required platform bindings. Use the project's normal
translation options and platform/runtime configuration.

EDG's `--list_macros` supplies the final active macro definitions after all
includes, conditionals, definitions, and undefinitions in one input translation
unit. With multiple inputs, `--macro-source path/to/input.c` is required. It must
name an input translation unit, not a separately interpreted header. This makes
configuration and the snapshot boundary explicit. A dedicated API translation
unit can include the library's public headers. A macro removed before the end of
the chosen translation unit is not exported.

The driver retains a generated native adapter TU that includes the original TU
and appends typed wrapper functions. EDG parses, resolves, expands, types, and
lowers those wrappers together with the library; Python does not interpret C
expressions or guess integer widths. C object macros use GNU `__typeof__` on a
comma expression; C++ uses an unevaluated function template call to deduce the
by-value result type and decay arrays. The same language/target/includes/defines
are used for discovery and translation.

The public managed result is:

- An integral or Boolean **`const` field** when EDG emits a single primitive
  literal return. This includes unsigned 64-bit values and folded integer
  expressions. These fields work in C# `case` labels and constant contexts.
- A typed **read-only property** for other object-like expressions, including
  string pointers, floating values, enums, casts not matched by the deliberately
  narrow literal recognition, and expressions reading current program state.
  A property evaluates the macro expression on each access. It is not a
  writable macro lvalue and does not become a compile-time C# constant.
- A typed **method** for a function-like macro with an explicit type contract.

For example, `#define API_INT (1 << 12)` produces
`public const int API_INT = 4096;`. A string macro produces an `sbyte*` property
pointing to the normal retained literal storage, not a managed string conversion.
For constants, packaging removes the unused private probe body and its lifecycle
boundary wrapper, along with their source-navigation entries. If another emitted
member references a probe, it is conservatively retained. Nonconstant properties
and methods keep their private adapters. The raw backend output and native probe
sources remain available for inspection; `adapterEliminated` in the macro
manifest records constant-probe removal.
The existing namespace, owner class, instance-globals, and file-splitting options
apply. Constants remain C# static constants even in an instance owner; properties
and methods belong to the instance when `--instance-methods` is selected.

## Function-like macro contracts

C macros have no standalone argument or result type. Use a JSON contract with
`--macro-signatures path/to/signatures.json`; every selected function-like macro
must have a contract. For example:

```json
{
  "API_AT": {
    "returnType": "int",
    "parameters": [
      {"type": "const int *", "name": "pointer"},
      {"type": "int", "name": "index"}
    ]
  }
}
```

Types are **native** type names in scope at the selected TU's end, including
library typedefs. Use typedefs for array/function-pointer declarators. A `void`
return type invokes the macro as a statement; other return types return its
result. EDG diagnoses incompatible expansions, invalid type names, conversions,
and arity. Aggregate and pointer results use the existing C# ABI adapters.
Object-like macros may also receive an explicit return type with no parameters.

These methods have a deliberate function-call boundary: C# evaluates each
argument once, before entering the adapter. Thus a method wrapping `TWICE(x)`
with replacement `((x)+(x))` receives one value, rather than re-evaluating the
caller's expression twice. Macros that depend on caller lvalues, declarations,
`return`/`break`/`goto` in the caller, token spelling, arbitrary types, or lazily
unevaluated arguments cannot be exposed with their full textual macro behavior
as ordinary C# methods. Choose concrete, meaningful typed APIs for QuickJS-style
macros; this feature does not infer a generally equivalent signature or translate
an arbitrary macro into a syntax transformer. Contracts carry the same pointer,
reference, and owner lifetime obligations as the underlying native API.

## Rejections and provenance

Empty/configuration macros, unmatched selectors, missing function contracts,
unused contracts, and managed name collisions are errors. Selected macros using
context-dependent builtins such as `__LINE__`, `__FILE__`, or `__COUNTER__`,
including dependencies in other active macros, are rejected conservatively.

An include-wrapper can change `__BASE_FILE__` or `__INCLUDE_LEVEL__` in the
original program. Before translation, EDG preprocesses the original TU and an
include-only wrapper. Their nonblank output lines must be equal after trimming
outer whitespace and removing line directives. A difference rejects the export
and retains both outputs for inspection. This check does not claim token-level
preprocessor equivalence for all possible extensions; in particular it can
conservatively reject harmless output differences. Keep inputs unchanged during
discovery and compilation, as for other multi-step compiler workflows.

`dot-gen.json` records the selector, selected final definitions, native
signatures, exact wrapper hash/path, and each emitted managed API kind/type.
Generated wrapper sources, macro dump, preprocessing comparison, diagnostics,
and input metadata are retained in `.macro-inputs/<content-hash>/` even if a
later stage fails. Source navigation records identify macro APIs as end-of-TU
snapshots with line zero, rather than inventing original definition locations.
Normal source functions retain their original include-file locations.

This first stage requires `--emit managedlib` and native translation. It rejects
`--raw-input` replay because retained C# alone cannot verify macro provenance.

## Verification

`backend/tests/macro_exports_verify.py` checks C/static and C++/instance outputs,
source splitting and navigation, actual constants in `const`/`switch` contexts
without leftover probe methods or navigation entries,
unsigned limits, negative values, redefine/undef behavior, UTF-8/embedded-NUL
string pointers, current-state properties, typed pointer/aggregate/function
macros, lifecycle boundaries with exception catches, explicit argument-once
behavior, and targeted rejection diagnostics.
`--nativeaot` adds execution of both managed consumers as NativeAOT binaries.
