"""Consume complete prebuilt entry/facade/library inputs without running tools."""
from pathlib import Path


def validate(campaign, proof_item, compiled):
    return _validate(campaign, proof_item, compiled)


def validate_native_abi(campaign, proof_item, compiled):
    """The exact native ABI project consumes only entry and C library.

    This separate route cannot qualify a managed API consumer without a facade.
    Its project, entry and complete references enforce that component boundary.
    """
    return _validate(campaign, proof_item, compiled, native_abi=True)


def compilation_review(campaign, selected, actual_input_proof, evidence_ref):
    """Strict completed embedded-profile review, without builds or inspectors.

    The test-profile route keeps its additional static-module/socket checks in
    profile_evidence.completed_compilation_review. This entrypoint requires the
    embedded publication and a complete managed entry/library/facade binding.
    """
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = helper(campaign/'tests/Audit/audit_product.py')
    audit.require(audit.product_identity(campaign,selected['form'],selected['project'],selected['receipt'])
                  == selected, 'Completed embedded publication differs')
    audit.require(isinstance(evidence_ref,dict) and Path(evidence_ref.get('path','')).is_file()
                  and audit.digest(evidence_ref['path']) == evidence_ref.get('sha256'),
                  'Completed embedded evidence missing/changed')
    evidence = audit.read(evidence_ref['path'])

    def record(name):
        item = evidence.get(name)
        audit.require(isinstance(item,dict) and Path(item.get('path','')).is_file()
                      and audit.digest(item['path']) == item.get('sha256'),
                      'Completed embedded '+name+' missing/changed')
        return audit.read(item['path'])

    compiled, document = record('compiled_proof'), record('actual_metadata')
    audit.require(compiled.get('identity') == selected
                  and document.get('sha256') == compiled.get('assembly_sha256')
                  and document.get('mvid') == compiled.get('mvid'),
                  'Completed embedded actual PE/receipt/source identity differs')
    observed = audit.d10_compiled(selected,evidence['inventory']['path'],evidence['compiled_il']['path'],document)
    audit.require(observed == compiled, 'Completed embedded compiled evidence differs from strict reader')
    actual = validate(campaign,actual_input_proof,compiled)
    exact = helper(campaign/'tests/Audit/consumer_evidence.py').data_review(compiled,evidence)
    allocation = audit.d10_allocations(compiled,exact)
    address = audit.d10_address_qualification(compiled,exact)
    audit.require(audit.product_identity(campaign,selected['form'],selected['project'],selected['receipt'])
                  == selected, 'Completed embedded publication changed during review')
    return {'status':'passed','compilation_input':actual['compilation_input'],
            'actual_input_proof':actual['actual_proof'],'actual_pdb_bindings':actual['pdb_bindings'],
            'compiled':compiled,'address':address,'allocation_and_semantics':allocation,
            'completed_evidence':evidence_ref,
            'scope':'Exact completed embedded PE/source/receipt/IL/RVA/native/allocation/semantic/address/lifetime and full entry/library/facade/PDB/Compile/PathMap/all-ILC binding; runtime/dependency matrix remains separate'}


