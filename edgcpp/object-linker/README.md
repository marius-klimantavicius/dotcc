# Owned semantic `.dotobj` compilation and linking

This implements the owned object path of the [new design](../docs/dotobj-format.md).
It uses a fresh SQLite schema and C# reader/linker/emitter, without the prior
semantic-export experiment or C# text/command objects.

The current profile is `scalar-control-v37`. It supports independently produced
SQLite and musl objects, typed provider selection, shared BCL runtime bindings,
and source type identities independent of optional execution representations.
The type graph retains `nullptr_t`, distinct character types, nominal enums and
exact enumerators, member-pointer owner/member types, and qualifiers. Semantic
type metadata can exist without a supported execution mapping; executable uses
still require a verified mapping. Full unused-declaration and EDG extension-type
coverage remains unfinished. The profile also supports explicit source startup
actions. C++ file-scope initialization owns generated
routine origins, original per-storage statement intervals and separate local
scaffolding, with scheduling and exception boundaries independent of source text.
Object destruction owns original lifetime trees, normal destructor calls and
exceptional cleanup states. Named locals remain readable; CLR unwinding runs
verified callbacks in EDG order, including ordinary base/member rollback and
temporary boundaries, without native stack-offset frames. Bounded source throws
and rethrows own original type identity, payload initialization and destructor
requirements, base adjustments and exception start/completion ordering. Their
shared `Edg.Runtime.CppExceptions` provider is included by default, with aligned
native payload storage and thread-local handler ownership. The explicit
`--exception-provider external` option retains unbound partial contracts for
custom hosts; it requires `--allow-externs`. Structured
`try` regions own their original lifetime, ordered typed/ellipsis handlers,
parameter bindings, adjusted caught-object addresses and exception-release
states. EDG-selected parameter copies and destructions remain in the handler
body. Constructor/destructor function-try blocks retain their original routine
kind, protected initialization/destruction and implicit rethrows. Delegating
constructor cleanup reuses the observed complete-object action. Fixed-array
cleanup owns element spans and helper-managed partial construction; virtual-base
cleanup retains exact hidden VTT arguments and observed physical base placements. Typed matching uses original builtin/class/data-pointer identities;
the shared matching/ownership implementation is documented in
[runtime/EXCEPTIONS.md](runtime/EXCEPTIONS.md).
Throw initializers own a saved entry cleanup cursor: evaluation failure destroys
initializer-local temporaries before releasing unfinished payload storage, then
propagates to the enclosing function's remaining cleanup. Nested throws preserve
separate boundaries. Payload-relative scalar member and nonvirtual base rollback
has explicit typed targets and complete-payload ownership paths. Successful
construction transfers destruction to the exception provider; rollback actions
skip completed payloads. The last member remains represented even when EDG
omits its unnecessary rollback entry. Independent fixture providers remain available to test the contract separately
from the shared implementation. Conditional scalar cleanup keeps
EDG's exact synthetic flag and original body stores; unwinding tests the flag
before evaluating the target, without adding reset stores. Comma expressions
retain the final item's execution storage; genuine record values get explicit
materialization for member selection, with safe copy and alignment checks.
Original class-value routine signatures retain explicit mappings to hidden
result buffers, indirect parameters and implicit receivers. Generated runtime
prototypes have observed origins distinct from user declarations. Packed records
retain observed layout and original directives; named fields keep ordinary member
syntax, with explicit unaligned operations restricted to raw accesses that lose
the packed field context. Empty GNU memory assembly owns original operand
regions and saved VLA extents; emission evaluates them once and uses a BCL
memory barrier without fabricated operand reads/writes. Fixed zero-bound arrays
retain distinct identity from unspecified arrays, zero field extent and zero
pointer stride while preserving operand side effects. Mathematical overflow
intrinsics support independent signed/unsigned operand and destination widths
through 128 bits, using allocation-free full-width arithmetic. Record construction,
compiler-temporary transfer and by-value transport use separate original C++
operation permissions: a custom assignment operator does not prohibit trivial
construction, and a trivially-copyable trait does not authorize a deleted copy
constructor. Hidden result storage and explicit user-defined operations remain
selected by EDG.
Original base declarations and generated
ctor/dtor entries retain separate provenance from exact physical subobject views.
COMDAT storage links to one canonical cell across translation units. Constructor
default adapters retain their original defaults and exact generated targets.
Template and local class identities preserve both shared inline and private
translation-unit ownership. Executable entries also support writable UTF-8
`main(int, char**)` arguments through source shutdown. The [campaign ledger](../docs/dotobj-project-campaign.md)
distinguishes project generation, compilation and execution evidence. Full C++
object layout, lifetime and exception support required by DuckDB remains in progress.
The earlier milestone descriptions below record their original bounded scope;
the [current contract](contract.md) supersedes those historical exclusions.

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

