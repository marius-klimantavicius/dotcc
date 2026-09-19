# Private openat and descriptor control

`src/HostIo/HostFileControl.c` provides the ordinary C definitions of the
campaign `fcntl.h` names: `blink_host_open`, `blink_host_openat`, and
`blink_host_fcntl`. They call the fixed C# callbacks in `HostIoBridge.cs`, which
use only the bound `InstanceIo`. Include this C translation unit in the managed
closure alongside that bridge; it is not a guest filesystem implementation or
a replacement for upstream descriptor algorithms.

Actual upstream `fds.c:157` uses `F_GETFL` when initializing a descriptor.
`syscall.c` uses host `F_DUPFD`, `F_SETFD`, and `F_SETFL` behind its guest fd
table, and its virtual-file macros route file opens through `openat`. The
campaign header now declares `openat` and includes native-measured
`F_DUPFD_CLOEXEC=1030` and `O_NDELAY=2048`.

## Directory-relative opens

The C `open` and `openat` paths can open read-only private directories.
`openat(dirfd, relative, ...)` resolves against that real directory description,
including after duplicating the descriptor and closing the original. Absolute
paths ignore dirfd. `AT_FDCWD` resolves from the current private root. Missing,
invalid, and non-directory inputs fail with `ENOENT`, `EBADF`, and `ENOTDIR` as
appropriate; an empty path reports `ENOENT` even with an invalid dirfd.
Traversal above the private root continues to return `EACCES`.

Directory descriptions retain their stable private path and inode. There is no
rename/unlink operation that could invalidate that identity. `fstat` reports
directory metadata and `fstatat` now uses the same relative lookup. Reading a
directory returns `EISDIR`; enumeration and directory seeking remain unsupported.
For compatibility, direct `VirtualFileSystem.Open` / `InstanceIo.OpenFile`
callers retain their previous default rejection of directories and must opt in;
`OpenFileAt` and the C callbacks do opt in.

Supported C open flags are the access mode, `O_CREAT`, `O_EXCL`, `O_TRUNC`,
`O_APPEND`, `O_CLOEXEC`, `O_DIRECTORY`, `O_NOFOLLOW`, and `O_NOCTTY`.
`O_DIRECTORY` requires a directory; combining it with `O_CREAT` is rejected.
The namespace cannot contain symlinks or controlling terminals, so no-follow and
no-controlling-terminal behavior are satisfied by the private model.
Nonblocking, synchronous, direct-I/O, and unknown flags return `ENOTSUP` before
creation or truncation. Creation now consumes the actual optional C mode when
`O_CREAT` is present and applies the private instance umask. Ordinary mode bits
and chmod/chown behavior are qualified by the
[permission boundary](../src/HostPermissions/README.md). Legacy managed callers
retain requested mode `0600` defaults; all C arguments are evaluated normally.

## Descriptor flags and shared status

| Command | Behavior |
| --- | --- |
| `F_GETFD` | Return this descriptor's `FD_CLOEXEC` flag. |
| `F_SETFD` | Set/clear that flag; unsupported bits return `EINVAL`. |
| `F_GETFL` | Return the description's access mode, append status, and retained directory/no-follow lookup flags. |
| `F_SETFL` | Set/clear shared append status and update actual write behavior; access mode and retained lookup flags remain unchanged. Unsupported modes, including nonblocking, return `ENOTSUP` without mutation. |
| `F_DUPFD` | Allocate the lowest free descriptor at or above the supplied minimum, sharing description and position; clear close-on-exec on the new descriptor. |
| `F_DUPFD_CLOEXEC` | The same allocation with close-on-exec set atomically on the new descriptor. |

`O_CLOEXEC` is recorded at open and is never mixed into the shared status word.
Ordinary `dup` also clears it. Closing and reusing a number cannot carry over
the old flag. The shared description owns append status: setting it through
one duplicate affects writes through every duplicate without changing their
original read/write access. The private LP64 profile's `O_LARGEFILE` is zero;
`F_GETFL` does not synthesize Linux's internal `0x8000` bit.

