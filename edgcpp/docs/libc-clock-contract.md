# BCL clock contract

The portable clock slice provides clock_gettime, timespec_get, time and
difftime. The only admitted clock IDs are CLOCK_REALTIME=0 and
CLOCK_MONOTONIC=1. This does not implement clock setting, process/thread CPU time,
clock_getres, civil calendar conversion, time zones or the rest of time.h.

The LP64 boundary requires an exact global `struct timespec` with two signed
64-bit fields named tv_sec and tv_nsec, offsets 0 and 8, size 16 and alignment 8.
The registry validates this layout before any managed wrapper is emitted; a
same-size arbitrary pointer does not qualify. Runtime pointers refer to the
portable struct, never a native OS timespec. Ordinary function addresses use the
same generated canonical wrappers as direct calls.

Realtime uses DateTimeOffset.UtcNow and subtracts the Unix epoch in 100 ns ticks.
Integer floor division produces normalized seconds plus a nonnegative nanosecond
remainder, including pre-epoch values. Timestamp representation has 100 ns units;
actual resolution depends on the BCL/host and is not claimed to be 100 ns.
Monotonic uses Stopwatch.GetTimestamp and Stopwatch.Frequency without doubles.
Remainder-to-nanosecond scaling uses Int128, so multiplication cannot overflow and
large counters retain low bits. The epoch is the underlying timer's unspecified
origin. There is no owner-relative startup epoch and no promise to count suspended
time identically across OS timers.

The same managed implementation runs on all supported platforms, delegating
platform selection to the BCL. The pinned .NET 10 sources show Unix's native
GetTimestamp boundary and Windows QueryPerformanceCounter. If a runtime falls
back to a non-high-resolution wall clock, the monotonic request returns ENOTSUP;
it does not silently weaken monotonicity. A negative/wrapped monotonic counter
returns EOVERFLOW. Unsupported IDs return EINVAL. A null output pointer returns
EFAULT as a defensive extension. Failure leaves output untouched; successful
calls preserve the current owner/thread errno.

Timespec_get accepts only TIME_UTC=1, returning that value on success. An unknown
base returns zero and leaves errno/output unchanged. Time accepts an optional
output pointer. Difftime subtracts in Int128 before converting the difference to
double, preserving adjacent seconds at extreme signed time_t values and avoiding
signed overflow. None of the steady-state operations allocate managed or native
storage. The lazily created runtime object retains only the owner's errno callback.

[Stopwatch API](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.gettimestamp),
[.NET 10 Unix implementation](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Diagnostics/Stopwatch.Unix.cs),
[.NET 10 Windows implementation](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Diagnostics/Stopwatch.Windows.cs),
[DateTimeOffset ticks](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.ticks?view=net-10.0).

Standalone evidence:
`artifacts/campaign/libc-clock-runtime-20261003-020350/results.json` passes JIT and
NativeAOT, with 10,105 exact C __int128 reference vectors, boundary/negative times,
monotonic loops, realtime bounds, errno ownership, extreme difftime and zero
steady-state allocation checks. Generated evidence:
`artifacts/campaign/libc-clock-translated-20261003-020830/results.json` passes the
native fixture and all eight C/C++ static/instance JIT/NativeAOT modes, including
real public function pointers, host realtime bounds, monotonic ordering and
owner/TLS errno. `libc-clock-abi-20261003-020830/results.json` rejects eleven
incompatible clock/narrow-locale declarations before emission. Tests are
synthetic; Linux x64 execution does not establish Windows/macOS acceptance.

With this generated acceptance, libc++'s vendor configuration may define
`_LIBCPP_HAS_CLOCK_GETTIME` as a real portable capability. There is no need to
pretend that the translated target is Linux to choose this path.
