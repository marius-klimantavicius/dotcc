# Independent atomics experiment progress

2026-10-22: implementation started. Ownership is restricted to
`pglite/src/Atomics/`, `pglite/tests/Atomics/`, `pglite/scripts/atomics.py`,
`pglite/docs/ATOMICS.md`, and `pglite/docs/atomics-progress.md`. No
delegation, shared compiler builds, source-reference mutation, publication,
commits, or changes to the recipe/native-control stage are authorized here.

Read repository `docs/campaigns.md` (the requested `v3/docs/campaigns.md` maps to
this checkout's root), PGlite plan/progress and JSON source/profile/translation
policy. The configured source stage and immutable integration compiler snapshot
are available. PostgreSQL's architecture/compiler selection can admit a guarded
C11 port before existing generic wrappers. DotCC's shared `stdatomic.h` lowers
numeric atomics and fences to the shared `AtomicLib` Interlocked implementation.
Use `_Atomic unsigned char` for the flag to preserve the native x64 one-byte
flag layout; shared `atomic_flag` is four bytes and cannot substitute directly.

Next: implement a private include overlay, native/emitted actual-header behavior
tests, bounded concurrency check, and a diagnostic full `postgres.c` retry. Parent
must review and explicitly integrate the overlay helper. SQL remains untested.

16:30: initial complete experiment receipt
`artifacts/atomics/20261022-163007-06e89f20/receipt.json`. Both actual native
ports and emitted C# JIT pass u32/u64 initialization, reads/writes, CAS success
and expected-value updates on failure, arithmetic/bit operations, flag state,
canaries, barriers, 16 layout values, four workers/20,000 increments and 2,000
payload fence handshakes. Emitted generic CAS loops call shared `Atomic.Load`
and `Atomic.CompareExchange`; byte flag uses `Atomic.Exchange<byte>` and fences
call shared `Atomic.ThreadFence`/`SignalFence`. No emitted C# edits.

Full `postgres.c` retry gets past atomics and stops at the separate
`storage/s_lock.h` unknown-compiler `#error` (spinlock support), despite the
configured profile's historical `--disable-spinlocks` argument. The selected
fork header still unconditionally requires TAS support. This experiment will
not fake `HAS_TEST_AND_SET` or introduce a spinlock bypass. Adding a small
actual-header reduction and documenting the next reviewed port requirement.

Final implementation and handoff: [PGlite atomics documentation](../pglite/docs/ATOMICS.md).
The final behavioral run is
`pglite/artifacts/atomics/20261022-163610-583fad2c/receipt.json`; native original,
native C11, and actual emitted C# JIT behavior/layout/concurrency pass. The final
fixture additionally checks failed-CAS observations retaining high bits for both
widths and independently measures generated CLR struct sizes/field addresses
against the native layout (in addition to the emitted C layout function).
`atomics/report.json` records the independent atomic pass and the blocked
complete-TU outcome separately. The reduced spinlock selection input is
`pglite/tests/Atomics/pg_spinlock_selection.c`: native reports one-byte `slock_t`,
DotCC fails at the same header `#error`. This is a missing port, not an established
compiler defect. No compiler change was made.

Overlay/syntax review receipt:
`pglite/artifacts/atomics/20261022-163336-ffe80722/receipt.json` (passed). Of 940
copied upstream include files, only `port/atomics.h` differs; the sole added
header is the authored C11 port. Missing architecture context, duplicate compiler
context, and source/destination overlap are rejected; context failures create no
destination. The actual generated generic CAS loops, byte flag access and shared
Interlocked-backed fences were inspected alongside execution.

Supervisor next steps: review and integrate `prepare_overlay` into translation
preparation if accepted; keep its include directory first. Decide a separate
reviewed spinlock port preserving TAS/lock semantics and the actual PostgreSQL
contention path, then retry complete `postgres.c`. `run(ctx, suite=None)` retains
a failing complete-input gate. Reproduction and exact command logs are linked
from `pglite/docs/ATOMICS.md`. No recipe integration, native-stage edits, shared
compiler builds/outputs, acquired-reference mutations, shared plan changes,
publication, commits, pushes, or PRs were performed by this experiment.
