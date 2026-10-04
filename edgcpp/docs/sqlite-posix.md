# SQLite against an emulated Unix platform

`SQLITE_OS_UNIX=1` is compatible with a cross-platform managed runtime. It selects
SQLite's source implementation of a Unix VFS; it does not require the eventual
.NET host to be Unix. The target ABI and POSIX behavior must be supplied by the
runtime, with BCL operations or runtime-selected platform APIs underneath.

The existing musl provider translates selected libc algorithms and supplies
managed services. It does not yet implement a complete POSIX descriptor,
mapping, locking or pthread layer. Therefore selecting the Unix VFS currently
exposes missing runtime contracts. This is distinct from an EDG translation
limitation.

## Translation probe

The retained `artifacts/campaign/persistent-continuation/sqlite-unix-audit-001`
probe translates the pinned amalgamation, including its upstream Unix VFS,
with portable header declarations and the existing application-mutex guard
adaptation. The baseline without the header overlay fails at missing pthread
declarations. The successful overlay emission retains 29 POSIX externs and two
application-mutex/barrier hooks. It is **not a runnable Unix-profile library**.

| Boundary | Remaining external functions in this probe |
| --- | --- |
| Descriptors and I/O | `open`, `close`, `read`, `write`, `pread`, `pwrite`, `ftruncate`, `fsync` |
| Metadata and paths | `access`, `getcwd`, `stat`, `fstat`, `lstat`, `unlink`, `mkdir`, `rmdir`, `readlink`, `fchmod`, `fchown`, `utimes` |
| Locks and mappings | `fcntl`, `mmap`, `munmap` |
| Process and time | `geteuid`, `getpid`, `sysconf`, `sleep`, `usleep`, `gettimeofday` |

This inventory describes one configured SQLite build. It is not a complete
POSIX or musl implementation requirement, nor an assertion that pthread APIs
have been implemented: the probe uses APPDEF mutexes.

## Implementation boundary

A reusable implementation belongs below SQLite, in a shared `Edg.Runtime` POSIX
service. It needs a fixed portable `stat`, `flock`, time and descriptor ABI;
integer descriptor ownership; shared file identity and lock coordination;
mapping lifetime; and target errno translation. Native host structures must
be converted at the platform boundary. The current `LibcFiles` abstraction is
opaque `FILE*` tokens over streams, not an integer POSIX descriptor table, so
descriptor support and `fdopen`/`fileno` integration are additional work.

BCL `RandomAccess` and `MemoryMappedFile` can provide most I/O and mapping
operations. Existing SQLite `HostPlatform` code is useful implementation
evidence for file identity, durability and locks. It is not a drop-in `fcntl`
implementation: it reports SQLite results, Linux uses OFD locks, and its current
range-query path is Unix-only. POSIX process-scoped locks, conversions,
`F_GETLK`/`F_SETLK`/`F_SETLKW`, descriptor-close effects and independent translated
owners require explicit semantics. Windows has no direct equivalent of
`F_GETLK`. Renaming an existing lock helper would not establish correctness.

The repository's experimental dotcc upstream Unix VFS uses a native Linux shim.
It demonstrates another translation route, but its direct native forwarding is
not the requested portable managed POSIX implementation.

## Relationship to the current host profile

`sqlite_host.py` instead selects `SQLITE_OS_OTHER` and reuses the authored
cross-platform VFS and BCL mutex implementation from `sqlite/src`. This provides
a filesystem/threading profile and a reference for validating a future Unix
profile. It bypasses SQLite's Unix VFS and does not close the POSIX gaps above.
The broader musl direction remains compatible with implementing those gaps and
then running the same SQL, threading and independent-process verification
against `SQLITE_OS_UNIX`.
