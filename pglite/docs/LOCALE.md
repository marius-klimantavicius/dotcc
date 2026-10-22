# POSIX locale host services

This bounded host implementation supplies the locale interfaces used by the real
PostgreSQL libc provider. It does not replace PostgreSQL collation, casemap,
regular expression, or builtin Unicode algorithms. The selected first product
uses C/C.UTF-8 conventions; arbitrary installed locale names are unsupported.
Managed SQL and complete product qualification remain the supervisor's gates.

## Supported contract

`DotCC.Lib/include/locale.h` declares an ordinary opaque `void *locale_t`.
`DotCC.Lib/include/wctype.h` uses the existing `wint_t`/`locale_t` declarations
and explicitly declares the fourteen implemented scalar `_l` operations. It
advertises no unimplemented descriptor or implicit-locale APIs.
`DotCC.Libc/PosixLocaleLib.cs` implements `newlocale`, `duplocale`, `uselocale`,
`freelocale`, `nl_langinfo_l`, `strcoll_l`, `strxfrm_l`, `strftime_l`, all twelve
narrow/wide classification functions, and narrow/wide simple case conversion.
PostgreSQL's existing static `mbstowcs_l`/`wcstombs_l` adapters still select a
locale with `uselocale` and invoke the managed conversions.

Accepted names are `C`, `POSIX`, `C.UTF-8`, and `C.utf8`. Empty names select C
deterministically, matching this runtime's existing `setlocale` policy; host
environment variables are not consulted. Unsupported names fail with NULL and
ENOENT. Masks use the Linux category ABI (`LC_ALL_MASK = 0x1fbf`); invalid masks,
NULL names, and invalid bases fail with EINVAL. A zero base starts with C in all
categories. Only LC_CTYPE changes character encoding: all supported numeric,
monetary, time, message, and collation conventions remain C. Unselected bases
are modified in place only after all checks succeed. Failed operations preserve
the base; a duplicate is independent of its source and owner-global changes.

Opaque handles are monotonically assigned pointer-sized identities within the
linked runtime, never dereferenced and never reused. Each owner validates its
own dictionary. A freed, disposed-owner, unknown, or foreign-owner token cannot
access another owner's state. Handle storage is managed and is cleared by
`DisposePosixLocales(RuntimeContext)` during runtime disposal. The lifecycle
owner integrated that exact hook in `RuntimeContextLib.cs`.

`uselocale(NULL)` queries; `uselocale(LC_GLOBAL_LOCALE)` restores the owner's
global selection. Thread selections belong to the physical thread and runtime
owner; nested owner bindings retain distinct selections. Changing the global
locale does not override an explicit selection. The two exact existing-runtime
edits are the effective-thread getter in `MultibyteLib.cs` and querying global
state directly from `LocaleLib.setlocale(NULL)`. Narrow classification remains
ASCII for both supported encodings. Multibyte conversion and ordinary
`nl_langinfo(CODESET)` consult the effective selection.

Misuses for which POSIX leaves behavior undefined are diagnosed: freeing or
modifying a locale selected on a live thread leaves it intact and sets EBUSY;
double/foreign frees set EINVAL. A completed physical thread no longer prevents
free. Explicit `_l` operations reject NULL and LC_GLOBAL_LOCALE with EINVAL;
callers needing a global snapshot use `duplocale(LC_GLOBAL_LOCALE)`. Returned
strings and C `localeconv` storage are immutable process-lifetime borrows.

Byte collation is unsigned byte order; `strxfrm_l` returns the full source
length, bounds writes by capacity, and emits a NUL when it fits. The supported
`strftime_l` directive subset is `YyCmdeHIMSpAaBbhjwuFTRDnt%`, shared with the
existing calendar implementation. Unsupported directives fail with 0/ENOTSUP
before writing. PostgreSQL's locale-name cache uses supported `%a/%A/%b/%B`.
`nl_langinfo_l` supports existing C time/numeric/message items, CODESET, and
Linux extended numeric/monetary item IDs. Unknown items return the existing
empty-string/ENOTSUP failure contract. Monetary unavailable bytes follow
dotcc's unsigned-char CHAR_MAX=255, consistently with its existing `localeconv`;
native Linux glibc's 127 sentinel is not claimed equal.

