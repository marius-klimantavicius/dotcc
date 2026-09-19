# Private file and stream C calls

The opt-in `src/Host/include/host-io.h` boundary maps open, close, dup, seek, read/write and
readv/writev into the authored `HostIoBridge.cs`, compiled beside unchanged
generated C#. It uses the existing per-instance `InstanceIo` descriptor table.
The binding lives in managed thread-local storage, outside C records. Calls
before binding fail with ENODEV and managed exceptions become errno failures.

C calls block only the dedicated interpreter worker. Async operations borrow
bounded owned byte arrays, never pointers into guest or C storage. Each read or
write transfers at most 64 KiB and returns the exact short count. Vectors have
a 1024-entry limit, validate total length against signed ssize_t, and use one
bounded transfer before scattering a successful read. Source bytes and received
bytes cross the pointer boundary synchronously.

Paths are strict UTF-8 with a 4096-byte including-terminator bound. Open supports
read/write/read-write, create, exclusive, truncate and append. Other flags fail
explicitly. The C variadic forwarding function preserves evaluation of an
optional mode argument; virtual files have no host permission bits. Access is
determined by the instance namespace and immutable image ownership. This
boundary never opens a host path.

`tests/HostIo/run.py` compares native C with raw/optimized JIT/NativeAOT for shared
duplicate cursors, vector I/O, sparse zero bytes, failures, optional argument
evaluation and 128 KiB exact byte transfers across legal short operations. The
consumer verifies captured standard streams, missing `/etc/passwd`, explicit
unbinding, forced GC, and two concurrent workers with separate files. All pass
without build warnings. Receipts preserve the compiler/profile/source hashes
and original generated output; the consumer is fully rooted for AOT.

These callbacks currently receive trusted host pointers from translated C.
Invalid *guest* addresses must be rejected by the upstream guest memory/syscall
layer, whose full integration is still pending. Arbitrary host-pointer validity
is not inferred from a non-null check. File metadata, directories, descriptor
flags, nonblocking behavior and complete socket/readiness callbacks remain open.
The opt-in test boundary has not yet replaced the full core's host declarations.
