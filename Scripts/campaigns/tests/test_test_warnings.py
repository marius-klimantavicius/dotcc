"""Behavioral warnings must survive every provenance policy without failing."""
from contextlib import redirect_stderr, redirect_stdout
import io
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.model import Recipe
from campaigns.runner import Context


class TestWarningTests(unittest.TestCase):
    def test_warning_is_recorded_and_displayed_under_every_hash_policy(self):
        for mode in ('off', 'warn', 'strict'):
            with self.subTest(mode=mode), tempfile.TemporaryDirectory() as directory:
                options = SimpleNamespace(profile=None, action='test', hashes=mode,
                                          fetch='never', restore='none')
                recipe = Recipe('fixture', 'TranslatedFixture', ('default',), (), lambda root: ())
                context = Context(directory, recipe, options)
                warning = 'WARNING: [test] Known timing dependent upstream outcome'
                with redirect_stdout(io.StringIO()), redirect_stderr(io.StringIO()) as log:
                    context.start()
                    context.run([sys.executable, '-c', f'print({warning!r})'], 'observation')
                    context.finish()
                self.assertIn(warning, log.getvalue())
                self.assertEqual(context.receipt['warnings'], [warning.removeprefix('WARNING: ')])
                self.assertEqual(context.receipt['status'], 'passed')


if __name__ == '__main__':
    unittest.main()
