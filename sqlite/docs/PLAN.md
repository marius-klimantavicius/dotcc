# SQLite amalgamation to C# with dotcc

Status: M0–M8 are complete for the documented profile. The translated engine
includes core, JSON/JSONB and FTS5, with richer managed SQL workloads and canonical
function-pointer fields verified through source/object linking, JIT and NativeAOT.
M9 remains design-only. M10 is now active: span-based varargs and a ref-struct
VaList, with lifetime, semantics and allocation validation.
M11 is complete for Linux x64: the product defaults to a real OS-file VFS.
Windows/macOS implementations and JIT/AOT CI are present but have not run locally.
M12 is complete for Linux x64 on SQLite 3.53.4: real shared-memory WAL,
checkpoints/recovery and native interoperability pass under JIT and NativeAOT.
See `validation.md` for completed checks and `usage.md` for build commands.
Branch: `sqlite`. Campaign working directory: `<repo>/sqlite/`.

## Objective and constraints

Translate the actual SQLite amalgamation into executable, reusable unsafe C#
using dotcc. Include SQLite core and the pinned release's JSON/JSONB features.
Include FTS5 in the follow-up profile; FTS3/4 remain deferred. Preserve generic
compiler, callback, and virtual-table support for later extensions. A native SQLite dependency or a separately maintained C#
SQLite port does not satisfy this objective; native SQLite is the test oracle.

The starting dotcc **cannot parse all the C used by SQLite and has code emission
problems**. Discover and fix these in dotcc during implementation. A successful
preprocess, parse, or C# compilation alone is not completion.

Use dotcc's supplied headers, libc runtime, and existing ports/translations for
non-platform dependencies. Extend their shared implementations where necessary.
The original campaign used a simple, process-local memory VFS. M11 adds a real
file-backed default to the managed product and retains the deterministic memory
adapter for compiler/native differential and fault-injection tests. Keep SQLite's
pager, B-tree, SQL parser, VM, transactions, and JSON implementation translated
from upstream C. Function pointers are supported API, including callback tables;
unsafe C# is explicitly acceptable.

Native SQLite interop is not required. M11 explicitly permits OS-level P/Invoke
for locking and durability on Windows, Linux and macOS, preferring BCL operations
where their semantics suffice. Any future extensions will be written in C#
against the translated SQLite API and loaded through explicit application
registration. No dynamic library or assembly loading is required. Native SQLite
remains a separate test oracle, not a runtime dependency or extension host.

Commit locally after each coherent, tested change. Do not push. This document tracks the implementation campaign and its remaining milestones.

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
  tests/                     SQL/C API/VFS suites and oracle corpus
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
| OS layer | `SQLITE_OS_OTHER=1`; explicit init/end hooks. Managed product defaults to `dotcc-host`; deterministic C corpora retain `dotcc-memory`. |
| Threading | Initial supported profile: `SQLITE_THREADSAFE=0`, all calls serialized on one thread. VFS locks still model contention between connections. Thread-safe hosting is a later profile using dotcc runtime facilities. |
| Temporary storage | `SQLITE_TEMP_STORE=3`; managed product named databases/rollback journals use real files. Host VFS also supports temporary/delete-on-close handles. |
| Extensions | `SQLITE_OMIT_LOAD_EXTENSION`; explicit C# extensions, functions and virtual tables. No dynamic loading or native SQLite interop; OS-level VFS P/Invoke is allowed. |
| Core | Keep ordinary default core features: transactions, triggers, views, constraints, foreign keys, CTEs, window functions, indexes, virtual-table API, UTF-8/UTF-16 APIs, backup, incremental blobs, and date/time functions. |
| JSON/JSONB | Keep JSON enabled; verify every JSON/JSONB function/operator available in the pinned profile, including table-valued functions. |
| FTS | Enable `SQLITE_ENABLE_FTS5` for the current follow-up. Keep FTS3/4 disabled. Verify positive FTS5 probes and explicit configuration against native. |
| Other optional extensions | Leave optional RTREE, session, RBU, and similar opt-in extensions at upstream defaults; do not expand the campaign to them. |
| WAL/mmap | M12 adds real host WAL shared-memory methods and opt-in `PRAGMA journal_mode=WAL`; deterministic memory fixtures retain their fallback. Database mmap remains disabled. |

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

