"""Actual application/runtime hashes for qualification receipts."""
import hashlib
from pathlib import Path


def hashes(paths):
    return {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(set(map(Path, paths)))}


def tree_hashes(directory):
    return hashes(p for p in Path(directory).rglob('*') if p.is_file())


def authored_sources(root, lifecycle=False):
    roots = [root / 'src' / name for name in ('Managed.S2nTls', 'CryptoProvider', 'Boundary')]
    roots.append(root / 'samples/ManagedConsumer')
    if lifecycle: roots += [root / 'tests/Managed', root / 'tests/Lifecycle']
    selected = []
    for directory in roots:
        selected.extend(p for p in directory.rglob('*') if p.is_file() and p.suffix in ('.cs', '.csproj', '.props', '.targets', '.json')
                        and not {'bin', 'obj'} & set(p.relative_to(directory).parts))
    for directory in (root, root.parent):
        selected += [directory / name for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'global.json', 'NuGet.Config')
                     if (directory / name).is_file()]
    return hashes(selected)


def capture(command, artifacts, identity, authored, product):
    if identity['kind'] != 'actual-runtime-identity': raise RuntimeError('Application omitted its actual runtime identity')
    executable = Path(command[1] if command[0] == 'dotnet' else command[0]).resolve()
    output = tree_hashes(executable.parent)
    runtime = hashes(Path(p) for p in identity['mapped_files'])
    if identity['dynamic_code']:
        runtime.update(tree_hashes(identity['runtime_directory']))
    return {'command': command, 'executable': str(executable), 'executable_sha256': output[str(executable)],
            'output_sha256': output, 'managed_build_closure_sha256': tree_hashes(Path(artifacts) / 'bin'),
            'actual_runtime': identity, 'runtime_closure_sha256': runtime,
            'authored_source_sha256': authored, 'product_snapshot': product}


def verify(record):
    # Private app output must stay identical through its executions. Runtime
    # and authored sources are identified at build time in the receipt.
    if hashes(record['output_sha256']) != record['output_sha256']:
        raise RuntimeError('Private application execution closure changed after build')
