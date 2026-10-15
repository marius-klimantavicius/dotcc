T4 is active. The unchanged full-feature SQLite object has production publication,
JIT and NativeAOT evidence; no native SQLite application serves generated calls.
The required matrix is in [ACCEPTANCE.md](ACCEPTANCE.md). Current finite evidence:

| Acceptance row | Verified execution | Concrete work remaining |
| --- | --- | --- |
| Core API | Open/close, prepared values, bind/reset/clear/finalize; exact blob destructor releases | Immediate primary/extended errors and rollback/recovery pass; controlled failure proof remains |
| SQL/transactions | Original 43 SQL statements plus expanded constraints, cascade, triggers, savepoints, UPSERT and rollback | Controlled failure recovery in separate fixture |
| Persistence | Close/reopen and both native/generated closed-file interchange directions in JIT/AOT | Broader failure/recovery is separate |
| WAL/locking | Two live connections: pinned reader, writer commit, BUSY, rollback, checkpoint | Explicit two-process WAL/BUSY/checkpoint/visibility and snapshot APIs also pass JIT/AOT |
| JSON/JSONB | Mutation, remove/patch, table traversal, NULL/missing/types, validity and binary round trips | No additional SQL prerequisite; wider behavior is outside finite proof |
| FTS3/4/5 | Tokenization/stemming/diacritics, matching/ranking/snippet/highlight, update/delete/rebuild/optimize | Fixture tokenizer/callback API interface |
| RTree/Geopoly | Spatial insert/update/delete/predicates and integrity | No additional prerequisite for finite SQL row |
| Session/preupdate/RBU | Preupdate old/new/order/unregister; changeset apply/OMIT conflicts; incremental backup | RBU save/close/reopen/completion passes native/JIT/AOT; failure variants are separate |
| Inspection | dbstat/dbpage/stmt/bytecode deterministic invariants | No additional prerequisite for finite SQL row |
| Math/metadata/STAT4 | Exact math/domain values, metadata pragmas, histogram/count invariants | Column-metadata C API also passes JIT/AOT |
| Snapshot/carray/normalize | Snapshot same-WAL lifecycle, carray/normalize APIs pass native/JIT/AOT | Wider carray datatype/ownership variants outside finite proof |
| Threading/unlock notify | Bounded host foreign-thread/reentry protocol proof | SQLite shared-cache/unlock-notify and two barrier-controlled threaded readers also pass JIT/AOT |
| Runtime boundaries | 103 exact required selections: 88 hash-pinned candidate host bindings + 15 runtime intrinsics; targeted errno/loader/callback/vararg/storage proofs | Complete selected-provider audit against exercised T4 paths and explicit remaining variants |
| Failure paths | Constraint/rollback native proof and expected BUSY finalize behavior | Controlled heap-refusal/pager-IOERR native proof passes; generated proof next |
| Repeat/restart | Original deterministic fixture generation; production/candidate four sources identical | Full input real cancellation and same-request fresh-process restart pass; all four source files byte-identical |
| Readability | T2 narrow review passes; full SQLite names/structure reviewed provisionally | Bounded full SQLite readability review passes focused and full new publication/JIT/AOT plus129SQL regression; cosmetic debt documented |

Frozen receipts:

- [Production execution](../acceptance/sqlite/generated-production-report.json)
- [Persistence and interchange](../acceptance/sqlite/stateful-report.json)
- [Ownership, preupdate, session and backup](../acceptance/sqlite/api-callbacks-report.json)
- [Expanded SQL features](../acceptance/sqlite/sql-features-report.json)
- [Snapshot and RBU lifecycles](../acceptance/sqlite/maintenance-report.json)
- [Repeat generation and cancellation/restart](../acceptance/sqlite/restart-report.json)
- [Carray, normalization, metadata and extended errors](../acceptance/sqlite/extended-apis-report.json)
- [Readable complete publication and final SQL regression](../acceptance/sqlite/readable-production-audit.json)
- [Cross-process WAL, unlock notification and threaded reads](../acceptance/sqlite/concurrency-report.json)

Controlled failure and concurrency native-only proofs are preparatory; their
generated evidence remains required. Successful finite rows do not establish
the entire upstream suite or every input/provider variant. Keep remaining work
specific rather than relabeling completed execution as an open generic matrix.
