"""Shared span varargs storage/lifetime tests independent of native emission."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', required=True, type=Path)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
hashes = {}
for source in [Path(__file__).with_name('varargs_span_runtime.cs'), *[root / 'backend/runtime' / name for name in ['VaArg.cs', 'VaPack.cs', 'VaList.cs']]]:
    target = out / source.name
    shutil.copy2(source, target)
    hashes[source.name] = hashlib.sha256(source.read_bytes()).hexdigest()
(out / 'input-sha256.json').write_text(json.dumps(hashes, indent=2)+'\n')
(out / 'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
commands = []
def run(label, command):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True, text=True)
    (out / (label+'.stdout')).write_text(result.stdout)
    (out / (label+'.stderr')).write_text(result.stderr)
    commands.append({'label': label, 'command': command, 'exitCode': result.returncode})
    (out / 'results.json').write_text(json.dumps({'commands': commands}, indent=2)+'\n')
    if result.returncode:
        raise RuntimeError(label+' failed:\n'+result.stdout+result.stderr)
    print(label+' passed', flush=True)
run('build', ['dotnet', 'build', out/'Probe.csproj', '-c', 'Release', '--nologo'])
run('jit', ['dotnet', out/'bin/Release/net10.0/Probe.dll'])
if args.nativeaot:
    run('publish', ['dotnet', 'publish', out/'Probe.csproj', '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=none', '-p:DebugSymbols=false', '-o', out/'aot', '--nologo'])
    run('aot', [out/'aot/Probe'])
