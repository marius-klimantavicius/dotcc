#!/usr/bin/env python3
"""Exercise portable socket headers and inet_pton binding through translated C."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--output-dir', type=Path, required=True)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
commands = []
def run(label, command, success=True):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True, text=True)
    (out / (label + '.stdout')).write_text(result.stdout)
    (out / (label + '.stderr')).write_text(result.stderr)
    commands.append({'label': label, 'command': command, 'exit': result.returncode})
    (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    assert (result.returncode == 0) == success, (label, result.stdout[-2000:], result.stderr[-2000:])
    return result
source = Path(__file__).with_name('libc_inet_translated.c')
run('native-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror', source, '-o', out / 'native'])
run('native', [out / 'native'])
run('portable-layout-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror',
    '-I', root / 'backend/runtime/posix/include', source, '-o', out / 'portable-layout'])
run('portable-layout', [out / 'portable-layout'])
common = [sys.executable, root / 'dotnet.py', '--compiler', args.compiler.resolve(),
          '--language', 'c', '--std', 'c11', '--target', 'portable-lp64-binary80-v2',
          '--libc', 'musl', '--posix', '--unbound-externals', 'reject']
for mode in ('static', 'instance'):
    generated = out / mode
    run(mode + '-translate', [*common, source, '--output-dir', generated,
                            *(['--instance-methods'] if mode == 'instance' else [])])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    assert not manifest['unboundExternals']['symbols']
    run(mode + '-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release',
                         '--nologo', '-p:TreatWarningsAsErrors=true'])
    run(mode + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
unsupported = out / 'pthread-unbound.c'
unsupported.write_text('#include <pthread.h>\nint main(void) { pthread_mutex_t m = PTHREAD_MUTEX_INITIALIZER; return pthread_mutex_lock(&m); }\n')
rejected = run('pthread-unbound', [*common, unsupported, '--output-dir', out / 'unsupported'], success=False)
assert 'pthread_mutex_lock' in rejected.stdout + rejected.stderr
(out / 'results.json').write_text(json.dumps({'passed': True, 'checks': [
    'native oracle', 'static and instance strict C translation, no unbound imports, JIT execution',
    'pthread calls rejected as unbound, declarations do not synthesize behavior']}, indent=2) + '\n')
print('inet_pton translated C static/instance and unsupported pthread rejection passed')
