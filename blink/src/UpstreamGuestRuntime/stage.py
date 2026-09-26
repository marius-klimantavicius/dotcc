#!/usr/bin/env python3
"""Stage a guarded membarrier dispatch after the exact reviewed stop adapter."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['src/UpstreamGuestRuntime/stage.py']

import argparse
import difflib
import hashlib
import importlib.util
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = _CAMPAIGN_SOURCE["revision"]
ORIGINAL_PIN = _CAMPAIGN_INPUTS['ORIGINAL_PIN']
PREDECESSOR_PIN = _CAMPAIGN_INPUTS['PREDECESSOR_PIN']
sha = lambda value: hashlib.sha256(value).hexdigest()
HELPER = '''#include "host-membarrier.h"
#if !defined(DISABLE_THREADS) || !defined(NOLINEAR) || !defined(DISABLE_JIT) || defined(HAVE_THREADS) || defined(HAVE_FORK)
#error Private membarrier staging requires one guest thread, no fork, and the nonlinear interpreter
#endif

static int SysMembarrier(struct Machine *m, int command, unsigned flags,
                        int cpu_id) {
  (void)m;
  return blink_host_membarrier(command, flags, cpu_id);
}

'''


def adapt(predecessor):
    if sha(predecessor) != PREDECESSOR_PIN:
        raise ValueError("Reviewed execution-stop predecessor hash differs")
    source = predecessor.decode()
    anchor = "static int SysSchedYield(struct Machine *m) {"
    dispatch = '    SYSCALL(3, 0x13E, "getrandom", SysGetrandom, STRACE_GETRANDOM);\n'
    if source.count(anchor) != 1 or source.count(dispatch) != 1 or "SysMembarrier" in source:
        raise ValueError("Guest-runtime insertion boundaries differ")
    staged = source.replace(anchor, HELPER + anchor, 1).replace(
        dispatch, dispatch + '    SYSCALL(3, 0x144, "membarrier", SysMembarrier, STRACE_3);\n', 1)
    patch = "".join(difflib.unified_diff(source.splitlines(True), staged.splitlines(True),
                                       fromfile="a/blink/syscall.c", tofile="b/blink/syscall.c"))
    return staged.encode(), patch.encode()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--predecessor", type=Path, required=True)
    parser.add_argument("--predecessor-receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--receipt", type=Path, required=True)
    args = parser.parse_args()
    outputs = [args.output / "syscall.c", args.output / "guest-runtime.patch", args.receipt]
    resolved = [path.resolve() for path in outputs]
    if len(set(resolved)) != len(resolved):
        raise SystemExit("Output paths must be distinct")
    for path in resolved:
        if not any(path.is_relative_to((ROOT / name).resolve()) for name in ("generated", "artifacts")):
            raise SystemExit("Output must stay in campaign generated/artifacts; immutable/authored outputs are forbidden")
        if path.exists():
            raise SystemExit("Use fresh output paths; previous evidence is immutable")

    frozen = {}

    def read(path):
        path = path.resolve()
        value = path.read_bytes()
        frozen[str(path)] = sha(value)
        return value

    script_hash = sha(read(Path(__file__)))
    original_path = ROOT / "ref" / ("blink-" + REVISION) / "blink/syscall.c"
    original = read(original_path)
    if sha(original) != ORIGINAL_PIN:
        raise SystemExit("Immutable upstream syscall.c hash differs")
    predecessor = read(args.predecessor)
    predecessor_receipt_bytes = read(args.predecessor_receipt)
    predecessor_receipt = json.loads(predecessor_receipt_bytes)
    stop_script = ROOT / "src/UpstreamExecutionStop/stage.py"
    stop_script_hash = sha(read(stop_script))
    stop_patch = read(stop_script.with_name("execution-stop.patch"))
    if (predecessor_receipt.get("kind") != "reviewed-private-execution-stop-adaptation"
            or predecessor_receipt.get("upstream") != REVISION
            or predecessor_receipt.get("stage_sha256") != stop_script_hash
            or predecessor_receipt.get("patch_sha256") != sha(stop_patch)):
        raise SystemExit("Execution-stop predecessor receipt identity differs")
    source_receipt = predecessor_receipt["sources"]["syscall.c"]
    if source_receipt["source_sha256"] != ORIGINAL_PIN or source_receipt["staged_sha256"] != PREDECESSOR_PIN:
        raise SystemExit("Execution-stop receipt source identity differs")
    for relative, expected in predecessor_receipt["required_headers"].items():
        path = (ROOT / relative).resolve()
        if not path.is_relative_to((ROOT / "src/Host/include").resolve()) or sha(read(path)) != expected:
            raise SystemExit("Execution-stop header identity differs")
    # Reproduce the predecessor using its existing reviewed adapter unchanged.
    specification = importlib.util.spec_from_file_location("reviewed_execution_stop", stop_script)
    module = importlib.util.module_from_spec(specification)
    specification.loader.exec_module(module)
    reproduced, reproduced_patch, _ = module.adapt(original)
    if reproduced != predecessor or reproduced_patch != stop_patch:
        raise SystemExit("Predecessor does not reproduce the reviewed stop adaptation")
    staged, patch = adapt(predecessor)
    reviewed = read(HERE / "guest-runtime.patch")
    if patch != reviewed:
        raise SystemExit("Generated adaptation differs from reviewed guest-runtime.patch")
    header_name = "src/Host/include/host-membarrier.h"
    header_hash = sha(read(ROOT / header_name))
    for name, expected in frozen.items():
        if sha(Path(name).read_bytes()) != expected:
            raise SystemExit("Staging inputs changed during derivation: " + name)
    receipt = dict(
        kind="reviewed-private-single-thread-membarrier-adaptation", upstream=REVISION,
        stage_sha256=script_hash, patch_sha256=sha(reviewed),
        scope="Linux x64 syscall324 delegates to an owned single-guest-thread/no-fork memory-barrier boundary",
        required_defines=["DISABLE_THREADS", "NOLINEAR", "DISABLE_JIT"], forbidden_defines=["HAVE_THREADS", "HAVE_FORK"],
        required_headers={header_name: header_hash},
        predecessor=dict(path=str(args.predecessor.resolve()), sha256=PREDECESSOR_PIN,
                         receipt=str(args.predecessor_receipt.resolve()), receipt_sha256=sha(predecessor_receipt_bytes),
                         stage_sha256=stop_script_hash, patch_sha256=sha(stop_patch)),
        sources={"syscall.c": dict(source_sha256=ORIGINAL_PIN, predecessor_sha256=PREDECESSOR_PIN,
                                   staged_sha256=sha(staged))}, frozen_inputs=frozen)
    args.output.mkdir(parents=True, exist_ok=True)
    with outputs[0].open("xb") as stream:
        stream.write(staged)
    with outputs[1].open("xb") as stream:
        stream.write(reviewed)
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    with args.receipt.open("x") as stream:
        stream.write(json.dumps(receipt, indent=2) + "\n")


if __name__ == "__main__":
    main()
