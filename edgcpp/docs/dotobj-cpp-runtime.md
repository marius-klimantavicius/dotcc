# Owned C++ exception and RTTI runtime boundary

This is the implementation contract for the object pipeline, not a statement
that C++ exception translation is complete. SQLite already executes through
the object pipeline. DuckDB still needs packed-record access, dynamic
initialization, construction/destruction scheduling and C++ EH. Runtime functions alone
cannot supply those missing semantic facts.

## Canonical implementation to reuse

The direct backend already implements managed exception carriers and native
payload ownership in `backend/dot_gen_exceptions.h`, primary exception handles
in `dot_gen_exception_ptr.h`, termination in `dot_gen_terminate.h`, and type
identity/casts in `dot_gen_exception_identity.h`, `dot_gen_typeinfo.h` and
`dot_gen_rtti.h`. Use these algorithms and their existing tests. Adapt their
metadata inputs to linked owned objects; do not introduce a second exception
ABI or translate the stock C runtime's setjmp machinery.

The stock backend remains the reference for construction and cleanup decisions.
`upstream/lib_src/throw.c` defines pointer qualification matching, adjusted
catch pointer storage, handler entry and final payload destruction.
`upstream/lib_src/rtti.c` defines the inheritance traversals. The managed direct
backend already adapts those operations to CLR unwind and managed callbacks.

## Linker-owned type identity

The direct backend assigns exception IDs and opaque RTTI token offsets within
one input IL. Those numbers cannot be persisted as global IDs in separate
objects. Objects must contain source type references and authoritative EDG
RTTI associations; the linker allocates deterministic final IDs after resolving
external identity and retaining distinct TU-local entities.

Preserve these distinctions independently from the emitted C# storage type:

- `long` versus `long long`, `char` versus `signed char`, and the language's
  distinct character types, even when widths and signedness match.
- Each enum and class's identity, including namespaces and internal linkage.
  Equal layouts, unqualified names or display strings do not imply equality.
- `nullptr_t`; ordered pointer levels and their cv qualifications; member
  pointer kind and declaring class; source function signature and `noexcept`.
- Public/ambiguous inheritance paths and the adjustment for the actual complete
  thrown type. A base's standalone size is not its occupied subobject extent.

The current `source_types` kinds already distinguish ordinary integer kinds,
and v17 preserves lvalue/rvalue references and source routine receivers.
They are useful inputs, not yet a complete exception type descriptor. In
particular, audit the dedicated character-type flags, original member/function
types and RTTI associations before accepting them as EH identities.

The linker, not the runtime binder, must build the catch compatibility and RTTI
graphs. A runtime binder has no source AST to recover missing identities from
generated method names or pointer signatures.

## Owned operations and runtime calls

| Boundary | Owned information required | Existing implementation |
| --- | --- | --- |
| Throw construction | source type, payload size/alignment, selected constructor, complete-object destructor, construction completion | `__edg_create_exception`, `__edg_exception_storage`, throw emission |
| Handler entry | ordered handlers, selected catch type, by-value copy initialization or reference binding, adjusted pointer kind, catch activation | `__edg_create_catch_frame`, `MarkCaught`, compatibility matchers |
| Scope/unwind exit | activation state, exact cleanup order, normal/exceptional edges, partial array/subobject completion | direct backend region/lifetime emission, CLR `try`/`finally` |
| Rethrow | current active handler and original payload, not the mutated adjusted catch copy | `__edg_rethrow`, carrier retain/release |
| `noexcept` / double exception | termination boundary and whether the transfer is a source exception | termination helpers, runtime-transfer classifier |
| Type information | source type, canonical name bytes, compiler RTTI versus actual user-created `type_info` storage | type tokens, name slots, `__edg_type_info_field` |
| Dynamic cast | complete object, source and target type, vtable address point, public/virtual base graph | `__edg_dynamic_cast`, EDG-derived walkers |

Exception filters execute during CLR first-pass search; cleanup runs during
second-pass unwind. Do not destroy catch storage or mark a catch as entered in
a filter. The existing catch-parameter initialization failure boundary must
terminate, and an exception from a destructor during unwind must not replace
the pending source exception. C11 thread-exit transfers must bypass ordinary
source catch and `noexcept` boundaries using the existing classifier.

## libc++ external ABI

The pinned provider's `backend/runtime/libcxx/abi_exceptions.cpp` supplies public
libc++ methods. Its bridge imports are the following families; the binder must
validate exact owned signatures before emitting bodies.

