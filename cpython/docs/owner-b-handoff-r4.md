# Shared libc handoff — Owner A recovery round 4

Product manifest is unchanged: all 193 units remain required. Do not copy any
native helper oracle into the product. B retains exclusive shared libc/header
and API/harness ownership; A has not edited those paths.

## Required POSIX process times service

Recovered whole-manifest receipt `20261017-180127-1e6ef991` has a second failure,
in `161-posixmodule.log`, after the first SHA2 failure:

```
dotcc does not yet support: local object 'process' requires a complete type: incomplete aggregate 'tms'
```

Same-pin `Modules/posixmodule.c:10732` unconditionally uses `struct tms process`
and `times(&process)` in its POSIX `os.times` implementation. The current target
policy does not enable `HAVE_SYS_TIMES_H`; shared DotCC has no `sys/times.h`,
`struct tms`, or real `times` service. This is a shared header/service gap, not
a grammar failure. No required `os.times` path can be removed or return success
with zero counters to close it.

B action: add the ordinary shared header with four clock_t CPU fields and real
`clock_t times(struct tms *)`, using existing shared runtime/OS binding policy.
Review `_SC_CLK_TCK`/`sysconf` units and child counters. Native/emitted JIT/AOT
controls should check ABI widths/offsets, nonnegative real CPU counters and
elapsed-tick ordering, observable CPU work, return/error behavior, and actual
CPython emission. A will enable the target's `HAVE_SYS_TIMES_H` only once the
shared service/header is present, then rerun all 193 units. All current native,
generated and production artifacts stay preserved.

The exact full source command and snapshot are in the receipt above. A's current
whole retry is `20261017-181448-fcb295be`; it uses the explicit target endian
configuration and C013 typedef repair and may reproduce this independently.

## Test profile integration

A read the completed B round-3 live integration contract in `api-progress.md`.
The 30-module/94-source helper metadata and frozen inventory will be integrated
as the separate translated `upstream-tests-static` profile once product core
closure permits. Its native build remains solely an oracle/generator; native
pyconfig/sysconfig/getpath platform values will not become target configuration.
Seven required native outcomes remain blockers and are preserved as required.


## Service handoff received

B's live round-4 progress now reports shared header/service READY, with native,
emitted JIT/AOT and waited-child CPU controls passing, plus **14 unit / 1
emitted** shared regressions. A enabled `HAVE_SYS_TIMES_H` and `HAVE_SYSCONF`
and started the next full retry after C015. The latter macro is required by
actual `Python/fileutils.c:_Py_GetTicksPerSecond` to use `_SC_CLK_TCK` rather
than its bogus fallback 60. No B-owned file was edited by A. Runtime/downstream
qualification remains open. Existing B thread was steered with exact handoff
and acceptance messages; no task/subagent was spawned.


## First complete raw-build shared-service gaps — new actionable handoff

Receipt `20261017-183944-5dd554f8` emits and links **193/193**, producing 69 C#
files, then raw C# build fails. Complete categorized evidence is
`artifacts/owner-a-r4-20261017/raw-diagnostics.json`; exact shared-service rows
are `shared-libc-diagnostics.json`. Native product/source selection is unchanged.
Actual output root:
`build/campaign/20261017-183944-5dd554f8/raw/TranslatedCPython`.

B owns these actual unresolved shared services/declarations: `fdopen` (2),
`copysign` (27), `setbuf` (3), `wcstombs` (3), `nextafter` (3), `clock_getres`
(2), `fma` (2), `munmap`, `O_NOFOLLOW`, `pause`, `mbstowcs`, `nl_langinfo`,
`CODESET`, `erf`, `erfc`, `exp2`, `utime`, `getpwuid`, `getpwnam`; `struct passwd`
is incomplete and lacks eight accessed fields. Each row has actual generated
file/line/message; retain target unsigned 16-bit wchar and correct errno/
ownership/OS contracts, with native/emitted JIT/AOT reductions. Do not declare
native wchar4 helpers compatible or replace math/random algorithms. Required
header declarations/runtime mappings must be honest; no success-zero stubs.

A separately owns the 514 unresolved `buckets` references, `_PYTHONFRAMEWORK`
configuration, self-initializer scope, fixed-buffer/negation/function-pointer/
qualified-member/goto/DA compiler errors. B must not repair generated C# or edit
A config/compiler. This handoff does not authorize source/module drops. All raw,
processed, interpreter, four-cell and production P6 gates remain open.
