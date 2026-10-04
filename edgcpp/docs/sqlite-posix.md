# SQLite through the shared POSIX runtime

The `--posix` driver option supplies a portable Unix ABI and shared
`Edg.Runtime.PosixContext`. SQLite now runs its upstream `SQLITE_OS_UNIX` VFS
against that runtime. It does not require a Unix .NET host or a native SQLite
library. Selected musl algorithms and the Unix VFS are translated into C#;
platform operations use BCL services and runtime-selected OS APIs.

## Build and review

```sh
python3 edgcpp/sqlite_host.py --profile unix \
  --output-dir edgcpp/artifacts/campaign/persistent-continuation/sqlite-posix-new \
  --nativeaot --process-tests
```

The final `sqlite-posix-004` campaign passes strict builds, all 11 in-process
SQLite groups and independent-process verification under JIT and Linux-x64
NativeAOT. The native oracle remains a separate test process. The library uses
`SQLITE_THREADSAFE=1`, APPDEF BCL mutexes, real disk temporary storage, WAL and
64 MiB default/256 MiB maximum database mappings. FTS5, math, percentile, column
metadata and preupdate hooks remain enabled. Only two SQLite-specific partial
hooks remain: the mutex table and memory barrier. Both have implementations.

`artifacts/review/sqlite`, `sqlite-readable` and `sqlite-unix` point to the Unix
emission. Start with `REVIEW.md` and `Generated.csproj`. `sqlite-host` retains
the earlier OS_OTHER VFS as a reference. Original SQLite API names, public object
macro constants and size-split source remain available.

For another project, use the ordinary driver:

```sh
python3 edgcpp/dotnet.py program.c --language c --std c17 \
  --libc musl --posix --output-dir path/to/generated
```

`--libc dotcc` can also use `--posix`. Both static and instance modes are tested;
instances remain opt-in through `--instance-methods`. No project-specific POSIX
partial implementation is needed. The driver copies the same authored runtime
sources, records the POSIX header hashes and ABI identity, and integrates
service disposal into generated shutdown before errno/storage disposal.

## Implemented boundary

| Area | Bound functions |
| --- | --- |
| Descriptors/I/O | `open`, `close`, `read`, `write`, `pread`, `pwrite`, `lseek`, `ftruncate`, `fsync`, `fdatasync`, `fcntl` |
| Paths/metadata | `access`, `getcwd`, `stat`, `fstat`, `lstat`, `unlink`, `mkdir`, `rmdir`, `readlink`, `fchmod`, `fchown`, `geteuid` |
| Mapping | `mmap`, `munmap` |
| Time/process | `gettimeofday`, `utimes`, `sysconf`, `getpid`, `sleep`, `usleep` |

The ABI uses fixed LP64 `stat`, `flock` and `timeval` records. OS-native records
are converted, never directly exposed to generated code. `stat`/`timeval`
imports are checked field-by-field before emission; incompatible records are
rejected. Independent C layout and native behavior oracles validate the public
boundary. Variadic `open`/`fcntl` consume the existing shared `VaArg` span without
per-call helpers or a heap argument pack.

Each owner has its own descriptor table and working-directory snapshot.
`/dev/null`, `/dev/random` and `/dev/urandom` are host-independent services.
Windows Unix paths use `/C:/...` or `/unc/server/share/...`; relative paths work
normally, `/tmp` maps to the BCL temporary directory, and `getcwd` returns the
Unix spelling. Managed callers of the upstream Unix VFS must supply Unix paths.
Metadata/permission operations convert host errors into target errno. Windows
projects permissions onto readonly attributes and uses virtual uid/gid zero;
unsupported ownership changes fail explicitly.

`RandomAccess` provides bulk positioned I/O. Ordinary allocations remain native;
no pinned arrays are used as substitutes for file mappings. OS mappings provide
shared/private/readonly protections and outlive the original descriptor. Unix
partial unmap is tracked precisely. Timestamp updates use one native operation,
validate both times before mutation and avoid opening/closing Unix descriptors.

## Lock ownership and platforms

An engine owner is a virtual POSIX process for record-lock ownership. Closing
any descriptor for an inode releases that owner's locks, as POSIX requires.
It does not release another owner's locks. Queries, blocking/nonblocking
acquisition, overlapping ranges, conversions and failed upgrades are tested.

Linux uses one canonical open-file-description lock anchor per owner/inode.
The kernel performs atomic conversions, and unrelated BCL/native descriptor
closes cannot invalidate protection. Independently copied runtime assemblies
also conflict correctly. Readonly anchors migrate to read-write descriptors
without a protection gap. Linux native POSIX processes interoperate with these
locks; native SQLite contention/recovery campaigns pass.

macOS uses the corresponding Darwin OFD operations, including the ARM64 variadic
calling convention. Apple's commands are private kernel interfaces, so their
availability is a runtime requirement; unsupported operations fail instead of
falling back to unsafe process-scoped locking. This path is implemented but not
executed locally. Definitions: [Apple fcntl.h](https://github.com/apple/darwin-xnu/blob/main/bsd/sys/fcntl.h),
[Linux fcntl.h](https://github.com/torvalds/linux/blob/master/include/uapi/asm-generic/fcntl.h).

Windows uses a shared advisory registry with a user-only, cross-session named
mutex and atomic record replacement. It supports atomic range conversions,
queries, close semantics and stale-process cleanup after crashes. Full Windows
file identities key the registry. Coordination covers translated peers under
**the same Windows account**. Other accounts and native Windows SQLite do not
participate; do not concurrently mix them on the same database. This is an
explicit protocol boundary, not a native interoperability claim. Registry
algorithms pass cross-process JIT/AOT tests on Linux, including interrupted
writers; Windows OS execution remains unverified. Direct `LockFileEx` cannot
provide the needed atomic upgrades: [Microsoft's lock semantics](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex).

## Verification and remaining scope

The final SQLite campaign checks concurrent startup, shared/independent
connections and owners, callback reentry, persistence, JSONB/FTS5/math,
blobs/backup/serialization, confirmed file temp storage, mappings, snapshots,
checkpoint contention and VACUUM. Translated/native process pairs exercise
rollback/WAL locking, forced crashes, readonly recovery, checkpoint races,
hardlinks and symlink paths. All pass JIT and Linux NativeAOT.

Additional retained tests cover portable ABI/behavior parity, static/instance
translation, lifecycle cleanup, readonly mappings, native lock observers,
independently copied runtimes, unrelated descriptor closes and Windows registry
crash recovery. [Evidence](../evidence/sqlite-posix-20261004.json).

This implements the POSIX surface needed by the configured SQLite build, not
all POSIX or musl. Standard integer descriptors 0/1/2, `dup`, `fdopen`/`fileno`,
process creation, general pthread APIs and additional syscall families remain
outside this slice. SQLite uses BCL APPDEF mutexes, not translated pthreads.
Native loadable SQLite extensions remain disabled because their ABI is
incompatible with owner-first managed callbacks.

Mappings reject MAP_FIXED/executable requests; Windows also rejects partial
unmap, PROT_NONE and address hints explicitly. A write-only OFD anchor holding
exclusive locks cannot migrate to a different read-capable descriptor safely
and returns ENOTSUP; SQLite's readonly/read-write descriptor patterns are
supported. Linux needs filesystem/kernel OFD support. None of these unsupported
cases silently substitutes weaker behavior.

Join workers and release SQLite statements/blobs/backups/connections, then call
`sqlite3_shutdown` before disposing the owner. Generated disposal releases POSIX
resources but cannot repair application use of dangling SQLite pointers.
