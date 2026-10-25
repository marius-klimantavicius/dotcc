# Managed host implementation

`VirtualFileSystem` is an implemented and independently tested part of the
per-instance host contract. It owns a private Linux-path namespace, clones image
bytes, exposes immutable image files and a bounded mutable file layer, and never
reads or writes the host filesystem. Instances share no file nodes, cursors or
descriptors. Required parent directories derive from the image. Namespace and
metadata operations are implemented in its authored partial classes.
The current public execution contract is documented in
[`MACHINE-API.md`](../src/Managed.Emulation/MACHINE-API.md).

Implemented behavior includes read/write access, create/exclusive/truncate/
append, EOF and short I/O, zero-filled sparse gaps, seek, shared-cursor descriptor
duplication, close, basic stat, bounded image bytes, aggregate writable bytes,
descriptor exhaustion and disposal. Error results use explicit Linux numbers.
Traversal above the virtual root is denied. Intermediate non-directories and
trailing slashes on files fail, and failed open/descriptor allocation cannot
create or truncate a file.

`FileAccessMode` is a typed host contract, not the Linux open-flag wire encoding.
The authored host bridges provide Linux flag translation, pointer validation and
errno marshalling for translated execution. This file module alone does not
start a guest; socket readiness and cancellation have separate owners.

Run `blink/scripts/test-host-files.sh` from any directory through its full path.
The separate consumer checks image ownership, two-instance isolation, shared
duplicate offsets, exact bytes including sparse zeroes, short writes at quota,
resource-failure atomicity, append, invalid paths, overflow, and repeated cleanup.
All four assertion groups pass under Linux x64 JIT and NativeAOT. These are
focused host-module checks; current guest qualification has its own receipts.

## Private TCP contract

`VirtualTcpNetwork` implements an instance-owned TCP namespace using real managed
sockets. A guest bind reserves a virtual port and an independent physical
loopback socket. Two instances can therefore bind the same guest port. Only an
explicit `Publish` exposes a host endpoint; guest endpoint queries retain virtual
addresses and ports. Outbound connections are restricted to listeners in the
same virtual namespace.

The module implements bind/listen/accept/connect, bounded socket buffers,
readiness waits, partial send/receive, shutdown and close. Cancellation is
observed by actual socket operations. Disposal cancels and closes owned sockets
and awaits pending operations before completing, so guest buffers are no longer
borrowed after disposal. No process-global socket table is used.

`test-host-sockets.sh` checks real separate clients, fragmented requests, exact
128 KiB responses, same-port isolation, explicit publication, virtual endpoint
metadata, accept cancellation and reuse, blocked receive closure, real send
backpressure, bounded disposal and descriptor exhaustion. Four assertion groups
pass under Linux x64 JIT and NativeAOT; source/binary/output receipts accompany
both host test suites.

`InstanceIo` supplies the common descriptor table and duplication for files,
sockets and standard streams. Its readiness and bridge layers implement bounded
polling and nonblocking translation. Public network permissions are described
in the machine API; module tests do not qualify a guest service.

## Common instance I/O

`InstanceIo` owns both modules behind one descriptor namespace. Descriptors 0–2
start as private input and captured output/error streams. Open/socket/dup/accept
share one limit; released numbers are reused. Dup shares the open description
and file/input cursor. A socket stays open until its final descriptor closes,
including listening sockets. Failed descriptor allocation cannot create or
truncate a file.

Standard input is cloned and bounded. Output/error have one aggregate byte limit,
report short writes at quota and return explicit errors once full. Captured
snapshots are copies; bounded logs remain readable after disposal. Disposal
releases input/file storage, cancels sockets and awaits pending acceptance and
network I/O before completing. Captured logs remain owned until this managed
object is collected.

`test-instance-io.sh` passes four assertion groups in Linux JIT/NativeAOT for
common descriptor numbers, file/input cursors, output ownership/quotas, resource
failure atomicity, real TCP through duplicated handles, final-close cancellation,
disposal and two-instance isolation.

## CoreCLR mapping support

Memory-file descriptions support sparse memfds and leased shared pages. A
mapping transaction pins one open description across page preparation and
rollback without allocating a guest descriptor. Read-only shared regular-file
pages use mounted guest inode identity, retain the underlying reader, and remain
coherent with successful guest writes and truncation. Rename, unlink, close and
descriptor reuse preserve mapped file identity. Writable shared regular files
and later write-permission upgrades remain unsupported.

Payload, page leases, transaction records and process-map snapshots use the
existing accounting. The memory-file sublimit is 64 MiB and is combined with
the execution owner's backing budget. Additional aliases do not copy payload
or grant additional backing capacity. Closing descriptors and disposing an
instance release their retained references and charges.

`/proc/self/maps` uses a guest page-table snapshot supplied by the execution
owner. Each open owns a read-only immutable snapshot of at most 1 MiB with normal
dup offsets and descriptor lifetime. It reports guest intervals and permissions;
it does not expose host mappings or backing filenames.

[`P6-CORECLR.md`](P6-CORECLR.md) records the actual managed execution
scope, finite fixture profile, retained failures and pending HTTP qualification.
