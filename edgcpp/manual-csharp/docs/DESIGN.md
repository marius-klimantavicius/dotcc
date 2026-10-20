# Initial API and implementation contracts

The campaign is an in-process handwritten C# frontend following selected EDG
algorithms. `PLAN.md` is authoritative. The first slice implements and analyzes
declarations/expressions together rather than constructing an unbound syntax
tree for a separate binder. Each implemented routine must have a named source
correspondence entry and explain its bounded omissions.

One session owns canonical scalar/function types, declaration identity, scopes,
source buffers, diagnostics, token storage and cursors. Graph nodes use managed
reference identity. Consumers inspect retained graph facts; they must not mutate
the state used to establish those facts. Two analyses of identical bytes have
independent mutable identities.

The initial profile is explicit C++20 with LP64 target widths. It does not imply
full C++20 support. Scalar conversions and integer operations must apply target
rules rather than silently inheriting CLR arithmetic. Unsupported and invalid
source diagnostics remain distinguishable. Error recovery must make progress
without accepting skipped syntax as supported semantics.

Retained token buffers and cursor checkpoints provide lexical replay. Scope,
diagnostic and deferred-work rollback require explicit restoration when such
speculative operations are introduced; lexical replay alone must not be called
full parser rollback. Token-cache template instantiation remains an active gate.

The library and default tests have no native/compiler or generated frontend
dependency. `tools/oracle.py` is an explicitly invoked native test adapter. It
records exact source/executable hashes, commands and output separately from
managed implementation results.
