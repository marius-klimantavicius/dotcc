# Translate libghostty-vt with DotCC

Status: C route accepted with the explicit user upstream-test deferral;
direct Zig phase not yet qualified. See [acceptance evidence](C-ROUTE-ACCEPTANCE.md).
Campaign: `libghostty/`. Updated: 2026-10-30.

This plan instantiates the [project template](../../docs/plans/translation-template.md)
and adopts [translation baseline v4](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md). Evidence belongs to this
campaign's actual artifacts. No prior project's completion is inherited.

## Agent orchestration

The supervisor runs `gpt-6.1-sol` with `high` effort. Two parallel implementation
tasks were launched through T3 with provider `codex`, model `gpt-6.1-sol`, and
`reasoningEffort=xhigh`; returned identities and settings matched the request.
The native task owns source/toolchain metadata, acquisition, native controls,
and `docs/UPSTREAM.md`. The frontend task owns narrow Zig frontend/grammar
repairs, focused regressions, translation probes, and `docs/ZIG-FRONTEND.md`.
The supervisor owns this plan, recipe, delivery, integration, and qualification.
Compiler builds are coordinated; independent native work can run concurrently.
Each subsequent task receives a bounded scope and acceptance check. The
supervisor reviews results and continues through P6 without another user prompt.

Repository baseline: branch `sqlite`, HEAD
`d3b1ccd975fd883f4f6b75d4b70fa4c9f112fc81`, with extensive preexisting tracked
and untracked changes in compiler, libc, tests, Blink, PGlite, CPython and Zydis.
Those changes are preserved and excluded from campaign commits. No applicable
root/ancestor AGENTS.md was found. Campaign milestone commits are authorized
after validation; pushing and creating a PR are outside this authorization.

## Authorized route sequence

The user authorized this order on 2026-10-29 and explicitly changed the test
scope on 2026-10-30: "Let's skip upstream tests (we'll come to them eventually
when we start working od direct zig translation)". The later instruction
supersedes the earlier upstream-matrix prerequisite for the C-route commit.

1. Finish the current genuine Zig → C → DotCC campaign through the remaining C-route P6 gates: final regeneration, actual native engine/boundary controls, both forms/JIT/NativeAOT consumer, all 24 differential/physical ABI/rooted qualification cases in every row, factory-allocation matrix, audit, ordinary root solution, offline warn/off reproduction/authored preservation and affected shared regressions. Upstream test suites and their 2,622-case matrix are deferred to the direct Zig phase and are not counted as passes. Earlier builds or consumer passes alone do not complete the retained gates.
2. Commit the accepted C-route campaign and focused shared fixes, preserving unrelated staged and unstaged work. Report its completion commit and evidence, and retain this reproducible route as an independent baseline. No push or PR is authorized.
3. After that completion commit, integrate the relevant Zig/compiler dependencies from true upstream `sebgod/dotcc`, reviewed at `d3678b5c50f86bf65d831ee4efda6134a7894116` (`refs/remotes/sebgod-review/main`; review evidence under `build/upstream-dotcc-review`). Do not merge unrelated features. No upstream integration or direct-route implementation starts before the accepted C route is committed.
4. Continue without another user prompt to actual pinned libghostty-vt Zig → DotCC → `Managed.GhosttyVt`, with no C intermediate for the direct route. Preserve the selected functionality, owning API, namespace, data/factory requirements and native/C-route controls. Fix the demonstrated `noreturn`, top-level comptime/`@export`, type-returning `Enum`/`anytype`, and `Result`/import gaps; upstream alone does not close them. Resume the retained 2,622-case upstream inventory, native/C controls and all four managed rows in this phase. Require full export, execution and reproduction acceptance before closing the direct route.

The supervisor retains implementation, integration and validation ownership through both routes; required bounded parallel agents and coordinated shared builds continue.

## Outcome and scope

