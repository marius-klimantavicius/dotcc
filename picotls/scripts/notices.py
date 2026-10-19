"""Deliver exact notices for the selected picotls sources and DotCC runtime."""
import hashlib
import json
from pathlib import Path
import tarfile
import xml.etree.ElementTree as ET


def digest(data):
    return hashlib.sha256(data).hexdigest()


def payload(root):
    root = Path(root)
    pin = json.loads((root / 'config/inputs.json').read_text())['picotls']
    archive = root / 'ref' / (pin['directory'] + '.tar.gz')
    if digest(archive.read_bytes()) != pin['sha256']:
        raise RuntimeError('picotls notice source archive differs from the pinned SHA256')
    names = [name.strip() for name in (root / 'config/core-sources.txt').read_text().splitlines()
             if name.strip() and not name.lstrip().startswith('#')]
    names += ['include/picotls.h', 'include/picotls/pembase64.h']
    notices, sources = [], {}
    with tarfile.open(archive, 'r:gz') as source:
        for name in names:
            data = source.extractfile(pin['directory'] + '/' + name).read()
            if (root / 'ref' / pin['directory'] / name).read_bytes() != data:
                raise RuntimeError('Changed picotls notice source: ' + name)
            end = data.find(b'*/')
            comment = data[:end + 2]
            if not data.startswith(b'/*') or end < 0 or b'Copyright' not in comment or b'Permission' not in comment:
                raise RuntimeError('Missing selected upstream copyright/permission comment: ' + name)
            notices.append(name.encode() + b'\n' + comment + b'\n')
            sources[name] = dict(sha256=digest(data), noticeSha256=digest(comment))
    contents = {'DotCC.LICENSE': (root.parent / 'LICENSE').read_bytes(),
                'Picotls.NOTICES': b'\n'.join(notices)}
    manifest = dict(files={name: digest(data) for name, data in contents.items()},
                    origins=dict(dotcc='Repository LICENSE; embedded runtime and authored host/provider support',
                                 picotls={key: pin[key] for key in ('revision', 'url', 'sha256', 'directory')}),
                    sources=sources,
                    scope='Selected library sources and consumed headers; platform libraries are not bundled')
    return manifest, contents


def validate(root, directory, owned=None):
    directory = Path(directory)
    expected, contents = payload(root)
    manifest = json.loads((directory / 'Picotls.NOTICES.json').read_text())
    if manifest != expected:
        raise RuntimeError('picotls notice manifest differs from exact source origins')
    for name, data in contents.items():
        if (directory / name).read_bytes() != data:
            raise RuntimeError('Missing or changed delivered notice: ' + name)
    if owned is not None and not {*contents, 'Picotls.NOTICES.json'} <= set(owned):
        raise RuntimeError('picotls notices are absent from the delivered ownership manifest')
    return manifest


def package(root, project):
    project = Path(project)
    manifest, contents = payload(root)
    for name, data in contents.items():
        (project.parent / name).write_bytes(data)
    (project.parent / 'Picotls.NOTICES.json').write_text(json.dumps(manifest, indent=2, sort_keys=True) + '\n')
    tree = ET.parse(project)
    group = None
    for name in (*contents, 'Picotls.NOTICES.json'):
        node = next((node for node in tree.iter('None') if node.get('Update') == name), None)
        if node is None:
            if group is None:
                group = ET.SubElement(tree.getroot(), 'ItemGroup')
            node = ET.SubElement(group, 'None', Update=name)
        node.set('CopyToOutputDirectory', 'PreserveNewest')
        node.set('CopyToPublishDirectory', 'PreserveNewest')
    ET.indent(tree, space='  ')
    tree.write(project, encoding='unicode')
    return validate(root, project.parent)
