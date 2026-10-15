# Complete SQLite inventory (T0)

T0 passed on 2026-10-15 from committed implementation
`bcb86a08e24d983cd654904c4833f46c62d15c87` in an isolated sparse checkout. Root
independently verified the complete report and accepted this inventory gate.
SQLite translation and SQL execution remain unimplemented.

The immutable full-feature input, exact target/configuration and numerical
budgets are pinned by [input.json](../acceptance/sqlite/input.json). The public
reader admits its actual format 2.0/lowered/native profile. No source or raw
database query supplied semantic facts to the consumer.

| Inventory item | Observed count |
| --- | ---: |
| Functions | 6,552 |
| Retained executable definitions | 4,212 |
| Body operation identities (deduplicated per body) | 592,512 |
| Variable/parameter storage descriptors | 25,623 |
| Type descriptors | 17,003 |
| External declarations | 2,359 |
| Required external function/intrinsic requirements | 103 |
| Required external variables | 0 |
| Required family/form rows | 132 |
| Required string values / retained string values | 2,226 / 2,230 |
| Diagnostics | 0 |

Counts are inventory observations, not dynamic execution counts. Family counts
can include an identity seen from a body and a separate dependency root; the
body-operation count remains the identity-deduplicated body traversal. Metadata
backing expressions are not execution roots. The dependency closure follows
constant-address bases, aggregate/repeat elements and actual storage/function
references. Independent genuine fixtures prove constant-base string closure,
cyclic globals, current storage classification, and normal/metadata/exceptional
cleanup separation.

The full report is preserved at
`/mnt/storage/roslynobj-csharp-20261015/t0/sqlite-inventory-address-closure/program-report.json`:
36,482,699 bytes, SHA256
`f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185`.
The [compact summary](../acceptance/sqlite/inventory-summary.json) records exact
source/build/input hashes, target, commands, family ledger and focused evidence.
The [import audit](../acceptance/sqlite/import-audit.json) records all 103 actual
requirements, selected signatures and the referenced ABI type descriptions.

Inventory used 97.00 seconds internally (97.82 seconds measured command wall
time), 1,964,974,080 bytes of translator process peak working set and
2,229,796,864 bytes of waited process high-water RSS. Report output is below
512 MiB and both memory measurements are below 8 GiB; wall time is below the
1,800-second budget. The reader issued 2,561,206 commands and read 3,897,892 rows.
These are one SQLite-unit inventory measurements, not DuckDB/scale acceptance.

The import audit separates 15 recorded compiler intrinsics from 88 other
requirements. Intrinsics need target-operation implementations (including
overflow, byte swaps, leading-zero count, atomic loads/stores and a fence), not
blind native symbol lookup. Candidate provider groups are explicitly
unimplemented: 15 memory/string functions, 24 math functions, 10 pthread
functions and 39 other libc/POSIX functions. No executable provider bindings
have been established; ownership, errors/errno, threads and callbacks remain
per-import acceptance work.

Nine required captured GNU assembly names retain a literal
`__USER_LABEL_PREFIX__` prefix. The pinned recipe forwards no definition for
that macro, and the backend stores EDG's actual assembly name directly. Preserve
these exact identities. The audit lists explicit candidate host aliases to
`open64`, `stat64`, `fstat64`, `ftruncate64`, `fcntl64`, `pread64`, `pwrite64`,
`mmap64` and `lstat64`; none is an established executable binding. Each still
requires selected-signature/layout and Linux/glibc boundary tests. No implicit
prefix stripping, source-name lookup, producer change or SQLite re-export is
authorized by this finding.

Actual bodies require 498 indirect calls, 1,043 variadic calls, 37 defined
variadic signatures, 41 volatile fetches, arrays/unions/bitfields, exact floating
values, pointer arithmetic, comma/conditional expressions, switch and labels/
goto. Seven generated variadic definitions expose addresses:
`sqlite3_snprintf`, `sqlite3_log`, `sqlite3_str_appendf`, `sqlite3_mprintf`,
`sqlite3_vtab_config`, `sqlite3_db_config` and `sqlite3_test_control`. Native
`fcntl` and `mremap` addresses also require a deliberate variadic ABI boundary.
Retained unused builtin address flags do not make those declarations required.

The largest retained bodies include `sqlite3VdbeExec` (19,570 operations) and
`yy_reduce` (11,354). These guide later compiler/resource tests; no large-method
emission or build success is implied by inventory. T2 implementation follows
[the reviewed representation direction](T2-CONTRACT.md); T3/T4 remain open.
