"""Zero-bound padding initialization versus original EDG C."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_zero_bound_padding(root, tests, out, nativeaot, run):
    source = tests / "zero_bound_padding.cpp"
    env = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"))
    c_source = out / "zero-bound-padding-oracle.c"
    run([root / "build/bin/cpfe", "--c++17", "--gnu_version", "130300", "--instantiate", "used",
         "--gen_c_file_name", c_source, "-o", out / "zero-bound-padding-oracle.il", source],
        cwd=out, env=env)
    native = out / "zero-bound-padding-oracle"
    run(["gcc", "-std=gnu11", c_source, root / "build/lib/libC.a", "-lm", "-o", native])
    run([native])
    checks = ["zero-bound padding: original EDG C oracle"]
    for mode in ("static", "instance"):
        directory = out / mode
        options = ["--instance-methods"] if mode == "instance" else []
        run([sys.executable, root / "dotnet.py", source, "--output-dir", directory, *options])
        if mode == "instance":
            entry = directory / "EntryPoint.cs"
            (directory / "EntryPoint.generated.cs.saved").write_bytes(entry.read_bytes())
            entry.write_text("""internal static class EntryPoint {
  public static int Main() {
    using var a = new Edg.Generated.TranslatedProgram();
    using var b = new Edg.Generated.TranslatedProgram();
    return a.Run() == 0 && b.Run() == 0 ? 0 : 99;
  }
}
""")
        project = directory / "Generated.csproj"
        run(["dotnet", "build", project, "-c", "Release", "--nologo"])
        run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
        checks.append(f"zero-bound padding {mode}: JIT global/TLS/local nested zero arrays, nonzero padding and initializer cursor")
        if nativeaot:
            run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                 "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
            run([directory / "aot/Generated"])
            checks.append(f"zero-bound padding {mode}: Linux x64 NativeAOT")
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
    checks = verify_zero_bound_padding(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} zero-bound padding checks passed")
