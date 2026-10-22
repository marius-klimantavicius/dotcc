"""Exercise provenance reads/guards without running historical build or seal jobs."""
import ast
import hashlib
import json
import os
from pathlib import Path
import tempfile
import unittest


CPY = Path(__file__).resolve().parents[2]
SCRIPTS = {
    'ExecutionStackContractR34/run_focused.py': 'executionStackContractR34',
    'ExecutionStackContractR34/run_legacy_revision.py': 'executionStackContractR34',
    'ExecutionStackContractR34/seal_revised_shared.py': 'executionStackContractR34',
    'PipeMetadataR38/seal_proof.py': 'pipeMetadataR38',
    'SymlinkR39/seal_proof.py': 'symlinkR39',
    'FinalMatrixR41/run.py': 'finalMatrixR41',
}


def tree(name):
    return ast.parse((CPY / 'tests' / name).read_text())


def load(name, root=CPY.parent):
    # These historical entrypoints execute builds and write sealed artifacts at
    # module scope. Isolate the real metadata read rather than importing them.
    node = next(node for node in tree(name).body if isinstance(node, ast.Assign)
                and any(isinstance(target, ast.Name) and target.id == 'PROVENANCE'
                        for target in node.targets))
    namespace = dict(ROOT=root, CPY=root / 'cpython', json=json)
    exec(compile(ast.Module(body=[node], type_ignores=[]), name, 'exec'), namespace)
    return namespace['PROVENANCE']


class QualificationProvenanceTests(unittest.TestCase):
    def test_all_entrypoints_load_their_metadata_outside_the_repository(self):
        metadata = json.loads((CPY / 'config/qualification-provenance.json').read_text())
        with tempfile.TemporaryDirectory() as directory:
            previous = Path.cwd()
            try:
                os.chdir(directory)
                for name, section in SCRIPTS.items():
                    with self.subTest(script=name):
                        self.assertEqual(load(name), metadata[section])
            finally:
                os.chdir(previous)

    def test_missing_damaged_or_incomplete_metadata_has_no_fallback(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            path = root / 'cpython/config/qualification-provenance.json'
            path.parent.mkdir(parents=True)
            for content, error in ((None, FileNotFoundError), ('{', json.JSONDecodeError), ('{}', KeyError)):
                if content is not None:
                    path.write_text(content)
                for name in SCRIPTS:
                    with self.subTest(script=name, error=error.__name__), self.assertRaises(error):
                        load(name, root)

    def test_each_existing_fingerprint_guard_rejects_different_bytes(self):
        checked = 0
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'input'
            def item(path):
                return {'sha256': hashlib.sha256(Path(path).read_bytes()).hexdigest()}
            for name in SCRIPTS:
                for node in ast.walk(tree(name)):
                    if not isinstance(node, ast.Assert) or not any(
                            isinstance(child, ast.Name) and child.id == 'PROVENANCE'
                            for child in ast.walk(node)):
                        continue
                    with self.subTest(script=name, line=node.lineno):
                        path.write_text('held source')
                        expected = item(path)['sha256']
                        namespace = dict(hashlib=hashlib, item=item, ref=item, release=path,
                                         old=path, canonical=path, ready_path=path,
                                         PROVENANCE={key: expected for key in load(name)})
                        guard = compile(ast.Module(body=[node], type_ignores=[]), name, 'exec')
                        def inputs():
                            namespace['d10'] = path.read_text()
                            namespace['proof'] = {'immutableV21Red': {'unchangedC': item(path)}}
                        inputs()
                        exec(guard, namespace)
                        path.write_text('changed source')
                        inputs()
                        with self.assertRaises(AssertionError):
                            exec(guard, namespace)
                        checked += 1
        self.assertEqual(checked, 8)

    def test_symlink_reference_retains_metadata_revision_and_reference_only_scope(self):
        name = 'SymlinkR39/seal_proof.py'
        node = next(node for node in tree(name).body if isinstance(node, ast.Assign)
                    and any(isinstance(target, ast.Name) and target.id == 'upstream_path'
                            for target in node.targets))
        provenance = load(name)
        namespace = dict(PROVENANCE=provenance, write=lambda name, value: value,
                         item=lambda path: {'path': str(path)}, upstream=Path('reference.cs'),
                         git_blob='observed blob')
        exec(compile(ast.Module(body=[node], type_ignores=[]), name, 'exec'), namespace)
        reference = namespace['upstream_path']
        self.assertEqual(reference['revision'], provenance['upstreamRevision'])
        self.assertIs(reference['referenceOnly'], True)
        self.assertIs(reference['matchesPinnedTree'], True)


if __name__ == '__main__':
    unittest.main()
