# Deliver the translated Blink library

From the repository root on Linux x64, with .NET 10, Python 3, GCC/binutils,
make and NativeAOT prerequisites installed:

```sh
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh
```

Use `--offline` once the manifest-pinned Blink source archive is cached in
`blink/ref`. This restricts Blink fetching; normal NuGet restore may still use
installed packages or configured package feeds. `--jobs 1..4` controls source
emission concurrency; `--timeout <seconds>` bounds each translation unit.
The script resolves repository paths itself, so it can be invoked from another
working directory. Its compiler and postprocessor must already be built; it
never rebuilds those shared tools during translation.

The stable result is:

```text
blink/generated/TranslatedBlink/
  TranslatedBlink.csproj
  Sources/                         # compiler-emitted, then postprocessed C#
  Bridges/                         # selected authored C-to-host bindings
  Host/Managed.Emulation.Host.csproj
```

`TranslatedBlink` is the assembly name. The translated type is
`Managed.Emulation.BlinkCore`; generated C records are nested as selected by
`--nest-types`. The compiler's actual managed-library link uses
`--runtime=c --split=size --split-size=102400`. No generated C# is manually
modified. A separate authored project file supplies relative build references.

An external application references `TranslatedBlink.csproj`. Host types in
`Managed.Emulation.Host` arrive through its copied Host project reference;
do not add a second reference to the source-tree Host project with the same
assembly name. The resulting source bundle can be copied elsewhere and built
with .NET 10. The generated API remains unsafe and requires the existing host
binding/lifetime contracts. The current normal-only `BlinkCore.main()` frontend
runs once in a fresh process; it is not a service API or a worker restart promise.
The owning usage sample/solution is documented separately.

Each invocation performs pinned fetch verification, the existing pinned native
assembly tests, a new frozen core profile, identity-checked complete closure
emission/link, and a delivery link with size splitting. Object reuse is allowed
only when the existing assembler verifies full emission identity and object
hash; producing profiles and receipts are retained explicitly. It never treats
an older compiler's objects as compatible merely because a filename matches.

The whole raw source/project/host bundle is preserved separately under
`blink/generated/translation-delivery/attempt-*/raw/`. Raw files/directories
are read-only and their hashes are checked after processing. To inspect a raw
build, copy that bundle to a writable scratch directory; the script never runs
MSBuild or the postprocessor inside the raw snapshot. A separate candidate is
restored and built, processed by the existing semantic postprocessor, and built
again. These builds do not execute guest code or custom failure tests.

Only a validated postprocessed source bundle replaces the stable directory.
Old output is archived under the new attempt's `previous-output/`; failure in
the directory replacement restores it. A single translation lock serializes
publishers. The two directory renames are not a cross-process atomic transaction;
finish active consumer builds before regenerating the stable path. Build caches
are removed from the delivered bundle so temporary absolute build paths cannot
masquerade as published artifacts.

`blink/artifacts/translation/attempt-*/receipt.json` records compiler,
postprocessor, script, profile, object, raw/final source and build-log identities.
`latest.json` points to the last successful delivery. After this publication
commits, interruption of stdout reporting cannot rewrite its successful receipt.
Failed attempts before publication retain
logs and `passed: false`; the source archive and older snapshots are preserved.
Each attempt has a dedicated temporary directory. Timed-out command groups are
terminated and drained. SIGTERM is forwarded through that cleanup path; an outer
runner must allow more than its three-second child grace before escalating.
This does not promise bounded cleanup of kernel-unkillable processes.

This gate establishes translation, semantic postprocessing and buildability of
the selected core. Runtime qualification, service startup, sample lifecycle,
platform coverage and indirect/framework boundary limitations remain separate.
Current test scope excludes custom fault injection and malformed-ELF tests;
only existing pinned upstream tests and ordinary functional execution belong
in reproduction commands.

## Authored source links and execution ownership

The active `generated/TranslatedBlink` project contains generated C# sources and
links original bridge files from `src` with parent-relative Compile items and
IDE Link metadata. Its Host ProjectReference points to the original
`src/Managed.Emulation.Host` project. No copied Host or Bridges directory is an
active product input. Edit authored sources in `src` and rebuild the solution;
regeneration preserves those edits. Immutable raw/profile copies remain archival
inputs for qualification. Delivery records the original source closure and
checks its hashes before and after generation and the final direct-source build.

Semantic postprocessing runs against private frozen copies for compilation
context. Only transformed generated sources are retained; private authored
copies are restored for validation and discarded from the active delivery.
The published project is built again at its final path against original `src`
files. Failed final builds roll the generated product back without writing to
those authored files.

The product excludes `CoreProbe`, its test `main`, and C execution orchestration.
A separate authored C# consumer uses the exported upstream functions/types and
jump-buffer transport. The test runner may add a probe frontend in a derived
test-only link with its own receipt. Canonical and delivery links enable
`--literal-pool`; this is a compiler link option, not a postprocessor feature.
