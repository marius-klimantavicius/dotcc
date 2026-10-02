# Public type_info storage adapter

The backend represents compiler RTTI by stable eight-byte tokens, independent
of native C++ ABI implementation classes. Libc++18's inline `name`, equality,
ordering and hashing operations instead read the public `std::type_info`
name-pointer field at offset eight. Treating a token as the complete public
object would read beyond its storage.

`dot_gen_typeinfo.h::typeinfo_field` recognizes the exact public name field
(`__type_name`, or EDG's internal prefix spelling `__name`) and the generated
vtable-pointer field. Name access on a compiler token maps to a separate,
lazily initialized pinned array of name pointers. Each pointer targets the
existing pinned, deduplicated RTTI-name bytes. These are immutable program
metadata shared across owners, with stable addresses under compacting GC.
The adapter preserves lvalue behavior instead of parsing or rewriting the
library's inline function bodies.

User-created objects derived from `std::type_info` retain their own ordinary
native object layout. Their name and vtable fields use the object's actual
storage, so protected constructors, custom names and virtual destruction work.
Virtual access to a statically known opaque compiler token is rejected during
translation; unknown receivers retain a runtime guard against token access. The
native ABI metadata-class hierarchy is not materialized by this adapter.
The matching/dynamic-cast runtime continues to use its separate typed graph.

The C backend's ordinary field emission and its two-field public RTTI prefix
provide the layout reference; this change does not modify EDG lowering.
The `lib_src/vec_newdel.c` overflow path also calls
`__cxa_throw_bad_array_new_length`. Array allocation now accepts a translated
definition of that exact entry point as well as the existing optional EDG
bootstrap helper, validating its nonvariadic `void()` signature.

`tests/type_info_fields_verify.py` passes nine checks in
`artifacts/campaign/type-info-fields-translated-005/`: original EDG C behavior,
static/instance JIT and NativeAOT, stable compiler-name/token identity, forced
GC, and separately constructed derived objects with virtual destruction.
An explicitly synthetic, pragma-designated public prefix without virtual
methods or `typeid` also exercises the zero-token emission path in both modes
and runtimes. That edge-case fixture is not claimed to be a standard-library
implementation. Linux x64 was executed; other platform execution remains open.
The existing 12-check RTTI suite also passes in `type-info-regression-001/`,
including its original rejection of virtual destruction on known compiler
metadata. No negative expectation was relaxed to admit the field adapter.

Libc++ seed consumers still require the rest of their out-of-line providers.
These focused checks alone do not establish complete libc++ acceptance.
