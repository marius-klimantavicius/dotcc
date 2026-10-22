# Calendar timezone host boundary

The actual managed initdb attempt exposed literal `%Z` timezone text. The C
runtime previously left `%Z/%z` unimplemented and returned a UTC placeholder in
local `tm_zone`. Calendar formatting now uses the caller's real `tm_zone` and
`tm_gmtoff`; unknown DST omits `%z` as on the selected native platform.

On Linux x64, scalar libc `localtime_r` and `mktime` supply actual calendar
fields and abbreviations. A scalar `strftime` import supplies the native zone
fallback only when a caller has no nonempty zone name. These are host calendar
services; PostgreSQL date/time, SQL and storage algorithms remain translated.
No call changes host TZ or locale. Returned zone names are copied into immutable
process-lifetime interned storage so caller-owned tm values survive thread,
owner and garbage collection boundaries. The tm ABI remains size 56 with
gmtoff at 40 and zone at 48. Other platforms retain managed calendar conversion
and use empty unavailable abbreviations instead of invented names.

`strftime_l` advertises the newly implemented Z/z directives alongside its
existing supported subset; unsupported directives still report ENOTSUP. Existing
UTC gmtime spelling remains UTC. Positive/negative half-hour, quarter-hour and
sub-minute offset signs follow native formatting.

The immutable19 receipt
`artifacts/calendar-timezone-owner-20261022-200842-63f9dbfc/receipt.json`
passes native, actual emitted JIT and normally trimmed NativeAOT comparisons in
five isolated child environments: UTC, New York, Kolkata, Kathmandu and St
Johns. All 15 executions agree exactly, including the two daylight-saving
fall-back overlap instants, supplied and missing names, unknown DST, measured
ABI and retained name lifetime. The 54 focused calendar/locale regressions also
pass under default, New York and Kathmandu environments. The earlier immutable18
emitted build failure is retained separately: it was an unqualified name in the
concurrent locale-process runtime addition, corrected in immutable19.

Reproduce with the normal `calendar-timezone` campaign suite or the bounded
helper against an existing immutable compiler:

```bash
bash pglite/scripts/test.sh --suite calendar-timezone --mode all --tools reuse --fetch never
python3 -B pglite/scripts/calendar-timezone-probe.py --compiler PATH/dotcc.dll --mode all
```

This boundary evidence does not qualify the complete SQL product or final P6
matrix. The real original initdb and canonical consumer retries remain required.
