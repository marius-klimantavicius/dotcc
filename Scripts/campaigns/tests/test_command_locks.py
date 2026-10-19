"""Exercise the live command boundary using fake tools and private locks only."""
from concurrent.futures import ThreadPoolExecutor
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import threading
import time
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(SCRIPTS))
from campaigns.process import CommandError, execute
import production_support as support


@unittest.skipIf(os.name == "nt", "Linux flock execution; Windows selection covered separately")
class CommandLocks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="campaign-command-locks-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.shared, self.native = self.root / "shared.lock", self.root / "native.lock"
        self.env = dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS="production")
        for name in ("DOTCC_COMPILER", "DOTCC_POSTPROCESSOR", "DOTCC_OVERRIDE_COMPILER", "DOTCC_EXECUTABLE"):
            self.env.pop(name, None)
        self.original = support.locked_command
        boundary = patch.object(support, "locked_command", side_effect=lambda args, **kw:
            self.original(args, shared_lock=self.shared, native_lock=self.native, **kw))
        boundary.start()
        self.addCleanup(boundary.stop)
        self.tool = self.root / "dotnet"
        self.tool.write_text("#!" + sys.executable + "\n"
            "import json,os,sys,time,subprocess\nfrom pathlib import Path\n"
            "marker=Path(os.environ['MARKER'])\n"
            "marker.write_text(json.dumps({'argv':sys.argv[1:], 'fds':"
            "[os.readlink(p) for p in Path('/proc/self/fd').glob('*') if p.exists() and os.readlink(p).endswith('.lock')]}))\n"
            "if os.environ.get('SPAWN'):\n"
            " p=subprocess.Popen([sys.executable,'-c','import time; time.sleep(30)'],start_new_session=True)\n"
            " marker.with_suffix('.child').write_text(str(p.pid))\n"
            "while os.environ.get('HOLD') and not Path(os.environ['HOLD']).exists(): time.sleep(.01)\n"
            "time.sleep(float(os.environ.get('DELAY','0')))\n"
            "raise SystemExit(int(os.environ.get('EXIT','0')))\n")
        self.tool.chmod(0o755)

    def run_tool(self, label="control", args=None, **kw):
        env = dict(self.env, MARKER=str(self.root / (label + ".json")))
        env.update(kw.pop("env", {}))
        return execute([self.tool, *(args or ["dotcc.dll", "--emit=obj", "fixture.c"])],
            label=label, cwd=self.root, output=self.root / (label + ".log"), env=env, **kw)

    def test_effective_argv_exit_and_no_inherited_lock_fd(self):
        row = self.run_tool(args=["build", "fixture with spaces.csproj", "-m:99"])
        actual = json.loads((self.root / "control.json").read_text())
        self.assertEqual(row["command"][:5], ["flock", "-n", "--close", str(self.shared), str(self.tool)])
        self.assertEqual(row["command"][5:], actual["argv"])
        self.assertEqual(row["requested_command"][-1], "-m:99")
        self.assertNotIn("-m:99", actual["argv"])
        self.assertIn("-m:1", actual["argv"])
        self.assertEqual(actual["fds"], [])
        self.assertGreater(row["pid"], 0)

    def test_external_process_contention_never_executes_child(self):
        # A distinct process owns the private lock, rather than this Python thread.
        ready = self.root / "ready"
        owner = subprocess.Popen(["flock", "-n", "--close", self.shared, sys.executable, "-c",
            "import pathlib,sys; pathlib.Path(sys.argv[1]).touch(); sys.stdin.read()", str(ready)], stdin=subprocess.PIPE)
        self.addCleanup(lambda: owner.poll() is None and owner.kill())
        import time
        deadline = time.monotonic() + 3
        while not ready.exists() and time.monotonic() < deadline:
            time.sleep(.01)
        self.assertTrue(ready.exists())
        try:
            with self.assertRaises(CommandError) as caught:
                self.run_tool()
            self.assertEqual(caught.exception.record["exit_code"], 1)
            self.assertFalse((self.root / "control.json").exists())
        finally:
            owner.communicate(timeout=3)

    def test_two_sibling_jobs_serialize_and_both_run(self):
        start = threading.Barrier(2)
        def work(index):
            start.wait(timeout=3)
            return self.run_tool(str(index), env={"DELAY": ".12"})
        with ThreadPoolExecutor(max_workers=2) as pool:
            rows = list(pool.map(work, (0, 1)))
        self.assertTrue(all(row["status"] == "passed" for row in rows))
        self.assertTrue(all((self.root / (str(i) + ".json")).exists() for i in (0, 1)))

    def test_nonzero_and_separate_stderr_propagate(self):
        with self.assertRaises(CommandError) as caught:
            self.run_tool(env={"EXIT": "37"}, separate=True)
        self.assertEqual(caught.exception.record["exit_code"], 37)
        self.assertTrue(Path(caught.exception.record["stderr"]).is_file())

    def test_timeout_kills_owned_detached_child_and_releases_lock(self):
        row = self.run_tool(env={"DELAY": "30", "SPAWN": "1"}, timeout=.25, check=False)
        self.assertEqual((row["status"], row["exit_code"]), ("timed_out", 124))
        pid = int((self.root / "control.child").read_text())
        stat = Path(f"/proc/{pid}/stat")
        self.assertTrue(not stat.exists() or stat.read_text().rsplit(")", 1)[1].split()[0] == "Z")
        self.run_tool("after-timeout")

    def test_native_build_is_bounded_and_already_wrapped_is_unchanged(self):
        make = self.root / "make"
        make.write_bytes(self.tool.read_bytes()); make.chmod(0o755)
        argv = support.locked_command([make, "-j99", "target"], env=self.env)
        self.assertEqual(argv[:8], ["flock", "-n", "--close", str(self.shared),
                                  "flock", "-n", "--close", str(self.native)])
        row = execute(argv, label="native", cwd=self.root, output=self.root / "native.log",
                      env=dict(self.env, MARKER=str(self.root / "native.json")))
        self.assertEqual(row["command"], argv)
        self.assertEqual(json.loads((self.root / "native.json").read_text())["argv"], ["target", "-j2"])

    def test_orchestrator_and_runtime_receive_no_outer_lock(self):
        for argv in (["bash", "specialist.sh"], [sys.executable, "hook.py"], ["dotnet", "Peer.dll"]):
            self.assertEqual(support.locked_command(argv, env=self.env), argv)

    def test_custom_tool_and_restore_use_effective_child_environment(self):
        env = dict(self.env, DOTCC_COMPILER="/private/custom-driver.dll", DOTCC_CAMPAIGN_RESTORE="none")
        self.assertEqual(support.locked_command(["dotnet", "/snapshot/custom-driver.dll"], env=env)[:4],
                         ["flock", "-n", "--close", str(self.shared)])
        self.assertIn("--no-restore", support.locked_command(["dotnet", "build", "p.csproj"], env=env))

    def test_windows_selection_has_no_flock(self):
        from types import SimpleNamespace
        with patch.object(support, "os", SimpleNamespace(name="nt", environ=self.env)):
            argv = support.locked_command(["dotnet", "build", "p.csproj"], env=self.env)
        self.assertEqual(argv[0], "dotnet")
        self.assertNotIn("flock", argv)

    def test_opt_out_and_empty_environment_do_not_wrap(self):
        row = self.run_tool(env={"DOTCC_CAMPAIGN_COMMAND_LOCKS": ""})
        self.assertEqual(row["command"][0], str(self.tool))
        self.assertNotIn("requested_command", row)

    def test_cancelled_waiter_cannot_start_another_build(self):
        event = threading.Event(); event.set()
        record = {}
        with self.assertRaises(CommandError):
            self.run_tool(cancellation=event, record=record)
        self.assertEqual(record["status"], "cancelled")
        self.assertFalse((self.root / "control.json").exists())

    def test_missing_executable_is_recorded_without_success(self):
        self.tool.unlink()
        row = self.run_tool(check=False)
        self.assertEqual(row["status"], "failed")
        self.assertNotEqual(row["exit_code"], 0)

    def test_native_hook_state_lock_does_not_hold_build_locks(self):
        from types import SimpleNamespace
        spec = importlib.util.spec_from_file_location("a23_native", SCRIPTS.parent / "cpython/scripts/native.py")
        module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
        ctx = SimpleNamespace(root=self.root, env=self.env)
        with module.generator_lock(ctx):
            self.run_tool()

    def test_native_inventory_runs_under_state_lock_without_double_native_lock(self):
        from types import SimpleNamespace
        spec = importlib.util.spec_from_file_location("a23_inventory", SCRIPTS.parent / "cpython/scripts/inventory.py")
        module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
        (self.root / "build").mkdir()
        ctx = SimpleNamespace(root=SCRIPTS.parent / "cpython", work=self.root / "build", env=self.env)
        make = self.root / "make"
        make.write_text("#!" + sys.executable + "\n"
            "print('LIBRARY_OBJS=fixture.o')\nprint('LIBHACL_SHA2_OBJS=')\n"
            "print('FROZEN_FILES_IN=')\nprint('MODBUILT_NAMES=fixture')\nprint('MODDISABLED_NAMES=')\n")
        make.chmod(0o755); (self.root / "fixture.c").write_text("int fixture;\n")
        def run(command, label, timeout, **kwargs):
            command[0] = make
            row = execute(command, label=label, cwd=self.root, output=self.root / "inventory.log", env=self.env, timeout=timeout)
            self.assertEqual(row["command"][:8], ["flock", "-n", "--close", str(self.shared), "flock", "-n", "--close", str(self.native)])
            return (self.root / "inventory.log").read_text()
        ctx.run = run
        # Patch only the cache-state lock location; production build locks remain
        # the private locks used by the live command adapter above.
        from campaigns.recipes import helper
        native = helper(SCRIPTS.parent / "cpython/scripts/native.py")
        with patch.object(module, "helper", return_value=native), patch.object(native, "generator_lock", side_effect=lambda ctx:
                __import__('campaigns.delivery', fromlist=['lock']).lock(self.root / 'cache.lock')):
            result = module.collect(ctx, self.root, self.root)
        self.assertEqual(result["units"][0]["path"], "fixture.c")

    def private_args(self, index):
        run = self.root / 'fixture/build/campaign/20261018-000000-1234abcd'
        (run / 'objects').mkdir(parents=True, exist_ok=True)
        compiler = run / 'tools/DotCC/dotcc.dll'
        compiler.parent.mkdir(parents=True, exist_ok=True)
        compiler.touch()
        source = run / 'source/unit.c'
        source.parent.mkdir(parents=True, exist_ok=True)
        source.write_text('int unit;')
        return [compiler, '-std=c11', source, '--emit=obj', '-o', run / f'objects/{index:03d}-unit.cs']

    def wait_for(self, paths):
        deadline = time.monotonic() + 5
        while not all(p.exists() for p in paths) and time.monotonic() < deadline:
            time.sleep(.01)
        self.assertTrue(all(p.exists() for p in paths), str(paths))

    def test_four_private_emitters_overlap_and_exclude_external_mutation(self):
        release = self.root / 'release'
        with patch.object(support, 'REPO', self.root), ThreadPoolExecutor(max_workers=4) as pool:
            pending = [pool.submit(self.run_tool, f'emit{i}', self.private_args(i),
                       env={'HOLD': str(release)}) for i in range(4)]
            try:
                self.wait_for([self.root / f'emit{i}.json' for i in range(4)])
                # A separate process cannot mutate the shared build/tool tree.
                blocked = subprocess.run(self.original([self.tool, 'build', 'other.csproj'],
                    shared_lock=self.shared), env=dict(self.env, MARKER=str(self.root / 'external.json')))
                self.assertNotEqual(blocked.returncode, 0)
                self.assertFalse((self.root / 'external.json').exists())
            finally:
                release.touch()
            rows = [p.result(timeout=5) for p in pending]
        self.assertLess(max(r['started'] for r in rows), min(r['finished'] for r in rows))
        for i, row in enumerate(rows):
            self.assertEqual(row['command_lock_mode'], 'shared-object-emission')
            self.assertEqual(row['command'][:5], ['flock', '-s', '-n', '--close', str(self.shared)])
            actual = json.loads((self.root / f'emit{i}.json').read_text())
            self.assertEqual(actual['fds'], [])
            self.assertEqual(actual['argv'], row['requested_command'][1:])
        self.run_tool('mutation-after-readers')

    def test_private_duplicate_output_rejects_while_distinct_outputs_share(self):
        release = self.root / 'release'
        with patch.object(support, 'REPO', self.root), ThreadPoolExecutor(max_workers=1) as pool:
            args = self.private_args(0)
            first = pool.submit(self.run_tool, 'owner', args, env={'HOLD':str(release)})
            try:
                self.wait_for([self.root / 'owner.json'])
                with self.assertRaises(CommandError) as caught:
                    self.run_tool('collision', args)
                self.assertNotEqual(caught.exception.record['exit_code'], 0)
                self.assertFalse((self.root / 'collision.json').exists())
            finally:
                release.touch()
            first.result(timeout=5)

    def test_mixed_local_reader_writer_batches_wait_and_all_pass(self):
        for first_shared in (False, True):
            with self.subTest(first_shared=first_shared):
                release = self.root / f'release-{first_shared}'
                first_args, second_args = self.private_args(0), ['build', 'fixture.csproj']
                if not first_shared:
                    first_args, second_args = second_args, first_args
                with patch.object(support, 'REPO', self.root), ThreadPoolExecutor(max_workers=2) as pool:
                    first = pool.submit(self.run_tool, f'first{first_shared}', first_args, env={'HOLD':str(release)})
                    try:
                        self.wait_for([self.root / f'first{first_shared}.json'])
                        second = pool.submit(self.run_tool, f'second{first_shared}', second_args)
                        time.sleep(.08)
                        self.assertFalse((self.root / f'second{first_shared}.json').exists())
                    finally:
                        release.touch()
                    rows = [first.result(timeout=5), second.result(timeout=5)]
                self.assertTrue(all(r['status'] == 'passed' for r in rows))

    def test_parallel_cancellation_drains_detached_children_and_all_leases(self):
        cancelled = threading.Event()
        with patch.object(support, 'REPO', self.root), ThreadPoolExecutor(max_workers=2) as pool:
            pending = [pool.submit(self.run_tool, f'cancel{i}', self.private_args(i),
                env={'SPAWN':'1', 'DELAY':'30'}, cancellation=cancelled) for i in range(2)]
            self.wait_for([self.root / f'cancel{i}.child' for i in range(2)])
            cancelled.set()
            for future in pending:
                with self.assertRaises(CommandError) as caught:
                    future.result(timeout=6)
                self.assertEqual(caught.exception.record['status'], 'cancelled')
            for i in range(2):
                pid = int((self.root / f'cancel{i}.child').read_text())
                stat = Path(f'/proc/{pid}/stat')
                self.assertTrue(not stat.exists() or stat.read_text().rsplit(')',1)[1].split()[0] == 'Z')
                self.run_tool(f'after{i}', self.private_args(i))
        self.run_tool('exclusive-after-cancel')

    def test_private_nonzero_timeout_and_external_exclusive_are_not_masked(self):
        with patch.object(support, 'REPO', self.root):
            args = self.private_args(0)
            with self.assertRaises(CommandError) as caught:
                self.run_tool('failure', args, env={'EXIT':'37'})
            self.assertEqual(caught.exception.record['exit_code'], 37)
            with self.assertRaises(CommandError) as caught:
                self.run_tool('timeout', args, env={'DELAY':'30'}, timeout=.2)
            self.assertEqual(caught.exception.record['exit_code'], 124)
            import fcntl
            with self.shared.open('a+b') as owner:
                fcntl.flock(owner, fcntl.LOCK_EX | fcntl.LOCK_NB)
                with self.assertRaises(CommandError):
                    self.run_tool('blocked-reader', args)
            self.assertFalse((self.root / 'blocked-reader.json').exists())
            self.run_tool('after-failures', args)

    def test_classifier_rejects_other_tools_modes_sources_and_destinations(self):
        with patch.object(support, 'REPO', self.root):
            args = list(map(str, [self.tool, *self.private_args(0)]))
            self.assertIsNotNone(support.private_object_output(args, env=self.env))
            wrong_tool = Path(args[1]).with_name('dotcc-postprocess.dll'); wrong_tool.touch()
            cases = [[args[0], str(wrong_tool), *args[2:]], args + ['-c'], args + ['--shared'],
                     args + ['--emit=managedlib'], args + ['--emit=obj'], args + ['@response'],
                     args + ['-MD'], args + ['--output=shared'], args + [args[3]],
                     [*args[:3], str(self.root / 'missing.c'), *args[4:]],
                     [*args[:-1], str(self.root / 'shared.cs')],
                     args + ['--override-report', str(self.root / 'report.jsonl')],
                     [args[0], args[1], '-D', '--emit=obj', args[3], '-o', args[-1]]]
            for command in cases:
                with self.subTest(command=command):
                    self.assertIsNone(support.private_object_output(command, env=self.env))
                    self.assertNotEqual(support.locked_command(command, env=self.env)[:2], ['flock','-s'])
            destination = Path(args[-1]); destination.symlink_to(self.root / 'outside.cs')
            self.assertIsNone(support.private_object_output(args, env=self.env))

    def test_cpython_honors_all_supported_worker_requests(self):
        from campaigns.cli import options_for, parser
        from campaigns.recipes import load
        recipe = load(SCRIPTS.parent, 'cpython')
        for jobs in (1, 2, 4, 8, 16):
            options = options_for(parser().parse_args(['translate','--jobs',str(jobs)]), recipe)
            self.assertEqual(options.jobs, jobs)
        self.assertEqual(options_for(parser().parse_args(['translate']), recipe).jobs, 4)

    def test_publication_excludes_shared_emitters_and_releases_before_build_validation(self):
        import fcntl
        from campaigns.delivery import promote
        source, target = self.root / 'candidate', self.root / 'product'
        source.mkdir(); target.mkdir()
        (source/'new.cs').write_text('new')
        (target/'old.cs').write_text('old')
        with patch.dict(os.environ, self.env), patch.object(support, 'SHARED_LOCK', str(self.shared)):
            with self.shared.open('a+b') as reader:
                fcntl.flock(reader, fcntl.LOCK_SH | fcntl.LOCK_NB)
                with self.assertRaisesRegex(RuntimeError, 'Another campaign owns'):
                    promote([(source,target)], self.root/'promotion.json', lambda: self.fail('not reached'))
            self.assertEqual((source/'new.cs').read_text(), 'new')
            self.assertEqual((target/'old.cs').read_text(), 'old')
            def validate():
                self.run_tool('published-build', ['build','private.csproj'])
                self.assertEqual((target/'new.cs').read_text(), 'new')
            promote([(source,target)], self.root/'promotion.json', validate)
