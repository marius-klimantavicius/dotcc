# Baseline for translating an upstream project

Baseline version: 1 (2026-09-26).

Use this baseline for a new DotCC translation campaign. Copy the
[project template](translation-template.md) to `<project>/docs/PLAN.md`, then fill
in its decisions and acceptance gates. Keep common methodology here and detailed
source, host, API, and test decisions in the project plan. Small projects may keep
those details in one document; split them out when they become difficult to review.

For example, from the repository root:

```bash
mkdir -p examplelib/docs
cp docs/plans/translation-template.md examplelib/docs/PLAN.md
```

Replace the template's `{{...}}` placeholders and resolve its decision rows before
treating it as the project's implementation plan. Keep the baseline version in
the plan so later shared guidance changes do not silently change accepted scope.

This is a planning baseline, not a statement that a particular translation works.
It does not transfer completion claims, branch choices, commit/push permissions,
or delegation instructions from an older campaign. Current user instructions
control the work. Record project-specific departures and their reasons explicitly;
an unresolved decision blocks only the work that depends on it.

## 1. Define a useful product and its boundaries

Describe one concrete application workflow that the translated library must
perform. Identify which upstream algorithms and state machines stay translated,
which execution services belong to the host, and what the consumer owns. A native
wrapper, handwritten replacement engine, parse success, or successful C# build
does not establish that the translation performs the workflow.

Make the following decisions before promising the corresponding functionality:

| Decision | Required project-specific answer |
| --- | --- |
| Product scope | Required APIs/features, explicitly excluded features, and a first useful workload. |
| Inputs | Release/commit selection, product and test dependencies, source/build manifests, generated upstream inputs, licenses/notices. |
| Configuration | Source/header closure, ordered includes/defines, C dialect/extensions, upstream feature selection and separate diagnostic profiles. |
| ABI and encodings | C data model, widths/alignment, endianness, text encodings, callback convention, and any wire/file/guest ABI distinct from the host. |
| Host services | Allocation, time, entropy, files, sockets, scheduling, synchronization, crypto/provider operations actually required by the selected sources. |
| Embedding | Static versus per-instance state, concurrency/reentrancy, initialization, readiness, shutdown, callbacks and resource ownership. |
| Dependencies | Existing translated libraries to reuse; permitted runtime/BCL dependencies; any explicitly allowed platform imports; separate native oracles. |
| Delivery | Generated project/class/namespace, default and alternate profiles, owning API, ordinary consumer references. |
| Qualification | Required platforms/architectures, raw/processed forms, JIT/NativeAOT, feature/role axes, test prerequisites and comparison rules. |

Start with the repository's supported toolchain and shared runtime. Verify
capabilities against the selected inputs; support documentation and another
project's successful build are starting information, not feasibility evidence.
Avoid build-system autodetection silently expanding the product's dependencies.
Use generated constants/enums in the host and sample where available; document
the origin and ABI of any necessary fallback values.

## 2. Use the framework and canonical layout

