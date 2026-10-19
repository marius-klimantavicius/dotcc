"""Consume A32's exact actual-PE proofs and existing dependency auditors.

No product auditor, analyzer, generator, data probe or library build is run here.
Facades are inspected with the retained immutable metadata tool in a separately
leased command. Library IL/data/lifetime proofs come exclusively from A32.
"""
import argparse
from pathlib import Path
import sys

from run import ART, A32, CPY, ROOT, LOCK, read, ref, verify, write, utc, bind, authorize, helper, execute
from selection import publications,request_selected,cell_selected,input_selected,execution_inputs


def runtime_basis():
    proof_path = CPY / 'artifacts/owner-b-r39-20261018/actual-input-proof.json'
    proof = read(proof_path)['final']
    bound = proof['DotCC.Libc']
    verify(bound['identity'])
    for path, digest in bound['sourceBinding']['actualCompileInputs'].items():
        assert ref(path)['sha256'] == digest, path
    metadata_ref = proof['actualServiceIL']['metadata']
    verify(metadata_ref)
    document = read(metadata_ref['path'])
    assert document['sha256'] == bound['identity']['sha256'] and document['mvid'] == bound['identity']['mvid']
    assert {('libc','symlink'),('libc','readlink')} <= {(r['library'],r['symbol']) for r in document['native_imports']}
    return document, dict(proof=ref(proof_path),metadata=metadata_ref,sourceBinding=bound['sourceBinding'])


