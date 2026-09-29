"""Preserve source-specific redistribution notices from selected bundled inputs."""
from collections import defaultdict
from pathlib import Path
import re


def render(source):
    source = Path(source)
    notices = defaultdict(list)
    for directory in ('src', 'contrib'):
        for path in sorted((source / directory).rglob('*')):
            if not path.is_file() or path.suffix.lower() not in ('.c', '.h'):
                continue
            text = path.read_text(errors='replace')
            # Preserve complete comment blocks, rather than reconstructing license
            # wording. Upstream notices occur in the first comment section.
            for match in re.finditer(r'/\*.*?\*/', text[:20000], re.S):
                block = match.group(0)
                if re.search(r'copyright|redistribution and use|permission is hereby granted', block, re.I):
                    notices[block].append(path.relative_to(source).as_posix())
    body = ['lwIP and bundled-source notices',
            '===============================',
            'The following notices are preserved from the pinned source tree, including',
            'separately delivered tools/examples and optional bundled modules. Inclusion',
            'of a notice does not claim that every optional module is enabled.', '',
            'COPYING', '-------', (source / 'COPYING').read_text(), '']
    for block, paths in notices.items():
        body.extend(['Files: ' + ', '.join(paths), '', block, ''])
    if not any('tinydir' in p for paths in notices.values() for p in paths):
        raise RuntimeError('Missing bundled tinydir redistribution notice')
    if not any('/ppp/polarssl/' in p for paths in notices.values() for p in paths):
        raise RuntimeError('Missing bundled PPP cryptographic primitive notices')
    return '\n'.join(body) + '\n'
