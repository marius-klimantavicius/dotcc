"""Finite native-only HTTP oracle and startup timing for the matched service pair."""
import http.client
import json
import os
from pathlib import Path
import socket
import subprocess
import time

from evidence import cleanup, sha

CONTRACT = [('GET', '/health', '', 'CORECLR SERVICE OK\n'),
    ('POST', '/echo', 'payload', 'CORECLR:payload\n'),
    ('POST', '/stop', '', 'STOP OK\n')]


def measure(ev, closure_path, label):
    closure = json.loads(Path(closure_path).read_text())
    images = Path(closure['Images'])
    work = ev.directory / ('native-work-' + label)
    work.mkdir()
    temporary = work / 'tmp'
    temporary.mkdir()
    overrides = dict(closure['Environment'],
        DOTNET_ROOT=str(images / 'dotnet'), DOTNET_ROLL_FORWARD='Disable',
        DOTNET_MULTILEVEL_LOOKUP='0', TMPDIR=str(temporary),
        LD_LIBRARY_PATH=':'.join(str(images / p.lstrip('/'))
            for p in closure['Environment']['LD_LIBRARY_PATH'].split(':')))
    with socket.socket() as reserve:
        reserve.bind(('127.0.0.1', 0))
        port = reserve.getsockname()[1]
    command = list(map(str, [images / 'dotnet/dotnet', images / 'apps/service/CoreClrService.dll', port]))
    stdout, stderr = [ev.directory / (label + suffix) for suffix in ('.stdout', '.stderr')]
    ev.check()
    row = dict(label=label, command=command, cwd=str(work), timeout_seconds=45,
        environment_overrides=overrides, stdout=str(stdout), stderr=str(stderr),
        exit_code=None, responses=[], passed=False, timing_clock='time.monotonic_ns',
        timing_origin='immediately before subprocess.Popen; input checks excluded',
        readiness_poll_seconds=.001)
    ev.value['commands'].append(row)
    ev.save()
    process = None
    began = time.monotonic_ns()
    try:
        with stdout.open('wb') as out, stderr.open('wb') as err:
            process = subprocess.Popen(command, cwd=work, env=dict(ev.environment, **overrides),
                stdin=subprocess.DEVNULL, stdout=out, stderr=err, start_new_session=True)
        row['pid'] = process.pid
        deadline = time.monotonic() + 20
        while b'SERVICE READY\n' not in stdout.read_bytes():
            if process.poll() is not None:
                raise RuntimeError('Native service exited before readiness')
            if time.monotonic() >= deadline:
                raise TimeoutError('Native readiness exceeded 20 seconds')
            time.sleep(.001)
        ready = time.monotonic_ns()
        row['ready_seconds'] = (ready - began) / 1e9
        for method, path, body, expected in CONTRACT:
            connection = http.client.HTTPConnection('127.0.0.1', port, timeout=10)
            requested = time.monotonic_ns()
            try:
                connection.request(method, path, body=body, headers={'Connection': 'close'})
                response = connection.getresponse()
                headers_at = time.monotonic_ns()
                actual = response.read().decode()
                complete = time.monotonic_ns()
                row['responses'].append(dict(path=path, status=response.status, body=actual,
                    headers=response.getheaders(), request_seconds=(complete - requested) / 1e9,
                    headers_seconds=(headers_at - requested) / 1e9,
                    since_launch_seconds=(complete - began) / 1e9))
                if response.status != 200 or actual != expected:
                    raise RuntimeError('Native HTTP mismatch: ' + path)
            finally:
                connection.close()
        row['first_http_seconds'] = row['responses'][0]['since_launch_seconds']
        row['first_http_request_seconds'] = row['responses'][0]['request_seconds']
        remaining = 45 - (time.monotonic_ns() - began) / 1e9
        row['exit_code'] = process.wait(timeout=max(.1, remaining))
        if row['exit_code'] != 0 or stdout.read_bytes() != b'SERVICE READY\nSERVICE STOPPED\n' or stderr.read_bytes():
            raise RuntimeError('Native process output/exit differs')
        row['passed'] = True
    except BaseException as error:
        row['error'] = repr(error)
        raise
    finally:
        row['cleanup'] = cleanup(process) if process is not None else dict(group_gone=True, signals=[])
        row['seconds'] = (time.monotonic_ns() - began) / 1e9
        row['logs'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
        row['passed'] &= row['cleanup']['group_gone'] and not row['cleanup']['signals']
        ev.save()
    if not row['passed']:
        raise RuntimeError('Native process group required cleanup')
    ev.check()
    return {key: row[key] for key in ('label', 'pid', 'ready_seconds', 'first_http_seconds',
        'first_http_request_seconds', 'seconds', 'passed', 'responses', 'cleanup')}
