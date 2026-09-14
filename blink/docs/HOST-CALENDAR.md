# Instance time and explicit UTC calendar

The calendar boundary supplies C time() through the bound instance realtime
provider. An optional result pointer receives the same whole Unix seconds as the
return value. An injected time of minus one second is preserved, with unchanged
errno; the conventional C return value alone cannot distinguish that valid time
from failure. Unbound calls return minus one with ENODEV.

Both gmtime_r and localtime_r use an explicit UTC policy. The generic dotcc UTC
calendar converter is reused as a pure date operation, rather than its host-local
timezone converter. The output is built in a local record and copied only after
success. Null pointers return EFAULT; dates outside .NET's years 1–9999 return
EOVERFLOW. This is a narrower declared range than every possible native time_t
calendar. UTC zone storage is the generic runtime's stable process-lifetime
four-byte string. No timezone is read from the OS or TZ environment, and guest
configurable timezone interpretation is not claimed.

This supplies actual log.c GetTimestamp's localtime_r call without modifying its
formatting algorithm. It does not qualify logger initialization, output policy,
or complete interpreter execution. Plain localtime/gmtime, timers and sleep
remain separate isolated operations when selected.

Run `python3 blink/tests/HostCalendar/run.py`. Receipt
`artifacts/host-calendar/attempt-b0l41_dd/receipt.json` records the intended native
C fixture and raw/optimized JIT/AOT consumers. Native TZ=UTC0 is set only for the
oracle subprocess. Native timezone initialization runs before errno preservation
checks because glibc's first conversion changed errno while initializing.
Cases cover year1, negative epoch, epoch, leap day2000, year2038, year9999,
overflow, all C89 tm fields, private output preservation and separate injected
worker times across compacting GC. Whole-consumer AOT rooting and CS8500 errors
are enabled; raw generated sources remain untouched.

The initial copied runner selected the older identity fixture and failed its C#
build; that attempt is retained and is not calendar evidence. The corrected
runner explicitly copies tests/HostCalendar/probe.c and Program.cs. No generated
C# was changed to repair that harness mistake.
