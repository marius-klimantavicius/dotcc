"""Package source-owned notices and actual NativeAOT dependency notices offline."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
import zipfile


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def validate(directory, owned=None, *, root=None):
    directory = Path(directory)
    manifest = json.loads((directory / 'SQLite.NOTICES.json').read_text())
    expected, _ = payload(Path(root) if root else Path(__file__).resolve().parents[1])
    if manifest != expected:
        raise RuntimeError('SQLite notice manifest differs from exact source origins')
    required = {'DotCC.LICENSE', 'SQLite.LICENSE.md'}
    if not required <= manifest['files'].keys():
        raise RuntimeError('Missing required SQLite/runtime notice manifest entries')
    for name, expected in manifest['files'].items():
        if Path(name).name != name or digest(directory / name) != expected:
            raise RuntimeError('Missing or changed delivered notice: ' + name)
    if owned is not None and not {*manifest['files'], 'SQLite.NOTICES.json'} <= set(owned):
        raise RuntimeError('SQLite notices are absent from the delivered ownership manifest')
    return manifest


def payload(root):
    root = Path(root)
    pins = json.loads((root / 'config/sources.json').read_text())
    pin = pins['tests']
    archive = root / 'ref' / pin['archive']
    if digest(archive) != pin['sha256']:
        raise RuntimeError('SQLite source archive differs; cannot attribute delivery')
    with zipfile.ZipFile(archive) as source:
        license_bytes = source.read(Path(pin['directory']).name + '/LICENSE.md')
    contents = {'DotCC.LICENSE': (root.parent / 'LICENSE').read_bytes(), 'SQLite.LICENSE.md': license_bytes}
    manifest = dict(files={name: hashlib.sha256(data).hexdigest() for name, data in contents.items()},
                    origins=dict(dotcc='Repository LICENSE; embedded DotCC runtime and authored host support',
                                 sqlite={key: {field: value[field] for field in ('url', 'archive', 'sha256')}
                                         for key, value in pins.items()}),
                    scope='Library source/runtime notices; NativeAOT runtime notices bind actual publish inputs separately')
    return manifest, contents


def package(root, project):
    root, project = Path(root), Path(project)
    manifest, contents = payload(root)
    output = project.parent
    for name, data in contents.items():
        (output / name).write_bytes(data)
    (output / 'SQLite.NOTICES.json').write_text(json.dumps(manifest, indent=2, sort_keys=True) + '\n')
    tree = ET.parse(project)
    group = None
    for name in (*manifest['files'], 'SQLite.NOTICES.json'):
        node = next((node for node in tree.iter('None') if node.get('Update') == name), None)
        if node is None:
            if group is None:
                group = ET.SubElement(tree.getroot(), 'ItemGroup')
            node = ET.SubElement(group, 'None', Update=name)
        node.set('CopyToOutputDirectory', 'PreserveNewest')
        node.set('CopyToPublishDirectory', 'PreserveNewest')
    ET.indent(tree, space='  ')
    tree.write(project, encoding='unicode')
    return validate(output, root=root)


def nativeaot(publish, response):
    """Copy exact notices from packages named by this completed ILC response."""
    publish, response = Path(publish), Path(response)
    packages = set()
    for line in response.read_text().splitlines():
        if not line.startswith('-r:'):
            continue
        path = Path(line[3:].strip('"'))
        for parent in path.parents:
            if (parent / '.nupkg.metadata').is_file():
                packages.add(parent)
                break
    if not packages:
        raise RuntimeError('No actual NativeAOT package inputs in ILC response')
    output = publish / 'SQLite.NativeAOT-notices'
    rows = []
    for package in sorted(packages):
        metadata = package / '.nupkg.metadata'
        archive = package / (package.parent.name + '.' + package.name + '.nupkg')
        if not archive.is_file():
            raise RuntimeError('Missing cached actual NativeAOT package: ' + str(archive))
        import base64
        content_hash = base64.b64encode(hashlib.sha512(archive.read_bytes()).digest()).decode()
        provenance = json.loads(metadata.read_text())
        # NuGet retains the raw archive digest in this sidecar. Its v2
        # metadata contentHash is a separate value for signed packages.
        if content_hash != archive.with_suffix(archive.suffix + '.sha512').read_text().strip():
            raise RuntimeError('NativeAOT archive SHA512 differs: ' + str(archive))
        with zipfile.ZipFile(archive) as cached:
            for name in ('LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT'):
                original = cached.read(name)
                if (package / name).read_bytes() != original:
                    raise RuntimeError('Changed extracted NativeAOT notice: ' + name)
                destination = output / (package.parent.name + '-' + package.name) / name
                destination.parent.mkdir(parents=True, exist_ok=True)
                destination.write_bytes(original)
                rows.append(dict(path=destination.relative_to(publish).as_posix(), sha256=digest(destination),
                                 package=str(package), packageSha256=digest(archive),
                                 packageSource=provenance['source'], archiveSha512=content_hash,
                                 nugetMetadataContentHash=provenance['contentHash']))
    result = dict(response=dict(path=str(response.resolve()), sha256=digest(response)), files=rows,
                  scope='Actual ILC-referenced .NET redistributable packages; system libraries are not bundled')
    (publish / 'SQLite.NativeAOT-NOTICES.json').write_text(json.dumps(result, indent=2) + '\n')
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--ilc-response', type=Path)
    args = parser.parse_args()
    result = nativeaot(args.directory, args.ilc_response) if args.ilc_response else validate(args.directory)
    print(json.dumps(result, indent=2))
