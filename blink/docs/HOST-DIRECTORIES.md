# Private directory streams

The private directory module implements actual `fdopendir`, `readdir`,
`rewinddir`, and `closedir` operations over existing VFS nodes and descriptor
descriptions. It also provides coherent `opendir`, `dirfd`, `telldir`, and
`seekdir` operations. There is no host filesystem enumeration.

At pinned upstream revision `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`,
`syscall.c:Getdents` calls `VfsOpendir(fd->fildes)`, which maps to `fdopendir`
under the selected VFS exclusion. It repeatedly reads names/inodes, aligns
guest records, and copies those records through existing guest-memory helpers.
`SysLseek` rewinds an existing stream for offset zero. `close.c:CloseFd` calls
`closedir` instead of the ordinary close callback once a stream exists. The
selected profile does not enable `HAVE_SEEKDIR`; providing tell/seek callbacks
does not change that feature selection or advertise nonzero guest directory
seeks. No upstream guest-memory or instruction algorithms are changed.

## Storage and long names

The retained supplied `DotCC.Lib/include/dirent.h` deliberately differs from
native Linux dirent storage. This module preserves its identity, including the
frozen core object declarations:

| Record/field | Supplied profile |
| --- | --- |
| `DIR` | 8-byte complete opaque record, alignment 8; no CLR reference. |
| `struct dirent` | Size 272, alignment 8. |
| `d_name[256]` | Offset 0. |
| `d_ino` | 8 bytes at offset 256. |
| `d_type` | 1 byte at offset 264. |

The native Linux record measured by the runner has size 280, inode at offset 0,
type at offset 18, and name at offset 19. Native system records are never passed
to the private adapter. The binding header statically asserts every fixed
private offset and both record sizes, rejecting future ABI drift at compilation.
The runner separately compiles the supplied profile with native cc and verifies
its sizes, offsets, and actual field addresses against managed emission. The
owner fills only that measured profile record, zeroing it before publishing
each next entry. `d_type` is 4 for directories or 8 for regular files; inode
numbers are the existing VFS metadata identifiers, including real `.` and `..`
node identities. No `DT_*` macros or shared profile headers are changed.

The VFS can contain components up to 4095 UTF-8 bytes, but this retained C
record holds at most 255 plus NUL. Acquisition checks the whole immediate
listing and fails with `ENAMETOOLONG` if an entry exceeds 255 bytes. It does so
before descriptor ownership transfers. Names are never truncated and a late
error cannot silently appear as EOF in upstream Getdents, whose loop does not
distinguish all readdir errors. Invalid Unicode in a VFS name is also rejected.
This limitation applies to this directory boundary; it does not shrink VFS
pathname storage or claim full directory access to all VFS names.

## Snapshot and ownership contract

Each successful acquisition takes a fixed snapshot: `.` and `..` first, then
immediate children sorted by ordinal name. Root's `..` uses root's inode.
Rewind returns to the same snapshot. Newly created nodes appear only in new
streams. Separate streams, including those acquired from duplicate descriptors,
have independent snapshot cursors; this is an explicit private policy, not a
claim that native buffered directory streams over a shared kernel offset have
identical cursor behavior. Tell returns the next snapshot index, and Seek
accepts only indices from zero through the snapshot length. Invalid positions
leave the cursor unchanged and set `EINVAL`. EOF returns null without changing
errno or the previously returned record.

`fdopendir` retains an internal description reference without allocating another
guest descriptor or consuming the descriptor quota. The original fd remains
usable for upstream metadata checks and is owned by the successful stream.
Failure drops any temporary internal lease and leaves the caller's fd unchanged.
`opendir` opens a private read-only directory fd with close-on-exec bookkeeping;
its failure closes only that newly opened fd.

