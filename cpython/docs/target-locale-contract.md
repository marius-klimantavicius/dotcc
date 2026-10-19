# CPython target locale and filesystem contract

The unsigned wchar16 managed target selects CPython's existing
`_Py_FORCE_UTF8_LOCALE=1` feature for both complete profiles. Upstream
`Include/pyport.h` derives `_Py_FORCE_UTF8_FS_ENCODING` with an empty replacement
list. The filesystem macro is never independently configured. This uses the
unchanged CPython 3.13.15 sources pinned in [source.json](../config/source.json).
The tested policy is live. Candidate validation and live activation are recorded
separately in `build/owner-b-r42-20261018`; this document alone is not an
activation receipt.

CPython's locale text conversions, `locale.getencoding()`, default locale text
encoding and filesystem text boundary use UTF-8 regardless of `LC_CTYPE`.
Both `current_locale=0` and `current_locale=1` enter the upstream UTF-8 conversion
algorithms. Filesystem error handling remains `surrogateescape`. Supported
strict, surrogateescape and surrogatepass behavior, invalid UTF-8, lone
surrogates, supplementary Unicode and embedded NUL retain the upstream APIs'
individual contracts; a NUL-terminated C API is not a length-aware Python codec.
The filesystem codec must remain UTF-8: upstream asserts that invariant when
initializing its filesystem codec. The authored Host does not expose a custom
filesystem-encoding option.

`PyPreConfig.utf8_mode` remains a real independent option. Public startup defaults
remain bytecode off / UTF-8 mode on; qualification remains bytecode on / UTF-8
mode off. `sys.flags.utf8_mode` reports the supplied option. Enabling the target
feature does not rewrite flags, sources, test IDs, methods or decorators.
The two existing complementary mode-1 witnesses remain required until the
coordinator reviews fresh raw outcomes under the new configuration.

Shared Libc retains its existing C/POSIX ASCII and C.UTF-8 behavior, including
`setlocale`, `nl_langinfo(CODESET)`, `mbstowcs` and `wcstombs`. For example,
CPython `locale.getencoding()` reports UTF-8 while `locale.nl_langinfo(CODESET)`
can still report the lower C locale's ASCII codeset. Collation and numeric/date
locale operations retain their lower locale semantics. This is an explicit
CPython text-boundary contract, not a claim that local Libc is always UTF-8 or
that ordinary native Linux CPython has this default.

The previous generic fileutils path assumes one Unicode scalar per `wchar_t`:
it rejects valid UTF-16 surrogate pairs per unit and splits pairs during
encoding. Whole shared Libc conversion is correct. Selecting only the
filesystem macro would leave the `current_locale=1` hole. The target keeps
unsigned 16-bit wchar and CPython's Unicode representation; it changes neither
the lower service nor upstream algorithms.

Native differential controls must use a new same-pin build with
`CPPFLAGS=-D_Py_FORCE_UTF8_LOCALE=1`. Its native wchar remains 32 bits; the managed
target remains unsigned 16 bits. B37's retained native archive and B41's original
four-case native PASS / target RED describe the old configuration. Neither
supplies parity evidence for new products. The pinned upstream DotCC
`d3678b5c50f86bf65d831ee4efda6134a7894116` configuration is read-only design
reference; its always-UTF8 Libc rationale and execution claims are not this
fork's contract or qualification evidence.

The resolver rejects common/per-unit split/glued `-D`/`-U` overrides for both
macros, defined-zero locale selection, independent filesystem selection,
profile disagreement and header drift. Only the locale value is present in
generated target sysconfig; the filesystem macro is a downstream header
derivation. Embedded assets contain no sysconfig file. Ordered 193/286 source,
46/75 builtins and 188/2329 asset closures remain required.

Policy/native/header controls establish only their measured scopes. A32 owns
normal product regeneration and data/address/lifetime proof; B41 owns fresh
actual four-case and full-suite qualification, complementary mode-1 witnesses
and final matrices. All final per-PE, four-cell and P0–P6 gates must refresh.
Exactly five approved native SKIP/XFAIL exceptions retain raw statuses; no
additional nonpass or unrun case is accepted here. Normative D10 is unchanged.

The new native control completed in 72.447 seconds. Both real UTF-8 modes pass
the original four strict/surrogateescape cases and original two surrogatepass
cases, with unchanged source/method/decorator witnesses. Each mode also passes
158 boundary rows across C/C.UTF-8, both `current_locale` values, strict/
surrogateescape/surrogatepass, BMP/non-BMP, malformed input, lone surrogates and
public locale/filesystem NUL handling. Preinitialization conversion uses the
actual fileutils functions; lower native C multibyte conversion still rejects
non-ASCII. The separate native 104-case run retains 99 PASS, two SKIP and three
XFAIL. Raw outcomes are in
[the native result](../build/owner-b-r42-20261018/native-feature/result.json).

Target-header native layout and four actual V22 object emissions agree before/
after in both profiles; macro-presence guards verify upstream FS derivation.
These are fixture/header controls, not regenerated-product ABI proof.
[Their result](../build/owner-b-r42-20261018/header-controls-signedness-scope/result.json)
retains measured values and emitted functions. The original baseline
`_Static_assert((wchar_t)-1 > 0)` exposes a pre-existing V22 constant-evaluator
limitation; its RED is preserved. Native header signedness and B41's actual
emitted runtime unsigned16 proof remain explicit. No compiler repair or
changed ABI is concealed by the narrower fixture.
