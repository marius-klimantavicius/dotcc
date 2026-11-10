# Real Linux desktop services

`DesktopServices.Tests.csproj` is an ordinary separate managed consumer of the
canonical CUI project. It executes the original translated public
`cui_app_file_dialog` and `cui_app_open_url` entry points. It never calls
`PlatformHost` directly, edits generated C#, or translates/links another core.
The null app argument intentionally resolves the live `DesktopApplication`.

From the repository root, use a **fresh**, caller-selected disposable directory
for each invocation (the Linux `xdg-open` generic branch requires this path to
contain no whitespace):

```bash
python3 cui/tests/desktop-services/run.py --work-root "$task_work/raw-jit" \
  --cui-project "$PWD/cui/generated/TranslatedCui.Raw/TranslatedCui.csproj"
python3 cui/tests/desktop-services/run.py --work-root "$task_work/processed-jit"
python3 cui/tests/desktop-services/run.py --work-root "$task_work/raw-aot" \
  --cui-project "$PWD/cui/generated/TranslatedCui.Raw/TranslatedCui.csproj" --aot
python3 cui/tests/desktop-services/run.py --work-root "$task_work/processed-aot" --aot
```

The standalone script starts its own authenticated Xvfb with TCP disabled; do not
wrap it in another `xvfb-run` or give it a personal desktop. When invoked by the
campaign, it replaces the enclosing test display with its own. It clears inherited
display/Wayland configuration, forces GTK/X11 and Qt/xcb, and gives the runner a
private HOME, XDG config/data/cache/runtime, unreachable session-bus address and
restricted tool PATH. Read-only GTK schemas, MIME definitions and icon resources
remain available; system desktop application associations do not. No desktop
defaults are changed outside the disposable directory. The only URL handler is
a private Python argv recorder for a unique fixture scheme and fixture text
files. No HTTP URL, network action, mail or personal file is used. Its private
X11 clipboard supplies exact Unicode text to the dialogs via Ctrl+V.

Linux prerequisites are .NET 10, Python 3, Mesa/OpenGL 3.3, `zenity`, `kdialog`,
`xdg-open`, `gio`, `xdg-mime`, `update-desktop-database`, `xdotool`, `xclip`,
`Xvfb`, `xvfb-run`, `xauth`, and ordinary shell utilities. NativeAOT also needs
the existing .NET native compiler prerequisites. Nothing installs packages.
`--dialog-tools zenity` or `--dialog-tools kdialog` selects a smaller workload;
the default requires and executes both. For the kdialog run the private PATH
contains no zenity, exercising the real missing-zenity fallback. It does not
claim fallback after a zenity cancellation/error, which the host intentionally
does not perform.

Each tool run creates a live OpenGL CUI app and drives actual OS windows for:

- Open confirmation and cancellation, save confirmation, folder confirmation,
  and a successful tool selection rejected by CUI for insufficient capacity.
- Exact UTF-8 bytes including spaces, Greek/Lithuanian letters, quotes and shell
  metacharacters, with the NUL terminator and buffer guards checked.
- Translated focus/mouse cleanup after each dialog, continued app pumping,
  shutdown, owning disposal and reinitialization; both dialog tools unavailable.
- Literal URL argv, a leading-dash Unicode filename, absent association, absent
  launcher, null/empty URL and a handler that deliberately exits 23.

Transparent Python recording trampolines invoke the **installed actual**
dialog/launcher executable with the received arguments and forward its stdout,
stderr and status unchanged. They do not produce dialog output or substitute
launcher results. Receipts identify the actual executable, argv, process and
exit status, plus byte-preserving stdout/stderr logs. The driver verifies that
each dialog window belongs to that actual executable's child PID. It waits for
tool completion and checks that actual tool processes were reaped. Xvfb and its
clipboard clients are disposed by the coordinator; fixture files and receipts
remain in the work directory for review.

Expected recorded tool statuses on the qualified Linux tool versions:

| Real process / case | Exit status | Public CUI result |
| --- | ---: | --- |
| zenity or kdialog: open, save, folder | 0 | true, exact UTF-8 path |
| zenity or kdialog: cancel | 1 | false, cleared output |
| zenity or kdialog: selection with small CUI buffer | 0 | false, cleared output |
| xdg-open generic: handler success | 0 | true |
| xdg-open generic: private handler exits 23 | 4 | false |
| xdg-open generic: unregistered private scheme | 3 | false |
| xdg-open generic: leading-dash local file | 0 | true |
| xdg-open GNOME/gio: private handler exits 23 | 0 | true |

The last row deliberately verifies the asynchronous boundary: **launcher
success does not establish desktop handler success**. Only the isolated generic
xdg-open branch waits for this fixture handler. Its failure behavior must not be
generalized to GNOME/gio or other desktop environments. Save automation supplies
the Unicode basename in the real tool's Name field; it does not assert that the
tool initially displays a default basename (GTK4 zenity can leave that blank).
The tool's invocation still records the exact default-name option.

The installed xdg-open 1.1.3 also misclassified an initial valid private URI
scheme containing digits as a filename (actual status 2, CUI false). The
successful fixture schemes therefore use unique alphabetic names. This is an
observed launcher limitation; these passes do not establish arbitrary URI
scheme support through that installed launcher.

Each invocation uses `TMPDIR`/`TMP`/`TEMP` and `dotnet --artifacts-path -m:1`.
`build.log`, `coordinator.log`, `xvfb.log` and top-level `evidence.json` summarize
the run. Tool subdirectories retain `run.log`, `automation.log`, individual
`tool-status/` receipts and handler argv/exit receipts. The command exits nonzero
on a failed assertion, missing prerequisite, unexpected actual status or timeout.
The Python coordinator and .NET runner explicitly report Windows **UNRUN**
(exit 77); they do not touch a Windows desktop or qualify Windows dialogs/URLs.
That interactive external platform gate remains required. Native Wayland,
other desktop environment launchers, overwrite prompts, and general desktop
handler outcomes are outside this runner's coverage.

On 2026-11-10 the canonical raw/processed × JIT/NativeAOT Linux matrix passed
with 56 assertions for each actual dialog tool, 112 per configuration and 448
total. That includes 40 actual dialogs and 40 actual xdg-open invocations across
the final matrix. Recorded versions were zenity 4.0.1, kdialog 23.08.5,
xdg-open 1.1.3 and gio 2.80.0. All four owned Xvfb processes terminated. Durable
counts, statuses, coverage limits and source fingerprints are in
[`evidence.json`](evidence.json); full receipts remain in disposable storage.
