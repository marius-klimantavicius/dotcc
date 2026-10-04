# Threadsafe filesystem-backed SQLite

The EDG host profile translates SQLite 3.53.4 and 213 selected musl/provider
units into an instance-mode `Edg.SQLite.SqliteCore`. It implements real file
I/O, rollback journals, WAL, shared memory, database mmap, file-backed temporary
storage and BCL mutexes. Original SQLite API names and 490 public object macro
constants are exported. The generated library has no native SQLite dependency.

Generate and verify a fresh retained artifact directory:

```sh
python3 edgcpp/sqlite_host.py \
  --output-dir edgcpp/artifacts/campaign/persistent-continuation/sqlite-host-new \
  --nativeaot --process-tests
```

The script records commands, logs and hashes. `--translate-only` stops after
emission; `--resume` reuses the emitted library and rebuilds the host adapter
and tests. Resume does not refresh copied shared runtime files; start a fresh
directory after changing the compiler or shared runtime. Failed logs are kept
when retrying. The native process oracle is a separate test executable, not a
library dependency. The full verification command requires .NET 10, Python,
GCC and NativeAOT prerequisites; local evidence is Linux x64.

## Configuration and runtime

The profile uses `SQLITE_THREADSAFE=1`, `SQLITE_MUTEX_APPDEF=1`,
`SQLITE_OS_OTHER=1`, `SQLITE_TEMP_STORE=1`, a 64 MiB default database mmap cap
and 256 MiB maximum. FTS5, math, percentile, column metadata and preupdate hooks
remain enabled. It preserves SQLite's runtime single-thread, multi-thread and
serialized mode selection. A shared FULLMUTEX connection serializes individual
API calls; callers still own multi-call transaction/statement sequences.

The existing guarded APPDEF source adaptation is verified against pinned
amalgamation hashes. Small C hooks expose the default mutex table, memory
barrier and current mmap configuration. The selected musl algorithms and
SQLite code are translated normally. The four resulting external partial
contracts have real implementations in the host adapter.

`backend/sqlite/prepare.py` adapts the existing authored `sqlite/src/HostVfs*.cs`
to EDG's owner-first managed callback ABI. Platform code has one source;
generated projects contain their own reproducible copy. Each engine owner has
its own VFS registration, mutex identities and error TLS. Low-level file
identity/lock/WAL registries remain shared so separate owners coordinate files.
Static mutex identities remain stable through shutdown/reinitialization.
Pinned arrays hold owner callback tables/names; ordinary SQLite allocations
use the native heap. See the [adapter contract](../backend/sqlite/README.md).

Before disposing an engine, join workers, release statements/blobs/backups,
close connections and call `sqlite3_shutdown`. Generated `Dispose` is not a
substitute for closing application resources. Keep an engine alive while any
returned pointer or callback can be used.

## Verification and review

`artifacts/review/sqlite`, `sqlite-readable` and `sqlite-host` point to the
latest verified host emission. Start with `REVIEW.md`, `Translated.cs`, the
size-split `Translated.part*.cs` and `Runtime.Sqlite.HostBindings.cs`.
The project is `Generated.csproj`; all four partial contracts are implemented.

The retained `sqlite-host-002` campaign passes strict builds, all 11 in-process
test groups and the full independent-process campaign under both JIT and
Linux-x64 NativeAOT. Coverage includes concurrent startup, static mutex restart
identity, callback reentry, shared/independent connections and owners, disk
persistence, SQL/JSONB/FTS5/math, blobs/backup/serialization, confirmed disk temp
storage, actual mmap fetches, snapshots, checkpoint contention, growth and
VACUUM. Process pairs use translated→translated, native→translated and
translated→native SQLite, checking rollback/WAL locks, forced death and recovery,
readonly media, concurrent checkpoints, hardlinks and canonical symlink paths.

The first campaign exposed a compare-exchange-based atomic load writing to a
read-only WAL index. The shared atomic runtime now performs read-only loads;
focused mapping and existing atomic/C11 conformance tests also pass. The
failed campaign is retained. A separate JIT/AOT concurrency fixture verifies
the existing heap ledger, errno/TLS and active-call shutdown synchronization.
[Evidence](../evidence/sqlite-host-20261004.json).

This is substantial project integration coverage, not exhaustive SQLite
conformance or complete libc. Runtime-selected Windows/macOS paths are present
in the shared VFS, but were not executed in this EDG campaign. Native loadable
extensions remain disabled because their ABI differs from the translated
managed callback ABI; authored/translated managed extensions can use SQLite's
registration APIs.

`SQLITE_OS_UNIX` is also translatable: an emission probe succeeds and leaves
29 POSIX functions plus two mutex/barrier hooks to implement. The current host
profile does not supply that POSIX layer. See the [Unix route analysis](sqlite-posix.md).