## Native service boundary and wide ABI

C.UTF-8 wide classification and simple casing use one immutable private Linux
libc LC_CTYPE object. Six explicit AOT-compatible pointer/scalar P/Invoke imports
are added: `newlocale`, `freelocale`, `wctype_l`, `wctrans_l`, `iswctype_l`, and
`towctrans_l`. The imported scalar ABI is uint32 `wint_t`, uint64 `wctype_t`,
and opaque 8-byte pointers, restricted to Linux LP64. Initialization validates
all descriptors. Missing libraries/symbols or an unavailable C.UTF-8 service
cause C.UTF-8 handle creation to fail with ENOTSUP; C remains managed. No native
global/thread locale is selected or modified. The immutable successful native
service and its descriptors live for the process lifetime, like literal/name
storage, independently of disposable public owner handles.

These are OS locale host services, explicitly accepted by the supervisor. They
import no database implementation. Unicode classifications, combining-property
membership and simple casing follow the installed libc's C.UTF-8 data, avoiding
an approximate managed Unicode-category replacement. Only scalar values cross
this boundary; no native `wchar_t *` conversion/string API is called.

DotCC's existing `wchar_t *` contract remains UTF-16. Native Linux uses 32-bit
wide units. U+1F680 therefore converts to two managed units and one native unit.
Supplementary scalar classification/casing is nevertheless exact because the
scalar `wint_t` boundary is 32 bits. The actual fixture records both conversion
counts explicitly. This does not qualify arbitrary PostgreSQL libc-provider
wide-string behavior for supplementary text: the selected builtin UTF8 provider
must continue using the original translated PostgreSQL Unicode algorithms.

## Actual source audit

The readonly configured source is the supervisor-owned
`build/native-control/14abb02f16cfb8013f7c/source` at the pin in
`config/sources.json`. Relevant callers are:

- `utils/pg_locale.h`: the actual `pg_locale_struct.info.lt` union field that
  initially prevented `access/hash/hashfunc.c` from parsing.
- `backend/utils/adt/pg_locale_libc.c`: CTYPE/COLLATE masks, two-step base updates,
  simple casing, collation/transforms, and `uselocale` conversion adapters.
- `backend/utils/adt/pg_locale.c`: LC_TIME object and localized day/month names.
- `backend/regex/regc_pg_locale.c`, `like.c`, `like_support.c`: explicit narrow
  and scalar-wide predicates/simple case operations. Provider dispatch remains
  upstream code.
- `port/chklocale.c`: `newlocale(LC_CTYPE_MASK)` and `nl_langinfo_l(CODESET)`.
- `port/pg_localeconv_r.c`: C monetary/numeric formatting. Without GNU extended
  langinfo macros in the shared header, upstream selects its existing
  `uselocale` plus `localeconv` fallback. No branch or algorithm is removed.

## Validation and reproduction

Focused runtime tests are in `DotCC.Tests/PosixLocaleHandleTests.cs`. They cover
stable identities, no token reuse, failed-base preservation, masks/names/errors,
duplicate snapshots, owner/thread isolation, active-thread free rejection,
disposal, strict managed multibyte conversion, supplementary scalar simple case,
bounded transforms, langinfo and explicit unsupported time formatting. Existing
`LibcLocaleTests` remain part of the focused gate. Private build/log locations
are `build/locale-owner-unit` and `artifacts/locale-owner-unit/tests.log`.
Current authored-runtime verification passes 23 focused cases and nine selected
existing CPython encoding/langinfo/invalid-byte/lone-surrogate cases; those two
filters overlap. `multibyte-regressions.log` retains the latter checks.