| Imports | Required runtime state |
| --- | --- |
| `__cxa_allocate_exception`, `__cxa_free_exception`, `__cxa_init_primary_exception` | NativeMemory payload, canonical type token, selected managed destructor callback, pending construction reference |
| `__cxa_current_primary_exception`, `__cxa_increment_exception_refcount`, `__cxa_decrement_exception_refcount`, `__cxa_rethrow_primary_exception` | active handled carrier, retained payload ownership, independent rethrow carrier |
| `__edg_cxx_current_exception`, `__edg_cxx_increment_exception_refcount`, `__edg_cxx_decrement_exception_refcount` | same operations plus holder address and owning storage classification |
| `__edg_cxx_set_terminate`, `__edg_cxx_get_terminate`, `__edg_cxx_terminate`, `__edg_cxx_terminate_default` | atomic handler pointer, callback invocation, terminating boundary |
| `__edg_cxx_uncaught_exceptions` | actual per-thread throw/handler-entry count, including rethrow |
| `__edg_cxx_set_unexpected`, `__edg_cxx_get_unexpected`, `__edg_cxx_unexpected`, `__edg_cxx_unexpected_default` | retained legacy libc++ ABI and dynamic exception specification behavior when selected |

`__cxa_bad_cast`, `__cxa_bad_typeid` and
`__cxa_throw_bad_array_new_length` are actual translated C++ definitions in
`abi_core.cpp`. They throw the corresponding public libc++ classes. A BCL
exception or an empty partial body would not preserve their source semantics.

No-op reference counts and constant-zero uncaught counts are invalid providers.
The storage carrier uses NativeMemory and a rooted owner handle; deterministic
release invokes the selected C++ destructor only after construction completed.
GC abandonment frees storage without invoking arbitrary translated destructors
on the finalizer thread. The existing holder/region bookkeeping also depends
on global/TLS/heap owner classification and must be connected to the object
runtime before claiming `exception_ptr` owner disposal.

For static assembly mode, keep canonical metadata rooted for assembly lifetime.
For optional independent owners, immutable type metadata can be shared, while
payload ownership, callbacks, termination handlers and explicit disposal must
retain their owner. Do not equate `GC.KeepAlive` with an ownership policy.

## Type-info ABI caveat

Opaque compiler RTTI tokens in the direct backend have eight-byte identity
slots. They are not native `std::type_info` objects that callers can freely
dereference. libc++'s inline `name()` reads the name-pointer field; the direct
backend redirects this through pinned name slots. User-created subclasses of
`type_info` retain actual record storage. Preserve that distinction as an owned
operation/provenance fact before emission. Matching a field's spelling alone is
not sufficient authority to reinterpret arbitrary user storage.

Built-in RTTI definitions that EDG deliberately leaves to the native runtime
may be supplied from their authoritative source type and canonical EDG symbol.
An unresolved arbitrary class RTTI symbol must remain a link error. Vtable
RTTI relocations, public type-info references and EH matchers must all refer to
the same linker-owned identity.

The owned-object pipeline now supplies real class RTTI vtables by translating
`backend/runtime/libcxx/abi_typeinfo.cpp`: EDG's existing `cxxabi.h` implementation
classes and the actual key destructor definitions from `lib_src/typeinfo.c`.
LLVM's original `src/typeinfo.cpp` supplies the base `std::type_info` destructor.
These are ordinary source-provider objects with real function relocations and
storage; no placeholder vtable cells or native ABI library are supplied.

`object-linker/libcxx-rtti-profile.json` pins the adapter and its authoritative
EDG source/header. The libc++ manifest now selects 56 original/provider units.
Only the two RTTI destructor providers use explicit `--no_exceptions`; their
empty/nonthrowing bodies need no source exception implementation. They preinclude
LLVM `<new>` so generated deleting destructors bind against declared standard
deallocator signatures. All other libc++ units retain their exception policy.

`tests/rtti_provider_verify.py` verifies actual virtual dispatch, derived/base
destruction, null/non-null delete, ordinary derived copies, multiple and empty
base layouts with native, JIT and NativeAOT execution. It requires the real class
and single-inheritance RTTI key functions to be selected and rejects unresolved
storage providers. This does not establish `dynamic_cast`, public `typeid`,
fundamental RTTI definitions or EH matching. Those still require their own owned
identity and lifetime tests; the direct backend's opaque-token scheme is not
automatically applied to these real RTTI objects.

