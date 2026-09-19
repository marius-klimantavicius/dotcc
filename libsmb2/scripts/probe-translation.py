#!/usr/bin/env python3
"""Emit each configured libsmb2 unit independently and retain a complete census.

Run probe-parse.py first to verify inputs and produce the configuration request.
This diagnostic does not link, build, postprocess, or promote product output.
"""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

from common import SOURCE_SPEC, fetch

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
OUT = ROOT / 'artifacts/translation-probe'
REQUEST = ROOT / 'artifacts/parse-probe/request.json'
OUT.mkdir(parents=True, exist_ok=True)
if not REQUEST.exists():
    raise SystemExit('Run libsmb2/scripts/probe-parse.py first to prepare verified inputs')
source = fetch()
request = json.loads(REQUEST.read_text())
for unit in request['Units']:
    if not Path(unit).resolve().is_relative_to(source.resolve()):
        raise SystemExit('Request contains a unit outside the verified source snapshot: ' + unit)
commands = [
    ['dotnet', 'build', str(ROOT / 'tests/TranslationProbe/TranslationProbe.csproj'),
     '-c', 'Release', '--nologo'],
    ['dotnet', str(ROOT / 'tests/TranslationProbe/bin/Release/net10.0/TranslationProbe.dll'),
     str(REQUEST), str(OUT / 'objects')],
]
for index, command in enumerate(commands):
    with (OUT / ('build.log' if index == 0 else 'run.log')).open('w') as log:
        result = subprocess.run(command, cwd=REPO, stdout=log, stderr=subprocess.STDOUT,
                                timeout=1200)
    if index == 0 and result.returncode:
        raise SystemExit(result.returncode)
assembly_dir = ROOT / 'tests/TranslationProbe/bin/Release/net10.0'
manifest = dict(
    repository_commit=subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO,
                                             text=True).strip(),
    sdk=subprocess.check_output(['dotnet', '--version'], text=True).strip(),
    source=SOURCE_SPEC,
    request=request,
    commands=commands,
    tool_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                 for p in assembly_dir.glob('*.dll')},
    exit_code=result.returncode,
)
(OUT / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print((OUT / 'run.log').read_text(), end='')
print(f'Evidence: {OUT}')
sys.exit(result.returncode)
