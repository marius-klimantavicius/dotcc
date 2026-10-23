"""Immutable completed-translation evidence within a still-running verification.

The parent verification receipt remains running until its behavioral gates pass.
This separate receipt covers only actual C emission, linkage, builds/publication.
"""
import copy
import hashlib
import json
from pathlib import Path
import sys
import time
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / 'Scripts'))
from campaigns.delivery import source_files


def snapshot_path(root, campaign):
    return Path(root) / 'artifacts/campaign' / Path(campaign).name / 'translation-receipt.json'


def check(record):
    if len(record.get('units', ())) != 175 or len(set(record['units'])) != 175:
        raise RuntimeError('Complete unique175-unit engine/adapter translation is required')
    if not record.get('outputs') or not set(record['outputs']) <= {'raw', 'processed'}:
        raise RuntimeError('Actual published translation outputs are missing')
    commands = record.get('commands', [])
    if not commands or any(command.get('exit_code') != 0 or command.get('status') != 'passed'
                           for command in commands):
        raise RuntimeError('Translation evidence contains an unsuccessful command')
    emitted = [row for row in commands if '--emit=obj' in row.get('command', [])]
    if len(emitted) != 175:
        raise RuntimeError('Translation evidence must record every175 actual object emission commands')
    actual = []
    for row in emitted:
        sources = [str(Path(arg).resolve()) for arg in row['command'] if arg.endswith('.c') and not arg.startswith('-')]
        if len(sources) != 1:
            raise RuntimeError('An actual emission command must identify exactly one C source')
        actual.extend(sources)
    if len(set(actual)) != 175 or set(actual) != {str(Path(path).resolve()) for path in record['units']}:
        raise RuntimeError('Actual emitted C sources do not cover the complete manifest exactly')
    for form, output in record['outputs'].items():
        if not output.get('project'):
            raise RuntimeError('Missing actual output identity: ' + form)


def freeze(ctx):
    check(ctx.receipt)
    observed = {}
    for form, output in ctx.receipt['outputs'].items():
        directory = Path(output['project']).parent
        observed[form] = {path.relative_to(directory).as_posix(): hashlib.sha256(path.read_bytes()).hexdigest()
                          for path in source_files(directory)}
        if not observed[form] or not Path(output['project']).is_file():
            raise RuntimeError('Actual generated translation output is missing: ' + form)
        for relative, expected in output['hashes'].items():
            path = directory / relative
            if hashlib.sha256(path.read_bytes()).hexdigest() != expected:
                raise RuntimeError('Actual published translation changed before closure: ' + str(path))
    path = snapshot_path(ctx.root, ctx.work)
    if path.exists():
        raise RuntimeError('A completed translation snapshot may not be overwritten')
    record = copy.deepcopy(ctx.receipt)
    record.update(evidence_kind='completed-translation-closure', status='passed',
                  scope='175 C objects, linkage and selected actual published builds only; behavioral verification remains separate',
                  parent_verification_status=ctx.receipt['status'], closed_at=time.time(),
                  parent_receipt=str(ctx.artifacts / 'receipt.json'),
                  closure_output_sha256=observed)
    temporary = path.with_suffix('.tmp')
    temporary.write_text(json.dumps(record, indent=2) + '\n')
    temporary.replace(path)
    ctx.receipt['translation_closure'] = {'receipt': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}
    ctx.save()
    return path


def read_closed(root, campaign, *, forms=('raw', 'processed')):
    root, campaign = Path(root), Path(campaign).resolve()
    path = snapshot_path(root, campaign)
    if path.exists():
        record = json.loads(path.read_text())
        if record.get('evidence_kind') != 'completed-translation-closure':
            raise RuntimeError('Unexpected translation snapshot scope')
    else:
        path = root / 'artifacts/campaign' / campaign.name / 'receipt.json'
        record = json.loads(path.read_text())
    if record.get('status') != 'passed' or record.get('run_id') != campaign.name:
        raise RuntimeError('A closed successful translation is required; running verification is not a pass')
    check(record)
    if set(forms) - record['outputs'].keys():
        raise RuntimeError('Required actual translation forms are missing')
    return path, record
