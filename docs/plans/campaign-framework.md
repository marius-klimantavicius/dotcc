Consolidate the seven translation campaigns behind standard Bash helpers, one
Python runner, and a small recipe API. Share execution, source acquisition,
translation, output handling, and test orchestration; keep source adaptations,
host contracts, and behavioral assertions with each project.

Status: implementation under review, 2026-09-26. The shared runner, recipes,
Bash entrypoints and canonical layouts are implemented. See
[current usage and validation scope](../campaigns.md). The design below remains
the target; comprehensive specialist orchestration/restore migration, clean
reproduction audits and the complete multi-platform validation matrix remain
follow-up work. No commits have been made for this implementation.

Hash policy revised per user preference: ordinary hash checks are limited and
advisory by default, and can be disabled. Strict input guards belong only to
text replacements that depend on exact input, unless strict verification is
explicitly requested. Compilation, structural checks, and behavioral tests
determine compatibility with changed sources.

**What exists today**

| Project | Current orchestration | Differences the framework must represent |
| --- | --- | --- |
| SQLite | [build.sh](../../sqlite/scripts/build.sh), [emit-engine.sh](../../sqlite/scripts/emit-engine.sh), [verify.sh](../../sqlite/scripts/verify.sh) | `build` fetches, builds tools, translates, postprocesses, and builds. Direct amalgamation emission; guarded source preparation; separate translated corpus executables and native transcript comparisons. Verification also runs repository tests and optional port regressions. |
| picotls | [translate.sh](../../picotls/scripts/translate.sh), [campaign.py](../../picotls/scripts/campaign.py), [test-campaign.py](../../picotls/scripts/test-campaign.py) | Direct multi-source emission, raw snapshot, processed product, provider project. Tests consume existing translations and native oracle libraries; compare raw/processed and JIT/AOT results, including independent peers. |
| MsQuic | [translate.sh](../../msquic/scripts/translate.sh), [build-product.py](../../msquic/scripts/build-product.py), [translate-fast.py](../../msquic/scripts/translate-fast.py), [test-all.sh](../../msquic/scripts/test-all.sh) | Normal translation includes host-contract/ABI gates and closure freezing. Fast translation is explicitly unqualified. Separate object emission/linking, source selection from upstream CMake, host overlay, inline exports, and picotls-dependent tests. |
| Blink | [translate.py](../../blink/scripts/translate.py), [assemble-core.py](../../blink/scripts/assemble-core.py), [core_inputs.py](../../blink/scripts/core_inputs.py) | Threaded and single-thread profiles; native oracle runs during translation. Content-addressed object inputs, typed boundary checks, custom project assembly, authored host isolation, output locking and rollback. Many guest/service/regression suites. |
| Valkey | [inputs.py](../../valkey/scripts/inputs.py), [pipeline.py](../../valkey/scripts/pipeline.py), [translate.py](../../valkey/scripts/translate.py), [verify.sh](../../valkey/scripts/verify.sh) | Strict source-tree verification, source generators and managed adaptations, per-unit options, parallel objects, instance methods, notices, tool snapshots, two-output rollback. Probe/unadapted modes must never publish a product. |
| libsmb2 | [common.py](../../libsmb2/scripts/common.py), [translate.py](../../libsmb2/scripts/translate.py), [test.py](../../libsmb2/scripts/test.py), [verify.py](../../libsmb2/scripts/verify.py) | Async product versus legacy Libc test profile. Parallel objects and host-binding audit. Despite its existing-output description, `test.py` also translates the legacy profile and prepares native oracle evidence. |
| Pinta | [translation.py](../../pinta/scripts/translation.py), [stage.py](../../pinta/scripts/stage.py), [test.py](../../pinta/scripts/test.py) | Direct multi-source emission through preinclude wrappers; hash-checked patches; release/debug and pristine variants; raw/processed products. Build validates provenance. Owning-consumer tests run a JIT/AOT matrix on supported x64 platforms. |

The recurring implementations are path resolution, Python/tool discovery,
download/hash/extraction, source staging, subprocess/log/timeout handling,
compiler flags, object scheduling, generated-file manifests, raw snapshots,
restore/postprocess/build, publication, receipts, and test matrices.

Several differences are deliberate contracts, not incidental duplication:

- Fetch policies differ. SQLite re-extracts verified archives; picotls' current
  `--no-fetch` checks directory existence; Valkey verifies the complete local
  tree. Blink verifies its archive but explicitly permits existing source edits.
  MsQuic documents manually supplied sources with `--no-fetch`.
- `--offline` currently means upstream acquisition only in Blink, with normal
  NuGet restore still allowed. It must not become a promise of no network access.
- Some tests translate their own C harnesses. These are distinct artifacts from
  the delivered library and must retain their own inputs and evidence.
- Success has different scopes: emitted code, buildable product, ABI proof,
  behavioral suite, and qualified delivery. Existing historical evidence must
  never be relabeled as evidence for newly generated output.

**Proposed structure**

```text
Scripts/campaign.sh                 # repository-wide Bash entrypoint
Scripts/campaign-common.sh          # shared paths, Python resolution, dispatch
Scripts/campaign.py                 # CLI, usable from any working directory
Scripts/campaigns/
    model.py                        # recipe, source, task, artifact, suite types
    layout.py                       # canonical paths, scaffolding, validation
    runner.py                       # dependency planning, execution, resources
    process.py                      # commands, logs, timeouts, cancellation
    inputs.py                       # acquisition, extraction, verification
    identity.py                     # input/tool/output manifests and receipts
    translation.py                  # direct emission and object/link execution
    delivery.py                     # raw/processed copies and promotion
    testing.py                      # suite matrices, comparisons, summaries
    tests/                          # framework contract/failure tests
<project>/scripts/campaign.py        # recipe and project-specific hooks
<project>/scripts/common.sh          # thin binding to shared Bash helpers
<project>/scripts/{fetch,translate,build,test,verify,probe}.sh
```

Use Python's standard library initially, with typed dataclasses and an explicit
registry of the seven projects. Python is already required throughout these
campaigns. Bash helpers are a permanent, supported interface for every project,
not just temporary compatibility wrappers. Also keep direct Python invocation
available on Linux and Windows, including environments without Bash. Declare
and check the minimum Python version, and provide platform-specific process/lock
helpers.

**Bash helpers and Python resolution**

Every project provides `scripts/common.sh` and the six standard command helpers
shown above. A helper dispatches its project and command to the common runner,
forwards `"$@"` unchanged, and preserves its exit status. Keep extra specialist
helpers where needed; they use the same common support.

