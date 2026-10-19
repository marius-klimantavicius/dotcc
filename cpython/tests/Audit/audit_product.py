"""Structural closure/dependency audit of actual products, never engine proof.

Invoked by framework hooks. Negative fixtures exercise rejection only. No
fetch/build/publish engine and no hand-authored CPython signatures or layouts.
"""
import hashlib
from bisect import bisect_left
import json
import importlib.util
from functools import lru_cache
from collections import Counter, defaultdict
from pathlib import Path
import re
import xml.etree.ElementTree as ET


class AuditError(RuntimeError):
    pass


def require(value, message):
    if not value:
        raise AuditError(message)


def read(path):
    return json.loads(Path(path).read_text())


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


@lru_cache(maxsize=1)
def typed_data():
    # Pass this module's require function into the consumer. Loading another
    # audit_product module would create a different AuditError class identity.
    spec = importlib.util.spec_from_file_location('cpython_typed_data', Path(__file__).with_name('typed_data.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def upstream_outcomes(required_ids, events, claims=None):
    """Validate unique outcomes and occurrence events without hiding duplicates.

    A repeated successful ID is an additional event, never another completed
    required case. Conflicting repeats require adjudication and fail this gate.
    Bookkeeping validity does not convert skips/XFAILs/failures into passes.
    """
    statuses = {'passed', 'failed', 'error', 'required-skip', 'required-expected-failure',
                'unexpected-success', 'failed-subtest', 'interrupted'}
    require(required_ids and len(required_ids) == len(set(required_ids)), 'Upstream required ID closure missing/duplicate')
    required = set(required_ids)
    started, outcomes = set(), defaultdict(list)
    for row in events:
        require(row.get('case') in required and row.get('status') in statuses | {'started'},
                'Upstream unknown case/status event')
        if row['status'] == 'started':
            started.add(row['case'])
        else:
            require(row['case'] in started, 'Upstream outcome without started case')
            outcomes[row['case']].append(row['status'])
    require(all(len(set(values)) == 1 for values in outcomes.values()), 'Upstream conflicting duplicate outcomes')
    event_counts = Counter(status for values in outcomes.values() for status in values)
    unique_counts = Counter(values[0] for values in outcomes.values())
    duplicates = {case:values for case,values in outcomes.items() if len(values) > 1}
    result = {'required':len(required), 'uniqueStarted':len(started), 'uniqueCompleted':len(outcomes),
              'outcomeEvents':dict(event_counts), 'uniqueOutcomes':dict(unique_counts),
              'duplicateOutcomeOccurrences':duplicates, 'withoutCompletedOutcome':len(required-outcomes.keys()),
              'neverStarted':len(required-started), 'fatalStartedWithoutOutcome':sorted(started-outcomes.keys())}
    if claims is not None:
        require(all(isinstance(n,int) and not isinstance(n,bool) and n >= 0
                    for n in claims.get('uniqueOutcomes', {}).values())
                and sum(claims.get('uniqueOutcomes', {}).values()) == claims.get('uniqueCompleted')
                and claims.get('uniqueCompleted', len(required)+1) <= len(required),
                'Upstream impossible unique outcome total')
        for key,value in result.items():
            require(claims.get(key) == value, 'Upstream claimed counts differ from actual journal: '+key)
    result.update(status='counts-validated',
                  required_qualification_passed=len(outcomes) == len(required) and unique_counts == {'passed':len(required)},
                  scope='unique/event accounting only; required skips/XFAILs/failures/unrun remain nonpasses')
    return result


def safe_file(base, name):
    path = Path(name)
    require(isinstance(name, str) and name and not path.is_absolute() and '..' not in path.parts
            and '\\' not in name, 'Unsafe manifest path: ' + str(name))
    candidate = Path(base) / path
    require(not any(part.is_symlink() for part in [candidate, *candidate.parents])
            and candidate.resolve().is_relative_to(Path(base).resolve()), 'Manifest symlink/escape: ' + name)
    require(candidate.is_file(), 'Missing manifest artifact: ' + str(candidate))
    return candidate


def assets(directory, campaign, form, *, packaged=False, policy=None):
    if policy is None:
        from campaigns.identity import HashPolicy
        policy = HashPolicy()
    directory, campaign = Path(directory), Path(campaign)
    manifest = read(directory/'assets.json')
    pin = read(campaign/'config/source.json')
    require(manifest['profile'] == 'embedded-gil' and manifest['form'] == form,
            'Asset profile/form mismatch')
    # A revision label is provenance, not proof of original bytes. Fresh copies
    # must agree with their actual packaged ownership manifest in every mode.
    expected = {'stdlib/'+name for name in read(campaign/'config/stdlib.json')['files']}
    expected.update('notices/'+name for name in pin['notices'])
    expected.update('notices/'+row['destination'] for row in pin.get('inline_notices', []))
    rows = manifest['assets']
    names = [row['path'] for row in rows]
    require(len(names) == len(set(names)) and set(names) == expected, 'Incomplete/duplicate runtime asset closure')
    for row in rows:
        path = safe_file(directory, row['path'])
        policy.check(path, row.get('sha256'))
        require(path.stat().st_size > 0 or row['path'].endswith('__init__.py'), 'Empty required asset: ' + str(path))
    if packaged:
        actual = {p.relative_to(directory).as_posix() for name in ('stdlib', 'notices')
                  for p in (directory/name).rglob('*') if p.is_file() and '__pycache__' not in p.parts}
        require(actual == expected, 'Undeclared/missing packaged stdlib or notice')
    return {'asset_count': len(rows), 'stdlib_count': len(read(campaign/'config/stdlib.json')['files']),
            'notices': sorted(name for name in names if name.startswith('notices/')),
            'manifest_sha256': digest(directory/'assets.json') if policy.mode != 'off' else None}


def generated(project, campaign, form, *, policy=None):
    project, campaign = Path(project).resolve(), Path(campaign).resolve()
    require(project.is_file(), 'Missing actual product project: ' + str(project))
    directory = project.parent
    names = (directory/'Dotcc.SourceFiles.txt').read_text().splitlines()
    require(names and len(names) == len(set(names)), 'Missing/duplicate generated source closure')
    files = [safe_file(directory, name) for name in names]
    require(all(path.suffix == '.cs' for path in files), 'Non-C# emitted source manifest')
    actual = {p.relative_to(directory).as_posix() for p in directory.rglob('*.cs')
              if not {'obj', 'bin'}.intersection(p.relative_to(directory).parts)}
    require(set(names) == actual, 'Unlisted/missing generated source')
    owned = read(directory/'.campaign-files.json')
    require(isinstance(owned, list) and len(owned) == len(set(owned)), 'Invalid framework ownership manifest')
    require(set(names) | {project.name, 'assets.json'} <= set(owned), 'Product files omitted from ownership manifest')
    for name in owned:
        safe_file(directory, name)
    tree = ET.parse(project)
    require(tree.findtext('.//AssemblyName') == 'TranslatedCPython', 'Wrong translated assembly identity')
    require(tree.findtext('.//OutputType') == 'Library', 'Product is not a library')
    require(not list(tree.iter('NativeLibrary')) and not list(tree.iter('NativeReference')),
            'Native product engine/archive reference')
    references = []
    for tag in ('ProjectReference', 'Reference', 'PackageReference'):
        for node in tree.iter(tag):
            name = node.get('Include', '')
            require(tag == 'ProjectReference' and '$' not in name and '*' not in name,
                    'Unreviewed product reference: ' + tag + ': ' + name)
            resolved = (directory/name).resolve()
            require(resolved == campaign.parent/'DotCC.Libc/DotCC.Libc.csproj' and resolved.is_file(),
                    'Unknown product dependency: ' + str(resolved))
            references.append(str(resolved))
    linked = []
    for node in tree.iter('Compile'):
        name = node.get('Include')
        if not name:
            continue
        require('$' not in name and '*' not in name, 'Unresolved Compile entry: ' + name)
        path = (directory/name).resolve()
        if not path.is_relative_to(directory):
            require(path.is_relative_to(campaign/'src') and path.is_file(), 'Unknown authored source link: ' + str(path))
            linked.append(str(path))
    for path in files:
        # A native import table is forbidden even if it could bind at runtime.
        require(not re.search(r'\bclass\s+DotCcImports\b', path.read_text()), 'Native symbol import table: ' + str(path))
    content = {}
    asset_names = {row['path'] for row in read(directory/'assets.json')['assets']}
    for node in tree.iter('None'):
        name = node.get('Update') or node.get('Include')
        if name and (name in asset_names or name == 'assets.json'):
            require(node.get('CopyToOutputDirectory') == 'PreserveNewest'
                    and node.get('CopyToPublishDirectory') == 'PreserveNewest'
                    and node.get('TargetPath') == 'cpython/'+name, 'Runtime content packaging missing: ' + name)
            require(name not in content, 'Duplicate content packaging: ' + name)
            content[name] = True
    manifest = assets(directory, campaign, form, policy=policy)
    require(len(content) == manifest['asset_count'] + 1, 'Assets lack ordinary project copy/publish rules')
    require(asset_names <= set(owned), 'Runtime content omitted from generated ownership manifest')
    return {'scope': 'actual generated structural audit; runtime qualification separate', 'project': str(project),
            'form': form, 'source_count': len(names), 'source_files': [{'path': name, 'sha256': digest(directory/name)} for name in names],
            'references': references, 'authored_links': linked, 'assets': manifest}


def copied_payload(source, destination):
    # Fresh byte-copy correctness is independent of historical provenance or
    # hashes. A legitimate edited asset is usable in warn/off modes, and its
    # relocated bytes must still equal the artifact actually chosen this run.
    def files(directory):
        result = {}
        for path in Path(directory).rglob('*'):
            require(not path.is_symlink(), 'Symlink in copied payload: '+str(path))
            if path.is_file():
                result[path.relative_to(directory).as_posix()] = path
        return result
    before, after = files(source), files(destination)
    require(before.keys() == after.keys(), 'Relocated payload file closure differs')
    for name in before:
        require(before[name].read_bytes() == after[name].read_bytes(), 'Relocated payload bytes differ: '+name)
    return {'files': len(before), 'comparison': 'fresh selected artifact bytes; no historical hash requirement'}


def metadata(document, runtime_basis, *, product=False):
    allowed = {'TranslatedCPython', 'Managed.CPython', 'DotCC.Libc', 'ManagedConsumer', 'Delivery',
               'Behavior', 'RootedBoundary', 'Upstream', 'Abi', 'netstandard', 'mscorlib', 'Microsoft.CSharp',
               'Microsoft.Win32.Primitives', 'Microsoft.Win32.Registry'}
    require(not product or document['assembly'] == 'TranslatedCPython', 'Missing actual translated assembly')
    require(document['assembly'] in allowed or document['assembly'].startswith('System.'),
            'Unknown delivered managed assembly: ' + document['assembly'])
    for name in document['references']:
        require(name in allowed or name.startswith('System.'), 'Unknown managed dependency: ' + name)
    basis = {(row['library'], row['symbol']) for row in runtime_basis['native_imports']}
    imports = []
    for row in document['native_imports']:
        pair = row['library'], row['symbol']
        require(pair in basis, 'Unreviewed native import: ' + repr(pair))
        imports.append({**row, 'basis': 'actual shared DotCC.Libc assembly metadata',
                        'platform': 'Windows service' if row['library'].lower() in ('kernel32.dll', 'psapi.dll') else
                            'Linux libm math service' if row['library'] == 'libm.so.6' else 'Linux libc service'})
    dynamic = []
    host_calls = []

    def runtime_name(qualified):
        # Actual --nest-types metadata uses '+'. Recognize only the selected
        # shared runtime owners; an arbitrary namespace/type suffix is no basis.
        for prefix in ('', 'DotCC.Libc.', 'Managed.Interpreters.CPython+'):
            if qualified.startswith(prefix):
                name = qualified[len(prefix):]
                if name in ('Libc', 'PthreadLib', 'AtomicLib', 'NativeImports'):
                    return name
        return None

    for row in document['calls']:
        target, method, caller = row['target_type'], row['target_method'], row['caller_type']
        if runtime_name(target) in ('Libc', 'PthreadLib', 'AtomicLib') and caller.startswith('Managed.Interpreters.CPython'):
            host_calls.append(row)
        risky = (target.endswith('NativeLibrary') and method in ('Load', 'TryLoad', 'GetExport', 'TryGetExport')
                 or target.endswith('NativeImports') and method in ('LoadLibrary', 'TryResolveExport')
                 or target.endswith('Libc') and method in ('dlopen', 'dlsym')
                 or target == 'System.Diagnostics.Process' and method == 'Start'
                 or target == 'System.Reflection.Assembly' and method.startswith('Load'))
        if risky:
            # Complete runtime splicing includes definitions for unused host
            # services. They remain visible; product/facade calls into dynamic
            # loading or process execution are forbidden in this static profile.
            service = runtime_name(caller) in ('Libc', 'NativeImports')
            require(service, 'Forbidden dynamic backend/loader call: ' + repr(row))
            dynamic.append({**row, 'classification': 'shared runtime service definition; product callers forbidden'})
    if product:
        require('Managed.Interpreters.CPython' in document['types'], 'Actual translated CPython type missing')
        require(host_calls, 'No actual generated host service calls inventoried')
    return {'assembly': document['assembly'], 'sha256': document['sha256'],
            'references': document['references'], 'native_imports': imports,
            'shared_runtime_dynamic_definitions': dynamic, 'generated_host_calls': host_calls,
            'method_count': len(document['methods'])}


def exports(document, campaign):
    campaign = Path(campaign)
    header = campaign/'src/Host/dotcc_cpython_host.h'
    required = set(re.findall(r'\b(dcp_\w+)\s*\(', header.read_text()))
    methods = {row['name']: row for row in document['methods'] if row['type'] == 'Managed.Interpreters.CPython'}
    inline = read(campaign/'config/embedding.json')['export_inline']
    for name in required | set(inline):
        require(name in methods, 'Missing actual embedding export: ' + name)
        require(methods[name]['attributes'] & 0x17 == 0x16, 'Embedding export is not public static: ' + name)
    fields = {row['name']: row for row in document['fields'] if row['type'] == 'Managed.Interpreters.CPython'}
    for name in read(campaign/'config/embedding.json')['export_enum']:
        require(name in fields, 'Missing actual exported header constant: ' + name)
        require(fields[name].get('constant') is not None, 'Exported header constant lacks actual metadata value: ' + name)
    return {'actual_header': str(header), 'header_sha256': digest(header),
            'methods': {name: methods[name] for name in sorted(required | set(inline))},
            'constants': {name: fields[name] for name in read(campaign/'config/embedding.json')['export_enum']},
            'basis': 'names from authored C header/config; signatures from compiled emission; facade compilation checks actual types'}


def builtins(config, campaign, document=None):
    text = Path(config).read_text()
    match = re.search(r'\b_PyImport_Inittab\s*\[\s*\]\s*=\s*\{(.*?)\n\};', text, re.S)
    require(match is not None, 'Actual generated static module table missing')
    rows = re.findall(r'\{\s*"([^"]+)"\s*,\s*(\w+)\s*\}', match.group(1))
    names = [name for name, _ in rows]
    expected = read(Path(campaign)/'config/stdlib.json')['builtins']
    require(len(names) == len(set(names)) and sorted(names) == sorted(expected),
            'Actual static module registrations differ from reviewed product closure')
    if document is not None:
        methods = {row['name'] for row in document['methods'] if row['type'] == 'Managed.Interpreters.CPython'}
        require(all(initializer == 'NULL' or initializer in methods for _, initializer in rows),
                'Static module initializer missing from actual compiled emission')
    return {'path': str(config), 'sha256': digest(config), 'registrations': dict(rows),
            'basis': 'actual generated Modules/config.c; names compared to frozen product manifest'}


def closure(campaign, form, project):
    campaign = Path(campaign)
    pointer = read(campaign/'artifacts/campaign/current-embedded-gil.json')
    receipt_path = Path(pointer['receipt'])
    receipt = read(receipt_path)
    require(receipt['project'] == 'cpython' and receipt['profile'] == 'embedded-gil', 'Wrong translation receipt selection')
    require(Path(pointer['outputs'][form]['project']).resolve() == Path(project).resolve(), 'Current-product path differs from audited artifact')
    paths = [row['path'] for row in read(campaign/'config/sources.json')['units']]
    paths += [row['path'] for row in read(campaign/'config/embedding.json')['units']]
    observed = receipt['units']
    require(len(observed) == len(paths) and len(set(observed)) == len(observed), 'Incomplete/duplicate actual C source closure')
    require(all(Path(actual).as_posix().endswith('/'+expected) for expected, actual in zip(paths, observed)),
            'Actual translation units differ from reviewed source/Host manifest')
    commands = receipt['commands']
    emits = [row for row in commands if '--emit=obj' in row['command']]
    require(len(emits) == len(paths) and all(row['status'] == 'passed' for row in emits), 'Missing/failed real unit emission')
    link = [row for row in commands if row['label'] == 'link' and row['status'] == 'passed']
    require(len(link) == 1, 'Missing real whole-closure link')
    objects = [Path(row['command'][row['command'].index('-o')+1]) for row in emits]
    require(all(path.is_file() for path in objects) and all(str(path) in link[0]['command'] for path in objects),
            'Actual complete object set not retained/linked')
    required = ['raw-build', 'final-raw-build']
    if form == 'processed':
        required += ['postprocess', 'processed-build', 'final-processed-build']
    positions = {}
    for label in required:
        found = [i for i, row in enumerate(commands) if row['label'] == label and row['status'] == 'passed']
        require(len(found) == 1, 'Missing/ambiguous required artifact build stage: ' + label)
        positions[label] = found[0]
    if form == 'processed':
        require(positions['raw-build'] < positions['postprocess'] < positions['processed-build'],
                'Raw must build before postprocessing actual sources')
    generated = receipt['generated_inputs']
    require(any(Path(row['path']).name == 'pyconfig.h' for row in generated)
            and any(Path(row['path']).name == 'config.c' for row in generated)
            and any(Path(row['path']).name == 'getpath.h' for row in generated)
            and any(Path(row['path']).name == 'importlib._bootstrap.h' for row in generated),
            'Missing actual config/static registration/frozen startup inputs')
    for row in generated:
        require(Path(row['path']).is_file(), 'Missing generated source input: ' + row['path'])
    return {'receipt': str(receipt_path), 'units': observed, 'objects': list(map(str, objects)),
            'generated_inputs': generated, 'build_stages': positions,
            'scope': 'actual retained source/emission/build closure; no historical hash equality or behavior inferred'}


def product_identity(campaign, form, project, expected_receipt=None):
    """Pin selected bytes for this observation; no historical success is reused."""
    campaign, project = Path(campaign), Path(project).resolve()
    pointer = read(campaign/'artifacts/campaign/current-embedded-gil.json')
    receipt_path = Path(pointer['receipt']).resolve()
    if expected_receipt:
        require(receipt_path == Path(expected_receipt).resolve(), 'Selected product receipt changed')
    receipt = read(receipt_path)
    require(receipt['status'] == 'passed' and receipt['profile'] == 'embedded-gil', 'Product translation is incomplete')
    require(Path(pointer['outputs'][form]['project']).resolve() == project, 'Selected product path changed')
    owned = read(project.parent/'.campaign-files.json')
    require(isinstance(owned, list) and len(set(owned)) == len(owned), 'Invalid product ownership closure')
    files = {name: digest(safe_file(project.parent, name)) for name in sorted(owned)}
    for name in ('.campaign-files.json', 'Dotcc.SourceFiles.txt', project.name):
        files[name] = digest(safe_file(project.parent, name))
    if expected_receipt:
        require(files == pointer['outputs'][form]['hashes'], 'Expected receipt no longer describes selected product bytes')
    return {'receipt': str(receipt_path), 'receipt_sha256': digest(receipt_path),
            'form': form, 'project': str(project), 'files': files}


def d10_compiled(identity, inventory_path, il_path, document, *, dynamic_arrays=()):
    """Validate fresh source/RVA/PE evidence. Allocation/semantics are separate.

    Fingerprints here bind simultaneous evidence, rather than enforce old source
    provenance. An edited source is usable but needs evidence for its own bytes.
    """
    inventory_path, il_path = Path(inventory_path), Path(il_path)
    inventory, il = read(inventory_path), read(il_path)
    directory = Path(identity['project']).parent
    require(Path(inventory['translationReceipt']).resolve() == Path(identity['receipt'])
            and inventory['receiptSha256'].lower() == identity['receipt_sha256'], 'D10 inventory receipt mismatch')
    require(Path(inventory['source']).resolve() == directory, 'D10 inventory source directory mismatch')
    names = (directory/'Dotcc.SourceFiles.txt').read_text().splitlines()
    files = inventory['files']
    require(len(files) == len(names) and {Path(row['path']).resolve() for row in files}
            == {directory/name for name in names}, 'D10 incomplete/duplicate source inventory')
    dynamic = {row['path']: row for row in dynamic_arrays}
    require(len(dynamic) == len(dynamic_arrays), 'D10 duplicate dynamic array explanation')
    address_examples, runtime_arrays, owned_runtime_arrays = [], [], []
    example_counts = {'pin': 0, 'byref': 0, 'lazy-byref': 0}
    lazy_caches = 0
    payloads = {}
    for row in files:
        path = Path(row['path']).resolve()
        name = path.relative_to(directory).as_posix()
        require(row['sha256'].lower() == identity['files'][name] == digest(path), 'D10 stale source inventory: '+name)
        text = path.read_text(encoding='utf-8')
        lazy_caches += len(re.findall(r'\b__DotccGlobalAddressCache\s+__dotcc_\w+\s*=\s*default\b', text))
        # Match spaced/qualified spellings too. Every such remaining byte-array
        # payload needs a reviewed dynamic explanation; constant sites cannot be
        # waived merely because their original expression came from a C macro.
        sites = list(re.finditer(r'\bAsSpan\s*\(\s*new\s+(?:global::System\.)?(?:byte|Byte)\s*\[\s*\]', text))
        if sites:
            explanation = dynamic.get(name, {})
            require(explanation.get('sha256') == row['sha256'] and explanation.get('sites') == len(sites)
                    and explanation.get('reason') and explanation.get('source_evidence'),
                    'D10 remaining allocating byte payload: '+name)
            runtime_arrays.append(explanation)
        for allocation in re.finditer(r'new\s+byte\s*\[([^\]]*)\]', text):
            # Inventory the remaining allocations too. These are mutable host
            # service storage, rather than immutable initializer payloads. Their
            # full source bytes are already bound above to the chosen product.
            origins = list(re.finditer(r'^// ---- ([^\n]+) ----$', text[:allocation.start()], re.M))
            origin = origins[-1].group(1) if origins else 'generated C'
            line = text[text.rfind('\n', 0, allocation.start())+1:text.find('\n', allocation.end())]
            extent = allocation.group(1).strip()
            if origin == 'FileLib.cs' and extent == 'size' and 'buffer = new byte' in line:
                reason = 'setvbuf-owned mutable output storage when the caller supplies no buffer; requested runtime capacity'
            elif origin == 'PipeLib.cs' and extent == '65536' and 'readonly byte[] Bytes' in line:
                reason = 'per-pipe mutable bounded ring storage; each pipe requires its own identity and writable bytes'
            elif any(site.start() <= allocation.start() < text.find(')', site.start()) for site in sites):
                continue  # The explicit dynamic-payload review above owns it.
            else:
                raise AuditError('D10 unreviewed remaining byte allocation: '+name+': '+line.strip()[:200])
            owned_runtime_arrays.append({'path': name, 'line': text.count('\n', 0, allocation.start())+1,
                                         'origin': origin, 'extent': extent, 'source': line.strip(), 'reason': reason})
        for match in re.finditer(r'\b(?:internal|private)\s+static\s+(?:global::System\.)?ReadOnlySpan<byte>\s+(B[0-9A-F]{64})\s*=>\s*\[([^\]]*)\]\s*;', text):
            key, initializer = match.groups()
            require(not re.sub(r'0x[0-9A-F]{2}|[\s,]', '', initializer), 'D10 nonconstant named payload: '+key)
            data = bytes(int(value, 16) for value in re.findall(r'0x([0-9A-F]{2})', initializer))
            sha = hashlib.sha256(data).hexdigest()
            require(key[1:].lower() == sha and key not in payloads, 'D10 payload name/bytes/identity mismatch')
            payloads[key] = (len(data), sha)
        if len(address_examples) < 12:
            for number, line in enumerate(text.splitlines(), 1):
                kind = ('pin' if '__dotcc_staticBase = &' in line or re.search(
                            r'\bfixed\s*\(\s*[\w.:<>]+\s*\*\s*@?\w+_destination\s*=', line) else
                        'byref' if re.search(r'ref \w+ __dotcc_address\d+ = ref ', line) else
                        'lazy-byref' if re.search(r'\b__DotccGlobalAddressCache\s+__dotcc_\w+\s*=\s*default\b', line) else None)
                if kind and example_counts[kind] < 6:
                    address_examples.append({'kind': kind, 'path': name, 'line': number, 'text': line.strip()[:600]})
                    example_counts[kind] += 1
    require(set(dynamic) == {row['path'] for row in runtime_arrays}, 'D10 unused/stale dynamic array exemption')
    if inventory.get('schemaVersion') == 2:
        require(inventory['totals'].get('lazy_tracked_address_caches', 0) == lazy_caches,
                'D10 lazy cache source inventory mismatch')
        require((lazy_caches > 0 or (inventory['totals']['tracked_address_locals'] > 0
                and inventory['totals']['pinned_initializer_chunks'] > 0)) and address_examples,
                'D10 scoped address reuse evidence missing')
        result = typed_data().compiled(require, identity, inventory_path, il_path, document)
        result.update(address_totals=inventory['totals'], address_examples=address_examples,
                      dynamic_arrays=runtime_arrays, owned_runtime_byte_arrays=owned_runtime_arrays,
                      address_cache_metadata={
                          'fields': [row for row in document.get('fields', []) if row['type'].endswith('+__DotccGlobalAddressCache')],
                          'methods': [row for row in document['methods'] if row['type'].endswith('+__DotccGlobalAddressCache') and row['name'] == 'Get']})
        return result
    expected = {row['name']: (row['length'], row['sha256'].lower()) for row in inventory['properties']}
    require(payloads and len(expected) == len(inventory['properties']) and expected == payloads, 'D10 named payload inventory mismatch')
    require(inventory['totals'].get('lazy_tracked_address_caches', 0) == lazy_caches, 'D10 lazy cache source inventory mismatch')
    require((lazy_caches > 0 or (inventory['totals']['tracked_address_locals'] > 0 and inventory['totals']['pinned_initializer_chunks'] > 0))
            and address_examples, 'D10 scoped address reuse evidence missing')
    require(document['assembly'] == 'TranslatedCPython' and document.get('mvid'), 'D10 actual product PE/MVID missing')
    assembly = Path(document['path']).resolve()
    proof_assembly = Path(il['assembly']).resolve()
    # Ordinary project references and relocation copy identical PEs to different
    # directories. Bind their bytes rather than require one physical filename.
    require(proof_assembly.is_file() and digest(proof_assembly) == digest(assembly) == document['sha256'].lower()
            == il['assemblySha256'].lower(), 'D10 compiled PE mismatch')
    require(Path(il['manifest']).resolve() == inventory_path.resolve() and il['manifestSha256'].lower() == digest(inventory_path),
            'D10 IL/source manifest mismatch')
    require(il['status'] == 'passed' and il['newarr'] == 0 and il['propertyCount'] == len(expected)
            and il['getterCallSites'] >= len(expected), 'D10 missing/allocating/unreferenced compiled getter')
    observed = {row['name'].removeprefix('get_'): (row['length'], row['sha256'].lower()) for row in il['properties']}
    require(len(observed) == len(il['properties']) and observed == expected, 'D10 RVA payload bytes/extent mismatch')
    expected_getters = {'get_'+key for key in expected}
    getters = [row for row in document['methods'] if row['name'] in expected_getters]
    require(len(getters) == len(expected), 'D10 compiled named getter closure mismatch')
    for getter in getters:
        require(getter['type'] == 'Managed.Interpreters.CPython+CPythonLiterals'
                and getter['attributes'] & 0x10 and getter.get('newarr_count') == 0 and getter.get('il_sha256')
                and getter['il_bytes'] > 0, 'D10 wrong-owner/allocating getter: '+getter['name'])
    return {'status': 'passed', 'scope': 'actual source/receipt/PE/RVA binding; allocation and semantic gates separate',
            'identity': identity, 'assembly': str(assembly), 'proof_assembly': str(proof_assembly),
            'assembly_sha256': digest(assembly), 'mvid': document['mvid'],
            'inventory': str(inventory_path), 'inventory_sha256': digest(inventory_path),
            'il_report': str(il_path), 'il_report_sha256': digest(il_path), 'properties': len(expected),
            'payload_bytes': sum(length for length, _ in expected.values()), 'getter_call_sites': il['getterCallSites'],
            'address_totals': inventory['totals'], 'address_examples': address_examples, 'dynamic_arrays': runtime_arrays,
            'address_cache_metadata': {
                'fields': [row for row in document.get('fields', []) if row['type'].endswith('+__DotccGlobalAddressCache')],
                'methods': [row for row in document['methods'] if row['type'].endswith('+__DotccGlobalAddressCache') and row['name'] == 'Get']},
            'owned_runtime_byte_arrays': owned_runtime_arrays}


def d10_allocations(compiled, evidence):
    """Consume A's source-bound typed probe; its PE need not equal the product PE.

    The private probe has its own Program/Main and thus its own PE/MVID.
    Schema 2 requires a direct reference to the exact selected product PE;
    legacy probes retain their original project-reference and source checks.
    """
    require(evidence is not None, 'D10 actual-product allocation/semantic evidence missing')
    require(evidence['receipt_sha256'] == compiled['identity']['receipt_sha256'], 'D10 allocation receipt mismatch')
    source = read(compiled['inventory'])
    actual_files = {row['path']: row['sha256'].lower() for row in source['files']}
    require(evidence['source_files'] == actual_files, 'D10 allocation source closure mismatch')
    expected = {row['name']: (row['length'], row['sha256'].lower()) for row in source['properties']}
    typed = source.get('schemaVersion') == 2
    reports = typed_data().allocations(require, compiled, evidence) if typed else {}
    for mode in (() if typed else ('jit', 'aot')):
        entry = evidence['allocations'][mode]
        for key in ('report', 'program', 'project', 'binary', 'probe_inputs'):
            item = entry[key]
            require(Path(item['path']).is_file() and digest(item['path']) == item['sha256'], 'D10 stale allocation '+key)
        project = Path(entry['project']['path'])
        tree = ET.parse(project)
        references = [(project.parent/node.get('Include', '')).resolve() for node in tree.iter('ProjectReference')]
        require(Path(compiled['identity']['project']) in references, 'D10 probe must reference actual product project')
        require(not any((project.parent/node.get('Include', '')).resolve().parent == Path(compiled['identity']['project']).parent
                        for node in tree.iter('Compile')), 'D10 probe recompiles product sources')
        inputs = read(entry['probe_inputs']['path'])
        require(Path(inputs['project']).resolve() == Path(compiled['identity']['project'])
                and inputs['project_sha256'] == compiled['identity']['files'][Path(inputs['project']).name]
                and inputs['program_sha256'] == entry['program']['sha256']
                and inputs['probe_project_sha256'] == entry['project']['sha256'], 'D10 allocation probe input mismatch')
        require(digest(inputs['inventory']) == inputs['inventory_sha256'], 'D10 stale probe inventory')
        probe_inventory = read(inputs['inventory'])
        require({row['path']: row['sha256'].lower() for row in probe_inventory['files']} == actual_files
                and probe_inventory['receiptSha256'].lower() == compiled['identity']['receipt_sha256'],
                'D10 probe inventory source/receipt mismatch')
        result = read(entry['report']['path'])
        require(result['status'] == 'passed' and result['dynamicCode'] is (mode == 'jit'), 'D10 allocation execution mode mismatch')
        observed = {row['name']: (row['length'], row['sha256'].lower()) for row in result['properties']}
        require(len(observed) == len(result['properties']) and observed == expected, 'D10 actual-product allocation property closure mismatch')
        require(all(row['allocated'] == 0 and row['iterations'] >= 10000 for row in result['properties']), 'D10 warmed payload allocation detected')
        reports[mode] = entry
    semantics = evidence['semantics']
    require(semantics.get('categories') and semantics.get('inputs') and semantics.get('commands'), 'D10 semantic control provenance missing')
    for item in [*semantics['inputs'], *semantics['commands']]:
        require(Path(item['path']).is_file() and digest(item['path']) == item['sha256'], 'D10 stale semantic control evidence')
    for item in semantics['commands']:
        command = read(item['path'])
        require(command.get('exit') == 0 and command.get('argv') and command.get('log'), 'D10 semantic control did not execute successfully')
        require(Path(command['log']).is_file() and digest(command['log']) == item['log_sha256'], 'D10 semantic control transcript missing/changed')
    required = {'changing-base', 'side-effects', 'branches', 'volatile-atomic-tls', 'alias-writes', 'gc-owner-lifetime',
                'exact-bytes-terminators', 'writable-identity-alignment', 'initialization-order', 'native', 'direct', 'object'}
    require(required <= set(semantics['categories']), 'D10 semantic control coverage incomplete')
    test_names = []
    test_counts = []
    require(semantics.get('test_results'), 'D10 executed semantic test results missing')
    namespace = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    for item in semantics['test_results']:
        require(digest(item['path']) == item['sha256'], 'D10 stale semantic test report')
        tree = ET.parse(item['path'])
        counters = tree.find('.//t:Counters', namespace)
        results = tree.findall('.//t:UnitTestResult', namespace)
        require(counters is not None and int(counters.get('passed', '0')) > 0
                and int(counters.get('passed', '0')) == len(results)
                and all(row.get('outcome') == 'Passed' for row in results), 'D10 incomplete/failed semantic test execution')
        test_counts.append(dict(counters.attrib))
        test_names.extend(row.get('testName', '') for row in results)
    for marker in ('scoped-global-addresses', 'constant-byte-payloads',
                   'Cached_global_addresses_track_GC_and_rebind_after_owner_changing_calls'):
        require(any(marker in name for name in test_names), 'D10 missing executed semantic regression: '+marker)
    require(semantics.get('native_transcripts'), 'D10 actual native semantic control missing')
    for item in semantics['native_transcripts']:
        require(digest(item['log']) == item['log_sha256'] and digest(item['expected']) == item['expected_sha256'],
                'D10 native semantic transcript changed')
        content = Path(item['log']).read_bytes()
        # The private command recorder adds exactly one ARGV metadata line.
        # Preserve it in the artifact; compare all actual stdout bytes after it.
        if content.startswith(b'ARGV '):
            content = content.split(b'\n', 1)[1]
        require(content == Path(item['expected']).read_bytes(), 'D10 native semantic values differ')
    return {'status': 'passed', 'allocations': reports, 'semantics': semantics,
            'semantic_test_counters': test_counts,
            'tools_final_stable': evidence.get('tools_final_stable', False),
            'scope': 'source-bound focused allocation/semantic controls; interpreter matrix remains separate'}


def d10_address_qualification(compiled, evidence):
    """Bind a complete semantic review to this held source/Release PE snapshot.

    An empty unresolved list or a qualified label alone proves nothing. The
    compiler owner's analyzer binds symbols/regions; independently check its
    source coverage, site ranges, cache uses and PE observations here.
    """
    item = evidence.get('address_review') if evidence else None
    require(item is not None and Path(item['path']).is_file() and digest(item['path']) == item['sha256'],
            'D10 final address-reuse qualification report missing/changed')
    review = read(item['path'])
    receipt = read(compiled['identity']['receipt'])
    require(review.get('receipt') == receipt['run_id']
            and review.get('receipt_sha256') == compiled['identity']['receipt_sha256']
            == digest(compiled['identity']['receipt']), 'D10 address review receipt mismatch')
    require(review.get('status') == 'qualified' and not review.get('remainingConservativeLimits'),
            'D10 address reuse remains unqualified: final static-storage CSE and conservative call/label limits need review; '
            + str(item['path']))
    form = compiled['identity']['form']
    observation = review.get('forms', {}).get(form, {})
    require(digest(compiled['inventory']) == compiled['inventory_sha256'], 'D10 compiled source inventory changed')
    inventory = read(compiled['inventory'])
    require(observation.get('source_files') == {row['path']: row['sha256'] for row in inventory['files']}
            and observation.get('unresolved_pure_sites') == [], 'D10 final address review source closure/incomplete safe-site analysis')
    analysis_item = observation.get('analysis')
    require(analysis_item and Path(analysis_item['path']).is_file()
            and digest(analysis_item['path']) == analysis_item['sha256'], 'D10 actual semantic analysis missing/changed')
    analysis = read(analysis_item['path'])
    require(analysis.get('receipt_sha256') == compiled['identity']['receipt_sha256']
            and Path(analysis['receipt']).resolve() == Path(compiled['identity']['receipt']).resolve(),
            'D10 semantic analysis receipt mismatch')
    require(analysis.get('source_files') == observation['source_files'], 'D10 semantic analysis source closure mismatch')
    require(digest(analysis['inventory']) == analysis['inventory_sha256'], 'D10 semantic inventory changed')
    basis = read(analysis['inventory'])
    require(basis.get('receiptSha256') == compiled['identity']['receipt_sha256']
            and Path(basis['translationReceipt']).resolve() == Path(compiled['identity']['receipt']).resolve()
            and Path(basis['source']).resolve() == Path(compiled['identity']['project']).parent
            and {row['path']: row['sha256'] for row in basis['files']} == observation['source_files']
            and len(basis['files']) == len(observation['source_files']), 'D10 semantic inventory receipt/source mismatch')
    require(analysis.get('assembly_sha256') == compiled['assembly_sha256'] == digest(analysis['assembly'])
            == digest(compiled['assembly']) and analysis.get('mvid') == compiled['mvid'], 'D10 semantic analysis PE/MVID mismatch')
    # Required fields must actually exist, including empty diagnostic lists.
    require(analysis.get('status') == 'passed' and all(analysis.get(key) == [] for key in
            ('semantic_errors', 'unknown_cache_bindings', 'unresolved_pure_sites')), 'D10 semantic analysis remains unqualified')
    analyzer = analysis.get('analyzer', {})
    require(analyzer.get('tool') and digest(analyzer['tool']) == analyzer.get('tool_sha256')
            and analyzer.get('mvid'), 'D10 semantic analyzer identity missing/changed')
    invocation = analyzer.get('invocation', [])
    invocation_cwd = Path(analyzer.get('cwd', ''))
    require(invocation_cwd.is_absolute(), 'D10 semantic analyzer invocation working directory missing')
    require(len(invocation) == 4 and [str((invocation_cwd/path).resolve()) for path in invocation] ==
            [str(Path(path).resolve()) for path in (analyzer['tool'], analysis['inventory'], analysis['assembly'], analysis_item['path'])],
            'D10 semantic analyzer invocation mismatch')
    sources = observation.get('analyzer_sources', [])
    embedded = analyzer.get('embedded_sources')
    require(isinstance(embedded, dict) and set(embedded) == {'GeneratedAddressAudit.Program.cs', 'GeneratedAddressAudit.Project'}
            and len(sources) == len(embedded) and {row['resource'] for row in sources} == set(embedded),
            'D10 semantic analyzer source provenance missing')
    for source in sources:
        require(digest(source['path']) == source['sha256'] == embedded[source['resource']], 'D10 semantic analyzer source changed')

    # Roslyn spans count UTF-16 code units. Preserve CRLF and BOM behavior of
    # File.ReadAllText rather than using Python's newline-normalizing read_text.
    texts = {}
    for path, sha in observation['source_files'].items():
        require(digest(path) == sha, 'D10 address source changed after analysis: '+path)
        text = Path(path).read_bytes().decode('utf-8-sig')
        texts[path] = (text, text.encode('utf-16-le'))

    def span(row, path=None, prefix=''):
        path = path or row.get('path') or row.get('Path')
        require(path in texts, 'D10 address site outside source closure')
        text, units = texts[path]
        start, length = row.get(prefix+'span_start'), row.get(prefix+'span_length')
        require(type(start) is int and type(length) is int and start >= 0 and length > 0
                and 2*(start+length) <= len(units), 'D10 address site span missing/out of range')
        actual = units[2*start:2*(start+length)].decode('utf-16-le')
        require(actual == row.get(prefix+'expression'), 'D10 address site expression/span mismatch')
        return (path, start, length)

    groups = analysis.get('unmanaged_static_storage_regions', [])
    caches = analysis.get('cache_bindings_and_uses', [])
    mechanisms = analysis.get('cache_mechanisms', [])
    require(groups and caches and len(mechanisms) == 1, 'D10 empty semantic analysis/cache mechanism')
    direct, grouped_caches, group_keys, cache_storage = set(), set(), set(), {}
    for group in groups:
        key = (group['path'], group['region'], group['storage'])
        require(all(key) and key not in group_keys and group.get('sites'), 'D10 duplicate/empty storage region')
        group_keys.add(key)
        kinds = {'direct': 0, 'tracked-lazy-cache': 0}
        loop_direct = 0
        for site in group['sites']:
            require((site.get('Path'), site.get('Region'), site.get('Storage')) == key and site.get('Kind') in kinds,
                    'D10 static site/storage region binding mismatch')
            site_key = span(site)
            kinds[site['Kind']] += 1
            target = direct if site['Kind'] == 'direct' else grouped_caches
            require(site_key not in target, 'D10 duplicate static site')
            target.add(site_key)
            if site['Kind'] == 'tracked-lazy-cache':
                cache_storage[site_key] = group['storage']
            loop_direct += int(site['Kind'] == 'direct' and site.get('Loop') is True)
        require(group.get('direct') == kinds['direct'] and group.get('caches') == kinds['tracked-lazy-cache']
                and group.get('repeated_loop_direct') == loop_direct, 'D10 storage region counters disagree with actual sites')
        require(sum(kinds.values()) == 1 and loop_direct == 0, 'D10 unresolved repeated pure storage address')
    cache_keys, use_keys = set(), set()
    for cache in caches:
        key = span(cache)
        require(key not in cache_keys and cache.get('storage_type') and cache.get('uses'), 'D10 duplicate/unused cache binding')
        cache_keys.add(key)
        require(cache_storage.get(key) == cache.get('storage'), 'D10 cache storage owner mismatch')
        declaration = span(cache, prefix='declaration_')
        name = cache['getter'].removesuffix('_Get')
        require(re.fullmatch(r'\s*'+re.escape(name)+r'\s*=\s*default\s*', cache['declaration_expression'])
                and re.search(r'\bstatic\s+ref\b', cache['expression'])
                and re.search(r'\b'+re.escape(cache['getter'])+r'\s*\(', cache['expression']), 'D10 cache declaration/getter contract mismatch')
        for use in cache['uses']:
            use_key = span(use, cache['path'])
            require(use_key not in use_keys and declaration[1] < use_key[1]
                    and re.fullmatch(re.escape(name)+r'\s*\.\s*Get\s*(?:<[^<>]+>\s*)?\(\s*&\s*'+re.escape(cache['getter'])+r'\s*\)', use['expression']),
                    'D10 cache def/use binding mismatch')
            use_keys.add(use_key)
    require(cache_keys == grouped_caches, 'D10 cache getter/storage region closure mismatch')
    mechanism = mechanisms[0]
    span(mechanism)
    require(mechanism.get('valid') is True and hashlib.sha256(mechanism['expression'].encode()).hexdigest()
            == mechanism.get('source_sha256'), 'D10 tracked reference mechanism changed')
    actual_metadata = compiled.get('address_cache_metadata', {})
    pe_metadata = analysis.get('actual_pe_cache_metadata', [])
    require(len(pe_metadata) == 1 and pe_metadata[0].get('fields') == [{'name': 'value', 'signature': '061005'}]
            and len(pe_metadata[0].get('methods', [])) == 1, 'D10 actual PE tracked-reference cache field/method missing')
    require([(row['name'], row['signature']) for row in actual_metadata.get('fields', [])] == [('value', '061005')]
            and len(actual_metadata.get('methods', [])) == 1, 'D10 independently inspected PE cache metadata missing')
    method, observed_method = pe_metadata[0]['methods'][0], actual_metadata['methods'][0]
    require(method.get('name') == 'Get' and method.get('il_bytes', 0) > 0 and all(method.get(key) == observed_method.get(key)
            for key in ('name', 'signature', 'il_sha256', 'il_bytes')), 'D10 PE cache getter IL mismatch')

    remaining = analysis.get('remaining_address_conversions')
    exceptions = observation.get('exceptions')
    require(isinstance(remaining, list) and isinstance(exceptions, list), 'D10 concrete remaining-site review missing')
    remaining_keys = [span(row) for row in remaining]
    bound_remaining = {}
    for row, key in zip(remaining, remaining_keys):
        facts, references = row.get('bound_symbol'), row.get('bound_references')
        require(isinstance(facts, dict) and facts.get('symbol_kind') == row.get('symbol_kind')
                and facts.get('symbol') == row.get('symbol') and facts.get('type')
                and isinstance(references, list) and references, 'D10 remaining conversion bound ownership facts missing')
        for reference in references:
            part = span(reference, key[0])
            require(key[1] <= part[1] and part[1]+part[2] <= key[1]+key[2]
                    and isinstance(reference.get('facts'), dict), 'D10 remaining reference outside its conversion')
        for fact in [facts, *(reference['facts'] for reference in references)]:
            for declaration in fact.get('declarations') or []:
                span(declaration)
        bound_remaining[key] = (row, facts, [reference['facts'] for reference in references])
    exception_keys = []
    categories = {'changed-base', 'side-effect', 'volatile-atomic', 'tls', 'runtime-owner',
                  'tracked-ref-lifetime', 'managed-local', 'pin-scope', 'bounded-helper'}
    for exception in exceptions:
        key = span(exception)
        require(exception.get('source_sha256') == observation['source_files'][key[0]]
                and exception.get('category') in categories and exception.get('reason', '').strip()
                and not re.search(r'\b(?:all|any|every|blanket)\s+(?:calls?|labels?|macros?)\b', exception['reason'], re.I),
                'D10 missing/conservative semantic exception')
        require(key in bound_remaining, 'D10 exception does not bind to an actual remaining conversion')
        row, facts, references = bound_remaining[key]
        category = exception['category']
        if category == 'managed-local':
            require(facts.get('symbol_kind') in ('Local', 'Parameter'), 'D10 managed-local exception has another storage owner')
        elif category == 'tracked-ref-lifetime':
            require(facts.get('symbol_kind') in ('Local', 'Parameter', 'Field')
                    and facts.get('ref_kind') in ('Ref', 'RefReadOnly', 'RefReadOnlyParameter', 'In', 'Out'),
                    'D10 tracked-ref exception has no bound managed reference')
        elif category == 'volatile-atomic':
            require(any(fact.get('is_volatile') for fact in [facts, *references])
                    or 'atomic' in str(facts.get('containing_type', '')).lower(), 'D10 volatile/atomic exception has no matching bound access')
        elif category == 'tls':
            require(any(fact.get('thread_static') for fact in [facts, *references]), 'D10 TLS exception has no bound thread storage')
        elif category == 'bounded-helper':
            require(row.get('repeats_in_loop') is False, 'D10 bounded-helper exception repeats through a loop')
        exception_keys.append(key)
    require(len(set(remaining_keys)) == len(remaining_keys) and len(set(exception_keys)) == len(exception_keys)
            and set(remaining_keys) == set(exception_keys), 'D10 remaining conversion/exception closure mismatch')
    require(not direct.intersection(remaining_keys), 'D10 static site disguised as remaining exception')
    # Independently enumerate source spellings, masking comments and strings.
    # Symbol semantics still come from the complete Roslyn compilation. An
    # unfamiliar spelling disagrees with its report and fails closed.
    literals = re.compile(r'//[^\r\n]*|/\*[\s\S]*?\*/|"{3,}[\s\S]*?"{3,}|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
    pointer = re.compile(r'(?<![\w.])(?:global::)?(?:System\s*\.\s*Runtime\s*\.\s*CompilerServices\s*\.\s*)?Unsafe\s*\.\s*AsPointer\s*(?:<[^<>]+>\s*)?\(')
    cache_call = re.compile(r'\b__dotcc_(?:address\d+|initializerAddress_[0-9A-F]+)\s*\.\s*Get\s*(?:<[^<>]+>\s*)?\(')
    cache_decl = re.compile(r'\b__DotccGlobalAddressCache\s+(?P<name>__dotcc_\w+)\s*=\s*default\b')
    pointer_starts, use_starts, declaration_starts, pin_assignments, fixed_starts = set(), set(), set(), set(), set()
    for path, (text, _) in texts.items():
        code = literals.sub(lambda match: re.sub(r'[^\r\n]', ' ', match.group()), text)
        non_bmp = [match.start() for match in re.finditer('[\U00010000-\U0010ffff]', text)]
        def start(index):
            return index + bisect_left(non_bmp, index)
        pointer_starts.update((path, start(match.start())) for match in pointer.finditer(code))
        use_starts.update((path, start(match.start())) for match in cache_call.finditer(code))
        declaration_starts.update((path, start(match.start('name'))) for match in cache_decl.finditer(code))
        fixed_starts.update((path, start(match.start())) for match in re.finditer(r'\bfixed\s*\(', code))
        pin_assignments.update((path, start(match.start('name'))) for match in re.finditer(
            r'\bfixed\s*\(\s*[\w.:<>]+\s*\*\s*@?(?P<name>__dotcc_staticBase|\w+_destination)\s*=', code))
    require(pointer_starts == {(path, begin) for path, begin, _ in direct.union(remaining_keys)},
            'D10 semantic AsPointer source-site coverage incomplete')
    require(use_starts == {(path, begin) for path, begin, _ in use_keys}, 'D10 cache use source-site coverage incomplete')
    require(declaration_starts == {(row['path'], row['declaration_span_start']) for row in caches},
            'D10 cache declaration source-site coverage incomplete')
    pins = analysis.get('scoped_initializer_pins')
    require(isinstance(pins, list), 'D10 initializer pin inventory missing')
    pin_declarations, pin_variables = set(), set()
    for pin in pins:
        pin_key = span(pin)
        begin, length = pin.get('pin_scope_start'), pin.get('pin_scope_length')
        require(type(begin) is int and type(length) is int and 0 <= begin <= pin_key[1]
                and begin+length >= pin_key[1]+pin_key[2] and 2*(begin+length) <= len(texts[pin_key[0]][1]),
                'D10 initializer pin scope missing/out of range')
        scope = texts[pin_key[0]][1][2*begin:2*(begin+length)].decode('utf-16-le')
        variable = pin.get('variable_name', '__dotcc_staticBase')
        require(re.fullmatch(r'__dotcc_staticBase|\w+_destination', variable)
                and re.match(r'fixed\s*\(', scope) and variable in pin['expression']
                and pin_key not in pin_declarations, 'D10 initializer pin declaration/scope mismatch')
        pin_declarations.add(pin_key)
        pin_variables.add((pin_key[0], pin_key[1]+len(pin['expression'].split(variable, 1)[0].encode('utf-16-le'))//2))
    require(pin_assignments == pin_variables,
            'D10 initializer pin source-site coverage incomplete')
    fixed_scopes = analysis.get('fixed_scope_inventory')
    # Historical source may have no named destination pins. New reports must
    # cover every fixed statement once; source-named initializers cannot use the
    # old analyzer's partial inventory.
    if fixed_scopes is not None or any(pin.get('variable_name', '').endswith('_destination') for pin in pins):
        require(isinstance(fixed_scopes, list), 'D10 complete fixed-scope inventory missing')
        observed, initializer_scopes = set(), set()
        for fixed in fixed_scopes:
            key = span(fixed)
            begin, length = fixed.get('pin_scope_start'), fixed.get('pin_scope_length')
            require(type(begin) is int and type(length) is int and 0 <= begin <= key[1]
                    and begin+length >= key[1]+key[2] and 2*(begin+length) <= len(texts[key[0]][1]),
                    'D10 fixed scope missing/out of range')
            scope = texts[key[0]][1][2*begin:2*(begin+length)].decode('utf-16-le')
            require(re.match(r'fixed\s*\(', scope) and (key[0], begin) not in observed,
                    'D10 fixed scope duplicate or incorrect')
            observed.add((key[0], begin))
            variables = fixed.get('variables')
            require(isinstance(variables, list) and variables and all(v.get('name') and v.get('symbol_type', '').endswith('*')
                    and v.get('initializer') for v in variables), 'D10 fixed scope semantic pointer binding missing')
            named = any(v['name'] == '__dotcc_staticBase' or v['name'].endswith('_destination') for v in variables)
            require(fixed.get('initializer') is named, 'D10 fixed scope initializer classification mismatch')
            if named:
                initializer_scopes.add(key)
        require(observed == fixed_starts and initializer_scopes == pin_declarations,
                'D10 fixed-scope source-site coverage incomplete')
    require(digest(compiled['identity']['receipt']) == review['receipt_sha256']
            and digest(compiled['assembly']) == compiled['assembly_sha256']
            and all(digest(path) == sha for path, sha in observation['source_files'].items()), 'D10 held source/PE snapshot changed')
    return {'status': 'qualified', 'report': item, 'analysis': analysis_item, 'cache_bindings': len(caches),
            'cache_uses': len(use_keys), 'static_regions': len(groups), 'concrete_exceptions': len(exceptions),
            'initializer_pins': len(pins), 'fixed_scopes': len(fixed_scopes) if fixed_scopes is not None else None,
            'scope': 'receipt/source-span/actual-PE-bound semantic review; runtime matrix remains separate'}


def elf_dependencies(text):
    names = re.findall(r'\(NEEDED\).*?\[([^\]]+)\]', text)
    allowed = {'libc.so.6', 'libm.so.6', 'libdl.so.2', 'libpthread.so.0', 'librt.so.1',
               'libgcc_s.so.1', 'libstdc++.so.6', 'ld-linux-x86-64.so.2'}
    require(all(name in allowed for name in names), 'Unknown/native-engine ELF dependency: ' + repr(names))
    return names


def dependencies(directory, assembly='ManagedConsumer'):
    document = read(Path(directory)/(assembly+'.deps.json'))
    allowed = {assembly, 'TranslatedCPython', 'Managed.CPython', 'DotCC.Libc'}
    libraries = sorted(document['libraries'])
    for item in libraries:
        name = item.split('/')[0]
        require(name in allowed or name.startswith('System.') or name.startswith('Microsoft.NETCore.App.'),
                'Unknown declared runtime dependency: ' + item)
    config = read(Path(directory)/(assembly+'.runtimeconfig.json'))['runtimeOptions']
    frameworks = config.get('frameworks', []) + ([config['framework']] if 'framework' in config else [])
    require(frameworks and all(row['name'] == 'Microsoft.NETCore.App' for row in frameworks),
            'Unknown/missing .NET runtime framework')
    return {'libraries': libraries, 'frameworks': frameworks}


def payload(directory):
    suspicious = []
    for path in Path(directory).rglob('*'):
        if path.is_symlink():
            raise AuditError('Symlink in relocated payload: ' + str(path))
        name = path.name.lower()
        if (re.search(r'(libpython|pythonnet|libpypy)', name)
                or re.fullmatch(r'(?:python\d*(?:_d)?|python\.runtime)\.dll', name)
                or re.fullmatch(r'python(?:\d+(?:\.\d+)*)?(?:\.exe)?', name)
                or '.so' in name or name.endswith(('.pyd', '.dylib', '.a', '.o', '.c', '.h', '.csproj', '.cs'))):
            suspicious.append(str(path))
    require(not suspicious, 'Forbidden/unreviewed delivered native/source artifact: ' + repr(suspicious))
    return {'files': sum(p.is_file() for p in Path(directory).rglob('*')), 'forbidden_artifacts': []}
