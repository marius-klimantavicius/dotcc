# Implementation ledger

Updated 2026-11-11. Linux qualification passes. Overall acceptance remains open
for actual Windows x64 execution. Behavioral qualification is summarized in
[EMOJI-VALIDATION.json](EMOJI-VALIDATION.json). The metadata-only JSON migration
and current sidecar hashes are in [METADATA-VALIDATION.json](METADATA-VALIDATION.json);
earlier reports retain their historical product hashes and qualification scope.

The safe wrapper and fully authored gallery follow-up is complete on Linux;
see [MANAGED-DELIVERY.md](MANAGED-DELIVERY.md),
[MANAGED-LIBRARY-VALIDATION.json](MANAGED-LIBRARY-VALIDATION.json) and
[MANAGED-GALLERY-VALIDATION.json](MANAGED-GALLERY-VALIDATION.json). All four
raw/processed JIT/NativeAOT variants pass after the synchronous-singleton storage
correction. This follow-up leaves generated core/gallery implementations,
metadata, managed GLFW and existing host unchanged. The original consumer remains
the independent reference application. Actual Windows execution remains unrun.

## Executed integration findings

The matching GLFW native control passes all nine selected deterministic CTest
entries. Of 18 registered entries, eight fuzz programs and one soak are excluded
by D9. Printed assertion totals are 6,681 (6,147 core, 378 devtools, 146
instrumentation, 10 todo); bidi/RTL/inflate/C++/screenshots use their own status
and output contracts. A private working directory must contain a directory entry
for upstream's filesystem-list assertion. The harness now supplies that fixture.

The unchanged upstream shaping program, shape implementation and bidi source
match the trusted Noto Sans Arabic/HarfBuzz oracle exactly for all 661 cases,
including ordered combining-mark pairs. Minimal translated bidi and inflate
programs also pass. These results do not establish the full library matrix.

| ID | Classification and reproducer | Repair and regression | Full-source retry |
| --- | --- | --- | --- |
| CUI-01 | Override signature parser rejects C preprocessor spelling `bool` in `cui_fs_list`; independently supplied signature text is not preprocessed | Use `_Bool` in typed override metadata; affected files/app/GLFW objects emit | All 58 selected objects emitted and linked on retry |
| CUI-02 | Missing shared libc `lround`, `lroundf`, `llround`, `llroundf`, `copysignf`; raw full-library build reports unresolved method names | Add header declarations and shared numeric implementations preserving ties-away rounding, domain handling and float sign bits; 19 numeric unit cases pass | Fresh tools regenerate all 58 units; raw/processed builds and complete Linux behavior matrix pass |
| CUI-03 | Addressed compound literal in a `for` declaration's comma expression emits invalid C# `&new` | Give once-evaluated initializer storage the loop's enclosing scope; direct/object retained-pointer and continue/post regressions pass | Fresh-tool raw/processed generation and full upstream tests pass; final related compound/loop set has 13 passing cases |
| CUI-04 | Original Arabic shaping input is truncated by decoded `Console.In` character-to-byte casts; old-tool pipe regression exits 1 vs native 0 | Shared stdio reads raw standard-input bytes with common lookahead/pushback; source/object raw OS-pipe cases 2/2 and affected stdio/file/wide unit cases 40/40 pass | Fresh translated upstream shaping matches native/HarfBuzz 661/661 |
| CUI-05 | Fresh full object link exceeds 600 seconds; diagnostic stack locates repeated whole-fragment splits in managed-boundary target selection | Read source-language metadata once per object; all 32 enabled C/Zig boundary contract cases pass | Full 58-object link passes in 62 seconds; no source/algorithm workaround |
| CUI-06 | Original upstream test ternary initializers take compound-record addresses in lazy arms and emit invalid C# `&new` | Recognize addressed record temporaries as branch storage; 13 focused compound/loop cases pass; reduced native and translated programs both exit 0 | Complete upstream test bundle builds/publishes and passes raw/processed JIT/AOT |
| CUI-07 | Strict core assertion count differs: native 6,147 vs managed 6,160 with zero failed; difference isolated to randomized docking | Shared C `rand` is BCL-backed and C also permits different operand evaluation orders. Record actual seed/line/call-site/value inputs; native replays every value at the same call site. Assertions and mutation/drag algorithms stay intact | Original native 6,147 baseline still mandatory. Aligned native/managed 6,160, all 69 cumulative group counts exact; all four trace hashes equal and all 168,572 records consumed |

