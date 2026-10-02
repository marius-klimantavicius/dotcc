# Local-time boundary after UTC/calendar acceptance

This is the implementation handoff for the remaining calendar provider, not a
claim that `localtime`, `mktime`, `tzset` or environment handling works yet.
The initial UTC/calendar component is documented in `libc-calendar-port.md`.

Keep musl's `__tm_to_secs`, `__secs_to_tm`, `localtime_r` and `mktime` algorithms.
`mktime` explicitly requests both the active offset and the opposite-DST offset,
adjusts a caller's nonnegative `tm_isdst` hint, and then normalizes the resulting
instant. Replacing it with `new DateTime(..., DateTimeKind.Local)` would lose that
policy and the integer-year range. The internal boundary to implement is:

```c
void __secs_to_zone(long long seconds, int local,
                    int *isdst, long *offset, long *opposite_offset,
                    const char **zone_name);
```

A plain BCL `TimeZoneInfo.GetUtcOffset` call is insufficient by itself. The
adapter must preserve both offsets at overlaps/gaps, seconds precision admitted
by the selected data source, local versus UTC lookup, and behavior before/after
the represented transition range. Synthetic fixtures must specify those policies
and compare with musl's native algorithm before acceptance.

Use separate data-source adapters behind that boundary:

1. Preserve musl's POSIX TZ parser and Gregorian recurring-rule calculation.
   It handles explicit offsets and `M`, `J` and day-number transition rules
   independently of the operating system and BCL date range.
2. Preserve musl's TZif transition search where TZif data is selected. Read bytes
   with BCL file APIs into owned storage; no native `mmap` or host `struct tm` ABI
   is needed. Validate all consumed byte ranges before exposing data to the
   translated parser. Synthetic TZif bytes provide the initial verification
   corpus, including second-resolution historical offsets and extended years.
3. Use BCL `TimeZoneInfo` for the host's default zone when no portable data source
   supplies it. An explicit conversion layer must translate its adjustment rules
   to the same calendar model rather than silently clamp integer years to
   `DateTime`. OS-specific discovery belongs at this selection boundary and is
   selected at runtime. Named-zone availability must be explicit on each OS;
   neither an unknown name nor a failed platform lookup should be confused with
   an accepted complete implementation.

Instance mode needs one environment/timezone owner, including `TZ`, cached
selection, locks, `timezone`, `daylight` and the two `tzname` pointer cells. Read
initial host configuration via BCL, but `setenv("TZ",...)` in one engine must
not alter another engine or the .NET host's process environment. The translated
environment API must be implemented with this same ownership policy; a calendar
only private setter is not final libc coverage.

Timezone name pointers are stable owner metadata, backed by pinned arrays.
Names returned inside `tm` need an explicit validity policy across `tzset` and
owner disposal. Mirror musl's pointer-validation rule for `%Z` without reading
arbitrary pointers. The existing immutable UTC pointer may remain shared across
owners. Temporary parsing buffers and non-global allocations use stack/native
storage. Cache rule/name objects so repeated conversion does not allocate on the
managed heap.

Acceptance must add synthetic northern/southern DST, non-hour offsets, spring
holes, repeated fall hours with each `tm_isdst` choice, year-boundary rules,
POSIX rules outside the BCL year range, malformed/bounded TZif input, concurrent
queries/configuration, independent owners, GC-stable names, exact error behavior,
and C/C++ static/instance JIT/NativeAOT. Add the remaining `localtime`, `ctime`,
`ctime_r`, `tzset` wrappers and wide formatting only with their actual dependencies.
Windows/macOS/Arm64 execution remains required even if Linux synthetic checks pass.
