"""Bounded mode1 native/actual TWO-case proof; full-suite interpretation separate."""
import json
import os
from pathlib import Path
import shutil

from run_actual import ART, WORK, HERE, CPY, ROOT, item, read, write, verify, payload, preserve_payload
from campaigns.process import execute
from campaigns.recipes import helper

commands = []
receipt = ART / 'complementary-commands.json'
assert not receipt.exists()
environment = dict(os.environ, LC_ALL='C.UTF-8', LANG='C.UTF-8', PYTHONUTF8='0',
    PYTHONDONTWRITEBYTECODE='1', PYTHONPATH='poison-r40', PYTHONHOME='/absent-poison-r40',
    DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0', MSBUILDDISABLENODEREUSE='1',
    UseSharedCompilation='false')


def run(argv, label, *, native=False, overrides=None):
    env = dict(environment, **(overrides or {}))
    record = {'declared_environment': {k: v for k, v in env.items() if k.startswith(('DOTCC_', 'PYTHON', 'LD_')) or k in ('LC_ALL', 'LANG')}}
    commands.append(record)
    lock = '/tmp/dotcc-cpython-native-build.lock' if native else '/tmp/dotcc-cpython-shared-build.lock'
    try:
        execute(['flock', '-n', '--close', lock, *argv], label=label, cwd=Path('/tmp'),
            output=ART / (label + '.log'), timeout=120, env=env, record=record)
    finally:
        write(receipt.name, commands)
    print(json.dumps({'label': label, 'status': record['status'], 'seconds': record['seconds']}), flush=True)


assert read(ART / 'locale-control-result.json')['status'].startswith('matched native')
actual = WORK / 'app'
products, output = WORK / 'complementary-products', WORK / 'complementary-entry'
project = HERE / 'ComplementaryR40.csproj'
run(['dotnet', 'build', project, '-c', 'Release', '--disable-build-servers', '-m:1',
     '-p:UseSharedCompilation=false', '-nodeReuse:false', '-p:NuGetAudit=false',
     f'-p:ActualInputDirectory={actual}', '--artifacts-path', products, '-o', output], 'complementary-reference-build')
for name in ('TranslatedCPython.dll', 'Managed.CPython.dll'):
    assert item(output / name)['sha256'] == item(actual / name)['sha256']
identity = helper(CPY / 'build/owner-a-r15-20261018/pe_identity.py')
binder = helper(CPY / 'tests/Audit/actual_profile_input.py')
entry = output / 'ComplementaryR40.dll'
write('complementary-input-proof.json', {'entry': identity.pe_identity(entry),
    'sourceBinding': binder.bind(CPY, entry, project, CPY / 'artifacts/owner-a-r24-20261018/pdb_sources_mapped.py', products),
    'references': [item(p) for p in sorted(output.glob('*.dll'))],
    'unchangedActualProductProof': item(ART / 'actual-input-proof.json')})
native = CPY / 'build/owner-b-r37-20261018/native-full'
exe = WORK / 'native-complementary-control'
run(['cc', '-std=c11', '-O2', '-Wl,--export-dynamic', '-Wl,-u,_PyInterpreterConfig_InitFromDict',
    '-I', native / 'build', '-I', native / 'source/Include', '-I', CPY / 'src/Host',
    HERE / 'native_complementary_control.c', CPY / 'src/Host/dotcc_cpython_host.c',
    native / 'build/libpython3.13.a', '-lm', '-ldl', '-lpthread', '-o', exe],
    'native-complementary-build', native=True)
full = read(ART / 'full-required-raw-jit.json')
details = {r['case']: r for g in full['discovery'] for r in g['case_details']}
ids = ['test.test_codecs.LocaleCodecTest.test_decode_surrogatepass', 'test.test_codecs.LocaleCodecTest.test_encode_surrogatepass']
inputs = {'cases': [{'case': case, 'detail': details[case], 'sourceSha256': item(details[case]['upstream_path'])['sha256']} for case in ids]}


def add_probe(app):
    shutil.copyfile(HERE / 'complementary_probe.py', app / 'locale_probe.py')
    (app / 'locale-probe-inputs.json').write_text(json.dumps(inputs) + '\n')


base = WORK / 'native-complementary'
library, app = base / 'stdlib', base / 'app'
shutil.copytree(native / 'consumer/lib/python3.13', library, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
app.mkdir()
add_probe(app)
shutil.copyfile(CPY / 'tests/Upstream/run_inventory.py', app / 'run_inventory.py')
native_report = ART / 'native-complementary-two.json'
run([exe, library, app, native_report], 'native-complementary-two', native=True,
    overrides={'LD_PRELOAD': str(WORK / 'native-locale-observer.so'), 'DOTCC_R40_SELECT_LOCALE': '1'})
publication = read(read(read(ART / 'direct-a31-authority.json')['release']['path'])['publication']['path'])
library, app, manifest = payload(Path(publication['project']['path']), 'actual-complementary-two')
add_probe(app)
actual_report = ART / 'actual-complementary-two.json'
run(['dotnet', entry, 'explicit', 'selected', 'probe', library, app, app / 'inventory.frozen.json', actual_report], 'actual-complementary-two')
a, n = read(actual_report), read(native_report)
for report in (a, n):
    assert report['beforeTests']['utf8_mode'] == report['afterTests']['utf8_mode'] == 1
    assert report['beforeTests']['locale'] == 'C.UTF-8' and report['executed'] == 2
    assert [r['case'] for r in report['cases']] == ids
    assert all(r['status'] == 'passed' for r in report['cases'])
for stage in ('beforeTests', 'afterTests'):
    for key in ('locale', 'getencoding', 'getpreferredencodingFalse', 'filesystemencoding', 'FS_NONASCII', 'utf8_mode', 'isolated', 'ignore_environment'):
        assert a[stage][key] == n[stage][key], key
write('complementary-result.json', {'status': 'TWO unchanged cases PASS under both native and actual supported mode1',
    'native': item(native_report), 'actual': item(actual_report), 'actualInput': item(ART / 'complementary-input-proof.json'),
    'nativeInputs': [item(p) for p in (exe, HERE / 'native_complementary_control.c', CPY / 'src/Host/dotcc_cpython_host.c', native / 'build/libpython3.13.a')],
    'commands': item(receipt), 'preservation': preserve_payload(manifest),
    'mode0OutcomeRemains': 'Both required-skip; native/actual exact mode0 observations retained',
    'mode1Original104ConditionalSkipsRetained': item(CPY / 'artifacts/owner-b-r37-20261018/native-104-utf8-1-suite-loaders.json'),
    'acceptanceInterpretation': 'Complementary witnessed-pass coverage proposal only; strict five-exception full-cell gate remains blocked. Supervisor review required; no broader waiver or full mode1 result inferred.'})
print(json.dumps({'nativePassed': 2, 'actualPassed': 2, 'coverageOnly': True}))
