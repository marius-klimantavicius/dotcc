# Private descriptor readiness

`InstanceIo.PollAsync` snapshots up to 1024 caller requests and resolves them
through the common private descriptor table. The authored C bridge copies
measured pollfd records into managed requests and copies revents back only on
success. Negative descriptors are ignored; unknown descriptors return POLLNVAL.
Regular private files and captured streams are immediately ready for the
requested ordinary read/write events, including readable EOF.

Sockets use nonblocking BCL Socket.Poll snapshots. Supported requested events
are POLLIN, POLLOUT, POLLRDNORM and POLLWRNORM. Error/hangup/invalid output flags
are returned independently of the corresponding input flags. Priority and band
events are explicitly unsupported. Native comparisons cover pending accepts,
readable peer FIN, writable TCP, and POLLHUP after both directions have closed.
The contract does not claim complete reset/OOB/ancillary semantics from those
cases; those remain additional qualification work.

An empty snapshot sleeps for at most five milliseconds before checking again,
using monotonic elapsed time for the deadline. It does not spin, consumes bounded
request/result arrays, and supports zero timeout and negative infinite timeout.
Pending polls join the instance's disposal drain: even a poll with no descriptors
and infinite timeout returns a cancellation error after disposal. Closing one
descriptor wakes its waiter through the next snapshot with POLLNVAL. Explicit
cancellation tokens are also supported by the managed API. The C worker blocks
on its owned task; no async task retains a C pointer.

`tests/HostReadiness/run.py` snapshots the entire Host source project, then runs
a native C oracle and raw/optimized JIT/NativeAOT consumers. All pass for invalid
and negative descriptors, timeout, real listener readiness, fragmented requests,
exact 128 KiB responses, FIN/half-close/hangup and descriptor-close/disposal waits.
Two simultaneous translated C servers use a barrier so both private guest-port
8080 listeners are live before clients connect to distinct published endpoints.
The consumer also forces GC and exercises the socket bridge's cancellation.

These are translated C callback tests, not completed x86 guest service startup.
The five-millisecond scan interval is a measured implementation choice to be
assessed in the later performance gate, not a latency-equivalence promise.
Guest memory validation remains upstream, and complete core binding is pending.
