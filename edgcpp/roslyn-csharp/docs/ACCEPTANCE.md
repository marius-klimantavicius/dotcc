# Translation milestones and acceptance

Status: approved gates, with implementation in progress. T1's initial runnable
fixture gate is closed; T0 remains open pending the complete SQLite inventory.
See [the design](PLAN.md) and [current evidence](COVERAGE.md).

## Milestones

| Gate | Deliverable | Evidence required to close |
| --- | --- | --- |
| T0 — contract and inventory | Separate projects, manifest, supported-profile checks, diagnostic/report contract, complete required-family inventory for SQLite | Genuine objects read through the public API; target/import/body classifications documented; no successful-output claim with unresolved required families |
| T1 — first runnable output | Small C and C++ fixtures emitted as readable unsafe C#, compiled and run | Arithmetic/conversions, pointers, calls, control flow, one record and a resolved template fixture match native observations; JIT and early NativeAOT smoke; inspect emitted source |
| T2 — storage, calls and program assembly | Aggregates/unions/bitfields/arrays, static data and address fixups, callbacks/function addresses, variadics, basic multiple TUs | Runnable fixtures prove layout/aliasing, exact constants, cyclic address initializers, callback identity/round trips, ABI aggregate calls, varargs forwarding, cross-unit functions/data and distinct internal names; explicit unsupported link-policy diagnostics |
| T3 — SQLite complete emission and first execution | Unchanged full-feature object produces a buildable library; every required import has an implementation and ABI binding | All retained executable definitions represented; no stubs or native SQLite fallback; generated open/prepare/bind/step/finalize/close executes in-memory SQL; both JIT and NativeAOT pass the initial workload; T4 supplies broader behavioral evidence |
| T4 — configured SQLite acceptance | Persistence, WAL/concurrency and the enabled-feature matrix work | Differential execution below, reproducible reports, import/provider audit, representative source-readability review, resource measurements and clean restart/repeat generation |
| T5 — broader C++ execution | Lifetimes/EH, inheritance/dispatch, concrete instantiations, RTTI, initialization/TLS and required extensions | Ordered event/value comparisons, normal and exceptional cleanup, constructor failure/partial arrays, throw/rethrow, virtual/multiple inheritance, pointer-to-member, static guards/TLS; no claim based on SQLite alone |
| T6 — portability and larger programs | Additional source/host ABI adapters and large multi-unit translation | Per-platform executable tests; bounded indexing/emission; selected C++ library slice then DuckDB core with explicit dependency closure; no claim that core objects alone are a complete linked DuckDB |

T2 must freeze function-pointer/callback and variadic conventions before broad
SQLite code generation. T5 requirements encountered by a real earlier target move
forward; milestone labels do not permit skipping a required behavior. Basic C++
fixtures precede SQLite; exhaustive C++ coverage does not block its first run.

Each capability ledger row records the captured form, projection/provider,
fixture or real target activation, build/execution evidence and remaining limits.
Statuses distinguish implemented, exercised, unsupported and not encountered.
No corrupted object tests are proposed. Invalid link configurations use valid,
independently produced objects, not edited databases.

## Pinned SQLite target

Use `/mnt/storage/roslynobj-sqlite-20261014/export/sqlite3.roslynobj`, documented
in [the export report](../../roslyn/docs/SQLITE.md): SQLite 3.54.0, GNU C17,
Linux x64, thread-safe build, 19 explicit feature definitions, default JSON/JSONB
and WAL. Object SHA256:
`e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c`.

The existing object has 3,887,908 entities and is 283.77 MiB. Its previous reader
navigation checks did not execute SQL. Preserve all configured features; do not
re-export a reduced SQLite configuration to obtain a milestone pass.

Build a fresh native reference from the exact recorded amalgamation/configuration
and run shared deterministic workloads against native and generated libraries.
Original source is permitted for the oracle, test design and readable-output
review, never as translator input or a way to replace untranslated bodies.
Use separate processes/artifacts so native SQLite cannot service translated
workloads accidentally. Record the exact providers loaded by the generated app.

## Required runtime workloads

