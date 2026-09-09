# SQLite amalgamation to C# with dotcc

Status: planned; implementation has not started.
Branch: `sqlite`. Campaign working directory: `<repo>/sqlite/`.

## Objective and constraints

Translate the actual SQLite amalgamation into executable, reusable unsafe C#
using dotcc. Include SQLite core and the pinned release's JSON/JSONB features.
Exclude FTS3/4/5 for now, while preserving the compiler and virtual-table support
needed to add FTS later. A native SQLite dependency or a separately maintained C#
SQLite port does not satisfy this objective; native SQLite is the test oracle.

The starting dotcc **cannot parse all the C used by SQLite and has code emission
problems**. Discover and fix these in dotcc during implementation. A successful
preprocess, parse, or C# compilation alone is not completion.

Use dotcc's supplied headers, libc runtime, and existing ports/translations for
non-platform dependencies. Extend their shared implementations where necessary.
File access and locking may use a simple, process-local memory VFS. Keep SQLite's
pager, B-tree, SQL parser, VM, transactions, and JSON implementation translated
from upstream C. Function pointers are supported API, including callback tables;
unsafe C# is explicitly acceptable.

Native-code interop is not required. Any future extensions will be written in C#
against the translated SQLite API and loaded through explicit application
registration. No dynamic library or assembly loading is required. Native SQLite
remains a separate test oracle, not a runtime dependency or extension host.

Commit locally after each coherent, tested change. Do not push. This document is
the planning deliverable; downloading and translating SQLite are future steps.

## Workspace and reproducible inputs

All SQLite-specific files, projects, scripts, tests, and artifacts live inside
`sqlite/`. Shared compiler/runtime fixes and generic regression tests belong in
their existing repository projects. Run campaign commands from `sqlite/`; scripts
must resolve paths from their own location so callers need not guess directories.

```text
sqlite/
  ref/                       downloaded, unmodified upstream inputs
    sqlite-amalgamation-<id>/ sqlite3.c, sqlite3.h, sqlite3ext.h, shell.c
    upstream-tests/          matching public test sources, if needed
  docs/
    PLAN.md                  this plan and milestone checkboxes
    source.md                version, source ID, exact URLs, checksums, provenance
    configuration.md         feature/ABI choices and effective compile options
    blockers.md              reduced failures, tests, fixes, and retry results
    validation.md            commands, results, coverage, exclusions, limitations
  config/                    one shared set of SQLite build definitions
  scripts/                   fetch, preprocess, translate, build, test, oracle
  src/                       C harness, memory VFS, minimal host-facing C# surface
  generators/                offsetof source generator and build integration
  tests/                     SQL/C API/VFS suites, generator tests, oracle corpus
  generated/                 regenerated C# and layout metadata; ignored
  build/                     native/.NET outputs; ignored
  artifacts/                 preprocessing, diagnostics, timings, diffs; ignored
```