Translate the actual Zig libghostty-vt terminal engine, including its parser,
screen storage, modes, Unicode handling and state transitions, into
`TranslatedGhosttyVt`. Its owning managed API and separate sample consume a
chunked UTF-8 terminal transcript with control sequences, inspect terminal
state, obtain a plain text screen snapshot, resize/reset, and release resources.
The consumer must execute this workflow through translated upstream algorithms.
Native code is an oracle only. A handwritten parser, stubbed state machine,
or native wrapper cannot satisfy any runtime gate.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required | Terminal creation/free, incremental `vt_write`, cursor/dimensions/mode queries, grid content and styles, text formatting, resize/reflow, reset, Unicode UTF-8 including chunk boundaries, alternate screen and two independent instances. |
| Excluded | Ghostty GUI, window system, GPU/font rendering, historical macOS embedding libghostty; Kitty graphics/image decoding, regex search, snapshot serialization, input event encoding and tmux integration. |
| Later extensions | Independently qualified graphics, input encoders, search and other targets. |
| Departures | Genuine Zig-to-C compiler intermediate selected, then DotCC C lowering. Direct DotCC Zig frontend remains a diagnostic route with repaired regressions. The complete selected upstream engine stays translated. |

## Decisions and feasibility work

| ID | Decision | Choice and reason | Status; evidence/gate |
| --- | --- | --- | --- |
| D1 | Source version | Ghostty `1.4.1-dev`, commit `93967b7f9478e94dcbb42125c8a90f34feb40d75`; source and native toolchain pins in JSON. | Decided; `config/source.json`, `config/toolchain.json`. |
| D2 | Configuration | Root `src/lib_vt.zig`, C ABI implementation `src/terminal/c/main.zig`; scalar `simd=false`; formatter, render-state and grid-introspection enabled, other optional feature gates disabled. Unicode dependency/generator closure retained in actual upstream command. | Decided; `config/translation.json`, `docs/UPSTREAM.md`; native parity passes. |
| D3 | ABI/text | First target Linux x64, little endian LP64; C enum/int widths and actual layouts measured natively and from emitted code; UTF-8 stream and Unicode cells. | Passed actual emitted/native physical layout probes in all four qualification rows; explicit x87 long-double storage is not a numeric ABI promise. |
| D4 | Host/dependencies | Freestanding x64 C emission, explicit public allocator callbacks using shared `posix_memalign/free`; no product filesystem/OS services, native terminal dependency, graphics or regex provider. | Selected; native control and actual managed workflow pass. Final dependency/import audit passes in `001153-ab4e8deb`. |
| D5 | Ownership | Each owner exclusively owns one upstream terminal and subordinate formatter/render handles; deterministic dispose, no use after dispose, synchronous serialized entry per owner; two owners must retain independent state. | Passed lifecycle, independent-instance and concurrent-owner controls in both forms/JIT/AOT. |
| D6 | Delivery | Actual engine emitted to C by pinned Zig, then DotCC object linking with authored ABI adapter. Class `GhosttyVt`, namespace `Managed` (full class `Managed.GhosttyVt`); owning API types also use namespace `Managed`; original owning API under `src/Managed.GhosttyVt`; shared `link_flags` defaults (256 KB splitting, literal pool, inline deduplication), plus unused inline pruning and metadata-selected `--aggregate-data` during object emission. | Canonical raw/processed projects published; owning consumer passes both forms/JIT/AOT. Final C-route acceptance passes; upstream test work is deferred. |
| D7 | Controls | Native library from the same pinned source, scalar feature configuration and ABI; assertion-based harness with exact deterministic observations. | Native original and generated-C oracle controls pass; exact transcripts are retained. |
| D8 | Matrix | Linux x64, scalar profile, raw/processed × JIT/NativeAOT, owning sample plus whole-library rooting qualification. | Owning consumer and 24-case differential/rooted qualification pass all four rows; current final retained gates pass. Upstream rows are deferred to direct Zig. |
| D9 | Tests | C-route product behaviors, ordinary errors/lifetimes, native controls and 24-case qualification remain required. Retain applicable upstream terminal/C API inventory and exclusions for direct Zig. No additional performance target or long fuzz/stress campaign selected. | User-deferred: 58 render and 2,564 core cases, plus upstream-only host-I/O service probes. No deferred row counts as passed. Policy: `config/verification.json`. |

