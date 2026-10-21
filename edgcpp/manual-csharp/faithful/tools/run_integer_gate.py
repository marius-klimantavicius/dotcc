#!/usr/bin/env python3
"""Supervisor-only serial gate. Requires explicit HOLD from every shared writer."""
import argparse
import hashlib
import json
from pathlib import Path
import shlex
import subprocess
import sys
import time

p = argparse.ArgumentParser()
p.add_argument("--output", type=Path, required=True)
p.add_argument("--holds", type=Path, required=True)
a = p.parse_args()
faithful = Path(__file__).resolve().parents[1]
root = faithful.parents[2]
out = a.output.resolve()
out.mkdir(parents=True, exist_ok=True)
holds = json.loads(a.holds.read_text())
assert holds["allSharedWritersHeld"] is True
receipt = {"holds": holds, "started_unix": time.time(), "steps": [], "passed": False}

def run(name, command, cwd=root):
    result = subprocess.run([str(x) for x in command], cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    log = out / (name + ".log")
    log.write_bytes(result.stdout)
    receipt["steps"].append({"name": name, "command": [str(x) for x in command], "cwd": str(cwd),
                             "exit_code": result.returncode, "log": str(log),
                             "log_sha256": hashlib.sha256(result.stdout).hexdigest()})
    print(json.dumps({"step": name, "exit_code": result.returncode, "tail": result.stdout.decode(errors="replace")[-2000:]}), flush=True)
    if result.returncode:
        raise SystemExit(result.returncode)
    return result.stdout

try:
    run("snapshot-before", [sys.executable, faithful / "tools/capture_snapshot.py", "--output", out / "snapshot-before.json"])
    run("leaf-hashes", ["sha256sum", "--check", faithful / "ledgers/const_ints-batch1.sha256"])
    run("arithmetic-hashes", ["sha256sum", "--check", faithful / "ledgers/const_ints-batch2.sha256"], cwd=faithful)
    run("native-prepare", [sys.executable, faithful / "tools/prepare_integer_oracle.py", "--output", out / "native"])
    oracle = out / "native/integer-oracle"
    native_abi = run("native-abi", [oracle, "--abi"])
    entries = json.loads((root / "edgcpp/build/compile_commands.json").read_text())
    entry = next(e for e in entries if e["file"].endswith("/const_ints.c") and "/cmake/cpfe/" in e.get("output", ""))
    original = shlex.split(entry["command"])
    flags = []
    i = 1
    while i < len(original):
        if original[i] == "-o": i += 2
        elif original[i] == "-c" or original[i] == entry["file"]: i += 1
        else:
            flags.append(original[i])
            i += 1
    run("header-abi-build", [original[0], *flags, "-iquote", str(Path(entry["file"]).parent), faithful / "baseline/integer_abi_probe.cpp", "-o", out / "header-abi"], cwd=Path(entry["directory"]))
    run("header-abi", [out / "header-abi"])
    for name in ["IntegerLeaves", "IntegerArithmetic"]:
        project = faithful / f"tests/{name}/{name}.csproj"
        run("build-" + name, ["dotnet", "build", project, "-c", "Release", "--nologo"])
        assembly = faithful / f"tests/{name}/bin/Release/net10.0/{name}.dll"
        run("jit-" + name, ["dotnet", assembly])
    leaf_assembly = faithful / "tests/IntegerLeaves/bin/Release/net10.0/IntegerLeaves.dll"
    managed_abi = run("managed-abi", ["dotnet", leaf_assembly, "--abi"])
    native_fields = json.loads(native_abi)
    managed_fields = json.loads(managed_abi)
    assert all(managed_fields.get(key) == value for key, value in native_fields.items()), "native/managed ABI differs"
    receipt["abi_native_measured"] = native_fields
    receipt["abi_managed_observed"] = managed_fields
    receipt["abi_source_only_managed_metadata"] = {key: value for key, value in managed_fields.items() if key not in native_fields}
    receipt["abi_limit"] = "max_fixed_point_size is source-derived managed configuration metadata, not measured after native targ_def.h undefines its constituent target macros"
    receipt["abi_equal"] = True
    for group in ["leaves", "arithmetic"]:
        run("diff-jit-" + group, [sys.executable, faithful / "tools/compare_integer_streams.py", "--native", oracle,
                                  "--managed", leaf_assembly, "--group", group, "--output", out / ("diff-jit-" + group)])
    for name in ["IntegerLeaves", "IntegerArithmetic"]:
        project = faithful / f"tests/{name}/{name}.csproj"
        run("publish-" + name, ["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64", "--self-contained", "true",
                                "-p:PublishAot=true", "-o", out / ("aot-" + name), "--nologo"])
        run("aot-" + name, [out / ("aot-" + name) / name])
    for group in ["leaves", "arithmetic"]:
        run("diff-aot-" + group, [sys.executable, faithful / "tools/compare_integer_streams.py", "--native", oracle,
                                  "--managed", out / "aot-IntegerLeaves/IntegerLeaves", "--aot", "--group", group,
                                  "--output", out / ("diff-aot-" + group)])
    run("snapshot-after", [sys.executable, faithful / "tools/capture_snapshot.py", "--output", out / "snapshot-after.json"])
    before = json.loads((out / "snapshot-before.json").read_text())
    after = json.loads((out / "snapshot-after.json").read_text())
    assert before == after, "held input bytes changed during gate"
    receipt["snapshot_equal"] = True
    receipt["passed"] = True
finally:
    receipt["finished_unix"] = time.time()
    (out / "gate-receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