| Area | Required observable behavior |
| --- | --- |
| Core API | Open/close; prepare/bind/step/reset/finalize; NULL, integer, real, text and blob results; correct lengths, ownership and destructor callbacks; errors and extended result codes |
| SQL and transactions | Tables/indexes, joins and aggregates, constraints, commit/rollback/savepoints, prepared statement reuse and query-result comparisons |
| Persistence | Write, close, reopen and verify data; native-to-generated and generated-to-native database interchange; compare semantics rather than arbitrary file byte identity |
| WAL and locking | WAL mode, concurrent connections/processes, visibility, checkpoints, busy handling and reopen; verify actual locking behavior, not only `journal_mode` text |
| JSON/JSONB | Extraction, mutation, table-valued queries, null/type behavior, JSONB round trips and documented invalid-input results |
| FTS3/4/5 | Create/populate/query each enabled family; tokenization, ranking/highlight where applicable; callback/tokenizer interfaces used by fixtures |
| RTree/Geopoly | Insert/update/query and representative geometric/spatial results |
| Session/preupdate/RBU | Preupdate event values/order, changeset creation/application/conflict handling, staged update and completion; include backup API interaction |
| Inspection modules | Execute dbstat, dbpage, stmt and bytecode virtual tables; compare deterministic fields or documented invariants, not unstable addresses |
| Math, metadata, STAT4 | Math values/special cases as supported; column metadata; ANALYZE statistics and result correctness without relying on unstable planner text |
| Snapshot/carray/normalize | Snapshot lifecycle and visibility; carray binding/value ownership; normalized SQL and corresponding execution |
| Threading/unlock notify | Registration/callback behavior under the configured shared-cache and locking conditions; concurrent use permitted by the pinned configuration |
| Runtime boundaries | Allocation/reallocation/free, memory/string operations, formatting/varargs, callback reentry, file I/O, time/randomness contracts and thread-local error state actually required by the inventory |
| Failure paths | Controlled allocation and I/O failures through runtime/test hooks; correct errors, ownership cleanup, transaction rollback and subsequent recovery; no input-object corruption |

API availability and workload preconditions follow the pinned source/configuration.
If a configured API is conditional on a runtime mode, enable that mode in the
workload and record it. Do not silently skip a required row. Nondeterministic
time, random, scheduling and address values are checked by contract/invariants;
deterministic outputs compare directly with the native reference.

Run the matrix in JIT and the selected NativeAOT deployment. Add relevant upstream
tests where practical and explicitly report their scope; a finite smoke matrix
does not establish that the entire upstream test suite passes.

## C++ fixtures and fidelity evidence

Use genuine independently compiled inputs and defined behavior. Compare return
values, output, memory observations and ordered lifetime/dispatch events. Cases
include conversions and sequencing; address identity; templates already selected
by EDG; constructors/copies/destructors; temporary cleanup and return paths;
exceptions/rethrow/termination; partially constructed arrays; local-static guards;
TLS; multiple/virtual inheritance and thunks; RTTI and member/function pointers.

Check emitted layout against captured EDG facts. Native reference execution is
an observation tool, not a competing semantic authority: resolve disagreements
using the retained EDG program and stock generator/runtime before changing our
consumer. Never modify EDG to make a test pass.

Select readable-output examples for pointer/record operations, ordinary loops,
switch fallthrough, callbacks, variadics and cleanup. Review names, recognizable
control flow, temporary count and helper use. Avoid brittle whole-project text
snapshots that merely duplicate emitter implementation.

## Evidence, performance and completion

Each gate records source/object/tool hashes, target/configuration, command lines,
generated-file inventory, required/supported/unresolved counts, build and run
results, runtime dependencies, remaining limitations and a milestone commit.
Measure inventory, indexing, emission, C# build, NativeAOT publish and execution
separately, including peak memory and generated source/binary size.

Budget per-body memory as well as reader caches; large routines and C# compiler
limits require explicit handling. Repeat generation from identical immutable
inputs must yield deterministic source. Test cancellation/resume at real phase
boundaries without mutating or corrupting inputs.
Freeze numerical memory, output-size and concurrency budgets in the manifest
before scale runs; report actual use against each budget and treat excess as a
failed resource gate rather than silently increasing the limit after execution.

T4 completion means the full configured SQLite library emits and the listed
runtime workloads pass. It does not claim every SQL program, every C++ feature,
all libc, native C++ unwinding interoperability, or full DuckDB translation.
