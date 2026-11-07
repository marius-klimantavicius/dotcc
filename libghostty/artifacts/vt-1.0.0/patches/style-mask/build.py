"""Build the reflected-mask source with established offline inputs and caches."""
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
BASE = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")
WORK = BASE / "patches/style-mask"
WORK.mkdir(parents=True, exist_ok=True)
CACHE = BASE / "patches/lazy-abi-manifest"
command = json.loads((HERE.parents[1] / "native/command.json").read_text())
command[command.index("--prefix") + 1] = str(WORK / "install")
command[command.index("--cache-dir") + 1] = str(CACHE / "zig-cache")
result = subprocess.run(command, cwd=ROOT / "libghostty/ref/libghostty-vt-1.0.0",
                        capture_output=True, text=True, timeout=240,
                        env=dict(os.environ, TMPDIR=str(WORK),
                                 ZIG_GLOBAL_CACHE_DIR=str(CACHE / "global-cache")))
(HERE / "build.json").write_text(json.dumps(dict(command=command, exit=result.returncode,
                                                stdout=result.stdout, stderr=result.stderr), indent=2) + "\n")
assert result.returncode == 0, result.stderr
print("Reflected-mask native static/shared build succeeds")