`scalar-control-v11` supports defined/declaration function identities with strong
external or TU-private linkage, signed/unsigned 8/16/32/64-bit integer operations,
IEEE binary32/binary64 (`float`/`double`) operations,
typed data pointers (including void pointers and pointer-to-pointer),
fixed local arrays of integers/floating values/pointers (including multidimensional arrays),
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

Floating constants contain exact 8/16-digit hexadecimal bit patterns in the
object. Finite C# literals use round-trip decimal spellings; negative zero is
retained, and nonfinite scalar values use `BitConverter` with their exact bits.
Arithmetic, comparisons, truth tests, numeric conversions and increment/decrement
use the selected float/double representation. Tests distinguish payload transport
from arithmetic NaN propagation, which need not retain a specific payload.
Only ordinary operations in the default floating environment are covered;
`long double`, floating environment control and `FENV_ACCESS ON` are rejected.
Floating-to-integer conversion promises apply to the source-defined input range.

Pointer types use explicit pointee IDs in each object and structural signatures
across translation units. Address-taking, dereferencing, indexing, null values,
pointer/integer casts, comparisons, arithmetic and increment/decrement emit
ordinary unsafe C#. Pointer arithmetic counts elements, including pointer-to-
pointer elements; pointer difference uses the target's 64-bit signed result.
Original pointer/typedef/const chains remain in source provenance. The generated
owner checks that the runtime has 64-bit pointers when pointer types are used.

C++ right-before-left indirect assignment accepts a provably fixed local storage
address with constant offsets, or operands both free of calls and mutations.
Other cases fail explicitly pending expression sequencing support. Address and
load/store nodes retain their distinct type and place contracts. Object lifetime
and bounds rules remain the source program's responsibility.

Fixed arrays retain their element type, extent and complete dimensions. Their
unmanaged storage uses generated value structs with explicit size and natural
leaf alignment. Explicit array decay yields an element pointer; taking the
array address yields a pointer to the whole array. Ordinary indexing is emitted
where possible. The first array slice caps each complete array at 65,536 bytes;
variable-length arrays and incomplete array objects remain unsupported.

Initializers clear storage and execute ordered element writes at the captured
initialization point. Constant numeric data uses `ReadOnlySpan<T>` properties
and `CopyTo`, trimming trailing positive zeros; sparse, pointer or dynamic data uses one
local pointer alias and indexed writes. Repeated block/loop entry resets local
initializers. Source metadata retains array bounds and parameter adjustments;
unknown-bound parameter arrays use a documented source-only adjustment marker.
Floating initializers preserve negative zero; nonfinite constants use integer
bit-pattern spans reinterpreted through `MemoryMarshal.Cast`, avoiding a runtime
array allocation. Byte-array string initialization includes embedded NULs. General string literal
storage and arrays as globals are still unfinished.

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

Globals/TLS, nontrivial record operations, C++ references, function pointers, volatile/atomic
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