Abstract and deleted virtual slots bind to shared `Edg.Runtime.CppVirtual`.
EDG's `pure_virt.c` and `del_virt.c` call `__abort_execution` directly; the managed
provider uses `Environment.FailFast` and does not invoke `std::terminate` hooks.
`rtti_provider_verify.py --fixture abstract` exercises an actual abstract
interface, overridden const calls, normal derived/base destruction and isolated
pure/deleted ABI abort paths in JIT and NativeAOT. Both callbacks have checked
zero-argument `void` signatures and never return a placeholder value.

## Executable arguments and shutdown lifetime

The owned executable entry accepts `int main(int, char**)` in addition to
zero-argument `int`/`void` entries. The shared `Edg.Runtime.ProgramArguments`
owner supplies writable UTF-8 argument strings and a null-terminated pointer
table in one native allocation. Generated entry code retains the owner around
the entire `ObjectProgram.Run`, including normal-exit callbacks and thread
cleanup, then releases the original allocation even if source code has changed
individual argument pointers. There is no mutable global argument cache.

`tests/runtime_arguments_verify.py` compares native, JIT and NativeAOT execution
with empty and Unicode arguments, a nonzero successful return value, and an
`atexit` callback that reads the modified arguments after `main` returns. It
also checks the storage owner separately and rejects nonstandard entry
signatures. This does not add the optional `envp` argument or establish native
Windows/macOS execution coverage.
The same helper and translated C executable also pass on Windows .NET JIT
under Wine, including empty/Unicode arguments and the callback after `main`.
That additional receipt is not native Windows or Windows NativeAOT validation.

## Independent-TU acceptance

`tests/cpp_cross_unit_verify.py --fixture indirect` now verifies nontrivial
class value calls across independently packaged provider/consumer units.
Factories, by-value parameters, callbacks and member functions retain the
source-to-execution ABI mapping, including the order of hidden result buffers
and receivers. A factory and an ordinary pointer function can share a physical
signature without sharing their source contract. The test checks required
copy-constructor side effects and caller isolation, nested prvalues, array
destinations and a constructor default adapter forwarding an indirect value.

Native038/managed026 passes native C++ O0/O2, compiled stock EDG C lowering,
source-hidden managed linking, JIT and NativeAOT. The stock C executable uses
the host Itanium array helper; translated execution uses the canonical managed
array runtime. Cleanup is deliberately trivial here; this does not establish
general C++ unwinding. See `evidence/dotobj-indirect-cross-unit-20261008.json`.

`object-linker/tests/cpp_cross_unit_verify.py --fixture exceptions` compiles provider, bridge
and consumer separately, retains stock-C lowering for each, and then attempts
owned packaging/linking. Normal mode requires runnable managed output;
`--allow-export-rejection` only records the current implementation gates.

The fixture checks source integer/class/enum distinctions, anonymous-namespace
types that must remain TU-local, cross-TU adjusted pointer rethrow, class
destruction, multiple-inheritance dynamic casts, null pointer conversion,
function-pointer noexcept conversion and multi-level pointer qualification.
Native O0/O2 pass. Native025/managed020 rejects provider base/subobject layout
and bridge/consumer destruction obligations. The retained receipt is
`dotobj-linked-eh-acceptance-002/results.json` in the campaign directory; there
is no managed execution claim for this fixture yet.

After these operations execute, reuse the existing libc++ exception-pointer,
uncaught-count, foreign-exception and owner-lifetime fixtures. Their successful
direct-backend runs are not evidence for the owned-object pipeline.

## DuckDB dependency acceptance

`tests/duckdb_dependencies_verify.py --fixture yyjson|mbedtls` selects original
units from the pinned DuckDB project manifest and adds a public-header consumer
as a separate translation unit. It compares native O0/O2 output with managed
JIT/NativeAOT output once packaging succeeds. Normal invocation fails on an
export rejection; `--allow-export-rejection` records the pending gate without
claiming managed execution.

The JSON corpus covers ordinary/in-situ reads, round trips, malformed UTF-8 and
escapes, integer limits, extension policy and a mutable document with 1,024
array elements. The SHA-256 corpus checks 36 pattern/length combinations against
Python `hashlib`, plus streaming, context cloning, unaligned input/output,
sentinels and context/buffer zeroization. SHA-256 includes the original
`platform_util.cpp` provider, preserving its secure-zeroization implementation.

