#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# != 0 ]]; then
  echo "Usage: $0" >&2
  exit 2
fi

prefix="$SQLITE_ROOT/artifacts/product-layout"
generated="$SQLITE_ROOT/generated/product-layout"
mkdir -p "$generated"
"$SQLITE_ROOT/scripts/emit-engine.sh" > "$prefix-emission.log" 2>&1

# Preserve product override precedence without native macro-redefinition
# diagnostics. Both preprocess and native oracle consume this identical list.
declare -A definitions=()
for configuration in defines.txt host-defines.txt; do
  while IFS= read -r definition; do
    [[ -z "$definition" || "$definition" == \#* ]] && continue
    definitions["${definition%%=*}"]="$definition"
  done < "$SQLITE_ROOT/config/$configuration"
done
printf '%s\n' "${definitions[@]}" | sort > "$prefix-defines.txt"
flags=()
while IFS= read -r definition; do flags+=("-D$definition"); done < "$prefix-defines.txt"
compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
source_directory="$SQLITE_ROOT/generated/sqlite-port"
dotnet "$compiler" -std=c17 "${flags[@]}" -I "$source_directory" -I "$SQLITE_ROOT/tests" \
  --overrides-file "$SQLITE_ROOT/config/dotcc-overrides.json" \
  -E "$source_directory/sqlite3.c" > "$prefix-preprocessed.c"
python3 "$SQLITE_ROOT/scripts/generate-layout-requests.py" \
  "$prefix-preprocessed.c" "$generated/layout_requests.h"
# Flexible-array storage can require additional offset contracts even when C
# never spells offsetof for that tail. Measure those independently in C too.
python3 - "$generated/layout_requests.h" "$SQLITE_ROOT/generated/TranslatedSqlite/Sqlite.cs" <<'PY'
import base64, pathlib, re, sys
header, engine = map(pathlib.Path, sys.argv[1:])
content = header.read_text()
aliases = {"Mem": "sqlite3_value"}
existing = {(aliases.get(owner, owner), member) for owner, member in
            re.findall(r"REQUIRED\(([^,]+), ([^)]+)\);", content)}
additional = set()
for block in re.findall(r"/\* dotcc-layout-v1\n(.*?)end-dotcc-layout \*/", engine.read_text(), re.S):
    for line in block.splitlines():
        parts = line.split("\t")
        if parts[0] != "request":
            continue
        owner, member = (base64.b64decode(value, validate=True).decode() for value in parts[2:4])
        if not re.fullmatch(r"[A-Za-z_]\w*", owner) or not re.fullmatch(r"[A-Za-z_]\w*(?:\.[A-Za-z_]\w*|\[\d+\])*", member):
            raise SystemExit("Unsupported product layout contract: " + repr((owner, member)))
        if (owner, member) not in existing:
            additional.add((owner, member))
header.write_text(content + "".join(f"REQUIRED({owner}, {member});\n" for owner, member in sorted(additional)))
print(f"Added {len(additional)} native probes for emitted flexible-array contracts")
PY

gcc -std=c17 -O1 "${SQLITE_NATIVE_FLAGS[@]}" "${flags[@]}" -DDOTCC_LAYOUT_REQUESTS \
  -I "$source_directory" -I "$SQLITE_ROOT/tests" -I "$generated" \
  "$SQLITE_ROOT/tests/ProductLayout/native.c" -lm -o "$SQLITE_ROOT/build/product-layout-native" \
  > "$prefix-native-build.log" 2>&1
run_sqlite_process "$SQLITE_ROOT/build/product-layout-native" > "$prefix-native.out"
python3 "$SQLITE_ROOT/scripts/generate-layout-storage-checks.py" \
  "$prefix-native.out" "$generated/LayoutStorageChecks.cs"
python3 "$SQLITE_ROOT/scripts/generate-product-offset-checks.py" \
  "$prefix-native.out" "$SQLITE_ROOT/generated/TranslatedSqlite/Sqlite.cs" \
  "$generated/ProductOffsetChecks.cs" > "$prefix-metadata.log"
cat "$prefix-metadata.log"

project="$SQLITE_ROOT/tests/ProductLayout/ProductLayout.csproj"
dotnet build "$project" -c Release --nologo > "$prefix-build.log" 2>&1
run_sqlite_process dotnet "$SQLITE_ROOT/tests/ProductLayout/bin/Release/net10.0/ProductLayout.dll" \
  > "$prefix-jit.out"
cat "$prefix-jit.out"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  publish="$SQLITE_ROOT/build/product-layout-aot"
  dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$publish" --nologo > "$prefix-aot-build.log" 2>&1
  run_sqlite_process "$publish/ProductLayout" > "$prefix-aot.out"
  diff -u "$prefix-jit.out" "$prefix-aot.out"
  cat "$prefix-aot.out"
fi
