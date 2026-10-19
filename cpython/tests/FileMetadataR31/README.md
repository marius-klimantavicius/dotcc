Focused generic Linux x64 metadata controls. `native.c` uses system headers;
`target_layout.c` independently uses the actual DotCC headers. Their shared
oracle lets the current-Libc JIT/AOT fixture compare every target field against
real host observations without sharing the runtime's host-layout declarations.
`ActualLayout` references an existing actual B30 binary; it never rebuilds CPython.
`Unit` runs persistent metadata and affected file/path/owner regressions.

Saved first-run commands, from any working directory:

```bash
python3 -B /home/marius/p/dotcc/cpython/tests/FileMetadataR31/run_phase.py baseline
python3 -B /home/marius/p/dotcc/cpython/tests/FileMetadataR31/run_phase.py final
python3 -B /home/marius/p/dotcc/cpython/tests/FileMetadataR31/run_phase.py aot
python3 -B /home/marius/p/dotcc/cpython/tests/FileMetadataR31/run_phase.py affected
```

The baseline was executed before repair. Phase evidence is immutable: the driver
rejects an already recorded phase. Choose fresh private WORK/ART roots for another
run; preserve the recorded RED. The runner reuses `campaigns.process.execute` and
the existing nonblocking per-command shared/native lock adapter. No compiler,
emission, product capture or full CPython suite is part of this fixture.
