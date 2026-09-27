"""Recognize strace split calls without weakening required thread observations."""
import importlib.util
from pathlib import Path
import tempfile
import unittest

SPEC = importlib.util.spec_from_file_location('guest_threads_runner',
    Path(__file__).resolve().parents[1] / 'tests/GuestThreads/run.py')
RUNNER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(RUNNER)

TRACE = '''10 clone(child_stack=0x1000, flags=CLONE_VM|CLONE_FS|CLONE_FILES|CLONE_SIGHAND|CLONE_THREAD|CLONE_SYSVSEM|CLONE_SETTLS|CLONE_PARENT_SETTID|CLONE_CHILD_CLEARTID|0x400000) = 11
10 futex(0x2000, FUTEX_WAIT_PRIVATE, 0, NULL) = 0
11 futex(0x2000, FUTEX_WAKE_PRIVATE, 1) = 1
10 futex(0x3000, FUTEX_WAIT, 11, NULL) = 0
11 exit(0 <unfinished ...>
11 <... exit resumed>) = ?
10 exit_group(0) = ?
'''


class GuestThreadsTraceTests(unittest.TestCase):
    def inspect(self, content):
        with tempfile.TemporaryDirectory() as temporary:
            path = Path(temporary) / 'trace'
            path.write_text(content)
            return RUNNER.trace_info(path, require_guest=True)

    def test_split_exit_preserves_required_observations(self):
        result = self.inspect(TRACE)
        self.assertEqual(result['tids'], [10, 11])
        self.assertEqual(result['unfinished_at_end'], {})

    def test_unsplit_exit_preserves_required_observations(self):
        self.inspect(TRACE.replace('exit(0 <unfinished ...>\n11 <... exit resumed>)', 'exit(0)'))

    def test_nonzero_child_exit_still_fails(self):
        with self.assertRaisesRegex(RuntimeError, 'coverage is incomplete'):
            self.inspect(TRACE.replace('exit(0 ', 'exit(1 '))

    def test_missing_child_exit_still_fails(self):
        with self.assertRaisesRegex(RuntimeError, 'coverage is incomplete'):
            self.inspect(TRACE.replace('11 exit(0 <unfinished ...>\n11 <... exit resumed>) = ?\n', ''))

    def test_missing_shared_futex_wait_still_fails(self):
        with self.assertRaisesRegex(RuntimeError, 'coverage is incomplete'):
            self.inspect(TRACE.replace('FUTEX_WAIT,', 'FUTEX_WAIT_PRIVATE,'))


if __name__ == '__main__':
    unittest.main()
