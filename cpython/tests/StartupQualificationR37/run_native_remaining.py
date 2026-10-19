"""Bounded native witnesses and actual Host controls; use only in assigned lane."""
import hashlib
import json
import os
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.process import execute

ART = ROOT / 'artifacts/owner-b-r37-20261018'
BASE = ROOT / 'build/owner-b-r37-20261018/native-full'
HERE = Path(__file__).resolve().parent
records = []
suffix = '-' + sys.argv[1] if len(sys.argv) > 1 else ''
receipt = ART / ('native-remaining-commands' + suffix + '.json')
if receipt.exists():
    raise SystemExit('Preserve completed/failed commands; no blind rerun')
environment = {**os.environ, 'LC_ALL': 'C.UTF-8', 'LANG': 'C.UTF-8',
               'PYTHONUTF8': '1', 'PYTHONDONTWRITEBYTECODE': '1', 'PYTHONPATH': 'poison-r37',
               'PYTHONHOME': '/absent-poison-r37'}


def run(argv, label, *, cwd=BASE, timeout=90):
    label += suffix
    record = {'declared_environment': {key: environment[key] for key in
        ('LC_ALL', 'LANG', 'PYTHONUTF8', 'PYTHONDONTWRITEBYTECODE', 'PYTHONPATH', 'PYTHONHOME')}}
    records.append(record)
    try:
        execute(argv, label=label, cwd=cwd, output=ART / (label + '.log'),
                env=environment, timeout=timeout, record=record)
    finally:
        receipt.write_text(json.dumps(records, indent=2) + '\n')


for mode in (0, 1):
    run([BASE / 'consumer/bin/python3.13', '-I', '-X', 'utf8=' + str(mode),
         HERE / 'native_controls.py', '--root', ROOT, '--source', BASE / 'source',
         '--phase', 'markers', '--markers', ROOT / 'artifacts/owner-b-r36-20261018/required-marker-source-inventory.json',
         '--report', ART / ('native-104-utf8-' + str(mode) + suffix + '.json'),
         '--work', BASE / ('native-104-utf8-' + str(mode) + suffix),
         '--expected-stdlib', BASE / 'consumer/lib/python3.13'], 'native-104-utf8-' + str(mode))

build = BASE / 'build'
run(['cc', '-std=c11', '-O2', '-I', build, '-I', BASE / 'source/Include', '-I', ROOT / 'src/Host',
     HERE / 'startup_control.c', ROOT / 'src/Host/dotcc_cpython_host.c', build / 'libpython3.13.a',
     '-lm', '-ldl', '-lpthread', '-o', BASE / 'startup-control'], 'native-host-startup-build', timeout=120)
run([BASE / 'startup-control', BASE / 'consumer/lib/python3.13', BASE / 'startup-app'], 'native-host-startup')
inputs = [HERE / 'startup_control.c', ROOT / 'src/Host/dotcc_cpython_host.c', ROOT / 'src/Host/dotcc_cpython_host.h',
          build / 'libpython3.13.a', build / 'pyconfig.h', BASE / 'startup-control']
(ART / 'native-host-startup-inputs.json').write_text(json.dumps([
    {'path': str(p), 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in inputs], indent=2) + '\n')
print(json.dumps({'commands': len(records), 'seconds': sum(r['seconds'] for r in records)}))
