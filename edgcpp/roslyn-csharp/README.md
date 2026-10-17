# RoslynObj C# translator

Status: implementation approved on 2026-10-15 (design commit `e40280bea`).
T0 complete SQLite inventory and T1 genuine C/C++ fixtures pass their gates.
The fixtures match native execution in JIT/NativeAOT with deterministic emission.
T2's bounded storage/calls/program fixture gate now passes, including separately
proved native aggregate boundaries. The unchanged full SQLite object now emits and builds completely. The initial
bind/reset/clear and 43-statement SQL workload match native in JIT/NativeAOT.
Production CLI source publication and its fresh JIT/NativeAOT execution proof
pass the T3 gate.
T4 configured SQLite acceptance now passes: persistence/interchange, WAL/process
locking, enabled SQL features, ownership/callbacks, snapshot/RBU, carray/normalize,
threading/unlock notification, controlled failure recovery, provider audit and
readability/restart evidence in JIT/NativeAOT. See the [consolidated receipt](acceptance/sqlite/t4-acceptance.json).
The first [normal C++ lifetime slice](docs/T5-NORMAL-LIFETIMES.md) also passes
production CLI and native/JIT/NativeAOT observations. The bounded
[generated-only integer exception slice](docs/T5-EH-CONTRACT.md) also passes,
including handler selection before cleanup and balanced payload ownership.
[Constructor-body failure](docs/T5-CONSTRUCTOR-FAILURE.md) now also preserves
completed-member cleanup through its captured nonzero receiver offset.
[Automatic partial arrays](docs/T5-PARTIAL-ARRAY.md) also pass with direct generated
callback execution and completed-prefix cleanup.
[Integer rethrow](docs/T5-RETHROW.md) now also preserves the original payload
through two handler entries and frees it once.
[Default noexcept termination](docs/T5-TERMINATION.md) also passes its explicit
unwind policy and isolated JIT/NativeAOT SIGABRT evidence.
[Single-inheritance virtual dispatch](docs/T5-VIRTUAL-DISPATCH.md) now also passes
actual slot execution, physical layout and pinned RTTI data-provider checks.
[Multiple inheritance](docs/T5-MULTIPLE-INHERITANCE.md) now also passes with a
retained secondary table and an independently executed fixed receiver adjustor.
[Virtual-base construction](docs/T5-VIRTUAL-INHERITANCE.md) also passes retained
VTT forwarding, construction-phase dispatch and data-driven receiver adjustment.
[Exact class exception payloads](docs/T5-CLASS-PAYLOAD.md) also pass selected copy,
const-reference catch and managed payload destruction before final free.
An [actual captured DuckDB getter](docs/MANAGED-ROOTS.md) now also executes in
JIT/NativeAOT through explicit managed-root selection over borrowed layout bytes.
The enclosing accessor, whole core/library, linking and class ownership remain open.
Broader C++ and scale remain open. See the [coverage ledger](docs/COVERAGE.md) and
[restart state](docs/RESTART.md) for current evidence and remaining work.

This separate consumer will translate a set of `.roslynobj` translation units
into readable unsafe C#. EDG remains the authority for C/C++ semantics; the
translator consumes the public `RoslynObj` model and implements its recorded
execution in C#. The reader remains independent of the translator.

The first runnable real target is the existing full-feature SQLite amalgamation.
Unsafe code is the normal output: preserve pointers, explicit storage, names and
ordinary source-like operations wherever C# can express the recorded behavior.

- [Design and implementation plan](docs/PLAN.md)
- [Acceptance milestones and SQLite requirements](docs/ACCEPTANCE.md)

Existing object exports and reader acceptance are input evidence, not evidence
that translation works.

Reproduce the first runnable fixtures from the repository root:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t1.py --artifacts /mnt/storage/roslynobj-csharp-t1-new
```

The runner uses the pinned frozen producer and separate native oracle processes.
An existing immutable fixture export directory can be supplied with `--objects`.
The [T1 report](acceptance/t1/report.json) records source/build/input/output hashes,
public layout/template evidence, commands, resource budgets and observations.
See [SQLite reproduction and provider configuration](acceptance/sqlite/README.md)
and the [first real-project execution evidence](docs/SQLITE-EXECUTION.md).
The configured SQLite gate is closed; broader C++ and portability/scale remain open.
