#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# != 0 ]]; then
  echo "Usage: $0" >&2
  exit 2
fi

compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
fixture="$DOTCC_ROOT/DotCC.FunctionalTests/Fixtures/static-function-identity"
expected="$fixture/expected-stdout.txt"
fragments="$SQLITE_ROOT/generated/function-identity-fragments"
mkdir -p "$fragments"

# Both routes must retain distinct addresses for identical static functions in
# separate C translation units. Reuse the repository's native-proven fixture.
for route in source objects; do
  project_name="FunctionIdentity-$route"
  output="$SQLITE_ROOT/generated/$project_name"
  prefix="$SQLITE_ROOT/artifacts/function-identity-$route"
  if [[ "$route" == source ]]; then
    inputs=("$fixture/main.c" "$fixture/other.c")
  else
    for unit in main other; do
      dotnet "$compiler" -std=c17 "$fixture/$unit.c" --emit=obj -o "$fragments/$unit.cs" \
        > "$prefix-$unit-emission.log" 2>&1
    done
    inputs=("$fragments/main.cs" "$fragments/other.cs")
  fi
  dotnet "$compiler" -std=c17 "${inputs[@]}" --emit=csproj -o "$output" \
    > "$prefix-emission.log" 2>&1
  dotnet build "$output/$project_name.csproj" -c Release --nologo \
    > "$prefix-build.log" 2>&1
  run_sqlite_process dotnet "$output/bin/Release/net10.0/$project_name.dll" \
    > "$prefix-jit.out"
  diff -u "$expected" "$prefix-jit.out"

  publish="$SQLITE_ROOT/build/function-identity-$route-aot"
  dotnet publish "$output/$project_name.csproj" -c Release -r linux-x64 \
    -p:PublishAot=true -o "$publish" --nologo \
    > "$prefix-aot-build.log" 2>&1
  run_sqlite_process "$publish/$project_name" > "$prefix-aot.out"
  diff -u "$expected" "$prefix-aot.out"
  echo "PASS function identity ($route): distinct static functions, JIT and NativeAOT"
done
