# Owned C++ exception and RTTI runtime boundary

This is the implementation contract for the object pipeline, not a statement
that C++ exception translation is complete. SQLite already executes through
the object pipeline. DuckDB still needs class subobjects, generated ABI entry
points, construction/destruction scheduling and C++ EH. Runtime functions alone
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

## Independent-TU acceptance

`object-linker/tests/cpp_linked_exceptions_verify.py` compiles provider, bridge
and consumer separately, retains stock-C lowering for each, and then attempts
owned packaging/linking. Normal mode requires runnable managed output;
`--allow-export-rejection` only records the current implementation gates.

The fixture checks source integer/class/enum distinctions, anonymous-namespace
types that must remain TU-local, cross-TU adjusted pointer rethrow, class
destruction, multiple-inheritance dynamic casts, null pointer conversion,
function-pointer noexcept conversion and multi-level pointer qualification.
Native O0/O2 pass. Native025/managed020 rejects provider base/subobject layout
and bridge/consumer destruction obligations. The retained receipt is
`dotobj-linked-eh-acceptance-001/results.json` in the campaign directory; there
is no managed execution claim for this fixture yet.

After these operations execute, reuse the existing libc++ exception-pointer,
uncaught-count, foreign-exception and owner-lifetime fixtures. Their successful
direct-backend runs are not evidence for the owned-object pipeline.
