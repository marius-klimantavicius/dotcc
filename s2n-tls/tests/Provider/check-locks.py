#!/usr/bin/env python3
"""Prove deterministic provider restore locks for JIT/AOT and propagated RID."""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--update", action="store_true", help="Explicitly generate the four authored Linux profile locks first")
args = parser.parse_args()
work, output = ROOT / "build/provider/lock-policy", ROOT / "artifacts/provider/lock-policy"
work.mkdir(parents=True, exist_ok=True);output.mkdir(parents=True, exist_ok=True)
project = ROOT / "src/CryptoProvider/CryptoProvider.csproj"
metadata = json.loads((ROOT / "config/provider.json").read_text())
profiles = [("jit", [], "packages.lock.json"),
            ("jit-rid", ["-p:RuntimeIdentifier=linux-x64"], "packages.linux-x64.lock.json"),
            ("aot", ["-p:PublishAot=true"], "packages.aot.lock.json"),
            ("aot-rid", ["-p:PublishAot=true", "-p:RuntimeIdentifier=linux-x64"], "packages.aot.linux-x64.lock.json")]
report = {"scope": "Provider lock-file isolation and locked JIT/NativeAOT build; not product TLS qualification", "status": "running", "commands": []}

def save():
    (output / "receipt.json").write_text(json.dumps(report, indent=2) + "\n")

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

def run(argv, label):
    argv = list(map(str, argv));entry = {"command": argv, "log": str(output / (label + ".log"))}
    report["commands"].append(entry)
    with Path(entry["log"]).open("w") as log:
        process = subprocess.run(argv, cwd=ROOT.parent, stdout=log, stderr=subprocess.STDOUT, timeout=600)
    entry["exit_code"] = process.returncode
    if process.returncode:
        raise RuntimeError("Command failed: " + entry["log"])

save()
try:
    if args.update:
        for name, flags, filename in profiles:
            run(["dotnet", "restore", project, "--force-evaluate", "--artifacts-path", work / name, *flags], "generate-" + name)
    locks = {filename: sha(project.parent / filename) for _, _, filename in profiles}
    pinned = None
    for _, _, filename in profiles:
        data = json.loads((project.parent / filename).read_text())
        dependency = data["dependencies"]["net10.0"]["BouncyCastle.Cryptography"]
        version=metadata["managed_dependency"]["package_version"]
        if dependency["requested"] != f"[{version}, {version}]" or dependency["resolved"] != version or dependency["contentHash"]!=metadata["managed_dependency"]["package_content_hash"]:
            raise RuntimeError("Bouncy Castle version changed in " + filename)
        if pinned is None:pinned = dependency
        elif dependency != pinned:raise RuntimeError("Bouncy Castle content hash differs between profiles")
    report["locks_sha256"] = locks;report["bouncycastle"] = pinned
    # Independent restores may run concurrently, each with its own assets path.
    def locked_restore(profile):
        name, flags, _ = profile
        run(["dotnet", "restore", project, "--locked-mode", "--artifacts-path", work / name, *flags], "locked-restore-" + name)
    with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(locked_restore, profiles))
    for name, flags, _ in (profiles[0], profiles[3]):
        run(["dotnet", "build", project, "-c", "Release", "--no-restore", "--artifacts-path", work / name, "-m:1", "-p:RestoreLockedMode=true", *flags], "locked-build-" + name)
    run(["dotnet", "publish", ROOT / "tests/Provider/ProviderTests.csproj", "-c", "Release", "-r", "linux-x64", "-p:PublishAot=true", "-p:RestoreLockedMode=true", "--artifacts-path", work / "native-publish", "-o", work / "native", "-m:1"], "locked-native-publish")
    run([work / "native/ProviderTests", ROOT / "build/provider/native-vectors", output / "aot", metadata["native_version_prefix"], "shake"], "locked-native-run")
    after = {filename: sha(project.parent / filename) for _, _, filename in profiles}
    if after != locks:raise RuntimeError("Locked build mutated an authored lock")
    report["locks_unchanged"] = True;report["status"] = "passed"
except Exception as error:
    report.update(status="failed", error=str(error));save();raise
save();print("PASS isolated locked JIT/AOT/RID restores and builds:", output / "receipt.json")
