"""Preserve the pinned source's individual notices alongside translated output."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def render(reference):
    reference = Path(reference)
    pin = json.loads((ROOT / 'config/source.json').read_text())
    files = [name for name in pin['required'] if name.endswith(('.c', '.h'))]
    parts = [f"QuickJS {pin['version']} — upstream notices\n",
             'The following notices are reproduced from the acquired upstream sources.\n'
             'The generated C# retains these notices in this companion file.\n',
             '=== Archive LICENSE ===\n' + (reference / 'LICENSE').read_text().rstrip()]
    notices, generated = [], []
    for name in files:
        text = (reference / name).read_text()
        end = text.find('*/')
        if not text.startswith('/*') or end < 0:
            raise RuntimeError(f'Expected reviewed leading source header: {name}')
        header = text[:end + 2]
        if 'Copyright' in header:
            parts.append(f'=== {name} ===\n' + header)
            notices.append(name)
        elif name == 'libunicode-table.h' and '/* Automatically generated file - do not edit */' in text[:200]:
            generated.append(name)
        else:
            raise RuntimeError(f'Unreviewed notice-free source header: {name}')
    if generated:
        parts.append('=== Generated upstream data ===\n'
                     + ', '.join(generated) + '\n'
                     + 'This file has an automatically-generated banner and no separate copyright notice.\n'
                     + 'The archive LICENSE is reproduced above.')
    return '\n\n'.join(parts) + '\n', notices


def validate(reference, asset=None):
    asset = Path(asset or ROOT / 'THIRD-PARTY-NOTICES')
    expected, files = render(reference)
    if not asset.is_file() or asset.read_text() != expected:
        raise RuntimeError('Upstream notices changed; regenerate and review THIRD-PARTY-NOTICES '
                           'with scripts/notices.py --source SOURCE --write')
    return {'asset': str(asset), 'sha256': hashlib.sha256(asset.read_bytes()).hexdigest(),
            'source_notices': files, 'archive_license': 'LICENSE'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True, type=Path)
    parser.add_argument('--write', action='store_true')
    args = parser.parse_args()
    if args.write:
        (ROOT / 'THIRD-PARTY-NOTICES').write_text(render(args.source)[0])
    print(json.dumps(validate(args.source), indent=2))
