#!/usr/bin/env python3
"""Run Npgsql SQL vectors with the matching-pin native callback engine (oracle only)."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import tempfile


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    root = Path(__file__).resolve().parents[1]
    metadata = json.loads((root / 'tests/NpgsqlAcceptance/source-mapping.json').read_text())
    core_pin = metadata['core_pin']
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native-cache', type=Path, default=root / 'build/native-control/14abb02f16cfb8013f7c')
    parser.add_argument('--control', type=Path, default=root / 'build/native-control/runs/20261022-163448-222180a4/native-control/embedding-control')
    parser.add_argument('--work-root', type=Path)
    args = parser.parse_args()
    cache, control = args.native_cache.resolve(), args.control.resolve()
    work = (args.work_root or Path(tempfile.mkdtemp(prefix='dotcc-npgsql-native-oracle-'))).resolve()
    work.mkdir(parents=True, exist_ok=True)
    prepared = json.loads((cache / 'prepared.json').read_text())
    manifest = json.loads((cache / 'native-source-manifest.json').read_text())
    if prepared['requested_revision'] != core_pin or manifest['revision'] != core_pin:
        raise RuntimeError('Native oracle must use the selected engine pin ' + core_pin)
    if prepared['profile'] != 'embedding-native':
        raise RuntimeError('Native oracle must use the matching embedding-native profile')
    prefix = Path(prepared['prefix'])
    # Cached controls contain their configured installation path. Fail if the
    # supplied binary and installation differ instead of silently mixing them.
    if str(prefix).encode() not in control.read_bytes():
        raise RuntimeError('Control does not identify selected native installation: ' + str(prefix))
    helper_spec = importlib.util.spec_from_file_location('npgsql_native_wire', root / 'scripts/native-control.py')
    helper = importlib.util.module_from_spec(helper_spec)
    helper_spec.loader.exec_module(helper)
    cases_path = root / 'tests/NpgsqlAcceptance/oracle-cases.json'
    cases = json.loads(cases_path.read_text())
    queries = work / 'queries.sql'
    queries.write_text(''.join(case['sql'] + '\n' for case in cases), encoding='utf-8')
    responses = work / 'responses'
    responses.mkdir()
    environment = dict(os.environ, LC_ALL='C', TZ='UTC')
    commands = []

    def run(command, log_name, timeout=180):
        path = work / log_name
        with path.open('w') as log:
            result = subprocess.run(list(map(str, command)), cwd=work, env=environment,
                                    stdout=log, stderr=subprocess.STDOUT, timeout=timeout)
        commands.append(dict(command=list(map(str, command)), exit_code=result.returncode,
                             log=str(path), log_sha256=sha(path)))
        if result.returncode:
            raise RuntimeError('Native oracle process failed: ' + str(path))

    run([prefix / 'bin/initdb-control', '-D', work / 'data', '-L', prefix / 'share/postgresql',
         '--no-locale', '--encoding=UTF8', '--auth=trust', '-U', 'postgres', '--no-sync'], 'initdb.log', 300)
    run([control, prefix / 'bin/postgres', work / 'data', queries, responses], 'engine.log')
    transcripts = sorted(responses.glob('*.wire'))
    if len(transcripts) != len(cases):
        raise RuntimeError('Native transcript count differs from case inventory')
    observations = []
    for case, wire in zip(cases, transcripts):
        observed = helper.parse_wire(wire.read_bytes())
        helper.compare(case, observed)
        observations.append(dict(name=case['name'], status='passed', observed=observed,
                                 transcript=str(wire), transcript_sha256=sha(wire)))
    # Rejection control validates the checker rather than just trusting a zero exit.
    try:
        helper.compare(dict(name='deliberate-wrong-row', rows=[['wrong']], ready='I'),
                       dict(rows=[['actual']], ready=['I'], commands=[], errors=[]))
    except AssertionError:
        pass
    else:
        raise RuntimeError('Native oracle comparator accepted a wrong row')
    receipt = dict(schema=1, status='passed', role='native-oracle-only', core_pin=core_pin,
                   native_profile=prepared['profile'], cases=len(cases),
                   comparisons_per_case=['rows', 'ReadyForQuery transaction state', 'SQLSTATE', 'command tags'],
                   assertions=len(cases) * 4 + 1,
                   negative_control='wrong row rejected', native_cache=str(cache),
                   preparation_sha256=sha(cache / 'prepared.json'),
                   native_source_manifest_sha256=sha(cache / 'native-source-manifest.json'),
                   control=str(control), control_sha256=sha(control),
                   case_inventory_sha256=sha(cases_path), commands=commands, observations=observations)
    path = work / 'native-oracle.json'
    path.write_text(json.dumps(receipt, indent=2, ensure_ascii=False) + '\n')
    print(f"PASS {receipt['assertions']} native oracle assertions; {len(cases)} cases; report={path}")


if __name__ == '__main__':
    main()
