# Selected musl wide scanf

This component supplies six functions: `wscanf`, `fwscanf`, `swscanf`, `vwscanf`,
`vfwscanf`, and `vswscanf`. It has native and all eight generated C/C++ static/instance JIT/NativeAOT
acceptance; automatic provider registration is coordinated separately.

The pinned musl 1.2.6 `vfwscanf.c` algorithm retains format parsing, wide-character
field widths, positional arguments, suppression, scansets, allocation/growth,
assignment counts, and integer stores. The original source, patch, license and
hashes are retained. Reproduction uses `backend/tests/vendor_musl_wscanf.py`.

Native FILE internals become a two-callback wide input cursor. String input
reads directly from wchar_t storage. FILE input calls the accepted wide FILE
operations while one managed monitor covers the entire scan. The managed lock
uses try/finally for foreign exceptions and supports static and explicit-owner
function pointers without a closure allocation.

Musl's numeric arm originally builds a temporary scanf format and calls its
internal byte fscanf on the same wide-oriented FILE. Public byte operations
cannot be used on our wide stream. The adaptation directly reuses the pinned
`__intscan`/`__floatscan` algorithms over the already accepted callback cursor.
Non-ASCII is mapped to a rejecting sentinel only for numeric parsing; the full
wide lookahead value is retained and restored. Counts and limits are wide
characters, and no transcoding buffer is allocated in the implementation.
Binary80 parsing retains the previously documented nearest-even/errno policy.

The reference is the isolated original musl wide algorithm with native FILE
boundary names changed. Its numeric fscanf boundary calls the isolated
original musl byte scanf, intscan and floatscan. Only this native oracle maps
remaining wide input into temporary ASCII storage and advances the original
wide cursor by consumed input. Original/reference and adapted results agree on
271 synthetic records covering malformed input, widths, suppression, positional
arguments, %n, binary80 extrema, Unicode whitespace/scansets, narrow and wide
destinations, and allocation. No whole-library or application translation is
used as acceptance.

Retained evidence:

- ../artifacts/campaign/libc-wide-scanf-native-safe-tail-20261003-053500/results.json
- ../artifacts/campaign/libc-wide-scan-lock-20261003-053200/results.json
- ../artifacts/campaign/libc-wide-scanf-revendor-20261003-053600/results.json

Public stream behavior, EOF/error, C byte escapes and generated ABI rejection
have passed focused acceptance. Wide printf, arbitrary locales, non-nearest floating
rounding and complete libc remain separate open work.

A bounded safety correction reserves at least four bytes for an initial narrow
`%mc` allocation. The pinned original reserves width+1 and can write a four-byte
UTF-8 scalar into two bytes; the retained original ASan reproduction demonstrates
the overflow. The correction follows the explicit maximum UTF-8 scalar width
of this provider, and later growth already reserves the same width. Original
source bytes remain unchanged. Native ASan tests for `%mc` and `%2mc` supplementary
scalars pass after adaptation; the original algorithm remains the differential
reference for the 271 cases not exercising this defect.

- ../artifacts/campaign/libc-wide-scanf-native-safe-tail-20261003-053500/allocated-char-asan.log
- ../artifacts/campaign/libc-wide-scanf-allocated-char-20261003-054000/results.json
- ../artifacts/campaign/libc-wide-scanf-revendor-safe-20261003-053900/results.json

Accepted evidence:

- [271 original/adapted cases plus all eight generated configurations, FILE/va_list/address/C-locale contracts and safe Unicode %mc](../artifacts/campaign/libc-wide-scanf-translated-20261003-055500/results.json).
- [Injected malloc/realloc failures in all eight generated modes](../artifacts/campaign/libc-wide-scanf-failure-20261003-055200/results.json).
- [Eight malformed wide callback/FILE/va_list ABIs rejected before emission](../artifacts/campaign/libc-wide-scanf-abi-20261003-055200/results.json).
- [All271 records under native AddressSanitizer](../artifacts/campaign/libc-wide-scanf-asan-corpus-20261003-054200/results.json).

Executed JIT/NativeAOT targets are Linux x64. Full libc, arbitrary locales and
wide formatted output remain open. Earlier narrow scanf's wide-destination
field-width conformance audit is separate from this wide-input implementation.
