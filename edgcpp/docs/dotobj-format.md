# `.dotobj`: owned semantic objects and a managed linker

## Decision and implementation status

The current user priority (2026-10-09) is complete DuckDB C# generation by filling
missing core `.dotobj` semantics. Generated-project compilation/execution is a
later checkpoint; libc++ implementation and runtime binding work is deferred.
Required initialization, lifetimes, exceptions and bodies must still be represented
correctly rather than omitted. The [campaign ledger](dotobj-project-campaign.md)
records the current profile and distinguishes generation from runtime acceptance.

The user selected one SQLite `.dotobj` per original C/C++ translation unit,
containing an owned semantic graph instead of C# text or emission commands.
The old semantic-export experiment is explicitly not the implementation
foundation. The incomplete text-command prototype is preserved under campaign
artifacts and is not a usable linker. A fresh scalar/control-flow execution
slice is now implemented in [object-linker](../object-linker/README.md), with
real per-TU compilation/linking and native/JIT/AOT checks. The v2 scalar profile
adds structured for/do loops, integer mutations and explicitly targeted loop
exits. The v3 profile extends integer widths to signed/unsigned 8/16/32/64 bits
and adds conditional expressions, bitwise operators and shifts. The v4 profile
adds original declaration/type snapshots at pre-lowering hooks and a source map,
with `sourceDeclarations=true`. V5 adds effective macro definition/undefinition provenance and a macro map.
V6 adds structured switches, case labels and explicit switch/loop transfer targets,
including C fallthrough represented by local C# `goto case`/`goto default` transfers.
V7 adds structural data-pointer types and typed address/dereference places,
with pointer arithmetic, cross-unit pointer signatures and ordinary unsafe C# output.
V8 adds fixed local arrays, multidimensional layout, array decay and ordered
initialization, with typed span copies for constant integer data.
V9 adds binary32/binary64 types and arithmetic, exact floating constant bits,
and static floating initializer data.
V10 adds explicit GNU lexical cleanup activation and direct managed try/finally
execution, with a derived lifetime map. General C++ lifetime/RAII capture remains
unfinished; the narrower capability is `lexicalCleanups=true`.
V11 adds owned nominal struct/field identities, natural layouts, named member
access and cross-object definition checks, including mixed C/C++ objects.
Record constructors/destructors and nontrivial object actions remain pending.
V12/v13 extend the C execution graph through global/TLS storage, relocation,
function pointers, varargs, unions/bitfields, complete record values, observable
volatile access, binary80 and typed provider selection. V14 adds explicit GNU
startup actions and library initialization. The complete prepared thread-safe
SQLite engine now runs through independent objects under JIT and NativeAOT;
the [campaign ledger](dotobj-project-campaign.md) links exact acceptance evidence.
Full source-tree/macro-expansion
and lifetime capture, broad C++ coverage and the full graph design below remain
unfinished; the initial profile explicitly records those missing capabilities.

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

## Throw initialization cleanup (profile 31)

Every `throw_expressions` row has a `throw_initialization_boundaries` row with
policy `unwind-to-entry-before-release`. This denotes a dynamic snapshot of the
function cleanup cursor immediately before payload allocation. It is not a
guessed lexical scope or a serialized native stack address. Each cleanup marker
inside an initializer also has a `throw_cleanup_transitions` row naming its exact
innermost throw, including unreachable markers. The verifier checks these owners
against executable graph nesting, function identity, and ordinary cleanup-state
and target metadata. Missing boundaries, missing transition ownership and foreign
owners are rejected after normal database/schema/digest validation.

The emitter uses the same typed cleanup actions for function exit and failed
initialization. On an accepted initialization failure it advances through actions
until reaching the saved cursor, releases the incomplete payload, and rethrows.
An initializer failure must not destroy outer locals at this boundary or invoke
the complete payload destructor. Nested initializers save independent cursors.
First-pass exception-start/termination classification remains the shared provider's
responsibility. Payload propagation itself bypasses the initialization-failure
catch. Profile 31 covers initializer-local scalar temporaries; profile 32 adds
the payload subobject ownership described below. Profile 33 adds catch-all
regions; typed handlers remain a gate.

## Payload subobject ownership (profile 32)

`throw_payload_subobjects` relates an original destruction obligation to the
throw whose complete payload destructor eventually owns it. `throw_payload_paths`
stores an ordered typed member/nonvirtual-base path from that payload; it uses
record-field and original-base IDs, not byte-offset text. The observer records
the initialization destination before stock aggregate lowering can omit the
final member's rollback entry. That optimization is valid because nothing can
throw between completing the final member and completing the aggregate; it does
not erase the member's eventual destruction obligation.

Rollback actions retain their ordinary cleanup-state ordering. Their new
`cleanup-payload-address` node has a `cleanup_payload_targets` row naming the
exact action and throw. The verifier checks the payload pointer type, function,
original obligation, terminal object type and matching member/base path. Arming
sites must be within the owning throw initializer. A payload address recipe
cannot appear as an ordinary executable expression. Missing target/ownership
proofs, foreign owners, empty paths and mismatched same-type member paths fail
verification, even with a valid database and recomputed digest.

