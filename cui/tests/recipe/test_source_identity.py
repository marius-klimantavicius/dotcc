"""Mocked emission contracts: common logical roots and immutable retained inputs."""
import ast
import importlib.machinery
import importlib.util
from pathlib import Path
import re
import shutil
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
sys.dont_write_bytecode = True
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.recipes import helper

qualification = helper(ROOT / "scripts/qualify.py")
host_probe = helper(ROOT / "tests/host-glfw/build-probe.py")
platform_probe = helper(ROOT / "tests/platform/run.py")


class Linked(Exception):
    """Stop at the object-only link, without invoking a compiler or build."""


def snapshot(root):
    return {p.relative_to(root).as_posix(): (p.stat().st_mode, p.read_bytes() if p.is_file() else None)
            for p in [root, *root.rglob("*")]}


class SourceIdentityContracts(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="cui-source-identity-")
        self.addCleanup(temporary.cleanup)
        self.base = Path(temporary.name)
        self.source = self.base / "retained/source"
        files = {
            "include/cui.h": "typedef struct { int x; } cui_value;\n",
            "src/cui_internal.h": '#include "../include/cui.h"\n',
            "src/core.c": '#include "cui_internal.h"\n',
            "src/cui_bidi.c": '#include "cui_internal.h"\n',
            "src/cui_shape.c": '#include "cui_internal.h"\n',
            "src/render/cui_render_gl.c": '#include "../cui_internal.h"\n',
            "examples/layout.c": '#include "../src/cui_internal.h"\n',
            "tests/local.h": '#include "../include/cui.h"\n',
        }
        for name in ("test_main.c", "test_todo.c", "test_devtools.c", "test_bidi.c", "test_inflate.c", "shape_dump.c"):
            files["tests/" + name] = ('#include "local.h"\n#include "../examples/layout.c"\n'
                                     'int main(int argc, char **argv) { return argc; }\n')
        for name, text in files.items():
            path = self.source / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(text)
        helper(ROOT / "scripts/source_identity.py").stage_headers(ROOT, self.source)
        self.objects = self.base / "retained/objects"
        self.objects.mkdir()
        (self.objects / "core.cs").write_text("//!dotcc object\n// retained core\n")
        for path in self.source.rglob("*"):
            path.chmod(path.stat().st_mode & ~0o222)
        self.source.chmod(self.source.stat().st_mode & ~0o222)
        (self.objects / "core.cs").chmod(0o444)
        self.objects.chmod(0o555)
        self.retained = snapshot(self.base / "retained")
        self.commands = []
        self.runner = object.__new__(qualification.Run)
        self.runner.work = self.base / "qualification"
        self.runner.work.mkdir()
        self.runner.report = {}
        self.runner.args = SimpleNamespace(objects_dir=self.objects, staged_source=self.source,
                                           source=self.source, compiler=self.base / "dotcc.dll", jobs=1)
        self.runner.run = self.compile_mock

    def compile_mock(self, command, label):
        command = list(map(str, command))
        self.commands.append((label, command))
        if "--emit=managedlib" in command:
            self.assertNotIn("--source-root", command)
            raise Linked()
        self.assertIn("--emit=obj", command)
        source = Path(command[command.index("--emit=obj") - 1])
        symbol = re.search(r"(?m)^int (\w+)\(", source.read_text())
        output = Path(command[command.index("-o") + 1])
        output.write_text("//!dotcc object\nint " + (symbol[1] if symbol else "unused") + "(int argc, char **argv) {}\n")

    def assert_emissions(self, root, inputs):
        emissions = [command for _, command in self.commands if "--emit=obj" in command]
        self.assertEqual(len(emissions), len(inputs))
        for command, relative in zip(emissions, inputs):
            self.assertEqual(command.count("--source-root"), 1)
            self.assertEqual(Path(command[command.index("--source-root") + 1]), root)
            self.assertEqual(Path(command[command.index("--emit=obj") - 1]), root / relative)
            includes = [Path(command[i + 1]) for i, value in enumerate(command) if value == "-I"]
            self.assertIn(root / "dotcc/include", includes)
            self.assertIn(root / "include", includes)
            self.assertTrue(all(p.is_relative_to(root) for p in includes))
            self.assertTrue((root / "dotcc/include/GLFW/glfw3.h").is_file())
        self.assertEqual(snapshot(self.base / "retained"), self.retained)

    def test_bundle_keeps_upstream_paths_and_quoted_includes_in_private_root(self):
        source, objects = self.runner.library()
        baseline = snapshot(source)
        with self.assertRaises(Linked):
            self.runner.bundle(["cui_test_todo", "cui_test_devtools"], source, objects)
        root = self.runner.work / "bundle-source"
        self.assert_emissions(root, ["tests/test_todo.c", "tests/test_devtools.c", "dotcc/qualification/bundle-dispatch.c"])
        self.assertEqual(snapshot(source), baseline, "bundle failures must leave the fallback source pristine")
        for name in ("test_todo.c", "test_devtools.c"):
            copied = root / "tests" / name
            text = copied.read_text()
            self.assertIn('#include "local.h"', text)
            self.assertIn('#include "../examples/layout.c"', text)
            self.assertNotIn(str(self.base), text)
            # Included definitions have the same identity as in retained core objects.
            header = (copied.parent / "../examples/../src/cui_internal.h").resolve()
            self.assertEqual(header.relative_to(root).as_posix(), "src/cui_internal.h")
            self.assertEqual(header.read_bytes(), (self.source / "src/cui_internal.h").read_bytes())
        dispatch = (root / "dotcc/qualification/bundle-dispatch.c").read_text()
        self.assertIn("cui_qualification_cui_test_todo(argc-1,argv+1)", dispatch)

    def test_separate_random_instrumentation_preserves_original_test_path(self):
        source, objects = self.runner.library()
        original = (source / "tests/test_main.c").read_text()
        with patch.object(qualification.RANDOM_INPUTS, "adapt", return_value=(original + "// recorded inputs\n", [])), \
                patch.object(qualification.RANDOM_INPUTS, "overrides"), self.assertRaises(Linked):
            self.runner.build("cui_tests", source, objects)
        self.assert_emissions(source, ["tests/test_main.c"])
        self.assertIn("// recorded inputs", (source / "tests/test_main.c").read_text())
        self.assertIn('#include "local.h"', (source / "tests/test_main.c").read_text())

    def test_bundle_restores_entry_before_another_test_includes_it(self):
        # Upstream rtl_paragraph includes rtl_layout.c with a main macro. A
        # previous entrypoint rename must not leak into that included file.
        for name, text in {
            'rtl_layout.c': 'int main(void) { return 0; }\n',
            'rtl_paragraph.c': '#define main layout_probe_main\n#include "rtl_layout.c"\n#undef main\nint main(void) { return 0; }\n',
        }.items():
            p = self.source / 'tests' / name
            p.parent.chmod(p.parent.stat().st_mode | 0o200)
            p.write_text(text)
        self.retained = snapshot(self.base / 'retained')
        source, objects = self.runner.library()
        original_mock = self.compile_mock
        def compile(command, label):
            if label == 'cui_rtl_paragraph/emit':
                layout = self.runner.work / 'bundle-source/tests/rtl_layout.c'
                self.assertEqual(layout.read_text(), 'int main(void) { return 0; }\n')
            return original_mock(command, label)
        self.runner.run = compile
        with self.assertRaises(Linked):
            self.runner.bundle(['cui_rtl_layout', 'cui_rtl_paragraph'], source, objects)
        root = self.runner.work / 'bundle-source'
        self.assert_emissions(root, ['tests/rtl_layout.c', 'tests/rtl_paragraph.c', 'dotcc/qualification/bundle-dispatch.c'])

    def test_fresh_fixture_uses_unique_prefix_in_core_root(self):
        source, objects = self.runner.library()
        with self.assertRaises(Linked):
            self.runner.build("qualification_pixels", source, objects)
        self.assert_emissions(source, ["dotcc/qualification/pixels.c"])

    def test_minimal_programs_share_upstream_root_with_their_extra_units(self):
        for name, inputs in {
            "cui_test_bidi": ["tests/test_bidi.c", "src/cui_bidi.c"],
            "cui_shape_dump": ["tests/shape_dump.c", "src/cui_bidi.c", "src/cui_shape.c"],
            "cui_test_inflate": ["tests/test_inflate.c"],
        }.items():
            with self.subTest(program=name):
                self.commands.clear()
                with self.assertRaises(Linked):
                    self.runner.build(name, self.source, [])
                self.assert_emissions(self.runner.work / "programs" / name / "source", inputs)

    def test_fresh_core_keeps_recipe_root_option_exactly_once(self):
        self.runner.args.objects_dir = None
        original_helper = qualification.helper

        def prepare(ctx):
            root = self.runner.stage_source(self.source, ctx.work / "source")
            return SimpleNamespace(units=[SimpleNamespace(path=root / "src/core.c", flags=())],
                                   flags=[*self.runner.flags(root), "--source-root", str(root)])

        with patch.object(qualification, "helper", side_effect=lambda p:
                          SimpleNamespace(prepare=prepare) if p.name == "campaign.py" else original_helper(p)):
            source, _ = self.runner.library()
        self.assert_emissions(source, ["src/core.c"])

    def test_host_probe_stages_both_sources_and_headers_under_one_root(self):
        work = self.base / "host"
        commands = []
        argv = ["build-probe.py", "--compiler", str(self.base / "dotcc.dll"),
                "--source-root", str(self.source), "--work-root", str(work)]
        with patch.object(sys, "argv", argv), patch("builtins.print"), \
                patch.object(host_probe.subprocess, "run", side_effect=lambda cmd, **_: commands.append(cmd)):
            host_probe.main()
        self.commands = [("host", list(map(str, command))) for command in commands]
        self.assert_emissions(work / "source", ["dotcc/tests/host-glfw/abi-probe.c", "src/render/cui_render_gl.c"])
        self.assertEqual(len(commands), 3)
        self.assertNotIn("--source-root", commands[-1])
        self.assertIn("Managed.Forms", commands[-1])

    def test_emission_rejects_a_different_root_already_in_recipe_flags(self):
        with self.assertRaisesRegex(RuntimeError, "common staged source root"):
            self.runner.emit(self.source / "src/core.c", self.runner.work / "bad.cs",
                             ["--source-root", self.source / "src"], "bad", self.source)
        self.assertEqual(self.commands, [])


