# Portable scheduling imports

`sched.h` supplies `sched_getcpu()` and `sched_yield()` declarations for the
optional `--posix` runtime. CPU affinity structures and operations are not
declared or emulated.

`PosixScheduler.cs` chooses the processor query at runtime:

- Linux calls the native `sched_getcpu` service and transfers failure errno.
- Windows calls `GetCurrentProcessorNumberEx`, then combines its processor
  group and group-relative number as `group * 64 + number` to avoid collisions
  between groups. The native record is a two-byte group followed by two
  one-byte fields. [Windows API and record definition](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-processor_number)
- macOS and other unsupported systems return `-1` with `ENOSYS`; DuckDB can
  use its existing thread-ID fallback in `GetEstimatedCPUId`.

The BCL `Thread.GetCurrentProcessorId()` is deliberately not used for this
POSIX API: its implementation can return a cached scheduling hint, which does
not preserve an instantaneous processor-query contract. [BCL implementation](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/Thread.cs)

`sched_yield` uses `Thread.Yield()` and returns zero. A false BCL result means
that another thread was not selected, not that the operation failed.

`backend/tests/posix_scheduler_verify.py` compares native and translated static
and instance builds under JIT and optional NativeAOT. On Linux it samples CPU
IDs and yields 1,000 times, checking a bound derived from the host's allowed
CPU IDs. It does not compare independently sampled processor numbers or use
`Environment.ProcessorCount` as an ID bound; CPU affinity and container quotas
can make that count smaller than valid host IDs. The same harness checks that
`ioctl(1, TIOCGWINSZ, ...)` reports `ENOTTY` for redirected stdout even when no
stdio import would otherwise register the standard descriptors.

The verification harness currently runs on Linux. Runtime-selected Windows
behavior and the macOS unsupported-service fallback require separate platform
verification before claiming those paths have been executed.