- [x] Add reproducible fetch/config/build/probe scripts and local ignore rules.
- [x] Download SQLite into `ref/` and record provenance in `docs/source.md`.
- [x] Record existing dotcc build/test results separately from SQLite failures.
- [x] Build a native reference from the pinned amalgamation, using the same
      feature definitions and, once available, the same memory VFS and C harness.
- [x] Attempt full preprocessing and translation; record actual blockers rather
      than treating existing C-support documentation as proof of compatibility.

Exit: repeatable inputs, a native baseline, and a recorded real SQLite failure.
Commit the baseline infrastructure and findings.

### M1 — Parse and lower the complete selected amalgamation

- [x] Apply the regression-first workflow until all enabled SQLite C parses and
      lowers to typed IR. Keep the amalgamation as one logical translation unit.
- [x] Investigate actual failures in macro expansion, typedef/tag scope, complex
      declarators, nested function-pointer signatures, aggregate initializers,
      array bounds, casts, and constant expressions; these are investigation
      areas, not asserted findings before the baseline runs.
- [x] Preserve line/source mapping and fail clearly on unsupported constructs.
      Record preprocessing/parsing time and memory for this large input.

Exit: the complete configured SQLite plus adapter/harness reaches C# emission
without skipping bodies. Commit each independent fix as it lands.

### M2 — Compute layouts and emit offsetof constants directly

The original source-generator requirement was withdrawn on 2026-09-10. The final
design keeps layout evaluation and direct constant emission inside dotcc; no
separate Roslyn offset generator is needed. Historical validation records retain
the original implementation steps.

- [x] Use one layout model for typed C constant evaluation and C# emission,
      now located in `DotCC.Lib/Layout/OffsetLayout.cs`.
- [x] Emit deterministic `size_t`-equivalent offsets, sizes, alignments, and
      required flexible-tail accessors directly into generated source.
- [x] Resolve C integer constant contexts before emission: array bounds, enums,
      case labels, and static assertions. Check emitted values against folding.
- [x] Support SQLite-required nested/anonymous structs/unions, typedefs, arrays,
      dotted/indexed paths, pointers/callbacks, bit-field storage, and flexible
      tails; diagnose invalid layouts and bit-field addresses explicitly.
- [x] Preserve metadata for native audits and object-link identity without
      requiring an analyzer, runtime reflection, or runtime offset evaluation.
- [x] Verify ordinary file/project/managed-library/object-link builds directly,
      with no `GeneratorDriver` or offset-analyzer project dependency.
- [x] Compare all 39 active SQLite offsets, 42 actual aggregate sizes/alignments,
      and eight pointer-array storage checks against native under JIT/NativeAOT.

Exit: required offsets agree with actual storage and native layout in constant
and runtime contexts. See `offset-layout.md` and `validation.md`.

### M3 — Compile emitted C# and preserve the callback API

- [x] Iterate on actual Roslyn diagnostics until all enabled SQLite code builds.
      Prioritize structural issues: aggregate storage/initialization, static
      lifetime, pointer conversions/arithmetic, integer promotions/overflow,
      function-pointer arrays/tables, switch/goto scopes, and address stability.
- [x] Preserve compatible `delegate*` signatures for translated callbacks.
      Use managed calling conventions for translated code and C# extensions;
      native ABI exports, unmanaged thunks, and `UnmanagedCallersOnly` are not
      required. Preserve callback/context lifetimes. Never use pointer types as
      generic type arguments.
- [x] Exercise callback registration and invocation through VFS methods, scalar/
      aggregate SQL functions, collations, busy/progress handlers, and destructors.
      Cover null callbacks, context pointers, `SQLITE_STATIC`/`SQLITE_TRANSIENT`,
      disposal, and GC stress so callable addresses and data remain valid.
- [x] Audit unresolved libc dependencies and extend dotcc's runtime as needed;
      exclude accidental imports of native SQLite. Keep runtime allocator,
      strings, memory operations, formatting, and math on dotcc implementations.
- [x] Start with a small C `main` harness for execution. Then expose a reusable
      C# assembly and minimal C-style callable API with clear ownership rules.
      Ensure the library shell exposes a usable managed API without requiring
      native `-shared` exports. Test a C# extension explicitly registered by the
      consumer, with no dynamic loading, and fix that seam if necessary.

Exit: generated C# builds, initialization and `SELECT 1` run, callback regressions
pass, and a separate C# consumer can call the translated engine. Commit per fix.

### M4 — Memory VFS and platform contract