The emitter retains a named native payload pointer and completion flag where
rollback needs them. Accepted initialization failures run the armed subobject
actions before releasing unfinished storage; a successful initialization marks
the payload complete. Subsequent function unwinding advances past its partial
actions without invoking them. The provider later invokes the selected complete
payload destructor exactly once. Other locals still unwind normally. No native
stack frame or EDG IL mutation is introduced.

The focused `cpp_throw_payload_verify.py` suite compares generated JIT/NativeAOT
traces with native Clang and retains independent GCC and stock EDG C traces.
Those references differ on some aggregate cases; the receipts preserve the
differences instead of claiming universal parity. The provider is fixture-only.
Array-sequence, virtual-base/VTT and delegated payload rollback, production EH
ownership/dispatch and typed source handlers remain separate work.

## Source catch-all regions (profile 33)

The retained `try` and `catch` statement nodes keep source structure. Each
`source_tries` row identifies its original `try-block` lifetime, and a
`source_handlers` row owns the exact ellipsis handler. The initial profile admits
one `catch (...)` per ordinary try. Typed matching, catch-parameter initialization
and constructor/destructor function-try blocks remain explicit gates.

A cleanup state now selects either a destructor action or a handler release.
`handler_node_id` denotes the exact runtime exception-release region observed
while EDG lowers that handler; no runtime function name or stack offset is used
to infer ownership. The original lifetime checker admits synthetic exception
release entries only when the observer recorded that entry in the same function.
The verifier requires one release state per handler and permits arming it only
inside that handler's executable region. Ordinary local destructor states can
chain through this release state to an enclosing handler or local object.

`handler_markers` preserves EDG's caught, catch-exit and try-exit operations.
The last distinction matters on a return: lowering can place outer-scope
destructions inside the retained try statement, after the source try has ended.
The emitted catch filter tests an active flag cleared by the try-exit marker,
so exceptions from those destructions bypass the completed source try. The
verifier checks marker kinds, function identity and protected-region ownership.
Control-flow flattening across protected regions remains unsupported; structured
returns, breaks, continues and outward gotos retain their observed exits.

At runtime, a try saves the cleanup cursor. An accepted catch unwinds the
protected body's locals to that cursor before entering the handler. Handler
locals die before its release state relinquishes the caught exception. Both
normal epilogues and exceptional exits use the same idempotent provider release;
rethrow and nested handling retain outstanding payload ownership. The generated
shared `CppHandlers` contract specifies nonthrowing handler bookkeeping,
first-pass filters and per-thread active-handler state. It is emitted only for
modules that use source handlers, preserving existing throw-only providers.
Production dispatch/ownership is still unbound; fixture execution does not claim
a complete exception runtime.

`cpp_catch_all_verify.py` compares 105 traces with native C++ and the unchanged
stock EDG C backend at O0/O2, then JIT and NativeAOT. Cases include nested handlers,
rethrow through a called function, replacement exceptions, constructor failures,
normal return/break/continue/goto, and throwing destructors on normal exits.
Malformed objects with missing/foreign markers, wrong original lifetimes,
misplaced handler bodies and prematurely armed release states are rejected.

## Ordered typed handlers (profile 34)

`source_handlers` now has dense per-try ordinals, a `typed` or `ellipsis` match
kind, and nullable `source_type_id`, `match_type_id` and `parameter_binding_id`.
All three are required for typed handlers and absent for ellipsis. The original
parameter type retains references and cv-qualifiers; the matching type removes
the outer reference. Ellipsis must be last. A handler's binding must belong to
its body and agree with its original source declaration and execution type.

`caught_object_addresses` owns the explicit `caught-object-address` expressions
already produced by EDG lowering. Each names its exact typed handler, has type
`void*`, and is valid only in that handler's region. The linker emits the source
handler order as successive C# catch filters. It does not infer a handler from
the CLR exception class or from the catch parameter's physical storage type.

`Edg.Runtime.CppTypedHandlers.generated.cs` extends the shared contract only for
modules using typed catches. `CppCatchType` carries original nominal identity,
leaf identity and pointer qualifications. `FilterTypedCatch` performs first-pass
matching without consuming the exception; `EnterTypedCatch` obtains a handler
frame after protected-body cleanup. Its stable address denotes the selected base
subobject for class references, or a per-handler converted pointer cell. The
original thrown pointer remains intact for rethrow, as in EDG's C runtime.

Parameter initialization is the existing EDG-selected expression/copy-constructor
call, followed by the observed caught marker. Catch-by-value objects participate
in the existing destructor state graph and die before the payload release state.
A failure before the caught marker goes through `FilterHandlerFailure`, whose
contract requires termination for a throwing parameter copy constructor.

The fixture validates 14 native/stock-C/JIT/NativeAOT traces covering ordered
matching, no numeric promotions, scalar references, class copies, nonzero base
adjustment, qualified pointer catches and rethrow identity. Separate child
processes verify parameter-copy failure termination in all four execution modes.
Five malformed, SQL-valid and digest-resealed objects test missing/foreign
addresses, wrong matching types, foreign parameter bindings and invalid order.

The matching provider remains fixture-only. Unsupported source exception type
identities, virtual-base conversions, constructor/destructor function-try blocks
and flattened transfers across protected regions remain explicit gates. Ordinary
function-try blocks follow EDG's ordinary try lowering; they do not require the
constructor/destructor wrapper semantics. No frontend behavior was changed.

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
