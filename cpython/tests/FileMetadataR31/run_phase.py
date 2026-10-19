"""Focused metadata controls using the existing campaign command boundary."""
from pathlib import Path
import hashlib
import json
import os
import shutil
import sys

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'Scripts'))
from campaigns.process import execute

HERE = Path(__file__).resolve().parent
WORK = ROOT / 'cpython/build/owner-b-r31-20261018'
ART = ROOT / 'cpython/artifacts/owner-b-r31-20261018'
WORK.mkdir(parents=True, exist_ok=True)
ART.mkdir(parents=True, exist_ok=True)
phase = sys.argv[1]
if (ART / (phase + '-commands.json')).exists():
    raise SystemExit('Phase evidence already exists; preserve it and select fresh private roots for another run.')
commands = []
environment = dict(os.environ, DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0',
                   MSBUILDDISABLENODEREUSE='1', UseSharedCompilation='false',
                   DOTCC_CAMPAIGN_COMMAND_LOCKS='production', LD_LIBRARY_PATH=str(WORK))
def run(argv, label, timeout=180, check=True):
    record = {'declared_environment': {k: environment[k] for k in
              ('DOTNET_PROCESSOR_COUNT', 'DOTNET_CLI_USE_MSBUILD_SERVER', 'MSBUILDDISABLENODEREUSE',
               'UseSharedCompilation', 'DOTCC_CAMPAIGN_COMMAND_LOCKS', 'LD_LIBRARY_PATH')}}
    commands.append(record)
    try:
        execute(list(map(str, argv)), label=label, cwd=Path('/tmp'), output=ART / (label + '.log'),
                timeout=timeout, env=environment, record=record, check=check)
    finally:
        (ART / (phase + '-commands.json')).write_text(json.dumps(commands, indent=2) + '\n')
    print(json.dumps({k: record.get(k) for k in ('label', 'status', 'exit_code', 'seconds')}), flush=True)
if phase == 'baseline':
    if not (ART / 'PosixLib.before.cs').exists():
        shutil.copyfile(ROOT / 'DotCC.Libc/PosixLib.cs', ART / 'PosixLib.before.cs')
    sources = [ROOT / 'DotCC.Libc/PosixLib.cs', ROOT / 'DotCC.Lib/include/sys/stat.h',
               ROOT / 'cpython/tests/RawReplayR30/pyc_permission_r30.py']
    (ART / 'original-inputs.json').write_text(json.dumps([
        {'path': str(p), 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in sources], indent=2) + '\n')
    lock = ['flock', '-n', '--close', '/tmp/dotcc-cpython-native-build.lock']
    run(lock + ['cc', '-std=c11', '-fPIC', '-nostdinc', '-I', ROOT / 'DotCC.Lib/include',
                '-c', HERE / 'target_layout.c', '-o', WORK / 'target_layout.o'], 'target-header-layout-build')
    run(lock + ['cc', '-std=c11', '-fPIC', '-shared', HERE / 'native.c', WORK / 'target_layout.o',
                '-o', WORK / 'libmetadata_oracle.so'], 'native-oracle-build')
    run(lock + ['cc', '-std=c11', '-DSTANDALONE', HERE / 'native.c', '-o', WORK / 'native'], 'native-control-build')
    run([WORK / 'native', WORK / 'native-file'], 'native-control')
if phase in ('baseline', 'fixed', 'final'):
    out = WORK / phase
    run(['dotnet', 'build', HERE / 'FileMetadataR31.csproj', '-c', 'Release', '--disable-build-servers',
         '-m:1', '-p:BuildInParallel=false', '-p:UseSharedCompilation=false', '--artifacts-path', out / 'products',
         '-o', out / 'app'], phase + '-build')
    run(['dotnet', out / 'app/FileMetadataR31.dll', out / 'files'], phase + '-jit', check=False)
if phase == 'aot':
    out = WORK / phase
    run(['dotnet', 'publish', HERE / 'FileMetadataR31.csproj', '-c', 'Release', '-r', 'linux-x64',
         '-p:PublishAot=true', '--disable-build-servers', '-m:1', '-p:BuildInParallel=false',
         '-p:UseSharedCompilation=false', '--artifacts-path', out / 'products', '-o', out / 'app'], 'fixed-aot-build', timeout=300)
    run([out / 'app/FileMetadataR31', out / 'files'], 'fixed-aot', check=False)
if phase == 'affected':
    run(['dotnet', 'test', HERE / 'Unit/Unit.csproj', '-c', 'Release', '--disable-build-servers',
         '-m:1', '-p:BuildInParallel=false', '-p:UseSharedCompilation=false', '--artifacts-path', WORK / 'unit-products',
         '--logger', 'trx;LogFileName=metadata.trx', '--results-directory', ART / 'unit-results'], 'affected-unit-tests')
    actual = ROOT / 'cpython/build/owner-b-r30-20261018/discovery-app'
    run(['dotnet', 'build', HERE / 'ActualLayout/ActualLayout.csproj', '-c', 'Release', '--disable-build-servers',
         '-m:1', '-p:BuildInParallel=false', '-p:UseSharedCompilation=false', '-p:ActualInputDirectory=' + str(actual),
         '--artifacts-path', WORK / 'actual-layout-products', '-o', WORK / 'actual-layout-app'], 'existing-actual-layout-build')
    run(['dotnet', WORK / 'actual-layout-app/ActualLayout.dll'], 'existing-actual-layout')
