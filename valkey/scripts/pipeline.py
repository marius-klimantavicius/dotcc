"""Shared staging and command receipts for the Valkey translation campaign."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def run(command, log, receipt, *, cwd=REPO, timeout=600, check=True):
    command = [str(value) for value in command]
    log = Path(log)
    log.parent.mkdir(parents=True, exist_ok=True)
    entry = {"command": command, "cwd": str(cwd), "log": str(log.relative_to(ROOT)),
             "started": time.time()}
    receipt.setdefault("commands", []).append(entry)
    with log.open("w") as output:
        try:
            result = subprocess.run(command, cwd=cwd, stdout=output,
                                    stderr=subprocess.STDOUT, timeout=timeout)
            entry["exit_code"] = result.returncode
        except subprocess.TimeoutExpired:
            entry["timed_out"] = True
            raise RuntimeError(f"Command timed out; see {log}") from None
        finally:
            entry["seconds"] = time.time() - entry["started"]
    if check and result.returncode:
        raise RuntimeError(f"Command failed ({result.returncode}); see {log}")
    return result.returncode


def stage_source(source, destination, receipt, logs):
    shutil.copytree(source, destination)
    run([sys.executable, destination / "utils/generate-command-code.py"],
        logs / "generate-commands.log", receipt, cwd=destination)
    fmtargs = destination / "src/fmtargs.h"
    text = fmtargs.read_text()
    marker = "/* Everything below this line"
    if text.count(marker) != 1:
        raise RuntimeError("Unexpected upstream fmtargs.h generator boundary")
    run([sys.executable, destination / "utils/generate-fmtargs.py"],
        logs / "generate-fmtargs.log", receipt, cwd=destination)
    fmtargs.write_text(text.split(marker)[0] + (logs / "generate-fmtargs.log").read_text())
    commit = receipt["inputs"]["commit"]
    (destination / "src/release.h").write_text(
        f'#define REDIS_GIT_SHA1 "{commit[:8]}"\n'
        '#define REDIS_GIT_DIRTY "0"\n'
        f'#define REDIS_BUILD_ID "dotcc-valkey-{commit}"\n'
        '#include "version.h"\n'
        '#define REDIS_BUILD_ID_RAW SERVER_NAME VALKEY_VERSION REDIS_BUILD_ID REDIS_GIT_DIRTY REDIS_GIT_SHA1\n')
    receipt["generated_inputs"] = {name: sha(destination / name) for name in
                                   ("src/commands.def", "src/fmtargs.h", "src/release.h")}
    return destination


def snapshot_tools(name, destination):
    source = REPO / name / "bin/Release/net10.0"
    if not source.is_dir():
        raise RuntimeError(f"Build tools first: missing {source}")
    destination.mkdir(parents=True)
    paths = [p for p in source.iterdir() if p.suffix in (".dll", ".json")]
    identities = {p.name: sha(p) for p in paths}
    for path in paths:
        shutil.copy2(path, destination / path.name)
    if any(sha(destination / p.name) != identities[p.name] or sha(p) != identities[p.name]
           for p in paths):
        raise RuntimeError("Tools changed during snapshot; retry after builds finish")
    return identities


def write_receipt(path, receipt):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(receipt, indent=2, sort_keys=True) + "\n")
