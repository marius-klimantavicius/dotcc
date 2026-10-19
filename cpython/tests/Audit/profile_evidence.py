"""Bind the separate static test profile without borrowing embedded-gil proof.

The existing D10 and dependency readers remain unchanged. This module selects
the profile's own publication, ownership/assets, source/options and helper
readiness before those readers inspect its actual compiled inputs.
"""
import copy
import json
import os
import re
from pathlib import Path
import xml.etree.ElementTree as ET

PROFILE = 'upstream-tests-static'


def getpid_call_review(document, definitions):
    """Require real upstream and shared-service IL in the actual consumer PE."""
    audit = reader(Path(__file__).resolve().parents[2])
    owner, libc = 'Managed.Interpreters.CPython', 'Managed.Interpreters.CPython+Libc'
    methods = {(row['type'], row['name']): row for row in document['methods']}
    for name in definitions.values():
        audit.require(methods.get((owner, name), {}).get('il_bytes', 0) > 1,
                      'Actual GETPID upstream implementation missing')
    service = methods.get((libc, 'getpid'), {})
    audit.require(service.get('signature') == '000008' and service.get('il_bytes', 0) > 1,
                  'Actual GETPID shared typed service missing')
    required = [(owner, definitions['os_getpid'], owner, definitions['os_getpid_impl']),
                (owner, definitions['os_getpid_impl'], libc, 'getpid'),
                (owner, definitions['os_getpid_impl'], owner, 'PyLong_FromLong'),
                (libc, 'getpid', 'System.Environment', 'get_ProcessId')]
    edges = {(row['caller_type'], row['caller_method'], row['target_type'], row['target_method'])
             for row in document['calls']}
    audit.require(set(required) <= edges, 'Actual GETPID call closure missing')
    return required


def source_definitions(campaign, receipt, unit, names):
    """Resolve private function names from this receipt's real emitted unit."""
    audit = reader(campaign)
    commands = [row for row in receipt['commands'] if '--emit=obj' in row['command']
                and any(str(arg).endswith('/Modules/'+unit+'.c') for arg in row['command'])]
    audit.require(len(commands) == 1 and commands[0]['status'] == 'passed'
                  and commands[0].get('exit_code') == 0, 'Actual '+unit+' source owner ambiguous')
    argv = commands[0]['command']
    source = Path(next(arg for arg in argv if str(arg).endswith('/Modules/'+unit+'.c')))
    obj = Path(argv[argv.index('-o')+1])
    text = obj.read_text()
    definitions = {}
    for name in names:
        matches = re.findall(r'^//!!dotcc-obj def:('+name+r'__unit_\w+)$', text, re.M)
        audit.require(len(matches) == 1, 'Actual '+unit+' object definition ambiguous')
        definitions[name] = matches[0]
    return definitions, {'source':str(source), 'source_sha256':audit.digest(source),
                         'object':str(obj), 'object_sha256':audit.digest(obj)}


