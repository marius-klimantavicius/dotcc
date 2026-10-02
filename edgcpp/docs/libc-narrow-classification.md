# Narrow classification and byte collation

The portable provider now implements the twelve C narrow classification
functions, tolower/toupper, isascii, all fourteen POSIX `_l` forms, and
strcoll/strxfrm with their `_l` forms. This closes the narrow `ctype.h` runtime
surface; it does not implement wide-character classification or arbitrary
locale support.

All admitted locale names (C, POSIX, C.UTF-8) classify narrow bytes as ASCII.
This matches the pinned musl 1.2.6 predicates; decoding a multibyte UTF-8 sequence
is a separate API. The defined domain is EOF or a value representable as unsigned
char. Case conversion preserves all other bytes and EOF. Classifiers return 0
or 1; callers must rely on zero versus nonzero, not native libc bit patterns.

The `_l` entry points validate an exact, live, owner-specific locale handle.
Invalid/foreign/global-sentinel handles set EINVAL; classifiers return zero,
case mapping preserves the input, and collation/transform returns zero without
dereferencing string pointers. POSIX does not define invalid-handle behavior;
these are explicit provider diagnostics. Successful calls preserve errno and
have no state-selection side effect. Steady-state methods do not allocate.

Collation compares unsigned bytes. Transform returns the source byte length and
copies the complete string including NUL only when the supplied bound exceeds
that length, preserving musl's behavior. An insufficient destination remains
unchanged; the standard only specifies its contents when the result fits.
A null destination is supported for size zero. Source/destination overlap retains
the C API's undefined behavior contract.

Audited originals and hashes are retained in
`backend/runtime/musl/ctype/source.json` with musl's copyright notice. The managed
predicates preserve their arithmetic; locale wrappers add portable ownership
validation. These originals are provenance, not additional automatic provider
translation units. No BCL Unicode classification table is involved.

Verification entry points:

* `libc_ctype_verify.py`: synthetic native oracle over every EOF/byte value in
  all three locales, normalized classifier results, actual function pointers,
  case mapping, unsigned-byte collation and transform boundaries; C/C++ static
  and owner-instance JIT/NativeAOT modes.
* `libc_ctype_runtime_verify.py`: invalid/foreign/freed handles, errno, exact
  insufficient-buffer write policy and zero managed steady-state allocations.
* `libc_text_verify.py`: existing 3,017 Unicode conversion vectors and owner/
  pinning regression after integrating the additional methods.

Accepted evidence:

* `artifacts/campaign/libc-ctype-translated-20261003-015730/results.json`:
  native oracle and all eight C/C++ static/instance JIT/NativeAOT modes.
* `artifacts/campaign/libc-ctype-runtime-20261003-015430/results.json`:
  runtime JIT/NativeAOT ownership, write bounds, zero allocations.
* `artifacts/campaign/libc-ctype-text-regression-20261003-015000/results.json`:
  existing UTF-8/text JIT/NativeAOT regression.

 Additional strict-signature evidence is recorded in
`libc-clock-abi-20261003-020830/results.json` for wrong classifier widths, locale
pointers and transform count widths. Cross-platform
implementation uses managed integer and pointer operations, but current execution
evidence remains Linux x64.
