# Portable libc completion ledger

The provider is incomplete. `--libc musl` packages selected translated musl
algorithms with the same BCL bridge used by `--libc dotcc`; it does not supply a
whole native musl installation. The campaign goal remains full libc and C++
coverage. This ledger tracks every C17 header category and major platform domain,
including facilities not yet needed by current libc++ probes.

The machine-readable [inventory](libc-api-inventory.json) records exact runtime
signatures and header declaration status. Regenerate it with
`python3 backend/tests/libc_inventory.py --output docs/libc-api-inventory.json`.
Its prototype census deliberately excludes macros, inline bodies, data symbols,
and language features; a count is not a completeness percentage. Selected source
entries are reviewed public definitions from the package's selected C files (currently 49 after wide integration).

| Domain / standard headers | Implemented evidence | Remaining work |
| --- | --- | --- |
| Memory, allocation, `string.h` | Shared owner NativeMemory heap, aligned allocation, memory and ordinary narrow strings, strerror/strerror_r, strtok/TLS | Audited allocation-failure breadth; POSIX extensions beyond explicit bindings. Byte collation accepted in all generated modes. |
| Character classification, `ctype.h` | Complete narrow classification, casing and explicit-locale forms; all8 generated modes | Unicode wide classification remains separate. |
| Locale, `locale.h` | Owner/TLS C/POSIX/C.UTF-8 objects, category masks, transactional selection, pinned lconv | Environment locale selection, arbitrary locales, actual non-C numeric/monetary/collation data, catalogues. |
| `stdio.h` | Byte FILE I/O, independent standard cells, buffering, seek/EOF/error/ungetc, synthetic append concurrency; selected musl printf family and one-pass asprintf | scanf family, freopen/tmpfile/tmpnam, perror/rewind, orientation and wide I/O, descriptor sharing, wider failure/concurrency/platform acceptance. fpos_t is still byte-only. |
| Numeric `stdlib.h`, `inttypes.h` | Integer/float string conversion including binary80, endptr/errno, supported-locale `_l`, atoi family | Wide numeric conversion; imaxabs/imaxdiv; div structs/calls and broader stdlib operations. |
| `math.h`, `float.h` | IEEE classification, frexp, abs/fabs, sqrt, copysign, integral rounding subset; binary80 exact probes | Most declared transcendental/remainder/power/scaling/decomposition functions; correct errno and directed rounding for all types. |
| `fenv.h`, `complex.h`, `tgmath.h` | No complete provider | Floating environment modes/flags/exceptions, complex algorithms, generic math interface. Existing nearest-mode arithmetic is not fenv support. |
| `wchar.h`, `wctype.h`, `uchar.h` | wctomb C byte escapes and UTF-8; 59 selected musl wide classifier/case/descriptor/string/memory/collation definitions | Resumable multibyte/UTF16/UTF32 packaging, wide numeric conversion, wide stream I/O; broader Unicode/locale repertoire beyond pinned musl. |
| `time.h` | BCL realtime/monotonic clock_gettime, timespec_get, time, difftime; exact counter oracle and all8 generated modes | Process clocks, civil conversion, local zones/DST, normalization, formatting/parsing, broader leap/range/error contracts. |
| `errno.h`, limits/integer/stddef | Portable normalized errno constants; pinned owner/thread errno; LP64 limits/types including least/fast | Broader conformance and architecture profiles. Portable values must be mapped at OS boundaries. |
| `stdarg.h` | Aligned typed pack/cursor, va_copy/end, formatter integration | Continue ABI coverage for all admitted aggregate/extended types; no native platform va_list mixing. |
| `assert.h`, `signal.h`, `setjmp.h` | No full libc header/provider | Assertion termination hooks, portable signals, compiler-assisted setjmp/longjmp with documented managed cleanup semantics. C++ EH does not imply setjmp support. |
| `stdatomic.h`, `threads.h` | Language atomic lowering and separate libc++ BCL thread provider exist | Public C11 atomics/header integration and C threads API, condition/error/cancellation/lifetime semantics. |
| `stdalign.h`, `stdbool.h`, `stdnoreturn.h`, `iso646.h` | Underlying language constructs largely supported | Portable C compatibility headers and dedicated C/C++ acceptance. |
| Remaining `stdlib.h` | Heap, numeric conversion, selected abs helpers | qsort/bsearch and callbacks, rand/state, environment, exit/atexit/quick_exit/abort, system/process model, multibyte APIs. |
| Filesystem / descriptors / process | BCL streams; runtime-selected append and remove/rename adapters | POSIX descriptor ownership, stat/directory/path/filesystem semantics, environment/process/execution; explicit unsupported platform capabilities rather than fake success. |
| Networking, terminals, dynamic loading and other POSIX | No completion claim | Separate scope inventory and portable capability/error model; not covered by the C17 header census. |

The exact signatures in the registry remain authoritative. Unimplemented external
calls fail closed; declarations added for libc++ parsing do not imply runtime
support. Linux JIT/NativeAOT evidence does not establish Windows/macOS execution.

## Pinned libc++ demand inventory

Audited candidate: LLVM libc++ 18.1.8, retained under the acquisition campaign
artifact. These are source dependencies, not evidence of successfully translating
or running whole libc++.

* Core/containers: allocation, memcpy/memmove/memset, string comparison/length,
  numeric conversions, errno, strerror_r, exception/RTTI and thread ABI. Root's
  original `system_error.cpp` next dependency was POSIX int-returning strerror_r;
  it now has synthetic translated acceptance.
* Locale facets (`src/locale.cpp`): narrow ctype and `_l`, strcoll/strxfrm and `_l`,
  wide ctype descriptors and `_l`, wide strings/collation, mbrtowc/wcrtomb and
  mbs[n]rtowcs/wcs[n]rtombs, btowc/wctob, locale objects/lconv, numeric conversion,
  scanning/formatting, time formatting and locale data. No BCL Unicode table can
  silently replace musl tables without defining version and semantic differences.
* Iostreams: byte and wide FILE orientation, fread/fwrite/ungetc and their wide
  counterparts, locking across compound operations, seek and fpos_t conversion.
  Existing byte-only fpos_t cannot silently serve a stateful wide stream.
* Chrono (`src/chrono.cpp`): `clock_gettime(CLOCK_REALTIME/CLOCK_MONOTONIC)` for the
  portable path. Plan BCL DateTimeOffset/Stopwatch with checked seconds/nanoseconds
  conversion, explicit resolution/range contracts, and errno-preserving success.
* Filesystem/time zones: descriptor/stat/directory APIs, local zone conversion,
  and locale/formatting demands beyond the current provider.

Next sequence: extend calendar/time formatting (root-owned slice); complete wide numeric conversion and stream orientation; grow stream
orientation/scanning, math/fenv, and remaining C/POSIX domains. A newly observed
libc++ blocker may change sequencing, never remove a category from the goal.
