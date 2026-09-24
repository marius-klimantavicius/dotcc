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


def apply_managed_profile(destination, receipt, *, root=ROOT):
    """Apply reviewed exact-input edits only to an isolated staging tree."""
    destination, root = Path(destination), Path(root)
    specification = root / "config/managed-adaptations.json"
    profile = json.loads(specification.read_text())
    if profile["commit"] != receipt["inputs"]["commit"]:
        raise RuntimeError("Managed adaptations do not match the verified source commit")
    pending = []
    identities = []

    def child(base, name):
        path = base / name
        if Path(name).is_absolute() or ".." in Path(name).parts or not path.resolve().is_relative_to(base.resolve()):
            raise RuntimeError(f"Unsafe managed adaptation path: {name}")
        return path

    # Validate every input and edit before changing any file. A partial profile
    # must never be mistaken for the reviewed managed configuration.
    for entry in profile["adaptations"]:
        path = child(destination, entry["path"])
        before = sha(path)
        if before != entry["sha256"]:
            raise RuntimeError(f"Managed adaptation input hash mismatch: {entry['path']}")
        text = path.read_text()
        for replacement in entry["replacements"]:
            if text.count(replacement["before"]) != 1:
                raise RuntimeError(f"Managed adaptation match is not unique: {entry['path']}")
            text = text.replace(replacement["before"], replacement["after"], 1)
        pending.append((path, text.encode()))
        identities.append({"path": entry["path"], "before_sha256": before,
                           "after_sha256": hashlib.sha256(text.encode()).hexdigest(),
                           "purpose": entry["purpose"]})
    injected = []
    for entry in profile["injected_files"]:
        source = child(root, entry["source"])
        target = child(destination, entry["path"])
        if target.exists():
            raise RuntimeError(f"Managed adaptation injection already exists: {entry['path']}")
        data = source.read_bytes()
        pending.append((target, data))
        injected.append({"source": entry["source"], "path": entry["path"],
                         "sha256": hashlib.sha256(data).hexdigest()})
    for path, data in pending:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
    receipt["managed_profile"] = {
        "name": profile["name"], "specification_sha256": sha(specification),
        "adaptations": identities, "injected_files": injected,
        "extra_sources": profile["extra_sources"],
        "qualification": "staged source adaptations; embedded lifecycle not yet qualified",
    }


def stage_source(source, destination, receipt, logs, *, managed_profile=False):
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
    if managed_profile:
        apply_managed_profile(destination, receipt)
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
