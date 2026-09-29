#!/usr/bin/env python3
"""Check declared source closure against the acquired release, without mutation."""
import argparse
import json
from pathlib import Path
import re


def check(root, source):
    groups = json.loads((root / 'config/source-groups.json').read_text())
    declared = [name for names in groups.values() for name in names]
    if len(declared) != len(set(declared)):
        raise RuntimeError('Source manifests contain duplicate translation units')
    actual = {p.relative_to(source).as_posix() for p in (source / 'src').rglob('*.c')}
    included_data = {'src/apps/http/fsdata.c'}
    if actual != set(declared) | included_data:
        raise RuntimeError(f'Unclassified source closure: {sorted(actual - set(declared) - included_data)}; stale {sorted(set(declared) - actual)}')
    cmake = (source / 'src/Filelists.cmake').read_text()
    listed = set(re.findall(r'\$\{LWIP_DIR\}/(src/[^\s)]+\.c)(?=[\s)])', cmake))
    if listed != set(declared):
        raise RuntimeError('Source manifest differs from upstream CMake explicit list')
    contrib = json.loads((root / 'config/contrib-inventory.json').read_text())
    observed = {p.relative_to(source).as_posix() for p in (source / 'contrib').rglob('*') if p.is_file()}
    if observed != {row['path'] for row in contrib}:
        raise RuntimeError('Contrib file classification is incomplete or stale')
    excluded = groups['lwipmbedtls_SRCS']
    tools = groups['lwipmakefsdata_SRCS']
    return {'source_units': len(declared), 'stack_units': len(declared)-len(excluded)-len(tools),
            'excluded_external_units': excluded, 'tool_units': tools,
            'included_data': sorted(included_data), 'contrib_files': len(contrib),
            'qualification': 'inventory-only; active preprocessing and behavior remain separate gates'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    source = args.source or root / 'ref' / json.loads((root / 'config/sources.json').read_text())['directory']
    print(json.dumps(check(root, source), indent=2))