- [x] Prefer a small portable C VFS in `src/`, compiled by dotcc and by the native
      oracle. Back storage with dotcc allocation/memory routines; use existing
      runtime facilities for clocks/randomness or an explicit deterministic test
      provider. Keep platform-specific code behind this adapter boundary.
- [x] Implement initialization/registration and a versioned `sqlite3_vfs` /
      `sqlite3_io_methods` table. Initial file methods version 1 is sufficient;
      advertise only implemented capabilities. Supply open/close, read/write,
      truncate/size, delete/access/path, lock/unlock/check-reserved-lock, sync,
      file-control, sector/device information, randomness, sleep, and time.
- [x] Model named files shared by handles, anonymous temporary files, rollback
      journals, delete-on-close, zero-filled growth, and short reads with required
      zero-fill plus `SQLITE_IOERR_SHORT_READ`. Respect access modes and error
      codes. Unknown file controls return `SQLITE_NOTFOUND` as appropriate.
- [x] Track lock ownership/transitions between connections; test successful and
      conflicting shared/reserved/exclusive requests. Sync can succeed as an
      in-memory operation, with no durability claim. Avoid reporting success for
      unsupported disk, mapping, shared-memory, or locking capabilities.
- [x] Test close/reopen within a process, rollback journals, injected I/O errors,
      transaction recovery after simulated failures, and resource cleanup.
      Distinguish this VFS from SQLite's own `:memory:` database mode.

