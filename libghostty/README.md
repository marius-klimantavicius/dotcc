# libghostty-vt translation campaign

This campaign targets Ghostty's Zig terminal engine exposed by its C API:
incremental VT input, Unicode terminal state, grid/style observation, formatting,
resize and reset. GUI and GPU rendering are excluded. The source and Zig
toolchain are pinned under `config/`.

Implementation and qualification are in progress. The native engine and the
genuine Zig-to-C whole-engine intermediate pass the same 16-case assertion
oracle. DotCC translation is still being repaired; no usable managed terminal
or JIT/NativeAOT qualification is claimed yet.

Read [the plan](docs/PLAN.md), [implementation evidence](docs/IMPLEMENTATION.md),
[upstream closure](docs/UPSTREAM.md), and [header adaptations](docs/ADAPTATIONS.md).
The authored owning API and transcript sample are pending translated-engine
validation.

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
