# PostgreSQL C11 atomic and spinlock compiler ports

The guarded private overlay passes actual native and emitted C# JIT controls
for PostgreSQL atomics and one-byte spinlocks. It retains upstream generic
wrappers and the complete `s_lock.c` contention/delay algorithms. The actual
`postgres.c` object now emits successfully, while required include closure
remains blocked. No translated SQL or product qualification is claimed.

Current evidence is under
`artifacts/atomics-spin-final-20261022-170928-69b2be/` with matching private
`build/atomics-spin-final-20261022-170928-69b2be/` outputs. The receipt deliberately
fails because DotCC reports three unresolved headers despite returning exit 0.
`atomics/report.json` separately records both `atomic_status` and
`spinlock_status` as `passed-native-and-emitted-jit`, plus the actual object
emission exit code. Tool selection is the immutable
`build/tools/supervisor-20261022-1710/dotcc.dll`; its DLL/JSON hashes are recorded.
Earlier atomic-only evidence remains in VALIDATION.md and
[atomics-progress.md](atomics-progress.md).

## Scope and implementation

Inputs are the exact revision selected in `config/sources.json`, the existing
configured `embedding-native` stage, and the immutable integration compiler
snapshot. The report retains selected paths and compiler DLL/JSON hashes.
The acquired reference, configured native stage, and compiler outputs are read
only. All authored files live in this experiment's owned directories. Emitted
C# is generated and executed without manual editing.

`src/Atomics/generic-dotcc-c11.h` supplies PostgreSQL's required compiler-port
primitives using standard `<stdatomic.h>`:

- `_Atomic uint32` and `_Atomic uint64` storage, initialization, relaxed
  reads/writes, and strong seq-cst CAS with expected-value updates on failure.
- A one-byte `_Atomic unsigned char` flag using atomic exchange/load/store.
  PostgreSQL returns true when acquiring a clear flag; the exchange's previous
  value is tested against zero. Using DotCC's `atomic_flag` would change this
  native x64 layout from one byte to four, so it is deliberately avoided.
- Signal/thread fences for compiler/full/read/write barriers. DotCC's existing
  shared runtime strengthens the requested memory orders to seq-cst
  Interlocked operations; there is no local atomic runtime implementation.

The port leaves the actual pinned `port/atomics.h` public wrappers and
`port/atomics/generic.h` algorithms intact. Exchange, fetch add/sub, bitwise
and/or, add/sub-fetch, membarrier access, and monotonic advancement use the
original generic CAS loops and wrappers. `_Atomic` qualification preserves
atomic accesses to `ptr->value` inside those original loops: inspection of the
generated code confirms calls to shared `Atomic.Load` and
`Atomic.CompareExchange`, including u64 operations. The byte flag uses
same-width shared `Atomic.Exchange`/`Store`; fences call shared
`Atomic.SignalFence`/`ThreadFence`, backed by `Interlocked.MemoryBarrier`.
Initialization remains a store before publication to other threads.

Compile-time assertions fail closed on unsupported LP64/type size/alignment or
non-lock-free native representations. No compiler identity is invented, no
GCC configure feature is redefined, and no atomic or barrier is removed.
PostgreSQL's portable empty processor-delay hint is retained. The actual
spin-count, sleep, backoff and timeout algorithms remain active.

`src/Atomics/s_lock-dotcc-c11.h` supplies only spinlock compiler primitives:

- `_Atomic unsigned char slock_t`, with compile-time one-byte size/alignment,
  LP64 and lock-free byte assertions.
- TAS exchanges 1 with acquire ordering and returns the previous byte: zero
  means acquired, the opposite convention to `pg_atomic_flag`.
- TAS_SPIN first loads the byte with relaxed ordering, exchanging only when
  apparently free, as the original x64 implementation does.
- Initialization uses `atomic_init`; unlock stores zero with release ordering;
  lock-state observation is a relaxed load.

The overlay does not define `S_LOCK`. The original header's `S_LOCK` calls the
original out-of-line `s_lock` on contention. Its `SpinDelayStatus`, adaptive
spins-per-delay, randomized delays, wait-event reporting and stuck-lock handling
are unchanged. The existing shared runtime lowers the byte operations to
same-width Interlocked primitives and conservatively strengthens memory orders.
No native synchronization import or alternate database algorithm is added.

## Staging and parent integration

`scripts/atomics.py:prepare_overlay(source, destination)` copies the configured
`src/include` closure as ordinary files. It requires a fresh private destination
independent of the source tree. A complete include copy matters: quoted includes
inside an original `postgres.h` or `c.h` could bypass a partial include overlay.

