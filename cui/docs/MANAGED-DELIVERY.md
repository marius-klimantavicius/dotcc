# ManagedCui and ManagedGallery delivery

This report records the original safe-library delivery. Its measurements and
receipts precede the namespace rename and CUI 1.4 refresh. Current regenerated
product qualification is recorded in [UPSTREAM-1.4.md](UPSTREAM-1.4.md) and its
receipt; historical timings are not rebound to new binaries.

Linux delivery is complete for raw/processed CUI × JIT/NativeAOT. This includes
the requested replacement of synchronous native singleton arena allocations with
stack locals. Actual Windows execution is unavailable and remains **unrun**.

The product is [ManagedCui](../src/ManagedCui/ManagedCui.csproj), namespace
`Cui.Managed`, and the authored [ManagedGallery](../samples/ManagedGallery/).
The sample compiles with `AllowUnsafeBlocks=false` and references only the
wrapper. The wrapper references the translated CUI core, which uses the committed
managed GLFW port. The original translated gallery and `ManagedConsumer` remain
independent comparison applications. The later namespace rename regenerates the
core as `Managed.Forms.Cui` and the companion as `Managed.Forms.Gallery.Gallery`, including host bindings and JSON
symbol targets. `Cui.Managed`, project/assembly names and behavior are preserved.
The original gallery C sources and copied GLFW implementation are unchanged.

## API and ownership

Start with the [API guide](MANAGED-API-GUIDE.md),
[contract and gallery coverage map](MANAGED-API-CONTRACT.md), and
[storage audit](MANAGED-STORAGE-AUDIT.md). The library provides 325 typed
C-derived entry points plus safe overloads. All 266 distinct gallery CUI calls
are mapped. All 27 pages use wrapper calls for their content, state, callbacks
and resources; no page delegates construction to the original gallery.

The public API has no pointers, function pointers, address tokens or raw
translated structures. Owner-thread, frame, scope order, context and disposal
checks guard access. Stable owned buffers, state leases, rooted callbacks and
bounded reusable frame storage protect data that CUI retains. Nested scopes
unwind through `using`; copied/repeated disposal is checked. Caller spans are
copied when deferred rendering needs their contents. Resources support explicit
deterministic cleanup. One active desktop application is the existing contract.

The storage correction replaces 225 arena singleton allocations in 172 methods:
157 option/box records and 68 other values. There are **zero arena singleton
sites remaining**; 63 variable-length sites retain frame-stable pointees.
Persistent `State<T>`, edit/terminal buffers and native callback roots keep their
required ownership. Checked-in [templates](../scripts/managed-api/README.md)
reproduce reviewed sources and verify hashes for 214 native definitions plus
the header. They also reject unexpected concurrent changes.

## Qualification and namespace rename

The [namespace rename receipt](NAMESPACE-VALIDATION.json) binds current sources
and fresh Linux binaries to the focused post-rename checks. The source-bound
[library](MANAGED-LIBRARY-VALIDATION.json) and
[gallery](MANAGED-GALLERY-VALIDATION.json) receipts are historical pre-rename
evidence at `d5a662499`: their hashes bind that version, not today's aliases.
Their full palette, interaction and performance results below remain historical.
The rename checks do not claim a repeated performance or sixteen-palette run.
Full logs, binaries, captures and benchmark data use external storage.

Fresh rename checks pass all four raw/processed × JIT/NativeAOT variants:
75 safety and 12,164 public-surface assertions each, exact 27-page light/neutral
pixels/text/layout against translated and retained native references, and 4,042
assertions/95 OS actions for each original and safe gallery. All four standalone
samples pass copied-output lifecycle/dependency checks; both original consumer
smoke modes also pass. Both solution selections, paired regeneration, metadata,
platform ABI, GL signatures and template/native-retention checks pass. Normal
solution builds retain five existing translated warnings; raw selection builds
both core forms and reports five from each. No new source warning is introduced.

| Gate | Pre-rename Linux result |
| --- | --- |
| Ordinary references and whole-wrapper NativeAOT | All four variants pass |
| Recursive public API audit | 293 types; zero unsafe public signatures; 12,164 assertions/variant |
| Safety | 75 assertions/variant, including returned stack overwrite, compacting GC and delayed render |
| Full pages/palettes | 27 pages × 2 themes × 8 accents × 4 variants = 1,728 wrapper captures |
| Translated comparison | Full-window RGBA, ordered main text and every node layout exact |
| Independent adapted native comparison | All 1,728 comparisons also observed zero channel error; text/layout exact |
| Real OS interaction | Original and wrapper each pass 4,042 assertions and 95 actions/variant |
| Standalone product | All four copied-output builds run all pages/108 frames, capture, dispose/reopen and invalid-input checks |
| Dependency closure | Exact ordinary managed graph; no original-gallery, native CUI or native GLFW dependency; file-access audit passes |
| Root solution and reference regression | Release build succeeds; original `--smoke` and `--gallery-smoke` pass |
| Campaign integration | Recipe tests and raw/processed project/dependency audit pass |

The native gate retains the established maximum one-byte normalized GL channel
rounding rule. The pre-rename comparisons were exact. Native layout floats
are compared by float32 bits after parsing equivalent C/C# decimal formats.
There are no pixel masks, sidebar exclusions or omitted dynamic pages.

