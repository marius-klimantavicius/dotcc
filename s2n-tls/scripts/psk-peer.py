"""Build an explicitly labelled test CLI with pinned, unchanged TLS libraries."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.identity import HashPolicy
from campaigns.inputs import acquire
from campaigns.recipes import load


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fetch", choices=("missing", "never"), default="never")
    args = parser.parse_args()
    policy_path = ROOT / "config/peer-adaptations.json"
    policy = json.loads(policy_path.read_text())
    sources = {source.name: source for source in load(ROOT.parent, ROOT.name).sources(ROOT)}
    source = sources[policy["source"]]
    acquire(source, HashPolicy("warn"), args.fetch)
    pin_path = ROOT / "config/inputs.json"
    pin = json.loads(pin_path.read_text())[source.name]
    base = ROOT / "build/native" / source.name
    # No native TLS build fallback, fetch runner, or modification of the shared
    # control tree. The normal native acquisition/build owns these prerequisites.
    required = ["libssl.a", "libcrypto.a", "Makefile", "configdata.pm", "apps/openssl"]
    inputs = {str(path): digest(path) for path in
              [policy_path, ROOT / policy["header"], source.archive,
               *[base / name for name in required]]}
    if digest(source.archive) != pin["sha256"]:
        raise RuntimeError("Pinned independent peer archive hash differs")
    for name in {row["path"] for row in policy["replacements"]}:
        if (base / name).read_bytes() != (source.directory / name).read_bytes():
            raise RuntimeError("Base peer CLI is not pristine: " + name)
        inputs[str(source.directory / name)] = digest(source.directory / name)
        inputs[str(base / name)] = digest(base / name)
    fingerprint = hashlib.sha256(json.dumps({"inputs": inputs, "pin": pin}, sort_keys=True).encode()).hexdigest()
    work = ROOT / "build/native/openssl-peer-psk" / fingerprint
    output = ROOT / "artifacts/native-peer-psk" / fingerprint
    receipt_path = output / "receipt.json"
    if receipt_path.is_file():
        previous = json.loads(receipt_path.read_text())
        if previous["status"] != "passed":
            raise RuntimeError("Prior derived peer attempt did not pass: " + str(receipt_path))
        for name, expected in {**previous["inputs"], **previous["outputs"]}.items():
            if digest(Path(name)) != expected:
                raise RuntimeError("Immutable derived peer input/output changed: " + name)
        print(receipt_path)
        return
    if work.exists():
        raise RuntimeError("Unreceipted derived peer directory already exists: " + str(work))
    output.mkdir(parents=True)
    receipt = {"status": "running", "label": policy["label"], "scope": policy["scope"],
               "pin": pin, "inputs": inputs, "commands": [], "adaptations": [],
               "base": str(base), "derived": str(work), "fingerprint": fingerprint}

    def save():
        receipt_path.write_text(json.dumps(receipt, indent=2) + "\n")

    save()
    try:
        shutil.copytree(base, work, symlinks=True)
        header = work / policy["staged_header"]
        shutil.copyfile(ROOT / policy["header"], header)
        for row in policy["replacements"]:
            path = work / row["path"]
            text = path.read_text()
            if text.count(row["old"]) != 1:
                raise RuntimeError("Expected unique peer adaptation: " + row["path"])
            before = digest(path)
            path.write_text(text.replace(row["old"], row["new"]))
            receipt["adaptations"].append({**row, "original_sha256": before, "derived_sha256": digest(path)})
        command = ["make", "-j2", "apps/openssl"]
        log = output / "build.log"
        with log.open("w") as stream:
            result = subprocess.run(command, cwd=work, stdout=stream, stderr=subprocess.STDOUT, timeout=600)
        receipt["commands"].append({"argv": command, "cwd": str(work), "exit_code": result.returncode, "log": str(log)})
        if result.returncode:
            raise RuntimeError("Derived test peer build failed: " + str(log))
        for name in ("libssl.a", "libcrypto.a"):
            if digest(work / name) != inputs[str(base / name)]:
                raise RuntimeError("Test CLI build modified a TLS library: " + name)
        for name, expected in inputs.items():
            if digest(Path(name)) != expected:
                raise RuntimeError("Derived peer build changed an input: " + name)
        receipt["outputs"] = {str(work / name): digest(work / name) for name in
                              [*required, policy["staged_header"], *sorted({row["path"] for row in policy["replacements"]})]}
        receipt["status"] = "passed"
    except BaseException as error:
        receipt["status"] = "failed"
        receipt["error"] = str(error)
        raise
    finally:
        save()
    print(receipt_path)


if __name__ == "__main__":
    main()