Source and toolchain metadata live under `config/`. Revision/checksum literals
belong there, not executable scripts. Default `--hashes warn` and supported `off`
accept usable local input trees without claiming identity to the pin. Strict
provenance is optional. Exact adaptations require unique local guards in all
modes; upstream inputs remain unmodified and preparation uses staged copies.

## Source, host and ownership contracts

The public description calls libghostty-vt a C library because it exposes a C
API; its engine is Zig. At this pin `ghostty_terminal_new` takes an allocator,
output terminal pointer, columns and rows directly. There is no construction
options struct. `vt_write` takes bytes and length; `resize` also takes cell pixel
dimensions. Queries write typed values through `void*`; their discriminator,
width and ownership must match the pinned headers. Formatter options use a sized
struct. Render/grid traversal is a separate API. Preserve generated enums where
available and document any fallback constants against these headers.

| Boundary | Translated responsibility | Host/reused dependency | Ownership, errors and validation |
| --- | --- | --- | --- |
| Allocation | Upstream terminal/page/style storage and cleanup. | Shared allocation runtime or allocator extension point. | Stable addresses and paired allocator/free; disposal and independent-instance probes. |
| Input | Incremental VT parsing, UTF-8 decoding and terminal mutation. | Consumer supplies borrowed bytes during the call. | No retained movable input; split-sequence and split-codepoint parity. |
| Observation | Query discriminators, grid/style state, formatter output. | Owner copies values/text before mutation invalidates upstream borrows. | Actual native/emitted widths/layouts; exact state/content comparisons. |
| Lifecycle | Construct, reset, resize, free subordinate handles and terminal. | Owning managed API guards lifetime and concurrent entry. | Repeated cycles, disposed access and two independently evolving instances. |
| Optional providers | Upstream feature selection excludes graphics/regex. | No replacement algorithms. | Configuration and unresolved-import audit. |

Allowed adaptations: upstream build options, explicit allocator extension point,
and five exact support-header macro rules documented in [ADAPTATIONS.md](ADAPTATIONS.md).
No engine source is edited. Grammar/lowering defects are repaired in the
shared compiler, not hidden by editing emitted C#. Generic frontend fixes need
native-valid reductions and execution checks plus full-source retries.

| Risk | Smallest useful experiment | Observable success/failure | Gate |
| --- | --- | --- | --- |
| Zig language/build closure | Compile actual `src/lib_vt.zig`, then all reachable terminal implementation/dependencies. | Precise parse/lowering diagnostics or complete emission; include real build option modules. | P0/P2 |
| C ABI via Zig exports | Probe exported routines and actual emitted layout against native. | Matching symbols, sizes, offsets and typed query values. | P1 |
| Upstream storage and Unicode | Feed chunked transcript through actual translated core. | Exact native cell/cursor/text parity after mutations. | P3/P4 |
| AOT and managed closure | Root full delivered library and run trimmed owning consumer. | Both raw/processed native executables run; no native terminal import. | P5/P6 |

## Framework integration and deliverables

- Recipe: `scripts/campaign.py`, `Recipe("libghostty", ...)`, scalar default profile.
- Final projects: `generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj` and
  `generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj`.
- Diagnostic profiles: `qualification` contains the native differential and
  physical layout harnesses; `upstream-render` contains 58 applicable unchanged
  upstream render cases and `upstream-core` selects 2,564 further applicable cases. These use canonical `generated/profiles/<profile>/`
  raw/processed paths. Their test entrypoints stay outside the scalar product.
- Root solution: `ManagedConsumer.slnx` with original authored API and sample.
- Sample: `samples/ManagedConsumer/ManagedConsumer.csproj`, translated project
  selected with `TranslatedProject`; deterministic transcript workflow above.
- Authored API: `src/Managed.GhosttyVt/`; original references retained through
  staging, postprocessing, promotion and subsequent rebuilds.
- Tool prerequisites: Python 3.11+, .NET 10, pinned Zig for native controls and
  any generated source intermediate, native C toolchain/NativeAOT prerequisites
  for selected Linux execution gates. Acquired native tools stay campaign local.

