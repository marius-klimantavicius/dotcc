"""Exercise Windows advisory registry algorithms, also directly testable on Unix."""
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
for name in ("PosixTypes", "PosixPlatform", "PosixWindowsLocks"):
    shutil.copy2(root / "backend/runtime" / (name + ".cs"), out / (name + ".cs"))
shutil.copy2(root / "backend/tests/posix_windows_registry.cs", out / "Program.cs")
project = out / "RegistryProbe.csproj"
project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup></Project>\n''')
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
run(["dotnet", "build", project, "-c", "Release", "--nologo"], "build")
run(["dotnet", out / "bin/Release/net10.0/RegistryProbe.dll"], "jit")
if args.nativeaot:
    os_name = {"Linux": "linux", "Darwin": "osx", "Windows": "win"}[platform.system()]
    arch = "arm64" if platform.machine().lower() in ("aarch64", "arm64") else "x64"
    run(["dotnet", "publish", project, "-c", "Release", "-r", os_name + "-" + arch,
         "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", out / "aot"], "publish")
    run([out / "aot" / ("RegistryProbe.exe" if os_name == "win" else "RegistryProbe")], "aot")
print("Windows advisory registry algorithm verification passed (host: " + platform.system() + ")")
