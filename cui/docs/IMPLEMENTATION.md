# Implementation ledger

Updated 2026-11-10. Full product/platform acceptance remains open.

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
| CUI-02 | Missing shared libc `lround`, `lroundf`, `llround`, `llroundf`, `copysignf`; raw full-library build reports unresolved method names | Add header declarations and shared numeric implementations preserving ties-away rounding, domain handling and float sign bits; 19 numeric unit cases pass | Fresh tool built; all 58 objects regenerated; combined link/build pending |
| CUI-03 | Addressed compound literal in a `for` declaration's comma expression emits invalid C# `&new` | Give once-evaluated initializer storage the loop's enclosing scope; direct/object retained-pointer and continue/post regression plus related loop/conditional cases: 11 pass | Fresh-tool full-source regeneration in progress |
| CUI-04 | Original Arabic shaping input is truncated by decoded `Console.In` character-to-byte casts; old-tool pipe regression exits 1 vs native 0 | Shared stdio reads raw standard-input bytes with common lookahead/pushback; source/object raw OS-pipe cases 2/2 and affected stdio/file/wide unit cases 40/40 pass | Fresh translated upstream shaping matches native/HarfBuzz 661/661 |
| CUI-05 | Fresh full object link exceeds 600 seconds; diagnostic stack locates repeated whole-fragment splits in managed-boundary target selection | Read source-language metadata once per object; all 32 enabled C/Zig boundary contract cases pass | Full 58-object link passes in 62 seconds; no source/algorithm workaround |
| CUI-06 | Original upstream test ternary initializers take compound-record addresses in lazy arms and emit invalid C# `&new` | Recognize addressed record temporaries as branch storage; 13 focused compound/loop cases pass; reduced native and translated programs both exit 0 | Complete upstream test bundle builds and publishes raw/processed JIT/AOT; behavior qualification ongoing |

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
native after decoding to RGBA. Complete upstream/render/lifecycle and final
framework verification remain in progress. The first complete-core test run
finds a strict assertion inventory difference (native 6,147 vs translated 6,160)
with zero failed assertions; this is an open gate under investigation.

The initial full-framework verification attempt used the system Python without
the private HarfBuzz package. Its nine native suites passed, then shaping setup
failed. The documented invocation now selects the private environment through
PATH, as required by the shared wrapper's Python selection, and verification
was restarted. This is a harness prerequisite failure, not a native pass claim
for that failed attempt.

## Risks requiring executable checks

Linux GL ABI, callbacks, handles, lifecycle and explicit software-window rejection
have executed successfully. Platform tests cover filesystem metadata and sorting,
runtime defaults, dialogs/URL status contracts and guarded failure cleanup.
Real desktop-tool interaction and actual Windows runtime execution remain open.
The linked-machine discovery tool was unavailable in this environment; no remote
Windows execution target has been established. Native Wayland and CUI direct
backends remain excluded by the user's scope.

An additional shared framework control ran 94 tests: 93 passed, and the
existing metadata-policy test rejected a literal hash in
`blink/scripts/full_jit_copy_bindings.py`. That literal is present at the
campaign's starting commit and the file has not been changed here. The affected
C/C# compiler/runtime regressions above run separately and pass. Do not describe
the optional full framework control as all passing.
