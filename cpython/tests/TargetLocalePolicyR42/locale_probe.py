"""Run unchanged upstream cases and independent target-contract boundary checks.

Runs inside the real same-pin interpreter. Native helpers are controls only.
"""
import hashlib
import importlib
import io
import json
import locale
from pathlib import Path
import sys
import unittest

import _testinternalcapi as internal
import _testlimitedcapi as limited
import run_inventory as runner


def run_probe(report):
    report = Path(report)
    inputs = json.loads((Path(__file__).parent / 'locale-probe-inputs.json').read_text())
    expected = {row['case']: row for row in inputs['cases']}
    module = importlib.import_module('test.test_codecs')
    tests = [test for test in runner.flatten(unittest.TestLoader().loadTestsFromModule(module)) if test.id() in expected]
    assert len(tests) == len(expected) == 6
    witnesses = []
    for test in tests:
        detail = runner.case_detail(test)
        original = expected[test.id()]
        assert all(detail[key] == original['detail'][key] for key in ('source', 'method', 'declaring_class', 'expected_failure'))
        source = Path(detail['upstream_path'])
        assert hashlib.sha256(source.read_bytes()).hexdigest() == original['sourceSha256']
        witnesses.append(dict(case=test.id(), sourceSha256=original['sourceSha256'], methodAndDecoratorUnchanged=True))

    def snapshot():
        return dict(locale=locale.setlocale(locale.LC_CTYPE), codeset=locale.nl_langinfo(locale.CODESET),
            getencoding=locale.getencoding(), preferred=locale.getpreferredencoding(False),
            filesystemencoding=sys.getfilesystemencoding(), filesystemerrors=sys.getfilesystemencodeerrors(),
            utf8_mode=sys.flags.utf8_mode, isolated=sys.flags.isolated, ignore_environment=sys.flags.ignore_environment,
            stdio=[(stream.encoding, stream.errors) for stream in (sys.stdin, sys.stdout, sys.stderr)])

    before = snapshot()
    assert before['getencoding'] == before['filesystemencoding'] == 'utf-8'
    assert before['filesystemerrors'] == 'surrogateescape'
    stream = io.StringIO()
    with report.with_suffix('.journal.jsonl').open('w') as journal:
        def factory(*args, **kwargs):
            result = runner.CaseResult(*args, **kwargs)
            result.journal = journal
            return result
        result = unittest.TextTestRunner(verbosity=2, resultclass=factory, stream=stream).run(unittest.TestSuite(tests))
    checks = []

    def raises(call, kind):
        try:
            call()
        except kind as error:
            return type(error).__name__
        raise AssertionError('Expected ' + kind.__name__)

    texts = ('ascii', '\u00e9\u20ac', '\U00010000\U0010ffff', '\udc80\udcff', '\ud800', '\udfff')
    values = (b'ascii', '\u00e9\u20ac\U0010ffff'.encode(), b'\x80\xff', b'\xed\xa0\x80',
              b'\xc0\xaf', b'\xf4\x90\x80\x80', b'\xe2\x82')
    initial_locale = locale.setlocale(locale.LC_CTYPE)
    for name in ('C', 'C.UTF-8'):
        locale.setlocale(locale.LC_CTYPE, name)
        observed = snapshot()
        assert observed['getencoding'] == observed['preferred'] == observed['filesystemencoding'] == 'utf-8'
        assert observed['utf8_mode'] == before['utf8_mode']
        assert observed['codeset'] == ('ANSI_X3.4-1968' if name == 'C' else 'UTF-8')
        for current in (0, 1):
            for errors in ('strict', 'surrogateescape', 'surrogatepass'):
                for text in texts:
                    try:
                        want = text.encode('utf-8', errors)
                    except UnicodeEncodeError:
                        outcome = raises(lambda: internal.EncodeLocaleEx(text, current, errors), RuntimeError)
                    else:
                        actual = internal.EncodeLocaleEx(text, current, errors)
                        assert actual == want, (name, current, errors, ascii(text), actual, want)
                        outcome = list(actual)
                    checks.append(dict(locale=name, current_locale=current, errors=errors, operation='encode',
                                       codepoints=list(map(ord, text)), outcome=outcome))
                for value in values:
                    try:
                        want = value.decode('utf-8', errors)
                    except UnicodeDecodeError:
                        outcome = raises(lambda: internal.DecodeLocaleEx(value, current, errors), RuntimeError)
                    else:
                        actual = internal.DecodeLocaleEx(value, current, errors)
                        assert actual == want, (name, current, errors, list(value), ascii(actual), ascii(want))
                        outcome = list(map(ord, actual))
                    checks.append(dict(locale=name, current_locale=current, errors=errors, operation='decode',
                                       bytes=list(value), outcome=outcome))
            raises(lambda: internal.EncodeLocaleEx('', current, 'backslashreplace'), ValueError)
            raises(lambda: internal.DecodeLocaleEx(b'', current, 'backslashreplace'), ValueError)
        # Actual public locale and length-aware filesystem APIs, including NUL.
        text = '\u00e9\u20ac\U0010ffff'
        encoded = text.encode()
        assert limited.unicode_encodelocale(text, 'strict') == encoded
        assert limited.unicode_decodelocaleandsize(encoded, 'strict') == text
        assert limited.unicode_decodelocale(encoded, 'strict') == text
        raises(lambda: limited.unicode_encodelocale('a\0b', 'strict'), ValueError)
        raises(lambda: limited.unicode_decodelocaleandsize(b'a\0b', 'strict'), ValueError)
        assert limited.unicode_decodefsdefaultandsize(encoded + b'\0suffix') == text + '\0suffix'
        assert limited.unicode_encodefsdefault(text + '\0suffix') == encoded + b'\0suffix'
        assert limited.unicode_decodefsdefault(b'a\0suffix') == 'a'
        assert limited.unicode_decodelocale(b'a\0suffix', 'strict') == 'a'
        assert limited.unicode_encodefsdefault('\udc80\udcff') == b'\x80\xff'
        assert limited.unicode_decodefsdefaultandsize(b'\x80\xff') == '\udc80\udcff'
        raises(lambda: limited.unicode_encodefsdefault('\ud800'), UnicodeEncodeError)
        raises(lambda: limited.unicode_decodelocaleandsize(b'\xed\xa0\x80', 'strict'), UnicodeDecodeError)
        assert limited.unicode_decodelocaleandsize(b'\xed\xa0\x80', 'surrogateescape') == '\udced\udca0\udc80'
        checks.append(dict(locale=name, publicLocaleAndFS=True, embeddedNul='API-specific reject/preserve/truncate checked',
                           lowerCodeset=observed['codeset']))
    locale.setlocale(locale.LC_CTYPE, initial_locale)
    report.write_text(json.dumps(dict(scope='New same-feature native CONTROL; no translated product qualification',
        before=before, after=snapshot(), sourceWitnesses=witnesses, executed=result.testsRun, cases=result.cases,
        diagnosticStringIO=stream.getvalue(), boundaryChecks=checks), ensure_ascii=True, indent=2) + '\n')
    return 0 if all(row['status'] == 'passed' for row in result.cases) else 1
