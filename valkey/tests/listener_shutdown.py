"""Native differential for the embedded select-backed listener shutdown repair."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--cc', default='cc')
    args = parser.parse_args()
    source, output = args.source.resolve(), args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    original = (source / 'src/server.c').read_text()
    begin = original.index('void closeListeningSockets(int unlink_unix_socket) {')
    end = original.index('\n}\n', begin) + 3
    body = original[begin:end]
    event_source = (source / 'src/ae.c').read_text()
    begin = event_source.index('void aeDeleteFileEvent(aeEventLoop *eventLoop, int fd, int mask) {')
    end = event_source.index('\n}\n', begin) + 3
    event_body = event_source[begin:end]
    adaptations = json.loads((ROOT / 'config/managed-adaptations.json').read_text())
    replacements = next(row['replacements'] for row in adaptations['adaptations'] if row['path'] == 'src/server.c')
    amended = body
    for row in replacements:
        if row['before'] in amended:
            if amended.count(row['before']) != 1:
                raise RuntimeError('Ambiguous listener shutdown guard')
            amended = amended.replace(row['before'], row['after'], 1)
    if amended == body:
        raise RuntimeError('Listener shutdown adaptation is absent')
    report = {'status': 'running', 'backend': 'actual upstream ae_select.c',
              'scope': 'Exact upstream listener close body and select backend; deterministically poll after listener closure as an offloaded worker can do.',
              'inputs': {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in
                         (source / 'src/server.c', source / 'src/ae.c', source / 'src/ae_select.c', source / 'src/ae.h',
                          ROOT / 'tests/listener_shutdown.c', ROOT / 'config/managed-adaptations.json')}, 'cases': []}
    try:
        for name, text, expected in [('original', body, 42), ('adapted', amended, 0)]:
            work = output / name; work.mkdir()
            (work / 'listener-close.inc').write_text(text)
            (work / 'listener-events.inc').write_text(event_body)
            binary = work / 'listener-shutdown'
            command = [args.cc, '-std=gnu11', '-O2', '-I', str(work), '-I', str(source / 'src'),
                       str(ROOT / 'tests/listener_shutdown.c'), '-o', str(binary)]
            built = subprocess.run(command, capture_output=True, text=True, timeout=60)
            (work / 'build.log').write_text(built.stdout + built.stderr)
            if built.returncode: raise RuntimeError(f'{name}: native reduction build failed')
            result = subprocess.run([str(binary)], capture_output=True, text=True, timeout=10)
            (work / 'stdout.log').write_text(result.stdout); (work / 'stderr.log').write_text(result.stderr)
            ok = result.returncode == expected and (name != 'original' or 'Bad file descriptor' in result.stderr)
            report['cases'].append({'name': name, 'command': command, 'expected_exit': expected,
                                    'actual_exit': result.returncode, 'status': 'passed' if ok else 'failed'})
            if not ok: raise RuntimeError(f'{name}: unexpected native reduction result')
        report['status'] = 'passed'
    finally:
        (output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print(output / 'report.json')


if __name__ == '__main__':
    main()
