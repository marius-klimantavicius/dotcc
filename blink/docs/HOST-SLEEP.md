# Relative sleep and explicit interruption

HostSleep owns at most one active wait and uses the BCL monotonic Stopwatch
clock. It rounds waiting intervals upward and rechecks the nanosecond deadline
before reporting success. Int128 deadline arithmetic accepts every nonnegative
signed64-second request without overflowing a TimeSpan or platform timeout.
Each monitor wait is at most one second and can be awakened immediately.

The worker-bound nanosleep callback copies duration fields before waiting and
retains no translated pointer in the owner. Success preserves errno and leaves
remaining output untouched. Negative seconds or nanoseconds outside0–999999999
return EINVAL; null request returns EFAULT; unbound owner returns ENODEV. An
already-disposed owner returns EBADF and a second concurrent owner wait EBUSY.
Those errors leave outputs unchanged.

Interrupt is a private, coalescing notification: one pending interrupt is
consumed by the current or next wait. It produces EINTR and a normalized
nonnegative remaining duration, written only when the caller supplies output.
Dispose wakes an active wait with EINTR; later waits fail EBADF. No host OS
signal is installed or raised. Separate owners cannot interrupt one another.

This boundary does not implement guest signal delivery or the instance stop
protocol. Actual SysNanosleep calls CheckInterrupt after EINTR and otherwise
retries; a future signal/stop coordinator must enqueue the corresponding guest
state before waking it. Disposing an owner while translated code can retry is
not a substitute for that protocol. SleepTime likewise expects EINTR from an
interrupted valid wait. The explicit profile lacks TIMER_ABSTIME, so upstream
SysClockNanosleep currently uses its original nanosleep/clock_gettime fallback;
no new clock_nanosleep capability is advertised.

`python3 blink/tests/HostSleep/run.py` checks a native real delay and SIGALRM
interruption, then the same C delay/validation/function-pointer invariants plus
private interruption in raw/optimized JIT and NativeAOT. Managed tests cover
long.MaxValue-second cancellation, unchanged errors, queued interrupts, disposal,
concurrent-wait rejection, two simultaneous owners and compacting GC. Passing
receipt is recorded in PROGRESS.md. Native signals are test-oracle operations
only. The product uses BCL synchronization and monotonic time.