Follow the [campaign framework guide](../campaigns.md) and its
[adding-a-project example](../campaigns.md#adding-a-project). The current API is
defined in [model.py](../../Scripts/campaigns/model.py). A root-level
`<project>/scripts/campaign.py` opts into discovery; no central project registry
or new project-name branch in the execution engine is needed.

```text
<project>/
  README.md
  ManagedConsumer.slnx
  docs/PLAN.md
  config/                              # source metadata and translation policy
  scripts/campaign.py                  # recipe, hooks and suite declarations
  scripts/{common,fetch,translate,build,test,verify,probe}.sh
  src/                                 # authored adapters, host and owning API
  samples/ManagedConsumer/ManagedConsumer.csproj
  tests/                               # fixtures and specialist harnesses
  ref/                                 # acquired upstream/test sources; ignored
  generated/TranslatedName/TranslatedName.csproj
  generated/TranslatedName.Raw/TranslatedName.csproj
  generated/profiles/<profile>/TranslatedName[.Raw]/
  build/campaign/<run-id>/              # private work; ignored
  artifacts/campaign/<run-id>/          # logs and receipts; ignored
```

The root solution includes the processed default product, original authored
projects, and sample. Generated projects reference authored files/projects at
their original `src/` locations. Rebuilding must pick up authored edits without
regeneration; translation and postprocessing must preserve those files. Keep
test entrypoints and fixed workloads outside the delivered library.

Supply a recipe, metadata and an authored consumer, then run
`bash Scripts/campaign.sh layout <project> --write` to create missing scaffolding.
It checks for the consumer rather than writing application code. Use shared Bash
dispatch, including its validated `python3` then `python` fallback, argument/exit
handling, and paths resolved from script locations. Test invocation from an
unrelated working directory. Do not add another Python resolver or runner.

The project supplies source selection, preparation, compiler/linker flags, host
bindings and behavioral checks. Reuse the framework for fetching, tool execution,
logging, raw/processed staging, managed builds and publication. Declare consumer
settings, suites and dependencies in the recipe; reuse dependency projects rather
than copying generated libraries. Any additional framework capability should be
generic and demonstrated by a concrete project need.

## 3. Keep provenance separate from compatibility

Select a reproducible upstream revision and record URLs, revisions, archive
layouts and optional checksums in JSON metadata under `config/`. Put strict
adaptation fingerprints there too. Python and Bash read metadata; they contain
no embedded checksum or revision literals. Record product, test/oracle, peer and
toolchain inputs separately. Source upgrades are deliberate metadata changes.

Preserve acquired references during preparation. Derive generated headers and
any reviewed adaptations in staged copies. Existing usable local source trees,
including edited trees supplied by the user, are accepted by the ordinary
workflow; record their observed provenance without claiming equality to a pin.

| Policy | Baseline behavior |
| --- | --- |
| `--hashes warn` | Default. Limited available provenance checks are advisory; missing historical evidence or differing hashes alone do not fail the run. |
| `--hashes off` | Disable common provenance checks. Content-dependent caches must rebuild or bypass reuse rather than trust unchecked objects. |
| `--hashes strict` | Explicit reproducibility mode requiring the configured integrity evidence. It is not a default completion requirement. |
| Exact text adaptations | Require unique local anchors, patch context or narrowly scoped fingerprints in every mode. Missing/ambiguous matches fail; unrelated edits should not require new whole-file pins. |
| Typed overrides | Validate intended symbol/type matches and behavioral contracts; avoid historical text-hash allowlists. |

Missing required files, unsafe extraction, incomplete source sets, compiler
errors, invalid host bindings and failing behavior remain failures in every mode.
Never use a historical successful receipt as a substitute for testing changed
artifacts. Fresh results remain useful even with provenance hashing disabled.

Use `--fetch missing` for optional acquisition and `--fetch never` to reuse local
trees/cached archives without downloading upstream inputs. Neither option controls
NuGet restore; document `--restore` and any specialist prerequisites separately.
Do not require blanket re-downloads, exhaustive hashes or strict-mode runs to
make ordinary progress.

## 4. Prove the difficult boundaries early

Establish an executable native reference with the same effective feature profile
and a matching ABI. Check the oracle's assertions, exit status and case inventory
before using its output as expected behavior. Distinguish native platform/harness
defects and upstream undefined behavior from translation failures; an oracle
crash is a blocker to classify, not an expected result to imitate.

Probe the project's highest-risk boundaries using actual translated code:

- Compare native observations to actual emitted sizes, alignment, offsets and
  storage. Handwritten mirror structs and metadata alone are insufficient.
- Exercise callback calling conventions, canonical function-pointer identity,
  rooted contexts, retained buffers and allocator/free pairing. No borrowed or
  movable address may outlive its valid storage. Contain exceptions at C boundaries.
- Prove required platform/provider capabilities on the intended execution target.
  Keep host services distinct from upstream algorithms and make unsupported
  operations return their documented failures.
- If multiple instances are required, audit globals, function statics, TLS and
  shared runtime state. Define owner propagation, concurrent entry and callback
  drain. A global lock or hidden subprocess does not prove in-process independence.
- Define shutdown and cancellation at the real execution boundary. An external
  flag alone does not establish interruption of a blocking translated operation.

Choose only relevant probes. A protocol library may need independent peers and
crypto vectors; an interpreter may need arena relocation and bytecode fixtures;
a persistent engine may need native/managed file interchange. Do not turn every
older project's specialized test into a prerequisite for every new translation.

Prefer upstream configuration and existing extension points, then typed host
bindings. Any necessary text adaptation needs a documented reason, local guards,
and appropriate native/translated checks. Keep upstream bug repairs separately
identified. Do not edit emitted C# or remove required algorithms to conceal a
compiler defect. Unsafe translation alone makes no isolation/security guarantee;
state only the embedding properties actually selected and tested.

## 5. Repair observed failures and retry real inputs

For each substantive preprocessing, parser, lowering, linking, C# build or runtime
failure:

1. Preserve the command, stage, profile, diagnostic, tool identity and relevant
   inputs/logs. Classify it as compiler, runtime, host, harness, upstream or scope.
2. Reduce compiler/runtime defects to meaningful minimal cases; establish native
   expected behavior where applicable and demonstrate a failing regression.
3. Fix the shared structural cause. Use `DotCC.Tests` for focused checks and
   `DotCC.FunctionalTests/Fixtures` for emitted compilation/execution. Rebuild
   generated parser tables after grammar changes. String checks alone do not
   prove runtime semantics.
4. Run focused and affected tests, then retry the full selected upstream source
   set. Add project integration coverage for semantic defects. Record the new
   first failure or passed stage, keeping unrelated baseline failures separate.

Use a blocker ledger with ID, classification, reproducer, expected/observed
behavior, regression, fix and full-source retry. Suspected gaps stay hypotheses
until reproduced. Shared changes trigger relevant downstream regressions; choose
those campaigns by affected behavior, rather than hardcoding all older projects
into every new plan. Coordinate builds that share output directories.

## 6. Milestones and exit gates

Adapt these phases into the project plan. Split P3/P4 into feature milestones
when needed, with explicit dependencies. API design can begin early; its runtime
acceptance requires the real translated implementation.

| Phase | Work and exit gate |
| --- | --- |
| P0 — Scope and baseline | Source/configuration inventory, recipe/layout, repository baseline, working native control and first real translation attempt. Gate: repeatable commands and concrete findings. |
| P1 — Boundary feasibility | Resolve the highest-risk ABI, host/provider and ownership questions. Gate: actual emitted/native probes and a decision for each required boundary. |
| P2 — Complete translation | Emit every selected unit, preserve linkage/initialization, build raw and semantically processed libraries, audit unresolved imports. Gate: complete products at canonical paths, with no manual repair or runtime success stubs. |
| P3 — Host and first useful execution | Implement required host contracts and run the chosen workflow through the actual translated core. Gate: correct results and initialization/cleanup in the selected execution modes. |
| P4 — Behavioral qualification | Cover required features, ordinary errors/lifetimes, applicable upstream cases and selected interoperability. Gate: passing required case/matrix rows with explicit comparison rules and omissions. |
| P5 — Consumer delivery | Deliver the owning API and separate sample through ordinary project references. Gate: root solution builds and a meaningful workflow runs using the delivered artifacts, including required AOT execution. |
| P6 — Reproduction and acceptance | Regenerate with final tools, rerun required suites and affected shared regressions, audit dependencies and document usage/limits. Gate: current evidence meets the complete selected scope. |

The normal `translate.sh` runs the whole source-to-product pipeline: prepare,
emit/link, build raw, postprocess a private copy, build processed, and publish the
validated artifacts. Use the shared transactional publisher and preserve the last
valid output on failure. Raw builds must work before optimization. `build.sh`
builds existing output; `test.sh` runs selected suites; `verify.sh` translates and
runs the recipe's declared verification suites. It establishes only their actual
coverage. A fast/probe/smoke run cannot satisfy undeclared qualification gates.

## 7. Validation and completion evidence

Use raw/processed × JIT/NativeAOT on the first actual target as the starting
matrix; record intentional departures and reasons in the project plan. Add
platforms, profiles, roles, encodings, peers or feature combinations only where
needed by the promised product. Cross-compiling does not prove execution on the
target. Whole-library rooting can expose hidden AOT compilation failures, while
a normally trimmed separate consumer proves a different delivery contract.

Map each required behavior to a suite and an assertion. Preserve case-level
pass/fail/skip counts for upstream tests, with reasons for exclusions. Native
oracles and independent peers are distinct evidence sources. Compare deterministic
bytes/status/layouts exactly; define explicit masks/invariants for nondeterminism,
undefined values and equivalent logical state. Preserve original transcripts.
Normalization must not hide lost data, wrong ownership or invalid outcomes.

Choose custom fault injection, fuzzing, malformed-input campaigns and extended
stress explicitly in the project plan; they are not inherited automatically.
Ordinary API errors, required rejection behavior, lifecycle checks and applicable
upstream cases follow the selected product contract. Honor explicit user test
exclusions. Record excluded and unrun tests accurately rather than calling them
passes. Performance measurements need equivalent workloads and settings; add
acceptance thresholds only when justified by a stated requirement.

Store current commands, selection, tool/runtime identity, counts, logs and
receipt paths, plus optional hashes. Link specialist reports from the framework
receipt rather than recreating its evidence format. A published build may still
fail later qualification; distinguish the current product from the latest
attempt. Record planned, passed, failed, blocked, unrun and excluded work clearly.

Completion requires the useful workflow, all required features/gates, the
canonical consumer, reproducible commands and current evidence for the selected
targets. Required skipped or blocked checks leave acceptance incomplete. A
smaller validated subset can be reported precisely without closing the whole
plan. Keep future features separate; never import historical completion claims.

## Lessons behind the baseline

These plans informed the baseline; their product requirements and historical
permissions do not automatically apply to a new project.

| Plan | Reusable lesson |
| --- | --- |
| [SQLite](../../sqlite/docs/PLAN.md) | Failure-driven compiler repairs, full-input retries, real storage/callback checks and persistence interoperability. |
| [picotls](../../picotls/docs/PLAN.md) | Provider capability probes, ownership and authentication contracts, independent peers and negative outcomes. |
| [MsQuic](../../msquic/docs/PLAN.md) | Reuse translated dependencies, prove host services separately, preserve incomplete recovery evidence. |
| [Blink](../../blink/docs/PLAN.md) | Distinguish host/guest ABIs, isolate instance state, preserve authored sources and explicit test-scope exclusions. |
| [Valkey](../../valkey/docs/PLAN.md) | Audit global state and embedded dependencies, prove shutdown/persistence, use shared Bash/input conventions. |
| [libsmb2](../../libsmb2/docs/PLAN.md) | Complete default translation, canonical solution/output paths, asynchronous ownership and real consumer delivery. |
| [Pinta](../../pinta/docs/PLAN.md) | Validate the native oracle itself, text widths, arena/GC ownership and actual fixture coverage. |

The [framework guide](../campaigns.md) governs current runner usage. Older plans'
duplicated runners, embedded pins, mandatory blanket hashes and old folder
layouts are not the baseline for new projects.
