"""Guarded source staging; acquired references are never edited."""
import hashlib
import json
from pathlib import Path
import shutil


def metadata(root):
    result = json.loads((root / 'config/adaptations.json').read_text())
    allowed = {'makefsdata-checksum-progress', 'ipv4-forwarding-software-checksums'}
    declared = [item['id'] for item in result['textPatches']]
    if len(declared) != len(set(declared)) or set(declared) != allowed:
        raise RuntimeError('Adaptation declarations must match the two supported, profile-assigned repairs')
    return result


def makefsdata(root, source, destination):
    root, source, destination = map(Path, (root, source, destination))
    shutil.copytree(source / 'src', destination / 'src', dirs_exist_ok=True)
    repairs = metadata(root)
    evidence = []
    for repair in repairs['textPatches']:
        if repair['id'] != 'makefsdata-checksum-progress':
            continue
        target = destination / repair['path']
        text = target.read_text()
        if text.count(repair['before']) != 1:
            raise RuntimeError('Upstream checksum-loop adaptation requires one unique local anchor')
        target.write_text(text.replace(repair['before'], repair['after'], 1))
        evidence.append({'id': repair['id'], 'path': repair['path'], 'matches': 1,
                         'kind': 'upstream-bug-repair', 'reason': repair['reason']})
    return evidence


def ipv4_forwarding(root, source, destination):
    """Stage one corrected translation unit and return (path, evidence)."""
    root, source, destination = map(Path, (root, source, destination))
    repairs = metadata(root)
    repair = next(item for item in repairs['textPatches']
                  if item['id'] == 'ipv4-forwarding-software-checksums')
    original = source / repair['path']
    data = original.read_bytes()
    text = data.decode()
    changes = []
    for change in repair['changes']:
        if text.count(change['before']) != 1:
            raise RuntimeError('IPv4 forwarding adaptation requires one unique local condition: ' + change['before'])
        text = text.replace(change['before'], change['after'], 1)
        changes.append({**change, 'matches': 1})
    target = destination / repair['path']
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(text)
    evidence = [{'id': repair['id'], 'path': repair['path'],
                 'kind': repair['classification'], 'reason': repair['reason'],
                 'source_sha256': hashlib.sha256(data).hexdigest(),
                 'staged_sha256': hashlib.sha256(target.read_bytes()).hexdigest(),
                 'changes': changes, 'scope': 'Software checksum profiles; hardware offload unqualified'}]
    (destination / 'forwarding-adaptation.json').write_text(json.dumps(evidence, indent=2) + '\n')
    return target, evidence
