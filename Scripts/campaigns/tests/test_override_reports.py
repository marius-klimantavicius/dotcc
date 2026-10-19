"""Object/probe commands own one distinct report path per translation unit."""
from pathlib import Path
import sys
import tempfile
import threading
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.identity import HashPolicy
from campaigns.model import Translation, Unit
from campaigns.translation import translate


class ObjectOverrideReportTests(unittest.TestCase):
    def test_later_failed_worker_cancels_earlier_running_unit(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root/'work').mkdir(); (root/'artifacts').mkdir()
            units = []
            for name in ('slow', 'failed'):
                source = root / (name + '.c'); source.write_text('int value;')
                units.append(Unit(source))
            started, stopped, cancellation = threading.Event(), threading.Event(), threading.Event()
            def run(command, label, timeout):
                if label.endswith('slow'):
                    started.set()
                    if not cancellation.wait(timeout=3):
                        raise AssertionError('A later failed unit did not cancel its running sibling')
                    stopped.set()
                    raise RuntimeError('slow sibling cancelled')
                self.assertTrue(started.wait(timeout=1))
                raise ValueError('actual later worker failure')
            ctx = SimpleNamespace(root=root, work=root/'work', artifacts=root/'artifacts',
                recipe=SimpleNamespace(prepare=lambda ctx: Translation(units, [], [])),
                options=SimpleNamespace(unit=[], action='probe', jobs=2, timeout=5),
                receipt={}, policy=HashPolicy('off'), compiler=root/'dotcc.dll',
                cancellation=cancellation, run=run)
            with self.assertRaisesRegex(ValueError, 'actual later worker failure'):
                translate(ctx)
            self.assertTrue(cancellation.is_set())
            self.assertTrue(stopped.is_set())

    def test_probe_replaces_recipe_report_and_keeps_unit_reports_distinct(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name in ('work', 'artifacts'):
                (root / name).mkdir()
            units = []
            for name in ('one', 'two'):
                path = root / (name + '.c')
                path.write_text('int value;')
                units.append(Unit(path))
            commands = []
            def run(command, label, timeout):
                commands.append(command)
                self.assertEqual(command.count('--override-report'), 1)
                Path(command[command.index('-o') + 1]).write_text('object')
            spec = Translation(units, ['-std=c11', '--override-report', str(root / 'recipe.jsonl')], [])
            context = SimpleNamespace(root=root, work=root/'work', artifacts=root/'artifacts',
                recipe=SimpleNamespace(prepare=lambda ctx: spec),
                options=SimpleNamespace(unit=[], action='probe', jobs=1, timeout=30),
                receipt={}, policy=HashPolicy('off'), compiler=root/'dotcc.dll',
                cancellation=threading.Event(), run=run)
            translate(context)
            reports = [Path(c[c.index('--override-report')+1]) for c in commands]
            self.assertEqual(len(set(reports)), 2)
            self.assertTrue(all(p.parent == context.artifacts for p in reports))
            self.assertTrue(all(c[2] == '-std=c11' for c in commands))


if __name__ == '__main__':
    unittest.main()
