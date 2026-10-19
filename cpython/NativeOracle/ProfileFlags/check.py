"""Native/emitted role guards exercise actual test_profile.extend unit flags."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import sys
from types import SimpleNamespace
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[3]
FIXTURE = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / 'Scripts'))
from campaigns.model import Unit

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', required=True, type=Path)
parser.add_argument('--work', required=True, type=Path)
parser.add_argument('--artifacts', required=True, type=Path)
args = parser.parse_args()
args.work.mkdir()
args.artifacts.mkdir()
commands = []

def run(label, argv, expect=0):
    argv = [str(value) for value in argv]
    result = subprocess.run(argv, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    log = args.artifacts / (label + '.log')
    log.write_bytes(result.stdout)
    commands.append({'label': label, 'argv': argv, 'cwd': str(ROOT), 'exit': result.returncode, 'log': str(log)})
    (args.artifacts / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    if expect == 0:
        assert result.returncode == 0, (label, result.returncode, result.stdout[-4000:])
    else:
        assert result.returncode != 0, (label, 'negative guard unexpectedly accepted')
    return result.stdout.decode()

shared = ['flock', '--close', '/tmp/dotcc-cpython-shared-build.lock']
native = ['flock', '--close', '/tmp/dotcc-cpython-native-build.lock']
orders = [
    (['-DCHOICE=7', '-UCHOICE'], '#ifdef CHOICE\n#error removed definition remains\n#endif'),
    (['-UCHOICE', '-DCHOICE=9'], '#if CHOICE != 9\n#error later definition lost\n#endif'),
    (['-D', 'CHOICE=7', '-U', 'CHOICE', '-D', 'CHOICE=13'], '#if CHOICE != 13\n#error spaced option order lost\n#endif'),
    (['-U__STDC_HOSTED__'], '#ifdef __STDC_HOSTED__\n#error predefined seed remains\n#endif'),
    (['-U__STDC_HOSTED__', '-D__STDC_HOSTED__=0'], '#if __STDC_HOSTED__ != 0\n#error seed restoration lost\n#endif'),
]
for index, (flags, guard) in enumerate(orders):
    path = args.work/('order-'+str(index)+'.c')
    path.write_text(guard+'\nint correct_macro_order;\n')
    for label, command in [('native', native+['cc']), ('emitted', shared+['dotnet', args.compiler])]:
        output = run(label+'-order-'+str(index), command+['-std=c11', *flags, '-E', path])
        assert 'correct_macro_order' in output
response = args.work/'macro-options.rsp'
response.write_text('-DCHOICE=1\n-UCHOICE\n-DCHOICE=17\n')
path = args.work/'response-order.c'
path.write_text('#if CHOICE != 17\n#error response-file option order lost\n#endif\nint correct_response_order;\n')
for label, command in [('native', native+['cc']), ('emitted', shared+['dotnet', args.compiler])]:
    assert 'correct_response_order' in run(label+'-response-order', command+['@'+str(response), '-E', path])
source = args.work / 'source'
modules = source / 'Modules'
modules.mkdir(parents=True)
for name in ['public.c', 'limited.c', 'internal.c', 'main.c', 'api.h']:
    shutil.copy2(FIXTURE / name, modules / name)
(modules / 'Setup.stdlib.in').write_text('\n'.join('@MODULE_'+name.upper()+'_TRUE@'+name+' '+name+'.c' for name in ['public', 'limited', 'internal']) + '\n')
(modules / 'config.c.in').write_text('/* -- ADDMODULE MARKER 1 -- */\n/* -- ADDMODULE MARKER 2 -- */\n')
spec = importlib.util.spec_from_file_location('test_profile', ROOT / 'cpython/scripts/test_profile.py')
profile = importlib.util.module_from_spec(spec)
spec.loader.exec_module(profile)
ctx = SimpleNamespace(root=ROOT/'cpython', receipt={'authored_units': []})
units = [Unit(modules/'main.c')]
selected = {'static_modules': ['public', 'limited', 'internal'], 'setup_overrides': {}, 'scope': 'reduced actual role-flag contract'}
with patch.object(profile, 'definition', return_value=selected):
    profile.extend(ctx, source, units, {'static_builtins': [], 'units': []})
common = ['-std=c11', '-DPy_BUILD_CORE', '-I', modules]
# The old inherited core role must fail on the same public source under both
# native and DotCC preprocessing, independently of runtime behavior.
run('native-before-public-guard', native + ['cc', *common, '-DPy_BUILD_CORE_BUILTIN', '-c', modules/'public.c', '-o', args.work/'before.o'], expect=1)
run('emitted-before-public-guard', shared + ['dotnet', args.compiler, *common, '-DPy_BUILD_CORE_BUILTIN', modules/'public.c', '--emit=obj', '-o', args.work/'before.cs'], expect=1)
native_objects = []
emitted_objects = []
for unit in units:
    stem = unit.path.stem
    obj = args.work/(stem+'.o')
    run('native-'+stem, native + ['cc', *common, *unit.flags, '-c', unit.path, '-o', obj])
    native_objects.append(obj)
    obj = args.work/(stem+'.cs')
    run('emitted-'+stem, shared + ['dotnet', args.compiler, *common, *unit.flags, unit.path, '--emit=obj', '-o', obj])
    emitted_objects.append(obj)
run('native-link', native + ['cc', *native_objects, '-o', args.work/'native'])
expected = '11 22 33\n'
assert run('native-run', native + [args.work/'native']) == expected
project = args.work/'ProfileFlags'
run('emitted-link', shared + ['dotnet', args.compiler, *emitted_objects, '--emit=csproj', '--runtime=c', '-o', project])
run('emitted-build', shared + ['dotnet', 'build', project/'ProfileFlags.csproj', '-c', 'Release', '--disable-build-servers',
    '-m:1', '--artifacts-path', args.work/'jit-products', '-o', args.work/'jit'])
assert run('emitted-run', shared + ['dotnet', args.work/'jit/ProfileFlags.dll']) == expected
paths = [*FIXTURE.glob('*'), ROOT/'cpython/scripts/test_profile.py', args.compiler, args.compiler.with_name('DotCC.Lib.dll')]
report = {'status': 'passed', 'scope': 'public/limited/internal builtin role contract; no CPython interpreter proof',
    'stdout': expected, 'units': [{'path': str(unit.path), 'flags': list(unit.flags)} for unit in units],
    'sha256': {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in paths if p.is_file()}}
(args.artifacts/'comparison.json').write_text(json.dumps(report, indent=2)+'\n')
print(json.dumps({'status': 'passed', 'units': len(units), 'nativeAndEmitted': expected, 'before': 'both rejected'}))
