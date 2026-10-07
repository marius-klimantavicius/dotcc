#!/usr/bin/env python3
"""Check folded designated-record assignments, callbacks, arrays and zero fill."""
import argparse
import json
from pathlib import Path
import platform
import subprocess
import sys

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--compiler', type=Path, required=True)
p.add_argument('--output-dir', type=Path, required=True)
p.add_argument('--nativeaot', action='store_true')
p.add_argument('--c-backend', type=Path, help='Optional stock EDG C backend for a second oracle')
a = p.parse_args()
root = Path(__file__).resolve().parents[2]
source = Path(__file__).with_name('folded_designated_initializers.c')
out = a.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
results = {'passed': False, 'runs': []}
def run(label, command):
    command = list(map(str, command))
    r = subprocess.run(command, capture_output=True)
    (out / (label + '.stdout')).write_bytes(r.stdout)
    (out / (label + '.stderr')).write_bytes(r.stderr)
    results['runs'].append({'label': label, 'command': command, 'exit': r.returncode})
    (out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    if r.returncode: raise RuntimeError(label + '\n' + r.stdout.decode()[-2000:] + r.stderr.decode()[-2000:])
run('native-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror', '-Wno-override-init', source, '-o', out/'native'])
run('native', [out/'native'])
if a.c_backend:
    run('c-backend', [a.c_backend.resolve(), '--c11', '--gnu_version', '130300',
        '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out/'stock-edg.c', source])
    run('c-backend-build', ['cc', '-std=c11', out/'stock-edg.c', '-o', out/'stock-edg'])
    run('c-backend-execute', [out/'stock-edg'])
for ms in (False, True):
    for instance in (False, True):
        label = ('ms-' if ms else 'gnu-') + ('instance' if instance else 'static')
        generated = out/label
        run(label+'-translate', [sys.executable, root/'dotnet.py', '--compiler', a.compiler.resolve(),
            '--language', 'c', '--std', 'c11', '--output-dir', generated,
            *(['--instance-methods'] if instance else []), *(['--ms-extensions'] if ms else []), source])
        text = '\n'.join(f.read_text() for f in generated.glob('Translated*.cs'))
        assert 'assign_plain' in text and 'assign_table' in text and 'callback' in text
        run(label+'-build', ['dotnet', 'build', generated/'Generated.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(label+'-jit', ['dotnet', generated/'bin/Release/net10.0/Generated.dll'])
        if a.nativeaot:
            system = {'Linux':'linux','Darwin':'osx','Windows':'win'}[platform.system()]
            arch = 'arm64' if platform.machine().lower() in ('arm64','aarch64') else 'x64'
            run(label+'-publish', ['dotnet', 'publish', generated/'Generated.csproj', '-c', 'Release', '-r', system+'-'+arch,
                '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',generated/'aot'])
            run(label+'-aot', [generated/'aot'/('Generated.exe' if system=='win' else 'Generated')])
results['passed'] = True
(out/'results.json').write_text(json.dumps(results, indent=2)+'\n')
print('Folded designated initializers: native, GNU/MS extension static/instance JIT' + ('/AOT' if a.nativeaot else '') + ' passed')
