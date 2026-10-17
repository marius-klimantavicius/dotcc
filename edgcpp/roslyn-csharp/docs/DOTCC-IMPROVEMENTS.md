# DotCC expression and storage improvement checkpoint

Implemented and verified on 2026-10-17. The user requested a pause after this
improvement pass; the saved DuckDB/provider work has not resumed. EDG remains
the semantic authority, and the public normalized reader remains independent of
the translator. No core EDG or separate dotobj implementation was used or edited.

The [integration receipt](../acceptance/dotcc-improvements/integration.json)
combines 88 expected outcomes: 85 successes and three intentional unpublished
TLS rejections. All measured budgets passed. These are focused current-source
checks, not a rerun or reattribution of the closed SQLite campaign.

## Typed expression rendering

[ExpressionPlan.cs](../src/RoslynObj.CSharp/Emission/ExpressionPlan.cs) gives
typed syntax nodes an explicit C# precedence contract. The
[renderer](../src/RoslynObj.CSharp/Emission/CSharpEmission.ExpressionPlans.cs)
uses precedence and associativity rather than wrapping every operand. Right
operands preserve their actual tree grouping, including floating addition,
subtraction, division and remainder. Opaque adapter text stays conservative;
effect stability never implies a syntax precedence. Unary token separation and
cast grammar remain explicit.

Nested arithmetic shares a known `unchecked` context where valid. Target shift
count conversion keeps its separate `checked` conversion, including when the
count expression contains explicitly unchecked unsigned arithmetic. No emitted
text is stripped or reparsed to remove parentheses.

[Occurrence routing](../src/RoslynObj.CSharp/Emission/CSharpEmission.Occurrences.cs)
prefers typed rendering for supported pure scalar trees. This choice is separate
from scheduling decisions: effectful, lifetime, volatile and branch plans retain
their existing execution paths. The pure route enforces a zero-prefix invariant.
Ordinary loops keep their ordinary headers and bodies.

For example, a retained arithmetic method now emits
`return unchecked((int)(a + (int)(b * c)));`. Required conversion boundaries are
still visible. A pointer round trip emits `return *p;`. Broader legacy-expression
paths remain; this pass does not claim that every generated method is minimal.

The [expression receipt](../acceptance/dotcc-improvements/expressions.json)
records 23 successful phases. A genuine C17 capture exercises 30 observations
and six sequenced events, with exact native/JIT/NativeAOT agreement and repeated
deterministic emission. Cases include nonassociative and floating grouping,
casts, narrowing, unsigned wrap, pointers, short circuiting, conditional effects
and an ordinary `for` loop. Nineteen additional intrinsic-type renderer probes
compile and execute under a checked C# project; these syntax probes are separate
from the native/JIT/NativeAOT source-fixture evidence.

The first attempt exposed ordinary arithmetic bypassing the typed renderer; that
routing was fixed generally. A later loop audit falsely stopped at a nested block;
the acceptance method extractor was corrected without a product change. Those
incomplete attempts remain separate in persistent storage. The final run reuses
the genuine 30-observation capture from attempt 2 and accurately records that it
did not reproduce that export.

## Rooted storage and initialization phases

[Storage emission](../src/RoslynObj.CSharp/Emission/CSharpEmission.Storage.cs)
groups process allocation roots behind a named program owner. Each captured
object still gets its own aligned `PinnedStorage`; equal-valued objects and
TU-local objects remain distinct. The runtime already retained the pinned array,
so no replacement allocator or new byte-memory algorithm was necessary.

Generated initialization names three phases: allocation/zeroing, constant
population and address fixups. Constant discovery completes before allocation
emission, including newly discovered literal storage. Every required address
exists before fixups. Typed scalar destinations improve the phase bodies while
captured byte offsets remain explicit for aggregate placement.

Existing selected dynamic startup and teardown remain separate from these
storage phases. CLR type initialization is not used as a replacement for C++
dynamic-local-static guards, retry or destruction.

The new file-scope TLS subset uses an actual per-thread owner rooted by an
uninitialized `[ThreadStatic]` field. Its factory allocates all admitted thread
storage, populates constants and applies fixups through its local owner before
publishing it. A type constructor cannot supply another thread's TLS values.
Process constants cannot capture a per-thread address.

[TLS admission](../src/RoslynObj.CSharp/Emission/CSharpEmission.Storage.Initialization.cs)
requires captured thread duration and either an exact retained constant or
initializer-free zero initialization. It admits arithmetic values, ordinary
pointers and recursive arrays of those representations. It rejects retained
dynamic/wrapper routines, rewritten or explicit zero initializers, records/classes,
function-local/guard obligations, cleanup/lifetime obligations and unresolved
weak/common/alias/COMDAT selection. Selected TLS initialization routines/aliases
also diagnose their unsupported execution contract. Nameless retained routines
are tested by identity; the public facts report includes explicit association
presence flags.

The [storage receipt](../acceptance/dotcc-improvements/storage.json) records 56
expected outcomes. Genuine two-TU process storage checks cross-object address
chains, zero/scalar/array/literal values, embedded NUL and terminator bytes,
overalignment, distinct equal values and distinct same-spelling local identities.
The TLS fixture checks standard and GNU thread storage on four simultaneously
live threads, per-thread initial values and mutation, shared process-pointer
fixups, stable aligned addresses and physical values across forced collections.
All owning threads stay alive during cross-thread pointer observations.

Both fixtures match independent native oracles under two JIT and two NativeAOT
runs, for both original source and an acceptance-only reverse-order split into
partial declarations. This proves initialization independence for that split;
it does not add a production source-splitting option. Generated runtime
dependencies exclude the reader, translator and native application bodies.
Genuine dynamic TLS, cleanup-bearing TLS and local TLS inputs reject before
publication with the `thread-initialization` family.

The [startup preservation receipt](../acceptance/dotcc-improvements/startup-preservation.json)
adds nine successful current-source phases. It rebuilds and runs the real native
global-startup fixture, translates its immutable existing capture through the
current production CLI, and runs the existing JIT/NativeAOT observer. Startup,
once-only repeated initialization, actual registration, distinct object/module
addresses, work and reverse registered destruction retain their complete native
observations and runtime protocol checks.

## Provenance and remaining boundaries

Artifacts are under
`/mnt/storage/roslynobj-csharp-20261015/t5/dotcc-improvements-1/`. Final sources
are copied and hash-verified beside each receipt. The integration receipt records
the frozen EDG producer, current wrapper, seven measured source captures and the
separate startup capture reuse. Supervisor review verified all current/frozen
source identities, 93 absolute artifact records, 176 phase logs and 31 generated
source/project hashes. SDK `csc.dll` and `MSBuild.dll` identities are recorded in
the expression and storage receipts. No native application fallback was used.

This pass does not implement dynamic TLS, TLS class lifetimes, thread-exit
destructors, local-static synchronization/failure/retry, independent instances,
new TLS COMDAT selection, broad C++ composition or a runnable DuckDB application.
Existing guards remain. The selected DuckDB inventory is still 112 bodies,
3,501 operations, 585 types and 45 imports with unresolved providers. The saved
original GCC helper capture remains unexecuted.

The remaining transfer ledger in [PLAN.md](PLAN.md#resumed-transfer-implementation-and-remaining-opportunities)
distinguishes existing memory-provider contracts, control-flow repair, named
bitfields and address adapters from actual future gaps. No duplicate memory or
bitfield implementation was added. Further implementation or provider capture
requires a later explicit user resume.