The 374-check acceptance run covers actual independent EDG compilation, native
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
Array fixtures add partial zero-fill, dynamic and nested initializer order,
loop resets, pointer-to-array stride, inferred bounds, qualifiers, array parameter
adjustments, byte strings, and 65,536-byte storage. Generated type names avoid
source method and configured owner-class collisions. Malformed array bounds,
indices, layouts, decay and signatures are rejected.
Floating fixtures verify C11/C++17 cross-unit calls, exact finite/signed-zero/
subnormal/nonfinite constants, embedded array data, runtime truncating numeric
conversions, precision boundaries, mutation, NaN truth/comparisons and payload
transport. Malformed constant encodings, invalid operators and incompatible
floating signatures are rejected.
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
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-float-acceptance-002/`.

The current schema requires `scalar-control-v37`. Earlier-profile objects and tools
remain retained together; regenerate objects to use the new tool. The runtime
calling ABI remains `dotobj-scalar-v1`.

The definition-text capture profile rejects actual `__VA_OPT__` replacement
operators: the stock inspection formatter is not a complete round-trip format
for trailing optional text. Quoted occurrences remain ordinary macro text.
EDG preprocessing semantics are unchanged; a future token-based macro graph can
represent this construct without relying on inspection formatting.


The v10 profile adds GNU local cleanup callbacks as explicit `register-cleanup`
statements, retaining the selected callback and local binding IDs. Managed
emission uses nested `try/finally` with direct calls, preserving reverse order,
return-value evaluation, loop exits and successful-initialization boundaries.
No cleanup runtime stack, heap allocation or pinning is added. Callback-only
references resolve across objects; `void*` parameters and discarded nonvoid
results are supported, and fixed arrays pass their whole-object address.
`lifetime-map.json` records activation nodes and protected block suffixes.

Reports distinguish `lexicalCleanups=true` from still-false general `lifetimes`.
C++ destructors, temporary lifetime extension, partial construction and source
exception handling remain unfinished. Ambiguous split array initialization,
uninitialized cleanup in multi-declarator statements and registrations directly
in switch bodies fail explicitly; scoped case blocks work. See the
[v10 contract](contract.md) for precise activation and exclusion rules.


The v11 profile adds complete named structs in global/named-namespace scope.
Record and field identities, exact natural layouts and original field types are
owned SQLite rows. Nested records, scalar/pointer array fields, recursive data
pointers, member addressing and zero/aggregate initialization are supported.
Record locals also work with GNU cleanup callbacks. C# uses explicit-layout
structs and ordinary named `value.member` / `pointer->member` expressions.

Matching records merge across independent objects, including C/C++ objects from
a common header. Conflicting layouts or field types fail linking. Original
function names remain unchanged when a source type shares their spelling; only
the colliding generated type is renamed. The source map includes record/field
metadata. No heap allocation or pinning is introduced for these locals.

This increment excludes record copies/by-value calls, arrays of records,
incomplete/empty/local/anonymous/nested-class/template records, unions,
bitfields, packing, over-alignment, inheritance and user/nontrivial member
functions. These are explicit errors. Constructors/destructors and full C++
lifetimes remain the next work; the existing lexical cleanup capability is
unchanged. Focused acceptance is available with `tests/verify.py --records-only`.

## Source startup actions

Library hosts call `Edg.Runtime.ObjectProgram.InitializeLibrary()` before
publishing translated APIs. Executables initialize automatically through
`ObjectProgram.Run`. Physical global images and relocations are ready before
source constructors execute. Constructors run outside CLR type initialization
and outside the initialization lock; same-thread initialization reentry returns,
other initializing hosts wait, and failures are cached without rerunning actions.
Ordinary exports do not implicitly initialize the library.

GNU constructor priorities include zero. Equal-priority actions use EDG routine
order within each object and original linker input order across objects. Two
registrations of a coalesced inline body remain two invocations. Extracting a
provider object retains its startup actions and their dependencies; an unused
provider does not run constructors. Shutdown attributes and C++ dynamic-global
initialization remain explicit errors pending their distinct lifetime policies.

`tests/verify.py --startup-only` tests registration, ordering and provider closure;
`tests/startup_verify.py` tests concurrent initialization, reentry, constructor
worker calls and cached failure using JIT and optional NativeAOT execution.
