"""Audit owning calls against actual emitted object signatures; never execute a substitute engine."""
import argparse
import base64
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]


def audit(objects):
    authored = '\n'.join(p.read_text() for p in (ROOT / 'src/Managed.S2nTls').glob('*.cs'))
    calls = sorted(set(re.findall(r'Raw\.((?:s2n_|dotcc_s2n_)\w+)\s*(?:\(|[,;])', authored)))
    mapping, signatures, units = {}, {}, []
    for path in sorted(objects.glob('*.cs')):
        text = path.read_text()
        units.append({'path': str(path), 'sha256': hashlib.sha256(text.encode()).hexdigest()})
        names = {}
        for identity, encoded in re.findall(r'^//!!dotcc-obj aggregate-identity:([^ ]+) ([^ ]+)', text, re.M):
            names[identity] = base64.b64decode(encoded).decode()
        for identity, encoded in re.findall(r'^//!!dotcc-obj anonymous-typedef:([^ ]+) ([^\s]+)', text, re.M):
            names[identity] = base64.b64decode(encoded).decode()
        for identity, name in names.items():
            mapping['__DotCcAggregate_' + hashlib.sha256(identity.encode()).hexdigest().upper()] = name
        for declaration in re.findall(r'^static unsafe ([^\n]+)', text, re.M):
            match = re.search(r'\b((?:s2n_|dotcc_s2n_)\w+)\(', declaration)
            if match and match[1] in calls:
                signatures.setdefault(match[1], set()).add(declaration)
    normalized = {}
    unresolved = []
    for name, declarations in signatures.items():
        rewritten = []
        for declaration in declarations:
            declaration = re.sub(r'__DotCcAggregate_[A-F0-9]+', lambda m: mapping.get(m[0], m[0]), declaration)
            rewritten.append(declaration)
            if '__DotCcAggregate_' in declaration:
                unresolved.append(name)
        normalized[name] = sorted(set(rewritten))
    missing = sorted(set(calls) - signatures.keys())
    controls = json.loads((ROOT / 'config/compatibility.json').read_text())['controls']
    covered, omitted = [], []
    for row in controls:
        if row['status'] == 'excluded': continue
        # The upstream random callback API is deliberately a documented no-op,
        # not a selectable entropy source in the owning API.
        if row['api'] in calls or row['api'] == 's2n_rand_set_callbacks': covered.append(row['api'])
        else: omitted.append(row['api'])
    return {'evidence_kind': 'static-emitted-api-contract', 'managed_execution': False,
            'status': 'passed' if not missing and not unresolved and not omitted else 'failed',
            'calls': calls, 'signatures': normalized, 'missing_emission': missing,
            'unresolved_types': sorted(set(unresolved)), 'owning_controls': covered, 'missing_controls': omitted,
            'objects': units,
            'limits': ['No ABI layout, linking, C# compilation, TLS execution or runtime qualification follows from this audit.']}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--objects', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.objects.resolve())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps({k: report[k] for k in ('status', 'missing_emission', 'unresolved_types', 'missing_controls')}))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
