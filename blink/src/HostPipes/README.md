# Private anonymous pipes

`HostPipesBridge.cs` binds pipe/pipe2 to `InstanceIo.Pipe` through
`host-pipes.h`. Two fd slots are reserved in the existing per-instance table,
with read-only and write-only descriptions. Both ends are published together;
failed validation/allocation leaves the C output pair unchanged. Supported flags
are O_NONBLOCK and O_CLOEXEC. Unknown flags return EINVAL. No OS pipe, native
handle, host process or unmanaged file descriptor is created.

`VirtualPipes` owns bounded byte rings: defaults are 64 KiB per pipe, 1 MiB total
per instance and 128 active transfers. Capacity can be configured from 4096 to
1048576 bytes. The aggregate byte budget and descriptor ceiling are independent;
exhausted buffer budget returns ENOMEM, missing fd slots EMFILE, and exhausted
transfer slots EAGAIN. Storage remains charged until both ends and any in-flight
transfer leases finish. Constructor limits are explicit; no unbounded queue of
copied write payloads is retained.

Reads consume available bytes and may return short progress. An empty pipe waits
while writers exist, returns EAGAIN in nonblocking mode, and returns EOF after
all writers close and buffered data drains. Writes of at most 4096 bytes are
atomic: they wait for the entire packet or return EAGAIN without writing a
prefix. Larger nonblocking writes may return the actual available prefix;
larger blocking writes continue until complete, or return their transferred
prefix if cancellation/reader closure intervenes. No readers means EPIPE for a
nonempty write. Correct-end zero-length I/O succeeds even after peer closure;
wrong-end I/O returns EBADF even for zero length, matching the native oracle.
The existing vector bridge gathers/scatters one bounded operation, so small
writev packets retain the same atomicity.

Duplicated descriptors share end lifetime and nonblocking status. CLOEXEC stays
per descriptor. Final fd close forbids new operations but existing transfers
lease their original end until completion, even if the fd number is reused.
Disposal wakes and drains blocked transfers and readiness waits; cancellation
without progress returns ECANCELED through the owner API. This is an owner
lifecycle contract, not asynchronous guest signal delivery. The actual upstream
write syscall handles guest SIGPIPE after receiving EPIPE.

Poll reports input data and unconditional HUP on the read end, and output
capacity/ERR on the write end. Writable readiness requires room for a full
4096-byte atomic packet. FIFO fstat metadata uses private inode identity and
mode 0600; seek/positioned I/O returns ESPIPE. Send/recv remain socket-only.
F_SETFL supports pipe O_NONBLOCK; fcntl pipe-size controls, named FIFOs, packet
mode, splice and native signal facilities are outside this boundary.

`python3 blink/tests/HostPipes/run.py` compares a real native pipe oracle with
translated C in raw/optimized JIT and NativeAOT. It checks pipe flags, indirect
calls, FIFO metadata, fcntl, readv/writev, content/short I/O, poll, EOF/EPIPE,
zero-length behavior and failure-output preservation. Copied Host model checks
cover atomic packets, fd reuse during blocked I/O, dup lifetime, cancellation
with partial progress, quota failures, operation limits and disposal including
very long readiness deadlines. Native capacity is not assumed equal to the
private configured capacity. Existing HostFiles/InstanceIo regressions also run.

Final qualification: `artifacts/host-pipes/attempt-73tfaaeh/receipt.json` passed
native comparison, all four managed modes, long-deadline cancellation, and
copied file/I/O regressions. The earlier complete run
`attempt-jxmwon1p` predates the long-deadline refinement and is historical.
