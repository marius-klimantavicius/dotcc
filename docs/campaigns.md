# Translation campaigns

SQLite, picotls, MsQuic, Blink, Valkey, libsmb2, Pinta, and QuickJS share the runner in
`Scripts/campaigns`. Their `scripts/campaign.py` recipes supply source selection,
adaptations, compiler options, host bindings, and named test suites. Existing
specialist harnesses retain their behavioral assertions and detailed reports.

Projects are discovered from root-level `<project>/scripts/campaign.py` files.
There is no project registry in the framework. Discovery is alphabetical, and
recipes declare cross-project dependencies, consumer settings, supported options,
and test adapters themselves.

For a new translation, use the [shared planning baseline](plans/translation-baseline.md)
and copy its [project plan template](plans/translation-template.md) to
`<project>/docs/PLAN.md`. The template records product decisions and acceptance
gates; the runner integration is described in [Adding a project](#adding-a-project).

The [CPython campaign](../cpython/docs/PLAN.md) is authorized for implementation.
Its discoverable recipe and exact-pin native controls now include required
Unicode-name parsing. The full 193-unit manifest includes the authored embedding
boundary. A complete retry now emits/links, builds and publishes raw and
processed libraries. Shared initializer repairs allow the actual raw JIT owning
consumer to pass two cycles; remaining ABI/matrix/rooted-AOT qualification stays
open. Final compiler/runtime regeneration is pending. No completed CPython milestone is claimed. See its
[implementation ledger](../cpython/docs/IMPLEMENTATION.md) for current evidence.

Shared campaign `link_flags` defaults to 256 KB source splitting
(`--split=size --split-size=262144`), `--literal-pool`, and
`--deduplicate-inline`. Recipes inherit these settings and document any explicit
exception. Standalone DotCC CLI defaults are unchanged.

Use Bash helpers from any directory. They resolve Python 3 as `python3`, then
`python`, checking the interpreter before dispatch. Python 3.11+ and .NET 10
are required; native/AOT/network suites have their project-specific prerequisites.
Python entry points can also use `python Scripts/campaign.py` directly. Script
staging and validation must work on Windows without administrator privileges or
Developer Mode: use ordinary copies, and report unavailable optional filesystem
link tests explicitly. Native/container prerequisites remain project-specific.

```bash
bash Scripts/campaign.sh list all --suites
bash Scripts/campaign.sh layout all --check
bash picotls/scripts/translate.sh
bash picotls/scripts/test.sh --form all --mode all
bash picotls/scripts/verify.sh --tools reuse --fetch never
bash valkey/scripts/probe.sh --unit src/server.c --hashes off
bash libsmb2/scripts/translate.sh --profile legacy
bash Scripts/campaign.sh verify all --dry-run
```

Every project has `fetch.sh`, `translate.sh`, `build.sh`, `test.sh`, `verify.sh`,
and `probe.sh`. The repository helper takes the action followed by the project
name (or `all`). Projects and suites run serially; `--jobs 1..16` bounds object
emission within a project (default 4). CPython honors every supported explicit
request without a hidden two-worker clamp. Native builds retain two jobs and
managed builds retain one MSBuild node. Choose the emission budget for available
CPU and memory; increasing it does not increase those build budgets.

| Action | Contract |
| --- | --- |
| `fetch` | Acquire declared source groups; preserve existing usable source trees. |
| `translate` | Prepare sources/tools, emit, preserve raw output, postprocess a private copy, build, and publish. |
| `build` | Build existing selected libraries and the default root consumer solution. |
| `test` | Run named suites against existing product libraries. Native/test harnesses may build their own executables. |
| `verify` | Translate and run the recipe's verification suites; prepare declared project/profile prerequisites. |
| `probe` | Emit diagnostic objects without publishing a product. `--unit` selects units; Pinta/Valkey also support `--pristine`. |
| `layout` | Check canonical paths, helpers, solutions and authored project references; `--write` creates missing scaffolding. |

`test` defaults to the consumer smoke suite. Use `--suite qualification` for
picotls or libsmb2's established comprehensive harness, or `verify` for the
recipe's complete declared selection. `--suite` can be repeated. `--with
repository` adds the compiler, functional and postprocessor test projects once
after verification. A dry run prints tasks, sources, suite scope and paths;
compiler arguments derived from staged inputs are resolved during execution.

Matrix-aware suites honor `--form raw|processed|all`, `--mode jit|aot|all`, and
`--rid`. Retained specialist suites may have a fixed matrix; their detailed
reports remain authoritative. MsQuic's normal translation always includes both
forms, host/public ABI gates, and rooted JIT/AOT boundary consumers. `--fast`
omits qualification; it does not claim that those gates passed. The simple
Blink/libsmb2 consumer checks run their help command, and MsQuic's generates
sample certificates; network behavior belongs to the specialist suites.

## Inputs and hashes

Keep checksum and revision literals in JSON metadata, never in Python or Bash
helpers. Recipes read their project's source manifests. SQLite's archive pins
are in `config/sources.json` and strict adaptation fingerprints in
`config/host-source.json`. Blink's source revision comes from
`config/source-manifest.json`; historical fingerprints used by individual staging
and evidence helpers live in `config/script-inputs.json`, keyed by helper path.
These metadata entries preserve the scope of the original checks.

`--hashes warn` is the default, including verification. Available archive and
product provenance differences are warnings. An edited usable source tree does
not need its old archive or a historical success receipt. `--hashes off` disables
the common provenance hashes/checks and Blink cache reuse. Specialist harnesses
can still calculate diagnostic fingerprints and validate their own cache entries.
`--hashes strict` requests historical integrity checks, including source manifests
or comparisons against cached archives. It is an explicit reproducibility mode.

A source can declare `archive_paths` to acquire only needed archive members.
Selectors are relative to the source's declared archive root: `include/` selects
that subtree and `LICENSE` selects exactly that file. The default empty tuple
extracts the complete archive. Selectors cannot contain traversal, glob syntax,
backslashes, or absolute paths. Every member name is checked for traversal and
the declared root, including unselected members. Unselected links and special
entries are skipped; selected links, special entries, and duplicate files still
fail. Required-file checks apply to the resulting tree, and strict comparison
extracts the same selection. This supports a header-only reference from an
upstream archive with unrelated links without weakening archive safety.

Exact source replacements and patch hunks always require unique matching local
context. Unrelated edits elsewhere in a file are accepted. Missing inputs,
unsafe archives, incomplete generated manifests, compiler errors, host binding
violations, and behavioral failures remain errors in every mode.

`--fetch missing|never` controls upstream acquisition. Translate/verify/fetch
default to `missing`; build/test/probe default to `never`. Existing archives can
be extracted with `never`. Fetch all groups explicitly, or use `--group product`
and `--group tests`. `--no-fetch` and `--offline` mean `--fetch never`; they do
not disable NuGet restore. `--restore normal|locked|none` separately controls
shared managed commands. Retained specialist scripts that invoke dotnet directly
still use their own restore behavior.

`--tools build|reuse` defaults to `build` for translate/verify and `reuse`
otherwise. The shared runner snapshots compiler/postprocessor DLLs and runtime
configuration for translation. Existing specialist test emitters retain their
own tool prerequisites. A build-only or consumer test command does not rebuild
the compiler merely to inspect an existing library.

`DOTCC_COMPILER` and `DOTCC_POSTPROCESSOR` select explicit existing tool paths.
The runner snapshots each selected closure and records its actual bytes. A
missing selected tool fails instead of falling back to root output.

The six production regression campaigns have legacy command adapters in
`Scripts/production_support.py` and `production-common.sh`. These cover individual
specialist builds, not entire verification or peer sessions. Linux uses
nonblocking shared/native `flock --close`; busy locks fail without starting the
command. Native make/CMake is bounded to two jobs. CPython keeps its existing
specialist lock protocol. Pure controls run with:

```bash
python3 -B -m unittest discover -s Scripts/production_tests -v
```

The live shared runner still copies top-level DLL/JSON tool files. Explicit
`DOTCC_CAMPAIGN_COMMAND_LOCKS=production` enables the shared command adapter for
individual build/emission/postprocessor commands. Linux uses nonblocking
`flock --close`. Validated immutable-tool object emissions to distinct private
run outputs use shared leases on the same global lock, plus an exclusive lease
for each object path. The classifier parses the actual compiler/source/options/
output/report paths; unsupported commands retain exclusive leases. A local
reader/writer scheduler lets eligible siblings overlap and waits for them before
an exclusive sibling command, without holding an idle OS lease. External
contention and duplicate output writers still fail immediately. Receipts retain
requested/effective argv, lock mode, start/end times and the actual process PID.
Runtime and
specialist orchestration commands have no outer build lock. CPython's native
preparation uses a separate cache-state lock under this opt-in, so individual
native commands acquire shared/native locks exactly once. The ordinary campaign
run lock and transactional publisher remain in use. Publication mutations take
an exclusive global lease and release it before final build validation, whose
commands acquire their own leases. Worker failure cancels and drains owned
process trees before a link can start. Windows adds no Linux flock.

A18's recursive tool-copy candidate remains private. The command hook and
private-lock controls do not establish complete production lock
coverage or qualify a production campaign. See the
[scoped A23 handoff](../cpython/artifacts/owner-a-r23-20261018/a23-handoff.json).

For early CPython blocker discovery, select `--form raw --jobs 4` on the ordinary
translation route, then run the actual raw JIT suite. This omits processed work
for that discovery run only; final raw/processed JIT/AOT acceptance remains
required. `--tools reuse` reuses tools and still emits every selected C unit.
Semantic-context self-builds and final canonical builds remain enabled: evaluated
reference/resource/generator inputs and moved-project PDB identity require proof
before replacing those checks with reference preparation or output reuse.

## Layout and evidence

```text
<project>/
  ManagedConsumer.slnx
  config/                         # project policy and source selection
  scripts/campaign.py              # recipe and project hooks
  samples/ManagedConsumer/
  src/
  tests/
  ref/                            # acquired upstream source
  generated/TranslatedName/TranslatedName.csproj
  generated/TranslatedName.Raw/TranslatedName.csproj
  generated/profiles/<profile>/TranslatedName[.Raw]/
  build/campaign/<run-id>/         # private inputs, tools, objects, prior outputs
  artifacts/campaign/<run-id>/     # logs and receipt.json
  artifacts/campaign/current-<profile>.json
  artifacts/campaign/latest-attempt.json
```

The default profiles are SQLite/picotls/MsQuic `default`, Blink `threaded`,
Valkey `managed`, libsmb2 `async`, and Pinta `release`. Nondefault profiles are
Blink `single-thread`, libsmb2 `legacy`, and Pinta `debug`. libsmb2 verification
prepares its legacy profile separately before its async qualification harness.

Publication holds a project lock, validates final paths, and rolls back both
forms and their current-product pointer on failure. A journal recovers interrupted
promotion. Files outside the prior generated-file ownership manifest are
preserved; conflicting new files fail publication. Authored host files are
copied for semantic postprocessing and linked from their original locations in
the delivered projects. Keep private work directories when upstream harnesses
need their object fragments, especially libsmb2's legacy tests.

Receipts record effective selection, commands/logs, input paths, optional hashes,
warnings and task outcomes. A failed later behavioral gate does not turn a
successfully built product into qualified output. Read the attempt status and
its actual test scope. MsQuic's fresh qualification closure is an attachment in
the run directory; `config/product-closure.json` remains historical. Standalone
historical archive/freeze and clean-reproduction audits keep their stricter
archival contracts and are separate from ordinary translation/testing.

## Migration notes

`build.sh` now builds existing output for every project. SQLite's former combined
workflow is available as `build-all.sh` (an alias for shared translation).
`emit-engine.sh`, picotls `build-only.sh`, and existing Python translation
entrypoints dispatch to the common runner. Python is resolved by the shared
Bash helper in specialist shell scripts too.

Consumer sources moved to `samples/ManagedConsumer`; solutions are at project
roots. Raw output names now use `.Raw`, and debug/legacy/single-thread profiles
use `generated/profiles`. Existing generated directories are not source inputs:
regenerate after updating a checkout with the old layout. Internal native helpers
use the path-only `Scripts/campaign-reference.py`; `fetch.sh` is a normal campaign
command with logs and receipts.

The framework tests are deliberately small fixtures:

```bash
python3 -B -m unittest discover -s Scripts/campaigns/tests -v
```

## Adding a project

Create `<project>/scripts/campaign.py` exporting a `Recipe` named after its
directory. For example, `examplelib/scripts/campaign.py` can contain:

```python
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, json_file, link_flags

def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin["directory"],
                 pin.get("sha256"), required=("example.c",))