At implementation start, choose and pin an official stable amalgamation release
with JSONB (at least 3.45.0); record its exact version, `SQLITE_SOURCE_ID`, archive
URL, and SHA-256 in `docs/source.md`. Download and extract beneath `ref/`, checking
the recorded checksum on every fetch. Do not depend on a moving latest URL.
Record matching public test-source URLs/checksums separately. Commit the fetch
recipe and provenance; ignore fetched archives and extracted upstream sources.
Preserve upstream files unchanged. Keep any necessary configuration in `config/`
and adapters in `src/`; do not patch SQLite C or generated C# to bypass failures.
The amalgamation includes the generated SQL parser, so Lemon is not a prerequisite
for the translation. [Upstream amalgamation documentation](https://www.sqlite.org/amalgamation.html).

## Initial feature and platform profile

Use a documented C dialect supported by the pinned source (start with C17) and
dotcc's LP64 model: 64-bit pointers, `long`, `size_t`, and `ptrdiff_t`. Start on a
64-bit host and use a matching LP64 native compiler for layout comparisons. Record
endianness, SDK/compiler versions, and whether LALR.CC resolves locally or through
NuGet. Do not impersonate GCC or a host OS to select unsupported compiler tricks.

| Area | Planned setting or behavior |
| --- | --- |
| OS layer | `SQLITE_OS_OTHER=1`; provide `sqlite3_os_init`, `sqlite3_os_end`, and register the memory VFS. |
| Threading | Initial supported profile: `SQLITE_THREADSAFE=0`, all calls serialized on one thread. VFS locks still model contention between connections. Thread-safe hosting is a later profile using dotcc runtime facilities. |
| Temporary storage | `SQLITE_TEMP_STORE=3`; named databases and rollback journals still exercise the memory VFS. |
| Extensions | `SQLITE_OMIT_LOAD_EXTENSION`; retain explicit registration of C# extensions, functions, and virtual tables against the translated API. No dynamic loading or native-code interop. |
| Core | Keep ordinary default core features: transactions, triggers, views, constraints, foreign keys, CTEs, window functions, indexes, virtual-table API, UTF-8/UTF-16 APIs, backup, incremental blobs, and date/time functions. |
| JSON/JSONB | Keep JSON enabled; verify every JSON/JSONB function/operator available in the pinned profile, including table-valued functions. |
| FTS | Leave all FTS enable macros undefined, including FTS3/4/5 and tokenizer variants. Check effective configuration and negative SQL probes. |
| Other optional extensions | Leave optional RTREE, session, RBU, and similar opt-in extensions at upstream defaults; do not expand the campaign to them. |
| WAL/mmap | Keep core code compiled. Initial VFS has no shared memory or mapped reads, so WAL operation and mmap acceleration are outside the supported platform profile. Test the actual fallback/refusal behavior. |

Do not add `SQLITE_OMIT_*` switches to hide compiler defects or remove required
core/JSON functionality. Assert the effective profile using macro inspection,
`sqlite3_compileoption_get`/`PRAGMA compile_options`, and positive/negative SQL
probes. Keep diagnostic/debug builds distinct from the release profile. SQLite
documents OS replacement and compile-time options in its
[configuration reference](https://www.sqlite.org/compile.html).

## Required failure-driven workflow

For every parser, lowering, C# compiler, or runtime failure:

1. Run the pinned full `ref/.../sqlite3.c` plus the current adapter/harness through
   preprocessing, translation, C# compilation, and execution as far as possible.
   Preserve stage, command, source location, diagnostic, and tool revision under
   `artifacts/`; summarize actionable blockers in `docs/blockers.md`.
2. Reduce the first root cause or a repeated diagnostic family to valid minimal C.
   Preserve the relevant macros, declarators, layouts, or control flow. Classify
   preprocessor/parser, IR/type semantics, emitter, runtime, or adapter failures.
3. **Add a regression test and demonstrate its failure before fixing it.** Use
   `DotCC.Tests` for focused preprocessing/parsing/IR/emission checks and
   `DotCC.FunctionalTests/Fixtures` for C -> emitted C# -> Roslyn compile -> run
   checks. Emitted-string checks alone do not establish code generation correctness.
4. Verify the fixture's expected behavior with a real matching-ABI C compiler.
   Keep native processes in opt-in oracle/script paths; normal repository fixture
   tests remain in-process. Add SQLite integration coverage for semantic bugs.
5. Fix the shared compiler or runtime at the structural cause. Do not rewrite
   generated C# with regex, replace SQLite functions, or silently swallow failures.
   Grammar changes require a full rebuild before testing generated parser tables.
6. Run focused regressions, then required affected suites serially. **Retry the
   full SQLite amalgamation after every fix**, record the new first failure or
   successful stage, and update the blocker entry with test and commit references.
7. Commit a coherent test+fix+evidence change locally. If SQLite exposes the next
   blocker, record it explicitly; it does not invalidate a verified incremental fix.

Use `dotnet build ../dotcc.sln -c Release`, then the unit and functional test
projects serially with `--no-build` when that build is fresh. Use existing oracle
conventions; add a Linux-native oracle script under `sqlite/scripts/` where the
repository's WSL-specific oracle is unsuitable. Check the NuGet LALR.CC build path
before milestone completion; local sibling changes alone must not hide missing
dependencies. Run existing Lua/chibi regressions after shared semantic changes,
and Zig/WAT checks when shared IR/layout behavior changes. Do not commit unrelated
workspace changes or push any branch.

## Milestones

### M0 — Pin inputs and establish the baseline

- [ ] Add reproducible fetch/config/build/probe scripts and local ignore rules.
- [ ] Download SQLite into `ref/` and record provenance in `docs/source.md`.
- [ ] Record existing dotcc build/test results separately from SQLite failures.
- [ ] Build a native reference from the pinned amalgamation, using the same
      feature definitions and, once available, the same memory VFS and C harness.
- [ ] Attempt full preprocessing and translation; record actual blockers rather
      than treating existing C-support documentation as proof of compatibility.

Exit: repeatable inputs, a native baseline, and a recorded real SQLite failure.
Commit the baseline infrastructure and findings.

### M1 — Parse and lower the complete selected amalgamation

- [ ] Apply the regression-first workflow until all enabled SQLite C parses and
      lowers to typed IR. Keep the amalgamation as one logical translation unit.
- [ ] Investigate actual failures in macro expansion, typedef/tag scope, complex
      declarators, nested function-pointer signatures, aggregate initializers,
      array bounds, casts, and constant expressions; these are investigation
      areas, not asserted findings before the baseline runs.
- [ ] Preserve line/source mapping and fail clearly on unsupported constructs.
      Record preprocessing/parsing time and memory for this large input.

Exit: the complete configured SQLite plus adapter/harness reaches C# emission
without skipping bodies. Commit each independent fix as it lands.

### M2 — Implement offsetof with a source generator

Current code has an `OffsetOf` IR node, constant-context layout evaluation in
`IrBuilder`, and a C# expression path that uses an inline `Func<ulong>` lambda with
stack-instance address subtraction. Existing offset tests and dotted-designator
fixtures are starting coverage, not a complete solution for SQLite.

- [ ] Add a Roslyn incremental source-generator project under `generators/`.
      Keep its design generic to dotcc aggregates despite its campaign location.
      Explicitly set a Roslyn-compatible target framework instead of inheriting
      the repository's `net10.0` target blindly.
- [ ] Specify deterministic input metadata from typed IR: target ABI, aggregate
      identity, fields and lowered storage, nesting, arrays, alignment/packing,
      unions, and requested member designators. Emit it with generated C# as an
      `AdditionalFiles` input (or equivalent documented structured contract).
- [ ] Share a single layout model between compiler constant evaluation and the
      generator. Generate typed `size_t`-equivalent offset constants, required
      access helpers/metadata, and diagnostics. Ensure generated storage layout
      matches that model; audit `sizeof` and alignment together with offsets.
      Do not maintain a hand-written table of SQLite struct offsets.
- [ ] Resolve C integer constant expressions during dotcc lowering from that
      shared model, including array bounds, enums, case labels, and static
      assertions. Roslyn runs later and cannot retroactively supply constants
      needed to parse/lower C; avoid a circular build dependency. Cross-check
      compiler-folded constants against source-generator output.
- [ ] Support typedefs, named/anonymous nested structs/unions, scalar and array
      members, fixed buffers, inline storage, pointer/function-pointer fields,
      dotted paths, and indexed designators needed by SQLite. Audit flexible
      array tails and fields following bit-field storage; diagnose `offsetof`
      applied to a bit-field itself and other invalid/nonconstant requests.
- [ ] Use generated helpers for valid runtime-only access when necessary, with
      no null-pointer dereference, runtime reflection, dynamic code generation,
      or per-call delegate allocation. Runtime helpers cannot stand in for C
      integer constants. Unsupported layouts must produce a clear diagnostic.
- [ ] Wire generation into emitted project/build output and in-process Roslyn
      fixture compilation via `GeneratorDriver`; cover object/link output as
      needed. Preserve standalone `--emit=file` usability by materializing the
      same generated declarations through the shared generation implementation.
      No hidden analyzer installed only on the developer's machine.
- [ ] Test generator determinism, invalid input diagnostics, constant contexts,
      and representative SQLite aggregates. Compare native C `sizeof`/alignment/
      offsets, compiler constants, generated constants, and actual unsafe C#
      address differences. Check supported 64-bit target layouts and AOT output.

Exit: all SQLite-required offsets come through the tested generator integration,
agree with actual storage/native layout, and work in both constant and runtime
contexts. Commit model, generator, integration, and regressions in focused units.
M1/M2 may interleave where layout-dependent declarations block lowering.

### M3 — Compile emitted C# and preserve the callback API

- [ ] Iterate on actual Roslyn diagnostics until all enabled SQLite code builds.
      Prioritize structural issues: aggregate storage/initialization, static
      lifetime, pointer conversions/arithmetic, integer promotions/overflow,
      function-pointer arrays/tables, switch/goto scopes, and address stability.
- [ ] Preserve compatible `delegate*` signatures for translated callbacks.
      Use managed calling conventions for translated code and C# extensions;
      native ABI exports, unmanaged thunks, and `UnmanagedCallersOnly` are not
      required. Preserve callback/context lifetimes. Never use pointer types as
      generic type arguments.
- [ ] Exercise callback registration and invocation through VFS methods, scalar/
      aggregate SQL functions, collations, busy/progress handlers, and destructors.
      Cover null callbacks, context pointers, `SQLITE_STATIC`/`SQLITE_TRANSIENT`,
      disposal, and GC stress so callable addresses and data remain valid.
- [ ] Audit unresolved libc dependencies and extend dotcc's runtime as needed;
      exclude accidental imports of native SQLite. Keep runtime allocator,
      strings, memory operations, formatting, and math on dotcc implementations.
- [ ] Start with a small C `main` harness for execution. Then expose a reusable
      C# assembly and minimal C-style callable API with clear ownership rules.
      Ensure the library shell exposes a usable managed API without requiring
      native `-shared` exports. Test a C# extension explicitly registered by the
      consumer, with no dynamic loading, and fix that seam if necessary.

Exit: generated C# builds, initialization and `SELECT 1` run, callback regressions
pass, and a separate C# consumer can call the translated engine. Commit per fix.

### M4 — Memory VFS and platform contract

- [ ] Prefer a small portable C VFS in `src/`, compiled by dotcc and by the native
      oracle. Back storage with dotcc allocation/memory routines; use existing
      runtime facilities for clocks/randomness or an explicit deterministic test
      provider. Keep platform-specific code behind this adapter boundary.
- [ ] Implement initialization/registration and a versioned `sqlite3_vfs` /
      `sqlite3_io_methods` table. Initial file methods version 1 is sufficient;
      advertise only implemented capabilities. Supply open/close, read/write,
      truncate/size, delete/access/path, lock/unlock/check-reserved-lock, sync,
      file-control, sector/device information, randomness, sleep, and time.
- [ ] Model named files shared by handles, anonymous temporary files, rollback
      journals, delete-on-close, zero-filled growth, and short reads with required
      zero-fill plus `SQLITE_IOERR_SHORT_READ`. Respect access modes and error
      codes. Unknown file controls return `SQLITE_NOTFOUND` as appropriate.
- [ ] Track lock ownership/transitions between connections; test successful and
      conflicting shared/reserved/exclusive requests. Sync can succeed as an
      in-memory operation, with no durability claim. Avoid reporting success for
      unsupported disk, mapping, shared-memory, or locking capabilities.
- [ ] Test close/reopen within a process, rollback journals, injected I/O errors,
      transaction recovery after simulated failures, and resource cleanup.
      Distinguish this VFS from SQLite's own `:memory:` database mode.

Exit: native and translated engines pass the same VFS contract and multi-connection
tests under serialized calls. No persistence across processes, cross-process
locking, power-loss durability, or concurrent-thread guarantee is claimed.
Follow upstream [VFS](https://www.sqlite.org/vfs.html),
[VFS object](https://www.sqlite.org/c3ref/vfs.html), and
[file methods](https://www.sqlite.org/c3ref/io_methods.html) contracts.
M4 can begin early to support the M0 native baseline and M3 execution.

### M5 — Verify core SQL, APIs, JSON, and JSONB

- [ ] Run identical inputs through native and translated builds of the pinned
      source/profile. Compare result codes, ordered rows, column types, byte
      lengths, text/blob bytes, errors, changes, and transaction outcomes. Use
      explicit ordering and controlled time/randomness; do not normalize away
      semantic differences. Store reviewable expected results for offline runs.
- [ ] Cover prepare/bind/step/reset/finalize, null/text/blob/numeric conversions,
      UTF-8/UTF-16, embedded NULs, ownership/destructors, and open/close errors.
- [ ] Cover DDL/DML, joins/subqueries, sorting/grouping/aggregates, indexes, views,
      triggers, foreign keys (enabled at runtime in tests), constraints, UPSERT,
      RETURNING, recursive CTEs, window functions, date/time, and pragmas.
- [ ] Cover commit/rollback/savepoints, attached databases, multiple connections,
      backup, incremental blob I/O, and integrity/foreign-key checks. Export and
      reopen database images between native and translated engines in both
      directions using the VFS test harness; verify content and integrity.
- [ ] Inventory the pinned release's JSON/JSONB surface. Test constructors,
      extraction/operators, updates/removal, validation/errors, aggregates,
      `json_each`/`json_tree` and any pinned JSONB table variants, JSON5 handling,
      Unicode/escaping, paths, null distinctions, nested values, and stored blobs.
      Check JSONB SQL type, operations, and round trips against that exact native
      version; do not promise binary stability across SQLite releases. Use the
      [upstream JSON reference](https://www.sqlite.org/json1.html) for the inventory.
- [ ] Verify FTS modules are absent while custom virtual tables and JSON table
      functions work, leaving a tested extension point for later FTS work.
- [ ] Add bounded deterministic randomized differential tests with saved seeds
      and reduced regressions. Include allocation/I/O failure injection, large
      values, overflow boundaries, callback re-entry where allowed, and GC stress.
- [ ] Incorporate relevant public upstream SQL/API tests from matching sources;
      document adaptations, executed cases, and skips. Native success alone does
      not count as translated coverage. Do not claim the proprietary TH3 suite or
      all SQLite tests passed when only a selected corpus ran.

Exit: the documented core and JSON/JSONB corpus matches native SQLite; all observed
parser/emitter/runtime defects have regression tests and full-amalgamation retries.

### M6 — Reproducibility and completion

- [ ] Reproduce fetch -> checksum -> preprocess -> translate -> source-generate
      -> build -> test from a clean checkout, using documented commands rooted at
      `sqlite/`. Ensure generated output is never edited by hand.
- [ ] Add a local campaign entry script and CI integration using existing repo
      conventions; CI configuration may live in `.github/workflows/`, while all
      SQLite workflow logic stays under `sqlite/scripts/`. No push is required.
- [ ] Run full required repository regressions serially and NativeAOT smoke
      validation of the generated engine/consumer. Record actual results and
      timings, dependency closure, and any remaining unsupported platform profile.
- [ ] Update `docs/validation.md`, the campaign blocker ledger, and shared
      `docs/C-SUPPORT.md` when generic features land. Document how a future FTS
      profile can reuse the same inputs, callback support, and oracle harness.
- [ ] Commit the final verified milestone and report local branch/commit state.

Completion requires a reusable translated C# engine with core plus JSON/JSONB,
the memory VFS contract, working function-pointer APIs, the integrated offsetof
source generator, reproducible native differential evidence, and passing required
regressions. Parser success, a build-only stub, or a SQL smoke test is insufficient.
