"""Selected tool snapshots must not silently fall back to root tool bytes."""
import os
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.identity import HashPolicy
from campaigns.runner import Context


class SelectedToolTests(unittest.TestCase):
    def fixture(self, root):
        for project, filename in [('DotCC', 'dotcc.dll'), ('DotCC.PostProcess', 'dotcc-postprocess.dll')]:
            path = root/project/'bin/Release/net10.0'/filename
            path.parent.mkdir(parents=True)
            path.write_bytes(('root '+project).encode())
        return SimpleNamespace(repo=root, work=root/'work', options=SimpleNamespace(tools='reuse'),
            session=set(), receipt={}, run=lambda *args: 'fixture-sdk', save=lambda: None,
            policy=HashPolicy('strict', []))

    def test_independent_compiler_and_postprocessor_selections_copy_exact_closures(self):
        with tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {}, clear=True):
            root = Path(directory); ctx = self.fixture(root)
            compiler = root/'selected compiler/custom-driver.dll'
            postprocessor = root/'selected postprocessor/custom-process.dll'
            for executable in [compiler, postprocessor]:
                executable.parent.mkdir()
                executable.write_bytes(executable.name.encode())
                (executable.parent/'dependency.dll').write_bytes(b'selected dependency')
                (executable.parent/'settings.json').write_text('{}')
            with patch.dict(os.environ, DOTCC_COMPILER=str(compiler), DOTCC_POSTPROCESSOR=str(postprocessor)):
                Context.tools(ctx)
            for project, selected, actual in [('DotCC', compiler, ctx.compiler),
                                               ('DotCC.PostProcess', postprocessor, ctx.postprocessor)]:
                self.assertEqual(actual.name, selected.name)
                expected = {p.name: p.read_bytes() for p in selected.parent.iterdir()}
                self.assertEqual({p.name: p.read_bytes() for p in actual.parent.iterdir()}, expected)
                self.assertEqual(set(ctx.receipt['tools'][project]), set(expected))
            self.assertEqual((root/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll').read_bytes(),
                             b'root DotCC.PostProcess')

    def test_missing_selected_postprocessor_fails_instead_of_using_root(self):
        with tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {}, clear=True):
            root = Path(directory); ctx = self.fixture(root)
            with patch.dict(os.environ, DOTCC_POSTPROCESSOR=str(root/'missing/process.dll')):
                with self.assertRaisesRegex(RuntimeError, 'Missing tool:.*missing/process.dll'):
                    Context.tools(ctx)

    def test_default_root_selection_remains_available(self):
        with tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {}, clear=True):
            root = Path(directory); ctx = self.fixture(root)
            Context.tools(ctx)
            self.assertEqual(ctx.compiler.read_bytes(), b'root DotCC')
            self.assertEqual(ctx.postprocessor.read_bytes(), b'root DotCC.PostProcess')


if __name__ == '__main__':
    unittest.main(verbosity=2)
