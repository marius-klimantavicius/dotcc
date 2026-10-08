# Owned semantic `.dotobj` compilation and linking

This is a working first slice of the [new design](../docs/dotobj-format.md).
It uses a fresh SQLite schema and C# reader/linker/emitter, without the prior
semantic-export experiment or C# text/command objects.

Each original source goes through one EDG invocation. The new dot-backend
adapter produces typed entities, bindings, expressions, statements and edges;
the packager stores those in one self-contained SQLite `.dotobj`, including
source/header snapshots and source locations. The C# linker consumes those
objects without EDG or access to the original source files.

## Build and try

Build the ordinary backend with its object adapter:

```sh
python3 -B edgcpp/backend/prepare.py
EDG_BASE="$PWD/edgcpp/upstream/bases/docker/dev-env/gcc" \
  cmake --build edgcpp/build --target cpfe-dot --parallel 4
dotnet build edgcpp/object-linker/managed/DotObj.Linker.csproj \
  -c Release --artifacts-path edgcpp/artifacts/dotobj-tool
```

Use fresh output/work paths; intermediate artifacts are retained:

```sh
python3 -B edgcpp/object-linker/compile.py \
  edgcpp/object-linker/tests/provider.cpp \
  --output /path/work/provider.dotobj --work-dir /path/work/provider-work
python3 -B edgcpp/object-linker/compile.py \
  edgcpp/object-linker/tests/consumer.cpp \
  --output /path/work/consumer.dotobj --work-dir /path/work/consumer-work
dotnet edgcpp/artifacts/dotobj-tool/bin/DotObj.Linker/release/dotobj-link.dll \
  link --out /path/work/generated --namespace Example.Library --class Core \
  --entry main /path/work/provider.dotobj /path/work/consumer.dotobj
dotnet run --project /path/work/generated/Translated.csproj -c Release
```

Omit `--entry` for a managed library. Names and method/binding references derive
from identities, with keyword escaping and collision suffixes where needed.
Namespace/class can change when relinking. The initial output splits by TU.
Generated applications need no SQLite dependency; the linker needs the platform
SQLite library. Linux x64 is the executed platform, not a claim of Windows/macOS
verification.

## Current acceptance and boundaries

`scalar-control-v7` supports defined/declaration function identities with strong
external or TU-private linkage, signed/unsigned 8/16/32/64-bit integer operations,
typed data pointers (including void pointers and pointer-to-pointer),
parameters and block locals, direct calls, conversions, initialization,
short-circuit and conditional expressions, bitwise operators and shifts,
assignments, pre/post increment/decrement, return,
if/else, while, for, do/while, switch, break and continue.
The backend preserves ordinary executable initialization order but currently
normalizes declaration placement to block entry, including for-init declarations
in an enclosing block. Break/continue carry explicit owner references; break
targets the nearest loop or switch, while continue targets the nearest loop.
Switches retain their lexical blocks and grouped case labels. The managed emitter
uses C# switch statements with `goto case`/`goto default` only for reachable C
fallthrough. Selector evaluation occurs once, and case values are converted to
the selector representation before duplicate validation and literal emission.
Integer constants retain exact decimal text, including the full unsigned
64-bit range. Arithmetic promotions and conversions come from EDG; C# emission
uses the selected widths and unsigned wraparound. Conditional expressions
evaluate only the selected branch. Source bool can have its
lowered byte representation in emitted signatures.

Pointer types use explicit pointee IDs in each object and structural signatures
across translation units. Address-taking, dereferencing, indexing, null values,
pointer/integer casts, comparisons, arithmetic and increment/decrement emit
ordinary unsafe C#. Pointer arithmetic counts elements, including pointer-to-
pointer elements; pointer difference uses the target's 64-bit signed result.
Original pointer/typedef/const chains remain in source provenance. The generated
owner checks that the runtime has 64-bit pointers when pointer types are used.

C++ right-before-left indirect assignment currently requires both the target
address expression and the assigned value to be free of calls and mutations;
other cases fail explicitly pending expression sequencing support. Address and
load/store nodes retain their distinct type and place contracts. Object lifetime
and bounds rules remain the source program's responsibility.

The profile records `sourceDeclarations=true`: read-only hooks before EDG's
incremental/type lowering snapshot original scalar types, typedef chains,
qualifiers, function signatures and parameter/local declarations. These typed
rows link to their execution counterparts. Source `long` and `long long`, and
source `bool` versus its physical byte representation, remain distinguishable.
Linking also writes `source-map.json` for inspecting this provenance without
reopening the original files. Code generation still uses the execution graph.

