# Translate the PGlite PostgreSQL fork with DotCC

Status: Complete through P6 for the agreed Linux x64 LP64 embedding-native target.
All four canonical raw/processed × JIT/NativeAOT rows pass 36 assertions, three
same-process lifetimes and two persistence reopens each. Final source/import/
notice/resource audits, 14 non-consumer suites and ordinary solution build pass.
Campaign: `pglite/`. Updated: 2026-10-22. Product: `TranslatedPGlite`.

This plan adopts [translation baseline v4](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md). The supervisor runs
`gpt-6.1-sol` high; implementation agents run that model xhigh through T3.
Source/configuration and native-control work owns `config/`, `ref/`,
`scripts/prepare.py`, `scripts/native-control.py`, `tests/native/`, and private
native outputs. Host/ABI work owns `src/`, `samples/`, `tests/Boundary/`,
`scripts/boundary.py` and private boundary outputs. The supervisor owns recipe,
plan, integration, shared compiler repairs and combined validation. Only the
supervisor builds shared compiler outputs. Sources have one acquiring writer.
A third xhigh agent owns the independent C11 atomic port under `src/Atomics/`,
`tests/Atomics/` and its helper/docs; the supervisor reviewed and integrated it.
No child commits; significant validated milestones are committed by the
supervisor. The two pre-existing modified shared plan files are excluded.

## Coordinator responsibility and continuation

The coordinator owns full implementation and qualification of this campaign
through P6. The user's request to start the port is authorization to pursue the
complete agreed product. P0 setup and its native/ABI evidence are intermediate
progress; they are not a separately sufficient "campaign start" deliverable.
All required managed workflow and selected final qualification gates now pass.
Earlier milestones alone were insufficient; the coordinator continued through
actual completion without requiring the user to ask again.

After every delegated result, the coordinator reviews and integrates the work,
updates this plan and evidence, and assigns or performs the next required task.
The parent retains this responsibility when a delegated supervisor finishes.
Run independent implementation tasks in parallel on `gpt-6.1-sol` with `xhigh`
reasoning; the supervisor uses the same model with `high` reasoning. Coordinate
shared compiler builds and file ownership.

Completion evidence and historical parallel ownership are in [IMPLEMENTATION.md](IMPLEMENTATION.md).
The spinlock and locale ports and signal-aware resumable boundary controls now
pass their focused gates. Whole backend emission, original initializer execution, paired delivery and
combined selected qualification now pass. The coordinator must continue
through initializer/bootstrap closure, complete raw/processed products, managed
SQL and error recovery, same-process shutdown/reopen, the owning API/consumer,
required JIT/NativeAOT checks and final reproduction. Technical blockers are
implementation work to resolve, not reasons to end the campaign. Do not reduce
the required product or acceptance matrix to claim success.

Stop only after all required gates pass, an explicit user pause/scope limit, or
a demonstrated external dependency, missing authority or necessary user decision
prevents all further useful progress. Exhaust independent work first. Record any
unavoidable interruption with attempted resolutions and the exact input needed,
leave acceptance incomplete, and resume when the impediment is resolved.

## Outcome and scope

Preserve PostgreSQL SQL parsing, planning, execution, transactions, catalogs,
WAL/buffer/storage behavior and PGlite's embedding state machine in translated
code. The first useful managed workflow initializes a private directory, creates
a table, inserts and queries exact values, rolls back one transaction, commits
another, cleanly closes, reopens and checks persisted values. Native executables
are controls only. A parse, emitted object, C# build or native workload never
substitutes for that workflow through the translated core.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required | UTF-8 SQL; one embedded owner; init/readiness/query; transactional table operations; clean close/reopen persistence; ordinary SQL errors followed by recovery |
| Excluded | PostgreSQL server/socket delivery; replication; multiprocess workers; extensions not needed by workflow; runtime loading of native PostgreSQL; JavaScript/Wasm as delivered execution engine |
| Later | Multiple independent instances, concurrent calls, cancellation during blocking queries, broad extensions and upstream full regression qualification |
| Departure | First target Linux x64; PGlite's wasm32 build is investigated separately from a native x64 feasibility profile; neither proves the other ABI |

## Decisions and feasibility work

