# C# lifetime cleanup from EDG region tables

Implemented against EDG 7.0 revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. The backend handles ordinary
objects and members, including virtual bases, fixed arrays, expression temporaries, class
returns and unordered temporary groups. It preserves EDG's normal destruction
calls and emits CLR exceptional cleanup from EDG's region-table metadata.
Built-in integer, enum, object/void pointer, nullptr and class
exception payloads, qualification-aware catch matching and rethrow are implemented.
Public unambiguous nonvirtual and virtual base matching is implemented.

## Current lowering boundary

The `cpfe-dot` configuration is:

```text
DO_IL_LOWERING=1
DO_FULL_PORTABLE_EH_LOWERING=0
GENERATE_EH_TABLES=1
DO_UNORDERED_EH_PROCESSING=1
KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED=1
```

This is EDG's existing partial-EH configuration. Full portable lowering remains
disabled, so the C# output contains no setjmp/longjmp implementation or native
EH-runtime dependency. No EDG lowering source was changed for this increment.
The earlier target-only descriptor-retention overlay in `backend/prepare.py`
is unchanged; retained lifetime descriptors still help inspect diagnostic IL.
Exceptional dispatch now uses the tables, not those descriptors' final links.

EDG supplies ordinary calls and control flow, plus:

- `leck_function_prologue`, identifying the routine's region and array tables.
- `leck_cleanup_state`, now containing a **region number**, and unreachable
  state annotations that must not change the executing cursor.
- Region entries containing a destructor, object handle, successor and flags.
  Conditional entries refer to a following flag-location supplement.
- Array supplements containing an object handle, element stride and count.
- Generated conditional-flag initialization, publication and clearing. These
  track unordered groups as well as conditional construction.

The table variables use EDG's function-local static initializer lists. The
backend locates them by identity through the prologue and consumes their
constants while emitting the function. They are not translated as mutable
application globals or generated C# arrays. Other function-local static storage
still rejects. Table handles are interpreted symbolically: variable plus offset,
indirection through a pointer variable, or an offset relative to `this`.
No native stack-layout offsets are copied into the managed program.

`dot_gen_cleanup.h` emits direct C# destructor calls in a switch. It validates
table shapes, supported flags, successor links, cycles, state references and
array bounds before publishing output. It supports ordinary object/array
records with conditional, indirect and `this`-relative addressing, including
base subobjects and hidden VTT supplements. Scalar allocation-failure cleanup
accepts translated delete providers, including sized delete. VLA and
local-static-guard records reject. The limit is 4096 region
entries per function, including supplements; ordinary storage/frame limits
still apply. Array cleanup is bounded to 64 MiB.

## Cleanup ownership and liveness

EDG owns normal-exit constructor/destructor calls, including return, break,
continue and goto cleanup. Generated C# has the following structure when a
cleanup cursor is needed:

```csharp
int state = 0; // zero represents EDG's null-region sentinel
bool unwinding = false;
try {
    // Lowered calls, conditional flags and region transitions.
} catch {
    unwinding = true;
    throw;
} finally {
    if (unwinding) {
        // Dispatch the region chain, advancing before invoking a destructor.
        // Test and clear conditional flags before destroying live objects.
        // Terminate if a destructor throws during this cleanup.
    }
}
```

EDG region numbers are mapped to positive switch cases; its null sentinel maps
to zero. Following the successor before invoking a destructor avoids repeating
that destructor if it throws. A flag is tested before evaluating its object
address, so an unconstructed receiver or argument is never dereferenced by its
conditional cleanup. Sequencing helpers receive a pointer to the caller's
cursor and explicitly captured caller variables. They create no closures or
copies of addressable C++ objects.

An outer `finally`, when needed, frees a large native local frame after object
cleanup. Successful constructor and return paths do not drain the exceptional
cursor: completed members or returned objects have transferred ownership.
Foreign managed exceptions retain their identity through cleanup and rethrow.
Noexcept violations and secondary destructor exceptions terminate through
`Environment.FailFast`.