Use shared acquisition, Python resolution, process logging, managed builds,
postprocessing and transactional publication. No project-name branch is needed
in the framework. Ordinary orchestration/staging uses copies and must not need
Windows administrator privileges, Developer Mode or symlinks. Linux native test
prerequisites are distinct from portable orchestration. Invoke helpers from an
unrelated directory and preserve authored source edits across regeneration.

## Milestones and evidence

| Phase | Concrete exit gate | Status | Evidence/remaining work |
| --- | --- | --- | --- |
| P0 — Scope/native baseline | Pinned inventory, recipe/layout, working native workflow and actual first translation attempt. | Passed | Offline shared native suite passes 16 assertions; original and genuine C engine transcripts match. Shared recipe first complete attempt reaches generated engine underaligned typedef error. Layout and offline acquisition pass, including unrelated working directory. See implementation ledger. |
| P1 — Boundary feasibility | Actual emitted/native ABI, allocator, borrow and independent-owner probes. | Passed | Actual engine differential qualification passes all 24 cases in both forms/JIT/AOT, including six physical ABI layouts and allocator/lifetime controls (`172036-a1c40c5a`). |
| P2 — Complete translation | Entire selected implementation builds raw/processed at canonical paths, no manual repair or unresolved required imports. | Passed | Normal scalar run `170255-7d3e0289` emits/links the complete engine and boundary, builds raw, postprocesses/builds processed and publishes both canonical projects after final builds. Final retained behavioral/P6 gates pass in `001153-ab4e8deb`. |
| P3 — Useful execution | Chunked UTF-8/control transcript, observation, resize/reset and cleanup through translated engine. | Passed | Canonical scalar owning consumer passes raw/processed JIT and NativeAOT (receipts `171252-f8cfacd0`, `171340-0c043e6e`), including split UTF-8, observation, screens, resize/reset, concurrency and disposal. |
| P4 — Feature qualification | C-route required features and ordinary errors/lifetimes pass native and 24-case qualification matrices. | Passed for retained C scope | Final `001153-ab4e8deb` passes native controls and all 24 cases in four rows with zero failures/skips. Upstream tests are user-deferred, not passed; inventories and partial/cancelled evidence remain. |
| P5 — Consumer delivery | Root solution and owning sample run JIT/AOT for both forms, plus full rooting. | Passed | Root solution builds; actual owning sample passes all four rows and differential qualification passes whole-library-rooted AOT for both forms. |
| P6 — Reproduction/acceptance | Final-tool regeneration, retained C suites, warn/off local reuse, preservation, audit and affected regressions. | Passed with user-authorized upstream deferral | WARN `001153-ab4e8deb` and OFF `205950-b1f45aae` pass; final original-reference rebuild passes in 22.907 s. All 89 current facts are preserved. See `C-ROUTE-ACCEPTANCE.md`. Direct Zig and its upstream matrix remain future work. |

Dependencies: P0 → P1 → P2 → P3 → P4/P5 → P6. Native controls, API design and
generic frontend reductions can progress independently. Significant validated
milestones receive focused commits; no push or PR is authorized. Blocker ledger:
`docs/IMPLEMENTATION.md`, with detailed frontend work in `docs/ZIG-FRONTEND.md`.

## Validation selection

