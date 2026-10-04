"""Standalone shared POSIX descriptors, mappings, virtual-process locks and time."""
import argparse
import json
import platform
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
for source in sorted((root / "backend/runtime").glob("Posix*.cs")) + [root / "backend/runtime/LibcFileDescriptors.cs", root / "backend/runtime/VaArg.cs", root / "backend/tests/posix_runtime.cs"]:
    shutil.copy2(source, out / source.name)
project = out / "PosixProbe.csproj"
project.write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors><Nullable>enable</Nullable>
    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
  </PropertyGroup>
</Project>
''')
results = []
def run(command, label):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True, text=True)
    (out / (label + ".stdout")).write_text(result.stdout)
    (out / (label + ".stderr")).write_text(result.stderr)
    results.append({"label": label, "command": command, "exit": result.returncode})
    (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
    if result.returncode:
        raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")
    print(result.stdout, end="")

native_args = []
if platform.system() in ("Linux", "Darwin"):
    native_source = root / "backend/tests/posix_runtime_native.c"
    shutil.copy2(native_source, out / native_source.name)
    native = out / "posix-runtime-native"
    run(["cc", "-Wall", "-Wextra", "-Werror", out / native_source.name, "-o", native], "native-build")
    native_args = [native]
run(["dotnet", "build", project, "-c", "Release", "--nologo"], "build")
run(["dotnet", out / "bin/Release/net10.0/PosixProbe.dll", *native_args], "jit")
if args.nativeaot:
    os_name = {"Linux": "linux", "Darwin": "osx", "Windows": "win"}[platform.system()]
    arch = "arm64" if platform.machine().lower() in ("aarch64", "arm64") else "x64"
    run(["dotnet", "publish", project, "-c", "Release", "-r", os_name + "-" + arch,
         "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", out / "aot"], "publish")
    run([out / "aot" / ("PosixProbe.exe" if os_name == "win" else "PosixProbe"), *native_args], "aot")
print("Shared POSIX runtime verification passed")