CLR unhandled exceptions need not execute finally blocks, particularly under
NativeAOT. Tests therefore provide an outer managed catch, including in expected
termination subprocesses. No cleanup guarantee is claimed for process
termination or fatal conditions such as stack overflow. C++ terminate handlers,
`uncaught_exceptions()` and the complete foreign-exception policy remain future
runtime work.

## Unordered groups and temporary receivers

The standard C backend uses `lower_eh.c::maintain_unordered_destructions_set` to
link an unordered group into the region table as a unit. Conditional flags tell
cleanup which objects have completed construction and remain live. In
`lower_init.c::gen_one_destruction`, normal destruction clears applicable flags
before calling a destructor; the group can remain reachable while individual
members are removed from it.

The C# emitter now consumes that same representation. This handles forms such
as `X(1).member(X(2), X(3))`, including the lowered order in which argument
construction moves ahead of receiver construction. It also handles conditional
receivers, nested returned receivers, partially constructed receiver members,
aggregate receivers, result objects overlapping argument temporaries and
loop/goto reentry. Constructor failure cleans only completed objects; throwing
normal destruction cleans the remaining live group members once.

The previous implementation used `GENERATE_EH_TABLES=0`, canonical successor
chains and a dependency check that rejected temporary receivers with multiple
temporary arguments. That rejection is now removed. `dot_gen_order.h` retains
only the operand-order validation relevant to C# emission. Unsupported
right-to-left lowered operations still reject.

Group cleanup follows the **standard C backend's table order**. For example,
`call-9` now yields `3129678`, matching standard `cpfe`; the previous canonical
profile yielded `3129768`. No traces are normalized and no EDG code was changed
to reconcile the profiles. The old `cpfe-dot-oracle` canonical-profile build
remains available for historical inspection, but it is no longer required by
the acceptance suite or used as this feature's primary oracle.

## Arrays, temporaries and class returns

`dot_gen_arrays.h` implements the EDG-generated `__cxa_vec_ctor` and
`__cxa_vec_dtor` calls in managed code. Only the compiler-generated routine
entries with those names are mapped. Construction counts completed elements
and destroys that prefix in reverse on failure. Normal destruction advances
its remaining count before each call; a first destructor failure cleans the
remaining elements and rethrows. Another failure during unwinding terminates.
The region-table array supplement supplies the address, count and stride for
exceptional destruction of completed arrays, including flattened multidimensional
arrays and element sequences.

`dot_gen_sequence.h` handles comma/lifetime expressions using explicit pointer
captures. Lvalue results return an address. Conditional and short-circuit effects
stay inside the original branch, and assignment helpers retain RHS-before-LHS
sequencing. No Roslyn pass or runtime code generation is needed for this state.

`lower_il.c::lower_return_statement` already uses caller-provided storage for
nontrivial returns. NRVO constructs the selected local there; copy/move returns
keep the selected constructor call. Direct, forwarded, member and indirect
returns preserve these hidden pointers. Guaranteed C++17 elision is tested with
deleted copy/move constructors, and self-address checks catch unintended copies.

With region tables enabled, EDG's cloned entries represent cleanup before and
after a returned object's initialization completes while argument temporaries
remain alive. The earlier C# completion-bit buffer and
`leck_initialization_completed` handling have been removed. This preserves the
return suite's agreement with executable EDG C output while using a single
cleanup representation for ordered and unordered cases.

With EDG exceptions disabled, normal destructor calls remain and EH metadata is
absent. A separate native/JIT/NativeAOT fixture checks those normal paths. The
public driver still enables EH; EH-disabled translated code has no promised
cleanup for escaping foreign managed exceptions.

## Validation and inspection

Build the backend plus the standard C oracle/runtime, then run:

```sh
edgcpp/build.sh
EDG_BUILD_TARGET=cpfe-dot edgcpp/build.sh
python3 edgcpp/backend/tests/verify.py --nativeaot
```

