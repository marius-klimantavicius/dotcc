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

Final implementation and handoff: [PGlite atomics documentation](ATOMICS.md).
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


2026-10-22 spinlock continuation (authorized bounded implementation):

- Added `src/Atomics/s_lock-dotcc-c11.h` with one-byte lock-free C11 storage,
  zero-on-success TAS, acquire exchange, relaxed test-before-exchange,
  release unlock and atomic initialization. `prepare_overlay` automatically
  returns both port defines and copies both authored headers. Three unique
  guarded selection hunks change only private `port/atomics.h` and
  `storage/s_lock.h`. The original S_LOCK and complete s_lock.c algorithms stay
  intact. No recipe or shared compiler edit was made by this implementation.
- Added actual upstream `s_lock.c`/PRNG/bitutils/sleep native/emitted fixtures,
  independent CLR field measurements, four workers/20,000 increments, 64 forced
  real sleeping S_LOCK contentions with protected payloads, seeded backoff and
  adaptive estimate/wait-event checks. Three bounded stuck-lock negative
  processes execute the original test-mode failure branch and exit 1.
  A separate one-waiter design respects upstream process-local adaptive globals.
- Added four overlay guard/preservation tests; they pass against the actual
  configured include closure. Context failures create no destination. Two
  upstream headers change and exactly two authored headers are added; generic
  wrappers and the generic spinlock suffix remain unchanged.
- Initial use of `supervisor-20261022-1700` reproduced its known unqualified
  Stopwatch runtime build issue. That failed receipt is
  `artifacts/atomics-spin-20261022-170215-cb946e/receipt.json`. The older immutable
  integration snapshot then passed native/emitted spinlock controls after the
  fixture supplied only the actual unused CurrentMemoryContext pointer storage.
  These intermediate failures were not repaired in emitted C#.
- The next actual postgres.c failure was reduced to PostgreSQL StaticAssertExpr
  containing do/while in a GNU statement expression. Native returns 42; the
  old compiler rejects DoWhile. Source and diagnostic receipt:
  `tests/Atomics/pg_static_assert_expr.c`,
  `artifacts/atomics-spin-reduction-20261022-170700-623867/receipt.json`.
  The supervisor owns and supplied the structural shared loop repair.
- Final behavioral/full-input receipt:
  `artifacts/atomics-spin-final-20261022-170928-69b2be/receipt.json`.
  Tools: immutable `build/tools/supervisor-20261022-1710/dotcc.dll`.
  Configured source: read-only
  `build/native-control/14abb02f16cfb8013f7c/source`.
  All original native/C11 native/emitted atomic and spinlock controls pass.
  Existing atomic probes retain 20,000 increments and 2,000 fence handshakes.
  Spinlock layout is 1/1, aggregate 32/8, offsets 1/2/8/24; seeded backoff is
  [1414,1744,2168,2307,1000]. Compiler DLL/JSON hashes, ordered flags and all
  original logs are retained. The supervisor's portability helper is applied
  after the atomic overlay to this run's private pg_config.h.
- Actual complete postgres.c now emits its object with exit 0. Its remaining
  unresolved regex.h/getopt.h/sys/prctl.h diagnostics keep the overall suite
  blocked. `atomics/report.json` separately records both port passes, successful
  object emission and unresolved include closure. Do not count compiler exit 0
  as complete source qualification when includes were unresolved.
- Final receipt also retains passing Python syntax/owned diff checks and
  `static-assert-current.log`: the exact actual-header reduction now emits with
  the final immutable snapshot. No shared compiler rebuild was performed here.

Supervisor continuation: the spinlock port is ready for integrated whole-source
retry; existing recipe consumption of prepare_overlay's returned include/defines
already enables it. Keep portability selection after this overlay. Resolve
required system-header/host closure and any new whole-source compiler failures,
then continue P1–P6. This agent retains no campaign-completion responsibility.
Only the explicitly owned atomics paths were written; shared parent files,
reference acquisition, configured stages, compiler outputs and other agents'
files were left to their owners. No commit, stage, push or PR was performed.
