# Upstream test qualification

This report distinguishes actual upstream native assertions, assertions compiled
through Zig's genuine C backend, and future managed execution. It does not close
P4 or P6. Pins and source/toolchain provenance are in [UPSTREAM.md](UPSTREAM.md).

## Inventory and current acceptance

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
| Accepted render assertions | 58 | Original Zig and native-compiled test C pass exactly the same named assertions. |
| Other ordinary applicable cases | 2,571 | Required; not accepted by the render profile. |
| Required corpus fixture | 1 | Existing pinned stream fuzz-regression corpus replay; fixture I/O remains necessary. |
| Required mixed-feature case | 1 | Core full reset test also asserts glyph registration; diagnostic feature resolution is required. |
| Excluded product components/features | 888 | Explicit GUI/fonts/input encoding/search/selection/snapshot/image/glyph/OS-service exclusions and feature-gated cases. |
| Declaration/root/build scaffolding | 40 | Separate from named workflow assertions. |
| Additional opt-in exhaustive campaign | 1 | `GHOSTTY_LZ4_SLOW=1` LZ4 exhaustive differential campaign; excluded by D9. |

The 2,573 remaining required cases are listed individually in
[`core-selection.json`](../tests/upstream/core-selection.json).
Normal bounded random/generated/differential unit cases remain required:
their names containing "fuzz" or "random" are not grounds for exclusion.
For example, the `printSlice` differential unit uses 1,700 bounded rounds, and
the light LZ4 differential test is retained. The exhaustive LZ4 campaign is
explicitly opt-in in upstream source and is the sole additional-stress exclusion.

## Accepted render diagnostic

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
name and the exact summary. The native hook and future managed harness compare
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

## Broader native observations

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
  Core reset qualification remains required. An explicitly broader diagnostic
  feature profile can exercise this mixed test without editing its assertions;
  this failed scalar observation is preserved.

The raw log is `build/native-tools/upstream/native-tests-debug.log`. These
aggregate observations are auxiliary evidence, not a passing gate or individual
pass status for every remaining case. The failed ReleaseFast compile is retained
in `native-tests.log` beside it.

## Remaining required workflow groups

[`groups.json`](../tests/upstream/groups.json) records exact per-module counts
and names. Ordered for the useful workflow, the 2,573 remaining cases comprise:

| Workflow group | Cases |
| --- | ---: |
| C terminal API | 143 |
| Other C ABI/cell/grid/support APIs | 99 |
| Formatters, C and implementation | 138 |
| Parser/sequence decoding and stream execution | 678 |
| Terminal and screen-set state transitions | 464 |
| Screen/page storage and compression | 647 |
| Unicode implementation and C queries | 16 |
| Internal logical render state | 31 |
| Modes/styles/colors and extension protocol state | 239 |
| Supporting algorithms and scalar text helpers | 118 |

These counts include the mixed full-reset case and corpus fixture exactly once.
The 58 accepted render assertions are additional to this table.

A single combined `upstream-core` diagnostic product is under investigation,
rather than a fleet of parallel test products. Its selected case names come
from the inventory. It must retain every ordinary applicable assertion and
allocator check. Enabling glyph registration only in that diagnostic profile
can resolve the mixed reset case while scalar delivery remains unchanged.
The unchanged core tests also need a deliberate `std.testing.io` host boundary;
the corpus replay needs its real pinned fixture inputs and an actual read service.
These are unresolved feasibility requirements, not reasons to silently exclude
required tests. Broad native/C and managed execution are not claimed yet.

If staged integration needs smaller diagnostic passes, useful filters are C
terminal/cell/grid API first, then parser/stream, page/screen state, formatters,
and Unicode/support algorithms. Those are implementation/debugging partitions;
they do not replace complete required acceptance.

## Commands

The recipe declares `upstream-native` for native/C controls and `upstream` for
the managed matrix using the `upstream-render` diagnostic product:

```bash
bash libghostty/scripts/test.sh --suite upstream-native --fetch never
bash libghostty/scripts/translate.sh --profile upstream-render --fetch never
bash libghostty/scripts/test.sh --suite upstream --form all --mode all --rid linux-x64
```

Managed commands above are reproduction targets, not passed results in this
report. All tool output and exit status remain in shared campaign receipts.
No shared DotCC compiler bins are written by these native hooks.
