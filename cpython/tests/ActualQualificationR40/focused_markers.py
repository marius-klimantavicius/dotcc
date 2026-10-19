"""Separate actual-product comparison; unchanged full inventory runs next.

The original discovery/load_tests/execute_groups machinery resolves every ID.
Required native SKIP/XFAIL remain nonpasses, including on exact parity.
"""
from collections import Counter
import contextlib
import errno
import hashlib
import importlib
import io
import json
import os
from pathlib import Path
import stat
import sys
import tempfile
import traceback

import run_inventory as runner


def services():
    checks = []
    r, w = os.pipe()
    try:
        a, b = os.fstat(r), os.fstat(w)
        assert stat.S_ISFIFO(a.st_mode) and stat.S_ISFIFO(b.st_mode)
        assert (a.st_dev, a.st_ino) == (b.st_dev, b.st_ino)
        assert a.st_ino != 0 and a.st_nlink == 1
        assert not os.get_inheritable(r) and not os.get_inheritable(w)
        with io.FileIO(r, 'r', closefd=False) as reader, io.FileIO(w, 'w', closefd=False) as writer:
            assert not reader.seekable() and not writer.seekable()
            try:
                reader.seek(0)
            except OSError as error:
                assert error.errno == errno.ESPIPE
            else:
                raise AssertionError('FIFO seek unexpectedly succeeded')
            assert writer.write(b'fifo-r40') == 8
            after_write = os.fstat(w)
            assert after_write.st_mtime >= a.st_mtime
            assert reader.read(8) == b'fifo-r40'
            after_read = os.fstat(r)
            assert after_read.st_atime >= a.st_atime
        checks.append({'case': 'real FIFO metadata/FileIO/nonseekable/bytes/timestamps', 'status': 'passed',
                       'sameQueueIdentity': True, 'bytes': 8})
    finally:
        os.close(r)
        os.close(w)
    with tempfile.TemporaryDirectory() as directory:
        root = Path(directory)
        (root / 'target').write_bytes(b'link-r40')
        os.symlink('target', root / 'link')
        assert os.readlink(root / 'link') == 'target'
        assert stat.S_ISLNK(os.lstat(root / 'link').st_mode)
        assert (root / 'link').read_bytes() == b'link-r40'
        os.symlink('absent', root / 'dangling')
        assert os.path.lexists(root / 'dangling') and not os.path.exists(root / 'dangling')
        for operation in (lambda: os.readlink(root / 'missing'),
                          lambda: os.stat(root / 'dangling')):
            try:
                operation()
            except OSError as error:
                assert error.errno == errno.ENOENT, error
            else:
                raise AssertionError('Missing symlink/target succeeded')
        checks.append({'case': 'relative symlink/readlink/lstat/target/dangling/ENOENT', 'status': 'passed'})
        fd = os.open(root / 'offsets', os.O_RDWR | os.O_CREAT | os.O_EXCL, 0o600)
        try:
            for initial, offset, length in ((5, 5, 0), (8, 6, 3), (8, 2, 3), (8, 11, 8),
                                           (8, 11, 20), (8, 25, 20),
                                           (2147483650, 2147483650, 2147483649),
                                           (2147483649, 2147483650, 2147483651)):
                os.ftruncate(fd, initial)
                assert os.lseek(fd, offset, os.SEEK_SET) == offset
                os.ftruncate(fd, length)
                assert (os.lseek(fd, 0, os.SEEK_CUR), os.fstat(fd).st_size) == (offset, length)
                assert os.write(fd, b'\x5a') == 1
                assert (os.lseek(fd, 0, os.SEEK_CUR), os.fstat(fd).st_size) == (offset + 1, max(length, offset + 1))
                checks.append({'case': 'native-matched ftruncate offsets', 'initial': initial,
                               'offset': offset, 'length': length, 'status': 'passed'})
        finally:
            os.close(fd)
    return checks


