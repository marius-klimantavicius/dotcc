"""Auditor rejection controls only. Synthetic artifacts prove no interpreter behavior."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[3]/'Scripts'))
from campaigns.identity import HashPolicy

spec = importlib.util.spec_from_file_location('owner_b_audit', Path(__file__).with_name('audit_product.py'))
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


class AuditControls(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='cpython-audit-control-')
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.campaign = self.root/'cpython'
        self.product = self.campaign/'generated/TranslatedCPython'
        self.product.mkdir(parents=True)
        config = self.campaign/'config'
        config.mkdir()
        (config/'source.json').write_text(json.dumps({'notices': ['LICENSE'], 'inline_notices': []}))
        (config/'stdlib.json').write_text(json.dumps({'files': ['encodings/__init__.py']}))
        (self.product/'stdlib/encodings').mkdir(parents=True)
        (self.product/'stdlib/encodings/__init__.py').write_text('# structural support fixture; not an interpreter\n')
        (self.product/'notices').mkdir()
        (self.product/'notices/LICENSE').write_text('harness notice fixture')
        names = ['stdlib/encodings/__init__.py', 'notices/LICENSE']
        self.manifest = {'form': 'processed', 'profile': 'embedded-gil',
                         'assets': [{'path': name, 'sha256': audit.digest(self.product/name)} for name in names]}
        self.save_manifest()
        (self.product/'Generated.cs').write_text('// harness support only; cannot execute CPython\n')
        (self.product/'Dotcc.SourceFiles.txt').write_text('Generated.cs\n')
        self.project = self.product/'TranslatedCPython.csproj'
        content = ''.join(f'<None Update="{name}" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" TargetPath="cpython/{name}" />'
                          for name in [*names, 'assets.json'])
        self.project.write_text('<Project><PropertyGroup><AssemblyName>TranslatedCPython</AssemblyName><OutputType>Library</OutputType></PropertyGroup>'
                                '<ItemGroup>'+content+'</ItemGroup></Project>')
        self.owned = ['Generated.cs', 'Dotcc.SourceFiles.txt', 'TranslatedCPython.csproj', 'assets.json', *names]
        self.save_owned()
        self.basis = {'native_imports': [{'library': 'libc', 'symbol': 'link'}]}
        self.document = {'assembly': 'Delivery', 'sha256': 'observed-control-only', 'references': ['System.Runtime'],
                         'native_imports': [], 'types': [], 'methods': [], 'calls': []}

    def save_manifest(self):
        (self.product/'assets.json').write_text(json.dumps(self.manifest))

    def save_owned(self):
        (self.product/'.campaign-files.json').write_text(json.dumps(self.owned))

    def test_complete_structural_fixture_is_only_structural(self):
        result = audit.generated(self.project, self.campaign, 'processed')
        self.assertIn('runtime qualification separate', result['scope'])
        self.assertEqual(result['source_count'], 1)

    def test_missing_generated_source_rejected(self):
        (self.product/'Generated.cs').unlink()
        with self.assertRaisesRegex(audit.AuditError, 'Missing manifest artifact'):
            audit.generated(self.project, self.campaign, 'processed')

    def test_unlisted_source_rejected(self):
        (self.product/'Unexpected.cs').write_text('// unsupported source')
        with self.assertRaisesRegex(audit.AuditError, 'Unlisted/missing generated source'):
            audit.generated(self.project, self.campaign, 'processed')

    def test_missing_runtime_asset_rejected(self):
        (self.product/'stdlib/encodings/__init__.py').unlink()
        with self.assertRaisesRegex(audit.AuditError, 'Missing manifest artifact'):
            audit.assets(self.product, self.campaign, 'processed')

    def test_asset_hash_drift_rejected_only_in_explicit_strict_mode(self):
        (self.product/'notices/LICENSE').write_text('different payload bytes')
        with self.assertRaisesRegex(RuntimeError, 'Hash differs'):
            audit.assets(self.product, self.campaign, 'processed', policy=HashPolicy('strict'))

    def test_asset_hash_drift_warn_remains_advisory(self):
        (self.product/'notices/LICENSE').write_text('edited source payload')
        policy = HashPolicy('warn')
        self.assertEqual(audit.assets(self.product, self.campaign, 'processed', policy=policy)['asset_count'], 2)
        self.assertEqual(len(policy.warnings), 1)

    def test_asset_hash_drift_off_avoids_historical_comparison(self):
        (self.product/'notices/LICENSE').write_text('edited source payload')
        policy = HashPolicy('off')
        result = audit.assets(self.product, self.campaign, 'processed', policy=policy)
        self.assertIsNone(result['manifest_sha256'])
        self.assertEqual(policy.warnings, [])

    def test_incomplete_asset_manifest_rejected(self):
        self.manifest['assets'].pop()
        self.save_manifest()
        with self.assertRaisesRegex(audit.AuditError, 'Incomplete/duplicate runtime asset closure'):
            audit.assets(self.product, self.campaign, 'processed')

    def test_wrong_form_rejected(self):
        with self.assertRaisesRegex(audit.AuditError, 'profile/form mismatch'):
            audit.assets(self.product, self.campaign, 'raw')

    def test_manifest_path_escape_rejected(self):
        with self.assertRaisesRegex(audit.AuditError, 'Unsafe manifest path'):
            audit.safe_file(self.product, '../private.txt')

    def test_native_python_payload_rejected(self):
        (self.root/'libpython3.13.so').write_bytes(b'negative control only')
        with self.assertRaisesRegex(audit.AuditError, 'Forbidden/unreviewed'):
            audit.payload(self.root)

    def test_actual_managed_product_names_are_not_native_python(self):
        payload = self.root/'payload'
        payload.mkdir()
        for name in ('TranslatedCPython.dll', 'Managed.CPython.dll'):
            (payload/name).write_bytes(b'filename classifier control only')
        self.assertEqual(audit.payload(payload)['forbidden_artifacts'], [])

    def test_windows_native_python_and_pythonnet_payloads_rejected(self):
        payload = self.root/'payload'
        payload.mkdir()
        for name in ('python313.dll', 'python3.dll', 'python_d.dll', 'Python.Runtime.dll'):
            with self.subTest(name=name):
                path = payload/name
                path.write_bytes(b'forbidden filename control')
                with self.assertRaisesRegex(audit.AuditError, 'Forbidden/unreviewed'):
                    audit.payload(payload)
                path.unlink()

    def test_unknown_native_dependency_rejected(self):
        with self.assertRaisesRegex(audit.AuditError, 'Unknown/native-engine ELF dependency'):
            audit.elf_dependencies('0x0001 (NEEDED) Shared library: [libpython3.13.so.1.0]')

    def test_system_elf_dependencies_retained(self):
        self.assertEqual(audit.elf_dependencies('0x0001 (NEEDED) Shared library: [libc.so.6]'), ['libc.so.6'])

    def test_unknown_managed_dependency_rejected(self):
        self.document['references'].append('Python.Runtime')
        with self.assertRaisesRegex(audit.AuditError, 'Unknown managed dependency'):
            audit.metadata(self.document, self.basis)

    def test_unknown_delivered_managed_assembly_rejected(self):
        self.document['assembly'] = 'Unreviewed.Engine'
        with self.assertRaisesRegex(audit.AuditError, 'Unknown delivered managed assembly'):
            audit.metadata(self.document, self.basis)

    def test_unreviewed_pinvoke_rejected(self):
        self.document['native_imports'] = [{'library': 'libpython3.13.so', 'symbol': 'Py_Initialize'}]
        with self.assertRaisesRegex(audit.AuditError, 'Unreviewed native import'):
            audit.metadata(self.document, self.basis)

    def test_product_dynamic_backend_call_rejected(self):
        self.document['calls'] = [{'caller_type': 'Managed.Interpreters.CPython', 'caller_method': 'main',
            'target_type': 'NativeImports', 'target_method': 'LoadLibrary', 'target_assembly': 'HarnessSupport'}]
        with self.assertRaisesRegex(audit.AuditError, 'Forbidden dynamic backend/loader call'):
            audit.metadata(self.document, self.basis)

    def test_unused_shared_runtime_loader_is_visible(self):
        self.document['calls'] = [{'caller_type': 'NativeImports', 'caller_method': 'LoadLibrary',
            'target_type': 'System.Runtime.InteropServices.NativeLibrary', 'target_method': 'TryLoad', 'target_assembly': 'System.Runtime'}]
        result = audit.metadata(self.document, self.basis)
        self.assertEqual(len(result['shared_runtime_dynamic_definitions']), 1)

    def test_actual_nested_shared_runtime_loader_is_visible(self):
        self.document['calls'] = [{'caller_type': 'Managed.Interpreters.CPython+NativeImports',
            'caller_method': 'LoadLibrary', 'target_type': 'System.Runtime.InteropServices.NativeLibrary',
            'target_method': 'TryLoad', 'target_assembly': 'System.Runtime'}]
        self.assertEqual(len(audit.metadata(self.document, self.basis)['shared_runtime_dynamic_definitions']), 1)

    def test_product_call_into_nested_loader_still_rejected(self):
        self.document['calls'] = [{'caller_type': 'Managed.Interpreters.CPython', 'caller_method': 'run',
            'target_type': 'Managed.Interpreters.CPython+NativeImports',
            'target_method': 'LoadLibrary', 'target_assembly': 'TranslatedCPython'}]
        with self.assertRaisesRegex(audit.AuditError, 'Forbidden dynamic backend/loader call'):
            audit.metadata(self.document, self.basis)

    def test_arbitrary_libc_suffix_is_not_shared_runtime(self):
        self.document['calls'] = [{'caller_type': 'Other.Engine.Libc', 'caller_method': 'dlopen',
            'target_type': 'System.Runtime.InteropServices.NativeLibrary', 'target_method': 'TryLoad',
            'target_assembly': 'System.Runtime'}]
        with self.assertRaisesRegex(audit.AuditError, 'Forbidden dynamic backend/loader call'):
            audit.metadata(self.document, self.basis)

    def test_no_fake_assembly_can_establish_product(self):
        with self.assertRaisesRegex(audit.AuditError, 'Missing actual translated assembly'):
            audit.metadata(self.document, self.basis, product=True)

    def test_actual_static_table_names_and_compiled_initializers_required(self):
        (self.campaign/'config/stdlib.json').write_text(json.dumps({'builtins': ['module', 'sys']}))
        config = self.root/'config.c'
        config.write_text('struct _inittab _PyImport_Inittab[] = {\n{"module", PyInit_module}, {"sys", NULL}, {0, 0}\n};\n')
        self.document['methods'] = [{'type': 'Managed.Interpreters.CPython', 'name': 'PyInit_module'}]
        self.assertEqual(len(audit.builtins(config, self.campaign, self.document)['registrations']), 2)
        self.document['methods'] = []
        with self.assertRaisesRegex(audit.AuditError, 'initializer missing'):
            audit.builtins(config, self.campaign, self.document)

    def test_static_table_cannot_silently_omit_required_module(self):
        (self.campaign/'config/stdlib.json').write_text(json.dumps({'builtins': ['required_module']}))
        config = self.root/'config.c'
        config.write_text('struct _inittab _PyImport_Inittab[] = {\n{"other_module", PyInit_other}, {0, 0}\n};\n')
        with self.assertRaisesRegex(audit.AuditError, 'registrations differ'):
            audit.builtins(config, self.campaign)


if __name__ == '__main__':
    unittest.main(verbosity=2)
