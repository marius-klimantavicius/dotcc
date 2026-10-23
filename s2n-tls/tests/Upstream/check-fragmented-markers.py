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


def marker_case(style, kind, layout):
    binary = bytes(range(128, 138))
    choices = {
        'full-repr': (binary, repr(binary), True),
        'stripped-repr': (binary, repr(binary)[2:-1], True),
        'plain-text': (b'READY_MARKER', 'READY_MARKER', True),
        'bytes': (b'RAW_MARKER\x80', b'RAW_MARKER\x80', True),
        'escaped-backslashes': (b'path\\literal\\x80', repr(b'path\\literal\\x80')[2:-1], True),
        'literal-backslashes-negative': (b'path\\literal\\x80', 'path\\literal\\x80', False),
        'plain-quotes': (b'tag"with"quotes', 'tag"with"quotes', True),
        'noncanonical-literal-negative': (b'plain', 'b"plain"', False),
        'unicode-escape-negative': (b'namea', r'name\N{LATIN SMALL LETTER A}', False),
    }
    payload, marker, positive = choices[style]
    if isinstance(marker, str):
        assert (marker in repr(payload)) is positive
    writes = [(1, payload)]
    if layout == 'split':
        a, b = len(payload) // 3, 2 * len(payload) // 3
        writes = [(1, payload[:a]), (1, payload[a:b]), (1, payload[b:])]
    elif layout == 'cross-channel':
        middle = len(payload) // 2
        writes = [(1, payload[:middle]), (2, payload[middle:])]
    writer = 'import os,sys,time;'
    if kind == 'close':
        # A real stdin handshake guarantees ARMED and the payload cannot
        # coalesce into one read, even when the host is busy compiling.
        writer += "os.write(1,b'ARMED');assert sys.stdin.buffer.read(1)==b'G';"
    writer += ';'.join('os.write(' + str(fd) + ',' + repr(data) + ');time.sleep(.035)' for fd, data in writes) + ';'
    writer += ("assert sys.stdin.buffer.read(3)==b'ACK'" if kind == 'send' else 'sys.stdin.buffer.read()')
    command = [sys.executable, '-c', writer]
    process = subprocess.Popen(command, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    communicator = _processCommunicator(process, style + '-' + kind + '-' + layout)
    options = ({'input_data': [b'ACK'], 'send_marker_list': [marker]} if kind == 'send' else
               {'input_data': [b'G'], 'send_marker_list': ['ARMED'], 'close_marker': marker})
    result = {'style': style, 'kind': kind, 'layout': layout, 'command': command,
              'marker_text': marker if isinstance(marker, str) else None,
              'marker_bytes_hex': marker.hex() if isinstance(marker, bytes) else None,
              'payload_hex': payload.hex(), 'expected_match': positive and layout != 'cross-channel',
              'timed_out': False}
    transcript = io.StringIO()
    try:
        with redirect_stdout(transcript):
            stdout, stderr = communicator.communicate(timeout=.75, **options)
        result.update(stdout_hex=stdout.hex(), stderr_hex=stderr.hex(), exit_code=process.returncode)
    except subprocess.TimeoutExpired as error:
        result.update(timed_out=True, stdout_hex=(error.output or b'').hex(), stderr_hex=(error.stderr or b'').hex())
    finally:
        if process.poll() is None:
            process.kill()
        process.wait(timeout=3)
        for stream in (process.stdin, process.stdout, process.stderr):
            if not stream.closed:
                stream.close()
    result['transcript'] = transcript.getvalue()
    assert result['timed_out'] is not result['expected_match'], result
    if result['expected_match']:
        assert result['exit_code'] == 0, result
    return result


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
    reference_styles = ('full-repr', 'stripped-repr', 'plain-text', 'escaped-backslashes',
                        'literal-backslashes-negative', 'plain-quotes',
                        'noncanonical-literal-negative', 'unicode-escape-negative')
    report['original_single_fragment_semantics'] = [marker_case(style, kind, 'single')
        for style in reference_styles for kind in ('send', 'close')]
    readiness.install(report)
    report['adapted'] = [execute('kill'), execute('close'), execute('cross-channel')]
    report['adapted_marker_cases'] = [marker_case(style, kind, layout)
        for style in ('full-repr', 'stripped-repr', 'plain-text', 'bytes')
        for kind in ('send', 'close') for layout in ('single', 'split', 'cross-channel')]
    report['adapted_literal_semantics'] = [marker_case(style, kind, 'single')
        for style in reference_styles[3:] for kind in ('send', 'close')]
    original_timeout = all(row['timed_out'] for row in report['original'])
    adapted = report['adapted']
    if not (original_timeout and not adapted[0]['timed_out'] and adapted[0]['exit_code'] != 0
            and not adapted[1]['timed_out'] and adapted[1]['exit_code'] == 0 and adapted[2]['timed_out']
            and {row['kind'] for row in report['control_markers'] if row['spanning_fragments']} >= {'kill', 'close'}):
        raise RuntimeError('Fragmented marker transport contract failed')
    report.update(status='passed', marker_contract_cases=len(report['original_single_fragment_semantics'])
        + len(report['adapted_marker_cases']) + len(report['adapted_literal_semantics']),
        source_sha256={str(path): hashlib.sha256(path.read_bytes()).hexdigest()
        for path in (Path(__file__).resolve(), Path(readiness.__file__), ROOT / 'ref/s2n-tls/tests/integrationv2/processes.py')})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(args.output))


if __name__ == '__main__':
    main()
