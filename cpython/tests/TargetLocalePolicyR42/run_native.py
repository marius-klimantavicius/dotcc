"""Bounded new same-feature native oracle; run only after direct A32 SAFE.

Uses the shared process helper, unchanged pinned source, and individual native
leases. No product generation or native-library substitution is performed.
"""
import argparse
import datetime
import fcntl
import hashlib
import json
import os
from pathlib import Path
import shutil
import sys
import tarfile
import time

CPY = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
WORK = CPY / 'build/owner-b-r42-20261018'
sys.path.insert(0, str(CPY.parent / 'Scripts'))
from campaigns.process import execute


def read(path):
    return json.loads(Path(path).read_text())


def ref(path):
    path = Path(path).resolve()
    return dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), bytes=path.stat().st_size)


def write(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n')


def main(args):
    release_ref = ref(args.safe)
    safe = read(args.safe)
    assert 'SAFE' in safe['status'] and safe['activeOwned'] == safe['queuedOwned'] == []
    assert all(not Path('/proc', str(pid)).exists() for pid in safe['terminalPidsAbsent'])
    locks = ('/tmp/dotcc-cpython-shared-build.lock', '/tmp/dotcc-cpython-native-build.lock', str(CPY / 'build/campaign/run.lock'))
    for lock in locks:
        with open(lock, 'a') as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            fcntl.flock(stream, fcntl.LOCK_UN)
    write(WORK / 'direct-a32-authority.json', dict(directSafe=release_ref, ownFreeChecks=list(locks),
        utc=datetime.datetime.now(datetime.timezone.utc).isoformat(), scope='Bounded native candidate controls only'))
    started = time.monotonic()
    base = WORK / 'native-feature'
    base.mkdir()
    build = base / 'build'
    build.mkdir()
    b37 = read(CPY / 'artifacts/owner-b-r37-20261018/native-build.json')
    source = CPY / 'build/owner-b-r37-20261018/native-full/source'
    pin = read(CPY / 'config/source.json')
    archive = CPY / 'ref' / pin['archive']
    assert ref(archive)['sha256'] == pin['sha256']
    # Independently compare every inherited native input against immutable
    # archive bytes and the current read-only tree; retain no giant backup.
    sources = b37['sourceInputs']
    wanted = {row['path']: row for row in sources}
    checked = set()
    with tarfile.open(archive) as tar:
        for member in tar:
            relative = member.name.partition('/')[2]
            if relative not in wanted or not member.isfile():
                continue
            digest = hashlib.sha256(tar.extractfile(member).read()).hexdigest()
            assert digest == wanted[relative]['sha256'] == ref(source / relative)['sha256'], relative
            checked.add(relative)
    assert checked == wanted.keys(), wanted.keys() - checked
    source_seal = dict(archive=ref(archive), sourceRoot=str(source), inputCount=len(sources), sourceInputs=sources,
        unchangedArchiveMatches=True, oldNativeBuildEvidence=ref(CPY / 'artifacts/owner-b-r37-20261018/native-build.json'))
    write(base / 'source-pin.json', source_seal)
    records = []
    environment = dict(os.environ, LC_ALL='C.UTF-8', LANG='C.UTF-8', CC='/usr/bin/cc',
        CPPFLAGS='-D_Py_FORCE_UTF8_LOCALE=1', PYTHONDONTWRITEBYTECODE='1', PYTHONUTF8='1',
        PYTHONHOME='/absent-poison-r42', PYTHONPATH='poison-r42', DOTCC_CAMPAIGN_COMMAND_LOCKS='')
    # Do not inherit build overrides unrelated to the selected feature.
    for key in ('CFLAGS', 'CXXFLAGS', 'LDFLAGS', 'CONFIG_SITE', 'LD_PRELOAD'):
        environment.pop(key, None)

    def run(argv, label, *, cwd=build, timeout=180, check=True):
        if time.monotonic() - started > 300:
            raise RuntimeError('Initial bounded group reached safe boundary; active command completed normally')
        record = dict(declared_environment={key: environment.get(key) for key in
            ('LC_ALL', 'LANG', 'CC', 'CPPFLAGS', 'CFLAGS', 'LDFLAGS', 'CONFIG_SITE', 'PYTHONHOME', 'PYTHONPATH', 'PYTHONUTF8')})
        records.append(record)
        print('START ' + label, flush=True)
        try:
            execute(['flock', '-n', '--close', locks[1], *argv], cwd=cwd, env=environment,
                    output=base / (label + '.log'), label=label, timeout=timeout, record=record, check=check)
        finally:
            write(base / 'commands.json', records)
        print(json.dumps(dict(label=label, exit_code=record.get('exit_code'), seconds=record.get('seconds'))), flush=True)
        return (base / (label + '.log')).read_text()

    configure = b37['configure']
    run(['cc', '--version'], 'native-compiler', timeout=10)
    run(['bash', source / 'configure', *configure], 'native-configure')
    run(['make', '-j2', 'all', 'libpython3.13.a'], 'native-build', timeout=240)
    library = base / 'consumer/lib/python3.13'
    shutil.copytree(source / 'Lib', library, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    extensions = build / (build / 'pybuilddir.txt').read_text().strip()
    shutil.copytree(extensions, library / 'lib-dynload', ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    native_bin = base / 'consumer/bin'
    native_bin.mkdir(parents=True)
    shutil.copyfile(build / 'python', native_bin / 'python3.13')
    (native_bin / 'python3.13').chmod(0o755)
    for path in build.glob('_sysconfigdata_*.py'):
        shutil.copyfile(path, library / path.name)
    for path in extensions.glob('_sysconfigdata_*.py'):
        shutil.copyfile(path, library / path.name)
    app = base / 'app'
    app.mkdir()
    shutil.copyfile(HERE / 'locale_probe.py', app / 'locale_probe.py')
    shutil.copyfile(CPY / 'tests/Upstream/run_inventory.py', app / 'run_inventory.py')
    prior_path = CPY / 'artifacts/owner-b-r41-20261018/cells/upstream-tests-static-raw-jit-upstream-initial/full-required.json'
    prior = read(prior_path)
    details = {r['case']: r for group in prior['discovery'] for r in group['case_details']}
    ids = ['test.test_codecs.LocaleCodecTest.test_' + name for name in
           ('decode_strict', 'decode_surrogateescape', 'encode_strict', 'encode_surrogateescape', 'decode_surrogatepass', 'encode_surrogatepass')]
    write(app / 'locale-probe-inputs.json', dict(cases=[dict(case=case, detail=details[case],
        sourceSha256=ref(details[case]['upstream_path'])['sha256']) for case in ids]))
    control = base / 'native-configured-control'
    run(['cc', '-std=c11', '-O2', environment['CPPFLAGS'], '-Wl,--export-dynamic', '-I', build,
         '-I', source / 'Include', '-I', CPY / 'src/Host', HERE / 'native_control.c',
         CPY / 'src/Host/dotcc_cpython_host.c', build / 'libpython3.13.a',
         '-Wl,-u,_PyInterpreterConfig_InitFromDict', '-lm', '-ldl', '-lpthread', '-o', control], 'native-control-build')
    reports = []
    # Primary matching configured Host, then the existing complementary mode.
    # The boundary probe changes C/C.UTF8 internally without changing mode.
    for mode in (0, 1):
        report = base / ('native-locale-mode' + str(mode) + '.json')
        run([control, library, app, report, str(mode), 'C.UTF-8'], 'native-locale-mode' + str(mode), cwd=base)
        observed = read(report)
        assert observed['executed'] == 6 and all(row['status'] == 'passed' for row in observed['cases'])
        assert observed['before']['utf8_mode'] == observed['after']['utf8_mode'] == mode
        reports.append(ref(report))
    # Preserve all 104 original marker outcomes under the changed feature.
    marker_report = base / 'native-104-mode0.json'
    run([native_bin / 'python3.13', '-I', '-X', 'utf8=0', CPY / 'tests/StartupQualificationR37/native_controls.py',
        '--root', CPY, '--source', source, '--phase', 'markers',
        '--markers', CPY / 'artifacts/owner-b-r36-20261018/required-marker-source-inventory.json',
        '--report', marker_report, '--work', base / 'marker-work', '--expected-stdlib', library],
        'native-104-mode0', cwd=base)
    outputs = [control, build / 'python', build / 'libpython3.13.a', build / 'pyconfig.h', build / 'Makefile',
               build / 'Modules/config.c', build / 'Modules/Setup.stdlib', build / 'config.log']
    input_paths = [HERE / 'native_control.c', HERE / 'locale_probe.py', CPY / 'src/Host/dotcc_cpython_host.c',
                   CPY / 'src/Host/dotcc_cpython_host.h', CPY / 'tests/Upstream/run_inventory.py',
                   CPY / 'tests/Upstream/inventory.frozen.json', CPY / 'config/source.json']
    assert all(ref(source / row['path'])['sha256'] == row['sha256'] for row in sources)
    write(base / 'result.json', dict(status='New same-feature native controls PASS; no translated product parity inferred',
        seconds=time.monotonic()-started, featureCPPFLAGS=environment['CPPFLAGS'], configure=configure,
        sourcePin=ref(base / 'source-pin.json'), inputs=[ref(p) for p in input_paths], outputs=[ref(p) for p in outputs],
        configuredHostReports=reports, native104=ref(marker_report), native104RawCounts=read(marker_report)['counts'],
        commands=ref(base / 'commands.json'), lowerLibcUnchanged=True,
        oldTargetRED=ref(CPY / 'artifacts/owner-b-r41-20261018/cells/upstream-tests-static-raw-jit-locale-reduction-initial/reduction-result.json')))
    print(json.dumps(dict(result=ref(base / 'result.json'), seconds=time.monotonic()-started)), flush=True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--safe', type=Path, required=True)
    main(parser.parse_args())
