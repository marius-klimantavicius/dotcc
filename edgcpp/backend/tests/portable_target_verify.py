"""Portable target macros, libc headers, EDG layout and generated runtime agree."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

PROFILE = "portable-lp64-binary80-v1"


def verify_portable_target(root, tests, out, nativeaot, run):
    checks = []
    source = tests / "portable_target.c"
    target = root / "backend/target"
    specification = json.loads((target / (PROFILE + ".json")).read_text())
    assert specification["id"] == PROFILE
    assert specification["floating"]["longDouble"]["precisionBits"] == 64
    environment = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"),
                       EDG_DOT_TARGET=PROFILE)
    empty = out / "portable-empty.cpp"
    empty.write_text("")
    # The hook itself must suppress the host predefined file, without relying
    # on the driver's redundant --clear_flag safeguard.
    macros = run([root / "build/bin/cpfe-dot", "--c++17", "--gnu_version", "130300",
                  "--list_macros", empty], cwd=out, env=environment).stdout
    (out / "portable-macros.txt").write_text(macros)
    for forbidden in ("__linux__", "__x86_64__", "__unix__", "_WIN32", "__APPLE__", "_GNU_SOURCE"):
        assert f"#define {forbidden} " not in macros, forbidden
    assert "#define __EDG_DOT_TARGET_VERSION__ 1" in macros
    checks.append("portable target hook suppresses host OS/CPU macro injection")
    for language in ("c", "c++"):
        for mode in ("static", "instance"):
            directory = out / f"portable-{language.replace('+', 'x')}-{mode}"
            options = ["--instance-methods", "--split", "size", "--split-size", "2048"] if mode == "instance" else []
            run([sys.executable, root / "dotnet.py", source, "--language", language,
                 "--target", PROFILE, "--libc", "dotcc", "--output-dir", directory, *options])
            project = directory / "Generated.csproj"
            manifest = json.loads((directory / "dot-gen.json").read_text())
            # Integration must record a content-addressed target contract.
            assert PROFILE in json.dumps(manifest)
            for name in (PROFILE + ".json", PROFILE + ".def"):
                digest = hashlib.sha256((target / name).read_bytes()).hexdigest()
                assert digest in json.dumps(manifest), name
            run(["dotnet", "build", project, "-c", "Release", "--nologo"])
            run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
            checks.append(f"portable {language} {mode}: macro/layout/header/binary80 JIT agreement")
            if nativeaot:
                run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                     "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true",
                     "-o", directory / "aot"])
                run([directory / "aot/Generated"])
                checks.append(f"portable {language} {mode}: Linux x64 NativeAOT agreement")
    return checks


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    commands = []

    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, text=True, capture_output=True, **kwargs)
        index = len(commands)
        (output / f"command-{index:03d}.stdout").write_text(result.stdout)
        (output / f"command-{index:03d}.stderr").write_text(result.stderr)
        commands.append({"command": command, "exit": result.returncode})
        (output / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")
        return result

    root = Path(__file__).resolve().parents[2]
    checks = verify_portable_target(root, Path(__file__).resolve().parent,
                                   output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} portable target checks passed")
