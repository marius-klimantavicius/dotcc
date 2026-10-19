"""Strict schema-2 consumers. No builds, execution, locks or product mutation.

The caller supplies require, preserving the existing AuditError identity. Source
syntax is an inventory only; acceptance also needs independent actual IL/RVA,
complete copy closure, PE/PDB/Compile binding and exact-input executable probes.
"""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import xml.etree.ElementTree as ET


OWNER = 'Managed.Interpreters.CPython'
HOLDER = OWNER + '+CPythonLiterals'
RUNTIME = OWNER + '+Libc'
DESTINATIONS = {'GlobalArrayZeroed', 'GlobalArrayFrom', 'GlobalAlignedZeroed', 'GlobalAlignedFrom'}


def read(path):
    return json.loads(Path(path).read_text())


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def load(path):
    spec = importlib.util.spec_from_file_location('typed_' + Path(path).stem, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def artifact(require, item, label):
    require(isinstance(item, dict) and item.get('path') and Path(item['path']).is_file()
            and digest(item['path']) == item.get('sha256'), 'D10 stale/missing typed ' + label)
    return Path(item['path']).resolve()


def compiled(require, identity, inventory_path, il_path, document):
    inventory, il = read(inventory_path), read(il_path)
    # Independently enumerate every source backing, view, copy and owned recipe.
    # An omitted manifest row cannot make an allocating/incorrect getter vanish.
    preparation = load(Path(__file__).resolve().parents[2] / 'scripts/generated_code_evidence.py')
    with tempfile.TemporaryDirectory(prefix='cpython-typed-inventory-') as temporary:
        actual = preparation.prepare_typed(Path(inventory['source']), Path(identity['receipt']),
            Path(temporary)/'inventory.json', holder=OWNER+'.CPythonLiterals', owner=OWNER)
    for key in ('files', 'properties', 'typedViews', 'totals', 'propertyTypes', 'viewTypes', 'copySites', 'ownedRecipes'):
        require(inventory.get(key) == actual[key], 'D10 typed source inventory mismatch: '+key)
    require(actual['properties'] and actual['totals'].get('allocating_AsSpan_payload_sites', 0) == 0,
            'D10 remaining allocating typed payload')
    assembly = Path(document['path']).resolve()
    proof_assembly = Path(il['assembly']).resolve()
    require(document['assembly'] == 'TranslatedCPython' and document.get('mvid')
            and document['mvid'] == il.get('mvid'), 'D10 typed actual PE/MVID mismatch')
    require(assembly.is_file() and proof_assembly.is_file()
            and digest(assembly) == digest(proof_assembly) == document['sha256'] == il['assemblySha256'],
            'D10 compiled PE mismatch')
    require(Path(il['manifest']).resolve() == Path(inventory_path).resolve()
            and digest(inventory_path) == il['manifestSha256'], 'D10 IL/source manifest mismatch')
    require(il.get('status') == 'passed' and il.get('schemaVersion') == 2 and il.get('auditRevision') == 3
            and il.get('newarr') == 0, 'D10 complete typed IL audit missing/obsolete/allocating')
    methods = {m['token']: m for m in document['methods'] if 'token' in m}
    require(len(methods) == sum('token' in m for m in document['methods']), 'D10 duplicate actual method token')
    getters = set()
    for group, observed_group in (('properties', 'properties'), ('typedViews', 'views')):
        wanted, observed = inventory[group], il[observed_group]
        require(len(observed) == len(wanted), 'D10 typed getter/view closure mismatch')
        for expected, row in zip(wanted, observed):
            name = 'get_'+expected['name'] if group == 'properties' else expected['name']
            require(row['name'] == name and row['owner'] == expected['owner'], 'D10 wrong typed getter/view owner')
            for key in ('elementWidth', 'length', 'sha256'):
                require(row[key] == expected[key], 'D10 typed width/bytes/extent mismatch: '+name)
            method = methods.get(row['token'], {})
            method_name = name if group == 'properties' else 'get_Values'
            require(method.get('type') == expected['owner'] and method.get('name') == method_name
                    and method.get('attributes', 0) & 0x10 and method.get('newarr_count') == 0
                    and method.get('il_bytes', 0) == row['ilBytes'] > 0
                    and method.get('il_sha256') == row.get('ilSha256'), 'D10 typed actual getter IL mismatch')
            require(row['token'] not in getters, 'D10 duplicate typed getter')
            getters.add(row['token'])
            if group == 'properties':
                require(row['elementType'] == expected['elementType'] and row['elements'] == expected['elements']
                        and row['fieldExtent'] == row['length'] and row['rva'] > 0 and row['fieldToken'] > 0
                        and row['signature'] == 'System.ReadOnlySpan`1<'+expected['elementType']+'>',
                        'D10 typed RVA backing signature/extent mismatch')
            else:
                require(row['backing'] == expected['backing'] and row['conversions'] == 0
                        and row['elementType'].replace('+', '.') == expected['csharpType'].removeprefix('global::')
                        and row['signature'] == 'System.ReadOnlySpan`1<'+row['elementType']+'>',
                        'D10 wrong typed view/bits')
    require(il['propertyCount'] == len(inventory['properties']) and il['viewCount'] == len(inventory['typedViews'])
            and il['rvaBytes'] == sum(p['length'] for p in inventory['properties']), 'D10 typed RVA totals mismatch')
    closure = il['transitiveCopyClosure']
    visited = {m['token']: m for m in closure['methods']}
    require(visited and len(visited) == len(closure['methods']), 'D10 missing/duplicate copy closure')
    direct = [(m, c) for m in methods.values() for c in m.get('call_sites', [])
              if c['opcode'] in ('call', 'callvirt', 'newobj') and c['target_token'] in getters]
    require(il['getterCallSites'] == len(direct) and {c['target_token'] for _, c in direct} == getters,
            'D10 unreferenced typed getter/view')
    roots = {m['token'] for m, _ in direct} | {m['token'] for m in methods.values()
        if m['name'].startswith('__dotcc_owned_array_') or m['type'] == HOLDER and m['name'] == '.cctor'}
    require(set(closure['roots']) == roots, 'D10 missing copy/owned-helper root')
    for token, row in visited.items():
        m = methods.get(token, {})
        require(row['name'] == m.get('type', '')+'.'+m.get('name', '')
                and m.get('il_sha256') == row.get('ilSha256') and m.get('il_bytes') == row.get('ilBytes')
                and m.get('newarr_count') == row.get('newarr') == 0, 'D10 allocating/stale transitive method')
    # Independently rebuild the local call graph from the other PE reader. Every
    # local edge must be traversed or identified as a concrete C destination.
    expected_edges, expected_destinations, expected_external, expected_initializers = set(), set(), set(), set()
    initializers = {m['type']:m['token'] for m in methods.values() if m['name'] == '.cctor'}
    acquire_targets = {(c['callerToken'],c['offset']):c for c in closure['trackedAcquireTargets']}
    observed_acquires = set()
    reached, pending = set(), list(roots)
    while pending:
        token = pending.pop()
        if token in reached:
            continue
        require(token in visited, 'D10 missing transitive copy/owned-helper method')
        reached.add(token)
        m = methods[token]
        for owner in {m['type']} | {f['target_type'] for f in m.get('field_sites', [])}:
            target = initializers.get(owner)
            if owner not in (OWNER, RUNTIME) and target and target != token:
                expected_initializers.add((token, target))
                pending.append(target)
        for call in m.get('call_sites', []):
            if call['opcode'] not in ('call', 'callvirt', 'newobj'):
                continue
            site = (token, call['offset'])
            if call['target_type'] == OWNER+'+__DotccGlobalAddressCache' and call['target_method'] == 'Get':
                acquire = acquire_targets.get(site, {})
                pointers = [c for c in m['call_sites'] if c['opcode'] == 'ldftn'
                            and c['offset'] == acquire.get('pointerOffset')]
                require(len(pointers) == 1 and pointers[0]['target_token'] == acquire.get('targetToken')
                        and acquire.get('cacheToken') == call['target_token'], 'D10 unbound tracked-ref acquire target closure')
                observed_acquires.add(site)
                expected_edges.add((token,acquire['pointerOffset'],acquire['targetToken']))
                pending.append(acquire['targetToken'])
            if call['opcode'] == 'newobj':
                require(call['target_type'].startswith(('System.ReadOnlySpan`1<', 'System.Span`1<'))
                        or call['target_token'] in methods and call['target_type'] in document.get('value_types', []),
                        'D10 array/object/delegate construction in transitive payload closure')
            require(call['target_method'] not in ('ToArray', 'CreateInstance')
                    and call['target_type'] != 'System.Activator', 'D10 equivalent temporary construction in transitive payload closure')
            if call['target_type'] == RUNTIME and call['target_method'] in DESTINATIONS:
                require(call['target_token'] in methods, 'D10 foreign C destination boundary')
                expected_destinations.add(site)
            elif call['target_token']:
                expected_edges.add((*site, call['target_token']))
                pending.append(call['target_token'])
            else:
                expected_external.add(site)
    require(reached == set(visited), 'D10 transitive closure has missing/unreachable methods')
    require(observed_acquires == set(acquire_targets), 'D10 unused/missing tracked-ref acquire binding')
    require(expected_edges == {(e['callerToken'], e['offset'], e['targetToken']) for e in closure['edges']},
            'D10 transitive local edge closure mismatch')
    require(expected_initializers == {(e['callerToken'], e['targetToken']) for e in closure['initializerEdges']},
            'D10 implicit type initializer closure mismatch')
    actual_indirect = {(token,c['offset'],c['signature']) for token in visited for c in methods[token].get('indirect_calls', [])}
    require(actual_indirect == {(c['callerToken'],c['offset'],c['signature']) for c in closure['trackedAddressCallbacks']}
            and all(methods[token]['type'] == OWNER+'+__DotccGlobalAddressCache' and methods[token]['name'] == 'Get'
                    for token,_,_ in actual_indirect), 'D10 unreviewed tracked-ref/indirect callback boundary')
    boundaries = closure['requiredDestinationBoundaries']
    require(expected_destinations == {(e['callerToken'], e['offset']) for e in boundaries}
            and all(e['reason'] and e['target'] in {RUNTIME+'.'+n for n in DESTINATIONS} for e in boundaries),
            'D10 required C destination boundary mismatch')
    external = closure['externalCallSites'] + closure['requiredPinnedPointerPoolAllocations']
    require(expected_external == {(e['callerToken'], e['offset']) for e in external}
            and len(external) == len(expected_external), 'D10 external/pointer-pool closure mismatch')
    for row in closure['requiredPinnedPointerPoolAllocations']:
        m = methods[row['callerToken']]
        require(row['pinned'] is True and row.get('lifetimeFields')
                and (m['type'] == HOLDER and m['name'] == '.cctor' and row['elementTypes'] == ['byte']
                     or m['type'] == RUNTIME and m['name'] == 'L16' and row['elementTypes'] == ['char']),
                'D10 unreviewed pinned pointer-pool boundary')
    for row in external:
        method = methods[row['callerToken']]
        call = next(c for c in method['call_sites'] if c['offset'] == row['offset'])
        require(row['target'] == call['target_type']+'.'+call['target_method'], 'D10 external boundary target mismatch')
    return {'status':'passed', 'schemaVersion':2,
            'scope':'actual source/receipt/PE/RVA/transitive binding; allocation and semantic gates separate',
            'requires_same_pe_address_lifetime_qualification':True,
            'identity':identity, 'assembly':str(assembly), 'proof_assembly':str(proof_assembly),
            'assembly_sha256':digest(assembly), 'mvid':document['mvid'],
            'inventory':str(inventory_path), 'inventory_sha256':digest(inventory_path),
            'il_report':str(il_path), 'il_report_sha256':digest(il_path),
            'properties':len(inventory['properties']), 'views':len(inventory['typedViews']),
            'payload_bytes':il['rvaBytes'], 'getter_call_sites':len(direct),
            'transitive_methods':len(visited), 'required_c_destinations':boundaries,
            'required_pointer_pools':closure['requiredPinnedPointerPoolAllocations']}


def allocations(require, compiled, evidence):
    """Consume exact-reference schema-2 probes without granting semantic waivers."""
    inventory = read(compiled['inventory'])
    proof = read(artifact(require, evidence.get('typed_data'), 'data proof'))
    require(proof.get('schemaVersion') == 2, 'D10 typed executable proof schema missing')
    actual_inventory = read(artifact(require, proof['inventory'], 'probe inventory'))
    for key in ('receiptSha256', 'files', 'properties', 'typedViews'):
        require(actual_inventory[key] == inventory[key], 'D10 typed probe inventory mismatch')
    product = proof['product']
    artifact(require, product, 'actual product PE')
    require(product['sha256'] == compiled['assembly_sha256'] and product['mvid'] == compiled['mvid'],
            'D10 typed probe product PE/MVID mismatch')
    inputs = read(artifact(require, proof['probeInputs'], 'probe inputs'))
    for key in ('project', 'reference', 'inventory', 'program', 'productTables', 'probeProject'):
        artifact(require, inputs[key], key)
    require(inputs['reference']['sha256'] == compiled['assembly_sha256']
            and Path(inputs['project']['path']).resolve() == Path(compiled['identity']['project']).resolve()
            and inputs['project']['sha256'] == compiled['identity']['files'][Path(inputs['project']['path']).name]
            and inputs['inventory'] == proof['inventory'], 'D10 typed direct input mismatch')
    project = Path(inputs['probeProject']['path'])
    tree = ET.parse(project)
    refs = list(tree.iter('Reference'))
    require(not list(tree.iter('ProjectReference')) and not list(tree.iter('PackageReference'))
            and len(refs) == 1 and refs[0].get('Include') == 'TranslatedCPython'
            and (project.parent/refs[0].findtext('HintPath', '')).resolve() == Path(inputs['reference']['path']).resolve(),
            'D10 typed probe must reference exact actual PE')
    sources = {(project.parent/n.get('Include', '')).resolve() for n in tree.iter('Compile')}
    require(tree.findtext('.//EnableDefaultCompileItems') == 'false'
            and sources == {Path(inputs[k]['path']).resolve() for k in ('program', 'productTables')},
            'D10 typed probe recompiles/omits sources')
    result = {}
    for mode in ('jit', 'aot'):
        entry = proof[mode]
        report = read(artifact(require, entry['report'], mode+' report'))
        command = read(artifact(require, entry['execution'], mode+' execution record'))
        stdout = artifact(require, command['stdout'], mode+' execution stdout')
        artifact(require, command['stderr'], mode+' execution stderr')
        expected_argv = ([entry['entry']['pe']['path']] if mode == 'jit' else [])
        expected_argv += [entry['report']['path'], proof['nativeOracle']['transcript']['path']]
        argv = command.get('argv', [])
        require(command.get('exitCode') == 0 and command.get('status') == 'passed'
                and not command.get('timedOut') and len(argv) >= len(expected_argv)+1
                and argv[-len(expected_argv):] == expected_argv
                and (Path(argv[-len(expected_argv)-1]).name == 'dotnet' if mode == 'jit'
                     else Path(argv[-len(expected_argv)-1]).resolve() == Path(entry['executable']['path']).resolve())
                and command.get('reportSha256') == entry['report']['sha256']
                and command.get('executableSha256') == entry['executable']['sha256']
                and stdout.stat().st_size > 0, 'D10 typed probe execution/result binding mismatch')
        require(report['status'] == 'passed' and report['dynamicCode'] is (mode == 'jit')
                and report['productMvid'] == compiled['mvid'], 'D10 typed execution mode/product MVID mismatch')
        for group, name in (('properties', 'properties'), ('typedViews', 'views')):
            expected = {p['name']:(p['length'], p['elementWidth'], p['sha256']) for p in inventory[group]}
            observed = {p['name']:(p['length'], p['width'], p['sha256']) for p in report[name]}
            require(len(observed) == len(report[name]) and observed == expected, 'D10 typed allocation property/view closure mismatch')
            require(all(p['allocated'] == 0 and p['iterations'] >= 10000 for p in report[name]),
                    'D10 warmed typed payload allocation detected')
        selected = artifact(require, entry['productReference'], mode+' product reference')
        require(digest(selected) == compiled['assembly_sha256']
                and entry['productReference']['mvid'] == compiled['mvid'], 'D10 selected probe product mismatch')
        # Source and actual product/entry PDB binding are mandatory. The mapper
        # independently reads the portable-PDB ID and document checksums.
        bindings = load(Path(__file__).with_name('typed_inputs.py'))
        bindings.validate(require, compiled, entry, inputs, mode)
        result[mode] = entry
    native = proof['nativeOracle']
    transcript = artifact(require, native['transcript'], 'native transcript')
    artifact(require, native['source'], 'native source')
    artifact(require, native['archive'], 'native archive')
    require(native.get('actualSourceBytes'), 'D10 native actual source binding missing')
    for pair in native['actualSourceBytes']:
        artifact(require, pair['original'], 'native source original')
        artifact(require, pair['actualNativeSource'], 'native source input')
        require(pair['original']['sha256'] == pair['actualNativeSource']['sha256'], 'D10 native source bytes differ')
    rows = {}
    for line in transcript.read_text().splitlines():
        name, length, data = line.split()
        require(name not in rows and int(length) == len(bytes.fromhex(data)), 'D10 duplicate/invalid native table')
        rows[name] = (int(length), hashlib.sha256(bytes.fromhex(data)).hexdigest())
    required = {'_PyLong_DigitValue':256, '_Py_ctype_table':1024, '_Py_ctype_tolower':256,
                '_Py_ctype_toupper':256, '_PyUnicode_ExtendedCase':4944}
    require({n:v[0] for n,v in rows.items()} == required, 'D10 incomplete native actual table closure')
    for mode in ('jit', 'aot'):
        tables = read(proof[mode]['report']['path'])['nativeTables']
        require(len(tables) == len(rows) and {r['name']:(r['length'], r['sha256']) for r in tables} == rows
                and all(r['nativeBytes'] is True and int(r['address'], 16) > 0 for r in tables)
                and len({r['address'] for r in tables}) == len(rows), 'D10 native byte/identity table mismatch')
    return result
