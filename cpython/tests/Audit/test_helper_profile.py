"""Negative readiness controls use synthetic receipts only; no interpreter."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('helper_role_audit', Path(__file__).with_name('helper_profile.py'))
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


class HelperProfileControls(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source = self.root/'build/run/source'
        (self.root/'config').mkdir()
        (self.root/'config/sources.json').write_text('{"units": []}')
        (self.root/'config/embedding.json').write_text('{"units": []}')
        (self.root/'tests/Upstream').mkdir(parents=True)
        self.receipt = {'project': 'cpython', 'profile': 'upstream-tests-static', 'run_id': 'fixture',
                        'staging': 'build/run', 'status': 'passed', 'outputs': {}, 'units': [], 'commands': [],
                        'translation_arguments': {'common': ['-DPy_BUILD_CORE']},
                        'test_profile': {'modules': [], 'selected_units': 3}}
        definitions = []
        for name in audit.ROLES:
            path = self.source/'Modules'/name/'entry.c'
            path.parent.mkdir(parents=True)
            path.write_text('#include "parts.h"\n')
            guard = ('#ifdef PYTESTCAPI_NEED_INTERNAL_API\n#  error public\n' if name == '_testcapi'
                     else '#define Py_LIMITED_API 0x03050000\n#  error limited\n' if name == '_testlimitedcapi'
                     else '#define Py_BUILD_CORE_MODULE 1\n')
            (path.parent/'parts.h').write_text(guard+'#undef Py_BUILD_CORE_BUILTIN\n#include "Python.h"\n')
            relative = path.relative_to(self.source).as_posix()
            definitions.append({'module': name, 'sources': [relative]})
            self.receipt['test_profile']['modules'].append({'name': name,
                'sources': [{'path': relative, 'sha256': audit.digest(path)}],
                'flags': ['-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN']})
            self.receipt['units'].append(str(path))
            self.receipt['commands'].append({'command': ['dotnet', 'fixture.dll', '-DPy_BUILD_CORE',
                '-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN', str(path), '--emit=obj'],
                'status': 'passed', 'exit_code': 0, 'label': name})
        (self.source/'Include').mkdir()
        (self.source/'Include/pyport.h').write_text('/* fixture only */')
        (self.root/'tests/Upstream/helper-contract.json').write_text(json.dumps({'modules': definitions}))
        (self.root/'config/stdlib.json').write_text(json.dumps({'builtins': ['sys']}))
        self.config = self.source/'Modules/config.c'
        self.config.write_text('struct init _PyImport_Inittab[] = {\n'+''.join(
            '{"'+name+'", PyInit_'+name+'},\n' for name in audit.ROLES)+'{"sys", NULL},\n};\n')
        self.path = self.root/'receipt.json'

    def inspect(self):
        self.path.write_text(json.dumps(self.receipt))
        return audit.audit(self.path, self.root)

    def errors(self):
        return [row['error'] for row in self.inspect()['issues']]

    def test_command_order_and_split_options(self):
        self.assertNotIn('Py_BUILD_CORE', audit.entry_macros(['-DPy_BUILD_CORE', '-U', 'Py_BUILD_CORE']))
        self.assertEqual(audit.entry_macros(['-UFOO', '-D', 'FOO=7'])['FOO'], '7')
        self.assertEqual(audit.entry_macros(['-DFOO=7', '-UFOO', '-DFOO=9'])['FOO'], '9')

    def test_metadata_absence_never_qualifies(self):
        result = self.inspect()
        self.assertFalse(result['ready'])
        self.assertEqual(len(result['role_units']), 3)
        self.assertIn('Actual compiled profile PE/registration inspection unrun', self.errors())

    def test_explicit_source_readiness_does_not_claim_compiled_proof(self):
        self.receipt['outputs'] = {'raw': {'project': str(self.root/'canonical/TranslatedCPython.csproj')}}
        self.path.write_text(json.dumps(self.receipt))
        result = audit.source_readiness(self.path, self.root)
        self.assertTrue(result['ready'])
        self.assertIsNone(result['compiled'])
        self.assertIn('compiled initialization and assertion execution unreviewed', result['scope'])

    def test_source_readiness_still_rejects_failed_emission(self):
        self.receipt['commands'][0].update(status='cancelled', exit_code=130)
        self.path.write_text(json.dumps(self.receipt))
        self.assertFalse(audit.source_readiness(self.path, self.root)['ready'])

    def test_inherited_core_rejected_despite_builtin(self):
        self.receipt['commands'][0]['command'].remove('-UPy_BUILD_CORE')
        errors = self.errors()
        self.assertIn('Inherited Py_BUILD_CORE reaches public/limited guard', errors)
        self.assertIn('Actual macro options differ from manifest/common flags', errors)

    def test_late_redefinition_rejected(self):
        self.receipt['commands'][1]['command'].insert(-1, '-DPy_BUILD_CORE')
        self.assertIn('Inherited Py_BUILD_CORE reaches public/limited guard', self.errors())

    def test_internal_role_cannot_be_erased(self):
        self.receipt['commands'][2]['command'].remove('-DPy_BUILD_CORE_BUILTIN')
        self.assertIn('Internal helper has no core role at entry', self.errors())

    def test_source_opt_in_preserved(self):
        module = self.receipt['test_profile']['modules'][0]
        path = self.source/module['sources'][0]['path']
        path.write_text('#define PYTESTCAPI_NEED_INTERNAL_API\n#include "parts.h"\n')
        module['sources'][0]['sha256'] = audit.digest(path)
        self.assertEqual(self.inspect()['role_units'][0]['role'], 'public-with-internal-opt-in')
        self.assertEqual(audit.role('_testcapi', '#include "parts.h"\n#define PYTESTCAPI_NEED_INTERNAL_API'),
                         'public')

    def test_command_cannot_force_source_opt_in(self):
        self.receipt['commands'][0]['command'].insert(-1, '-DPYTESTCAPI_NEED_INTERNAL_API')
        self.assertIn('Command overrides source-selected API/profile role', self.errors())

    def test_cancelled_and_missing_commands_remain_unready(self):
        self.receipt['commands'][0].update(status='cancelled', exit_code=130)
        self.receipt['commands'].pop()
        result = self.inspect()
        self.assertFalse(result['ready'])
        self.assertEqual(result['role_units'][0]['status'], 'cancelled')
        self.assertEqual(result['role_units'][2]['status'], 'unrun-or-ambiguous')

    def test_missing_static_registration_rejected(self):
        self.config.write_text(self.config.read_text().replace('{"_testlimitedcapi", PyInit__testlimitedcapi},\n', ''))
        with self.assertRaisesRegex(ValueError, 'registration closure'):
            self.inspect()

    def test_source_drift_rejected(self):
        Path(self.receipt['units'][0]).write_text('changed')
        with self.assertRaisesRegex(ValueError, 'source differs'):
            self.inspect()

    def test_helper_source_drop_rejected(self):
        self.receipt['test_profile']['modules'][0]['sources'] = []
        with self.assertRaisesRegex(ValueError, 'source closure'):
            self.inspect()

    def test_product_unit_drop_rejected(self):
        (self.root/'config/sources.json').write_text('{"units": [{"path": "Python/ceval.c"}]}')
        with self.assertRaisesRegex(ValueError, 'drops/adds units'):
            self.inspect()

    def test_guard_removal_rejected(self):
        (self.source/'Modules/_testcapi/parts.h').write_text('#include "Python.h"\n')
        with self.assertRaisesRegex(ValueError, 'API guard missing'):
            self.inspect()

    def metadata_fixture(self):
        output = self.root/'generated/test-profile'
        output.mkdir(parents=True)
        assembly = output/'TranslatedCPython.dll'
        assembly.write_bytes(b'synthetic identity fixture, never loaded as a PE')
        self.receipt['outputs'] = {'raw': {'project': str(output/'TranslatedCPython.csproj')}}
        document = {'assembly': 'TranslatedCPython', 'path': str(assembly), 'sha256': audit.digest(assembly),
                    'mvid': 'synthetic fixture', 'methods': [
                        {'type': 'Managed.Interpreters.CPython', 'name': 'PyInit_'+name} for name in audit.ROLES]}
        return document

    def inspect_metadata(self, document):
        self.path.write_text(json.dumps(self.receipt))
        path = self.root/'metadata.json'
        path.write_text(json.dumps(document))
        return audit.audit(self.path, self.root, path)

    def test_complete_fixture_checks_all_registrations(self):
        result = self.inspect_metadata(self.metadata_fixture())
        self.assertTrue(result['ready'])  # Readiness-envelope fixture only.
        self.assertEqual(len(result['static_registrations']['entries']), 4)

    def test_missing_compiled_initializer_rejected(self):
        document = self.metadata_fixture()
        document['methods'].pop()
        with self.assertRaisesRegex(ValueError, 'initializer missing'):
            self.inspect_metadata(document)

    def test_other_product_pe_rejected(self):
        document = self.metadata_fixture()
        self.receipt['outputs']['raw']['project'] = str(self.root/'unrelated/TranslatedCPython.csproj')
        with self.assertRaisesRegex(ValueError, 'outside actual published'):
            self.inspect_metadata(document)

    def test_metadata_from_changed_pe_rejected(self):
        document = self.metadata_fixture()
        Path(document['path']).write_bytes(b'changed bytes')
        with self.assertRaisesRegex(ValueError, 'PE identity mismatch'):
            self.inspect_metadata(document)


if __name__ == '__main__':
    unittest.main(verbosity=2)