def run_embedded(inventory_path, report_path):
    assert sys.version_info[:3] == (3, 13, 15) and __debug__
    assert sys.flags.isolated and sys.flags.ignore_environment
    assert sys.flags.utf8_mode == 0 and not sys.dont_write_bytecode
    assert 'poison-r40' not in sys.path
    inventory_path, report_path = Path(inventory_path), Path(report_path)
    inventory = json.loads(inventory_path.read_text())
    inputs = json.loads((Path(__file__).parent / 'focused-inputs.json').read_text())
    markers = inputs['markers']['cases']
    expected = {row['case']: row for row in inputs['native']['witnesses']}
    selected = [row['case'] for row in markers]
    assert len(selected) == len(set(selected)) == 104
    assert set(selected) == set(expected) <= set(inventory['required_cases'])
    report = {'scope': 'separate focused actual RAW JIT comparison; not full qualification',
              'implementation': 'translated', 'qualifies_translated_interpreter': False,
              'selectedIds': selected, 'cases': [], 'executed': 0, 'status': 'running',
              'startup': {'utf8_mode': sys.flags.utf8_mode, 'dont_write_bytecode': sys.dont_write_bytecode,
                          'isolated': sys.flags.isolated, 'ignore_environment': sys.flags.ignore_environment},
              'nativeEvidence': inputs['nativeEvidence']}

    def save():
        report['counts'] = dict(Counter(row['status'] for row in report['cases']))
        report_path.write_text(json.dumps(report, indent=2) + '\n')

    work = report_path.with_suffix('.work')
    work.mkdir()
    with contextlib.chdir(work):
        save()
        try:
            report['services'] = services()
            report['selectedModuleImports'] = []
            for name in inputs['selectedModules']:
                module = importlib.import_module(name)
                assert module.__spec__.origin == 'built-in', (name, module.__spec__.origin)
                report['selectedModuleImports'].append({'name': name, 'origin': module.__spec__.origin})
            assert len(report['selectedModuleImports']) == 75
            for name in ('socket', 'subprocess', 'asyncio', 'importlib.resources', 'test.support'):
                importlib.import_module(name)
            report['importsCompleteBeforeDiscovery'] = True
            live = runner.DiscoveredCases()
            discovery = runner.discover(inventory['discovery_modules'], live,
                                        inventory.get('platform_only_module_skips'))
            runner.validate_inventory(inventory, discovery)
            report['discovery'] = discovery
            report['sourceWitnesses'] = []
            for marker in markers:
                for test in live[marker['case']]:
                    detail = runner.case_detail(test)
                    method = getattr(type(test), test._testMethodName)
                    path = Path(runner.inspect.getsourcefile(runner.source_callable(method)))
                    source = marker['source']
                    assert hashlib.sha256(path.read_bytes()).hexdigest() == source['file']['sha256'], path
                    assert detail['source'] == source['originalMethodSource']
                    assert detail['expected_failure'] == source['expectedFailure']
                report['sourceWitnesses'].append({'case': marker['case'], 'path': str(path),
                    'sha256': source['file']['sha256'], 'methodAndDecoratorUnchanged': True})
            save()
            with report_path.with_suffix('.journal.jsonl').open('w') as journal, \
                    report_path.with_suffix('.unittest.log').open('w') as stream:
                report['unchangedRunnerSuccess'] = runner.execute_groups(inventory, discovery, report,
                    journal, selected, stream=stream, checkpoint=save)
            report['nativeComparison'] = []
            for case in selected:
                actual = [r for r in report['cases'] if r['case'] == case]
                native = expected[case]['nativeModeWitnesses']['0']['outcomes']
                a = [(r['status'], r.get('reason')) for r in actual]
                n = [(r['status'], r.get('reason')) for r in native]
                report['nativeComparison'].append({'case': case, 'actual': a, 'native': n, 'equal': a == n})
            report['all104ExactNativeStatusesMatch'] = all(r['equal'] for r in report['nativeComparison'])
            report['status'] = 'observed; required nonpasses retained'
        except BaseException:
            report['status'] = 'actual focused blocker'
            report['failure'] = traceback.format_exc()
            raise
        finally:
            save()
    return 0 if report['unchangedRunnerSuccess'] else 1
