T4 finite SQLite acceptance is committed at `53dec72d7`; bounded T5 activation
is now open. Both candidates have genuine captures, twice-run separate native
observations and focused public-model reports. The bounded normal slice now has
[production CLI/JIT/NativeAOT acceptance](T5-NORMAL-LIFETIMES.md); exceptional
execution remains the next separate implementation. [PLAN.md](PLAN.md) and
[ACCEPTANCE.md](ACCEPTANCE.md) require ordered normal/exceptional lifetime
evidence; SQLite success does not establish these C++ contracts.

The two header-free, single-unit C++17 candidates are:

| Candidate | First bounded execution to establish | Deliberately separate later work |
| --- | --- | --- |
| [normal-lifetime.cpp](../tests/Fixtures/T5/normal-lifetime.cpp) | One record with noexcept constructor, explicit copy and destructor; nested automatic storage; early return; copy from a const-reference parameter into a function result; full-expression temporary passed by const reference | Constructor failure, partial arrays, static guards/TLS, inheritance, virtual dispatch, native C++ ABI interoperability |
| [exception-cleanup.cpp](../tests/Fixtures/T5/exception-cleanup.cpp) | Normal and throwing branches; int throw across one generated helper; two automatic objects around an int catch; exceptional cleanup before handler entry; surviving outer object's normal destruction on return | Class exception copying, rethrow, catch by reference, polymorphic matching, termination/double exception, partial construction, cross-boundary unwinding |

Each candidate has a scalar C entry and scalar observation functions for the
ordered event buffer, intermediate values, live-object count and overflow.
They run normal/early-return or normal/throwing paths in one finite invocation.
Repeated invocations retain the prior live count as an observation rather than
resetting it to conceal leaked cleanup. The record has one integer field, no
bases, vptr, allocation, library headers or foreign callbacks. Event storage is
bounded; overflow is observable.

The normal candidate returns a const-reference parameter by value, avoiding a
named-local NRVO choice. C++17 result initialization avoids optional extra
copies of that returned prvalue. A separate explicit temporary exercises the
full-expression boundary. This source intent is not a predicted retained
schedule: actual EDG-selected calls, storage and result roles govern the
accepted representation.

The frozen producer captured both with `--g++ --c++17 --gnu_version 130300
--exceptions`. Source/object/compiler/native-tool identities are in
`/mnt/storage/roslynobj-csharp-20261015/t5/capture-provenance-1.json`.
The normal object SHA-256 is
`4704fcfeb821ca3119ac883d9323203a7d529bba5d0147b7205e941ffa751718`;
the exceptional object is
`5836f0588d1f168b367996c52a0c84cb681ae0cd953a2050acf9b6a49e08eee5`.
The independent native oracle observes normal result 26 / 13 events and exceptional
result 11 / 14 events, twice each, with matching value/event arrays and live/overflow
zero. This is reference observation, not generated execution acceptance.

The focused [LifetimeFacts probe](../tests/LifetimeFacts/Program.cs) uses only
public navigation and semantic equality. Its first shadow-variable compile
failure is preserved; the corrected build has zero warnings/errors. Both runs
pass in about 0.40 seconds with unchanged inputs. The frozen probe source is
`/mnt/storage/roslynobj-csharp-20261015/t5/facts-source-2`. Reports:

- `t5/normal-lifetime-facts-2.json`: 621 catalog entities and 398 structural
  relationship occurrences; SHA-256
  `f90a60ba401082eea8980d26b8885681bf19e18a430f23ef62b25bf91e4be72d`.
- `t5/exception-cleanup-facts-2.json`: 564 entities and 336 relationship
  occurrences; SHA-256
  `75d42131c496af3fb874a8b32771018bbda43a4e34832807679b7ec13940810f`.

Report paths above are relative to the campaign artifact root
`/mnt/storage/roslynobj-csharp-20261015`. Structural parent/role/ordinal paths
preserve repeated identities and metadata boundaries. They are not an execution
schedule; typed facets can alias one operand and must not be replayed. Public
initialization and operation views sharing one entity retain both facets.

The normal capture has 16 required definitions and no required imports. Its
copy_for_return selected result is void with ordered return_parameter then
ordinary_parameter, ReturnsValueAsParameter=true and
ReturnsValueViaCopyConstructor=true. The source const-reference parameter is
already a natural 8/8 recorded reference pointer; no source signature substitutes
for this result convention. All PassedViaCopyConstructor parameter flags are
false. C1 constructor/copy entries take this_parameter then ordinary_parameter;
the actual C2 bodies take this_parameter then abi_parameter and explicitly call
C1. D2 explicitly calls D1. Entry identity is retained rather than collapsed
from a spelling or primary-entry association.

