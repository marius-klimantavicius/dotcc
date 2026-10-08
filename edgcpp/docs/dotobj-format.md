# `.dotobj`: owned semantic objects and a managed linker

## Decision and implementation status

The user selected one SQLite `.dotobj` per original C/C++ translation unit,
containing an owned semantic graph instead of C# text or emission commands.
The old semantic-export experiment is explicitly not the implementation
foundation. The incomplete text-command prototype is preserved under campaign
artifacts and is not a usable linker. This document is the new design; no
semantic object producer/linker implementation is claimed yet.

Build the graph contract around semantics already handled by `dot_gen_be.c`,
then implement an independent C#/.NET linker, IR passes and C# generator.
The existing dot backend stays available as a behavior/readability reference.
Old DuckDB whole-program retries remain paused.

## Representation and lowering boundary

Use one owned graph with two linked views:

- A resolved source-facing view: declarations and ownership, source types,
  selected overloads/template instances, structured expressions/statements,
  lexical/lifetime boundaries, source and macro origins.
- A normalized execution view derived from the same structural lowering and
  partial-EH representation that the current dot backend consumes: explicit
  calls, object operations, layout/adjustments, cleanup and initialization.

The execution view determines behavior. The source view supports readable output
only where an explicit correspondence to execution operations justifies it.
The relation is many-to-many; one source `new` expression may generate several
actions, and synthesized actions have an origin rather than invented source.
Do not execute both representations or serialize two unrelated full ASTs.
Mark executable versus descriptive/source-only nodes explicitly. Descriptive
operands, including retained coroutine information, must not acquire effects
merely because a generic graph walk visits them.

EDG retains responsibility for lookup, overload resolution, template selection
and instantiation, required constant evaluation, implicit conversions, special
members and ABI layout. C# does not reimplement those language decisions. The
object contains selected identities, typed operations and their contracts,
never EDG pointers or a dependency on an EDG IL checkpoint at link time.

Preserve structured if/loops/switches/blocks, typed member access and ordinary
expressions whenever valid. Do not prematurely convert all members to pointer
offsets or all loops to gotos. Normalize difficult lowered C++ constructs into
documented typed operations/intrinsics with complete operands, rather than
opaque C# text or raw EDG tables. Unknown required forms are explicit errors.

## Capture timing

The final `back_end` callback is too late to recover all original information.
`scope_stk.c:finish_function_body_processing` lowers eligible function bodies,
including delayed instantiated bodies, and then cleans up lifetime information.
`fe_wrapup.c` later lowers file scope. `lower_il.c` rewrites C++ linkage and
clears parent namespace/class relationships for promoted entities.

Capture source ownership, original types/signatures/linkage and lifetime/source
provenance before each destructive lowering, then normalize executable IL
afterward. Assign stable adapter IDs and finalize external identities at TU
completion; an early callback need not have final TU-wide identity information.
Investigate supported hooks first. Any required observational instrumentation
must leave frontend semantics unchanged. Coverage of normal/deferred/generated
bodies is a proof gate, not an assumed property of a final tree walk.

Do not treat preserved source-sequence lists as executable ordering: EDG's
`host_envir.h` documents that lowering does not update them. Source sequence
information is presentation provenance, checked against semantic relationships.

## Core node families

| Family | Required information |
| --- | --- |
| Entities/scopes | Declaration kind, declaring scope, language/lowered linkage, local identity, external key, source spelling, definition policy and origin. |
| Types | Nominal identity separately from physical representation; qualifiers, builtins, enum, pointer/reference, array, function/member pointer, records/bases and required vector/atomic forms. |
| ABI/layout | Size/alignment, fields/bitfields, base paths, source/lowered signatures, hidden parameters/results, calling conventions, virtual/member-pointer representations. |
| Expressions/places | Exact type and value category, binding/member references, constants, address/load, conversions, operators, assignment/update, conditional/short-circuit, direct/indirect/virtual calls and materialization. |
| Statements/control | Blocks/declarations/init, if/loops/switch, return/break/continue, labels/goto targets, try/handler, throw/rethrow and exceptional transfers. |
| C++ actions | Construction/destruction, allocation/deallocation, base/member adjustments, virtual dispatch and runtime operations with selected targets or normalized execution sequences. |
| Storage/data | Object duration/TLS class, layout/alignment, typed/byte initializers, symbolic address relocations, startup priorities/order, local-static guards and destructor registration. |
| Lifetime/EH | Cleanup activation/conditions, predecessor/parent boundary, array progress, catch binding/release, exception payload ownership and terminate boundaries. |
| Linking | Declarations/definitions, aliases, strong/weak/tentative/COMDAT policy, associated groups, shared address identity and runtime imports. |
| Provenance | Source files/ranges/blobs, include occurrences, spelling/expansion positions, macro definitions/undefinitions/arguments/expansions and source-to-execution mappings. |

Every renamable entity has an ID: functions, types, fields, enumerators,
parameters, locals, labels and compiler-generated helpers/temporaries. Names are
presentation properties. Parameter ordinal, declaration scope and type are
independent of spelling. Identical names in different scopes remain distinct.

Record loads versus addressable places, volatile/atomic behavior, bitfields,
signedness/width, floating representation and evaluation sequencing. Sharing
type/constant nodes does not permit sharing evaluation of side-effecting
expression occurrences. Explicit temporaries represent evaluate-once behavior.
Unknown effects remain conservative, never silently pure/nonthrowing.

Keep `MemberAccess(object, fieldId, accessMode)` where possible, with base/layout
metadata available to emission. Physical access remains available for unions,
overlapping storage and lowered ABI operations. Global/data references remain
symbolic until link storage planning, not TU-specific final offsets.