def hostname_call_review(document, definition):
    """Require the actual method-table, wrapper, shared service and DNS path."""
    audit = reader(Path(__file__).resolve().parents[2])
    owner = 'Managed.Interpreters.CPython'
    libc, pointers = owner+'+Libc', owner+'+CPythonFunctionPointers'
    methods = {(row['type'], row['name']):row for row in document['methods']}
    audit.require(methods.get((owner, definition), {}).get('il_bytes', 0) > 1,
                  'Actual hostname upstream implementation missing')
    service = methods.get((libc, 'gethostname'), {})
    audit.require(service.get('signature', '').lower() == '0002080f050b'
                  and service.get('il_bytes', 0) > 1, 'Actual hostname shared typed service missing')
    calls = document['calls']
    wrapper = [row for row in calls if row['caller_type'] == owner
               and row['caller_method'] == definition]
    required = [(owner, definition, libc, 'gethostname'),
                (libc, 'gethostname', 'System.Net.Dns', 'GetHostName'),
                (pointers, 'get_'+definition, owner, definition)]
    required += [(owner, definition, owner, name) for name in (
        'PySys_Audit', 'PyEval_SaveThread', 'PyEval_RestoreThread', 'PyUnicode_DecodeFSDefault')]
    error_edges = [row for row in wrapper if row['target_type'] == owner
                   and row['target_method'].startswith('set_error__unit_')]
    table_edges = [row for row in calls if row['caller_type'] == owner
                   and row['caller_method'].startswith('__DotCcInitializeGlobals_')
                   and row['target_type'] == pointers and row['target_method'] == 'get_'+definition]
    audit.require(error_edges and table_edges, 'Actual hostname method-table/error closure missing')
    required += [(row['caller_type'], row['caller_method'], row['target_type'], row['target_method'])
                 for row in error_edges+table_edges]
    edges = {(row['caller_type'], row['caller_method'], row['target_type'], row['target_method'])
             for row in calls}
    audit.require(set(required) <= edges, 'Actual hostname call closure missing')
    audit.require(all(row['target_assembly'] == ('System.Net.NameResolution'
        if row['target_type'] == 'System.Net.Dns' else 'TranslatedCPython')
        for row in calls if (row['caller_type'], row['caller_method'], row['target_type'], row['target_method'])
        in required), 'Actual hostname call assembly differs')
    return required


def reader(campaign):
    from campaigns.recipes import helper
    return helper(Path(campaign)/'tests/Audit/audit_product.py')


def socket_basis(campaign, selected, document):
    """Fresh source-owned socket paths; no historical PE signatures reused."""
    from collections import defaultdict, deque
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = reader(campaign)
    receipt = audit.read(selected['receipt'])
    roots = {'socket':'sock_initobj_impl', 'bind':'sock_bind', 'listen':'sock_listen',
             'connect':'sock_connect', 'accept':'sock_accept', 'getsockname':'sock_getsockname',
             'getpeername':'sock_getpeername', 'getaddrinfo':'socket_getaddrinfo',
             'getnameinfo':'socket_getnameinfo', 'gethostbyname':'socket_gethostbyname_ex',
             'get_h_errno':'socket_gethostbyname_ex', 'set_h_errno':'socket_gethostbyname_ex',
             'freeaddrinfo':'socket_getaddrinfo'}
    definitions, source = source_definitions(campaign, receipt, 'socketmodule', sorted(set(roots.values())))
    owner, libc = 'Managed.Interpreters.CPython', 'Managed.Interpreters.CPython+Libc'
    pointers = owner+'+CPythonFunctionPointers'
    methods = {(m['type'],m['name']):m for m in document['methods']}
    graph = defaultdict(list)
    for edge in document['calls']:
        graph[(edge['caller_type'],edge['caller_method'])].append(edge)

    def path(start, target, maximum):
        pending, seen = deque([(start,[])]), {start}
        while pending:
            node, edges = pending.popleft()
            if node == target:
                return edges
            if len(edges) >= maximum:
                continue
            for edge in graph[node]:
                key = (edge['target_type'],edge['target_method'])
                if key not in seen:
                    seen.add(key)
                    pending.append((key,edges+[edge]))
        audit.require(False, 'Actual socket source/host call path missing: '+str((start,target)))

    source_ref = {'path':source['source'],'sha256':source['source_sha256']}
    object_ref = {'path':source['object'],'sha256':source['object_sha256']}
    limits = {'connect':2,'accept':3,'get_h_errno':2,'set_h_errno':2}
    services = {}
    for service, root in roots.items():
        upstream, runtime = methods.get((owner,definitions[root]),{}), methods.get((libc,service),{})
        audit.require(upstream.get('il_bytes',0) > 1 and runtime.get('il_bytes',0) > 1,
                      'Actual socket upstream/runtime method missing')
        services[service] = {'source':source_ref,'object':object_ref,'sourceMethod':upstream,'runtimeMethod':runtime,
            'actualILPath':path((owner,definitions[root]),(libc,service),limits.get(service,1))}
    for service, target in [('getaddrinfo','GetHostAddrInfo'),('getnameinfo','HostGetNameInfo'),
                            ('gethostbyname','HostGetHostByName')]:
        services[service]['realHostILPath'] = path((libc,service),(libc,target),8)
    tables = {}
    object_text = Path(source['object']).read_text()
    for root in ('sock_bind','sock_listen','sock_connect','sock_accept','sock_getsockname',
                 'sock_getpeername','socket_getaddrinfo','socket_getnameinfo','socket_gethostbyname_ex'):
        name = definitions[root]
        audit.require('DotCcPointers.'+name in object_text, 'Actual socket method-table source binding missing')
        ptrs = [e for e in document['calls'] if e['caller_type'] == pointers
                and e['target_type'] == owner and e['target_method'] == name]
        initializers = [e for e in document['calls'] if e['target_type'] == pointers and e['target_method'] == 'get_'+name]
        audit.require(ptrs and initializers, 'Actual socket pointer/initializer path missing')
        tables[root] = {'pointerEdges':ptrs,'tableInitializerEdges':initializers}
    expected = {'serviceBindings':services,'methodTableBindings':tables}
    return helper(campaign/'tests/Audit/actual_profile_input.py').socket_review(campaign,document,expected)


