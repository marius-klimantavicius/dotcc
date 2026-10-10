# Original template type and constant metadata

Profile 42 keeps source-only dependent types independent of physical execution
layout. A dependent class specialization references its original prototype and
ordered typed arguments through `source_record_instantiations` and
`source_template_arguments`. Its identity is reconstructed from the prototype
and argument signatures, including qualifiers, nested owner types and original
parameter declaration provenance. Equal parameter names in unrelated templates
do not identify the same parameter. Missing arguments cannot be repaired with
execution-layout guesses.

`source_semantic_constants` and ordered operand rows preserve integer values,
original non-type parameters, supported typed operations, expression wrappers
and dependent member references. A member retains its owner source type, name
and result source type, including an unresolved dependent result. These are
semantic nodes, not emitted C# or strings substituted into executable code.
The reader checks graph ownership, cycles, arity, integer range and reconstructed
identities. Unsupported expression families remain explicit export failures.

Every source array distinguishes a constant bound, an unspecified bound and a
dependent bound. `source_array_bounds` references the original dependent
constant graph; dependent arrays do not acquire a fabricated execution extent.
Casts and wrappers remain present, so `T::extent` used as an array size can
retain an expression/cast/member chain rather than losing its conversion to
the array-bound type. Erasing a bound cannot silently turn it into `[]`.

The focused fixtures cover distinct parameter binders, nested/qualified class
arguments, symbolic arithmetic bounds, arrays of dependent arrays, unsized
arrays, dependent trait members and malformed metadata. Source files are hidden
before managed verification and generation. These additions do not yet represent
all C++ template constructs: further dependent source expression families,
partial specializations, template-template arguments and packs still require
explicit owned representations where encountered.

## Profile 43 additions

Typed source operands and compiler type-trait nodes retain the exact original
operator opcode/name and ordered type arguments. The profile checks 101
all-type builtin operator contracts, with their result and arity rules; supported
operator names do not imply every possible type-argument family is supported.
Size/alignment queries retain type-versus-expression operands and standard
versus GNU alignment form. These source-only descriptions do not evaluate
uninstantiated templates or fabricate concrete values.

Nonreal dependent member templates are a distinct instantiation relation. EDG
deliberately supplies no ordinary class prototype for them. The relation keeps
the exact enclosing source type, member name, ordered arguments and unknown-base
qualification flags, following stock template equivalence. A fabricated ordinary
prototype is rejected. Template-template parameters, encountered next in libc++
node handles, require a separate template reference and formal-parameter graph.

## Profile 44 additions

`source_templates` stores original template-template parameter references and
`source_template_parameters` stores their complete ordered formal lists. Type,
non-type, and nested-template formals retain their typed placeholder payloads,
defaults, pack/abbreviated flags, and declaration locations. Template-valued
arguments reference this graph. A `template-parameter` instantiation points to
its original template reference rather than inventing a concrete class prototype.
Canonical identities derive from the declaration binder, complete formal graph,
and ordered argument identities; database row IDs are never identity components.

EDG deliberately assigns `NO_NESTING_DEPTH` (zero) to inner template-template
formals (`il_def.h:4486`, `templates.c:33023`). Zero is retained exactly and is
accepted only with an owned formal declaration and matching ordinal/parameter
position. Ordinary source parameters still need their original positive nesting
depth. Missing formals, substituted placeholders, changed defaults, missing
arguments, and mismatched template references are rejected after resealing the
object digest. Stock template equivalence is documented in `templates.c:8174`
and `templates.c:8689`; original formal/default payloads are in `il_def.h:17137`.

Native111/managed121 passed **45 checks**, including native O0/O2, source-hidden
verification/linking, managed execution, NativeAOT execution, and eight metadata
negative cases. The retained receipt is
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-source-template-parameters-v44-111-002/results.json`.
This proves the parameter-reference family used by libc++ node handles; concrete
template references, substituted-template mappings, constraints, and argument
packs still have explicit gates until their original semantics are represented.

## Profile 45 nested declarations

A nonreal nested type retains both its dependent member view and its exact
original declaration through `originalNestedTypeId`. The separate
`source_nested_type_declarations` relation records the original enclosing type;
it is distinct from the anonymous-field `source_record_owners` relation. Named
nested record and enum identities incorporate that enclosing semantic identity,
and the verifier checks the declaration family, owner, name and dependent view.
Original prototype/dependent flags remain unchanged. A typedef declaration may
retain its own source type family instead of being cast to a record.

Native112/managed123 passed **28 checks** (native O0/O2, hidden-source
verification/linking, JIT, NativeAOT and four resealed negatives) at
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-source-nested-types-v45-112-004/results.json`.
The real driver was `duckdb/common/arena_linked_list.hpp:94`, whose `Node` has an
original declaration and a nonreal dependent view; stock
`lookup.c:create_nonreal_version_of_nested_type` explicitly describes those two
forms. The independent defined-enum reducer remains retained as
`duckdb_source_nested_enum_definition.cpp`: its prototype enumerator values need
a typed semantic expression, and are not silently treated as resolved integers.
