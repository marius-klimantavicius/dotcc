# libghostty-vt translation campaign

This campaign targets Ghostty's Zig terminal engine exposed by its C API:
incremental VT input, Unicode terminal state, grid/style observation, formatting,
resize and reset. GUI and GPU rendering are excluded. The source and Zig
toolchain are pinned under `config/`.

The default product is official **libghostty-vt 1.0.0**, translated directly from
Zig to C# by DotCC. It runs the selected terminal workflow. See the
[upgrade evidence and reproduction](docs/VT-1.0.0.md). Its canonical projects are
[processed](generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj) and
[raw](generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj).
Both forms pass the owning consumer, 45 differential/version/API cases and
allocation checks under JIT and whole-library-rooted NativeAOT. Full upstream
managed qualification remains pending; the old pin's 2,622-case native and
287-case managed results are not 1.0.0 acceptance. See
[direct status and reproduction](docs/DIRECT-ZIG.md).

Build and run the ordinary owning solution; no project override is needed:

```bash
mkdir -p /mnt/storage/dotcc-libghostty-disposable/direct-sample/tmp
TMPDIR=/mnt/storage/dotcc-libghostty-disposable/direct-sample/tmp \
dotnet build libghostty/ManagedConsumer.slnx -c Release \
  -p:PublishAot=false \
  -p:UseArtifactsOutput=true \
  -p:ArtifactsPath=/mnt/storage/dotcc-libghostty-disposable/direct-sample
dotnet /mnt/storage/dotcc-libghostty-disposable/direct-sample/bin/ManagedConsumer/release/GhosttyVt.Consumer.dll
```

The historical 0.1.0 C route was accepted for the selected Linux x64 terminal-core scope with the
explicit upstream-test deferral below. Current native controls, consumer and
24-case differential/physical-ABI qualification in all four rows, factory checks,
audit, root solution and warn/off reproduction pass. See
[C-route acceptance and exact evidence](docs/C-ROUTE-ACCEPTANCE.md).
Its accepted source products are preserved under `generated/c-route/` as an
explicit fallback. The ordinary reference chain is `ManagedConsumer` →
`Managed.GhosttyVt` → `generated/TranslatedGhosttyVt` (direct Zig).
An explicit `TranslatedProject` override can still select the C fallback.

On 2026-10-30, the user deferred upstream test suites, their 2,622-case matrix
and the upstream-only host-I/O probe to the direct Zig phase. Their inventories,
20 core partitions, specialist commands and historical subset results remain
available in [upstream test qualification](docs/UPSTREAM-TESTS.md). Deferred work
is not counted as passed; the stopped expanded verification remains cancelled.

The accepted C gates include the original root solution, local warn/off
reproduction with preservation and affected shared/downstream regressions.
After the completion commit, work continues automatically
with relevant true-upstream DotCC integration and actual direct Zig translation.
The full upstream managed matrix remains required for final direct acceptance.

Read [the plan](docs/PLAN.md), [implementation evidence](docs/IMPLEMENTATION.md),
[upstream closure](docs/UPSTREAM.md), and [header adaptations](docs/ADAPTATIONS.md).
The authored owning API and transcript sample run against the translated
engine through ordinary project references.

```bash
bash libghostty/scripts/fetch.sh
bash libghostty/scripts/test.sh --profile scalar --suite native --fetch never
bash libghostty/scripts/translate.sh --fetch never
```

Normal `translate.sh` regeneration selects the `direct` profile and publishes
the canonical raw/processed paths. The accepted C translator remains available
with `--profile scalar`, which publishes to a nondefault profile directory.
Default direct `verify.sh` currently checks the owning consumer; it does not
claim the pending full upstream qualification. Campaign intermediates stay under
`/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/campaign/`.

Python 3.11+, .NET 10 and the pinned Zig toolchain are required. Native controls
also require a GNU-compatible C compiler and libm on Linux x64. Source acquisition
uses the shared framework and official signed source/dependency archives.
The matching five-package bundle supplies Zig's offline `--system` package
layout; `--fetch never` requires those pinned local inputs. Fetch and NuGet restore are independent (`--restore` controls the
latter). Existing usable source trees are preserved under default `warn` and
`off` provenance policies.

The generated entry class is `Managed.GhosttyVt`; the owning `Terminal` API is
also in namespace `Managed`. `--aggregate-data` is enabled during object
emission, with the shared literal pool retained at link time. See
[output options and measured sizes](docs/OUTPUT-OPTIONS.md). Constant expression
factory helpers have at most two parameters. Their immutable getters and
zero-argument factories pass the allocation checks documented in
[FactoryAllocation](tests/FactoryAllocation/RESULTS.md).
