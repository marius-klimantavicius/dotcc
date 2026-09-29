#!/usr/bin/env python3
"""Exercise translated C against exact Linux glibc calls; not product qualification."""
import argparse
import hashlib
import json
import pathlib
import platform
import shutil
import subprocess
import time

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--aot', action='store_true')
parser.add_argument('--postprocess', action='store_true')
parser.add_argument('--compiler', type=pathlib.Path, default=ROOT / 'DotCC/bin/Release/net10.0/dotcc.dll')
args = parser.parse_args()
if platform.system() != 'Linux' or platform.libc_ver()[0] != 'glibc' or platform.machine() != 'x86_64':
    raise SystemExit('This diagnostic validates Linux glibc x64 only; other hosts are unrun.')
out = ROOT / 'libuv/artifacts/native-boundary'
out.mkdir(parents=True, exist_ok=True)
(out / 'result.json').write_text(json.dumps({'status': 'running', 'diagnostic_only': True}) + '\n')
commands = []
def run(argv):
    commands.append([str(x) for x in argv])
    subprocess.run(commands[-1], check=True, cwd=ROOT)
run(['dotnet', args.compiler, HERE / 'native-boundary.c', '--emit=managedlib', '--class-name', 'Probe',
     '--namespace', 'Managed.Libuv.Probes', '--runtime=c', '--overrides-file', HERE / 'native-boundary-overrides.json', '-o', out])
shutil.copyfile(HERE / 'NativeBoundary.cs', out / 'NativeBoundary.cs')
(out / 'Program.cs').write_text('int result = Managed.Libuv.Probes.Probe.native_boundary_probe();\nreturn result != 0 ? result : Managed.Libuv.Probes.NativeBoundary.CapturedCloseError != 9 ? 7 : Managed.Libuv.Probes.NativeThreadBoundary.Run();\n')
project = out / 'native-boundary.csproj'
project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>'))
run(['dotnet', 'run', '--project', project, '-c', 'Release'])
if args.aot:
    run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot'])
    run([out / 'aot/native-boundary'])
if args.postprocess:
    run(['dotnet', ROOT / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll', project, '--in-place'])
    run(['dotnet', 'run', '--project', project, '-c', 'Release'])
    if args.aot:
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot-processed'])
        run([out / 'aot-processed/native-boundary'])
(out / 'result.json').write_text(json.dumps({'status': 'passed', 'target': 'linux-glibc-x64', 'jit': True,
    'native_aot': args.aot, 'postprocessed': args.postprocess, 'compiler_dll_sha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest(), 'compiler_library_sha256': hashlib.sha256((args.compiler.parent / 'DotCC.Lib.dll').read_bytes()).hexdigest(), 'product_qualification': False,
    'commands': commands, 'unix_time': time.time()}, indent=2) + '\n')
