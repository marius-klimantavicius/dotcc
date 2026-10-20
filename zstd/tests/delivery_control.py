#!/usr/bin/env python3
"""Exercise delivery's actual copy path with private stale receipt inputs."""
from __future__ import annotations
import argparse
import importlib.util
import json
from pathlib import Path
import tempfile


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists():
        raise RuntimeError('Refusing to overwrite control output')
    args.output.mkdir(parents=True)
    spec = importlib.util.spec_from_file_location('zstd_delivery_control_target', Path(__file__).with_name('delivery.py'))
    delivery = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(delivery)
    report = dict(status='running', cases=[])
    with tempfile.TemporaryDirectory(prefix='zstd-delivery-control-') as temporary:
        root = Path(temporary)
        delivery.REPO = root
        source = root / 'zstd/src/Optimization/Primitives.cs'
        source.parent.mkdir(parents=True)
        source.write_text('// Private original adapter fixture.\n')
        receipt = dict(production_inputs={str(source): delivery.sha(source)})
        relative, entry = delivery.copy_source(source, root / 'matching-copy', receipt['production_inputs'])
        assert entry['receipt_production_sha256'] == receipt['production_inputs'][str(source)]
        assert entry['sha256'] == delivery.sha(root / 'matching-copy' / relative)
        report['cases'].append(dict(name='matching-recorded-input', status='passed'))
        source.write_text('// Private changed adapter fixture, same stale receipt.\n')
        for name, stale in [('stale-adapter', source), ('stale-root-metadata', root / 'Directory.Build.props')]:
            if stale != source:
                stale.write_text('<Project/>\n')
                receipt['production_inputs'][str(stale)] = delivery.sha(stale)
                stale.write_text('<Project><!-- private changed metadata --></Project>\n')
            destination = root / name
            try:
                delivery.copy_source(stale, destination, receipt['production_inputs'])
            except RuntimeError as error:
                if 'Verified production input changed:' not in str(error):
                    raise
                assert not destination.exists()
                report['cases'].append(dict(name=name, status='passed', rejection=str(error)))
            else:
                raise RuntimeError('Stale receipt input was accepted')
        relative, entry = delivery.copy_source(source, root / 'unrecorded-copy', {})
        assert entry['receipt_production_sha256'] is None and entry['receipt_output_sha256'] is None
        assert entry['sha256'] == delivery.sha(root / 'unrecorded-copy' / relative)
        report['cases'].append(dict(name='unrecorded-input-explicitly-unbound', status='passed'))
    report['status'] = 'passed'
    report['delivery_helper_sha256'] = delivery.sha(Path(__file__).with_name('delivery.py'))
    (args.output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print(args.output / 'report.json')


if __name__ == '__main__':
    main()
