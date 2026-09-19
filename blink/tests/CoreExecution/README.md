# Actual upstream core consumer

Run `python3 blink/tests/CoreExecution/run.py --assembly-receipt <objects/KEY/receipt.json>`
after `assemble-core.py` has linked the complete frozen object set. `--prepare-only`
creates the owning projects before the link finishes; `--build-only` stops after
the first raw C# build and preserves exact diagnostics.

The runner verifies the profile and linked-source hashes, copies only frozen
host bridges and the frozen Host project, and references the unmodified linked
C# from a separate library. The consumer binds private IO, environment and
identity, calls the genuine upstream embedding probe once, then discards the
worker. Both raw and postprocessed libraries have separate JIT and NativeAOT
runs; NativeAOT roots the whole translated library. Postprocessing operates on
an isolated copy, and the raw source hash is checked again afterward.

Execution rows must match the real native archive oracle exactly. The ABI row
is checked against a separate native storage probe using the explicit signal
jump record, since that record intentionally differs from native sigjmp_buf.
The driver's owned-memory accounting footer is validated separately. Failed
C# builds retain the complete diagnostic text and source-location evidence;
isolated host symbols are distinguished from other names without pretending
that textual upstream matches prove reachability or implementation.

Preparing projects or emitting/linking C# does not establish P1. Actual bounded
instruction, budget and exit execution must pass the native comparison. The
current probe exercises ordinary arithmetic/memory, bounded branches, exit and
exit_group twice in one process. The former custom undefined-instruction and
unmapped-memory cases are excluded by the updated user test scope and removed
from this default probe; historical results remain historical. Required runtime
fault handling stays implemented, but no excluded test is run or counted passed.

Each consumer build now runs a snapshotted BoundaryAudit tool before execution.
Incomplete direct inventories, traversed native imports and direct process
exit/start/native-loader calls fail the gate. Reports include unused runtime
native declarations, indirect/virtual sites and exact assembly identities;
this direct-graph policy does not establish runtime isolation. Raw and optimized
reports are independently preserved and hashed in the consumer receipt.

Execution now records and checks JIT consumer/dependency and NativeAOT binary
hashes before/after each run. `scripts/audit-published-core.py <receipt>` requires
these execution-time identities and inventories the corresponding Linux ELF
headers, dynamic dependencies/imports and load segments. It rejects writable
executable load segments or named native-emulator dependencies, while explicitly
not claiming to detect static code, later dynamic loading or executable mappings.