| ID | Decision | Choice | State and resolving gate |
| --- | --- | --- | --- |
| D1 | Inputs | Fork `29b6424aa312ec3556247db069939006dd27f878` from REL_18_6-pglite; separate builder/lifecycle research pins in JSON; immutable acquisition under ignored `ref/` | Decided P0; SOURCE.md |
| D2 | Configuration | Linux LP64 `embedding-native` variant with actual __PGLITE__ remaps; frozen 863-unit observed backend closure expanded from 876 to 928 selected units including required Snowball, separate 73-unit initdb assembly and staged bootstrap assets; all four canonical execution modes pass | Backend inventory passed P0; config/backend-units.json and profile.json |
| D3 | ABI | Intended translated target LP64 little-endian Linux x64, UTF-8 SQL, PostgreSQL disk/wire representations explicit; wasm32 is a distinct upstream ABI | Native/emitted PostgreSQL 13-field ABI and selected host boundaries pass; wasm32 remains outside this qualification |
| D4 | Host | Shared C runtime plus reviewed C11 atomics/spinlocks; native imports prohibited for database algorithms; owner-scoped process, signals/timers, locales, descriptors and directory durability | Selected native/JIT/AOT host gates and whole-product route/import/ELF audit pass |
| D5 | Lifetime | One owner and serialized entry initially; no reentrant callbacks; owner holds directory, buffers and retained roots until shutdown | Decided and verified: fresh generated contexts, three clean same-process lifetimes/two reopens in every canonical row |
| D6 | Delivery | Object emission, TranslatedPGlite, shared `link_flags` defaults: 256 KB split, literal pool, inline deduplication; authored source retains original paths | Decided; complete closure at P2 |
| D7 | Controls | Same-revision native SQL and embedding-profile feasibility separately labeled; no ordinary upstream configuration presented as embedding proof | Passed scoped P0; 21 native SQL cases with fresh-process reopen caveat |
| D8 | Matrix | Linux x64 raw/processed × JIT/NativeAOT first target; other OS/architectures unqualified | Passed all four canonical rows at P4/P6 |
| D9 | Tests | Shared 21-case native/managed SQL inventory, actual boundary contracts and all four Linux x64 consumer rows; full upstream regression, custom faults/fuzz/stress and performance thresholds excluded from this first product | Decided before P4; QUALIFICATION.md |

Hashes default to warn; off remains supported; strict is optional reproduction.
Acquired references are preserved. Generated headers and adaptations use staged
copies and exact local guards. Source/checksum literals belong only in JSON.

## Source, host and ownership contracts

[Source investigation](SOURCE.md) records the selected build instructions,
dependencies, manifest and configuration. [Boundary investigation](BOUNDARIES.md)
records actual APIs, layouts and ownership findings. Required native imports,
error unwinding, globals and generated-header assumptions are feasibility gates.
No successful runtime stubs, omitted required algorithms, or manually repaired
emitted C# are permitted.

| Boundary | Translated responsibility | Host service | Ownership and check |
| --- | --- | --- | --- |
| Memory | PostgreSQL contexts and object lifetimes | Stable unmanaged allocation/free via shared runtime | Allocation pairing, retained buffer roots; native/emitted layout checks |
| Files/persistence | Pages, WAL, catalogs, transaction semantics | Filesystem handles, sync, directory and file errors | Exclusive owned directory; clean shutdown and reopen assertions |
| Query/results | Parser/planner/executor and protocol processing | UTF-8 request and result transfer | No borrowed buffer survives call unless owner retains it |
| Errors | PostgreSQL error stack and cleanup | Supported setjmp/longjmp lowering or typed reviewed bridge | Real upstream PG_TRY/error path, recovery and subsequent query |
| Lifecycle | Fork startup/readiness/close | Serialized owner and callback rooting | No process backend delivered; actual close tested before releasing resources |

Highest risks: PGlite embedding depends on Emscripten services; LP64 differs
from wasm32; PostgreSQL uses process globals and nonlocal error unwinding;
initialization needs generated catalogs and data files. The smallest useful
experiments are real source object emission, actual-header ABI probes and a
same-revision native SQL control. Unresolved boundaries block P1/P3, not source
inventory or diagnostic translation.

## Framework integration and deliverables

Recipe `scripts/campaign.py` opts into discovery. Canonical projects are
`generated/TranslatedPGlite/TranslatedPGlite.csproj` and
`generated/TranslatedPGlite.Raw/TranslatedPGlite.csproj`; alternate diagnostics
live under `generated/profiles/`. Root solution `ManagedConsumer.slnx` and
`samples/ManagedConsumer/ManagedConsumer.csproj` use ordinary `TranslatedProject`
references. The consumer must report an unavailable embedded core as failure.
Normal translation retains the observed 863-unit backend inventory, selects the
reviewed portable platform sources and adds required original PL/pgSQL plus its
static export/provider units. Receipts record the exact expanded selection.
The 4-unit diagnostic selection is restricted to probes and native-diagnostic.
The original frontend and bootstrap assets are implemented. Coherent 928-unit
backend linking, both form builds, actual initialization and all four canonical
JIT/AOT SQL rows pass.
Complete embedding-native delivery is enabled; diagnostic subsets remain
probe-only. Paired raw/processed publication and final canonical checks pass.
Framework owns fetching, tool snapshots, logs, emission, build, postprocessing
and transactional publication. Project hooks own configuration and native/ABI
experiments. Scripts use shared dispatch and portable copies; Linux-native
control prerequisites are separate from portable orchestration. Source/build/
generated/log outputs remain ignored.

