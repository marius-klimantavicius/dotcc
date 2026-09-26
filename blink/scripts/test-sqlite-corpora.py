#!/usr/bin/env python3
"""Fresh native and raw/optimized JIT/AOT runs of SQLite's five non-injection C corpora."""
import sys
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
SQLITE = ROOT / 'sqlite'
BLINK = ROOT / 'blink'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--cache', type=Path, default=SQLITE / 'ref')
args = parser.parse_args()
SELECTED = ('core', 'api', 'vtable', 'upstream', 'fts5')
MODES = ('raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot')
EXCLUDED = {
    'allocation': {'source': 'sqlite/tests/allocation_native.c',
                   'reason': 'Custom deterministic allocator fault injection (128 forced failures).'},
    'vfs': {'source': 'sqlite/tests/vfs_native.c',
            'reason': 'Custom forced read/write/truncate/sync/delete/open host failures.'},
}
# This wrapper has no override that re-enables excluded custom injection suites.
HARNESS = {'core': 'sqlite/src/probe.c', 'api': 'sqlite/tests/api_native.c',
           'vtable': 'sqlite/tests/vtable_native.c', 'fts5': 'sqlite/tests/fts5_native.c',
           'upstream': 'sqlite/scripts/generate-upstream-jsonb.py'}
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
base = BLINK / 'artifacts/sqlite-corpora'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
work = BLINK / 'generated/sqlite-corpora' / out.name
work.mkdir(parents=True)
def tools():
    return {str(p.relative_to(ROOT)): sha(p) for project in ('DotCC', 'DotCC.PostProcess')
        for p in (ROOT / project / 'bin/Release/net10.0').iterdir()
        if p.is_file() and (p.suffix == '.dll' or p.name.endswith(('.deps.json', '.runtimeconfig.json')))}
names = subprocess.check_output(['git', 'ls-files', 'sqlite'], cwd=ROOT, text=True).splitlines()
receipt = dict(kind='fresh-sqlite-five-non-injection-corpora', passed=False, tools=tools(),
    authored={n: sha(ROOT / n) for n in names}, runner_sha256=sha(Path(__file__)),
    commands=[], cases={}, archives={},
    selection={'policy': 'no-custom-resource-or-host-fault-injection',
               'selected': list(SELECTED), 'modes': list(MODES),
               'excluded': {name: dict(row, source_sha256=sha(ROOT / row['source'])) for name, row in EXCLUDED.items()},
               'harnesses': {name: {'path': path, 'sha256': sha(ROOT / path)} for name, path in HARNESS.items()},
               'expected_matrix': [{'suite': suite, 'mode': mode} for suite in SELECTED for mode in MODES],
               'ordinary_errors': 'Ordinary SQL syntax/constraint/API outcomes remain included; exclusions concern injected implementation failures.'})
env = dict(os.environ, LC_ALL='C', TMPDIR=str(out / 'tmp'), SQLITE_SOURCE_SPLIT='none',
           DOTCC_COMPILER=str(ROOT / 'DotCC/bin/Release/net10.0/dotcc.dll'))
