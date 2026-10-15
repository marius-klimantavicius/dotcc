The accepted public inventory and actual production declarations establish
availability for the next T4 API slice. The exact facts and selected signatures
are in [availability.json](../tests/SqliteExtendedApis/availability.json); no new
object database scan, reflection or model bypass is involved.

The first finite slice is
[SqliteExtendedApis](../tests/SqliteExtendedApis/README.md): carray INT32 static
and INT64 transient values, custom v2 destructor/context release at clear,
finalize and failed bind, normalized statement execution, table-column
metadata, immediate primary/extended errors and rollback/recovery. Its pinned
native oracle passed; generated JIT/NativeAOT and reproducible differential
evidence remain pending. This does not close broader carray datatypes,
allocation/I/O failures or every core error/ownership contract.

All five snapshot routines have required retained bodies and actual generated
declarations: get, open, free, cmp and recover. The selected snapshot record is
size48/alignment1. A valid get needs non-autocommit WAL mode, no write
transaction, and a committed transaction in the current WAL. Opening a snapshot
also needs the connection to know its WAL mode and no active statement if a
read transaction already exists. Age comparison is defined only for handles
from the same database since the same WAL file identity was last deleted.
Each successful get must be freed. Recover has its own retained-WAL/no-active-
read-transaction preconditions. A future lifecycle workload must preserve those
conditions explicitly; ordinary WAL visibility alone is not snapshot API
acceptance.

RBU is present in this particular amalgamation. Fourteen `sqlite3rbu_*`
required definitions are generated: open, vacuum, step, db, close, progress,
bp_progress, state, savestate, create_vfs, destroy_vfs, rename_handler,
temp_size_limit and temp_size. The pinned source embeds the RBU extension and
its API documentation beginning around sqlite3.c226977. This is stronger
evidence than the compile definition alone; native export/execution acceptance
for RBU is still separate.

RBU updates require a target outside WAL mode. The update database must contain
the documented data tables/control fields; each open must have a matching
close. Step performs finite b-tree operations; close before completion saves
state for reopening with the same state path. Intervening external writes to
a suspended target can make resumption BUSY. The future fixture must therefore
use fresh rollback-journal artifacts, controlled stages and an explicit
close/reopen boundary, rather than reusing the WAL stateful database or
claiming the row unavailable.

Snapshot, RBU, session/preupdate/changeset/backup interactions and all other
unclosed acceptance rows remain explicit pending work. Borrowed-string copying,
Cdecl callback exception containment and the finite successful native slice
do not imply general thread, failure, provider or enabled-feature acceptance.

The frozen extended-API gate now passes all five measured native/JIT/NativeAOT
phases. [Receipt](../acceptance/sqlite/extended-apis-report.json) pins sources,
workload, public/actual API signatures, native oracle, commands and loaded
libraries. All four generated case arrays compare exactly with native;
connections close, pinned files remain unchanged, three destructor notifications
match with callbackFailed=false, and no native SQLite library is loaded by
generated execution. NativeAOT explicitly pins clang 18; publish is 72.55 seconds,
sampled process-tree peak 3.115 GB. Short-run sampling is not a startup peak;
waited kernel RSS is retained separately.

The raw availability/report snapshots retain their original probe/remaining-area
status. They are not retroactively rewritten: snapshot/RBU and prior session/
preupdate/backup execution are independently established by the current
[T4 ledger](T4-PROGRESS.md). Availability facts and signature validation remain
distinct from executed contract variants. No claim extends carray beyond the
exercised integer/ownership variants or closes broader failure/concurrency rows.
