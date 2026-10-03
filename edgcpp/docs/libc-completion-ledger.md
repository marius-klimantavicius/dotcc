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
entries are reviewed public definitions from the package's selected C files.
The provider manifest records the current packaged components; accepted candidate
components can be newer than the last committed package milestone.

| Domain / standard headers | Implemented evidence | Remaining work |
| --- | --- | --- |
| Memory, allocation, `string.h` | Shared owner NativeMemory heap, aligned allocation, memory and ordinary narrow strings, strerror/strerror_r, strtok/TLS, selected musl case-insensitive comparisons and explicit-locale forms | Audited allocation-failure breadth; POSIX extensions beyond explicit bindings. Byte collation accepted in all generated modes. |
| Character classification, `ctype.h` | Complete narrow classification, casing and explicit-locale forms; all8 generated modes | Broader locale repertoire beyond the supported C/POSIX/C.UTF-8 profile. |
| Locale, `locale.h` | Owner/TLS C/POSIX/C.UTF-8 objects, category masks, transactional selection, pinned lconv | Environment locale selection, arbitrary locales, actual non-C numeric/monetary/collation data, catalogues. |
| `stdio.h` | Byte FILE I/O, independent standard cells, buffering, seek/EOF/error/ungetc, synthetic append concurrency; selected musl printf family and one-pass asprintf; selected musl narrow scanf/v* with binary80, FILE locking and allocation-failure proof; captured byte/wide orientation and wide FILE operations, version2 stateful fpos ABI, strict obsolete-ABI rejection; selected wide scanf family with 271 original/native records and all8 generated modes; selected wide printf six-API family with native174/ASan and all8 generated configurations; freopen/tmpfile/tmpnam, perror/rewind/setbuf with generated9 + ABI8 + runtime2 (including disposal race and immediate process exit) | Scanf field extents beyond the pinned signed-int width representation; descriptor sharing, wider failure/concurrency/platform acceptance. |
| Numeric `stdlib.h`, `inttypes.h` | Integer/float string conversion including binary80, endptr/errno, supported-locale `_l`, atoi family; nine wide conversions plus seven `_l` forms, 12,277 exact cases in all8 generated modes | Integer abs/div family and aggregates now have native/all8 generated proof; broader stdlib operations remain. |
| `math.h`, `float.h` | IEEE classification, frexp, abs/fabs, sqrt, copysign, integral rounding subset; binary80 exact probes | Most declared transcendental/remainder/power/scaling/decomposition functions; correct errno and directed rounding for all types. |
| `fenv.h`, `complex.h`, `tgmath.h` | No complete provider | Floating environment modes/flags/exceptions, complex algorithms, generic math interface. Existing nearest-mode arithmetic is not fenv support. |
| `wchar.h`, `wctype.h`, `uchar.h` | wctomb C byte escapes and UTF-8; 59 selected musl wide classifier/case/descriptor/string/memory/collation definitions plus20-TU resumable multibyte/UTF16/UTF32 component with exhaustive scalar native and all8 generated modes, owner/thread hidden state isolation; wide numeric parsing with shared cursor; selected wide formatted output with original-core and independent pointer-profile oracles | broader Unicode/locale repertoire beyond pinned musl. |
| `time.h` | BCL realtime/monotonic clock_gettime, timespec_get, time, difftime; musl UTC/civil/local conversion, TZif/POSIX DST rules, mktime/timegm, asctime/ctime, narrow/wide strftime and explicit-locale forms, process CPU clock and strptime; full Gregorian-cycle/extreme-year and all8 generated modes | Local zones/DST and mktime normalization now have pinned TZif/POSIX-rule synthetic native/JIT/AOT evidence; custom unmappable OS defaults and broader leap/range/error contracts remain. |
| `errno.h`, limits/integer/stddef | Portable normalized errno constants; pinned owner/thread errno; LP64 limits/types including least/fast | Broader conformance and architecture profiles. Portable values must be mapped at OS boundaries. |
| `stdarg.h` | Aligned typed pack/cursor, va_copy/end, formatter integration | Continue ABI coverage for all admitted aggregate/extended types; no native platform va_list mixing. |
| `assert.h` | Re-inclusion/NDEBUG/single evaluation; owner stderr diagnostics and uncatchable abort verified C/C++ static/instance JIT/AOT | SIGABRT handlers remain part of the separate signal provider. |
| `signal.h`, `setjmp.h` | Compiler-assisted nonlocal transfer has focused evidence; no complete signal provider | Portable signals and broader nonlocal control-transfer contracts; see the current language plan for VLA restoration evidence. |
| `stdatomic.h` | Public C11 generic APIs plus implicit qualified accesses, exact-width shared atomics, pointer scaling, aggregate/NaN CAS, contention, owner isolation and allocation checks under JIT/AOT | Broader architecture atomic_flag lock-free guarantees, native ABI interoperability and wider memory-model conformance. |
| `threads.h` | Public worker APIs reuse owner-specific BCL service; result/exit/TSS/TLS, mutex/condition/once, native oracles, ABI13, JIT/AOT10 accepted; executable initial-thread join/exit, descendant lifetime and actual-last-thread status0 process finalization accepted10 | Foreign-host/managed Run thrd_exit and signal interruption remain explicit gates. Cancellation is not a C11 thread API. |
| `stdalign.h`, `stdbool.h`, `stdnoreturn.h`, `iso646.h` | Pinned musl compatibility headers; bool, all alternative tokens, local/global alignment and noreturn pointer checks in native and all8 generated modes | Additional standards-version profiles beyond C11/C++17. |
| Remaining `stdlib.h` | Heap, numeric conversion, full integer abs/div helpers, source-backed smoothsort qsort/qsort_r and bsearch with owner/context/exception callbacks; normal/quick/immediate process termination, unified atexit order and executable/managed entry distinction with native/JIT/AOT18 and ABI9 | owner termination, abort signal semantics, POSIX process and descriptor model. Source-backed owner-local environment is accepted; BCL supplies only the initial pinned snapshot. Selected multibyte conversion algorithms have accepted synthetic coverage. |
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
  The version2 fpos ABI preserves wide decoder state and rejects old byte-only source declarations.