Every observed call's raw operands are the callee followed by its Arguments.
The receiver aliases argument 0 / raw operand 1 for constructors/destructors; the
result slot aliases argument 0 / raw operand 1 for copy_for_return. These aliases
must not add duplicate hidden arguments. Calls record left-to-right evaluation.
The ordinary_path body has 11 call occurrences, including six explicit D1 calls
across early return, inner normal exit, temporary and final-return paths.

Normal code still has real EH metadata: ordinary_path has four cleanup regions,
a region table, actual initial/null state 65535, ten cleanup-state operations,
one unreachable-cleanup-state operation, a prologue and two epilogues. Region
chains are outer 0 → null, copied 1 → 0, returned 2 → 0 and temporary 3 → 2. Each action is
an unconditional direct-variable destructor at offset 0, with no array/indirect/
base/VLA/ABI-detail feature. Three reachable lifetimes retain four dik_none
destructor recipes, all metadata, with no expression/operands/master/reuse.
One recipe records a temporary lifetime. The body has one LifetimeOperation
whose value is an explicit comma containing normal cleanup; no live
TemporaryOperation or class construction initialization recipe remains.
The constructor inside read_temporary's argument and the subsequent destructor
are already explicit calls. Replaying the metadata would duplicate them.
Noexcept facts are true for the source noexcept methods; generated C2/D2
wrappers have nullable specifications and explicit forwarding bodies. A future
bounded normal adapter must establish its nonthrowing path from those retained
facts, not discard all EH tables because the source looks simple.

The exceptional capture has 13 required definitions and exactly one required
EDG runtime helper, __destroy_exception_object. Retained _ZTIi is not a required
native import. The actual throw and handler share the same captured integer
type; the helper is selected by an exceptional cleanup action with no_object
storage, not an ordinary body call. Its generated runtime ownership semantics
need an explicit contract; it must not be bound to libc or inferred solely
from the helper's name. The exceptional interpretation remains separate from
the first normal execution slice.

Existing genuine evidence already distinguishes the contexts. The T0
[cleanup fixture](../tests/Inventory/cleanup.cpp) and its
[assertions](../tests/Inventory/Program.cs) passed inventory-only checks using
the current admitted public reader. The retained report is
`/mnt/storage/roslynobj-csharp-20261015/t0/cleanup-generated-4-cleanup-inventory.json`,
SHA-256 `249e979dbd9bd8ca73123270354897cd7afac71ccb35ef823cbca33df993f299`.
Its cleanup_context body has 102 inventoried operation identities and precisely
two selected constructor-call identities. It records three exceptional
destructor actions and metadata initialization recipes required as cleanup
dependencies with executionRoot=false. A separate catch initialization remains
metadata. This proves inventory context separation, not execution or a
destruction schedule. The external may_throw import has no implementation in
that fixture. The focused assertion did not activate the same recipe identity
through every combination of metadata/cleanup/execution context; do not claim
that additional test from the existing result.

Public facts to inspect on the new positive captures are already available:

