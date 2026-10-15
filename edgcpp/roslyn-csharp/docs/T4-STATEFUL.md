The first stateful T4 slice passes native, generated JIT and NativeAOT execution.
The compact [receipt](../acceptance/sqlite/stateful-report.json) records all 15
measured phases and 13 observation reports; all passed with unchanged pinned
inputs and closed connections. The unchanged full-feature input and existing 23-API scalar/pointer
harness surface remain the target; this workload does not reduce features or
replace generated algorithms with native application code.

The finite action sequence covers commit/close/reopen persistence, typed
integer/text/real/blob/null rows, a retained reader transaction snapshot,
committed versus uncommitted visibility, a competing writer's immediate BUSY
result, rollback recovery, a successful TRUNCATE checkpoint and subsequent
reopen/integrity observations. Ordered SQL actions are explicit connection
barriers. Locking scope is two connections within one process; simultaneous
cross-process lock contention is still open.

Separate-process interchange passes in both directions for JIT and NativeAOT. A completed
producer closes all connections and checkpoints before exit. Only then does
the coordinator let the receiving process copy the exact hashed standalone
database into a new artifact directory. The receiving engine verifies the
base rows, commits its distinctive row, checkpoints and closes; the original
engine verifies the returned state. The runner checks recorded closure and
absent sidecars without claiming those checks independently establish global
file closure. Semantics, not arbitrary file-byte equality, decide acceptance.

The native runner loads only the pinned oracle library whose build receipt
SHA-256 is `c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435`.
Its separate process cannot service generated calls. The workload SHA-256 is
`8f9cdc8262ac0c8b29f73fd6a375fd693abf0311dfc4335493b8e5c02ef75a8a`.
All expected typed observations, terminal step/finalize codes and bounded row/
column extents are explicit. The native runner preserves actual observations
before assertion, including checkpoint results and expected BUSY. Generated JIT/NativeAOT observations compare exactly with the separate native
oracle: 40 stateful actions, 13 receiving append actions and 5 return verification
actions. Generated loaded-library inventories contain no native SQLite.

Native attempt1 preserved all 40 expected action observations, including the
BUSY step/finalize5 protocol and checkpoint0/0/0, but remained incomplete because
its final read-only connection left a zero-byte WAL and a 32768-byte SHM file.
The report at `t4/native-stateful-1/stateful-report.json` under the artifact root
has SHA-256 `142b73627377401d5d263ac5dabd3db4e869d251b811155d39ad304c62e73ce2`.
The pinned source's `sqlite3WalCheckpoint` rejects a read-only WAL connection;
`sqlite3WalClose` deletes the sidecars only after a successful closing
checkpoint. Final verification opens now use readwrite-without-create while
executing the same read-only SQL, leaving normal cleanup to SQLite. All modes
retain the sidecar absence guard; no file is deleted by the runner. The original
attempt and its workload identity remain preserved in that report.

| T4 acceptance row | Scope remaining after this prepared slice |
| --- | --- |
| Core API | Destructor callbacks, broader ownership/lifetime and extended-error coverage |
| SQL/transactions | Broader constraints, joins, savepoints, statement reuse and transaction-error recovery |
| Persistence | Initial close/reopen and both interchange directions pass; broader workloads remain |
| WAL/locking | Initial two-connection snapshot/BUSY/rollback/checkpoint passes; simultaneous cross-process schedules, active-reader checkpoint cases and busy callbacks remain |
| JSON/JSONB | Full mutation, table-valued, invalid-input and round-trip matrix beyond initial smoke |
| FTS3/4/5 | Full tokenization, ranking/highlight and callback matrix beyond initial smoke |
| RTree/Geopoly | Broader update and spatial-result matrix beyond initial smoke |
| Session/preupdate/RBU | Event values/order, changesets/conflicts, staged updates and backup interaction |
| Inspection modules | dbstat/dbpage/stmt/bytecode invariants beyond initial smoke |
| Math/metadata/STAT4 | Broader math behavior, column metadata and statistics correctness |
| Snapshot/carray/normalize | Snapshot API lifecycle, carray ownership and normalized-SQL API behavior |
| Threading/unlock notify | Shared-cache registration/callback conditions and allowed concurrent use |
| Runtime boundaries | Broader provider/error/TLS/time/randomness and callback contracts |
| Failure paths | Controlled allocation/I/O failure, cleanup, rollback and recovery |

Provider auditing, representative readability review, measured resources and
clean restart/repeat generation are still required alongside these behavioral
rows. This slice adds no callbacks, faults, application-native fallback or
feature-matrix completion claim.

The NativeAOT publish passes in 71.21 seconds with sampled process-tree RSS
3.305 GB. Its command set `CC=clang-18`, but the SDK linker property was not
explicitly selected and the link command was not captured. This receipt makes
no pinned clang-18 claim: the SDK defaults to unversioned clang then gcc fallback,
unversioned clang is absent, and gcc 13 is installed. The earlier T3 proof pins
its linker explicitly; future necessary harness builds do so as well. A failed
standalone SDK metadata probe is preserved separately and is not an application
failure. Sampling limitations and kernel waited RSS are retained in each phase.

Frozen reports retain their original broad `remainingT4Areas` text, including
execution/interchange work now completed here; this ledger and the compact
receipt distinguish the current remaining scope. T4 as a whole remains open.
