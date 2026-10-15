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
