"""Run the unchanged R18 controls with an explicitly verified V13 tool capture.

This is a service/compiler regression group, never interpreter qualification.
Every managed/native command takes its own nonblocking campaign build lock.
"""
from pathlib import Path
import argparse
import ast
import datetime
import hashlib
import json
import os
import pefile
import shutil
import struct
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET

R = Path(__file__).resolve().parents[3]
C = R / 'cpython'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--work', type=Path, required=True)
parser.add_argument('--artifacts', type=Path, required=True)
parser.add_argument('--handoff', type=Path, required=True)
args = parser.parse_args()
W, A = args.work.resolve(), args.artifacts.resolve()
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
read = lambda p: json.loads(Path(p).read_text())
item = lambda p: {'path': str(Path(p).resolve()), 'sha256': sha(p)}
handoff = read(args.handoff)
preflight = read(A / 'independent-preflight.json')
assert sha(args.handoff) == preflight['handoffPins']['owner-a-r10-20261018/v13-tools-handoff.json']
cli, post = Path(handoff['compiler']), Path(handoff['postprocessor'])
shared = ['flock', '-n', '--close', '/tmp/dotcc-cpython-shared-build.lock']
native = ['flock', '-n', '--close', '/tmp/dotcc-cpython-native-build.lock']
env = {**os.environ, 'DOTNET_CLI_USE_MSBUILD_SERVER': '0',
       'MSBUILDDISABLENODEREUSE': '1', 'DOTNET_CLI_TELEMETRY_OPTOUT': '1',
       'DOTNET_PROCESSOR_COUNT': '2'}
commands, cells = [], []
start = time.monotonic()


def write(name, value):
    (A / name).write_text(json.dumps(value, indent=2) + '\n')


def run(label, argv, expected=0):
    # Do not queue another command outside the authorized fifteen-minute group.
    assert time.monotonic() - start < 15 * 60, 'Group limit reached at safe boundary'
    row = {'label': label, 'command': list(map(str, argv)), 'cwd': str(R),
           'startedUtc': datetime.datetime.now(datetime.timezone.utc).isoformat()}
    commands.append(row)
    write('commands.json', commands)
    print('START', label, flush=True)
    tick = time.monotonic()
    with (A / (label + '.log')).open('w') as output:
        proc = subprocess.Popen(row['command'], cwd=R, env=env,
                                stdout=output, stderr=subprocess.STDOUT)
        row['pid'] = proc.pid
        write('commands.json', commands)
        code = proc.wait()
    row.update(exitCode=code, seconds=round(time.monotonic() - tick, 3),
               finishedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat())
    write('commands.json', commands)
    print('DONE', label, code, row['seconds'], flush=True)
    assert code == expected, (label, code, expected)
    return (A / (label + '.log')).read_text()


# Reuse the reviewed pure parser only, without executing the old seal script.
parserpath = C / 'build/owner-b-r18-20261018/release_v12_after.py'
node = next(n for n in ast.parse(parserpath.read_text()).body
            if isinstance(n, ast.FunctionDef) and n.name == 'pe_identity')
exec(compile(ast.Module(body=[node], type_ignores=[]), str(parserpath), 'exec'))
shutil.copyfile(parserpath, W / 'pe-parser-source.py.txt')


def pin(path):
    path = Path(path).resolve()
    dest = W / 'actual-input-copies' / str(path).lstrip('/')
    if not dest.exists():
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, dest)
        dest.chmod(0o444)
    assert sha(dest) == sha(path) and dest.stat().st_ino != path.stat().st_ino
    return {**item(path), 'byteCopy': str(dest), 'independentInode': True}