The suite contains 14 ordinary lifetime traces, 48 array/temporary traces,
114 return-object traces and **317 unordered/group traces**. Each suite runs in
static and instance modes under JIT and Linux x64 NativeAOT. The expanded group
suite adds 128 cases covering temporary receivers and related cleanup paths.
It checks values, self addresses and preserved exception identity, and injects
failures at successive occurrences of construction, body and destruction events.
Five group termination scenarios per owner/runtime mode include receiver
construction failure and a throwing destructor after a member-call body fails.

The group oracle lowers the shared header-free native harness through the
original `cpfe`, compiles that generated C with both GCC and Clang, links EDG's
`libC.a`, and requires identical traces from both native executables and C#.
That archive is used only by the test oracle. Managed code has no native EDG
runtime dependency. Return tests likewise use standard `cpfe` as the primary
oracle; their GCC/Clang C++ comparisons remain informative observations.

The array suite retains its GCC/Clang C++ comparison. Its two previously recorded
GCC 13.3 nested-brace cleanup differences are `1237676` versus `12376`, and
`123487676` versus `1234876`; the translated code still matches the Clang traces.
The return suite records, among other differences, EDG `19` versus GCC `196`
when a guard destructor fails after direct result construction, and EDG `1466`
versus Clang `146` when a parameter temporary destructor fails. These comparisons
do not change EDG or replace the stated primary oracle.

Inspect `artifacts/backend-tests/` after verification:

- `unordered-portable.c`, `unordered-oracle.cpp` and
  `unordered-native-oracles.json`: standard C reference and compiler evidence.
- `unordered-{static,instance}/`: split C#, numeric-region IL traces and hosts.
- `return-objects-oracle.c` and `return-objects-{static,instance}/`: return evidence.
- `array-temps-{static,instance}/` and `lifetime-noeh/`: earlier regression evidence.
- `results.json`: full-suite results; the durable summary and source hashes are
  in `evidence/dot-gen-backend.json`.

Set `EDG_DOT_TRACE=1` on `dotnet.py` to inspect lowered lifetimes and numeric
cleanup transitions. The manifest records `cleanupModel: edg-region-tables`.
Tracing resets its diagnostic identifiers before emission and does not change
normal generated names.

## Integer and class exceptions

`backend/dot_gen_exceptions.h` consumes retained `enk_throw` supplements and
lowered thrown/caught-object address nodes. It allocates a managed exception
carrier with a pinned byte array and the payload type's required alignment.
EDG's lowered initialization expression writes that payload before the CLR
throw. `GC.KeepAlive` roots the carrier across helper calls and failing operand
evaluation. Managed storage is reclaimed by the GC, while class destruction is
explicit and follows EDG's exception use counts.

Matching uses the original EDG integer kind and bool/character-type flags,
not the C# field type: long and long long, char and signed char, bool and
unsigned char, and wchar_t/char16_t/char32_t and their underlying integers
remain distinct. Typedefs and top-level const work. EDG initializes catch
parameters, including reference binding; modifying a by-value catch leaves the
payload unchanged, whereas modifying a reference changes the rethrown value.
Class/struct/union types get distinct translation-unit IDs, including
local classes with the same unqualified name. Native RTTI definitions and their
initializer dependencies are replaced by this metadata. Enums also retain
nominal IDs; pointer, polymorphic class and virtual base matching are described below.

Each try saves its incoming cleanup region. A side-effect-free CLR filter checks
type identity and whether the try remains active. On exceptional entry, its
generated catch drains the EDG chain to that boundary before entering the first
matching source handler. Outer objects remain live. Catch-body failures drain handler-local cleanup
before leaving the active catch. Cleanup dispatch is duplicated at boundaries
in this prototype; sharing it is a later code-size optimization.

A thread-static stack of managed catch frames retains the active exception.
`throw;` uses `ExceptionDispatchInfo` and therefore works from called functions,
including a call through another instance of the same generated program class.
Nested handlers restore the previous frame. Bare rethrow without an active
handler terminates. Catch-all also catches foreign managed exceptions; typed
typed catches do not, and rethrow preserves the foreign object identity.

Two details follow the C backend rather than inferred lexical C# scope:

