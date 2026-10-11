# Relational semantic program restart

This contract supersedes the raw-EDG-graph direction in the 2026-10-11 authority
plan. EDG remains the sole authority for C/C++ semantics.

The object is a source-shaped, resolved program graph. Expressions carry their
EDG-selected type, value category, selected declaration and ordered children.
Statements retain structured control flow. Named types retain field layout,
qualifiers, relationships and template instantiations. Initialization, lifetime
and exception behavior must be explicit semantic relationships, not reconstructed
by the C# consumer. Source locations and macro APIs belong in this model.

Storage is ordinary SQLite tables, integer references and scalar columns. No JSON
transport or JSON semantic payloads; no custom binary field blocks. Literal data
may eventually use byte storage; semantic relationships may not. Each entity has
one identity per TU. No parser state, broad frontend object walk, or history of
unchanged observations. A necessary source/lowered correspondence is an explicit
edge between program entities, not a snapshot of surrounding compiler internals.

The native exporter writes SQLite directly with prepared statements. It captures
program bodies before EDG lowering can erase source type/category/control-flow
information. Later needed ABI facts have explicit columns/relationships. The
managed linker uses recorded linkage identities; the generator trusts EDG's
semantics. Missing data or unsupported emission is an explicit error, never an
opaque fallback or an attempt to re-run overload/template/lifetime analysis.

## Implementation boundary

New implementation: `object-linker/relational/`, `backend/dot_gen_semantic.h` and
`backend/dot_gen_semantic_sqlite.h`. Existing backend/runtime algorithms remain
available for porting. Preserve legacy code and artifacts for comparison; do not
resume the frozen 2,635-unit export run. `EDG_DOT_SEMANTIC` selects the new direct
SQLite path; the old `EDG_DOT_OBJECT` path is not used by it.

First acceptance is two original C++ source files containing a named record,
member call, instantiated template, cross-TU function call and structured loops.
Inspect their SQL rows, link and emit C#, compare native and managed execution,
and publish the original source/database/generated source together for review.
This slice is not full C++ support. Next, port explicit construction/destruction
and exception semantics, test normal and exceptional paths, then a bounded real
DuckDB subset. Measure per-table size and frontend cost before widening the run.

Required next coverage remains: lifetime/exception execution, full source type
variants, record/template declaration relationships, macros, statics/TLS,
initialization/allocation, ABI-sensitive C++ operations, provider/runtime closure
and full-project translation. Unsupported constructs must fail visibly. No
successful subset is evidence those requirements are already implemented.
