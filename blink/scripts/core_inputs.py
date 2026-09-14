"""Content identity of the actual compiler process and its local dependencies."""
import hashlib
from pathlib import Path


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
    return dict(version=1, source=entry['path'], source_sha256=entry['sha256'],
                dependencies=dependencies, compiler_sha256=inputs['compiler'],
                options=OBJECT_OPTIONS, include_order=['snapshot', 'pinned-upstream', 'authored', 'host'],
                source_inventory_sha256=sha(campaign / 'config/source-inventory.json'),
                isolator_sha256=sha(campaign / 'scripts/isolate-core.py'),
                helper_sha256=sha(campaign / 'scripts/core_inputs.py'))


def canonical_emission(profile: Path, campaign: Path, inputs: dict, entry: dict) -> tuple[Path, Path, dict, str]:
    """Materialize immutable exact C inputs at stable paths across profile copies."""
    import json
    import os
    import shutil
    import tempfile
    identity = emission_identity(profile, campaign, inputs, entry)
    key = hashlib.sha256(json.dumps(identity, sort_keys=True).encode()).hexdigest()
    parent = campaign / 'generated/core-emission'
    parent.mkdir(parents=True, exist_ok=True)
    target = parent / key
    relative_source = Path('source') / Path(entry['path']).name
    if not target.exists():
        temporary = Path(tempfile.mkdtemp(prefix='.pending-', dir=parent))
        try:
            for name in identity['dependencies']:
                destination = temporary / name
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(profile / name, destination)
            (temporary / relative_source).parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(campaign / entry['staged_path'], temporary / relative_source)
            (temporary / 'identity.json').write_text(json.dumps(identity, indent=2) + '\n')
            try:
                temporary.rename(target)
            except FileExistsError:
                pass  # another identical emission won publication
        finally:
            if temporary.exists():
                shutil.rmtree(temporary)
    if json.loads((target / 'identity.json').read_text()) != identity:
        raise RuntimeError('canonical identity mismatch: ' + str(target))
    for name, expected in dict(identity['dependencies'], **{str(relative_source):entry['sha256']}).items():
        if hashlib.sha256((target / name).read_bytes()).hexdigest() != expected:
            raise RuntimeError('canonical C input checksum mismatch: ' + str(target / name))
    return target, target / relative_source, identity, key