def completed_compilation_review(campaign, selected, actual_input_proof, evidence_ref):
    """Consume finished exact-input evidence; never rebuild or run inspectors.

    This explicit route requires complete entry/facade/library PDB, Compile,
    PathMap and response-selected ILC bindings. Missing data/semantic/lifetime
    reports fail through the same strict readers as ordinary compilation_review.
    """
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = reader(campaign)
    audit.require(identity(campaign,selected['form'],selected['project'],selected['receipt']) == selected,
                  'Completed test-profile publication differs')
    audit.require(isinstance(evidence_ref,dict) and Path(evidence_ref.get('path','')).is_file()
                  and audit.digest(evidence_ref['path']) == evidence_ref.get('sha256'),
                  'Completed A32 evidence missing/changed')
    evidence = audit.read(evidence_ref['path'])

    def record(name):
        item = evidence.get(name)
        audit.require(isinstance(item,dict) and Path(item.get('path','')).is_file()
                      and audit.digest(item['path']) == item.get('sha256'), 'Completed '+name+' missing/changed')
        return audit.read(item['path'])

    compiled, document = record('compiled_proof'), record('actual_metadata')
    audit.require(compiled.get('identity') == selected and document.get('sha256') == compiled.get('assembly_sha256')
                  and document.get('mvid') == compiled.get('mvid'), 'Completed actual PE/receipt/source identity differs')
    observed = audit.d10_compiled(selected,evidence['inventory']['path'],evidence['compiled_il']['path'],document)
    audit.require(observed == compiled, 'Completed compiled evidence differs from strict reader')
    actual = helper(campaign/'tests/Audit/completed_inputs.py').validate(campaign,actual_input_proof,compiled)
    audit.require(actual['proof'].get('profile') == PROFILE, 'Completed actual profile differs')
    consumer = helper(campaign/'tests/Audit/consumer_evidence.py')
    exact = consumer.data_review(compiled,evidence)
    allocation = audit.d10_allocations(compiled,exact)
    address = audit.d10_address_qualification(compiled,exact)
    readiness = helper(campaign/'tests/Audit/helper_profile.py').source_readiness(selected['receipt'],campaign)
    audit.require(readiness.get('ready') and not readiness.get('issues'), 'Completed helper-profile readiness failed')
    registrations = readiness['static_registrations']['entries']
    methods = {m['name']:m for m in document['methods'] if m['type'] == 'Managed.Interpreters.CPython'}
    audit.require(len(registrations) == 75 and sum(name != 'NULL' for name in registrations.values()) == 73
                  and all(name == 'NULL' or methods.get(name,{}).get('il_bytes',0) > 1 for name in registrations.values()),
                  'Completed actual static initializer closure differs')
    receipt = audit.read(selected['receipt'])
    getpid, getpid_owner = source_definitions(campaign,receipt,'posixmodule',('os_getpid','os_getpid_impl'))
    hostname, hostname_owner = source_definitions(campaign,receipt,'socketmodule',('socket_gethostname',))
    getpid_edges = getpid_call_review(document,getpid)
    hostname_edges = hostname_call_review(document,hostname['socket_gethostname'])
    socket = socket_basis(campaign,selected,document)
    exports = audit.exports(document,campaign)
    audit.require(identity(campaign,selected['form'],selected['project'],selected['receipt']) == selected,
                  'Completed publication changed during review')
    return {'status':'passed','compilation_input':actual['compilation_input'], 'actual_input_proof':actual['actual_proof'],
            'actual_pdb_bindings':actual['pdb_bindings'],'compiled':compiled,'address':address,
            'allocation_and_semantics':allocation,'helper_source_readiness':readiness,'static_initializers':len(registrations),
            'getpid':{'definitions':getpid,'source_object':getpid_owner,'actual_call_edges':getpid_edges},
            'hostname':{'definitions':hostname,'source_object':hostname_owner,'actual_call_edges':hostname_edges},
            'socket':socket,'exports':exports,'completed_evidence':evidence_ref,
            'scope':'Completed same-PE source/receipt/IL/RVA/data/semantic/address/lifetime plus entry/facade/PDB/Compile/PathMap/all-ILC binding; no execution or inspector rerun'}


