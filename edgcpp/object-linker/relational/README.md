# Relational semantic program objects

This is the replacement for the broad EDG observation-graph path. EDG writes one
SQLite database directly per source translation unit. The database contains
program entities and resolved references in ordinary SQL rows; it contains no
JSON documents, encoded semantic field blocks, or frontend observation history.

The current implementation is an initial executable slice, not full C++ or
DuckDB support. The source-shaped bodies are captured before lowering, preserving
expression types/value categories and structured statements. Original source
names and EDG linkage names are separate. Instantiations reference their selected
template and actual arguments. The C# consumer resolves recorded link identities
and emits supported operations; it does not redo EDG's C++ semantic analysis.

`schema.sql` is the storage contract. `browse.sql` contains ordinary queries for
functions, expression types/selected symbols, children, record layout and template
arguments. A SQLite browser needs no custom decoder. The initial schema has no
BLOB columns at all. Future literal byte arrays may use BLOBs; semantic graphs may
not.

Build the native compiler through the existing `backend/prepare.py` and CMake
path. SQLite development headers/library are required by the native exporter.
Build the consumer:

```sh
dotnet build edgcpp/object-linker/relational/managed/DotObj.Relational.csproj \
  -c Release -o edgcpp/artifacts/relational-tool
```

Compile and link:

```sh
python3 edgcpp/object-linker/relational/compile.py example.cpp \
  --output /path/example.dotobj --work-dir /path/example-work
dotnet edgcpp/artifacts/relational-tool/dotobj-relational.dll link \
  --out /path/generated --namespace Example --class Core --entry main \
  /path/example.dotobj
```

Strict linking requires providers for referenced external symbols. To emit reviewable
sources before those providers are available, add `--unresolved-providers required-partial`.
The generator emits explicit public partial function declarations and typed persistent
address accessors, with exact recorded identities in `Imports.tsv`. Missing implementations
fail C# compilation. Provider code must implement those declarations and keep external
storage addresses stable; generated code supplies no fallback storage or function bodies.
Original callable names take priority over generated storage properties and getters.
`tests/verify_required_providers.py` checks required bindings, cross-TU function pointers,
writable global addresses and a genuine native RTTI provider with source files hidden.

Selected type-trait builtin recipes retain EDG's exact operation name and ordered
source/type operands without computing a result or importing a callable.
`tests/verify_builtin_recipes.py` checks that separation from concrete execution.

The native compiler opens the SQLite transaction and inserts prepared rows.
There is no JSON wire or packager. On success the driver publishes the object;
on failure it preserves diagnostics and the unpublished partial file. Linking
and C# generation do not require the original source files.

The cross-unit test creates two objects, inspects SQL relations, hides original
sources, links and executes generated C#. It also compares native execution:

```sh
python3 edgcpp/object-linker/relational/tests/verify.py \
  --out /path/new-test-directory \
  --linker edgcpp/artifacts/relational-tool/dotobj-relational.dll --nativeaot
```

Explicit construction/destruction and exception execution now have a verified
cross-unit slice. `initializations`, `initialization_arguments`, `lifetimes`,
`lifetime_destructions`, `constructor_initializers`, `try_handlers` and `throws`
store EDG-selected semantic facts. Destruction ordinals are the actual EDG order;
initialization nodes and cleanup actions reference the same initialization row.
The generator preserves executable initialization positions, arms cleanup after
success, and reuses the existing shared C++ exception runtime. Run:

```sh
python3 edgcpp/object-linker/relational/tests/verify_lifetime.py \
  --out /path/new-lifetime-directory \
  --linker edgcpp/artifacts/relational-tool/dotobj-relational.dll --nativeaot
```

Nonvirtual base layout, selected pointer/reference conversions and explicit base
construction/destruction are also supported. `base_subobjects` and
`class_conversions` retain EDG-selected identities and adjustments, including null
preservation. Named pointer projections respect reused base tail padding. Run
`tests/verify_bases.py` with the same arguments to compare native, source-hidden
C# and NativeAOT execution, including second-base construction failure.

Named enums retain EDG-selected underlying types, ordered exact values and selected
enumerator references in `enums`, `enum_values` and `nodes.enum_value_id`. Generated
C# uses named enum declarations and values; folded constants retain their recorded
numeric value. `tests/verify_enums.py` exercises cross-unit storage, promotions,
unsigned 64-bit values and enum exception identity.

