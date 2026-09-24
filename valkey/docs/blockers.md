# Valkey translation blockers

All inputs are the pinned 9.1.2 release. Logs and exact hashes live in the
referenced attempt receipts; downloaded C is unchanged. Each shared repair must
have a reduced regression and a full-closure retry.

## B1 — Unix parameter and logging headers

The first direct server object attempt (`artifacts/probe/server.log`) could not
resolve `sys/param.h` and rejected the resulting missing `BYTE_ORDER` definition.
The full initial probe reported this first for 68 units. A reduced execution
fixture also exposed missing `syslog.h`.

The shared repair adds `sys/param.h` using existing dotcc LP64 types, limits and
endianness, plus declaration-only `syslog.h` constants/prototypes. It does not
invent a logging runtime or claim that Valkey syslog hosting works. The reduced
old-binary failure is `artifacts/probe/unix-parameter-headers-red.log`.
The rebuilt object emits, all 15 `LinuxHeaderTests` pass, and the functional
fixture plus native GCC oracle pass (two checks). Logs share the
`artifacts/probe/unix-parameter-headers-` prefix. Full-source retry
`artifacts/translation/attempt-lehgdnjb/result.json` emits 60/164 units, with 104
remaining failures. The parameter/logging header diagnostics are gone; newly
reachable source exposes the recorded declarator/attribute families. The full
functional suite passes 620 tests with 1,083 optional oracle skips after this fix.

## B2 — GNU allocation annotations

Ten units first fail in `zmalloc.h` on `alloc_size(1, 2)`. The grammar accepts
only one generic attribute argument, and the attribute validator also lacks
the diagnostic/optimization-only `malloc` and `alloc_size` hints. A native-checked
allocation fixture prints `7 0 9`; the old compiler's comma parse failure is
preserved in `artifacts/probe/allocation-red.log`. The repaired grammar accepts
two/three argument annotations without competing with the old special format
production. Validation accepts allocation hints and keeps unknown/layout-changing
annotations as errors. Existing GNU-format tests and allocation execution pass.

## B3 — Declarator and typedef-parameter scope

Native-checked reducers under `artifacts/probe/declarators/` establish failures
for typedef-name parameters, pointer-to-array fields, incomplete outer dimensions
of extern arrays and parenthesized function-form parameters. Shared grammar,
binding and typedef scope repairs preserve row strides and prototype/function
scope. The execution fixture prints `0 7 4 16 8 4 42` under native and translated
execution. Further real-source typedef shadowing still needs investigation;
these reduced passes do not claim all declarators are resolved.

Combined B2/B3, additional POSIX declarations and vfprintf validation passes 32
focused compiler/header tests, 24 stdio/lifetime tests, eight functional tests
(12 optional oracles skipped) and four explicitly executed GCC differential
oracles. Full retry `artifacts/translation/attempt-14em_5wb/result.json` emits
70/164 units, with 94 failures. New leading failures include unsigned preprocessor
limits and packed aggregate layout. The improved count is object emission only.

## B4 — Additional POSIX declarations and vfprintf

The complete closure requires `grp.h`, `strings.h`, `termios.h` and `libgen.h`.
Shared declarations/layouts were checked against native Linux. Existing C-locale
case comparison implementations back strings.h; declarations for other host
functions do not imply a runtime backend. The byte-oriented `vfprintf`/`vprintf`/
`vsprintf` functions and fixed va_list prototypes remove the bundled Lua strbuf
lifetime error, preserve an already advanced cursor and format once. Native and
managed checks cover exact bytes, UTF-8, embedded NULs, return counts, readonly
errors and redirected standard streams. Their actual source-unit retry is part
of attempt-14em_5wb above.

## B5 — Packed layouts and nested array designators

GNU packed structs/unions now cap member alignment separately from source
`#pragma pack` state, and combine with explicit aggregate alignment. The reduced
SDS-style flexible-header fixture checks byte offsets, size, alignment, nested
storage and actual reads/writes. Array element initializers now accept member
designators, including reordered, repeated and omitted members in static and
automatic storage. Native GCC and translated execution agree for both fixtures.
The focused combined batch under `artifacts/probe/designated-arrays/` passes 263
unit checks, seven functional checks and four explicitly enabled GCC oracles
(including preprocessing and POSIX path components). Fourteen optional oracle
cases were skipped. These are reduced checks, not full Valkey execution.

## B6 — POSIX path components

`dirname` and `basename` operate on writable C byte strings and return aliases
into the input or stable libc-owned dot storage. Eighteen runtime checks cover
roots, trailing/repeated slashes, empty/null inputs, UTF-8 bytes, lifetime and
buffer boundaries. The native differential fixture confirms Linux path rules.
This provides the path helpers used by persistence; it does not establish the
remaining filesystem or persistence lifecycle.

