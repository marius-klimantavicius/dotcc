# Calendar and time-formatting port

Status: the initial UTC/calendar component passes native and translated
C/C++ static/instance JIT/NativeAOT acceptance. Local zones remain unimplemented.
Root owns the new `runtime/musl/calendar` component and any new BCL calendar
service. The libc worker owns stateful multibyte conversion and shared public
headers; the coordinator owns provider registration and compiler integration.

The immediate consumer is pinned libc++18 `locale.cpp`, whose time facet storage
calls `strftime_l`. `libcxx-core-seeds-026` parses the full selected source closure
and reaches that missing binding after the accepted wide and builtin adapters.

Use the pinned musl1.2.6 sources retained at
`artifacts/campaign/musl-feasibility-20261002-214747/musl-1.2.6/`:

- `src/time/strftime.c`: retain directive parsing, ISO week/year calculations,
  padding, recursion and buffer-boundary behavior. It depends on the already
  translated formatting/numeric provider, `__tm_to_secs`, `__nl_langinfo_l`,
  `__tm_to_tzname`, and current-locale selection.
- `src/locale/langinfo.c`: retain the original C/POSIX name/format tables. Replace
  native locale-structure reads with a checked category-mask helper. Current
  supported locales are C/POSIX and C.UTF-8; their time/message names are the
  same, while CODESET and category-name queries follow the selected locale.
- Pure calendar arithmetic (`__year_to_secs`, `__month_to_secs`, `__tm_to_secs`,
  `__secs_to_tm`) avoids BCL DateTime's narrower year range. UTC conversion and
  related formatting can reuse original `gmtime_r`, `timegm`, and wrappers.
- Local-time conversion/timezone names require a subsequent explicit portable
  service. Prefer BCL TimeZoneInfo with documented behavior for large years and
  POSIX TZ selection; preserve instance state and runtime OS selection. Do not
  silently implement local time as UTC or claim complete calendar coverage
  before this boundary is implemented and tested.

`LibcLocale.Categories(void*)` is now supplied by the libc worker. Its private
`__edg_locale_categories(locale_t)` import returns the effective thread/global
mask for null, or a validated owner's explicit handle mask; invalid, foreign or
global-sentinel handles return -1 and set EINVAL. Public `_l` entry points must
still use strict explicit-handle validation; the internal null convention does
not make null a valid public locale handle.

The portable `struct tm` has nine 32-bit integer fields followed by a 64-bit
UTC offset and a name pointer (56 bytes, alignment eight). Private musl field
spellings can map to `tm_gmtoff` and `tm_zone` in compatibility headers. Any BCL
boundary must validate these exact offsets rather than import a host tm layout.
Static name tables and TLS output buffers may use pinned storage; calendar
frames and temporary allocation remain stack/native storage.

Retain original/adapted hashes, license and replayable patches as for the other
musl components. Verification must use synthetic UTC/locale inputs and original
musl/native oracles, including leap centuries, negative epochs, ISO week edges,
extended years, exact/truncated buffers, all supported directives, explicit and
thread-selected locales, owner separation, GC and JIT/NativeAOT. Timezone/DST
tests must be added before local-time coverage is accepted.

## First component and evidence

The initial component contains eleven original musl algorithms/table translation
units and one explicit alias/locale-validation wrapper unit. Original files are
retained under `runtime/musl/calendar/originals`; `source.json` pins both versions
and `calendar-managed.patch` replays with zero fuzz (`libc-calendar-patch-replay-001`).
C++ compatibility changes only spell casts, rename the C variable `new`, provide
private declarations and replace native weak aliases with checked wrappers.

`libc-calendar-native-002` compares the original and adapted algorithms against
one synthetic fixture. Both produce digest `222821310091552590`: all 146097 days
in a Gregorian cycle, normalization, integer-year extremes, ISO weeks, directive
padding, small buffers and all locale-table entries. The native shim supplies
only the two admitted locale categories and registered UTC name; it does not
claim an oracle for local timezone behavior.

The selected `LibcCalendar` boundary holds one immutable pinned empty/UTC name
array, shared safely across owners. Its pointer validation matches musl's rule:
an arbitrary `tm_zone` pointer is not dereferenced. Translated `gmtime` and
`asctime` static buffers follow the generated class's global-storage mode.

`asctime_r` retains musl's abnormal-termination behavior for an output that cannot
fit its required 26-byte buffer. `LibcProcess.Abort` calls BCL `Environment.FailFast`
without managed unwinding; this is not a complete `abort`/SIGABRT signal provider.
Signal registration/delivery remains a mandatory separate gate. Calendar tests
run the overflow path in child processes and require failure before any managed
`finally` marker. Initial generated attempt `libc-calendar-translated-001` is
retained: it discovered the missing process-termination binding, not a calendar
algorithm failure.

The dotcc calendar/environment implementations were also inspected
(`DotCC.Libc/CalendarLib.cs`, `StdlibLib.cs`, `RuntimeTerminationLib.cs`). Their
BCL integration is useful, but importing those calendar bodies directly would
narrow the supported year range to `DateTime`, use process-global timezone
selection and retain the current UTC placeholder for local names. This port
keeps musl's integer calendar arithmetic and formatting, and reserves BCL for
the actual platform boundary. A future owner-aware timezone/environment service
must not mutate the process environment merely to select one engine's TZ.

## Wide formatter component

`runtime/musl/wide_calendar` keeps original `wcsftime.c` and replaces its weak
alias with a checked `wcsftime_l` wrapper. It depends on the narrow calendar
helper, the separately accepted wide numeric scanner (`wcstoul`), resumable
multibyte conversion and wide string/memory functions. No platform calls or BCL
DateTime formatting are introduced. Original/adapted native synthetic formatting
matches digest `13444143990234568375` (`libc-wide-calendar-native-001`), including
Unicode literals, extended years, flags, locale selection and buffer limits.
Its patch replays with zero fuzz (`libc-wide-calendar-patch-replay-001`). `libc-wide-calendar-translated-002` now passes nine checks: this native oracle
and all eight generated C/C++ static/instance JIT/NativeAOT configurations. The
first attempt exposed a missing `stdlib.h` declaration in the private adapter;
that include is fixed and both native harnesses now reject implicit function
declarations. The component is ready for selected-provider integration.

The first generated calendar compile (`libc-calendar-translated-002`) exposed
compatible C array types mapped to different nominal C# storage wrappers at
`__strftime_fmt_1(char (*)[100],...)`. A reduced fixture also covers returned,
conditional and assigned array pointers and indirect calls. Both original EDG
C/C++ oracles pass (`array-pointer-compat-native-001`); the coordinator owns
array-storage type interning. This is a backend representation issue, not an
EDG or musl algorithm change.


`libc-calendar-translated-003` passes nine checks after the backend array-storage
fix: original/adapted native oracle plus C/C++ static/instance JIT/NativeAOT.
The generated digest matches `222821310091552590`, including all Gregorian-cycle
days and extreme integer years. Host probes verify independent global buffers
and locale selection, per-thread locale selection, pinned UTC names across GC
and owners, rejected invalid/foreign `_l` handles, and abnormal-asctime child
termination without managed `finally`. All commands, outputs and child traces
are retained. `array-pointer-compat-translated-001` separately passes ten checks
(two EDG C/C++ oracles and eight generated modes); it does not establish
unknown-bound array-pointer conversion coverage.

The accepted narrow component is registered in the 84-TU provider. The
coordinator separately verifies selected-provider packaging. Adding the two
accepted wide formatter TUs brings the provider to 86; this remains selected
libc coverage, not completion of calendar/timezone or libc support.