This is selected declaration/type provenance, not a complete source syntax tree
or all redeclarations, scopes, expressions and lifetime boundaries. The profile
still explicitly records `prelowerSource=false`, `macros=false` and
`lifetimes=false`. Effective macro definition capture is separately marked
`macroDefinitions=true`: EDG's preprocessor records installed object/function
macros, parameter names, canonical definition text, accepted undef events,
origin (source/command-line/predefined) and source locations. Linking writes
`macro-map.json` without executing or translating that metadata into C# APIs.
The wrapper accepts `-D` and `-U` and uses EDG's definition/undefinition handling.

This is effective definition provenance, not complete directive/expansion
history. Original trivia, identical discarded redefinitions, replacement-token
identity graphs and expansion/argument-use records are not retained. Dynamic
builtins are flagged with empty definition text. Active push_macro/pop_macro,
PCH/module macro imports and non-UTF8/NUL metadata are explicitly rejected.
Source/header bytes remain retained separately. This is still a partial source
and execution graph, not the complete design.

Globals/TLS, arrays/records, C++ references, function pointers, volatile/atomic
behavior, inline/template/COMDAT,
RAII/EH/noexcept, arbitrary goto and provider runtime linkage are rejected.
Nonzero folded pointer constants are also outside this profile.
The switch slice rejects nonblock bodies, nested case labels (including Duff’s
device), GNU case ranges and executable statements before the first label. These are
unfinished implementation work, not implicit fallbacks to raw code. No complete
DuckDB translation or coverage parity with the existing dot backend is claimed.

The native adapter currently buffers its transient JSON protocol before SQLite
packaging. Objects contain normalized typed rows rather than that JSON blob;
the transport still needs incremental writing for large inputs. The managed
reader loads bodies per function through indexed queries. Neither fact is a
measured DuckDB memory bound.

## Verification

```sh
python3 -B edgcpp/object-linker/tests/verify.py \
  --output-dir /path/fresh/dotobj-acceptance --nativeaot
```

The 287-check acceptance run covers actual independent EDG compilation, native
reference execution, managed JIT/NativeAOT, private name isolation, scope
shadowing, loop-local reinitialization, short-circuit effects, bool/byte
conversions, namespace collisions and implicit main return. Loop fixtures cover
C++17 and C11, nested exits, omitted for clauses, continue reaching for-step and
do-condition expressions, unbraced bodies, shadowed loop locals and byte
wraparound with prefix/postfix values. Switch fixtures cover grouped/trailing
labels, fallthrough to case/default, nested switches and loops, break versus
continue, scoped locals, empty switches and selector side effects, and signed/
unsigned 64-bit cases. Invalid duplicate labels and transfer targets are rejected.
Pointer fixtures link two independently compiled units whose pointer type IDs
differ, with native/JIT/AOT execution in C11 and C++17. They cover aliasing,
indirect stores/mutation, pointer-to-pointer arithmetic, source qualifiers,
null/casts/truth tests, 64-bit values and byte/bool storage. Malformed pointer
types, source mappings, operands and signatures are rejected.
Integer fixtures verify width boundaries,
signed extension, narrowing, unsigned arithmetic/negation, bitwise/shift behavior
and conditional branch side effects. Cross-unit tests distinguish long and
long long overloads despite their common 64-bit representation. Invalid loop
target references, constants and conditional graph shapes are rejected. It also verifies
input-order-stable source output, relinking with source files unavailable and
recompiling one changed unit while keeping the other's object unchanged.
Rejection tests cover duplicate/unresolved symbols, incompatible signatures,
unsupported ABI/node/edge forms, altered schema or snapshots, hash mismatches
and unsupported source features. Source tests cover alias chains, const and
bool capture, differing prototype/definition spelling, same-named block typedefs,
complete binding links, invalid source cycles/mappings and capture-phase checks.
Macro tests cover active/inactive directives, effective redefinition order,
empty/object/function/variadic macros, stringizing/pasting, command-line -D/-U,
source locations and malformed event/parameter records. Unsupported active
push/pop pragmas fail before object publication.
The existing normal dot-backend scalar test
also still executes successfully after the opt-in hook.

`schema.sql` is the canonical database shape; `contract.md` documents the typed
transport and digest. The C# reader verifies the supported schema, logical
digest, snapshots, references, scope/type/operation contracts and required
indexes. A hash demonstrates content integrity, not semantic correctness.

Full logs/objects/generated projects remain under
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-pointer-acceptance-001/`.

The current schema requires `scalar-control-v7`. Earlier v1/v2/v3/v4/v5/v6 objects and tools
remain retained together; regenerate objects to use the new tool. The runtime
calling ABI remains `dotobj-scalar-v1`.

The definition-text capture profile rejects actual `__VA_OPT__` replacement
operators: the stock inspection formatter is not a complete round-trip format
for trailing optional text. Quoted occurrences remain ordinary macro text.
EDG preprocessing semantics are unchanged; a future token-based macro graph can
represent this construct without relying on inspection formatting.