def identity(campaign, form, project, expected_receipt):
    audit = reader(campaign)
    campaign, project = Path(campaign).resolve(), Path(project).resolve()
    pointer = audit.read(campaign/'artifacts/campaign'/('current-'+PROFILE+'.json'))
    receipt_path = Path(pointer['receipt']).resolve()
    audit.require(receipt_path == Path(expected_receipt).resolve(), 'Test-profile receipt changed')
    receipt = audit.read(receipt_path)
    audit.require(receipt['project'] == 'cpython' and receipt['profile'] == PROFILE
                  and receipt['status'] == 'passed', 'Test-profile translation incomplete')
    output = receipt['outputs'][form]
    audit.require(Path(output['project']).resolve() == project
                  == Path(pointer['outputs'][form]['project']).resolve(), 'Test-profile project changed')
    directory = project.parent
    owned = audit.read(directory/'.campaign-files.json')
    audit.require(isinstance(owned, list) and len(owned) == len(set(owned)),
                  'Invalid test-profile ownership closure')
    files = {name:audit.digest(audit.safe_file(directory, name)) for name in sorted(owned)}
    for name in ('.campaign-files.json', 'Dotcc.SourceFiles.txt', project.name):
        files[name] = audit.digest(audit.safe_file(directory, name))
    audit.require(files == output['hashes'] == pointer['outputs'][form]['hashes'],
                  'Test-profile publication bytes changed')
    return {'receipt':str(receipt_path), 'receipt_sha256':audit.digest(receipt_path),
            'form':form, 'project':str(project), 'files':files}