Path(env['TMPDIR']).mkdir()
def save(): (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
def run(command, label, timeout=1200):
    command = list(map(str, command))
    row = dict(label=label, command=command)
    receipt['commands'].append(row); save()
    print('RUN ' + label, flush=True)
    start = time.monotonic()
    with (out / (label + '.out')).open('wb') as stdout, (out / (label + '.err')).open('wb') as stderr:
        result = subprocess.run(command, cwd=ROOT, env=env, stdout=stdout, stderr=stderr, timeout=timeout)
    row.update(exit_code=result.returncode, seconds=time.monotonic()-start,
               stdout_sha256=sha(out / (label + '.out')), stderr_sha256=sha(out / (label + '.err'))); save()
    if tools() != receipt['tools']: raise RuntimeError('Frozen tool drift')
    if result.returncode: raise RuntimeError(label + ' failed; see preserved output')
    return (out / (label + '.out')).read_bytes()
def sources(directory):
    return {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*') if p.is_file()
        and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
save(); print(out / 'receipt.json', flush=True)
try:
    for pin in json.loads((SQLITE / 'config/sources.json').read_text()).values():
        relative, digest = pin['archive'], pin['sha256']
        cached = args.cache / relative
        if sha(cached) != digest: raise RuntimeError('Pinned archive cache mismatch: ' + relative)
        destination = SQLITE / 'ref' / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        if not destination.exists(): shutil.copyfile(cached, destination)
        if sha(destination) != digest: raise RuntimeError('Pinned archive destination mismatch: ' + relative)
        receipt['archives'][relative] = digest
    run([sys.executable, SQLITE / 'scripts/fetch.py'], 'verify-pinned-inputs', 180)
    upstream = SQLITE / 'ref/upstream-tests/sqlite-src-3530400/test/jsonb01.test'
    receipt['upstream_test'] = {'path': str(upstream), 'sha256': sha(upstream),
                                'archive': 'upstream-tests/sqlite-src-3530400.zip',
                                'adapter': receipt['selection']['harnesses']['upstream']}
    for suite in SELECTED:
        case = receipt['cases'][suite] = dict(passed=False, modes={})
        try:
            native = 'native.sh' if suite == 'core' else 'test-' + suite + '-native.sh'
            native_output = run(['bash', SQLITE / 'scripts' / native], suite + '-native')
            expected_name = 'native-corpus.expected' if suite == 'core' else 'upstream-jsonb.expected' if suite == 'upstream' else 'native-' + suite + '.expected'
            expected_path = SQLITE / 'tests' / expected_name
            expected = expected_path.read_bytes()
            case['expected_transcript'] = {'path': str(expected_path), 'sha256': sha(expected_path)}
            if native_output != expected: raise RuntimeError('Fresh native transcript differs: ' + suite)
            case['native_output_sha256'] = hashlib.sha256(native_output).hexdigest()
            run(['bash', SQLITE / 'scripts/test-translated.sh', suite, '--emit-only'], suite + '-fresh-emission', 2400)
            generated = SQLITE / 'generated' / ('Test-' + suite)
            project = generated / ('Test-' + suite + '.csproj')
            case['raw_sources'] = sources(generated)
            shutil.copytree(generated, work / (suite + '-raw'), ignore=shutil.ignore_patterns('bin', 'obj'))
            for mode in ('raw', 'optimized'):
                if mode == 'optimized':
                    run(['dotnet', ROOT / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll', project, '--in-place'], suite + '-postprocess')
                case[mode + '_sources'] = sources(generated)
                run(['dotnet', 'build', project, '-c', 'Release', '-p:WarningsAsErrors=CS8500'], suite + '-' + mode + '-build')
                jit = generated / 'bin/Release/net10.0' / ('Test-' + suite + '.dll')
                jit_hashes = {p.name: sha(p) for p in jit.parent.glob('*.dll')}
                label = suite + '-' + mode + '-jit'
                if run(['dotnet', jit], label, 150) != expected: raise RuntimeError(label + ' transcript mismatch')
                if jit_hashes != {p.name: sha(p) for p in jit.parent.glob('*.dll')}: raise RuntimeError('JIT binary drift')
                case['modes'][mode + '-jit'] = dict(passed=True, binary_sha256=jit_hashes)
                publish = work / (suite + '-' + mode + '-aot')
                run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                    '-p:WarningsAsErrors=CS8500', '-o', publish], suite + '-' + mode + '-aot-build')
                executable = publish / ('Test-' + suite)
                digest = sha(executable); label = suite + '-' + mode + '-aot'
                if run([executable], label, 150) != expected: raise RuntimeError(label + ' transcript mismatch')
                if sha(executable) != digest: raise RuntimeError('AOT binary drift')
                case['modes'][mode + '-aot'] = dict(passed=True, binary_sha256=digest)
            if list(case['modes']) != list(MODES): raise RuntimeError('Suite mode coverage differs: ' + suite)
            if sources(work / (suite + '-raw')) != case['raw_sources']: raise RuntimeError('Raw source snapshot changed: ' + suite)
            case['passed'] = True
        except Exception as error:
            case['error'] = str(error)
        save()
    receipt['tools_after'] = tools()
    receipt['authored_after'] = {n: sha(ROOT / n) for n in names}
    if receipt['tools_after'] != receipt['tools'] or receipt['authored_after'] != receipt['authored']:
        raise RuntimeError('Frozen inputs changed')
    if sha(Path(__file__)) != receipt['runner_sha256']: raise RuntimeError('Runner changed')
    receipt['actual_matrix'] = [{'suite': suite, 'mode': mode}
        for suite, case in receipt['cases'].items() for mode in case['modes']]
    receipt['passed'] = (list(receipt['cases']) == list(SELECTED)
        and receipt['actual_matrix'] == receipt['selection']['expected_matrix']
        and all(x['passed'] for x in receipt['cases'].values()))
finally: save()
print(out / 'receipt.json', flush=True)
raise SystemExit(0 if receipt['passed'] else 1)
