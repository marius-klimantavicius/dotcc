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

`scalar-control-v3` supports defined/declaration function identities with strong
external or TU-private linkage, signed/unsigned 8/16/32/64-bit integer operations,
parameters and block locals, direct calls, conversions, initialization,
short-circuit and conditional expressions, bitwise operators and shifts,
assignments, pre/post increment/decrement, return,
if/else, while, for, do/while, break and continue.
The backend preserves ordinary executable initialization order but currently
normalizes declaration placement to block entry, including for-init declarations
in an enclosing block. Loop exits carry explicit references to their owning loop;
the linker validates these before emitting ordinary C# break/continue statements.
Integer constants retain exact decimal text, including the full unsigned
64-bit range. Arithmetic promotions and conversions come from EDG; C# emission
uses the selected widths and unsigned wraparound. Conditional expressions
evaluate only the selected branch. Source bool can have its
lowered byte representation in emitted signatures.

The profile explicitly records `prelowerSource=false`, `macros=false` and
`lifetimes=false`. Source/header bytes are retained, but macro definitions and
expansion history are **not** semantically captured yet. This is an execution
graph slice, not the complete two-view source/execution graph in the design.

Globals/TLS, pointers/records, volatile/atomic behavior, inline/template/COMDAT,
RAII/EH/noexcept, switch/goto and provider runtime linkage are rejected. These are
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

The 124-check acceptance run covers actual independent EDG compilation, native
reference execution, managed JIT/NativeAOT, private name isolation, scope
shadowing, loop-local reinitialization, short-circuit effects, bool/byte
conversions, namespace collisions and implicit main return. Loop fixtures cover
C++17 and C11, nested exits, omitted for clauses, continue reaching for-step and
do-condition expressions, unbraced bodies, shadowed loop locals and byte
wraparound with prefix/postfix values. Integer fixtures verify width boundaries,
signed extension, narrowing, unsigned arithmetic/negation, bitwise/shift behavior
and conditional branch side effects. Cross-unit tests distinguish long and
long long overloads despite their common 64-bit representation. Invalid loop
target references, constants and conditional graph shapes are rejected. It also verifies
input-order-stable source output, relinking with source files unavailable and
recompiling one changed unit while keeping the other's object unchanged.
Rejection tests cover duplicate/unresolved symbols, incompatible signatures,
unsupported ABI/node/edge forms, altered schema or snapshots, hash mismatches
and unsupported source features. The existing normal dot-backend scalar test
also still executes successfully after the opt-in hook.

`schema.sql` is the canonical database shape; `contract.md` documents the typed
transport and digest. The C# reader verifies the supported schema, logical
digest, snapshots, references, scope/type/operation contracts and required
indexes. A hash demonstrates content integrity, not semantic correctness.

Full logs/objects/generated projects remain under
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-integer-acceptance-002/`.

The current schema requires `scalar-control-v3`. Earlier v1/v2 objects and tools
remain retained together; regenerate objects to use the new tool. The runtime
calling ABI remains `dotobj-scalar-v1`.