Focused GLFW/GL boundary qualification passes 120 assertions each in JIT and
NativeAOT on Linux/X11 with Xvfb/Mesa. It executes the translated upstream GL
renderer, real callbacks, exception containment, handles, textures, shaders,
triangle rendering and pixel readback. All 46 GL declarations match upstream.
Focused platform qualification passes 130 managed assertions, native browser/app
checks and 24 missing/duplicate staging-anchor rejections. Its Windows ABI shim
does not establish execution on Windows. Reproduction contracts are in
`tests/host-glfw/README.md`, `tests/platform/README.md` and
`tests/qualification/README.md`. All full transcripts remain in caller-selected
disposable storage; durable qualification summaries will be added after final
integration.

## Preliminary evidence

- Inspected CUI revision `629b6fcb991d71527f2d67b25c51429c3dcdb498` and its
  CMake source/options manifest.
- Emitted 57 shared/core translation units independently using the existing
  Release DotCC tool, with `CUI_TEXT_SHAPING` and `CUI_IMAGE_ALL_FORMATS` enabled.
  All 57 object emissions succeeded. A diagnostic link of those objects also
  succeeded, but it excludes the window backend and is not a buildable delivery.
- Native Release X11 control built with the same features. Eight selected CTest
  suites passed: `cui_tests`, `cui_test_devtools`, `cui_test_instrument`,
  `cui_cpp_compat`, `cui_test_inflate`, `cui_test_todo`, `cui_test_bidi`,
  `cui_rtl_layout`. This control preceded the user's GLFW-only selection.
- Native GLFW control configured and built with the same features; the later
  nine-suite deterministic baseline and shaping results supersede this probe.
- Copied 43 managed GLFW source/project files and the upstream GLFW license.
  The copied project builds under .NET 10 with zero warnings and errors.
- The CUI GLFW adapter emits successfully with 40 typed function overrides.
  This proves override selection/emission only, not the authored host's build
  or behavior.
- The initial 11 staged anchors matched. The final platform work uses 12 guarded
  adaptations, with native behavior checks and 24 rejection cases passing.

Original diagnostic logs and intermediate products are disposable. These
summaries do not substitute for framework receipts from final-tool regeneration.

## Current implementation state

Both complete raw and processed libraries are published at canonical paths.
The root consumer solution builds. The owning desktop sample runs in all four
Linux form/runtime combinations with nonuniform 640×480 actual GL readback,
software-window and second-app rejection, owner-thread enforcement, disposal
and reinitialization. The recipe declares all seven executable suites.

Full translated desktop input qualification passes raw JIT (458 assertions),
raw NativeAOT (442), processed JIT (468) and processed NativeAOT (442). Each
configuration executes three OS-driven input lifecycles, including button
clicks, keyboard editing, Unicode text, Enter submission, resize, GL readback and
OS close. Cleanup regressions verify C UI allocation release after deferred
callback exceptions in raw and owning destruction. Counts include polling checks
and therefore vary; completed workload/state assertions are fixed.

The four-way original upstream Noto shaping matrix is exact for all 661 cases
against native/HarfBuzz. Five original RTL paragraph PNG captures also match
native after decoding to RGBA. All 12 selected programs pass all four Linux
variants: 48 passing rows. Image checks cover all five formats, ownership and
errors; software comparison checks two 256,000-byte theme buffers exactly and
128,000 RGBA/BGRA channel invariants per variant; offscreen lifetime tests check
33 assertions with allocation refusal and eight balanced cleanup cycles.
The randomized-core discrepancy is resolved by CUI-07's input-aligned control.
Original failures and original native counts remain in the evidence.

The matching native GLFW/OpenGL window runs the separate consumer's exact
four-frame widget workload. All four managed RGBA captures are byte-identical
to native: 1,228,800 bytes at 640×480 and 33,023 nonuniform pixels. The declared
GL tolerance is one channel value; observed maximum error is zero.

Real installed zenity/kdialog and xdg-open/gio pass 448 assertions across the
four configurations: 40 actual dialogs and 40 actual launcher invocations.
Unicode paths, cancellation/capacity, literal argv, failures, cleanup and owned
Xvfb termination are checked. A launcher success can outlive a failing detached
handler; installed xdg-open's digit-scheme limitation is recorded explicitly.
See `tests/desktop-services/` for exact versions, statuses and limitations.

