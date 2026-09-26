#!/usr/bin/env python3
"""Audit pinned inputs, authored boundaries and qualified public consumer artifacts.

This static inventory does not claim that every generic embedded libc method is
reachable. The full consumer receipt supplies actual JIT/trimmed NativeAOT proof.
"""
import hashlib
import importlib.util
import itertools
import json
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
LOG = ROOT / 'artifacts/product-audit'
LOG.mkdir(parents=True, exist_ok=True)
spec = importlib.util.spec_from_file_location('picotls_product_audit', REPO / 'picotls/scripts/audit-product.py')
lex = importlib.util.module_from_spec(spec)
previous_bytecode = sys.dont_write_bytecode
try:
    sys.dont_write_bytecode = True
    spec.loader.exec_module(lex)
finally:
    sys.dont_write_bytecode = previous_bytecode
report = dict(passed=False, scope='static provenance/dependency inventory plus bound public consumer evidence',
              violations=[], missing=[], sources={}, projects=[], generated={}, embedded_runtime_imports=[],
              consumer_binaries={}, native_needed={}, limitations=[
                  'Generic embedded libc declarations are inventoried separately from translated-core calls.',
                  'Linux x64 receipts do not qualify unavailable platforms.',
                  'Performance and final SQLite correctness are separate mandatory qualification gates.',
                  'Lexical source checks are conservative inventories, not C# semantic binding or call-graph reachability proofs.',
                  'DT_NEEDED records direct ELF dependencies only; it does not prove transitive loader behavior or dynamic library reachability.',
                  'Framework-dependent JIT manifests do not enumerate every assembly in the installed .NET shared framework.'],
              dependency_manifests=[], consumer_asset_inventory={}, picotls_provenance={}, receipt_case_matrix=[])


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def bind(path):
    report['sources'][str(path.relative_to(REPO))] = sha(path)


def require(condition, message):
    if not condition: report['violations'].append(message)


def read(path):
    bind(path)
    return json.loads(path.read_text())


def check_hashes(mapping, base, purpose):
    require(isinstance(mapping, dict) and bool(mapping), purpose + ': empty/malformed hash inventory')
    for relative, expected in mapping.items():
        path = (base / relative).resolve()
        require(path.is_relative_to(REPO), purpose + ': path escapes workspace ' + str(path))
        if not path.is_relative_to(REPO): continue
        require(path.is_file() and sha(path) == expected, purpose + ': changed/missing ' + str(path))


def product_sources(folder):
    return sorted(p for p in folder.rglob('*.cs') if not {'bin', 'obj'}.intersection(p.relative_to(folder).parts))


def normalized_code(source):
    # Resolve escaped identifier spellings after removing literals/comments, not
    # before (a string containing "\\u0053ystem" is not an identifier).
    code = lex.code_only(source)
    code = re.sub(r'\\(?:u([0-9a-fA-F]{4})|U([0-9a-fA-F]{8}))',
                  lambda m: chr(int(m[1] or m[2], 16)), code)
    return re.sub(r'@(?=[A-Za-z_])', '', code).replace('global::', '')


ALIASES = re.compile(r'\b(global\s+)?using\s+([A-Za-z_]\w*)\s*=\s*([A-Za-z_]\w*(?:(?:\s*\.\s*|\s*::\s*)[A-Za-z_]\w*)*)\s*;')
FORBIDDEN_QUIC = re.compile(r'\bSystem\s*(?:\.|::)\s*Net\s*(?:\.|::)\s*Quic\b')


