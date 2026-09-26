#!/usr/bin/env python3
"""Run the existing Zig oracle with the repository CI's pinned Linux x64 tool.

The downloaded oracle stays under ignored blink/ref; it never replaces dotcc.
Already-built test libraries must match the frozen campaign compiler exactly.
"""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['scripts/test-zig-regression.py']

import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tarfile
import tempfile
import urllib.request
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
BLINK = ROOT / 'blink'
VERSION = '0.16.0'
ARCHIVE = 'zig-x86_64-linux-' + VERSION + '.tar.xz'
DIGEST = _CAMPAIGN_INPUTS['DIGEST']
URL = 'https://ziglang.org/download/' + VERSION + '/' + ARCHIVE
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--offline', action='store_true')
args = parser.parse_args()
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
base = BLINK / 'artifacts/zig-regression'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
receipt = dict(passed=False, version=VERSION, archive_url=URL, archive_sha256=DIGEST,
    runner_sha256=sha(Path(__file__)), ci_workflow_sha256=sha(ROOT / '.github/workflows/dotnet.yml'))
def save(): (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
save()
print(out / 'receipt.json', flush=True)
try:
    archive = BLINK / 'ref' / ARCHIVE
    if not archive.exists():
        if args.offline: raise RuntimeError('Missing pinned Zig archive in offline mode')
        with urllib.request.urlopen(URL, timeout=60) as response:
            data = response.read()
        if hashlib.sha256(data).hexdigest() != DIGEST: raise RuntimeError('Downloaded Zig checksum mismatch')
        archive.write_bytes(data)
    if sha(archive) != DIGEST: raise RuntimeError('Cached Zig checksum mismatch')
    # Extract per attempt so an old unpacked executable cannot satisfy the gate.
    unpack = BLINK / 'build/zig-regression' / out.name
    unpack.mkdir(parents=True)
    with tarfile.open(archive) as stream: stream.extractall(unpack, filter='data')
    tool = unpack / ARCHIVE.removesuffix('.tar.xz') / 'zig'
    if subprocess.check_output([tool, 'version'], text=True).strip() != VERSION:
        raise RuntimeError('Zig version differs from pin')
    receipt['tool_sha256'] = sha(tool)
    project = ROOT / 'DotCC.FunctionalTests/bin/Release/net10.0'
    inputs = {str(p): sha(p) for p in project.iterdir() if p.is_file() and p.suffix in ('.dll', '.json')}
    compiler_inputs = ROOT / 'DotCC/bin/Release/net10.0'
    if sha(project / 'DotCC.Lib.dll') != sha(compiler_inputs / 'DotCC.Lib.dll'):
        raise RuntimeError('Functional test compiler differs from campaign CLI')
    receipt['shared_compiler_inputs'] = {}
    for tool_input in compiler_inputs.glob('*.dll'):
        test_input = project / tool_input.name
        if test_input.exists():
            if sha(test_input) != sha(tool_input):
                raise RuntimeError('Compiler dependency differs: ' + tool_input.name)
            receipt['shared_compiler_inputs'][tool_input.name] = sha(tool_input)
    receipt['inputs'] = inputs
    tmp = out / 'tmp'; tmp.mkdir()
    env = dict(os.environ, PATH=str(tool.parent) + os.pathsep + os.environ['PATH'], TMPDIR=str(tmp),
        DOTCC_RUN_ZIG_ORACLE='1', ZIG_LOCAL_CACHE_DIR=str(out / 'local-cache'),
        ZIG_GLOBAL_CACHE_DIR=str(out / 'global-cache'), DOTCC_ZIG_LIB_DIR=str(tool.parent / 'lib'))
    receipt['zig_std_source'] = str(tool.parent / 'lib')
    command = ['dotnet', 'test', ROOT / 'DotCC.FunctionalTests/DotCC.FunctionalTests.csproj',
        '-c', 'Release', '--no-build', '--filter', 'FullyQualifiedName~ZigOracleTests',
        '--logger', 'trx;LogFileName=results.trx', '--results-directory', out, '--blame-hang-timeout', '300s']
    receipt['command'] = list(map(str, command)); save()
    with (out / 'tests.log').open('w') as log:
        result = subprocess.run(receipt['command'], cwd=ROOT, env=env,
            stdout=log, stderr=subprocess.STDOUT, timeout=3600)
    receipt['exit_code'] = result.returncode
    trx = ET.parse(out / 'results.trx').getroot()
    receipt['counts'] = trx.find('.//{*}Counters').attrib
    receipt['cases'] = [dict(name=x.attrib.get('testName'), outcome=x.attrib.get('outcome'))
        for x in trx.findall('.//{*}UnitTestResult')]
    receipt['inputs_after'] = {n: sha(Path(n)) for n in inputs}
    if receipt['inputs_after'] != inputs or sha(tool) != receipt['tool_sha256']:
        raise RuntimeError('Executed inputs changed')
    receipt['passed'] = result.returncode == 0 and bool(receipt['cases']) and all(
        x['outcome'] == 'Passed' for x in receipt['cases'])
except Exception as error:
    receipt['error'] = str(error)
    raise
finally:
    save()
print(receipt.get('counts'), flush=True)
raise SystemExit(0 if receipt['passed'] else 1)