Implement Python discovery once in `Scripts/campaign-common.sh`, following
[SQLite's common.sh](../../sqlite/scripts/common.sh):

```bash
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
    PYTHON_CMD=python
fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
    echo "Python 3 is required" >&2
    exit 1
fi
```

Check the framework's minimum supported version after selection. Never assume
`python3` exists merely because a command was found or a shebang names it. Bash
launches every Python helper through `"$PYTHON_CMD"`; Python child processes use
`sys.executable`. Retained scripts must receive/use that same selection instead
of performing their own discovery or hardcoding `python3`.

Resolve repository/project paths relative to `${BASH_SOURCE[0]}`, quote paths,
and use `set -euo pipefail`. Sourcing the shared helper must not fetch, build,
or create output directories. Dispatch through `exec` so signals and exit codes
reach the caller. The project `common.sh` only binds its project/root and sources
the shared helper; the Python-resolution block is not copied into seven files.

For example, the eventual standard `pinta/scripts/translate.sh` is just:

```bash
#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"
campaign_exec translate "$@"
```

The shared `campaign_exec` uses the project binding to execute the repository
runner with the selected Python. `Scripts/campaign.sh` exposes the same commands
with an explicit project argument, including multi-project operations.

**Required project layout**

The framework owns path conventions and validates them. Recipes declare a
product name and default profile, not arbitrary locations for the standard
solution and delivered library. All seven projects converge on:

```text
<project>/
    ManagedConsumer.slnx                     # checked-in, root entry solution
    config/                                 # pins, source lists, overrides
    scripts/                                # recipe, common.sh, Bash helpers
    src/                                    # authored host/facade projects
    samples/ManagedConsumer/                 # owning consumer/example
        ManagedConsumer.csproj
    tests/                                  # suites, fixtures, test harnesses
    ref/                                    # acquired or supplied upstream inputs
    generated/
        Translated<Name>/                   # default profile, processed form
            Translated<Name>.csproj
        Translated<Name>.Raw/               # default profile, raw form
            Translated<Name>.csproj
        profiles/<profile>/                 # non-default profiles, same naming
            Translated<Name>/
            Translated<Name>.Raw/
    build/                                  # private staging, objects, binaries
    artifacts/campaign/<run-id>/            # logs, receipts, test results
```

| Project | Canonical processed project |
| --- | --- |
| sqlite | `sqlite/generated/TranslatedSqlite/TranslatedSqlite.csproj` |
| picotls | `picotls/generated/TranslatedPicotls/TranslatedPicotls.csproj` |
| msquic | `msquic/generated/TranslatedMsQuic/TranslatedMsQuic.csproj` |
| blink | `blink/generated/TranslatedBlink/TranslatedBlink.csproj` |
| valkey | `valkey/generated/TranslatedValkey/TranslatedValkey.csproj` |
| libsmb2 | `libsmb2/generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj` |
| pinta | `pinta/generated/TranslatedPinta/TranslatedPinta.csproj` |

Raw and processed forms keep the same project/assembly name and differ by
directory. The root solution selects the default processed library, its authored
host/facade dependencies, and the managed consumer. Tests select raw or alternate
profiles through explicit project properties; do not build competing forms into
the same output/intermediate directories. Test-only projects stay under `tests/`
and need not be included in the consumer solution. Framework/compiler tools are
not consumer solution dependencies.

Expose these paths through one `Layout` object used by recipes, Bash dispatch,
project assembly, test properties, and publication. Profile names and artifact
paths must remain within their declared roots. Internal generated-source shape
can vary where necessary (for example Blink's `Sources/`); the public product
directory and project path cannot.

Add `layout <project> --check|--write`. Check validates the root solution,
standard helpers, project references, and configured output destinations without
requiring generated products on a fresh checkout. Write scaffolds missing
directories/helpers and creates or updates framework-owned solution entries
from declared projects. Preserve user solution entries and authored files; report
conflicts rather than silently overwrite them. Required authored projects must
exist; scaffolding does not invent application behavior.

Before execution, validate checked-in structure and create only the working
directories needed by the selected tasks. Before promotion, validate canonical
output paths and project references. Missing required structure is a structural
error with a concrete repair command, independent of `--hashes`. `list` and
`--dry-run` report layout issues without creating or repairing anything.

Migration must move picotls' nested consumer solution to the project root, add
root solutions for MsQuic and Pinta, and normalize raw/profile directories and
consumer locations across all projects. Update relative references, MSBuild
properties/targets, test helpers, documentation, and CI together. Preserve
specialist staging paths, including Blink's canonical emission paths, until
their path-sensitive behavior is explicitly migrated. Public delivery layout
and internal staging are separate concerns.

Recipes reference existing configuration instead of copying it into another
manifest. Use ordinary Python hooks for nontrivial transformations; avoid a
second programming language encoded in JSON. A recipe declares:

- Source groups: product, upstream tests, native oracle, independent peers, guest
  assets; archive pins, extraction roots, and verification policies.
- Profiles and ordered translation units: common/per-unit options, ordered
  include paths, overrides, emission mode, link options, split settings,
  class/namespace, and authored host context.
- Named artifacts: raw and processed projects, native executables, translated
  test executables, consumers, published AOT apps, and their input identities.
- Named tasks and dependencies, including project-specific prerequisite gates.
- Suites: artifact dependencies, supported platforms/profiles/forms/modes,
  timeout, resource locks, result validator, and qualification scope.

The extension boundary is small: `prepare_sources(context) -> SourceSet`,
`assemble_project(context, emission) -> ProjectSet`, plus named validators and
specialist tasks. Hooks return declared artifacts and evidence. They use
`context.run(...)` and receive resolved paths/policies; they do not implement
their own fetching, timeout manager, receipt writer, or success publication.
Extract other reusable hooks only after two recipes actually need them.

Existing scripts can temporarily be task adapters. That is a migration tool,
not the completion criterion: common lifecycle code must eventually be removed
from the project scripts.

**Command contract**

```sh
bash Scripts/campaign.sh list
bash Scripts/campaign.sh list libsmb2 --suites
bash Scripts/campaign.sh layout picotls --check
bash sqlite/scripts/fetch.sh --group product
bash valkey/scripts/translate.sh --fetch never --tools reuse --jobs 4
bash picotls/scripts/build.sh --form processed
bash pinta/scripts/test.sh --form all --mode all --profile release
bash libsmb2/scripts/verify.sh --fetch missing
bash Scripts/campaign.sh verify all --with repository --dry-run
```

| Command | Contract |
| --- | --- |
| `list` | Show projects, profiles, suites, prerequisites, supported matrices, and scope without modifying the workspace. |
| `layout` | Check or scaffold the standard project structure and root consumer solution. No fetching or translation. |
| `fetch` | Acquire selected source groups, check archive structure, and apply the chosen hash policy. Never change the selected upstream revision. |
| `translate` | Check required inputs, prepare tools/staging, emit, preserve raw output, postprocess, build-check requested forms, and promote generated projects. No behavioral qualification is implied. Explicit recipe prerequisite checks remain visible. |
| `build` | Build existing generated projects or their declared owning project. Report provenance differences according to the hash policy. Never regenerate the library. |
| `test` | Run selected suites against existing library artifacts. May compile declared native or translated test harnesses and publish test consumers. Never silently regenerate a missing library/profile. |
| `verify` | Compose fresh translation, required profile/dependency preparation, audits, and the project's declared verification suites. Record the exact coverage. Add repository suites once per invocation with `--with repository`. |
| `probe` | Emit selected units for diagnostics, optionally using unadapted/pristine inputs. Never promote product output or issue a qualification receipt. |

Common policies:

- `--fetch missing|never`: acquisition policy only; required-file and structural
  checks always run, while hash checks follow `--hashes`.
  Default to `missing` for fetch/translate/verify and `never` for build/test/probe.
  Cached archives may be extracted with `never`. An existing usable source tree
  suffices without an archive or trusted hash manifest in the default mode.
- `--hashes off|warn|strict`: default `warn` for every command, including `verify`.
  Check only available archive digests and a small declared set of relevant
  files; no mandatory whole-tree hashing. Differences or missing historical
  hashes produce concise warnings, not failed builds/tests. `off` skips ordinary
  hash calculation and comparison. `strict` is an explicit reproducibility
  option; recipes declare its required evidence. Exact replacement guards apply
  in every mode. See the detailed policy below.
- `--tools build|reuse`: build tools once per invocation, or require and snapshot
  existing binaries. Translate/verify default to build; build/test need compiler
  tools only if a selected task actually consumes them.
- `--restore normal|locked|none`: separate NuGet behavior from source fetching.
  Locked mode requires appropriate package lock files; it is not itself offline.
  `none` requires existing restore assets and disables implicit restore on all
  managed build/publish tasks. Native tool downloads must also be declared.
- `--profile`, `--form raw|processed|all`, `--mode jit|aot|all`, `--rid`, and
  repeatable `--suite`: reject unsupported combinations before work begins.
  Raw-only translation can omit postprocessing but cannot qualify processed output.
- `--jobs`: bound independent translation workers. Start with serial suites and
  projects; use declared locks for shared generated directories, ports, build
  outputs, and tool builds. Do not run the seven projects concurrently by default.
- `--dry-run`: resolve and display dependencies, source groups, commands, output
  paths, and missing prerequisites without fetching, staging, or running hooks.
  Input-derived commands may remain explicitly unresolved until staging.

Define a small default test selection and a separate explicit verification
selection for every recipe. `all` means all declared applicable matrix entries,
not an assertion that every upstream feature or platform is qualified. A missing
prerequisite for a required suite fails verification; unsupported or unselected
entries remain visible in the summary.

All standard Bash helpers eventually have the same semantics as the runner's
commands. Preserve existing defaults temporarily while migrating each project,
then document intentional changes. In particular, SQLite's current composite
`build.sh` workflow needs an explicitly named compatibility helper before
`build.sh` adopts the shared build-only contract. picotls' `build-only.sh` can
remain an alias of its new standard `build.sh`. Legacy
`--raw`, `--all`, `--aot`, `--no-fetch`, and `--no-build-tools` are translated at
the wrapper boundary. Document intentional behavior changes separately.
The relaxed hash policy is an intentional change: wrappers and retained helpers
must honor it rather than silently restoring their old strict hash gates.
Canonical folder conventions and standardized helper semantics likewise take
precedence over preserving old project-specific paths and command meanings.

MsQuic's fast/qualified paths become explicit task selections with distinct
scope. Blink's native oracle and MsQuic's ABI/host gates remain prerequisites
where currently required; consolidation must not quietly remove them.
libsmb2 verification prepares both profiles before running suites; its legacy
wrapper can still request that composite sequence.

**Relaxed hash policy**

Treat hashes as optional provenance and cache information, not general
compatibility requirements. An upstream edit, added file, rebuilt compiler,
changed host source, edited generated C#, or missing old translation receipt
must not by itself prevent a build or a fresh test run in `warn`/`off` mode.
Do not require a separate local-source flag just to work with changed inputs.

| Check | Default behavior |
| --- | --- |
| Available downloaded/cached archive checksum | Warn on mismatch and continue if the archive is readable and structurally valid; skip in `off`. |
| Source, header, tool, configuration, or generated-output hash drift | Limit checks to declared relevant files; warn and continue. Do not scan entire trees by default. |
| Missing historical hashes or provenance receipt | Report unavailable provenance; build/test the current artifacts. No need to regenerate solely to obtain a receipt. |
| Exact text replacement or patch | Require its target/context to match exactly and with the expected occurrence count. Fail that adaptation if it cannot be applied unambiguously. |
| Missing required files, invalid manifest paths, incomplete emission, compiler failure, ABI/behavioral assertion failure | Fail as actual task errors, independently of hash mode. |
| Cache identity mismatch or unavailable cache evidence | Treat as a cache miss and regenerate, not as a campaign failure. |

For replacements, prefer a guard on the exact replaced text and necessary local
context. An unrelated edit elsewhere in the file should not invalidate the
adaptation. Retain a strict whole-file before/after digest only when the
particular replacement demonstrably relies on those exact bytes; declare that
requirement next to the replacement. No-fuzz patch application and exact match
counts remain mandatory. Typed semantic overrides use their existing declaration
and signature checks without acquiring historical file-hash requirements.

Warnings belong in the console summary and receipt but do not change a passing
exit status or require confirmation. Receipts distinguish requested source
revision from observed inputs and record `hash_policy`, warnings, and unchecked
provenance. Record paths, run IDs, commands, and fresh test outcomes even with
hashing off; leave unavailable digests absent rather than implying verification.
Do not reuse historical test success as proof for changed/unchecked artifacts;
run the selected tests again. New passing results remain valid for the current
run without requiring equality to historical hashes.

**Shared lifecycle and evidence**

Use a small dependency graph with named tasks and declared artifact inputs.
Validate unknown dependencies and cycles before execution. A task runs once per
resolved configuration within an invocation. No distributed scheduler, daemon,
or general-purpose build-system DSL is needed.

```text
available sources + structural/replacement checks + tool snapshot
    -> isolated source preparation
    -> direct emission OR ordered per-unit objects -> link
    -> raw project -> private processed copy -> build checks -> promotion

library artifacts + native/test/peer artifacts
    -> selected suites and audits -> verification receipt
```

Project gates can add dependencies at the appropriate stage. In particular,
MsQuic TLS/peer suites explicitly depend on matching picotls artifacts. `test`
checks those existing artifacts; `verify` can prepare them once in its graph.
Blink's SQLite/picotls/MsQuic regression campaigns remain opt-in suites with
separate preparation and evidence, not dependencies of ordinary translation.

Implement these shared guarantees:

1. **Inputs.** Download to a temporary file, apply the selected checksum policy,
   extract safely to a private directory, then expose the usable tree. Support ZIP
   and TAR, nested dependencies such as picotest, file modes, and declared
   archive layouts. Reject unreadable/unsafe archives without repairing existing
   trees; a checksum difference alone follows the hash policy.
   Keep pristine references separate from staged patches and generated headers.
   Keep Valkey's complete tree verification available only in explicit strict mode.
2. **Local sources.** Accept edited or manually supplied existing trees by
   default, subject to required-file and structural/adaptation checks. Record
   observed provenance according to the hash policy without claiming equality
   to the requested pin. Preserve Blink's current workflow through its wrapper;
   map MsQuic's manual-source option deliberately. Source revision
   selection/update remains a separate maintainer operation, initially using
   the existing MsQuic selector.
3. **Processes.** One runner records argv, cwd, declared environment overrides,
   duration, exit/timeout/cancellation status, and log paths. Log hashes are
   optional provenance, not required gates. Preserve separate
   stdout/stderr when stdout is an oracle transcript. Terminate the entire owned
   process tree, including compiler/AOT children, on timeout or interruption.
   Persist failure evidence even when process startup fails.
4. **Identity.** Snapshot compiler/postprocessor dependencies, not only the main
   DLL. Record SDK/runtime/native compiler identity and effective configuration,
   ordered units/includes/flags, and paths to relevant inputs. Apply limited
   advisory hashes to declared files in `warn` mode; reserve full input manifests
   for explicit strict runs or cache construction. Detected input changes warn
   by default. Use private snapshots and locks for consistency instead of broad
   historical hash gates. Keep project-specific provenance as attachments.
5. **Emission.** Support both existing strategies without forcing SQLite/Pinta/
   picotls into per-unit compilation. Parallel objects retain manifest link order,
   have collision-free names, and require the complete selected closure before
   linking. Preserve per-unit override reports and project validators.
6. **Postprocessing.** Validate the emitted file manifest, preserve raw output,
   and process a private copy with the necessary semantic host context. Deliver
   transformed generated sources with original authored sources. Never rewrite
   authored host code as a side effect. Retain project-specific project assembly.
7. **Promotion.** Stage on the output filesystem, lock publication, validate all
   requested forms, and replace only owned outputs. Preserve unknown/user files
   or fail with a clear conflict. Roll back the complete raw/processed set if any
   rename or final-path build fails. Multi-directory replacement is a transaction
   under a lock, not one atomic filesystem operation. Readers must pin an artifact
   set or hold the same lock. Keep a recovery journal for interrupted promotion.
8. **Receipts.** Store immutable attempts under
   `<project>/artifacts/campaign/<run-id>/`, with private work directories and
   atomic receipt writes. Distinguish `latest-attempt` from `current-product`.
   A failed attempt leaves the last good product identifiable, but cannot make
   it appear to be the output of that attempt. Tests record the current artifacts,
   profile, form, runtime, and dependencies they exercised. Link a translation
   receipt when available; missing or differing hash evidence is advisory by
   default. Fresh test results are attached to this run, not an assumed old build.

Use a versioned receipt envelope: project/action/profile, requested and resolved
selection, status/scope, source provenance, tool identities, task results,
optional input/output hashes, prerequisite receipt references, and attachments.
Include the hash policy and provenance warnings. Task status
must distinguish passed, failed, timed out, cancelled, skipped, and blocked.
Produce a concise console summary and JSON initially; add CI report adapters
only where needed. Qualification success requires every selected required gate.

Output normalization belongs to a named project validator recorded in the receipt.
For example, picotls excludes an incidental TLS assertion count from comparisons;
the framework should not silently normalize arbitrary output. Retain exact raw
logs alongside normalized comparison results.

**Caching and path stability**

First share lifecycle code without adding a new persistent cache. Preserve
Blink's existing cache through an adapter. Add general object reuse only after
the execution and identity contracts are validated across multiple recipes.

Cache keys must cover source/header content, generated fragments, include order,
defines, compiler dependency snapshot, overrides, preparation logic, emission
options, and relevant environment/toolchain inputs. Hash outputs on cache reads
only when using a content-validated cache. A mismatch causes a rebuild. With
`--hashes off`, bypass persistent content-based caches, including Blink's adapter,
instead of requiring hashes or trusting unchecked objects. Cache fingerprints
identify current inputs; they do not enforce historical upstream pins.
Use conservative header closures until dependency completeness is demonstrated.

Blink explicitly documents physical paths affecting anonymous-type identities,
static symbol names, and `__FILE__`. Preserve its canonical shared header tree
and per-source paths. Do not relocate sources into arbitrary run directories and
assume byte-identical emission or cross-checkout cache reuse. Record path inputs;
path-independent caching would require separate compiler support and validation.

**Migration sequence and completion gates**

1. **Inventory and contracts.** Record the current command/default/environment
   mapping, profiles, emitted project paths, source policies, test matrices,
   external prerequisites, and receipt consumers for all seven projects. Capture
   representative baseline commands and outputs with fixed tools/inputs. Define
   the recipe/receipt API and wrapper compatibility table before moving behavior.
2. **Execution core.** Implement shared Bash dispatch/Python resolution, the
   layout model/scaffolder/checker, process execution, identities, receipts,
   locks, and task planning. Extract the strongest existing implementations from
   Valkey, Blink, and picotls rather than copying a whole campaign. Add contract
   tests for interpreter fallback, argument forwarding, layout validation,
   cancellation, startup failure, stale artifacts, and task ordering.
3. **First vertical slice: picotls.** Migrate acquisition, direct translation,
   raw/processed handling, build, and the existing test campaign. Keep peer
   harnesses as specialized tasks. Gate: supported raw/processed JIT/AOT and
   peer comparisons retain their previous results, with provenance differences
   handled by the shared relaxed policy. Move its solution to the root and
   normalize consumer/raw paths as the first layout migration.
4. **Object pipeline: libsmb2, then Valkey.** Add shared ordered object emission,
   link, tool snapshots, staged adaptation helpers, and transactional promotion.
   Migrate async/legacy profiles, then Valkey's per-unit flags, generators,
   notices, host links, probes, and managed validation. Gate: no lost profile
   separation, required host binding, rollback guarantee, or test coverage.
5. **SQLite and Pinta.** Reuse direct emission with source-preparation hooks.
   Register SQLite native/translated corpora and generated layout tests as named
   artifacts; register Pinta patches, profiles, fixtures, and test matrices.
   Gate: native comparisons and owning-consumer results remain equivalent;
   SQLite's existing Windows host-VFS workflow retains its coverage.
6. **MsQuic.** Unify fast and qualified execution infrastructure, retain ABI and
   host-contract gates, closure archival, dynamic source selection, and explicit
   picotls dependencies. Gate: a fast run cannot satisfy qualified prerequisites;
   historical closure/peer evidence is not reused for changed or unchecked
   artifacts, but fresh verification can proceed; source-selection tests pass.
7. **Blink.** Adopt shared execution/delivery/test orchestration while retaining
   profile derivation, semantic/boundary validators, canonical emission paths,
   guest construction, and specialist audits as recipe code. Gate: existing
   clean-delivery/reproduction checks and selected guest/service regressions pass
   with correct cache invalidation and unchanged authored host sources.
8. **Remove duplicates and switch CI.** Provide the complete standard Bash helper
   set and root `ManagedConsumer.slnx` for all seven projects. Normalize public
   generated/profile paths, convert old entrypoints to aliases where useful,
   update README commands and affected workflows, migrate internal receipt
   consumers, and delete replaced helpers. Compatibility receipts may bridge a
   migration but must be projections of one canonical result. Remove those
   projections once consumers migrate. Run the declared verification selection
   for each project; attach platform/suite omissions explicitly.

Each migration is independently reviewable and reversible. Compare effective
compiler arguments, selected sources, generated project contracts, and behavioral
results. Require byte equality only where paths and nondeterministic metadata are
controlled; document any comparison normalization. Existing tests remain the
  qualification authority while the orchestration changes.

Framework tests should cover all three hash modes: ordinary source/tool/output
drift and missing receipts continue in `warn`/`off`, warnings preserve a passing
exit status, and explicit strict mode rejects missing required evidence or
mismatches. Test that unrelated edits do not break locally guarded replacements,
while missing/ambiguous replacement matches fail in every mode. Also cover
unreadable/missing inputs with fetching disabled, cache misses and cache bypass,
incomplete object sets, failed postprocessing, concurrent publication, partial
promotion and recovery, process-tree timeouts, unsupported matrices, skipped
required gates, and fresh tests after cross-project provenance drift. Use tiny
fixture programs and fake tools for these tests; use the existing project suites
for real translation behavior. Exercise path handling and process cleanup on
Windows as well as Linux.

Test Bash dispatch with only `python3`, only Python 3 named `python`, a broken
`python3` plus a working `python`, and no usable Python 3. Check argument/exit-code
preservation and invocation from unrelated working directories and paths with
spaces. Test layout scaffolding on fresh and existing trees, preservation of user
solution entries, relative project references, raw/profile isolation, and
rejection of noncanonical delivery paths. For each migrated project, translate
then build its root `ManagedConsumer.slnx` and run its declared consumer check.

Completion means all seven projects have the standard Bash helpers, root consumer
solution, and canonical generated project paths, using one CLI and lifecycle
implementation. Changes to old wrapper behavior are explicitly documented. Fetching remains
optional, and relaxed hash handling works consistently through wrappers and
specialist helpers. Verification reports what was actually built/tested and what
provenance was unchecked, without failing merely because hashes changed.
Project code should contain
source/host/test knowledge; it should no longer maintain another implementation
of the common runner, snapshotter, publisher, or receipt protocol.
