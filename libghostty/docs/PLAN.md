# Translate libghostty-vt with DotCC

Status: implementation in progress; acceptance incomplete.
Campaign: `libghostty/`. Updated: 2026-10-29.

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
| D3 | ABI/text | First target Linux x64, little endian LP64; C enum/int widths and actual layouts measured natively and from emitted code; UTF-8 stream and Unicode cells. | Provisional until emitted P1 probes. |
| D4 | Host/dependencies | Freestanding x64 C emission, explicit public allocator callbacks using shared `posix_memalign/free`; no product filesystem/OS services, native terminal dependency, graphics or regex provider. | Selected; native control passes; managed import audit P2/P6 pending. |
| D5 | Ownership | Each owner exclusively owns one upstream terminal and subordinate formatter/render handles; deterministic dispose, no use after dispose, synchronous serialized entry per owner; two owners must retain independent state. | Decided contract; P1/P3 lifecycle acceptance pending. |
| D6 | Delivery | Actual engine emitted to C by pinned Zig, then DotCC object linking with authored ABI adapter. Class `GhosttyVt`, namespace `Managed.GhosttyVt.Native`; original owning API under `src/Managed.GhosttyVt`; shared `link_flags` defaults (256 KB splitting, literal pool, inline deduplication), plus unused inline pruning. | Decided route; native whole-engine C transcript equals original. No managed product accepted yet. |
| D7 | Controls | Native library from the same pinned source, scalar feature configuration and ABI; assertion-based harness with exact deterministic observations. | In progress; native oracle must pass before transcript is accepted. |
| D8 | Matrix | Linux x64, scalar profile, raw/processed × JIT/NativeAOT, owning sample plus whole-library rooting qualification. | Required P4/P6; no rows passed yet. |
| D9 | Tests | Required behaviors and ordinary API errors/lifetimes plus applicable upstream terminal/C API tests; inventory and exclusion reasons retained. No additional performance target or long fuzz/stress campaign selected. | Required P4; inventory pending. |

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
and two exact support-header macro rules documented in [ADAPTATIONS.md](ADAPTATIONS.md).
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
  upstream render cases. Both use canonical `generated/profiles/<profile>/`
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
| P1 — Boundary feasibility | Actual emitted/native ABI, allocator, borrow and independent-owner probes. | In progress | Six real public-ABI storage probes match native/emitted code. Native original/accessor-engine boundary controls pass. Actual emitted engine execution remains required. |
| P2 — Complete translation | Entire selected implementation builds raw/processed at canonical paths, no manual repair or unresolved required imports. | In progress | Complete current source parsing advances through line 447923 of approximately 531000; no managed product published. |
| P3 — Useful execution | Chunked UTF-8/control transcript, observation, resize/reset and cleanup through translated engine. | Unrun | Owning API acceptance requires engine. |
| P4 — Feature qualification | Required features, ordinary errors/lifetimes and applicable upstream case inventory pass matrix. | In progress | Native original/C 58 applicable render cases pass; 3 Kitty-only cases explicitly excluded under existing scope. Remaining applicable upstream groups and managed matrix stay open. |
| P5 — Consumer delivery | Root solution and owning sample run JIT/AOT for both forms, plus full rooting. | Unrun | No runtime acceptance inferred from scaffolding. |
| P6 — Reproduction/acceptance | Final-tool regeneration, full required suites, local reuse/hash modes, authored preservation, dependency audit and affected regressions. | Unrun | Campaign remains incomplete until all required gates pass. |

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
| Upstream cases | `upstream-native` and `upstream`: 58 selected render cases with exact case transcripts, allocator checks and 3 recorded graphics exclusions. Other applicable groups remain open. | Both forms, JIT/AOT for the managed diagnostic profile. | Required; remaining selection inventory pending. |
| Native oracle | `native`: native assertions and case inventory. | Linux x64 native. | Required. |
| Closure | `audit`: complete implementation inputs, dependency and unresolved-import inventory, rooted AOT. | Both forms. | Required. |
| Shared regressions | Focused Zig grammar/frontend/native oracle execution, affected compiler/runtime tests and downstream campaigns selected by changed semantics. | Relevant targets. | Required after shared fixes. |

Default suite: `consumer`. Current verification suites: `native`, `consumer`,
`qualification`, `upstream-native`, `upstream`, `audit`. The single 24-case
qualification harness covers ABI, behavior and lifecycle without rerunning
identical cases under different suite names. Additional required upstream
selection remains open and must be declared before acceptance. Deterministic bytes, dimensions,
cursor coordinates, statuses and cell/style values compare exactly. Any
normalization requires a documented invariant and retained original transcript.
No external protocol peer applies to an in-process terminal state product.
Optional extended fuzzing, stress and performance campaigns are excluded.

## Reproduction commands and acceptance

The following canonical commands are required once recipe/delivery is ready;
they are acceptance instructions, not claims of successful execution:

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
