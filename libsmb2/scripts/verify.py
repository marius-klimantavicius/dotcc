#!/usr/bin/env python3
"""Fresh outside-directory generation, implemented qualification, compiler suites."""
import argparse
import json
import tempfile
from common import ROOT, REPO, run

argparse.ArgumentParser(description=__doc__).parse_args()
logs = ROOT / 'artifacts/verification'
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, scope='regeneration, implemented qualification and repository test suites', plan_complete=False)
try:
    with tempfile.TemporaryDirectory(prefix='libsmb2-verify-') as outside:
        run([ROOT / 'scripts/translate.sh'], logs / 'translate.log', receipt, cwd=outside, timeout=3600)
    run([ROOT / 'scripts/test.sh'], logs / 'qualification.log', receipt, timeout=7200)
    for name in ('DotCC.Tests', 'DotCC.FunctionalTests', 'DotCC.PostProcess.Tests'):
        run(['dotnet', 'test', REPO / name / (name + '.csproj'), '-c', 'Release', '--nologo',
             '--logger', 'trx;LogFileName=' + name + '.trx', '--results-directory', logs],
            logs / (name + '.log'), receipt, timeout=3600)
    receipt['passed'] = True
    print('Verification suites passed. Full plan acceptance and other campaign/platform coverage remain separately tracked.')
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
