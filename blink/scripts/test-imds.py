#!/usr/bin/env python3
"""Qualify private IMDSv2 with the real AWS SDK guest in both machine modes."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
BLINK = ROOT / "blink"
PROJECT = BLINK / "tests/ImdsMachine/ImdsMachine.csproj"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def input_identity():
    paths = set()
    for directory in (BLINK / "src", BLINK / "generated/TranslatedBlink", ROOT / "DotCC.Libc"):
        for path in directory.rglob("*"):
            if path.is_file() and not {"bin", "obj"}.intersection(path.relative_to(directory).parts) and path.suffix in (".cs", ".csproj", ".json", ".props", ".targets"):
                paths.add(path)
    for directory in (ROOT / "DotCC/bin/Release/net10.0", ROOT / "DotCC.PostProcess/bin/Release/net10.0"):
        paths.update(directory.glob("*.dll"))
    latest = BLINK / "artifacts/translation/latest.json"
    if latest.exists():
        paths.add(latest)
        paths.add(Path(json.loads(latest.read_text())["receipt"]))
    return {str(path.relative_to(ROOT)): sha(path) for path in sorted(paths)}


def stop_group(process):
    for sig in (signal.SIGTERM, signal.SIGKILL):
        try:
            os.killpg(process.pid, sig)
        except ProcessLookupError:
            break
        try:
            process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            continue
        if sig == signal.SIGTERM:
            # A parent may exit before its worker: reap the owned group too.
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
        break


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--elf", type=Path, default=BLINK / "build/imds-nativeaot/ImdsGuest")
    parser.add_argument("--skip-build", action="store_true", help="Use existing JIT and AOT consumer builds")
    parser.add_argument("--jit-only", action="store_true", help="Development check; does not qualify NativeAOT consumers")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    artifacts = BLINK / "artifacts/imds"
    artifacts.mkdir(parents=True, exist_ok=True)
    output = args.output.resolve() if args.output else Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    output.mkdir(parents=True, exist_ok=True)
    if (output / "receipt.json").exists():
        raise RuntimeError("Refusing to overwrite an existing attempt receipt")
    elf = args.elf.resolve()
    jit = PROJECT.parent / "bin/Release/net10.0/ImdsMachine.dll"
    aot_dir = BLINK / "build/imds-machine-aot"
    aot = aot_dir / "ImdsMachine"
    receipt = {"passed": False, "jit_only": args.jit_only, "commands": [], "sources": {}}
    print(output, flush=True)

    def save():
        (output / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

    def run(name, argv, timeout=900):
        row = {"name": name, "argv": list(map(str, argv)), "cwd": str(ROOT)}
        receipt["commands"].append(row)
        started = time.monotonic()
        save()
        print("RUN " + name, flush=True)
        try:
            with (output / (name + ".log")).open("wb") as log:
                process = subprocess.Popen(row["argv"], cwd=ROOT, stdout=log,
                    stderr=subprocess.STDOUT, start_new_session=True)
                try:
                    row["exit_code"] = process.wait(timeout=timeout)
                except BaseException:
                    stop_group(process)
                    raise
        finally:
            row["elapsed_seconds"] = time.monotonic() - started
            row["log_sha256"] = sha(output / (name + ".log"))
            save()
        if row["exit_code"] != 0:
            raise RuntimeError(f"{name} failed; see {output / (name + '.log')}")
        print("PASS " + name, flush=True)

    def check_runtime(name, native):
        result = json.loads((output / name / "result.json").read_text())
        if not result["passed"] or result["consumer_dynamic_code_supported"] == native:
            raise RuntimeError("Consumer runtime does not match requested qualification: " + name)
        if native and name.endswith("SeparateProcess"):
            expected = sha(aot_dir / "blink-worker/Managed.Emulation.Worker")
            if any(row["worker_executable_sha256"] != expected for row in result["runs"]):
                raise RuntimeError("Actual worker executable differs from the qualified NativeAOT binary")

    try:
        receipt["guest_sha256"] = sha(elf)
        receipt["inputs_before"] = input_identity()
        delivery = json.loads((BLINK / "artifacts/translation/latest.json").read_text())
        delivery_path = Path(delivery["receipt"])
        if sha(delivery_path) != delivery["sha256"] or not json.loads(delivery_path.read_text())["passed"]:
            raise RuntimeError("The current translated delivery receipt is not a valid pass")
        receipt["translation_delivery"] = delivery
        for folder in (BLINK / "tests/ImdsGuest", BLINK / "tests/HostImds", PROJECT.parent):
            for path in folder.iterdir():
                if path.is_file(): receipt["sources"][str(path.relative_to(ROOT))] = sha(path)
        receipt["sources"][str(Path(__file__).resolve().relative_to(ROOT))] = sha(Path(__file__))
        run("elf-headers", ["readelf", "-h", "-l", "-d", elf])
        headers = (output / "elf-headers.log").read_text()
        if "EXEC (Executable file)" not in headers or "Advanced Micro Devices X86-64" not in headers or "INTERP" in headers or "(NEEDED)" in headers:
            raise RuntimeError("Expected static x64 non-PIE ELF")
        if not args.skip_build:
            run("build-jit", ["dotnet", "build", PROJECT, "-c", "Release", "--nologo"])
        run("host-jit", ["dotnet", "run", "--project", BLINK / "tests/HostImds", "-c", "Release"])
        run("native-oracle", ["dotnet", BLINK / "tests/HostImds/bin/Release/net10.0/HostImds.dll", elf], timeout=90)
        for mode in ("InProcess", "SeparateProcess"):
            run("jit-" + mode, ["dotnet", jit, elf, mode, output / ("jit-" + mode)], timeout=300)
            check_runtime("jit-" + mode, native=False)
        if not args.jit_only:
            host_aot = BLINK / "build/host-imds-aot"
            run("host-publish-aot", ["dotnet", "publish", BLINK / "tests/HostImds", "-c", "Release", "-r", "linux-x64",
                "--self-contained", "true", "-p:PublishAot=true", "-o", host_aot, "--nologo"])
            run("host-aot", [host_aot / "HostImds"])
            if not args.skip_build:
                run("publish-aot", ["dotnet", "publish", PROJECT, "-c", "Release", "-r", "linux-x64",
                    "--self-contained", "true", "-p:PublishAot=true", "-o", aot_dir, "--nologo"], timeout=1800)
            run("aot-elf-headers", ["readelf", "-h", aot, aot_dir / "blink-worker/Managed.Emulation.Worker"])
            for mode in ("InProcess", "SeparateProcess"):
                run("aot-" + mode, [aot, elf, mode, output / ("aot-" + mode)], timeout=300)
                check_runtime("aot-" + mode, native=True)
        receipt["jit_sha256"] = sha(jit)
        receipt["managed_closure"] = {
            str(path.relative_to(jit.parent)): sha(path)
            for path in sorted(jit.parent.rglob("*.dll"))
        }
        if not args.jit_only:
            receipt["aot_sha256"] = sha(aot)
            receipt["aot_worker_sha256"] = sha(aot_dir / "blink-worker/Managed.Emulation.Worker")
        receipt["passed"] = True
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
        raise
    finally:
        receipt["inputs_after"] = input_identity()
        receipt["inputs_unchanged"] = receipt.get("inputs_before") == receipt["inputs_after"]
        receipt["guest_unchanged"] = receipt.get("guest_sha256") == sha(elf)
        receipt["fixture_sources_unchanged"] = all(sha(ROOT / path) == digest for path, digest in receipt["sources"].items())
        if not all(receipt[key] for key in ("inputs_unchanged", "guest_unchanged", "fixture_sources_unchanged")):
            receipt["passed"] = False
            receipt.setdefault("error", "Guest, fixture, source or compiler identity changed during qualification")
        save()
    if not receipt["passed"]:
        raise RuntimeError(receipt["error"])


if __name__ == "__main__":
    main()
