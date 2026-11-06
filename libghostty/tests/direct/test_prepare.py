"""Small pure controls; no native/managed build or upstream test execution.

PYTHONDONTWRITEBYTECODE=1 python3 libghostty/tests/direct/test_prepare.py
"""
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch


REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO / "Scripts"))
from campaigns.recipes import helper

prepare = helper(Path(__file__).with_name("prepare.py"))


class DirectPreparationControls(unittest.TestCase):
    def setUp(self):
        evidence = REPO / "libghostty/build/direct-preparation-r27/controls"
        evidence.mkdir(parents=True, exist_ok=True)
        self.temp = tempfile.TemporaryDirectory(dir=evidence)
        self.addCleanup(self.temp.cleanup)
        self.base = Path(self.temp.name)

    def source(self, name, contents="pub const value = 1;\n"):
        path = self.base / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(contents)
        return path

    def test_native_dependency_scopes_aliases_and_options_keep_order(self):
        for name in ("src/root.zig", "config.zig", "other.zig"):
            self.source(name)
        modules, globals_ = prepare.native_modules([
            "zig", "build-lib", "-fPIC", "-Ofast", "--dep", "local=config", "-Mroot=src/root.zig",
            "-Odebug", "--dep", "local=other", "-Mconfig=config.zig", "-Mother=other.zig",
            "--cache-dir", "private-cache", "-static", "-fcompiler-rt"], self.base)
        self.assertEqual([row["name"] for row in modules], ["root", "config", "other"])
        self.assertEqual(modules[0]["dependencies"], [{"name": "local", "module": "config"}])
        self.assertEqual(modules[1]["dependencies"], [{"name": "local", "module": "other"}])
        self.assertEqual(modules[0]["options"], ["-fPIC", "-Ofast"])
        self.assertEqual(modules[1]["options"], ["-Odebug"])
        self.assertEqual(globals_, ["--cache-dir", "private-cache", "-static", "-fcompiler-rt"])

    def test_missing_duplicate_dangling_and_unknown_native_inputs_fail(self):
        self.source("root.zig")
        cases = [(["--dep", "missing", "-Mroot=root.zig"], "Missing native dependency"),
                 (["-Mroot=root.zig", "-Mroot=root.zig"], "unique native root"),
                 (["-Mroot=root.zig", "--dep", "root"], "no following"),
                 (["-Mroot=missing.zig"], "Missing native module root"),
                 (["--new-upstream-switch", "-Mroot=root.zig"], "Unclassified"),
                 (["--dep", "a=root", "--dep", "a=root", "-Mroot=root.zig"], "Duplicate dependency")]
        for tail, message in cases:
            with self.subTest(tail=tail), self.assertRaisesRegex(RuntimeError, message):
                prepare.native_modules(["zig", "build-lib", *tail], self.base)

    def make_root(self, contents=None):
        root = REPO / "libghostty"
        source = self.base / "upstream"
        file = self.source("upstream/src/lib_vt.zig", contents or
                           'const lib = @This();\ncomptime {\n    if (@import("root") == lib) {\n'
                           '        @export(&api, .{ .name = "original_export" });\n    }\n}\n')
        self.source("upstream/src/terminal/c/main.zig")
        return root, source, file

    def test_actual_root_is_original_bytes_plus_only_abi_import(self):
        root, source, file = self.make_root()
        original = file.read_bytes()
        entry, adapter, proof = prepare.stage_actual(root, source, self.base / "work", "src/lib_vt.zig")
        self.assertEqual(entry.name, "actual.zig")
        self.assertEqual(file.read_bytes(), original)
        self.assertEqual((entry.parent / "lib_vt.zig").read_bytes(), original)
        self.assertEqual(entry.read_bytes()[:len(original)], original)
        self.assertEqual(entry.read_bytes()[len(original):],
                         b'\n// Authored ABI address accessor; upstream exports above are preserved.\n'
                         b'comptime { _ = @import("dotcc_ghostty_api.zig"); }\n')
        rows = json.loads((root / "config/abi.json").read_text())["operations"]
        accessor = (entry.parent / "dotcc_ghostty_api.zig").read_text()
        self.assertEqual(accessor.count("=> @ptrCast(&api."), len(rows))
        self.assertTrue(proof["guard_preserved"])
        with self.assertRaisesRegex(RuntimeError, "staging already exists"):
            prepare.stage_actual(root, source, self.base / "work", "src/lib_vt.zig")

    def test_changed_or_preextended_root_guard_is_rejected_before_staging(self):
        root, source, file = self.make_root()
        original = file.read_text()
        for changed in (original.replace("const lib = @This();", "const lib = other;"),
                        original.replace('if (@import("root") == lib)', "if (true)"),
                        original + '// dotcc_ghostty_api.zig\n'):
            file.write_text(changed)
            with self.assertRaisesRegex(RuntimeError, "actual-root/export guard"):
                prepare.stage_actual(root, source, self.base / "work", "src/lib_vt.zig")
            self.assertFalse((self.base / "work/source").exists())

    def test_generated_feature_mismatch_is_loud(self):
        config = dict(features=["formatter"], excluded_features=["tmux"])
        build = self.source("build_options.zig", "pub const simd: bool = false;\n")
        terminal = self.source("terminal_options.zig", "pub const simd: bool = false;\n"
                               "pub const c_abi: bool = true;\npub const formatter: bool = true;\n"
                               "pub const tmux_control_mode: bool = false;\n")
        modules = [dict(name="build_options", root=str(build)), dict(name="terminal_options", root=str(terminal))]
        self.assertEqual(len(prepare._validate_options(config, modules)), 2)
        terminal.write_text(terminal.read_text().replace("formatter: bool = true", "formatter: bool = false"))
        with self.assertRaisesRegex(RuntimeError, "formatter"):
            prepare._validate_options(config, modules)

    def test_native_only_context_uses_private_caches_and_offline_policy(self):
        root, source, file = self.make_root()
        zig = self.source("zig/zig")
        work = self.base / "native"
        calls = []

        def run(command, label, **kwargs):
            calls.append((list(map(str, command)), kwargs))
            library = work / "install/lib/libghostty-vt.a"
            library.parent.mkdir(parents=True)
            library.write_bytes(b"native control")
            return "verbose native command"

        ctx = SimpleNamespace(root=root, options=SimpleNamespace(fetch="never", hashes="strict"), run=run)
        native_helper = SimpleNamespace(_source=lambda context: source, _zig=lambda context: zig,
                                        _engine_command=lambda log, exe, tree: [str(zig), "build-lib", "-Mroot=" + str(file)])
        with patch.object(prepare, "helper", return_value=native_helper):
            result = prepare._native(ctx, work)
        command, keywords = calls[0]
        self.assertEqual(command[1], "build")
        self.assertIn("-j2", command)
        self.assertIn("--system", command)
        self.assertNotIn("--global-cache-dir", command)  # Zig 0.17 build rejects it.
        self.assertEqual(Path(keywords["env"]["ZIG_GLOBAL_CACHE_DIR"]).parent, work)
        self.assertEqual(ctx.options.hashes, "strict")
        self.assertFalse(any("-ofmt=c" in part for part in command))
        self.assertEqual(result["upstream"], source)

    def test_context_contract_separates_compiler_graph_from_native_options(self):
        root, source, original = self.make_root()
        self.source("upstream/src/build/config.zig")
        build = self.source("build_options.zig", "pub const simd: bool = false;\n")
        config = json.loads((root / "config/translation.json").read_text())
        values = dict(simd=False, c_abi=True, **{name: True for name in config["features"]},
                      **{{"tmux": "tmux_control_mode"}.get(name, name): False for name in config["excluded_features"]})
        terminal = self.source("terminal_options.zig", "".join(
            f"pub const {name}: bool = {str(value).lower()};\n" for name, value in values.items()))
        zig = self.source("zig/zig")
        native = dict(upstream=source, zig=zig, native_library=self.base / "native.a", command=["zig", "build", "-j2"],
                      argv=[str(zig), "build-lib", "-fPIC", "-Ofast", "-lc", "--dep", "build_options", "--dep", "terminal_options",
                            "-Mroot=" + str(original), "-Odebug", "-Mbuild_options=" + str(build),
                            "-Mterminal_options=" + str(terminal), "-Mconfig=src/build/config.zig"])
        ctx = SimpleNamespace(root=root, work=self.base / "context", receipt={})
        with patch.object(prepare, "_native", return_value=native) as prepare_native:
            result = prepare.prepare_direct(ctx)
            self.assertIs(prepare.prepare_direct(ctx), result)
            prepare_native.assert_called_once()
        graph = json.loads(result["module_graph_file"].read_text())
        self.assertEqual(set(graph), {"schema_version", "root_module", "zig_lib_dir", "output_mode", "optimize_mode", "modules"})
        self.assertEqual(graph["output_mode"], "Lib")
        self.assertEqual(graph["optimize_mode"], "fast")
        self.assertEqual(set(graph["modules"][0]), {"name", "root", "dependencies"})
        self.assertEqual(graph["modules"][0]["root"], str(result["source"]))
        self.assertEqual(graph["modules"][-1]["root"], str(result["source"].parent / "build/config.zig"))
        self.assertEqual(result["unit"].path.suffix, ".zig")
        self.assertEqual(result["boundary_unit"].path, root / config["bridge"])
        audit = json.loads(result["manifest_file"].read_text())
        self.assertFalse(audit["generated_c_engine"])
        self.assertFalse(audit["compiler_target_applied"])
        self.assertEqual(audit["native_target"], "native")
        self.assertEqual(audit["c_backend_comparison_target"], config["c_target"])
        self.assertTrue(audit["compiler_output_mode_applied"])
        self.assertEqual(audit["compiler_output_mode"], "Lib")
        self.assertEqual(audit["native_modules"][0]["options"], ["-fPIC", "-Ofast", "-lc"])
        self.assertEqual(audit["native_modules"][1]["options"], ["-Odebug"])
        self.assertTrue(audit["compiler_optimization_applied"])
        self.assertEqual(audit["compiler_optimize_mode"], "fast")
        self.assertEqual(audit["compiler_optimization_context"], dict(
            native_module="root", native_option="-Ofast", native_option_index=1, native_mode="fast",
            optimize_mode="fast", normalized=False, builtin_scope="root/shared-std"))
        self.assertEqual(audit["unapplied_native_module_options"], [
            dict(name="root", options=["-fPIC", "-lc"]), dict(name="build_options", options=["-Odebug"]),
            dict(name="terminal_options", options=[]), dict(name="config", options=[])])
        self.assertFalse(audit["compiler_link_libc_applied"])
        self.assertFalse(audit["compiler_zig_version_applied"])
        self.assertTrue(audit["aggregate_data"])
        self.assertEqual((audit["namespace"], audit["class_name"]), ("Managed", "GhosttyVt"))
        self.assertIn("-fno-emit-bin", audit["direct_native_command"])
        self.assertIn("-Mconfig=" + graph["modules"][-1]["root"], audit["direct_native_command"])
        for spelling, mode in {"debug": "debug", "safe": "safe", "fast": "fast", "small": "small",
                               "Debug": "debug", "ReleaseSafe": "safe", "ReleaseFast": "fast", "ReleaseSmall": "small"}.items():
            context = prepare._optimization_context(["-O" + spelling])
            self.assertEqual(context["optimize_mode"], mode)
            self.assertEqual(context["native_mode"], spelling)
            self.assertEqual(context["native_option"], "-O" + spelling)
            self.assertEqual(context["normalized"], spelling != mode)
        self.assertIsNone(prepare._optimization_context(["-D", "-Ofast"]))
        context = prepare._optimization_context(["-D", "-Ofast", "-Osafe"])
        self.assertEqual(context["optimize_mode"], "safe")
        self.assertEqual(context["native_option_index"], 2)
        for options in (["-Oturbo"], ["-O"], ["-Ofast", "-Osafe"], ["-Ofast", "-Ofast"]):
            with self.subTest(options=options), self.assertRaisesRegex(RuntimeError, "optimization"):
                prepare._optimization_context(options)
        # A dependency's explicit mode, C target and managed build flavor cannot
        # supply missing root compilation context.
        native["argv"].remove("-Ofast")
        absent = SimpleNamespace(root=root, work=self.base / "absent-context", receipt={})
        with patch.object(prepare, "_native", return_value=native):
            absent_result = prepare.prepare_direct(absent)
        absent_graph = json.loads(absent_result["module_graph_file"].read_text())
        absent_audit = json.loads(absent_result["manifest_file"].read_text())
        self.assertNotIn("optimize_mode", absent_graph)
        self.assertFalse(absent_audit["compiler_optimization_applied"])
        self.assertIsNone(absent_audit["compiler_optimize_mode"])
        self.assertIsNone(absent_audit["compiler_optimization_context"])
        self.assertEqual(absent_audit["unapplied_native_module_options"][1]["options"], ["-Odebug"])


if __name__ == "__main__":
    unittest.main()
