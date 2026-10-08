#!/usr/bin/env python3
"""Verify the pinned termcolor staging adapter, preserving all test artifacts."""
import argparse
import hashlib
import io
import json
import os
from pathlib import Path
import sys
import unittest

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import portable


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    relative = Path('test/sqlite/termcolor.hpp')
    source_path = args.source.resolve() / relative
    original = source_path.read_bytes()
    original_hash = 'b8c83e67c4662278108109922f3ef4d41fc01353bf9cc72207ed87b235b55783'
    adapted_hash = 'af447ee81a8d4e63bddc7641ca4ce1c7b95e496fcaf34654c347dd5b3093b839'
    assert hashlib.sha256(original).hexdigest() == original_hash
    inputs = [Path(__file__).resolve(), Path(portable.__file__).resolve(), source_path]
    fingerprints = [dict(path=str(p), sha256=portable.sha(p)) for p in inputs]

    class AdaptationTests(unittest.TestCase):
        def setUp(self):
            self.case = out / self._testMethodName
            self.source, self.stage = self.case / 'source', self.case / 'staged'
            for root in (self.source, self.stage):
                (root / relative).parent.mkdir(parents=True)
                (root / relative).write_bytes(original)

        def adapt(self):
            return portable.adapt_termcolor(self.source, self.stage)

        def reject(self, message):
            before = [(p / relative).read_bytes() for p in (self.source, self.stage)]
            with self.assertRaisesRegex(ValueError, message):
                self.adapt()
            self.assertEqual(before, [(p / relative).read_bytes() for p in (self.source, self.stage)])

        def test_exact_reviewed_output(self):
            receipt = self.adapt()
            self.assertEqual(portable.sha(self.stage / relative), adapted_hash)
            self.assertEqual((self.source / relative).read_bytes(), original)
            self.assertEqual(receipt['originalSha256'], original_hash)
            self.assertEqual(receipt['stagedSha256'], adapted_hash)

        def test_idempotence(self):
            receipt = self.adapt()
            before = (self.stage / relative).stat().st_mtime_ns
            self.assertEqual(self.adapt(), receipt)
            self.assertEqual((self.stage / relative).stat().st_mtime_ns, before)

        def test_original_changed(self):
            (self.source / relative).write_bytes(original + b'\n')
            self.reject('original header changed')

        def test_staged_changed(self):
            (self.stage / relative).write_bytes(original + b'\n')
            self.reject('neither pinned')

        def test_partial_adaptation(self):
            (self.stage / relative).write_bytes(original.replace(b'#if defined(_WIN32)', b'#if defined(__EDG_DOT_TARGET__)'))
            self.reject('neither pinned')

        def test_modified_adapted_rejected(self):
            self.adapt()
            p = self.stage / relative
            p.write_bytes(p.read_bytes().replace(b'::isatty', b'::not_isatty'))
            self.reject('neither pinned')

        def test_adapted_source_rejected(self):
            self.adapt()
            (self.source / relative).write_bytes((self.stage / relative).read_bytes())
            self.reject('original header changed')

        def test_staged_symlink(self):
            (self.stage / relative).unlink()
            (self.stage / relative).symlink_to(self.source / relative)
            self.reject('distinct regular')

        def test_original_symlink(self):
            saved = self.case / 'saved.hpp'
            saved.write_bytes(original)
            (self.source / relative).unlink()
            (self.source / relative).symlink_to(saved)
            self.reject('distinct regular')

        def test_hardlink_alias(self):
            (self.stage / relative).unlink()
            os.link(self.source / relative, self.stage / relative)
            self.reject('distinct regular')

        def test_identical_root(self):
            with self.assertRaisesRegex(ValueError, 'distinct regular'):
                portable.adapt_termcolor(self.source, self.source)
            self.assertEqual((self.source / relative).read_bytes(), original)

        def test_other_files_unchanged(self):
            other = self.stage / 'untouched.cpp'
            other.write_bytes(b'int value;\n')
            self.adapt()
            self.assertEqual(other.read_bytes(), b'int value;\n')

        def test_no_global_platform_defines(self):
            self.adapt()
            added = set((self.stage / relative).read_bytes().splitlines()) - set(original.splitlines())
            self.assertEqual([line for line in added if line.startswith(b'#define')], [b'#define TERMCOLOR_OS_EDG'])
            self.assertIn(b'#undef TERMCOLOR_OS_EDG', added)

    stream = io.StringIO()
    result = unittest.TextTestRunner(stream=stream, verbosity=2).run(
        unittest.defaultTestLoader.loadTestsFromTestCase(AdaptationTests))
    (out / 'tests.log').write_text(stream.getvalue())
    unchanged = all(portable.sha(Path(record['path'])) == record['sha256'] for record in fingerprints)
    receipt = dict(schema=1, status='passed' if result.wasSuccessful() and unchanged else 'failed',
                   tests=result.testsRun, failures=len(result.failures), errors=len(result.errors),
                   inputs=fingerprints, inputsUnchanged=unchanged, fullRestage=False,
                   translatedExecution=False, scope='pinned source staging adapter controls')
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(stream.getvalue(), end='')
    return 0 if receipt['status'] == 'passed' else 1


if __name__ == '__main__':
    sys.exit(main())
