"""Bounded native lifecycle evidence using existing cached native libraries."""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    run_id = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
    output = (args.output or ROOT / 'artifacts/managed-native-lifecycle' / run_id).resolve()
    output.mkdir(parents=True, exist_ok=False)
    work = ROOT / 'build/managed-native-lifecycle' / run_id
    work.mkdir(parents=True)
    report = {'evidence_kind': 'native-lifecycle-control', 'managed_execution': False,
              'status': 'failed', 'commands': [], 'cycles': []}
    source = ROOT / 'build/native/s2n-source'
    backend = ROOT / 'build/native/openssl-legacy'
    paths = [ROOT / 'build/native/s2n-adapted/lib/libs2n.a', backend / 'libcrypto.a',
             ROOT / 'src/Boundary/dotcc_s2n_compat.c', ROOT / 'tests/Managed/native-lifecycle.c']
    try:
        for path in paths:
            if not path.is_file():
                raise RuntimeError('Cached native prerequisite missing; no library rebuild is attempted: ' + str(path))
        report['inputs_sha256'] = {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in paths}
        commands = [['cc', '-std=gnu99', '-I' + str(source), '-I' + str(source / 'api'),
                     '-I' + str(ROOT / 'src/Boundary'), '-I' + str(backend / 'include'),
                     str(paths[3]), str(paths[2]), str(paths[0]), str(paths[1]), '-lpthread', '-ldl',
                     '-o', str(work / 'control')]]
        commands += [[str(work / 'control'), mode] for mode in ('automatic-crypto', 'caller-crypto')]
        for i, command in enumerate(commands):
            result = subprocess.run(command, cwd=ROOT, env=dict(os.environ, S2N_DONT_MLOCK='1'),
                                    capture_output=True, text=True, timeout=30)
            log = output / f'{i:02d}.log'
            log.write_text(result.stdout + result.stderr)
            report['commands'].append({'command': command, 'exit_code': result.returncode, 'log': str(log)})
            if result.returncode:
                raise RuntimeError('Native lifecycle control failed: ' + str(log))
            if i:
                cycles = [json.loads(line) for line in result.stdout.splitlines() if line.startswith('{')]
                if len(cycles) != 3 or any(not r['cleanup_final'] or r['disable_crypto_after_init'] >= 0 for r in cycles):
                    raise RuntimeError('Missing exact cycle observations')
                report['cycles'].extend(cycles)
        report['status'] = 'passed'
        report['contract'] = {'global_restart_native': True, 'cycles_per_process': 3,
                              'crypto_ownership_change_after_first_init': False,
                              'cleanup_is_noop': True, 'cleanup_final_required': True}
    except Exception as error:
        report['error'] = str(error)
    (output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(output / 'report.json'))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
