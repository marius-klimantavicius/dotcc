#!/usr/bin/env python3
"""Serially qualify the separate owning consumer against raw/optimized JIT/AOT libraries."""
import argparse
import hashlib
import json
from pathlib import Path
import platform
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]


def run(command, log, timeout):
    print("Running: " + " ".join(map(str, command)), flush=True)
    started = time.monotonic()
    with log.open("w") as output:
        try:
            result = subprocess.run(list(map(str, command)), stdout=output,
                stderr=subprocess.STDOUT, cwd=ROOT.parent, timeout=timeout)
            code = result.returncode
        except subprocess.TimeoutExpired:
            code = 124
    text = log.read_text(errors="replace")
    print(text[-10000:], end="", flush=True)
    return {"command": list(map(str, command)), "exit_code": code,
        "seconds": round(time.monotonic() - started, 3),
        "log": str(log.relative_to(ROOT)), "sha256": hashlib.sha256(log.read_bytes()).hexdigest(),
        "pass_lines": sum(line.startswith("PASS ") for line in text.splitlines())}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--form", choices=["all", "raw", "optimized"], default="all")
    parser.add_argument("--mode", choices=["all", "jit", "aot"], default="all")
    parser.add_argument("--profile", choices=["release", "debug"], default="release")
    args = parser.parse_args()
    machine = platform.machine().lower()
    if machine not in ("x86_64", "amd64"):
        raise RuntimeError("This campaign currently targets x64 only")
    rid = "win-x64" if sys.platform == "win32" else "linux-x64" if sys.platform.startswith("linux") else None
    if rid is None:
        raise RuntimeError("Linux and Windows execution are the required platform matrix")
    # Verify the exact translated inputs and output hashes before consuming the products.
    subprocess.run([sys.executable, str(ROOT / "scripts/dependency-audit.py"), "--profile", args.profile, "--form", "processed" if args.form == "optimized" else args.form], check=True)
    project = ROOT / "samples/ManagedConsumer/ManagedConsumer.csproj"
    fixtures = ROOT / "ref/upstream/Marius.Pinta.Test.Files"
    forms = ["raw", "optimized"] if args.form == "all" else [args.form]
    modes = ["jit", "aot"] if args.mode == "all" else [args.mode]
    receipts = []
    variant = Path() if args.profile == "release" else Path("profiles/debug")
    artifacts = ROOT / "artifacts/managed" / variant / rid
    artifacts.mkdir(parents=True, exist_ok=True)
    receipt = {"platform": platform.platform(), "rid": rid, "profile": args.profile, "runs": receipts,
        "source_lock_sha256": hashlib.sha256((ROOT / "config/source-lock.json").read_bytes()).hexdigest(),
        "authored_receipt_sha256": hashlib.sha256((ROOT / "tests/fixtures/receipt.pint").read_bytes()).hexdigest(),
        "authored_callback_sha256": hashlib.sha256((ROOT / "tests/fixtures/callback-return.pint").read_bytes()).hexdigest(),
        "authored_module_variants_sha256": {name: hashlib.sha256((ROOT / "tests/fixtures" / name).read_bytes()).hexdigest()
            for name in ("receipt-swapped.pint", "invalid-opcode.pint")},
        "host_consumer_sha256": {str(path.relative_to(ROOT)): hashlib.sha256(path.read_bytes()).hexdigest()
            for folder in (ROOT / "src/ManagedApi", ROOT / "samples/ManagedConsumer")
            for path in sorted(folder.iterdir()) if path.suffix in (".cs", ".csproj")},
        "qualification_scope": "owning consumer ABI/globals/callbacks/lifetime; not the complete P0-P6 corpus"}
    reference_stdout = None
    try:
        for form in forms:
            library = ROOT / "generated" / variant / ("TranslatedPinta.Raw" if form == "raw" else "TranslatedPinta") / "TranslatedPinta.csproj"
            for mode in modes:
                output = ROOT / "build/managed" / variant / rid / form / mode
                common = ["-c", "Release", "--nologo", "--artifacts-path", str(output / "intermediate"),
                    f"-p:PintaProject={library}"]
                if mode == "jit":
                    command = ["dotnet", "build", project, *common, "-o", output / "app"]
                    invocation = ["dotnet", output / "app/ManagedConsumer.dll", fixtures, ROOT / "tests/fixtures/receipt.pint", ROOT / "tests/fixtures/callback-return.pint"]
                else:
                    command = ["dotnet", "publish", project, *common, "-r", rid,
                        "-p:PublishAot=true", "-o", output / "app"]
                    invocation = [output / "app" / ("ManagedConsumer.exe" if sys.platform == "win32" else "ManagedConsumer"), fixtures, ROOT / "tests/fixtures/receipt.pint", ROOT / "tests/fixtures/callback-return.pint"]
                build = run(command, artifacts / f"{form}-{mode}-build.log", 900)
                item = {"form": form, "mode": mode, "build": build}
                receipts.append(item)
                if build["exit_code"]:
                    return build["exit_code"]
                result = run(invocation, artifacts / f"{form}-{mode}-run.log", 120)
                item["run"] = result
                executable = Path(invocation[1] if mode == "jit" else invocation[0])
                item["executable"] = {"bytes": executable.stat().st_size,
                    "sha256": hashlib.sha256(executable.read_bytes()).hexdigest()}
                if result["exit_code"]:
                    return result["exit_code"]
                stdout = (artifacts / f"{form}-{mode}-run.log").read_bytes()
                if reference_stdout is None:
                    reference_stdout = stdout
                item["matches_first_form_stdout"] = stdout == reference_stdout
                if stdout != reference_stdout:
                    print("Consumer output differs across forms", file=sys.stderr)
                    return 1
                if mode == "jit":
                    audit = run([sys.executable, ROOT / "scripts/dependency-audit.py", "--profile", args.profile, "--form", "processed" if form == "optimized" else form, "--deps",
                        output / "app/ManagedConsumer.deps.json"], artifacts / f"{form}-{mode}-audit.log", 60)
                    item["dependency_audit"] = audit
                    if audit["exit_code"]:
                        return audit["exit_code"]
    finally:
        (artifacts / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
