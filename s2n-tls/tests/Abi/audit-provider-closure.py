#!/usr/bin/env python3
"""Compile the exact staged C profile privately and reconcile native imports.

This measures every selected TU, including externally callable paths which a
single handshake would not reach. It neither builds DotCC nor links a product.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--campaign", type=Path, required=True)
parser.add_argument("--jobs", type=int, default=2)
args = parser.parse_args()
campaign = args.campaign.resolve()
run = campaign.name
receipt = json.loads((ROOT / "artifacts/campaign" / run / "receipt.json").read_text())
work = ROOT / "build/provider/closure" / run
output = ROOT / "artifacts/provider/closure" / run
work.mkdir(parents=True, exist_ok=True)
output.mkdir(parents=True, exist_ok=True)
provider = ROOT / "src/CryptoProvider/provider-overrides.json"
profile = json.loads(provider.read_text())
bound = {x["name"] for x in profile["functionOverrides"]}
commands = []
for item in receipt["commands"]:
    cmd = item["command"]
    if "--emit=obj" not in cmd or not any(str(x).endswith(".c") for x in cmd):
        continue
    source = next(x for x in cmd if x.endswith(".c"))
    flags = []
    for index, flag in enumerate(cmd):
        if flag == "-I":
            flags += [flag, cmd[index+1]]
        elif flag.startswith(("-D", "-std=", "-I")):
            flags.append(flag)
    obj = work / (item["label"] + ".o")
    commands.append(["cc", "-O0", "-Wno-deprecated-declarations", *flags, "-c", source, "-o", str(obj)])
if len(commands) != 175:
    raise RuntimeError(f"Expected all 175 staged C units, found {len(commands)}")

def compile_one(command):
    result = subprocess.run(command, capture_output=True, text=True)
    (output / (Path(command[-1]).stem + ".log")).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError("Native declaration-profile compilation failed: " + command[-3])

def nm(path):
    result = subprocess.run(["nm", "-g", str(path)], capture_output=True, text=True, check=True)
    defined, undefined = set(), set()
    for line in result.stdout.splitlines():
        fields = line.split()
        if len(fields) == 2 and fields[0] == "U":
            undefined.add(fields[1])
        elif len(fields) == 3 and fields[1] not in ("U", "w", "v"):
            defined.add(fields[2])
    return defined, undefined

report = {"scope": "All selected staged C units compiled natively with the exact translation declaration flags; no product native backend",
          "status": "running", "campaign": str(campaign), "units": len(commands), "commands": commands,
          "provider_profile_sha256": hashlib.sha256(provider.read_bytes()).hexdigest()}
destination = output / "receipt.json"
destination.write_text(json.dumps(report, indent=2) + "\n")
try:
    with ThreadPoolExecutor(max_workers=args.jobs) as pool:
        list(pool.map(compile_one, commands))
    own, unresolved, sites = set(), set(), {}
    for command in commands:
        defined, undefined = nm(command[-1])
        own.update(defined)
        unresolved.update(undefined)
        for name in undefined:
            sites.setdefault(name, []).append(command[-3])
    crypto, _ = nm(ROOT / "build/native/openssl-legacy/libcrypto.a")
    required = (unresolved - own) & crypto
    report.update(status="passed" if required <= bound else "failed",
                  libcrypto_imports=sorted(required),
                  missing_bindings=sorted(required-bound),
                  bindings_without_native_import=sorted(bound-required),
                  import_units={x: sites[x] for x in sorted(required)},
                  other_imports=sorted(unresolved-own-crypto))
except Exception as error:
    report.update(status="failed", error=str(error))
    raise
finally:
    destination.write_text(json.dumps(report, indent=2) + "\n")
print(f"{len(required)} selected native libcrypto imports; {len(required-bound)} missing bindings: {destination}")
if required-bound:
    raise SystemExit(1)