Closing a stream closes its original fd only if the descriptor table still
points to the acquired description, then releases the internal lease exactly
once. Existing duplicate descriptors remain valid. External close or `dup2`
replacement invalidates subsequent stream reads; closing that invalid stream
returns `EBADF` while releasing its retained lease, without closing a reused
descriptor's new description. Unknown, foreign-owner, or already-closed tokens
return `EBADF` without dereferencing the supplied address. InstanceIo disposal
cancels stream operations; directory-owner disposal still frees their storage.
Descriptor leases are explicitly checked against their originating InstanceIo.

## Bounds and lifecycle

`HostDirectories` bounds each context to 32 active streams, 128 total successful
registrations, 4096 active snapshot entries, and 262144 active UTF-8 name bytes
including NULs. Stream/registration exhaustion returns `EMFILE`; snapshot entry
or byte exhaustion returns `ENOMEM`. The VFS itself also retains independent
node, pathname, and descriptor quotas. Failed acquisition preserves those
counters and descriptors. Closing releases snapshot entries/name storage and
the 272-byte returned-record allocation. The 8-byte token is retained as a
tombstone until owner disposal, so an old token cannot refer to a newly opened
stream within the same context. Total token storage is at most 1024 bytes.

All C storage uses NativeMemory, contains only bytes/integer identities, and
survives compacting GC. The managed registry owns the description references
and snapshots. An entry pointer is borrowed until the next read or stream
close; token/entry pointers must not outlive owner disposal. Explicit disposal
is required; there is no finalizer freeing memory behind an untracked C pointer.
The invocation count and allocation bounds do not represent host disk space.

Create `new HostDirectories(io)` and call `Blink.BindHostDirectories(owner)` on
the same worker that binds HostIo. A second binding throws without replacing
the first. After upstream cleanup/exit callbacks and all C borrowers stop,
unbind and dispose the directory owner before disposing InstanceIo. Dispose
and unbind are idempotent. Unbound C calls return `ENODEV`; a bound disposed
directory owner returns `EBADF`. Caches or tokens from a previous owner must be
discarded before binding a new one. Each worker uses a separate bound owner;
the module does not change the lifetime rules for other upstream global state.

Integration uses `src/HostDirectories/HostDirectoriesBridge.cs`, preamble header
`host-directories.h`, and Host project sources `HostDirectories.cs`,
`InstanceIo.Directories.cs`, and the additive VFS snapshot method. No authored
C implementation source, native library, shared header, or compiler change is
needed. The header supplies explicit `DIR*` and `dirent*` prototypes for the
new binding names while retaining the supplied record declarations.
The bridge uses the shared `BLINK_FULL_CORE` conditional to select containing
class `BlinkCore` for the full consumer, keeping `Blink` for focused fixtures.

## Qualification

`tests/HostDirectories/run.py` records both native and supplied record layouts,
then compares a real native directory oracle with raw/optimized JIT/AOT C
fixtures against copied Host sources. The C fixture uses the upstream
Getdents-style strlen/aligned-record/full-buffer copy loop with guard bytes and
independent record decoding. It verifies names, nonzero inodes, fdopendir
failure ownership, dup lifetime, rewind, native valid tell/seek cookies, EOF
errno, and path errors. This is a host-boundary loop, not execution of the
unchanged complete Getdents function inside an emitted Machine.

Private checks verify deterministic snapshots, inode/type bytes, 255/256-byte
names, external fd replacement, independent duplicate cursors, cross-owner
lease refusal, stream/registration/entry/name-byte quotas, close/disposal and
error behavior. Two simultaneous worker fixtures hold actual translated TLS
`DIR*`/`dirent*` pointers across compacting GC and call back through the C
directory functions. Existing HostFiles and InstanceIo regression consumers
also run against the copied Host implementation. Raw source is retained and
hashed unchanged; a separate copy is postprocessed, and `CS8500` is an error.

Final qualified receipt:
`artifacts/host-directories/attempt-wkfwdrzj/receipt.json`. Native common
invariants, supplied-profile native layout/static assertions, all four managed
modes, and both existing model regression consumers passed. This receipt
includes the full-core class conditional, actual translated TLS pointer GC
checks, independent snapshot byte/entry quotas, and descriptor flag checks.
