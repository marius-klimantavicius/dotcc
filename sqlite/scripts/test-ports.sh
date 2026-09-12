#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
export SQLITE_EXECUTION_TIMEOUT="${SQLITE_PORT_TIMEOUT:-600}"
mkdir -p "$TMPDIR"
compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
# A prior emitter may have used another source filename (for example Program.cs
# instead of DotCcProgram.cs). Keep each run in a fresh output tree so MSBuild's
# source glob cannot combine old and current translations. Preserve old runs.
port_output=$(mktemp -d "$SQLITE_ROOT/generated/Regression-Ports-XXXXXXXX")
echo "Regression translation outputs: $port_output"

# Mirror the repository's Lua workflow, retaining its original upstream runner.
lua_source="$DOTCC_ROOT/examples/lua/lua-src"
lua_units=(lapi lcode lctype ldebug ldo ldump lfunc lgc llex lmem lobject lopcodes
  lparser lstate lstring ltable ltm lundump lvm lzio lauxlib lbaselib lcorolib ldblib
  liolib lmathlib loadlib loslib lstrlib ltablib lutf8lib linit lua)
lua_inputs=()
for unit in "${lua_units[@]}"; do lua_inputs+=("$lua_source/$unit.c"); done
lua_output="$port_output/Regression-Lua"
dotnet "$compiler" --emit=csproj -I "$lua_source" "${lua_inputs[@]}" -o "$lua_output" \
  > "$SQLITE_ROOT/artifacts/regression-lua-emission.log" 2>&1
dotnet build "$lua_output/Regression-Lua.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/regression-lua-build.log" 2>&1
(
  cd "$lua_source/testes"
  run_sqlite_process dotnet "$lua_output/bin/Release/net10.0/Regression-Lua.dll" -e '_U=true' all.lua
) > "$SQLITE_ROOT/artifacts/regression-lua.out" 2>&1
rg -q 'final OK !!!' "$SQLITE_ROOT/artifacts/regression-lua.out"
echo "PASS shared Lua upstream conformance"

# Mirror chibi.yml's native stub bootstrap and explicit static-module build.
chibi_root="$DOTCC_ROOT/examples/chibi"
chibi_source="$chibi_root/chibi-src"
make -C "$chibi_source" -j2 chibi-scheme \
  > "$SQLITE_ROOT/artifacts/regression-chibi-native-build.log" 2>&1
make -C "$chibi_source" lib/chibi/filesystem.c lib/chibi/io/io.c lib/chibi/process.c lib/chibi/time.c \
  >> "$SQLITE_ROOT/artifacts/regression-chibi-native-build.log" 2>&1
chibi_inputs=()
for unit in gc sexp bignum gc_heap opcodes vm eval simplify main; do
  chibi_inputs+=("$chibi_source/$unit.c")
done
chibi_output="$port_output/Regression-Chibi"
dotnet "$compiler" --emit=csproj -I "$chibi_root/gen-include" -I "$chibi_source/include" \
  -D SEXP_USE_INTTYPES -D SEXP_USE_NTPGETTIME -D SEXP_USE_DL=0 -D SEXP_USE_POLL_PORT=0 \
  -D SEXP_USE_STATIC_LIBS=1 -D SEXP_USE_STATIC_LIBS_NO_INCLUDE=0 \
  -I "$chibi_root/gen-lib" -I "$chibi_source" "${chibi_inputs[@]}" -o "$chibi_output" \
  > "$SQLITE_ROOT/artifacts/regression-chibi-emission.log" 2>&1
dotnet build "$chibi_output/Regression-Chibi.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/regression-chibi-build.log" 2>&1
(
  cd "$chibi_source"
  CHIBI_IGNORE_SYSTEM_PATH=1 CHIBI_MODULE_PATH=lib \
    run_sqlite_process dotnet "$chibi_output/bin/Release/net10.0/Regression-Chibi.dll" tests/r7rs-tests.scm
) > "$SQLITE_ROOT/artifacts/regression-chibi.out" 2>&1
python3 - "$SQLITE_ROOT/artifacts/regression-chibi.out" "$chibi_root/baseline-r7rs.txt" <<'PY'
import difflib, pathlib, re, sys
def normalized(path):
    text = pathlib.Path(path).read_text()
    text = re.sub(r'\x1b\[[0-9;]*m', '', text)
    return re.sub(r' in [0-9.]+ seconds', '', text)
actual, expected = map(normalized, sys.argv[1:])
if actual != expected or '1225 out of 1225' not in actual:
    sys.stderr.writelines(difflib.unified_diff(expected.splitlines(True), actual.splitlines(True), 'native baseline', 'translated chibi'))
    raise SystemExit('Chibi conformance mismatch')
PY
echo "PASS shared Chibi R7RS conformance"

# The shared IR also feeds WAT; require tools so this check cannot silently skip.
wat2wasm --version
node --version
DOTCC_RUN_WAT=1 dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" \
  -c Release --no-build --blame-hang-timeout 300s --filter FullyQualifiedName~WatOracleTests -- -parallel none \
  > "$SQLITE_ROOT/artifacts/regression-wat.log" 2>&1
echo "PASS shared WAT execution oracle"