Getters take no variadic argument. Only the supported setter/duplicate commands
perform `va_arg(..., int)` in the C wrapper. Extra caller expressions still
evaluate once through ordinary C calling semantics, including for a getter or
unsupported command. Locking/pointer commands are unsupported and their
arguments are never interpreted as a fabricated integer or lock record.

The owner API `CloseOnExecDescriptors()` closes precisely the marked numbers
while retaining unmarked duplicates and their descriptions. This is an explicit
transition to call after a successful exec decision; no completed guest exec or
process-image replacement is claimed. Socket/readiness implementations and their
partial classes are unchanged. Nonblocking socket requests fail explicitly.

## Allocation limits and remaining gap

Descriptor capacity still applies to the single private fd namespace, including
directory opens and both duplication commands. Exhaustion is checked before
file creation/truncation, and failing operations leave metadata and flags intact.

The subsequent namespace quota change closes the zero-length-file gap. The
owner now accepts `nodeLimit` (default 1024) and `pathBytesLimit` (default
256 KiB), separate from payload and descriptor limits. Root, image files,
implicit image parent directories, and writable files count as nodes. Each
canonical node name is charged once by its UTF-8 byte length, including `/`.
Image namespace overflow rejects construction. New-file quota failures return
ENOSPC before inserting the file or allocating a descriptor; closing a file does
not remove its node charge. Existing opens/truncations do not add another charge.
Disposal clears all accounting. Paths and resolved names also have a 4096-byte
UTF-8 limit, matching the C boundary rather than counting Unicode characters.

These are logical resource limits, not exact CLR heap accounting. Bounded node
counts and name bytes bound retained namespace metadata; temporary allocations,
generic translated malloc, runtime overhead and total worker memory still need
the broader execution/worker limits. They are not claimed as complete instance
allocation bounds.

`python3 blink/tests/HostFiles/run.py` passes the owning BCL module under JIT and
NativeAOT (`artifacts/host-files/attempt-oq71eiku/receipt.json`). It verifies
zero-length create/close exhaustion, unchanged failure state, UTF-8 accounting,
exact image-parent limits and disposal, alongside the previous filesystem cases.
The unified InstanceIo regression consumer also passes. Earlier translated C
control/mapping receipts below retain their earlier source snapshots; this new
receipt does not retroactively change them.

## Evidence

Run `python3 blink/tests/HostFileControl/run.py`; successful receipts are under
`artifacts/host-file-control`, with `latest.json` identifying the final attempt.
The completed control/exec matrix is
`artifacts/host-file-control/attempt-8b1lrt9p/receipt.json`; the metadata refresh is
`artifacts/host-file-metadata/attempt-tcod0686/receipt.json`.
The runner snapshots the complete Host source project, headers, C wrappers,
bridges, compiler, and postprocessor. It preserves raw output and checks
`CS8500` as an error in raw and postprocessed consumers.

Native C and raw/optimized JIT/AOT execute the same supported invariants:
directory-relative and absolute opens, native constants, fd flags, duplicate
minimums, append changes shared across duplicates, retained access modes,
metadata, invalid inputs, and variadic side effects. Comparison intentionally
uses supported status bits rather than Linux's internal large-file bit.
A separate native subprocess performs a real exec and proves that a marked fd
closes while its ordinary duplicate remains readable. The managed owner
transition checks the corresponding retained identity and usable description.

Managed-only assertions cover nonblocking/locking rejection, path isolation,
root escape, fd reuse, compacting GC, socket status preservation, and atomic
failure at capacity. Existing `HostFiles` and `InstanceIo` regression programs
also pass against the copied Host sources. The full metadata raw/optimized
JIT/AOT matrix was separately refreshed after directory integration.
