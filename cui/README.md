# Translated CUI

CUI's core, widgets, layout, shaping/bidi, image decoding, renderers and shared
app loop are translated to C#. Its upstream GLFW adapter uses the managed GLFW
source copied into `src/Glfw`. The same library selects Windows x64 or Linux
x64/X11 at runtime. Native CUI and native GLFW are not product dependencies.
The pinned source and feature profile are in `config/`.

For safe C# applications, reference `src/ManagedCui/ManagedCui.csproj` and use
the `Cui.Managed` namespace. [ManagedGallery](samples/ManagedGallery/) is the
authored wrapper-based showcase; its caller code has unsafe blocks disabled.
The original translated gallery and `ManagedConsumer` remain the independent
comparison application. Read the [API guide](docs/MANAGED-API-GUIDE.md),
[contract](docs/MANAGED-API-CONTRACT.md), [managed campaign plan](docs/MANAGED-PLAN.md)
and [qualified delivery report](docs/MANAGED-DELIVERY.md).

The product includes embedded Inter/Geist/Lucide fonts, all widgets, PNG/JPEG/
BMP/GIF/TGA decoding, shaping/bidi, OpenGL windows, and offscreen software
rendering. Native Wayland and CUI's direct Win32/X11 backends are outside this
campaign. XWayland can supply X11. GLFW cannot present software pixels: explicit
software-window requests and failed GL initialization return failure. Offscreen
software rendering remains available through the translated `cui_sw_*` API.

See [the plan](docs/PLAN.md) and [implementation evidence](docs/IMPLEMENTATION.md)
for acceptance status. Actual Windows execution is a required outstanding gate;
an ABI shim, cross-build or Wine run does not close it.

## Regenerate and verify

Use .NET 10 and Python 3.11 or newer. Choose a disposable directory on external storage and
set `CUI_WORK_ROOT` to its absolute path. Campaign commands require it; acquired
inputs, tool snapshots, emissions and managed intermediates use that directory.
Final generated projects and durable campaign receipts live under `cui/`.
`CUI_SOURCE_ROOT` can select an existing usable CUI source tree. Otherwise the
recipe acquires its pinned archive into `CUI_WORK_ROOT/inputs`.

From the repository root, with environment variables set:

```bash
mkdir -p "$CUI_WORK_ROOT/tmp"
export TMPDIR="$CUI_WORK_ROOT/tmp"
bash cui/scripts/translate.sh --jobs 4
bash cui/scripts/test.sh --suite consumer --form all --mode all
```

The first command builds current tools, emits all 58 core units and the two
upstream gallery units, links the separate core and gallery libraries, builds
raw, semantically processes private copies, builds processed and publishes both
forms and their companion projects transactionally. `build.sh` builds existing products; `probe.sh` emits
diagnostic objects. `--tools reuse`, `--fetch never`, and `--hashes off` have the
shared framework meanings. `DOTCC_COMPILER` and `DOTCC_POSTPROCESSOR` can select
explicit prebuilt tools. Generated C# must be regenerated, never patched.

The CUI profile explicitly enables both `--deduplicate-inline` (already supplied
by the shared recipe default) and `--deduplicate-static` (CUI opt-in). Core,
gallery and qualification links use `scripts/codegen.py` and the
`deduplication` settings in `config/profile.json`; unrelated campaigns keep
their defaults. Deduplication merges only proven equivalent implementations.
Address-used functions and functions depending on distinct static storage retain
their identity. Static objects are not pooled by these flags.

Full Linux qualification needs CMake, a C/C++ compiler, GLFW development files
for the native oracle, X11/GLX and OpenGL 3.3, `xdotool`, `xclip`, `zenity`,
`kdialog`, `xdg-utils`, `gio`, `desktop-file-utils`, Xvfb and xauth. The real
desktop-service runner requires a disposable work path without whitespace for
the installed generic launcher. Install the Python test dependencies into a
private environment on external storage:

