T4 is active. The unchanged full-feature SQLite object has production publication,
JIT and NativeAOT evidence; no native SQLite application serves generated calls.
The required matrix is in [ACCEPTANCE.md](ACCEPTANCE.md). Current finite evidence:

| Acceptance row | Verified execution | Concrete work remaining |
| --- | --- | --- |
| Core API | Open/close, prepared values, bind/reset/clear/finalize; exact blob destructor releases | Extended-result/error recovery and additional ownership API slice in progress |
| SQL/transactions | Original 43 SQL statements plus expanded constraints, cascade, triggers, savepoints, UPSERT and rollback | Controlled failure recovery in separate fixture |
| Persistence | Close/reopen and both native/generated closed-file interchange directions in JIT/AOT | Broader failure/recovery is separate |
| WAL/locking | Two live connections: pinned reader, writer commit, BUSY, rollback, checkpoint | Explicit cross-process contention and busy callback; snapshot API generated proof in progress |
| JSON/JSONB | Mutation, remove/patch, table traversal, NULL/missing/types, validity and binary round trips | No additional SQL prerequisite; wider behavior is outside finite proof |
| FTS3/4/5 | Tokenization/stemming/diacritics, matching/ranking/snippet/highlight, update/delete/rebuild/optimize | Fixture tokenizer/callback API interface |
| RTree/Geopoly | Spatial insert/update/delete/predicates and integrity | No additional prerequisite for finite SQL row |
| Session/preupdate/RBU | Preupdate old/new/order/unregister; changeset apply/OMIT conflicts; incremental backup | RBU save/close/reopen/completion passes native/JIT/AOT; failure variants are separate |
| Inspection | dbstat/dbpage/stmt/bytecode deterministic invariants | No additional prerequisite for finite SQL row |
| Math/metadata/STAT4 | Exact math/domain values, metadata pragmas, histogram/count invariants | Column-metadata C API generated proof in progress |
| Snapshot/carray/normalize | Snapshot same-WAL lifecycle passes native/JIT/AOT; carray/normalize native+JIT pass | Carray/normalize NativeAOT proof in progress |
| Threading/unlock notify | Bounded host foreign-thread/reentry protocol proof | SQLite shared-cache/unlock-notify and permitted concurrent API use |
| Runtime boundaries | 103 exact required selections: 88 hash-pinned candidate host bindings + 15 runtime intrinsics; targeted errno/loader/callback/vararg/storage proofs | Complete selected-provider audit against exercised T4 paths and explicit remaining variants |
| Failure paths | Constraint/rollback native proof and expected BUSY finalize behavior | Generated extended errors; controlled allocation/I/O failure cleanup and recovery |
| Repeat/restart | Original deterministic fixture generation; production/candidate four sources identical | Full input real prepublication cancellation passes; same-request fresh-process comparison running |
| Readability | T2 narrow review passes; full SQLite names/structure reviewed provisionally | Representative full output review and any necessary narrow cleanup |

Frozen receipts:

- [Production execution](../acceptance/sqlite/generated-production-report.json)
- [Persistence and interchange](../acceptance/sqlite/stateful-report.json)
- [Ownership, preupdate, session and backup](../acceptance/sqlite/api-callbacks-report.json)
- [Expanded SQL features](../acceptance/sqlite/sql-features-report.json)
- [Snapshot and RBU lifecycles](../acceptance/sqlite/maintenance-report.json)

Extended-API NativeAOT work remains in progress; native/JIT results alone do not
close that deployment gate. Successful finite rows do not establish
the entire upstream suite or every input/provider variant. Keep remaining work
specific rather than relabeling completed execution as an open generic matrix.
