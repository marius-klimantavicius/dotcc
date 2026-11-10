# Original upstream gallery qualification

This harness references `TranslatedCuiGallery` and its matching canonical core.
It executes the translated `gallery_ui.c` and `gallery_more.c`, rather than a
managed reconstruction of their controls. The native control compiles the same
original sources against native CUI's GLFW backend with shaping and all image
formats enabled.
It also runs an independent native OS-input control for all 27 sidebar choices
and the same representative stateful controls before building managed variants.

Run from the repository root after translating both core and gallery products:

```bash
export CUI_WORK_ROOT=/path/on/external/storage/gallery-tests
mkdir -p "$CUI_WORK_ROOT/tmp"
export TMPDIR="$CUI_WORK_ROOT/tmp"
python3 -B cui/tests/gallery/run.py \
  --work-root "$CUI_WORK_ROOT" --source /path/to/pinned/cui \
  --form all --mode all --xvfb
```

`--form raw|processed|all` and `--mode jit|aot|all` select the Linux matrix.
`--native-only` runs the native capture and OS-input controls without managed builds.
Omit `--xvfb` to use an existing X11 display. Prerequisites are .NET 10 with
NativeAOT tools, CMake, a C compiler, GLFW development files, X11/GLX, Mesa,
`xdotool`, and Xvfb when selected. There are no Python package dependencies.
All temporary files, native builds, managed artifacts and logs stay under the
caller-selected external work root. Native builds use two jobs; every managed
build/publish uses `--artifacts-path` and `-m:1`.
Both executables run in one private fixed filesystem fixture, since the original
Files page browses the current directory.

The fixed workload is:

- Render all 27 original upstream pages at 1240×820, four frames per page.
  Assert nonblank **main content**, at least three main-content text nodes,
  distinct content text per page, and full RGBA capture length. Compare native
  and managed page names, node counts, main-content text counts and ordered UTF-8
  text hashes exactly. Compare every main-content RGBA channel with tolerance 1.
  No page or dynamic-content region is excluded. The sidebar is outside the
  raster comparison because its selection spring moves between pages.
- Select every page using real OS pointer input on the original sidebar,
  scrolling the sidebar to reveal extended pages. Assert the upstream selected
  page value and nonblank main-content GL readback after each selection.
- Exercise original checkbox, switch, disabled checkbox, radio, slider, text
  input, loading/reset buttons, tabs, forward/back stepper, modal open/Escape,
  and the extended Actions split button. Check actual upstream fields or
  rendered results, including persistence after navigating away and back.
- Scroll main content with the OS wheel, assert changed scroll offset and
  navigation's reset to the top. Change theme and accent through sidebar
  controls, assert both gallery and core state plus changed GL pixels, and
  exercise the original Ctrl+J theme shortcut. Wait for palette transitions to
  finish before comparing theme and accent captures.
- Close through the OS, observe the app loop ending, dispose twice, observe
  exactly one UI allocation release and no current GLFW context/X11 display,
  then create a fresh gallery and assert its original base-state defaults.

Only the capture phase fixes `ui.time=10` and `ui.dt=1/60` before calling the
original gallery. Both controls use the same explicit render boundary on every
frame. This stabilizes time-driven widgets for the differential; the OS input
phase uses the ordinary app clock. Tests select pages directly only during
capture; all behavioral navigation and changes go through XTEST, copied GLFW,
and translated input/widget code. No `cui_input_*`, input callbacks, or synthetic
control-state writes substitute for OS interaction. Extended-page upstream
static storage retains its normal process lifetime.

The runner owns and reaps its Xvfb process even after a test failure. External
`report.json` includes source hashes, the native page records, per-variant
comparison records, execution state, and display cleanup. Each variant retains
its build/run logs, all 27 `.rgba` captures, and a managed action/state report.
Failed commands or missing products fail the runner and preserve the partial
report. Windows execution remains explicitly **unrun**; this runner does not
claim Windows runtime qualification.

Repeated runs preserve previous logs and the report under `history/`. UTF-8
main-content transcripts beside each `.rgba` capture identify string differences
without normalizing their values.

The 2026-11-10 Linux run passes native and raw/processed JIT/NativeAOT. All 108
managed main-content buffers match native byte-for-byte, with maximum channel
error zero. Every managed variant completes 27 OS page selections, 53 recorded
workflow steps and cleanup. Counts and retained artifact paths are in
[evidence.json](evidence.json). The first differential caught a shared `%g`
default-precision defect; final full-source regeneration and all four gallery
variants pass after its repair. Original failure evidence is retained.
Artifact paths in the evidence are relative to the caller-chosen external gallery
test work root; the canonical translation receipt is relative to the repository.