| Public surface | Concrete inspection and use |
| --- | --- |
| FunctionSymbol.Signature/Parameters, SpecialFunctionKind, ConstructorOrDestructorKind, PrimaryEntry/AlternateEntryPoints | Record current selected entry identity, every parameter role/order/type, retained variable symbol and ctor/dtor variant. Existing T0 selected entries include this_parameter and an alternate-entry abi_parameter. Do not collapse entries from names or PrimaryEntry association alone. |
| RoutineType.HasThisParameter, ReturnsValueAsParameter, ReturnValueParameterFollowsThis, ReturnsValueViaCopyConstructor; ParameterType.Details.PassedViaCopyConstructor | Record exact current receiver/result/copy conventions on definitions and uses. SourceParameters and declaration signatures cannot substitute for the selected execution signature. Nullable facts remain nullable. |
| CallOperation.Callee, TargetMethod, Arguments/Role/Parameter, Receiver, Call.ReturnSlot; Operator.EvalLeftToRight/EvalRightToLeft | Map actual constructor/copy/destructor calls, receiver and return storage, then preserve occurrence and operand constraints. SourceTargetMethod is descriptive, not a rebinding key. |
| Initialization and InitializationDetails | Record kind, target, constructor/destructor, exact expression/constant/operand roles and order, reuse/conditional/partial flags, InitExpressionLifetime and object Lifetime separately. Master and NextInDestructionList associations add no automatic evaluation. |
| TemporaryOperation, LifetimeOperation, BlockDescriptor, Scope.ConstructorInitializers | Inspect actual temporary target/storage and initializer, full-expression lifetime, and recorded effective field initializer/source expression. Do not substitute default C# construction or infer initialization order from field declarations. |
| FunctionBody.Lifetimes and Lifetime.Cleanups | Reachability does not assign ownership to the function. Cleanup ordinals and conditional/unordered recipe flags are facts to interpret in context, not a lexical-scope destructor list. Lifetime.Children is reverse creation order and may omit implicit children. |
| ExceptionHandlingInfo and FunctionEhDetails | Record the actual profile's region/array tables, prologue/epilogues and nullable initial/null states. Enumerate regions/actions, next-region and supplement relationships. A missing table/state is not permission to invent one. |
| CleanupAction/Condition/StorageLocation/Array/AbiDetail | Inspect selected routine, exact base entity/variable, indirect/this-relative byte offset, conditions/comparison values, counts, VTT/VLA/delegation facts and flags. CleanupAction.IsUnordered is unknown; recipe IsUnordered is a different fact. |
| LoweredEhOperation and CleanupStateOperation | Distinguish cleanup-start, initialization-completed, handler/try and actual retained state operations by kind/RepresentationRole. A reference to a recipe is not an executed initialization edge. |
| ThrowOperation, TryOperation, ExceptionHandler | Capture thrown/caught types by semantic identity, operand/initialization/destructor/rethrow facts, handlers in retained order, parameter initialization and its lifetime. Match the admitted exception type/ownership contract rather than comparing display names. |

These surfaces are in the current public
[operations](../../roslyn/managed/RoslynObj/Operations.cs),
[extended operations](../../roslyn/managed/RoslynObj/ExtendedOperations.cs),
[control flow](../../roslyn/managed/RoslynObj/ControlFlow.cs),
[routine metadata](../../roslyn/managed/RoslynObj/RoutineMetadata.cs),
[lifetimes](../../roslyn/managed/RoslynObj/Lifetimes.cs) and
[lifetime descriptors](../../roslyn/managed/RoslynObj/LifetimeDescriptors.cs).
DescendantOperations may locate inventory identities; its deduplication and
metadata exclusion make it unsuitable as an execution schedule. Execution
must follow actual typed occurrences and selected operand/cleanup edges.

The accepted T4 product's emitter guards are intentional starting points,
not capture gaps:

- EmitFunction rejects nonordinary execution parameter roles and bodies with
  retained lifetime cleanups or EH regions. ValidateOrdinaryRoutineRoles also
  rejects this/result/copy roles. The first slice needs an explicitly bounded
  selected-role adapter; ordinary native-scalar admission must stay separate.
- Both legacy and planned calls reject a receiver/return slot. The admitted
  constructor/copy/return paths need an actual storage-and-role plan first.
- Initialization admits only its existing expression/constant/bitwise forms
  and rejects constructors/destructors and unproved conditional/reused cases.
  TemporaryOperation and cleanup-bearing LifetimeOperation have no general
  plan. A class-valued return is not established by the earlier plain-C record
  return gateway.
- Try/throw/lowered EH and required cleanup state have no accepted execution
  mapping. Do not remove these guards wholesale to make the positive fixture
  compile; implement only its observed profile and keep unsupported forms
  explicit before publication.

The coordinator has captured both candidates under the existing Linux x64
target and the explicit C++17/EH profile above. Inspect the bounded facts and
the actual pristine lower_eh/runtime contracts when an interpretation is
uncertain; the C generator's lowered-EH debug printer is not runtime semantics.
No source recovery,
remangling, new C++ semantic analysis or EDG modification is involved. A
native reference observes the independently compiled source; it does not
override the retained EDG program on a disagreement.

Establish the normal candidate first: exact event/value observations in native,
JIT and NativeAOT, with zero remaining live objects and no overflow after each
invocation. Preserve lowered normal destruction calls without replaying their
metadata recipes. Only then add the second candidate's generated-only throw,
payload/type match, cleanup activation and catch/return lifecycle. Exceptional
cleanup must run the actual actions/conditions exactly where selected, without
duplicating normal destruction or treating every reachable recipe as live.
No GC/finalizer, blanket using, source-scope unwind reconstruction or host C++
exception boundary substitutes for this work.

These two candidates do not close T5. The remaining acceptance rows still need
their own genuine activation: constructor failure/partial arrays, throw/rethrow/
termination, multiple/virtual inheritance and thunks, concrete instantiations,
RTTI, pointer-to-member, initialization guards/TLS and required extensions.
T5 activation changes no completed SQLite gate or captured input.
