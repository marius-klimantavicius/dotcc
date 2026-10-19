"""B40 actual V22 controls, only after the DIRECT A31 receipt and genuine SAFE.

Shared process/lock helpers run ordinary consumer builds and executions. No
compiler build, emission, native rebuild, canonical write or publication occurs.
"""
import argparse
import datetime
import fcntl
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sys
import traceback

ROOT = Path(__file__).resolve().parents[3]
CPY = ROOT / 'cpython'
HERE = Path(__file__).resolve().parent
ART = CPY / 'artifacts/owner-b-r40-20261018'
WORK = CPY / 'build/owner-b-r40-20261018'
sys.path.insert(0, str(CPY / 'tests/ActualValidationR36'))
from reporting import item, count_journal
sys.path.insert(0, str(ROOT / 'Scripts'))
from campaigns.process import execute
from campaigns.recipes import helper


def read(path):
    return json.loads(Path(path).read_text())


def verify(reference):
    assert item(reference['path'])['sha256'] == reference['sha256'], reference['path']
    return reference


def write(name, value):
    path = ART / name
    temporary = path.with_suffix(path.suffix + '.pending')
    temporary.write_text(json.dumps(value, indent=2, default=str) + '\n')
    temporary.replace(path)
    return item(path)


def authorize(authority_path):
    authority = read(authority_path)
    verify(authority['release'])
    release = read(authority['release']['path'])
    assert authority['directA31MessageReceived'] and release['activeOwned'] == release['queuedOwned'] == []
    assert len(release['terminalPidsAbsent']) == 328
    assert all(not Path('/proc', str(pid)).exists() for pid in release['terminalPidsAbsent'])
    assert len(release['locks']) == 3 and all(row['independentAcquireAndRelease'] for row in release['locks'])
    for key in ('receipt', 'publication', 'sourceBinding', 'capture', 'sameC', 'actualRawPE'):
        verify(release[key])
    receipt = read(release['receipt']['path'])
    assert receipt['status'] == 'passed' and receipt['profile'] == 'upstream-tests-static'
    assert receipt['hash_policy'] == 'strict' and receipt['selection']['form'] == 'raw'
    assert receipt['selection']['jobs'] == 8 and len(receipt['units']) == 286
    assert set(receipt['outputs']) == {'raw'}
    capture = read(release['capture']['path'])
    for key in ('driver', 'compilerLibrary', 'postprocessor'):
        verify(capture['tools'][key]['pe'])
    assert receipt['tools']['DotCC']['dotcc.dll'] == capture['tools']['driver']['pe']['sha256']
    assert receipt['tools']['DotCC']['DotCC.Lib.dll'] == capture['tools']['compilerLibrary']['pe']['sha256']
    for path, digest in release['sourcePins'].items():
        assert item(path)['sha256'] == digest, path
    native = read(CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-inputs.json')
    for reference in native:
        verify(reference)
    for lock in release['locks']:
        with Path(lock['path']).open('r+') as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            fcntl.flock(stream, fcntl.LOCK_UN)
    return release, Path(receipt['outputs']['raw']['project'])


def runtime_sections(project, release):
    required = ('PipeLib.cs', 'PosixLib.cs', 'PosixFsLib.cs', 'PthreadLib.cs', 'PthreadAttributesLib.cs')
    sources = {Path(path).name: {'path': path, 'sha256': digest}
               for path, digest in release['sourcePins'].items() if Path(path).name in required}
    assert len(sources) == 5
    text = '\n'.join(p.read_text(encoding='utf-8-sig') for p in sorted(project.parent.glob('*.cs')))
    for name, reference in sources.items():
        verify(reference)
        marker = '// ---- ' + name + ' ----\n#nullable enable\n'
        assert text.count(marker) == 1, name
        start = text.index(marker) + len(marker)
        end = text.index('#nullable restore', start)
        expected = ''.join(line + '\n' for line in Path(reference['path']).read_text().split('\n')
            if not line.lstrip().startswith(('#nullable', 'namespace ')) and not line.startswith('using ')) + '\n'
        assert text[start:end] == expected, name
    return list(sources.values())


def payload(project, label):
    destination = WORK / (label + '-payload')
    assert not destination.exists(), 'Fresh per-cell consumer payload required'
    source = project.parent / 'stdlib'
    forbidden = [str(p) for p in source.rglob('*') if p.is_file() and p.suffix in ('.so', '.pyd', '.dll', '.dylib')]
    assert not forbidden, forbidden
    library = destination / 'stdlib'
    shutil.copytree(source, library, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    app = destination / 'app'
    app.mkdir()
    for path in (CPY / 'tests/Upstream/run_inventory.py', CPY / 'tests/Upstream/inventory.frozen.json',
                 HERE / 'focused_markers.py'):
        shutil.copyfile(path, app / path.name)
    markers_path = CPY / 'artifacts/owner-b-r36-20261018/required-marker-source-inventory.json'
    native_path = CPY / 'artifacts/owner-b-r37-20261018/native-marker-adjudication.json'
    native_imports = read(CPY / 'artifacts/owner-b-r37-20261018/native-imports.json')
    inputs = {'markers': read(markers_path), 'native': read(native_path),
              'nativeEvidence': item(native_path),
              'selectedModules': [r['name'] for r in native_imports['selectedModuleImports']]}
    (app / 'focused-inputs.json').write_text(json.dumps(inputs) + '\n')
    for cycle in range(6):
        (app / ('startup_fixture_' + str(cycle) + '.py')).write_text('VALUE = 37\n')
    rows = []
    for original in sorted(source.rglob('*')):
        if not original.is_file() or '__pycache__' in original.parts or original.suffix == '.pyc':
            continue
        copied = library / original.relative_to(source)
        a, b = item(original), item(copied)
        assert a['sha256'] == b['sha256']
        rows.append({'original': a, 'copy': b})
    manifest = write(label + '-payload.json', {'source': str(source), 'destination': str(destination),
        'targetSysconfigOnly': True, 'nativeDSOs': forbidden, 'files': rows,
        'application': [item(p) for p in sorted(app.iterdir())]})
    return library, app, manifest


def preserve_payload(manifest):
    content = read(manifest['path'])
    for row in content['files']:
        verify(row['original'])
        verify(row['copy'])
    for reference in content['application']:
        verify(reference)
    generated = [str(p) for p in Path(content['destination']).rglob('*.pyc')]
    return {'allOriginalAndConsumerSourceBytesPreserved': True, 'generatedCacheFiles': len(generated)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--authority', type=Path, required=True)
    parser.add_argument('--phase', choices=('startup', 'qualify'), required=True)
    args = parser.parse_args()
    ART.mkdir(parents=True, exist_ok=True)
    WORK.mkdir(parents=True, exist_ok=True)
    release, project = authorize(args.authority)
    commands_path = ART / (args.phase + '-commands.json')
    assert not commands_path.exists(), 'Preserve previous attempt; no blind rerun'
    commands = []
    environment = dict(os.environ, DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0',
        MSBUILDDISABLENODEREUSE='1', UseSharedCompilation='false', DOTCC_B29_ORIGINAL_FOUR='1',
        LC_ALL='C.UTF-8', LANG='C.UTF-8', PYTHONUTF8='1', PYTHONDONTWRITEBYTECODE='1',
        PYTHONPATH='poison-r40', PYTHONHOME='/absent-poison-r40')
    declared = ('DOTNET_PROCESSOR_COUNT', 'DOTNET_CLI_USE_MSBUILD_SERVER', 'MSBUILDDISABLENODEREUSE',
                'UseSharedCompilation', 'DOTCC_B29_ORIGINAL_FOUR', 'LC_ALL', 'LANG', 'PYTHONUTF8',
                'PYTHONDONTWRITEBYTECODE', 'PYTHONPATH', 'PYTHONHOME')

    def run(argv, label, timeout=120, check=True):
        record = {'declared_environment': {key: environment[key] for key in declared}}
        commands.append(record)
        try:
            execute(['flock', '-n', '--close', '/tmp/dotcc-cpython-shared-build.lock', *argv],
                label=label, cwd=Path('/tmp'), output=ART / (label + '.log'), env=environment,
                timeout=timeout, record=record, check=check)
        finally:
            write(commands_path.name, commands)
        print(json.dumps({'label': label, 'status': record['status'], 'exit_code': record.get('exit_code'),
                          'seconds': record.get('seconds')}), flush=True)
        return record

    products, output = WORK / 'products', WORK / 'app'
    entry_project = HERE / 'ActualQualificationR40.csproj'
    entry = output / 'ActualQualificationR40.dll'
    inventory = CPY / 'tests/Upstream/inventory.frozen.json'
    if args.phase == 'startup':
        write('actual-entry.json', {'authority': item(args.authority), 'release': item(authority_release_path(args.authority)),
            'utc': datetime.datetime.now(datetime.timezone.utc).isoformat(),
            'nativeHostReuse': item(CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-inputs.json'),
            'originalNativeHostTranscript': item(CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-suite-loaders.log'),
            'ownSources': [item(p) for p in sorted(HERE.iterdir()) if p.is_file()]})
        run(['dotnet', 'build', entry_project, '-c', 'Release', '--disable-build-servers', '-m:1',
             '-p:UseSharedCompilation=false', '-nodeReuse:false', '-p:NuGetAudit=false',
             '-p:RestoreIgnoreFailedSources=true', f'-p:CPythonProject={project}',
             '--artifacts-path', products, '-o', output], 'actual-entry-build', timeout=240)
        identity = helper(CPY / 'build/owner-a-r15-20261018/pe_identity.py')
        binder = helper(CPY / 'tests/Audit/actual_profile_input.py')
        mapper = CPY / 'artifacts/owner-a-r24-20261018/pdb_sources_mapped.py'
        proof = {'publication': release['publication'], 'capture': release['capture'],
                 'sameC': release['sameC'], 'runtimeSections': runtime_sections(project, release),
                 'runtimeConfiguration': item(entry.with_suffix('.runtimeconfig.json')),
                 'dependencyManifest': item(entry.with_suffix('.deps.json')),
                 'references': [item(p) for p in sorted(output.glob('*.dll'))]}
        for name, pe, actual_project in (
                ('entry', entry, entry_project),
                ('facade', output / 'Managed.CPython.dll', CPY / 'src/Managed.CPython/Managed.CPython.csproj'),
                ('library', output / 'TranslatedCPython.dll', project)):
            proof[name] = {'identity': identity.pe_identity(pe),
                'sourceBinding': binder.bind(CPY, pe, actual_project, mapper, products)}
        write('actual-input-proof.json', proof)
        for mode in ('host', 'startup'):
            library, app, manifest = payload(project, mode)
            record = run(['dotnet', entry, mode, library, app, inventory, ART / (mode + '.json')],
                         'actual-' + mode, timeout=120)
            preservation = preserve_payload(manifest)
            caches = [str(p.relative_to(app)) for p in app.rglob('startup_fixture_*.pyc')]
            assert len(caches) == 2 and all(any('startup_fixture_' + str(cycle) in p for p in caches) for cycle in (3, 4)), caches
            actual = (ART / ('actual-' + mode + '.log')).read_text()
            expected = (CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-suite-loaders.log').read_text()
            if mode == 'host':
                assert actual == expected, 'Actual translated Host/native exact transcript mismatch'
            write(mode + '-result.json', {'command': record, 'payload': manifest, 'preservation': preservation,
                'fixtureCaches': caches, 'nativeHostTranscriptExact': mode == 'host',
                'generations': 6, 'allFourCombinationsAndDefaults': True,
                'actualInputProof': item(ART / 'actual-input-proof.json'), 'status': 'passed'})
    else:
        assert read(ART / 'host-result.json')['status'] == read(ART / 'startup-result.json')['status'] == 'passed'
        proof = read(ART / 'actual-input-proof.json')
        for reference in proof['references']:
            verify(reference)
        for mode in ('markers', 'full'):
            library, app, manifest = payload(project, mode)
            report = ART / ('focused-104-raw-jit.json' if mode == 'markers' else 'full-required-raw-jit.json')
            record = run(['dotnet', entry, mode, library, app, app / inventory.name, report],
                         'actual-' + mode, timeout=900, check=False)
            if report.with_suffix('.journal.jsonl').exists():
                selected = (read(app / 'focused-inputs.json')['markers']['cases'] if mode == 'markers'
                            else read(inventory)['required_cases'])
                ids = [r['case'] for r in selected] if mode == 'markers' else selected
                write(mode + '-journal-counts.json', count_journal(report.with_suffix('.journal.jsonl'), ids))
            ordinary = read(report) if report.exists() else {}
            write(mode + '-result.json', {'command': record, 'payload': manifest,
                'preservation': preserve_payload(manifest), 'report': item(report) if report.exists() else None,
                'scope': 'Required markers remain nonpasses; full matrix/P0-P6 open'})
            if mode == 'markers':
                assert ordinary.get('all104ExactNativeStatusesMatch'), 'Actual focused mismatch; preserve/reduce before long suite'
            else:
                assert ordinary['selected_case_ids'] == read(inventory)['required_cases']
    for path, digest in release['sourcePins'].items():
        assert item(path)['sha256'] == digest, path


def authority_release_path(path):
    return read(path)['release']['path']


if __name__ == '__main__':
    try:
        main()
    except BaseException:
        ART.mkdir(parents=True, exist_ok=True)
        write('actual-blocker-' + datetime.datetime.now(datetime.timezone.utc).strftime('%H%M%S') + '.json',
              {'status': 'actual command/input/harness blocker', 'traceback': traceback.format_exc()})
        raise
