"""Pure controls: fake executables, copied fixture tools and private lock files.

No compiler, managed SDK, native build, server or campaign suite is executed.
"""
import ast
import fcntl
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import time
from types import SimpleNamespace
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPTS))
import production_support as support


class ProductionControls(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="dotcc-production-controls-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.shared = self.root / "shared.lock"
        self.native = self.root / "native.lock"
        self.environment = patch.dict(os.environ, {}, clear=False)
        self.environment.start()
        self.addCleanup(self.environment.stop)
        for key in ("DOTCC_COMPILER", "DOTCC_POSTPROCESSOR", "DOTCC_PRODUCTION_TOOLS"):
            os.environ.pop(key, None)

    def tool(self, directory, name="dotcc.dll"):
        directory.mkdir(parents=True)
        tool = directory / name
        tool.write_bytes(b"fixture-selected-tool")
        (directory / "DotCC.Lib.dll").write_bytes(b"fixture-runtime-resource-container")
        tool.with_suffix(".runtimeconfig.json").write_text("{}")
        tool.with_suffix(".deps.json").write_text(json.dumps({"targets": {"fixture": {
            "fixture/1": {"runtime": {name: {}, "DotCC.Lib.dll": {}},
                          "resources": {"lib/net8.0/cs/messages.resources.dll": {"locale": "cs"}}}}}}))
        (directory / "cs").mkdir()
        (directory / "cs/messages.resources.dll").write_bytes(b"fixture-satellite")
        tool.with_suffix(".pdb").write_bytes(b"fixture-pdb")
        (directory / "nested").mkdir()
        (directory / "nested/source.cs").write_bytes(b"fixture-source-copy")
        return tool

    def fake(self, name):
        executable = self.root / name
        executable.write_text("#!" + sys.executable + "\nimport json,os,sys\n"
            "from pathlib import Path\n"
            "record={'arguments':sys.argv[1:],'lockFDs':[os.readlink(p) for p in Path('/proc/self/fd').glob('*') "
            "if p.exists() and os.readlink(p).endswith('.lock')]}\n"
            "Path(os.environ['CONTROL_MARKER']).write_text(json.dumps(record))\n")
        executable.chmod(0o755)
        return executable

    def launch(self, command):
        marker = self.root / "executed.json"
        result = subprocess.run(support.locked_command(command, shared_lock=self.shared, native_lock=self.native),
            env=dict(os.environ, CONTROL_MARKER=str(marker)), capture_output=True, text=True, timeout=5)
        return result, marker

    def framework_context(self):
        commands = []
        context = SimpleNamespace(options=SimpleNamespace(tools="reuse"), repo=self.root, session=set(),
            work=self.root / "work", receipt={}, save=lambda: None,
            run=lambda command, label: commands.append(command) or "fixture SDK",
            managed=lambda *args, **kwargs: self.fail("Existing production tools must not be rebuilt"),
            policy=SimpleNamespace(manifest=lambda paths, directory: {
                p.relative_to(directory).as_posix(): support.digest(p) for p in paths}))
        return context, commands

    def test_default_selects_existing_repository_tool(self):
        expected = self.tool(self.root / "DotCC/bin/Release/net10.0")
        self.assertEqual(support.selected_tool(repo=self.root), expected)

    def test_explicit_selection_wins_over_existing_root(self):
        self.tool(self.root / "DotCC/bin/Release/net10.0")
        expected = self.tool(self.root / "selected")
        self.assertEqual(support.selected_tool(repo=self.root, env={"DOTCC_COMPILER": str(expected)}), expected)

    def test_missing_explicit_tool_never_uses_root(self):
        self.tool(self.root / "DotCC/bin/Release/net10.0")
        with self.assertRaisesRegex(RuntimeError, "no root fallback"):
            support.selected_tool(repo=self.root, env={"DOTCC_COMPILER": str(self.root / "absent.dll")})

    def test_empty_override_is_an_error(self):
        with self.assertRaisesRegex(RuntimeError, "empty tool path"):
            support.selected_tool(env={"DOTCC_COMPILER": ""})

    def test_recursive_snapshot_copies_pdb_resource_source_and_retains_previous(self):
        selected = self.tool(self.root / "selected")
        destination = self.root / "frozen"
        destination.mkdir()
        (destination / "old-extra.dll").write_bytes(b"old-independent-bytes")
        compiler, binding = support.snapshot_tool(selected, destination)
        self.assertEqual(support.tool_manifest(destination), support.tool_manifest(selected.parent))
        self.assertEqual(compiler.read_bytes(), selected.read_bytes())
        self.assertNotEqual(compiler.stat().st_ino, selected.stat().st_ino)
        self.assertEqual((Path(binding["previous"]) / "old-extra.dll").read_bytes(), b"old-independent-bytes")
        self.assertFalse((destination / "old-extra.dll").exists())

    def test_missing_satellite_fails_before_snapshot_publication(self):
        tool = self.tool(self.root / "selected")
        (tool.parent / "cs/messages.resources.dll").unlink()
        with self.assertRaisesRegex(RuntimeError, "dependency is missing"):
            support.snapshot_tool(tool, self.root / "frozen")
        self.assertFalse((self.root / "frozen").exists())

    def test_pdb_and_nested_resource_drift_rejects_identity(self):
        tool = self.tool(self.root / "selected")
        expected = support.validate_tool_closure(tool)
        for name in ("dotcc.pdb", "cs/messages.resources.dll", "nested/source.cs"):
            path = tool.parent / name
            original = path.read_bytes()
            path.write_bytes(original + b"-drift")
            with self.assertRaisesRegex(RuntimeError, "closure differs"):
                support.require_tool_identity(tool, expected, "negative")
            path.write_bytes(original)

    def test_copy_drift_is_rejected_before_replacing_previous(self):
        tool = self.tool(self.root / "selected")
        destination = self.root / "frozen"
        destination.mkdir()
        (destination / "old").write_bytes(b"preserved")
        real_copy = support.shutil.copytree
        def mutate(source, target):
            real_copy(source, target)
            (Path(target) / "dotcc.pdb").write_bytes(b"wrong")
        with patch.object(support, "shutil", SimpleNamespace(copytree=mutate)):
            with self.assertRaisesRegex(RuntimeError, "changed while copying"):
                support.snapshot_tool(tool, destination)
        self.assertEqual((destination / "old").read_bytes(), b"preserved")

    def test_busy_shared_lock_prevents_fake_build_execution(self):
        executable = self.fake("dotnet")
        with self.shared.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            result, marker = self.launch([executable, "build", "fixture.csproj"])
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(marker.exists())

    def test_busy_native_lock_prevents_fake_compile_and_releases_shared(self):
        executable = self.fake("cc")
        with self.native.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            result, marker = self.launch([executable, "fixture.c", "-o", "fixture"])
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(marker.exists())
        with self.shared.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)

    def test_build_executes_without_inherited_lock_descriptors(self):
        result, marker = self.launch([self.fake("dotnet"), "publish", "fixture.csproj", "-m:16"])
        self.assertEqual(result.returncode, 0, result.stderr)
        row = json.loads(marker.read_text())
        self.assertEqual(row["lockFDs"], [])
        self.assertIn("-m:1", row["arguments"])
        self.assertNotIn("-m:16", row["arguments"])
        self.assertIn("--disable-build-servers", row["arguments"])

    def test_unlocked_runtime_can_run_while_build_lock_is_busy(self):
        with self.shared.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            result, marker = self.launch([self.fake("dotnet"), "Peer.dll", "server"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertTrue(marker.exists())

    def test_native_make_receives_only_two_jobs(self):
        for args in (("-j99",), ("-j", "99"), ("--jobs=99",), ("--jobs", "99")):
            result, marker = self.launch([self.fake("make"), *args, "target"])
            self.assertEqual(result.returncode, 0, result.stderr)
            self.assertEqual(json.loads(marker.read_text())["arguments"], ["target", "-j2"])

    def test_cmake_build_receives_two_jobs(self):
        result, marker = self.launch([self.fake("cmake"), "--build", "fixture", "--parallel", "99"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(json.loads(marker.read_text())["arguments"], ["--build", "fixture", "--parallel", "2"])

    def test_orchestrators_receive_no_outer_build_lock(self):
        for command in (("bash", "verify.sh"), (sys.executable, "suite.py"), ("docker", "run", "peer")):
            self.assertEqual(support.locked_command(command), list(command))

    def test_process_adapter_does_not_modify_standard_subprocess(self):
        original = subprocess.run
        with patch.object(support, "locked_command", side_effect=lambda args: list(map(str, args))):
            result = support.subprocess.run([sys.executable, "-c", "print('adapter')"], capture_output=True, text=True)
        self.assertEqual(result.stdout, "adapter\n")
        self.assertIs(subprocess.run, original)

    def test_live_host_hook_rejects_missing_override_before_work(self):
        hook = SCRIPTS.parent / "msquic/scripts/test-host-contract.py"
        result = subprocess.run([sys.executable, "-B", hook, "--no-fetch"],
            env=dict(os.environ, DOTCC_COMPILER=str(self.root / "missing.dll")),
            capture_output=True, text=True, timeout=5)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("no root fallback", result.stderr)

    def test_shell_preflight_rejects_missing_override_before_child(self):
        marker = self.root / "child-ran"
        helper = SCRIPTS / "production-common.sh"
        result = subprocess.run(["bash", "-c", 'source "$1"; touch "$2"', "control", helper, marker],
            env=dict(os.environ, DOTCC_POSTPROCESSOR=str(self.root / "missing.dll")),
            capture_output=True, text=True, timeout=5)
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(marker.exists())

    def test_live_abi_hook_rejects_missing_cli_override_before_work(self):
        hook = SCRIPTS.parent / "msquic/scripts/test-abi.py"
        result = subprocess.run([sys.executable, "-B", hook, "--compiler", self.root / "missing.dll"],
            capture_output=True, text=True, timeout=5)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("no root fallback", result.stderr)

    def test_live_host_symbol_audit_rejects_missing_override_before_work(self):
        import builtins
        from types import ModuleType
        repo = self.root / "isolated-repo"
        hook = repo / "msquic/scripts/audit-host-symbols.py"
        hook.parent.mkdir(parents=True)
        hook.write_bytes((SCRIPTS.parent / "msquic/scripts/audit-host-symbols.py").read_bytes())
        scripts = repo / "Scripts"
        scripts.mkdir()
        helper = scripts / "production_support.py"
        helper.write_bytes((SCRIPTS / "production_support.py").read_bytes())
        stage = repo / "msquic/build/product-source"
        stage.mkdir(parents=True)
        (stage / "manifest.json").write_text('{"units":["fixture.c"],"defines":[]}')
        (stage / "fixture.c").write_text('int fixture;\n')
        # Even an old hook, an absolute command, or a newly added command can
        # never reach the host launcher. Every subprocess import is local/fake.
        external_calls = []
        def forbidden(*args, **kwargs):
            external_calls.append((args, kwargs))
            raise AssertionError("External execution forbidden in preflight fixture")
        external = ModuleType("subprocess")
        for name in ("run", "Popen", "call", "check_call", "check_output", "getoutput", "getstatusoutput"):
            setattr(external, name, forbidden)
        for name in ("PIPE", "STDOUT", "DEVNULL"):
            setattr(external, name, getattr(subprocess, name))
        for name in ("gcc", "nm"):
            fake = self.fake(name)
            fake.write_text(fake.read_text() + "\nraise SystemExit(97)\n")
        def fixture_import(name, globals=None, locals=None, fromlist=(), level=0):
            if name == "subprocess":
                return external
            if name == "production_support":
                module = ModuleType("isolated_production_support")
                module.__dict__.update(__file__=str(helper), __builtins__=fixture_builtins)
                exec(compile(helper.read_bytes(), str(helper), "exec"), module.__dict__)
                return module
            return builtins.__import__(name, globals, locals, fromlist, level)
        fixture_builtins = dict(vars(builtins), __import__=fixture_import)
        namespace = dict(__file__=str(hook), __name__="isolated_host_symbol_audit", __builtins__=fixture_builtins)
        with patch.dict(os.environ, DOTCC_COMPILER=str(self.root / "missing.dll")), \
                patch.object(sys, "path", list(sys.path)):
            with self.assertRaisesRegex(RuntimeError, "no root fallback"):
                exec(compile(hook.read_bytes(), str(hook), "exec"), namespace)
        self.assertEqual(external_calls, [], "Preflight must reject before any launcher or adapter execution")
        self.assertFalse((repo / "msquic/artifacts").exists())

    def test_live_framework_tools_selects_and_copies_complete_closures(self):
        from campaigns.runner import Context
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        context, commands = self.framework_context()
        with patch.dict(os.environ, DOTCC_COMPILER=str(compiler), DOTCC_POSTPROCESSOR=str(postprocessor),
                        DOTCC_PRODUCTION_TOOLS="existing"):
            Context.tools(context)
        self.assertEqual(commands, [["dotnet", "--version"]])
        self.assertEqual(support.tool_manifest(context.compiler.parent), support.tool_manifest(compiler.parent))
        self.assertEqual(support.tool_manifest(context.postprocessor.parent), support.tool_manifest(postprocessor.parent))
        self.assertIn("dotcc.pdb", context.receipt["tools"]["DotCC"])
        self.assertIn("cs/messages.resources.dll", context.receipt["tools"]["DotCC"])

    def test_production_existing_selection_never_rebuilds_under_build_option(self):
        from campaigns.runner import Context
        context, commands = self.framework_context()
        context.options.tools = "build"
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        with patch.dict(os.environ, DOTCC_PRODUCTION_TOOLS="existing", DOTCC_COMPILER=str(compiler),
                        DOTCC_POSTPROCESSOR=str(postprocessor)):
            Context.tools(context)
        self.assertEqual(commands, [["dotnet", "--version"]])
        self.assertEqual(context.session, set())
        for project, selected in (("DotCC", compiler), ("DotCC.PostProcess", postprocessor)):
            support.require_tool_identity(selected, context.receipt["tools"][project], "live snapshot")
            self.assertEqual(context.receipt["tool_snapshots"][project]["selected"], str(selected))

    def test_production_missing_or_empty_override_fails_before_sdk_or_snapshot(self):
        from campaigns.runner import Context
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        for variable in ("DOTCC_COMPILER", "DOTCC_POSTPROCESSOR"):
            for value in ("", " ", str(self.root / "missing.dll")):
                with self.subTest(variable=variable, value=value):
                    context, commands = self.framework_context()
                    environment = dict(DOTCC_PRODUCTION_TOOLS="existing", DOTCC_COMPILER=str(compiler),
                                       DOTCC_POSTPROCESSOR=str(postprocessor))
                    environment[variable] = value
                    with patch.dict(os.environ, environment), self.assertRaises(RuntimeError):
                        Context.tools(context)
                    self.assertEqual(commands, [])
                    self.assertFalse(context.work.exists())

    def test_production_missing_dependency_fails_before_sdk_or_snapshot(self):
        from campaigns.runner import Context
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        (postprocessor.parent / "cs/messages.resources.dll").unlink()
        context, commands = self.framework_context()
        with patch.dict(os.environ, DOTCC_PRODUCTION_TOOLS="existing", DOTCC_COMPILER=str(compiler),
                        DOTCC_POSTPROCESSOR=str(postprocessor)):
            with self.assertRaisesRegex(RuntimeError, "dependency is missing"):
                Context.tools(context)
        self.assertEqual(commands, [])
        self.assertFalse(context.work.exists())

    def assert_production_requires_both_environment_keys(self, root_tools):
        from campaigns.runner import Context
        if root_tools:
            self.tool(self.root / "DotCC/bin/Release/net10.0")
            self.tool(self.root / "DotCC.PostProcess/bin/Release/net10.0", "dotcc-postprocess.dll")
        for missing in (("DOTCC_COMPILER",), ("DOTCC_POSTPROCESSOR",),
                        ("DOTCC_COMPILER", "DOTCC_POSTPROCESSOR")):
            with self.subTest(missing=missing, root_tools=root_tools):
                context, commands = self.framework_context()
                environment = dict(DOTCC_PRODUCTION_TOOLS="existing", DOTCC_COMPILER="explicit-compiler.dll",
                                   DOTCC_POSTPROCESSOR="explicit-postprocessor.dll")
                for key in missing:
                    environment.pop(key)
                with patch.dict(os.environ, environment, clear=True), \
                        patch.object(support, "selected_tool") as selected, \
                        patch.object(support, "validate_tool_closure") as validate, \
                        patch.object(support, "snapshot_tool") as snapshot:
                    with self.assertRaisesRegex(RuntimeError, "require explicit environment keys"):
                        Context.tools(context)
                    selected.assert_not_called()
                    validate.assert_not_called()
                    snapshot.assert_not_called()
                self.assertEqual(commands, [])
                self.assertEqual(context.receipt, {})
                self.assertFalse(context.work.exists())

    def test_production_absent_environment_keys_reject_before_selection(self):
        self.assert_production_requires_both_environment_keys(root_tools=False)

    def test_production_absent_keys_never_fall_back_to_present_root_tools(self):
        self.assert_production_requires_both_environment_keys(root_tools=True)

    def test_production_drift_during_sdk_call_is_rejected(self):
        from campaigns.runner import Context
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        context, _ = self.framework_context()
        def sdk(command, label):
            compiler.with_suffix(".pdb").write_bytes(b"changed after preflight")
            return "fixture SDK"
        context.run = sdk
        with patch.dict(os.environ, DOTCC_PRODUCTION_TOOLS="existing", DOTCC_COMPILER=str(compiler),
                        DOTCC_POSTPROCESSOR=str(postprocessor)):
            with self.assertRaisesRegex(RuntimeError, "changed before snapshot"):
                Context.tools(context)
        self.assertNotIn("tools", context.receipt)

    def test_command_locks_alone_retain_default_tool_snapshot_behavior(self):
        from campaigns.runner import Context
        compiler = self.tool(self.root / "selected-compiler")
        postprocessor = self.tool(self.root / "selected-postprocessor", "dotcc-postprocess.dll")
        context, _ = self.framework_context()
        with patch.dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS="production", DOTCC_COMPILER=str(compiler),
                        DOTCC_POSTPROCESSOR=str(postprocessor)):
            Context.tools(context)
        self.assertNotIn("dotcc.pdb", context.receipt["tools"]["DotCC"])
        self.assertNotIn("tool_snapshots", context.receipt)

    def test_live_framework_process_lock_blocks_execution(self):
        from campaigns import process as module
        marker = self.root / "never-executed"
        env = dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS="production", CONTROL_MARKER=str(marker))
        original = support.locked_command
        with self.shared.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            with patch.object(support, "locked_command", side_effect=lambda argv, **kwargs:
                    original(argv, shared_lock=self.shared, native_lock=self.native, **kwargs)):
                result = module.execute([self.fake("dotnet"), "build", "fixture.csproj"], label="control",
                    cwd=self.root, output=self.root / "process.log", env=env, check=False, timeout=5)
        self.assertEqual(result["status"], "failed")
        self.assertFalse(marker.exists())

    def test_stale_sqlite_product_stops_before_any_fake_tool_or_regeneration(self):
        import shutil
        repo = self.root / "repo"
        scripts = repo / "Scripts"
        scripts.mkdir(parents=True)
        for name in ("campaign-common.sh", "production-common.sh", "production_support.py"):
            shutil.copyfile(SCRIPTS / name, scripts / name)
        sqlite = repo / "sqlite"
        (sqlite / "scripts").mkdir(parents=True)
        (sqlite / "config").mkdir()
        for name in ("test-product-layout.sh", "legacy-common.sh", "product_identity.py"):
            shutil.copyfile(SCRIPTS.parent / "sqlite/scripts" / name, sqlite / "scripts" / name)
        (sqlite / "config/sources.json").write_text('{"product":{"directory":"fixture-source"}}')
        for name in ("defines.txt", "native-flags.txt"):
            (sqlite / "config" / name).write_text("")
        stale = self.root / "stale-receipt.json"
        stale.write_text('{}')
        tool = self.tool(self.root / "selected")
        marker = self.root / "never-executed"
        self.fake("dotnet")
        self.fake("gcc")
        result = subprocess.run(["bash", sqlite / "scripts/test-product-layout.sh"],
            env=dict(os.environ, PATH=str(self.root) + os.pathsep + os.environ["PATH"],
                SQLITE_EXPECTED_RECEIPT=str(stale), DOTCC_COMPILER=str(tool), CONTROL_MARKER=str(marker)),
            capture_output=True, text=True, timeout=5)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("wrong expected receipt", result.stderr)
        self.assertFalse(marker.exists())
        self.assertFalse((sqlite / "generated/TranslatedSqlite").exists())

    def test_live_publisher_releases_shared_lock_before_validation(self):
        from campaigns import delivery as module
        source, target = self.root / "candidate", self.root / "product"
        source.mkdir()
        (source / "new").write_bytes(b"new")
        observed = []
        def validate():
            with module.lock(self.shared):
                observed.append((target / "new").read_bytes())
        with patch.object(module, "publication_lock", side_effect=lambda: module.lock(self.shared)):
            module.promote([(source, target)], self.root / "journal.json", validate)
        self.assertEqual(observed, [b"new"])

    def test_live_publisher_busy_lock_preserves_both_trees(self):
        from campaigns import delivery as module
        source, target = self.root / "candidate", self.root / "product"
        source.mkdir(); target.mkdir()
        (source / "new").write_bytes(b"new")
        (target / "old").write_bytes(b"old")
        with self.shared.open("w") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            with patch.object(module, "publication_lock", side_effect=lambda: module.lock(self.shared)):
                with self.assertRaisesRegex(RuntimeError, "Another campaign owns"):
                    module.promote([(source, target)], self.root / "journal.json", lambda: self.fail("not reached"))
        self.assertEqual((source / "new").read_bytes(), b"new")
        self.assertEqual((target / "old").read_bytes(), b"old")

    def test_managed_net_quic_selects_matching_forms_and_distinct_products(self):
        script = SCRIPTS.parent / "msquic/scripts/test-managed-net-quic.py"
        tree = ast.parse(script.read_text())
        loop = next(n for n in tree.body if isinstance(n, ast.For) and ast.unparse(n.target) == "variant")
        # Exercise the real build-command construction only. TLS/runtime loops
        # and the script's success message are deliberately outside this control.
        loop.body = loop.body[:next(i for i, n in enumerate(loop.body)
            if isinstance(n, ast.For) and ast.unparse(n.target) == "(runtime, command)")]
        module = ast.fix_missing_locations(ast.Module(body=[loop], type_ignores=[]))
        commands = []
        namespace = dict(ROOT=self.root, project=self.root / "consumer.csproj",
            args=SimpleNamespace(variants=["raw", "optimized"], jit_only=False),
            run=lambda command, label: commands.append(list(map(str, command))))
        exec(compile(module, str(script), "exec"), namespace)
        self.assertEqual(len(commands), 4)
        for index, variant in enumerate(("raw", "optimized")):
            suffix = ".Raw" if variant == "raw" else ""
            for command in commands[index * 2:index * 2 + 2]:
                self.assertIn('-p:TranslatedMsQuicDirectory=' + str(self.root / 'generated' / ('TranslatedMsQuic' + suffix)), command)
                self.assertIn('-p:PicotlsProject=' + str(self.root.parent / 'picotls/generated' /
                    ('TranslatedPicotls' + suffix) / 'TranslatedPicotls.csproj'), command)
                self.assertIn(str(self.root / 'build/managed-net-quic' / variant / 'products'), command)
        self.assertNotEqual(commands[0][commands[0].index('-o') + 1], commands[2][commands[2].index('-o') + 1])

    def test_live_framework_serializes_ineligible_sibling_fake_emissions(self):
        from concurrent.futures import ThreadPoolExecutor
        from campaigns import process as module
        executable = self.fake("dotnet")
        executable.write_text(executable.read_text() + "\nimport time\ntime.sleep(0.1)\n")
        original = support.locked_command
        def call(index):
            return module.execute([executable, 'dotcc.dll', '--emit=obj', 'fixture.c'], label=str(index),
                cwd=self.root, output=self.root / f'worker-{index}.log', timeout=5,
                env=dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS='production',
                         CONTROL_MARKER=str(self.root / f'worker-{index}.json')))
        with patch.object(support, "locked_command", side_effect=lambda argv, **kwargs:
                original(argv, shared_lock=self.shared, native_lock=self.native, **kwargs)):
            with ThreadPoolExecutor(max_workers=2) as pool:
                results = list(pool.map(call, (0, 1)))
        self.assertTrue(all(row['exit_code'] == 0 for row in results))

    def test_live_libsmb2_adapter_schedules_sibling_builds_before_os_leases(self):
        from concurrent.futures import ThreadPoolExecutor
        path = SCRIPTS.parent / "libsmb2/scripts/common.py"
        spec = importlib.util.spec_from_file_location("p2_libsmb2_common", path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        executable = self.fake("dotnet")
        executable.write_text(executable.read_text() + "\nimport time\ntime.sleep(0.1)\n")
        original = support.locked_command
        def call(index):
            receipt = {}
            module.run([executable, "build", "fixture.csproj"], self.root / f"smb-{index}.log", receipt,
                cwd=self.root, env=dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS="production",
                    CONTROL_MARKER=str(self.root / f"smb-{index}.json")))
            return receipt["commands"][0]
        with patch.object(support, "locked_command", side_effect=lambda argv, **kwargs:
                original(argv, shared_lock=self.shared, native_lock=self.native, **kwargs)):
            with ThreadPoolExecutor(max_workers=2) as pool:
                results = list(pool.map(call, (0, 1)))
        self.assertTrue(all(row["status"] == "passed" for row in results))
        for row in results:
            self.assertEqual(row["requested_command"][:2], [str(executable), "build"])
            self.assertEqual(row["command_lock_mode"], "exclusive-command")

    def test_sqlite_snapshot_consumer_uses_current_authored_sample(self):
        import xml.etree.ElementTree as ET
        path = SCRIPTS.parent / "sqlite/scripts/test-postprocess.py"
        tree = ast.parse(path.read_text())
        method = next(n for n in tree.body if isinstance(n, ast.FunctionDef) and n.name == "consumer_project")
        module = ast.fix_missing_locations(ast.Module(body=[method], type_ignores=[]))
        root = SCRIPTS.parent / "sqlite"
        namespace = dict(ROOT=root, build=self.root / "consumers", ET=ET)
        exec(compile(module, str(path), "exec"), namespace)
        project = namespace["consumer_project"]("Original", "ManagedConsumer", self.root / "library.csproj")
        includes = [Path(node.attrib["Include"]) for node in ET.parse(project).iter("Compile")]
        actual = {p.resolve() for include in includes for p in include.parent.glob(include.name)}
        expected = {p.resolve() for p in (root / "samples/ManagedConsumer").glob("*.cs")}
        self.assertTrue(expected)
        self.assertEqual(actual, expected)


if __name__ == "__main__":
    unittest.main()
