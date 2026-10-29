# Upstream test qualification

This report distinguishes actual upstream native assertions, assertions compiled
through Zig's genuine C backend, and recorded managed subset execution. It does
not close P4 or P6. Pins and source/toolchain provenance are in
[UPSTREAM.md](UPSTREAM.md).

## Current phase: upstream deferred on 2026-10-30

The user's 2026-10-30 scope change defers upstream testing until work on direct
Zig translation. It supersedes the earlier requirement to finish the upstream
matrix before the C-route completion commit. The `upstream-native`, `upstream`
(render), `upstream-core-native`, `upstream-core` and upstream-only `host-io`
specialists remain defined and explicitly runnable. Ordinary C-route verification
does not select these suites or prepare their profiles; its selection is in
[`verification.json`](../config/verification.json).

All 2,622 applicable case names, exclusions, 20 core partitions and prior evidence
are retained. Deferred work is not passed. The direct Zig phase must establish
the full 58-render plus 2,564-core matrix in raw/processed JIT and rooted NativeAOT,
with the native/genuine-C controls and upstream host services. Historical C-route
subset passes do not satisfy acceptance of actual direct Zig output.

The expanded hashes-warn verification was stopped gracefully with SIGINT, exit
130. Its final `upstream-core-parser-stream-02` attempt,
`20261029-234518-39b51fe2`, is retained as `cancelled`; eight earlier profile
publications and all prior evidence are retained. The
[`user-scope-stop.json`](../artifacts/p6-full-warn/user-scope-stop.json) record
confirms no recorded PIDs remain, the campaign lock is free, and all 86 protected
inputs are unchanged. Cancellation establishes no additional test passes.

The remaining C-route checks are the actual 16-case native whole-engine oracle
and two boundary controls, four consumer rows, 24 differential/physical-ABI
qualification cases in all four rows including rooted NativeAOT, four
factory-allocation rows, audit, the original root solution, local warn/off
reproduction with authored preservation, and affected shared/downstream
regressions. These retained C gates now pass; see [C-route acceptance](C-ROUTE-ACCEPTANCE.md). After the completion commit, work continues automatically with relevant true-upstream DotCC
integration and actual direct Zig translation; upstream testing resumes there.
See [the plan](PLAN.md) for the supervisor's acceptance ledger.

## Retained inventory for direct Zig qualification

The pinned scalar Debug C-API test executable reports **3,560 actual cases** via
the Zig test-server metadata protocol. Its Zig-module counterpart reports 3,035;
the C-API product is a superset, so those counts must not be added for coverage.
The source scan records 4,707 `test` blocks across all application and library
sources; this lexical count is not the executable coverage denominator.

[`pinned-inventory.json`](../tests/upstream/pinned-inventory.json) contains every
loaded C-API case name, its module, selection, reason, and the source-block scan.
[`inventory.py`](../tests/upstream/inventory.py) queries compiled test metadata
without executing or repeating its assertions.

The complete executable inventory currently partitions as follows:

| Classification | Cases | Meaning |
| --- | ---: | --- |
| Render assertions with native/C evidence | 58 | Recorded original Zig and native-compiled test C runs pass exactly the same named assertions. |
| Other ordinary applicable cases | 2,562 | Required in the direct Zig phase; outside the render profile. |
| Required corpus fixture | 1 | Existing pinned stream fuzz-regression corpus replay; fixture I/O remains necessary when resumed. |
| Required mixed-feature case | 1 | Core full reset also asserts glyph registration; the retained diagnostic profile enables that feature. |
| Excluded product components/features | 896 | Explicit GUI/fonts/input encoding/search/selection/snapshot/image/glyph/OS-service exclusions and feature-gated cases. |
| Upstream no-assertion placeholder | 1 | An unconditional skip with all assertions commented out; exact source condition is retained. |
| Declaration/root/build scaffolding | 40 | Separate from named workflow assertions. |
| Additional opt-in exhaustive campaign | 1 | `GHOSTTY_LZ4_SLOW=1` LZ4 exhaustive differential campaign; excluded by D9. |

The 2,564 core cases required when upstream testing resumes are listed in
[`core-selection.json`](../tests/upstream/core-selection.json).
Normal bounded random/generated/differential unit cases remain required:
their names containing "fuzz" or "random" are not grounds for exclusion.
For example, the `printSlice` differential unit uses 1,700 bounded rounds, and
the light LZ4 differential test is retained. The exhaustive LZ4 campaign is
explicitly opt-in in upstream source and is the sole additional-stress exclusion.

## Historical render diagnostic evidence

Upstream declares 61 named `terminal.c.render.test.*` cases. Three immediately
return `SkipZigTest` when the agreed disabled Kitty graphics feature is absent:

- `render: image iterator kitty virtual placement`
- `render: image iterator kitty fixed placement`
- `render: image iterator fixed placement above the viewport`