`tests/Locale/pg_locale.c` uses actual PostgreSQL headers and compares eight
actual/emitted size, alignment and field-offset values, including real CLR
storage. It runs real locale operations and enumerates every one of the
1,112,064 Unicode scalars, retaining a digest over twelve classifications and
two case mappings. The native control reports locale_t size/alignment 8/8,
pg_locale_struct size/alignment 32/8, collate/info offsets 8/16, and lt
size/offset 8/16. The native Unicode digest on this host is
6102733416904788069. Native wchar size/count 4/1 and intended managed 2/2 are
separate observations. The runner builds actual emitted JIT and normally
trimmed NativeAOT harnesses, then retries five actual source units including the
original hashfunc failure. No emitted C# is edited.

```bash
dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release -m:1 \
  --artifacts-path pglite/build/locale-owner-unit \
  --filter 'FullyQualifiedName~PosixLocaleHandleTests|FullyQualifiedName~LibcLocaleTests'
python3 -B pglite/scripts/locale-probe.py \
  --compiler /absolute/supervisor-allocated-immutable/dotcc.dll \
  --source /absolute/readonly/configured/source --mode all
```

The standalone probe reuses the shared campaign Context logger and cancellation,
records the immutable compiler closure and commands, and uses only private
`locale-owner-*` build/artifact directories. It never prepares shared inputs,
builds compiler tools, changes campaign pointers, publishes product artifacts,
or changes the three parent-owned plans. A failed actual-source retry remains
a failed receipt even when the locale subgate passes.

## Allocated snapshot result

`artifacts/locale-owner-20261022-173346-d95d9e82/receipt.json` records the
supervisor-allocated immutable `build/tools/supervisor-20261022-1735/dotcc.dll`.
Native controls, actual emitted JIT and normally trimmed NativeAOT all pass.
`locale/{native,jit,aot}.json` retain the eight equal layout values and equal
Unicode scalar digest. Real CLR union/pointer storage, common C/UTF8 operations,
owner errors, and the deliberately different wchar32/UTF16 conversion counts
are checked by both managed execution modes. All 1,112,064 scalar values are
covered, including supplementary values; native-class predicate bitmasks are
normalized to truth values before comparison.

The original `src/backend/access/hash/hashfunc.c` now emits with exit 0 and no
diagnostic. `pg_locale.c`, `chklocale.c`, and `pg_localeconv_r.c` also emit cleanly.
`pg_locale_libc.c` emits an object with exit 0 but reports unresolved
`#include 'wctype.h'`. The runner treats that unresolved required header as a
failed source-closure gate despite the successful exit. The overall receipt is
therefore **failed**, with `locale_status = passed-native-emitted-all` recording
the separate passing host gate. The supervisor owns that next shared header and
whole-source continuation. No complete product or managed SQL success is claimed.

Snapshot 1735 predates only the final startup self-check of the two scalar
imports. Current authored code additionally probes `iswctype_l` and `towctrans_l`
during service initialization, making missing symbols fail before returning a
public UTF8 handle. Focused current-runtime tests validate that refinement;
final P6 regeneration must include it. The first attempted probe failed a
duplicated `--runtime=c` harness argument; the runner was corrected and the
complete run above is fresh evidence, with no emitted-source repair.

After the supervisor expanded header ownership, the real `wctype.h` described
above resolved that failure. Fresh receipt
`artifacts/locale-owner-20261022-173730-c6ae9211/receipt.json` **passes the bounded
experiment**: native, actual emitted JIT, normally trimmed NativeAOT, and all
five actual source retries, each with exit 0 and no diagnostics. This run uses
the allocated 1735 compiler with an exact copy of the authored `wctype.h` in a
private include directory; all other system headers remain from the immutable
compiler. The receipt records the authored header hash. No emitted file was
changed. There is no remaining failure in this five-unit locale selection;
the supervisor retains full-backend and P6 continuation.

Final integration must rebuild the compiler to embed `wctype.h` and the scalar
symbol startup self-check, regenerate the complete products and rerun the
required campaign gates. The disposal call and two existing locale runtime
hooks are already integrated in authored sources. The bounded locale result
does not qualify the entire PostgreSQL source/import closure or managed SQL.