def consume(authority):
    selection = publications(authority)
    published = {p:authorize(authority,p) for p in selection}
    audit = helper(CPY / 'tests/Audit/audit_product.py')
    profile = helper(CPY / 'tests/Audit/profile_evidence.py')
    basis, basis_binding = runtime_basis()
    output = ART / 'audits'
    output.mkdir(parents=True,exist_ok=True)
    rows, by_sha = [], {}
    historical_requests, historical_cells, failed_diagnostics = [], [], []
    # Explicit actual consumer requests, never canonical-only evidence.
    for request_path in sorted((ART / 'actual-pe-requests').glob('*.json')):
        request = read(request_path)
        if not request_selected(request,selection):
            historical_requests.append(ref(request_path))
            continue
        selected = published[request['profile']]['products'][request['form']]
        identity = request['pe']
        verify(identity)
        tag = request['profile'] + '-' + request['form'] + '-' + identity['sha256'][:16]
        directory = A32 / 'd10' / tag
        evidence, compiled, document, lifetime = [read(directory / name) for name in
            ('evidence.json','compiled-proof.json','metadata.json','lifetime-result.json')]
        assert compiled['assembly_sha256'] == document['sha256'] == identity['sha256']
        assert compiled['mvid'] == document['mvid'] == identity['mvid']
        assert read(directory / 'product-inputs.json')['compileInputs'] == request['compileInputs']
        assert read(directory / 'product-inputs.json')['pathMap'] == request['pathMap']
        assert compiled['identity']['receipt_sha256'] == request['receipt']['sha256']
        assert lifetime['status'] == 'passed' and len(lifetime['concreteGuards']) == 9
        typed = helper(CPY / 'tests/Audit/consumer_evidence.py').data_review(compiled,evidence)
        row = dict(profile=request['profile'],form=request['form'],actualPE=identity,request=ref(request_path),
            metadata=audit.metadata(document,basis,product=True),exports=audit.exports(document,CPY),
            data=audit.d10_allocations(compiled,typed),address=audit.d10_address_qualification(compiled,evidence),
            lifetime=ref(directory / 'lifetime-result.json'),basis=basis_binding)
        # Source readiness makes no canonical compiled claim. Check the full
        # registration/initializer closure in this exact actual consumer's IL.
        if request['profile'] == 'upstream-tests-static':
            readiness = helper(CPY / 'tests/Audit/helper_profile.py').source_readiness(request['receipt']['path'],CPY)
            assert readiness['ready'] and not readiness['issues']
            registrations = readiness['static_registrations']['entries']
            methods = {r['name']:r for r in document['methods'] if r['type'] == 'Managed.Interpreters.CPython'}
            assert len(registrations) == 75
            assert sum(name != 'NULL' for name in registrations.values()) == 73
            assert all(name == 'NULL' or methods.get(name,{}).get('il_bytes',0) > 1 for name in registrations.values())
            row.update(generated=profile.generated(CPY,selected['identity']),
                helperReadiness=readiness,actualStaticRegistrations=registrations)
        else:
            closure = audit.closure(CPY,request['form'],Path(selected['project']['path']))
            config = next(r['path'] for r in closure['generated_inputs'] if Path(r['path']).name == 'config.c')
            row.update(generated=audit.generated(Path(selected['project']['path']),CPY,request['form']),
                closure=closure,builtins=audit.builtins(config,CPY,document))
        row['status'] = 'passed'
        write(output / (tag + '.json'),row)
        row_ref = ref(output / (tag + '.json'))
        rows.append(row_ref)
        by_sha[identity['sha256']] = row_ref
    assert rows, 'No current actual library requests/proofs selected'

    # Every cell's actual entry, library, facade, PDB/Compile and ALL ILC inputs
    # remain independently bound, even when two entries select identical bytes.
    facades = {}
    cells = []
    for path in sorted((ART / 'cells').glob('*/result.json')):
        cell = read(path)
        if not cell_selected(path,cell,selection):
            historical_cells.append(ref(path))
            continue
        if not cell.get('actualInputs'):
            continue
        cell_bindings = []
        successful_inputs,failed_inputs = execution_inputs(path,cell)
        failed_diagnostics.extend(dict(cell=ref(path),actualInput=item,
            status='failed diagnostic retained; excluded from successful execution qualification') for item in failed_inputs)
        for reference in successful_inputs:
            verify(reference)
            actual = read(reference['path'])
            assert input_selected(actual,selection), 'Cell contains another publication binding'
            for role in ('entry','library','facade'):
                if role not in actual: continue
                verify(actual[role]['identity'])
                for source, digest in actual[role].get('sourceBinding',{}).get('actualCompileInputs',{}).items():
                    assert ref(source)['sha256'] == digest, source
            if actual.get('ILC'):
                ilc = actual['ILC']
                for item in [ilc['response'],ilc['entry'],ilc['executable'],*ilc['objects'],*ilc['references']]:
                    verify(item)
                assert next(r['sha256'] for r in ilc['references'] if Path(r['path']).name == 'TranslatedCPython.dll') == actual['library']['identity']['sha256']
            library = actual['library']['identity']
            assert library['sha256'] in by_sha, 'Missing exact actual library audit'
            if 'facade' in actual:
                facade = actual['facade']['identity']
                facades.setdefault(facade['sha256'],facade)
            cell_bindings.append(dict(actualInput=reference,libraryAudit=by_sha[library['sha256']]))
        cells.append(dict(cell=ref(path),bindings=cell_bindings,status='passed'))
    assert cells, 'No current actual consumer input closures selected'

    plan = read(CPY / 'artifacts/owner-a-r31-20261018/d10-actual-input-command-plan.json')
    tool = plan['verifiedReusableAuditTools']['metadata-auditor']
    verify(tool['pe'])
    for path, digest in tool['actualCompileInputs'].items(): assert ref(path)['sha256'] == digest
    commands = []
    facade_rows = []
    for sha, facade in facades.items():
        path = output / ('facade-' + sha[:16] + '.metadata.json')
        if not path.exists():
            record = {}; commands.append(record)
            try:
                execute(['flock','-n','--close',LOCK,'dotnet',tool['pe']['path'],facade['path'],str(path)],
                    label='facade-metadata-' + sha[:16],cwd=Path('/tmp'),
                    output=output / ('facade-' + sha[:16] + '.log'),timeout=120,record=record)
            finally:
                write(output / 'facade-commands.json',commands)
        document = read(path)
        assert document['sha256'] == facade['sha256'] and document['mvid'] == facade['mvid']
        facade_rows.append(dict(actualPE=facade,metadata=ref(path),dependencies=audit.metadata(document,basis)))
    result = dict(status='passed',utc=utc(),authority=ref(authority),actualLibraryAudits=rows,facadeAudits=facade_rows,cells=cells,
        retainedHistoricalRequests=historical_requests,retainedHistoricalCells=historical_cells,
        retainedFailedDiagnosticInputs=failed_diagnostics,
        scope='Exact actual PE/schema2 data/address/nine lifetimes/native imports/exports/source/helper closure/PDB/Compile/PathMap/ILC closure. Runtime/trimmed/rooted/delivery/upstream and expanded semantic gates remain independently reported.')
    write(output / 'result.json',result)
    print('PASS actual audits',len(rows),'libraries',len(facades),'facades',len(cells),'cells')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--authority',required=True,type=Path)
    consume(parser.parse_args().authority)
