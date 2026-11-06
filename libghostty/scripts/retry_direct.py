"""Retry the pinned Zig engine and record its explicit compatibility patches."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--graph", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--evidence", type=Path, required=True)
    args = parser.parse_args()
    work, evidence = args.work.resolve(), args.evidence.resolve()
    disposable = Path("/mnt/storage/dotcc-libghostty-disposable").resolve()
    if not work.is_relative_to(disposable):
        parser.error("--work must be under /mnt/storage/dotcc-libghostty-disposable")
    graph_path = args.graph.resolve()
    graph = json.loads(graph_path.read_text())
    if (graph.get("output_mode"), graph.get("optimize_mode"), graph.get("stdlib_types")) != ("Lib", "fast", "source"):
        parser.error("the direct engine requires Lib/fast/source")
    root = next(module["root"] for module in graph["modules"] if module["name"] == graph["root_module"])
    root = (graph_path.parent / root).resolve()
    if root.name != "lib_vt.zig":
        parser.error("the direct engine root must be the original lib_vt.zig")
    work.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    output, log = work / "engine.csobj", evidence / "engine.log"
    if output.exists():
        output.unlink()
    command = ["dotnet", str(args.compiler.resolve()), str(root), "--zig-modules-file", str(graph_path),
               "--emit=obj", "--aggregate-data", "-o", str(output)]
    started = time.monotonic()
    with log.open("w") as stream:
        result = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT)
    receipt = dict(command=command, exit_code=result.returncode, elapsed_seconds=time.monotonic() - started,
                   engine_object_emitted=output.is_file(), linked=False, built=False, ran=False,
                   root=str(root), root_sha256=hashlib.sha256(root.read_bytes()).hexdigest(),
                   graph=graph, compiler=str(args.compiler.resolve()))
    patch_manifest = Path(__file__).resolve().parents[1] / "patches/direct/manifest.json"
    if patch_manifest.is_file():
        patch_data = json.loads(patch_manifest.read_text())
        final_hashes = {entry["source"]: entry["patched_sha256"] for entry in patch_data["patches"]}
        receipt["compatibility_patches"] = [
            dict(entry, final_sha256=final_hashes[entry["source"]],
                 observed_sha256=hashlib.sha256((root.parent.parent / entry["source"]).read_bytes()).hexdigest())
            for entry in patch_data["patches"]
            if (root.parent.parent / entry["source"]).is_file()
        ]
    (evidence / "engine.json").write_text(json.dumps(receipt, indent=2) + "\n")
    print(log.read_text()[-6000:])
    print(f"engine emit exit {result.returncode}; object {output.is_file()}")
    return result.returncode


if __name__ == "__main__":
    raise SystemExit(main())
