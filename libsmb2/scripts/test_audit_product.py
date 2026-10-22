"""Exact runtime import bindings using tiny files, without managed builds."""
import copy
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('libsmb2_product_audit', HERE / 'audit-product.py')
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
import production_support


class RuntimeImportTests(unittest.TestCase):
    def match(self, method, *, declaration=None, assembly='TranslatedLibsmb2',
              relative='LibSmb2.cs', module=None, owner='LibSmb2', namespace='Managed.Smb',
              local=False):
        rule = audit.RUNTIME_IMPORTS[method]
        declaration = declaration or rule['declaration']
        if method == 'ResizeFile' or local:
            declaration = 'public static int ftruncate(int fd, long length) { ' + declaration + ' }'
        source = f'namespace {namespace}; class {owner} {{ class Libc {{ {declaration} }} }}'
        code = audit.without_comments(source)
        match = audit.IMPORT.search(code)
        self.assertIsNotNone(match)
        return audit.runtime_import_rule(assembly, relative, source, code, match,
                                         module or rule['runtimeModule'])

    def test_metadata_loads_from_an_unrelated_working_directory(self):
        command = ('import runpy; module = runpy.run_path(sys.argv[1]); '
                   'print(json.dumps([module["RUNTIME_RESOURCE_PROOF"], module["RUNTIME_IMPORTS"]]))')
        with tempfile.TemporaryDirectory() as directory:
            result = subprocess.run([sys.executable, '-B', '-c',
                'import sys, json; sys.path.insert(0, sys.argv[2]); ' + command,
                str(HERE / 'audit-product.py'), str(HERE)], cwd=directory,
                capture_output=True, text=True, check=True)
        self.assertEqual(json.loads(result.stdout), [audit.RUNTIME_RESOURCE_PROOF, audit.RUNTIME_IMPORTS])

    def test_all_33_exact_declarations_keep_their_member_scope(self):
        self.assertEqual(len(audit.RUNTIME_IMPORTS), 33)
        for method, rule in audit.RUNTIME_IMPORTS.items():
            with self.subTest(method=method):
                self.assertEqual(self.match(method), rule)

    def test_other_product_file_module_namespace_or_owner_cannot_reuse_rules(self):
        for method in audit.RUNTIME_IMPORTS:
            for change in ({'assembly': 'OtherProduct'}, {'relative': 'Other.cs'},
                           {'module': 'OtherLib.cs'}, {'namespace': 'Other.Namespace'},
                           {'owner': 'OtherOwner'}):
                with self.subTest(method=method, change=change):
                    self.assertIsNone(self.match(method, **change))

    def test_changed_library_symbol_visibility_or_import_kind_is_rejected(self):
        method = 'PosixErf'
        declaration = audit.RUNTIME_IMPORTS[method]['declaration']
        for old, new in (('libm.so.6', 'other.so'), ('"erf"', '"other"'),
                         ('private', 'public'), ('DllImport', 'LibraryImport')):
            with self.subTest(change=new):
                self.assertIsNone(self.match(method, declaration=declaration.replace(old, new)))

    def test_ordinary_member_cannot_be_moved_into_a_local_scope(self):
        self.assertIsNone(self.match('PosixErf', local=True))

    def test_resize_local_import_requires_the_exact_containing_method(self):
        declaration = audit.RUNTIME_IMPORTS['ResizeFile']['declaration']
        source = 'namespace Managed.Smb; class LibSmb2 { class Libc { ' + declaration + ' } }'
        code = audit.without_comments(source)
        self.assertIsNone(audit.runtime_import_rule('TranslatedLibsmb2', 'LibSmb2.cs', source, code,
                          audit.IMPORT.search(code), audit.RUNTIME_IMPORTS['ResizeFile']['runtimeModule']))


class RuntimeProofTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name) / 'libsmb2'
        self.root.mkdir()
        self.rules = copy.deepcopy(audit.RUNTIME_IMPORTS)
        sources = {}
        for rule in self.rules.values():
            path = self.root.parent / 'DotCC.Libc' / rule['runtimeModule']
            path.parent.mkdir(exist_ok=True)
            path.write_text('fixture source for ' + rule['runtimeModule'])
            rule['sourceSha256'] = audit.sha(path)
            sources[rule['resourceName']] = dict(resourceName=rule['resourceName'], source=str(path),
                                                sha256=rule['sourceSha256'])
        self.compiler = audit.RUNTIME_RESOURCE_PROOF['compilerLibrarySha256']
        self.resources = dict(compilerLibrarySha256=self.compiler, runtimeResourceCount=84,
                              headerResourceCount=68, sources=list(sources.values()))
        self.proof_path = self.root / 'resources.json'
        self.proof = copy.deepcopy(audit.RUNTIME_RESOURCE_PROOF)
        self.proof['path'] = str(self.proof_path)
        self.seal()
        for name, value in (('ROOT', self.root), ('RUNTIME_IMPORTS', self.rules),
                            ('RUNTIME_RESOURCE_PROOF', self.proof)):
            guard = patch.object(audit, name, value)
            guard.start()
            self.addCleanup(guard.stop)
        for name, value in (('selected_tool', Path('fixture-dotcc.dll')),
                            ('validate_tool_closure', {'DotCC.Lib.dll': self.compiler})):
            guard = patch.object(production_support, name, return_value=value)
            guard.start()
            self.addCleanup(guard.stop)

    def seal(self):
        self.proof_path.write_text(json.dumps(self.resources))
        self.proof['sha256'] = audit.sha(self.proof_path)

    def test_exact_proof_and_sources_pass(self):
        self.assertEqual(audit.runtime_source_binding(), self.proof)

    def test_changed_proof_bytes_are_rejected(self):
        self.proof_path.write_text('{}')
        with self.assertRaisesRegex(RuntimeError, 'resource proof changed'):
            audit.runtime_source_binding()

    def test_other_compiler_cannot_reuse_the_resource_proof(self):
        with patch.object(production_support, 'validate_tool_closure', return_value={'DotCC.Lib.dll': 'other'}):
            with self.assertRaisesRegex(RuntimeError, 'Selected compiler differs'):
                audit.runtime_source_binding()

    def test_compiler_identity_and_both_resource_counts_are_checked(self):
        for key, value in (('compilerLibrarySha256', 'other'), ('runtimeResourceCount', 83),
                           ('headerResourceCount', 67)):
            with self.subTest(field=key), patch.dict(self.resources, {key: value}):
                self.seal()
                with self.assertRaisesRegex(RuntimeError, 'Selected compiler differs'):
                    audit.runtime_source_binding()

    def test_changed_live_source_is_rejected(self):
        Path(self.resources['sources'][0]['source']).write_text('changed source')
        with self.assertRaisesRegex(RuntimeError, 'Exact shared runtime source changed'):
            audit.runtime_source_binding()

    def test_missing_relocated_or_rehashed_source_record_is_rejected(self):
        for field, value in (('resourceName', 'Other.Resource'), ('source', '/other/source.cs'),
                             ('sha256', 'other')):
            with self.subTest(field=field), patch.dict(self.resources['sources'][0], {field: value}):
                self.seal()
                with self.assertRaisesRegex(RuntimeError, 'Exact shared runtime source changed'):
                    audit.runtime_source_binding()


if __name__ == '__main__':
    unittest.main()
