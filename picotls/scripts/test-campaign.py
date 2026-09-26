#!/usr/bin/env python3
"""Test existing picotls translations serially; never fetch or translate inputs.

Default: optimized JIT. --raw selects raw JIT; --all compares both variants.
--aot additionally runs NativeAOT for every selected variant. The validated
native ABI/oracle profile is currently Linux x64 only.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shlex
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, provenance
REPO = ROOT.parent


def digest(path):
    return None if policy().mode == "off" else hashlib.sha256(path.read_bytes()).hexdigest()


def public_result(suite, output):
    if suite != "TlsTests":
        return output
    # Fresh certificates and ECDSA signature lengths change fragment-pump
    # assertion counts. Preserve all scenario text and the complete raw log;
    # only this incidental counter is excluded from cross-run comparison.
    pattern = r"^PASS: [0-9]+ TLS assertions;(?= )"
    if len(re.findall(pattern, output, re.MULTILINE)) != 1:
        raise RuntimeError("TlsTests must emit exactly one successful scenario summary")
    return re.sub(pattern, "PASS: TLS assertions;", output, flags=re.MULTILINE)


def generated_hashes(project):
    names = (project.parent / "Dotcc.SourceFiles.txt").read_text().splitlines()
    if not names or len(names) != len(set(names)):
        raise RuntimeError(f"Invalid generated source manifest: {project.parent}")
    for name in names:
        if not re.fullmatch(r"[A-Za-z_][A-Za-z_0-9.]*\.cs", name) or ".." in name:
            raise RuntimeError(f"Unsafe generated source name: {name!r}")
    return {name: digest(project.parent / name)
            for name in sorted(names + [project.name, "Dotcc.SourceFiles.txt"])}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    selection = parser.add_mutually_exclusive_group()
    selection.add_argument("--raw", action="store_true")
    selection.add_argument("--all", action="store_true", help="test raw and optimized, then compare deterministic results")
    parser.add_argument("--aot", action="store_true", help="also publish and execute NativeAOT")
    parser.add_argument("--runtime", default=os.environ.get("PICOTLS_RUNTIME", "linux-x64"))
    args = parser.parse_args()
    artifacts = ROOT / "artifacts/tests"
    artifacts.mkdir(parents=True, exist_ok=True)
    receipt = artifacts / "PASS.json"
    receipt.unlink(missing_ok=True)
    run = Path(tempfile.mkdtemp(prefix="run-", dir=artifacts))
    temporary = run / "tmp"
    temporary.mkdir()
    env = dict(os.environ, TMPDIR=str(temporary))
    variants = ["raw", "optimized"] if args.all else ["raw" if args.raw else "optimized"]
    projects = {variant: ROOT / "generated" / ("TranslatedPicotls.Raw" if variant == "raw" else "TranslatedPicotls") / "TranslatedPicotls.csproj"
                for variant in variants}
    commands = []
    results = {}

    def execute(name, argv, timeout):
        argv = [str(value) for value in argv]
        stdout, stderr = run / (name + ".stdout"), run / (name + ".stderr")
        entry = {"name": name, "argv": argv, "command": shlex.join(argv), "cwd": str(ROOT),
                 "stdout": stdout.name, "stderr": stderr.name, "timeout_seconds": timeout,
                 "started_unix": time.time()}
        commands.append(entry)
        (run / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
        print(f"RUN {name}", flush=True)
        with stdout.open("w") as output, stderr.open("w") as errors:
            with subprocess.Popen(argv, cwd=ROOT, env=env, stdout=output, stderr=errors,
                                  start_new_session=True) as process:
                try:
                    entry["exit_code"] = process.wait(timeout=timeout)
                except subprocess.TimeoutExpired:
                    entry["timed_out"] = True
                    os.killpg(process.pid, signal.SIGTERM)
                    try:
                        process.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        os.killpg(process.pid, signal.SIGKILL)
                        process.wait()
                    entry["exit_code"] = 124
                finally:
                    entry["finished_unix"] = time.time()
                    (run / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
        if entry["exit_code"] != 0:
            raise RuntimeError(f"{name} failed (exit {entry['exit_code']}); see {stdout} and {stderr}")
        return stdout.read_text()

    try:
        if args.runtime != "linux-x64" or platform.system() != "Linux" or platform.machine() not in ("x86_64", "AMD64"):
            raise RuntimeError("This native ABI/oracle campaign currently runs on Linux x64 with --runtime linux-x64")
        build_timeout = int(os.environ.get("PICOTLS_BUILD_TIMEOUT", "900"))
        test_timeout = int(os.environ.get("PICOTLS_TEST_TIMEOUT", "180"))
        peer_timeout = int(os.environ.get("PICOTLS_PEER_TIMEOUT", "3600"))
        if min(build_timeout, test_timeout, peer_timeout) <= 0:
            raise RuntimeError("Campaign timeout values must be positive seconds")
        for project in projects.values():
            if not project.is_file():
                raise RuntimeError(f"Missing translated product {project}; run scripts/translate.sh first")
        translation_record = ROOT / "artifacts/campaign/current-default.json"
        provenance(ROOT, "TranslatedPicotls", "default", forms=tuple("processed" if v == "optimized" else v for v in variants))
        inputs = json.loads((ROOT / "config/inputs.json").read_text())
        source = ROOT / "ref" / inputs["picotls"]["directory"]
        if not (source / "include/picotls.h").is_file():
            raise RuntimeError("Missing pinned native headers; prepare scripts/fetch.sh explicitly")
        oracle_inputs = [ROOT / "build/oracle" / name for name in ("libpicotls-core.a", "libpicotls-openssl.a")]
        if not all(path.is_file() for path in oracle_inputs):
            raise RuntimeError("Missing native reference libraries; prepare scripts/oracle.sh explicitly")
        core_paths = [source / name for name in (ROOT / "config/core-sources.txt").read_text().splitlines()
                      if name.strip() and not name.startswith("#")]
        def input_hashes():
            paths = {*oracle_inputs, *core_paths}
            if translation_record.exists(): paths.add(translation_record)
            for directory in (ROOT / "config", ROOT / "scripts", ROOT / "src", ROOT / "tests", ROOT / "samples/ManagedConsumer",
                              source / "include", *(project.parent for project in projects.values())):
                paths.update(path for path in directory.rglob("*") if path.is_file()
                             and not {"bin", "obj", "__pycache__"}.intersection(path.relative_to(directory).parts)
                             and path.suffix in (".cs", ".c", ".h", ".csproj", ".props", ".targets", ".json", ".txt", ".py", ".sh"))
            paths.update(REPO.glob("Directory.*.props"))
            paths.update(REPO.glob("Directory.*.targets"))
            paths.update(REPO / name for name in ("global.json", "NuGet.Config") if (REPO / name).is_file())
            return {"files": {str(path.relative_to(REPO)): digest(path) for path in sorted(paths)},
                    "generated": {variant: generated_hashes(project) for variant, project in projects.items()}}

        initial = input_hashes()
        (run / "inputs.json").write_text(json.dumps(initial, indent=2) + "\n")
        execute("environment", ["dotnet", "--info"], 30)
        native = run / "native-layout"
        definitions = ["-D" + line.strip() for line in (ROOT / "config/core-defines.txt").read_text().splitlines()
                       if line.strip() and not line.lstrip().startswith("#")]
        execute("native-layout-build", ["cc", "-std=c11", *definitions, "-I", source / "include", ROOT / "tests/native-layout.c", "-o", native], build_timeout)
        execute("native-layout", [native], test_timeout)
        native_values = run / "native-layout.stdout"
        native_checks = native_values.read_text().splitlines()
        if len(native_checks) != 92 or len({line.split("=", 1)[0] for line in native_checks}) != 92:
            raise RuntimeError("Native ABI oracle did not produce the expected 92 distinct checks")
        suites = ("CopiedConsumer", "TranslatedAbi", "ProviderVectors", "UpstreamVectors", "TlsTests")
        for variant, product in projects.items():
            properties = ["-p:PicotlsProject=" + str(product)]
            for suite in suites:
                project = ROOT / "tests" / suite / (suite + ".csproj")
                prefix = variant + "-" + suite
                execute(prefix + "-build", ["dotnet", "build", project, "-c", "Release", "--nologo", *properties], build_timeout)
                arguments = [native_values] if suite == "TranslatedAbi" else []
                results[prefix + "-jit"] = public_result(suite, execute(prefix + "-jit", ["dotnet", project.parent / "bin/Release/net10.0" / (suite + ".dll"), *arguments], test_timeout))
                if not results[prefix + "-jit"].strip().startswith("PASS"):
                    raise RuntimeError(f"{prefix} returned without its successful test summary")
                if args.aot:
                    output = run / "publish" / variant / suite
                    execute(prefix + "-publish", ["dotnet", "publish", project, "-c", "Release", "-r", args.runtime,
                                                   "-p:PublishAot=true", *properties, "-o", output, "--nologo"], build_timeout)
                    results[prefix + "-aot"] = public_result(suite, execute(prefix + "-aot", [output / suite, *arguments], test_timeout))
                    if results[prefix + "-jit"] != results[prefix + "-aot"]:
                        raise RuntimeError(f"{prefix} deterministic JIT/NativeAOT result differs")
            for mode in (["jit", "aot"] if args.aot else ["jit"]):
                peer_receipt = run / f"{variant}-peer-{mode}.json"
                peer_args = [ROOT / "scripts/test-managed-peer.sh", "--no-prepare", "--runtime", args.runtime, "--receipt", peer_receipt]
                if variant == "raw": peer_args.append("--raw")
                if mode == "aot": peer_args.append("--aot")
                execute(f"{variant}-peer-{mode}", peer_args, peer_timeout)
                peers = json.loads(peer_receipt.read_text())
                if peers["raw"] != (variant == "raw") or peers["aot"] != (mode == "aot") or peers["runtime"] != args.runtime:
                    raise RuntimeError("Peer receipt does not match the requested execution variant")
                results[f"{variant}-peer-{mode}"] = peers["cases"]
            if args.aot and results[f"{variant}-peer-jit"] != results[f"{variant}-peer-aot"]:
                raise RuntimeError(f"{variant} peer public results differ between JIT and NativeAOT")
        if args.all:
            for key, value in results.items():
                if key.startswith("raw-") and value != results["optimized-" + key.removeprefix("raw-")]:
                    raise RuntimeError(f"Raw/optimized deterministic public result differs: {key}")
        if initial != input_hashes():
            policy().issue("Campaign inputs changed during validation")
        (run / "results.json").write_text(json.dumps(results, indent=2) + "\n")
        passed = {"format": "picotls-test-v1", "variants": variants, "aot": args.aot, "runtime": args.runtime,
                  "run": str(run.relative_to(ROOT)), "inputs_sha256": digest(run / "inputs.json"),
                  "results_sha256": digest(run / "results.json"), "translation_sha256": digest(translation_record) if translation_record.exists() else None,
                  "completed_unix": time.time()}
        (run / "PASS.json").write_text(json.dumps(passed, indent=2) + "\n")
        receipt.write_text(json.dumps(passed, indent=2) + "\n")
        print(f"PASS campaign: {run}")
    except Exception as error:
        (run / "FAIL.txt").write_text(str(error) + "\n")
        raise SystemExit(f"FAIL campaign: {error}; evidence: {run}") from error


if __name__ == "__main__":
    main()
