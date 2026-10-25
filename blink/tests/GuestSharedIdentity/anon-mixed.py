#!/usr/bin/env python3
"""Ordinary bounded Linux futex flag comparison, without changing any source."""
import ctypes
import errno
import json
from pathlib import Path
import tempfile
import threading
import time

libc = ctypes.CDLL(None, use_errno=True)
libc.syscall.restype = ctypes.c_long


class Timespec(ctypes.Structure):
    _fields_ = [('sec', ctypes.c_long), ('nsec', ctypes.c_long)]


def main():
    base = Path(__file__).resolve().parent / '.artifacts'
    base.mkdir(exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='anon-mixed-', dir=base))
    receipt = dict(passed=False, completed=False, cases=[],
        scope='Ordinary Linux syscall202 on anonymous ctypes word, no Blink qualification')
    try:
        for wait_op, wake_op in [(128, 1), (0, 129), (128, 129), (0, 1)]:
            word, timeout = ctypes.c_int(0), Timespec(1, 0)
            ready, result = threading.Event(), {}

            def wait():
                ready.set()
                ctypes.set_errno(0)
                rc = libc.syscall(202, ctypes.byref(word), wait_op, 0, ctypes.byref(timeout), 0, 0)
                result.update(result=rc, errno=ctypes.get_errno())

            thread = threading.Thread(target=wait)
            thread.start()
            if not ready.wait(1): raise RuntimeError('Wait worker did not start')
            time.sleep(.05)
            ctypes.set_errno(0)
            count = libc.syscall(202, ctypes.byref(word), wake_op, 1, 0, 0, 0)
            wake_error = ctypes.get_errno()
            thread.join(2)
            matching = wait_op == wake_op - 1
            receipt['cases'].append(dict(wait_op=wait_op, wake_op=wake_op, wake_result=count,
                wake_errno=wake_error, wait=result, thread_joined=not thread.is_alive()))
            if thread.is_alive() or count != int(matching): raise RuntimeError('Unexpected wake count')
            if result != dict(result=0 if matching else -1, errno=0 if matching else errno.ETIMEDOUT):
                raise RuntimeError('Unexpected wait completion')
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = repr(error)
    finally:
        receipt['completed'] = True
        target = attempt / 'receipt.json'
        target.write_text(json.dumps(receipt, indent=2) + '\n')
        print('receipt=' + str(target))
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
