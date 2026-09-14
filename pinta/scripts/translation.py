#!/usr/bin/env python3
"""Translate the pinned complete core; retain raw output and reproducibility receipts."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
SOURCE = ROOT / "ref/upstream"
COMPILER = REPO / "DotCC/bin/Release/net10.0/dotcc.dll"
POSTPROCESSOR = REPO / "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def lines(path):
    return [s for line in path.read_text().splitlines() if (s := line.strip()) and not s.startswith("#")]


def run(command, log, timeout=600):
    log.parent.mkdir(parents=True, exist_ok=True)
    print("Running: " + " ".join(map(str, command)), flush=True)
    started = time.monotonic()
    with log.open("w") as stream:
        try:
            result = subprocess.run(list(map(str, command)), cwd=REPO, stdout=stream,
                                    stderr=subprocess.STDOUT, timeout=timeout)
            code = result.returncode
        except subprocess.TimeoutExpired:
            code = 124
    print(log.read_text(errors="replace")[-16000:], end="", flush=True)
    return {"command": list(map(str, command)), "exit_code": code,
            "seconds": round(time.monotonic() - started, 3), "log": str(log.relative_to(ROOT)),
            "log_sha256": sha(log)}


def prepare(profile, pristine):
    subprocess.run([sys.executable, str(ROOT / "scripts/fetch.py")], check=True, stdout=subprocess.DEVNULL)
    if not pristine:
        subprocess.run([sys.executable, str(ROOT / "scripts/stage.py")], check=True, stdout=subprocess.DEVNULL)
    names = lines(ROOT / "config/core-sources.txt")
    if len(names) != 29 or len(set(names)) != 29:
        raise RuntimeError("Expected exactly 29 distinct pinned core translation units")
    staged = ROOT / "generated/inputs" / profile
    staged.mkdir(parents=True, exist_ok=True)
    preinclude = ROOT / "config/pinta-preinclude.h"
    result = []
    for name in names:
        source = SOURCE / name
        source.resolve().relative_to(SOURCE.resolve())
        if not source.is_file():
            raise RuntimeError(f"Missing manifest source: {source}")
        wrapper = staged / source.name
        # Include adapters as an authored preinclude; upstream bytes stay unchanged.
        wrapper.write_text(f'#include "{preinclude.name}"\n#include "{name}"\n')
        result.append(wrapper)
    return result


def identity(profile):
    config = ROOT / "config"
    # Only configuration consumed by fetching/staging/emitting the product belongs
    # here. Native test preincludes and host adapters have separate harness receipts.
    paths = [config / name for name in ["source-lock.json", "core-sources.txt",
                                       "core-defines.txt", "pinta-preinclude.h", "patches.json"]]
    # stage.py applies every entry in this hash-checked manifest. Record those
    # precise patch files, rather than unrelated files sharing the config tree.
    for entry in json.loads((config / "patches.json").read_text()):
        patch = config / entry["patch"]
        patch.resolve().relative_to(config.resolve())
        paths.append(patch)
    paths += [ROOT / "scripts" / name for name in
              ["translation.py", "translate.sh", "fetch.py", "stage.py"]]
    paths += list((SOURCE / "Marius.Pinta/inc").rglob("*.h"))
    paths += [SOURCE / name for name in lines(ROOT / "config/core-sources.txt")]
    paths += [COMPILER, COMPILER.parent / "DotCC.Lib.dll", POSTPROCESSOR]
    return {"profile": profile, "sha256": {str(p.relative_to(REPO)): sha(p)
            for p in sorted(paths) if p.is_file()}}


def generated_files(directory):
    names = lines(directory / "Dotcc.SourceFiles.txt")
    if not names or len(names) != len(set(names)):
        raise RuntimeError("Missing or duplicate emitted sources")
    for name in names:
        if Path(name).name != name or not name.endswith(".cs") or (directory / name).is_symlink():
            raise RuntimeError(f"Unsafe emitted filename: {name}")
    return names + ["Dotcc.SourceFiles.txt", "TranslatedPinta.csproj"]


def hash_output(directory):
    return {name: sha(directory / name) for name in sorted(generated_files(directory))}


def main():
    global SOURCE
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["probe", "translate", "build"])
    parser.add_argument("--profile", choices=["release", "debug"], default="release")
    parser.add_argument("--no-build-tools", action="store_true")
    parser.add_argument("--pristine", action="store_true", help="Probe untouched upstream instead of hash-checked safety patches")
    parser.add_argument("--unit", help="Probe only this source basename")
    args = parser.parse_args()
    variant = args.profile + ("-pristine" if args.pristine else "")
    if not args.pristine:
        SOURCE = ROOT / "generated/native-input"
    artifacts = ROOT / "artifacts/translation" / variant
    artifacts.mkdir(parents=True, exist_ok=True)
    temp = ROOT / "artifacts/tmp"
    temp.mkdir(parents=True, exist_ok=True)
    os.environ["TMPDIR"] = str(temp)
    product = ROOT / "generated" / ("TranslatedPinta" if variant == "release" else variant + "/TranslatedPinta")
    raw = product.parent / "TranslatedPintaRaw"
    receipt = artifacts / "success.json"
    if args.action == "build":
        if not receipt.exists():
            raise RuntimeError("No successful current translation receipt; run translate.sh first")
        saved = json.loads(receipt.read_text())
        if saved["inputs"] != identity(args.profile):
            raise RuntimeError("Inputs/compiler changed; regenerate before building")
        results = []
        for label, directory in [("raw", raw), ("optimized", product)]:
            if saved[label] != hash_output(directory):
                raise RuntimeError(f"{label} output changed since translation")
            result = run(["dotnet", "build", directory / "TranslatedPinta.csproj", "-c", "Release", "--nologo"], artifacts / f"build-{label}.log")
            results.append({"form": label, **result})
            if result["exit_code"]:
                break
        (artifacts / "build.json").write_text(json.dumps(results, indent=2) + "\n")
        return results[-1]["exit_code"]
    if not args.no_build_tools:
        for project in ["DotCC", "DotCC.PostProcess"]:
            result = run(["dotnet", "build", REPO / project / f"{project}.csproj", "-c", "Release", "--nologo"], artifacts / f"tool-{project}.log")
            if result["exit_code"]:
                return result["exit_code"]
    for tool in [COMPILER, POSTPROCESSOR]:
        if not tool.is_file():
            raise RuntimeError(f"Missing {tool}; rerun without --no-build-tools")
    sources = prepare(variant, args.pristine)
    defines = [f"-D{d}" for d in lines(ROOT / "config/core-defines.txt")
               if not d.startswith("PINTA_DEBUG=")]
    defines.append(f"-DPINTA_DEBUG={int(args.profile == 'debug')}")
    base = ["dotnet", COMPILER, "-std=c17", *defines, "-I", ROOT / "config",
            "-I", SOURCE, "-I", SOURCE / "Marius.Pinta/inc"]
    before = identity(args.profile)
    (artifacts / "input-state.json").write_text(json.dumps(before, indent=2) + "\n")
    if args.action == "probe":
        selected = [p for p in sources if not args.unit or p.name == args.unit]
        if not selected:
            raise RuntimeError(f"Source not in pinned manifest: {args.unit}")
        results = []
        with tempfile.TemporaryDirectory(prefix="probe-", dir=temp) as temporary:
            for source in selected:
                result = run([*base, source, "--emit=obj", "-o", Path(temporary) / (source.stem + ".cs")],
                             artifacts / f"probe-{source.stem}.log")
                results.append({"unit": source.name, **result})
        if before != identity(args.profile):
            raise RuntimeError("Inputs changed during probe; results are not reproducible")
        (artifacts / "probe.json").write_text(json.dumps({"inputs": before, "results": results}, indent=2) + "\n")
        return int(any(r["exit_code"] for r in results))
    receipt.unlink(missing_ok=True)
    # A fresh directory prevents a failed emission being mistaken for an older success.
    with tempfile.TemporaryDirectory(prefix="emit-", dir=temp) as temporary:
        emitted = Path(temporary) / "TranslatedPinta"
        result = run([*base, *sources, "--emit=managedlib", "--nest-types", "--runtime=c",
                      "--class-name", "Pinta", "--namespace", "Managed.Interpreters",
                      "--split=size", "--split-size=102400", "-o", emitted], artifacts / "emit.log")
        (artifacts / "attempt.json").write_text(json.dumps({"inputs": before, "emission": result}, indent=2) + "\n")
        if result["exit_code"]:
            return result["exit_code"]
        names = generated_files(emitted)
        for directory in [raw, product]:
            directory.mkdir(parents=True, exist_ok=True)
            previous = generated_files(directory) if (directory / "Dotcc.SourceFiles.txt").exists() else []
            for name in set(previous) - set(names):
                (directory / name).unlink()
            for name in names:
                if (directory / name).is_symlink():
                    raise RuntimeError(f"Refusing to overwrite symlink: {directory / name}")
                shutil.copyfile(emitted / name, directory / name)
    result = run(["dotnet", "restore", product / "TranslatedPinta.csproj", "--nologo"], artifacts / "restore.log")
    if result["exit_code"]:
        return result["exit_code"]
    result = run(["dotnet", POSTPROCESSOR, product / "TranslatedPinta.csproj", "--in-place"], artifacts / "postprocess.log")
    if result["exit_code"]:
        return result["exit_code"]
    if before != identity(args.profile):
        raise RuntimeError("Inputs/compiler changed during translation; rerun")
    receipt.write_text(json.dumps({"format": "pinta-translation-v1", "inputs": before,
                                   "raw": hash_output(raw), "optimized": hash_output(product)}, indent=2) + "\n")
    print("Complete core emitted and postprocessed; VM behavior requires separate validation.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (RuntimeError, subprocess.CalledProcessError) as error:
        print(f"pinta: {error}", file=sys.stderr)
        sys.exit(1)
