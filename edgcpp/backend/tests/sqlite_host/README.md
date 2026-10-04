# EDG SQLite host verification

`SqliteHostTests` consumes the original SQLite API names emitted into
`Edg.SQLite.SqliteCore`. It runs unchanged under JIT and NativeAOT. Supply the
translated library through `-p:TranslatedProject=/absolute/path/Generated.csproj`.
The `edgcpp/sqlite_host.py` runner translates, builds, runs and retains evidence.

Coverage includes cold concurrent initialization, the three threading modes,
static and dynamic mutex identity and contention, recursive callback reentry,
shared serialized connections, independent owners, concurrent separate rollback
and WAL connections, real file persistence, JSONB/FTS5/math, read-only errors,
actual mmap reads and fallback, snapshots, blocked checkpoints, file growth,
VACUUM, incremental blob I/O, backup, serialize/deserialize, file-backed temporary
storage with confirmed VFS handles, and clean shutdown/restart. Time-bounded workers turn deadlocks into a
failed test. The tests require real APPDEF mutexes; THREADSAFE=1 alone is not
sufficient with SQLITE_OS_OTHER.

`serve <path> [readonly]` implements the bounded E/Q/C/X protocol used by
`sqlite/scripts/test-host-vfs-processes.py`. Reusing that process oracle checks
cross-process locking, forced-crash recovery and interoperability with the pinned
native SQLite amalgamation, without spawning processes inside the library.

This is project integration coverage, not a claim of exhaustive upstream SQLite
conformance. Native SQLite loadable extensions use an incompatible native ABI
and are deliberately disabled in this managed host profile.