def generated(campaign, selected):
    """Audit this profile's ordinary project and its complete test assets."""
    campaign = Path(campaign).resolve()
    audit = reader(campaign)
    project = Path(selected['project'])
    directory = project.parent
    receipt = audit.read(selected['receipt'])
    names = (directory/'Dotcc.SourceFiles.txt').read_text().splitlines()
    actual = {path.relative_to(directory).as_posix() for path in directory.rglob('*.cs')
              if not {'bin','obj'} & set(path.relative_to(directory).parts)}
    audit.require(names and len(names) == len(set(names)) and set(names) == actual,
                  'Test-profile generated source closure differs')
    owned = set(audit.read(directory/'.campaign-files.json'))
    audit.require(set(names) | {project.name, 'assets.json'} <= owned,
                  'Test-profile generated source ownership missing')
    tree = ET.parse(project)
    audit.require(tree.findtext('.//AssemblyName') == 'TranslatedCPython'
                  and tree.findtext('.//OutputType') == 'Library', 'Wrong test-profile assembly')
    audit.require(not list(tree.iter('NativeLibrary')) and not list(tree.iter('NativeReference')),
                  'Native test-profile engine/archive reference')
    for tag in ('ProjectReference','Reference','PackageReference'):
        for node in tree.iter(tag):
            name = node.get('Include','')
            audit.require(tag == 'ProjectReference' and '$' not in name and '*' not in name
                          and (directory/name).resolve() == campaign.parent/'DotCC.Libc/DotCC.Libc.csproj',
                          'Unknown test-profile managed dependency')
    linked = []
    for node in tree.iter('Compile'):
        name = node.get('Include')
        if name:
            audit.require('$' not in name and '*' not in name, 'Unresolved test-profile Compile')
            path = (directory/name).resolve()
            if not path.is_relative_to(directory):
                audit.require(path.is_relative_to(campaign/'src') and path.is_file(),
                              'Unknown test-profile authored source')
                linked.append(str(path))
    manifest = audit.read(directory/'assets.json')
    audit.require(manifest['profile'] == PROFILE and manifest['form'] == selected['form'],
                  'Test-profile asset selection differs')
    rows = manifest['assets']
    asset_names = [row['path'] for row in rows]
    # The passed receipt owns the actual selected test assets; the helper
    # contract additionally binds their C module/source closure below.
    expected = {name for name in selected['files'] if name.startswith(('stdlib/','notices/'))}
    audit.require(len(asset_names) == len(set(asset_names)) and set(asset_names) == expected,
                  'Incomplete/duplicate test-profile asset closure')
    for row in rows:
        path = audit.safe_file(directory, row['path'])
        audit.require(audit.digest(path) == row['sha256'] == selected['files'][row['path']],
                      'Test-profile asset bytes differ')
    packaged = {}
    for node in tree.iter('None'):
        name = node.get('Update') or node.get('Include')
        if name in expected or name == 'assets.json':
            audit.require(name not in packaged and node.get('CopyToOutputDirectory') == 'PreserveNewest'
                          and node.get('CopyToPublishDirectory') == 'PreserveNewest'
                          and node.get('TargetPath') == 'cpython/'+name,
                          'Test-profile asset packaging differs')
            packaged[name] = True
    audit.require(set(packaged) == expected | {'assets.json'}, 'Test-profile asset packaging missing')
    commands = receipt['commands']
    stages = {}
    for label in ('link','raw-build','postprocess','processed-build','final-raw-build','final-processed-build'):
        positions = [i for i,row in enumerate(commands) if row['label'] == label
                     and row['status'] == 'passed' and row.get('exit_code') == 0]
        audit.require(len(positions) == 1, 'Missing test-profile stage: '+label)
        stages[label] = positions[0]
    audit.require(stages['raw-build'] < stages['postprocess'] < stages['processed-build'],
                  'Test-profile raw/postprocess order differs')
    emits = [row for row in commands if '--emit=obj' in row['command']]
    audit.require(len(emits) == len(receipt['units']) == receipt['test_profile']['selected_units']
                  and len(set(receipt['units'])) == len(emits), 'Test-profile source/emission count differs')
    link = commands[stages['link']]['command']
    objects = []
    for command in emits:
        audit.require(command['status'] == 'passed' and command.get('exit_code') == 0,
                      'Failed test-profile emission')
        argv = command['command']
        obj = Path(argv[argv.index('-o')+1])
        audit.require(obj.is_file() and str(obj) in link, 'Test-profile object not retained/linked')
        objects.append({'path':str(obj), 'sha256':audit.digest(obj)})
    return {'scope':'actual separate-profile source/build/project/assets only; runtime/D10 separate',
            'source_count':len(names), 'asset_count':len(rows), 'objects':objects,
            'stages':stages, 'authored_links':linked}