The initial full-framework verification attempt used the system Python without
the private HarfBuzz package. Its nine native suites passed, then shaping setup
failed. The documented invocation now selects the private environment through
PATH, as required by the shared wrapper's Python selection, and verification
was restarted. This is a harness prerequisite failure, not a native pass claim
for that failed attempt. The following run was deliberately cancelled after its
older loaded harness encountered the known unaligned count difference; its
translation/native/consumer/ABI gates had passed. The updated-harness upstream,
render, lifecycle and audit retry passed, as did the final combined ABI/input/
real-services suite and the native-window/consumer/audit invocation. These
receipts collectively execute all seven declared Linux suites on final products.

Clean reproduction used a `git archive` of committed source at `39cf7adbf`,
including compiler/runtime/postprocessor/framework/CUI and root build metadata,
without the unrelated uncommitted Zig/libghostty work. Tools were rebuilt on
external storage, all 58 units regenerated, and raw/processed publication,
four-way consumer, host/OS-input and dependency audit passed from an unrelated
working directory. Relocated translation-unit and anonymous-type identities
differ, so byte-identical generated source is not claimed. Subsequent changes
add qualification fixtures/orchestration, project-path contract checks/docs and
the Windows-only STA entry attribute. The final root solution/sample also builds
and runs. Published project references stay campaign-local; no sibling nhor or
scratch dependency exists. A contract test verifies private host processing and
portable final links, including simulated different-drive private Windows paths.
Five generated CS1718 warnings retain intentional upstream NaN self-comparisons.

## Risks requiring executable checks

Linux GL ABI, callbacks, handles, lifecycle, explicit software-window rejection,
filesystem behavior and real desktop-tool interaction have executed successfully.
Actual Windows runtime execution remains open.
The linked-machine discovery tool was unavailable in this environment; no remote
Windows execution target has been established. Native Wayland and CUI direct
backends remain excluded by the user's scope.

To close the remaining required platform gates, provide an interactive Windows
x64 target with .NET 10 SDK, NativeAOT native tools, OpenGL 3.3 and native-oracle
prerequisites. Execute raw/processed JIT/AOT window/input/GL/lifetime and OS-service
checks plus upstream differentials. The existing specialist native compilation
commands and real Linux dialog/launcher driver are Linux-specific; their Windows
counterparts need target-specific qualification. Cross-publication or Wine does
not close these gates. No Windows pass or complete P6 acceptance is claimed.

An additional shared framework control ran 94 tests: 93 passed, and the
existing metadata-policy test rejected a literal hash in
`blink/scripts/full_jit_copy_bindings.py`. That literal is present at the
campaign's starting commit and the file has not been changed here. The affected
C/C# compiler/runtime regressions above run separately and pass. Do not describe
the optional full framework control as all passing.

## Upstream gallery follow-up

ManagedConsumer now defaults to the original 27-page component gallery from the
pinned upstream `gallery_ui.c` and `gallery_more.c`, with unmodified C/header
copies, license and provenance in the generated companion. The 58-unit core
stays separate. `TranslatedCuiGallery[.Raw]` imports canonical core aggregate,
function and runtime identities and references the matching ordinary core
project. The sample preserves its STA entry point and owning desktop lifetime.
It opens at upstream's 1240×820 with a 720×480 minimum. Its original four-widget
`--smoke` implementation is preserved except for moving its class/entry method;
`--gallery-smoke`, `--list-pages` and numeric/named `--page` select the new work.

Three concrete integration needs were addressed without editing emitted C#:

| ID | Finding | Repair and focused evidence |
| --- | --- | --- |
| GALLERY-01 | A separate gallery assembly needs the core's actual struct/enum/callback types and shared C runtime | Managed-symbol export/import for nested static C managed object links; canonical aggregate signatures and runtime fingerprints checked. Four unit gates, separate-assembly callback execution, namespace/split/nesting checks and the 20-case affected execution selection pass. One imported runtime owner is supported; import/re-export chains and instance/state-context modes are rejected. |
| GALLERY-02 | Upstream virtual-list macro's comma initializer captured addressed locals in an IIFE, producing CS1686 | Reuse one-time enclosing statement storage for comma initializers; direct/object regressions assert declaration scope, ordering, continue and post behavior. Fresh complete core/gallery regeneration builds. |
| GALLERY-03 | Feedback's `%g` meter label printed `18.399999618530273` instead of native `18.4` | Shared printf uses C's default six significant digits and treats precision zero as one. Thirteen new cases fail before repair; 94 focused unit and 16 functional/native-oracle tests pass after repair. Fresh runtime-embedding tools and all 58+2 units are regenerated. |

