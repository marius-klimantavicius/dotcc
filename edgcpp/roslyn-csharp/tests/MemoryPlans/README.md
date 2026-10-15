This focused input activates ordinary data-pointer `+=`/`-=`, unsigned64
compound shifts, recorded unsigned16/unsigned32 volatile fetches and stores,
and variadic calls whose argument and selected-branch prefixes require typed
planning. Its pointer observations distinguish byte stride1, record stride12
and pointer-to-pointer stride8. A selected-place helper runs exactly twice.
The four exact branch events are1,2,3,4; unselected branches contribute none.

The coordinator runs the measured reproduction from the repository root:

```sh
python3 edgcpp/roslyn-csharp/tests/MemoryPlans/run-memory-plans.py \
  --aot-cc clang-18 --artifacts /absolute/new/artifact-directory
```

The driver verifies the frozen genuine producer, exports the inputs, records
public operation inspection as inventory only, builds an independent native
oracle, and compares all24 observations/events with generated JIT and NativeAOT
execution twice each. It checks deterministic generated sources, exact managed
dependencies and native dependency observations, and rejects a genuine
volatile64 input before publishing output. Separate managed checks exercise
invalid shift-count and misaligned volatile-address policies without executing
undefined accesses in the native oracle. Reports retain phase budgets, source,
compiler, object, build and binary hashes. `--objects` reuses immutable inputs
and explicitly records that their export was not reproduced in that run.

Volatile address representation and actual memory operations remain separate.
Only the captured unsigned16/unsigned32 primitive accesses are admitted here;
volatile64, aggregates, bitfields and atomic-qualified object accesses remain
unsupported. The runtime checks natural address alignment. Planned calls reuse
the existing selected scalar variadic pack; planned `va_*` cursor operations
remain unsupported. The native application is only an independent oracle and
is never linked into generated execution. This receipt establishes neither
SQLite execution nor complete language/provider support or production CLI
admission.