def readiness(campaign, selected, metadata_path, previous_receipt, *, feature_stage='fork-only'):
    from campaigns.recipes import helper
    campaign = Path(campaign).resolve()
    audit = reader(campaign)
    document = audit.read(metadata_path)
    canonical = Path(selected['project']).parent/'bin/Release/net10.0/TranslatedCPython.dll'
    audit.require(Path(document['path']).resolve() == canonical.resolve()
                  and audit.digest(canonical) == document['sha256'] and document.get('mvid'),
                  'Test-profile canonical compiled identity differs')
    # Re-run the unchanged, complete feature/source/object-layout auditor. It
    # includes the independent B helper-role and all75 compiled-init checks.
    feature = helper(campaign/'NativeOracle/ForkHooks/audit_features.py').audit(
        selected['receipt'], previous_receipt, metadata_path, feature_stage)
    audit.require(feature['ready'], 'Test-profile feature/helper readiness failed')
    audit.require(identity(campaign, selected['form'], selected['project'], selected['receipt']) == selected,
                  'Test-profile changed during readiness audit')
    return feature


def compilation_review(ctx, selected, cell, assembly, label):
    """Freshly inspect an actual JIT/ILC input before upstream assertions run.

    The required index belongs to this test-profile receipt and contains its
    concrete source-span lifetime review. The unchanged readers reject an
    embedded-profile index, missing review, different PE or lifetime closure.
    """
    from campaigns.recipes import helper
    audit = reader(ctx.root)
    evidence_path = os.environ.get('DOTCC_CPYTHON_TEST_PROFILE_EVIDENCE')
    audit.require(evidence_path and Path(evidence_path).is_absolute(),
                  'Explicit test-profile compiled/lifetime evidence index required')
    evidence_index = audit.read(evidence_path)
    audit.require(evidence_index['receipt_sha256'] == selected['receipt_sha256']
                  and Path(evidence_index['receipt']).resolve() == Path(selected['receipt']),
                  'Test-profile evidence receipt differs')
    evidence = evidence_index['forms'][selected['form']]
    delivery = helper(ctx.root/'tests/Delivery/campaign_hooks.py')
    consumer = helper(ctx.root/'tests/Audit/consumer_evidence.py')
    binding = consumer.compilation_input(cell, assembly)
    metadata_tool = evidence_index.get('metadata_tool')
    audit.require(metadata_tool and audit.digest(metadata_tool['path']) == metadata_tool['sha256'],
                  'Explicit immutable test-profile metadata reader missing/changed')
    tools = Path(metadata_tool['path']).parent
    document = delivery.inspect(ctx, tools, Path(binding['pe']), label+'-actual-input')
    metadata_path = ctx.artifacts/(label+'-actual-input.metadata.json')
    audit.require(document['sha256'] == binding['sha256'], 'Test-profile actual metadata differs')
    actual_input = helper(ctx.root/'tests/Audit/actual_profile_input.py').review(
        ctx, selected, binding, document, evidence_index, label)
    inventory = Path(evidence['inventory']['path'])
    audit.require(audit.digest(inventory) == evidence['inventory']['sha256'],
                  'Test-profile inventory changed')
    data_tool = evidence_index['data_tool']
    audit.require(audit.digest(data_tool['path']) == data_tool['sha256'], 'Test-profile data reader changed')
    il = ctx.artifacts/(label+'-actual-il-rva.json')
    ctx.run(['flock','-n','--close',delivery.LOCK,'dotnet',data_tool['path'],binding['pe'],inventory,il],
            label+'-actual-il-rva',timeout=300)
    compiled = audit.d10_compiled(selected,inventory,il,document)
    canonical_il = evidence['compiled_il']
    audit.require(audit.digest(canonical_il['path']) == canonical_il['sha256'],
                  'Test-profile canonical IL review changed')
    canonical_pe = Path(selected['project']).parent/'bin/Release/net10.0/TranslatedCPython.dll'
    rejections = []
    if audit.digest(canonical_pe) != binding['sha256']:
        try:
            audit.d10_compiled(selected,inventory,canonical_il['path'],document)
        except audit.AuditError as error:
            audit.require(str(error) == 'D10 compiled PE mismatch',
                          'Unexpected canonical-only proof rejection')
            rejections.append({'case':'canonical-only proof for different actual PE','error':str(error)})
        else:
            raise RuntimeError('Test-profile canonical-only proof accepted for different actual PE')
    address = consumer.address_review(ctx,compiled,evidence,document,metadata_path,label=label)
    fresh = dict(evidence,address_review=address['report'])
    for field, value, case in (
            ('mvid', '00000000-0000-0000-0000-000000000000', 'wrong actual MVID'),
            ('assembly_sha256', '0'*64, 'wrong actual PE bytes')):
        wrong = copy.deepcopy(compiled)
        wrong[field] = value
        try:
            audit.d10_address_qualification(wrong,fresh)
        except RuntimeError as error:
            audit.require(str(error) == 'D10 semantic analysis PE/MVID mismatch',
                          'Unexpected '+case+' rejection')
            rejections.append({'case':case,'error':str(error)})
        else:
            raise RuntimeError('Test-profile '+case+' accepted')
    review = audit.read(address['report']['path'])
    review['forms'][selected['form']]['exceptions'] = []
    negative = ctx.artifacts/(label+'-negative-empty-exceptions.json')
    negative.write_text(json.dumps(review,indent=2)+'\n')
    try:
        audit.d10_address_qualification(compiled,dict(fresh,address_review={
            'path':str(negative),'sha256':audit.digest(negative)}))
    except RuntimeError as error:
        audit.require(str(error) == 'D10 remaining conversion/exception closure mismatch',
                      'Unexpected empty-exception rejection')
        rejections.append({'case':'empty lifetime exceptions','error':str(error)})
    else:
        raise RuntimeError('Test-profile empty lifetime exceptions accepted')
    features = audit.read(evidence['readiness']['path'])
    audit.require(audit.digest(evidence['readiness']['path']) == evidence['readiness']['sha256'],
                  'Test-profile feature/readiness review changed')
    registrations = features['compiled_helper_review']['static_registrations']['entries']
    methods = {row['name']:row for row in document['methods'] if row['type'] == 'Managed.Interpreters.CPython'}
    audit.require(all(init == 'NULL' or methods.get(init, {}).get('il_bytes', 0) > 1
                      for init in registrations.values()),
                  'Actual consumer helper initializer missing')
    nonnull = [init for init in registrations.values() if init != 'NULL']
    audit.require(len(registrations) == 75 and len(nonnull) == 73,
                  'Actual consumer registration closure differs')
    for case in ('missing initializer', 'empty initializer'):
        bad = copy.deepcopy(methods)
        if case == 'missing initializer':
            del bad[nonnull[0]]
        else:
            bad[nonnull[0]]['il_bytes'] = 0
        try:
            audit.require(all(bad.get(init, {}).get('il_bytes', 0) > 1 for init in nonnull),
                          'Actual consumer helper initializer missing')
        except RuntimeError as error:
            audit.require(str(error) == 'Actual consumer helper initializer missing',
                          'Unexpected initializer negative rejection')
            rejections.append({'case':case, 'method':nonnull[0], 'error':str(error)})
        else:
            raise RuntimeError('Invalid initializer accepted')
    receipt = audit.read(selected['receipt'])
    posix = [row['command'] for row in receipt['commands']
             if '--emit=obj' in row['command']
             and any(str(arg).endswith('/Modules/posixmodule.c') for arg in row['command'])]
    audit.require(len(posix) == 1, 'Actual GETPID source owner ambiguous')
    object_path = Path(posix[0][posix[0].index('-o')+1])
    definitions = {}
    for name in ('os_getpid', 'os_getpid_impl'):
        matches = re.findall(r'^//!!dotcc-obj def:('+name+r'__unit_\w+)$', object_path.read_text(), re.M)
        audit.require(len(matches) == 1, 'Actual GETPID object definition ambiguous')
        definitions[name] = matches[0]
    getpid_edges = getpid_call_review(document, definitions)
    for edge in getpid_edges:
        bad = dict(document, calls=[row for row in document['calls']
            if (row['caller_type'], row['caller_method'], row['target_type'], row['target_method']) != edge])
        try:
            getpid_call_review(bad, definitions)
        except RuntimeError as error:
            audit.require(str(error) == 'Actual GETPID call closure missing',
                          'Unexpected GETPID negative rejection')
            rejections.append({'case':'missing GETPID call edge', 'edge':edge, 'error':str(error)})
        else:
            raise RuntimeError('Missing GETPID call edge accepted')
    hostname_definitions, hostname_owner = source_definitions(
        ctx.root, receipt, 'socketmodule', ('socket_gethostname',))
    hostname_edges = hostname_call_review(document, hostname_definitions['socket_gethostname'])
    for edge in hostname_edges:
        bad = dict(document, calls=[row for row in document['calls']
            if (row['caller_type'], row['caller_method'], row['target_type'], row['target_method']) != edge])
        try:
            hostname_call_review(bad, hostname_definitions['socket_gethostname'])
        except RuntimeError as error:
            audit.require(str(error) in ('Actual hostname call closure missing',
                          'Actual hostname method-table/error closure missing'),
                          'Unexpected hostname negative rejection')
            rejections.append({'case':'missing hostname call edge', 'edge':edge, 'error':str(error)})
        else:
            raise RuntimeError('Missing hostname call edge accepted')
    for type_name, method_name in (
            ('Managed.Interpreters.CPython', hostname_definitions['socket_gethostname']),
            ('Managed.Interpreters.CPython+Libc', 'gethostname')):
        bad = dict(document, methods=[row for row in document['methods']
            if (row['type'], row['name']) != (type_name, method_name)])
        try:
            hostname_call_review(bad, hostname_definitions['socket_gethostname'])
        except RuntimeError as error:
            audit.require(str(error) in ('Actual hostname upstream implementation missing',
                          'Actual hostname shared typed service missing'),
                          'Unexpected missing hostname method rejection')
            rejections.append({'case':'missing hostname method', 'method':method_name, 'error':str(error)})
        else:
            raise RuntimeError('Missing hostname method accepted')
    audit.exports(document,ctx.root)
    audit.require(binding == consumer.compilation_input(cell,assembly),
                  'Test-profile input changed during compiled review')
    audit.require(identity(ctx.root,selected['form'],selected['project'],selected['receipt']) == selected,
                  'Test-profile publication changed during compiled review')
    return {'status':'passed','compilation_input':binding,'compiled':compiled,'address':address,
            'actual_pdb_project_socket_proof':actual_input,
            'static_initializers':len(registrations),'negative_rejections':rejections,
            'getpid':{'definitions':definitions,'source_object':str(object_path),
                      'source_object_sha256':audit.digest(object_path),'actual_call_edges':getpid_edges},
            'hostname':{'definitions':hostname_definitions, 'owner':hostname_owner,
                        'actual_call_edges':hostname_edges},
            'scope':'actual source/receipt/PE/MVID/IL/RVA/cache/lifetime and helper/export closure; allocation/runtime separate'}
