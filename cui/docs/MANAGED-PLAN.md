# Safe managed CUI and ManagedGallery

Status: implementation in progress. Baseline: `d1aec0cc9`. This campaign extends
[the translated CUI plan](PLAN.md), preserving its generated products, managed
GLFW implementation and independent `ManagedConsumer` reference application.
The [translation baseline](../../docs/plans/translation-baseline.md) applies.

Library milestone: Linux raw/processed × JIT/NativeAOT passed ordinary builds,
whole-wrapper AOT rooting, recursive public API audits, 69 safety assertions per
variant and three repeated equivalent-work benchmark trials per variant. The
source-hashed receipt is `MANAGED-LIBRARY-VALIDATION.json`. All 27 authored page
captures now match the translated gallery's full RGBA/text/layout exactly;
frozen wall-clock qualification also removes native Planning clock drift without
masking pixels. Full gallery interaction/matrix delivery remains in progress.

## Product and boundary

Deliver `cui/src/ManagedCui/ManagedCui.csproj`, namespace `Cui.Managed`, and
`cui/samples/ManagedGallery/ManagedGallery.csproj`. The gallery is authored safe
C# and compiles with `AllowUnsafeBlocks=false`. It references the owning wrapper;
the original translated gallery is an oracle dependency of qualification only.
Translated CUI remains responsible for widgets, layout, text and rendering.

The wrapper's public graph contains deliberate safe values, options, callbacks
and opaque owned handles. No raw context, address, pointer, function pointer or
unsafe-bearing translated structure escapes. Calls enforce owner thread,
disposal, frame validity, scope order and resource ownership. Retained inputs
must remain stable for their actual upstream lifetime, including deferred paint.
Public borrowed references or spans require enforceable lifetimes; caller-owned
copy destinations are preferred. Persistent resources have deterministic cleanup.

Desktop uses the existing managed GLFW port, Windows/Linux X11 runtime dispatch
and desktop OpenGL. Software rendering remains an offscreen option. The existing
one-active-desktop-application contract remains explicit. Native CUI and GLFW
libraries, native Wayland and CUI's own platform backends are outside scope.

## Work ownership and orchestration

Coordinator: `gpt-6.1-sol`, high reasoning. Implementation agents:
`gpt-6.1-sol`, xhigh reasoning. Freeze the API contract and retention rules before
parallel page implementation. Assign disjoint files; coordinate shared changes
and builds. The coordinator owns integration, final review, evidence and focused
local commits. No push is authorized.

All scratch inputs, temporary files, build output, logs and benchmark data live
on externally configured storage. Set `TMPDIR` and pass `--artifacts-path` to
every .NET build or publish. Authored deliverable sources belong in this repo;
runtime closure must survive deleting scratch. Keep machine-specific storage
and sibling repository paths out of committed sources.

## Gallery inventory and parity

The original files under `generated/TranslatedCuiGallery/upstream/` define the
oracle: nine primary pages and eighteen extended pages. Preserve every demo,
label, option, initial state, callback, data set, navigation and theme interaction.

| Pages | Required families |
| --- | --- |
| 0–8 | Overview, Buttons, Inputs, Selection, Navigation, Overlays, Feedback, Data display, Charts |
| 9–17 | Actions, Smart inputs, Pickers, Capture, Disclosure, Reader, Data grid, Code, Social and media |
| 18–26 | Planning, More charts, Motion, AI, Audio, Files, Developer tools, More developer tools, Instruments |

Data grid selection follows row position through sorting, as upstream does.
Preserve this behavior. Preserve existing UTF-8 scalar editing, emoji picker
scope repair and editable color emoji display adaptations. This campaign adds
neither grapheme editing nor shaped static-label emoji features.

## Gates and evidence

1. Audit actual upstream pointer retention and freeze the safe API contract.
   Inventory public API coverage against every gallery call and data type.
2. Build the full library and all 27 authored pages through ordinary project
   references. Audit the recursive public API surface and safe gallery build.
3. Compare deterministic full-page pixels, text and layout against the original
   translated gallery and adapted native oracle using fixed clock, viewport,
   theme, fonts and filesystem fixtures. Investigate mismatches; do not add
   arbitrary tolerances or omissions.
4. Exercise real X11 OS input with state assertions across editing and emoji,
   selection, grid sort/selection, overlays, scrolling/navigation and theme/accent.
   Include close/reopen and ordinary failures, not only static captures.
5. Exercise GC pressure and deferred data/callbacks, exception unwinding, copied
   and repeated scope disposal, invalid frame use, wrong thread/context, disposed
   reuse, bounds and resource cleanup. Audit unmanaged growth.
6. Run Linux raw/processed × JIT/NativeAOT. Retain relevant original regression
   qualification. Audit project/dependency closure and generated-code preservation.
   Actual Windows execution is unavailable and must remain explicitly unrun.
7. Benchmark equivalent direct and wrapper UI workloads in warmed Release runs:
   light controls and complex/virtualized pages. Separate UI construction and
   wrapper cost from layout/paint, GL and vsync. Report managed bytes/frame,
   unmanaged/resource high water and time distributions across repeated samples.
   The allocation-conscious wrapper path targets zero steady-state wrapper
   allocations. Identify inherent translated-core and application allocations
   separately. Set numeric throughput gates from representative measured costs;
   no unmeasured performance claims.
8. Publish reproducible commands, matrix and case-level evidence, API guide and
   examples, dependency/license boundary and practical limitations. Review the
   final diff and make focused local milestone commits after relevant checks.

Required failures or missing qualification leave the corresponding gate open.
Historical translation evidence supports the unchanged baseline only; it does
not establish wrapper or authored-gallery correctness.
