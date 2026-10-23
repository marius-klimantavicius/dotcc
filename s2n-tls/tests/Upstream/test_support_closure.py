"""No-build regressions for original support fixtures in object relocations."""
import importlib.util
from pathlib import Path
import sys
import tempfile
import unittest

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
specification = importlib.util.spec_from_file_location('upstream_support_builder', HERE / 'build.py')
builder = importlib.util.module_from_spec(specification)
specification.loader.exec_module(builder)


class SupportClosureTests(unittest.TestCase):
    def references(self, section, body):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'fixture.obj.txt'
            path.write_text('//!dotcc object 2\n//!!dotcc-obj section:' + section + '\n' + body)
            return builder.object_references(path)

    def test_address_only_external_fixture_selects_its_original_source(self):
        # These fields are the actual retained client_psk object's relocation
        # shape; its only two uses of this fixture take its address.
        root = 'security_policy_test_tls13_retry'
        fields = [root, '__DotCcAggregate_AD6DC12E5B8E8078889B83CEC086DA595AE12A2F7108EC0741DBAB3E164F6A31',
                  '__dotcc_address2', '((void*)Unsafe.AsPointer(ref /*__dotcc_global_ref__*/' + root + '))']
        encoded = 'C' + '\0'.join(fields).encode().hex().upper()
        body = 'value = /*__dotcc_address_use__*/' + encoded + ';\n'
        self.assertNotIn(root, body)
        self.assertIn(root, self.references('functions', body))
        source = HERE.parents[1] / 'ref/s2n-tls/tests/testlib/s2n_security_policy_testlib.c'
        self.assertIn('const struct s2n_security_policy ' + root + ' = {', source.read_text())

    def test_initializer_address_references_preserve_all_original_roots(self):
        for kind, fields in (
            ('address_declaration', ['fixture', 'int', 'local']),
            ('address_use', ['fixture', 'int', 'local', 'contextual']),
            ('context_address', ['fixture', 'declaration']),
            ('initializer_address', ['fixture', 'int', 'contextual']),
            ('bound_initializer_address', ['fixture', 'int', 'contextual'])):
            with self.subTest(kind=kind):
                record = 'C' + '\0'.join(fields).encode().hex().upper()
                self.assertIn('fixture', self.references('global-initializer',
                    '/*__dotcc_' + kind + '__*/' + record))
                self.assertNotIn('fixture', self.references('aliases',
                    '/*__dotcc_' + kind + '__*/' + record))

    def test_malformed_relocation_cannot_silently_omit_a_fixture(self):
        for record in ('', '@record', 'C' + b'fixture\0int\0local'.hex().upper() + '@tail', 'C0', 'CFF', 'C' + b'fixture\0missing-fields'.hex().upper(),
                       'X' + b'fixture\0int\0local'.hex().upper()):
            with self.subTest(record=record), self.assertRaisesRegex(RuntimeError, 'Malformed'):
                self.references('functions', '/*__dotcc_address_declaration__*/' + record + ';')

    def test_encoded_context_keeps_nested_plain_and_structured_roots(self):
        inner = 'C' + b'nested_fixture\0int\0context'.hex().upper()
        contextual = ('/*__dotcc_global_ref__*/ordinary_fixture + /*__dotcc_thread_slot__*/thread_fixture + '
                      '/*__dotcc_initializer_address__*/' + inner)
        outer = 'C' + ('outer_fixture\0int\0local\0' + contextual).encode().hex().upper()
        actual = self.references('functions', '/*__dotcc_address_use__*/' + outer)
        self.assertTrue({'outer_fixture', 'ordinary_fixture', 'thread_fixture', 'nested_fixture'} <= actual)


if __name__ == '__main__':
    unittest.main()
