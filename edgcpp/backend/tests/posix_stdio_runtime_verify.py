"""Unified POSIX descriptors and stdio; EDG compiler host-service regression."""
import argparse
import json
from pathlib import Path
import platform
import shutil
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
runtime = root / 'backend/runtime'
sources = sorted(runtime.glob('Posix*.cs')) + [runtime / (name + '.cs') for name in (
    'VaArg', 'LibcFileDescriptors', 'LibcFiles', 'LibcFilesDescriptors', 'LibcFilesManagement',
    'LibcFilesWide', 'LibcAppend', 'LibcPaths')]
for source in sources + [root / 'backend/tests/posix_stdio_runtime.cs']:
    shutil.copy2(source, out / source.name)
(out / 'Probe.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>
<IsAotCompatible>true</IsAotCompatible><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup></Project>''')
results = []
def run(command, label):
    command = list(map(str, command))
    result = subprocess.run(command, text=True, capture_output=True)
    (out / (label + '.stdout')).write_text(result.stdout)
    (out / (label + '.stderr')).write_text(result.stderr)
    results.append({'label': label, 'command': command, 'exit': result.returncode})
    (out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    print(result.stdout, end='')
    if result.returncode:
        print(result.stderr)
        raise SystemExit(result.returncode)
run(['dotnet', 'build', out / 'Probe.csproj', '-c', 'Release', '--nologo'], 'build')
run(['dotnet', out / 'bin/Release/net10.0/Probe.dll'], 'jit')
if args.nativeaot:
    os_name = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}[platform.system()]
    arch = 'arm64' if platform.machine().lower() in ('aarch64', 'arm64') else 'x64'
    run(['dotnet', 'publish', out / 'Probe.csproj', '-c', 'Release', '-r', os_name + '-' + arch,
         '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot'], 'publish')
    run([out / 'aot' / ('Probe.exe' if os_name == 'win' else 'Probe')], 'aot')