```bash
python3 -m venv "$CUI_WORK_ROOT/python"
"$CUI_WORK_ROOT/python/bin/python" -m pip install -r cui/tests/qualification/requirements.txt
export PATH="$CUI_WORK_ROOT/python/bin:$PATH"
```

The `managed-gallery` suite additionally needs a qualification-only libfaketime
shared library. Set `CUI_FAKETIME_LIBRARY` to its absolute path on external
storage; see [the managed gallery runner](tests/managed-gallery/README.md).

Set `CUI_SHAPING_FONT` to the trusted Noto font described in
[font.json](tests/qualification/font.json). The font is a test fixture. Optional
`CUI_NATIVE_BUILD` reuses a matching native GLFW control; otherwise verification
builds one privately, with two native jobs. On headless Linux install Xvfb,
xauth and Mesa, then run:

```bash
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 1280x800x24 +extension GLX' \
  bash cui/scripts/verify.sh --jobs 4 --form all --mode all
```

Verification declares `native`, `consumer`, `gallery`, `managed`, `managed-gallery`,
`abi`, `upstream`, `render`, `lifecycle` and `audit`. It executes original upstream tests and compares native
results, exact shaping records and software pixels. Its ABI suite also runs
real desktop input, GL readback, dialog/URL tools and cleanup tests. The native
and consumer suites compare the same full-window GL workload when selected
together. The eight existing fuzz
programs and one soak are excluded by plan D9. Specialist prerequisites,
counts and comparison rules are documented in [qualification](tests/qualification/README.md),
[host tests](tests/host-glfw/README.md), [platform tests](tests/platform/README.md)
and [desktop input](tests/desktop/README.md), [gallery qualification](tests/gallery/README.md),
[real desktop services](tests/desktop-services/README.md), and
[native desktop control](tests/native-desktop/README.md).

For a targeted qualification retry, `CUI_QUALIFICATION_OBJECTS` and
`CUI_QUALIFICATION_STAGED_SOURCE` can select a matching retained object/source
pair from an already validated final translation. Supply both and preserve its
profile/tool provenance. The specialist runner records all reused object hashes.
Ordinary `verify.sh` emits a fresh complete source closure.

For Windows use Python to invoke the shared CLI (`python Scripts/campaign.py`)
or Git Bash wrappers, an ordinary interactive desktop, and `--rid win-x64`.
The current native differential runner requires the documented Linux toolchain;
Windows native controls and real desktop execution must be qualified separately.

## Consume

The translated core type is `Managed.Forms.Cui`; the independent translated
gallery type is `Managed.Forms.Gallery.Gallery`. Authored desktop adapters use
`Managed.Forms`. Project and assembly names remain `TranslatedCui` and
`TranslatedCuiGallery`; the safe wrapper remains `Cui.Managed`. See the
[namespace rename evidence](docs/NAMESPACE-VALIDATION.json).

Reference `generated/TranslatedCui/TranslatedCui.csproj`, or select the raw form
at `generated/TranslatedCui.Raw/TranslatedCui.csproj`. Both reference the original
authored host sources and the copied GLFW project. No scratch directory or
sibling checkout is needed to build or run the delivered product.

`DesktopApplication` owns the window and translated context. Create, pump and
dispose it on the same thread. One desktop app can be active per process. `Ui`
is a borrowed unsafe pointer for the complete translated `Cui` API; release it
by disposing the app. Offscreen contexts have upstream ownership rules. Call
`NextFrame`, build widgets, then repeat; `Quit` ends the loop. `CapturePixels`
finishes the current frame and returns actual GL RGBA readback. Call `Dispose`
even after a managed callback throws.
On Windows, use an `[STAThread]` entry point when using folder dialogs; the
sample supplies it. COM apartment initialization failure reports dialog failure.
Raw GLFW handles are adapter-owned opaque tokens. Pass them only to this
translation's GLFW API; they are not native GLFW pointers or OS window handles.

