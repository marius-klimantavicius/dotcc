# Full translated desktop input qualification

This runner references the full CUI library and creates visible OpenGL windows
through `DesktopApplication`. It uses OS pointer, keyboard and text events, then
asserts actual translated widget state. Three app lifecycles each verify exactly
two button clicks, text insertion, left/backspace/home/delete/end editing,
Ctrl+A selection, UTF-8 `CUI λ` replacement, Enter submission, an OS resize to
800×560, nonuniform GL framebuffer pixels, and an OS close that ends the app
loop. It does not call `cui_input_*` or invoke input callbacks directly.

Linux requires X11/GLX, `xdotool` (XTEST), and OpenGL 3.3. The optional `--xvfb`
flag runs an isolated Xvfb display with Mesa software GL; install Xvfb and xauth.
X11 close uses a `WM_PROTOCOLS/WM_DELETE_WINDOW` client message, including without
a window manager. It deliberately does not destroy the native window directly.
The Windows counterpart uses `SendInput`, `SetCursorPos`, `SetWindowPos` and
`WM_CLOSE` on an interactive desktop. Cross-compilation or Wine does not qualify
Windows runtime behavior.

Additional callback regressions use real OS refresh messages and a throwing
refresh callback. They check deferred error visibility at the translated app
pump; raw `cui_app_destroy` completing without a deferred exception before its C
frees; the pending error surviving teardown/reinitialization; and owning
disposal reporting the exception while still releasing its app. The copied
GLFW pump is used only to leave an error pending for these cleanup regressions.
A forwarding allocator-free hook verifies that both raw and throwing owning
destruction actually reach the C UI context release after GLFW termination.

All temporary files, build artifacts and logs go under a caller-selected work
root. From the repository root, run each final library form and execution mode:

```bash
python3 cui/tests/desktop/run.py --work-root "$task_work/raw-jit" \
  --cui-project "$PWD/cui/generated/TranslatedCui.Raw/TranslatedCui.csproj" --xvfb
python3 cui/tests/desktop/run.py --work-root "$task_work/processed-jit" --xvfb
python3 cui/tests/desktop/run.py --work-root "$task_work/raw-aot" \
  --cui-project "$PWD/cui/generated/TranslatedCui.Raw/TranslatedCui.csproj" --aot --xvfb
python3 cui/tests/desktop/run.py --work-root "$task_work/processed-aot" --aot --xvfb
```

On Windows omit `--xvfb`; the same runner selects Windows OS input and `win-x64`
for `--aot`. Each run writes `build.log` and `run.log` and exits nonzero on any
failed check, unavailable prerequisite, or timeout. The script prints the exact
build and execution commands. It never generates or edits translated C#.
