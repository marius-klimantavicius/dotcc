#!/usr/bin/env python3
"""Verify large formatter response chunking at a UTF-16 surrogate boundary."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT))
from backend.source_format import format_sources


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    small = {'Small.cs': 'class Small { int F(){return 1;} }'}
    small_result, metadata = format_sources(ROOT, small)
    tool = ROOT / 'artifacts/source-formatter' / metadata['sourceSha256'] / 'lib/Edg.SourceFormatter.dll'
    boundary = 16 * 1024 * 1024
    # Emoji starts at UTF-16 offset boundary-1; a naive Substring would split it.
    comment = '/*' + 'a' * (boundary - 3) + '\U0001f642\\"\n\u03bb\n*/'
    source = comment + '\nclass Large { int F(){return 1;} }'
    sources = dict(small, **{'Large.cs': source})
    result = subprocess.run(['dotnet', str(tool)], input=json.dumps(sources),
                            capture_output=True, text=True, check=True)
    response = json.loads(result.stdout)
    assert response['diagnostics'] == []
    assert isinstance(response['files']['Small.cs'], str)
    assert response['files']['Small.cs'] == small_result['Small.cs']
    chunks = response['files']['Large.cs']
    assert isinstance(chunks, list) and len(chunks) == 2
    assert len(chunks[0]) == boundary - 1
    assert chunks[1].startswith('\U0001f642')
    joined = ''.join(chunks)
    assert joined.startswith(comment)
    assert '    int F()\n    {\n        return 1;\n    }' in joined
    # Exercise the Python decoder/rejoin too, against the direct protocol result.
    decoded, _ = format_sources(ROOT, sources)
    assert decoded['Large.cs'] == joined
    (out / 'results.json').write_text(json.dumps({
        'formatter': metadata, 'chunkCharacters': [len(c) for c in chunks],
        'sha256': hashlib.sha256(joined.encode()).hexdigest(),
        'surrogateBoundaryPreserved': True, 'escapedUnicodeAndNewlinesPreserved': True,
        'smallOutputUnchanged': True, 'pythonRejoinIdentical': True,
    }, indent=2) + '\n')
    print('PASS: chunk boundary, surrogate pair, escaping, newlines, small response, Python rejoin')


if __name__ == '__main__':
    main()
