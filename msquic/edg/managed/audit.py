#!/usr/bin/env python3
"""Audit prepared project dependencies and the recorded source identities."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET


def audit(prepared: Path) -> dict:
    prepared = prepared.resolve()
    pending = [prepared / 'ManagedApi/ManagedApi.csproj']
    projects, sources, packages = set(), set(), []
    while pending:
        path = pending.pop().resolve()
        if path in projects:
            continue
        projects.add(path)
        tree = ET.parse(path)
        sources.update((path.parent / n.attrib['Include']).resolve() for n in tree.findall('.//Compile'))
        pending.extend((path.parent / n.attrib['Include']).resolve() for n in tree.findall('.//ProjectReference'))
        packages.extend(n.attrib for n in tree.findall('.//PackageReference'))
    imports, unexpected = [], []
    for path in sorted(sources):
        if '/generated/' in str(path):
            unexpected.append({'path': str(path), 'reason': 'Original dotcc generated source dependency'})
        for number, line in enumerate(path.read_text().splitlines(), 1):
            # Source-attribution comments do not create dependencies.
            if line.lstrip().startswith('//'):
                continue
            if re.search(r'\bDotCC\.', line):
                unexpected.append({'path': str(path), 'line': number, 'reason': 'DotCC symbol dependency'})
            match = re.search(r'\b(?:DllImport|LibraryImport|NativeLibrary\.(?:Load|TryLoad))\s*\(\s*"([^"]+)"', line)
            if match:
                item = {'path': str(path), 'line': number, 'library': match.group(1), 'text': line.strip()}
                imports.append(item)
                if match.group(1) not in ('libc', 'kernel32.dll'):
                    unexpected.append(item)
    changed = []
    def verify(manifest):
        for section in ('generatedSources', 'authoredSources'):
            for item in manifest.get(section, []):
                for name, hash_name in (('path', 'sha256'), ('adaptedPath', 'adaptedSha256')):
                    if name in item and hashlib.sha256(Path(item[name]).read_bytes()).hexdigest() != item[hash_name]:
                        changed.append(item[name])
        if 'picotls' in manifest:
            verify(manifest['picotls'])
    verify(json.loads((prepared / 'provider-manifest.json').read_text()))
    return {'status': 'passed' if not packages and not unexpected and not changed else 'failed',
            'projects': sorted(map(str, projects)), 'sourceCount': len(sources), 'packageReferences': packages,
            'nativeImports': imports, 'unexpectedDependencies': unexpected, 'changedManifestInputs': changed,
            'note': 'Existing shared-runtime OS imports remain; platform choice is handled at runtime. No declaration here implies it is executed by the QUIC path.'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--prepared', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.prepared)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(report['status'], f"{len(report['projects'])} projects, {report['sourceCount']} sources")
    raise SystemExit(0 if report['status'] == 'passed' else 1)
