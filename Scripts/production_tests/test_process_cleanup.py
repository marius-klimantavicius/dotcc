"""Legacy adapter cleanup and subprocess compatibility with tiny fake tools."""
import json
import os
from pathlib import Path
import signal
import subprocess
import sys
import tempfile
import time
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPTS))
import production_support as support


@unittest.skipUnless(sys.platform == 'linux', 'Linux private flock/process cleanup fixture')
class ProductionProcessCleanup(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='p2-process-cleanup-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.shared, self.native = self.root / 'shared.lock', self.root / 'native.lock'
        original = support.locked_command
        boundary = patch.object(support, 'locked_command', side_effect=lambda argv, **kwargs:
            original(argv, shared_lock=self.shared, native_lock=self.native, **kwargs))
        boundary.start()
        self.addCleanup(boundary.stop)

    def fake(self, name, code):
        path = self.root / name
        path.write_text('#!' + sys.executable + '\n' + code)
        path.chmod(0o755)
        return path

    def assert_locks_free(self):
        for path in (self.shared, self.native):
            result = subprocess.run(['flock', '-n', '--close', path, 'true'], timeout=3)
            self.assertEqual(result.returncode, 0)

    def cleanup_case(self, mode):
        executable = self.fake('cc', 'import json,os,subprocess,sys,time\nfrom pathlib import Path\n'
            'child=subprocess.Popen([sys.executable,"-c","import time; time.sleep(30)"],start_new_session=True)\n'
            'Path(os.environ["P2_OWNED"]).write_text(json.dumps({"direct":os.getpid(),"detached":child.pid,"inner_wrapper":os.getppid()}))\n'
            'print("stdout-ready",flush=True)\nprint("stderr-ready",file=sys.stderr,flush=True)\ntime.sleep(30)\n')
        unrelated = subprocess.Popen([sys.executable, '-c', 'import time; time.sleep(30)'], start_new_session=True)
        driver = subprocess.Popen([sys.executable, '-B', SCRIPTS / 'production_tests/cleanup_driver.py',
            mode, executable, self.root], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True,
            start_new_session=True)
        try:
            if mode == 'interrupt':
                deadline = time.monotonic() + 5
                while not (self.root / 'owned.json').exists() and time.monotonic() < deadline:
                    time.sleep(.01)
                self.assertTrue((self.root / 'owned.json').exists())
                os.kill(driver.pid, signal.SIGINT)
            stdout, stderr = driver.communicate(timeout=10)
            self.assertEqual(driver.returncode, 0, stderr)
            report = json.loads((self.root / 'result.json').read_text())
            print('P2 owned cleanup: ' + json.dumps(report), flush=True)
            self.assertIsNone(unrelated.poll(), 'Unrelated sibling process was killed')
            self.assertTrue(report['passed'], report)
            self.assertEqual(set(report['owned']), {'wrapper', 'inner_wrapper', 'direct', 'detached'})
            self.assertTrue(all(state is None for state in report['statesAfterAdapter'].values()))
            self.assertEqual(report['testCleanupNeeded'], [])
            self.assert_locks_free()
            return report
        finally:
            if driver.poll() is None:
                driver.kill()
            driver.communicate(timeout=3)
            unrelated.terminate()
            unrelated.wait(timeout=3)

    def test_timeout_drains_wrapper_direct_detached_and_releases_both_locks(self):
        report = self.cleanup_case('timeout')
        self.assertEqual(report['exception'], 'TimeoutExpired')
        self.assertEqual(report['timeout'], .4)
        self.assertEqual(report['stdout'], 'stdout-ready\n')
        self.assertEqual(report['stderr'], 'stderr-ready\n')

    def test_keyboard_interrupt_drains_owned_tree_and_preserves_unrelated_process(self):
        report = self.cleanup_case('interrupt')
        self.assertEqual(report['exception'], 'KeyboardInterrupt')

    def test_base_exception_cancellation_drains_owned_tree(self):
        report = self.cleanup_case('cancel')
        self.assertEqual(report['exception'], 'FixtureCancellation')

    def echo_tool(self):
        return self.fake('dotnet', 'import os,sys\n'
            'data=sys.stdin.buffer.read()\nsys.stdout.buffer.write(data+b"\\x00stdout")\n'
            'sys.stderr.buffer.write(b"stderr")\nraise SystemExit(int(os.environ.get("P2_EXIT","0")))\n')

    def test_bytes_input_capture_completed_process_and_effective_argv(self):
        command = [self.echo_tool(), 'build', 'file with spaces.csproj']
        result = support.subprocess.run(command, input=b'input\x00\xff', capture_output=True, check=True, timeout=3)
        self.assertIsInstance(result, subprocess.CompletedProcess)
        self.assertEqual((result.returncode, result.stdout, result.stderr), (0, b'input\x00\xff\x00stdout', b'stderr'))
        self.assertEqual(result.args[:4], ['flock', '-n', '--close', str(self.shared)])
        self.assertEqual(result.args.count('flock'), 1)
        self.assert_locks_free()

    def test_text_encoding_and_nonzero_check_compatibility(self):
        command = [self.echo_tool(), 'build', 'fixture.csproj']
        environment = dict(os.environ, P2_EXIT='37')
        result = support.subprocess.run(command, input='λ', capture_output=True, text=True,
            encoding='utf-8', errors='strict', env=environment, timeout=3)
        self.assertEqual((result.returncode, result.stdout, result.stderr), (37, 'λ\x00stdout', 'stderr'))
        with self.assertRaises(subprocess.CalledProcessError) as caught:
            support.subprocess.run(command, input='λ', capture_output=True, text=True,
                encoding='utf-8', errors='strict', env=environment, timeout=3, check=True)
        self.assertEqual((caught.exception.returncode, caught.exception.stdout, caught.exception.stderr),
                         (37, 'λ\x00stdout', 'stderr'))
        self.assertEqual(caught.exception.cmd, result.args)
        self.assert_locks_free()

    def test_env_cwd_and_file_output_are_preserved(self):
        tool = self.fake('dotnet', 'import json,os,sys\nprint(json.dumps([os.getcwd(),os.environ["P2_VALUE"]]))\n')
        with (self.root / 'output').open('w') as output:
            result = support.subprocess.run([tool, 'build', 'fixture.csproj'], cwd=self.root,
                env=dict(os.environ, P2_VALUE='literal $value λ'), stdout=output, text=True, timeout=3)
        self.assertIsNone(result.stdout)
        self.assertEqual(json.loads((self.root / 'output').read_text()), [str(self.root), 'literal $value λ'])

    def test_input_and_capture_conflicts_fail_before_process_start(self):
        tool = self.echo_tool()
        with self.assertRaises(ValueError):
            support.subprocess.run([tool], input=b'x', stdin=subprocess.PIPE)
        for stream in ('stdout', 'stderr'):
            with self.subTest(stream=stream), self.assertRaises(ValueError):
                support.subprocess.run([tool], capture_output=True, **{stream: subprocess.PIPE})

    def test_popen_runtime_peer_retains_inherited_session_behavior(self):
        with support.subprocess.Popen([sys.executable, '-c', 'import os; print(os.getpgrp())'],
                stdout=subprocess.PIPE, text=True) as peer:
            stdout, _ = peer.communicate(timeout=3)
        self.assertEqual(peer.returncode, 0)
        self.assertEqual(int(stdout), os.getpgrp())


if __name__ == '__main__':
    unittest.main()
