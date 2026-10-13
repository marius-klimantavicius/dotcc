#!/usr/bin/env python3
"""Validate literal native tags against the append-only shared catalog.

Dynamic EDG discriminants are checked by generation/dispatch and genuine
exports. This check covers static adapter/helper tags before compiler builds.
"""
import csv
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[1]
catalog = {(r['family'], r['name']) for r in
           csv.DictReader((ROOT / 'schema/kinds.tsv').open(), delimiter='\t')}
patterns = [
    (re.compile(r'\b(?:K|roslyn_tag)\("([^"\n]+)",\s*"([^"\n]+)"'), None),
    (re.compile(r'\bDB\.fresh\("([^"\n]+)"'), 'entity'),
    (re.compile(r'\bDB\.claim\([^,\n]+,\s*"([^"\n]+)"'), 'entity'),
    (re.compile(r'\broslyn_fact\([^,\n]+,\s*"([^"\n]+)"'), 'fact'),
    (re.compile(r'\broslyn_(?:child|reference)\([^,\n]+,\s*"([^"\n]+)"'), 'role'),
    (re.compile(r'\broslyn_constant_component\([^,\n]+,\s*"([^"\n]+)"'), 'constant_component_role'),
]
checked = set()
errors = []
for path in sorted((ROOT / 'backend').glob('*')):
    if path.suffix not in ('.c', '.h') or path.name in ('roslyn_kinds.h', 'roslyn_schema.h'):
        continue
    source = path.read_text()
    source = re.sub(r'/\*.*?\*/', lambda m: '\n' * m.group(0).count('\n'), source, flags=re.S)
    source = re.sub(r'//[^\n]*', '', source)
    for pattern, family in patterns:
        for match in pattern.finditer(source):
            tag = (family, match.group(1)) if family else match.groups()
            checked.add(tag)
            if tag not in catalog:
                line = source.count('\n', 0, match.start()) + 1
                errors.append(f'{path.relative_to(ROOT)}:{line}: missing {tag[0]}.{tag[1]}')
if errors:
    raise SystemExit('\n'.join(errors))
print(f'{len(checked)} literal native catalog tags validated')
