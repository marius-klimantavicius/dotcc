"""Input-selection rejection controls; synthetic files never qualify D10."""
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[3]/'Scripts'))
from campaigns.recipes import helper

consumer = helper(Path(__file__).with_name('consumer_evidence.py'))
audit = helper(Path(__file__).with_name('audit_product.py'))


class ConsumerInputBinding(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='cpython-consumer-binding-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.products = self.root/'products'
        self.output = self.root/'app'
        self.output.mkdir()
        self.pe = self.products/'bin/TranslatedCPython/release/TranslatedCPython.dll'
        self.pe.parent.mkdir(parents=True)
        self.pe.write_bytes(b'actual isolated input')
        self.entry = self.products/'obj/ManagedConsumer/release/ManagedConsumer.dll'
        self.entry.parent.mkdir(parents=True)
        self.entry.write_bytes(b'consumer entry')
        self.obj = self.entry.parent/'native/ManagedConsumer.o'
        self.obj.parent.mkdir()
        self.obj.write_bytes(b'native object')
        self.rsp = self.obj.with_suffix('.ilc.rsp')
        self.lines = [str(self.entry), '-o:'+str(self.obj), '-r:'+str(self.pe)]
        self.write_response()
        self.executable = self.output/'ManagedConsumer'
        self.executable.write_bytes(b'\x7fELFsynthetic binding control only')
        self.cell = {'mode':'aot', 'output':str(self.output), 'build_products':str(self.products),
                     'compiled_artifacts':{str(self.executable):audit.digest(self.executable)}}

    def write_response(self):
        self.rsp.write_text('\n'.join(self.lines)+'\n')

    def test_uses_response_input_instead_of_adjacent_jit_copy(self):
        (self.output/'TranslatedCPython.dll').write_bytes(b'different JIT bytes')
        binding = consumer.compilation_input(self.cell, 'ManagedConsumer')
        self.assertEqual(binding['sha256'], audit.digest(self.pe))
        self.assertEqual(binding['pe'], str(self.pe))
        self.assertEqual(binding['ilc_response_sha256'], audit.digest(self.rsp))

    def test_missing_response_cannot_inherit_jit_proof(self):
        self.rsp.unlink()
        (self.output/'TranslatedCPython.dll').write_bytes(b'canonical-only proof')
        with self.assertRaisesRegex(RuntimeError, 'ILC response'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_duplicate_response_rejected(self):
        duplicate = self.products/'stale'/self.rsp.name
        duplicate.parent.mkdir()
        duplicate.write_bytes(self.rsp.read_bytes())
        with self.assertRaisesRegex(RuntimeError, 'ambiguous'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_another_entry_response_does_not_replace_selected_entry(self):
        self.rsp.with_name('OtherEntry.ilc.rsp').write_text('unrelated entry control\n')
        actual = consumer.compilation_input(self.cell, 'ManagedConsumer')
        self.assertEqual(actual['ilc_response'], str(self.rsp))

    def test_duplicate_product_reference_rejected(self):
        self.lines.append('-r:'+str(self.pe))
        self.write_response()
        with self.assertRaisesRegex(RuntimeError, 'product input'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_foreign_existing_product_rejected(self):
        foreign = self.root/'canonical/TranslatedCPython.dll'
        foreign.parent.mkdir()
        foreign.write_bytes(self.pe.read_bytes())
        self.lines[-1] = '-r:'+str(foreign)
        self.write_response()
        with self.assertRaisesRegex(RuntimeError, 'outside build'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_missing_actual_native_object_rejected(self):
        self.obj.unlink()
        with self.assertRaisesRegex(RuntimeError, 'entry/object'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_missing_nonproduct_ilc_reference_rejected(self):
        self.lines.append('-r:'+str(self.root/'missing-framework.dll'))
        self.write_response()
        with self.assertRaisesRegex(RuntimeError, 'reference closure'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_duplicate_nonproduct_ilc_reference_rejected(self):
        framework = self.root/'System.Control.dll'
        framework.write_bytes(b'synthetic reference')
        self.lines += ['-r:'+str(framework)]*2
        self.write_response()
        with self.assertRaisesRegex(RuntimeError, 'reference closure'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_all_ilc_references_participate_in_simultaneous_binding(self):
        framework = self.root/'System.Control.dll'
        framework.write_bytes(b'synthetic reference')
        self.lines.append('-r:'+str(framework))
        self.write_response()
        before = consumer.compilation_input(self.cell, 'ManagedConsumer')
        framework.write_bytes(b'changed reference')
        after = consumer.compilation_input(self.cell, 'ManagedConsumer')
        self.assertNotEqual(before,after)

    def test_changed_delivered_executable_rejected(self):
        self.executable.write_bytes(b'\x7fELFreplacement')
        with self.assertRaisesRegex(RuntimeError, 'changed since build'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_whole_library_roots_do_not_qualify_trimmed_consumer(self):
        self.lines += ['--root:TranslatedCPython','--root:Managed.CPython']
        self.write_response()
        with self.assertRaisesRegex(RuntimeError, 'unexpectedly roots'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')

    def test_jit_input_must_still_match_build_snapshot(self):
        pe = self.output/'TranslatedCPython.dll'
        pe.write_bytes(b'JIT actual input')
        entry = self.output/'ManagedConsumer.dll'
        entry.write_bytes(b'JIT entry')
        self.cell.update(mode='jit', compiled_artifacts={str(pe):audit.digest(pe),str(entry):audit.digest(entry)})
        self.assertEqual(consumer.compilation_input(self.cell, 'ManagedConsumer')['sha256'], audit.digest(pe))
        pe.write_bytes(b'stale build replacement')
        with self.assertRaisesRegex(RuntimeError, 'changed since build'):
            consumer.compilation_input(self.cell, 'ManagedConsumer')


if __name__ == '__main__':
    unittest.main()
