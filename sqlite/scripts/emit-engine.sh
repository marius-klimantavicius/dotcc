#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
postprocess=true
# Group whole functions into roughly 256 KiB files for navigation in Rider.
split="${SQLITE_SOURCE_SPLIT:-size}"
split_size="${SQLITE_SOURCE_SPLIT_SIZE:-262144}"
case "${1:-}" in
  --no-postprocess) postprocess=false; shift ;;
  --help|-h) echo "Usage: $0 [--no-postprocess] (SQLITE_SOURCE_SPLIT=none|function|size; SQLITE_SOURCE_SPLIT_SIZE=bytes)"; exit 0 ;;
esac
if (( $# )); then
  echo "Usage: $0 [--no-postprocess] (SQLITE_SOURCE_SPLIT=none|function|size; SQLITE_SOURCE_SPLIT_SIZE=bytes)" >&2
  exit 1
fi
split_args=("--split=$split")
if [[ "$split" == size ]]; then split_args+=("--split-size=$split_size"); fi
"$PYTHON_CMD" "$SQLITE_ROOT/scripts/prepare-host-source.py" >&2
SQLITE_HOST_DEFINES=()
while IFS= read -r definition; do
  [[ -z "$definition" || "$definition" == \#* ]] || SQLITE_HOST_DEFINES+=("-D$definition")
done < "$SQLITE_ROOT/config/host-defines.txt"
dotnet "${DOTCC_COMPILER:-$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll}" \
  -std=c17 "${SQLITE_DEFINES[@]}" "${SQLITE_HOST_DEFINES[@]}" -I "$SQLITE_ROOT/generated/sqlite-port" \
  "$SQLITE_ROOT/generated/sqlite-port/sqlite3.c" \
  --overrides-file "$SQLITE_ROOT/config/dotcc-overrides.json" \
  --override-report "$SQLITE_ROOT/artifacts/engine-overrides.jsonl" \
  -MD -MF "$SQLITE_ROOT/artifacts/engine.d" \
  --emit=managedlib --literal-pool --nest-types --runtime=c --class-name Sqlite --namespace Managed.Database "${split_args[@]}" -o "$SQLITE_ROOT/generated/TranslatedSqlite"

if "$postprocess"; then
  dotnet build "$DOTCC_ROOT/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release --nologo
  # MSBuild evaluation needs assets even when the emitted C# has not been built.
  dotnet restore "$SQLITE_ROOT/generated/TranslatedSqlite/TranslatedSqlite.csproj" --nologo
  exec dotnet "$DOTCC_ROOT/DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll" \
    "$SQLITE_ROOT/generated/TranslatedSqlite/TranslatedSqlite.csproj" --in-place
fi
