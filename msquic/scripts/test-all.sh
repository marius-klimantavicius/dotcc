#!/usr/bin/env bash
# Rebuild the translated product and run every MsQuic validation campaign serially.
set -euo pipefail

msquic_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
python_cmd=python3
if ! "$python_cmd" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then python_cmd=python; fi
if ! "$python_cmd" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
  echo "$0: Python 3 is required" >&2
  exit 1
fi

run() {
  printf 'RUN %q' "$1"
  shift
  printf ' %q' "$@"
  printf '\n'
  "$@"
}

run "translate" "$msquic_root/scripts/translate.sh"

run "platform-host" "$python_cmd" "$msquic_root/scripts/test-platform-host.py"
run "packet-crypto" "$python_cmd" "$msquic_root/scripts/test-packet-crypto.py"
run "tls-adapter" "$python_cmd" "$msquic_root/scripts/test-tls-adapter.py"
run "datapath-host" "$python_cmd" "$msquic_root/scripts/test-datapath-host.py"
run "managed-peer" "$python_cmd" "$msquic_root/scripts/test-managed-peer.py"
run "managed-api" "$python_cmd" "$msquic_root/scripts/test-managed-api.py" --all
run "public-consumer" "$python_cmd" "$msquic_root/scripts/test-public-consumer.py"
run "managed-net-quic" "$python_cmd" "$msquic_root/scripts/test-managed-net-quic.py"
run "malformed-corpus" "$python_cmd" "$msquic_root/scripts/test-malformed-corpus.py"
run "endpoint-controls" "$python_cmd" "$msquic_root/scripts/test-endpoint-controls.py"
run "injected-host" "$python_cmd" "$msquic_root/scripts/test-injected-host.py"
run "managed-independent" "$python_cmd" "$msquic_root/scripts/test-managed-independent.py"
run "cid-rotation" "$python_cmd" "$msquic_root/scripts/test-managed-independent.py" --rotate-cid \
  --output "$msquic_root/artifacts/cid-rotation"
run "recovery" "$python_cmd" "$msquic_root/scripts/test-recovery.py"
