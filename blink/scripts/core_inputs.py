"""Content identity of the actual compiler process and its local dependencies."""
import hashlib
from pathlib import Path


def stage_semantic_intrinsics(campaign: Path, profile: Path) -> None:
    """Pin the reviewed physical header before selecting ordinary byte helpers.

    Optional per-TU rules allow producers which never include endian.h. Emission
    reports and the final assembly require the complete set on the core units.
    """
    import json
    import shutil
    specification = campaign / 'config/semantic-intrinsics.json'
    spec = json.loads(specification.read_text())
    if spec['version'] != 1 or spec['header'] != 'blink/endian.h':
        raise RuntimeError('Unknown semantic intrinsic profile')
    header = campaign / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580' / spec['header']
    if hashlib.sha256(header.read_bytes()).hexdigest() != spec['header_sha256']:
        raise RuntimeError('Reviewed endian implementation changed')
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
    shutil.copyfile(specification, profile / 'semantic-intrinsics.json')


def semantic_selection(profile: Path, report: Path, source: str) -> dict:
    """Validate actual typed selection, retaining absent-unit evidence explicitly."""
    import json
    specification = profile / 'semantic-intrinsics.json'
    if not specification.exists():
        return {}  # Historical profiles retain their original contract.
    spec = json.loads(specification.read_text())
    expected = {rule['name']: 'intrinsic:' + rule['target']['name'] for rule in spec['functionOverrides']}
    events = [json.loads(line) for line in report.read_text().splitlines() if line]
    selected = [row for row in events if row.get('event') == 'function-override']
    unmatched = [row['name'] for row in events if row.get('event') == 'function-override-unmatched']
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
    physical_header = {rule['declarationFile'] for rule in rules}
    if len(physical_header) != 1:
        raise RuntimeError('Ambiguous endian physical declaration selector')
    for row in selected:
        if row['target'] != expected[row['name']] or row['declarationFile'] not in physical_header or row['matches'] != '1':
            raise RuntimeError('Unexpected semantic replacement provenance: ' + source)
    return dict(report=str(report), report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                specification_sha256=hashlib.sha256(specification.read_bytes()).hexdigest(),
                selected=selected, absent=not selected)


def compiler_identity(directory: Path) -> dict[str, str]:
    required = ('dotcc.dll', 'dotcc.deps.json', 'dotcc.runtimeconfig.json')
    for name in required:
        if not (directory / name).is_file():
            raise RuntimeError('missing compiler process input: ' + str(directory / name))
    paths = sorted({path for pattern in ('*.dll', '*.deps.json', '*.runtimeconfig.json')
                    for path in directory.glob(pattern) if path.is_file()})
    return {path.name:hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}


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
    if 'semantic-intrinsics.json' in files:
        dependencies['semantic-intrinsics.json'] = files['semantic-intrinsics.json']
    return dict(version=2, source=entry['path'], source_sha256=entry['sha256'],
                dependencies=dependencies, compiler_sha256=inputs['compiler'],
                options=OBJECT_OPTIONS, include_order=['snapshot', 'pinned-upstream', 'authored', 'host'],
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
