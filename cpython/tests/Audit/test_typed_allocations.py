"""Pure executable-evidence rejection controls; never product qualification.

Only the separately tested actual PE/PDB input mapper is mocked. These controls
exercise the schema-2 consumer's real file hashes, project references, report
closure, execution records and native transcript checks without subprocesses.
"""
import copy
import importlib.util
import json
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch


HERE = Path(__file__).resolve().parent


def module(name):
    spec = importlib.util.spec_from_file_location('allocation_control_'+name, HERE/(name+'.py'))
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


audit = module('audit_product')
typed = module('typed_data')


class TypedAllocationEvidenceControls(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='cpython-typed-allocation-rejection-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.product = self.file('TranslatedCPython.dll', 'synthetic product; no actual PE')
        self.product['mvid'] = 'synthetic-product-only'
        self.project = self.file('TranslatedCPython.csproj', '<Project />')
        row = {'name':'bits', 'length':4, 'elementWidth':4, 'sha256':'a'*64}
        self.inventory = {'schemaVersion':2, 'receiptSha256':'synthetic-receipt',
                          'files':[], 'properties':[row], 'typedViews':[dict(row, name='view')]}
        inventory = self.json('inventory.json', self.inventory)
        self.compiled = {'inventory':inventory['path'], 'assembly_sha256':self.product['sha256'],
                         'mvid':self.product['mvid'], 'identity':{'project':self.project['path'],
                         'receipt_sha256':'synthetic-receipt',
                         'files':{'TranslatedCPython.csproj':self.project['sha256']}}}
        self.inputs = {'project':self.project, 'reference':self.product, 'inventory':inventory,
                       'program':self.file('Program.cs', '// synthetic'),
                       'productTables':self.file('ProductTables.cs', '// synthetic')}
        self.project_text = ('<Project><PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
            '</PropertyGroup><ItemGroup><Reference Include="TranslatedCPython"><HintPath>'+self.product['path']+
            '</HintPath></Reference><Compile Include="Program.cs" /><Compile Include="ProductTables.cs" />'
            '</ItemGroup></Project>')
        self.inputs['probeProject'] = self.file('Probe.csproj', self.project_text)
        native_sizes = {'_PyLong_DigitValue':256, '_Py_ctype_table':1024, '_Py_ctype_tolower':256,
                        '_Py_ctype_toupper':256, '_PyUnicode_ExtendedCase':4944}
        table_data = {name:bytes([index])*length for index,(name,length) in enumerate(native_sizes.items(),1)}
        transcript = self.file('native.txt', ''.join(name+' '+str(len(data))+' '+data.hex()+'\n'
                                                   for name,data in table_data.items()))
        source = self.file('native-source.c', '// same synthetic source')
        self.proof = {'schemaVersion':2, 'product':self.product, 'inventory':inventory,
                      'probeInputs':self.json('probe-inputs.json', self.inputs),
                      'nativeOracle':{'transcript':transcript, 'source':source,
                        'archive':self.file('libsynthetic.a', 'not a native build'),
                        'actualSourceBytes':[{'original':source, 'actualNativeSource':source}]}}
        self.reports, self.commands = {}, {}
        for mode in ('jit','aot'):
            entry_pe = self.file(mode+'/Probe.dll', 'synthetic '+mode+' entry')
            executable = entry_pe if mode == 'jit' else self.file(mode+'/Probe', '\x7fELFsynthetic')
            report = {'status':'passed', 'dynamicCode':mode == 'jit', 'productMvid':self.product['mvid'],
                      'properties':[dict(row, width=4, allocated=0, iterations=10000)],
                      'views':[dict(row, name='view', width=4, allocated=0, iterations=10000)],
                      'nativeTables':[{'name':name, 'length':len(data), 'sha256':typed.hashlib.sha256(data).hexdigest(),
                                       'nativeBytes':True, 'address':hex(0x10000+index*0x10000)}
                                      for index,(name,data) in enumerate(table_data.items())]}
            self.reports[mode] = report
            report_item = self.json(mode+'/report.json',report)
            argv = (['dotnet',entry_pe['path']] if mode == 'jit' else [executable['path']])
            argv += [report_item['path'],transcript['path']]
            command = {'exitCode':0, 'status':'passed', 'timedOut':False, 'argv':argv,
                       'reportSha256':report_item['sha256'], 'executableSha256':executable['sha256'],
                       'stdout':self.file(mode+'/stdout','synthetic result\n'),
                       'stderr':self.file(mode+'/stderr','')}
            self.commands[mode] = command
            self.proof[mode] = {'report':report_item, 'execution':self.json(mode+'/execution.json',command),
                                'entry':{'pe':entry_pe}, 'executable':executable,
                                'productReference':dict(self.product)}
        self.input_validator = Mock()

    def file(self, name, content):
        path = self.root/name
        path.parent.mkdir(parents=True,exist_ok=True)
        path.write_text(content)
        return {'path':str(path), 'sha256':typed.digest(path)}

    def json(self, name, value):
        return self.file(name,json.dumps(value))

    def reseal(self, mode):
        self.proof[mode]['report'] = self.json(mode+'/report.json',self.reports[mode])
        self.commands[mode]['reportSha256'] = self.proof[mode]['report']['sha256']
        self.proof[mode]['execution'] = self.json(mode+'/execution.json',self.commands[mode])

    def consume(self):
        evidence = {'typed_data':self.json('proof.json',self.proof)}
        with patch.object(typed,'load',return_value=SimpleNamespace(validate=self.input_validator)):
            return typed.allocations(audit.require,self.compiled,evidence)

    def reject(self, message):
        with self.assertRaisesRegex(audit.AuditError,message):
            self.consume()

    def test_structural_evidence_still_delegates_each_actual_input_binding(self):
        self.assertEqual({'jit','aot'},set(self.consume()))
        self.assertEqual(2,self.input_validator.call_count)
        self.assertEqual(['jit','aot'],[call.args[-1] for call in self.input_validator.call_args_list])

    def test_changed_report_cannot_be_relabelled_as_prior_execution(self):
        Path(self.proof['jit']['report']['path']).write_text('{}')
        self.reject('stale/missing typed jit report')

    def test_resealed_reports_must_cover_exact_typed_bits_widths_and_warm_measurement(self):
        original = copy.deepcopy(self.reports['jit'])
        mutations = [lambda r:r['views'].clear(),
                     lambda r:r['properties'].append(dict(r['properties'][0])),
                     lambda r:r['views'][0].update(width=8),
                     lambda r:r['views'][0].update(sha256='b'*64),
                     lambda r:r['views'][0].update(allocated=1),
                     lambda r:r['properties'][0].update(iterations=9999),
                     lambda r:r.update(dynamicCode=False),
                     lambda r:r.update(productMvid='other')]
        for mutate in mutations:
            with self.subTest(mutation=mutations.index(mutate)):
                self.reports['jit'] = copy.deepcopy(original)
                mutate(self.reports['jit'])
                self.reseal('jit')
                self.reject('property/view closure|payload allocation|mode/product MVID')

    def test_execution_must_bind_actual_result_and_executable(self):
        original = copy.deepcopy(self.commands['aot'])
        mutations = [lambda c:c.update(exitCode=1), lambda c:c.update(timedOut=True),
                     lambda c:c.update(status='unrun'), lambda c:c.update(executableSha256='b'*64),
                     lambda c:c['argv'].__setitem__(0,str(self.root/'other-native-executable'))]
        for mutate in mutations:
            with self.subTest(mutation=mutations.index(mutate)):
                self.commands['aot'] = copy.deepcopy(original)
                mutate(self.commands['aot'])
                self.reseal('aot')
                self.reject('execution/result binding')

    def test_canonical_other_bytes_cannot_supply_selected_product(self):
        self.proof['aot']['productReference'] = dict(self.file('canonical/TranslatedCPython.dll','other bytes'),
                                                   mvid=self.product['mvid'])
        self.reject('selected probe product mismatch')

    def test_project_reference_cannot_rebuild_selected_product(self):
        self.inputs['probeProject'] = self.file('Probe.csproj',self.project_text.replace(
            '</ItemGroup>','<ProjectReference Include="TranslatedCPython.csproj" /></ItemGroup>'))
        self.proof['probeInputs'] = self.json('probe-inputs.json',self.inputs)
        self.reject('reference exact actual PE')

    def test_missing_or_aliased_native_table_never_closes_native_gate(self):
        original = copy.deepcopy(self.reports['aot'])
        mutations = [lambda r:r['nativeTables'].pop(),
                     lambda r:r['nativeTables'][1].update(address=r['nativeTables'][0]['address']),
                     lambda r:r['nativeTables'][0].update(nativeBytes=False),
                     lambda r:r['nativeTables'][0].update(sha256='b'*64)]
        for mutate in mutations:
            with self.subTest(mutation=mutations.index(mutate)):
                self.reports['aot'] = copy.deepcopy(original)
                mutate(self.reports['aot'])
                self.reseal('aot')
                self.reject('native byte/identity table mismatch')

    def test_semantic_controls_remain_required_after_typed_execution_gate(self):
        self.compiled['schemaVersion'] = 2
        evidence = {'receipt_sha256':'synthetic-receipt', 'source_files':{},
                    'semantics':{'categories':[], 'inputs':[], 'commands':[]}}
        with patch.object(audit.typed_data(),'allocations',return_value={'jit':{},'aot':{}}):
            with self.assertRaisesRegex(audit.AuditError,'semantic control provenance missing'):
                audit.d10_allocations(self.compiled,evidence)


if __name__ == '__main__':
    unittest.main()
