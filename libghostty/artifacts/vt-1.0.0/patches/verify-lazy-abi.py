"""Rebuild the relocated ABI validation and compare its native consumer bytes."""
import argparse
import ctypes
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
BASE = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")
WORK = BASE / "patches/lazy-abi-manifest"
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
WORK.mkdir(parents=True, exist_ok=True)
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--label", default="lazy-abi-manifest")
args = parser.parse_args()
assert args.label in ("lazy-abi-manifest", "sized-validation")
sha = lambda data: hashlib.sha256(data).hexdigest()
records = []


def run(stage, argv, cwd=ROOT):
    result = subprocess.run(argv, cwd=cwd, capture_output=True, text=True,
                            env=dict(os.environ, TMPDIR=str(WORK),
                                     ZIG_GLOBAL_CACHE_DIR=str(WORK / "global-cache")),
                            timeout=240)
    records.append(dict(stage=stage, argv=argv, exit_code=result.returncode,
                        stdout=result.stdout, stderr=result.stderr))
    (HERE / (args.label + "-native.json")).write_text(json.dumps(records, indent=2) + "\n")
    assert result.returncode == 0, records[-1]
    return result.stdout


before = json.loads((HERE / "lazy-abi-manifest-before.json").read_text())
payload_before = Path(before["snapshot"]).read_bytes()
assert len(payload_before) == before["manifest_bytes"]
assert sha(payload_before) == before["manifest_sha256"]
argv = json.loads((HERE.parent / "native/command.json").read_text())
for option, value in [("--prefix", WORK / "install"), ("--cache-dir", WORK / "zig-cache")]:
    argv[argv.index(option) + 1] = str(value)
run("native-build", argv, cwd=SOURCE)
library = WORK / "install/lib/libghostty-vt.so.1.0.0"
loaded = ctypes.CDLL(str(library))
loaded.ghostty_type_json.restype = ctypes.c_char_p
payload_after = loaded.ghostty_type_json()
assert payload_after == payload_before
(WORK / "after.json").write_bytes(payload_after)
summary = dict(manifest_bytes=len(payload_after), manifest_sha256=sha(payload_after),
               patch_manifest_sha256=sha((ROOT / "libghostty/patches/vt-1.0.0/manifest.json").read_bytes()),
               exact_manifest_bytes_equal=True, original_library_sha256=before["library_sha256"],
               patched_library=str(library), patched_library_sha256=sha(library.read_bytes()))

# Replay all existing native boundary commands against the patched static lib,
# with new executables confined to this adaptation's scratch directory.
baseline = json.loads((HERE.parent / "boundary/native-checks.json").read_text())
old_lib = str(BASE / "native/install/lib/libghostty-vt.a")
new_lib = str(WORK / "install/lib/libghostty-vt.a")
for check in baseline["checks"]:
    argv = [arg.replace(old_lib, new_lib) for arg in check["argv"]]
    # Input headers and indirect ABI adapters remain the established inputs.
    if "-o" in argv:
        index = argv.index("-o") + 1
        argv[index] = str(WORK / Path(argv[index]).name)
    else:
        argv[0] = str(WORK / Path(argv[0]).name)
    observed = run(check["stage"], argv)
    assert observed == check["stdout"], check["stage"]
summary.update(native_cases=baseline["cases"], native_passed=baseline["passed"],
               direct_and_indirect_outputs_identical_to_pre_patch=True)
(HERE / (args.label + "-parity.json")).write_text(json.dumps(summary, indent=2) + "\n")
print(json.dumps(summary, indent=2))
