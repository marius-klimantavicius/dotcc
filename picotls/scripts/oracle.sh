#!/usr/bin/env bash
# Serial native oracle build/test. Coordinate the repository build slot first.
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
export TMPDIR="$campaign/artifacts/tmp/p0"
mkdir -p "$TMPDIR" "$campaign/artifacts/oracle"
source_root=$("$campaign/scripts/fetch.sh")
build="$campaign/build/oracle"
logs="$campaign/artifacts/oracle"
{
    date -u +%FT%TZ
    uname -a
    cc --version
    cmake --version
    openssl version -a
    dotnet --info
    dpkg-query -W gcc cmake libssl-dev libssl3t64 openssl 2>/dev/null || true
} > "$logs/environment.txt"
cmake -S "$source_root" -B "$build" \
    -DCMAKE_BUILD_TYPE=Debug -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
    -DCMAKE_C_FLAGS='-DPTLS_HAVE_LOG=0 -DPICOTLS_USE_DTRACE=0' \
    -DPKG_CONFIG_EXECUTABLE="$campaign/scripts/oracle-pkg-config.sh" \
    -DWITH_DTRACE=OFF -DWITH_FUSION=OFF -DWITH_AEGIS=OFF \
    -DWITH_MBEDTLS=OFF -DBUILD_FUZZER:BOOL=OFF 2>&1 | tee "$logs/configure.log"
# Verify configure did not silently widen the selected source closure.
python3 - "$build" <<'PY'
import json
from pathlib import Path
import sys
build = Path(sys.argv[1])
commands = json.loads((build / 'compile_commands.json').read_text())
for entry in commands:
    if 'PICOTLS_USE_BROTLI' in entry['command'] or entry['file'].endswith('/certificate_compression.c'):
        raise SystemExit('Unexpected Brotli dependency in native oracle')
cache = (build / 'CMakeCache.txt').read_text()
for option in ('WITH_DTRACE', 'WITH_FUSION', 'WITH_AEGIS', 'WITH_MBEDTLS', 'BUILD_FUZZER'):
    if f'{option}:BOOL=OFF' not in cache:
        raise SystemExit(f'Expected {option}=OFF')
PY
timeout 600 cmake --build "$build" --parallel 1 \
    --target picotls-core picotls-openssl cli test-openssl.t test-minicrypto.t \
    2>&1 | tee "$logs/build.log"
(cd "$source_root" && timeout 180 prove --exec '' -v "$build/test-openssl.t" "$build/test-minicrypto.t") \
    2>&1 | tee "$logs/tests.log"
timeout 10 "$build/cli" -h > "$logs/cli-help.txt" 2>&1
ldd "$build/cli" > "$logs/native-dependencies.txt"
cp "$build/compile_commands.json" "$logs/compile_commands.json"
printf 'Native oracle passed; evidence: %s\n' "$logs"
