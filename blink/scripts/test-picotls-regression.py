#!/usr/bin/env python3
"""Revalidate picotls using existing frozen tools; never build the compiler.

Preparation copies only checksum-verified archives, verifies/extracts using the
campaign recipe, builds its native oracle, and regenerates raw/optimized output.
Validation runs the existing complete Linux-x64 JIT/AOT campaign and audit.
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
CAMPAIGN = ROOT / "picotls"
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()

def tools_identity():
    files = []
    for project in ("DotCC", "DotCC.PostProcess"):
        directory = ROOT / project / "bin/Release/net10.0"
        files.extend(directory.glob("*.dll"))
        files.extend(directory.glob("*.deps.json"))
        files.extend(directory.glob("*.runtimeconfig.json"))
    for name in ("DotCC/bin/Release/net10.0/dotcc.dll", "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"):
        if not (ROOT / name).is_file():
            raise RuntimeError("Missing frozen tool: " + name)
    return {str(p.relative_to(ROOT)): sha(p) for p in sorted(set(files))}

def authored_identity():
    paths = subprocess.check_output(["git", "ls-files", "picotls"], cwd=ROOT, text=True).splitlines()
    return {name: sha(ROOT / name) for name in paths}

def save():
    (run / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

def execute(name, argv, seconds):
    command = dict(name=name, argv=[str(x) for x in argv], cwd=str(ROOT), timeout_seconds=seconds,
                   started_unix=time.time(), log=str(run / (name + ".log")))
    receipt["commands"].append(command)
    save()
    print("RUN " + name, flush=True)
    with Path(command["log"]).open("w") as log:
        process = subprocess.Popen(command["argv"], cwd=ROOT, stdout=log, stderr=subprocess.STDOUT,
                                   start_new_session=True)
        try:
            command["exit_code"] = process.wait(timeout=seconds)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGTERM)
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                os.killpg(process.pid, signal.SIGKILL)
                process.wait()
            command["exit_code"] = 124
            command["timed_out"] = True
        command["finished_unix"] = time.time()
        save()
    if tools_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise RuntimeError("Frozen tool or tracked picotls source drift during " + name)
    if command["exit_code"]:
        raise RuntimeError(name + " failed; preserved log: " + command["log"])

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--prepare-only", action="store_true")
parser.add_argument("--resume", type=Path)
parser.add_argument("--cache", type=Path, default=Path("/home/marius/p/dotcc/picotls/ref"))
args = parser.parse_args()
artifacts = ROOT / "blink/artifacts/picotls-regression"
artifacts.mkdir(parents=True, exist_ok=True)
if args.resume:
    run = args.resume.resolve()
    run.relative_to(artifacts.resolve())
    receipt = json.loads((run / "receipt.json").read_text())
    if not receipt.get("prepared") or receipt.get("passed"):
        raise SystemExit("Resume requires a prepared, uncompleted attempt")
    if tools_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise SystemExit("Cannot resume: frozen input identity changed")
else:
    run = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = dict(kind="picotls-regression-frozen-tools", passed=False, prepared=False,
                   root=str(ROOT), started_unix=time.time(), tools=tools_identity(),
                   authored=authored_identity(), runner_sha256=sha(Path(__file__)),
                   git_head=subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
                   commands=[], archives=[])
save()
print("RECEIPT " + str(run / "receipt.json"), flush=True)
try:
    if not receipt["prepared"]:
        pins = json.loads((CAMPAIGN / "config/inputs.json").read_text())
        (CAMPAIGN / "ref").mkdir(parents=True, exist_ok=True)
        for item in pins.values():
            source = args.cache / (item["directory"] + ".tar.gz")
            if sha(source) != item["sha256"]:
                raise RuntimeError("Pinned cache archive mismatch: " + str(source))
            destination = CAMPAIGN / "ref" / source.name
            if destination.exists() and sha(destination) != item["sha256"]:
                raise RuntimeError("Existing archive mismatch: " + str(destination))
            if not destination.exists():
                shutil.copyfile(source, destination)
            if sha(destination) != item["sha256"]:
                raise RuntimeError("Copied archive mismatch")
            receipt["archives"].append(dict(source=str(source), destination=str(destination), sha256=sha(destination)))
        save()
        execute("fetch-verify", [CAMPAIGN / "scripts/fetch.sh"], 120)
        execute("environment", ["dotnet", "--info"], 30)
        execute("native-oracle", [CAMPAIGN / "scripts/oracle.sh"], 1200)
        execute("regenerate", [CAMPAIGN / "scripts/translate.sh", "--no-build-tools", "--no-fetch"], 1800)
        translation = CAMPAIGN / "artifacts/translation/success.json"
        shutil.copyfile(translation, run / "translation.json")
        receipt["translation_sha256"] = sha(translation)
        receipt["prepared"] = True
        save()
    if not args.prepare_only:
        if sha(CAMPAIGN / "artifacts/translation/success.json") != receipt["translation_sha256"]:
            raise RuntimeError("Translation receipt changed before matrix")
        execute("matrix", [CAMPAIGN / "scripts/test.sh", "--all", "--aot", "--runtime", "linux-x64"], 14400)
        execute("dependency-audit", ["python3", CAMPAIGN / "scripts/audit-product.py"], 180)
        shutil.copyfile(CAMPAIGN / "artifacts/tests/PASS.json", run / "campaign-pass.json")
        receipt["passed"] = True
        receipt["completed_unix"] = time.time()
except Exception as error:
    receipt["error"] = str(error)
    raise
finally:
    receipt["tools_after"] = tools_identity()
    receipt["authored_after"] = authored_identity()
    receipt["artifacts"] = {str(p.relative_to(ROOT)): sha(p)
                            for directory in (CAMPAIGN / "artifacts", CAMPAIGN / "generated", CAMPAIGN / "build")
                            for p in sorted(directory.rglob("*")) if p.is_file()
                            and "tmp" not in p.relative_to(directory).parts
                            and "obj" not in p.relative_to(directory).parts}
    save()
print("PREPARED" if args.prepare_only else "PASS", flush=True)
