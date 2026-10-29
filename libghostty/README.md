# libghostty-vt translation campaign

This campaign targets Ghostty's Zig terminal engine exposed by its C API:
incremental VT input, Unicode terminal state, grid/style observation, formatting,
resize and reset. GUI and GPU rendering are excluded. The source and Zig
toolchain are pinned under `config/`.

The C route is accepted for the selected Linux x64 terminal-core scope with the
explicit upstream-test deferral below. Current native controls, consumer and
24-case differential/physical-ABI qualification in all four rows, factory checks,
audit, root solution and warn/off reproduction pass. See
[C-route acceptance and exact evidence](docs/C-ROUTE-ACCEPTANCE.md).
Direct Zig translation is the next phase and is not yet qualified.

On 2026-10-30, the user deferred upstream test suites, their 2,622-case matrix
and the upstream-only host-I/O probe to the direct Zig phase. Their inventories,
20 core partitions, specialist commands and historical subset results remain
available in [upstream test qualification](docs/UPSTREAM-TESTS.md). Deferred work
is not counted as passed; the stopped expanded verification remains cancelled.

The accepted C gates include the original root solution, local warn/off
reproduction with preservation and affected shared/downstream regressions.
After the completion commit, work continues automatically
with relevant true-upstream DotCC integration and actual direct Zig translation.
The full upstream matrix is required again for that phase.

Read [the plan](docs/PLAN.md), [implementation evidence](docs/IMPLEMENTATION.md),
[upstream closure](docs/UPSTREAM.md), and [header adaptations](docs/ADAPTATIONS.md).
The authored owning API and transcript sample run against the translated
engine through ordinary project references.

```bash
bash libghostty/scripts/fetch.sh
bash libghostty/scripts/test.sh --suite native --fetch never
bash libghostty/scripts/translate.sh --fetch never
```

Python 3.11+, .NET 10 and the pinned Zig toolchain are required. Native controls
also require a GNU-compatible C compiler and libm on Linux x64. Source acquisition
uses the shared framework. Zig acquires its pinned build dependencies on the
first online native preparation; subsequent `--fetch never` uses Zig's offline
package mode. Fetch and NuGet restore are independent (`--restore` controls the
latter). Existing usable source trees are preserved under default `warn` and
`off` provenance policies.

The generated entry class is `Managed.GhosttyVt`; the owning `Terminal` API is
also in namespace `Managed`. `--aggregate-data` is enabled during object
emission, with the shared literal pool retained at link time. See
[output options and measured sizes](docs/OUTPUT-OPTIONS.md). Constant expression
factory helpers have at most two parameters. Their immutable getters and
zero-argument factories pass the allocation checks documented in
[FactoryAllocation](tests/FactoryAllocation/RESULTS.md).
