# Qualification integration contract

Owner: tests agent. This file coordinates the shared checkout; it is not a claim
that a suite has passed. No shared managed/tool builds are run by this agent.

## Pending source inspection

The upstream acquisition/recipe agent owns the source pin and effective feature
configuration. Tests will use the recipe's actual translation flags and units
for the native oracle, with a native compiler replacing the DotCC driver. The
oracle must never silently default to a separately configured Zydis build.

Planned authored source: `tests/qualification.c` (and its header), compiled as a
real translation `Unit` for qualification. It includes the actual upstream public
headers, inspects actual types with `sizeof`/`_Alignof`/`offsetof`, and calls the
selected upstream APIs. It supplies no decoder, encoder, formatter, or utility
replacement. Keep this test adapter outside the delivered library, using a test
companion product if needed; do not add a CLI to the product.

Proposed managed test driver: `tests/Qualification/Qualification.csproj`, using
the `TranslatedProject` property. It calls the translated C adapter entrypoints
directly and emits deterministic transcripts. Public API tests can additionally
exercise `Managed.Zydis.Zydis`, but the native differential must reach the actual
translated C probes. Driver details will be finalized after source inspection.

## Recipe coordination

Source inspection is now available: upstream has 210 `tests/cases/*.in/.out`
decoder/formatter snapshots, 52 encoder JSON cases, 122 re-encoder JSON cases,
and `tools/ZydisTestEncoderAbsolute.c`. These all apply to the selected full
library and must run. The original upstream tools will be **test-only Units**,
with renamed entrypoints; they are never product Units. Fixture JSON is emitted
as C initializers against upstream types, avoiding Python/managed mirror layouts.

Concrete companion proposal (parent/recipe agent please review): independently
link the identical product Units plus the following test Units as class
`Managed.Zydis.Tests.ZydisQualification` in `TranslatedZydisQualification`:

| Unit | Extra defines |
| --- | --- |
| `tests/qualification.c` | generated fixture-header include directory |
| upstream `tools/ZydisInfo.c` | `main=ZqInfoMain` |
| upstream `tools/ZydisToolsShared.c` | none |
| Zycore `src/API/Terminal.c` | test-only original CLI color-detection dependency; `NO_COLOR=1` at execution |
| upstream `tools/ZydisFuzzShared.c` | `main=ZqUnusedFuzzMain`, `ZydisFuzzTarget=ZqFuzzEncode` |
| upstream `tools/ZydisFuzzEncoder.c` | `ZydisFuzzTarget=ZqFuzzEncode` |
| upstream `tools/ZydisFuzzReEncoding.c` | `ZydisFuzzTarget=ZqFuzzReencode` |
| upstream `tools/ZydisTestEncoderAbsolute.c` | `main=ZqAbsoluteMain` |

Exports: `int ZqRun(unsigned group)` writes deterministic transcripts. Groups
0=ABI, 1=authored functional/error/callback controls, 2=all upstream info cases,
3=all upstream encoder/re-encoder cases, 4=original absolute encoder tests,
5=deliberate assertion-failure control (must return 1). Nonzero means failure.
No `main` is translated. A native-only `tests/native_main.c` dispatches `ZqRun`.

`tests/harness.py` will expose `test_units(ctx, source)` which generates fixture
headers under `ctx.work` and returns `Unit` objects with their individual flags.

`scripts/qualification.py` now exports the concrete `companions(ctx)` callback.
Set it as `Translation.companions` in the recipe. It emits only test Units and
links them with the exact current `ctx.work/objects/NNN-<stem>.cs` product objects,
then returns `{ "TranslatedZydisQualification": raw_directory }` for the shared
framework's processing/build/publication. It must be invoked after core object
emission. It performs no shared tool builds itself. The managed test project
calls only `Managed.Zydis.Tests.ZydisQualification.ZqRun(group)` and does not
need trailing-underscore type names. The helper implements `native`, `abi`,
`qualification`, `upstream`, `audit` and an optional `rooted-aot` suite.

Native control command (uses `ctx.run` logs, max two compiler jobs):

```bash
python3 -B zydis/scripts/qualification.py --native --work zydis/build/native-agent/controls-02
```

`scripts/qualification.py` will export `run(ctx, suite)` for `Suite.run` and use
`ctx.run` / `ctx.managed` for logged commands. Anticipated suite names are
`native`, `abi`, `qualification`, and `upstream`. The last three need matrix
execution for raw/processed and JIT/NativeAOT. A native-only entrypoint will allow
the tests agent to validate controls in private `build/native-agent/` paths.

Upstream fixture inventory must retain each discovered case, selected runner,
exact status, and any explicit exclusion reason. Native crashes, missing
fixtures, malformed input, and unimplemented required managed runners are hard
failures, never expected oracle results or successful skips.

## Parent integration needs

- Confirm source location and effective flags via the actual recipe, including
  Zycore headers, feature defines, and complete selected source closure.
- Review/finalize the test companion interface and suite names before wiring.
- Parent owns all managed builds and combined raw/processed JIT/AOT execution.
- Test agent will update this file with concrete entrypoints and current native
  evidence after upstream files arrive.