Native036/managed024 receipts show both native oracles pass and both consumers
package. Original `sha256.cpp` still rejects packed-record alignment; yyjson
and `platform_util.cpp` reject their empty inline-assembly compiler barriers.
The tests retain these failures and do not patch source or generated code to
get around them. See `evidence/dotobj-duckdb-dependency-oracles-20261008.json`.

## Empty assembly memory barriers

The detached `tests/runtime_barriers_verify.py` audit checks native GNU empty
memory constraints against a proposed BCL lowering. It does not enable owned
inline-assembly support. On the recorded Linux x64 JIT and NativeAOT builds,
`Thread.MemoryBarrier` preserves a second pointer load that the no-barrier
control merges with the first. Both direct clearing and a volatile function
pointer call complete before the fence. Seven runtime assertions also check
operand evaluation, unchanged operand memory and unaligned zeroization with
sentinels. The retained disassembly and 25 checks are recorded in
`evidence/dotobj-memory-barriers-20261008.json`.

The emitted `lock or [rsp],0` is a hardware fence; native empty assembly needs
no such instruction. This is a conservative correctness starting point with
additional cost, not a performance equivalence claim. The fence touches its
own stack slot and does not read or write through assembly operand addresses.
An owned barrier must separately retain input/output direction, evaluated
addresses and extents, including side effects of VLA-shaped operands. It must
not introduce memory accesses merely to model a constraint.

[Thread.MemoryBarrier](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.memorybarrier?view=net-10.0)
specifies ordering; that contract alone is not a universal guarantee against
eliminating unused private stores. Mbed TLS's actual source already makes an
opaque call through a volatile `memset` pointer. Preserve that operation and
verify the resulting project code. Dedicated secure-zero APIs can instead use
the BCL's [CryptographicOperations.ZeroMemory](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.cryptographicoperations.zeromemory?view=net-10.0),
which specifically addresses write-elimination concerns. Arbitrary assembly,
other architectures and general private-buffer dead-store guarantees remain
outside this detached audit.

## Packed field access

Preserve ordinary named field access when the generated CLR record carries the
correct explicit offsets and packing. The absence of an `unaligned.` IL prefix
does not establish that a field access is unsafe: the runtime has type layout
information, and its code generator can account for alignment independently of
that prefix. The previous proposal to rewrite every packed field access as an
`Unsafe.ReadUnaligned`/`Unsafe.WriteUnaligned` call was based on insufficient
evidence and must not serve as the emission rule.

The .NET 10 runtime exposes managed layout alignment through
[`getClassAlignmentRequirementStatic`](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/vm/jitinterface.cpp).
Its JIT also introduces unaligned access flags itself for some visibly
misaligned ARM32 floating-point accesses in
[`fgMorphFinalizeIndir`](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/jit/morph.cpp).
That ARM32 example disproves the inference from IL alone; it is not an ARM32
execution claim. The current object target requires a 64-bit process.

Keep the distinction between a named packed field and an arbitrary typed
pointer. Casting a byte address to `int*`, or decaying an array inside a packed
record to its element pointer, can lose the containing layout's alignment
information. Explicit unaligned operations remain appropriate for raw accesses
whose known address alignment is insufficient for their accessed type.
Record copies must use their declared record alignment rather than the maximum
natural alignment of all descendant fields. A pointer to an ordinary record
that has itself been placed at an insufficiently aligned address is a separate
case; packing does not repair an incorrectly declared pointee type.

Atomic and volatile operations need their own alignment and ordering checks.
Successful ordinary packed loads do not prove that an unaligned atomic or
volatile operation is supported. Host x64 tests likewise do not establish
execution coverage on other architectures.

The translated packed-layout fixture also passes Windows .NET JIT under Wine
on x64; this is supplementary execution evidence, not native Windows or ARM64
coverage. The [runtime audit receipt](../evidence/dotobj-packed-runtime-20261009.json)
retains source hashes, the executed assembly hash and the initial harness run
that rejected Wine's synchronization startup notice despite application exit 0.

The profile21 SQLite regression freshly packages all 225 original SQLite/musl
units, resolves all 70 runtime imports, and passes the eight engine suites in
Linux x64 JIT and NativeAOT. The subsequent property-accessor naming correction
produces the same set of 237 translator-generated C# files, byte for byte, as
that executed build. Runtime binding files are separately identified by their
provider receipts. See [SQLite profile21 evidence](../evidence/dotobj-sqlite-pipeline-20261009.json).

