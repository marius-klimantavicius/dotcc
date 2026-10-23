"""Exercise original and adapted process control on actual fragmented OS pipes."""
import argparse
from contextlib import redirect_stdout
import hashlib
import io
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'ref/s2n-tls/tests/integrationv2'))
from processes import _processCommunicator
import readiness


def execute(kind):
    if kind == 'kill':
        marker = b'a' * 2048 + b'b' * 2047
        writer = "import os,time;os.write(1,b'a'*2048);time.sleep(.1);os.write(1,b'b'*2047);time.sleep(3)"
        options = {'kill_marker': marker}
    elif kind == 'cross-channel':
        marker = b'a' * 2048 + b'b' * 2047
        writer = "import os,time;os.write(1,b'a'*2048);time.sleep(.1);os.write(2,b'b'*2047);time.sleep(3)"
        options = {'kill_marker': marker}
    else:
        marker = bytes(range(128, 138))
        writer = "import os,time,sys;os.write(1,b'READY\\n');time.sleep(.05);os.write(1,bytes(range(128,133)));time.sleep(.1);os.write(1,bytes(range(133,138)));sys.stdin.buffer.read()"
        options = {'send_marker_list': ['READY'], 'close_marker': str(marker)}
    command = [sys.executable, '-c', writer]
    process = subprocess.Popen(command, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    communicator = _processCommunicator(process, 'fragmented-' + kind)
    transcript = io.StringIO()
    result = {'kind': kind, 'command': command, 'timed_out': False}
    try:
        with redirect_stdout(transcript):
            stdout, stderr = communicator.communicate(timeout=.75, **options)
        result.update(stdout_hex=stdout.hex(), stderr_hex=stderr.hex(), exit_code=process.returncode)
    except subprocess.TimeoutExpired:
        result['timed_out'] = True
    finally:
        if process.poll() is None:
            process.kill()
        process.wait(timeout=3)
        for stream in (process.stdin, process.stdout, process.stderr):
            stream.close()
    result['transcript'] = transcript.getvalue()
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = {'evidence_kind': 'actual-os-pipe-marker-contract', 'status': 'failed',
              'original': [execute('kill'), execute('close')]}
    readiness.install(report)
    report['adapted'] = [execute('kill'), execute('close'), execute('cross-channel')]
    original_timeout = all(row['timed_out'] for row in report['original'])
    adapted = report['adapted']
    if not (original_timeout and not adapted[0]['timed_out'] and adapted[0]['exit_code'] != 0
            and not adapted[1]['timed_out'] and adapted[1]['exit_code'] == 0 and adapted[2]['timed_out']
            and {row['kind'] for row in report['control_markers'] if row['spanning_fragments']} >= {'kill', 'close'}):
        raise RuntimeError('Fragmented marker transport contract failed')
    report.update(status='passed', source_sha256={str(path): hashlib.sha256(path.read_bytes()).hexdigest()
        for path in (Path(__file__).resolve(), Path(readiness.__file__), ROOT / 'ref/s2n-tls/tests/integrationv2/processes.py')})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(args.output))


if __name__ == '__main__':
    main()
