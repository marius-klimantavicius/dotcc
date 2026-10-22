# PGlite implementation continuation

The campaign remains active through P6. This ledger records intermediate repairs;
none establishes managed SQL acceptance. The coordinator requirements in PLAN.md
remain authoritative. The two shared plan files and PLAN.md contain parent-owned
changes and are excluded from implementation commits.

## Parallel ownership, 2026-10-22

Three Codex `gpt-6.1-sol` implementation tasks use explicit `xhigh` reasoning:

- `pglite-completion-spinlock-r1`: atomic/spinlock headers, overlay helper and
  actual-header contention tests.
- `pglite-completion-initdb-r1`: process-service runtime additions, whole libc
  build, and original initdb/frontend/bootstrap closure.
- `pglite-completion-lifecycle-r1`: resumable yield/exit, owned signal/timer
  services, forwarding boundary and lifecycle tests.

The supervisor owns shared compiler changes/builds, recipe integration,
qualification, combined validation and subsequent assignments. Existing acquired
sources are read only; private output trees separate tasks. No child commits.

## Portable PostgreSQL algorithms

The actual `src/port/pg_bitutils.c` fails with the native GCC-generated
configuration because `cpuid.h` is unavailable, followed by GCC-only syntax.
The normal whole-source retry also exposed unsupported
`__builtin_types_compatible_p` selected by that configuration in `storage/fd.h`.
These are compiler capability selection errors: DotCC does not advertise those
GCC features. `config/portable-codegen.json` explicitly selects PostgreSQL's
existing portable software popcount and slicing-by-eight CRC32C paths, and its
existing non-GCC type-check/cast fallback. The guarded helper modifies only a
private copied `pg_config.h`, after validating every unique original declaration.

The observed native backend manifest remains unchanged as provenance. Two
unconditional hardware CRC implementation/dispatch units are explicitly excluded
from the translated configuration; `pg_crc32c_sb8.c` and `pg_bitutils.c` retain
the actual upstream algorithms. The other native units remain selected, including
conditional architecture files. No database algorithm is handwritten or omitted.

The embedded platform also selects the original two PostgreSQL getopt units,
using their non-system declarations, and the upstream postmaster-liveness pipe
fallback instead of OS parent-death signal registration. A single guarded
adaptation omits regex/regex.h's system-header collision check; its complete
PostgreSQL replacement interfaces and regular-expression algorithms are retained.
The omitted include exists only to force system-name collisions before replacing
them, not to supply any system regex operation.

Receipt `20261022-165858-8d0f2cdf` passes native and actual emitted JIT known
CRC32C/popcount vectors and 771 length/alignment cases against independent simple
bit-by-bit expectations. This receipt predates the later type-compatibility
configuration addition and must be refreshed for final acceptance.

Current integrated receipt `20261022-171904-fa0d4751` passes portable-codegen,
all original/C11/emitted atomic and spinlock controls, the four overlay controls,
and complete actual postgres.c emission without unresolved include diagnostics.
The spinlock controls preserve one-byte layout, 20,000 protected increments,
64 forced sleeping contentions, the original seeded backoff and stuck-lock
failure. This is a boundary milestone; SQL and complete product closure remain
required.

Whole-source receipt `20261022-170022-e1febf33` gets beyond spinlock selection
and fails on `__builtin_types_compatible_p` syntax. After capability correction,
receipt `20261022-170107-1b2dbf11` reaches statement-expression lowering in actual
`brin.c`. That shared compiler repair and full-input retry are active.

## Whole libc and runtime snapshots

The missing `popen`, `pclose`, `optind`, and `getpagesize` names are supplied by
authored runtime additions. Unsupported process creation returns real errors;
the translated initializer must use in-process transport. Receipt
`20261022-165926-aa516316` resolves those names and exposes an unqualified
`Stopwatch` in the concurrent timer resource. The owner corrected it in authored
source; a rebuilt immutable tool and full-libc retry are required. No emitted C#
was edited. Current snapshot paths are recorded in the ignored
`artifacts/supervisor-current-tools.json`.

Required remaining outcomes are unchanged: complete source and initdb closure,
raw/processed products, real managed init/readiness/SQL/transactions/error
recovery, same-process clean close/reopen, owning API and canonical consumer,
JIT/NativeAOT qualification and final reproduction with affected shared tests.