Template argument packs use explicit parent/member rows, including empty packs
and selected template-template references. `template_formals`,
`template_type_parameters` and dependence flags preserve source declarations and
prototype fields. The generator follows recorded executable references to choose
types for C# emission; it does not instantiate dependent recipes. Empty function
packs have a source declaration row and no runtime storage. Run `tests/verify_packs.py`
for the executable slice. `tests/verify_floating_metadata.py` checks explicit
floating formats and source identities. `floating_values` stores EDG-selected exact
scalar bits, referenced by expressions and floating template arguments/defaults.
`tests/verify_floats.py` checks negative zero, readable float/double literals and
binary80/binary128 arithmetic through the existing software runtime, including
source-hidden C# and NativeAOT. Floating environment/rounding-mode completeness
and half-precision execution remain work.

Dependent template values/defaults reference source expression graphs and explicit
parameter/member references. `nodes.domain='template-recipe'` keeps those graphs
inspectable without loading or instantiating them in the executable consumer.
`tests/verify_dependent_values.py` checks concrete execution and recipe isolation,
including dependent integer and floating defaults.

Records first referenced while incomplete retain their entity ID when EDG completes
the definition. Layout and source fields/bases are filled once before ABI lowering.
`tests/verify_record_completion.py` checks an early pointer-reference function followed
by construction and member access after the definition.

Lambda expressions and declaration-optimized closures retain their selected closure
type, call operator and aggregate initialization actions. Stock-generated captureless
entry helpers retain their completed body and scope-owned storage declarations.
`tests/verify_lambdas.py` checks value/reference/initialized captures, reference return
and an executed function-pointer conversion in native, source-hidden C# and NativeAOT.
Capture initialization is never reconstructed from source capture lists. Entry helpers
with caller-provided class results remain an explicit export boundary.

Schema 1022 stores dynamic exception specifications separately from `noexcept`, with
ordered allowed-type references in `exception_specification_types`; an empty dynamic
list is `throw()`. Member-pointer signatures retain their source class alias, member
type and EDG layout through existing type relations. These declaration facts do not
force executable type realization. `tests/verify_dynamic_exception_specs.py` checks
both metadata paths and source-hidden declaration-only execution. Defined dynamic
specification bodies retain an explicit generation boundary until runtime handling
is implemented; member-pointer operations also remain unsupported.

Array types keep fixed/zero/incomplete counts separate from selected dependent or
runtime bound metadata in `array_bound_kind` and `array_bound_node_id`. Bound recipes
retain EDG's exact local-expression correspondence and never add another runtime
evaluation. `tests/verify_array_bound_metadata.py` checks selected bound navigation,
concrete array execution and the explicit VLA dimension boundary. Source-only pack
size and size/alignment queries retain their selected operands without computing
or instantiating them in the consumer.

Compound assignments carry EDG's selected calculation type in
`nodes.operation_type_id`, distinct from their stored/result type. Named generated
helpers preserve operand order, narrowing and lvalue result identity. Run
`tests/verify_compound_assignments.py` for arithmetic, bitwise and shift coverage,
including conditional execution and address/reference results.
`tests/verify_pointer_compounds.py` covers fixed-layout pointer addition/subtraction
assignments with integer offsets, selected order and stable lvalue results.

EDG builtin va_list types carry a `semantic_role` while keeping their source type
chains and layout in SQL. The consumer uses the shared portable VaArg/VaPack/VaList
runtime, ReadOnlySpan argument packs and typed function pointers. `va_start` retains
its nonexecuted source anchor separately. Run `tests/verify_varargs.py` for argument
promotions, cursor forwarding/copying, indirect calls and exception cleanup.
Aggregate payloads and variadic virtual dispatch remain unsupported.

Fixed scalar/nested arrays use explicit initializer indices/repeats/value references
and EDG-selected zero actions. Global backing is pinned and aligned; local backing
is function-owned and survives source cleanup. `tests/verify_arrays.py` covers
lookup tables, partial/empty/nested initialization, declaration-order completion,
cross-unit mutation and cleanup reads. Array actions follow function-body leases.
Dynamic/record array initialization and local-static guards remain unsupported.

Record aggregate initialization uses selected field/base references and native-selected
zero byte extents, including empty-base and constructor-base destinations. Initialized
global records have pinned aligned backing. `tests/verify_record_initializers.py`
covers temporary receivers, partial/designated/nested values and cross-unit globals.
Dynamic components and partial-construction cleanup remain unsupported. These facts
serve generation; exposed compiler metadata alone is not a reason to add a relation.

String objects retain exact target bytes in ordinary offset/value rows, with selected
storage identities and address offsets. Narrow stock-lowering hooks preserve inline
literal linker identities; the consumer does not pool by contents or reinterpret
character encodings. Explicit copy/zero actions exist only for initialization uses.
The generator streams bytes into pinned aligned storage. `tests/verify_literals.py`
covers embedded zeros, character widths, constexpr references, offsets, and arrays.
Complete persistent variable addresses at byte offset zero use ordinary selected
cast/address-of/variable expressions. Other variable relocations remain unsupported.
Selected routine addresses at offset zero retain one typed `routine-address` node
and exact linkage identity; the consumer reuses ordinary function-pointer emission.

