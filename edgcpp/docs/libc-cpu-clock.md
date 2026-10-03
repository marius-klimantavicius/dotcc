# Standard C process CPU clock

The `LibcCpuClock` candidate maps `clock()` to BCL `Environment.CpuUsage`.
It sums user and kernel ticks with Int128 before dividing by ten, yielding the
portable ABI's `CLOCKS_PER_SEC=1000000` without separate rounding or TimeSpan
sum overflow. It uses no process object, native handles or managed allocation
on the warmed path. Success leaves translated errno unchanged. Platforms where
the BCL API is unavailable return the C unavailable result `(clock_t)-1`.

CPU accounting belongs to the host process, including all translated engines,
CLR work and other host code. It is not elapsed wall time or per-instance CPU
instrumentation. No new mutable libc state or owner/TLS roots are introduced.

The BCL contract is documented by Microsoft for
[Environment.CpuUsage](https://learn.microsoft.com/en-us/dotnet/api/system.environment.cpuusage?view=net-10.0).
The implementation audit uses official .NET10
[Unix](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Environment.UnixOrBrowser.cs)
and [Windows](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Environment.Windows.cs)
source. The translated ABI does not depend on their private platform layouts.

`libc-cpu-clock-runtime-001` passes strict standalone JIT/NativeAOT exact-tick,
BCL-bracket, CPU-work monotonicity and allocation-free query checks. The public
binding and translated C/C++ static/instance matrix are pending integration;
this document does not claim acceptance of that binding yet. Thread CPU clocks,
clock setting, timers and sleep APIs are separate remaining work.
