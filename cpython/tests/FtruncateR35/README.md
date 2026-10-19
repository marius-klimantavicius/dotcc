# Focused ftruncate position control

This reduction belongs to B35's generic Libc repair. It does not execute or
qualify translated CPython. The normal DotCC.Tests project includes
`FtruncatePositionTests.cs`; `Unit/Unit.csproj` selects those tests plus affected
file durability, stdio and descriptor ownership tests without building the
compiler.

The independent native control is `../Abi/ftruncate_position_r35.c`. Use those
exact bytes for emitted JIT/AOT controls after the next normal compiler capture;
do not repair generated output. It checks sparse files beyond 2 GiB without
filling them. The managed executable repeats its operations through the current
Libc registry and compares error results to the native log. It uses the unchanged
96-byte DotCC stat layout with the size field at offset40. It does not assume
that target and host stat layouts are equal.

`run_phase.py` uses the existing campaign command boundary for individual
commands, private output roots and isolated MSBuild products. It refuses to
overwrite existing phase receipts. Recorded phases are `native`, `baseline`,
`native-corrected`, `baseline-faithful`, `units-red`, `fixed`, `units-fixed`,
and `aot`. The first native harness assumed LONG_MAX was a valid seek offset;
the host filesystem rejected it. Its source and original failure are retained.
The corrected fixture tests the ftruncate length boundaries and preserves the
previous valid offset. Both faithful RED and fixed runs use those same bytes.

`seal_proof.py` binds already-built PE/PDB/Compile inputs, exact ILC references,
entry, native object and executed binary, then verifies source preservation.
The original pre-repair PDB checks were collected before the production edit;
the small retained original PosixFs source supports later historical checks.
No compiler capture, emission, product regeneration or full required suite runs
are performed by these scripts.

Actual commands, immutable input hashes, outcomes and the direct safe release
are under `../../artifacts/owner-b-r35-20261018/`. Required actual unchanged
Python cases and the final product matrix remain open until the assigned owners
capture and validate the repaired runtime normally.