def prepare(ctx):
    source = ctx.sources["product"]
    return Translation([Unit(source / "example.c")],
                       ["-std=c17", *flags(includes=[source])],
                       link_flags("Example", "Managed.Example"))

recipe = Recipe(
    "examplelib", "TranslatedExample", ("default",),
    ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(Suite("consumer", ("consumer",), matrix=True),),
    default_suites=("consumer",), verify_suites=("consumer",),
)
```

Supply the source metadata and an authored consumer whose project reference uses
the `TranslatedProject` MSBuild property. `Consumer.property` can name a different
property; `Consumer.arguments` supports `{root}`, `{work}` and `{label}` for smoke
test arguments. A different consumer project and a preliminary solution build
can also be declared there. `Consumer.build_arguments` supplies additional MSBuild
options, such as serial builds for consumers with shared dependency outputs.

```bash
bash Scripts/campaign.sh layout examplelib --write
bash Scripts/campaign.sh translate examplelib
bash examplelib/scripts/test.sh --form all
```

The scaffold creates directories, the root solution, and Bash wrappers while
preserving existing files. It checks for the consumer project rather than
inventing application code. No edit to `Scripts/campaigns` is needed.

`Suite.verify_profiles` lists alternate products prepared only by `verify`.
The framework validates their names and prepares the deduplicated union with
`required_profiles`. The suite's custom runner still controls execution across
these profiles. Explicit `test --profile ... --form ...` does not require
verify-only products; `required_profiles` retains its existing cross-profile
dependency semantics for both test and verify.

For custom tests, supply `Suite(..., run=callable)` taking `(context, suite)`, or
use ordinary `("script", "helper.py", ...)` / `("python", ...)` commands.
`Recipe.script_arguments` adapts existing harness options; `before_suite` prepares
suite-specific environment. `dependencies`, `supports_pristine`, `supports_fast`,
and `validate_options` keep project policy in the recipe. Optional `legacy_owned`
patterns describe pre-framework generated files when migrating an existing
delivery layout.

## Validation scope

Linux validation for this migration includes translations and raw/processed JIT
consumer checks for all seven projects, picotls JIT qualification, Pinta processed
NativeAOT, libsmb2 async/legacy translations and a legacy upstream harness build.
All seven root consumer solutions build. MsQuic's normal ABI/boundary translation
and raw/processed TLS adapter JIT/NativeAOT checks pass. The 35 framework contract
tests and 50 migrated project unit tests pass; shell/Python syntax and diff
whitespace checks pass as well. The framework tests include a temporary new
project exercising automatic discovery, layout scaffolding, offline acquisition,
and both consumer forms without changing framework code.
The full cross-project verification matrix, Docker/network suites, clean-checkout
reproduction audits, and Windows/macOS execution are not claimed by these checks.

The Pinta upstream verification attempt exposes a native reference failure:
`code_v2` and `code_globals_properties_v2` exit with SIGSEGV in the corrected
native harness on this Linux host. The managed owning consumer passes. The runner
reports this as a failure; incomplete native transcripts are not accepted as a
passing differential baseline.
