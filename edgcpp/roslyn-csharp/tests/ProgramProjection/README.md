# Generated multi-unit program checks

These are unaccepted two-unit scalar/pointer candidates. The generated checker
references only a generated project through `-p:GeneratedProject=...` and runs
that application twice, checking every observation against the independent
native oracle. It does not link any native application implementation.

The observations distinguish same-spelling internal functions/data, shared
external storage changes, cross-TU storage addresses and canonical function
addresses. The C case also passes an opaque pointer across incomplete/complete
TU views. The C++ case uses already-captured overloaded identities.

The emitter's program selection report must additionally show one admitted
allocation per selected storage definition and one address field per addressed
selected function. Selection uses public ProgramAssembly.Resolve, never a source
name table. Preserve caller TU pointer views without requiring full pointee
equivalence; native aggregate calls remain unsupported.

Generated execution requires the explicit program-assembly emitter hooks, which
are still pending. Run native, generated JIT and generated NativeAOT programs,
including this checker, and preserve hashes/dependency receipts before claiming
the focused contract. This is no bounded-scale or SQLite execution gate.
