#!/usr/bin/env python3
"""Validate official Npgsql test provenance, complete numeric vectors and mapped case inventory."""
import argparse
import hashlib
import json
from pathlib import Path
import re


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-root', required=True, type=Path, help='Extracted official npgsql v10.0.1 archive root')
    parser.add_argument('--report', required=True, type=Path)
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1] / 'tests/NpgsqlAcceptance'
    mapping = json.loads((project / 'source-mapping.json').read_text())
    assertions = 0
    for file, record in mapping['sources'].items():
        path = args.source_root / record['path']
        if hashlib.sha256(path.read_bytes()).hexdigest() != record['sha256']:
            raise AssertionError('Official test source differs: ' + str(path))
        assertions += 1
    for case in mapping['cases']:
        for source in case['sources']:
            text = (args.source_root / 'test/Npgsql.Tests' / source['file']).read_text(encoding='utf-8-sig')
            for method in source['methods']:
                if not re.search(r'\b' + re.escape(method) + r'(?:<[^>]+>)?\(', text):
                    raise AssertionError('Missing mapped upstream method: ' + source['file'] + ':' + method)
                assertions += 1
    original = (args.source_root / 'test/Npgsql.Tests/Types/NumericTests.cs').read_text(encoding='utf-8-sig')
    vector_section = original[original.index('    static readonly object[] ReadWriteCases ='):original.index('    [Test]')]
    expected_vectors = re.findall(r'new object\[\] \{ (.*) \}', vector_section)
    actual_vectors = re.findall(r'^\s*\((.*)\),?$', (project / 'NumericVectors.cs').read_text(), re.M)
    if actual_vectors != expected_vectors:
        raise AssertionError('Numeric inputs/constants/order differ from complete upstream ReadWriteCases')
    assertions += 1
    defined = []
    for path in project.glob('*.cs'):
        defined += re.findall(r'(?:yield return new\(|new AcceptanceCase\()"([\w.-]+)"', path.read_text())
    mapped = [case['id'] for case in mapping['cases']]
    if len(set(defined)) != len(defined) or set(defined) != set(mapped):
        raise AssertionError('Acceptance case inventory differs from mapping: ' + repr((sorted(set(defined)-set(mapped)), sorted(set(mapped)-set(defined)))))
    assertions += 1
    receipt = dict(status='passed', release=mapping['release'], commit=mapping['commit'],
                   assertions=assertions, source_files=len(mapping['sources']),
                   mapped_cases=len(mapped), numeric_vectors=len(actual_vectors),
                   role='provenance-and-inventory-only; runtime acceptance separately required')
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(receipt, indent=2) + '\n')
    print(f"PASS {assertions} source assertions; {len(mapped)} mapped cases; {len(actual_vectors)} exact upstream numeric vectors; report={args.report}")


if __name__ == '__main__':
    main()