def forbidden_quic(source, path, global_aliases, global_imports=()):
    code = normalized_code(source)
    aliases = dict(global_aliases)
    aliases.update({m[2]: re.sub(r'\s+', '', m[3]).replace('::', '.') for m in ALIASES.finditer(code)})
    # Expand namespace aliases, including aliases to aliases. Merely searching
    # fully qualified names misses `using N=System.Net; N.Quic.QuicConnection`.
    for _ in range(len(aliases) + 1):
        changed = False
        for key, value in list(aliases.items()):
            head, dot, tail = value.partition('.')
            if head in aliases and head != key:
                replacement = aliases[head] + (dot + tail if dot else '')
                if replacement != value: aliases[key] = replacement; changed = True
        if not changed: break
    findings = []
    patterns = [FORBIDDEN_QUIC]
    for alias, value in aliases.items():
        if value == 'System': suffix = r'(?:\.|::)\s*Net\s*\.\s*Quic\b'
        elif value == 'System.Net': suffix = r'(?:\.|::)\s*Quic\b'
        elif value == 'System.Net.Quic' or value.startswith('System.Net.Quic.'): suffix = r'\b'
        else: continue
        patterns.append(re.compile(r'\b' + re.escape(alias) + r'\s*' + suffix))
    for imported in [*global_imports, *re.findall(r'\busing\s+(?:static\s+)?([\w.\s]+)\s*;', code)]:
        imported = re.sub(r'\s+', '', imported)
        if imported == 'System': patterns.append(re.compile(r'\bNet\s*\.\s*Quic\b'))
        if imported == 'System.Net': patterns.append(re.compile(r'\bQuic\s*\.'))
    for pattern in patterns:
        for match in pattern.finditer(code):
            findings.append(dict(path=str(path.relative_to(REPO)), line=code.count('\n', 0, match.start()) + 1,
                                 kind='System.Net.Quic namespace/type or alias', token=match[0]))
    return findings


PRODUCT_LIBRARIES = {'PublicQuicSample', 'MsQuic.ManagedApi', 'MsQuic.BclHost', 'BclProvider', 'TranslatedMsQuic', 'TranslatedPicotls'}
FORBIDDEN_DEPENDENCY = re.compile(
    r'(?:System\.Net\.Quic|(?:^|[/\\])(?:lib)?(?:msquic|picotls|quictls)'
    r'(?:\.so(?:\.|$)|\.dll$|\.dylib$|/|$))', re.I)


def consumer_dependencies(directory, identity, runtime):
    manifests = sorted(directory.rglob('*.deps.json'))
    if runtime == 'jit': require((directory / 'PublicQuicSample.deps.json').is_file(), 'JIT consumer dependency manifest missing')
    listed_assets = set()
    for manifest in manifests:
        data = read(manifest)
        inventory = dict(path=str(manifest.relative_to(REPO)), runtime_target=data.get('runtimeTarget'),
                         libraries=data.get('libraries', {}), targets=[])
        require(bool(data.get('targets')), 'Dependency manifest lacks targets: ' + str(manifest))
        for library, info in data.get('libraries', {}).items():
            name = library.split('/', 1)[0]
            # SDK project-reference aliases refer to the same authored DLL;
            # inventory them explicitly instead of mistaking them for packages.
            canonical = name.removesuffix('.Reference')
            allowed = canonical in PRODUCT_LIBRARIES or name.startswith(('Microsoft.NETCore.App', 'runtimepack.Microsoft.NETCore.App'))
            require(allowed, 'Unexpected consumer dependency: ' + library)
            require(not FORBIDDEN_DEPENDENCY.search(library), 'Forbidden consumer dependency: ' + library)
        for target, libraries in data.get('targets', {}).items():
            for library, info in libraries.items():
                row = dict(target=target, library=library, dependencies=info.get('dependencies', {}),
                           assembly_assets=info.get('runtime', {}), native_assets=info.get('native', {}),
                           runtime_targets=info.get('runtimeTargets', {}), resources=info.get('resources', {}))
                inventory['targets'].append(row)
                for dependency in info.get('dependencies', {}):
                    require(not FORBIDDEN_DEPENDENCY.search(dependency), 'Forbidden dependency edge: ' + dependency)
                for category in ['runtime', 'native', 'runtimeTargets', 'resources']:
                    for asset in info.get(category, {}):
                        require(not FORBIDDEN_DEPENDENCY.search(asset), 'Forbidden consumer asset: ' + asset)
                        listed_assets.add(Path(asset).name)
        report['dependency_manifests'].append(inventory)
    assets = []
    for path in sorted(directory.rglob('*')):
        if not path.is_file(): continue
        relative = str(path.relative_to(directory))
        require(not path.is_symlink(), 'Symlink in consumer output: ' + str(path))
        require(not FORBIDDEN_DEPENDENCY.search(relative), 'Forbidden output asset: ' + relative)
        with path.open('rb') as artifact:
            magic = artifact.read(4)
        kind = ('managed assembly candidate (.dll; not a metadata reachability proof)' if path.suffix == '.dll' else
                'native shared library' if '.so' in path.name or path.suffix == '.dylib' else
                'ELF executable/image' if magic == b'\x7fELF' else 'metadata/debug/other')
        assets.append(dict(path=relative, kind=kind, sha256=sha(path), mentioned_in_deps=path.name in listed_assets))
        if runtime == 'jit' and path.suffix == '.dll':
            require(path.name in listed_assets, 'JIT assembly absent from dependency manifests: ' + relative)
    report['consumer_asset_inventory'][identity] = assets


