#!/usr/bin/env bash
# Rebuild the qualified compiler without unrelated/pending root experiments,
# then genuinely translate the pinned release privately. No canonical writes.
set -euo pipefail
repo=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../../.." && pwd)
work=/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0
base=51b1be1f59bb351806e724ed58b712f0bdb8e091
mkdir -p "$work/compiler-source" "$work/tmp"
export TMPDIR="$work/tmp"
export PYTHONDONTWRITEBYTECODE=1
if [[ ! -f "$work/compiler-source/DotCC/DotCC.csproj" ]]; then
    git -C "$repo" archive "$base" DotCC DotCC.Lib DotCC.Libc DotCC.PostProcess \
        LICENSE Directory.Packages.props Directory.Build.props nuget.config \
        | tar -x -C "$work/compiler-source"
    git -C "$work/compiler-source" apply "$repo/libghostty/artifacts/vt-1.0.0/compiler-vt-1.0.0.patch"
fi
git -C "$work/compiler-source" apply --reverse --check \
    "$repo/libghostty/artifacts/vt-1.0.0/compiler-vt-1.0.0.patch"
dotnet build "$work/compiler-source/DotCC/DotCC.csproj" -c Release \
    -p:PublishAot=false -p:UseLocalLalrCc=false -p:UseArtifactsOutput=true \
    -p:ArtifactsPath="$work/compiler-build" --nologo
dotnet build "$work/compiler-source/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release \
    -p:PublishAot=false -p:UseArtifactsOutput=true -p:ArtifactsPath="$work/postprocess-build" --nologo
cd "$repo"
bash libghostty/scripts/fetch.sh --fetch missing --tools reuse
python3 libghostty/patches/vt-1.0.0/apply.py libghostty/ref/libghostty-vt-1.0.0
python3 libghostty/scripts/prepare_direct.py \
    --zig "$work/tools/zig-x86_64-linux-0.17.0/zig" --work "$work/native" \
    --evidence libghostty/artifacts/vt-1.0.0/native
python3 libghostty/scripts/translate_direct.py \
    --compiler "$work/compiler-build/bin/DotCC/release/dotcc.dll" \
    --zig "$work/tools/zig-x86_64-linux-0.17.0/zig" --work "$work/translation" \
    --evidence libghostty/artifacts/vt-1.0.0/translation \
    --graph "$work/native/module-graph-managed.json"
python3 libghostty/scripts/verify_direct_consumer.py \
    --raw-project "$work/translation/raw/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj" \
    --processed-project "$work/translation/processed/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj" \
    --work "$work/owning" --evidence libghostty/artifacts/vt-1.0.0/owning