Two exact, unique local guards in staged `port/atomics.h` insert atomic selection by
`PGLITE_DOTCC_C11_ATOMICS`. The architecture block selects the authored header;
the compiler block skips GCC/MSVC/SunPro only when that explicit port is selected.
A third unique guard in staged `storage/s_lock.h` selects the authored spinlock
header with `PGLITE_DOTCC_C11_SPINLOCKS` ahead of GCC/ICC. Both explicit defines
are returned automatically. Otherwise upstream selection remains available.
No upstream implementation text or public wrapper is replaced. The helper
returns include/define paths plus
source/staged hashes and a generic-header preservation check. It never writes to
the source or invokes native preparation.

The integrated translation preparation calls:

```python
port = helper(ctx.root / "scripts/atomics.py").prepare_overlay(
    Path(info["source"]), ctx.work / "atomics-include")
# Place this include directory BEFORE the configured original src/include.
includes = [Path(port["include"]), *original_includes]
defines = [*original_defines, *port["defines"]]
```

Keep the existing embedding remaps and profile flags. The recipe already
consumes the returned include and defines, so this spinlock addition needs no
change to `translation.py` or `campaign.py`. Apply the supervisor's separate
`portability.prepare_overlay(root, port["include"])` after this helper to select
upstream software CRC/popcount and non-GCC checks in the private `pg_config.h`.
The atomics suite uses that same preparation order. The adapter `run(ctx, suite=None)` is
registered as the `atomics` framework suite. It uses only `ctx.work/atomics`, `ctx.artifacts/atomics`, and the shared
`ctx.run` command logger/cancellation support; it never builds shared tools or
creates another campaign runner.

## Behavioral evidence

The same C fixture includes the actual `c.h` and `port/atomics.h`. It runs against
three implementations: the original configured PostgreSQL native x64 port,
the native staged C11 port, and actual generated C# JIT execution of that C11
port. Native controls use `cc -std=gnu11 -O2 -pthread`; managed emission uses the
existing compiler with `--emit=managedlib --runtime=c`, the shared 256 KB split,
literal-pool and inline-deduplication settings. Managed builds use one MSBuild
node and private `--artifacts-path` output.

All three agree on 16 layout fields and pass the same C behavior assertions:
initialization/read/write, successful and failed CAS, expected-value updates
including high-bit/64-bit observations, fetch add/sub including negative
operands, exchange, bitwise and/or, add/sub-fetch, unsigned wrapping,
membarrier access, monotonic advancement, flag state transitions and byte
canaries. The extra C# harness compares native layout data and invokes the
actual emitted entrypoints, rather than accepting parse/emission success.
It also measures the generated CLR structs' actual `sizeof` and field addresses
independently of the emitter's `offsetof` metadata, comparing those runtime
measurements to the native report.

Four workers exercise the original generic CAS increment loops and a flag-
protected non-atomic counter: each counter must reach exactly 20,000, with the
u64 counter starting above 32 bits. Separate producer/consumer threads transfer
2,000 non-atomic payloads through ready-state handshakes using actual PostgreSQL
read/write barriers. Both directions include release/acquire fencing before a
payload can be reused. The managed workers synchronize their start. Native
workers use pthreads; managed workers use host threads calling emitted C.
These are bounded functional concurrency probes, not a stress or performance
qualification or a proof for all memory-order interleavings.

Observed layouts are u32 size/alignment 4/4, u64 8/8, flag 1/1, all value offsets
zero, and the aggregate fixture size/alignment 48/8. Aggregate u32/u64/flag
offsets are 4/8/17, the following canary is at 18, and the protected u64 is at
24. All three executions match exactly.

The new spinlock fixture includes the complete actual upstream
`src/backend/storage/lmgr/s_lock.c`, `src/common/pg_prng.c`,
`src/port/pg_bitutils.c` and `src/port/pgsleep.c`. It uses upstream `S_LOCK_TEST`
mode, renames only that test's `main`, and observes `pg_usleep` calls before
calling the real sleep function. Test I/O uses the standard host functions.
An otherwise-unused inline requires storage for the actual declared
`CurrentMemoryContext` pointer; the fixture provides no memory-context behavior.
The original native control links the existing `libpgport.a` for its selected
platform helpers; no native archive is used by emitted execution.

Original native, C11 native and emitted JIT agree on eight spinlock layout
fields and five seeded backoff values. `slock_t` is 1/1 size/alignment, the
aggregate is 32/8, and lock/following canary/value/phase offsets are 1/2/8/24.
The managed harness also measures the actual generated CLR lock field width,
aggregate size and field addresses independently of emitted layout metadata.

