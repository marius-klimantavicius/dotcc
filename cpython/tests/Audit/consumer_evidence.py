"""Bind existing source review to freshly inspected consumer compilation inputs.

Isolated builds can change PE/MVID and FieldRVA locations without changing
generated sources. Run the unchanged semantic analyzer on that actual PE; never
substitute canonical metadata or relax the receipt/source/PE qualification gate.
"""
import copy
import json
from pathlib import Path


def data_review(compiled, evidence):
    """Select schema-2 probes for these exact actual consumer library bytes.

    A single form can have different canonical/JIT/ILC PEs. A source-equivalent
    probe for another PE cannot qualify this one; fresh probes must be supplied.
    Address lifetime explanations still pass through the unchanged review gate.
    """
    from campaigns.recipes import helper
    audit = helper(Path(__file__).with_name('audit_product.py'))
    audit.require(evidence is not None, 'D10 actual-product allocation/semantic evidence missing')
    result = dict(evidence)
    candidates = evidence.get('typed_data_by_pe', {})
    if compiled['assembly_sha256'] in candidates:
        result['typed_data'] = candidates[compiled['assembly_sha256']]
    item = result.get('typed_data')
    audit.require(item and Path(item['path']).is_file() and audit.digest(item['path']) == item['sha256'],
                  'D10 exact actual consumer typed-data evidence missing/changed')
    proof = json.loads(Path(item['path']).read_text())
    audit.require(proof.get('product', {}).get('sha256') == compiled['assembly_sha256']
                  and proof['product'].get('mvid') == compiled['mvid'],
                  'D10 typed probe product PE/MVID mismatch; fresh actual-input probes required')
    return result


def compilation_input(cell, assembly):
    """Identify the PE actually loaded by JIT or selected by this ILC response.

    An adjacent JIT build is not an AOT input oracle. Keep the response, entry
    assembly, native object and delivered executable in the same binding.
    """
    return _compilation_input(cell, assembly)


def completed_compilation_input(cell, assembly, *, campaign, library, compiled):
    """Select an exact source/PDB-bound library for a prebuilt companion.

    Direct-reference companions retain the original library artifacts root.
    Rebind that library independently before selecting its exact ILC reference;
    the ordinary compilation_input route still requires the entry build root.
    """
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = helper(campaign/'tests/Audit/audit_product.py')
    identity = library.get('identity', {})
    source = library.get('sourceBinding', {})
    project = source.get('actualProject', {})
    binary = Path(identity.get('path', '')).resolve()
    audit.require(binary.is_file() and binary.name == 'TranslatedCPython.dll'
                  and identity.get('sha256') == compiled['assembly_sha256'] == audit.digest(binary)
                  and identity.get('mvid') == compiled['mvid']
                  and helper(campaign/'tests/Audit/pe_identity.py').pe_identity(binary)['mvid'] == compiled['mvid'],
                  'Completed selected library PE identity differs')
    audit.require(project.get('path') and Path(project['path']).resolve() == Path(compiled['identity']['project']).resolve()
                  and project.get('sha256') == audit.digest(project['path'])
                  and library.get('products') and Path(library['products']).is_absolute(),
                  'Completed selected library project/products missing or different')
    inputs = helper(campaign/'tests/Audit/actual_profile_input.py')
    bound = inputs.bind(campaign, binary, project['path'], campaign/'tests/Audit/pdb_sources.py', library['products'])
    maps, _ = inputs.project_sources(project['path'], library['products'])
    audit.require(bound['actualProject']['path'] == str(Path(project['path']).resolve())
                  and bound['actualProject']['sha256'] == project['sha256']
                  and all(bound.get(key) == source.get(key) for key in
                  ('actualCompileInputs','documents','pdbSha256','portablePdbId'))
                  and library.get('pathMap') == [list(row) for row in maps],
                  'Completed selected library source/PDB binding differs')
    return _compilation_input(cell, assembly, bound_library=identity)


