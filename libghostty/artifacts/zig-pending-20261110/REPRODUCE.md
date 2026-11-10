# Focused Zig follow-up verification

Run from the repository root. This uses one compiler source export and places
all builds, caches and temporary test products on storage. The retained export
was committed base `7a449b290f3fd9b28b584cf03f00a9f47d7cbdc6` plus exactly the
25 source/test files listed in `summary.json`. Each matched the working copy
before commit. Later independent CUI commits are preserved as commit ancestors.

For a new reproduction, export the committed source into an empty `$D`:

```sh
W=/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110
D=$W/compiler-source
ZIG=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/tools/zig-x86_64-linux-0.17.0/zig
mkdir -p "$D"
# Omit this export when reusing the already validated source directory.
git archive HEAD DotCC DotCC.Lib DotCC.Libc DotCC.PostProcess DotCC.FunctionalTests \
  DotCC.PostProcess.Tests DotCC.Tests LICENSE Directory.Build.props \
  Directory.Packages.props nuget.config | tar -x -C "$D"
export TMPDIR=$W
export DOTCC_TARGET_ZIG_LIB=$(dirname "$ZIG")/lib
dotnet build "$D/DotCC/DotCC.csproj" -c Release -p:PublishAot=false \
  -p:UseLocalLalrCc=false -p:UseArtifactsOutput=true -p:ArtifactsPath="$W/build" --nologo
dotnet build "$D/DotCC.Libc/DotCC.Libc.csproj" -c Release \
  -p:UseArtifactsOutput=true -p:ArtifactsPath="$W/build" --nologo
dotnet test "$D/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" -c Release \
  -p:PublishAot=false -p:UseLocalLalrCc=false -p:UseArtifactsOutput=true \
  -p:ArtifactsPath="$W/build" -p:BuildProjectReferences=false \
  --filter 'FullyQualifiedName~ZigGenericElementWidthExecutionTests|FullyQualifiedName~ZigOptionalStorageVectorExecutionTests|FullyQualifiedName~ZigRenderOptionalExecutionTests|(FullyQualifiedName~ZigStaticPointerExecutionTests&DisplayName!~testing-allocator)'
dotnet test "$D/DotCC.Tests/DotCC.Tests.csproj" -c Release \
  -p:PublishAot=false -p:UseLocalLalrCc=false -p:UseArtifactsOutput=true \
  -p:ArtifactsPath="$W/build" -p:BuildProjectReferences=false \
  --filter FullyQualifiedName~ZigOptionalLayoutTests
PYTHONDONTWRITEBYTECODE=1 python3 libghostty/artifacts/zig-pending-20261110/native-checks.py
```

Results: compiler zero warnings/errors; 136 functional passes, zero skips;
22 optional-layout unit passes. The two existing `testing-allocator` failures
are explicitly excluded, not counted as passing. `preexisting-thread-parser.txt`
records their independent reproduction with the accepted 1.0.0 compiler.
`native-final.json` records positive execution and expected negative rejection
statuses. The three vector CPU profiles require an x64 host supporting AVX512
to execute all of baseline/v3/native controls as recorded here.

The real product was then freshly regenerated and executed:

```sh
CLI=$W/build/bin/DotCC/release/dotcc.dll
python3 libghostty/scripts/translate_direct.py --compiler "$CLI" --zig "$ZIG" \
  --work "$W/final-engine" --evidence "$W/final-engine-evidence" \
  --graph /mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/native/module-graph-managed.json
python3 libghostty/scripts/verify_direct_consumer.py \
  --raw-project "$W/final-engine/raw/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj" \
  --processed-project "$W/final-engine/processed/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj" \
  --work "$W/owning" --evidence "$W/owning-evidence"
dotnet build libghostty/ManagedConsumer.slnx -c Release -p:PublishAot=false \
  -p:UseArtifactsOutput=true -p:ArtifactsPath="$W/default-check" --nologo
dotnet "$W/default-check/bin/ManagedConsumer/release/GhosttyVt.Consumer.dll"
```

`final-engine-evidence-*.json` records all eight successful translation/build
stages. `owning-evidence-*.json` records raw/processed JIT and whole-library-rooted
NativeAOT build/run success. The ordinary canonical solution builds with zero
warnings/errors and prints the same owning transcript PASS without a
`TranslatedProject` override. No canonical generated files were replaced.
Full upstream managed qualification is still pending.

The independent old-pin Patch16 reproduction and its non-C layout limitation
are documented in `legacy-size/REPORT.txt`.
