The new [SqliteFailures fixture](../tests/SqliteFailures/README.md) prepares two
finite native/generated failure contracts for the unchanged full-feature
SQLite object. The separate pinned native oracle passed. Generated execution
must use the new readable production CLI publication and its validated API
receipt; its JIT/NativeAOT proof is pending at this source-preparation point.
No native SQLite application body may serve generated calls.

The native report SHA-256 is
`9389da20b10cb2670577adcbb8054f63627a2640a9c8b50097c68b4155770905`
at `/mnt/storage/roslynobj-csharp-20261015/t4/native-failures-1/failures-report.json`.
The measured-run receipt SHA-256 is
`4014c7f76608d779302a4900bf9f043178372d21aa393ec9ff3433ebab6050a6`
at `/mnt/storage/roslynobj-csharp-20261015/t4/native-failures-run-1/run.json`.
The shared workload SHA-256 is
`f1cfaffcc508d27de1c7aedca10744b15a04300d06a25911e70666533a83d6bf`.

The recovery states are deliberately distinct:

| Native activation | Immediate failure | State after finalization | Recovery observed |
| --- | --- | --- | --- |
| Hard limit refuses the prepared randomblob allocation | step7, primary7, extended7, finalize7 | autocommit0; transaction is active | Explicit ROLLBACK restores row10; later insert20 succeeds |
| First armed pager fault400 aborts COMMIT | step10, primary10, extended10, finalize10 | autocommit1; rollback already occurred | Row30 remains; later transaction writes31 and persists through reopen/integrity_check |

The limit is set only after the demanding statement is prepared, using actual
SQLite memory_used plus a bounded headroom. The previous limit is restored in
`finally` before finalizing, including failed test paths. Live memory and cap
values are audit observations, since generated and native allocators can use
different amounts. They are not part of exact differential case equality.
The current native audit reports used83,504/cap149,040/prior0, with the prior
limit restored to0; this is finite evidence, not a universal refusal threshold.

The I/O hook is the pinned source's explicit
`if( sqlite3FaultSim(400) ) return SQLITE_IOERR;` in
sqlite3PagerCommitPhaseOne, around sqlite3.c65481. FAULT_INSTALL9 around
sqlite3.c192663 installs an int(int) callback and probes code0 immediately.
The test leaves that probe unarmed, arms only the subsequent COMMIT step,
returns1 only for the first armed400, and always disarms/uninstalls. Native
observed one probe0 and one activation400. This is injected pager-commit IOERR;
it establishes no actual OS, device or arbitrary VFS failure behavior.

The generated callback uses caught Cdecl UnmanagedCallersOnly code. Its local
adapter calls the actual retained variadic projection through the existing
SysV runtime pack, with captured natural pointer8/8, tag24/8, field offsets
0/4/8/16, GP offset8 and FP offset48. A new public-inventory/actual-source
validator checks selected signatures, builtin tag/array and callback facts,
then replays the existing production23 surface validator. The validator is
parameterized by actual completion and base API receipts; no old generated
directory or source-name lookup substitutes for publication evidence.

Both harnesses copy immediate errcode/extended_errcode/errmsg before unrelated
SQLite calls, finalize each statement once, and track every close. The I/O
database uses a fresh DELETE-journal path; successful recovery is followed by
normal close, reopen, persisted-row verification and integrity_check. The
closed-file guard requires absent sidecars without deleting them. Generated
execution compares exact typed case observations to the separate native
report and audits its loaded libraries to exclude native SQLite.

This slice does not cover every allocator failure point, actual OS/device
failure, all VFS/fault sites, multiconnection failure schedules or callback
failure combinations. It does not replace the remaining API, provider,
concurrency and enabled-feature rows in [the T4 ledger](T4-PROGRESS.md).


The frozen native/JIT/NativeAOT gate now passes all five measured phases against
the final readable source-13 publication. [Compact receipt](../acceptance/sqlite/failures-report.json)
pins source, workload, API, compiler, native oracle and loaded-library identities.
Both generated modes match the native semantic cases. Allocation refusal leaves the transaction active and requires explicit rollback; injected pager commit IOERR automatically rolls back. Prior heap limit is restored, the fault hook is uninstalled, finalization happens once, connections close and recovery/reopen/integrity pass. Live memory/cap accounting stays separate.
Generated dependencies contain only Generated, Runtime and the test harness;
process maps exclude native SQLite. NativeAOT explicitly pins clang 18. This is
finite configured Linux/x64 evidence, not an exhaustive failure, tokenizer or
provider-variant claim.