def _compilation_input(cell, assembly, *, bound_library=None):
    from campaigns.recipes import helper
    audit = helper(Path(__file__).with_name('audit_product.py'))
    output = Path(cell['output']).resolve()
    if cell['mode'] == 'jit':
        pe = output/'TranslatedCPython.dll'
        executable = output/(assembly+'.dll')
        audit.require(pe.is_file() and executable.is_file(), 'Actual JIT input missing')
        audit.require(cell['compiled_artifacts'].get(str(pe)) == audit.digest(pe)
                      and cell['compiled_artifacts'].get(str(executable)) == audit.digest(executable),
                      'Actual JIT input changed since build')
        return {'mode':'jit', 'pe':str(pe), 'sha256':audit.digest(pe),
                'executable':str(executable), 'executable_sha256':audit.digest(executable)}

    audit.require(cell['mode'] == 'aot', 'Unknown consumer compilation mode')
    products = Path(cell['build_products']).resolve()
    responses = list(products.rglob(assembly+'.ilc.rsp'))
    audit.require(len(responses) == 1,
                  'Missing/ambiguous actual consumer ILC response')
    response = responses[0]
    lines = response.read_text().splitlines()
    references = [Path(line[3:].strip('"')) for line in lines if line.startswith('-r:')]
    selected = [path for path in references if path.name == 'TranslatedCPython.dll']
    audit.require(len(selected) == 1 and selected[0].is_absolute() and selected[0].is_file(),
                  'Actual consumer ILC product input missing/ambiguous/outside build')
    if bound_library is None:
        audit.require(selected[0].resolve().is_relative_to(products),
                      'Actual consumer ILC product input missing/ambiguous/outside build')
    else:
        audit.require(selected[0].resolve() == Path(bound_library['path']).resolve()
                      and audit.digest(selected[0]) == bound_library['sha256'],
                      'Completed response-selected library differs from source/PDB-bound input')
    audit.require(references and len(references) == len(set(references))
                  and all(path.is_absolute() and path.is_file() for path in references),
                  'Actual consumer ILC reference closure incomplete/duplicate')
    pe = selected[0].resolve()
    entries = [Path(line.strip('"')) for line in lines if line and not line.startswith('-')]
    objects = [Path(line[3:].strip('"')) for line in lines if line.startswith('-o:')]
    audit.require(len(entries) == len(objects) == 1 and entries[0].is_absolute()
                  and objects[0].is_absolute() and entries[0].resolve().is_relative_to(products)
                  and objects[0].resolve().is_relative_to(products)
                  and entries[0].name == assembly+'.dll' and objects[0].name == assembly+'.o'
                  and entries[0].is_file() and objects[0].is_file(),
                  'Actual consumer ILC entry/object binding missing')
    executable = output/assembly
    audit.require(executable.is_file() and executable.read_bytes()[:4] == b'\x7fELF'
                  and cell['compiled_artifacts'].get(str(executable)) == audit.digest(executable),
                  'Actual AOT executable changed since build')
    roots = [line for line in lines if line.startswith('--root:')]
    if assembly == 'RootedBoundary':
        audit.require({'--root:TranslatedCPython','--root:Managed.CPython'} <= set(roots),
                      'Actual whole-assembly AOT roots missing')
    elif assembly == 'ManagedConsumer':
        audit.require(not {'--root:TranslatedCPython','--root:Managed.CPython'} & set(roots),
                      'Ordinary consumer unexpectedly roots whole library')
    return {'mode':'aot', 'pe':str(pe), 'sha256':audit.digest(pe),
            'ilc_response':str(response), 'ilc_response_sha256':audit.digest(response), 'roots':roots,
            'entry_assembly':str(entries[0]), 'entry_assembly_sha256':audit.digest(entries[0]),
            'native_object':str(objects[0]), 'native_object_sha256':audit.digest(objects[0]),
            'references':{str(path):audit.digest(path) for path in references},
            'executable':str(executable), 'executable_sha256':audit.digest(executable)}