Capture fixtures verify frozen native/translated wall time, CUI animation time,
UTC, viewport, fonts and filesystem. Input runs separately with normal clocks so
the test's .NET subprocess controller can drive XTEST/xdotool reliably. Actions
cover all-page navigation, scrolling, themes/accents, fields and multiline
editing, checkbox/switch/radio/disabled controls, sliders, buttons, tabs, stepper,
modal and filter popovers, emoji selection/recent items/17 UTF-8 editing
checkpoints, 2,000-row selection, grid editing/commit/cancel/sort, window resize
and drawer navigation. Grid selection deliberately follows row position through
sort, matching upstream. Existing emoji scope and editable color-glyph fixes
are retained; this work adds no grapheme editor or shaped static-label feature.

Safety also covers wrong thread/context, stale frame/ID/handle use, bounds,
exception unwinding, copied scope disposal, resource disposal before paint,
callback exceptions/reentry/root release and eight allocator-observed lifecycles.
A descriptor-only frame of 256 boxes fits a 4 KiB marshalling budget and uses
zero arena bytes after the correction. Delayed-render regression overwrites
returned stack storage, compacts GC and mutates caller arrays before requiring
exact software pixels against direct CUI.

## Pre-rename measured cost

Measurements use .NET SDK 10.0.114/runtime 10.0.14, Linux x64 and an AMD Ryzen 9
7950X. Each variant has three independent Release benchmark processes, with
tiered compilation disabled for optimized JIT. Library trials use 200 warmup
frames and 1,200 alternating paired samples; real-page trials use 200/600.
Equivalent direct/wrapper workloads must first have identical software pixels
and node counts. Timed construction and translated layout/paint phases exclude
GL, vsync and software rasterization. Receipts retain every p50/p95/p99.

The table shows the median of three process p50 values for **processed JIT**,
in microseconds. These are phase summaries, not a full-frame quantile or FPS.

| Workload | Direct build | Wrapper build | Direct layout/paint | Wrapper layout/paint | Wrapper managed B/frame |
| --- | ---: | ---: | ---: | ---: | ---: |
| Light, 38 nodes | 1.483 | 2.575 | 23.244 | 22.893 | 0 |
| Dense, 362 nodes | 13.426 | 22.363 | 132.803 | 133.534 | 0 |
| Virtualized 20,000 rows, 200 nodes | 14.287 | 31.921 | 56.577 | 56.858 | 0 |
| Gallery Buttons, 238 nodes | 16.661 | 23.485 | 106.913 | 107.002 | 952 |
| Gallery Data grid, 506 nodes | 39.324 | 39.715 | 170.863 | 170.913 | 792 |
| Gallery Developer tools, 712 nodes | 79.731 | 101.352 | 247.308 | 254.000 | 50,524.24 |

The [interactive performance chart](MANAGED-PERFORMANCE.html) exposes each
variant and phase p50/p95/p99, using median process quantiles.

All four variants pass the gates: zero additional steady-state managed bytes in
the allocation-conscious library workloads, p95 construction overhead at most
0.5 µs per emitted node, and sum of construction and layout/paint medians at
most twice direct. The worst observed library ratio is 1.264 or less, and p95
overhead 0.108 µs/node or less. Actual gallery pages peak below 1.088 and
0.062 µs/node respectively. The sum of phase medians is deliberately not labeled
a full-frame median. These limits bound measured overhead; they do not promise
hardware-independent latency.

Full applications still allocate. The direct equivalents for Buttons, Grid and
Developer tools allocate 2,824, 11,624 and 52,248 B/frame respectively. The safe
Developer tools page allocates about 50.5 kB under JIT and 50.9 kB under AOT
(about 49.3/49.7 KiB). This includes core/app
formatting and result work. Actual-page gates allow at most 1,024 additional
B/frame over direct; all pass. Whole-app zero allocation is not claimed.

Current library arena demand is 256 / 2,560 / 21,104 bytes for light / dense /
virtualized workloads. The pre-change arena-use baseline was 1,824 / 16,800 /
21,376 bytes; these historical values are only a storage comparison. Minimum
reserved capacity remains 65,536 bytes in both versions. Actual gallery arena
demand is 4,256 / 7,936 / 34,400 bytes for Buttons / Grid / Developer tools.

Separate allocator instrumentation observes core high water of 243,128 /
478,264 / 390,596 bytes for the three library workloads after observer
installation. Retention is identical at frames 200 and 700; observed allocations
return to zero on disposal. This excludes initial context/font allocation,
software renderer, graphics drivers and wrapper NativeMemory; wrapper arena and
owner/root counts are observed separately. No unbounded wrapper cache is used.

## Reproduce and review

Use external `TMPDIR` and `--artifacts-path`, as in the
[sample instructions](../samples/ManagedGallery/README.md).
[Library](../tests/managed/README.md) and
[gallery](../tests/managed-gallery/README.md) runners document the complete
matrix, standalone closure pass, fixed-clock tool and portable receipt creation.
Campaign entry points expose `--suite managed` and `--suite managed-gallery`.

The root solution has five existing CS1718 warnings from unchanged translated
code and zero errors. Authored library/sample code introduces no warnings.
Windows/Linux runtime dispatch remains present, but Linux execution, AOT
compilation or shims do not qualify actual Windows behavior. Existing translated
core/emoji/metadata receipts retain their historical scopes. Product dependency
and license boundaries are unchanged: copied managed GLFW, translated CUI and
embedded assets keep the licenses/notices committed with their projects. Native
GLFW and libfaketime are qualification tools only. Product outputs run from a
copied directory without accessing source or original build outputs.
