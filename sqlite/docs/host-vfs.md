# Real file-backed VFS

The reusable `TranslatedSqlite` library defaults to **dotcc-host**. An ordinary
`sqlite3_open` or `sqlite3_open_v2` therefore creates or opens a real filesystem
database. Rollback journals and WAL sidecars are real files too. The SQLite engine, SQL/JSONB/
FTS5 implementation and application extensions remain translated/managed C#;
there is no native SQLite dependency or dynamic extension loader.

`scripts/build.sh` builds this library. `scripts/emit-engine.sh` applies the host
threading/mmap profile; a semantic override binds `sqlite3_os_init` to authored
C# `HostVfs.RegisterVfs`; `sqlite3_os_end` binds to `HostVfs.UnregisterVfs`. `sqlite/Directory.Build.targets` compiles the managed VFS/mutex sidecars
alongside the generated project. Downloaded references and emitted C# remain
unchanged; a generated input copy has one guarded mutex-selection adaptation
([details](threading-mmap.md)). Keep that targets file and the sidecars when building the
generated project; the built DLL is independently usable by other C# projects.

Use SQLite's built-in `:memory:` mode for in-memory databases. The named
`dotcc-memory` VFS exists only in [test corpora](memory-vfs.md), which compile
`tests/memory_vfs.c` for deterministic file behavior and error injection.

## Files and durability

BCL `RandomAccess` supplies offset reads/writes, size/truncate and disk flushes.
Reads loop over partial results and zero-fill the unread tail when returning
`SQLITE_IOERR_SHORT_READ`. Open modes honor creation, exclusive creation,
read-only fallback, temporary files and delete-on-close; managed contexts and OS
handles are released by `xClose`. POSIX temporary files are unlinked immediately;
Windows uses the OS delete-on-close flag. Time, sleeping and randomness use real
BCL services rather than deterministic test values.

UTF-8 paths are made absolute and symlink components are resolved before `..`.
This gives a database and its journal one canonical pathname. The VFS reports
`SQLITE_OK_SYMLINK` so the pager can reject aliases requested with
`SQLITE_OPEN_NOFOLLOW`; the final OS open also applies a no-follow check.

File sync uses `RandomAccess.FlushToDisk`. POSIX directory entries are synced
after relevant creation/deletion; macOS full sync first requests `F_FULLFSYNC`,
with the native SQLite-style fsync fallback. Windows uses `FlushFileBuffers`
through the BCL and follows native SQLite's Windows behavior for directory
metadata, which has no Unix-style directory-fsync step. The adapter advertises no
atomic-write, powersafe-overwrite or other storage guarantees it cannot prove.
The process-kill tests verify hot-journal recovery, not physical power-loss or
storage-device behavior beyond the OS flush contract.

## Locking and platform scope

The VFS uses SQLite's actual rollback lock bytes: pending `0x40000000`, reserved
`+1`, and the 510-byte shared range beginning at `+2`. Shared/reserved/pending/
exclusive transitions, failed upgrades, downgrades and reserved probes coordinate
with separate native SQLite processes, rather than a private lock-file protocol.
All managed callback addresses are captured once in static readonly tables.

| Platform | OS services | Validation status |
| --- | --- | --- |
| Linux x64 | OFD `fcntl` byte locks; OS open/unlink and directory fsync | Local JIT/NativeAOT rollback and WAL contracts, native-process interoperability, checkpoint stress and recovery pass. |
| Windows x64/ARM64 | `CreateFileW`, `LockFileEx`/`UnlockFileEx`, readonly probes and reparse-point checks | Implemented and reviewed; dedicated JIT/AOT CI configured, not run locally. |
| macOS x64/ARM64 | POSIX `fcntl`, inode identity, deferred descriptor close, full file/directory sync | Implemented and reviewed; dedicated JIT/AOT CI configured, not run locally. |
| Linux ARM64 | Same 64-bit OFD interface | Guarded implementation; not executed locally. |
| BSD and other platforms | None | Explicitly unsupported in this phase. |

Linux requires kernel/filesystem support for OFD locks; unsupported locking fails
with an I/O result rather than substituting process-local or ineffective locks.
The kernel coordinates hardlink aliases on Linux and Windows. macOS requires a
process-wide device/inode registry: all VFS descriptors participate, and closing
an unlocked sibling is deferred while another connection holds a POSIX lock.
macOS ARM64's variadic C calling convention is handled explicitly for `fcntl` and
`open`; runtime confirmation belongs to the platform CI job.