`ManagedConsumer.slnx` builds both independent samples, the safe wrapper and the
translated upstream gallery companion. The core library remains separate from
these example workloads.
Use `-p:CuiProject=...` to select the raw core; the sample selects the matching
raw gallery automatically. `-p:CuiGalleryProject=...` overrides the companion.
Build outputs should stay on external storage:

```bash
dotnet build cui/ManagedConsumer.slnx -c Release \
  --artifacts-path "$CUI_WORK_ROOT/consumer" -m:1
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedConsumer/release/ManagedConsumer.dll"
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedGallery/release/ManagedGallery.dll"
```

The default application is the pinned upstream component gallery: all 27 pages
(nine primary and 18 extended), with its original navigation, responsive drawer,
themes, accent swatches, state and interactive controls. It opens at 1240×820,
with a 720×480 minimum size. The gallery C sources are translated into the
separate `generated/TranslatedCuiGallery` and `TranslatedCuiGallery.Raw` projects,
which reference the corresponding core rather than duplicating it.

Pass `--list-pages` to print the registered indices/names without opening a
window, or `--page Selection` / `--page 3` to choose the initial page. Quote page
names containing spaces. `--gallery-smoke` visits every page for four frames,
checks actual GL readback and draw commands, then checks disposal and
reinitialization. `--smoke` preserves the original 640×480 four-widget,
four-frame pixel/lifetime regression used by the native consumer oracle.
Add `--capture PATH` to either bounded mode to save its final framebuffer as
raw RGBA bytes; interactive capture saves frame four and leaves the window open.

```bash
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedConsumer/release/ManagedConsumer.dll" --list-pages
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedConsumer/release/ManagedConsumer.dll" --page "Data display"
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedConsumer/release/ManagedConsumer.dll" --gallery-smoke
```

NativeAOT publication is
target-specific; use `dotnet publish` with `-r linux-x64` or `-r win-x64`,
`-p:PublishAot=true`, `--artifacts-path` and `-m:1`.

Run `bash cui/scripts/test.sh --suite gallery --form all --mode all` for the
Linux native/translated gallery differential and real desktop-input workflows.
That suite owns its Xvfb display. Gallery follow-up evidence is recorded
in [EMOJI-VALIDATION.json](docs/EMOJI-VALIDATION.json), including the native/managed
emoji-selection regression. The subsequent metadata-only migration is recorded
in [METADATA-VALIDATION.json](docs/METADATA-VALIDATION.json): core link metadata is
now `Dotcc.ManagedSymbols.json`, and all executable C# remains unchanged. Keep
this sidecar with the core when regenerating the gallery. Preparation applies
one guarded upstream gallery repair: close the emoji popup after leaving its
ID scope. The original acquired source remains unchanged; the native control
uses the identical repair. Earlier gallery, deduplication and core reports
retain their historical product hashes and qualification scope. The later
[namespace rename receipt](docs/NAMESPACE-VALIDATION.json) binds current products;
all prior validation JSON retains its pre-rename bindings.
Preparation also connects missing emoji in the original editable text renderer
to CUI's existing color emoji engine. The shared text iterator supplies matching
paint, caret and hit-test advances; the original UTF-8 buffer and editor remain
upstream CUI. A targeted area-filter repair permits bitmap emoji at small input
font sizes. Native qualification applies the same adaptations. Color glyphs use
the existing system emoji font discovery (Noto Color Emoji on Linux, Segoe UI
Emoji on Windows); no additional native library is required. The gallery suite
requires actual colored glyph pixels inside the original input and OS-driven
UTF-8 navigation, insertion, deletion and selection replacement. Actual Windows
font rendering remains unrun.

Linux file dialogs use installed `zenity` or `kdialog`; missing tools,
cancellation and failed launchers report failure. URL opening waits for the
launcher exit status and can block with handlers that remain attached. Windows
dialogs and URL opening use host OS APIs. CUI and dependency licenses are copied
under each generated project's `notices/`; managed GLFW retains its own license.
