#!/usr/bin/env python3
"""Qualify regenerated MsQuic with frozen tools and the qualified picotls product.

Default prepares fresh ABI/product gates. --finish <attempt> freezes their
reviewable tracked closure via the existing recipe, then tests the actual public
owning consumer under raw/optimized JIT/NativeAOT. Never builds compiler tools.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / "msquic"
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
CLOSURE = "msquic/config/product-closure.json"

def tool_identity():
    files = []
    for project in ("DotCC", "DotCC.PostProcess"):
        directory = ROOT / project / "bin/Release/net10.0"
        files += list(directory.glob("*.dll")) + list(directory.glob("*.deps.json")) + list(directory.glob("*.runtimeconfig.json"))
    return {str(p.relative_to(ROOT)): sha(p) for p in sorted(set(files))}

def authored_identity():
    names = subprocess.check_output(["git", "ls-files", "msquic", "picotls"], cwd=ROOT, text=True).splitlines()
    return {n: sha(ROOT / n) for n in names if n != CLOSURE}

def save():
    (run / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

def execute(name, argv, seconds):
    entry = dict(name=name, argv=[str(x) for x in argv], cwd=str(ROOT), started_unix=time.time(),
                 timeout_seconds=seconds, log=str(run / (name + ".log")))
    receipt["commands"].append(entry)
    save()
    print("RUN " + name, flush=True)
    with Path(entry["log"]).open("w") as output:
        process = subprocess.Popen(entry["argv"], cwd=ROOT, stdout=output, stderr=subprocess.STDOUT, start_new_session=True)
        try:
            entry["exit_code"] = process.wait(timeout=seconds)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGTERM)
            try: process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                os.killpg(process.pid, signal.SIGKILL)
                process.wait()
            entry.update(exit_code=124, timed_out=True)
    entry["finished_unix"] = time.time()
    save()
    if tool_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise RuntimeError("Tool/source drift during " + name)
    if sha(ROOT / "picotls/artifacts/translation/success.json") != receipt["picotls_translation_sha256"]:
        raise RuntimeError("Qualified picotls translation drift")
    if entry["exit_code"]:
        raise RuntimeError(name + " failed; see " + entry["log"])

def verified_copy(source, destination, expected):
    if sha(source) != expected: raise RuntimeError("Cache mismatch: " + str(source))
    if destination.exists() and sha(destination) != expected:
        raise RuntimeError("Refusing overwrite: " + str(destination))
    destination.parent.mkdir(parents=True, exist_ok=True)
    if not destination.exists(): shutil.copy2(source, destination)
    if sha(source) != expected or sha(destination) != expected:
        raise RuntimeError("Copy identity changed: " + str(source))
    receipt["copied"].append(dict(source=str(source), destination=str(destination), sha256=expected))

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--finish", type=Path)
parser.add_argument("--cache-root", type=Path, default=Path("/home/marius/p/dotcc/msquic"))
args = parser.parse_args()
artifacts = ROOT / "blink/artifacts/msquic-regression"
artifacts.mkdir(parents=True, exist_ok=True)
if args.finish:
    run = args.finish.resolve()
    run.relative_to(artifacts.resolve())
    receipt = json.loads((run / "receipt.json").read_text())
    if not receipt.get("prepared") or receipt.get("passed"):
        raise SystemExit("Finish requires prepared, unfinished evidence")
    if receipt["tools"] != tool_identity() or receipt["authored"] != authored_identity():
        raise SystemExit("Frozen input identity changed before finish")
else:
    run = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = dict(kind="msquic-regression-frozen-tools", passed=False, prepared=False,
                   started_unix=time.time(), runner_sha256=sha(Path(__file__)),
                   tools=tool_identity(), authored=authored_identity(),
                   picotls_translation_sha256=sha(ROOT / "picotls/artifacts/translation/success.json"),
                   closure_before_sha256=sha(ROOT / CLOSURE), commands=[], copied=[])
save()
print("RECEIPT " + str(run / "receipt.json"), flush=True)
try:
    if not args.finish:
        # Prove the existing generator's tracked writes are byte-identical before
        # invoking recipes that regenerate those declarations in place.
        mirror = run / "generator-preflight"
        for name in ("scripts/generate-host-contract.py", "config/managed-host/operations.json"):
            destination = mirror / name
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(CAMPAIGN / name, destination)
        for name in ("src/Host", "tests/HostContract"): (mirror / name).mkdir(parents=True)
        execute("generator-preflight", ["python3", mirror / "scripts/generate-host-contract.py"], 30)
        for name in ("src/Host/msquic_host.h", "src/Host/forwarders.c", "src/Host/required_slots.inc", "tests/HostContract/callbacks.inc"):
            if sha(mirror / name) != sha(CAMPAIGN / name):
                raise RuntimeError("Generator would change tracked source: " + name)
        pin = json.loads((CAMPAIGN / "config/source.json").read_text())
        verified_copy(args.cache_root / "ref" / pin["archive"], CAMPAIGN / "ref" / pin["archive"], pin["sha256"])
        previous = json.loads((ROOT / CLOSURE).read_text())
        # Preserve actual old closure-bound local evidence via the unchanged
        # archival recipe; none of these files qualify the new generated core.
        historical = {"build/product-source/manifest.json": previous["stage_manifest_sha256"], **previous["evidence_sha256"]}
        historical.update({"build/host-contract/compiler/" + n: h for n, h in previous["compiler_hashes"].items()})
        for variant, files in previous["generated"].items():
            historical.update({previous["generated_directories"][variant] + "/" + n: h for n, h in files.items()})
        for name, expected in historical.items(): verified_copy(args.cache_root / name, CAMPAIGN / name, expected)
        save()
        execute("fetch-verify", ["python3", CAMPAIGN / "scripts/fetch.py"], 120)
        execute("fast-regenerate", [CAMPAIGN / "scripts/translate.sh", "--fast", "--no-build-tools", "--jobs", "2"], 3600)
        execute("host-contract", ["python3", CAMPAIGN / "scripts/test-host-contract.py"], 7200)
        execute("public-abi", ["python3", CAMPAIGN / "scripts/test-abi.py", "--groups", "public"], 1200)
        execute("product-gates", ["python3", CAMPAIGN / "scripts/build-product.py"], 3600)
        receipt["prepared"] = True
    else:
        if sha(ROOT / CLOSURE) != receipt["closure_before_sha256"]:
            raise RuntimeError("Closure changed before reviewed freeze")
        shutil.copyfile(ROOT / CLOSURE, run / "closure-before.json")
        execute("freeze-qualified-closure", ["python3", CAMPAIGN / "scripts/freeze-product.py", "--without-sqlite"], 600)
        shutil.copyfile(ROOT / CLOSURE, run / "closure-after.json")
        receipt["closure_after_sha256"] = sha(ROOT / CLOSURE)
        save()
        execute("public-consumer-all4", ["python3", CAMPAIGN / "scripts/test-public-consumer.py"], 7200)
        receipt["passed"] = True
        receipt["completed_unix"] = time.time()
except Exception as error:
    receipt["error"] = str(error)
    raise
finally:
    receipt["tools_after"] = tool_identity()
    receipt["authored_after"] = authored_identity()
    receipt["artifacts"] = {str(p.relative_to(ROOT)): sha(p)
                            for directory in (CAMPAIGN / "artifacts", CAMPAIGN / "generated", CAMPAIGN / "build")
                            for p in sorted(directory.rglob("*")) if p.is_file()
                            and "obj" not in p.relative_to(directory).parts}
    save()
print("PASS" if args.finish else "PREPARED", flush=True)