The sample's four Linux raw/processed JIT/NativeAOT runs each render all 27
pages in 108 frames with nonblank main content and actual GL draw/readback,
then dispose and reinitialize. The retained smoke captures remain exactly equal
to the fresh native 640×480 control in all four variants, maximum channel error
zero. A no-argument launch opens the real visible gallery and exits successfully
on an OS close event. Evaluated core/gallery/sample project references remain
campaign-local; no native CUI/GLFW import or scratch/sibling reference is added.

Clean reproduction rebuilds tools from the committed compiler/runtime/framework
archive plus an explicitly hashed authored CUI overlay, excluding unrelated
working-tree changes. All 58 core and two gallery units regenerate; all four
final projects, the solution, four-way consumer/audit and four-way bounded
gallery sample pass with external artifacts and one MSBuild node. The overlay
matches the delivered authored build, input, sample and test sources. Relocated generated identities differ;
byte-identical generated source across different source roots is not claimed.

The new strict gallery runner compares every main-panel RGBA channel and exact
node/text records for all 27 pages against the original native gallery. All four
Linux variants pass: 108 main-panel buffers are byte-identical, with maximum
channel error zero. Each variant passes all 27 OS sidebar selections and 53
recorded actions. The original native OS control passes 1,192 assertions;
managed counts are 1,069, 1,071, 1,071 and 1,070, including timing-dependent
polling checks. The documented recipe's processed JIT gallery route also passes.
The runner also
uses real OS input for all sidebar choices and representative stateful controls,
scrolling, themes/accents, overlays, persistence, OS close and cleanup. Only its
deterministic capture phase pins the clock; ordinary input uses the app clock.
Native-proven harness issues in pointer synchronization, partially clipped
sidebar navigation, animated scroll and modal scrim timing were corrected while
retaining required actions. Its initial managed capture also started a theme
transition unlike native; matching initial light/neutral descriptions removes
that harness mismatch. Original failures and the `%g` defect captures/transcripts
remain in external history. No raster tolerance or required page was removed.

Final matrix rows, current source/tool/product hashes and exact scoped evidence
are in [GALLERY-VALIDATION.json](GALLERY-VALIDATION.json). The earlier
`VALIDATION.json` remains historical core qualification under its original
hashes; its full 48-program matrix is not claimed rerun for this follow-up.
Actual Windows execution remains unrun, and the direct CUI backends remain
deferred by the user's scope.

## Emoji selection follow-up

The reported Smart inputs emoji-selection failure reproduces in both the
original native gallery and the retained managed raw product: the selected
emoji is appended as the exact UTF-8 bytes, but the visible popover remains
open. `cui_popover` pushes an ID scope for its body. The gallery called
`cui_popover_close("emoji-pop")` inside that scope, resolving a different ID
and creating an already-closed popup instead of closing the visible one.

The guarded `adapt_widgets` preparation step retains the selected pointer until
the popover body ends, then appends and closes in the same caller scope as the
toggle. This is a targeted upstream gallery repair, with no replacement picker
algorithm or compiler/runtime change. The acquired upstream remains untouched.
Native qualification applies the identical widget adaptation to its own
private source copy. Companion provenance retains original source hashes and
records both the adaptation and the staged translation hashes.

The native harness also requires `CUI_TEXT_SHAPING` and
`CUI_IMAGE_ALL_FORMATS` when reading `cui_ctx`: upstream sets those definitions
privately on its library, so omitting them from the executable changes internal
field offsets. The gallery control now uses the matching definitions.

The original native render probe confirms that the bundled plain-text fonts
do not display the selected emoji in the message input: its GL crop is
unchanged although its actual edit-paint payload contains the exact appended
UTF-8 bytes. The picker cells themselves contain color emoji pixels. This
popup-only follow-up left that visibility failure unresolved. Its failed
message-pixel assertion and crops remain historical evidence in
[EMOJI-POPUP-VALIDATION.json](EMOJI-POPUP-VALIDATION.json).

