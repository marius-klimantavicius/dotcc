"""Build original upstream C harnesses with an existing complete campaign tool/object snapshot.

All outputs are private. Never rebuild tools, change generated C#, edit pinned
tests, weaken assertions, or capability-skip a required case.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import uuid
import xml.etree.ElementTree as ET
from feature_adaptations import apply as apply_features

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.delivery import ignore_build_caches
from campaigns.translation import relocate_project
sys.path.insert(0, str(ROOT / 'tests/Managed'))
from execution_identity import authored_sources, capture, hashes
sys.path.insert(0, str(ROOT / 'scripts'))
from translation_evidence import read_closed
PRELUDE = b'#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n'


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def object_exports(path):
    return set(re.findall(r'^//!!dotcc-obj (?:def|global-def):(\w+)', path.read_text(), re.M))


def object_references(path):
    text = path.read_text()
    references = set(re.findall(r'^//!!dotcc-obj import:(\w+)', text, re.M))
    field_counts = {'address_declaration': 3, 'address_use': 4, 'context_address': 2,
                    'initializer_address': 3, 'bound_initializer_address': 3}
    pattern = r'/\*__dotcc_(' + '|'.join(field_counts) + r')__\*/([A-Za-z_0-9@]*)'
    # Address-taking and externally defined fixture globals also participate.
    for part in text.split('//!!dotcc-obj section:')[1:]:
        if part.startswith(('functions\n', 'global-initializer\n')):
            fragments, seen = [part], set()
            while fragments:
                fragment = fragments.pop()
                if fragment in seen: continue
                seen.add(fragment)
                references.update(re.findall(r'\b[A-Za-z_]\w*\b', fragment))
                # GlobalStorageReferences.AddressRecord retains the C root
                # inside NUL-separated UTF-8 hex until the final object link.
                # Context/type fields may themselves contain global refs or
                # nested address records, so inspect those decoded fields too.
                for kind, record in re.findall(pattern, fragment):
                    try:
                        fields = bytes.fromhex(record[1:]).decode('utf-8').split('\0')
                    except (ValueError, UnicodeError) as error:
                        raise RuntimeError('Malformed emitted global address reference: ' + str(path)) from error
                    if (not record.startswith('C') or len(fields) != field_counts[kind]
                        or not re.fullmatch(r'@?[A-Za-z_]\w*', fields[0])):
                        raise RuntimeError('Malformed emitted global address reference: ' + str(path))
                    references.add(fields[0].removeprefix('@'))
                    fragments.extend(fields[1:])
    return references


def external_roots(path):
    text = path.read_text()
    inline = set()
    for match in re.finditer(r'^//!!dotcc-obj inline:(\w+)\s+(\w+)', text, re.M): inline.update(match.groups())
    return {name for name in object_exports(path) - inline if '__unit_' not in name}


def dependencies(path):
    body = path.read_text().replace('\\\n', ' ').split(':', 1)[1]
    return sorted({(Path(value) if Path(value).is_absolute() else ROOT / value).resolve() for value in shlex.split(body)})


def verify_dependency_inputs(entry):
    captured = entry.get('dependency_inputs_sha256')
    if not captured:
        raise RuntimeError('Reusable emission lacks its original dependency contents; emit again with the selected compiler')
    observed = hashes(dependencies(Path(entry['dependency_file'])))
    if captured != observed:
        raise RuntimeError('Reusable emission dependency contents changed after compilation')


def postprocess_private(raw, processed, postprocessor, command):
    """Evaluate and rewrite only a private semantic tree and copied link inputs."""
    shutil.copytree(raw, processed, ignore=ignore_build_caches)
    project = processed / 'S2nTls.csproj'
    relocate_project(project, raw)
    tree = ET.parse(project)
    links = processed.parent / 'semantic-linked'; links.mkdir()
    original_nodes, copied = [], []
    for index, node in enumerate(tree.iter('Compile')):
        if not node.get('Include'): continue
        source = (processed / node.get('Include')).resolve()
        if source.is_relative_to(processed): continue
        target = links / (str(index) + '-' + source.name)
        shutil.copyfile(source, target)
        original_nodes.append((node, dict(node.attrib)))
        copied.append((source, target))
        node.set('Include', str(target))
    artifacts = processed.parent / 'semantic-artifacts'
    for node in tree.iter('ProjectReference'):
        original_nodes.append((node, dict(node.attrib)))
        previous = node.get('AdditionalProperties', '')
        node.set('AdditionalProperties', previous + (';' if previous else '') +
                 'UseArtifactsOutput=true;ArtifactsPath=' + str(artifacts))
    props = processed / 'Directory.Build.props'
    if props.exists(): raise RuntimeError('Refusing to replace an existing private semantic property file')
    settings = ET.Element('Project')
    ET.SubElement(settings, 'Import', Project=str(ROOT.parent / 'Directory.Build.props'))
    group = ET.SubElement(settings, 'PropertyGroup')
    for key, value in {'UseArtifactsOutput': 'true', 'ArtifactsPath': str(artifacts),
                       'GeneratedMSBuildEditorConfigFile': str(processed / 'obj/Release/S2nTls.GeneratedMSBuildEditorConfig.editorconfig')}.items():
        ET.SubElement(group, key).text = value
    ET.indent(settings); ET.ElementTree(settings).write(props, encoding='unicode')
    (processed / 'obj/Release').mkdir(parents=True, exist_ok=True)
    ET.indent(tree); tree.write(project, encoding='unicode')
    authored = authored_sources(ROOT)
    authored.update(hashes(source for source, target in copied))
    before = hashes(target for source, target in copied)
    generated_before = hashes(processed.glob('*.cs'))
    receipt = {'tool_sha256': digest(postprocessor), 'original_authored_sha256': authored,
               'semantic_linked_sha256': before, 'semantic_properties': props.read_text(),
               'generated_before_sha256': generated_before}
    try:
        command(['dotnet', 'build', project, '-c', 'Release', '--artifacts-path', artifacts,
                 '-m:1', '-p:UseSharedCompilation=false'], timeout=1200)
        command(['dotnet', postprocessor, project, '--in-place'], timeout=900)
        if hashes(before) != before: raise RuntimeError('Postprocessor changed a private authored input copy; originals remain intact')
        if hashes(authored) != authored: raise RuntimeError('Original authored input changed during semantic processing')
        receipt['generated_after_sha256'] = hashes(processed.glob('*.cs'))
        receipt['status'] = 'passed'
    finally:
        for node, attributes in original_nodes:
            node.attrib.clear(); node.attrib.update(attributes)
        ET.indent(tree); tree.write(project, encoding='unicode')
        props.unlink()
    return receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--campaign-run', type=Path, required=True, help='Existing complete build/campaign/<run> directory')
    parser.add_argument('--compiler', type=Path, help='Optional immutable repaired dotcc.dll snapshot; retains the original complete product objects')
    parser.add_argument('--reuse-emission', type=Path, help='Reuse verified emitted harness objects from the exact same campaign/tool snapshot; missing entries fail explicitly')
    parser.add_argument('--reuse-support-manifest', type=Path, action='append', default=[], help='Reuse exact successful original support TU emissions from same-campaign executable manifests')
    parser.add_argument('--case', action='append', help='Exact required source path; default builds every applicable C case')
    parser.add_argument('--endpoint', action='append', choices=('s2nc', 's2nd'), help='Build test-only original CLI for unmodified Python integration cases')
    parser.add_argument('--provider-project', type=Path, help='Select one test-only provider assembly instead of the production project')
    parser.add_argument('--provider-binding', type=Path, action='append', default=[], help='Additional test-only C# binding; repeatable, frozen per harness')
    parser.add_argument('--provider-policy', type=Path, help='Per-case reviewed test provider catalogue; other cases retain the production provider')
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--output', type=Path, required=True, help='Executable manifest, including failed/unrun entries')
    parser.add_argument('--emit-only', action='store_true', help='Bounded C test emission probe; no link, product build or execution claim')
    args = parser.parse_args()
    if args.provider_policy and (args.provider_project or args.provider_binding):
        parser.error('--provider-policy cannot be combined with --provider-project/--provider-binding')
    run = args.campaign_run.resolve()
    run_id = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
    work = ROOT / 'build/managed-upstream' / run_id
    work.mkdir(parents=True)
    logs = ROOT / 'artifacts/managed-upstream' / run_id
    logs.mkdir(parents=True)
    manifest = {'status': 'failed', 'campaign_run': str(run), 'commands': [], 'executables': [], 'omissions': []}
    if args.emit_only:
        manifest['evidence_kind'] = 'upstream-object-emission-only'
        manifest['omissions'].append('No harness linking, managed product build or upstream assertions executed (--emit-only).')
    sequence = 0

    def command(argv, timeout=300):
        nonlocal sequence
        sequence += 1
        log = logs / f'{sequence:05d}.log'
        argv = list(map(str, argv))
        with log.open('w') as stream:
            try:
                result = subprocess.run(argv, cwd=ROOT, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout)
                code = result.returncode
            except subprocess.TimeoutExpired:
                code = 'timeout'
        manifest['commands'].append({'command': argv, 'exit_code': code, 'log': str(log)})
        if code != 0: raise RuntimeError(f'Command failed ({code}): {log}')

    inventory = json.loads((ROOT / 'config/upstream-tests.json').read_text())['cases']
    required = [r for r in inventory if r['status'].startswith('required')]
    cases = [r for r in required if r['path'].endswith('.c')]
    manifest['required_python_families'] = [r['path'] for r in required if r['path'].endswith('.py')]
    manifest['python_runner'] = 'tests/Upstream/python-integration.py; separate original CLI endpoint manifests are required'
    if args.endpoint:
        if args.case: parser.error('--endpoint and --case are separate bounded selections')
        cases = [{'path': 'bin/' + name + '.c'} for name in args.endpoint]
        manifest['omissions'].append('Test-only original s2nc/s2nd CLI harness selection; not product delivery binaries.')
    if args.case:
        if set(args.case) - {r['path'] for r in cases}: raise RuntimeError('--case must identify an exact required frozen case')
        cases = [r for r in cases if r['path'] in args.case]
        manifest['omissions'].append('Explicit bounded --case selection; full upstream gate remains open.')
    forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
    modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
    for case in cases:
        for form in forms:
            for mode in modes:
                manifest['executables'].append({'case': case['path'], 'form': form, 'mode': mode, 'status': 'unrun'})
    try:
        # Require the real product gate before any managed harness build. A
        # partial emission snapshot cannot silently become an alternate product.
        canonical = ROOT / 'generated/TranslatedS2nTls/TranslatedS2nTls.csproj'
        if not canonical.is_file() and not args.emit_only: raise RuntimeError('Canonical complete translated product is not published: ' + str(canonical))
        receipt_path, receipt = read_closed(ROOT, run, forms=('raw',) if args.form == 'raw' else ('raw', 'processed'))
        manifest['translation_receipt'] = {'path': str(receipt_path), 'sha256': digest(receipt_path)}
        first = next(c['command'] for c in receipt['commands'] if '--emit=obj' in c['command'])
        original_compiler = Path(first[1])
        compiler = args.compiler.resolve() if args.compiler else original_compiler
        if not compiler.is_file(): raise RuntimeError('Original campaign tool snapshot missing')
        expected = len(json.loads((ROOT / 'config/source-manifest.json').read_text())['units']) + 1
        objects = sorted((run / 'objects').glob('*.cs'))
        if len(objects) != expected: raise RuntimeError(f'Expected complete {expected}-object product; found {len(objects)}')
        for path in objects:
            if not path.read_text().startswith('//!dotcc object'): raise RuntimeError('Invalid emitted object: ' + str(path))
        manifest['original_product_tool_sha256'] = {str(original_compiler): digest(original_compiler)}
        manifest['tool_sha256'] = {str(p): digest(p) for p in sorted(compiler.parent.rglob('*'))
                                   if p.is_file() and p.suffix in ('.dll', '.json')}
        manifest['product_objects_sha256'] = {str(p): digest(p) for p in objects}
        reused_units = {}
        reused_support = {}
        if args.reuse_emission:
            reuse = json.loads(args.reuse_emission.read_text())
            if Path(reuse['campaign_run']).resolve() != run or reuse['tool_sha256'] != manifest['tool_sha256']:
                raise RuntimeError('Reusable harness emission must match the exact campaign and compiler snapshot')
            if reuse['product_objects_sha256'] != manifest['product_objects_sha256']:
                raise RuntimeError('Reusable harness emission core object identities differ')
            manifest['reused_emission_manifest'] = {'path': str(args.reuse_emission.resolve()), 'sha256': digest(args.reuse_emission)}
            for entry in reuse['executables']:
                if entry['status'] != 'emitted': continue
                previous = reused_units.setdefault(entry['case'], entry)
                if previous['object_sha256'] != entry['object_sha256']:
                    raise RuntimeError('Reusable harness emission has ambiguous objects for one case')
        for path in args.reuse_support_manifest:
            previous = json.loads(path.read_text())
            if (Path(previous['campaign_run']).resolve() != run or previous['tool_sha256'] != manifest['tool_sha256']
                or previous['product_objects_sha256'] != manifest['product_objects_sha256']):
                raise RuntimeError('Reusable original support must match the exact campaign, compiler and core objects')
            provenance = {'path': str(path.resolve()), 'sha256': digest(path)}
            manifest.setdefault('reused_support_manifests', []).append(provenance)
            for entry in previous.get('support_emissions', []):
                if entry['status'] != 'emitted': continue
                original = Path(entry['source'])
                stage_root = next((p for p in original.parents if p.name == 'source'), None)
                if stage_root is None: raise RuntimeError('Reusable support source has no exact private stage root')
                relative = original.relative_to(stage_root)
                frozen = ROOT / 'ref/s2n-tls' / relative
                if (digest(frozen) != entry['original_sha256'] or original.read_bytes() != PRELUDE + frozen.read_bytes()
                    or digest(entry['object']) != entry['sha256'] or digest(entry['dependency_file']) != entry['dependency_sha256']):
                    raise RuntimeError('Reusable original support emission provenance changed: ' + str(relative))
                verify_dependency_inputs(entry)
                # Choose one complete original TU by exact source identity;
                # never merge function names or prune its external roots.
                reused_support.setdefault(relative, (entry, provenance))
        original_units = [Path(value).resolve() for value in receipt['units']]
        if len(original_units) != len(objects): raise RuntimeError('Original object/source inventory is incomplete')
        core_units = list(zip(objects, original_units))
        for index, (obj, source) in enumerate(core_units):
            if not obj.name.startswith(f'{index:03d}-' + source.stem):
                raise RuntimeError('Original object/source ordering is ambiguous: ' + str(obj))
        options = []
        skip_next = False
        for item in first[2:]:
            if skip_next: skip_next = False; continue
            if item in ('--override-report', '-o'): skip_next = True; continue
            if item == '--emit=obj' or item.endswith('.c'): continue
            options.append(item)
        # Provider authoring may continue independently. Freeze each selected
        # override input for this run rather than mixing revisions across cases.
        manifest['override_inputs_sha256'] = {}
        for i, value in enumerate(options):
            if i and options[i - 1] == '--overrides-file':
                selected = Path(value)
                frozen = work / 'tool-inputs' / (str(i) + '-' + selected.name)
                frozen.parent.mkdir(exist_ok=True)
                shutil.copyfile(selected, frozen)
                manifest['override_inputs_sha256'][str(selected)] = digest(frozen)
                options[i] = str(frozen)
        if args.endpoint:
            # Test-only original CLI standard execution services. Freeze the
            # provider profile above, then extend only this private profile.
            index = options.index('--overrides-file') + 1
            extended = json.loads(Path(options[index]).read_text())
            services = json.loads((ROOT / 'tests/Upstream/getopt-overrides.json').read_text())
            extended['functionOverrides'].extend(services['functionOverrides'])
            profile = work / 'tool-inputs/endpoint-overrides.json'
            profile.write_text(json.dumps(extended, indent=2) + '\n')
            options[index] = str(profile)
            options += ['-I', str(ROOT / 'tests/Upstream/include')]
            manifest['test_service_inputs_sha256'] = {str(p): digest(p) for p in
                (ROOT / 'tests/Upstream/getopt-overrides.json', ROOT / 'tests/Upstream/include/getopt.h',
                 ROOT / 'tests/Upstream/GetoptBindings.cs', ROOT / 'tests/Upstream/EndpointMain.cs')}
            manifest['test_service_inputs_sha256'][str(ROOT / 'tests/Upstream/StdfdBindings.cs')] = digest(ROOT / 'tests/Upstream/StdfdBindings.cs')
        stage = work / 'source'
        shutil.copytree(ROOT / 'ref/s2n-tls/tests', stage / 'tests')
        shutil.copytree(ROOT / 'ref/s2n-tls/docs/examples', stage / 'docs/examples')
        shutil.copytree(ROOT / 'ref/s2n-tls/bin', stage / 'bin')
        # Both s2n_test.h and tests/s2n_test.h must resolve to this same
        # physical private header, preserving upstream pragma-once identity.
        options = ['-I', str(stage / 'tests'), '-I', str(stage), *options]
        support = ([stage / 'bin' / name for name in ('common.c', 'echo.c', 'https.c')] if args.endpoint else
                   sorted((stage / 'tests/testlib').glob('*.c')) + sorted((stage / 'docs/examples').glob('*.c')))
        support_index = {}
        global_declarations = []
        for header in (stage / 'tests/testlib').glob('*.h'):
            for declaration in re.findall(r'(?m)^extern\s+[^;]+;', header.read_text()):
                match = re.search(r'\b(\w+)\s*(?:\[[^]]*\])?\s*;', declaration)
                if match: global_declarations.append(match.group(1))
        manifest['support_source_index'] = []
        for source in support:
            text = source.read_text()
            definitions = set(re.findall(r'(?m)^[A-Za-z_][A-Za-z_0-9\s*]*?\b(\w+)\s*\([^;{}]*\)\s*\{', text))
            definitions.update(name for name in global_declarations if
                re.search(r'(?m)^(?!extern)[^\n;{}]*\b' + re.escape(name) + r'\s*(?:\[[^]]*\])?\s*=', text))
            for name in definitions: support_index.setdefault(name, []).append(source)
            manifest['support_source_index'].append({'source': str(source), 'sha256': digest(source), 'symbols': sorted(definitions)})
        support_cache = {}
        emitted_dependencies = {}
        manifest['support_emissions'] = []
        product_exports = set().union(*(object_exports(p) for p in objects))

        def support_closure(unit):
            supplied = product_exports | object_exports(unit)
            needed = object_references(unit)
            selected = []
            decisions = []
            while True:
                pending = sorted((needed - supplied) & support_index.keys())
                if not pending: break
                symbol = pending[0]
                candidates = support_index[symbol]
                if len(candidates) != 1:
                    raise RuntimeError('Ambiguous original support symbol ' + symbol + ': ' + str(candidates))
                source = candidates[0]
                if source in support_cache:
                    cached = support_cache[source]
                    if isinstance(cached, Exception): raise cached
                    target = cached
                else:
                    target = work / 'support' / (source.stem + '.cs'); target.parent.mkdir(exist_ok=True)
                    original_bytes = source.read_bytes()
                    source.write_bytes(PRELUDE + original_bytes)
                    observation = {'source': str(source), 'original_sha256': hashlib.sha256(original_bytes).hexdigest()}
                    manifest['support_emissions'].append(observation)
                    try:
                        dependency_file = target.with_suffix('.d')
                        previous = reused_support.get(source.relative_to(stage))
                        if previous:
                            entry, provenance = previous
                            if entry['original_sha256'] != observation['original_sha256']:
                                raise RuntimeError('Selected reusable support TU differs from staged original source')
                            shutil.copyfile(entry['object'], target)
                            shutil.copyfile(entry['dependency_file'], dependency_file)
                            observation['reused_emission'] = {'manifest': provenance, 'object': entry['object'],
                                'verified_at_reuse_dependency_sha256': hashes(dependencies(dependency_file))}
                        else:
                            command(['dotnet', compiler, *options, source, '--emit=obj', '-MD', '-MF', dependency_file, '-o', target])
                        emitted_dependencies[target] = dependencies(dependency_file)
                        support_cache[source] = target
                        observation.update(status='emitted', object=str(target), sha256=digest(target),
                                           dependency_file=str(dependency_file), dependency_sha256=digest(dependency_file),
                                           dependency_inputs_sha256=hashes(emitted_dependencies[target]))
                    except Exception as error:
                        support_cache[source] = error
                        observation.update(status='failed', error=str(error))
                        raise
                if target in selected:
                    raise RuntimeError('Declared original support does not export required symbol: ' + symbol)
                selected.append(target)
                decisions.append({'required_symbol': symbol, 'selected_source': str(source), 'object': str(target)})
                supplied.update(object_exports(target))
                needed.update(object_references(target))
            return selected, decisions

        def core_coverage(unit, support_objects):
            carriers = [unit, *support_objects]
            retained, coverage = [], []
            for obj, staged_source in core_units:
                included_by = [carrier for carrier in carriers if staged_source in emitted_dependencies[carrier]]
                row = {'object': str(obj), 'object_sha256': digest(obj), 'staged_source': str(staged_source),
                       'staged_source_sha256': digest(staged_source)}
                if len(included_by) > 1:
                    raise RuntimeError('Original core TU included in multiple harness objects: ' + str(staged_source))
                if included_by:
                    carrier = included_by[0]
                    missing = external_roots(obj) - object_exports(carrier)
                    if missing: raise RuntimeError('Partial included core TU would lose original external roots: ' + str(staged_source) + ': ' + str(sorted(missing)))
                    row.update(disposition='included-complete-tu', included_object=str(carrier),
                        included_object_sha256=digest(carrier), preserved_external_roots=sorted(external_roots(obj)),
                        reason='Compiler -MD proves exact staged C TU was included; its entire original external root set remains in this actual harness object.')
                else:
                    retained.append(obj)
                    row.update(disposition='separate-original-object', reason='Original core TU is not included by the harness/support dependency graph.')
                coverage.append(row)
            if len(coverage) != expected: raise RuntimeError('Core TU coverage is incomplete')
            return retained, coverage
        provider = json.loads((ROOT / 'config/provider.json').read_text())
        selected_provider = args.provider_project.resolve() if args.provider_project else ROOT / provider['project_reference']
        extra_bindings = [path.resolve() for path in args.provider_binding]
        if not selected_provider.is_file() or any(not path.is_file() for path in extra_bindings):
            raise RuntimeError('Selected provider project or extra test binding is missing')
        manifest['provider_selection'] = {'project': str(selected_provider), 'extra_bindings': list(map(str, extra_bindings)),
            'scope': 'Test harness assembly only; the delivered production provider/profile remains unchanged'}
        provider_policy = None
        if args.provider_policy:
            original_policy = args.provider_policy.resolve()
            policy_hash = digest(original_policy)
            frozen_policy = work / 'tool-inputs/test-provider-policy.json'
            frozen_policy.parent.mkdir(exist_ok=True)
            shutil.copyfile(original_policy, frozen_policy)
            if digest(frozen_policy) != policy_hash or digest(original_policy) != policy_hash:
                raise RuntimeError('Test provider policy changed during private snapshot creation')
            provider_policy = json.loads(frozen_policy.read_text())
            required_c = {case['path'] for case in required if case['path'].endswith('.c')}
            if (provider_policy.get('schema_version') != 1 or not isinstance(provider_policy.get('project'), str)
                or not isinstance(provider_policy.get('bindings_by_case'), dict)
                or set(provider_policy['bindings_by_case']) - required_c
                or any(not isinstance(paths, list) or not paths or not all(isinstance(path, str) for path in paths)
                       for paths in provider_policy['bindings_by_case'].values())):
                raise RuntimeError('Test provider policy must map exact required C paths to nonempty binding lists')
            manifest['provider_policy'] = {'original': str(original_policy), 'sha256': policy_hash,
                                            'snapshot': str(frozen_policy), 'snapshot_sha256': digest(frozen_policy)}
        for case in cases:
            entries = [r for r in manifest['executables'] if r['case'] == case['path']]
            if not case['path'].endswith('.c'):
                for row in entries: row['reason'] = 'Python integration case requires its exact endpoint/parameter adapter; remains required.'
                continue
            try:
                source = stage / case['path']
                original = ROOT / 'ref/s2n-tls' / case['path']
                adapted, adaptations = apply_features(case['path'], original.read_bytes())
                source.write_bytes(PRELUDE + adapted)
                for row in entries: row['test_adaptations'] = adaptations
                unit_options = [*options, *(['-DDOTCC_TEST_EXCLUDE_KTLS=1'] if adaptations else [])]
                unit = work / 'units' / (source.stem + '.cs'); unit.parent.mkdir(exist_ok=True)
                dependency_file = unit.with_suffix('.d')
                if args.reuse_emission:
                    previous = reused_units.get(case['path'])
                    if previous is None: raise RuntimeError('Exact required harness has no successful reusable emission: ' + case['path'])
                    for field, expected_value in (('original_sha256', digest(original)), ('test_adaptations', adaptations)):
                        if previous.get(field, [] if field == 'test_adaptations' else None) != expected_value:
                            raise RuntimeError('Reusable emitted source/adaptation identity differs: ' + case['path'])
                    emitted_source = Path(previous['compiled_source'])
                    if emitted_source.read_bytes() != PRELUDE + adapted or digest(emitted_source) != previous['compiled_sha256']:
                        raise RuntimeError('Reusable emitted original harness source changed')
                    if digest(previous['object']) != previous['object_sha256'] or digest(previous['dependency_file']) != previous['dependency_sha256']:
                        raise RuntimeError('Reusable emitted object/dependency provenance changed')
                    verify_dependency_inputs(previous)
                    shutil.copyfile(previous['object'], unit)
                    shutil.copyfile(previous['dependency_file'], dependency_file)
                    for row in entries:
                        row['reused_emission'] = {'manifest': manifest['reused_emission_manifest'],
                            'original_object': previous['object'], 'object_sha256': digest(unit),
                            'verified_at_reuse_dependency_sha256': hashes(dependencies(dependency_file))}
                    source = emitted_source
                else:
                    command(['dotnet', compiler, *unit_options, source, '--emit=obj', '-MD', '-MF', dependency_file, '-o', unit])
                emitted_dependencies[unit] = dependencies(dependency_file)
                if args.emit_only:
                    unused, direct_coverage = core_coverage(unit, [])
                    for row in entries:
                        row.update(status='emitted', compiled_source=str(source), compiled_sha256=digest(source),
                                   original_sha256=digest(original), object=str(unit), object_sha256=digest(unit),
                                   dependency_file=str(dependency_file), dependency_sha256=digest(dependency_file),
                                   dependency_inputs_sha256=hashes(emitted_dependencies[unit]))
                        row['core_tu_coverage'] = direct_coverage
                    continue
                raw = work / source.stem / 'raw' / 'S2nTls'
                provider_snapshot = None
                harness_provider, harness_bindings = selected_provider, extra_bindings
                policy_entry = None
                if provider_policy and case['path'] in provider_policy['bindings_by_case']:
                    policy_entry = {'case': case['path'], 'project': provider_policy['project'],
                                    'bindings': provider_policy['bindings_by_case'][case['path']]}
                    harness_provider = (ROOT / policy_entry['project']).resolve()
                    harness_bindings = [(ROOT / path).resolve() for path in policy_entry['bindings']]
                    for row in entries:
                        row['provider_policy_selection'] = dict(manifest['provider_policy'], selected_entry=policy_entry)
                if args.provider_project or args.provider_binding or policy_entry:
                    helper = ROOT / 'tests/Provider/UpstreamSurface/provider_snapshot.py'
                    specification = importlib.util.spec_from_file_location('dotcc_test_provider_snapshot', helper)
                    module = importlib.util.module_from_spec(specification)
                    specification.loader.exec_module(module)
                    provider_snapshot = module.snapshot_provider(harness_provider, harness_bindings,
                                                                  work / source.stem / 'provider-inputs')
                    harness_provider = Path(provider_snapshot['project'])
                    harness_bindings = list(map(Path, provider_snapshot['bindings']))
                    if hashes(provider_snapshot['snapshot_inputs_sha256']) != provider_snapshot['snapshot_inputs_sha256']:
                        raise RuntimeError('Frozen test-only provider inputs differ from their snapshot receipt')
                    for row in entries:
                        row['provider_snapshot'] = provider_snapshot
                support_objects, support_decisions = support_closure(unit)
                core_objects, coverage = core_coverage(unit, support_objects)
                for row in entries:
                    row['support_closure'] = support_decisions
                    row['core_tu_coverage'] = coverage
                command(['dotnet', compiler, *core_objects, *support_objects, unit, '--emit=managedlib', '--nest-types',
                         '--runtime=c', '--class-name', 'S2nTls', '--namespace', 'Managed.Security.S2n',
                         '--split=size', '--split-size=262144', '--literal-pool', '--deduplicate-inline',
                         '--prune-unused-inline', '-o', raw], timeout=600)
                project = raw / 'S2nTls.csproj'
                tree = ET.parse(project); root = tree.getroot(); properties = ET.SubElement(root, 'PropertyGroup')
                ET.SubElement(properties, 'OutputType').text = 'Exe'
                ET.SubElement(properties, 'AssemblyName').text = source.stem
                entry_name = 'EndpointMain' if args.endpoint else 'HarnessMain'
                ET.SubElement(properties, 'StartupObject').text = entry_name
                declaration = next((m for p in raw.glob('*.cs') for m in re.findall(r'public static (?:unsafe )?int main\(([^\n]*)\)', p.read_text())), None)
                if declaration is None: raise RuntimeError('Actual translated public main signature is missing')
                definitions = '$(DefineConstants);UPSTREAM_EXECUTION_IDENTITY'
                if declaration:
                    if len(declaration.split(',')) != 2: raise RuntimeError('Unhandled real upstream main signature: ' + declaration)
                    definitions += ';UPSTREAM_MAIN_ARGUMENTS' + (';UPSTREAM_HOST_SERVICES' if args.endpoint else '')
                ET.SubElement(properties, 'DefineConstants').text = definitions
                group = ET.SubElement(root, 'ItemGroup')
                ET.SubElement(group, 'Compile', Include=str(ROOT / 'tests/Upstream' / (entry_name + '.cs')), Link=entry_name + '.cs')
                ET.SubElement(group, 'Compile', Include=str(ROOT / 'samples/ManagedConsumer/ExecutionIdentity.cs'), Link='ExecutionIdentity.cs')
                existing_providers = [node for node in root.iter('ProjectReference')
                                      if (raw / node.get('Include')).resolve() == (ROOT / provider['project_reference']).resolve()]
                if existing_providers:
                    raise RuntimeError('Generated harness already references the production provider; refusing to load both providers')
                ET.SubElement(group, 'ProjectReference', Include=str(harness_provider))
                for binding in [*provider['authored_bindings'], 'src/Boundary/S2nHost.cs',
                                *(['tests/Upstream/GetoptBindings.cs', 'tests/Upstream/StdfdBindings.cs'] if args.endpoint else [])]:
                    ET.SubElement(group, 'Compile', Include=str(ROOT / binding), Link=Path(binding).name)
                for index, binding in enumerate(harness_bindings):
                    ET.SubElement(group, 'Compile', Include=str(binding), Link='TestProviderBinding-' + str(index) + '-' + binding.name)
                ET.indent(tree); tree.write(project, encoding='unicode')
                trees = {'raw': raw}
                if 'processed' in forms:
                    processed = raw.parent.parent / 'processed' / 'S2nTls'
                    postprocessor = run / 'tools/DotCC.PostProcess/dotcc-postprocess.dll'
                    semantic_receipt = postprocess_private(raw, processed, postprocessor, command)
                    for row in entries: row['semantic_postprocess'] = semantic_receipt
                    trees['processed'] = processed
                for row in entries:
                    form, mode = row['form'], row['mode']; project = trees[form] / 'S2nTls.csproj'
                    target = work / source.stem / form / mode
                    argv = ['dotnet', 'publish' if mode == 'aot' else 'build', project, '-c', 'Release',
                            '--artifacts-path', target / 'artifacts', '-m:1']
                    if mode == 'aot': argv += ['-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', target / 'publish']
                    authored = authored_sources(ROOT)
                    authored.update(hashes(p for p in (ROOT / 'tests/Upstream').rglob('*') if p.is_file() and p.suffix in ('.cs', '.json', '.h')))
                    if provider_snapshot:
                        authored.update(provider_snapshot['snapshot_inputs_sha256'])
                    if provider_policy:
                        authored[str(frozen_policy)] = manifest['provider_policy']['snapshot_sha256']
                    command(argv, timeout=1200)
                    if hashes(authored) != authored: raise RuntimeError('Authored upstream/provider source changed during build')
                    executable = target / 'publish' / source.stem if mode == 'aot' else target / 'artifacts/bin/S2nTls/release' / (source.stem + '.dll')
                    if not executable.is_file(): raise RuntimeError('Expected translated harness executable missing: ' + str(executable))
                    closure = list(executable.parent.glob('*'))
                    launch = [str(executable)] if mode == 'aot' else ['dotnet', str(executable)]
                    command([*launch, '--identity'], timeout=120)
                    runtime = json.loads(Path(manifest['commands'][-1]['log']).read_text())
                    record = capture(launch, target / 'artifacts', runtime, authored,
                                     {'campaign_run': str(run), 'core_tu_coverage': coverage,
                                      'test_provider_snapshot': provider_snapshot})
                    identity_path = target / 'execution-identity.json'
                    identity_path.write_text(json.dumps(record, indent=2) + '\n')
                    row.update(status='built', command=launch, application_execution_receipt=str(identity_path),
                        application_execution_receipt_sha256=digest(identity_path),
                        compiled_source=str(source), compiled_sha256=digest(source), original_sha256=digest(original),
                        dependency_file=str(dependency_file), dependency_sha256=digest(dependency_file),
                        dependency_inputs_sha256=hashes(emitted_dependencies[unit]),
                        execution_sha256={str(p): digest(p) for p in closure if p.is_file()})
            except Exception as error:
                for row in entries:
                    if row['status'] != 'built': row.update(status='failed', error=str(error))
        success = 'emitted' if args.emit_only else 'built'
        for path, expected_hash in manifest['tool_sha256'].items():
            if digest(path) != expected_hash: raise RuntimeError('Selected immutable tool snapshot changed: ' + path)
        if manifest['executables'] and all(r['status'] == success for r in manifest['executables']): manifest['status'] = success
    except Exception as error:
        manifest['error'] = str(error)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(manifest, indent=2) + '\n')
    print('MANIFEST ' + str(args.output))
    return 0 if manifest['status'] in ('built', 'emitted') else 1


if __name__ == '__main__':
    raise SystemExit(main())
