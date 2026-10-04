"""Check OFD isolation from copied runtimes/native code and atomic conversion."""
import argparse
import json
import platform
from pathlib import Path
import shutil
import subprocess

p = argparse.ArgumentParser(description=__doc__)
p.add_argument("--output-dir", type=Path, required=True)
p.add_argument("--nativeaot", action="store_true")
a = p.parse_args()
root = Path(__file__).resolve().parents[2]
out = a.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
if platform.system() not in ("Linux", "Darwin"):
    raise SystemExit("OFD edge fixture requires Linux or macOS")
for source in sorted((root / "backend/runtime").glob("Posix*.cs")) + [root / "backend/runtime/VaArg.cs"]:
    shutil.copy2(source, out / source.name)
    # Separate namespaces simulate independently copied runtime assemblies:
    # no lock registry, owner type or other managed state is shared.
    (out / ("Copy." + source.name)).write_text(source.read_text().replace("Edg.Runtime", "Copied.Runtime"))
shutil.copy2(root / "backend/tests/posix_edges.cs", out / "Program.cs")
shutil.copy2(root / "backend/tests/posix_runtime_native.c", out / "probe.c")
(out / "Edges.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors><Nullable>enable</Nullable>
</PropertyGroup></Project>''')
results = []
def run(command, label):
    command = list(map(str, command))
    result = subprocess.run(command, text=True, capture_output=True)
    (out / (label + ".stdout")).write_text(result.stdout)
    (out / (label + ".stderr")).write_text(result.stderr)
    results.append({"command": command, "label": label, "exit": result.returncode})
    (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
    print(result.stdout, end="")
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)
run(["cc", "-Wall", "-Wextra", "-Werror", out / "probe.c", "-o", out / "probe"], "native-build")
run(["dotnet", "build", out / "Edges.csproj", "-c", "Release", "--nologo"], "build")
run(["dotnet", out / "bin/Release/net10.0/Edges.dll", out / "probe"], "jit")
if a.nativeaot:
    rid = ("osx" if platform.system() == "Darwin" else "linux") + ("-arm64" if platform.machine().lower() in ("arm64", "aarch64") else "-x64")
    run(["dotnet", "publish", out / "Edges.csproj", "-c", "Release", "-r", rid, "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", out / "aot"], "publish")
    run([out / "aot/Edges", out / "probe"], "aot")
