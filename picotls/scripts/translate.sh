#!/usr/bin/env bash
# Translate the unmodified selected core, preserve raw output, then optimize in place.
source "$(dirname -- "$0")/common.sh"
build_tools=true
case "${1:-}" in
    --no-build-tools) build_tools=false; shift ;;
    --help|-h) echo "Usage: $0 [--no-build-tools]"; exit 0 ;;
esac
if (( $# )); then echo "Usage: $0 [--no-build-tools]" >&2; exit 1; fi
if "$build_tools"; then
    dotnet build "$DOTCC_ROOT/DotCC/DotCC.csproj" -c Release --nologo
    dotnet build "$DOTCC_ROOT/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release --nologo
fi
compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
postprocessor="$DOTCC_ROOT/DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"
for tool in "$compiler" "$postprocessor"; do
    [[ -f "$tool" ]] || { echo "Missing tool $tool; rerun without --no-build-tools" >&2; exit 1; }
done
source_dir=$("$PICOTLS_ROOT/scripts/fetch.sh")
translation_sources=$(python3 "$PICOTLS_ROOT/scripts/translation-sources.py")
mapfile -t sources <<< "$translation_sources"
logs="$PICOTLS_ROOT/artifacts/translation"
mkdir -p "$logs"
rm -f "$logs/success.json"
python3 "$PICOTLS_ROOT/scripts/snapshot-translation.py" inputs
timeout --kill-after=10s "${PICOTLS_TRANSLATE_TIMEOUT:-600}s" \
    dotnet "$compiler" -std=c17 "${PICOTLS_DEFINES[@]}" -I "$source_dir/include" -I "$source_dir" \
    "${sources[@]}" --emit=managedlib --class-name Picotls --namespace Managed.Security \
    --split=size --split-size=102400 -o "$(dirname -- "$PICOTLS_PROJECT")" \
    2>&1 | tee "$logs/emit.log"
require_picotls_project "$PICOTLS_PROJECT"
python3 "$PICOTLS_ROOT/scripts/snapshot-translation.py" copy
dotnet restore "$PICOTLS_PROJECT" --nologo
timeout --kill-after=10s "${PICOTLS_POSTPROCESS_TIMEOUT:-600}s" \
    dotnet "$postprocessor" "$PICOTLS_PROJECT" --in-place 2>&1 | tee "$logs/postprocess.log"
python3 "$PICOTLS_ROOT/scripts/snapshot-translation.py" record
