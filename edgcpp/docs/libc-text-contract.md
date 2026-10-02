# Formatter text dependencies

The new `LibcText` helper provides bounded C/POSIX and C.UTF-8 character
behavior, stable error strings, and ASCII digit classification. It is
now integrated through `libc-text-bindings.def` with exact ABI validation. Its
declarations have been promoted into the active provider headers;
the original draft headers under `backend/tests/libc-text-include/` are retained.
Existing `LibcContext.Memchr` and `Strnlen` already cover the other two requested
formatter dependencies and remain unchanged.

## Ownership and interfaces

Create one `LibcText(Action<int> setErrno)` per generated owner. Connect the
callback to that owner's `LibcContext.SetErrno`, whose state is thread-local.
Dispose the text helper during owner shutdown; no per-thread cleanup is needed
for its immutable strings or stateless encodings.

| API | Contract |
| --- | --- |
| `Strerror(int)` | Borrowed NUL-terminated UTF-8 pointer, stable through subsequent calls and GC until owner disposal. Does not change errno. |
| `SetLocale(int, sbyte*)` | Supports LC_CTYPE=0 and LC_ALL=6; null queries, C/POSIX selects byte mode, C.UTF-8/C.utf8 selects UTF-8. Returns a stable canonical name pointer. |
| `MbCurMax()` | 1 in C/POSIX, 4 in C.UTF-8. |
| `Wctomb(sbyte*, int)` | Uses 32-bit wchar_t and returns encoded byte count or -1/EILSEQ. Null output returns zero and ignores the input character. |
| `IsDigit(int)` | Stateless ASCII 0/1 result; EOF and values outside ASCII digits return zero. |

Error messages and locale names live in one owner-specific pinned byte array,
following dotcc's literal-pool technique. This is global library storage, not a
pinned allocation for each conversion. Error pointers from distinct owners do
not alias. The pool contains musl 1.2.6's 95 message entries using the translated
ABI's generic errno numbers. Unknown, negative and out-of-range errors use
musl's fallback `No error information`; spelling need not match host glibc.
The generation inputs, hashes and license are preserved in
`backend/runtime/musl/errno/` and `backend/runtime/musl/COPYRIGHT`.

Locale selection is owner-wide, matching process-wide setlocale inside a
translated program instance; errno remains per owner/thread. Updates publish
one immutable encoding selection atomically. Other categories, arbitrary
locale names and the empty environment-driven locale selection are rejected
with null/EINVAL and preserve the prior selection. Numeric formatting and
messages remain the C profile. This deliberately bounded interface must not be
advertised as general setlocale, locale_t, uselocale or localeconv support.

## Encoding choice and consulted implementations

Pinned musl `src/multibyte/wctomb.c`, `wcrtomb.c` and `internal.h` define the
selected encoding behavior. In the C locale, ASCII maps directly, and
U+DF80..U+DFFF maps back to bytes 0x80..0xFF. That musl surrogate-escape range
preserves arbitrary byte streams; other non-ASCII wide values fail with EILSEQ.
The reverse decoder remains future work, so this is not yet full multibyte
round-trip library coverage.

C.UTF-8 accepts Unicode scalar values through U+10FFFF, rejects surrogates,
and uses BCL `Rune.TryCreate` plus span-based `EncodeToUtf8`. There is no managed
allocation per character. Failed conversions do not alter the output buffer.
The encodings are stateless, so wctomb's null-output reset returns zero.

Dotcc's `WcharLib` was consulted but uses 16-bit wchar_t and does not implement
the explicit mbstate_t conversion model. Copying that ABI would corrupt this
translator's 32-bit wide values. Dotcc's pinned `strerror` literal pool and
ASCII `isdigit` are appropriate techniques to reuse; their ambient singleton
ownership is replaced by a per-owner pool.

## Synthetic evidence

The standalone test result is
[`artifacts/campaign/libc-text-20261002-230635/results.json`](../artifacts/campaign/libc-text-20261002-230635/results.json).
**3,017 UTF-8 native vectors pass in managed JIT and NativeAOT on Linux x64**.
Additional checks cover all ASCII and C surrogate-escape bytes, invalid Unicode,
zero-allocation repeated encoding, sticky errno on success, per-thread errors,
independent owners/locales, stable pointers under forced GC, unsupported locale
requests and disposal. The repeatable verifier is
`backend/tests/libc_text_verify.py --nativeaot --output-dir <new-directory>`.

The first native comparison is intentionally retained in
`artifacts/campaign/libc-text-20261002-230517/`: host glibc accepts obsolete
UTF-8 encodings beyond U+10FFFF, while musl and BCL reject them. The corrected
oracle compares common Unicode inputs and tests the musl/BCL upper bound as an
explicit contract instead of falsely equating every host extension with the
chosen implementation.

The initial evidence above covers helpers. The later selected formatter
acceptance at `artifacts/campaign/musl-printf-translated-20261002-235819/`
passes generated C/C++ static/instance JIT/NativeAOT using these imports,
canonical function addresses and owner lifecycle. Malformed wchar_t and error
pointer signatures are rejected in `libc-import-abi-20261003-000221`. Windows
and macOS execution remain separate gates.
