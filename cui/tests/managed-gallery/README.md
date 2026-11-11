# ManagedGallery qualification

The showcase remains safe C#. This test executable uses raw observation only to
retain the independent gallery's node/edit/allocator assertions, and references
the translated gallery only for benchmark/oracle work. Those references are not
dependencies of `samples/ManagedGallery`.

Run the full Linux matrix from any working directory, using absolute paths for
the script, original pinned source and externally configured work/tool directories:

```bash
export CUI_FAKETIME_LIBRARY="$CUI_TEST_TOOLS/libfaketimeMT.so.1"
python3 cui/tests/managed-gallery/run.py --work-root "$MANAGED_GALLERY_WORK" \
  --source "$CUI_ORIGINAL_SOURCE" --form all --mode all --xvfb
python3 cui/tests/managed-gallery/product.py --work-root "$MANAGED_GALLERY_PRODUCT" \
  --form all --mode all
```

Prerequisites match the original gallery suite: .NET 10/NativeAOT tools, CMake,
C compiler, native GLFW development files for the oracle only, X11/GLX, Xvfb,
xdotool and the color emoji font. Standalone file-access auditing uses `strace`
when available. `libfaketime` is a qualification-only dependency. It can be
unpacked into the external tools directory without installing it system-wide;
the product neither links nor loads it. Supply `--faketime-library` instead of
the environment variable if preferred. No root privileges are required.

The runner verifies the actual native and translated `time()` value is Unix
epoch `1794398400` (2026-11-11 12:00 UTC). Static captures freeze wall time and
CUI's animation clock, with UTC timezone and a shared immutable filesystem
fixture. This matters because Planning's current-time label reads wall time
independently of `ui->time`. Input workflows run as separate processes with real
wall/monotonic time; freezing .NET's subprocess environment would interfere with
its xdotool controller. The separation does not omit either pass.

Default capture scope is all 27 pages × two themes × eight accents for each
raw/processed × JIT/NativeAOT variant: 1,728 wrapper captures, 1,728 translated
reference captures and 432 independent native palette captures. Full-window RGBA
and all node identities/parents/rectangle/font/length observations match the
translated gallery exactly. Native node floats are compared by IEEE single-
precision bits after decoding C's nine-digit and C#'s round-trip text formats;
this is equivalent formatting, not a numeric tolerance. Native full-window GL
channels retain the established one-byte conversion-rounding bound. Ordered
main-content text is exact. No sidebar mask or dynamic-page exclusion is used.

Each variant executes the original and wrapper real XTEST workflows: all-page
sidebar selection, scroll/navigation, fields, checkbox/switch/radio/disabled
controls, slider, buttons, tabs, stepper, modal, themes/accents and emoji picking,
recent items and 17 UTF-8 editing checkpoints. The shared extension adds multiline
textarea persistence, filter-popover state/escape, 2,000-row Ctrl+A/clear, grid
edit commit/cancel and sorting. Selection stays at the row position through
sort, matching upstream. Typed application observations and exact state
transcripts compare original and wrapper behavior. Inline emoji and picker-cell
crops compare exactly. Every lifecycle observes one UI free, released GLFW
context/display and successful reopen. Narrow-window OS resize, drawer navigation
and restoration of the wide sidebar are also checked. Each current variant
passes 4,042 assertions and 95 state-asserted actions. Actual Windows execution
remains unrun.

Actual Buttons, Data grid and Developer tools pages also run three benchmark
process trials per variant, with 200 warmup frames and 600 alternating paired
samples. Exact software pixels are required before timing. Optimized JIT disables
tiered compilation; measured regions exclude GL/vsync and software rasterization.
Report construction and layout/paint p50/p95/p99 separately, managed bytes/frame,
wrapper arena and core arena reservation. These complete pages include translated
core and authored application allocations. They do not claim whole-app zero
allocation. The separate library suite verifies zero additional wrapper bytes.
Actual-page gates limit the sum of construction and layout/paint medians to
twice direct and p95 construction overhead to 0.5 µs/node. The phase-median sum
is not a full-frame quantile. The authored app may add at
most 1 KiB/frame over the direct equivalent for formatting/results.

`product.py` builds the standalone showcase, copies its output to a fresh
directory and runs from an empty working directory. It checks all 27 pages,
invalid selection, RGBA capture, repeated disposal, disposed reuse rejection,
reopen, exact managed dependency closure and native imports. With `strace`,
source-tree and original-build-output file access is rejected. The product
project explicitly sets `AllowUnsafeBlocks=false`.

`--palettes baseline` and `--capture-only` are diagnostic selections and do not
satisfy the full campaign gate. `--native-reference` can reuse an independently
passed, hash-verified native-only run using the same wall-clock fixture.
`qualify-existing.py` adds a fresh native observer pass and timing-gate checks
to a retained full matrix without rebuilding or rerunning managed samples.
Every runner retains an `evidence.json`, case-level rows, binary hashes, source
observations and command logs under its caller-selected external work directory.

`record.py --matrix "$MANAGED_GALLERY_WORK" --product "$MANAGED_GALLERY_PRODUCT"
--native "$CUI_NATIVE_GALLERY_WORK" --source-commit REV --audit AUDIT_RECEIPT.json
--solution-log SOLUTION.log --recipe-log RECIPE.log --output RECEIPT.json`
produces a portable delivery receipt only from passing complete matrices. It
checks current wrapper hashes against the library receipt and retains each
page/palette result, timings, actions, source hashes and full-receipt hashes.
