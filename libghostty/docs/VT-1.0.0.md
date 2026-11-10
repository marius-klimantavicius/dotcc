# libghostty-vt 1.0.0 direct upgrade

Follow-up, 2026-11-10: the previously excluded Zig changes are now completed
and validated. See [the source/test inventory and remaining limitations](ZIG-FOLLOWUP-20261110.md).
The original upgrade receipts and canonical products described below are unchanged.

Published as the canonical direct default on 2026-11-07. The actual selected
engine emits, links, builds and runs in raw/processed JIT and whole-library-rooted
NativeAOT. All 45 native differential/version/API cases pass in every row;
owning-consumer, allocation and delivery checks pass separately. Normal default
regeneration publishes the conventional raw/processed projects. The ordinary
`ManagedConsumer.slnx` builds with zero warnings/errors and its owning consumer
passes with no `TranslatedProject` override. The previous working 0.1.0 milestone
is retained at `51b1be1f59bb351806e724ed58b712f0bdb8e091`.

The source is the official `vt-1.0.0` release tarball, with independent library
version `1.0.0`, tag object `6b38d55f04e66ebecc472d8be19946edcc6d2e9d`, and
commit `0fa3d577a5614150ff8e2c152de5fb9d5c6d2dec`. The official source and offline
dependency bundle match their signed SHA256SUMS; both content and trusted-comment
signatures were verified with the official minisign key. The source archive
contains `LIB_VERSION`, unlike an ordinary repository archive. Zig remains
0.17.0. See [provenance](../artifacts/vt-1.0.0/provenance.json),
[source pin](../config/source.json), and [upstream closure](UPSTREAM.md).

The product remains scalar Linux x64 LP64 with formatter, render-state and grid
introspection. DotCC translates the actual `src/lib_vt.zig` and its eleven-module
graph directly with Lib/fast/source settings and managed function pointers. The
authored C boundary is translated separately; no C engine intermediate or native
Ghostty engine is used by managed execution. The selected entry inventory grows
from 92 to 96: formatter set/get, version and sized initialization are added.
Unselected GUI, input encoding, search, snapshot and image features remain outside
this product.

The owning `Managed.Terminal` API remains stable. Its adapter uses the release's
new formatter set/get options, packed style flags and current sized records.
The released low-level surface has seven changed declarations, including pointer
SGR/size-report arguments and `GhosttyResult` returns. Their assessment and
native controls are under [boundary evidence](../artifacts/vt-1.0.0/boundary).

Nineteen documented source compatibility patches retain the upstream engine.
Patches 0001–0014 port the accepted adaptations; 0015 moves standalone ABI JSON
validation into its existing lazy manifest initializer, 0016 makes existing
sized validation explicitly comptime, and 0017–0018 bound incidental vector
optimizations/use the existing scalar store path for the scalar product. Patch
0019 derives the same reserved-style mask from actual field layout instead of a
comptime pointer reinterpretation. Native
parity includes 2,940 real-engine cases around print-run boundaries. See the
[patch rationale](../patches/vt-1.0.0/README.md) and
[parity receipt](../artifacts/vt-1.0.0/patches/print-batches/parity.json).

Compiler changes address reached release semantics: enum-value type methods,
nonexhaustive enum dispatch, reflected mixed loops and arrays, module constants,
dependent comptime string parameters, explicit comptime type comparisons,
full-width vector OR reduction and wide untyped integer peers. The compiler is
built from the accepted baseline plus these changes in one small clean export;
unrelated runtime edits and earlier unvalidated loop/keys/metadata/optional
experiments are excluded. Old pending `patches/direct/0016` is not part of the
release patch chain.

Passed upgrade checks cover actual raw/processed emission and build, owning
terminal behavior, 45 native differential/version/API cases in both JIT and
whole-library-rooted NativeAOT, aggregate RVA/bounded-helper/allocation checks,
delivery audit, and the ordinary `ManagedConsumer.slnx` build and run without a
`TranslatedProject` override. The final [delivery receipt](../artifacts/vt-1.0.0/DELIVERY.json) links these checks.
Detailed receipts live under
[`artifacts/vt-1.0.0`](../artifacts/vt-1.0.0).

Full upstream managed qualification remains pending. Previous 287-case managed
and 2,622-case native results belong to the old source pin and are not 1.0.0
acceptance. Upgrading the useful product does not claim that matrix passed.

All disposable tools, caches, native controls, objects, managed builds and scratch
are under `/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0`. Durable source pins,
patches and compact commands/receipts stay in this repository. Accepted C-route
history and the previous working direct milestone are preserved.

Rebuild and run the ordinary published consumer without an override:

```sh
TMPDIR=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0 \
dotnet build libghostty/ManagedConsumer.slnx -c Release \
  -p:PublishAot=false -p:UseArtifactsOutput=true \
  -p:ArtifactsPath=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/default-check

dotnet /mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/default-check/bin/ManagedConsumer/release/GhosttyVt.Consumer.dll
```

[`reproduce-private.sh`](../artifacts/vt-1.0.0/reproduce-private.sh) reconstructs
the qualified compiler from the accepted milestone plus a compact patch, fetches
the pinned release inputs and genuinely regenerates private raw/processed sources.
This keeps preexisting unvalidated working-tree compiler/runtime experiments out
of the selected tool. The actual normal default publication command was:

```sh
DOTCC_COMPILER=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/compiler-build/bin/DotCC/release/dotcc.dll \
DOTCC_POSTPROCESSOR=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/postprocess-build/bin/DotCC.PostProcess/release/dotcc-postprocess.dll \
bash libghostty/scripts/translate.sh --fetch never --tools reuse
```

The profile, generated paths and owning references use their ordinary defaults.
The selected tool paths preserve unrelated root tool builds and pending source
experiments. The compiler patch also contains the postprocessor repair; a clean
postprocessor can be built from that same export when recreating these tool paths.
Publication additionally supports moving source products across the disposable
storage/repository filesystem boundary, with failed-copy and final-build rollback
controls. The postprocessor recognizes the exact SDK-generated editorconfig path
reported by MSBuild when managed artifacts live outside the source project.
