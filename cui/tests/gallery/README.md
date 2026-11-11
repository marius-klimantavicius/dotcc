# Original upstream gallery qualification

This harness references `TranslatedCuiGallery` and its matching canonical core.
Their translated types are `Managed.Forms.Gallery.Gallery` and
`Managed.Forms.Cui`; the project and assembly names are unchanged.
It executes the translated `gallery_ui.c` and `gallery_more.c`, rather than a
managed reconstruction of their controls. The native control compiles the same
staged gallery sources against native CUI's GLFW backend with shaping and all
image formats enabled. Its private copy receives the same guarded adaptations
from `stage.adapt_widgets` and `stage.adapt_text_renderer` as the managed product:
close the emoji popover in its caller ID scope, bridge missing editable glyphs
to the existing color engine, and permit small bitmap downsampling.
Original acquired sources remain unchanged. The native executable
uses the core's private shaping/image feature defines when reading `cui_ctx`.
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
The default `--native-oracle adapted` applies `stage.adapt_widgets` and
`stage.adapt_text_renderer` to a private
native source copy. `--source` must identify original acquired upstream, before
staging. To preserve the original native failure separately, use another work
root and `--native-only --native-oracle original`; that diagnostic is expected
to fail because picking appends UTF-8 but leaves the original popup open. It is
not an accepted oracle result. `report.json` records original and effective
source hashes plus the native adaptation list.

For before-fix evidence only, add `--diagnostic-allow-invisible-emoji` in a
separate work root. This records failed visibility assertions and continues
through all selections, edits and comparisons. Every other assertion stays
enabled. The runner marks the report `diagnostic`, exits nonzero, and never
counts that run as qualification. Normal invocation clears the diagnostic
environment variable before launching either executable. Direct executable
diagnostics use `CUI_GALLERY_DIAGNOSTIC_INVISIBLE=1`; their transcripts and
managed reports are likewise explicitly diagnostic.

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
- On Smart inputs, open the original smile icon with OS input and pick 😀,
  reopen and pick 😃, navigate away/back, then pick 😀 from Recent. Read the
  original input renderer's payload to assert exact UTF-8 message bytes after
  each selection. Check the original scoped popup closes, creates no phantom
  popup, and stays closed after its exit animation. Reopen to verify recent
  order, deduplication and persistence, then dismiss through OS Escape. Native
  observes read-only prefixes of pinned private paint payloads with `memcpy`;
  managed uses actual emitted payload types. Neither changes gallery storage.
  Cell targets come from the original grid's rendered geometry. Each selected
  cell must contain at least 50 colored GL pixels (channel spread >32), and the
  message framebuffer crop must contain rendered text and change after append.
  Independently crop the actual `message ^ 0xED17` edit-paint leaf inside the
  original editable input, excluding the smile button, field border and picker.
  Each picked message must contain at least 30 colored pixels per emoji, a
  colored shape at least 6 pixels high and 6 pixels wide per emoji, and at least
  three distinct 4-bit RGB shades. The original neutral plain-text crop must
  contain zero colored pixels. Capture with the original input unfocused and
  popup exit animation finished so caret, selection and popup cannot satisfy
  the color check. Retain cell/message/inline `.rgba` crops and dimensions in
  `EMOJI_PIXELS` and shape metrics in `EMOJI_INLINE`. Python independently
  recomputes every inline metric from the saved RGBA. Compare native/managed
  message bytes and recent-order transcripts exactly in all four variants, all
  three selected-cell crops with channel tolerance 1, and all six inline crops
  **byte-for-byte**.
- Edit that same original input through the OS after all three picks. Assert
  exact UTF-8 bytes and byte cursor/anchor positions at 17 checkpoints:
  End, Left/Right across a four-byte emoji, insert `X` between emoji, Home,
  Right over plain text, End, Backspace over a whole emoji, Left over `X` and
  the previous emoji, Delete over a whole emoji, Right, Backspace over `X`,
  Shift+Left selecting exactly one emoji, replace it with `Z`, and Backspace
  restoring the original plain-text prefix. Check focus belongs to the original
  input and all cursor/selection endpoints are UTF-8 boundaries. Require color
  shape in the mixed `😀😃X😀` input, then zero colored pixels after removing
  every emoji. Navigate away/back and reopen Recent to verify the edited message
  persists and typing/deletion leaves recent order/deduplication unchanged.
  `EMOJI_EDIT` records must match independently specified expected transcripts
  and the native transcript exactly.
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
its build/run logs, all 27 page `.rgba` captures, emoji GL crops, and a managed
action/state report.
Failed commands or missing products fail the runner and preserve the partial
report. Windows execution remains explicitly **unrun**; this runner does not
claim Windows runtime qualification.

Repeated runs preserve previous logs and the report under `history/`. UTF-8
main-content transcripts beside each `.rgba` capture identify string differences
without normalizing their values.

The 2026-11-10 deduplication follow-up passes native and raw/processed
JIT/NativeAOT with both `--deduplicate-inline` and `--deduplicate-static` on the
core and gallery links. All 108
managed main-content buffers match native byte-for-byte, with maximum channel
error zero. Every managed variant completes 27 OS page selections, 53 recorded
workflow steps and cleanup. Counts and retained artifact paths are in
[evidence.json](evidence.json). The first differential caught a shared `%g`
default-precision defect; final full-source regeneration and all four gallery
variants pass after its repair. Original failure evidence is retained.
Historical deduplication product hashes and targeted core results are in
[DEDUP-VALIDATION.json](../../docs/DEDUP-VALIDATION.json). Artifact paths in the
evidence are relative to the caller-chosen `CUI_WORK_ROOT`; the canonical
translation receipt is relative to the repository.

The emoji regression requires a color emoji font supported by the pinned picker
(e.g. Noto Color Emoji). Missing color rendering fails the test. The first two
Smileys cells must be the pinned 😀 and 😃 entries; the test checks the bytes
actually appended rather than accepting any glyph. The original native and
retained managed failure are classified as an upstream popup-ID scope defect;
correct emoji bytes already append before the repair. The prior 53-action,
108-capture deduplication evidence above predates this added workflow. The
historical popup-only emoji matrix evidence is in
[EMOJI-POPUP-VALIDATION.json](../../docs/EMOJI-POPUP-VALIDATION.json).
Current visibility/editing qualification is in
[EMOJI-VALIDATION.json](../../docs/EMOJI-VALIDATION.json).
Actual Windows execution remains unrun.

The historical popup-only product leaves picked emoji invisible inside the
plain message input. The stronger inline-visibility gate intentionally rejects
that product, even when exact bytes, picker pixels and popup behavior pass.
Historical evidence remains unchanged; it does not establish passage of the
new visibility/editing gate. Save new reports and before-fix diagnostics under
new work roots, preserving their source/binary hashes and original transcripts.
