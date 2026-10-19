"""Pure rejection controls. Synthetic PE markers never qualify a product.

These compare independent source/metadata/report closures. The NativeOracle
controls separately compile and inspect real IL once the actual lane is safe.
"""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


HERE = Path(__file__).resolve().parent
audit = module('typed_control_audit', HERE/'audit_product.py')
preparation = module('typed_control_preparation', HERE.parents[1]/'scripts/generated_code_evidence.py')
OWNER = 'Managed.Interpreters.CPython'
HOLDER = OWNER+'+CPythonLiterals'


class TypedDataControls(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='cpython-typed-rejection-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.directory = self.root/'product'
        self.directory.mkdir()
        self.project = self.directory/'TranslatedCPython.csproj'
        self.project.write_text('<Project />')
        self.receipt = self.root/'receipt.json'
        self.receipt.write_text('{"run_id":"synthetic-rejection-controls"}')
        self.byte_data = b'A\0B\0'
        self.bits = bytes.fromhex('00000080010000004200c07f0000807f')
        self.byte_name = 'B'+hashlib.sha256(self.byte_data).hexdigest().upper()
        self.bits_name = 'Puint_'+hashlib.sha256(self.bits).hexdigest().upper()
        self.view = '__DotccPrimitiveView_'+'A'*64
        self.source = self.directory/'CPython.cs'
        self.source.write_text('internal static global::System.ReadOnlySpan<byte> '+self.byte_name+' => [0x41, 0x00, 0x42, 0x00];\n'
            'internal static global::System.ReadOnlySpan<uint> '+self.bits_name+' => [0x80000000U, 0x00000001U, 0x7FC00042U, 0x7F800000U];\n'
            'internal static class '+self.view+' { public static global::System.ReadOnlySpan<float> Values => '
            'global::System.Runtime.InteropServices.MemoryMarshal.Cast<uint, float>(DotCcLiterals.'+self.bits_name+'); }\n'
            'ref RuntimeState __dotcc_address0 = ref Globals._PyRuntime;\n'
            'fixed (RuntimeState* __dotcc_staticBase = &Globals._PyRuntime) {}\n')
        (self.directory/'Dotcc.SourceFiles.txt').write_text('CPython.cs\n')
        self.inventory_path = self.root/'inventory.json'
        self.inventory = preparation.prepare_typed(self.directory, self.receipt, self.inventory_path,
            holder=OWNER+'.CPythonLiterals', owner=OWNER)
        self.identity = {'project':str(self.project), 'receipt':str(self.receipt), 'receipt_sha256':audit.digest(self.receipt),
            'form':'raw', 'files':{'CPython.cs':audit.digest(self.source), self.project.name:audit.digest(self.project)}}
        self.pe = self.root/'TranslatedCPython.dll'
        self.pe.write_bytes(b'synthetic-marker; never an actual assembly or D10 pass')
        self.il_path = self.root/'il.json'
        self.document = {'assembly':'TranslatedCPython', 'path':str(self.pe), 'sha256':audit.digest(self.pe),
            'mvid':'synthetic-only', 'methods':[], 'fields':[], 'value_types':[]}
        def method(token, owner, name, calls=()):
            return {'token':token, 'type':owner, 'name':name, 'attributes':0x10, 'il_bytes':12,
                'il_sha256':hashlib.sha256((owner+'.'+name).encode()).hexdigest(), 'newarr_count':0,
                'call_sites':list(calls), 'field_sites':[]}
        def call(offset, token, owner, name):
            return {'offset':offset, 'target_token':token, 'target_type':owner, 'target_method':name, 'opcode':'call'}
        self.document['methods'] = [method(1,HOLDER,'get_'+self.byte_name), method(2,HOLDER,'get_'+self.bits_name),
            method(3,OWNER+'+'+self.view,'get_Values',[call(0,2,HOLDER,'get_'+self.bits_name),
                call(5,0,'System.Runtime.InteropServices.MemoryMarshal','Cast')]),
            method(4,OWNER,'Fill',[call(0,1,HOLDER,'get_'+self.byte_name),call(5,3,OWNER+'+'+self.view,'get_Values')]),
            method(5,OWNER,'__dotcc_owned_array_0',[call(0,6,OWNER,'OwnedFill')]),method(6,OWNER,'OwnedFill')]
        self.methods = {m['token']:m for m in self.document['methods']}
        observed = []
        for token, row in enumerate(self.inventory['properties'], 1):
            observed.append(dict(row, name='get_'+row['name'], token=token,
                signature='System.ReadOnlySpan`1<'+row['elementType']+'>', rva=1024*token, fieldToken=100+token,
                fieldExtent=row['length'], ilBytes=12, ilSha256=self.methods[token]['il_sha256'], newarr=0))
        view = dict(self.inventory['typedViews'][0], token=3, signature='System.ReadOnlySpan`1<float>',
            ilBytes=12, ilSha256=self.methods[3]['il_sha256'], newarr=0, conversions=0)
        self.il = {'status':'passed', 'schemaVersion':2, 'auditRevision':3, 'assembly':str(self.pe),
            'assemblySha256':audit.digest(self.pe), 'mvid':'synthetic-only', 'manifest':str(self.inventory_path),
            'newarr':0, 'propertyCount':2, 'viewCount':1, 'rvaBytes':20, 'getterCallSites':3,
            'properties':observed, 'views':[view], 'transitiveCopyClosure':{
                'roots':[3,4,5], 'methods':[{'token':m['token'], 'name':m['type']+'.'+m['name'],
                    'ilBytes':12, 'ilSha256':m['il_sha256'], 'newarr':0} for m in self.methods.values()],
                'edges':[{'callerToken':m['token'], 'offset':c['offset'], 'targetToken':c['target_token']}
                    for m in self.methods.values() for c in m['call_sites'] if c['target_token']],
                'initializerEdges':[], 'trackedAddressCallbacks':[], 'trackedAcquireTargets':[],
                'requiredDestinationBoundaries':[], 'requiredPinnedPointerPoolAllocations':[],
                'externalCallSites':[{'callerToken':3,'offset':5,'target':'System.Runtime.InteropServices.MemoryMarshal.Cast'}]}}

    def compiled(self):
        self.inventory_path.write_text(json.dumps(self.inventory))
        self.il['manifestSha256'] = audit.digest(self.inventory_path)
        self.il_path.write_text(json.dumps(self.il))
        return audit.d10_compiled(self.identity, self.inventory_path, self.il_path, self.document)

    def rejected(self, message):
        with self.assertRaisesRegex(audit.AuditError, message):
            self.compiled()

    def test_consumer_checks_synthetic_closures_without_runtime_claim(self):
        result = self.compiled()
        self.assertEqual((2,1,6), (result['properties'],result['views'],result['transitive_methods']))
        self.assertIn('allocation and semantic gates separate',result['scope'])
        with self.assertRaisesRegex(audit.AuditError, 'allocation/semantic evidence missing'):
            audit.d10_allocations(result, None)

    def test_omitted_source_backing_rejected(self):
        self.inventory['properties'].pop()
        self.rejected('typed source inventory mismatch')

    def test_omitted_typed_view_rejected(self):
        self.inventory['typedViews'].clear()
        self.rejected('typed source inventory mismatch')

    def test_wrong_source_view_width_rejected(self):
        self.inventory['typedViews'][0]['elementWidth'] = 8
        self.rejected('typed source inventory mismatch')

    def test_exact_float_bits_cannot_be_changed(self):
        self.il['properties'][1]['sha256'] = hashlib.sha256(self.bits.replace(b'\x42',b'\x43')).hexdigest()
        self.rejected('typed width/bytes/extent mismatch')

    def test_wrong_view_type_rejected(self):
        self.il['views'][0]['elementType'] = 'int'
        self.rejected('wrong typed view/bits')

    def test_wrong_view_backing_rejected(self):
        self.il['views'][0]['backing'] = self.byte_name
        self.rejected('wrong typed view/bits')

    def test_view_conversion_rejected(self):
        self.il['views'][0]['conversions'] = 1
        self.rejected('wrong typed view/bits')

    def test_rva_extent_loss_rejected(self):
        self.il['properties'][0]['fieldExtent'] -= 1
        self.rejected('RVA backing signature/extent')

    def test_non_rva_backing_rejected(self):
        self.il['properties'][0]['rva'] = 0
        self.rejected('RVA backing signature/extent')

    def test_hidden_transitive_newarr_rejected(self):
        self.methods[6]['newarr_count'] = 1
        self.rejected('allocating/stale transitive method')

    def test_helper_array_cannot_be_omitted(self):
        self.il['transitiveCopyClosure']['methods'].pop()
        self.rejected('missing transitive copy/owned-helper method')

    def test_owned_helper_root_cannot_be_omitted(self):
        self.il['transitiveCopyClosure']['roots'].remove(5)
        self.rejected('missing copy/owned-helper root')

    def test_transitive_edge_cannot_be_omitted(self):
        self.il['transitiveCopyClosure']['edges'].pop()
        self.rejected('transitive local edge closure')

    def test_hidden_array_constructor_rejected(self):
        self.methods[6]['call_sites'] = [{'offset':0, 'opcode':'newobj', 'target_token':0,
            'target_type':'byte[,]', 'target_method':'.ctor'}]
        self.rejected('array/object/delegate construction')

    def test_equivalent_helper_toarray_rejected(self):
        self.methods[6]['call_sites'] = [{'offset':0, 'opcode':'call', 'target_token':0,
            'target_type':'System.Linq.Enumerable', 'target_method':'ToArray'}]
        self.rejected('equivalent temporary construction')

    def test_implicit_helper_type_initializer_cannot_be_omitted(self):
        self.methods[6]['field_sites'] = [{'target_type':OWNER+'+HiddenCache'}]
        self.document['methods'].append(dict(self.methods[6], token=7, type=OWNER+'+HiddenCache', name='.cctor'))
        self.rejected('missing transitive copy/owned-helper method')

    def test_required_destination_is_not_a_payload_allocation(self):
        self.methods[6]['call_sites'] = [{'offset':0,'opcode':'call','target_token':7,
            'target_type':OWNER+'+Libc','target_method':'GlobalArrayZeroed'}]
        self.document['methods'].append(dict(self.methods[6], token=7, type=OWNER+'+Libc', name='GlobalArrayZeroed'))
        self.il['transitiveCopyClosure']['requiredDestinationBoundaries'] = [{'callerToken':6,'offset':0,
            'target':OWNER+'+Libc.GlobalArrayZeroed','reason':'independently writable C final storage'}]
        self.assertEqual(len(self.compiled()['required_c_destinations']),1)

    def test_generic_libc_suffix_cannot_create_boundary(self):
        self.methods[6]['call_sites'] = [{'offset':0,'opcode':'call','target_token':7,
            'target_type':'Unreviewed.Libc','target_method':'GlobalArrayZeroed'}]
        self.document['methods'].append(dict(self.methods[6], token=7, type='Unreviewed.Libc', name='GlobalArrayZeroed'))
        self.rejected('missing transitive copy/owned-helper method')

    def test_stale_source_rejected(self):
        self.source.write_text(self.source.read_text()+'// edited after snapshot\n')
        self.rejected('stale source inventory')

    def test_wrong_actual_pe_rejected(self):
        self.pe.write_bytes(b'wrong actual PE marker')
        self.rejected('compiled PE mismatch')

    def test_wrong_actual_mvid_rejected(self):
        self.il['mvid'] = 'another-consumer'
        self.rejected('PE/MVID mismatch')

    def test_old_report_cannot_inherit_new_closure_guards(self):
        self.il.pop('auditRevision')
        self.rejected('obsolete')

    def test_transitive_il_hash_must_match_independent_pe_reader(self):
        self.methods[6]['il_sha256'] = '0'*64
        self.rejected('allocating/stale transitive method')

    def test_missing_actual_data_evidence_is_required_failure(self):
        result = self.compiled()
        evidence = {'receipt_sha256':self.identity['receipt_sha256'],
            'source_files':{str(self.source):audit.digest(self.source)}}
        with self.assertRaisesRegex(audit.AuditError, 'typed data proof'):
            audit.d10_allocations(result,evidence)

    def cache_callback(self):
        self.methods[6]['call_sites'] = [
            {'offset':0,'opcode':'ldftn','target_token':7,'target_type':OWNER,'target_method':'Acquire'},
            {'offset':6,'opcode':'call','target_token':8,'target_type':OWNER+'+__DotccGlobalAddressCache','target_method':'Get'}]
        for token,owner,name in [(7,OWNER,'Acquire'),(8,OWNER+'+__DotccGlobalAddressCache','Get')]:
            method=dict(self.methods[6],token=token,type=owner,name=name,call_sites=[],field_sites=[])
            self.document['methods'].append(method);self.methods[token]=method
            self.il['transitiveCopyClosure']['methods'].append({'token':token,'name':owner+'.'+name,
                'ilBytes':12,'ilSha256':method['il_sha256'],'newarr':0})
        self.il['transitiveCopyClosure']['edges'] += [{'callerToken':6,'offset':0,'targetToken':7},
            {'callerToken':6,'offset':6,'targetToken':8}]
        self.il['transitiveCopyClosure']['trackedAcquireTargets']=[{'callerToken':6,'offset':6,
            'pointerOffset':0,'cacheToken':8,'targetToken':7}]

    def test_actual_acquire_target_cannot_be_omitted(self):
        self.cache_callback()
        self.il['transitiveCopyClosure']['trackedAcquireTargets'].clear()
        self.rejected('unbound tracked-ref acquire target')

    def test_hidden_allocation_in_exact_cache_acquire_closure_rejected(self):
        self.cache_callback();self.methods[7]['newarr_count']=1
        self.rejected('allocating/stale transitive method')

    def test_acquire_target_cannot_be_substituted(self):
        self.cache_callback();self.il['transitiveCopyClosure']['trackedAcquireTargets'][0]['targetToken']=8
        self.rejected('unbound tracked-ref acquire target')

    def test_data_subgate_keeps_same_pe_lifetime_gate_mandatory(self):
        self.assertTrue(self.compiled()['requires_same_pe_address_lifetime_qualification'])


if __name__ == '__main__':
    unittest.main()
