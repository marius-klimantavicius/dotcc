# Application ID integration

This unsafe-disabled consumer exercises the 1.4 application ID through the safe
owning API and observes actual X11 `WM_CLASS` with `xdotool`/`xprop`. It checks
two distinct IDs, empty/default reset, NUL rejection, compacting GC after the
creation pin ends, repeated disposal, disposed reuse and reopen. The separate
host-GLFW suite also overwrites UTF-8 caller storage before window creation and
checks Unicode hints, wrong-thread calls and lifecycle reset at the bridge.

Use external temporary and artifact directories:

```sh
TMPDIR="$CUI_TEST_TEMP" dotnet build cui/tests/app-id/AppId.Tests.csproj \
  -c Release --artifacts-path "$CUI_TEST_ARTIFACTS" -m:1
TMPDIR="$CUI_TEST_TEMP" LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a \
  dotnet "$CUI_TEST_ARTIFACTS/bin/AppId.Tests/release/AppId.Tests.dll"
```

Repeat with `-p:CuiVariant=Raw` and with `dotnet publish -r linux-x64
-p:PublishAot=true`, executing the published native executable, for the full
four-variant matrix. Actual Windows execution remains unrun; compilation or
X11 observation does not qualify its runtime behavior.