[`selection.json`](../tests/upstream/selection.json) records all three exact
names and reasons, discovered 61, excluded 3, and required 58. No required
terminal-core assertion is removed. Zig also includes nine anonymous root/type
scaffolding tests despite `--test-filter`; the runner selects actual named cases
by the compiler-generated `builtin.test_functions` names.

The 58 cases pass in original Zig and native-compiled C with byte-identical
stdout, zero skips, zero failures, and no live host allocations. The accepted
stdout is [`expected.txt`](../tests/upstream/expected.txt), including every case
name and the exact summary. The native hook and managed harness compare
those bytes; counts alone do not satisfy this gate.

Evidence:

- `build/native-tools/upstream-context-r3/artifacts/receipt.json`: all native/C
  compilation and execution commands passed.
- `upstream-render-original-run.log` and `upstream-render-c-run.log` in that
  artifact directory both exactly equal `expected.txt`.
- `upstream-render.json`: 58 passed, zero skipped/failed, zero live allocations.
- `build/native-tools/upstream-prepare-final/artifacts/receipt.json`: the final
  `prepare_tests(ctx)` hook builds/emits offline through a real shared Context.

The test C unit contains original upstream tests and `std.testing` assertions;
the runner never substitutes terminal behavior. This native/C evidence makes
no claim that DotCC has translated or executed the diagnostic product.

## Test source and host closure

`tests/upstream/run.py:prepare_tests(ctx)` returns the generated test `source`,
include directories, `host.c`, `case_count`, full C/native argv, and source/work
paths. It obtains `product` and `zig` through shared campaign Source acquisition,
uses `Context.run` for every tool command, and consumes profile/pin metadata.
It captures the actual upstream `test-lib-vt-build -Demit-test-exe=true` C-API
test compile command. The profile retains scalar feature gates but uses Debug:
some upstream page-list integrity tests require `slow_runtime_safety=true`.
ReleaseFast's first real test build failed that upstream comptime requirement.

The authored runner exports `dotcc_upstream_count`, `dotcc_upstream_name`, and
`dotcc_upstream_run`. It executes original function pointers from
`builtin.test_functions`, catches original test errors, preserves skip/failure
distinctions, checks the original `std.testing.SafeAllocator` for leaks, and
reports errors logged at the selected test log level. The host supplies tracked
aligned memory, CPU-count information, and reporting. It checks allocation/free
length/alignment and reports final live blocks. No native engine wrapper supplies
terminal results to the diagnostic product.

Zig's SafeAllocator references `std.Thread.getCpuCount` even in this sequential
runner. That service is unsupported by Zig's freestanding target. A private
copy of the pinned Zig library redirects only that service to an explicit host
CPU-count callback. The unique source anchor, replacement, and reason are
recorded in `toolchain.json:test_adaptations`; acquired toolchain files stay
unchanged. The allocator algorithm and all upstream tests/assertions remain
unchanged. The runner uses the documented
`root.debug.getDebugInfoAllocator` override for diagnostic allocation.

Selection data is generated from JSON as `selection.zig` beside a staged copy
of the authored runner. Complete C argv and module closure are saved in
`upstream-render/emit-test-c-command.json` and `test-closure.json`.
Native controls compile the original Zig assertions with the original compiler
library and the same host allocator; C controls compile the emitted test unit
with the staged `zig.h` and the same host. This separates C-emission behavior
from the test allocator and result transport.

## Historical broader native observations

The actual unfiltered scalar Debug `zig build test-lib-vt` completed:

| Native variant | Passed | Skipped | Failed | Total |
| --- | ---: | ---: | ---: | ---: |
| Zig module | 2,928 | 105 | 2 | 3,035 |
| C-API superset | 3,403 | 155 | 2 | 3,560 |

Both failures remain explicit in
[`native-baseline.json`](../tests/upstream/native-baseline.json):

- `lib.tinyio.test.test.dirRealPathFile edge cases`: native filesystem error
  expectation differs on this host. Native TinyIo/filesystem implementation is
  outside the freestanding terminal product.
- `terminal.stream_terminal.test.full reset`: the test unconditionally asserts
  registration of a glyph while the scalar profile disables glyph registration.
  Core reset qualification remains required in the direct Zig phase. A broader diagnostic
  feature profile can exercise this mixed test without editing its assertions;
  this failed scalar observation is preserved.

The raw log is `build/native-tools/upstream/native-tests-debug.log`. These
aggregate observations are auxiliary evidence, not a passing gate or individual
pass status for every remaining case. The failed ReleaseFast compile is retained
in `native-tests.log` beside it.

## Core workflow groups retained for direct Zig

[`groups.json`](../tests/upstream/groups.json) records exact per-module counts
and names. Ordered for the useful workflow, the 2,564 required core cases comprise:

| Workflow group | Cases |
| --- | ---: |
| C terminal API | 143 |
| Other C ABI/cell/grid/support APIs | 99 |
| Formatters, C and implementation | 138 |
| Parser/sequence decoding and stream execution | 675 |
| Terminal and screen-set state transitions | 464 |
| Screen/page storage and compression | 646 |
| Unicode implementation and C queries | 16 |
| Internal logical render state | 31 |
| Modes/styles/colors and extension protocol state | 235 |
| Supporting algorithms and scalar text helpers | 117 |

These counts include the mixed full-reset case and corpus fixture exactly once.
The 58 accepted render assertions are additional to this table.

The original combined control discovered 2,573 cases: 2,564 passed and nine
skipped with zero failures or live allocations. Exact skip names, source lines
and conditions are retained in `core-native-baseline.json`: eight concern
excluded product/OS capabilities and one has no active upstream assertions.
The generated 455 MB C control compiled and ran all discovered cases, exposing
four missing entropy services and one additional unhosted C allocator skip;
these attempted results remain distinct from acceptance. Real secure entropy,
read-only fixture I/O and hosted allocator capability now have focused controls.
The final monolithic retry was cancelled when initial product delivery became
the priority; no completed zero-skip monolithic result is claimed.

`core-partitions.json` now declares 20 bounded diagnostic products whose exact
union is all 2,564 required cases, with no duplicates or omissions. Nineteen
products retain the delivered scalar features and contain at most 192 cases;
one isolates the unchanged full-reset glyph assertion with the additional
diagnostic glyph feature. When upstream qualification resumes, every partition
must independently pass original native, genuine C and raw/processed
JIT/rooted-AOT assertions. This resource partitioning does not reduce required
scope or change the upstream assertions.
The original complete manifest, discovered exclusions and candidate transcript
remain available. Complete native/C/managed acceptance across all partitions is
deferred; the historical subset results below do not close it.

The real pinned corpus stays required. Test services supply tracked aligned and
zeroed page memory, read-only directory/positioned binary reads, CPU count and
secure entropy; original allocators/assertions remain in the test products.
Toolchain/source service adaptations use guarded private copies only. Compiler
full-name filters are substring matches, so exact runtime selection and the
per-case transcript/count gate determine acceptance; extra compiled scaffolding
is never counted as a required assertion pass.

## Historical managed subsets and compiler attempt

The 2026-10-29 exploratory C-route matrices passed 427 core cases and 58 render
cases in all four managed rows: C terminal 143 (`191842-a755c4a3`), Unicode 16
(`193053-eb32999b`), supporting C 99 (`194748-b647912c`), formatters 138
(`202222-30921070`), logical render 31 (`203651-4886d22a`) and render 58
(`204410-28a880c5`). These are 485 distinct cases, a subset of the retained 2,622.
Recorded native/C controls cover 811 of the 2,564 core cases. These partial
results do not establish a complete upstream pass or final-tool regeneration of
every partition, and the cancelled expanded rerun adds no accepted result.

Earlier full-compiler attempt `20261029-182910-7a8cf18a` advanced past the
packed-record failure and reached valid declaration-specifier ordering at core
C line 16118: `_Alignas(128) static struct ...`. Its frozen command, input and
log remain in that receipt. This is historical failure evidence; subsequent
frontend repairs and the subset results above supersede it as the progress
description. No managed case pass is claimed from the failed attempt. Native/C
controls from that period are historical evidence, with outcomes recorded in
their own receipts.

## Retained specialist commands for deferred work

The recipe declares `upstream-native` for native/C controls and `upstream` for
the managed matrix using the `upstream-render` diagnostic product:

```bash
bash libghostty/scripts/test.sh --suite host-io --fetch never
bash libghostty/scripts/test.sh --suite upstream-native --fetch never
bash libghostty/scripts/translate.sh --profile upstream-render --fetch never
bash libghostty/scripts/test.sh --suite upstream --form all --mode all --rid linux-x64
```

These commands reproduce the retained C-route controls and diagnostics; commands
alone are not passing results. Actual direct Zig output must be qualified when
that phase resumes. All tool output and exit status remain in campaign receipts.
No shared DotCC compiler bins are written by these native hooks.


Core profiles have names declared by `core-partitions.json`. The first bounded
profile can be controlled and qualified independently through normal helpers:

```bash
bash libghostty/scripts/test.sh --profile upstream-core-c-terminal-01 --suite upstream-core-native --fetch never
bash libghostty/scripts/translate.sh --profile upstream-core-c-terminal-01 --fetch never
bash libghostty/scripts/test.sh --profile upstream-core-c-terminal-01 --suite upstream-core --form all --mode all --rid linux-x64
```

The `upstream-core` suite retains all 20 profiles in its `verify_profiles`
metadata, with exact per-profile names and case lists in
[`core-partitions.json`](../tests/upstream/core-partitions.json). Full replay must
cover every partition and check the exact union; the example above covers only
the first profile. Ordinary C-route verification omits upstream prerequisites.
A single-profile result cannot close the deferred upstream acceptance. All four
execution rows remain required for all 2,622 applicable cases in the direct Zig
phase (58 render plus 2,564 core).
