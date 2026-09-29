# Active implementation coordination

The user authorized implementation and coordinator/sub-agent delegation on
2026-09-29. Coordinators must keep required incomplete plan work open. The parent
coordinator reviews integration and owns commits; workers do not stage or commit.

A separate lwIP session uses this workspace. Do not edit, stage, revert, clean, or
otherwise absorb `lwip/` or unrelated changes. Source/reference/output cleanup is
limited to each campaign's own declared files. Shared compiler edits require
explicit cross-session coordination and inspection of existing diffs.

| Owner | Paths/responsibility |
| --- | --- |
| libuv coordinator | Integration, plan acceptance, review, milestone commits and next worker assignments. |
| campaign worker | `libuv/src/Managed.Libuv.Native`, `libuv/tests/dispatch`, `libuv/config`, `libuv/scripts`, `libuv/tests/native`, `libuv/tests/abi/linux-layout.c`, acquired reference, README, licenses, documentation and evidence. |
| compiler/native worker | `libuv/src`, managed consumer, native boundary and translation probes under `libuv/tests/abi` / `libuv/tests/probes`; compiler changes below. |

Compiler/native worker owns these shared paths for current focused fixes:

- `DotCC.Lib/CPreprocessingOptions.Functions.cs`
- `DotCC.Lib/Ir/IrBuilder.MallocPromote.cs`
- `DotCC.Lib/Backends/CSharpBackend.cs`
- `DotCC.FunctionalTests/ManagedLibraryTests.AllocatorOverrides.cs`
- `DotCC.FunctionalTests/ManagedLibraryTests.NativeHeaderAttributes.cs`
- `DotCC.Lib/c.lalr.yaml`
- `DotCC.Lib/Ir/IrBuilder.cs`
- `DotCC.Tests/AttributeTests.cs`
- Related verified behavior documentation in `docs/function-overrides.md` and
  `docs/C-SUPPORT.md` (inspect preexisting diffs first).

Compiler tests/builds use `--artifacts-path libuv/build/compiler` to isolate outputs.
Campaign/native work does not launch shared DotCC rebuilds. Native CMake outputs
are `libuv/build/native-baseline` and `libuv/build/native-upstream`. Campaign probes
must use a coordinator-approved compiler snapshot or isolated build. The ordinary
shared runner's `--tools build` must be coordinated before use.

Both workers may update this coordination document with their exact owned paths.