## Milestones and evidence

| Phase | Gate | Status |
| --- | --- | --- |
| P0 | Frozen source/profile, discoverable layout, 863 backend units, working native SQL/error/persistence control, actual translation attempts and repaired failures | Passed scoped campaign-start baseline; VALIDATION.md |
| P1 | Native/emitted ABI, C11 atomic primitives and concurrency, host/lifecycle decisions | Passed selected ABI/atomics/spinlock, locale, signals/timers and owner boundaries; actual same-process reset/persistence verified by all four canonical consumer rows |
| P2 | Every selected product unit emits/links, raw/processed build, import audit | Passed all 928 selected units plus original 73-unit frontend emission/strict link; paired raw/processed semantic processing/builds and final transactional publication; source/import/ELF/notice ownership audit passes |
| P3 | Managed init/SQL/transactions/close/reopen through actual translated core | Passed all four canonical modes: original managed initdb/BKI/post-bootstrap SQL, PL/pgSQL/Snowball and sync; 36 assertions, three clean same-process lifetimes and two persistence reopens per row |
| P4 | Required error/lifetime/upstream/interop assertions, full first-target matrix | Passed selected 21-case shared SQL inventory, 15 additional owner/recovery checks, boundary contracts and full Linux x64 raw/processed × JIT/NativeAOT matrix |
| P5 | Owning API and ordinary separate consumer, solution builds and required AOT runs | Passed owning API, ordinary separate consumer/default project graph, four-project solution and both normally trimmed NativeAOT products |
| P6 | Final tools regenerate, all selected gates and affected shared regressions pass | Passed immutable20 fresh 928 emission and exact retained-source processing/publication, all 14 non-consumer suites plus canonical matrix, current audits/offline warn/off reproduction; shared tests retain only the separately reproduced 16 compiler and 1 Blink baseline failures |

P0 → P1 → P2 → P3 → P4 → P5 → P6; independent boundary design and native controls
run in parallel. [Blocker ledger](BLOCKERS.md) records classifications,
reproducers, native expectations, reductions/fixes and full-input retries.
[Validation](VALIDATION.md) records actual commands/logs/status. P0 evidence alone did
not complete the campaign; final selected completion is recorded in
`artifacts/final-qualification.json` and VALIDATION.md.

## Validation selection

The recipe declares `consumer`, `native-control`, `boundary`, `atomics`,
`libc-build`, `initializer`, `modules`, `locale`, `portable-codegen`, `semaphore`,
`posix-host`, `backend-host` and `inventory`. Default suites: `consumer`.
Verification requires every declared suite. Consumer defaults must exercise the useful SQL
workflow; a help switch or unavailable-product check is packaging evidence only.
Verification retains required host/build/behavior gates and fails on missing core.
All four rows pass against the exact final canonical published libraries, with
normal trimming and no custom NativeAOT root directives.
Deterministic values and layout fields compare exactly. Original native and
translated logs remain retained. File interchange compares logical rows under a
matching database/file ABI, not cross-ABI bytes. [Selected qualification](QUALIFICATION.md) resolves the test-scope decision before P4: the shared 21-case SQL inventory, required boundary contracts and all four consumer matrix rows remain required; custom faults/fuzz/stress, performance thresholds and the full upstream regression runner are excluded from this first product. These exclusions do not permit stopping before the required product is qualified.

## Reproduction commands

```bash
bash Scripts/campaign.sh layout pglite --write
bash Scripts/campaign.sh layout pglite --check
bash pglite/scripts/fetch.sh --fetch missing
bash pglite/scripts/test.sh --suite native-control --fetch never
bash pglite/scripts/probe.sh --fetch never --tools reuse --jobs 1
bash pglite/scripts/translate.sh --fetch never --tools reuse --jobs 1
bash pglite/scripts/test.sh --suite boundary --fetch never --tools reuse
bash pglite/scripts/test.sh --suite atomics --fetch never --tools reuse
bash pglite/scripts/test.sh --suite libc-build --fetch never --tools reuse
dotnet build pglite/ManagedConsumer.slnx -c Release
bash pglite/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
```

Commands above describe entrypoints; only the validation ledger establishes
which were run and their outcomes. Invoke wrappers from an unrelated directory,
check usable local inputs with fetch disabled and warn/off hashing, and preserve
authored files through regeneration. Missing product libraries block consumer
and root-solution build and are never accepted as a passed SQL test.
