# Private file metadata

`HostFileMetadataBridge.cs` supplies `stat`, `fstat`, `lstat`, and a restricted
`fstatat` over the currently bound `InstanceIo`. All lookups stay inside that
instance's `VirtualFileSystem`; callbacks never call `File`, `Directory`, native
stat, or the generic dotcc POSIX stat implementation.

This boundary is needed by actual upstream code. `loader.c:555` checks regular
file type, `:560` requires execute permission, and `:725` consumes `fstat` size
before loading. With `DISABLE_VFS`, `vfs.h` maps `VfsFstat` to `fstat` and
`VfsStat` to `fstatat`. `xlat.c:1148` copies all the host stat fields into Blink's
separate `struct stat_linux`. This work does not implement the loader's remaining
file-backed mapping requirements or demonstrate full managed guest startup.

## Storage and callback binding

The campaign `config/managed-host/sys/stat.h` replaces the generic partial stat
record with an independently measured Linux x86-64 LP64 record: 144 bytes,
alignment 8. Its fields are:

| Field | Offset | Bytes |
| --- | ---: | ---: |
| dev, ino, nlink | 0, 8, 16 | 8 each |
| mode, uid, gid | 24, 28, 32 | 4 each |
| padding | 36 | 4 |
| rdev, size, blksize, blocks | 40, 48, 56, 64 | 8 each |
| atim, mtim, ctim | 72, 88, 104 | 16 each |
| reserved | 120 | 24 |

Each time record has signed 64-bit seconds and nanoseconds. The usual
`st_atime`/`st_mtime`/`st_ctime` aliases select their seconds fields. This host
record does not change Blink's guest record or assume that dotcc's generic stat
layout is native-compatible.

Two-stage macros distinguish the C struct tag `blink_host_stat_record` from
the callback `blink_host_stat`. Ordinary `stat(...)` calls are redirected;
a bare `stat` function designator is currently unsupported and resolves to the
campaign record name, preventing accidental generic runtime binding. The other
implemented callback names have ordinary object macros. The new header also
redirects chmod/mkdir/mkfifo variants to distinct, **unimplemented** host names;
those operations remain unresolved rather than silently accessing host files.

The bridge borrows C pointers only for the synchronous callback. Null pointers
return `EFAULT`; arbitrary invalid non-null host pointers are outside this C
boundary's contract. Upstream must continue validating guest memory before
passing host addresses. No CLR reference is stored in the C record. Successful
calls fill a local zero-initialized record before copying it out, clearing
padding and reserved bytes; failed calls preserve destination contents.

## Private metadata policy

Files and directories receive stable, nonzero inode numbers at creation, unique
within one instance. Device 1 and uid/gid 0 identify its single virtual device
and owner; these are never host identities. Inodes survive separate opens,
descriptor duplication, writes, and truncation. File link count is one; directory
link count is two plus its immediate subdirectories. The model has no hard links,
symlinks, unlink, or rename. Read-only directory descriptors are now available
through the [openat/control boundary](HOST-FILE-CONTROL.md).

Image files report mode `0444`; paths explicitly passed in the constructor's
`executablePaths` set report `0555`. That set must name existing image files.
Image directories retain mode `0755`. Mutable files/directories now report
their actual requested creation modes after private umask, and supported chmod
changes are reflected in metadata and execute access; see the
[permission boundary](../src/Host/docs/HostPermissions.md). Legacy managed creation
defaults to requested mode `0600`. The fixed private UID/GID remains zero, and
image metadata stays immutable. Merely placing bytes in an image does not grant
execution.

Regular file size is the current owned byte-array length, including zero-filled
write gaps. Directory size is zero. `st_blocks` reports owned payload rounded
to 512-byte units, and `st_blksize` is the virtual 4096-byte I/O hint; neither
claims physical disk usage or total CLR allocation cost. `st_rdev` is zero.

Creation initializes owned UTC timestamps using the BCL clock. Nonempty read
requests update access time; successful writes and truncation update modification
and change times. Creating a child updates its parent's modification/change
times. A stat query itself does not update timestamps. Precision is the BCL tick
(100 nanoseconds); timestamps do not assert a virtual deterministic or monotonic
clock. The immutable image has no imported source-file timestamp metadata.

`fstat` follows the common descriptor table to its shared open description.
Duplicates therefore observe the same metadata after another descriptor writes,
truncates, or closes. Non-file descriptors return `ENOTSUP`; invalid/closed
descriptors return `EBADF`.

`lstat` has the same result as `stat` because this namespace cannot contain
symlinks. `fstatat` accepts relative paths with `AT_FDCWD` (the current private
root), or absolute paths regardless of dirfd, and optionally
`AT_SYMLINK_NOFOLLOW`. Other flags, including `AT_EMPTY_PATH`, return `ENOTSUP`.
A relative path uses the actual private directory description when supplied;
a bad dirfd returns `EBADF` and an existing non-directory descriptor returns
`ENOTDIR`. Changing cwd remains unsupported. Missing paths, file-as-directory traversal, root escape,
invalid UTF-8, and overlong C paths return their explicit existing errors.
An unbound worker returns `ENODEV`.

## Validation

Run `python3 blink/tests/HostFileMetadata/run.py`. The final passing receipt is
`artifacts/host-file-metadata/attempt-e0a3xiaw/receipt.json`; `latest.json` in that
directory's parent points to the latest successful attempt. The runner snapshots
the complete Host project, C/header inputs, bridges, compiler, and postprocessor,
then uses isolated builds. Raw generated C# remains unchanged; postprocessing
operates on a separate copy. `CS8500` is an error in both consumers.

The native system record, native compilation of the authored header, and actual
emitted layout agree on sizes, alignment, field offsets, and field addresses.
Selected type/permission constants and `ENAMETOOLONG=36` are measured directly
against native headers. A real native temporary filesystem runs the same C
invariants for inode/type/mode/size, descriptor duplication, writes, sparse gaps,
truncation, timestamps, and errors. Numeric inodes, clocks, and physical block
counts are deliberately compared by valid invariants rather than fabricated
cross-filesystem equality.

Raw and optimized JIT/AOT execute the translated C callers through the authored
callbacks and match the native invariant output. Additional managed checks cover
private namespace isolation, execute opt-in, compacting GC, invalid pointers,
unsupported flags/descriptors, UTF-8/path limits, zero reserved bytes, preserved
errno on success, and no binding after worker unbind. Existing `HostFiles` and
`InstanceIo` regression programs also pass against the frozen Host sources,
including socket/shared-fd behavior and disposal.

The metadata matrix was refreshed after directory/control integration at
`artifacts/host-file-metadata/attempt-tcod0686/receipt.json`. The separate control
matrix adds native/managed directory-relative `fstatat` and absolute-path
dirfd-ignore checks.