## Lifetime is not liveness

Object lifetime/destruction obligations are required semantics. Variable
liveness is a derived CFG analysis for optimization/storage reuse, not evidence
that a destructor can be omitted. Derive normal and exceptional CFG edges from
structured IR; cached analyses must be versioned and invalidated by changes.

Construction completion activates cleanup; destruction consumes it. Preserve
temporary lifetime extension, full-expression boundaries, constructor failure,
partially constructed arrays/subobjects, return-object/copy-elision destinations
and exceptional deallocation. Moving from an object does not end its lifetime.

Normalize the existing partial-EH region information into typed cleanup actions:
destructor/object, predecessor, conditional activation, array count/stride,
base/VTT arguments, allocation cleanup, static-guard reset and catch release.
Move state to the predecessor and clear activation before invoking destruction,
as the current cleanup implementation does. Destroy only successfully
constructed array elements, in the required reverse order. Preserve noexcept
and termination during throwing cleanup.

Managed `try/finally` is an emission choice when equivalent for all entry/exit
and activation paths; other cases keep explicit cleanup state. The source scope
view and normalized cleanup representation must never both execute cleanup.
Do not introduce native setjmp protocols: the current dot backend already uses
EDG partial EH and CLR unwinding.

## Pipeline

1. One EDG invocation reads the original TU with its exact options and included
   headers, performing internal preprocessing and semantic analysis/lowering.
   Record macros during preprocessing; `.ii` alone cannot recover them.
2. Write one self-contained SQLite object with all required main-file/header
   declarations, bodies, semantic metadata and source/include/macro provenance.
   Preserve public declarations even when no function in this TU uses them.
   Source snapshots are stored or explicitly marked unavailable.
3. Release the frontend. Compile libc/libc++ providers independently, without
   appending their entire implementation to every source compilation.
4. The managed linker resolves identity, definition groups, shared storage,
   RTTI/EH identities, runtime requirements and initialization. Read summaries
   first and function bodies on demand; a temporary SQLite link catalog can
   hold large indexes. Measure memory rather than assuming SQLite bounds it.
5. Managed-specific lowering handles C# sequencing/expression restrictions,
   names/scopes, unsafe access, bitfield properties and managed cleanup. Emit
   one project/assembly with optional source/size splitting. Namespace/class
   are presentation settings; owner and callable ABIs are compatibility inputs.

Linking must work without another EDG invocation or original source availability.
Maintain correspondence with original C/C++ source and existing C++ behavior. SQLite is
a build-tool dependency, not a translated-application runtime dependency.

## SQLite and identity contracts

Use normalized typed tables and ordered relations, not a single JSON blob:
`object_info`, producer/dependency records, `entities`, `scopes`, `types` and
type-specific detail tables, `functions`, `parameters`, `bindings`, `nodes`,
expression/statement detail tables, `ordered_children`, `cleanup_*`, definition
groups, data/relocations, source and macro tables. These are proposed families;
the concrete schema and reader remain to be implemented.

Compact integer IDs are object-local. FKs/checks validate structural integrity;
the verifier checks edge kinds, scope/statement ownership, types, layouts,
signature agreement, transfer targets and cleanup semantics. Recursive type
graphs are legal where recursive embedded storage is not.

External identity derives from frontend language identity and mangled/assembler
keys where applicable. Internal identity includes TU/declaration scope. Nominal
types are not merged by equal name/layout/path. C tag compatibility and C++
class/template identity need explicit rules. ODR hashes support checks; they do
not replace binding/COMDAT selection policy. Canonicalize function addresses,
inline static locals/guards and cross-unit type/exception descriptors.

Publish only a committed/closed standalone database with no required WAL or
journal. Version container/graph/ABI/runtime contracts. Use a logical digest
over ordered typed records independently of SQLite page layout, and physical
file hashes for exact artifact provenance. Read completed objects in a stable
read-only snapshot. Preserve every failed attempt and diagnostic.

Macros have definitions and expansion contexts, not necessarily one semantic
type. Distinguish raw definitions, evaluated constants, resolved expansion
instances and explicitly typed API wrappers. Never replay arbitrary macros in
the linker to invent missing semantic decisions.

## Shared headers later

Deduplicate identical source/token/constant bytes first. Sharing semantic header
objects requires target/language/frontend, effective macro, packing/pragma,
include and relevant lookup/instantiation context fingerprints. Header pathname
or content alone is insufficient. Header internal-linkage entities remain per
TU. Introduce explicit versioned context-bound imports before moving semantic
nodes out of the initially self-contained object.

## Implementation gates

1. Inventory current dot-backend node handling, source views, cleanup and runtime
   contracts; specify owned types/operations and audit capture points. The full
   design must accommodate current coverage, not stop at a scalar-only AST.
2. Implement the new writer, independent C# reader/verifier and one-TU emitter.
   Compare behavior/readability with dot_gen_be using existing native/JIT/AOT
   tests; record unsupported forms. No dependency on the old semantic exporter.
3. Link actual independently compiled provider/consumer units: shared globals,
   private isolation, forward/complete types and canonical function addresses;
   then templates/inline static state, TLS, RTTI/EH and initialization. Demonstrate
   relinking without source/compiler access and rebuilding one changed unit.
4. Move DuckDB to this path once its required features pass. Use upstream tests,
   measure frontend/linker peaks and retain fail-closed diagnostics. Export or
   preprocessing success is not full translation/execution acceptance.

Generated code remains cross-platform/NativeAOT compatible, with optional
independent owner instances, pinned global/TLS storage, native heap allocation,
readable names/structured flow and auto-generated markers.
