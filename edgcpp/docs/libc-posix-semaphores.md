# Portable unnamed POSIX semaphores

The optional `--posix` runtime implements `sem_init`, `sem_destroy`, `sem_wait`,
`sem_trywait`, `sem_timedwait`, `sem_post`, and `sem_getvalue`. The portable
`semaphore.h` declares an eight-byte owner token. The backend verifies this
record layout and the two-field LP64 `timespec` before emitting runtime casts;
host operating-system semaphore records never cross the translation boundary.

`PosixSemaphores.cs` implements a counting semaphore with BCL monitors and an
owner-specific token table. Tokens are unique across owner instances and are
never reused. Initialization accepts uninitialized `sem_t` bytes. Post/wait
operations synchronize through the same monitor, including publication of
memory protected by the semaphore. Multiple posts retain their count even
when no thread is waiting. `sem_getvalue` returns the nonnegative available
count, including zero when threads are blocked.

Try-wait reports `EAGAIN`; an absolute realtime deadline reports `ETIMEDOUT`;
invalid nanoseconds report `EINVAL` when the operation must wait. An available
token can be consumed immediately without waiting for or validating a deadline.
Timed waits round upwards to BCL millisecond resolution and periodically
recheck realtime so clock adjustments do not turn the deadline into a purely
monotonic timeout. Arithmetic uses `Int128` to support the full portable
signed-64-bit `time_t` range. Posting at `SEM_VALUE_MAX` reports `EOVERFLOW`.

Destroying a semaphore with blocked waiters returns `EBUSY` without destroying
the object; POSIX otherwise leaves this caller error undefined. Destroying a
valid idle semaphore clears its token and removes the runtime entry. Owner
disposal invalidates remaining entries and wakes blocked runtime waits with
`EINVAL`. A BCL thread interruption returns `EINTR` and preserves any available
token for another waiter.

This implementation supports **unnamed, process-local** semaphores. Requests
for process-shared semaphores return `ENOSYS`; named semaphore operations are
not declared or supplied. There is no native signal-handler integration or
claim that the monitor-based post operation is async-signal-safe in a native
signal handler. Native pthread cancellation is also outside this boundary.
These limits do not affect DuckDB's concurrentqueue wrapper, which calls
`sem_init` with `pshared=0` from ordinary execution contexts.

DuckDB's pristine concurrentqueue header remains unchanged. `duckdb/edg/portable.py`
records a source overlay adding `__EDG_DOT_TARGET__` to its existing POSIX
platform branches. That branch then calls this shared runtime. The overlay
does not advertise Linux/Unix CPU features to unrelated project code.

Verification is in `backend/tests/posix_semaphores_verify.py`. The native C
oracle and translated static/instance fixtures check counts, empty/overflow
errors, absolute timeouts, reuse, and four threads protecting 4,000 increments.
Separate BCL controls test cross-owner token rejection, blocked destruction,
interruption, extreme deadlines, and owner disposal. Both fixtures support JIT
and NativeAOT execution. Receipts identify which modes actually ran; the BCL
implementation is cross-platform, while a Linux execution alone does not
establish Windows or macOS verification.