- `leck_try_epilogue` disables that try's handler before return/goto cleanup
  outside the try. A side-effect-free CLR filter tests this flag.
- `leck_catch_epilogue` and the region action `__destroy_exception_object`
  pop the catch frame at EDG's exit point. An idempotent CLR finally completes
  the pop if necessary. Destructors executed after leaving the catch cannot
  accidentally rethrow its previous payload.

EDG also emits bare returns on unreachable/fall-through paths of some non-void
functions. Its C backend accepts that shape; the C# emitter uses an
`InvalidProgramException` trap if such a path is reached. No EDG lowering code
was changed for this increment.

`tests/exceptions.cpp` and `exceptions_verify.py` compare 32 traces with the
standard EDG C backend plus libC, compiled independently by GCC and Clang.
All pass in static/instance and JIT/NativeAOT modes, including nested handlers,
temporaries in throw operands, failed operand evaluation, return/goto exits,
catch reentry, reference mutation and rethrow during normal destruction. Managed
hosts force GC while a caught reference is live, overlap two threads' handlers,
and check cross-owner rethrow and foreign identity. Seven termination cases per
mode check noexcept, double exceptions and bare/stale rethrow (28 subprocesses).

## Class payload ownership and failure paths

EDG's `a_throw_supplement` supplies the lowered initialization expression,
payload type and destructor. The backend preserves that initialization and the
lowered catch-parameter copy. Storage is stable through constructor/member calls,
reference catches, return-slot construction, rethrows and GC. Payloads are bounded
by the existing aggregate size limit (65536 bytes) and alignment limit (4096).
Unnamed const aggregate templates created by `lower_init.c` get stable pinned
arena storage. User-declared function-local statics still reject.

The carrier starts with one use for the original throw; `throw;` adds another,
as `lib_src/throw.c::push_throw_stack` does. Each catch epilogue or exceptional
catch cleanup releases its use. Nested rethrows caught inside an active handler
do not destroy its payload. When the count reaches zero, a completed payload's
destructor runs once; failed construction does not call the complete destructor.
EDG constructor cleanup remains responsible for completed members. The carrier
marks destruction before calling the destructor, following `destroy_thrown_object`,
so another exception cannot cause a duplicate call.

The destructor callback is a managed function pointer taking the payload address
and its originating owner. The carrier holds that owner strongly. An exception
thrown by owner B and caught by owner A therefore destroys against B's globals.
Catch-frame release destroys before popping the runtime entry, matching
`__free_thrown_object`. No finalizer runs C++ destructors.

The native runtime's throw-processing marker also defines the failure boundary:
`leck_exception_started` marks the throw-copy phase, and `leck_exception_caught`
ends catch-parameter initialization. A failure escaping either phase terminates.
Dedicated CLR filters terminate during exception search, before intervening
finally blocks can perform additional cleanup. Typed try filters must reject
nonmatching exceptions during that same search; catching everything and testing
types after entering a handler would unwind too early. A constructor that handles
its own exception completes normally. This follows the pinned EDG runtime's
policy without changing EDG lowering.

Normal exception-object and catch-parameter destructor failures use EDG's behavior:
the `noexcept(false)` cases can propagate to an outer handler. A failure during
existing exceptional cleanup terminates. Six termination fixtures compare the
complete pre-termination event stream with C compiled by both GCC and Clang,
including failed copies with live members and a nonmatching inner handler.

`tests/class_exceptions.cpp` and `class_exceptions_verify.py` validate 39 normal
and recoverable-failure traces in static/instance JIT/NativeAOT modes. Coverage
includes split output with top-level/nested types, value/reference catches, original-versus-copy mutation, nested uses,
replacement exceptions, normal/goto/loop exits, members, empty/trivial/local/union
types, returned objects, forced GC and cross-owner destruction. Every trace ends
with zero live objects and no invalid self-addresses. Foreign exceptions raised
in a handler destroy its payload, while failed throw-operand evaluation does not
destroy an unconstructed object; both preserve the foreign exception's identity.

