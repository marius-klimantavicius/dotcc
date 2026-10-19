"""Review actual ordered unit options and all source header feature guards."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sys
from types import SimpleNamespace


def review(args):
    root = Path(__file__).resolve().parents[2]
    sys.path.insert(0, str(root.parent / 'Scripts'))
    spec = importlib.util.spec_from_file_location('r37_config_review', root / 'scripts/pyconfig.py')
    config = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(config)
    receipt = json.loads(args.receipt.read_text())
    rows = receipt['profile_feature_policy']['units']
    # First native-selected source path is Modules/getbuildinfo.c.
    source = Path(rows[0]['path']).parents[1]
    product = json.loads((root / 'config/sources.json').read_text())
    product_paths = {str(source / row['path']) for row in product['units']}
    product_paths.add(str(root / 'src/Host/dotcc_cpython_host.c'))
    result = {'scope': 'source/header/every-unit option audit; no actual new product or layout pass',
              'referenceReceipt': str(args.receipt.resolve()), 'profiles': {}}
    header = args.report.parent / (args.report.stem + '.h')
    for profile, selected in [('embedded-gil', [row for row in rows if row['path'] in product_paths]),
                              ('upstream-tests-static', rows)]:
        if len(selected) != (193 if profile == 'embedded-gil' else 286):
            raise RuntimeError('Ordered actual profile unit closure differs')
        resolved = config.resolve(json.loads((root / 'config/pyconfig-policy.json').read_text()), profile)
        header.write_text(config.render(resolved))
        ctx = SimpleNamespace(root=root, profile=profile, receipt={})
        config.bind(ctx, resolved, header,
                    [SimpleNamespace(path=Path(row['path']), flags=tuple(row['ordered_options'])) for row in selected], [])
        result['profiles'][profile] = ctx.receipt['profile_feature_policy']
    files = {Path(row['path']) for row in rows}
    files.update(source.rglob('*.h'))
    files.update([root / 'src/Host/dotcc_cpython_host.h', root.parent / 'DotCC.Lib/include/unistd.h',
                  root.parent / 'DotCC.Lib/include/sys/stat.h', root.parent / 'DotCC.Lib/include/fcntl.h'])
    result['featureSourceSites'] = []
    result['upstreamCoreLayoutSites'] = []
    for path in sorted(files):
        if not path.is_file():
            raise RuntimeError('Actual reviewed header/source absent: ' + str(path))
        lines = path.read_text(errors='replace').splitlines()
        hits = [{'line': number, 'text': line} for number, line in enumerate(lines, 1)
                if re.search(r'\bHAVE_(PIPE|SYMLINK)\b', line)]
        if hits:
            row = {'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'sites': hits}
            result['featureSourceSites'].append(row)
            if 'Include' in path.parts:
                result['upstreamCoreLayoutSites'].append(row)
    if result['upstreamCoreLayoutSites']:
        raise RuntimeError('Review newly reached core layout feature sites')
    result['branchReview'] = {
        'pipe': 'posix HAVE_PIPE -> real pipe/_Py_set_inheritable/fcntl/close/Py_BuildValue and Clinic METH_NOARGS. HAVE_PIPE2 remains absent. Both profiles select posix.',
        'symlink': 'posix+Clinic HAVE_SYMLINK -> narrow symlink/audit/GIL/error/path conversion. Existing READLINK/LSTAT retained; SYMLINKAT remains absent.',
        'platformDefines': 'The source-owned HAVE_PIPE/SYMLINK definitions in posix are MS_WINDOWS branches, inactive for selected Linux profile.',
        'layout': 'No HAVE_PIPE/SYMLINK guard in any pinned upstream Include header. No source/algorithm/module closure is changed by this policy.',
        'qualification': 'Source guard/header proof only. Services and whole-unit/new-product qualification remain separate.'}
    args.report.write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'profiles': {name: len(row['units']) for name, row in result['profiles'].items()},
                      'sourceOwners': len(result['featureSourceSites']), 'layoutGuardSites': 0}))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--receipt', type=Path, required=True)
    parser.add_argument('--report', type=Path, required=True)
    review(parser.parse_args())
