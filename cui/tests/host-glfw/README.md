# GLFW/OpenGL boundary tests

`HostGlfw.Tests.csproj` calls the **emitted** `Cui.glfw*` overrides and translated
`cui_gl_*` renderer. It references the full processed library by default; supply
`-p:CuiProject=/absolute/path/to/TranslatedCui.csproj` to select raw or another
output. The tests require .NET 10, a working Windows desktop or Linux X11/GLX,
and OpenGL 3.3. On headless Linux, install Xvfb, xauth and Mesa. The renderer uses
a hidden 32×32 window. Windows execution remains a separate qualification gate.

Set `task_work` to an absolute disposable directory, then run from the repository
root. All build and temporary files go there:

```bash
mkdir -p "$task_work/tmp"
export TMPDIR="$task_work/tmp"
python3 cui/tests/host-glfw/verify-abi.py --source-root "$CUI_SOURCE_ROOT"
python3 cui/scripts/generate-gl.py --check
dotnet build cui/tests/host-glfw/HostGlfw.Tests.csproj -c Release \
  --artifacts-path "$task_work/artifacts" -m:1
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 800x600x24 +extension GLX' \
  dotnet "$task_work/artifacts/bin/HostGlfw.Tests/release/HostGlfw.Tests.dll"

dotnet publish cui/tests/host-glfw/HostGlfw.Tests.csproj -c Release \
  -r linux-x64 -p:PublishAot=true \
  --artifacts-path "$task_work/artifacts-aot" -m:1
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 800x600x24 +extension GLX' \
  "$task_work/artifacts-aot/publish/HostGlfw.Tests/release_linux-x64/HostGlfw.Tests"
```

Use ordinary `dotnet`/executable invocation without Xvfb on a Windows desktop and
select `win-x64` for NativeAOT. A different `CuiProject` property must be passed
to both build and publish when selecting an alternate library.

When the full campaign output is unavailable, `build-probe.py` emits all 40 GLFW
overrides from `abi-probe.c` and the **unmodified upstream GL renderer** with the
campaign profile and compiler. It writes a disposable project referencing the
authored host files and copied managed GLFW. No generated C# is patched:

```bash
python3 cui/tests/host-glfw/build-probe.py --compiler "$DOTCC_COMPILER" \
  --source-root "$CUI_SOURCE_ROOT" --work-root "$task_work/probe"
dotnet build cui/tests/host-glfw/HostGlfw.Tests.csproj -c Release \
  -p:CuiProject="$task_work/probe/ProbeLibrary.csproj" \
  --artifacts-path "$task_work/artifacts" -m:1
```

This focused product validates the ABI/host/renderer boundary. It does not
validate the full CUI app loop, widgets, stage adaptations, OS services, or
owning consumer. Rerun against both final campaign forms during integration.

Coverage includes all 46 GL manifest declarations checked against upstream;
emitted GLFW video-mode layout; all ten callback signatures, previous callback
pointers, UTF-8 file paths, and GC rooting; exceptions contained across a native
reverse-call boundary and an actual X11 expose event; owner thread and second
initialization rejection; missing-display initialization and retry; invalid
window creation; cursor/window lifetime and stale tokens across initialization
cycles; cached GL calls after context release/destruction/termination; float GL
arguments, native pointer-sized buffer lengths, texture upload/update/release,
shader/program creation, GLboolean vertex normalization, indexed triangle
rendering, and exact interior/exterior pixel invariants with ±1 clear-color
rounding tolerance. It reports assertions and GL driver version.
