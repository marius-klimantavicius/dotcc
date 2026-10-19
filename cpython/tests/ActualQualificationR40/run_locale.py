"""Matched native Host / exact existing actual PE locale reduction and retry."""
import argparse
import json
import os
from pathlib import Path
import shutil

from run_actual import ART, WORK, CPY, HERE, ROOT, read, write, item, verify, payload, preserve_payload
from campaigns.process import execute
from campaigns.recipes import helper


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--full', action='store_true')
    parser.add_argument('--exported-native-control', action='store_true')
    parser.add_argument('--seal-native-helper', action='store_true')
    args = parser.parse_args()
    if args.seal_native_helper:
        args.exported_native_control = True
    command_path = ART / ('locale-corrected-full-commands.json' if args.full else
        'locale-control-helper-link-commands.json' if args.seal_native_helper else
        'locale-control-exported-native-commands.json' if args.exported_native_control else 'locale-control-commands.json')
    assert not command_path.exists()
    commands = []
    environment = dict(os.environ, LC_ALL='C.UTF-8', LANG='C.UTF-8', PYTHONUTF8='1',
        PYTHONDONTWRITEBYTECODE='1', PYTHONPATH='poison-r40', PYTHONHOME='/absent-poison-r40',
        DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0', MSBUILDDISABLENODEREUSE='1',
        UseSharedCompilation='false')

    def run(argv, label, *, native=False, overrides=None, check=True, timeout=120):
        env = dict(environment, **(overrides or {}))
        record = {'declared_environment': {k: v for k, v in env.items() if k.startswith(('DOTCC_', 'PYTHON', 'LD_'))
                  or k in ('LC_ALL', 'LANG', 'DOTNET_PROCESSOR_COUNT', 'DOTNET_CLI_USE_MSBUILD_SERVER',
                           'MSBUILDDISABLENODEREUSE', 'UseSharedCompilation')}}
        commands.append(record)
        lock = '/tmp/dotcc-cpython-native-build.lock' if native else '/tmp/dotcc-cpython-shared-build.lock'
        try:
            execute(['flock', '-n', '--close', lock, *argv], cwd=Path('/tmp'), label=label,
                output=ART / (label + '.log'), timeout=timeout, env=env, record=record, check=check)
        finally:
            write(command_path.name, commands)
        print(json.dumps({'label': label, 'status': record['status'], 'exit_code': record.get('exit_code'),
                          'seconds': record.get('seconds')}), flush=True)

    authority = read(ART / 'direct-a31-authority.json')
    release = read(authority['release']['path'])
    publication = read(release['publication']['path'])
    project = Path(publication['project']['path'])
    actual = WORK / 'app'
    probe_output, products = WORK / 'locale-entry', WORK / 'locale-products'
    entry = probe_output / 'LocaleControlR40.dll'
    inventory = CPY / 'tests/Upstream/inventory.frozen.json'
    original = read(ART / 'full-required-raw-jit.json')
    ids = ('test.test_codecs.LocaleCodecTest.test_decode_surrogatepass',
           'test.test_codecs.LocaleCodecTest.test_encode_surrogatepass',
           'test.test_importlib.metadata.test_main.FileSystem.test_unicode_dir_on_sys_path')
    details = {r['case']: r for group in original['discovery'] for r in group['case_details']}
    inputs = {'cases': [{'case': case, 'detail': details[case],
                         'sourceSha256': item(details[case]['upstream_path'])['sha256']} for case in ids]}

    def add_probe(app):
        shutil.copyfile(HERE / 'locale_probe.py', app / 'locale_probe.py')
        (app / 'locale-probe-inputs.json').write_text(json.dumps(inputs) + '\n')

    if not args.full:
        assert read(ART / 'full-journal-counts.json')['uniqueCompleted'] == 4867
        if not args.exported_native_control:
            run(['dotnet', 'build', HERE / 'LocaleControlR40.csproj', '-c', 'Release', '--disable-build-servers',
             '-m:1', '-p:UseSharedCompilation=false', '-nodeReuse:false', '-p:NuGetAudit=false',
             f'-p:ActualInputDirectory={actual}', '--artifacts-path', products, '-o', probe_output],
                'locale-reference-entry-build')
        for name in ('Managed.CPython.dll', 'TranslatedCPython.dll'):
            assert item(probe_output / name)['sha256'] == item(actual / name)['sha256']
        identity = helper(CPY / 'build/owner-a-r15-20261018/pe_identity.py')
        binder = helper(CPY / 'tests/Audit/actual_profile_input.py')
        proof = {'entry': identity.pe_identity(entry),
            'sourceBinding': binder.bind(CPY, entry, HERE / 'LocaleControlR40.csproj',
                CPY / 'artifacts/owner-a-r24-20261018/pdb_sources_mapped.py', products),
            'references': [item(p) for p in sorted(probe_output.glob('*.dll'))],
            'unchangedActualProductProof': item(ART / 'actual-input-proof.json'),
            'beforeFullAndFocusedRetained': [item(ART / p) for p in
                ('full-required-raw-jit.json', 'full-required-raw-jit.journal.jsonl', 'focused-104-raw-jit.json')]}
        if args.exported_native_control:
            assert read(ART / 'locale-entry-input-proof.json') == proof
        else:
            write('locale-entry-input-proof.json', proof)
        observer = WORK / 'native-locale-observer.so'
        if not args.exported_native_control:
            run(['cc', '-std=c11', '-D_POSIX_C_SOURCE=200809L', '-shared', '-fPIC', '-O2',
                 HERE / 'native_locale_observer.c', '-o', observer], 'native-locale-observer-build', native=True)
        native = CPY / 'build/owner-b-r37-20261018/native-full'
        for ref in read(CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-inputs.json'):
            verify(ref)
        native_executable = native / 'startup-control'
        if args.exported_native_control:
            native_executable = WORK / ('native-configured-locale-control-helpers' if args.seal_native_helper else 'native-configured-locale-control')
            run(['cc', '-std=c11', '-O2', '-Wl,--export-dynamic', '-I', native / 'build',
                '-I', native / 'source/Include', '-I', CPY / 'src/Host', HERE / 'native_locale_control.c',
                CPY / 'src/Host/dotcc_cpython_host.c', native / 'build/libpython3.13.a',
                *(['-Wl,-u,_PyInterpreterConfig_InitFromDict'] if args.seal_native_helper else []),
                '-lm', '-ldl', '-lpthread', '-o', native_executable],
                'native-configured-locale-control-helper-link' if args.seal_native_helper else 'native-configured-locale-control-build', native=True)
        native_reports = {}
        for selection in ('environment', 'selected'):
            tag = ('helpers-' if args.seal_native_helper else 'exported-' if args.exported_native_control else '')
            base = WORK / ('native-locale-' + tag + selection)
            library, app = base / 'stdlib', base / 'app'
            shutil.copytree(native / 'consumer/lib/python3.13', library,
                            ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
            app.mkdir()
            add_probe(app)
            shutil.copyfile(CPY / 'tests/Upstream/run_inventory.py', app / 'run_inventory.py')
            report = ART / ('native-locale-' + tag + selection + '.json')
            for cycle in range(6):
                script = 'VALUE = 37\n'
                if cycle == 1:
                    script += ('import sys\nsys.path.append(' + repr(str(library / 'lib-dynload')) + ')\n'
                               'import locale_probe\nlocale_probe.run_probe(' + repr(str(report)) + ')\n')
                (app / ('startup_fixture_' + str(cycle) + '.py')).write_text(script)
            argv = [native_executable, library, app, report] if args.exported_native_control else [native_executable, library, app]
            run(argv, 'native-exact-host-locale-' + tag + selection,
                native=True, overrides={'LD_PRELOAD': str(observer),
                    'DOTCC_R40_SELECT_LOCALE': '1' if selection == 'selected' else '0'})
            native_reports[selection] = item(report)
        actual_reports = {}
        for route in ('explicit', 'legacy'):
            for selection in ('environment', 'selected'):
                label = 'actual-locale-' + route + '-' + selection
                report = ART / (label + '.json')
                if args.seal_native_helper:
                    assert report.is_file()
                    manifest = item(ART / (label + '-payload.json'))
                else:
                    library, app, manifest = payload(project, label)
                    add_probe(app)
                    run(['dotnet', entry, route, selection, 'probe', library, app, inventory, report], label)
                actual_reports[route + '-' + selection] = item(report)
                preserve_payload(manifest)
        comparisons = []
        for route in ('explicit', 'legacy'):
            for selection in ('environment', 'selected'):
                a = read(actual_reports[route + '-' + selection]['path'])
                n = read(native_reports[selection]['path'])
                for stage in ('beforeTests', 'afterTests'):
                    for key in ('locale', 'getencoding', 'getpreferredencodingFalse', 'filesystemencoding',
                                'FS_NONASCII', 'utf8_mode', 'isolated', 'ignore_environment'):
                        assert a[stage][key] == n[stage][key], (route, selection, stage, key, a[stage], n[stage])
                statuses = lambda r: [(c['case'], c['status'], c.get('reason')) for c in r['cases']]
                assert statuses(a) == statuses(n), (route, selection)
                assert [r['status'] for r in a['cases']] == (
                    ['required-skip', 'required-skip', 'error'] if selection == 'environment' else ['required-skip', 'required-skip', 'passed'])
                comparisons.append({'route': route, 'selection': selection, 'exactObservationsAndStatusesMatch': True,
                                    'native': native_reports[selection], 'actual': actual_reports[route + '-' + selection]})
        write('locale-control-result.json', {'status': 'matched native configured Host/actual controls complete',
            'comparisons': comparisons, 'nativeExecutable': item(native_executable),
            'nativeObserverControlOnly': item(observer), 'commands': item(command_path),
            'classification': 'Missing consumer locale setup before isolated preinit causes the Unicode-path ERROR; explicit consumer C.UTF-8 fixes it and matches native. Two surrogatepass SKIPs persist natively with utf8_mode=0, outside authorized5; acceptance stays blocked. No runtime/header/Host/config repair justified.',
            'actualReferenceProof': item(ART / 'locale-entry-input-proof.json')})
    else:
        assert read(ART / 'locale-control-result.json')['status'].startswith('matched native')
        for ref in read(ART / 'locale-entry-input-proof.json')['references']:
            verify(ref)
        library, app, manifest = payload(project, 'locale-corrected-full')
        report = ART / 'locale-corrected-full-required-raw-jit.json'
        run(['dotnet', entry, 'explicit', 'selected', 'full', library, app, app / inventory.name, report],
            'locale-corrected-full-required-raw-jit', check=False, timeout=900)
        from reporting import count_journal
        write('locale-corrected-full-journal-counts.json', count_journal(report.with_suffix('.journal.jsonl'), read(inventory)['required_cases']))
        write('locale-corrected-full-preservation.json', preserve_payload(manifest))


if __name__ == '__main__':
    main()
