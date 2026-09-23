# Filesystem ownership and mount contract

`IGuestFileSystem` covers ordinary files, directories, metadata and open-file
descriptions. `VirtualFileSystem` keeps private data in memory.
`HostDirectoryFileSystem.OpenRoot` exposes an explicitly selected host folder
through System.IO only. There are no native filesystem imports, native helper
processes or fallback accesses to the host root. `readOnly` defaults to false:
ordinary guest writes update the host folder directly and remain after sessions
or the machine are disposed. The caller's host permissions still apply.

`MountedFileSystem` keeps the root and mount stores across executions.
`AcquireSession` prevents configuration changes until the lease is disposed;
lease disposal closes namespace descriptors without clearing stores. InstanceIo
closes its descriptors and disposes the injected filesystem only when explicitly
given ownership. Both execution modes can use this namespace: in-process roots
may remain in memory; fresh worker processes use a machine-owned private host
directory, so acknowledged file changes do not depend on a final snapshot.
Filesystem persistence here means process lifetime persistence, not recovery
after a power loss.

Mount paths are absolute guest paths. Mounting over an existing directory
requires `replaceExistingDirectory: true`; underlying contents are hidden and
retained. The longest component-prefix mount wins. Duplicate mounts are
rejected; nested children must be unmounted first. Mount roots and their
ancestors cannot be renamed or deleted while mounted. Cross-mount rename
returns EXDEV. Relative paths and directory component checks use the same
namespace as executable/cwd lookup. Guest hard-link, symlink and special-node
creation remain unsupported. Executable selection must check guest mode bits.

`privateWritableLimit` bounds aggregate writable bytes in the root and mounts
marked `privateStorage: true`, including COW imports. Extending writes may be
partial; truncate cannot exceed the aggregate cap. Live grants have separately
configured per-root quotas. `privateNodeLimit` bounds the sum of private store
nodes, including each store root and detached open nodes; mount acceptance and
file/directory creation enforce it. Host-root quotas count existing content, open files
unlinked by this backend, nodes and path bytes; trusted external modifications
can change these observations. Caller-owned stores must not be mutated directly
while a session uses them. Independent machine mounts of the same host folder
do not share the in-process lock; host concurrency is not a transactional quota
or atomic append guarantee.

`GuestFileSystemCopy.Copy` performs a bounded eager copy into a private store
for COW, or into an explicitly selected empty destination for export. The result
reports copied bytes/files/directories. COW reads the frozen import; it is not a
live mount. It preserves representable mode bits, rejects overwriting existing
files, and checks each source file for size/time changes while reading. This
does not claim an atomic snapshot of a concurrently modified host tree. Failure
can leave a partial destination; discard a new private import or report partial
export. `Clear` explicitly discards a caller-selected private store; never use it
on live grants. Machine reset should replace private stores while idle.

Host-path checks reject symlinks/reparse points and known device attributes at
each component, and check again after file open. They are **not atomic** with
BCL I/O under hostile concurrent host namespace mutation. Portable BCL APIs do
not establish host inode identity or expose a reliable hard-link count. Host
roots must contain ordinary files/directories, without external hard-link
aliases or special files; trusted host actors must not replace components or
reparent granted directories during access. These checks mediate guest access
in a trusted embedding process; they are not a hardened sandbox against that
process or other hostile host actors. Directory descriptors retain virtual
paths, so externally renamed/deleted directories can make later path operations
fail; there is no portable dirfd-relative BCL equivalent.

File data uses open FileStream handles and managed descriptor offsets. Guest
duplicates share offsets and append settings. Directory snapshots are bounded.
Inodes are virtual identities, not host inode numbers. Advisory locks cover only
descriptions in one backend; they do not acquire host-process-wide flock locks.
Atomic replacement of an existing directory and directory fsync are explicitly
unsupported. Explicit sub-100ns timestamp updates are unsupported; other file
time operations use BCL timestamps. Change time is a BCL metadata observation,
not a promise of POSIX ctime equivalence.

On Windows, ordinary host reads/writes, read-only denial, directory enumeration,
and data persistence use the same BCL backend. Virtual default modes are 0600
for files and 0755 for directories. `SupportsPersistentUnixModes` is false:
chmod, non-default creation modes, and imports/exports requiring different mode
bits fail explicitly rather than retain session-only permission metadata.
Mounted Windows files do not gain executable permission implicitly. Persistent
executable configurations must therefore be rejected or handled by an explicit
future permission capability. Unix hosts use File.Get/SetUnixFileMode. Actual
qualification currently targets Linux x64; no Windows execution equivalence is
claimed.
