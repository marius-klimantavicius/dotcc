"""Check actual release packed fields with the fresh native dependency graph."""
import argparse
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
WORK = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/patches/graph")
WORK.mkdir(parents=True, exist_ok=True)
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--graph", type=Path, default=WORK.parents[1] / "native/module-graph-managed.json")
args = parser.parse_args()
graph = json.loads(args.graph.read_text())
zig = WORK.parents[1] / "tools/zig-x86_64-linux-0.17.0/zig"
probe = ROOT / "libghostty/artifacts/direct-resume-20261104/field-mask-path/native.zig"
command = [str(zig), "build-exe", "--cache-dir", str(WORK / "cache"),
           "--global-cache-dir", str(WORK / "global"), "-femit-bin=" + str(WORK / "mask"),
           "--dep", "vt", "-Mprobe=" + str(probe)]
for module in graph["modules"]:
    for dep in module["dependencies"]:
        target = "vt" if dep["module"] == graph["root_module"] else dep["module"]
        command.extend(["--dep", dep["name"] + "=" + target])
    name = "vt" if module["name"] == graph["root_module"] else module["name"]
    command.append("-M" + name + "=" + module["root"])
result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, timeout=120)
receipt = dict(command=command, compile_exit=result.returncode,
               stdout=result.stdout, stderr=result.stderr)
if result.returncode == 0:
    ran = subprocess.run([str(WORK / "mask")], capture_output=True, text=True, timeout=30)
    receipt.update(run_exit=ran.returncode, run_stdout=ran.stdout, run_stderr=ran.stderr)
(HERE / "graph-mask.json").write_text(json.dumps(receipt, indent=2) + "\n")
assert receipt["compile_exit"] == 0 and receipt.get("run_exit") == 42, receipt
assert not receipt["run_stdout"] and not receipt["run_stderr"], receipt
print("All recursively nested actual release Row/Cell masks match original traversal; combined, duplicate, and empty paths pass")
