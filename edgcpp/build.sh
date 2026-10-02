#!/usr/bin/env bash
set -euo pipefail
root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
revision=89e67e07c7f6d0fc38622f9778d8e79f40deaa0f
if [[ ! -f "$root/upstream/CMakeLists.txt" || ! -f "$root/upstream/src/cfe.c" ]]; then
    echo "Missing embedded EDG sources in $root/upstream; restore them from the dotcc repository." >&2
    exit 1
fi
if [[ $(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["revision"])' "$root/upstream-origin.json") != "$revision" ]]; then
    echo "Expected embedded EDG base revision $revision; update the build profile when importing another revision." >&2
    exit 1
fi
mkdir -p "$root/upstream/cmake/macro-conf/all-extensions" "$root/artifacts"
for conf in "$root"/config/*.cmakedef; do
    target="$root/upstream/cmake/macro-conf/all-extensions/$(basename "$conf")"
    if ! cmp -s "$conf" "$target"; then cp "$conf" "$target"; fi
done
export EDG_BASE="$root/upstream/bases/docker/dev-env/gcc"
export PYTHONPATH="$root/upstream/dev_tools/pylibs${PYTHONPATH:+:$PYTHONPATH}"
export EDG_GCC_VER_SCRAPE
EDG_GCC_VER_SCRAPE=$("$root/upstream/dev_tools/bin/edg-scrape-compiler" gcc version)
if [[ ${EDG_BUILD_TARGET:-} == cpfe-dot || ${EDG_BUILD_TARGET:-} == cpfe-dot-oracle ]]; then
    python3 "$root/backend/prepare.py"
fi
cmake -S "$root/upstream" -B "$root/build" -G Ninja \
    -DCMAKE_BUILD_TYPE=Release -DEDG_MACRO_CONF=all-extensions \
    -DEDG_CPP_RT_LIBS=linux_x86_64 -DEDG_EAGER_REBUILD_LIB=ON \
    -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
    2>&1 | tee "$root/artifacts/configure.log"
build_target=()
if [[ -n ${EDG_BUILD_TARGET:-} ]]; then build_target=(--target "$EDG_BUILD_TARGET"); fi
cmake --build "$root/build" "${build_target[@]}" --parallel "${EDG_BUILD_JOBS:-12}" \
    2>&1 | tee "$root/artifacts/build.log"
"$root/build/bin/${EDG_BUILD_TARGET:-cpfe}" --version
