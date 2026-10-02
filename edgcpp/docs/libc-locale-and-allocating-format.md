# Bounded locales and allocating formatting

This slice implements locale objects for C, POSIX and C.UTF-8/C.utf8. It does
not load arbitrary system locales, consult ambient managed CultureInfo, or
silently accept unsupported names. Empty environment-based locale selection
remains unsupported. The design was checked against pinned musl's locale
sources and the locale-object contracts documented by the POSIX/Linux manuals.

## Owner and thread state

`LibcText` owns one `LibcLocale`. Global category selection belongs to that
translated engine owner; `uselocale` selects an object per owner and managed
thread. `wctomb` and `MB_CUR_MAX` use that effective selection. New managed
threads default to the owner's global locale. Generated `ReleaseThread`
releases the selection, and text disposal disposes its locale resources before
LibcContext. No operating-system global locale is mutated.

`newlocale` accepts the six portable category mask bits. Unselected categories
come from the supplied base or C for a new object. Base updates are transactional.
`duplocale(LC_GLOBAL_LOCALE)` snapshots the owner's global selection. Category
queries use canonical names; mixed LC_ALL values use six semicolon-separated
names and can be passed back to restore the selection.

Unknown locale names fail with ENOENT in newlocale; bad masks/handles fail with
EINVAL. Unsupported setlocale selections fail with EINVAL and preserve state.
Successful calls preserve errno. Selection counts prevent freeing or modifying
an actively selected locale, returning EBUSY as a defensive extension for those
otherwise invalid uses. Invalid/foreign currently live handles are rejected by
the owner registry without dereferencing them.

Opaque tokens use NativeMemory. `freelocale` removes the registry record and
frees the token immediately; surviving tokens are reclaimed at owner disposal.
Use of a freed pointer is undefined, especially after allocator address reuse;
there is no claim of detecting every stale-pointer ABA case. This avoids
retaining every freed locale for the entire engine lifetime.

`localeconv` returns owner-specific pinned storage with the portable 96-byte
lconv layout: ten pointers, fourteen signed character fields, alignment 8.
The C numeric decimal point is '.', the other strings are empty, and monetary
fields are CHAR_MAX=127. C.UTF-8 has the same numeric/monetary values. Import
validation checks the complete field names, offsets and types, not just a
matching struct tag. Locale names and all returned lconv pointers remain
stable across GC while their owner is alive.

Selected `strto*_l` source wrappers validate the explicit owner handle and call
the verified numeric parsers. Their supported locales have identical numeric
grammar, so the calls need not change thread selection. LC_GLOBAL_LOCALE is not
accepted as an explicit _l handle; callers can duplicate it into an object.

## One output pass for asprintf

`runtime/musl/printf/format_allocating.c` invokes the selected formatter once
with a growing sink. It does not run a sizing format and repeat it. The existing
musl core's side-effect-free positional argument scan remains unchanged, while
%n executes during the single output pass.

The sink grows geometrically using the shared translated malloc/realloc/free
provider, backed by NativeMemory. It maintains a terminator after each write,
handles embedded NUL bytes by explicit counts, and allocates one byte for empty
output. Failure frees partial storage, returns -1 and leaves the output pointer
null. Count errors and allocator errno propagate. The va_list is handled by the
existing formatter's va_copy/va_end discipline.

The synthetic test deliberately formats a source string and then uses %n to
overwrite that source. A second formatting pass would see different bytes;
the allocating wrapper retains the original output. A positional variant,
embedded NUL, wide UTF-8, geometric growth, invalid formatting, count overflow
and injected allocation failure are also checked.

## Evidence and limits

Initial standalone locale checks pass JIT and Linux-x64 NativeAOT in
`artifacts/campaign/libc-locale-runtime-20261003-012000`. They cover category
restoration, snapshots, base mutation, owner/thread separation, busy/invalid
handles, release, lconv layout and forced-GC pinning. The existing 3,017-vector
text suite also passes after effective-thread-locale integration:
`artifacts/campaign/libc-text-locale-regression-20261003-012525`.

Native allocating formatting passes in
`artifacts/campaign/libc-asprintf-native-20261003-012655`. The initial host
oracle accidentally called glibc fortify wrappers instead of the selected
asprintf definitions. The preserved failure is `...-012525`; disabling fortify
redirection applies only to that native reference build, not generated code.

Generated C/C++ static/instance acceptance is performed by
`backend/tests/libc_locale_translated_verify.py`; standalone checks use
`libc_locale_verify.py`. These tests do not establish arbitrary locale data,
all wide-character APIs, full floating environment support, or execution on
non-Linux platforms. Additional declarations requested by libc++ (isascii,
mbsnrtowcs and wcsnrtombs) remain unbound until implemented separately.

## Final focused acceptance

After immediate locale-resource release was adopted, the standalone JIT and
NativeAOT checks passed again in
[`libc-locale-runtime-20261003-013000/results.json`](../artifacts/campaign/libc-locale-runtime-20261003-013000/results.json).
The native formatter and all eight generated C/C++ × static/instance ×
JIT/NativeAOT combinations pass:
[`libc-locale-translated-20261003-013235/results.json`](../artifacts/campaign/libc-locale-translated-20261003-013235/results.json).
This includes actual _l conversions, one-pass allocating formatting, simulated
allocation failure, explicit generated ReleaseThread, foreign-owner rejection,
active-handle lifetime enforcement and pinned lconv addresses. The initial
host-harness nullable annotation failure is retained in `...-013135`; generated
library compilation itself succeeded there.

Six incompatible import declarations are rejected before managed emission:
[`libc-locale-abi-20261003-013320/results.json`](../artifacts/campaign/libc-locale-abi-20261003-013320/results.json).
These cover complete/fake opaque locale types, mask width, invalid lconv layout,
monetary-field signedness and a void-pointer locale-validator declaration.
