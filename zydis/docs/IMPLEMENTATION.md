# Zydis implementation ledger

Campaign started 2026-10-27 with baseline v4 and full P0–P6 authorization.
Initial branch: `sqlite`. Preexisting workspace status/HEAD/diff captured under
`/tmp/zydis-campaign-baseline-20261027/`; unrelated edits are not campaign work.

## Delegation and builds

Live T3 catalog verified supervisor `codex/gpt-6.1-sol/high`. Three shared-checkout
children launched with explicit `reasoningEffort=xhigh`: source/recipe,
native/qualification, and owning API/consumer. Their returned model/options match.
Managed builds are supervisor-owned; native controls use private outputs.

## Attempts and defects

### ZY-001: inferred outer dimension of struct/union arrays (compiler)

First full 18-unit diagnostic emission used the Release CLI with C11, static
build macros, explicit x64 architecture, standard libc profile and all upstream
`src/*.c`; output/logs are under `artifacts/supervisor-probe/`. It failed with
`inferred multidimensional arrays of struct/union elements are not supported`.
Zydis `Generated/DecoderTables.inc` uses `ZydisDecoderTreeNode FILTERS_*[][N]`.

Three focused reduced tests failed before the repair (`inferred-before.log`).
Native C11 execution of the functional fixture establishes row extents,
per-row zero fill, unions, nested dimensions, designators, local/static/global
storage and aggregate-expression copies. The repair removes the element-kind
restriction in `InferOuterArrayDimensions`: array extent inference is independent
of element kind; existing target-directed aggregate lowering handles rows.
Focused/affected unit tests now pass 17/17 (`inferred-after.log`). Functional
execution and full-source retry are pending. No generated output was edited.

### Baseline test finding (unrelated)

Shared Python framework tests: 89 passed, 1 failed, 90 total. The failure is the
existing metadata-literal policy assertion on
`blink/tests/GuestCoreClrExecution/run-getcpu.py`; its embedded digest predates
this campaign. Log: `/tmp/zydis-campaign-baseline-20261027/framework-tests.log`.
This is recorded separately and no Blink edits were made.

## Milestone commits

`b2c10fae1` — shared compiler inferred outer-bound repair, 17 focused unit tests,
2 runtime fixtures and subsequent 3489/3489 compiler unit cases.

## P0 native/source baseline and first useful execution

Pins: Zydis v4.1.1 and its exact Zycore gitlink. Source closure: all 18 library
units, headers-only required Zycore product dependency (10 headers), 636 pinned
regular-file inventory entries including tests/licenses. Recipe acquisition,
layout, relocation, offline warn/off/strict and script checks are recorded in
`config/recipe-checks.json`. Tests additionally compile original Zycore Terminal.c.

Native control receipt/report:
`build/native-agent/controls-05/logs/receipt.json` and
`build/native-agent/controls-05/logs/zydis-native-report.json`.
393 passed, 0 failed/skipped: eight ABI/feature control groups and 385 upstream
cases (210 info snapshots, 52 encoder, 122 re-encoder, original absolute suite).
Deliberate assertion failure returns 1. Original rejecting CLI cases are checked
separately, including status/stderr; native harness mistakes (immutable argv and
assuming every info fixture succeeds) were corrected without upstream changes.

Raw authored API now builds. `owning-consumer-first.log` records useful actual
translated execution: decode 16/32/64, Intel/AT&T, metadata/operands,
encode/mutate/roundtrip, addresses, NOPs, errors, owning snapshots/concurrency.
This is private first-execution evidence, not canonical matrix acceptance.

Second companion attempt `20261027-070328-dbe36a45` emits all core units plus
original C test tools and a separate qualification adapter, links both products
and has reached normal postprocessing. Explicit `__linux` was added after
Terminal.c's unsupported-platform preprocessing diagnostic in the preceding
attempt; no source adaptation. Final regeneration after test harness freeze
remains required.

ZY-001 downstream validation: new and existing aggregate-dimension execution
fixtures pass 2/2; full compiler unit suite passes 3489/3489. Full canonical
source retry `20261027-065728-f4b894b4` emits/links all 18 units and reaches raw
build. Private core-only project builds the identical emitted C# with 0 errors
(2 existing shared-runtime unused-field warnings). Integrated raw build currently
has authored API naming errors, handled by the API agent against actual output.
Shared compiler repair milestone: `b2c10fae1`.

Canonical initial attempt `20261027-065712-85121b5e` failed source preprocessing
because Zycore needs an architecture macro. Configuration now explicitly selects
`__x86_64__` once, shared with the native configuration. No upstream text patch.
Inventory suite and canonical layout pass; inventory receipt:
`artifacts/campaign/20261027-065942-193fb9e8/receipt.json`.
