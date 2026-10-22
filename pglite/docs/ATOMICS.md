# Independent PostgreSQL atomic compiler port

The private C11 port passes actual native and emitted C# JIT controls. It is
reviewed and integrated into the default `embedding-native` recipe through a
private header overlay. The complete `postgres.c` retry remains blocked by the separate
spinlock compiler-port selection in `storage/s_lock.h`. No translated SQL,
product library, or broader delivery milestone is claimed.

Progress and supervisor handoff are in [atomics-progress.md](atomics-progress.md).

Evidence is retained under
`artifacts/atomics/20261022-163610-583fad2c/` and the matching private
`build/atomics/20261022-163610-583fad2c/`. The overall receipt intentionally fails
because complete `postgres.c` still fails; `atomics/report.json` separately
records `atomic_status: passed-native-and-emitted-jit`.

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
PostgreSQL's existing generic empty spin-delay hint is retained; it is not an
atomic operation. No spinlock implementation is supplied by this experiment.

## Staging and parent integration

`scripts/atomics.py:prepare_overlay(source, destination)` copies the configured
`src/include` closure as ordinary files. It requires a fresh private destination
independent of the source tree. A complete include copy matters: quoted includes
inside an original `postgres.h` or `c.h` could bypass a partial include overlay.

Two exact, unique local guards in staged `port/atomics.h` insert selection by
`PGLITE_DOTCC_C11_ATOMICS`. The architecture block selects the authored header;
the compiler block skips GCC/MSVC/SunPro only when that explicit port is selected.
Otherwise upstream selection remains available. No upstream implementation text
or public wrapper is replaced. The helper returns include/define paths plus
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

Keep the existing embedding remaps and profile flags. The recipe owns this
integration in translation preparation. The adapter `run(ctx, suite=None)` is
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

Overlay review receipt `artifacts/atomics/20261022-163336-ffe80722/receipt.json`
confirms that only `port/atomics.h` changes among 940 copied upstream files and
that the sole added include file is the authored C11 port. Missing architecture
context, duplicate compiler context, and source/destination overlap are
rejected. Context failures create no destination. Python syntax validation
passed in that same framework-logged review.

## Remaining blockers and retry requirements

The complete actual `src/backend/tcop/postgres.c` command includes the new
private headers plus the original profile's embedding remaps. It now passes the
`pg_atomic_uint32` selection failure and stops with:

```text
dotcc: #error: PostgreSQL does not have spinlock support on this platform . Please report this to pgsql - bugs @ lists . postgresql . org .
```

The reduction `tests/Atomics/pg_spinlock_selection.c` includes the actual
`c.h`/`storage/s_lock.h` and returns `sizeof(slock_t)`. Native compilation and
execution pass and report one byte. The same staged DotCC input fails with the
same preprocessor error. Original logs are
`atomics-postgres-full-emission.log`, `atomics-spinlock-native-build.log`,
`atomics-spinlock-native-run.log`, and `atomics-spinlock-reduction.log`.
This is a missing separate compiler-port selection, not evidence of a broken
C11 CAS or an established compiler defect.

Although the native profile records `--disable-spinlocks`, the selected fork's
`storage/s_lock.h` still unconditionally requires `HAS_TEST_AND_SET`. Do not
define that macro alone or bypass its error. A further reviewed spinlock port
would have to preserve the one-byte `slock_t`, TAS return convention, acquire/
release semantics and actual generic `S_LOCK`/`s_lock` contention behavior. It
needs its own real native/emitted probes, followed by another complete
`postgres.c` emission and build/closure investigation. The supervisor owns that
scope decision and any compiler repairs.

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

The framework snapshots existing tools and prepares the configured native headers;
`PGLITE_ATOMICS_SOURCE` may explicitly select an existing configured source for
independent controls. Expected result: atomic tests pass and the overall suite
fails on the real full-input spinlock blocker. Original experiment receipts are
retained above; final integrated receipts are in VALIDATION.md. No blocked
receipt is counted as a successful SQL product.
