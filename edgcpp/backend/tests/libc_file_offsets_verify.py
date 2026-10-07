"""Compare native and translated 64-bit stdio positions, including sparse files."""
import argparse
import json
from pathlib import Path
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    fixture = Path(__file__).with_name("libc_file_offsets.c")
    report = {"passed": False, "commands": [], "checks": []}

    def run(stage, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (stage + ".stdout")).write_text(result.stdout)
        (out / (stage + ".stderr")).write_text(result.stderr)
        report["commands"].append({"stage": stage, "command": command, "exit": result.returncode})
        (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(stage + " failed; see " + str(out))
        return result.stdout

    run("native-build", ["gcc", "-std=c11", "-D_POSIX_C_SOURCE=200809L", fixture, "-o", out / "native"])
    expected = run("native-run", [out / "native"])
    for mode in ("static", "instance"):
        generated = out / mode
        command = [sys.executable, root / "dotnet.py", fixture, "--language", "c", "--std", "c11",
                   "--libc", "musl", "--posix", "--emit", "exe", "--output-dir", generated]
        if mode == "instance":
            command.append("--instance-methods")
        run(mode + "-translate", command)
        manifest = json.loads((generated / "dot-gen.json").read_text())
        if manifest.get("unboundExternals", {}).get("requiredPartials"):
            raise RuntimeError("Unresolved externs remain")
        run(mode + "-build", ["dotnet", "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        actual = run(mode + "-jit", ["dotnet", generated / "bin/Release/net10.0/Generated.dll"])
        if actual != expected:
            raise RuntimeError(mode + " JIT differs from native")
        report["checks"].append(mode + " JIT: sparse >8 GiB file, signed seeks, errors, EOF, mixed API spelling")
        if args.nativeaot:
            run(mode + "-publish", ["dotnet", "publish", generated / "Generated.csproj", "-c", "Release",
                "-r", "linux-x64", "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", generated / "aot"])
            if run(mode + "-aot", [generated / "aot/Generated"]) != expected:
                raise RuntimeError(mode + " NativeAOT differs from native")
            report["checks"].append(mode + " NativeAOT: same native-oracle behavior")
    report["passed"] = True
    (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")


if __name__ == "__main__":
    main()
