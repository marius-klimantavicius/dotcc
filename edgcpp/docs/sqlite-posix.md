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

The final `sqlite-posix-005` campaign passes strict builds, all 11 in-process
SQLite groups and independent-process verification under JIT and Linux-x64
NativeAOT. The native oracle remains a separate test process. The library uses
`SQLITE_THREADSAFE=1`, APPDEF BCL mutexes, real disk temporary storage, WAL and
64 MiB default/256 MiB maximum database mappings on Unix hosts. FTS5, math, percentile, column
metadata and preupdate hooks remain enabled. Three SQLite-specific partial hooks provide the mutex table, memory barrier
and runtime database-mapping capability. All have implementations.

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
| Descriptors/I/O | `open`, `openat`, `close`, `read`, `write`, `pread`, `pwrite`, `lseek`, `ftruncate`, `truncate`, `fsync`, `fdatasync`, `fcntl` |
| Paths/metadata | `access`, `getcwd`, `stat`, `fstat`, `lstat`, `unlink`, `unlinkat`, `link`, `symlink`, `mkdir`, `rmdir`, `readlink`, `pathconf(_PC_PATH_MAX)`, `fchmod`, `fchown`, `geteuid` |
| Directories | `opendir`, `fdopendir`, `readdir`, `closedir` |
| Mapping | `mmap`, `munmap` |
| Time/process | `gettimeofday`, `utimes`, `sysconf`, `getpid`, `sleep`, `usleep` |

The ABI uses fixed LP64 `stat`, `flock` and `timeval` records. The current `stat`
is 120 bytes and contains three 16-byte `timespec` fields, preserving nanoseconds;
`st_atime`, `st_mtime` and `st_ctime` are C aliases for their seconds fields.
Previously generated 96-byte records must be retranslated. OS-native records
are converted, never directly exposed to generated code. `stat`/`timeval`
imports are checked field-by-field before emission; incompatible records are
rejected. Independent C layout and native behavior oracles validate the public
boundary. Variadic `open`/`fcntl` consume the existing shared `VaArg` span without
per-call helpers or a heap argument pack.

Directory-relative operations resolve against retained handles, including after
a directory rename. `fdopendir` takes ownership without invalidating the original
fd: recursive `openat`/`unlinkat` calls remain possible until `closedir`. Directory
enumeration uses an owned duplicate and a finalizable native stream handle.
Normal end-of-directory preserves errno; closing a stream cannot accidentally
close a subsequently reused portable descriptor.

Windows uses `NtCreateFile` with `RootDirectory` and runtime-selected Win32
deletion/link APIs. Dot components are walked separately; parent lookup checks
the directory's file identity to reject stale paths. Windows requires a known
file/directory kind for symlink creation, so a missing target currently returns
`ENOTSUP`. Linux NativeAOT and native-C parity tests cover these operations.
Wine passes the standalone metadata/time suite, but the directory-relative suite
currently stops at parent lookup after rename; native Windows/macOS execution
and this Wine failure remain explicit validation gaps.

The updated ABI also passes the complete `.dotobj` SQLite engine suite under
JIT and Linux NativeAOT: 225 original translation units, 70 canonical runtime
imports and no unresolved imports. Retained results and limitations are recorded
in [the object filesystem evidence](../evidence/dotobj-posix-relative-20261008.json).

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

Windows rejects shrinking files with live mapped views (`ERROR_USER_MAPPED_FILE`),
while the upstream Unix VFS intentionally keeps its mappings across truncation.
SQLite initialization therefore clamps database mmap sizes to zero on Windows,
including later `PRAGMA mmap_size` requests. Ordinary positioned reads preserve
SQL behavior; WAL shared-memory mappings remain enabled. This is selected at
runtime, using the same generated assembly. General POSIX file mappings are still
available, but cannot make Windows support shrinking a live mapped file. Growing a mapped
file remains supported and is needed when the WAL index expands.

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
writers. Windows .NET 10 under Wine 11 also passes 83 Windows-path runtime
checks, the independent-process registry suite and all 11 SQLite test groups.
Translated Windows peers also pass rollback/WAL contention, killed-writer
recovery and reopen integrity checks. A 5,015-frame WAL expands its index to
two shared-memory regions; independent reads and checkpoint contention pass.
Native Windows execution remains
unverified. Direct `LockFileEx` cannot
provide the needed atomic upgrades: [Microsoft's lock semantics](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex).

## Verification and remaining scope

The final SQLite campaign checks concurrent startup, shared/independent
connections and owners, callback reentry, persistence, JSONB/FTS5/math,
blobs/backup/serialization, confirmed file temp storage, mappings, snapshots,
checkpoint contention and VACUUM. Translated/native process pairs exercise
rollback/WAL locking, forced crashes, readonly recovery, checkpoint races,
hardlinks and symlink paths. All pass JIT and Linux NativeAOT.

The reusable `backend/tests/posix_wine_verify.py` runner takes explicit Wine,
Windows .NET, prefix and compiled-fixture paths (`--help` lists the arguments).
The final `posix-wine-003` run uses a dedicated prefix, the official Windows x64 .NET runtime,
Windows NLS globalization and Wine's built-in `mscoree`. It executes the Win32
branches; it is not a substitute for native Windows or Windows NativeAOT tests.

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