Constant/zero static locals retain lexical declarations and EDG-selected persistent
link identities. Scalar globals and statics use aligned pinned backing, allocated
before recorded scalar/record/array constant initialization actions execute.
`tests/verify_static_locals.py` covers shared inline and separate internal statics,
stable addresses and forward cross-TU pointer initializers. Dynamic static guards,
static destruction and TLS remain unsupported.

Static RTTI addresses retain EDG's selected represented type and exact persistent
ABI symbol, separately from the public source `std::type_info` view. External
RTTI declarations publish valid objects; executable generation reports a missing
provider explicitly. `tests/verify_typeinfo_addresses.py` checks ordinary SQL
selection, ABI layouts, source navigation and this source-hidden diagnostic.
Dynamic/CLI typeid and unprojected source expression operands remain unsupported;
no opaque type tokens or invented provider storage are used.

Fixed-layout pointer differences use selected typed operands and result, preserving
element counts through ordinary C# pointer subtraction. `tests/verify_pointer_differences.py`
checks cross-unit char/int counts, effects and conditional execution. Void, function,
incomplete and runtime-sized pointees remain unsupported.

Scalar default member initializers reuse ordinary selected constructor actions when
EDG explicitly selects the field initializer. `tests/verify_default_members.py`
covers constants, typed null pointers, templates, and explicit overrides. Richer
defaults needing constructor remapping or expression cleanup remain unsupported.
Cleanup-free folded record default constants also use those selected actions and
recorded field/base/zero writes; `tests/verify_default_record_constants.py` checks
nested values, explicit overrides and independent instances.
Class defaults with scalar constant constructor arguments use EDG's exact copied
initialization and finalized constructor cleanup order. `tests/verify_class_defaults.py`
covers partial failure, explicit overrides and constructor-exit destructor failure;
member-valued default arguments still reject explicitly.

Plain allocation uses EDG-selected count bindings, check/size expression graphs,
failure calls and allocator calls. `tests/verify_allocations.py` supplies explicit
allocator providers and checks once-only effects, invalid/zero/fixed counts,
conditional execution and exceptions. This is not a standard runtime binder.
Scalar placement construction now retains selected complete-record constructor or
expression actions, reusable placement/failure arguments, exact protected scope
and exception-only deallocation. Stock lowering supplies the nullable recorded
construction null guard. `tests/verify_placement_allocations.py` checks stable
destination identity, copy actions, once-only arguments and failure ordering.
Conditional/short-circuit placement, richer temporary cleanup, aligned/array
construction and cookies remain unsupported.

Dynamic scalar aggregate components reference exact initialization actions at the
existing selected field/array destinations. `tests/verify_dynamic_aggregates.py`
covers nested aggregates, returned records, side effects, zeroing and throws.
Cleanup-free complete record components use the same selected actions;
`tests/verify_record_aggregate_components.py` exercises record fields initialized
from references and returned values. Exported record array components still meet
an explicit managed record-array generation boundary.
Nontrivial component cleanup and dynamic repeated elements remain unsupported.

Local static assertions retain source declarations and recipe-only condition/message
navigation. The generator emits an accepted-EDG assertion comment and performs no
condition rechecking or message allocation; `tests/verify_static_assertions.py`
checks this separation. Dependent explicit template references retain ordered
source arguments in recipe graphs; `tests/verify_dependent_template_reference.py`
checks their isolation from concrete execution.

`if constexpr` retains EDG's recorded selected boolean and selected-body alias.
The predicate remains recipe metadata, while any C++17 initializer and its cleanup
remain executable. `tests/verify_constexpr_if.py` checks true/false template branches,
normal/exceptional initializer cleanup and explicit rejection of nonempty retained
discarded branches. The consumer never evaluates the compile-time predicate.

Residual `__builtin_is_constant_evaluated` queries use stock EDG's typed runtime
zero, while compile-time folded values remain EDG constants. Exact selected builtin
spelling remains navigation metadata, with no callable ABI/provider. Run
`tests/verify_constant_evaluation_query.py` for runtime and compile-time separation.
EDG-selected builtin addressof uses ordinary address-of on its selected lvalue,
bypassing overloaded `operator&`; `tests/verify_builtin_addressof.py` checks
once-only reference selection and cross-unit storage identities.

Switches retain exact selected case/default identities, values and source body order.
`tests/verify_switches.py` covers lexical fallthrough, nested exits and condition/local
cleanup. The reader emits the supplied control flow; it does not infer reachability.
Nested label-entry scopes, GNU case ranges and 128-bit dispatch currently diagnose
unsupported generation.

