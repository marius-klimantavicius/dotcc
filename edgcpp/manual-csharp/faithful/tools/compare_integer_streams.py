#!/usr/bin/env python3
"""Supervisor-only bounded differential driver, using worker-authored C# runners."""
import argparse
import hashlib
import itertools
import json
from pathlib import Path
import random
import subprocess

p = argparse.ArgumentParser()
p.add_argument("--native", type=Path, required=True)
p.add_argument("--managed", type=Path, required=True)
p.add_argument("--aot", action="store_true", help="Managed path is a published NativeAOT executable")
p.add_argument("--group", choices=["leaves", "arithmetic"], required=True)
p.add_argument("--output", type=Path, required=True)
a = p.parse_args()
out = a.output.resolve()
out.mkdir(parents=True, exist_ok=True)
rng = random.Random(20261021)
values = [0, 1, 0xffff, 0x10000, (1 << 64) - 1, 1 << 64,
          (1 << 127) - 1, 1 << 127, (1 << 128) - 1]
values += [rng.getrandbits(128) for _ in range(9)]
rows = []

def add(op, alias, s1, s2, count, first, second):
    rows.append(f"{op} {alias} {s1} {s2} {count} {first:032x} {second:032x}\n")

if a.group == "leaves":
    binary = ["or", "and", "xor", "compare", "copy-signed", "copy-unsigned",
              "set-signed", "set-unsigned"]
else:
    binary = ["add", "add-mixed", "subtract", "subtract-mixed"]
for first, second, alias, s1, s2, op in itertools.product(values, values, [0, 1], [0, 1], [0, 1], binary):
    add(op, alias, s1, s2, 0, first, second)
if a.group == "leaves":
    for first, s1, op in itertools.product(values, [0, 1], ["extract", "increment", "complement"]):
        add(op, 0, s1, 0, 0, first, 0)
else:
    for first, count, s1, s2, op in itertools.product(values, [0, 1, 15, 16, 17, 31, 32, 63, 64, 65, 112, 127], [0, 1], [0, 1], ["left", "right"]):
        add(op, 0, s1, s2, count, first, 0)
    for first, count, op in itertools.product(values, [1, 15, 16, 17, 31, 32, 63, 64, 65, 112, 127, 128], ["mask", "extend"]):
        add(op, 0, 0, 0, count, first, 0)
    for first in values:
        add("negate", 0, 0, 0, 0, first, 0)
data = "".join(rows).encode()
(out / "inputs.txt").write_bytes(data)
managed_command = [str(a.managed.resolve()), "--stream"] if a.aot else ["dotnet", str(a.managed.resolve()), "--stream"]
commands = [[str(a.native.resolve())], managed_command]
results = []
for name, command in zip(["native", "managed"], commands):
    run = subprocess.run(command, input=data, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (out / f"{name}.stdout.txt").write_bytes(run.stdout)
    (out / f"{name}.stderr.txt").write_bytes(run.stderr)
    results.append(run)
native = results[0].stdout.splitlines()
managed = results[1].stdout.splitlines()
mismatches = []
for index, (n, m) in enumerate(itertools.zip_longest(native, managed)):
    if n != m and len(mismatches) < 32:
        mismatches.append({"index": index, "input": rows[index].strip() if index < len(rows) else None,
                           "native": None if n is None else n.decode(errors="replace"),
                           "managed": None if m is None else m.decode(errors="replace")})
passed = all(r.returncode == 0 for r in results) and len(native) == len(managed) == len(rows) and not mismatches
receipt = {"group": a.group, "commands": commands, "cases": len(rows),
           "native_rows": len(native), "managed_rows": len(managed),
           "exit_codes": [r.returncode for r in results], "passed": passed,
           "input_sha256": hashlib.sha256(data).hexdigest(),
           "native_stdout_sha256": hashlib.sha256(results[0].stdout).hexdigest(),
           "managed_stdout_sha256": hashlib.sha256(results[1].stdout).hexdigest(),
           "mismatches_first_32": mismatches,
           "scope": "Bounded valid leaf argument contracts; raw limb values, error/comparison results, host bits, alias partition and post-call other mutation. Not frontend initialization or complete parity."}
(out / "comparison.json").write_text(json.dumps(receipt, indent=2) + "\n")
print(json.dumps({"passed": passed, "cases": len(rows), "receipt": str(out / "comparison.json"), "mismatches": mismatches[:3]}, indent=2))
raise SystemExit(0 if passed else 1)
