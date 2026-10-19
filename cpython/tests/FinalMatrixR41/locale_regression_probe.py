"""Exact four unchanged cases; StringIO only prevents diagnostic encoding loss."""
import hashlib
import importlib
import io
import json
import locale
from pathlib import Path
import sys
import unittest

import run_inventory as runner


def run_probe(report):
    path = Path(report)
    inputs = json.loads((Path(__file__).parent/'locale-probe-inputs.json').read_text())
    module = importlib.import_module('test.test_codecs')
    loaded = list(runner.flatten(unittest.TestLoader().loadTestsFromModule(module)))
    expected = {row['case']:row for row in inputs['cases']}
    tests = []
    witnesses = []
    for test in loaded:
        if test.id() not in expected:
            continue
        detail = runner.case_detail(test)
        source = expected[test.id()]
        assert all(detail[key] == source['detail'][key] for key in ('source','method','declaring_class','expected_failure'))
        filename = Path(detail['upstream_path'])
        assert hashlib.sha256(filename.read_bytes()).hexdigest() == source['sourceSha256']
        witnesses.append(dict(case=test.id(),sourceSha256=source['sourceSha256'],methodAndDecoratorUnchanged=True))
        tests.append(test)
    assert len(tests) == len(expected) == 4
    def snapshot():
        return dict(locale=locale.setlocale(locale.LC_CTYPE),getencoding=locale.getencoding(),
            filesystemencoding=sys.getfilesystemencoding(),utf8_mode=sys.flags.utf8_mode,
            isolated=sys.flags.isolated,ignore_environment=sys.flags.ignore_environment)
    before = snapshot()
    stream = io.StringIO()
    with path.with_suffix('.journal.jsonl').open('w') as journal:
        def factory(*args,**kwargs):
            result = runner.CaseResult(*args,**kwargs)
            result.journal = journal
            return result
        result = unittest.TextTestRunner(verbosity=2,resultclass=factory,stream=stream).run(unittest.TestSuite(tests))
    # JSON uses escaped Unicode; no frozen test/runner/source is rewritten.
    raw = []
    for text in ('ascii','BMP:\u00e9\u20ac','nonBMP:\U0010ffff','UCS:\u00e9\u20ac\U0010ffff','surrogates:\udc80\udcff'):
        for errors in ('strict','surrogateescape'):
            row = dict(operation='encode',textCodepoints=[ord(c) for c in text],errors=errors)
            try:
                value = module._testinternalcapi.EncodeLocaleEx(text,0,errors)
                row.update(status='returned',bytes=list(value),length=len(value))
            except Exception as error:
                row.update(status='raised',type=type(error).__name__,message=str(error))
            raw.append(row)
    for value in (b'ascii',b'BMP:\xc3\xa9\xe2\x82\xac',b'nonBMP:\xf4\x8f\xbf\xbf',
                  b'UCS:\xc3\xa9\xe2\x82\xac\xf4\x8f\xbf\xbf',b'invalid:\xed\xa0\x80',b'truncated:\xe2\x82'):
        for errors in ('strict','surrogateescape'):
            row = dict(operation='decode',bytes=list(value),length=len(value),errors=errors)
            try:
                text = module._testinternalcapi.DecodeLocaleEx(value,0,errors)
                row.update(status='returned',textCodepoints=[ord(c) for c in text],length=len(text))
            except Exception as error:
                row.update(status='raised',type=type(error).__name__,message=str(error))
            raw.append(row)
    observed = dict(scope='Exact unchanged four-case native/actual reduction; no full qualification',
        before=before,after=snapshot(),sourceWitnesses=witnesses,cases=result.cases,executed=result.testsRun,
        diagnosticStringIO=stream.getvalue(),rawConversions=raw)
    path.write_text(json.dumps(observed,ensure_ascii=True,indent=2)+'\n')
    return 0 if all(row['status']=='passed' for row in result.cases) else 1