As with native SQLite, writable databases must use a consistent pathname: distinct
hardlinks can still create distinct journal names even though lock identity is
correct. On macOS, unrelated code in the same process must not independently open
and close the database inode behind this registry; any such close can release
classic POSIX locks. Separate native oracle processes are safe and tested.
Failed macOS closes remove the disposed connection's lease and retain only the
descriptor while peers still have locks. Orphaned lock state blocks new lock
acquisition until safe deferred close drains it. A separate two-case fixture
checks this ownership path with simulated lock errors and real safe handles;
it does not substitute for running the macOS ABI in platform CI.

## WAL mode

Enable WAL on a database through the ordinary translated API:

```sql
PRAGMA journal_mode=WAL;
```

The result is `wal`, and the mode persists across reopen. New databases retain
SQLite's default DELETE journal mode until explicitly changed. `ManagedConsumer`
selects WAL before running its SQL/JSONB/FTS5 and callback workloads.

The version-3 I/O table includes the WAL methods `xShmMap`, `xShmLock`, `xShmBarrier` and `xShmUnmap`.
BCL `MemoryMappedFile` maps the actual `database-shm` file, with stable pointers
for all previously mapped regions when the index grows. These mappings are for
the WAL index. Separate read-only database mappings implement `xFetch`/`xUnfetch`
with a 64 MiB default cap; see [threading and mmap](threading-mmap.md). The translated engine
continues to implement WAL records, checksums, snapshots, checkpointing and
recovery itself.

Shared/exclusive locks use SQLite's shm byte range 120–127 and dead-man byte 128.
One process registry per database inode coordinates local connections, while OS
locks coordinate independent processes and native SQLite. Initialization follows
the OS VFS protocol, including clearing an abandoned index only with the required
exclusive dead-man lock. Readonly shm mappings report SQLite's readonly result
codes so the engine can select its readonly recovery path. Unmap releases the
connection's locks; the last local lease releases mappings and the file handle.
The engine requests sidecar deletion when it can safely complete WAL cleanup.

The pinned 3.53.4 engine includes the upstream WAL-reset race fix. WAL requires
all processes on the same host and filesystem support for shared mappings and
byte locks; use a local filesystem. Keep the database and live `-wal` file
together when moving/copying database state, or use SQLite's backup API.

The product enables `SQLITE_THREADSAFE=1` with BCL mutexes; concurrent connections
and serialized shared connections are supported. The deterministic C corpus
retains its separate `SQLITE_THREADSAFE=0` configuration. WAL permits readers to
retain a snapshot while another connection or process commits;
it still permits only one writer at a time. Long reader transactions can prevent
a checkpoint from completing. `sqlite3_wal_checkpoint_v2` supports passive, full,
restart, truncate and the current release's no-op mode. `PRAGMA journal_mode=DELETE`
returns to rollback journals when other connections/transactions permit it.

## Verification

From `sqlite/`, after building dotcc and fetching the pinned sources:

```sh
SQLITE_AOT=1 scripts/test-host-vfs.sh
SQLITE_AOT=1 scripts/test-managed-consumer.sh
SQLITE_AOT=1 scripts/test-threading.sh
```

The first command runs raw callbacks, SQL/JSONB/FTS5 disk persistence and readonly
checks, raw mapping lifetime/range/fallback and mapped SQL snapshot/VACUUM
contracts, then a bounded Python process oracle against the pinned native SQLite
using its real OS VFS. Native→managed, managed→native and managed→managed pairs
exercise writer/reader contention, pending-lock admission, actual forced process
termination with a synced hot-journal header, rollback recovery, integrity and
reuse of released locks. Hardlink and symlink cases cover identity and journal
paths. WAL contracts additionally cover snapshots, writer contention, checkpoint modes,
multiple index regions, readonly media, close/reopen and journal-mode transitions.
Forced-kill cases preserve committed frames, discard spilled uncommitted tails
and rebuild stale or missing indexes. The same campaign runs against the
NativeAOT executable.

The normal `scripts/verify.sh` includes this gate. The separate
`.github/workflows/sqlite-host-vfs.yml` builds/runs managed process contracts under
JIT and NativeAOT on Linux, Windows and macOS; it detects the installed .NET RID.
Adding this workflow does not run remote CI. See `validation.md` for local results.

References: [SQLite WAL](https://www.sqlite.org/wal.html),
[SQLite rollback locking](https://www.sqlite.org/lockingv3.html),
[SQLite VFS API](https://www.sqlite.org/c3ref/vfs.html),
[BCL FlushToDisk](https://learn.microsoft.com/en-us/dotnet/api/system.io.randomaccess.flushtodisk).
