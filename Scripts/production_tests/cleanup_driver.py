"""Disposable Linux fake-process fixture; never invokes a compiler or SDK."""
import ctypes
import json
import os
from pathlib import Path
import signal
import subprocess
import sys
import time

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import production_support as support

mode, executable, directory = sys.argv[1:]
root = Path(directory)
marker = root / 'owned.json'
# Adopt and reap this fixture's orphaned children after the real cleanup kills
# their parents. This is test-only and confined to this disposable driver.
if ctypes.CDLL(None, use_errno=True).prctl(36, 1, 0, 0, 0) != 0:
    raise OSError(ctypes.get_errno(), 'PR_SET_CHILD_SUBREAPER failed')
original_boundary = support.locked_command
support.locked_command = lambda argv, **kwargs: original_boundary(
    argv, shared_lock=root / 'shared.lock', native_lock=root / 'native.lock', **kwargs)
original_popen = support.subprocess.Popen
processes = []


class FixtureCancellation(BaseException):
    pass


def start(*args, **kwargs):
    process = original_popen(*args, **kwargs)
    processes.append(process)
    (root / 'wrapper.json').write_text(json.dumps({'pid': process.pid}))
    if mode == 'cancel':
        def cancel(*args, **kwargs):
            deadline = time.monotonic() + 3
            while not marker.exists() and time.monotonic() < deadline:
                time.sleep(.01)
            if not marker.exists():
                raise RuntimeError('Fake child did not start')
            raise FixtureCancellation('fixture cancellation')
        process.communicate = cancel
    return process


support.subprocess.Popen = start
report = dict(mode=mode, scope='private fake wrapper, direct child and detached child only')


def identities():
    result = json.loads(marker.read_text()) if marker.exists() else {}
    if processes:
        result['wrapper'] = processes[0].pid
    return result


def states(pids):
    result = {}
    for name, pid in pids.items():
        path = Path(f'/proc/{pid}/stat')
        result[name] = path.read_text().rsplit(')', 1)[1].split()[0] if path.exists() else None
    return result


def reap_until(pids, seconds):
    deadline = time.monotonic() + seconds
    while True:
        while True:
            try:
                child, _ = os.waitpid(-1, os.WNOHANG)
            except ChildProcessError:
                break
            if child == 0:
                break
        if all(state is None for state in states(pids).values()) or time.monotonic() >= deadline:
            break
        time.sleep(.01)


try:
    support.subprocess.run([executable, 'fixture.c', '-o', 'fixture'],
        env=dict(os.environ, P2_OWNED=str(marker)), capture_output=True, text=True,
        timeout=.4 if mode == 'timeout' else 5)
    report['exception'] = None
except subprocess.TimeoutExpired as error:
    report.update(exception=type(error).__name__, timeout=error.timeout,
                  stdout=error.stdout.decode() if isinstance(error.stdout, bytes) else error.stdout,
                  stderr=error.stderr.decode() if isinstance(error.stderr, bytes) else error.stderr)
except (KeyboardInterrupt, FixtureCancellation) as error:
    report['exception'] = type(error).__name__
finally:
    owned = identities()
    reap_until(owned, .3)
    report.update(owned=owned, statesAfterAdapter=states(owned))
    report['passed'] = bool(processes) and all(value is None for value in report['statesAfterAdapter'].values())
    # RED controls must also leave no fake processes running. Preserve the
    # pre-cleanup states, then terminate only the fixture's recorded PIDs.
    report['testCleanupNeeded'] = [name for name, state in report['statesAfterAdapter'].items() if state]
    for name in report['testCleanupNeeded']:
        try:
            os.kill(owned[name], signal.SIGKILL)
        except ProcessLookupError:
            pass
    reap_until(owned, 2)
    report['statesAfterTestCleanup'] = states(owned)
    (root / 'result.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report), flush=True)
