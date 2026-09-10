# Host threading and database mmap

The managed library uses `config/host-defines.txt` overrides after the common
`config/defines.txt` definitions. The host product enables `SQLITE_THREADSAFE=1`,
with serialized connections by default, and database mmap with a 64 MiB default
and 256 MiB maximum per file. `scripts/build.sh` remains a build-only entry point.
The deterministic native/translated C corpora retain their original single-thread,
unmapped memory-VFS profile. Their layout evidence is supplemented by the separate
product layout gate for the host profile.

## Mutexes and initialization

`HostVfs.Mutex.cs` supplies SQLite's default mutex table using BCL monitors and
opaque GCHandle identities. Callback addresses are captured once. Static mutex
identities remain stable across shutdown/reinitialization; SQLite frees dynamic
connection/initialization mutexes. Monitor recursion permits callback reentry on
the same connection. Twelve static GCHandles intentionally live for the process
lifetime, including across `sqlite3_shutdown`; collectible assembly unloading
is unsupported. Shared host inode/WAL state and named memory VFS state have
their own synchronization, so distinct connections can run concurrently.

Setting `THREADSAFE=1` alone is insufficient with upstream `SQLITE_OS_OTHER`:
that configuration selects no-op mutexes and removes initialization barriers.
`scripts/prepare-host-source.py` generates a copy under `generated/sqlite-port/`
with one guarded adjustment to permit `SQLITE_MUTEX_APPDEF`. The adapter supplies
`sqlite3DefaultMutex()` and `sqlite3MemoryBarrier()`; the latter calls BCL
`Thread.MemoryBarrier`. SQLite retains its own initialization and configuration
logic, including concurrent cold initialization. No module initializer is used.

The generator requires exact SQLite 3.53.4 source/header hashes, exactly one
matching selection guard, and the expected adapted hash. It writes an adaptation
manifest. The downloaded archives and files in `ref/` remain unchanged. This is a
platform adaptation of the generated input, not a compiler recognition of SQLite
names or a fork of its SQL algorithms. Upgrading SQLite requires reviewing this
explicit guard again.

Ordinary `sqlite3_open` connections use SQLite's serialized mode. Separate
connections opened with `SQLITE_OPEN_NOMUTEX` can run concurrently, but each such
connection and its statements must have one caller at a time. A FULLMUTEX
connection serializes individual API calls; application-level sequences such as
an entire transaction or `step` followed by column reads still need ownership or
an application lock when shared between threads. A SQL callback can reenter the
same serialized connection.

Global configuration and shutdown require caller quiescence, as with SQLite:
join workers and close statements/connections before shutdown or reconfiguration.
Do not free callback contexts while registered callbacks can run. See SQLite's
[threading modes](https://www.sqlite.org/threadsafe.html).

## Database mappings

`HostVfs.Mmap.cs` implements version-3 file methods with read-only BCL
`MemoryMappedFile` views created from the existing file handle. Writes continue
through ordinary file I/O. The view owns an acquired native pointer until its
last required release; no managed array is exposed as a database mapping.
Read-only database handles are supported. This is separate from WAL's shared
`-shm` mappings, which continue to use the existing WAL implementation.

For example, select a smaller cap for one connection with:

```sql
PRAGMA mmap_size = 16777216;
PRAGMA mmap_size;             -- query the effective cap
PRAGMA mmap_size = 0;         -- use ordinary reads
```

`SQLITE_FCNTL_MMAP_SIZE` returns the previous cap when setting it, and negative
inputs query without changing it. Requests are clamped to SQLite's configured
maximum. Live `xFetch` references prevent a limit change or remapping that would
invalidate their addresses. Duplicate fetches are tracked separately; each must
be matched by `xUnfetch`. A null-pointer unfetch invalidates an idle mapping.

Ranges must fit within both the mapped file and cap, with SQLite 3.53.4's extra
256-byte safety margin beyond the requested page. Reads near EOF therefore use
ordinary I/O. Growth beyond a borrowed mapping also falls back until remapping
is safe. Truncation releases idle mappings and refuses to invalidate live
borrows. Idle views are released on unlock; on Windows the last unfetch also
releases the view so it cannot obstruct truncation from a sibling connection.
Mapping allocation/OS failures fall back to ordinary reads. SQLite's page locks,
WAL snapshots and change detection still govern access to the database.

`HostVfs.MappedReadCount`, `MappedViewCount` and `MappedReferenceCount` provide
observability for tests and consumers. A positive PRAGMA value alone does not
prove that any page was mapped; the tests assert actual successful fetches.
Mapping is not a promise of higher throughput for every workload.

## Validation entry points

- `SQLITE_AOT=1 scripts/test-threading.sh`: cold initialization/restart, mutex
  identity/ownership/recursion/try, shared FULLMUTEX callbacks, separate NOMUTEX
  host rollback/WAL and named-memory connections, error recovery and allocation.
- `SQLITE_AOT=1 scripts/test-host-vfs.sh`: raw I/O/WAL/mmap lifetime contracts,
  actual SQL mapping/fallback, growth, snapshots, checkpoint, VACUUM, readonly
  reopen, and independent native/managed process interoperability.
- `SQLITE_AOT=1 scripts/test-product-layout.sh`: native host-profile layout
  constants compared with the actual managed product under JIT and NativeAOT.
  Native link stubs in this gate are layout-only and are never SQLite execution
  or mutex correctness evidence.

Current execution evidence and platform limits are recorded in `validation.md`.
The locally tested platform is Linux x64. Windows/macOS implementations use BCL
and existing platform services; source support is distinct from local execution.
Arbitrary managed exceptions unwinding through translated C callbacks are outside
SQLite's callback contract; managed extension callbacks should report failures
through SQLite result APIs.
