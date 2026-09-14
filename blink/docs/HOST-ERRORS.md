# Measured host error constants

`config/managed-host/host-errors.h` includes the generic `<errno.h>` and
then supplies the native-measured public errno names missing from that
header. It does not replace `errno` storage or define an operating-system
or CPU identity macro. Every preexisting definition must match the reviewed
number; a mismatch triggers `#error` instead of silently changing the ABI.
The complete numeric table is `error-constants.json`.

The header exposes 134 public error names measured with the native Linux
compiler and libc, including aliases such as `EWOULDBLOCK == EAGAIN` and
`ENOTSUP == EOPNOTSUPP`. This is the campaign's explicit host error ABI,
not a claim that all corresponding host operations are implemented. The
generic error indicator remains the same thread-local runtime storage.
All 59 names in the existing generic header match; 75 names are additions.

This addition changes more than whether a C expression resolves: actual
upstream error translation and description code selects cases using
`#ifdef` on errno names. For example, private callbacks can already return
`ENAMETOOLONG` (36), while the generic header did not define that macro.
The full-core profile must include `host-errors.h` near the beginning of
its binding preamble, before upstream headers or source select error cases.
Standalone fixtures should include it after `<errno.h>`.

## Measurement and qualification

Run `python3 blink/tests/HostErrors/measure.py`. It obtains the native
public `E*` macro list from `cc -D_GNU_SOURCE -dM -E -include errno.h`, then
compiles and executes a program that prints each actual integer value.
Macro text and aliases are not guessed or numerically parsed as constants.
After initial generation, the script refuses to silently rewrite either
the reviewed JSON table or header when native measurements differ. Each
attempt records the macro dump, C source, values, compiler version, and
source hashes. Initial receipt:
`artifacts/host-errors/native-58tflabk/receipt.json`.
The measurement runner also injects an incorrect preexisting
`ENAMETOOLONG` definition and verifies that preprocessing rejects it.

Run `python3 blink/tests/HostErrors/run.py` for a native and four-mode
emitted comparison. The fixture requires every macro with `#ifndef` checks,
prints all 134 values, and compares complete native output with raw and
optimized JIT/AOT consumers of translated C. Thus resolving a bare name
through a runtime constant cannot hide a missing preprocessor definition.
The managed fixture also checks that distinct workers retain independent
`errno` values across forced compacting GC. The runner freezes headers
and compiler identities and preserves raw generated C# before optimizing
a copy; `CS8500` remains a build error.
Receipt `artifacts/host-errors/attempt-s9o3tpn2/receipt.json` records the
native and all four managed comparisons passing.

The profile addition requires a fresh full-core preprocessing identity.
Previously emitted objects with missing macro branches are not qualified
merely because their host callbacks return the same integer values.
