#!/usr/bin/env bash
# Run a runtime-loaded regex profile with a NativeAOT compiler, compare with C,
# and verify CLI dependency and mixed source/object behavior.
source "$(dirname "$0")/common.sh"
compiler="${DOTCC_OVERRIDE_COMPILER:-$SQLITE_ROOT/build/dotcc-overrides-aot/dotcc}"
fixture="$DOTCC_ROOT/examples/macro-overrides"
output="$SQLITE_ROOT/artifacts/macro-overrides-cli"
mkdir -p "$output"
"$compiler" "$fixture/helper.c" --emit=obj -o "$output/helper.o"
"$compiler" "$fixture/main.c" "$output/helper.o" \
  --overrides-file "$fixture/profile.json" --override-report "$output/report.jsonl" \
  -MD -MF "$output/probe.d" --split=function -o "$output/generated"
gcc -std=c17 "$fixture/main.c" "$fixture/helper.c" -o "$output/native"
"$output/native" > "$output/native.out"
dotnet build "$output/generated" -c Release --nologo > "$output/build.log" 2>&1
run_sqlite_process dotnet "$output/generated/bin/Release/net10.0/generated.dll" > "$output/managed.out"
diff -u "$output/native.out" "$output/managed.out"
"$compiler" -E "$fixture/main.c" --overrides-file "$fixture/profile.json" > "$output/preprocessed.c"
python3 - "$output" "$fixture/profile.json" <<'PY'
import json, pathlib, sys
root=pathlib.Path(sys.argv[1])
assert sys.argv[2] in (root/'probe.d').read_text()
assert '__dotcc_is_little_endian' in (root/'preprocessed.c').read_text()
events=[json.loads(line) for line in (root/'report.jsonl').read_text().splitlines()]
assert {e['name'] for e in events if e['event']=='selected'} == {'LITTLE','BIG','X'}
assert any(e['event']=='object-profile' for e in events)
PY
if "$compiler" "$output/helper.o" --override-macro 'X=1' -o "$output/rejected" > "$output/rejected.log" 2>&1; then
  echo 'Object-only overrides unexpectedly accepted' >&2; exit 1
fi
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$output/generated" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$output/aot" --nologo > "$output/aot-build.log" 2>&1
  run_sqlite_process "$output/aot/generated" > "$output/aot.out"
  diff -u "$output/native.out" "$output/aot.out"
fi
echo 'PASS macro overrides: runtime regex profile, native oracle, mixed objects, dependency and rejection checks'
