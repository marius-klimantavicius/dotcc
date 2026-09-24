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
preserved in `artifacts/probe/allocation-red.log`. Repair and validation are in
progress; layout-changing attributes must not be silently discarded.

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
