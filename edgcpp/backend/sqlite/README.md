# SQLite host adapter for the EDG backend

`prepare.py` prepares a filesystem-backed `SQLITE_OS_OTHER` host profile for an
instance-mode `Edg.SQLite.SqliteCore`. The SQLite algorithms and selected musl
algorithms are translated; the filesystem and mutex callbacks use authored C#.
The adapter reuses `sqlite/src/HostVfs*.cs` at preparation time. Those files remain
the single implementation of file I/O, byte locks, WAL shared memory, database
mmap, and BCL mutex services. The generated project can be copied independently
and does not require the source repository at runtime.

Run preparation after translation:

```sh
python3 edgcpp/backend/sqlite/prepare.py --output-dir path/to/generated
```

Preparation records source hashes in `sqlite-host-sources.json` and adds its
generated files to `Generated.csproj` idempotently. It performs these ABI changes:

- Imports SQLite types and constants from `Edg.SQLite.SqliteCore`.
- Adds static callback thunks with the EDG instance owner as the first argument.
- Converts callback UTF-8 `sbyte*` parameters to the shared VFS's `byte*` views.
- Gives each owner its own VFS registration, error TLS, mutex identities, mmap
  configuration and diagnostic counters. The low-level inode/WAL registries
  remain process-wide, so independent owners coordinate access to the same file.
- Stores VFS global callback tables and names in pinned arrays. Temporary file
  storage and SQLite allocations continue to use their normal native allocators.

`HostBindings.cs` supplies the generated partial extern implementations and
small translated-owner bridges. `sqlite3_os_init` registers the `dotcc-host`
default VFS; no separate initialization call is needed before `sqlite3_open_v2`.
The C adaptation supplies `edg_sqlite_mutex_methods`, a memory barrier, and mmap
configuration getters. `SQLITE_THREADSAFE=1` alone is insufficient with upstream
`SQLITE_OS_OTHER`; the source preparation also selects real APPDEF mutexes.

The partial implementation currently follows the backend's generated
`Managed.Runtime` owner for scalar platform externs. It does not independently
rename that generated contract to the proposed `Edg.Runtime` shared owner.

## Lifetime

Before disposing a `SqliteCore`, join its workers, finalize statements and other
SQLite resources, close every connection, and call `sqlite3_shutdown`. Check its
return code. As with native SQLite, application shutdown/configuration requires
quiescence. `sqlite3_close_v2` can leave a zombie connection alive until all its
statements and related resources have been released.

The generated owner's `Dispose`/`Shutdown` releases translated allocations; it
does **not** automatically call the application API `sqlite3_shutdown` or close
SQLite connections. Calling it with live database resources violates the host
adapter lifetime contract and can strand file contexts, GCHandles and OS locks.
Do not use it as an emergency connection closer.

`xClose` releases file GCHandles, mmap views and OS handles. SQLite releases its
dynamic mutexes; `xMutexEnd` preserves the owner's static mutex identities across
shutdown and reinitialization, matching SQLite's static mutex contract. A mutex
adapter finalizer releases those static GCHandles when the adapter is collected.
Callback-table backing
arrays are managed owner storage, not unmanaged allocations requiring a
finalizer. Static mutex pointers remain valid across `sqlite3_shutdown` and
reinitialization of the same live owner. They must not outlive that owner.

`core.Host` exposes `OpenHandleCount`, `MappedReadCount`, `MappedViewCount` and
`MappedReferenceCount`. `core.Mutexes.DynamicMutexCount` exposes dynamic mutex
ownership for verification. These counters do not make application-level
transaction sequences on shared connections atomic.

## Platform and feature limits

The shared host source selects Linux, Windows and macOS APIs at runtime where
BCL I/O cannot express SQLite's locking/durability contracts. It contains no
native SQLite dependency. Linux uses OFD byte locks and requires kernel and
filesystem support for them. Linux and macOS ARM64 platform paths and Windows
paths require execution on their respective platforms before claiming an EDG
runtime pass there; a successful Linux run is not cross-platform test evidence.
Unsupported platforms fail explicitly. See `sqlite/docs/host-vfs.md` for the
shared VFS's pathname, filesystem, locking and durability constraints.

The profile omits native dynamic extension loading (`SQLITE_OMIT_LOAD_EXTENSION`).
Native extension function pointers and native SQLite structures cannot be
assumed compatible with the translated owner-first managed ABI. Translated or
authored C# extensions can instead use the SQLite registration APIs.

This adapter is a reusable filesystem host/reference profile. It is distinct
from translating upstream SQLite's `SQLITE_OS_UNIX` VFS against a managed POSIX
implementation; it does not claim to have implemented that alternate route.