def bind(label, mode, project, app, products):
    name = project.stem
    binary = app / (name if mode == 'aot' else name + '.dll')
    binding = {'executable': pin(binary), 'project': pin(project)}
    if mode == 'aot':
        responses = list(products.rglob('*.ilc.rsp'))
        assert len(responses) == 1, responses
        rsp = responses[0]
        lines = rsp.read_text().splitlines()
        entries = [Path(s.strip('"')) for s in lines if s and not s.startswith('-')]
        objects = [Path(s[3:].strip('"')) for s in lines if s.startswith('-o:')]
        assert len(entries) == len(objects) == 1
        assert all(p.is_absolute() and p.resolve().is_relative_to(products) for p in entries + objects)
        actual = entries[0]
        binding.update(response=pin(rsp), nativeObject=pin(objects[0]),
                       roots=[s for s in lines if 'root' in s.lower()],
                       selectedReferences=[item(s[3:].strip('"')) for s in lines if s.startswith('-r:')])
        run(label + '-elf', ['readelf', '-dW', binary])
    else:
        actual = binary
        binding['jitOutputClosure'] = {str(p.relative_to(app)): sha(p)
                                      for p in sorted(app.rglob('*')) if p.is_file()}
    binding['actualPe'] = pe_identity(actual)
    binding['actualPeCopy'] = pin(actual)
    if actual.with_suffix('.pdb').exists():
        binding['pdb'] = pin(actual.with_suffix('.pdb'))
    return binding


def build(label, project, mode):
    app, products = W / label / 'app', W / label / 'products'
    command = ['dotnet', 'publish' if mode == 'aot' else 'build', project,
               '-c', 'Release', '--artifacts-path', products, '-o', app,
               '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false']
    if mode == 'aot':
        command += ['-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=true']
    run(label + '-build', [*shared, *command])
    binary = app / (project.stem if mode == 'aot' else project.stem + '.dll')
    text = run(label + '-run', [*shared, *(['dotnet'] if mode == 'jit' else []), binary])
    return text, bind(label, mode, project, app, products)


def verify_runtime(generated):
    text = '\n'.join(p.read_text(encoding='utf-8-sig') for p in generated.glob('*.cs'))
    rows = []
    for p in sorted((R / 'DotCC.Libc').glob('*.cs')):
        token = '// ---- ' + p.name + ' ----\n#nullable enable\n'
        if token not in text:
            continue
        assert text.count(token) == 1
        begin = text.index(token) + len(token)
        end = text.index('#nullable restore', begin)
        expected = ''.join(line + '\n' for line in p.read_text().split('\n')
                           if not line.lstrip().startswith(('#nullable', 'namespace '))
                           and not line.startswith('using ')) + '\n'
        assert text[begin:end] == expected, p
        rows.append(item(p))
    assert len(rows) == 76, len(rows)
    return rows


