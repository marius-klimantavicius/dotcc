# Private filesystem capacity

`HostCapacityBridge.cs` implements the measured `statvfs` and `fstatvfs` C
boundary over the private `InstanceIo` filesystem. It queries no host disk,
mount table, or host pathname. The actual selected POSIX branch of upstream
`blink/statfs.c` calls these two functions and copies their fields into the
guest record. No upstream translation or guest-memory algorithm changes are
required for this boundary.

`VirtualFileSystem.Capacity` validates an existing path through the same locked
canonical resolver as open/stat, or validates an existing file/directory
description. It snapshots the actual quota counters while holding that lock.
The only new retained accounting field is the constructor's already-validated
sum of immutable image payload bytes. InstanceIo retains the common descriptor
namespace and current-directory semantics; duplicate descriptors query the same
filesystem, including after the original closes. Streams and sockets have no
private filesystem and return `ENOTSUP`; unknown or disposed descriptors return
`EBADF`.

| Field | Private meaning |
| --- | --- |
| `f_bsize` | 4096-byte preferred transfer size. |
| `f_frsize` | 1 byte, the actual allocation quota unit. |
| `f_blocks` | Actual immutable image payload bytes plus the writable byte limit. Unused image allowance is not advertised as free space. |
| `f_bfree`, `f_bavail` | Writable byte limit minus current mutable file lengths, including zero-filled gaps. No privileged reserve exists. |
| `f_files` | Configured node limit, including files and directories. |
| `f_ffree`, `f_favail` | Node limit minus current files and directories; root and image parent directories consume nodes. |
| `f_fsid` | 1 within this owner's private namespace; it is not a global host filesystem identifier. |
| `f_flag` | `ST_NOSUID | ST_NODEV`; neither set-ID execution nor device nodes exist in the model. |
| `f_namemax` | 4095 UTF-8 bytes, the maximum component under the root allowed by the resolver's 4096-byte canonical path ceiling. |

Writes, growth, shrink, and truncation immediately change free-byte counts.
Creating an empty file consumes a node without consuming payload bytes. Failed
quota operations leave all counts unchanged. The byte and node reports are
independent: aggregate pathname storage, descriptor limits, per-path length,
and actual CLR allocation failure can still prevent an operation despite
positive free counts. The whole namespace includes writable storage, so an
immutable image file does not make `statvfs` report `ST_RDONLY`. A zero writable
byte limit can still permit zero-length nodes when the other limits allow them.
The quantities describe ephemeral private memory, not disk durability or
physical memory reserved ahead of time.

The bridge decodes strict UTF-8 and requires a NUL within its 4096-byte input
bound. This C boundary therefore cannot query every theoretical maximum-length
canonical absolute name accepted by the .NET API; current-directory-relative
names remain available within that bound. There is no separate handwritten
resolver. Empty/missing paths return `ENOENT`, traversing a regular file returns
`ENOTDIR`, root escape returns `EACCES`, invalid UTF-8 returns `EINVAL`, and
overlong inputs return `ENAMETOOLONG`. Null path/output pointers return `EFAULT`.
Unbound calls return `ENODEV`. Errors preserve the entire output record;
successful filling uses one zero-initialized local record before publishing it,
including zeroed reserved fields, and does not clear errno.

Integration requires the existing `HostIo` owner binding, the additive Host
`InstanceIo.Capacity.cs` source, `HostCapacityBridge.cs`, and optional preamble
header `host-capacity.h`. Existing measured `sys/statvfs.h` prototypes and aliases
are reused unchanged. No C implementation source or additional Begin/End is
needed. No native OS fallback exists.

`tests/HostCapacity/run.py` compares native, authored-native, and emitted record
sizes, offsets, actual address offsets, and flags. Its real native filesystem
oracle checks common path/descriptor/dup/directory/error invariants without
comparing the host's unrelated disk capacities to private quotas. The managed
C callbacks check exact quota values after growth, shrink, failure and creation,
errno/output atomicity, strict pathname errors, disposal/unbinding, sockets, and
two simultaneous bound owners after compacting GC. The Host API checks also
exercise independent node/path limits and the actual 4095-byte component
ceiling. Raw and postprocessed JIT/AOT all run against copied Host sources;
raw generated source remains unchanged and `CS8500` is an error. Existing
HostFiles and InstanceIo regression consumers run against the same source copy.
This evidence qualifies the host boundary, not execution of the complete core.

Qualified receipt: `artifacts/host-capacity/attempt-7y2zni_0/receipt.json`;
native layout/oracle, raw/optimized JIT/AOT, and both existing model regression
consumers passed. The receipt records copied source/compiler hashes, commands,
logs, and AOT binary identities.
