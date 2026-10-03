"""Standalone libc++ BCL thread contract, before generated import integration."""
import argparse
import json
from pathlib import Path
import shutil
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--output-dir", type=Path, required=True)
parser.add_argument("--nativeaot", action="store_true")
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
for source in (root / "backend/runtime/LibcxxThreads.cs", root / "backend/runtime/LibcC11Threads.cs", root / "backend/runtime/LibcInitialThread.cs", root / "backend/tests/libcxx_threads_runtime.cs"):
    shutil.copy2(source, out / source.name)
project = out / "ThreadProbe.csproj"
project.write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
  </PropertyGroup>
</Project>
''')
results = []


def run(command, label, expected=0):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True, text=True)
    (out / (label + ".stdout")).write_text(result.stdout)
    (out / (label + ".stderr")).write_text(result.stderr)
    results.append({"label": label, "command": command, "exit": result.returncode})
    (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
    if result.returncode != expected:
        raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")


run(["dotnet", "build", project, "-c", "Release", "--nologo"], "build")
run(["dotnet", out / "bin/Release/net10.0/ThreadProbe.dll"], "jit")
run(["dotnet", out / "bin/Release/net10.0/ThreadProbe.dll", "fatal"], "jit-fatal", 73)
assert (out / "jit-fatal.stdout").read_text() == "body\nterminate\n"
if args.nativeaot:
    run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
         "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", out / "aot"], "publish")
    run([out / "aot/ThreadProbe"], "aot")
    run([out / "aot/ThreadProbe", "fatal"], "aot-fatal", 73)
    assert (out / "aot-fatal.stdout").read_text() == "body\nterminate\n"
print("Thread runtime synthetic checks passed")
