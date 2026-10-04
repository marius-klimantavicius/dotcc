"""C/C++ compiler host services through translated static/instance owners."""
import argparse
import hashlib
import json
from pathlib import Path
import platform
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
commands = []
def run(command, label, cwd=None):
    command = list(map(str, command))
    result = subprocess.run(command, cwd=cwd, capture_output=True, text=True)
    (out / (label + '.stdout')).write_text(result.stdout)
    (out / (label + '.stderr')).write_text(result.stderr)
    commands.append({'label': label, 'command': command, 'exit': result.returncode})
    (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    if result.returncode:
        print(result.stdout[-8000:]); print(result.stderr[-8000:]); raise SystemExit(result.returncode)
    return result.stdout
source = root / 'backend/tests/posix_stdio_translated.c'
expected = 'POSIX translated stdio host services passed\n'
run(['cc', '-Wall', '-Wextra', '-Werror', source, '-o', out / 'native'], 'native-build')
native_work = out / 'native-work'; native_work.mkdir()
assert run([out / 'native'], 'native', native_work) == expected
for language, standard in [('c', 'c11'), ('c++', 'c++14')]:
    for mode in ['static', 'instance']:
        label = ('c' if language == 'c' else 'cpp') + '-' + mode
        generated = out / label
        run([sys.executable, root / 'dotnet.py', source, '--compiler', args.compiler.resolve(),
            '--language', language, '--std', standard, '--libc', 'musl', '--posix',
            '--unbound-externals', 'reject', '--namespace', 'Host.Integration', '--class-name', 'Core',
            *(['--instance-methods'] if mode == 'instance' else []), '--output-dir', generated], label + '-translate')
        manifest = json.loads((generated / 'dot-gen.json').read_text())
        assert manifest['unboundExternals']['symbols'] == [], manifest['unboundExternals']
        run(['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'], label + '-build')
        work = out / (label + '-work'); work.mkdir()
        assert run(['dotnet', generated / 'bin/Release/net10.0/Generated.dll'], label + '-jit', work) == expected
        if args.nativeaot:
            rid = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}[platform.system()] + ('-arm64' if platform.machine().lower() in ('aarch64', 'arm64') else '-x64')
            run(['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release', '-r', rid, '-p:PublishAot=true',
                '-p:TreatWarningsAsErrors=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'], label + '-publish')
            assert run([generated / 'aot/Generated'], label + '-aot', work) == expected
        print(label + ': passed', flush=True)
(out / 'results.json').write_text(json.dumps({'status': 'passed', 'compilerSha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
    'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(), 'modes': ['C/static', 'C/instance', 'C++14/static', 'C++14/instance'],
    'nativeaot': args.nativeaot, 'expectedOutput': expected}, indent=2) + '\n')