class PlatformSourceIdentityContract(unittest.TestCase):
    @unittest.skipUnless(sys.platform == "linux", "platform runner requires Linux")
    def test_selected_functions_and_nested_fixtures_are_inside_emission_root(self):
        # Stage/execution are mocked; the real runner still selects, writes and
        # includes every source fixture, and assembles its actual compiler call.
        with tempfile.TemporaryDirectory(prefix="cui-platform-identity-") as temporary:
            base = Path(temporary)
            source, work = base / "upstream", base / "work"
            seed = {
                "src/cui_emoji.c": ('bool cui_emoji_font_load_system(cui_ctx *ui) { return false; }\n'
                                    '/* ------------------------------------------------------------------------ */\n/* Code points to a glyph'),
                "src/platform/cui_app_glfw.c": ('typedef struct {\n    GLFWwindow *win;\n} cui__glfw;\nstatic cui__glfw *cui__g;\n'
                                                 'bool cui__plat_init(cui_app *app) {\n    app->plat = g;\n}'),
                "src/cui_files.c": ('#include "cui_internal.h"\nstatic const char *cui__file_icon;\n'
                                    'typedef struct {\n    char loaded[1];\n} cui__fb_state;\nstatic void cui__fb_dtor(void);\n'
                                    'static cui__fb_state *cui__fb_sorting;\n/* the filtered, sorted index; */\n'
                                    'static void cui__fb_default_places(void) {}\n/* one frame of the browser: */'),
                "src/platform/cui_app.c": ('#include "cui_platform.h"\n/* NULL means the app */\n'
                                           'static cui_app *cui__app_or_current(void);\nvoid *cui_app_native_window(void);\n'
                                           'static void cui__app_set_clip(void *user, const char *utf8) {}\nvoid cui__app_render(void);'),
                "include/cui.h": "// public header\n",
                "examples/gallery_more.c": "// anchor fixture\n",
                "src/cui_text.c": "// anchor fixture\n",
            }
            for name, text in seed.items():
                path = source / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(text)
            tree = ast.parse((ROOT / "scripts/stage.py").read_text())
            anchors = [ast.literal_eval(call.args[0]) for call in ast.walk(tree)
                       if isinstance(call, ast.Call) and isinstance(call.func, ast.Name) and call.func.id == "replace"]

            class StageLoader:
                def create_module(self, spec):
                    return None

                def exec_module(self, module):
                    def stage(src, dst):
                        if dst.name.startswith("rejected-"):
                            index = int(dst.name.split("-")[1])
                            raise RuntimeError("Expected one adaptation anchor in " + anchors[index] + ": fixture")
                        shutil.copytree(src, dst)
                        return []
                    module.stage = stage

            original_spec = importlib.util.spec_from_file_location

            def load_spec(name, path):
                return (importlib.machinery.ModuleSpec(name, StageLoader()) if name == "stage"
                        else original_spec(name, path))

            commands = []

            def run(cmd, *_args, **_kwargs):
                commands.append(list(map(str, cmd)))
                return SimpleNamespace(stdout="{}", stderr="")

            argv = ["run.py", "--source", str(source), "--work", str(work), "--dotcc", str(base / "dotcc.dll")]
            with patch.object(sys, "argv", argv), patch.dict(platform_probe.os.environ), patch("builtins.print"), \
                    patch.object(platform_probe.importlib.util, "spec_from_file_location", side_effect=load_spec), \
                    patch.object(platform_probe, "run", side_effect=run):
                platform_probe.main()
            command = next(cmd for cmd in commands if "--emit=managedlib" in cmd)
            root = work / "source"
            fixtures = root / "dotcc/tests/platform"
            self.assertEqual(command.count("--source-root"), 1)
            self.assertEqual(Path(command[command.index("--source-root") + 1]), root)
            self.assertEqual(Path(command[2]), fixtures / "fixture.c")
            includes = [Path(command[i + 1]) for i, value in enumerate(command) if value == "-I"]
            self.assertTrue(all(p.is_relative_to(root) for p in includes))
            self.assertIn(root / "dotcc/include", includes)
            for name in ("fixture.c", "emoji-fixture.c", "glfw-prefix-fixture.c", "platform-emoji-selected.c",
                         "platform-glfw-prefix-selected.c", "platform-files-selected.c", "platform-app-selected.c"):
                self.assertTrue((fixtures / name).is_file(), name)
            self.assertTrue((root / "dotcc/include/GLFW/glfw3.h").is_file())


if __name__ == "__main__":
    unittest.main()