try:
    pin = read(ROOT / 'config/source.json')
    closure = read(ROOT / 'config/product-closure.json')
    stage_path = ROOT / 'build/product-source/manifest.json'
    stage = read(stage_path)
    require(closure['revision'] == pin['commit'] == stage['revision'], 'Source revision mismatch')
    require(sha(stage_path) == closure['stage_manifest_sha256'], 'Staging manifest differs from qualified closure')
    git_defines = [value.split('=', 1)[1] for value in stage['defines'] if value.startswith('VER_GIT_HASH=')]
    require(len(git_defines) == 1 and git_defines[0] in (pin['commit'], '"' + pin['commit'] + '"'),
            'Compiled source revision metadata is not the pin')
    require(sha(ROOT / 'config/api-profile.json') == closure['api_profile_sha256'], 'API profile differs from closure')
    require(stage['units'] == closure['units'], 'Translation unit closure changed')
    reference = ROOT / 'ref' / pin['directory']
    reference_verification = closure.get('reference_verification', 'archive')
    require(reference_verification in ('archive', 'local-source'), 'Unknown source verification mode')
    if reference_verification == 'archive':
        require(sha(ROOT / 'ref' / pin['archive']) == pin['sha256'], 'Pinned source archive mismatch')
    for item in closure['source_files']:
        staged = ROOT / 'build/product-source' / item['path']
        require(staged.is_file() and sha(staged) == item['sha256'], 'Staged source changed: ' + item['path'])
        origin = reference / item['origin'] if item['role'] == 'unchanged upstream' else ROOT / item['origin']
        require(origin.is_file() and sha(origin) == item['sha256'], 'Authored/upstream input changed: ' + str(origin))
        if origin.is_file(): bind(origin)
    check_hashes(closure['evidence_sha256'], ROOT, 'P2 evidence')
    if reference_verification == 'local-source':
        abi = read(ROOT / 'artifacts/abi/results.json')
        require(abi.get('reference_verification') == 'local-source' and abi.get('reference_stable'),
                'Local source closure lacks matching ABI evidence')
        check_hashes(abi.get('local_reference_sha256', {}), ROOT, 'Local reference inputs')
    # The frozen emitter directory is the actual compiler used by the P2 object
    # campaign; a separately rebuilt CLI is not a substitute for its provenance.
    check_hashes(closure['compiler_hashes'], ROOT / 'build/host-contract/compiler', 'Frozen compiler')
    bind(Path(__file__).resolve())
    bind(REPO / 'picotls/scripts/audit-product.py')
    pico_path = REPO / 'picotls/artifacts/translation/success.json'
    pico = read(pico_path)
    require(pico.get('format') == 'picotls-translation-v1', 'Unsupported picotls provenance format')
    for name, value in lex.input_state().items():
        require(pico.get(name) == value, 'Reused picotls translation input changed: ' + name)
    check_hashes(pico.get('tool_sha256', {}), REPO, 'picotls compiler/postprocessor provenance')
    report['picotls_provenance'] = dict(path=str(pico_path.relative_to(REPO)), sha256=sha(pico_path),
                                       tool_sha256=pico.get('tool_sha256', {}), input_revision=pico['inputs']['picotls']['revision'])
    patterns = dict(lex.PATTERNS)
    patterns['native TLS facade'] = re.compile(r'\bSslStream\b')
    folders = [ROOT / 'src/BclHost', ROOT / 'src/ManagedApi', REPO / 'picotls/src/BclProvider', ROOT / 'samples/ManagedConsumer']
    authored = [path for folder in folders for path in product_sources(folder)]
    global_aliases = {m[2]: re.sub(r'\s+', '', m[3]).replace('::', '.') for path in authored
                      for m in ALIASES.finditer(normalized_code(path.read_text())) if m[1]}
    global_imports = [name for path in authored for name in
                      re.findall(r'\bglobal\s+using\s+([\w.\s]+)\s*;', normalized_code(path.read_text()))]
    for folder in folders:
        for path in product_sources(folder):
            bind(path)
            code = normalized_code(path.read_text())
            report['violations'].extend(forbidden_quic(path.read_text(), path, global_aliases, global_imports))
            for name, pattern in patterns.items():
                for match in pattern.finditer(code):
                    report['violations'].append(dict(path=str(path.relative_to(REPO)),
                        line=code.count('\n', 0, match.start()) + 1, kind=name, token=match[0]))
        for path in sorted(folder.glob('*.csproj')):
            bind(path)
            nodes = list(ET.parse(path).iter())
            refs = [dict(kind=n.tag.split('}')[-1], **n.attrib) for n in nodes if n.tag.split('}')[-1] in
                    ('ProjectReference', 'PackageReference', 'Reference', 'FrameworkReference')]
            report['projects'].append(dict(path=str(path.relative_to(REPO)), references=refs))
            require(not any(n.tag.split('}')[-1] in ('PackageReference', 'Reference') for n in nodes), 'Unexpected external authored project dependency: ' + str(path))
    marker = lex.RUNTIME_MARKER
    for variant in ['raw', 'optimized']:
        directory = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        check_hashes(closure['generated'][variant], directory, variant + ' generated closure')
        names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
        require(bool(names) and len(names) == len(set(names)) and
                all(Path(name).name == name and name.endswith('.cs') for name in names), 'Invalid generated source manifest')
        actual = {str(p.relative_to(directory)) for p in directory.rglob('*.cs')
                  if not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
        require(actual == set(names), 'Unmanifested generated sources: ' + variant)
        require(set(names) == {n for n in closure['generated'][variant] if n.endswith('.cs')}, 'Compiler source list differs from closure: ' + variant)
        report['generated'][variant] = {name: sha(directory / name) for name in names}
        for name in names:
            path = directory / name
            source = path.read_text()
            translated, separator, runtime = source.partition(marker)
            for finding in lex.findings(translated, str(path.relative_to(REPO))):
                report['violations'].append(finding)
            report['violations'].extend(forbidden_quic(translated, path, {}))
            if separator:
                report['embedded_runtime_imports'].extend(lex.native_imports(runtime, str(path.relative_to(REPO)), translated.count('\n')))
        pico_directory = REPO / 'picotls/generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
        pico_files = lex.generated_files(pico_directory)
        require({p.name: sha(p) for p in pico_files} == pico[variant], 'Reused picotls generated closure differs: ' + variant)
        report['generated']['picotls-' + variant] = {p.name: sha(p) for p in pico_files}
        for path in pico_files:
            if path.suffix != '.cs': continue
            translated, separator, runtime = path.read_text().partition(marker)
            report['violations'].extend(lex.findings(translated, str(path.relative_to(REPO))))
            report['violations'].extend(forbidden_quic(translated, path, {}))
            if separator:
                report['embedded_runtime_imports'].extend(lex.native_imports(runtime, str(path.relative_to(REPO)), translated.count('\n')))
    consumer_path = ROOT / 'artifacts/public-consumer/results.json'
    if not consumer_path.is_file():
        report['missing'].append('Separate public consumer receipt')
    else:
        consumer = read(consumer_path)
        require(consumer.get('passed') is True, 'Public consumer full raw/optimized JIT/NativeAOT gate is pending')
        require(consumer.get('exact_source_metadata_required') is True, 'Consumer did not require exact compiled source metadata')
        require(consumer.get('source_revision') == pin['commit'], 'Consumer source revision differs')
        require(consumer.get('product_closure_sha256') == sha(ROOT / 'config/product-closure.json'), 'Consumer used a different product closure')
        require(consumer.get('picotls_translation_sha256') == sha(pico_path), 'Consumer used different picotls provenance')
        require(consumer.get('profile_sha256') == sha(ROOT / 'config/api-profile.json'), 'Consumer used a different selected profile')
        # Both supported receipt names are bound maps, never an existence-only check.
        inputs = consumer.get('input_sha256', consumer.get('source_hashes', {}))
        require(bool(inputs), 'Consumer receipt lacks source hashes')
        check_hashes(inputs, ROOT, 'Public consumer input')
        for path in authored + [ROOT / 'scripts/test-public-consumer.py']:
            key = str(path.relative_to(ROOT)) if path.is_relative_to(ROOT) else '../' + str(path.relative_to(REPO))
            require(inputs.get(key) == sha(path), 'Current authored input absent from consumer receipt: ' + key)
        variants = consumer.get('variants', [])
        require(len(variants) == 2 and {v['name'] for v in variants} == {'raw', 'optimized'}, 'Public consumer variants incomplete/duplicated')
        expected_cases = set(itertools.product(['raw', 'optimized'], ['jit', 'aot'], ['ipv4', 'ipv6'], [None, 'wrong-trust', 'wrong-name', 'wrong-alpn']))
        cases = consumer.get('cases', [])
        actual_cases = [(c.get('variant'), c.get('runtime'), c.get('family'), c.get('negative')) for c in cases]
        require(len(actual_cases) == len(expected_cases) and set(actual_cases) == expected_cases, 'Consumer authentication/roundtrip case matrix incomplete/duplicated')
        for case in cases:
            require(case.get('passed') is True, 'Consumer case is not passing: ' + str(case.get('name')))
            require(all(case.get(role, {}).get('clean_close') is True for role in ['client', 'server']), 'Consumer case did not drain owners')
            for role in ['client', 'server']:
                metadata = case.get(role + '_metadata', {})
                require(metadata.get('source_revision') == pin['commit'] and metadata.get('effective_versions') == [1]
                        and metadata.get('tls_provider') == 'picotls' and metadata.get('aot') == (case.get('runtime') == 'aot'),
                        'Consumer case metadata mismatch: ' + str(case.get('name')))
            report['receipt_case_matrix'].append(dict(name=case.get('name'), passed=case.get('passed'),
                variant=case.get('variant'), runtime=case.get('runtime'), family=case.get('family'), negative=case.get('negative')))
        for variant in variants:
            require(variant.get('passed') is True, 'Consumer variant did not pass')
            require(variant.get('generated_sha256') == {**closure['generated'][variant['name']]}, 'Consumer MsQuic generated inputs differ')
            require(variant.get('picotls_generated_sha256') == pico[variant['name']], 'Consumer picotls generated inputs differ')
            runtimes = variant.get('runtimes', {})
            require(set(runtimes) == {'jit', 'aot'}, 'Public consumer runtimes incomplete')
            for runtime, entry in runtimes.items():
                require(entry.get('passed') is True, 'Consumer runtime did not pass')
                directory = Path(entry['output']).resolve()
                require(directory.is_relative_to(ROOT / 'build/public-consumer') and directory.is_dir(), 'Consumer output is outside its build directory or missing')
                if not directory.is_relative_to(ROOT / 'build/public-consumer') or not directory.is_dir(): continue
                recorded = entry['binary_sha256']
                require(bool(recorded), 'Consumer runtime binary inventory is empty')
                actual = {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*') if p.is_file()}
                require(actual == recorded, 'Public consumer binaries changed: ' + str(directory))
                report['consumer_binaries'][variant['name'] + '-' + runtime] = recorded
                consumer_dependencies(directory, variant['name'] + '-' + runtime, runtime)
                if runtime == 'aot':
                    executable = directory / 'PublicQuicSample'
                    output = subprocess.run(['readelf', '-d', str(executable)], check=True, capture_output=True, text=True).stdout
                    needed = re.findall(r'\(NEEDED\).*?\[(.*?)\]', output)
                    report['native_needed'][variant['name']] = needed
                    require(not any(re.search(r'(?:msquic|picotls|libssl|quictls)', n, re.I) for n in needed), 'Native QUIC/TLS backend dependency')
    for name, digest in report['sources'].items():
        path = REPO / name
        require(path.is_file() and sha(path) == digest, 'Audit input changed during inventory: ' + name)
    report['passed'] = not report['violations'] and not report['missing']
except Exception as error:
    report['missing'].append(str(error))
finally:
    (LOG / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(dict(passed=report['passed'], violations=len(report['violations']), missing=report['missing'])))
sys.exit(0 if report['passed'] else 1)