## Owned empty memory assembly, profile22

The object now owns exact empty GNU assembly memory operands: directions and
constraints, optional operand names, source locations, element types, fixed
dimensions, and the saved total-element binding of a rank-one VLA. The C#
emitter evaluates each address and extent once and calls `Thread.MemoryBarrier`.
It introduces no read or write through those addresses. The hardware fence is
stronger than the original compiler-only barrier; optimizing that cost remains
separate from preserving correctness. Nonempty assembly, register-only operands,
goto labels, other retained clobbers and multidimensional VLA operands remain
explicit gates.

`tests/memory_asm_verify.py` passes 63 checks with native041/managed033, including
native C++ O0/O2, source-hidden independent-TU linking, JIT and NativeAOT. It
checks side effects, unchanged operand bytes, fixed multidimensional arrays,
dynamic byte and wider-element extents, and malformed semantic metadata after
digest repair. Stock C output is retained to inspect lowering: input VLA array
expressions decay to pointer expressions, while their evaluated dimensions
survive in synthetic bindings. The exporter observes the original region before
that decay. Stock C is not an execution oracle for those VLA operands.

Original DuckDB SHA-256 and platform zeroization sources now package, link and
execute with the musl providers under managed031 JIT and NativeAOT. The synthetic
consumer uses `MBEDTLS_ALLOW_PRIVATE_ACCESS`, matching DuckDB's original wrapper;
without it, Mbed TLS intentionally exposes differently named context members.
Nominal record compatibility was not weakened to hide that configuration mismatch.
All 36 digest cases match independent Python `hashlib` results, including
streaming, cloning, unaligned buffers, sentinels and zeroization.

The final managed033 alignment analysis distinguishes unknown pointer origins
from observed storage alignment. A normal `void*` or byte-pointer value does not
prove that a later typed access is underaligned; defined source accesses retain
the destination type's contract. Known packed addresses remain tracked through
casts and byte offsets. This allows original yyjson to verify and generate C#.
Its runtime binding currently stops at `_ZTIv`, and is deferred while missing
object features take priority.

The final033 Mbed TLS and yyjson review projects are generation evidence only.
yyjson's C# is unchanged from managed032; Mbed TLS changes one musl `vfprintf`
file from its executed managed031 version. These partial translations are linked
under `artifacts/review/duckdb-dotobj`; they are not complete DuckDB output.
[Owned memory assembly evidence](../evidence/dotobj-memory-asm-20261009.json)
records the exact tools, receipts and retained initial failures.

## Shared provider implementation checkpoint (v36)

The object linker now includes `object-linker/runtime/CppExceptions.cs` by default
for reachable source exceptions. It implements the previously captured contract:
aligned native payload allocation, partial/complete ownership, exact/base/pointer
matching, stable converted-pointer cells, thread-local handlers, nested rethrows
and termination paths. `--exception-provider external` preserves explicit custom
provider integration and independent fixture-provider testing. The implementation
uses BCL APIs and managed function pointers and is tested under JIT and NativeAOT.
See [the public runtime boundary](../object-linker/runtime/EXCEPTIONS.md) for managed
library consumption and remaining ABI/platform gaps.

`CppArrays.cs` supplies source-owned vector construction/copy/destruction and
reverse cleanup. Actual EDG/LLVM RTTI source providers remain separate linked
objects. This checkpoint does not implement `std::exception_ptr`, custom
`set_terminate`, native C++ interoperation, cross-thread exception transfer, or
platform lifecycle release of a C++ propagation abandoned by a forced transfer.

## ICU register-or-memory alias barriers, profile41

ICU's empty `asm volatile("" : : "rm"(pointer) : "memory")` permits a memory
operand. The exporter preserves the original `rm` or `mr` constraint and chooses
that permitted alternative only for an original object lvalue. Its owned
address/extent evaluation and the existing BCL fence preserve side effects
without fabricated loads or stores. Register-only constraints, rvalues requiring
new temporary materialization and array-decay alternatives remain gated. The
stock C backend preserves the constraint and original operand; EDG's configured
parsed constraint list carries both alternatives, which our observer retains.

