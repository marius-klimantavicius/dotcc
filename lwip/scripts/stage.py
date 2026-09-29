"""Guarded source staging; acquired references are never edited."""
import json
from pathlib import Path
import shutil


def makefsdata(root, source, destination):
    root, source, destination = map(Path, (root, source, destination))
    shutil.copytree(source / 'src', destination / 'src', dirs_exist_ok=True)
    metadata = json.loads((root / 'config/adaptations.json').read_text())
    evidence = []
    for repair in metadata['textPatches']:
        if repair['id'] != 'makefsdata-checksum-progress':
            raise RuntimeError('Unknown adaptation: ' + repair['id'])
        target = destination / repair['path']
        text = target.read_text()
        if text.count(repair['before']) != 1:
            raise RuntimeError('Upstream checksum-loop adaptation requires one unique local anchor')
        target.write_text(text.replace(repair['before'], repair['after'], 1))
        evidence.append({'id': repair['id'], 'path': repair['path'], 'matches': 1,
                         'kind': 'upstream-bug-repair', 'reason': repair['reason']})
    return evidence
