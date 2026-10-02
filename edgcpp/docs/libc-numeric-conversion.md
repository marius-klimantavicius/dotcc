# Selected numeric conversion implementation

The next provider slice uses pinned musl parsers rather than BCL `Parse` or a
binary64 approximation of long double. The source package is
`backend/runtime/musl/strto`, with originals, patches, hashes and license.
Its public translated definitions are `strtol`, `strtoll`, `strtoul`, `strtoull`,
`strtoimax`, `strtoumax`, `strtof`, `strtod`, `strtold`, `atoi`, `atol`, `atoll`
and `atof`. They cover end pointers, optional signs, ASCII whitespace, base
selection, hexadecimal floating input, decimal rounding, overflow and
subnormal results in the selected nearest-even C/POSIX/C.UTF-8 profile.

Explicit C/POSIX/C.UTF-8 locale handles and `strto*_l` wrappers have since
passed generated acceptance, as have localeconv and allocating printf. See
[libc-locale-and-allocating-format.md](libc-locale-and-allocating-format.md).
Other declared functions remain unavailable unless an implementation is supplied.

## Exact helper and dependencies

`LibcScanMath.Remainder` handles the finite operands and positive divisor used
by floatscan. It normalizes the two binary80 significands, reduces using
UInt128 in chunks of at most 64 exponent steps, and repacks the exact remainder,
including subnormals and signed zero. It does not allocate or depend on the
host floating representation. The private import is ABI checked. Eight
thousand native fmodl vectors and allocation checks pass under JIT/NativeAOT:
[`libc-scan-math-20261003-004018/results.json`](../artifacts/campaign/libc-scan-math-20261003-004018/results.json).

Private binary80 scaling uses the already verified LibcFloat.ScaleB. Pinned
floatscan keeps its decimal limb and rounding algorithms. Its fixed 8192-byte
scratch array remains explicit. The overall floating parser still uses the
existing Float80 arithmetic implementation, which can allocate BigInteger
intermediates; no claim of zero-allocation parsing is made.

## Observable contract choices

All numeric values and consumed-byte counts in the first 2,527-case native
corpus agree with the host libc, excluding implementation-specific NaN payload
and sign choices. Evidence and the complete errno differences are retained in
`artifacts/campaign/libc-numeric-native-20261003-004425/host-comparison.json`.
The first corpus exposed two issues that were addressed before generated
acceptance:

- glibc headers could inline atoi as strtol and contaminate the selected-source
  oracle. `-fno-inline` now ensures the native oracle executes musl's wrappers.
- Pinned floatscan omitted ERANGE on some hex overflows. Replayable adaptations
  add overflow errors and select a consistent underflow policy: all nonzero
  subnormals set ERANGE, including exact ones. The parser's zero-underflow
  handling remains; explicit infinity/NaN text leaves errno unchanged.

Invalid conversions retain musl's EINVAL extension. Integer saturation, negative
unsigned conversion, base validation and end-pointer rollback retain the pinned
algorithm. C17 integer grammar is used; no C23 binary-prefix extension is added.
Direct atox overflow has the source language's undefined behavior, so tests of
atoi/atol/atoll use representable values.

The expanded corpus has 2,563 cases with both signs of overflow, exact/inexact
hex subnormals, finite-to-infinity rounding boundaries, invalid and incomplete
exponents, all integer bases, signed/unsigned limits, NaNs and infinities,
deterministic decimal inputs, and 19,000-digit strings crossing the scratch
capacity. Explicit policy assertions inspect the native golden output.
The repeatable verifier is `backend/tests/libc_numeric_verify.py`; every run
retains the selected sources, corpus, golden bytes, commands and logs.

The current evidence is synthetic, Linux x64 and nearest-even. Full fenv and
non-Linux execution acceptance remain separate work. `--libc musl` now selects
the accepted parser/formatter sources automatically; explicit source inputs
are also supported. Declaration availability alone does not implement other
libc functions.

## Generated acceptance

The native oracle and all eight C/C++ × static/instance × JIT/NativeAOT
combinations pass exact result/endptr/errno comparison for 2,563 cases:
[`libc-numeric-translated-20261003-005855/results.json`](../artifacts/campaign/libc-numeric-translated-20261003-005855/results.json).
Twelve additional 19,000-digit nonzero-tail cases also pass all eight existing
executables and agree byte-for-byte with the independent host libc. One input
is just above the binary64 halfway point at 1; its digit beyond the scanner's
scratch capacity correctly makes it round upward:
[`libc-numeric-long-tail-20261003-010130/results.json`](../artifacts/campaign/libc-numeric-long-tail-20261003-010130/results.json).
These supplemental cases are now included in the repeatable verifier, whose
full corpus is 2,575 cases.

The first C++ parse exposed a source adaptation need: musl strtol.c originally
does not include stdlib.h, so its C++ definitions lacked the public C linkage.
Adding the header makes the selected wrapper definitions compatible across
translation units; no EDG change was made. The first failed snapshot and
subsequent synthetic-harness export-name failure are retained in
`libc-numeric-translated-20261003-005550` and `...-005715`.
