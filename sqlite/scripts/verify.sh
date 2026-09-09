#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
export SQLITE_AOT="${SQLITE_AOT:-1}"
mkdir -p "$TMPDIR"
if [[ $# -gt 1 || ( -n "${1:-}" && "$1" != --with-ports ) ]]; then
  echo "Usage: $0 [--with-ports]" >&2
  exit 2
fi

python3 "$SQLITE_ROOT/scripts/fetch.py"
"$SQLITE_ROOT/scripts/test-repository.sh" \
  > "$SQLITE_ROOT/artifacts/campaign-repository.log" 2>&1
"$SQLITE_ROOT/scripts/preprocess.sh" > "$SQLITE_ROOT/artifacts/sqlite3.i"

native_check() {
  local script="$1" expected="$2" name="$3"
  "$SQLITE_ROOT/scripts/$script" > "$SQLITE_ROOT/artifacts/campaign-native-$name.out" \
    2> "$SQLITE_ROOT/artifacts/campaign-native-$name-build.log"
  diff -u "$SQLITE_ROOT/tests/$expected" "$SQLITE_ROOT/artifacts/campaign-native-$name.out"
  echo "PASS native $name"
}
native_check native.sh native-corpus.expected core
native_check test-api-native.sh native-api.expected api
native_check test-vfs-native.sh native-vfs.expected vfs
native_check test-vtable-native.sh native-vtable.expected vtable
native_check test-allocation-native.sh native-allocation.expected allocation
native_check test-upstream-native.sh upstream-jsonb.expected upstream
native_check layout-native.sh layout-native.expected layout

"$SQLITE_ROOT/scripts/test-layout-translated.sh" \
  > "$SQLITE_ROOT/artifacts/campaign-layout.log" 2>&1
"$SQLITE_ROOT/scripts/test-managed-consumer.sh" \
  > "$SQLITE_ROOT/artifacts/campaign-managed-consumer.log" 2>&1
for suite in core api vfs vtable allocation upstream; do
  "$SQLITE_ROOT/scripts/test-translated.sh" "$suite" \
    > "$SQLITE_ROOT/artifacts/campaign-translated-$suite.log" 2>&1
  echo "PASS translated $suite"
done
"$SQLITE_ROOT/scripts/test-image-exchange.sh" \
  > "$SQLITE_ROOT/artifacts/campaign-image-exchange.log" 2>&1
if [[ "${1:-}" == --with-ports ]]; then
  "$SQLITE_ROOT/scripts/test-ports.sh"
fi
echo "PASS SQLite campaign: core, JSON/JSONB, memory VFS, callbacks, layout and image exchange (AOT=$SQLITE_AOT)"
