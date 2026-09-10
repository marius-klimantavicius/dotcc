# Real file-backed VFS

The reusable `TranslatedSqlite` library defaults to **dotcc-host**. An ordinary
`sqlite3_open` or `sqlite3_open_v2` therefore creates or opens a real filesystem
database. Its rollback journals are real files too. The SQLite engine, SQL/JSONB/
FTS5 implementation and application extensions remain translated/managed C#;
there is no native SQLite dependency or dynamic extension loader.

`scripts/build.sh` builds this library. `scripts/emit-engine.sh` adds
`DOTCC_HOST_VFS=1`; the translated `sqlite3_os_init` explicitly registers the host
adapter. `sqlite/Directory.Build.targets` compiles `src/HostVfs.cs` and
`src/HostVfs.Platform.cs` alongside the generated project. No upstream or generated
C# source is patched. Keep that targets file and the sidecars when building the
generated project; the built DLL is independently usable by other C# projects.

The named `dotcc-memory` VFS remains available through `sqlite3_open_v2` for
intentional process-local databases. C/native differential fixtures omit
`DOTCC_HOST_VFS` and retain their deterministic memory default and error injection.
The engine still supports SQLite's normal `:memory:` databases.

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
| Linux x64 | OFD `fcntl` byte locks; OS open/unlink and directory fsync | Local JIT and NativeAOT contracts and native-process interoperability pass. |
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

This is a **rollback-journal VFS**, with version-1 I/O methods. WAL shared-memory
methods and mmap are not advertised; asking for WAL on a rollback database retains
the supported journal mode. The existing `SQLITE_THREADSAFE=0` profile remains:
serialize SQLite calls in each process. OS locks permit independent processes and
multiple serialized connections to coordinate access; they do not make the
translated engine thread-safe.

## Verification

From `sqlite/`, after building dotcc and fetching the pinned sources:

```sh
SQLITE_AOT=1 scripts/test-host-vfs.sh
SQLITE_AOT=1 scripts/test-managed-consumer.sh
```

The first command runs raw callbacks, SQL/JSONB/FTS5 disk persistence and readonly
checks, then a bounded Python process oracle against the pinned native SQLite
using its real OS VFS. Native→managed, managed→native and managed→managed pairs
exercise writer/reader contention, pending-lock admission, actual forced process
termination with a synced hot-journal header, rollback recovery, integrity and
reuse of released locks. Hardlink and symlink cases cover identity and journal
paths. The same campaign runs against the NativeAOT executable.

The normal `scripts/verify.sh` includes this gate. The separate
`.github/workflows/sqlite-host-vfs.yml` builds/runs managed process contracts under
JIT and NativeAOT on Linux, Windows and macOS; it detects the installed .NET RID.
Adding this workflow does not run remote CI. See `validation.md` for local results.

References: [SQLite rollback locking](https://www.sqlite.org/lockingv3.html),
[SQLite VFS API](https://www.sqlite.org/c3ref/vfs.html),
[BCL FlushToDisk](https://learn.microsoft.com/en-us/dotnet/api/system.io.randomaccess.flushtodisk).
