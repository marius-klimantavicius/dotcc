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
