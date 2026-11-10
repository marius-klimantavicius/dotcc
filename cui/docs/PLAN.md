# Translate CUI with DotCC

Status: Linux implementation and qualification passed; overall acceptance remains
open for required actual Windows x64 execution.
Campaign: `cui/`. Updated: 2026-11-10.

This plan adopts [translation baseline v4](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md). The user selected the simpler
GLFW translation first. CUI's own Win32/X11 backends are deferred for a later
decision. Current session instructions govern agent use and commits.

## Outcome and scope

Deliver `TranslatedCui`, containing CUI's translated widgets, layout, text,
shaping, image decoding, software and OpenGL renderers, and shared app loop plus
its GLFW adapter. A separate managed consumer opens a window, draws controls,
accepts input, captures rendered output and shuts down cleanly. The same managed
assembly chooses Windows or Linux/X11 at runtime. NativeAOT executables remain
target-specific, as required by .NET; this does not require separate CUI source
translations or different platform feature profiles.

The supplied managed GLFW implementation is copied under `src/Glfw/`, with its
license and source snapshot metadata. No sibling checkout or native GLFW binary
is a product dependency. OS/windowing and graphics imports remain permitted.

| Scope | Selection |
| --- | --- |
| Required | All core units, embedded Inter/Geist/Lucide fonts, stb PNG/JPEG/BMP/GIF/TGA decoding, text shaping and bidi, app/input/clipboard, OpenGL windows, offscreen software rendering, local file browsing, platform dialogs/URL services |
| Runtime targets | Windows x64 and Linux x64/X11; explicit unsupported-platform failure |
| Deferred by user | CUI's direct Win32 and X11 window backends |
| Excluded | Cocoa, SDL2, browser/Wasm, native Wayland, 32-bit, software presentation through GLFW |
| Baseline departures | Desktop platform execution may require external target access and cannot be inferred from compilation |

GLFW's upstream CUI adapter cannot present software pixels. Preserve software
rendering for offscreen use, and reject unsupported desktop renderer requests or
GL initialization failures rather than accepting an invisible software fallback.
XWayland may provide X11 on a Wayland desktop; this is not native Wayland support.

## Agent orchestration

The user explicitly confirmed coordinator/implementation-agent orchestration.
Use `gpt-6.1-sol` with `high` reasoning for the coordinator and `xhigh` for
implementation agents. Both settings were confirmed available. Run independent
work packages in parallel with explicit file ownership; coordinate shared builds
and compiler edits. The coordinator owns integration and continued execution
through all required gates, with the parent retaining overall responsibility.

Initial independent packages: GLFW/GL ABI and lifecycle under `src/Host/Glfw*`
and `OpenGlHost.cs`; OS services and guarded adaptations under
`src/Host/PlatformHost.cs` and `scripts/stage.py`; native/upstream qualification
under `tests/`. The coordinator owns the recipe, consumer, integration, evidence
and milestone commits. Adjust boundaries explicitly if a shared compiler repair
is needed. Existing unrelated working-tree edits are not campaign work.

## Decisions and feasibility

| ID | Decision | Choice and evidence/gate |
| --- | --- | --- |
| D1 | Inputs | CUI revision in `config/source.json`; vendored GLFW snapshot in `config/glfw-source.json`; preserve CUI and dependency notices. Decided. |
| D2 | Features | `config/profile.json`: app, default fonts, image decoder/all formats and shaping enabled. Native packaging/LTO/unwind options do not control managed functionality. Decided. |
| D3 | ABI | One DotCC source ABI, 64-bit pointers, UTF-8 CUI strings and DotCC UTF-16 wide strings. Emitted layouts, callbacks and native GL calls pass Linux JIT/AOT probes; Windows structures pass a shim, with real Windows execution open. |
| D4 | Boundary | Typed managed GLFW/OS overrides and exact managed-to-native GL thunks are implemented and executed on Linux. Copied GLFW and translated CUI algorithms remain the product boundary. |
| D5 | Lifetime | One active desktop app, owner-thread calls, rooted retained callbacks and stable opaque handles. Raw and owning destruction preserve deferred-error visibility while releasing C allocations. Linux JIT/AOT lifecycle probes and full desktop workflows pass; Windows execution open. |
| D6 | Delivery | Object emission, class `Cui`, namespace `Managed.Cui`; shared 256 KB `link_flags`, literal pooling and inline deduplication. Canonical raw/processed products. Decided. |
| D7 | Oracles | Matching native GLFW 9/9 and native window workload pass. All four managed window buffers match native exactly. Upstream 12-program Linux matrix and Noto/HarfBuzz 661-case oracle pass. Native X11 preliminary control is diagnostic only. |
| D8 | Matrix | Linux x64 raw/processed × JIT/AOT; Windows x64 same runtime dispatch and consumer, requiring actual execution. No Windows completion inferred from cross-compilation or Wine. P4/P6 open. |
| D9 | Tests | Linux 12-program × four-variant matrix, software/images/RTL pixels, shaping, ordinary failures/lifetimes, native/managed GLFW windows, actual OS input and real desktop services pass. Existing eight fuzz and one soak programs remain excluded. Windows axis open. |

