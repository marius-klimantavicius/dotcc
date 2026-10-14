# Object format and reader admission

Final genuine-input evidence and measurements: [REAL-INPUTS.md](REAL-INPUTS.md).

The current producer emits public format 2.0 with 251 tables and 39,145
append-only stable kind IDs. Its exact framed SQL+catalog fingerprint is
`3e818d17da55433edff5f5c9b035c58110e953371c3c6fa09cb99d0f6f84b5f6`.
The reader admits exactly these version/fingerprint pairs:

| Version | Required normalized schema fingerprint |
| --- | --- |
| 2.0 | `3e818d17da55433edff5f5c9b035c58110e953371c3c6fa09cb99d0f6f84b5f6` |
| 1.0 | `4bb94e5feda94e85fe4b6fe4e8eb5f3494f3b112d14a56dc71e6b069861a2559` |

Format 2.0 makes `base_subobject.most_derived_type_id` nullable. Stock EDG
retains unresolved constructor-initializer base placeholders in template prototypes
with an actual null `derived_class` pointer (`il.c:5019–5028`,
`decl_inits.c:7541–7572`). The base identity, base type and constructor-initializer
association remain present; `BaseSubobject.MostDerivedType` exposes that absence.
The reader never substitutes an enclosing class. This changes the earlier public
non-null guarantee, so the producer uses a new major version. The catalog and all
other column contracts remain unchanged. See [the exact contract comparison](nullable-base-major2-contract.json)
and [DuckDB evidence](DUCKDB.md).

The reader also accepts genuine objects at the published 1.0 boundary without
migration. Older development fingerprints with the same numeric version remain
incompatible. It checks a complete snapshot header, an exact admitted pair,
required and enabled normalized_semantic_schema_v1 capability, compatible catalog
names/IDs and every enabled required capability. Supported required names are:

- normalized_semantic_graph_v1
- exact_target_values_v1
- edg_partial_eh_v1
- source_semantic_graph_v1
- normalized_semantic_schema_v1

An artifact records actual resolved compiler/target/runtime capabilities and source
revision; these are informational unless explicitly required. An ABI name is not
a claim that every target feature is representable. Source-semantic and lowered
execution graphs retain their actual distinct operation roles and availability.

Future supported minor contracts must be additive and explicitly admitted/tested
with genuine artifacts. Incompatible semantics require a major change. The current
reader may reject an unknown future minor. There is no migration or compatibility
promise for old development objects or arbitrary future contracts.

One artifact contains one independent translation unit. Reopening or copying the
same immutable artifact preserves ArtifactIdentity; new compilation creates a new
identity even for identical names/source. Entity identity is artifact+entity, and
canonical object instances are local to a reader. RoslynWorkspace holds independent
artifacts without linking, resolving duplicate declarations or merging identities.

Storage is normalized STRICT SQLite with typed scalar/foreign-key columns and
explicit ordered relations. Exact target integers use decimal text; string units
retain target bytes/character units and exact floating values retain their semantic
parts. SQL has no opaque semantic BLOB or host floating REAL fallback. Kind IDs
remain reserved after placeholder retirement, and each object stores only used
kind rows. Defaults and nullability follow retained EDG facts; optional unknown
associations remain null/empty without recovery or guessed fallback.

The writer constructs an unpublished temporary database, checks all declared
foreign keys before and after the complete header, commits, restores and verifies
foreign-key enforcement, then atomically publishes. A failure discards the private
output and preserves any existing destination. This explicit private FK-disabled
construction policy is a measured backend optimization, not a relaxed published
object contract.

See [profiles](PROFILES.md) for accepted modes, [closure](CLOSURE.md) for exact
retained-source and stock input limits, and [the managed guide](../managed/README.md)
for ordinary property navigation. The library
does not implement a translator, C++ semantic resolver or cross-TU linker.

Current retained-data scope is summarized in [limits](LIMITS.md).
