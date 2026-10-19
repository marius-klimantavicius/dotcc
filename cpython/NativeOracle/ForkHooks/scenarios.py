"""Run inside the actual interpreter; never imported by a collector host."""
import errno
import gc
import importlib
import os
import threading
import time
import weakref
import sys
import _imp
import _posixsubprocess
import fork_probe

assert hasattr(os, 'fork') and hasattr(os, 'register_at_fork')
assert fork_probe.state() == 0
events = []
callback_errors = []
sys.unraisablehook = lambda item: callback_errors.append(str(item.exc_value))
added_before = added_parent = False


def before_a():
    events.append('before-a')


def before_b():
    global added_before
    events.append('before-b')
    if not added_before:
        added_before = True
        os.register_at_fork(before=lambda: events.append('before-c'),
                            after_in_parent=lambda: events.append('parent-c'),
                            after_in_child=lambda: events.append('CHILD-c'))


def parent_a():
    global added_parent
    events.append('parent-a')
    assert not _imp.lock_held() and fork_probe.state() == 0
    # New thread-state creation and import locking exercise HEAD/import/world
    # cleanup before the parent callbacks run, while this owner releases GIL.
    done = []

    def work():
        _imp.acquire_lock()
        try:
            assert importlib.import_module('json').loads('[42]') == [42]
            done.append(42)
        finally:
            _imp.release_lock()

    worker = threading.Thread(target=work)
    worker.start()
    worker.join(5)
    assert not worker.is_alive() and done == [42]
    if not added_parent:
        added_parent = True
        os.register_at_fork(after_in_parent=lambda: events.append('parent-d'))
    # Deliberately clobber the low-level errno after the rejected syscall.
    try:
        os.close(-1)
    except OSError as error:
        assert error.errno == errno.EBADF
    else:
        raise AssertionError('invalid descriptor unexpectedly closed')


os.register_at_fork(before=before_a, after_in_parent=parent_a,
                    after_in_child=lambda: events.append('CHILD-a'))
os.register_at_fork(before=before_b,
                    after_in_parent=lambda: events.append('parent-b'),
                    after_in_child=lambda: events.append('CHILD-b'))

# These three callables are retained only by the three real interpreter lists.
# The C harness checks their release after each actual Py_FinalizeEx.
class Owner:
    def __init__(self):
        self.notify = fork_probe.released

    def __call__(self):
        pass

    def __del__(self):
        self.notify()


owners = [Owner(), Owner(), Owner()]
references = [weakref.ref(owner) for owner in owners]
os.register_at_fork(before=owners[0], after_in_parent=owners[1], after_in_child=owners[2])
del owners
gc.collect()
assert all(reference() is not None for reference in references)

def subprocess_reject():
    # No pipes/exec/child side are required to reach the actual failed fork path.
    # The preexec callable is never reached when shared fork returns -1.
    _posixsubprocess.fork_exec([b'/unexecuted'], [b'/unexecuted'], False, (),
                              None, None, -1, -1, -1, -1, -1, -1, -1, -1,
                              False, False, -1, None, None, None, -1,
                              lambda: events.append('PREEXEC-CHILD'), False)


progress = [0]
stop = threading.Event()


def progressing():
    while not stop.is_set():
        progress[0] += 1
        time.sleep(0.001)


running = threading.Thread(target=progressing)
running.start()
operations = [fork_probe.pair, os.fork, os.fork, os.fork,
              subprocess_reject, subprocess_reject]
for index, operation in enumerate(operations):
    events.clear()
    before = progress[0]
    try:
        operation()
    except OSError as error:
        assert error.errno == errno.EPERM, (index, error.errno)
    else:
        raise AssertionError('process creation unexpectedly succeeded')
    expected = (['before-b', 'before-a', 'parent-a', 'parent-b', 'parent-c']
                if index == 0 else
                ['before-c', 'before-b', 'before-a', 'parent-a', 'parent-b', 'parent-c', 'parent-d'])
    assert events == expected, (index, events)
    assert callback_errors == [], callback_errors
    assert fork_probe.state() == 0 and not _imp.lock_held()
    deadline = time.monotonic()+5
    while progress[0] <= before and time.monotonic() < deadline:
        time.sleep(0.001)
    assert progress[0] > before
    assert eval('6 * 7') == 42 and importlib.import_module('gc').collect() >= 0
stop.set()
running.join(5)
assert not running.is_alive()
print('fork-hooks-cycle operations=%d child-executions=0 snapshot-order=ok errno=EPERM cleanup=ok' % len(operations))