Provenance hashes default to `warn`, with `off` supported. Guard every staged
source adaptation using unique local anchors. Never repair generated C#.

## Source and host contracts

Source selection follows `CUI_CORE_SOURCES`, adds shaping/bidi, embedded fonts
and inflate, then `cui_app.c` and `cui_app_glfw.c`. Platform dialog functions
are host services because their implementations are OS-specific and the POSIX
version assumes `fork`. Preserve their observable input/output/error contract.

| Boundary | Translated responsibility | Host responsibility and validation |
| --- | --- | --- |
| Window/input | CUI GLFW event mapping, app loop, pacing and state | Managed GLFW adapter; stable opaque handles, rooted callbacks, owner-thread enforcement, create/fail/destroy cycles |
| OpenGL | CUI shaders, renderer state, draw calls and texture lifecycle | Proc-address adapters with exact native signatures; execute real GL rendering and readback |
| Software renderer | Rasterization, tiles, damage, threading decisions | Shared libc threads/CPU count; compare deterministic pixels with native |
| Files/time | CUI browser sorting, filtering, layout and date algorithms | Shared libc where portable; targeted runtime OS choices for home/root paths, hidden files and emoji discovery |
| Dialogs/URLs | Public CUI option/result contract | Windows imports or Linux desktop tools; report cancellation/unavailability faithfully |
| Memory | Upstream allocation and buffer ownership | Shared allocator; pair releases, preserve callback/string lifetimes |

Highest risks: native GL calling convention (generated callbacks otherwise use
managed convention); GLFW object-to-C handle translation; callbacks after
destruction; Linux-selected filesystem defaults on Windows; blank-window
software fallback. Resolve with executable probes before claiming P1/P3.

## Framework and storage

Recipe: `scripts/campaign.py`. Final products:
`generated/TranslatedCui/TranslatedCui.csproj` and
`generated/TranslatedCui.Raw/TranslatedCui.csproj`.
Root solution: `ManagedConsumer.slnx`. Consumer:
`samples/ManagedConsumer/ManagedConsumer.csproj`, with `CuiProject` selection.
Authored adapters stay at their original `src/` paths and are referenced by
both forms. `src/Glfw` must remain independently buildable.

Disposable source acquisition, tools, native controls, object emission and
managed intermediate outputs use a caller-selected temporary directory through
environment configuration. Do not embed the current machine's scratch path in
scripts, source, generated project references or reproduction requirements.
Final products and durable summarized evidence remain in the campaign tree.
Scripts must also work on Windows with ordinary copies and no elevation.

## Milestones

| Phase | Exit gate | Status |
| --- | --- | --- |
| P0 | Frozen profile, recipe/layout, native GLFW workload, real translation attempt | Complete: 58-unit source selection emitted and linked; native GLFW 9/9 and trusted shaping baseline pass; copied GLFW builds |
| P1 | Executed GL/callback/handle/ABI probes and runtime-selection decisions | Linux executed probes pass; Windows ABI shim passes, real Windows axis open |
| P2 | Full source closure, raw/processed builds, import audit, canonical publication | Complete: all 58 units, final-tool raw/processed publication and dependency audit pass |
| P3 | Actual translated window workflow and correct input/render/shutdown | Linux raw/processed JIT/AOT OS-input and GL-readback workflows pass; Windows open |
| P4 | Required upstream behavior, software pixels, failures and lifetimes | Linux complete: 48 program rows, shaping/pixels/lifetimes, input-aligned docking, real dialogs/URLs; Windows open |
| P5 | Owning API, separate consumer, solution builds and JIT/AOT workflow | Owning API/sample delivered, solution builds, final Linux four-way consumer/native GL comparisons pass; Windows execution open |
| P6 | Final-tool regeneration, reproduction, dependency audit, required target matrix | Linux final-tool regeneration, clean committed-source reproduction, all seven suites and affected shared regressions pass; required actual Windows axis unavailable/unrun |

Commit significant milestones only after their gates pass. Leave unrelated
existing compiler and libghostty edits untouched. Track observed failures,
classification, reproducer, regression, repair and complete-source retries in
`docs/IMPLEMENTATION.md`. Required failed or unrun targets leave acceptance open.

## Validation selection

The recipe will declare `consumer`, `native`, `abi`, `upstream`, `render`,
`lifecycle` and `audit` suites. Default: `consumer`. Verification: all seven.
All seven suites have executable recipe runners. Specialist prerequisites and
comparison contracts are documented under `tests/`.

Preserve upstream assertion/case counts and exclusions. Compare integer statuses,
layouts and deterministic software output exactly; use explicit tolerances for
GL rasterization and timing. Preserve original logs. Shaping requires a trusted
Arabic-script font fixture; font availability is a prerequisite, not an excuse
to disable the feature. Headless offscreen/null checks do not establish desktop
window behavior. Real X11 controls can use Xvfb with Mesa GL.

Reproduction commands are in `README.md` and the specialist test READMEs.
Final receipts, source/tool/product hashes and exact scope are summarized in
`docs/VALIDATION.json`, with specialist evidence under `tests/`.
The initial probes and native controls are feasibility evidence. P0 is complete;
later phases retain their required runtime/platform gates.
