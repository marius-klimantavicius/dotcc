#!/usr/bin/env python3
"""Compile staged upstream core TUs separately to isolate the first failure."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import time
from core_inputs import compiler_identity, profile_sources, canonical_emission, semantic_selection, OBJECT_OPTIONS

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', type=Path, required=True, help='immutable generated/core-profile/attempt-* directory')
parser.add_argument('--timeout', type=float, default=20)
parser.add_argument('--start', default='', help='first source filename, e.g. machine.c')
parser.add_argument('--only', action='store_true', help='stop after the first selected source, even if it emits')
args = parser.parse_args()
profile = args.profile.resolve()
profile_inputs = json.loads((profile / 'inputs.json').read_text())
for name, expected in profile_inputs['staged_headers'].items():
    if hashlib.sha256((profile / name).read_bytes()).hexdigest() != expected:
        raise SystemExit('staged profile checksum mismatch: ' + name)
out = Path(tempfile.mkdtemp(prefix='isolate-', dir=root / 'artifacts/core'))
closure_path = profile / 'closure.json'
if not closure_path.exists():
    closure_path = root / 'artifacts/core/closure.json'  # historical diagnostic snapshots
closure = {'sources': profile_sources(profile, root, profile_inputs)}
compiler = root.parent / 'DotCC/bin/Release/net10.0'
receipt = {'profile': str(profile), 'profile_inputs_sha256': hashlib.sha256((profile / 'inputs.json').read_bytes()).hexdigest(),
           'compiler_sha256': compiler_identity(compiler),
           'compiler_identity_script_sha256': hashlib.sha256((root / 'scripts/core_inputs.py').read_bytes()).hexdigest(),
           'timeout_seconds': args.timeout, 'isolation_script_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
           'closure_sha256': hashlib.sha256(closure_path.read_bytes()).hexdigest(), 'rows': []}
active = not args.start
for entry in closure['sources']:
    source = root / entry['staged_path']
    if source.name == args.start:
        active = True
    if not active:
        continue
    if hashlib.sha256(source.read_bytes()).hexdigest() != entry['sha256']:
        raise SystemExit('staged source checksum mismatch: ' + str(source))
    original_source = source
    emission_profile, source, c_identity, emission_key = canonical_emission(profile, root, profile_inputs, entry)
    command = ['dotnet', str(compiler / 'dotcc.dll'), *OBJECT_OPTIONS,
               '-I', str(emission_profile), '-I', str(root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'),
               '-I', str(emission_profile / 'authored'), '-I', str(emission_profile / 'host'),
               '--overrides-file', str(emission_profile / 'overrides.json')]
    log = out / (source.stem + '.log')
    invocation = command + [str(source), '-o', str(out / (source.stem + '.cs')),
                            '--override-report', str(out / (source.stem + '.overrides.jsonl'))]
    started = time.monotonic()
    with log.open('wb') as stream:
        try:
            result = subprocess.run(invocation, stdout=stream, stderr=subprocess.STDOUT, timeout=args.timeout)
            code = result.returncode
            kind = 'emitted object' if code == 0 else 'compiler diagnostic'
        except subprocess.TimeoutExpired:
            code = 124
            kind = 'per-TU timeout; not an architectural blocker'
    inactive_overrides = None
    report = out / (source.stem + '.overrides.jsonl')
    if code == 2 and 'requireMatch: no active definition was selected' in log.read_text() and report.is_file():
        events = [json.loads(line) for line in report.read_text().splitlines() if line]
        # A small TU such as dll.c never includes builtin.h. Required matches
        # remain mandatory for the campaign, but cannot occur in that TU. Only
        # retry when the actual preprocessing trace proves no rule encountered
        # any definition; a mismatching or partly selected profile still fails.
        if events and not any(event.get('event') in {'candidate', 'selected', 'expansion'} for event in events):
            first_log = out / (source.stem + '.required-overrides.log')
            first_report = out / (source.stem + '.required-overrides.jsonl')
            log.rename(first_log)
            report.rename(first_report)
            inactive_overrides = dict(command=list(invocation), diagnostic_log=str(first_log),
                                      report=str(first_report), reason='no override definition active in this TU')
            # Only the absent macro definitions are optional here. Preserve
            # typed function rules and their actual per-unit selection report.
            retry_profile = json.loads((emission_profile / 'overrides.json').read_text())
            retry_profile['macroOverrides'] = []
            retry_path = out / (source.stem + '.optional-macros.json')
            retry_path.write_text(json.dumps(retry_profile, indent=2) + '\n')
            invocation[invocation.index('--overrides-file') + 1] = str(retry_path)
            with log.open('wb') as stream:
                try:
                    result = subprocess.run(invocation, stdout=stream, stderr=subprocess.STDOUT,
                                            timeout=max(0.01, args.timeout - (time.monotonic() - started)))
                    code = result.returncode
                    kind = 'emitted object; no override definitions active' if code == 0 else 'compiler diagnostic'
                except subprocess.TimeoutExpired:
                    code = 124
                    kind = 'per-TU timeout; not an architectural blocker'
    row = {'source': entry['path'], 'source_sha256': entry['sha256'], 'exit_code': code,
           'classification': kind, 'seconds': time.monotonic() - started, 'log': str(log),
           'command': invocation, 'emission_key': emission_key, 'emission_identity': c_identity,
           'canonical_source': str(source), 'original_source': str(original_source)}
    if inactive_overrides:
        row['inactive_overrides'] = inactive_overrides
    if receipt['compiler_sha256'] != compiler_identity(compiler):
        code = 125
        row['exit_code'] = code
        row['classification'] = 'compiler changed during invocation; retry required'
    if code == 0:
        row['semantic_intrinsics'] = semantic_selection(emission_profile, report, entry['path'])
        artifact = out / (source.stem + '.cs')
        row['object_path'] = str(artifact)
        row['object_sha256'] = hashlib.sha256(artifact.read_bytes()).hexdigest()
    receipt['rows'].append(row)
    (out / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: row[key] for key in ('source', 'exit_code', 'classification', 'seconds', 'log')}), flush=True)
    if code:
        raise SystemExit(code)
    if args.only:
        break
if not receipt['rows']:
    raise SystemExit('no source matched --start')
