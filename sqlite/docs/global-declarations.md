# File-scope redeclarations

SQLite declares `sqlite3_temp_directory`, `sqlite3_data_directory`, and
`sqlite3WhereTrace` before their initialized definitions. These declarations
must designate one C object. The compiler now keeps one canonical symbol and
one storage entry for scalar and pointer declarations handled by
`BuildGlobalDecls`.

A declaration without an initializer is tentative unless it is `extern`.
Repeated tentative declarations do not allocate additional storage. A later
initializer replaces the tentative storage entry, and subsequent tentative
or `extern` declarations retain that initializer. An `extern` declaration
with an initializer is itself a definition. Conflicting types and a second
initialized definition produce diagnostics.

Preserving the symbol is essential as well as preserving the field: an
earlier function may already have taken the object's address. Reusing that
symbol retains the pointer-storage representation and every earlier reference.
Function pointer types compare structurally, so independently spelled but
compatible callback typedefs can redeclare the same object.

The `tentative-globals` functional fixture has a GCC-verified transcript and
exercises zero initialization, initialized definitions, `extern` initializers,
shared pointer address identity, static objects, and callback pointers.
`TentativeGlobalTests` checks conflicting definitions/types and field counts.
Baseline evidence is in ignored `sqlite/artifacts/tentative-globals/` and
`sqlite/artifacts/opaque-aggregates/unit-before.log`.

This increment covers scalar/pointer declarator lists. Array declarations and
existing positional aggregate initializer productions retain their separate
registration paths. The existing whole-program limitation for identical
header-defined static objects across multiple translation units remains:
those objects may share storage. This change retains that behavior only for
identical static declarations across different units; it does not hide a
second initialized scalar definition in the same unit.
