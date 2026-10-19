"""SAME-C native/immutable-V21 and explicit-owner symlink control, no repair."""
import hashlib
import json
import os
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.process import execute

ART = ROOT / 'artifacts/owner-b-r37-20261018'
WORK = ROOT / 'build/owner-b-r37-20261018/symlink'
HERE = Path(__file__).resolve().parent
COMPILER = ROOT / 'build/owner-a-r30-20261018/v21/tools/compiler/dotcc.dll'
if WORK.exists():
    raise SystemExit('Preserve existing reduction; no blind rerun')
WORK.mkdir()
records = []
environment = {**os.environ, 'DOTNET_PROCESSOR_COUNT': '2', 'DOTNET_CLI_USE_MSBUILD_SERVER': '0',
    'MSBUILDDISABLENODEREUSE': '1', 'UseSharedCompilation': 'false',
    'DOTCC_CAMPAIGN_COMMAND_LOCKS': 'production'}


def run(argv, label, *, cwd=WORK, check=True):
    record = {}
    records.append(record)
    try:
        execute(argv, label=label, cwd=cwd, output=ART / (label + '.log'), timeout=15,
                env=environment, record=record, check=check)
    finally:
        (ART / 'symlink-commands.json').write_text(json.dumps(records, indent=2) + '\n')
    return record


fixture = HERE / 'symlink_control.c'
run(['cc', '-std=c17', '-D_POSIX_C_SOURCE=200809L', '-O2', '-Wall', '-Wextra', fixture,
     '-o', WORK / 'native'], 'symlink-native-build')
native_work = WORK / 'native-files'
native_work.mkdir()
run([WORK / 'native'], 'symlink-native', cwd=native_work)
emitted = WORK / 'v21'
run(['dotnet', COMPILER, '--emit=csproj', '--runtime=c', '-std=c17', '-D_POSIX_C_SOURCE=200809L',
     fixture, '-o', emitted], 'symlink-v21-emit')
common = ['-c', 'Release', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false',
          '-p:UseLocalLalrCc=false', '-p:RestoreSources=' + str(Path.home() / '.nuget/packages')]
run(['dotnet', 'build', emitted, *common], 'symlink-v21-build')
jit_work = WORK / 'v21-files'
jit_work.mkdir()
run(['dotnet', emitted / 'bin/Release/net10.0/v21.dll'], 'symlink-v21-jit', cwd=jit_work, check=False)
owner_out = WORK / 'owner-app'
run(['dotnet', 'build', HERE / 'SymlinkOwner/SymlinkOwner.csproj', *common,
     '-p:NativeSymlinkSource=' + str(emitted / 'DotCcProgram.cs'),
     '--artifacts-path', WORK / 'owner-products', '-o', owner_out], 'symlink-v21-owner-build')
owner_work, other_work = WORK / 'owner-files', WORK / 'other-owner-files'
owner_work.mkdir()
other_work.mkdir()
run(['dotnet', owner_out / 'SymlinkOwner.dll', owner_work, other_work],
    'symlink-v21-owner-jit', cwd=WORK, check=False)
paths = [fixture, COMPILER, emitted / 'DotCcProgram.cs', emitted / 'bin/Release/net10.0/v21.dll',
         emitted / 'bin/Release/net10.0/v21.pdb', owner_out / 'SymlinkOwner.dll', owner_out / 'SymlinkOwner.pdb']
(ART / 'symlink-inputs.json').write_text(json.dumps([
    {'path': str(p), 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths], indent=2) + '\n')
print(json.dumps({'commands': len(records), 'seconds': sum(r['seconds'] for r in records),
    'outcomes': {r['label']: r['exit_code'] for r in records}, 'flagEnabled': False}))