* Chrono (`src/chrono.cpp`): `clock_gettime(CLOCK_REALTIME/CLOCK_MONOTONIC)` for the
  portable path. Plan BCL DateTimeOffset/Stopwatch with checked seconds/nanoseconds
  conversion, explicit resolution/range contracts, and errno-preserving success.
* Filesystem/time zones: descriptor/stat/directory APIs, local zone conversion,
  and locale/formatting demands beyond the current provider.

Next sequence: finish C11 thread boundaries; implement remaining formatted output,
math/fenv, process/signals and C/POSIX domains. A newly observed
libc++ blocker may change sequencing, never remove a category from the goal.

Selected PRNG support adds 11 musl source units (provider 129). Eighteen focused
checks cover exact sequences/state ownership, concurrent BSD random state and
automatic packaging; see libc-prng.md. Other libc categories remain open.

Selected fmod/modf families add six source units (provider135), with18 focused
checks recorded in libc-math-parts.md. Dynamic rounding and exception flags
remain absent; this is not complete math/fenv coverage.

Selected musl fmin/fmax/fdim families add nine source units, bringing the
automatic provider to 147. Eighteen checks cover original/adapted native
semantics, all C/C++ static/instance JIT/NativeAOT modes, overflow errno,
owner/TLS isolation, allocation-free binary80 extrema, packaging and exact
patch replay. Full libm/fenv remains open; see libc-math-extrema.md.

Selected logarithm families add 18 source units (provider166). Native original/
adapted results, all eight C/C++ JIT/NativeAOT modes, errno and owner/TLS checks,
packaging and exact patch replay pass18 focused checks; see libc-math-logs.md.
Dynamic floating environment support and remaining libm families remain open.

The authored C17 `system` facade has 17 focused native/JIT/NativeAOT/ABI checks.
Its BCL bridge uses owner `environ` and the FILE service’s captured working
directory, returns explicit BCL exit codes and probes the actual processor.
Child standard handles still belong to the host OS; source `freopen` propagation,
POSIX signal/wait status and broader process integration remain open.
See [libc-system-profile.md](libc-system-profile.md).

The authored system facade brings the provider to167 source units. Its C17
profile uses owner environment and captured working directory, raw BCL exit
codes and host OS standard handles;25 focused checks pass. POSIX signal/wait
semantics and owner FILE-to-child descriptor mapping remain open. See
libc-system-profile.md and evidence/milestone-system.json.

Selected remainder/remquo families add six musl source units (provider173).
Eighteen focused checks cover original/adapted native bits and quotient outputs,
all generated modes, errno, owner/TLS, packaging, GNU aliases and patch replay.
No dynamic fenv or complete libm claim; see libc-math-remainders.md.

The authored synchronous signal component now has 19 focused checks: native
source/host observations, C/C++ static/instance JIT/NativeAOT and strict private
ABI rejection. Owner-specific signal/raise, callback reset/identity, abort and
assertion SIGABRT delivery are implemented, with source longjmp escape and
process-fatal default actions. External OS delivery, POSIX masking/actions and
the backend default C++ terminate-to-abort connection remain open.
See [libc-signals.md](libc-signals.md).

The provider now packages184 source units: nine selected power-of-two scaling
algorithms plus two authored synchronous signal/assertion units were added.
This combined milestone has37 focused checks (scaling9, replay1, signals19,
shared packaging8). OS-delivered signals, POSIX masks and default C++ termination
integration remain separate gates; see libc-signals.md and libc-math-scale.md.