Four workers perform exactly 20,000 TAS-protected increments. A separate holder
and waiter force 64 `S_LOCK` contentions: the holder retains ownership until the
waiter has entered the original sleeping path, then releases it. Every acquisition
returns a positive delay count, checks protected 64-bit payloads in both
directions and leaves both byte canaries intact. The coordination phase uses
relaxed C11 operations so native payload visibility relies on the spinlock.
Only one slow-path waiter uses PostgreSQL's process-local adaptive/PRNG state;
this avoids pretending that those upstream globals are a thread-local API.

Direct checks of the original delay functions cover the initial spin count,
increase/decrease of adaptive estimates, wait-event start/end, seeded randomized
backoff and wrap at the maximum delay. The observed backoff sequence is
`[1414, 1744, 2168, 2307, 1000]`. Separate native-original, native-C11 and emitted
processes initialize a delay status at the final timeout attempt: all execute
the original `s_lock_stuck`, diagnose the stuck lock and exit 1. This is a bounded
check of the real timeout branch, not a minute-long stuck-lock stress test.
The product's normal PANIC path is unchanged by test-only `S_LOCK_TEST` mode.

Four overlay tests pass, including six missing/duplicate context cases,
source/destination overlap rejection, preservation of an existing destination,
and a hash comparison of the real copied closure. Before the separate portable
configuration selection, only `port/atomics.h` and `storage/s_lock.h` differ
among the original include files; exactly the two authored port headers are
added. Generic atomic wrappers and the generic spinlock suffix are unchanged.

## Actual full-input retry and next work

With final tools and both private overlays, the real complete
`src/backend/tcop/postgres.c` object emits with exit 0. The former spinlock
`#error` is resolved. DotCC still reports:

```text
#include 'regex.h' not resolvable (not in -I dirs or system headers)
#include 'getopt.h' not resolvable (not in -I dirs or system headers)
#include 'sys/prctl.h' not resolvable (not in -I dirs or system headers)
```

These are required system-header closure diagnostics; successful object emission
is recorded separately from complete input qualification. The suite remains
blocked while they occur. The supervisor owns shared header/runtime coverage and
whole-backend integration. `regex/regex.h` deliberately includes the system
`<regex.h>` before defining PostgreSQL's replacement interfaces; `pg_getopt.h`
selects `<getopt.h>` from native configuration; `storage/pmsignal.h` selects
`<sys/prctl.h>`. Supply reviewed shared declarations/host behavior or a proven
upstream profile selection, then retry actual source. No missing header is
silently replaced by an empty port header.

Earlier snapshots rejected PostgreSQL's `StaticAssertExpr` because its GNU
statement expression contains `do { _Static_assert(...); } while (0)`.
`tests/Atomics/pg_static_assert_expr.c` preserves the actual macro: native returns
42 and old snapshots reject it. Diagnostic receipt
`artifacts/atomics-spin-reduction-20261022-170700-623867/receipt.json` retains the
native/old-compiler evidence. The final supervisor snapshot contains the shared
loop fix; this is historical, rather than the current full-TU failure.
The same actual-header reduction emits successfully with the final snapshot;
`static-assert-current.log` records that retry. Python syntax and owned-path
diff whitespace checks also pass in the final receipt.

The historical upstream `--disable-spinlocks` argument is unrecognized by this
fork's configure. It is not used as a synchronization bypass. This port supplies
real TAS/acquire/release regardless of that obsolete argument.

No shared-memory cross-process atomics, alternate architecture/ABI, processed
output, NativeAOT, complete backend closure, SQL execution, or useful workflow
is qualified here. Shared runtime `_Bool` atomic limitations are avoided by
numeric storage. PostgreSQL's existing prohibition on subtracting INT_MIN or
PG_INT64_MIN remains in its wrappers and is not relaxed. This PostgreSQL API
has and/or operations, not an added XOR operation. Diagnostic object emission
alone would not qualify a product even once it succeeds.

## Reproduction

```bash
bash pglite/scripts/test.sh --suite atomics --fetch never --tools reuse
bash pglite/scripts/probe.sh --unit postgres.c --fetch never --tools reuse --jobs 1
```

Use explicit `DOTCC_COMPILER` and `PGLITE_ATOMICS_SOURCE` to select an existing
immutable compiler and a read-only configured/built native stage; doing so avoids
native preparation for this suite. Every managed build uses a private
`--artifacts-path`. The framework adapter logs native/emitted controls, negative
stuck-lock processes and the complete `postgres.c` command in one receipt.
Expected current result: atomics and spinlocks pass, the object emits, and the
suite fails its unresolved-include gate. No blocked receipt qualifies SQL.
The supervisor retains P6 completion, full backend retry and integration.
This bounded work does not build shared compiler outputs, fetch sources,
commit, push, publish, or change the parent-owned plan files.