The C oracle's `eh_alloc_on_stack` uses `MOST_STRICT_ALIGNMENT` (8 in this runtime)
and `__throw_setup_dtor` has no alignment argument. Thus the 64-byte-aligned probe
can report different alignment observations between C builds. These are recorded
in `class-exceptions-oracle-alignment.json`; the value/lifetime/address-identity
traces still must agree, and each generated C# run separately requires 64-byte
alignment. No C runtime or lowering change was made.

This ownership contract covers translated handlers in one generated program.
Retaining exception carriers in an external managed host, moving them between
threads/modules, and exception_ptr/native ownership transfer remain unsupported.
An escaping carrier is not a public resource-management API.

### Pointer, nullptr and enum matching

The backend prescans throws and handlers for the entire translation unit and
assigns deterministic IDs using C++ type identity. Enum identities remain
distinct from integers and from other enums with identical underlying storage.
Scoped enums, typedefs, local enums, reference mutation and 64-bit values are
covered. The original nullptr typeref must be inspected before stripping it:
EDG lowers its physical representation to `void*` while retaining its original
type through `get_underlying_type`.

Pointer descriptors record their leaf identity and the qualifiers at each
pointee level. Generated side-effect-free matching helpers implement
`lib_src/throw.c::check_exception_type_specifications` and
`check_pointer_levels_and_qualifiers`: equal-depth qualification conversion
requires every earlier destination level to be const when adding a qualifier.
Single-level source qualifiers constrain void-pointer conversion; for multilevel
pointers EDG stores qualifiers in a separate array, and its void-pointer fallback
uses the single-level flags. The implementation follows that C runtime behavior.
The same matching helper is used during exception search and handler selection.
No reflection or native RTTI lookup is involved.

Each typed pointer catch allocates one pinned pointer slot and copies the
original thrown pointer into it. `leck_caught_object_address` refers to this
slot, mirroring EDG's `a_throw_stack_entry::pointer_buffer`. Thus modifying a
`T*&` catch affects that handler's buffer, while rethrow uses the original
pointer and original type. Nested rethrows get independent buffers. In contrast,
integer, enum and class reference catches still bind to the payload itself.
The managed catch frame roots its buffer; the payload continues to root the
originating owner. A nullptr converted to a pointer catch receives a null slot.

