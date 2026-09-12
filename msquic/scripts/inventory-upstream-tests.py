#!/usr/bin/env python3
"""Inventory literal upstream GoogleTest declarations, without claiming execution."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
# Ignore comments and string/character contents, retaining newlines for locations.
TRIVIA = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
TEST = re.compile(r'\b(TEST|TEST_F|TEST_P|TYPED_TEST|TYPED_TEST_P)\s*\(\s*([A-Za-z_]\w*)\s*,\s*([A-Za-z_]\w*)\s*\)')


def inventory():
    pin_path = ROOT / 'config/source.json'
    pin = json.loads(pin_path.read_text())
    source = ROOT / 'ref' / pin['directory']
    manifest = dict(format='dotcc-msquic-upstream-test-inventory-v1', commit=pin['commit'],
        source_archive_sha256=pin['sha256'], source_pin_sha256=hashlib.sha256(pin_path.read_bytes()).hexdigest(),
        scope='Literal GoogleTest declarations in src C/C++ sources; all preprocessor branches included',
        execution_claim=False, parameter_expansions_counted=False, files=[], cases=[])
    for path in sorted((source / 'src').rglob('*')):
        if path.suffix not in ('.c', '.cpp', '.h', '.hpp'):
            continue
        text = path.read_text(errors='strict')
        cleaned = TRIVIA.sub(lambda match: ''.join('\n' if c == '\n' else ' ' for c in match[0]), text)
        matches = list(TEST.finditer(cleaned))
        if not matches:
            continue
        relative = str(path.relative_to(source))
        category = ('core-unit' if '/core/unittest/' in relative else
                    'platform-unit' if '/platform/unittest/' in relative else
                    'transport-api' if relative.startswith('src/test/') else 'other')
        manifest['files'].append(dict(path=relative, sha256=hashlib.sha256(path.read_bytes()).hexdigest(), declarations=len(matches), category=category))
        for match in matches:
            manifest['cases'].append(dict(path=relative, line=cleaned.count('\n', 0, match.start()) + 1,
                macro=match[1], suite=match[2], name=match[3], category=category,
                direct_port_status='unqualified'))
    manifest['declaration_count'] = len(manifest['cases'])
    manifest['category_counts'] = dict(sorted(Counter(case['category'] for case in manifest['cases']).items()))
    manifest['macro_counts'] = dict(sorted(Counter(case['macro'] for case in manifest['cases']).items()))
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true', help='Compare the checked-in inventory without rewriting it')
    args = parser.parse_args()
    path = ROOT / 'config/upstream-test-inventory.json'
    result = inventory()
    encoded = json.dumps(result, indent=2) + '\n'
    if args.check:
        if path.read_text() != encoded:
            raise RuntimeError('Upstream test inventory differs from pinned declarations')
    else:
        path.write_text(encoded)
    print(json.dumps({key: result[key] for key in ['declaration_count', 'category_counts', 'macro_counts']}))


if __name__ == '__main__':
    main()
