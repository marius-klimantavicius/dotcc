"""Content identity of the actual compiler process and its local dependencies."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]

import hashlib
from pathlib import Path


def stage_semantic_intrinsics(campaign: Path, profile: Path) -> None:
    """Select ordinary byte helpers by physical declaration and typed signature.

    Optional per-TU rules allow producers which never include endian.h. Emission
    reports and the final assembly require the complete set on the core units.
    """
    import json
    specification = campaign / 'config/semantic-intrinsics.json'
    spec = json.loads(specification.read_text())
    if spec['version'] != 1 or spec['header'] != 'blink/endian.h':
        raise RuntimeError('Unknown semantic intrinsic profile')
    header = campaign / ("ref/" + _CAMPAIGN_SOURCE["directory"]) / spec['header']
    spec['header_sha256'] = hashlib.sha256(header.read_bytes()).hexdigest()
    expected = {prefix + str(width): f'{operation}.u{width}.le'
                for width in (16, 32, 64) for prefix, operation in [('Get', 'load'), ('Put', 'store')]}
    if {rule['name']: rule['target']['name'] for rule in spec['functionOverrides']} != expected:
        raise RuntimeError('Reviewed semantic target set differs')
    overrides = json.loads((profile / 'overrides.json').read_text())
    if overrides.get('functionOverrides'):
        raise RuntimeError('Semantic functions already selected')
    overrides['functionOverrides'] = [dict(rule, declarationFile=str(header.resolve()))
                                      for rule in spec['functionOverrides']]
    (profile / 'overrides.json').write_text(json.dumps(overrides, indent=2) + '\n')
    (profile / 'semantic-intrinsics.json').write_text(json.dumps(spec, indent=2) + '\n')


def semantic_selection(profile: Path, report: Path, source: str) -> dict:
    """Validate actual typed selection, retaining absent-unit evidence explicitly."""
    import json
    specification = profile / 'semantic-intrinsics.json'
    if not specification.exists():
        return {}  # Historical profiles retain their original contract.
    spec = json.loads(specification.read_text())
    expected = {rule['name']: 'intrinsic:' + rule['target']['name'] for rule in spec['functionOverrides']}
    events = [json.loads(line) for line in report.read_text().splitlines() if line]
    selected = [row for row in events if row.get('event') == 'function-override' and row.get('name') in expected]
    unmatched = [row['name'] for row in events if row.get('event') == 'function-override-unmatched' and row.get('name') in expected]
    names = [row['name'] for row in selected]
    if names and (len(names) != len(expected) or set(names) != set(expected)):
        raise RuntimeError('Partial or duplicate endian selection: ' + source)
    if names and unmatched:
        raise RuntimeError('Mixed selected and unmatched endian rules: ' + source)
    if source in spec['required_units'] and set(names) != set(expected):
        raise RuntimeError('Required core endian selection missing: ' + source)
    if not names and (len(unmatched) != len(expected) or set(unmatched) != set(expected)):
        raise RuntimeError('Absent-unit typed selection was not reported: ' + source)
    rules = json.loads((profile / 'overrides.json').read_text())['functionOverrides']
    physical_header = {rule['declarationFile'] for rule in rules if rule['name'] in expected}
    if len(physical_header) != 1:
        raise RuntimeError('Ambiguous endian physical declaration selector')
    for row in selected:
        if row['target'] != expected[row['name']] or row['declarationFile'] not in physical_header or row['matches'] != '1':
            raise RuntimeError('Unexpected semantic replacement provenance: ' + source)
    return dict(report=str(report), report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                specification_sha256=hashlib.sha256(specification.read_bytes()).hexdigest(),
                selected=selected, absent=not selected)


def stage_managed_boundaries(campaign: Path, profile: Path, instance_methods: bool = False) -> None:
    """Select reviewed whole-function owner handoffs in the threaded profile."""
    import json
    path = campaign / 'config/managed-boundaries.json'
    spec = json.loads(path.read_text())
    if spec['version'] != 1:
        raise RuntimeError('Unknown managed boundary profile')
    if instance_methods:
        spec['headers'].append('blink/signal.h')
        spec['required_units']['blink/signal.c'] = ['TerminateSignal']
    upstream = campaign / ("ref/" + _CAMPAIGN_SOURCE["directory"])
    # Record this build's inputs; typed overrides do not require historical bytes.
    for category in ('headers', 'implementations'):
        spec[category] = {name: hashlib.sha256((upstream / name).read_bytes()).hexdigest()
                          for name in spec[category]}
    overrides = json.loads((profile / 'overrides.json').read_text())
    rules = spec['functionOverrides']
    if instance_methods:
        for rule in rules:
            rule['target']['passInstance'] = True
        rules.append(dict(name='blink_host_guest_pthread_atfork', linkage='external',
            signature=dict(returnType='int', parameterTypes=['void (*)(void)'] * 3, variadic=False),
            target=dict(kind='managedMethod', method='global::Managed.Emulation.BlinkCore.blink_host_guest_pthread_atfork', passInstance=True),
            requireMatch=False))
        for name, result, parameters in [
            ('blink_host_guest_thread_start', 'int', ['struct Machine *']),
            ('blink_host_guest_signal_checkpoint', 'void', ['struct Machine *']),
            ('blink_host_guest_signal_wake', 'int', ['struct Machine *']),
            ('blink_host_guest_signal_enqueue_info', 'void', ['struct Machine *', 'int', 'int', 'unsigned int']),
            ('blink_host_guest_signal_deliver_tkill', 'void', ['struct Machine *', 'int', 'int', 'unsigned int']),
            ('blink_host_guest_signal_apply_info', 'void', ['struct Machine *', 'int', 'struct siginfo_linux *']),
        ]:
            rules.append(dict(name=name, linkage='external',
                signature=dict(returnType=result, parameterTypes=parameters, variadic=False),
                target=dict(kind='managedMethod', method='global::Managed.Emulation.BlinkCore.' + name, passInstance=True),
                requireMatch=False))
        rules.append(dict(name='TerminateSignal', linkage='external', declarationFile='blink/signal.h',
            signature=dict(returnType='void', parameterTypes=['struct Machine *', 'int', 'int'], variadic=False),
            target=dict(kind='managedMethod', method='global::Managed.Emulation.BlinkCore.TerminateSignal', passInstance=True),
            requireMatch=False))
        header = campaign / 'src/Host/include/host-guest-threads.h'
        spec['authored_headers'] = {'host-guest-threads.h': hashlib.sha256(header.read_bytes()).hexdigest()}

    existing = {rule['name'] for rule in overrides['functionOverrides']}
    if existing.intersection(rule['name'] for rule in rules):
        raise RuntimeError('Managed boundary duplicates existing selection')
    for rule in rules:
        source_defined = (rule['name'] == 'TrackHostPage' and rule['linkage'] == 'internal'
                          and 'declarationFile' not in rule and 'blink/memorymalloc.c' in spec['implementations'])
        if (rule['target']['kind'] != 'managedMethod' or rule['linkage'] not in ('external', 'internal')
                or (rule.get('declarationFile') not in {**spec['headers'], **spec['implementations']}
                    and not source_defined and not rule['name'].startswith('blink_host_guest_'))):
            raise RuntimeError('Unreviewed managed boundary selector')
    overrides['functionOverrides'] += [dict(rule, declarationFile=str((upstream / rule['declarationFile']).resolve())) if 'declarationFile' in rule else rule
                                       for rule in rules]
    (profile / 'overrides.json').write_text(json.dumps(overrides, indent=2) + '\n')
    (profile / 'managed-boundaries.json').write_text(json.dumps(spec, indent=2) + '\n')


def managed_boundary_selection(profile: Path, report: Path, source: str) -> dict:
    """Check the independent managed-method group without masking absent rules."""
    import json
    specification = profile / 'managed-boundaries.json'
    if not specification.exists():
        return {}
    spec = json.loads(specification.read_text())
    rules = {rule['name']: rule for rule in spec['functionOverrides']}
    bound = {rule['name']: rule for rule in json.loads((profile / 'overrides.json').read_text())['functionOverrides']
             if rule['name'] in rules}
    if set(bound) != set(rules):
        raise RuntimeError('Managed boundary bound rules differ')
    events = [json.loads(line) for line in report.read_text().splitlines() if line]
    selected = [row for row in events if row.get('event') == 'function-override' and row.get('name') in rules]
    unmatched = [row['name'] for row in events if row.get('event') == 'function-override-unmatched' and row.get('name') in rules]
    names = [row['name'] for row in selected]
    if len(names + unmatched) != len(rules) or set(names + unmatched) != set(rules):
        raise RuntimeError('Missing or duplicate managed boundary selection: ' + source)
    if not set(spec['required_units'].get(source, [])).issubset(names):
        raise RuntimeError('Required managed boundary missing: ' + source)
    for row in selected:
        rule = bound[row['name']]
        source_defined = row['name'] == 'TrackHostPage'
        if source_defined:
            # The staged C file has a content-addressed path which itself
            # depends on these rules. Pin its origin after typed selection.
            if source != 'blink/memorymalloc.c' or row['declarationFile'] != row['translationUnit']:
                raise RuntimeError('Page-table source selector differs: ' + source)
        elif 'declarationFile' not in rule:
            declared = Path(row['declarationFile'])
            if hashlib.sha256(declared.read_bytes()).hexdigest() != spec['authored_headers'].get(declared.name):
                raise RuntimeError('Authored callback declaration identity differs: ' + source)
        if (row['target'] != 'managedMethod:' + rule['target']['method'] or row['matches'] != '1'
                or ('declarationFile' in rule and row['declarationFile'] != rule['declarationFile'])
                or (not source_defined and 'declarationFile' not in rule and Path(row['declarationFile']).name != 'host-guest-threads.h')
                or row.get('passInstance', 'false') != str(rule['target'].get('passInstance', False)).lower()
                or row.get('doesNotReturn', 'false') != str(rule['target'].get('doesNotReturn', False)).lower()):
            raise RuntimeError('Managed boundary target/declaration differs: ' + source)
    return dict(report=str(report), report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                specification_sha256=hashlib.sha256(specification.read_bytes()).hexdigest(),
                selected=selected, unmatched=unmatched, absent=not selected)


def compiler_identity(directory: Path) -> dict[str, str]:
    required = ('dotcc.dll', 'dotcc.deps.json', 'dotcc.runtimeconfig.json')
    for name in required:
        if not (directory / name).is_file():
            raise RuntimeError('missing compiler process input: ' + str(directory / name))
    paths = sorted({path for pattern in ('*.dll', '*.deps.json', '*.runtimeconfig.json')
                    for path in directory.glob(pattern) if path.is_file()})
    return {path.name:hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}


def upstream_identity(campaign: Path) -> dict[str, str]:
    """Current include/source inputs for cache freshness, not compatibility pins."""
    import json
    manifest = json.loads((campaign / 'config/source-manifest.json').read_text())
    upstream = campaign / 'ref' / manifest['upstream']['directory']
    return {str(path.relative_to(upstream)): hashlib.sha256(path.read_bytes()).hexdigest()
            for path in sorted((upstream / 'blink').rglob('*')) if path.is_file()}


def profile_sources(profile: Path, campaign: Path, inputs: dict) -> list[dict]:
    """Resolve the frozen upstream/addition/author list through every override."""
    import json
    closure_path = profile / 'closure.json'
    if not closure_path.exists():
        closure_path = campaign / 'artifacts/core/closure.json'
    entries = json.loads(closure_path.read_text())['sources']
    additions_path = profile / 'managed-additions.json'
    if additions_path.exists():
        for entry in json.loads(additions_path.read_text())['sources']:
            entries.append(dict(entry, staged_path=str(profile / 'additional' / Path(entry['path']).name)))
    for entry in entries:
        override = inputs.get('source_overrides', {}).get(entry['path'])
        if override:
            if entry['sha256'] != override['original_sha256']:
                raise RuntimeError('override original differs from source: ' + entry['path'])
            entry.update(staged_path=override['staged_path'], sha256=override['sha256'])
    bindings = profile / 'binding-sources.json'
    authored = (json.loads(bindings.read_text())['authored_c'] if bindings.exists()
                else ['authored/managed-driver.c', 'authored/HostSignals.c', 'authored/HostMemory.c'])
    for relative in authored:
        path = profile / relative
        if path.is_file():
            entries.append(dict(path=relative, staged_path=str(path), sha256=inputs['staged_headers'][relative]))
        else:
            raise RuntimeError('missing authored profile source: ' + relative)
    names = [Path(entry['path']).name for entry in entries]
    if len(names) != len(set(names)):
        raise RuntimeError('source basenames collide in isolated output names')
    return entries


OBJECT_OPTIONS = ['-std=c17', '-D_GNU_SOURCE', '-DNDEBUG', '-DNOLINEAR', '--emit=obj']


def instance_methods(profile: Path) -> bool:
    import json
    marker = profile / 'instance-abi.json'
    if not marker.exists():
        return False
    if json.loads(marker.read_text()) != {'abi': 'instance-v1'}:
        raise RuntimeError('Unknown program instance ABI')
    return True


def object_options(profile: Path) -> list[str]:
    return OBJECT_OPTIONS + (['--instance-methods'] if instance_methods(profile) else [])


def emission_identity(profile: Path, campaign: Path, inputs: dict, entry: dict) -> dict:
    """Conservative C inputs, excluding independent TUs and consumer C# sources.

    Canonical content-addressed paths are part of the contract: the compiler
    uses absolute source paths for static symbol names and C __FILE__ values.
    """
    import re
    sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
    files = inputs['staged_headers']
    dependencies = {name:digest for name, digest in files.items()
                    if Path(name).suffix in {'.h', '.inc'}}
    # The authored driver includes probe.c. Preserve included source fragments,
    # while independent C translation units remain separate cache entries.
    included = set()
    for name in files:
        if Path(name).suffix in {'.h', '.inc', '.c'}:
            included.update(re.findall(r'^\s*#\s*include\s*["<]([^">]+)[">]',
                                       (profile / name).read_text(), re.M))
    for name, digest in files.items():
        if Path(name).suffix == '.c' and any(name == item or name.endswith('/' + item) for item in included):
            dependencies[name] = digest
    dependencies['overrides.json'] = files['overrides.json']
    for specification in ('semantic-intrinsics.json', 'managed-boundaries.json', 'instance-abi.json'):
        if specification in files:
            dependencies[specification] = files[specification]
    return dict(version=2, source=entry['path'], source_sha256=entry['sha256'],
                upstream_sha256=inputs.get('upstream_inputs', {}),
                dependencies=dependencies, compiler_sha256=inputs['compiler'],
                options=object_options(profile), include_order=['snapshot', 'pinned-upstream', 'authored', 'host'],
                source_inventory_sha256=sha(campaign / 'config/source-inventory.json'),
                isolator_sha256=sha(campaign / 'scripts/isolate-core.py'),
                helper_sha256=sha(campaign / 'scripts/core_inputs.py'))


def canonical_emission(profile: Path, campaign: Path, inputs: dict, entry: dict) -> tuple[Path, Path, dict, str]:
    """Materialize immutable exact C inputs at stable paths across profile copies."""
    import json
    import shutil
    import tempfile
    identity = emission_identity(profile, campaign, inputs, entry)
    key = hashlib.sha256(json.dumps(identity, sort_keys=True).encode()).hexdigest()
    parent = campaign / 'generated/core-emission'
    # Anonymous aggregate identities use the physical header path. Therefore
    # every TU must share the same canonical header tree, while each selected
    # source retains its own canonical path for static symbol qualification.
    header_identity = dict(dependencies=identity['dependencies'],
                           upstream_sha256=identity['upstream_sha256'],
                           source_inventory_sha256=identity['source_inventory_sha256'])
    header_key = hashlib.sha256(json.dumps(header_identity, sort_keys=True).encode()).hexdigest()
    header_target = parent / 'headers' / header_key
    source_target = parent / 'units' / key
    relative_source = Path('source') / Path(entry['path']).name

    def publish(target, files, metadata):
        target.parent.mkdir(parents=True, exist_ok=True)
        if not target.exists():
            temporary = Path(tempfile.mkdtemp(prefix='.pending-', dir=target.parent))
            try:
                for name, original in files.items():
                    destination = temporary / name
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(original, destination)
                (temporary / 'identity.json').write_text(json.dumps(metadata, indent=2) + '\n')
                try:
                    temporary.rename(target)
                except OSError:
                    if not target.is_dir():
                        raise
                    # Concurrent different TUs can publish the same header set.
            finally:
                if temporary.exists():
                    shutil.rmtree(temporary)
        if json.loads((target / 'identity.json').read_text()) != metadata:
            raise RuntimeError('canonical identity mismatch: ' + str(target))

    publish(header_target, {name:profile / name for name in identity['dependencies']}, header_identity)
    publish(source_target, {str(relative_source):campaign / entry['staged_path']}, identity)
    for name, expected in identity['dependencies'].items():
        if hashlib.sha256((header_target / name).read_bytes()).hexdigest() != expected:
            raise RuntimeError('canonical header checksum mismatch: ' + str(header_target / name))
    selected = source_target / relative_source
    if hashlib.sha256(selected.read_bytes()).hexdigest() != entry['sha256']:
        raise RuntimeError('canonical source checksum mismatch: ' + str(selected))
    return header_target, selected, identity, key
