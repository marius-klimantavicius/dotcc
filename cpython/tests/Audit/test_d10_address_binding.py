"""Rejection controls for final address evidence; no interpreter qualification."""
import json
import copy
import hashlib
from pathlib import Path
import unittest

import test_d10_audit as controls

audit = controls.audit


class AddressBindingRejections(unittest.TestCase):
    def setUp(self):
        self.control = controls.D10Rejections('runTest')
        self.control.setUp()
        self.addCleanup(self.control.doCleanups)

    def review(self, **changes):
        control = self.control
        compiled = control.compiled()
        observation = {'source_files': {str(control.source): audit.digest(control.source)},
                       'unresolved_pure_sites': []}
        document = {'receipt': 'rejection-control-only', 'status': 'qualified',
                    'receipt_sha256': audit.digest(control.receipt),
                    'forms': {'raw': observation}}
        document.update(changes)
        path = control.root/'final-address-review.json'
        path.write_text(json.dumps(document))
        return compiled, document, {'address_review': {'path': str(path), 'sha256': audit.digest(path)}}

    def test_self_declared_qualified_with_empty_analysis_is_rejected(self):
        compiled, _, evidence = self.review()
        with self.assertRaisesRegex(audit.AuditError, 'semantic analysis'):
            audit.d10_address_qualification(compiled, evidence)

    def test_same_run_id_with_wrong_receipt_bytes_is_rejected(self):
        compiled, _, evidence = self.review(receipt_sha256='0'*64)
        with self.assertRaisesRegex(audit.AuditError, 'receipt'):
            audit.d10_address_qualification(compiled, evidence)

    def full_rejection_input(self):
        """Format support only; every caller introduces a required rejection.

        The PE/tool markers below are deliberately not executable assemblies.
        None of these records may establish a product pass.
        """
        c = self.control
        mechanism = ('internal unsafe ref struct __DotccGlobalAddressCache { private ref byte value; '
                     'internal T* Get<T>(delegate* managed<ref T> acquire) where T : unmanaged { '
                     'if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value)) '
                     'value = ref global::System.Runtime.CompilerServices.Unsafe.As<T, byte>(ref acquire()); '
                     'return (T*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref value); } }')
        declaration = '__dotcc_address0 = default'
        getter = 'static ref int __dotcc_address0_Get() => ref Owner.slot;'
        use = '__dotcc_address0.Get<int>(&__dotcc_address0_Get)'
        other = 'global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref local)'
        source = (c.source.read_text()+'// \U0001f600 UTF-16 span control\r\n'+mechanism+'\r\n'
                  '__DotccGlobalAddressCache '+declaration+';\r\n'+getter+'\r\n'+use+';\r\nint local = 0;\r\n'+other+';\r\n')
        c.source.write_bytes(source.encode())
        c.identity['files']['CPython.cs'] = audit.digest(c.source)
        c.inventory['files'][0]['sha256'] = audit.digest(c.source)
        c.inventory['totals']['lazy_tracked_address_caches'] = 1
        compiled, review, evidence = self.review()
        observation = review['forms']['raw']

        def site(expression):
            return {'path': str(c.source), 'span_start': len(source[:source.index(expression)].encode('utf-16-le'))//2,
                    'span_length': len(expression.encode('utf-16-le'))//2, 'expression': expression}

        getter_site, decl_site, use_site = site(getter), site(declaration), site(use)
        storage, region = 'Owner.slot', 'method:control:0'
        cache = dict(getter_site, storage=storage, getter='__dotcc_address0_Get', storage_type='int',
                     declaration_span_start=decl_site['span_start'], declaration_span_length=decl_site['span_length'],
                     declaration_expression=declaration, uses=[use_site])
        grouped = dict(getter_site, Path=str(c.source), Region=region, Storage=storage, Kind='tracked-lazy-cache', Loop=False)
        method = {'name': 'Get', 'signature': 'control-method-signature', 'il_sha256': '1'*64, 'il_bytes': 12}
        field = {'name': 'value', 'signature': '061005'}
        compiled['address_cache_metadata'] = {'fields': [field], 'methods': [method]}
        binary = c.root/'analyzer-marker.dll'
        binary.write_bytes(b'rejection-only; not an analyzer PE')
        analysis_path = c.root/'semantic-analysis.json'
        sources = []
        for resource in ('GeneratedAddressAudit.Program.cs', 'GeneratedAddressAudit.Project'):
            path = c.root/resource
            path.write_text('Rejection-only provenance marker: '+resource)
            sources.append({'resource': resource, 'path': str(path), 'sha256': audit.digest(path)})
        remaining = [site('global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref value)'), site(other)]
        for row, name, kind, declaration_text, ref_kind in (
                (remaining[0], 'value', 'Field', 'ref byte value', 'Ref'),
                (remaining[1], 'local', 'Local', 'local = 0', 'None')):
            facts = {'symbol': name, 'symbol_kind': kind, 'type': 'byte' if kind == 'Field' else 'int',
                     'ref_kind': ref_kind, 'containing_type': 'Support.__DotccGlobalAddressCache',
                     'is_static': False, 'is_volatile': False, 'thread_static': False,
                     'declarations': [site(declaration_text)]}
            offset = row['expression'].index('ref '+name)+4
            part = {'span_start': row['span_start']+offset, 'span_length': len(name), 'expression': name, 'facts': facts}
            row.update(symbol=name, symbol_kind=kind, bound_symbol=facts, bound_references=[part])
        pin_scope = 'fixed (pyruntimestate* __dotcc_staticBase = &Globals._PyRuntime) {}'
        pin = dict(site('pyruntimestate* __dotcc_staticBase = &Globals._PyRuntime'),
                   pin_scope_start=site(pin_scope)['span_start'], pin_scope_length=site(pin_scope)['span_length'])
        analysis = {'receipt': str(c.receipt), 'receipt_sha256': audit.digest(c.receipt),
                    'inventory': str(c.inventory_path), 'inventory_sha256': audit.digest(c.inventory_path),
                    'assembly': str(c.binary), 'assembly_sha256': audit.digest(c.binary), 'mvid': compiled['mvid'],
                    'status': 'passed', 'source_files': observation['source_files'],
                    'semantic_errors': [], 'unknown_cache_bindings': [], 'unresolved_pure_sites': [],
                    'analyzer': {'tool': str(binary), 'tool_sha256': audit.digest(binary), 'mvid': 'rejection-marker',
                        'cwd': str(c.root),
                        'invocation': [str(binary), str(c.inventory_path), str(c.binary), str(analysis_path)],
                        'embedded_sources': {row['resource']: row['sha256'] for row in sources}},
                    'cache_mechanisms': [dict(site(mechanism), valid=True, source_sha256=hashlib.sha256(mechanism.encode()).hexdigest())],
                    'cache_bindings_and_uses': [cache], 'actual_pe_cache_metadata': [{'fields': [field], 'methods': [method]}],
                    'remaining_address_conversions': remaining, 'scoped_initializer_pins': [pin],
                    'unmanaged_static_storage_regions': [{'path': str(c.source), 'region': region, 'storage': storage,
                        'direct': 0, 'caches': 1, 'repeated_loop_direct': 0, 'sites': [grouped]}]}
        observation['analyzer_sources'] = sources
        observation['exceptions'] = [dict(row, source_sha256=audit.digest(c.source), category='tracked-ref-lifetime',
             reason='Generic rejection fixture only; tracks an interior managed reference through a reached conversion.') for row in remaining]
        observation['exceptions'][1]['category'] = 'managed-local'
        observation['analysis'] = {'path': str(analysis_path), 'sha256': None}
        return compiled, review, analysis, evidence

    def reject(self, compiled, review, analysis, evidence, error):
        entry = review['forms']['raw']['analysis']
        Path(entry['path']).write_text(json.dumps(analysis))
        entry['sha256'] = audit.digest(entry['path'])
        item = evidence['address_review']
        Path(item['path']).write_text(json.dumps(review))
        item['sha256'] = audit.digest(item['path'])
        with self.assertRaisesRegex(audit.AuditError, error):
            audit.d10_address_qualification(compiled, evidence)

    def test_analysis_binding_rejections(self):
        baseline = self.full_rejection_input()
        mutations = [
            ('wrong form source map', lambda c,r,a: a.update(source_files={}), 'source closure'),
            ('wrong receipt', lambda c,r,a: a.update(receipt_sha256='0'*64), 'receipt'),
            ('wrong PE', lambda c,r,a: a.update(assembly_sha256='0'*64), 'PE/MVID'),
            ('wrong MVID', lambda c,r,a: a.update(mvid='unrelated'), 'PE/MVID'),
            ('semantic errors', lambda c,r,a: a.update(semantic_errors=['actual diagnostic']), 'remains unqualified'),
            ('unknown binding', lambda c,r,a: a.update(unknown_cache_bindings=['unbound']), 'remains unqualified'),
            ('missing diagnostic inventory', lambda c,r,a: a.pop('semantic_errors'), 'remains unqualified'),
            ('no analysis sites', lambda c,r,a: a.update(unmanaged_static_storage_regions=[]), 'empty semantic analysis'),
            ('no caches', lambda c,r,a: a.update(cache_bindings_and_uses=[]), 'empty semantic analysis'),
            ('tool replaced', lambda c,r,a: a['analyzer'].update(tool_sha256='0'*64), 'analyzer identity'),
            ('different invocation', lambda c,r,a: a['analyzer']['invocation'].__setitem__(2,str(self.control.project)), 'invocation'),
            ('embedded source drift', lambda c,r,a: a['analyzer']['embedded_sources'].update({'GeneratedAddressAudit.Program.cs':'0'*64}), 'source changed'),
            ('missing tool source', lambda c,r,a: r['forms']['raw'].update(analyzer_sources=[]), 'source provenance'),
            ('relative invocation uses recorded cwd', lambda c,r,a: (
                a['analyzer'].update(invocation=[str(Path(path).relative_to(self.control.root)) for path in a['analyzer']['invocation']]),
                a.update(unmanaged_static_storage_regions=[])), 'empty semantic analysis'),
        ]
        for name, mutate, error in mutations:
            with self.subTest(name=name):
                compiled, review, analysis, evidence = copy.deepcopy(baseline)
                mutate(compiled, review, analysis)
                self.reject(compiled, review, analysis, evidence, error)

    def test_source_named_destination_pin_cannot_use_empty_legacy_inventory(self):
        compiled, review, analysis, evidence = self.full_rejection_input()
        c = self.control
        old = '__dotcc_staticBase'
        new = 'x' * (len(old) - len('_destination')) + '_destination'
        c.source.write_bytes(c.source.read_bytes().replace(old.encode(), new.encode()))
        source_sha = audit.digest(c.source)
        c.identity['files'][c.source.name] = source_sha
        c.inventory['files'][0]['sha256'] = source_sha
        c.inventory_path.write_text(json.dumps(c.inventory))
        compiled['inventory_sha256'] = audit.digest(c.inventory_path)
        analysis['inventory_sha256'] = compiled['inventory_sha256']
        analysis['source_files'][str(c.source)] = source_sha
        review['forms']['raw']['source_files'][str(c.source)] = source_sha
        for exception in review['forms']['raw']['exceptions']:
            exception['source_sha256'] = source_sha
        analysis['scoped_initializer_pins'] = []
        self.reject(compiled, review, analysis, evidence, 'initializer pin source-site coverage incomplete')

    def test_empty_complete_fixed_scope_inventory_is_rejected(self):
        compiled, review, analysis, evidence = self.full_rejection_input()
        analysis['fixed_scope_inventory'] = []
        self.reject(compiled, review, analysis, evidence, 'fixed-scope source-site coverage incomplete')

    def test_actual_site_binding_rejections(self):
        baseline = self.full_rejection_input()
        mutations = [
            ('false expression', lambda c,r,a: a['cache_bindings_and_uses'][0].update(expression='other'), 'expression/span'),
            ('no span', lambda c,r,a: a['cache_bindings_and_uses'][0].pop('span_start'), 'span missing'),
            ('UTF16 offsets miscounted', lambda c,r,a: a['cache_bindings_and_uses'][0].update(span_start=a['cache_bindings_and_uses'][0]['span_start']-1), 'expression/span'),
            ('wrong storage', lambda c,r,a: a['cache_bindings_and_uses'][0].update(storage='Owner.other'), 'storage owner'),
            ('unused cache', lambda c,r,a: a['cache_bindings_and_uses'][0].update(uses=[]), 'unused cache'),
            ('missing uses', lambda c,r,a: a['cache_bindings_and_uses'][0]['uses'][0].update(expression='arbitrary'), 'expression/span'),
            ('duplicate group', lambda c,r,a: a['unmanaged_static_storage_regions'].append(a['unmanaged_static_storage_regions'][0]), 'duplicate/empty storage region'),
            ('false counters', lambda c,r,a: a['unmanaged_static_storage_regions'][0].update(direct=0,caches=0), 'counters'),
            ('untracked helper', lambda c,r,a: a['cache_mechanisms'][0].update(valid=False), 'mechanism changed'),
            ('bad ref field', lambda c,r,a: a['actual_pe_cache_metadata'][0].update(fields=[{'name':'value','signature':'060F05'}]), 'tracked-reference cache'),
            ('no independent PE metadata', lambda c,r,a: c.pop('address_cache_metadata'), 'independently inspected'),
            ('different Get IL', lambda c,r,a: c.update(address_cache_metadata={'fields':[{'name':'value','signature':'061005'}],'methods':[dict(a['actual_pe_cache_metadata'][0]['methods'][0],il_sha256='0'*64)]}), 'getter IL'),
            ('missing exception', lambda c,r,a: r['forms']['raw']['exceptions'].pop(), 'exception closure'),
            ('duplicate exception', lambda c,r,a: r['forms']['raw']['exceptions'].append(r['forms']['raw']['exceptions'][0]), 'exception closure'),
            ('generic call waiver', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(reason='all calls invalidate'), 'semantic exception'),
            ('macro waiver', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(category='macro-origin'), 'semantic exception'),
            ('exception wrong bytes', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(source_sha256='0'*64), 'semantic exception'),
            ('missing ownership facts', lambda c,r,a: a['remaining_address_conversions'][0].pop('bound_symbol'), 'ownership facts'),
            ('false managed local', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(category='managed-local'), 'another storage owner'),
            ('false tracked ref', lambda c,r,a: r['forms']['raw']['exceptions'][1].update(category='tracked-ref-lifetime'), 'no bound managed reference'),
            ('false volatile waiver', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(category='volatile-atomic'), 'no matching bound access'),
            ('false TLS waiver', lambda c,r,a: r['forms']['raw']['exceptions'][0].update(category='tls'), 'no bound thread storage'),
            ('unreported conversion', lambda c,r,a: (a['remaining_address_conversions'].pop(),r['forms']['raw']['exceptions'].pop()), 'AsPointer source-site coverage'),
            ('missing pin inventory', lambda c,r,a: a.update(scoped_initializer_pins=[]), 'pin source-site coverage'),
            ('pin scope not covering declaration', lambda c,r,a: a['scoped_initializer_pins'][0].update(pin_scope_length=1), 'pin scope'),
            ('pin wrong source', lambda c,r,a: a['scoped_initializer_pins'][0].update(expression='arbitrary'), 'expression/span'),
        ]
        for name, mutate, error in mutations:
            with self.subTest(name=name):
                compiled, review, analysis, evidence = copy.deepcopy(baseline)
                mutate(compiled, review, analysis)
                self.reject(compiled, review, analysis, evidence, error)

    def test_repeated_pure_site_cannot_be_hidden_by_empty_unresolved_list(self):
        compiled, review, analysis, evidence = self.full_rejection_input()
        group = analysis['unmanaged_static_storage_regions'][0]
        direct = dict(analysis['remaining_address_conversions'][0], Path=group['path'], Region=group['region'],
                      Storage=group['storage'], Kind='direct', Loop=False)
        group['sites'].append(direct)
        group['direct'] = 1
        self.reject(compiled, review, analysis, evidence, 'unresolved repeated pure storage')

    def test_single_direct_site_repeating_in_loop_is_not_single_evaluation(self):
        compiled, review, analysis, evidence = self.full_rejection_input()
        group = analysis['unmanaged_static_storage_regions'][0]
        group['sites'] = [dict(analysis['remaining_address_conversions'][0], Path=group['path'], Region=group['region'],
                               Storage=group['storage'], Kind='direct', Loop=True)]
        group.update(direct=1, caches=0, repeated_loop_direct=1)
        self.reject(compiled, review, analysis, evidence, 'unresolved repeated pure storage')

    def test_source_replaced_after_semantic_report_is_rejected(self):
        compiled, review, analysis, evidence = self.full_rejection_input()
        self.control.source.write_text('// replaced source')
        self.reject(compiled, review, analysis, evidence, 'source changed after analysis')

if __name__ == '__main__':
    unittest.main(verbosity=2)