def address_review(ctx, compiled, evidence, document, metadata_path, *, label=None):
    from campaigns.recipes import helper
    audit = helper(ctx.root/'tests/Audit/audit_product.py')
    try:
        return audit.d10_address_qualification(compiled, evidence)
    except audit.AuditError as error:
        # All unrelated failures remain failures. Only the observed compilation
        # identity mismatch is eligible for fresh analysis of the actual PE.
        if str(error) != 'D10 semantic analysis PE/MVID mismatch':
            raise
        original_rejection = str(error)

    form = compiled['identity']['form']
    label = label or 'audit-'+form+'-actual-consumer'
    old_item = evidence['address_review']
    old_review = json.loads(Path(old_item['path']).read_text())
    observation = old_review['forms'][form]
    item = observation['analysis']
    audit.require(audit.digest(item['path']) == item['sha256'], 'Original address analysis changed')
    original = json.loads(Path(item['path']).read_text())
    analyzer = original['analyzer']
    tool = Path(analyzer['tool'])
    audit.require(audit.digest(tool) == analyzer['tool_sha256'], 'Immutable address analyzer changed')
    for source in observation['analyzer_sources']:
        audit.require(audit.digest(source['path']) == source['sha256']
                      == analyzer['embedded_sources'][source['resource']], 'Address analyzer source changed')
    audit.require(document['path'] == compiled['assembly'] and document['sha256'] == compiled['assembly_sha256']
                  and document['mvid'] == compiled['mvid'], 'Actual consumer metadata mismatch')
    metadata_path = Path(metadata_path)
    audit.require(json.loads(metadata_path.read_text()) == document, 'Actual metadata report differs')
    rejected = ctx.artifacts/(label+'-canonical-review-rejected.json')
    rejected.write_text(json.dumps({'status':'rejected as required', 'error':original_rejection,
        'actual_pe':compiled['assembly'], 'actual_sha256':compiled['assembly_sha256'], 'actual_mvid':compiled['mvid'],
        'canonical_analysis':item}, indent=2)+'\n')

    report = ctx.artifacts/(label+'-address-analysis.json')
    ctx.run(['flock','-n','--close','/tmp/dotcc-cpython-shared-build.lock','dotnet',tool,
             compiled['inventory'],compiled['assembly'],report], label+'-address-analysis', timeout=300)
    analysis = json.loads(report.read_text())
    audit.require(analysis['source_files'] == original['source_files'] == observation['source_files'],
                  'Actual consumer source closure differs from reviewed source')
    audit.require(analysis['status'] == 'passed' and all(analysis.get(key) == [] for key in
                  ('semantic_errors','unknown_cache_bindings','unresolved_pure_sites')),
                  'Fresh actual consumer address analysis failed')

    # The nine lifetime explanations apply to exact source spans. Independently
    # preserve their actual IL call closure too; a changed caller invalidates it.
    names = ('L','L16','L32','PinAndRoot')
    edges = {name:[row for row in document['calls'] if row['target_type']=='Managed.Interpreters.CPython+Libc'
                   and row['target_method']==name] for name in names}
    prior_edges_item = observation['actual_lifetime_call_edges']
    audit.require(audit.digest(prior_edges_item['path']) == prior_edges_item['sha256'], 'Reviewed lifetime edges changed')
    prior_edges = json.loads(Path(prior_edges_item['path']).read_text())
    matching = [row for row in prior_edges['forms'] if row['form']==form]
    audit.require(len(matching)==1 and matching[0]['runtimeCallEdges']==edges,
                  'Actual consumer lifetime call closure differs')
    edge_report = ctx.artifacts/(label+'-lifetime-edges.json')
    edge_report.write_text(json.dumps({'form':form,'actual_pe':compiled['assembly'],
        'sha256':compiled['assembly_sha256'],'mvid':compiled['mvid'],'runtimeCallEdges':edges},indent=2)+'\n')

    review = copy.deepcopy(old_review)
    review['forms'] = {form:copy.deepcopy(observation)}
    refreshed = review['forms'][form]
    refreshed['analysis'] = {'path':str(report),'sha256':audit.digest(report)}
    refreshed['actual_metadata'] = {'path':str(metadata_path),'sha256':audit.digest(metadata_path)}
    refreshed['actual_lifetime_call_edges'] = {'path':str(edge_report),'sha256':audit.digest(edge_report)}
    refreshed['pe'] = {'path':compiled['assembly'],'sha256':compiled['assembly_sha256'],'mvid':compiled['mvid']}
    review['actual_consumer_derivation'] = {'original_review':old_item,'retained_rejection':str(rejected),
        'scope':'Same receipt/source-span lifetime review; fresh unchanged analyzer on actual consumer PE, plus independent actual IL/RVA/cache/lifetime edges'}
    review_path = ctx.artifacts/(label+'-address-review.json')
    review_path.write_text(json.dumps(review,indent=2)+'\n')
    updated = dict(evidence, address_review={'path':str(review_path),'sha256':audit.digest(review_path)})
    result = audit.d10_address_qualification(compiled, updated)
    result['actual_consumer_derivation'] = review['actual_consumer_derivation']
    return result
