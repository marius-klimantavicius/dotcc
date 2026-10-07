"""Verify C++17 operator-call operand order against native and EDG C output."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    source = Path(__file__).with_name("ordered_operator_calls.cpp")
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {"passed": False, "commands": [], "checks": []}

    def run(stage, command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        (out / (stage + ".stdout")).write_text(result.stdout)
        (out / (stage + ".stderr")).write_text(result.stderr)
        report["commands"].append({"stage": stage, "command": command, "exit": result.returncode})
        (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(stage + " failed; see " + str(out))
        return result.stdout

    run("native-build", ["g++", "-std=c++17", source, "-o", out / "native"])
    expected = run("native-run", [out / "native"])
    run("c-lower", [root / "build/bin/cpfe", "--c++17", "--gnu_version", "130300",
        "--instantiate", "used", "--gen_c_file_name", out / "original.c", "-o", out / "original.il", source],
        env=dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc")), cwd=out)
    run("c-build", ["gcc", "-std=gnu11", "-w", out / "original.c", root / "build/lib/libC.a", "-o", out / "original"])
    if run("c-run", [out / "original"]) != expected:
        raise RuntimeError("EDG C behavior differs from native C++17")
    report["checks"].append("Native C++17 and unchanged EDG C agree on ten evaluation/cleanup traces")
    for mode in ("static", "instance"):
        generated = out / mode
        command = [sys.executable, root / "dotnet.py", source, "--std", "c++17", "--libc", "musl",
                   "--runtime", "edg-exceptions", "--emit", "exe", "--output-dir", generated]
        if mode == "instance":
            command.append("--instance-methods")
        run(mode + "-translate", command)
        run(mode + "-build", ["dotnet", "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        if run(mode + "-jit", ["dotnet", generated / "bin/Release/net10.0/Generated.dll"]) != expected:
            raise RuntimeError(mode + " JIT differs from native")
        report["checks"].append(mode + " JIT: overloaded assignment/compound assignment, explicit calls, throwing RHS and result use")
        if args.nativeaot:
            run(mode + "-publish", ["dotnet", "publish", generated / "Generated.csproj", "-c", "Release",
                "-r", "linux-x64", "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", generated / "aot"])
            if run(mode + "-aot", [generated / "aot/Generated"]) != expected:
                raise RuntimeError(mode + " NativeAOT differs from native")
            report["checks"].append(mode + " NativeAOT: same native and EDG C behavior")
    report["passed"] = True
    (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")


if __name__ == "__main__":
    main()