| Behavior/gate | Suite and assertions | Matrix | Selection |
| --- | --- | --- | --- |
| ABI/ownership | `qualification`: six physical layout/field-address cases and two allocator/delivery-boundary cases. | Both forms, JIT/AOT, Linux x64. | Required. |
| Useful workflow | `consumer`: transcript feed, snapshot, cursor, resize/reset, dispose. | Both forms, JIT/AOT. | Required. |
| Terminal behavior | `qualification`: 16-case exact native differential covers controls, chunking, Unicode, styles, scrolling/reflow, alternate screen and callback replies. | Both forms, JIT/AOT. | Required. |
| Errors/lifetimes | `qualification` and `consumer`: ordinary rejection/status behavior, allocation failure, 32 balanced cycles, independent instances and disposed access. | Both forms, JIT/AOT. | Required. |
| Upstream cases | `upstream-native` and `upstream`: 58 render cases; core selection retains 2,564 cases in 20 bounded products. All case names, exclusions, helpers and prior evidence remain. | Both forms, JIT/rooted AOT when resumed in direct Zig phase. | Deferred by user for current C route; 2,622 cases are not counted as passed. |
| Native oracle | `native`: native assertions and case inventory. | Linux x64 native. | Required. |
| Closure | `audit`: complete implementation inputs, dependency and unresolved-import inventory, rooted AOT. | Both forms. | Required. |
| Factory data/allocations | `factory-allocation`: current Release PE RVA/IL, strict warmed managed allocation checks with positive controls for every immutable getter/zero-argument value factory, exact canonical-source private executables. Argument-taking factories retain IL and separate consumer coverage. | Both forms, JIT/whole-rooted NativeAOT. | Required user output gate. |
| Shared regressions | Focused Zig grammar/frontend/native oracle execution, affected compiler/runtime tests and downstream campaigns selected by changed semantics. | Relevant targets. | Required after shared fixes. |

Default suite: `consumer`. Current C-route verification suites: `native`,
`consumer`, `qualification`, `factory-allocation`, `audit`, selected by
`config/verification.json`. Explicit upstream specialists and the upstream-only
`host-io` probe remain runnable, but ordinary C-route verify does not select or
prepare them. The single 24-case
qualification harness covers ABI, behavior and lifecycle without rerunning
identical cases under different suite names. The deferred upstream selection
remains fixed at 2,622 cases for the direct Zig phase. Deterministic bytes, dimensions,
cursor coordinates, statuses and cell/style values compare exactly. Any
normalization requires a documented invariant and retained original transcript.
No external protocol peer applies to an in-process terminal state product.
Optional extended fuzzing, stress and performance campaigns are excluded.

## Reproduction commands and acceptance

The following canonical commands reproduce the retained C-route checks:

```bash
bash Scripts/campaign.sh layout libghostty --write
bash Scripts/campaign.sh layout libghostty --check
bash libghostty/scripts/fetch.sh --fetch missing
bash libghostty/scripts/translate.sh --fetch never
dotnet build libghostty/ManagedConsumer.slnx -c Release
bash libghostty/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash libghostty/scripts/verify.sh --fetch never --rid linux-x64
```

Keep framework receipts under `artifacts/campaign/`, specialist evidence linked
from them, source/toolchain identity and current validation in the implementation
ledger. Repeat ordinary local reuse with `warn` and `off`, then fresh campaign
outputs without deleting unrelated work. Restore is controlled separately from
fetch. No required runtime, platform or reproduction row has passed at plan
creation. A smaller validated subset cannot close this campaign.

User output choices (2026-10-29): the generated type is `Managed.GhosttyVt`.
The authored owning types share namespace `Managed`; assembly/project/folder
names remain unchanged. `config/translation.json:aggregate_data=true` enables
`--aggregate-data` in source/object emission for every profile, retaining the
shared literal-pool link default. Output sizes and payload/fallback behavior
are measured against `artifacts/user-output-options-baseline.json`; reduction
is not assumed. Both forms and the execution matrices must be regenerated and
revalidated after these choices.

Large aggregate expression factories now use immutable span payloads for constant primitive and proven record tables, copied into owned inline storage; runtime values use typed value argument packs. Canonical regeneration `181517-e418fbb7` retains `Managed.GhosttyVt`, with a maximum factory/helper arity of two. Native and emitted sequencing/floating-width regressions pass. Current PE audits and private executables compiling exact canonical sources execute 3,277 getters and 92 zero-argument factories per form with zero warmed managed allocations in JIT and whole-rooted NativeAOT; positive controls allocate. Argument-taking factories retain IL and separate functional/consumer coverage. See `tests/FactoryAllocation/RESULTS.md`; final retained C P4/P6 acceptance is recorded in `C-ROUTE-ACCEPTANCE.md`; direct Zig remains unqualified.
