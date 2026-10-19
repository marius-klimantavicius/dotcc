"""Run the real selector in a miniature tree with every launch hard intercepted."""
import builtins
import contextlib
import io
import json
from pathlib import Path
import sys
import tempfile
import types
import unittest
from unittest.mock import patch

SCRIPT = Path(__file__).resolve().parents[2] / 'scripts/test-postprocess.py'
CHECKS = ([f'Original-{name}-{mode}' for name in ('HostVfsTests', 'ThreadingTests') for mode in ('jit', 'aot')]
          + [f'Optimized-{name}-{mode}' for name in ('core', 'api', 'upstream', 'fts5') for mode in ('jit', 'aot')]
          + [f'Original-{name}-aot' for name in ('core', 'upstream', 'fts5')])


class SelectorTests(unittest.TestCase):
    def run_selector(self, checks):
        with tempfile.TemporaryDirectory() as directory:
            repo = Path(directory)
            root = repo / 'sqlite'
            script = root / 'scripts/test-postprocess.py'
            script.parent.mkdir(parents=True)
            script.write_bytes(SCRIPT.read_bytes())
            (repo / 'Scripts').mkdir()
            (repo / 'Scripts/production_support.py').write_text('# import is intercepted\n')
            (root / 'tests').mkdir()
            (root / 'generated').mkdir()
            for suite, expected in [('core', 'native-corpus.expected'), ('api', 'native-api.expected'),
                                    ('upstream', 'upstream-jsonb.expected'), ('fts5', 'native-fts5.expected')]:
                project = root / 'generated' / ('Test-' + suite) / ('Test-' + suite + '.csproj')
                project.parent.mkdir()
                project.write_text('<Project/>')
                (root / 'tests' / expected).write_text('fixture\n')
            snapshot = repo / 'selection'
            snapshot.mkdir()
            (snapshot / 'manifest.json').write_text(json.dumps(dict(OriginalProject='raw.csproj',
                OptimizedProject='pp.csproj', AssemblyName='TranslatedSqlite')))
            calls = []
            support = types.ModuleType('production_support')

            def fake_run(command, **kwargs):
                command = list(map(str, command))
                calls.append(command)
                if '--output' in command:
                    output = Path(command[command.index('--output') + 1])
                    output.mkdir(parents=True)
                    name = Path(command[2]).stem
                    (output / 'manifest.json').write_text(json.dumps(dict(OriginalProject='raw.csproj',
                        OptimizedProject='pp.csproj', AssemblyName=name)))
                kwargs['stdout'].write('fixture\n')
                return types.SimpleNamespace(returncode=0)

            support.subprocess = types.SimpleNamespace(run=fake_run, STDOUT=-2)
            support.selected_tool = lambda kind: Path('/fake/selected-postprocessor.dll')
            real_import = builtins.__import__

            def fake_import(name, *args, **kwargs):
                return support if name == 'production_support' else real_import(name, *args, **kwargs)

            scoped = dict(vars(builtins), __import__=fake_import)
            namespace = dict(__name__='__main__', __file__=str(script), __builtins__=scoped)
            argv = [str(script), str(snapshot), '--artifacts', str(repo / 'logs')]
            argv += sum((['--check', check] for check in checks), [])
            stdout, stderr = io.StringIO(), io.StringIO()
            with patch.object(sys, 'argv', argv), contextlib.redirect_stdout(stdout), contextlib.redirect_stderr(stderr):
                with self.assertRaises(SystemExit) as result:
                    exec(compile(script.read_bytes(), str(script), 'exec'), namespace)
            return result.exception.code, calls, stdout.getvalue(), stderr.getvalue()

    def test_exact_fifteen_cells_no_benchmark_and_four_shared_rewrites(self):
        code, calls, stdout, _ = self.run_selector(CHECKS)
        self.assertEqual(code, 0)
        self.assertEqual({line[5:] for line in stdout.splitlines() if line.startswith('PASS ')}, set(CHECKS))
        primary = [c for c in calls if (c[0] == 'dotnet' and c[1].endswith('.dll') and '--output' not in c)
                   or (len(c) == 1)]
        self.assertEqual(len(primary), 15)
        self.assertEqual(sum('--output' in c for c in calls), 4)
        self.assertFalse(any('PostprocessBenchmark' in value for c in calls for value in c))
        # Every external execution in the copied script reached our fake callable.
        self.assertEqual(len(calls), 36)

    def test_benchmark_is_rejected_before_any_external_call(self):
        code, calls, _, _ = self.run_selector(['Optimized-PostprocessBenchmark-jit'])
        self.assertEqual(code, 2)
        self.assertEqual(calls, [])


if __name__ == '__main__':
    unittest.main()
