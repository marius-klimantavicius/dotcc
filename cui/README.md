# Translated CUI

CUI's core, widgets, layout, shaping/bidi, image decoding, renderers and shared
app loop are translated to C#. Its upstream GLFW adapter uses the managed GLFW
source copied into `src/Glfw`. The same library selects Windows x64 or Linux
x64/X11 at runtime. Native CUI and native GLFW are not product dependencies.
The pinned source and feature profile are in `config/`.

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

Use .NET 10 and Python 3. Choose a disposable directory on external storage and
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

The first command builds current tools, emits all 58 units, links, builds raw,
semantically processes a private copy, builds processed and publishes both
forms transactionally. `build.sh` builds existing products; `probe.sh` emits
diagnostic objects. `--tools reuse`, `--fetch never`, and `--hashes off` have the
shared framework meanings. `DOTCC_COMPILER` and `DOTCC_POSTPROCESSOR` can select
explicit prebuilt tools. Generated C# must be regenerated, never patched.

Full Linux qualification needs CMake, a C/C++ compiler, GLFW development files
for the native oracle, X11/GLX and OpenGL 3.3, `xdotool`, and the Python test
dependencies. Install them into a private Python environment on external storage:

```bash
python3 -m venv "$CUI_WORK_ROOT/python"
"$CUI_WORK_ROOT/python/bin/python" -m pip install -r cui/tests/qualification/requirements.txt
export PATH="$CUI_WORK_ROOT/python/bin:$PATH"
```

Set `CUI_SHAPING_FONT` to the trusted Noto font described in
[font.json](tests/qualification/font.json). The font is a test fixture. Optional
`CUI_NATIVE_BUILD` reuses a matching native GLFW control; otherwise verification
builds one privately, with two native jobs. On headless Linux install Xvfb,
xauth and Mesa, then run:

```bash
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 1280x800x24 +extension GLX' \
  bash cui/scripts/verify.sh --jobs 4 --form all --mode all
```

Verification declares `native`, `consumer`, `abi`, `upstream`, `render`,
`lifecycle` and `audit`. It executes original upstream tests and compares native
results, exact shaping records and software pixels. Its ABI suite also runs
real desktop input, GL readback and cleanup tests. The eight existing fuzz
programs and one soak are excluded by plan D9. Specialist prerequisites,
counts and comparison rules are documented in [qualification](tests/qualification/README.md),
[host tests](tests/host-glfw/README.md), [platform tests](tests/platform/README.md)
and [desktop input](tests/desktop/README.md).

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

`ManagedConsumer.slnx` builds the separate sample. Use `-p:CuiProject=...` to
select the alternate library. Build outputs should stay on external storage:

```bash
dotnet build cui/ManagedConsumer.slnx -c Release \
  --artifacts-path "$CUI_WORK_ROOT/consumer" -m:1
dotnet "$CUI_WORK_ROOT/consumer/bin/ManagedConsumer/release/ManagedConsumer.dll"
```

Add `--smoke` for bounded GL/readback and lifetime checks, or `--capture PATH`
with `--smoke` to save the readback as raw RGBA bytes. The interactive sample
provides a button, checkbox, slider and text field. NativeAOT publication is
target-specific; use `dotnet publish` with `-r linux-x64` or `-r win-x64`,
`-p:PublishAot=true`, `--artifacts-path` and `-m:1`.

Linux file dialogs use installed `zenity` or `kdialog`; missing tools,
cancellation and failed launchers report failure. URL opening waits for the
launcher exit status and can block with handlers that remain attached. Windows
dialogs and URL opening use host OS APIs. CUI and dependency licenses are copied
under each generated project's `notices/`; managed GLFW retains its own license.
