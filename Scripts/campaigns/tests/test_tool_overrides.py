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
from production_support import validate_tool_closure


class SelectedToolTests(unittest.TestCase):
    def tool(self, path):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(path.name.encode())
        path.with_suffix('.deps.json').write_text('{}')
        path.with_suffix('.runtimeconfig.json').write_text('{}')
        if path.name == 'dotcc.dll':
            (path.parent / 'DotCC.Lib.dll').write_bytes(b'compiler library')
        return path

    def fixture(self, root):
        for project, filename in [('DotCC', 'dotcc.dll'), ('DotCC.PostProcess', 'dotcc-postprocess.dll')]:
            path = root/project/'bin/Release/net10.0'/filename
            self.tool(path).write_bytes(('root '+project).encode())
        return SimpleNamespace(repo=root, work=root/'work', options=SimpleNamespace(tools='reuse'),
            session=set(), receipt={}, run=lambda *args: 'fixture-sdk', save=lambda: None,
            policy=HashPolicy('strict', []))

    def test_independent_compiler_and_postprocessor_selections_copy_exact_closures(self):
        with tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {}, clear=True):
            root = Path(directory); ctx = self.fixture(root)
            compiler = root/'selected compiler/custom-driver.dll'
            postprocessor = root/'selected postprocessor/custom-process.dll'
            for executable in [compiler, postprocessor]:
                self.tool(executable)
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

    def test_normal_path_preserves_recursive_dependency_contract_for_each_hash_policy(self):
        for mode in ('off', 'warn', 'strict'):
            with self.subTest(mode=mode), tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {}, clear=True):
                root = Path(directory); ctx = self.fixture(root)
                ctx.policy = HashPolicy(mode, [])
                for project, filename in [('DotCC', 'dotcc.dll'), ('DotCC.PostProcess', 'dotcc-postprocess.dll')]:
                    tool = root / project / 'bin/Release/net10.0' / filename
                    nested = tool.parent / 'lib/net8.0/cs/System.CommandLine.resources.dll'
                    nested.parent.mkdir(parents=True)
                    nested.write_bytes(b'satellite dependency')
                    tool.with_suffix('.deps.json').write_text(
                        '{"targets":{"net10.0":{"package":{"resources":'
                        '{"lib/net8.0/cs/System.CommandLine.resources.dll":{"locale":"cs"}}}}}}')
                Context.tools(ctx)
                for project, actual in [('DotCC', ctx.compiler), ('DotCC.PostProcess', ctx.postprocessor)]:
                    original = root / project / 'bin/Release/net10.0' / actual.name
                    closure = validate_tool_closure(actual)
                    self.assertEqual(closure, validate_tool_closure(original))
                    self.assertEqual(ctx.receipt['tool_snapshots'][project]['files'], closure)
                    self.assertEqual(ctx.receipt['tools'][project], {} if mode == 'off' else closure)


if __name__ == '__main__':
    unittest.main(verbosity=2)