Range-based loops retain EDG-selected initialization, comparison and increment
expressions with explicit outer/iterator lifetimes. `tests/verify_ranges.py` covers
arrays, custom cursor/sentinel calls, C++20 initializers, nested control flow and
cleanup on return/throw/constructor failure. The reader performs no iterator lookup.

Plain scalar-pointee deletion retains the selected call and stock converted arguments
as ordinary expression nodes with a source `delete` node. `tests/verify_deallocations.py`
checks once-only pointer effects, null arguments, skipped branches, and cross-TU calls.
The consumer executes the supplied graph; it does not invent a null guard or choose
a deallocator. Class destructors, destroying delete, and array cookies remain unsupported.

Signed/unsigned 128-bit integers retain exact EDG decimal scalar values and layout,
using System.Int128/UInt128 in generated C#. No separate wide-value payload is used.
`tests/verify_wide_integers.py` covers literals, templates, arithmetic, arrays,
varargs, pointer conversion and extended floating conversion. Wide enum execution
remains unsupported.

Standard `std::nullptr_t` retains its distinct source identity and EDG layout.
`nodes.value_normalization` explicitly records zero-valued prvalue behavior;
the consumer preserves operand effects and follows that fact. Run
`tests/verify_nullptr.py` for byte-written storage, selected overloads, and
cross-unit parameters/returns. Nullptr exception matching remains unsupported.

Selected return initializations and `types.return_storage` distinguish ordinary
value returns from caller-provided result storage. The latter becomes an explicit
result pointer in C#, preserving object identity through forwarded calls. Run
`tests/verify_returns.py` to check self-pointer identity and construction failure
cleanup. Implicit member copies use the recorded `copy_source_symbol_id`; optional
NRVO optimization and more complex return-result unwinding remain work.
Shared conditional/comma class results explicitly reference their selected master
initialization through `destination_initialization_id`. The generator constructs
directly in that supplied destination, with one cleanup owner;
`tests/verify_shared_results.py` checks nested destinations and failure paths.

Condition declarations retain selected initialization, test expression, optional
C++17 initializer and lifetime ownership. Generated loops preserve per-iteration
cleanup and the selected increment/continue order. `tests/verify_conditions.py`
checks these with Clang as the original-source oracle (GCC bug 86769 destroys
for-condition variables too early), plus source-hidden C# and NativeAOT.

Virtual dispatch uses `virtual_calls`, `virtual_tables`, `virtual_table_entries`
and `vptr_writes`. Targeted stock lowering hooks supply exact targets, slots,
adjustments and constructor/destructor phases. The consumer uses pinned slot
arrays and typed function pointers, without selecting overrides. Run
`tests/verify_virtual.py` for phase changes, pure-slot metadata, second-base
adjustment and receiver/argument effects. Construction tables, virtual destructors,
virtual-base/covariant adjustments and variadic slots remain unsupported.

Scalar runtime-array byte-zero initialization follows an observed stock action,
its exact saved byte extent and selected null guard. Nonvirtual nonthrowing record
delete retains the chosen complete destructor, pointer reuse, null guard and
selected deallocator arguments. `tests/verify_allocation_actions.py` checks these
across source-hidden objects, including member cleanup and class-sized deletion.
Class/cookie arrays and throwing/virtual record deletion remain explicit boundaries.

Vacuous destructor calls retain the selected source operation and evaluate only
its object/pointer designation, without a destructor/provider. Optional type-name
operands remain recipe metadata. `tests/verify_vacuous_destructors.py` checks
scalar/record/field and value effects. Named gotos retain exact label targets and
separate common/containing lifetime references, without changing cleanup owners.
Forward jumps to labels in current/enclosing source blocks preserve existing
finally cleanup; richer target scopes reject explicitly. Run
`tests/verify_named_gotos.py` for nested loop/if exits and skipped construction.

This covers lexical return cleanup, partial member/base construction, reference catches,
rethrow and exception-object destruction. Arrays/virtual-base construction, nontrivial
temporaries, function-try blocks, complex catch conversions, macros, all type
variants, addressable globals/TLS storage and complex initialization remain work.
Unsupported forms must produce diagnostics, not an opaque fallback or silently
incomplete object. The preserved legacy runtime/emitter contains algorithms to
port for those requirements. Full-project bounded loading also remains to be
implemented. The C# consumer now loads one selected function body at a time and
streams generated source; the exception prepass uses the same bounded leases.
`tests/verify_body_loading.py` checks this with five units and 41 functions. CLI
metrics distinguish peak loaded body rows from catalogs and global expressions,
which still remain resident. This is not yet a whole-project memory bound.