## B7 — Preprocessor integer widths and directive expansion

Conditional expressions now use C intmax_t/uintmax_t signedness and conversions,
with short-circuit evaluation. Macro-expanded include names, null directives and
truthful GNU attribute capability queries are supported. This removes the false
64-bit architecture rejection and the subsequently exposed `__has_attribute`
parse failure. The final targeted batch passes 44 checks; five functional checks
and two native GCC oracles pass (also covering the global array declaration-list
repair). Logs live under `artifacts/reductions/preprocessor/`. Real-source retries
emit `rax.c` and `valkey_strtod.c`; `quicklist.c` and `server.c` advance to typedef
parameter shadowing. No target architecture macro is forged to bypass checks.

## B8 — Primitive typedef shadowing and POSIX base types

Typedef rewriting now recognizes primitive and tagged declaration specifiers,
preserves prototype/function scopes and distinguishes grouped callback names.
`unistd.h` exposes `ssize_t` through `sys/types.h`; `sys/ioctl.h` declares Linux
`winsize` and its window-size request constants. These declarations do not add a
successful ioctl backend. The combined batch passes 34 unit checks, three
functional checks and three enabled GCC differential oracles, also covering
explicit pointer-to-array parameters. Logs use the
`artifacts/probe/posix-base-type-headers-` prefix.

Full retry `artifacts/translation/attempt-wq79iazt/result.json` emits 91/164 units.
Of its 73 failures, 67 stop at a newly reachable atomic field whose name matches
a typedef in `server.h`. Other first failures are GNU vector/aligned attributes,
weak functions, an alignment query on a pointer declaration and `sizeof` of an
array type. This is still object-emission evidence only.

## Other observed frontend families

## B9 — Attributes, weak definitions, array sizes and alignment

The next full snapshot emits 154/164 units
(`artifacts/translation/attempt-clliaomh/result.json`). This includes all bundled
Lua units. Remaining first failures are recorded below; successful object
emission does not establish successful C# compilation.

- Unused GNU vector typedefs retain an explicitly unsupported vector type.
  Any use or layout query diagnoses unsupported lowering; the attribute is never
  erased into an ABI-incompatible scalar. The scalar CRC path can now translate.
- GNU alignment on static scalar/array declarations reaches actual managed
  aligned storage. `sizeof(T[N])` accepts constant dimensions and rejects negative
  or unrepresentable bounds, preserving libvalkey's compile-time assertion.
- Weak function definitions are resolved against strong definitions in either
  source or object input order, with canonical callback addresses preserved.
  Weak-only definitions remain callable; duplicate strong definitions and
  unresolved optional weak references fail explicitly.
- `_Alignas` checks the complete declared object, including pointers and mixed
  declarator lists, and combines repeated requests. It no longer queries the
  layout of void when aligning a void pointer.
- Mixed positional/member initializers preserve nested designators, union member
  selection and continuation through nested fields. Aggregate arrays accept the
  universal `{0}` initializer. Buffered typedef parsing recognizes nested
  aggregate fields whose names match typedefs.
- Linux `glob.h`, `sys/mman.h` and `sys/utsname.h` now provide declarations and
  native-checked layouts/constants. This does not provide their runtime backends.

Validation is recorded under `artifacts/reductions/vector-typedef/` (65 units,
four functional and four GCC oracle passes) and
`artifacts/probe/alignas-combined-*.log` (64 units, 38 functional including 22
weak-link checks, and four GCC oracle passes). The latter batch covers the newly
reached initializer, alignment, typedef and header repairs after the 154/164
snapshot. A targeted real-source retry is in progress.

## Initial failure inventory

The initial full probe also records typedef-name parameter shadowing,
pointer-to-array members, incomplete multidimensional extern arrays,
parenthesized function parameters, packed/vector attributes, missing additional
POSIX headers, target-width feature selection, an initialized array in a shared
declarator list, include-macro expansion, nested designators and undeclared
`vfprintf` causing a va_list lifetime diagnostic. These are actual first failures,
not a complete inventory of downstream errors. Record each reduced repair here
before treating any affected unit as resolved.

## Host boundaries after compilation

[host-contract.md](host-contract.md) and [persistence.md](persistence.md) identify
the real module wakeup pipe, event readiness, static Lua registration, per-owner
filesystem/exit behavior and shutdown contracts. Existing libc `fork()` returns
`-1` with `EPERM`, as authorized by the user; it needs no fake success or child
process backend. The initial foreground persistence profile remains required.