The visibility repair first investigated `cui_font_load_file/memory`,
`cui_font_add_fallback`, glyph lookup and the existing color outline helpers.
Text fallbacks feed the monochrome stb outline atlas; they do not integrate the
picker's CBDT/sbix/COLR color atlas. `cui_emoji_supported` and the existing
`cui__em_draw` path already provide font discovery, caching, color rasterization,
image rendering and clipping. Reuse those algorithms through a small internal
opacity-preserving bridge. `adapt_text_renderer` guards each source anchor and
runs for both translated preparation and the native comparison.

The original editor sets `text_unshaped` and shares its scalar glyph iterator
between measurement, hit testing, selection and paint. That iterator now assigns
one em to supported missing emoji and paints the color image on the text baseline
inside the original input clip. Ordinary glyph lookup/advances remain unchanged;
the original buffer, UTF-8 edit operations, widget and paint callback remain in
CUI. Static shaped labels and grapheme-cluster editing are not added by this
repair. An installed supported emoji font remains necessary. Linux uses the
existing Noto search; Windows uses the existing Segoe UI Emoji discovery and
COLR path, with actual Windows execution still unrun.

The first native attempt reached the emoji draw successfully but still produced
transparent 13px cells. Upstream's area downsampler silently returned when the
bitmap strike exceeded seven source pixels per output pixel. Replace its fixed
eight-tap capacity with the capacity needed for the existing area filter. This
retains the filter and picker pixels, while allowing input-sized glyphs. The
native/translated reduced probe covers 13px, 8px and 1px output, transparent
alpha and unchanged picker-size output. No native dependency or broad renderer
rewrite is introduced. All acquired original files remain untouched.

The stricter gallery regression rejects the retained popup-only native and
managed controls. It independently recomputes colored shape metrics from saved
RGBA inside the original edit-paint leaf, compares six inline crops exactly to
adapted native, and checks 17 OS-driven UTF-8 cursor/edit states. It retains all
popup, recent-history, persistence, navigation and all-page comparisons. A
diagnostic invocation always reports diagnostic status and exits nonzero.

Fresh-tool regeneration emits all 58 core and two gallery units with both
deduplication flags and matching managed-reference metadata. All four Linux
gallery variants pass: 108 page buffers, 24 inline-input crops and 12 picker
cells are byte-identical to adapted native. Each completes 75 state-checked
actions and all 17 UTF-8 editing checkpoints. The 12 core programs pass all
48 variant rows, including 661 exact shaping cases per variant, software pixels,
images and lifecycle checks. Four-way consumer/sample execution and dependency
audit pass; the sample's original four-widget buffer matches fresh native
exactly. Platform checks pass 130 assertions and all 42 anchor rejections.
An earlier attempt passed the four consumer rows before stopping on a missing
Pillow prerequisite; the retained retry uses the pinned qualified Python
environment and passes the remaining suites. Original failures remain preserved.

Current results, commands, original failing transcripts, source/tool/product
hashes and verification limits are recorded in
[EMOJI-VALIDATION.json](EMOJI-VALIDATION.json). Earlier gallery and deduplication
reports describe their historical products. Actual Windows execution remains
unrun.


## Managed-symbol metadata format

`--export-managed-symbols` now emits an indented `Dotcc.ManagedSymbols.json`
sidecar. It carries the same versioned type mappings, function names and runtime
fingerprint, and `--managed-reference` accepts both JSON and the original
comment-wrapped format. CUI's companion recipe imports JSON. Generated-file
ownership removes the old marker on regeneration; postprocessing preserves the
sidecar in place and alongside both snapshot projects, without compiling it.

The migration re-linked the retained qualified core/gallery objects with the
updated compiler. Their emitted C# and the decoded metadata payload are unchanged;
only the re-emitted sidecar and ownership manifests were published. Both CUI
consumer forms build and enumerate all 27 pages. Unit, separate-assembly execution,
packaging, recipe and postprocessor checks pass. The broader campaign framework
suite has one unrelated existing Blink digest-literal failure; details and
current metadata hashes are in [METADATA-VALIDATION.json](METADATA-VALIDATION.json).
Prior runtime qualification remains recorded against its original inputs.