def _validate(campaign, proof_item, compiled, *, native_abi=False):
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = helper(campaign/'tests/Audit/audit_product.py')
    pe = helper(campaign/'tests/Audit/pe_identity.py')
    source_inputs = helper(campaign/'tests/Audit/actual_profile_input.py')
    mapper = helper(campaign/'tests/Audit/pdb_sources.py')
    consumer = helper(campaign/'tests/Audit/consumer_evidence.py')

    def artifact(item, label):
        audit.require(isinstance(item, dict) and item.get('path') and item.get('sha256'),
                      'Completed input missing '+label)
        path = Path(item['path']).resolve()
        audit.require(path.is_file() and audit.digest(path) == item['sha256'], 'Completed input changed '+label)
        return path

    if isinstance(proof_item, (str, Path)):
        path = Path(proof_item).resolve()
        proof_item = {'path':str(path), 'sha256':audit.digest(path)}
    path = artifact(proof_item, 'actual proof')
    proof = audit.read(path)
    audit.require(proof.get('mode') in ('jit','aot') and proof.get('form') == compiled['identity']['form'],
                  'Completed input mode/form differs')
    audit.require(proof.get('profile') == audit.read(compiled['identity']['receipt'])['profile'],
                  'Completed input profile differs')
    roles = ('entry','library') if native_abi else ('entry','library','facade')
    audit.require(all(isinstance(proof.get(role), dict) for role in roles),
                  'Completed actual entry/library/facade closure missing')
    if native_abi:
        entry = proof['entry']
        audit.require('facade' not in proof
                      and Path(entry.get('identity',{}).get('path','')).name == 'Abi.dll'
                      and Path(entry.get('sourceBinding',{}).get('actualProject',{}).get('path','')).resolve()
                      == campaign/'tests/Abi/Abi.csproj'
                      and all(Path(row.get('path','')).name != 'Managed.CPython.dll' for row in proof.get('references',[])),
                      'Completed native ABI component/project boundary differs')
    bindings = {}
    for role in roles:
        item = proof[role]
        binary = artifact(item.get('identity'), role+' PE')
        if role in ('library','facade'):
            audit.require(binary.name == ('TranslatedCPython.dll' if role == 'library' else 'Managed.CPython.dll'),
                          'Completed wrong '+role+' assembly')
        actual = pe.pe_identity(binary)
        audit.require(actual['mvid'] == item['identity'].get('mvid'), 'Completed input MVID differs: '+role)
        source = item.get('sourceBinding', {})
        project = artifact(source.get('actualProject'), role+' project')
        if role == 'facade':
            audit.require(project == campaign/'src/Managed.CPython/Managed.CPython.csproj',
                          'Completed wrong owning facade project')
        audit.require(item.get('products') and Path(item['products']).is_absolute(),
                      'Completed actual products directory missing: '+role)
        maps, inputs = source_inputs.project_sources(project, item['products'])
        audit.require(source.get('actualCompileInputs') == {str(p):audit.digest(p) for p in sorted(inputs)}
                      and item.get('pathMap') == [list(m) for m in maps],
                      'Completed actual Compile/PathMap differs: '+role)
        try:
            bound = mapper.bind(binary, binary.with_suffix('.pdb'), maps, inputs)
        except (AssertionError, ValueError, OSError, KeyError) as error:
            audit.require(False, 'Completed actual PE/PDB/Compile binding differs: '+role+': '+str(error))
        audit.require(bound['documents'] and bound['documents'] == source.get('documents')
                      and bound['pdbSha256'] == source.get('pdbSha256')
                      and bound['portablePdbId'] == source.get('portablePdbId'),
                      'Completed actual PDB documents differ: '+role)
        bindings[role] = bound
    library = proof['library']['identity']
    audit.require(library['sha256'] == compiled['assembly_sha256'] and library['mvid'] == compiled['mvid']
                  and Path(proof['library']['sourceBinding']['actualProject']['path']).resolve()
                  == Path(compiled['identity']['project']).resolve(), 'Completed library PE/project differs from reviewed input')
    entry = proof['entry']['identity']
    assembly = Path(entry['path']).stem
    executable = artifact(proof.get('executable'), 'executable')
    references = proof.get('references')
    audit.require(isinstance(references, list) and references, 'Completed managed reference closure missing')
    paths = [artifact(item, 'reference') for item in references]
    audit.require(len(paths) == len(set(paths)), 'Completed duplicate managed reference')
    for item, reference in zip(references, paths):
        if item.get('mvid'):
            audit.require(pe.pe_identity(reference)['mvid'] == item['mvid'], 'Completed reference MVID differs')
    cell = {'mode':proof['mode'], 'output':str(executable.parent),
            'build_products':proof['entry']['products'],
            'compiled_artifacts':{str(executable):proof['executable']['sha256']}}
    if proof['mode'] == 'jit':
        audit.require(proof.get('ILC') is None and executable == Path(entry['path']).resolve(),
                      'Completed JIT entry/executable differs')
        actual_files = {p.resolve() for p in executable.parent.glob('*.dll')}
        audit.require(set(paths) == actual_files and {Path(proof[r]['identity']['path']).resolve()
                      for r in roles} <= set(paths), 'Completed JIT managed reference closure differs')
        cell['compiled_artifacts'].update({str(p):item['sha256'] for p,item in zip(paths,references)})
    else:
        ilc = proof.get('ILC')
        audit.require(isinstance(ilc,dict), 'Completed ILC inputs missing')
        response = artifact(ilc.get('response'), 'ILC response')
        audit.require(response.name == assembly+'.ilc.rsp', 'Completed wrong entry ILC response')
        actual_entry = artifact(ilc.get('entry'), 'ILC entry')
        artifact(ilc.get('executable'), 'ILC executable')
        objects = ilc.get('objects')
        audit.require(isinstance(objects,list) and len(objects) == 1, 'Completed ILC object closure missing')
        native_object = artifact(objects[0], 'ILC object')
        ilc_refs = ilc.get('references')
        audit.require(isinstance(ilc_refs,list) and len(ilc_refs) == len(references), 'Completed ILC reference closure differs')
        for item, reference in zip(ilc_refs, paths):
            audit.require(artifact(item,'ILC reference') == reference
                          and pe.pe_identity(reference)['mvid'] == item.get('mvid'), 'Completed ILC selected reference differs')
        audit.require(actual_entry == Path(entry['path']).resolve()
                      and ilc['executable']['sha256'] == proof['executable']['sha256'], 'Completed ILC entry/executable differs')
    selected = consumer.completed_compilation_input(cell, assembly, campaign=campaign,
                                                   library=proof['library'], compiled=compiled)
    audit.require(selected['sha256'] == library['sha256'] and Path(selected['pe']).resolve() == Path(library['path']).resolve(),
                  'Completed response-selected library differs')
    if proof['mode'] == 'aot':
        audit.require(Path(selected['ilc_response']).resolve() == response
                      and Path(selected['entry_assembly']).resolve() == actual_entry
                      and Path(selected['native_object']).resolve() == native_object
                      and selected['roots'] == ilc.get('roots'), 'Completed ILC response/entry/object/root closure differs')
    return {'status':'passed', 'actual_proof':proof_item, 'compilation_input':selected,
            'pdb_bindings':bindings, 'proof':proof}
