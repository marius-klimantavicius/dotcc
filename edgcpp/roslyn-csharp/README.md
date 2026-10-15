# RoslynObj C# translator

Status: implementation approved on 2026-10-15 (design commit `e40280bea`).
T1 genuine C/C++ fixtures pass native comparison, JIT, NativeAOT and deterministic
emission. T0 complete SQLite inventory remains in progress. See the [coverage ledger](docs/COVERAGE.md) and
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
The [SQLite manifest](acceptance/sqlite/README.md) is an inventory input;
SQLite emission/execution and broader C++ support remain unimplemented.