This follows GCC's [simple operand constraints](https://gcc.gnu.org/onlinedocs/gcc/Simple-Constraints.html)
and [extended assembly contract](https://gcc.gnu.org/onlinedocs/gcc/Extended-Asm.html).
The stronger BCL fence remains a correctness choice with potential runtime cost.
Native099/managed110 passes29 focused checks in `duckdb_icu_alias_barrier_verify.py`: native and stock C at O0/O2, managed JIT, original constraint
metadata and resealed invalid-object rejection. Actual bundled ICU `format.cpp`
now packages; other ICU units still have independently diagnosed lifetime/array
ABI gates. No full ICU runtime claim follows from this change.

## Default member initializer ownership, profile41

A C++ default member initializer is an original source template. EDG copies its
DIP into each constructor that selects it; an explicit member initializer must
not be attributed to that template. `.dotobj` preserves the original field,
type, selected root destructor when one exists, nested lifetime graph and
original declaration ordering. The constructor execution relation separately
records the observed constructor-list ordinal and stock copying or no-effect
removal. An unused template remains source metadata without invented storage,
selected destructor or physical record layout.

A root field rollback targets the observed constructor receiver and original
member path. A partial field-array rollback retains its original static-graph
obligation and has a per-constructor action or exact terminal no-throw
disposition. A nested argument temporary instead adopts its original expression
lifetime into the constructor; its cleanup targets the temporary, not the
member. The original constructor graph is retained before expansion. Selected
root destruction requires an owned operation or an observed stock elimination;
absence of a cleanup action alone is not an elimination proof.

Native102/managed112 verifies the shared field-array fixture under native and
stock C at O0/O2 and managed JIT (37 checks, including source-hidden and resealed
malformed metadata). Native103/managed114 additionally passes the nested argument
temporary fixture (40 checks) and root no-effect destruction fixture (39 checks).
Both preserve original source ownership and reject resealed malformed metadata.
The separate constructor-initializer no-effect pass is now observed as well;
the original base-destructor obligations remain even when stock removes their
calls. Full DuckDB export still reaches independent dependent-template type
identity gaps. Final NativeAOT acceptance with native103/managed114 passed 43
field-array, 43 field-temporary and 42 field-no-effect checks, retained under
`dotobj-v41-focused-aot-103`. The complete v41 focused set passed 288 checks
and the broad frozen regression suite passed 1042 checks.

## Vector allocation protocol ownership (profile 42)

The array-new relation preserves EDG's selected runtime protocol rather
than invent an explicit cleanup region around a helper that already owns it.
`lower_array_new` retains the original allocation rollback DIP in
`ndsp->freeing_of_storage_on_exception`. For the IA64 `__cxa_vec_new2` and
`__cxa_vec_new3` paths, `make_vec_new_call` chooses the final constructor,
destructor, allocator and deallocator callbacks and emits seven arguments:
count, element size, padding, constructor, destructor, allocator, deallocator.
The stock implementation is in `upstream/lib_src/vec_newdel.c`.

`array_allocation_operations` rows contain the call node, original
allocation obligation and optional original element-destruction obligation,
selected allocator/deallocator entities, element type,
nullable constructor/destructor callback entities, helper kind, observed padding
size and `observed-array-new-runtime` capture proof. These are distinct source
obligations: element rollback destroys the constructed prefix, while allocation
rollback invokes the selected deallocator. The call remains in the
owned expression graph. Verification checks exact argument references and callback
signatures, original selected `operator delete[]`, dynamic count type, element
extent and observed padding. Ordinary allocation-obligation completeness then
includes this helper-owned operation alongside explicit rollback and observed
elimination.

Observation must surround the actual `make_vec_new_call` in `lower_array_new`
and inspect the finalized helper call before later casts and assignments. It
applies only when the helper allocates storage (`entity_node == NULL`). Placement
or aligned allocation outside the helper retains its separate rollback proof.
The implicit-default `__cxa_vec_new` form needs a distinct proof of its default
allocator/deallocator selection and must not be accepted merely by its name.

Acceptance must compare native C++, stock lowered C and translated execution
for successful allocation, allocator failure without deallocation, constructor
failure after a constructed prefix, reverse prefix destruction, and exactly one
selected deallocation. The sized-delete form receives the element extent plus
padding. For zero elements, stock EDG may allocate one payload byte while still
passing zero element extent plus padding to sized delete; preserve that observed
protocol rather than equating allocation granularity with constructed extent.
Runtime implementation can follow this capture contract; adding the relation
alone does not make the runtime helper executable. Native and stock C oracles
for both helper forms pass 28 checks, including an out-of-line constructor whose
receiver ABI is lowered after the callback address is created
(`dotobj-vector-allocation-oracles-003`). Only an exactly observed finalized
callback operand may use the selected target declaration ABI in that case; the
general routine-designator type equality remains enforced.

Native106/managed116 passes 69 focused metadata/generation checks and 28 native
and stock C oracle checks at `dotobj-vector-allocation-v42-003`. Both new2 and
new3 generate C# with source files hidden and reject nine resealed malformed
objects each. This uses an explicit allocation-only fixture mode: successful
arrays are destroyed and deallocated with ordinary source C++ calls, preserving
the same event oracle while isolating the separate vector-delete exporter gap.
The full `delete[]` fixture remains rejected by missing original destruction
ownership (`dotobj-vector-allocation-v42-002`). Production new2/new3 runtime
providers are also pending; these results do not claim translated execution.

The next independent source relation will retain the pre-lowering pair between
an allocation DIP's `assoc_new` initialization and the exact element DIP inside
the array aggregate/repeat initializer. Current protocol capture observes both
original obligations and validates their execution, callback and element type;
the additional relation will reject swapping otherwise matching obligations
between two source array-new expressions. Do not infer this pair from adjacent
ordinals: stock scanning can add argument temporaries between observations.

Profile 43 also preserves the original allocation-to-element relationship in
`source_array_allocation_elements`. It follows the frontend's
`allocation DIP -> assoc_new -> dynamic_init` repeated-element initializer,
recorded before lowering. Cleanup-list adjacency is not assumed: constructor
arguments may introduce intervening temporaries. Runtime allocation operations
must reference that exact original pair. Native107/managed117 passes 83 focused
checks plus 28 native/stock oracle checks with two distinct same-type new
expressions in the same function, including swapped-element and missing-pair
mutations (`dotobj-vector-allocation-pair-v43-107-002`).

The explicit-deallocator array-delete protocol is represented separately by
`source_array_deletions` and `array_deletion_operations`. The source relation
retains the original partial element-destruction obligation and selected
`operator delete[]`; the runtime relation retains the finalized delete2/delete3
call, complete destructor callback, selected deallocator, element extent and
padding. The owned helper call performs reverse destruction, cleanup of the
remaining elements if a destructor throws, and deallocation. Exact stock root
replacement is observed because `lower_array_delete` overwrites the source
expression with the helper call. Virtual destructor dispatch, aligned separate
deallocation and implicit-default helper forms still require additional owned
protocols. Native109/managed119 passes 123 full-fixture metadata/generation checks plus
28 native/stock oracle checks (`dotobj-vector-full-v43-109-001`), including
source-hidden generation, exact original pairing for two new expressions,
malformed relation rejection, null deletion, zero-length arrays and a throwing
destructor. This resolves the earlier full `delete[]` fixture exporter gate.
Production vector runtime providers and translated execution remain pending.

Profile 44 distinguishes generated record temporary storage from source local
variables. `source_record_temporaries` records each represented original
`enk_temp_init` DIP's source owner, type, dynamic initialization kind, traversal
ordinal, position and original destruction obligation. The observation runs
before lowering, including constructor-initializer expressions and initializer
templates. `record_temporary_initializations` ties an owned `record-storage-init`
node and its generated local binding to that original DIP through stock
`lower_temp_init`'s exact destination selection. This does not create a source
local declaration or grant value-copy/default-value permission to a noncopyable
record. Zeroing follows the same explicit-zero/partial-initialization predicate
as stock `c_gen_be::start_initializer_assignments`; otherwise only the actual
member stores are emitted.

The reduced DuckDB delegating-constructor temporary passes 36 focused
native/stock-C O0/O2, source-hidden C# generation, JIT execution and malformed
metadata checks with native110/managed120
(`dotobj-record-temporary-v44-110-002`). The two-same-type-temporary probe retains
separate original DIPs and generated bindings; source traversal order and
reverse destruction-list order are recorded independently. The hardened two-temporary case passes 42 checks with native111/managed121,
including NativeAOT execution and rejection of swapped same-type original
temporary IDs (`dotobj-record-two-temporary-v44-111-001`). The verifier requires
generated local storage and matches its original destruction obligation to an
independent emitted cleanup/normal-destruction target, or an exact observed
elision in that execution. Source locals remain on their separate provenance
path when a master initialization targets existing storage.

Profile 45 normalizes function-designator `eok_lvalue_adjust` using stock
`dump_lvalue_cast`'s function-to-pointer decay rule. The owned graph already
represents a function designator as its function pointer, so this adjustment
performs only the required function-pointer conversion. It does not take the
address of that pointer as data storage. Ordinary function-pointer variables
and members remain typed places and retain their real address/cast/dereference
operations. The focused native112/managed122 case passes 19 checks, including
native and stock-C O0/O2, original-source-reference metadata, source-hidden
JIT/NativeAOT execution, a side-effecting function-reference factory evaluated
once, ordinary function-pointer lvalues/members, and a noexcept function
reference (`dotobj-function-reference-v45-112-001`).

Profile 46 retains lazy defaulted/generated routine exception-specification
resolution through the exact stock
`form_exception_specification_for_generated_function` boundary. The original
source-type snapshot keeps the initial specification flags and policy;
`source_routine_exception_resolutions` records the original/final type identities,
final flags and policy, constructor/destructor/operator kind, defaulted/generated
origin, and an original function entity when already emitted. The observer does
not resolve EDG state or force routine bodies. Both semantic routine rows and
source ABI signatures use the observed final policy, while ordinary ABI/function
consistency checks remain strict. `exception_policy_origin` distinguishes this
path from an unchanged capture and makes missing resolution provenance invalid.

The actual DuckDB `bind_recursive_cte_node.cpp` reproducer passes 15 metadata
checks with native113/managed124, including nine resealed malformed-provenance
rejections (`dotobj-exception-policy-v46-113-001`). Its three defaulted `vector`
constructors were initially captured without a specification and later finalized
as `noexcept`; all three retain the same source type IDs and naturally owned
function entities. The unit and two RTTI provider objects generate C# containing
1,043 function definitions. Unresolved imports are retained; this result verifies
semantic metadata and C# generation, not compilation or execution of DuckDB.

Profile 47 asks EDG to complete the implicit exception specification of an
actually referenced in-class defaulted member through its existing
`complete_defaulted_exc_spec` frontend API. The query occurs before stock
`force_definition_of_compiler_generated_routine`, because that call can lower
the selected body and receiver before returning. It is restricted to original,
complete frontend types with no explicit specification or pending field
initializer processing; it neither forces extra bodies nor relaxes link-time
signature equality. The existing finalization observer retains the initial
absent specification and the exact resolved throwing or nonthrowing policy.

The two-translation-unit test passes 29 checks with native116/managed128,
including native and stock-C O0/O2, implicit nonthrowing and throwing defaulted
constructors, an explicit `noexcept(false)` constructor, source-hidden combined
C# generation, JIT execution and NativeAOT execution
(`dotobj-defaulted-policy-v47-116-001`). A further 15 checks reject nine resealed
malformed resolution records (`dotobj-defaulted-policy-negatives-116`).

The native117/managed128 strengthened test also passes 29 checks when a
different member lowers the receiver in an earlier function, and retains an
unused defaulted constructor with an ill-formed dependent initializer without
instantiating it (`dotobj-defaulted-policy-v47-117-001`); the 15 provenance
checks pass again (`dotobj-defaulted-policy-negatives-117`). EDG explicitly
supports already-prelowered receivers in
`update_generated_exception_spec_for_fields`, skipping generated base fields
that its base-class traversal already handled. The query therefore checks
that the selected routine's original ABI has not yet been lowered, rather than
requiring an untouched receiver class. Queries remain confined to referenced
members; class-wide eager exception-specification instantiation is not used.


### Typed floating classifications (v48)

EDG's canonical builtin kinds for `signbit`, `isnan`, `isinf`, and `isfinite`
now produce typed intrinsic nodes, retaining their exact builtin spelling and
unpromoted `float`, `double`, or binary80 operand. Their polymorphic frontend
ellipsis declarations do not describe an ordinary variadic call. Ordinary
variadic calls still require default argument promotions. Fixed `f`/`l`
variants require the corresponding operand width.

C# uses BCL classification and bit operations for `float`/`double`; binary80
uses its original sign, exponent and significand, without rounding through
`double`. Each operand is evaluated once. The native119/managed134 focused
suite passes 26 checks, including native and stock-C O0/O2, signed zeros,
subnormals, infinities, signaling/quiet NaNs, finite values beyond binary64's
range, source-hidden generation, JIT/NativeAOT, and four resealed malformed
objects (`dotobj-v48-floating-119-134-001`).
