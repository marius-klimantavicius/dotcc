"""Semantic EDG multi-TU merge: linkage, inline state, RTTI and EH identity."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def verify_multi_tu(root, tests, out, nativeaot, run):
    checks = []
    inputs = tests / "multi_tu"
    env = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"))
    clang = shutil.which("clang") or next(
        (str(p) for p in sorted(Path("/usr/lib").glob("llvm-*/bin/clang"))), None)
    if clang is None:
        raise RuntimeError("Clang is required for the multi-TU C oracle")
    for label, language, filenames in (
        ("cxx", "c++", ("main.cpp", "other.cpp")),
        ("c", "c", ("opaque.c", "complete.c")),
    ):
        sources = [inputs / name for name in filenames]
        oracle = out / ("multi-tu-" + label)
        oracle.mkdir(parents=True, exist_ok=True)
        run([root / "build/bin/cpfe", "--c++17" if language == "c++" else "--c11",
             "--gnu_version", "130300", "--gen_c_file_name", oracle / "oracle.c",
             "-o", oracle / "oracle.il", *sources], cwd=oracle, env=env)
        for compiler_label, compiler in (("gcc", "gcc"), ("clang", clang)):
            executable = oracle / compiler_label
            run([compiler, "-w", "-std=gnu11", oracle / "oracle.c",
                 root / "build/lib/libC.a", "-lm", "-o", executable])
            run([executable])
        checks.append(f"multi-TU {label}: EDG C oracle agrees under GCC/Clang")
        for mode in ("static", "instance"):
            directory = oracle / mode
            options = ["--runtime", "edg-exceptions"] if language == "c++" else []
            if mode == "instance":
                options += ["--instance-methods", "--split", "function"]
            run([sys.executable, root / "dotnet.py", *sources,
                 "--language", language, "--output-dir", directory, *options])
            if mode == "instance":
                # Each owner's inline local static must start at 3 independently.
                original = directory / "EntryPoint.cs"
                (directory / "EntryPoint.generated.cs.saved").write_bytes(original.read_bytes())
                original.write_text("""internal static class EntryPoint {
  public static int Main() {
    using var first = new Edg.Generated.TranslatedProgram();
    using var second = new Edg.Generated.TranslatedProgram();
    int a = first.Run(), b = second.Run();
    return a == 0 && b == 0 ? 0 : 90;
  }
}
""")
            project = directory / "Generated.csproj"
            run(["dotnet", "build", project, "-c", "Release", "--nologo"])
            run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
            checks.append(f"multi-TU {label} {mode}: JIT linkage/state/execution")
            if nativeaot:
                run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                     "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true",
                     "-o", directory / "aot"])
                run([directory / "aot/Generated"])
                checks.append(f"multi-TU {label} {mode}: Linux x64 NativeAOT")
    return checks


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    records = []

    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(records)
        (output / f"command-{index:03d}.stdout").write_text(result.stdout)
        (output / f"command-{index:03d}.stderr").write_text(result.stderr)
        records.append({"command": command, "exit": result.returncode})
        (output / "commands.json").write_text(json.dumps(records, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")
        return result

    root = Path(__file__).resolve().parents[2]
    checks = verify_multi_tu(root, Path(__file__).resolve().parent, output,
                            args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} multi-TU checks passed")
