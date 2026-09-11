#!/usr/bin/env bash
# Retained libc prerequisite evidence; this does not claim translated TLS passes.
source "$(dirname -- "$0")/common.sh"
if (( $# )); then echo "Usage: $0 (runs the Linux x64 NativeAOT dependency suite)" >&2; exit 1; fi
if [[ "$(uname -s):$(uname -m)" != Linux:x86_64 ]]; then
    echo "This validation recipe executes linux-x64 NativeAOT binaries; run it on Linux x64." >&2
    exit 1
fi
logs="$PICOTLS_ROOT/artifacts/libc"
export TMPDIR="$PICOTLS_ROOT/artifacts/tmp/libc"
mkdir -p "$logs" "$TMPDIR"
rm -f "$logs/PASS.txt"
dotnet --info > "$logs/dotnet-info.txt"
cc --version > "$logs/cc-version.txt"
git -C "$DOTCC_ROOT" rev-parse HEAD > "$logs/repository-commit.txt"
timeout --kill-after=10s 600s dotnet build "$DOTCC_ROOT/DotCC/DotCC.csproj" \
    -c Release --nologo 2>&1 | tee "$logs/compiler-build.log"
compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
sha256sum "$compiler" "$(dirname -- "$compiler")/DotCC.Lib.dll" > "$logs/compiler-sha256.txt"
unit_filter='FullyQualifiedName~LibcPosixMemalignTests|FullyQualifiedName~LibcAllocationFailureTests|FullyQualifiedName~PthreadLibTests|FullyQualifiedName~ThreadsLibTests|FullyQualifiedName~ThreadLocalTests'
timeout --kill-after=10s 600s dotnet test "$DOTCC_ROOT/DotCC.Tests/DotCC.Tests.csproj" \
    -c Release --filter "$unit_filter" --nologo 2>&1 | tee "$logs/unit-tests.log"
fixture_filter='FullyQualifiedName~FixtureTests&DisplayName~pthread|FullyQualifiedName~FixtureTests&DisplayName~posix-memalign'
timeout --kill-after=10s 600s dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" \
    -c Release --filter "$fixture_filter" --nologo 2>&1 | tee "$logs/functional-tests.log"

for fixture in pthread posix-memalign; do
    source="$DOTCC_ROOT/DotCC.FunctionalTests/Fixtures/$fixture/main.c"
    expected="$DOTCC_ROOT/DotCC.FunctionalTests/Fixtures/$fixture/expected-stdout.txt"
    native="$PICOTLS_ROOT/build/libc-dependencies/$fixture-native"
    generated="$PICOTLS_ROOT/generated/LibcDependencies-$fixture"
    published="$PICOTLS_ROOT/build/libc-dependencies/$fixture-aot"
    mkdir -p "$(dirname -- "$native")"
    timeout --kill-after=10s 120s cc -std=c11 -D_POSIX_C_SOURCE=200809L -pthread \
        "$source" -o "$native" > "$logs/$fixture-native-build.log" 2>&1
    timeout --kill-after=10s 60s "$native" \
        > "$logs/$fixture-native.out" 2> "$logs/$fixture-native.err"
    diff -u "$expected" "$logs/$fixture-native.out"
    test ! -s "$logs/$fixture-native.err"

    timeout --kill-after=10s 120s dotnet "$compiler" -std=c17 "$source" \
        --emit=csproj -o "$generated" > "$logs/$fixture-emit.log" 2>&1
    timeout --kill-after=10s 600s dotnet publish "$generated/LibcDependencies-$fixture.csproj" \
        -c Release -r linux-x64 -p:PublishAot=true -o "$published" --nologo \
        > "$logs/$fixture-aot-publish.log" 2>&1
    for debug in 0 1; do
        timeout --kill-after=10s 60s env DOTCC_DEBUG_HEAP="$debug" DOTCC_DEBUG_HEAP_SCAN="$debug" \
            "$published/LibcDependencies-$fixture" \
            > "$logs/$fixture-aot-debug-$debug.out" 2> "$logs/$fixture-aot-debug-$debug.err"
        diff -u "$expected" "$logs/$fixture-aot-debug-$debug.out"
        test ! -s "$logs/$fixture-aot-debug-$debug.err"
    done
    echo "PASS $fixture: native oracle, current emitted NativeAOT, normal and checked/scan heaps"
done
echo 'PASS selected libc unit/functional tests, native oracles, and current linux-x64 NativeAOT dependencies' \
    | tee "$logs/PASS.txt"
