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

## Other observed frontend families

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
