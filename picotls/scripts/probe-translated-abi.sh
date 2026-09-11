#!/usr/bin/env bash
# Audit actual product structs, a separate header's compiler metadata, and native C.
source "$(dirname -- "$0")/common.sh"
variant=optimized
metadata_only=false
while (( $# )); do
    case "$1" in
        --raw) variant=raw ;;
        --metadata-only) metadata_only=true ;;
        --help|-h) echo "Usage: $0 [--raw] [--metadata-only] (PICOTLS_AOT=1; PICOTLS_RUNTIME=linux-x64)"; exit 0 ;;
        *) echo "Usage: $0 [--raw] [--metadata-only]" >&2; exit 1 ;;
    esac
    shift
done
project="$PICOTLS_PROJECT"
[[ "$variant" != raw ]] || project="$PICOTLS_RAW_PROJECT"
if ! "$metadata_only"; then require_picotls_project "$project"; fi
source_dir=$("$PICOTLS_ROOT/scripts/fetch.sh")
artifacts="$PICOTLS_ROOT/artifacts/translated-abi/$variant"
mkdir -p "$artifacts"
timeout --kill-after=10s 120s cc -std=c11 "${PICOTLS_DEFINES[@]}" \
    -I "$source_dir/include" "$PICOTLS_ROOT/tests/native-layout.c" -o "$artifacts/native-layout"
timeout --kill-after=10s 30s "$artifacts/native-layout" > "$artifacts/native-layout.txt"
# This independent header translation requests metadata without adding test
# functions or changing the product's three-unit source closure.
rm -f "$artifacts/layout-metadata.cs"
timeout --kill-after=10s "${PICOTLS_TRANSLATE_TIMEOUT:-600}s" \
    dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 \
    "${PICOTLS_DEFINES[@]}" -I "$source_dir/include" "$PICOTLS_ROOT/tests/native-layout.c" \
    --emit=obj -o "$artifacts/layout-metadata.cs" 2>&1 | tee "$artifacts/metadata-emission.log"
python3 "$PICOTLS_ROOT/scripts/check-translated-abi-metadata.py" \
    "$artifacts/native-layout.txt" "$artifacts/layout-metadata.cs"
if "$metadata_only"; then exit 0; fi
consumer="$PICOTLS_ROOT/tests/TranslatedAbi/TranslatedAbi.csproj"
dotnet build "$consumer" -c Release --nologo "-p:PicotlsProject=$project" \
    2>&1 | tee "$artifacts/build.log"
timeout --kill-after=10s 60s dotnet \
    "$PICOTLS_ROOT/tests/TranslatedAbi/bin/Release/net10.0/TranslatedAbi.dll" \
    "$artifacts/native-layout.txt" | tee "$artifacts/actual-storage.log"
if [[ "${PICOTLS_AOT:-0}" == 1 ]]; then
    runtime="${PICOTLS_RUNTIME:-linux-x64}"
    output="$PICOTLS_ROOT/build/translated-abi/$variant/$runtime"
    dotnet publish "$consumer" -c Release -r "$runtime" -p:PublishAot=true \
        "-p:PicotlsProject=$project" -o "$output" --nologo 2>&1 | tee "$artifacts/publish-$runtime.log"
    executable="$output/TranslatedAbi"
    [[ ! -f "$executable.exe" ]] || executable="$executable.exe"
    timeout --kill-after=10s 60s "$executable" "$artifacts/native-layout.txt" \
        | tee "$artifacts/actual-storage-$runtime.log"
fi
