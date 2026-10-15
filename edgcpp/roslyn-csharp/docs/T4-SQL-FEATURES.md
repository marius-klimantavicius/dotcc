The expanded configured SQL matrix passes all 12 cases / 129 statements in the
separate pinned native reference and translated JIT/NativeAOT. Six measured
phases pass, including the original 43-statement mode compatibility check.
See [workload and exact scope](../tests/SqliteFeatureMatrix/README.md) and
[receipt](../acceptance/sqlite/sql-features-report.json).

This finite evidence covers JSON/JSONB mutation, typed values and tree traversal;
FTS3/4/5 tokenization, ranking/snippet/highlight and update/delete; RTree/Geopoly
spatial updates/results; deterministic inspection invariants; math domains and
values; schema metadata pragmas; STAT4 histogram invariants; and constraints,
savepoints, cascade, UPSERT, rollback and subsequent successful writes.
Native inspection/STAT4 predicates all report their intended true invariants;
invalid JSON predicates distinguish strict/JSON5/invalid bytes as observed.
Exact binary64 values are preserved in the native/generated comparisons.

The optional test-only admission file pins this distinct workload hash and exact
counts. Existing three/four-argument initial mode retains its original identity
and result status. The first minimal snapshot build failed at missing validator
path and is preserved; the fresh snapshot reuses the exact production validator.
NativeAOT explicitly pins clang 18, publishes in 72.30 seconds with sampled
process-tree peak 3.348 GB. All generated library maps exclude native SQLite.
Short execution samples do not establish startup peak; waited RSS is separate.

Carray/normalized SQL/column-metadata C APIs and extended errors, snapshot and RBU
lifecycle, concurrent process/shared-cache/unlock-notify behavior, tokenizer
interfaces, controlled allocation/I/O failures, selected provider contracts and
readability/restart evidence remain separate acceptance rows. This finite matrix
is not an upstream-suite or whole-T4 completion claim.