Exit: native and translated engines pass the same VFS contract and multi-connection
tests under serialized calls. No persistence across processes, cross-process
locking, power-loss durability, or concurrent-thread guarantee is claimed.
The same C adapter and all listed contracts pass through both native and
translated engines.
Follow upstream [VFS](https://www.sqlite.org/vfs.html),
[VFS object](https://www.sqlite.org/c3ref/vfs.html), and
[file methods](https://www.sqlite.org/c3ref/io_methods.html) contracts.
M4 can begin early to support the M0 native baseline and M3 execution.

### M5 — Verify core SQL, APIs, JSON, and JSONB

- [x] Run identical inputs through native and translated builds of the pinned
      source/profile. Compare result codes, ordered rows, column types, byte
      lengths, text/blob bytes, errors, changes, and transaction outcomes. Use
      explicit ordering and controlled time/randomness; do not normalize away
      semantic differences. Store reviewable expected results for offline runs.
- [x] Cover prepare/bind/step/reset/finalize, null/text/blob/numeric conversions,
      UTF-8/UTF-16, embedded NULs, ownership/destructors, and open/close errors.
- [x] Cover DDL/DML, joins/subqueries, sorting/grouping/aggregates, indexes, views,
      triggers, foreign keys (enabled at runtime in tests), constraints, UPSERT,
      RETURNING, recursive CTEs, window functions, date/time, and pragmas.
- [x] Cover commit/rollback/savepoints, attached databases, multiple connections,
      backup, incremental blob I/O, and integrity/foreign-key checks. Export and
      reopen database images between native and translated engines in both
      directions using the VFS test harness; verify content and integrity.
- [x] Inventory the pinned release's JSON/JSONB surface. Test constructors,
      extraction/operators, updates/removal, validation/errors, aggregates,
      `json_each`/`json_tree` and any pinned JSONB table variants, JSON5 handling,
      Unicode/escaping, paths, null distinctions, nested values, and stored blobs.
      Check JSONB SQL type, operations, and round trips against that exact native
      version; do not promise binary stability across SQLite releases. Use the
      [upstream JSON reference](https://www.sqlite.org/json1.html) for the inventory.
- [x] Original core profile verified FTS absence while custom virtual tables and
      JSON table functions worked. M8 supersedes that profile with positive FTS5
      checks; preserve the non-FTS regression coverage.
- [x] Add bounded deterministic randomized differential tests with saved seeds
      and reduced regressions. Include allocation/I/O failure injection, large
      values, overflow boundaries, callback re-entry where allowed, and GC stress.
- [x] Incorporate relevant public upstream SQL/API tests from matching sources;
      document adaptations, executed cases, and skips. Native success alone does
      not count as translated coverage. Do not claim the proprietary TH3 suite or
      all SQLite tests passed when only a selected corpus ran.

Exit: the documented core and JSON/JSONB corpus matches native SQLite; all observed
parser/emitter/runtime defects have regression tests and full-amalgamation retries.

### M6 — Reproducibility and completion

- [x] Reproduce fetch -> checksum -> preprocess -> translate -> emit layout constants
      -> build -> test from a clean checkout, using documented commands rooted at
      `sqlite/`. Ensure generated output is never edited by hand.
- [x] Add a local campaign entry script and CI integration using existing repo
      conventions; CI configuration may live in `.github/workflows/`, while all
      SQLite workflow logic stays under `sqlite/scripts/`. No push is required.
- [x] Run full required repository regressions serially and NativeAOT smoke
      validation of the generated engine/consumer. Record actual results and
      timings, dependency closure, and any remaining unsupported platform profile.
- [x] Update `docs/validation.md`, the campaign blocker ledger, and shared
      `docs/C-SUPPORT.md` when generic features land. Document how a future FTS
      profile can reuse the same inputs, callback support, and oracle harness.
- [x] Commit the final verified milestone and report local branch/commit state.

Completion of the original M0–M6 campaign required a reusable translated C# engine with core plus JSON/JSONB,
the memory VFS contract, working function-pointer APIs, direct offsetof
constant emission, reproducible native differential evidence, and passing required
regressions. Parser success, a build-only stub, or a SQL smoke test is insufficient.


### M7 — Managed SQL workloads and canonical function pointers (complete)

- [x] Extend `tests/ManagedConsumer` to create related tables, indexes/views and
      triggers; insert parameter-bound data; and assert complete result sets.
- [x] Exercise simple predicates, joins, aggregates, correlated subqueries, CTEs,
      window functions, stored JSONB, updates, UPSERT, deletes, transactions and
      savepoint rollback. Verify final contents, changes, integrity and cleanup.
- [x] Emit one canonical static readonly typed function-pointer field per
      addressable function identity. All emitted address-taking/designator uses,
      callback tables and comparisons reuse that field. Managed library consumers
      can reuse public canonical fields for translated functions.
- [x] Initialize address fields independently of user global initializers to avoid
      static initialization cycles/default-null captures. Preserve function
      signature/calling convention, linkage, name hygiene and object-link identity;
      distinguish same-spelled static functions in different translation units.
- [x] Cover runtime-provided functions as well as translated ones. Callback values
      received from callers remain caller-owned values; caching must not freeze
      mutable callback variables, callback context, or dynamically selected targets.
      Null and destructor sentinels retain their semantics.
- [x] Give the managed consumer's own callbacks static readonly fields, reused by
      registration and identity checks. Verify reference equality to canonical
      addresses, actual invocation, global initialization, cross-object linking,
      repeated accesses under warmup/GC, and both JIT and NativeAOT.
- [x] Keep native-checked reduced compiler regressions and full SQLite retries.
      CS8909 may remain at pointer comparisons; correctness relies on reusing the
      captured address, not on assuming separate method-address captures coincide.

Exit: the expanded managed consumer and shared compiler regressions pass, and
all generated function-address uses refer to their canonical static fields.

### M8 — Build and verify FTS5 with dotcc (complete)

- [x] Coordinator delegates bounded compiler and FTS work, serializes shared
      builds/tests/commits, and continues fix/test/full-amalgamation retries until
      the enabled module actually executes correctly. Do not stop at compilation.
- [x] Enable FTS5 in the shared pinned SQLite configuration, compiled from the
      unchanged amalgamation. Keep the memory VFS, core and JSONB, with no native
      SQLite dependency or dynamic extension loading. Leave FTS3/4 deferred.
- [x] Record a native baseline with identical feature/ABI definitions. Reduce
      every new C parser/emitter/runtime defect, demonstrate a failing regression,
      fix dotcc, and retry the full enabled engine.
- [x] Add deterministic native/translated tests for table creation and CRUD,
      MATCH terms/phrases/prefixes/boolean/NEAR queries, column filtering, Unicode
      tokenization, ranking, highlighting/snippets, and vocabulary tables.
- [x] Exercise pinned built-in tokenizer variants, external-content synchronization,
      contentless storage, transactions, index maintenance/integrity, and reopen
      through the memory VFS. Test malformed-query diagnostics and changes after
      updates/deletes; normalize only genuinely nondeterministic output.
- [x] Cover the explicitly registered managed FTS extension API where exposed
      (tokenizer/auxiliary callback lifecycle) without adding dynamic loading.
      Document supported cases, upstream test selection and remaining limits.
- [x] Include an FTS CRUD/search demonstration in the separate managed consumer.
      Verify native differentials and the actual engine under JIT/NativeAOT; rerun
      core/JSONB/API/VFS/layout regressions and database-image interoperability.
- [x] Update scripts, CI inputs, feature assertions and docs that formerly expected
      FTS absence. Capture checks/commits in the ledger and commit locally, no push.

FTS5 is enabled by `SQLITE_ENABLE_FTS5` in the amalgamation. Verified SQL/API
coverage follows the pinned release and the
[upstream FTS5 reference](https://www.sqlite.org/fts5.html).
Exit: FTS5 builds with dotcc and its checked SQL/index/callback behavior agrees
with native, while existing required functionality remains verified.

### M9 — Roslyn post-processor for Cond.B (plan only; do not implement)

- [ ] Design a separate build-time Roslyn post-processing step over emitted C#,
      using symbols/semantic models to identify dotcc's exact `Cond.B` overload.
      Do not match arbitrary methods by text, modify upstream C, or reintroduce an
      offset source generator. Retain a comparison path with processing disabled.
- [ ] Inline the resolved overload's semantics: bool arguments stay bool, numeric
      arguments compare with correctly typed zero, pointer arguments compare with
      null, and CBool arguments preserve the existing conversion to int/nonzero.
      Preserve selected implicit/user conversions rather than dropping them.
- [ ] Preserve one evaluation of the operand, side effects, short-circuiting,
      checked/unchecked behavior and expression precedence. Cover assignments,
      increments, volatile/atomic reads, pointer/function-pointer conditions,
      floating NaN and signed zero, and shadowed names/aliases. Leave an invocation
      unchanged with a diagnostic if equivalence cannot be established.
- [ ] Specify ordering after whole-program/object linking and before final C#
      compilation; choose deterministic file/project integration, cancellation,
      source mapping and readable diagnostics. Keep Roslyn out of the translated
      runtime and preserve dotcc's AOT-compatible compiler packaging.
- [ ] Add syntax/semantic regressions plus original-vs-processed execution checks
      and SQLite core/JSONB/FTS5 JIT/AOT differentials. Measure compile time, code
      size, allocations and execution before adopting the pass: JIT/AOT may already
      inline these helpers, so benefit must be demonstrated.

This milestone is documentation only in the current phase; no postprocessor,
rewrites, build hook, or behavior change is authorized by this plan-only request.

### M10 — Span-based varargs and ref-struct VaList (implement now)

- [ ] Change emitted `params VaArg[] x` to
      `params ReadOnlySpan<VaArg> x`, with `VaList` becoming a **ref struct** holding
      a readonly `ReadOnlySpan<VaArg>` plus a mutable cursor. `VaArg` already is a
      readonly struct; keep its integer/pointer and floating representation.
- [ ] Inventory every producer/consumer: variadic function declarations and
      callbacks, direct/indirect calls, libc formatting, SQLite configuration and
      printf paths, headers/typedef aliases, IR, object metadata/linking and public
      managed APIs. Cover empty/expanded/explicit argument storage and forwarding.
- [ ] Preserve C default promotions (small integers to int, float to double),
      signedness, null/pointer/callback conversion, argument evaluation once and
      left-to-right emitted behavior. Keep `va_start`, `va_arg`, `va_copy` and
      `va_end` behavior: copied lists share borrowed storage but advance independent
      cursors, and forwarding must not accidentally consume the caller's cursor.
- [ ] Audit C uses of va_list address-taking, fields, globals, arrays, returns,
      captures, and callbacks. A ref struct cannot escape to heap storage or outlive
      its borrowed span. Specify scoped/ref signatures and diagnostics or explicit
      owned-storage alternatives for incompatible valid-C lifetime patterns; never
      silently generate dangling stack storage or discard supported paths.
- [ ] Verify compiler/Roslyn language-version support, overload resolution and
      managed API compatibility. Do not assume the params modifier guarantees zero
      allocation; inspect emitted code and measure empty/small/large/forwarded
      calls, stack pressure and any required heap-backed fallback.
- [ ] Add regression coverage for scalar/pointer/function-pointer promotions,
      nested forwarding, independent va_copy cursors, cleanup and escape rejection.
      Run core/JSONB/FTS5, Lua/Chibi, and JIT/NativeAOT comparisons and allocation
      benchmarks before adopting the representation.

The user has now authorized M10 implementation. Replace the borrowed-pack API,
retain explicit heap arrays as caller-owned backing storage when needed, and
validate lifetime restrictions and measured allocations. M9 remains plan-only.

### M11 — Real file-backed VFS (complete for the documented platform scope)

- [x] Make `dotcc-host` the managed library's default VFS through explicit
      `sqlite3_os_init` registration. Compile managed source alongside generated
      SQLite without rewriting the amalgamation or generated C#. Retain the named
      memory VFS and its unchanged deterministic fixture profile.
- [x] Use BCL random-access file reads/writes, length/truncation, safe handles,
      secure randomness, real time and sleeping. Implement SQLite short-read zero
      filling, temporary/delete-on-close files, open/create/exclusive/read-only
      modes, UTF-8 full paths, access checks and operation-specific errors.
- [x] Implement shared/reserved/pending/exclusive rollback-journal locks against
      SQLite's actual lock bytes, including upgrades, downgrades, failed upgrades,
      reserved probes, process exit and same-process connections. Use runtime OS
      detection: Linux OFD fcntl, Windows LockFileEx and coordinated macOS POSIX
      locks. Account for inode aliases and descriptor-close effects. Unsupported
      platforms/filesystems must fail explicitly; BSD support is optional.
- [x] Flush file contents and necessary directory entries; honor full sync on
      macOS with F_FULLFSYNC. Preserve journaling and hot-journal recovery. Start
      with version-1 I/O methods (rollback journals); WAL shared-memory and mmap
      support are separate future work and must not be advertised or stubbed.
- [x] Give every managed VFS callback a canonical static address, contain managed
      exceptions at callback boundaries and release handles/contexts on every
      normal/error path. Preserve the current serialized SQLite calling profile.
- [x] Add raw callback contract tests plus real SQL/JSONB/FTS persistence, readonly,
      temporary-file, rollback and reopen checks. Check multiple connections and
      independently running processes, including lock conflicts with native SQLite,
      abrupt process termination/hot journals, and file format interoperability.
- [x] Run Linux JIT and NativeAOT validation and configure Windows/macOS checks.
      Record which operating systems actually ran; do not claim remote CI results.
      Recheck the managed consumer and existing deterministic VFS regressions,
      update build/usage/validation docs and commit locally without pushing.

Exit: the ordinary managed library creates real durable database files by default,
and tested persistence, rollback, locking and recovery behavior agrees with native
SQLite. OS interop is limited to platform services; SQLite and extensions remain C#.

Evidence and OS limitations: [host VFS](host-vfs.md) and the M11 section in
[validation](validation.md). M11 left BSD, WAL/mmap and concurrent engine calls
outside its scope; M12 below adds WAL. M9/M10 remain plan-only.


### M12 — WAL shared memory and checkpoint/recovery (complete for the documented platform scope)

- [x] Move the pinned unchanged SQLite inputs to the 3.53.4 release,
      which fixes the upstream WAL-reset race; verify hashes and rerun translation.
- [x] Advertise version-2 I/O methods on `dotcc-host`: file-backed shared mappings,
      SQLite-compatible shared/exclusive shm range locks, memory barriers and
      unmap/cleanup. Preserve live mapping addresses during region growth and
      coordinate initialization/dead-man locks with native SQLite on each OS.
- [x] Handle multiple same-process connections and separate processes, readonly
      databases, missing/stale shm files, failed lock/map operations and cleanup.
      Keep Linux/Windows/macOS runtime selection and explicit unsupported-platform
      behavior. Database mmap (`xFetch`) is independent and remains disabled.
- [x] Add raw shared-memory contracts and SQL WAL tests for snapshots, single
      writer contention, stale-reader upgrades, savepoints, JSONB/FTS5, all
      checkpoint modes, multiple index regions, close/reopen and switching back
      to DELETE. Preserve rollback-journal and deterministic-memory regressions.
- [x] Compare independent native/managed processes, including abrupt termination
      after committed and uncommitted WAL writes and rebuilding a missing index.
      Run JIT/NativeAOT, managed consumer and maintenance-update corpus regressions.
- [x] Document WAL opt-in with `PRAGMA journal_mode=WAL`, platform evidence,
      same-host/local-filesystem requirements and serialized in-process calls.
      Commit often locally; do not push. M9/M10 remain plan-only.

Exit: WAL mode succeeds on the ordinary managed host VFS; mapped index sharing,
locking, snapshots, checkpoints and crash recovery pass against native SQLite.

Evidence: the M12 section in [validation](validation.md). Windows/macOS CI is
configured but unexecuted locally. M9/M10 remain plan-only.
