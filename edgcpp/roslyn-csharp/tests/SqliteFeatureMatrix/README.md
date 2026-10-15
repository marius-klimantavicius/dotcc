# Expanded configured SQL feature matrix

`sql-features.json` uses the existing `workloadVersion: 1` typed-row schema. Each
case starts with a fresh in-memory database. Every statement is intended to
complete successfully with ROW/DONE; it does not require an expected-error mode.
The exact native observations are the oracle. There are no predicted row constants
or substituted query results in the workload.

The pinned workload contains 12 cases and 129 statements. Its SHA256 is
`2abaf5bcd0d933561d251747c4cc1658f79014fb16a6ddf41d164af9e9a46b99`.

The configuration is the unchanged SQLite 3.54.0 object and all 19 definitions in
the accepted native build recipe. This adds a finite SQL activation slice:

| Case | Observable coverage |
| --- | --- |
| JSON | Insert/replace/set/remove/patch, JSON5/strict validity, invalid inputs through `json_valid`, NULL/missing/scalar types, `json_each` and `json_tree` leaves |
| JSONB | Binary round trips, mutation/removal/patch, strict and superficial validity, table-valued values |
| FTS3 | Porter stemming, offsets, snippet, matchinfo, update and delete |
| FTS4 | Unicode/diacritic tokenization, offsets, snippet, matchinfo, update/delete/rebuild |
| FTS5 | Unicode tokenization, prefix/NEAR matches, bm25, snippet/highlight, update/delete/optimize |
| RTree | Rectangle intersection before/after update/delete, coordinates and integrity |
| Geopoly | Area, point containment, overlap/within, indexed overlap queries, shape update/delete and representation |
| Inspection | dbstat page/cell invariants, dbpage size/header, statement metadata invariants, bytecode presence |
| Math | Values, rounding, conversions, domain results and compile-option presence |
| SQL metadata | Generated columns, declared defaults/nullability, index/collation and foreign-key pragma rows |
| STAT4 | Nonempty bounded histogram samples and count invariants, ANALYZE row estimates, deterministic grouped/filtered results |
| Transactions | Primary-key/CHECK IGNORE paths, foreign-key cascade, triggers, nested savepoints/rollback, UPSERT, REPLACE, subsequent valid writes and integrity |

Rows use explicit ordering where their order matters. The workload omits unstable
statement memory/step statistics, pointer-bearing bytecode fields, query-plan
text, timestamps and random values. STAT4 compares bounded/nonempty/numeric
invariants instead of selected sample contents. Geometry uses exactly represented
small coordinates. Math/bm25 still preserve exact binary64 observations through
the shared protocol; they are not rounded by the test runner.

The existing separate native runner can consume this schema unchanged:

```sh
python3 edgcpp/roslyn-csharp/acceptance/sqlite/run-native-sql.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --workload edgcpp/roslyn-csharp/tests/SqliteFeatureMatrix/sql-features.json \
  --output /absolute/new/native-sql-features.json
```

The coordinator schedules all runs. The frozen initial generated harness still
pins its original 43-statement workload hash/count; this file must be explicitly
admitted by the coordinator's generic SQL runner before generated execution.
Do not alter or relabel the initial proof, manufacture an old workload receipt,
or use a native SQLite binding in the generated process. No execution is claimed
by this preparation.

Column-metadata C APIs, callback/tokenizer interfaces, carray, normalization,
snapshot, RBU, shared-cache/unlock-notify/threads, failure injection and broader
provider contracts remain separate. Successful IGNORE/UPSERT/savepoint paths do
not replace required extended-error, allocation or I/O failure tests.

This finite matrix now passes the separate native oracle and generated JIT and
NativeAOT. The [receipt](../../acceptance/sqlite/sql-features-report.json) pins
six measured phases, exact source/admission/workload hashes, the native build
receipt, production completion, loaded libraries and all result reports. All
12 cases / 129 statements compare exactly. NativeAOT explicitly uses clang 18;
no generated process loads native SQLite. The original 43-statement mode also
passes a relevant JIT compatibility check with the updated shared harness.

[admission.json](admission.json) selects this exact hash and count. Pass it as
the fifth argument to the shared `SqliteGenerated` harness after the separate
native report. This mode uses a distinct result status and records admission
provenance. Build commands and property values are retained in the receipt.
The first minimal-snapshot build omitted the adjacent validator and is preserved;
the project now allows selecting the exact validator path pinned by the reused
production API report. No translator, emitted source or runtime code changed.
