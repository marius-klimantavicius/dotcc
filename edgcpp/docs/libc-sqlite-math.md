# SQLite double-precision math closure

SQLite's existing full configuration enables `SQLITE_ENABLE_MATH_FUNCTIONS`.
The current source translator reached an unresolved address of `exp`; the same
configuration needs the trigonometric, inverse, hyperbolic and power functions.
The selected `sqlite_math` component implements that concrete dependency closure
without changing SQLite's requested feature definitions.

The component contains 28 C translation units from pinned musl 1.2.6: `exp`,
`pow`, `sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `atan2`, `sinh`, `cosh`,
`tanh`, `asinh`, `acosh`, `atanh`, plus the `expm1` and `log1p` dependencies;
six internal argument-reduction/evaluation kernels, two constant tables and
three floating overflow/underflow helpers. Existing selected log/scalbn and
runtime sqrt/fabs/floor support remain dependencies. No float or long-double
exp/trigonometric family or new managed math implementation is introduced.

`backend/runtime/musl/sqlite_math/source.json` records every original, adapted
file and exact patch hash. The release archive is musl 1.2.6, SHA256
`d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a`.
Original sources and their license notices are retained, with the release's
COPYRIGHT. Generic `libm.h` uses the already audited portable ABI configuration;
constant table initializers are reordered to their declaration order for C++
parsing, without changing their values. There is no EDG source change.

## Error contract

Musl's numerical algorithms are retained under private names. Authored public
wrappers implement the current `MATH_ERRNO` profile and restore the caller's
errno on valid non-range results, including infinities and NaNs where specified.
They report EDOM for finite negative-base/nonintegral powers, out-of-domain
inverse functions and infinite ordinary trig inputs. Poles and finite-input
overflow report ERANGE. Power identities, negative zero and infinity cases retain
the original numerical behavior; for example `pow(NaN,0)` and `pow(1,NaN)` return
one, while `pow(-0,-3)` returns negative infinity and reports a pole.

The selected underflow policy reports ERANGE when finite inputs with a nonzero
mathematical result round to zero. Nonzero tiny results preserve incoming errno;
this includes the exact result `pow(2,-1074)`. Underflow error reporting for such
tiny values is an implementation choice in this profile. This is not unchanged
musl errno behavior and does not claim a complete floating environment. Directed
rounding, signaling-NaN behavior and floating exception flags remain outside the
accepted nearest-mode runtime contract.

All public functions are ordinary translated definitions. Function-address
arrays and calls use the backend's existing source callable representation;
there are no name-only imports or unmanaged native-libm callbacks. Existing
owner/thread errno storage is reused.

## Verification

All evidence is under `artifacts/campaign/persistent-continuation/`:

* `libc-sqlite-math-native-002/results.json`: original/adapted numerical agreement
  and replay of all exact patches. The first native run retains a vendor-script
  error that renamed a function spelling in a comment; the corrected patches
  rename the actual definition and leave the original comment intact.
* `libc-sqlite-math-translated-001/results.json`: native comparison and C/C++
  static/instance JIT/NativeAOT, nine checks. Each numerical corpus contains
  71,644 results: arbitrary exponent/sign bit patterns, reduction of very large
  trig inputs, signed zeros, subnormals, poles, infinities, NaNs, values adjacent
  to one and pi/2, overflow boundaries and exact powers. Finite values and signed
  zeros agree exactly; NaN payloads are normalized for comparison. Additional
  assertions check the authored errno policy and owner/thread isolation.
* `libc-sqlite-math-native-extra-001/ubsan.run.log`: the adapted corpus under UBSan.
* `libc-sqlite-math-native-extra-002/results.json`: native C++17 corpus and errno.
  The first C++ host fixture selected a host float overload for the float-typed
  INFINITY macro. Explicit double casts correct that fixture. Portable generated
  headers have only the double declarations, so the accepted generated run
  already exercised the intended double signatures.

The reusable hook is
`verify_libc_sqlite_math(ROOT, TESTS, out, nativeaot, run, translate=True)`.
It detects automatic provider registration and avoids duplicate source inputs.
NativeAOT uses `DebugType=none` and `DebugSymbols=false`; optimization and runtime
semantics are unchanged. Existing evidence is Linux x64; other host execution
coverage remains open. The SQLite SQL-level transcript is a separate integration
check and is not replaced by this numerical corpus.

Registered provider integration now passes the unchanged full-config SQLite
core corpus: native, strict JIT and NativeAOT transcripts all match6,337bytes at
`artifacts/campaign/persistent-continuation/sqlite-core-full-001/results.json`.
The selected provider contains213 translation units. This project check uses
THREADSAFE=0 and the existing deterministic test-only memory VFS; broader
project acceptance remains separate.