try:
    # Real CLR resource-name/offset extraction, followed by independent byte
    # decoding below. No resource, generated source or fixture is repaired.
    inspector = cli.parent.parent / 'v13-resource-inspector/ToolResources.dll'
    specification = C / 'artifacts/owner-a-r10-20261018/compiler-resource-source-specification.json'
    reportpath = A / 'actual-compiler-resources.json'
    run('actual-compiler-resources', [*shared, 'dotnet', inspector,
                                    cli.parent / 'DotCC.Lib.dll', specification, reportpath])
    report = read(reportpath)
    assert report['runtimeResourceCount'] == 83 and report['headerResourceCount'] == 68
    pe = pefile.PE(str(cli.parent / 'DotCC.Lib.dll'))
    cor = pe.OPTIONAL_HEADER.DATA_DIRECTORY[14].VirtualAddress
    rva, length = struct.unpack('<II', pe.get_data(cor + 24, 8))
    blob = pe.get_data(rva, length)
    for row in report['sources']:
        offset = row['offsetWithinClrResources']
        size = struct.unpack('<I', blob[offset:offset + 4])[0]
        data = blob[offset + 4:offset + 4 + size]
        assert size == row['bytes'] and data == Path(row['source']).read_bytes()
        assert hashlib.sha256(data).hexdigest() == row['sha256']
        dest = W / 'decoded-resources' / row['resourceName']
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_bytes(data)
    for name in ['strftime', 'wait']:
        source = C / 'tests/Abi' / (name + '_profile_r18.c')
        assert sha(source) == preflight['sourceHashes'][str(source.relative_to(R))]
        native_exe = W / (name + '-native')
        run(name + '-native-build', [*native, 'cc', '-std=c11', '-O2', '-Wall',
                                    '-Wextra', '-Werror', source, '-o', native_exe])
        oracle = run(name + '-native-run', [*native, native_exe])
        pin(native_exe)
        for route in ['direct', 'object']:
            label = name + '-' + route
            generated = W / label
            opts = ['-DDOTCC_MANAGED_REJECTION=1'] if name == 'wait' else []
            if route == 'direct':
                run(label + '-emit', [*shared, 'dotnet', cli, '--emit=csproj',
                                     '--runtime=c', '-std=c11', *opts, source, '-o', generated])
            else:
                obj = W / (name + '.obj')
                run(label + '-object-emit', [*shared, 'dotnet', cli, '--emit=obj',
                                            '-std=c11', *opts, source, '-o', obj])
                pin(obj)
                run(label + '-object-link', [*shared, 'dotnet', cli, obj,
                                            '--emit=csproj', '--runtime=c', '-o', generated])
            runtime = verify_runtime(generated)
            project = generated / (label + '.csproj')
            for mode in ['jit', 'aot']:
                cell = label + '-' + mode
                observed, binding = build(cell, project, mode)
                if name == 'strftime':
                    assert observed == oracle == 'strftime ordinary=19 wide=19 tight=0 empty=0 failures=0\n'
                else:
                    assert observed.splitlines()[:-1] == oracle.splitlines()[:-1]
                    assert observed.splitlines()[-1] == 'wait rejection=-1 errno=1 status=305419896 failures=0'
                    assert oracle.splitlines()[-1] == 'wait rejection=-1 errno=10 status=305419896 failures=0'
                cells.append({'name': name, 'route': route, 'mode': mode, 'form': 'raw',
                              'source': item(source), 'nativeOracle': pin(native_exe),
                              'stdout': observed, 'binding': binding,
                              'generatedSources': {str(p): sha(p) for p in generated.glob('*.cs')},
                              'runtimeSourceBindings': runtime})
                write('results.json', {'status': 'running', 'cells': cells})
    # Existing actual-source tests verify compatibility, function-pointer ABI,
    # bounds/NUL/errno/sentinel behavior independently of generated C dispatch.
    direct = C / 'tests/Abi/BootstrapDirect/BootstrapDirect.csproj'
    direct_results = []
    for mode in ['jit', 'aot']:
        text, binding = build('runtime-direct-' + mode, direct, mode)
        assert text == ('233 direct shared-service assertions; dynamicCode=' +
                        ('True' if mode == 'jit' else 'False') + '\n')
        direct_results.append({'mode': mode, 'assertions': 233, 'binding': binding, 'stdout': text})
    unit = C / 'tests/Abi/BootstrapUnit/BootstrapUnit.csproj'
    run('runtime-units', [*shared, 'dotnet', 'test', unit, '-c', 'Release',
                          '--artifacts-path', W / 'unit-products', '--disable-build-servers', '-m:1',
                          '-p:UseSharedCompilation=false', '--logger', 'trx;LogFileName=bootstrap.trx',
                          '--results-directory', A / 'unit-results'])
    counters = ET.parse(A / 'unit-results/bootstrap.trx').find(
        '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}ResultSummary/'
        '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters').attrib
    assert counters['total'] == counters['passed'] == '27'
    assert counters['failed'] == counters['notExecuted'] == '0'
    for p, expected in preflight['sourceHashes'].items():
        assert sha(R / p) == expected
    write('results.json', {'status': 'passed', 'cells': cells,
                           'directRuntime': direct_results, 'unitCounters': counters,
                           'seconds': round(time.monotonic() - start, 3),
                           'scope': 'Unchanged R18 C, native and 8 emitted raw cells; direct-runtime compatibility controls. No interpreter/profile/P6 qualification.'})
except BaseException as error:
    write('group-failure.json', {'error': repr(error), 'completedCells': len(cells),
                                 'seconds': round(time.monotonic() - start, 3),
                                 'lastCommand': commands[-1] if commands else None})
    raise
