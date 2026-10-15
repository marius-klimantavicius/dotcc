T4 is CLOSED for the finite configured SQLite acceptance matrix on the pinned
Linux x64 GNU/Itanium/glibc profile, .NET 10/C# 14, JIT and NativeAOT.
[Consolidated acceptance](../acceptance/sqlite/t4-acceptance.json) records exact
input/publication provenance, every row and explicit limits. T5/T6 remain open.

| Required row | Passing finite evidence | Publication attribution |
| --- | --- | --- |
| Core API | Open/close, typed prepared values, bind/reset/clear/finalize, destructor ownership, immediate primary/extended errors and recovery | Original production; extended errors also source 11; controlled failures source 13 |
| SQL/transactions | Original 43 statements and expanded constraints/cascade/triggers/savepoints/UPSERT/rollback | Fresh source-13 JIT/AOT reruns include all 129 expanded statements |
| Persistence | Close/reopen and both native/generated closed-file interchange directions | Original source 11; source-13 concurrency adds close/reopen |
| WAL/locking | Two-connection snapshots/BUSY; real two-process writer contention, busy callback, pinned-reader checkpoints and visibility | Original stateful source 11; process concurrency source 13 |
| JSON/JSONB | Mutation/remove/patch, traversal, null/missing/types, validity and binary round trips | Original and readable source-13 JIT/AOT matrix |
| FTS3/4/5 | Tokenization/stemming/diacritics, ranking/snippet/highlight, lifecycle; custom FTS5 callbacks | SQL source 11 and 13; exact 84-event tokenizer source 13 |
| RTree/Geopoly | Insert/update/delete, spatial predicates and integrity | Original and readable source-13 matrix |
| Session/preupdate/RBU | Preupdate values/order/unregister, changesets/apply/conflicts, incremental backup; RBU save/close/reopen/completion | Original source 11 |
| Inspection | dbstat/dbpage/stmt/bytecode deterministic invariants | Original and readable source-13 matrix |
| Math/metadata/STAT4 | Math domains/values, metadata pragmas/C API, histogram/count invariants | SQL source 11 and 13; metadata C API source 11 |
| Snapshot/carray/normalize | Same-WAL snapshot get/open/cmp/recover/free; integer carray ownership/transient copy; normalized statement execution | Original source 11 |
| Threading/unlock notify | Actual shared-cache LOCKED/notification/retry; two barrier-controlled separate threaded connections | Readable source 13 |
| Runtime boundaries | Exact 103 selections: 88 pinned native bindings + 15 runtime intrinsics; focused errno/loader/callback/ABI proofs; finite clock/random-buffer/allocator contracts | Original focused receipts plus source-13 provider contracts |
| Failure paths | Heap-limit refusal with explicit rollback; injected pager commit IOERR with automatic rollback; cleanup, recovery and reopen | Readable source 13 |
| Repeat/restart | Real prepublication cancellation, retained incomplete stage, fresh-process same-request restart and four-file byte equality | Frozen translator source 12, original output format; no stage reuse |
| Readability/resources | Guarded scalar switch dispatch, pure discard removal, selected source review, frozen budgets and separate phase measurements | Focused and full readable source 13 |

Source 11 means the original accepted production output. Source 13 is the final
readable publication at `t4/sqlite-readable-generated-1`; its project/native ABI/
import-manifest bytes match the original. The runtime project remains immutable
snapshot 11. Earlier source-11 tests are not relabeled as source-13 runs.

Finite limits remain explicit: no upstream-suite/all-SQL/all-provider-variant
claim; 16 host targets plus one logical-errno substitution have exact focused
implementation evidence, while 71 other native selections are not individually
traced. Every selection and its contracts/provider/hash are reconciled. Random
quality, arbitrary clock adjustments, every allocator schedule, actual device
failure, other lock schedules, invalidated snapshots and wider carray/tokenizer/
RBU variants are outside this proof. Barrier-controlled threads permit overlap
without proving simultaneous in-engine execution on every scheduler run.

The fault-400 test injects existing pager commit IOERR and does not corrupt
objects or simulate an OS device. Generated applications depend on Generated and
Runtime, exclude the reader SQLite package and load no native SQLite application.
No cleanup of failed attempts, immutable inputs or incomplete stages is authorized.
