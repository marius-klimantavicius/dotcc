Run the focused suite from any working directory with a fresh disposable output directory:

```sh
python3 /path/to/dotcc/cui/tests/platform/run.py \
  --source /path/to/acquired/cui \
  --work /path/to/disposable/platform-test-run \
  --dotcc /path/to/dotcc/DotCC/bin/Release/net10.0/dotcc.dll
```

Prerequisites: Linux x64, a C compiler with GNU-style section garbage collection,
.NET 10 SDK, and a built DotCC tool. The runner sets `TMPDIR` and uses
`--artifacts-path` with `-m:1`; all copies, generated code, executables and
intermediate outputs go under `--work`. Use a new `--work` for each invocation.
No native GLFW library, desktop session or dialog application is used.

The runner checks every staging anchor for missing and duplicate matches, then
compiles a native control using the actual staged browser and app sources. Its
native sizes/offsets are compared against actual DotCC-emitted CUI types.
The managed fixture compiles verbatim selected staged path, sorting, default
places and app-creation functions, with controlled window/core/GL services.
It emits `Managed.Forms.Cui` alongside the authored `Managed.Forms` host.
The GLFW init/allocation-failure prefix is selected verbatim and aborts if
execution reaches the part that would create a real window. Emoji discovery
uses the actual staged function with a recorded file-loading service. No
algorithm is copied into C# and no generated C# is edited.

Behavior checks cover caller-owned UTF-8 names, exact capacities and sentinels,
file metadata and symbolic links, natural/date/size sorting, folders first in
both directions, Windows/Linux paths, software-window rejection, GL failure
cleanup, GLFW allocation failure, and emoji search/overflow behavior. Temporary
Linux tools check literal argument boundaries, cancellation, unavailable tools,
nonzero statuses and valid/invalid UTF-8 results. URL launching waits for the
Linux launcher's exit status; launchers that wait for their handler can block
for the handler's lifetime.

The Windows native shim verifies x64 structure sizes/offsets and actual UTF-16
buffer writes through P/Invoke. It checks common-dialog filters and flags,
folder PIDL allocation/free pairing, COM success (`S_OK`/`S_FALSE`), failure and
cancellation. It is a test double: it does not establish Windows OS behavior.
The fake Linux tools do not establish real desktop-tool interaction. Font
decoding/rasterization, real rendering, raw/processed whole-library builds and
target-matrix qualification belong to the integration suites. Arbitrary
non-UTF-8 Linux filename bytes are not qualified by this UTF-8 API suite.