Partial EDG lowering creates unnamed integer arrays of pointer qualifier flags
even though the partial handler descriptor does not retain those array links
(`lower_eh.c`'s partial `lower_try_block` path). The backend preserves initialized
storage for these synthesized arrays, as it does for constant aggregate
templates. It does not discard arbitrary unnamed variables or enable user local
statics. EDG lowering and its C backend/runtime remain unchanged.

`pointer_exceptions.cpp` checks 48 result/cleanup traces against original EDG C
compiled by GCC and Clang. Static and instance projects run in JIT and NativeAOT,
with forced GC, nested pointer-reference mutations, cross-owner throws/rethrows,
foreign managed errors and concurrent catches. Both split output modes must
regenerate deterministically. A separate catch-all-only translation unit covers
leaf RTTI traversal without incidental roots from typed handlers. Function pointers, member pointers, array pointees,
and volatile access remain explicit exception rejections.

### Nonvirtual bases and adjusted catches

The backend consumes EDG's `is_lowered_base_class` fields and base offsets.
The compiler-generated vptr is also accepted after validating its name,
offset and pointer type against EDG's class metadata. Base fields retain ordinary
C# member access. EDG's truncated subobject types use explicit sizes and `Pack=1`,
so a five-byte base representation stays five bytes even when its complete
object needs eight. This handles the tail-padding case that the C backend must
flatten in `dump_field_list` to avoid native C struct size rounding.

EDG sometimes lowers base assignment to `memcpy` with the precise subobject
byte count (`lower_init.c::make_memcpy_call`). Generated managed copy/zero
helpers implement those compiler-generated `memcpy`/`memset` calls and validate
their ABI. They do not bind arbitrary external libc functions. Unnamed,
statically initialized templates of source record types can also be unqualified
in aggregate base initializers; these use the existing pinned template arena.

`RDF_BASE_CLASS_SUBOBJECT` (0x40) is accepted for non-array entries.
The descriptor names the subobject destructor; the emitter follows that
selection, as the C runtime does. Its overload as a VLA flag remains rejected.
Hidden VTT arguments are covered below.
Normal calls and partial-construction cleanup remain EDG-owned.

Exception matching helpers return a byte adjustment or -1. The backend reads
`a_base_class::has_public_derivation`, `ambiguous` and `offset`, following the C
runtime's public/unambiguous conversion rule. This applies to class values and
single-level pointers, with qualifier checks; `Derived**` does not match `Base**`.
Each handler computes its address from the original payload. Pointer catches
adjust their separate pinned slot while preserving null. Base-reference catches
use the adjusted payload address, and base-value catches run EDG's copy code.
Rethrow and final owner-specific destruction keep the original complete-object
identity and address, even after catching through a nonzero-offset base.

`inheritance.cpp` compares 48 layout/value/lifetime traces with original EDG C
under GCC and Clang. JIT/NativeAOT runs in static and instance modes cover multiple,
indirect and repeated bases; inaccessible/ambiguous matches; empty bases; tail
padding; null adjustment; construction failure; value/reference/pointer catches;
and nested rethrows. Host checks force GC and verify cross-owner destruction,
foreign exception identity and separate inherited globals.

Next are type_info support, member pointers and exception_ptr. Uncaught-exception counts,
cross-module type identity and complete foreign/native exception policy remain
unimplemented. The current tests execute only the Linux x64 target.
Full allocation/runtime ownership, dynamic static/TLS initialization, function-try-blocks
and the full runtime/library policy remain separate implementation work.

### Virtual dispatch, VTT cleanup and complete-object destruction

`dot_gen_virtual.h` identifies EDG's ordinary and construction vtables. The
backend retains their offsets, virtual-base displacements, VTT pointers and
lowered call expressions. Routine slots become managed function pointers, with
explicit-owner adapters in instance mode. `enk_result_of_overriding_function`
forwards thunk parameters like `c_gen_be::dump_result_of_overriding_function`;
EDG supplies the surrounding `this` and covariant-result adjustments. Pure
virtual slots point to a managed `Environment.FailFast` stub.

Only identified RTTI address constants in vtable initializers are replaced by
opaque pinned token addresses. A nested static holder initializes their storage
before global relocations read it, independently of split-file ordering. Tokens
are shared across owners, survive GC, and differ for distinct types in this
translation unit. Pointer `dynamic_cast` uses these tokens and the graph described
below. Reference casts use the optional standard-exception provider described
below; public `type_info` operations are covered in the later RTTI increment.

A base-region entry with flag 0x20 has a VTT supplement after any condition
supplement. The emitter resolves its direct/indirect handle and passes the VTT
as the destructor's second argument, following `lib_src/throw.c`. Thus failed
construction and unwinding use EDG's construction vtables. Complete exception
payloads require the complete-object destructor: following full portable
`lower_throw`, the backend selects EDG's existing `cdk_complete` entry rather
than a primary/subobject entry requiring a hidden VTT. It never creates or
changes EDG constructor/destructor variants.

Virtual-base catches use EDG's offset from the known complete thrown type to
the shared virtual base, with the same public/unambiguous and pointer-depth
checks as nonvirtual catches. Rethrow retains complete-object identity, and final
destruction uses the original owner's complete-object entry.

Scalar new/delete works with translated providers, including the optional
exception bootstrap described below.
`RDF_NEW_ALLOCATION` cleanup skips null and calls the selected provider; for
sized delete it reads the size from the array supplement's `element_size`, as
`throw.c` does. The virtual-destructor fixture covers primary/secondary-base
delete, constructor failure and a throwing destructor. In that last case the
C oracle's deleting entry calls the complete destructor before delete and emits
no delete event after the throw; the generated C# preserves that sequence.
General allocator/libc integration and array-allocation helpers remain open; the
optional bootstrap now supplies basic scalar allocation.

`virtual_dispatch.cpp` has 32 differential result/lifetime cases;
`virtual_inheritance.cpp` has 24. Original EDG C agrees under GCC and Clang.
Both generated static and instance projects run under JIT and NativeAOT, with
forced GC, cross-owner rethrow/destruction and foreign exception identity.
Pure-virtual calls terminate in both native oracles and all four managed modes.
A destructor-bearing constant aggregate initializer in this fixture also checks
that `stmk_init` emits initialization separately from destruction, matching the
C backend. Validation remains Linux x64 only.

### Pointer RTTI and active construction layouts

`dot_gen_rtti.h` consumes the existing `__class_type_info`,
`__si_class_type_info` and `__vmi_class_type_info` constant initializers. The
resulting managed graph retains descriptor kind, VMI flags, direct-base order,
nominal type IDs and packed signed offset/access/virtual flags. Negative virtual
base offsets remain signed longs. Runtime descriptors are immutable and shared
across owners; each cast keeps its search state in local variables.

The managed algorithm follows `lib_src/rtti.c`'s `traverse_base_classes`,
`try_downcast`, `find_base_class_at_addr` and `derived_to_base_conversion_r`.
It first searches for a unique destination containing the specified public
source subobject. If that fails, it checks source accessibility and performs the
cross-cast search. Matches require type and address, since different base types
can share an address. Virtual bases are located through their current vtable
entries, rather than through offsets for a hypothetical complete standalone
object. This preserves EDG's construction/destruction vtable behavior.

Only compiler-generated `__dynamic_cast` calls with the expected four-argument
ABI are mapped. Source and destination descriptor constants become graph IDs;
the native optimization hint is ignored as it is in EDG's runtime. The object
still supplies its active dynamic type token and offset-to-top. EDG supplies the
null check and operand sequencing. `dynamic_cast<void*>` is already arithmetic
in the lowered IL. No native RTTI allocation, reflection, setjmp or dynamic code
generation is involved. A nested static holder avoids split-file initialization
order dependencies. Graph depth and traversal-size bounds reject before output
publication.

`dynamic_cast.cpp` compares 42 result/lifetime traces in all four managed modes
with original EDG C under GCC and Clang. Tests include repeated source bases,
ambiguous/inaccessible destinations, nonpolymorphic siblings, shared virtual
bases, construction layouts with displaced virtual bases, side-effecting/null
operands, const/void casts and polymorphic exception payloads. Managed hosts also
check forced GC, foreign cleanup and casts through another owner.

Reference casts now bridge `__cxa_bad_cast` to a translated C++ provider when
`--runtime edg-exceptions` is selected. They remain rejected without a provider.
Public `type_info` operations are implemented below; native/cross-module RTTI identity remains open. This
increment does not change EDG lowering or its existing exception-matching rules.
EH still uses its generated type IDs and matching switches; those IDs are not
yet unified with the RTTI graph for cross-module use.

### Reference casts and a translated standard-exception provider

EDG `lib_src/rtti.c` implements `__cxa_bad_cast` by throwing `std::bad_cast()`.
The opt-in `backend/runtime/exceptions.cpp` follows that rule with ordinary C++
source. `dot_gen_runtime.h` connects the compiler runtime entry to its translated
provider function after validating both ABIs. The normal exception prescan sees
the provider's throw, registering its type before any matching helpers are
emitted. EDG supplies the standard class layout, vtables, construction and
complete-object destruction; no synthetic CLR exception type impersonates the
C++ payload. This also keeps instance-mode owner retention and active-catch
stack behavior identical to other translated throws.

The provider includes EDG's pinned standard exception declarations and defines
`exception`, `bad_cast` and `bad_alloc` members. Virtual deleting entries use its
scalar delete provider, while final exception-payload release uses the complete
destructor and never frees the pinned carrier as native heap memory. Scalar
new/delete bridge to .NET NativeMemory, with zero-size normalization and failure
conversion to a C++ `bad_alloc`. The current bootstrap has no new handlers,
aligned/array/placement/nothrow allocation or automatic owner disposal.

The 24 reference-cast traces compare with original EDG C and libC under GCC and
Clang, then static/instance JIT and NativeAOT. They cover typed/base/value/ellipsis
catches, copying/slicing, stable payload identity across rethrows, active-catch
restoration, failed construction, scalar allocation and virtual deletion. Host
checks force GC, rethrow through another owner and preserve foreign exception
identity. Noexcept and secondary-failure subprocesses require termination in all
four managed modes and both native oracles. The outer host/native handler forces
actual unwinding, avoiding a false positive from unhandled-exception termination.
No EDG lowering changes were needed.

### Public type_info and bad_typeid

The RTTI registry now includes EDG's non-tag exception/RTTI type list as well as
scope types. Class graph entries retain their existing IDs; nonclass metadata
does not enter inheritance traversal. Zero-offset projections through EDG's
synthetic metadata base fields become references to the same opaque token used
by vtable slots. EDG's private two-field typeinfo prefix is represented by the
public `std::type_info` pointer type at reference boundaries. Sequence helpers
do not capture metadata variables as application locals.

The optional C++ provider supplies equality, inequality, `before()` and `name()`.
A single validated name bridge returns pinned bytes extracted from EDG's native
descriptor/name initializers. For built-ins defined only by libC, the Itanium
symbol suffix supplies the type name after checking EDG's built-in runtime-type
rule. Name strings are interned in an immutable lazy holder; token/name storage
is shared across owners. This follows EDG's name-pointer equality and ordering,
without depending on native link order or application-global initialization.

EDG `lower_typeid` already emits active-vtable lookups, single evaluation and
null checks. Its `__cxa_bad_typeid` entry bridges to ordinary C++
`throw std::bad_typeid()`, just like the bad_cast provider. Existing cleanup,
catch copying/slicing, rethrow and owner-specific payload destruction apply.
Virtual operations on metadata objects themselves reject: opaque tokens do
not provide the native ABI metadata vtables. Cross-module identity also remains
outside this increment.

The oracle uses the original `cpfe`, C backend and libC; it never links the new
provider. GCC/Clang traces agree for 34 result/lifetime cases and 24 names.
Static/instance JIT/NativeAOT match them and check GC/thread/owner stability,
foreign cleanup and noexcept termination. Ordering tests check EDG's pointer
ordering rule and strict-order properties rather than comparing arbitrary
native/managed allocation orders. Explicit-cast case 16 has result 0 in the
original C backend, whose output uses static L/R descriptors; that behavior is
retained. The pointer-variable form in case 32 exercises dynamic identity.
No EDG implementation or lowering was changed.


## Dynamic initialization and allocation increment (2026-10-02)

The backend now consumes the C runtime's local-static guard region action (flag 0x80), including abort/retry on failed initialization. EDG's generated startup routines initialize globals after constant relocations; priorities are ordered explicitly. TLS aliases resolve to the generated initializer and owner/thread arenas retain their guards. Global and thread destructor registrations use reverse order.

`dot_gen_lifecycle.h` supplies active-call leases, synchronized guards, explicit `ReleaseThread()` and idempotent `Shutdown()`/instance `Dispose()`. Shutdown waits for active calls, permits their reentry, rejects new calls, and refuses to destroy foreign-thread TLS on the wrong thread. Canonical function-pointer adapters also check lifecycle state. Hosts must release each translated thread; there is no automatic CLR thread-exit hook.

`dot_gen_arrays.h` follows `lib_src/vec_newdel.c` for vector cookies, sized deallocation, completed-prefix constructor cleanup and reverse destruction. `leck_internal_try` preserves EDG's expression-level cleanup followed by rethrow; it unwinds the retained regions to the expression boundary before deallocating. The runtime bootstrap supplies array/aligned/nothrow/placement signatures and new handlers in translated C++, backed by a per-owner NativeMemory ledger.

See `backend/tests/initialization_allocation_verify.py` for native differential traces and managed lifecycle/concurrency checks. These supersede earlier pending statements for dynamic initialization, local-static guards and bootstrap allocation; complete libc/native resource ownership remains pending.
