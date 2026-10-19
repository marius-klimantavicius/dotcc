"""D10 evidence rejection controls; these fixtures execute no interpreter."""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('d10_audit_control', Path(__file__).with_name('audit_product.py'))
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


class D10Rejections(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='cpython-d10-rejection-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.directory = self.root/'product'
        self.directory.mkdir()
        self.project = self.directory/'TranslatedCPython.csproj'
        self.project.write_text('<Project />')
        self.receipt = self.root/'receipt.json'
        self.receipt.write_text('{"run_id":"rejection-control-only"}')
        self.data = b'A\0B\0'
        self.sha = hashlib.sha256(self.data).hexdigest()
        self.name = 'B'+self.sha.upper()
        self.source = self.directory/'CPython.cs'
        self.source.write_text('internal static global::System.ReadOnlySpan<byte> '+self.name+' => [0x41, 0x00, 0x42, 0x00];\n'
                               'ref pyruntimestate __dotcc_address0 = ref Globals._PyRuntime;\n'
                               'fixed (pyruntimestate* __dotcc_staticBase = &Globals._PyRuntime) {}\n')
        (self.directory/'Dotcc.SourceFiles.txt').write_text('CPython.cs\n')
        (self.directory/'.campaign-files.json').write_text(json.dumps(['CPython.cs', self.project.name, 'Dotcc.SourceFiles.txt']))
        self.binary = self.root/'TranslatedCPython.dll'
        self.binary.write_bytes(b'negative-control-marker; never a real PE')
        self.identity = {'receipt': str(self.receipt), 'receipt_sha256': audit.digest(self.receipt),
                         'project': str(self.project), 'form': 'raw', 'files': {'CPython.cs': audit.digest(self.source),
                         self.project.name: audit.digest(self.project)}}
        self.inventory = {'translationReceipt': str(self.receipt), 'receiptSha256': audit.digest(self.receipt),
            'source': str(self.directory), 'files': [{'path': str(self.source), 'sha256': audit.digest(self.source)}],
            'properties': [{'name': self.name, 'length': len(self.data), 'sha256': self.sha}],
            'totals': {'tracked_address_locals': 1, 'pinned_initializer_chunks': 1}}
        self.inventory_path = self.root/'inventory.json'
        self.il_path = self.root/'il.json'
        self.il = {'status': 'passed', 'assembly': str(self.binary), 'assemblySha256': audit.digest(self.binary),
                   'manifest': str(self.inventory_path), 'newarr': 0, 'propertyCount': 1, 'getterCallSites': 1,
                   'properties': [{'name': 'get_'+self.name, 'length': len(self.data), 'sha256': self.sha}]}
        self.document = {'assembly': 'TranslatedCPython', 'path': str(self.binary), 'sha256': audit.digest(self.binary),
                         'mvid': 'control-only-no-interpreter-claim', 'methods': [{
                         'name': 'get_'+self.name, 'type': 'Managed.Interpreters.CPython+CPythonLiterals',
                         'attributes': 0x10, 'newarr_count': 0, 'il_sha256': 'control-only', 'il_bytes': 12}]}

    def compiled(self):
        self.inventory_path.write_text(json.dumps(self.inventory))
        self.il['manifestSha256'] = audit.digest(self.inventory_path)
        self.il_path.write_text(json.dumps(self.il))
        return audit.d10_compiled(self.identity, self.inventory_path, self.il_path, self.document)

    def test_unrelated_receipt_rejected(self):
        self.inventory['receiptSha256'] = '0'*64
        with self.assertRaisesRegex(audit.AuditError, 'receipt mismatch'):
            self.compiled()

    def test_source_omission_rejected(self):
        self.inventory['files'].clear()
        with self.assertRaisesRegex(audit.AuditError, 'source inventory'):
            self.compiled()

    def test_fresh_source_needs_fresh_proof(self):
        self.source.write_text(self.source.read_text()+'// changed after snapshot\n')
        with self.assertRaisesRegex(audit.AuditError, 'stale source inventory'):
            self.compiled()

    def test_spaced_allocating_payload_is_detected(self):
        self.source.write_text(self.source.read_text()+'System.MemoryExtensions.AsSpan ( new byte [ ] {1, 0} ).CopyTo(dst);')
        self.identity['files']['CPython.cs'] = audit.digest(self.source)
        self.inventory['files'][0]['sha256'] = audit.digest(self.source)
        with self.assertRaisesRegex(audit.AuditError, 'remaining allocating byte payload'):
            self.compiled()

    def test_other_byte_array_spelling_needs_review(self):
        self.source.write_text(self.source.read_text()+'Copy(new byte[] { 65, 0, 66, 0 });\n')
        self.identity['files']['CPython.cs'] = audit.digest(self.source)
        self.inventory['files'][0]['sha256'] = audit.digest(self.source)
        with self.assertRaisesRegex(audit.AuditError, 'unreviewed remaining byte allocation'):
            self.compiled()

    def test_wrong_payload_hash_rejected(self):
        self.inventory['properties'][0]['sha256'] = '0'*64
        with self.assertRaisesRegex(audit.AuditError, 'payload inventory mismatch'):
            self.compiled()

    def test_another_assembly_cannot_supply_product_proof(self):
        self.document['assembly'] = 'AllocationProbe'
        with self.assertRaisesRegex(audit.AuditError, 'product PE/MVID'):
            self.compiled()

    def test_assembly_replaced_after_inspection_rejected(self):
        self.binary.write_bytes(b'replaced PE marker')
        with self.assertRaisesRegex(audit.AuditError, 'compiled PE mismatch'):
            self.compiled()

    def test_missing_mvid_rejected(self):
        self.document.pop('mvid')
        with self.assertRaisesRegex(audit.AuditError, 'product PE/MVID'):
            self.compiled()

    def test_newarr_in_getter_rejected(self):
        self.document['methods'][0]['newarr_count'] = 1
        with self.assertRaisesRegex(audit.AuditError, 'allocating getter'):
            self.compiled()

    def test_same_named_getter_in_wrong_type_rejected(self):
        self.document['methods'][0]['type'] = 'Unrelated.Probe'
        with self.assertRaisesRegex(audit.AuditError, 'wrong-owner'):
            self.compiled()

    def test_unused_getter_rejected(self):
        self.il['getterCallSites'] = 0
        with self.assertRaisesRegex(audit.AuditError, 'unreferenced compiled getter'):
            self.compiled()

    def test_terminator_or_extent_loss_rejected(self):
        self.il['properties'][0]['length'] -= 1
        with self.assertRaisesRegex(audit.AuditError, 'bytes/extent mismatch'):
            self.compiled()

    def test_missing_scope_evidence_rejected(self):
        self.inventory['totals']['pinned_initializer_chunks'] = 0
        with self.assertRaisesRegex(audit.AuditError, 'scoped address reuse'):
            self.compiled()

    def test_lazy_tracked_cache_does_not_require_legacy_byref_spelling(self):
        text = self.source.read_text().replace('ref pyruntimestate __dotcc_address0 = ref Globals._PyRuntime;',
                                              '__DotccGlobalAddressCache __dotcc_address0 = default;')
        self.source.write_text(text)
        self.identity['files']['CPython.cs'] = audit.digest(self.source)
        self.inventory['files'][0]['sha256'] = audit.digest(self.source)
        self.inventory['totals'].update(tracked_address_locals=0, lazy_tracked_address_caches=1)
        result = self.compiled()
        self.assertIn('allocation and semantic gates separate', result['scope'])
        self.assertTrue(any(row['kind'] == 'lazy-byref' for row in result['address_examples']))

    def test_lazy_cache_counter_without_source_declarations_rejected(self):
        self.inventory['totals']['lazy_tracked_address_caches'] = 1
        with self.assertRaisesRegex(audit.AuditError, 'lazy cache source inventory mismatch'):
            self.compiled()

    def test_compiled_report_does_not_close_runtime_gate(self):
        result = self.compiled()
        self.assertIn('allocation and semantic gates separate', result['scope'])
        with self.assertRaisesRegex(audit.AuditError, 'allocation/semantic evidence missing'):
            audit.d10_allocations(result, None)

    def allocation_evidence(self):
        def artifact(name, data):
            path = self.root/name
            path.write_bytes(data)
            return {'path': str(path), 'sha256': audit.digest(path)}
        result = {'receipt_sha256': self.identity['receipt_sha256'],
                  'source_files': {str(self.source): audit.digest(self.source)}, 'allocations': {}}
        for mode in ('jit', 'aot'):
            report = {'status': 'passed', 'dynamicCode': mode == 'jit', 'properties': [{
                'name': self.name, 'sha256': self.sha, 'length': len(self.data), 'allocated': 0, 'iterations': 10000}]}
            result['allocations'][mode] = {key: artifact(mode+'-'+key, json.dumps(report).encode() if key == 'report' else b'control')
                                          for key in ('report', 'program', 'project', 'binary')}
            result['allocations'][mode]['project'] = artifact(mode+'-project',
                ('<Project><ItemGroup><ProjectReference Include="'+str(self.project)+'" /></ItemGroup></Project>').encode())
            self.inventory_path.write_text(json.dumps(self.inventory))
            result['allocations'][mode]['probe_inputs'] = artifact(mode+'-inputs', json.dumps({
                'project': str(self.project), 'project_sha256': audit.digest(self.project),
                'program_sha256': result['allocations'][mode]['program']['sha256'],
                'probe_project_sha256': result['allocations'][mode]['project']['sha256'],
                'inventory': str(self.inventory_path), 'inventory_sha256': audit.digest(self.inventory_path)}).encode())
        result['semantics'] = {'inputs': [artifact('semantic-source', b'control')],
            'commands': [artifact('semantic-command', b'{"exit":1,"argv":["control"],"log":"missing"}')],
            'categories': ['native']}
        return result

    def change_report(self, evidence, mode, transform):
        entry = evidence['allocations'][mode]['report']
        path = Path(entry['path'])
        report = json.loads(path.read_text())
        transform(report)
        path.write_text(json.dumps(report))
        entry['sha256'] = audit.digest(path)

    def test_reduced_allocation_source_cannot_qualify_product(self):
        evidence = self.allocation_evidence()
        evidence['source_files'] = {}
        with self.assertRaisesRegex(audit.AuditError, 'allocation source closure'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_nonzero_warmed_allocation_rejected(self):
        evidence = self.allocation_evidence()
        self.change_report(evidence, 'jit', lambda row: row['properties'][0].update(allocated=24))
        with self.assertRaisesRegex(audit.AuditError, 'warmed payload allocation'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_jit_cannot_stand_in_for_nativeaot(self):
        evidence = self.allocation_evidence()
        self.change_report(evidence, 'aot', lambda row: row.update(dynamicCode=True))
        with self.assertRaisesRegex(audit.AuditError, 'execution mode mismatch'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_allocation_probe_must_cover_actual_payloads(self):
        evidence = self.allocation_evidence()
        self.change_report(evidence, 'aot', lambda row: row['properties'].clear())
        with self.assertRaisesRegex(audit.AuditError, 'property closure mismatch'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_failed_semantic_command_is_not_evidence(self):
        with self.assertRaisesRegex(audit.AuditError, 'did not execute successfully'):
            audit.d10_allocations(self.compiled(), self.allocation_evidence())

    def test_probe_without_product_reference_rejected(self):
        evidence = self.allocation_evidence()
        entry = evidence['allocations']['jit']['project']
        Path(entry['path']).write_text('<Project />')
        entry['sha256'] = audit.digest(entry['path'])
        with self.assertRaisesRegex(audit.AuditError, 'reference actual product project'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_probe_input_hash_mismatch_rejected(self):
        evidence = self.allocation_evidence()
        entry = evidence['allocations']['jit']['probe_inputs']
        inputs = audit.read(entry['path'])
        inputs['program_sha256'] = '0'*64
        Path(entry['path']).write_text(json.dumps(inputs))
        entry['sha256'] = audit.digest(entry['path'])
        with self.assertRaisesRegex(audit.AuditError, 'probe input mismatch'):
            audit.d10_allocations(self.compiled(), evidence)

    def test_missing_final_address_review_cannot_close_d10(self):
        with self.assertRaisesRegex(audit.AuditError, 'qualification report missing'):
            audit.d10_address_qualification(self.compiled(), {})

    def test_partial_address_review_with_generic_call_limits_cannot_close_d10(self):
        path = self.root/'address-review.json'
        path.write_text(json.dumps({'receipt': 'rejection-control-only',
            'receipt_sha256': audit.digest(self.receipt),
            'scope': 'representative cases only', 'remainingConservativeLimits': ['all calls invalidate']}))
        with self.assertRaisesRegex(audit.AuditError, 'address reuse remains unqualified'):
            audit.d10_address_qualification(self.compiled(), {'address_review': {'path': str(path), 'sha256': audit.digest(path)}})


if __name__ == '__main__':
    unittest.main(verbosity=2)
