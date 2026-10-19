"""Read-only PE/PDB/project and socket-closure proof for an actual consumer.

The publication supplies expected source-owned edges, never execution results.
Every edge and source checksum is checked against the newly selected input.
"""
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def project_sources(project, products=None):
    project = Path(project).resolve()
    tree = ET.parse(project).getroot()
    mapping = tree.findtext('.//PathMap')
    maps = [tuple(s.replace('$(MSBuildProjectDirectory)', str(project.parent)).split('=', 1))
            for s in mapping.split(',')] if mapping else []
    sources = set()
    for node in tree.iter('Compile'):
        name = node.get('Include')
        if name:
            sources.update(p.resolve() for p in (project.parent.glob(name) if '*' in name else [project.parent/name]))
    if tree.findtext('.//EnableDefaultCompileItems') != 'false' and tree.findtext('.//EnableDefaultItems') != 'false':
        sources.update(p.resolve() for p in project.parent.rglob('*.cs')
                       if not {'bin', 'obj'}.intersection(p.relative_to(project.parent).parts))
    generated = (Path(products)/'obj'/project.stem) if products else project.parent/'obj/Release/net10.0'
    sources.update(p.resolve() for p in generated.rglob('*.cs'))
    return maps, sources


def bind(campaign, pe, project, mapper_path, products=None):
    from campaigns.recipes import helper
    audit = helper(Path(campaign)/'tests/Audit/audit_product.py')
    mapper = helper(Path(mapper_path))
    pe, project = Path(pe), Path(project)
    maps, sources = project_sources(project, products)
    result = mapper.bind(pe, pe.with_suffix('.pdb'), maps, sources)
    audit.require(all(Path(d['path']).resolve() in sources for d in result['documents']),
                  'PDB document outside actual project Compile inputs')
    result['actualProject'] = {'path': str(project), 'sha256': audit.digest(project)}
    result['actualCompileInputs'] = {str(p): audit.digest(p) for p in sorted(sources)}
    return result


def socket_review(campaign, document, expected):
    from campaigns.recipes import helper
    audit = helper(Path(campaign)/'tests/Audit/audit_product.py')
    methods = {(m['type'], m['name']): m for m in document['methods']}
    fields = ('caller_type', 'caller_method', 'target_type', 'target_method', 'target_assembly')
    edges = {tuple(e[k] for k in fields) for e in document['calls']}
    required = []
    for service, row in expected['serviceBindings'].items():
        for kind in ('source', 'object'):
            audit.require(audit.digest(row[kind]['path']) == row[kind]['sha256'], 'Socket source/object changed')
        for key in ('sourceMethod', 'runtimeMethod'):
            m = row[key]
            actual = methods.get((m['type'], m['name']), {})
            audit.require(actual.get('il_bytes', 0) > 1 and actual.get('signature') == m['signature'],
                          'Actual socket source/service method missing or mistyped')
        required.extend(row['actualILPath'])
        required.extend(row.get('realHostILPath', []))
    for row in expected['methodTableBindings'].values():
        required.extend(row['pointerEdges'])
        required.extend(row['tableInitializerEdges'])
    audit.require(all(tuple(e[k] for k in fields) in edges for e in required),
                  'Actual socket source/service/table call closure missing')
    for service, host in [('socket', '.ctor'), ('bind', 'Bind'), ('listen', 'Listen'),
                          ('connect', 'Connect'), ('accept', 'Accept'),
                          ('getsockname', 'get_LocalEndPoint'), ('getpeername', 'get_RemoteEndPoint')]:
        audit.require(any(e['caller_type'] == 'Managed.Interpreters.CPython+Libc'
                          and e['caller_method'] == service and e['target_type'] == 'System.Net.Sockets.Socket'
                          and e['target_method'] == host for e in document['calls']),
                      'Actual socket host service edge missing')
    return {'services': len(expected['serviceBindings']), 'requiredEdges': required,
            'actualPE': document['path'], 'sha256': document['sha256'], 'mvid': document['mvid']}


def review(ctx, selected, binding, document, evidence_index, label):
    from campaigns.recipes import helper
    audit = helper(ctx.root/'tests/Audit/audit_product.py')
    mapper = evidence_index['pdb_mapper']
    publication = evidence_index['publication']
    for item in (mapper, publication):
        audit.require(audit.digest(item['path']) == item['sha256'], 'Actual input proof helper changed')
    pub = json.loads(Path(publication['path']).read_text())
    expected = pub['forms'][selected['form']]
    pdb = bind(ctx.root, binding['pe'], selected['project'], mapper['path'], ctx.work/label/'products')
    socket = socket_review(ctx.root, document, expected)
    negatives = []
    mapper_module = helper(Path(mapper['path']))
    pe = Path(binding['pe'])
    maps, sources = project_sources(selected['project'], ctx.work/label/'products')
    def reject(case, operation):
        try:
            operation()
        except (RuntimeError, AssertionError, OSError) as error:
            negatives.append({'case': case, 'error': str(error) or type(error).__name__})
        else:
            raise RuntimeError('Invalid actual-input proof accepted: '+case)
    reject('wrong V16 tool PDB for actual product', lambda: mapper_module.bind(
        pe, ctx.root/'build/owner-a-r24-20261018/tools/compiler/dotcc.pdb', maps, sources))
    other = pub['forms']['processed' if selected['form'] == 'raw' else 'raw']['project']['path']
    reject('wrong actual compiled product project', lambda: bind(
        ctx.root, pe, other, mapper['path'], ctx.work/label/'products'))
    first = next(d for d in pdb['documents'] if Path(d['path']).name.startswith('CPython.'))
    logical = str(Path(first['pdbDocumentPath']).parent)+'/'
    reject('invented project PathMap without unique Compile input', lambda: mapper_module.bind(
        pe, pe.with_suffix('.pdb'), [('/absent-b28-physical-input/', logical)], sources))
    edge = socket['requiredEdges'][0]
    bad = dict(document, calls=[r for r in document['calls'] if r != edge])
    try:
        socket_review(ctx.root, bad, expected)
    except RuntimeError as error:
        audit.require(str(error) == 'Actual socket source/service/table call closure missing', 'Unexpected socket negative')
        negatives.append({'case': 'missing actual socket edge', 'error': str(error)})
    else:
        raise RuntimeError('Missing socket edge accepted')
    refs = {str(p): audit.digest(p) for p in sorted(Path(binding['pe']).parent.glob('*.dll'))}
    def references(values):
        audit.require(values and values.get(binding['pe']) == binding['sha256']
                      and values.get(binding['executable']) == binding['executable_sha256']
                      and str(pe.parent/'Managed.CPython.dll') in values,
                      'Actual managed reference closure incomplete')
        audit.require(all(audit.digest(p) == h for p, h in values.items()), 'Actual managed reference bytes differ')
    references(refs)
    reject('empty actual managed references', lambda: references({}))
    reject('wrong actual managed reference bytes', lambda: references(dict(refs, **{binding['pe']: '0'*64})))
    report = ctx.artifacts/(label+'-pdb-project-socket-proof.json')
    report.write_text(json.dumps({'pdb': pdb, 'socket': socket, 'references': refs, 'negatives': negatives}, indent=2)+'\n')
    return {'path': str(report), 'sha256': audit.digest(report)}
